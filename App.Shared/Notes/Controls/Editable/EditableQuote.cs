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

                // store our parent so we know our bound restrictions
                RectangleF ParentFrame { get; set; }
                
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
                    SizeF parentSize = new SizeF( parentParams.Width, parentParams.Height );

                    // get our margin / padding
                    // note - declare a temp margin on the stack that we'll throw out. We store this in our BaseControl.
                    RectangleF tempMargin;
                    RectangleF tempBounds = new RectangleF( );
                    RectangleF padding;
                    GetMarginsAndPadding( ref mStyle, ref parentSize, ref tempBounds, out tempMargin, out padding );
                    ApplyImmediateMargins( ref tempBounds, ref tempMargin, ref parentSize );

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
                            // if they press return, commit the changed text.
                            QuoteLabel.Text = EditMode_TextBox_Quote.Text;
                            QuoteLabel.Frame = new RectangleF( QuoteLabel.Frame.Left, QuoteLabel.Frame.Top, 0, 0 );
                            QuoteLabel.SizeToFit( );

                            Citation.Text = EditMode_TextBox_Citation.Text;
                            Citation.Frame = new RectangleF( Citation.Frame.Left, Citation.Frame.Top, 0, 0 );
                            Citation.SizeToFit( );
                                
                            EnableEditMode( false );

                            SetPosition( Frame.Left, Frame.Top );
                            
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
                        float xOffset = xPos - Frame.Left;
                        float yOffset = yPos - Frame.Top;

                        base.AddOffset( xOffset, yOffset );

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

                public void HandleKeyUp( KeyEventArgs e )
                {
                    // ignore
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