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

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableQuote: Quote, IEditableUIControl
            {
                bool EditMode_Enabled = false;
                TextBox EditMode_TextBox_Quote = null;
                TextBox EditMode_TextBox_Citation = null;
                                
                // store the background color so that if we change it for hovering, we can restore it after
                uint OrigBackgroundColor = 0;

                // the size (in pixels) to extend the paragraph's frame
                // for mouse interaction
                const float CornerExtensionSize = 5;

                // Store the canvas that is actually rendering this control, so we can
                // add / remove edit controls as needed (text boxes, toolbars, etc.)
                System.Windows.Controls.Canvas ParentEditingCanvas;

                // store our literal parent control so we can notify if we were updated
                object ParentControl { get; set; }

                RectangleF Padding;
                int BorderPaddingPx;
                SizeF ParentSize;
                
                public EditableQuote( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // create our textbox that will display the text being edited.
                    EditMode_TextBox_Quote = new TextBox( );
                    EditMode_TextBox_Quote.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Citation = new TextBox( );
                    EditMode_TextBox_Citation.KeyUp += EditMode_TextBox_KeyUp;
                    
                    // this will be null if the parent is the actual note
                    ParentControl = parentParams.Parent;

                    // take the max width / height we'll be allowed to fit in
                    ParentSize = new SizeF( parentParams.Width, parentParams.Height );

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
                            // only allow enditing to end if there's text in both boxes
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Quote.Text ) == false && 
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Citation.Text ) == false )
                            {
                                // if they press return, commit the changed text.
                                QuoteLabel.Text = EditMode_TextBox_Quote.Text;
                                Citation.Text = EditMode_TextBox_Citation.Text;
                                
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
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Quote );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Citation );

                            // position and size the textboxes
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Quote, QuoteLabel.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Quote, QuoteLabel.Frame.Top );

                            EditMode_TextBox_Quote.Width = QuoteLabel.Frame.Width;
                            EditMode_TextBox_Quote.Height = QuoteLabel.Frame.Height;

                            // speaker
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Citation, Citation.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Citation, Citation.Frame.Top );

                            EditMode_TextBox_Citation.Width = Citation.Frame.Width;
                            EditMode_TextBox_Citation.Height = Citation.Frame.Height;
                            
                            // assign each text box
                            EditMode_TextBox_Quote.Text = QuoteLabel.Text;
                            EditMode_TextBox_Citation.Text = Citation.Text;

                            Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                            { 
                                EditMode_TextBox_Quote.Focus( );
                                Keyboard.Focus( EditMode_TextBox_Quote );
                                EditMode_TextBox_Quote.CaretIndex = EditMode_TextBox_Quote.Text.Length + 1;
                            }));
                        }
                        else
                        {
                            // exit enable mode. We know the parent is a canvas because of the design
                            (EditMode_TextBox_Quote.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Quote );
                            (EditMode_TextBox_Citation.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Citation );
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

                    // now see if there's a parent that we should notify
                    IEditableUIControl editableParent = ParentControl as IEditableUIControl;
                    if( editableParent != null )
                    {
                        editableParent.HandleChildStyleChanged( style, this );
                    }
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

                        
                        // now, we need to ensure we stay within our parent, whether that's a control or the Note
                        RectangleF parentFrame;

                        BaseControl parentAsBase = ParentControl as BaseControl;
                        if ( parentAsBase != null )
                        {
                            // our parent is a control, so clamp our X/Y to its X/Y
                            parentFrame = parentAsBase.GetFrame( );
                        
                            // clamp the yPos to the vertical bounds of our parent
                            yPos = Math.Max( yPos, parentFrame.Top );
                            yPos = Math.Min( yPos, parentFrame.Bottom );


                            // now left, which is easy
                            xPos = Math.Max( xPos, parentFrame.Left );
                        }
                        else
                        {
                            // no restriction on X/Y, but take width/height
                            parentFrame = new RectangleF( 0, 0, ParentSize.Width, ParentSize.Height );
                        }

                         // Now do the right edge. This is tricky because we will allow this control to move forward
                        // until it can't wrap any more. Then, we'll clamp its movement to the parent's edge.

                        // Get the width of the widest child, which is always the citation plus glyph.
                        float minRequiredWidth = Citation.Frame.Width + UrlGlyph.Frame.Width;
                            
                        // now, if the control cannot wrap any further, we want to clamp its movement
                        // to the parent's right edge
                        if ( Math.Floor( Frame.Width ) <= Math.Floor( minRequiredWidth ) )
                        {
                            // Right Edge Check
                            xPos = Math.Min( xPos, parentFrame.Right - minRequiredWidth );
                        }


                        //TODO / FIX: Somehow, our left edge isn't holding to Max( Left, CitationWidth ) like it should,
                        // causing the quote to shrink too much.


                        float xOffset = xPos - Frame.Left;
                        float yOffset = yPos - Frame.Top;

                        base.AddOffset( xOffset, yOffset );
                        

                        // now update the actual width and height of the Quote based on the available width left
                        float availableWidth = ParentSize.Width - Frame.Left - Padding.Left - Padding.Width - (BorderPaddingPx * 2);
                        QuoteLabel.Frame = new RectangleF( QuoteLabel.Frame.Left, QuoteLabel.Frame.Top, availableWidth, 0 );
                        QuoteLabel.SizeToFit( );

                        Citation.Frame = new RectangleF( Citation.Frame.Left, Citation.Frame.Top, availableWidth, 0 );
                        Citation.SizeToFit( );

                        

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
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Quote.Text ) == false && 
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
                        IEditableUIControl editableParent = ParentControl as IEditableUIControl;
                        if ( editableParent != null )
                        {
                            editableParent.HandleChildDeleted( this );
                        }
                        else
                        {
                            Note noteParent = ParentControl as Note;
                            noteParent.HandleChildDeleted( this );
                        }
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

                public string Export( )
                {
                    string encodedQuote = HttpUtility.HtmlEncode( QuoteLabel.Text );
                    string encodedCitation = HttpUtility.HtmlEncode( Citation.Text );

                    string xml = "<Q Citation=\"" + encodedCitation + "\">" + 
                                    encodedQuote +
                                 "</Q>";
                    return xml;
                }
            }
        }
    }
}
#endif