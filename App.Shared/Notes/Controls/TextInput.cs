using System;
using System.Xml;
using Rock.Mobile.UI;
using System.Collections.Generic;
using App.Shared.Notes.Model;
using System.Drawing;

namespace App
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// A label displaying Placeholder text. When tapped, allows
            /// a user to enter text via keyboard.
            /// </summary>
            public class TextInput : BaseControl
            {
                /// <summary>
                /// Actual textfield object.
                /// </summary>
                /// <value>The text field.</value>
                protected PlatformTextView TextView { get; set; }

                protected override void Initialize( )
                {
                    base.Initialize( );

                    TextView = PlatformTextView.Create( );
                }

                public TextInput( CreateParams parentParams, XmlReader reader )
                {
                    Initialize( );

                    // Always get our style first
                    mStyle = parentParams.Style;
                    Styles.Style.ParseStyleAttributesWithDefaults( reader, ref mStyle, ref ControlStyles.mTextInput );

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
                    TextView.SetFont( mStyle.mFont.mName, mStyle.mFont.mSize.Value );
                    TextView.TextColor = mStyle.mFont.mColor.Value;
                    TextView.KeyboardAppearance = App.Shared.Config.GeneralConfig.iOSPlatformUIKeyboardAppearance;

                    // check for border styling
                    if ( mStyle.mBorderColor.HasValue )
                    {
                        TextView.BorderColor = mStyle.mBorderColor.Value;
                    }

                    if( mStyle.mBorderRadius.HasValue )
                    {
                        TextView.CornerRadius = mStyle.mBorderRadius.Value;
                    }

                    if( mStyle.mBorderWidth.HasValue )
                    {
                        TextView.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    if( mStyle.mBackgroundColor.HasValue )
                    {
                        TextView.BackgroundColor = mStyle.mBackgroundColor.Value;
                    }

                   
                    // set the dimensions and position
                    TextView.Bounds = bounds;
                    TextView.Placeholder = " ";

                    // get the hint text if it's as an attribute
                    string result = reader.GetAttribute( "Placeholder" );
                    if( string.IsNullOrEmpty( result ) == false )
                    {
                        TextView.Placeholder = result;
                    }

                    // parse the rest of the stream
                    if( reader.IsEmptyElement == false )
                    {
                        bool finishedLabel = false;
                        while( finishedLabel == false && reader.Read( ) )
                        {
                            switch( reader.NodeType )
                            {
                                case XmlNodeType.Element:
                                {
                                    switch( reader.Name )
                                    {
                                        case "Placeholder":
                                        {
                                            TextView.Placeholder = reader.ReadElementContentAsString( );
                                            break;
                                        }
                                    }
                                    break;
                                }

                                case XmlNodeType.EndElement:
                                {
                                    // if we hit the end of our label, we're done.
                                    //if( reader.Name == "TextInput" || reader.Name == "TI" )
                                    if( ElementTagMatches( reader.Name ) )
                                    {
                                        finishedLabel = true;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    // adjust the text according to the style
                    switch( mStyle.mTextCase )
                    {
                        case Styles.TextCase.Upper:
                        {
                            TextView.Placeholder = TextView.Placeholder.ToUpper( );
                            break;
                        }

                        case Styles.TextCase.Lower:
                        {
                            TextView.Placeholder = TextView.Placeholder.ToLower( );
                            break;
                        }
                    }

                    // size to fit to calculate the height, then reset our width with that height.
                    //TextView.SizeToFit( );
                    TextView.Frame = new RectangleF( bounds.X, bounds.Y, bounds.Width, bounds.Height );

                    // set the color of the hint text
                    TextView.PlaceholderTextColor = mStyle.mFont.mColor.Value;
                }

                public override IUIControl TouchesEnded( PointF touch )
                {
                    // hide the keyboard
                    TextView.ResignFirstResponder( );

                    return null;
                }

                public override void AddOffset( float xOffset, float yOffset )
                {
                    base.AddOffset( xOffset, yOffset );

                    TextView.Position = new PointF( TextView.Position.X + xOffset, 
                        TextView.Position.Y + yOffset );
                }

                public override void AddToView( object obj )
                {
                    TextView.AddAsSubview( obj );

                    TryAddDebugLayer( obj );
                }

                public override void RemoveFromView( object obj )
                {
                    TextView.RemoveAsSubview( obj );

                    TryRemoveDebugLayer( obj );
                }

                public override void BuildHTMLContent( ref string htmlStream, List<IUIControl> userNotes )
                {
                    htmlStream += TextView.Text;
                }
                public override PlatformBaseUI GetPlatformControl()
                {
                    return TextView;
                }

                public void SetText( string text )
                {
                    TextView.Text = text;
                }

                public override RectangleF GetFrame( )
                {
                    return TextView.Frame;
                }

                public static bool ElementTagMatches(string elementTag)
                {
                    if ( elementTag == "TI" || elementTag == "TextInput" )
                    {
                        return true;
                    }
                    return false;
                }

                public NoteState.TextInputState GetState()
                {
                    return new NoteState.TextInputState( TextView.Text );
                }
            }
        }
    }
}
