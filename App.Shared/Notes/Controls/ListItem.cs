using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

using App.Shared.Notes.Styles;
using Rock.Mobile.UI;
using App.Shared.Config;
using System.Drawing;
using App.Shared.PrivateConfig;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A container that displays children in a vertical stack almost exactly like a stack panel.
            /// </summary>
            public class ListItem : StackPanel
            {
                public ListItem( CreateParams parentParams, XmlReader reader )
                {
                    // verify that our parent is a list. That's the only acceptable place for us.
                    if( (parentParams.Parent as List) == null )
                    {
                        throw new Exception( string.Format( "<ListItem> parent must be <List>. This <ListItem> parent is: <{0}>", parentParams.Parent.GetType() ) );
                    }

                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mListItem );
                    mStyle.mAlignment = null; //don't use alignment

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );
                    ParseCommonAttribs( reader, ref parentSize, ref bounds );

                    //ignore positioning attributes.
                    bounds = new RectangleF( );
                    bounds.Width = parentParams.Width;

                    // Get margins and padding
                    RectangleF padding;
                    RectangleF margin;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref bounds, out margin, out padding );

                    // apply margins to as much of the bounds as we can (bottom must be done by our parent container)
                    ApplyImmediateMargins( ref bounds, ref margin, ref parentSize );
                    Margin = margin;


                    // check for border styling
                    int borderPaddingPx = 0;
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        BorderView.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        BorderView.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        BorderView.BorderWidth = mStyle.mBorderWidth.Value;
                        borderPaddingPx = (int)Rock.Mobile.Graphics.Util.UnitToPx( mStyle.mBorderWidth.Value + PrivateNoteConfig.BorderPadding );
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        BorderView.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }
                    //

                    // now calculate the available width based on padding. (Don't actually change our width)
                    float availableWidth = bounds.Width - padding.Left - padding.Width - (borderPaddingPx * 2);

                    // Parse Child Controls
                    bool finishedParsing = false;
                    while( finishedParsing == false && reader.Read( ) )
                    {
                        switch( reader.NodeType )
                        {
                            case XmlNodeType.Element:
                            {
                                // let each child have our available width.
                                IUIControl control = Parser.TryParseControl( new CreateParams( this, availableWidth, parentParams.Height, ref mStyle ), reader );
                                if( control != null )
                                {
                                    ChildControls.Add( control );
                                }
                                break;
                            }

                            case XmlNodeType.Text:
                            {
                                // grab the text. remove any weird characters
                                string text = Regex.Replace( reader.Value, @"\t|\n|\r", "" );

                                // now break it into words so we can do word wrapping
                                string[] words = text.Split( ' ' );

                                // the very very first word gets a bullet point!
                                string sentence = "";
                                foreach( string word in words )
                                {
                                    // create labels out of each one
                                    if( string.IsNullOrEmpty( word ) == false )
                                    {
                                        sentence += word + " ";
                                    }
                                }

                                NoteText textLabel = new NoteText( new CreateParams( this, availableWidth, parentParams.Height, ref mStyle ), sentence );
                                ChildControls.Add( textLabel );
                                break;
                            }

                            case XmlNodeType.EndElement:
                            {
                                // if we hit the end of our label, we're done.
                                //if( reader.Name == "ListItem" || reader.Name == "LI" )
                                if( ElementTagMatches( reader.Name ) )
                                {
                                    finishedParsing = true;
                                }

                                break;
                            }
                        }
                    }

                    LayoutStackPanel( bounds, padding.Left, padding.Top, availableWidth, padding.Height, borderPaddingPx );
                }

                public override bool ShouldShowBulletPoint()
                {
                    // let our first control (which will be displayed first) decide
                    return ChildControls[0].ShouldShowBulletPoint( );
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    htmlStream += "<li>";

                    foreach( IUIControl control in ChildControls )
                    {
                        control.BuildHTMLContent( ref htmlStream, userNotes );
                        htmlStream += "<br>";
                    }

                    // handle user notes
                    EmbedIntersectingUserNotes( ref htmlStream, userNotes );

                    // closing markup
                    htmlStream += "</li>";
                }

                public static new bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "LI" || elementTag == "ListItem" )
                    {
                        return true;
                    }
                    return false;
                }

                protected override List<IUIControl> GetChildControls( )
                {
                    return ChildControls;
                }
            }
        }
    }
}
