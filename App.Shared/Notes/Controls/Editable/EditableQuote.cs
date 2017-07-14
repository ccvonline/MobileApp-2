#if __WIN__
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

using MobileApp.Shared.Notes.Styles;
using Rock.Mobile.UI;
using MobileApp.Shared.Config;
using System.Drawing;
using MobileApp.Shared.PrivateConfig;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using RestSharp.Extensions.MonoHttp;
using WinNotes;
using App.Shared;

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableQuote: Quote, IEditableUIControl
            {
                public const string sDefaultNewQuoteText = "New Quote Body";
                public const string sPlaceholderUrlText = "Optional URL";

                bool EditMode_Enabled = false;
                EditModeTextBox EditMode_TextBox_Quote = null;
                EditModeTextBox EditMode_TextBox_Citation = null;
                EditModeTextBox EditMode_TextBox_Url = null;
                                
                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;

                // the size (in pixels) to extend the paragraph's frame
                // for mouse interaction
                const float CornerExtensionSize = 5;

                // Store the canvas that is actually rendering this control, so we can
                // add / remove edit controls as needed (text boxes, toolbars, etc.)
                System.Windows.Controls.Canvas ParentEditingCanvas;

                // store our literal parent control so we can notify if we were updated
                Note ParentNote { get; set; }

                RectangleF Padding;
                int BorderPaddingPx;
                SizeF ParentSize;
                
                public EditableQuote( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // create our textbox that will display the text being edited.
                    EditMode_TextBox_Quote = new EditModeTextBox( );
                    EditMode_TextBox_Quote.TextWrapping = System.Windows.TextWrapping.Wrap;
                    EditMode_TextBox_Quote.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Citation = new EditModeTextBox( );
                    EditMode_TextBox_Citation.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Url = new EditModeTextBox( );
                    EditMode_TextBox_Url.KeyUp += EditMode_TextBox_KeyUp;
                    
                    // this will be null if the parent is the actual note
                    ParentNote = parentParams.Parent as Note;

                    // take the max width / height we'll be allowed to fit in (restore the padding, since we actively remove it when dragging / exporting)
                    ParentSize = new SizeF( parentParams.Width + ParentNote.Padding.Left + ParentNote.Padding.Right, parentParams.Height );

                    // get our margin / padding
                    // note - declare a temp margin on the stack that we'll throw out. We store this in our BaseControl.
                    RectangleF tempMargin;
                    RectangleF tempBounds = new RectangleF( );
                    GetMarginsAndPadding( ref mStyle, ref ParentSize, ref tempBounds, out tempMargin, out Padding );
                    ApplyImmediateMargins( ref tempBounds, ref tempMargin, ref ParentSize );
                    
                    if( mStyle.mBorderWidth.HasValue )
                    {
                        BorderView.BorderWidth = mStyle.mBorderWidth.Value;
                        BorderPaddingPx = (int)Rock.Mobile.Graphics.Util.UnitToPx( mStyle.mBorderWidth.Value + PrivateNoteConfig.BorderPadding );
                    }

                    OrigBackgroundColor = BorderView.BackgroundColor;
                }

                public override void AddToView( object obj )
                {
                    // store our parent canvas so we can toggle editing as needed
                    ParentEditingCanvas = obj as System.Windows.Controls.Canvas;

                    base.AddToView( obj );
                }

                public object GetStyleValue( EditStyling.Style style )
                {
                    return null;
                }

                public void SetStyleValue( EditStyling.Style style, object value )
                {
                }

                public void ResetBounds( )
                {
                }
                
                private void EditMode_TextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
                {
                    switch ( e.Key )
                    {
                        case System.Windows.Input.Key.Escape:
                        {
                            EnableEditMode( false );
                            break;
                        }

                        case System.Windows.Input.Key.Return:
                        {
                            // editing can end as long as ONE of the two has text
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Quote.Text ) == false ||
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Citation.Text ) == false )
                            {
                                // if they press return, commit the changed text.
                                QuoteLabel.Text = EditMode_TextBox_Quote.Text;
                                Citation.Text = EditMode_TextBox_Citation.Text;
                                
                                // try to set the URL
                                string activeUrl = EditMode_TextBox_Url.Text;
                                if( activeUrl.Trim( ) != sPlaceholderUrlText && string.IsNullOrWhiteSpace( activeUrl ) == false )
                                {
                                    // help them out by adding 'http://' if it isn't there.
                                    if ( activeUrl.StartsWith( "http://" ) == false && BibleRenderer.IsBiblePrefix( activeUrl ) == false )
                                    {
                                        activeUrl = activeUrl.Insert( 0, "http://" );
                                    }

                                    ActiveUrl = activeUrl;
                                }
                                else
                                {
                                    ActiveUrl = null;
                                }
                                
                                EnableEditMode( false );

                                // update the position, which also effects layout
                                SetPosition( Frame.Left, Frame.Top );
                            }
                            break;
                        }
                    }
                }

                private void EnableEditMode( bool enabled )
                {
                    // don't allow setting the mode to what it's already set to
                    if( enabled != EditMode_Enabled )
                    {
                        // enter enable mode
                        if ( enabled == true )
                        {
                            // hide the normal controls
                            QuoteLabel.Hidden = true;
                            Citation.Hidden = true;
                            BorderView.Hidden = true;
                            UrlGlyph.Hidden = true;

                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Quote );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Citation );

                            // first get the size of the control, which should be whatever's largest: the quote or citation.
                            // we do this to prevent taking a 0 width, which it could be if the quote or citation are blank.
                            float controlWidth = Math.Max( QuoteLabel.Frame.Width, Citation.Frame.Width );
                            float controlHeight = Math.Max( QuoteLabel.Frame.Height, Citation.Frame.Height );


                            // position and size the textboxes
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Quote, QuoteLabel.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Quote, QuoteLabel.Frame.Top );
                            
                            float availWidth = Math.Min( (ParentSize.Width - QuoteLabel.Frame.Left), controlWidth * 4 );

                            EditMode_TextBox_Quote.Width = availWidth;
                            EditMode_TextBox_Quote.Height = 128;
                            
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Citation, QuoteLabel.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Citation, QuoteLabel.Frame.Top + EditMode_TextBox_Quote.Height  );

                            EditMode_TextBox_Citation.Width = availWidth;
                            EditMode_TextBox_Citation.Height = Citation.Frame.Height;
                            
                            // assign each text box
                            EditMode_TextBox_Quote.Text = QuoteLabel.Text.Trim( ' ' );
                            EditMode_TextBox_Citation.Text = Citation.Text.Trim( ' ' );


                            // and now the URL support
                            EditMode_TextBox_Url.Text = ActiveUrl == null ? sPlaceholderUrlText : ActiveUrl;
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Url );
                            EditMode_TextBox_Url.Width = availWidth;
                            EditMode_TextBox_Url.Height = 33;
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Url, QuoteLabel.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Url, QuoteLabel.Frame.Top + EditMode_TextBox_Quote.Height + EditMode_TextBox_Citation.Height + 5 );


                            Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                            { 
                                EditMode_TextBox_Quote.Focus( );
                                Keyboard.Focus( EditMode_TextBox_Quote );
                                EditMode_TextBox_Quote.CaretIndex = EditMode_TextBox_Quote.Text.Length + 1;

                                if( EditMode_TextBox_Quote.Text == sDefaultNewQuoteText )
                                {
                                    EditMode_TextBox_Quote.SelectAll( );
                                    EditMode_TextBox_Citation.SelectAll( );
                                }

                                if ( EditMode_TextBox_Url.Text == sPlaceholderUrlText )
                                {
                                    EditMode_TextBox_Url.SelectAll( );
                                }
                            }));
                        }
                        else
                        {
                            // unhide the normal controls
                            QuoteLabel.Hidden = false;
                            Citation.Hidden = false;
                            BorderView.Hidden = false;
                            UrlGlyph.Hidden = false;

                            // exit enable mode. We know the parent is a canvas because of the design
                            (EditMode_TextBox_Quote.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Quote );
                            (EditMode_TextBox_Citation.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Citation );
                            (EditMode_TextBox_Url.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Url );
                        }

                        // store the change
                        EditMode_Enabled = enabled;
                    }
                }

                // Sigh. This is NOT the EditStyle referred to above. This is the Note Styling object
                // used by the notes platform.
                public MobileApp.Shared.Notes.Styles.Style GetControlStyle( )
                {
                    return mStyle;
                }
                
                public List<EditStyling.Style> GetEditStyles( )
                {
                    return new List<EditStyling.Style>( );
                }

                public void HandleChildStyleChanged( EditStyling.Style style, IEditableUIControl childControl )
                {
                    // for now, lets just redo our layout.
                    SetPosition( Frame.Left, Frame.Top );
                }

                public PointF GetPosition( )
                {
                    return new PointF( Frame.Left, Frame.Top );
                }

                public void SetPosition( float xPos, float yPos )
                {
                    // we're not moving if we're in edit mode
                    if( EditMode_Enabled == false )
                    {
                        // first, let the Citation + Glyph define the minimum width this control can be
                        Citation.Frame = new RectangleF( );
                        Citation.SizeToFit( );
                        
                        if ( string.IsNullOrEmpty( ActiveUrl ) == false )
                        {
                            UrlGlyph.Text = PrivateNoteConfig.CitationUrl_Icon;
                        }
                        else
                        {
                            UrlGlyph.Text = string.Empty;
                        }
                        UrlGlyph.SizeToFit( );
                        //
                        
                        // clamp the yPos to the vertical bounds of our parent
                        yPos = Math.Max( yPos, ParentNote.Padding.Top );

                        // now left, which is easy
                        xPos = Math.Max( xPos, ParentNote.Padding.Left );

                         // Now do the right edge. This is tricky because we will allow this control to move forward
                        // until it can't wrap any more. Then, we'll clamp its movement to the parent's edge.

                        // Get the width of the widest child, which is always the citation plus glyph.
                        float minRequiredWidth = Citation.Frame.Width + UrlGlyph.Frame.Width + (BorderPaddingPx * 2) + Padding.Left + Padding.Width;
                            
                         // now, if the control cannot wrap any further, we want to clamp its movement
                        // to the parent's right edge

                        // Right Edge Check
                        xPos = Math.Min( xPos, ParentSize.Width - ParentNote.Padding.Right - minRequiredWidth );

                        float xOffset = xPos - Frame.Left;
                        float yOffset = yPos - Frame.Top;

                        base.AddOffset( xOffset, yOffset );
                        

                        // now update the actual width and height of the Quote based on the available width remaining
                        // our width remaining is the parent's right edge minus the control's left edge minus all padding.
                        float availableWidth = (ParentSize.Width - ParentNote.Padding.Right) - Frame.Left - Padding.Left - Padding.Width - (BorderPaddingPx * 2);
                        
                        QuoteLabel.Frame = new RectangleF( QuoteLabel.Frame.Left, QuoteLabel.Frame.Top, Math.Max( minRequiredWidth, availableWidth ), 0 );
                        QuoteLabel.SizeToFit( );

                        Citation.Frame = new RectangleF( Citation.Frame.Left, Citation.Frame.Top, Math.Max( minRequiredWidth, availableWidth ), 0 );
                        Citation.SizeToFit( );

                        UrlGlyph.Frame = new RectangleF( Citation.Frame.Right, Citation.Frame.Top, Math.Max( 0, (availableWidth - Citation.Frame.Right) ), 0 );
                        UrlGlyph.SizeToFit( );

                        // now that we know our text size, we can adjust the citation
                        // for citation width, attempt to use quote width, but if there was no quote text,
                        // the width will be 0, so we'll fallback to the citation width.

                        RectangleF frame;
                        if( string.IsNullOrEmpty( QuoteLabel.Text ) != true )
                        {   
                            // when taking the citation frame, put it at the left edge of the quote,
                            // because if it's longer than the quote we'll want the bounding frame to use
                            // its width as opposed to the quote's width.
                            Citation.Frame = new RectangleF( QuoteLabel.Frame.Left, 
                                                             QuoteLabel.Frame.Bottom, 
                                                             Citation.Frame.Width,
                                                             Citation.Frame.Height );

                            UrlGlyph.Frame = new RectangleF( Citation.Frame.Right,
                                                             Citation.Frame.Top - ( (UrlGlyph.Frame.Height - Citation.Frame.Height) / 2),
                                                             UrlGlyph.Frame.Width,
                                                             UrlGlyph.Frame.Height );

                            RectangleF citationGlyphFrame = Parser.CalcBoundingFrame( Citation.Frame, UrlGlyph.Frame );

                            // get a bounding frame for the quote and citation
                            frame = Parser.CalcBoundingFrame( QuoteLabel.Frame, citationGlyphFrame );

                            // now right-adjust it IF it's smaller than the quote
                            if ( citationGlyphFrame.Width < QuoteLabel.Frame.Width )
                            {
                                Citation.Position = new PointF( QuoteLabel.Frame.Right - citationGlyphFrame.Width, Citation.Position.Y );
                                UrlGlyph.Position = new PointF( Citation.Frame.Right, UrlGlyph.Frame.Top );
                            }
                        }
                        else
                        {
                            Citation.Frame = new RectangleF( QuoteLabel.Frame.Left, 
                                                             QuoteLabel.Frame.Top, 
                                                             Citation.Frame.Width,
                                                             Citation.Frame.Height );

                            UrlGlyph.Frame = new RectangleF( Citation.Frame.Right,
                                                             Citation.Frame.Top - ( (UrlGlyph.Frame.Height - Citation.Frame.Height) / 2),
                                                             UrlGlyph.Frame.Width,
                                                             UrlGlyph.Frame.Height );

                            // get a bounding frame for the quote and citation
                            frame = Parser.CalcBoundingFrame( Citation.Frame, UrlGlyph.Frame );
                        }

                        Citation.TextAlignment = TextAlignment.Right;

                        // reintroduce vertical padding
                        frame.Height = frame.Height + Padding.Top + Padding.Height;

                        // setup our bounding rect for the border

                        // because we're basing it off of the largest control (quote or citation),
                        // we need to reintroduce the border padding.
                        frame = new RectangleF( frame.X - BorderPaddingPx - Padding.Left, 
                                                frame.Y - BorderPaddingPx - Padding.Top, 
                                                frame.Width + (Padding.Width * 2) + (BorderPaddingPx * 2), 
                                                frame.Height + (Padding.Height) + BorderPaddingPx );

                        // and store that as our bounds
                        BorderView.Frame = frame;
                        Frame = frame;
                        SetDebugFrame( Frame );
                    }
                }
                
                public IEditableUIControl HandleMouseDoubleClick( PointF point )
                {
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );
                    if ( frame.Contains( point ) )
                    {
                        // notify the caller we're consuming, and turn on edit mode
                        EnableEditMode( true );

                        return this;
                    }

                    return null;
                }
                
                public IEditableUIControl HandleMouseDown( PointF point )
                {
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );
                    if ( frame.Contains( point ) )
                    {
                        return this;
                    }

                    return null;
                }

                public bool HandleFocusedControlKeyUp( KeyEventArgs e )
                {
                    // for controls with textBoxes used for editing, we need
                    // to be consistent with how it will handle input.
                    bool releaseFocus = false;
                    switch( e.Key )
                    {
                        case Key.Return:
                        {
                            // on return, editing will only end, (and thus focus should clear)
                            // if there's text in the text box
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Quote.Text ) == false ||
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Citation.Text ) == false )
                            {
                                releaseFocus = true;
                            }

                            break;
                        }

                        case Key.Escape:
                        {
                            // on escape, always release focus
                            releaseFocus = true;
                            break;
                        }
                    }

                    return releaseFocus;
                }

                public bool IsEditing( )
                {
                    return EditMode_Enabled;
                }

                public void HandleDelete( bool notifyParent )
                {
                    RemoveFromView( ParentEditingCanvas );

                    // notify our parent if we need to
                    if( notifyParent )
                    {
                        ParentNote.HandleChildDeleted( this );
                    }
                }

                public void HandleChildDeleted( IEditableUIControl childControl )
                {
                    // this control doesn't support children
                }

                public IEditableUIControl ContainerForControl( System.Type controlType, PointF mousePos )
                {
                    // we can return ourselves, as we store any control type, but first see if any of our children want this
                    // see if any of our child controls contain the point
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );
                    if ( frame.Contains( mousePos ) )
                    {
                        return this;
                    }

                    return null;
                }

                public IUIControl HandleCreateControl( System.Type controlType, PointF mousePos )
                {
                    return null;
                }
                
                public IEditableUIControl HandleMouseHover( PointF mousePos )
                {
                   IEditableUIControl consumingControl = null;

                    // otherwise, see if we're hovering over the paragraph, and if we are, highlight it
                    RectangleF frame = GetFrame( );
                    frame.Inflate( CornerExtensionSize, CornerExtensionSize );

                    // are we hovering over this control?
                    bool mouseHovering = frame.Contains( mousePos );
                    if( mouseHovering == true )
                    {
                        // take us as the consumer
                        consumingControl = this;

                        // alter the hover appearance
                        BorderView.BackgroundColor = 0xFFFFFF77;
                    }
                    // we're NOT hovering
                    else
                    {
                        // so revert the color and turn the Hovering flag off
                        BorderView.BackgroundColor = OrigBackgroundColor;
                    }

                    return consumingControl;
                }

                public string Export( RectangleF parentPadding, float currYPos )
                {
                    // start by setting our position to our global position, and then we'll translate.
                    float controlLeftPos = Frame.Left;
                    float controlTopPos = Frame.Top;
                    
                     // for vertical, it's relative to the control above it, so just make it relative to that
                    controlTopPos -= currYPos;

                    // for horizontal, it just needs to remove padding, since it'll be re-applied on load
                    controlLeftPos -= parentPadding.Left;
                    
                    string encodedQuote = HttpUtility.HtmlEncode( QuoteLabel.Text );
                    string encodedCitation = HttpUtility.HtmlEncode( Citation.Text );

                    // Add the tag and attribs
                    // Note: remove margin, because the default_style includes it, and that makes no sense when we will visually place it
                    string xml = string.Format( "<Q Margin=\"0\" Citation=\"{0}\" Top=\"{1}\"", encodedCitation, controlTopPos );

                    controlLeftPos /= (ParentSize.Width - parentPadding.Left - parentPadding.Right);
                    xml += string.Format( " Left=\"{0:#0.00}%\"", controlLeftPos * 100 );
                    
                    if ( string.IsNullOrWhiteSpace( ActiveUrl ) == false )
                    {
                        xml += string.Format( " Url=\"{0}\"", HttpUtility.HtmlEncode( ActiveUrl ) );
                    }
                    xml += ">";

                    // and the content
                    xml += encodedQuote + "</Q>";
                    return xml;
                }
            }
        }
    }
}
#endif