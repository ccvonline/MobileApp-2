using System;
using System.Xml;
using System.Collections.Generic;
using Rock.Mobile.UI;
using System.Drawing;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Control that lays out basic text. Used by Paragraphs.
            /// </summary>
            public class NoteText : BaseControl
            {
                /// <summary>
                /// Actual text label.
                /// </summary>
                /// <value>The platform label.</value>
                protected PlatformLabel PlatformLabel { get; set; }

                protected NoteText( )
                {
                }

                public NoteText( CreateParams parentParams, string text )
                {
                    base.Initialize( );

                    PlatformLabel = PlatformLabel.Create( );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );

                    // take our parent's style, and for anything not set by them use the default.
                    mStyle = parentParams.Style;
                    Styles.Style.MergeStyleAttributesWithDefaults( ref mStyle, ref ControlStyles.mText );

                    PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        PlatformLabel.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        PlatformLabel.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        PlatformLabel.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                    // set the dimensions and position
                    if( bounds.Width == 0 )
                    {
                        // always take the available width, in case this control
                        // is specified to be offset relative to its parent
                        bounds.Width = parentParams.Width - bounds.X;
                    }
                    PlatformLabel.Bounds = bounds;

                    // get text
                    SetText( text );

                    // position ourselves in absolute coordinates, and trust our parent to offset us to be relative to them.
                    PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
                }

                // This constructor is called when explicit Note Text is being declared.
                // This means the XML has "<NoteText>Something</NoteText>. Its used when
                // the user wants to alter a particular piece of text within a paragraph.
                public NoteText( CreateParams parentParams, XmlReader reader )
                {
                    base.Initialize( );

                    PlatformLabel = PlatformLabel.Create( );
                    PlatformLabel.SetFade( 0.0f );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mText );

                    // check for attributes we support
                    RectangleF bounds = new RectangleF( );
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );
                    ParseCommonAttribs( reader, ref parentSize, ref bounds );

                    // Get margins and padding
                    RectangleF padding;
                    RectangleF margin;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref bounds, out margin, out padding );

                    // apply margins to as much of the bounds as we can (bottom must be done by our parent container)
                    ApplyImmediateMargins( ref bounds, ref margin, ref parentSize );
                    Margin = margin;

                    // create the font that either we or our parent defined
                    PlatformLabel.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        PlatformLabel.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        PlatformLabel.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        PlatformLabel.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                    // see if the user wants this text underlined
                    string underlined = reader.GetAttribute( "Underlined" );
                    if( string.IsNullOrWhiteSpace( underlined ) == false )
                    {
                        bool addUnderline = bool.Parse( underlined );
                        if( addUnderline )
                        {
                            PlatformLabel.AddUnderline( );
                        }
                    }

                    // parse the stream
                    string noteText = "";
                    if( reader.IsEmptyElement == false )
                    {
                        bool finishedLabel = false;
                        while( finishedLabel == false && reader.Read( ) )
                        {
                            switch( reader.NodeType )
                            {
                                case XmlNodeType.Text:
                                {
                                    // support text as embedded in the element
                                    noteText = reader.Value.Replace( System.Environment.NewLine, "" );

                                    break;
                                }

                                case XmlNodeType.EndElement:
                                {
                                    // if we hit the end of our label, we're done.
                                    //if( reader.Name == "NoteText" || reader.Name == "NT" )
                                    if( ElementTagMatches( reader.Name ) )
                                    {
                                        finishedLabel = true;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    // adjust the text
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            noteText = noteText.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            noteText = noteText.ToLower( );
                            break;
                        }
                    }

                    SetText( noteText );

                    PlatformLabel.Position = new PointF( bounds.X, bounds.Y );
                }

                public void SetText( string text )
                {
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            PlatformLabel.Text = text.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            PlatformLabel.Text = text.ToLower( );
                            break;
                        }

                        case Styles.TextCase.Normal:
                        {
                            PlatformLabel.Text = text;
                            break;
                        }
                    }

                    // resize the label to fit the text
                    PlatformLabel.SizeToFit( );
                }

                public string GetText( )
                {
                    return PlatformLabel.Text;
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    PlatformLabel.Position = new PointF( PlatformLabel.Position.X + xOffset, 
                                                         PlatformLabel.Position.Y + yOffset );
                }

                public override void AddToView( object obj )
                {
                    PlatformLabel.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    PlatformLabel.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public static bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "NT" || elementTag == "NoteText" )
                    {
                        return true;
                    }
                    return false;
                }

                public override PlatformBaseUI GetPlatformControl()
                {
                    return PlatformLabel;
                }

                public override void BuildHTMLContent( ref string htmlStream, ref string textStream, List<IUIControl> userNotes )
                {
                    textStream += PlatformLabel.Text;
                    htmlStream += PlatformLabel.Text;
                }

                public override RectangleF GetFrame( )
                {
                    return PlatformLabel.Frame;
                }
            }
        }
    }
}
