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
            public class EditableHeader: Header, IEditableUIControl
            {
                bool EditMode_Enabled = false;
                TextBox EditMode_TextBox_Title = null;
                TextBox EditMode_TextBox_Date = null;
                TextBox EditMode_TextBox_Speaker = null;

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
                
                public EditableHeader( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // create our textbox that will display the text being edited.
                    EditMode_TextBox_Title = new TextBox( );
                    EditMode_TextBox_Title.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Speaker = new TextBox( );
                    EditMode_TextBox_Speaker.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Date = new TextBox( );
                    EditMode_TextBox_Date.KeyUp += EditMode_TextBox_KeyUp;

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
                            mTitle.Text = EditMode_TextBox_Title.Text;
                            mTitle.Frame = new RectangleF( mTitle.Frame.Left, mTitle.Frame.Top, 0, 0 );
                            mTitle.SizeToFit( );

                            mSpeaker.Text = EditMode_TextBox_Speaker.Text;
                            mSpeaker.Frame = new RectangleF( mSpeaker.Frame.Left, mSpeaker.Frame.Top, 0, 0 );
                            mSpeaker.SizeToFit( );

                            mDate.Text = EditMode_TextBox_Date.Text;
                            mDate.Frame = new RectangleF( mDate.Frame.Left, mDate.Frame.Top, 0, 0 );
                            mDate.SizeToFit( );
                                
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
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Title );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Speaker );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Date );

                            // position and size the textboxes
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Title, mTitle.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Title, mTitle.Frame.Top );

                            EditMode_TextBox_Title.Width = mTitle.Frame.Width;
                            EditMode_TextBox_Title.Height = mTitle.Frame.Height;

                            // speaker
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Speaker, mSpeaker.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Speaker, mSpeaker.Frame.Top );

                            EditMode_TextBox_Speaker.Width = mSpeaker.Frame.Width;
                            EditMode_TextBox_Speaker.Height = mSpeaker.Frame.Height;

                            // date
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Date, mDate.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Date, mDate.Frame.Top );

                            EditMode_TextBox_Date.Width = mDate.Frame.Width;
                            EditMode_TextBox_Date.Height = mDate.Frame.Height;

                            
                            // assign each text box
                            EditMode_TextBox_Title.Text = mTitle.Text;
                            EditMode_TextBox_Speaker.Text = mSpeaker.Text;
                            EditMode_TextBox_Date.Text = mDate.Text;

                            Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                            { 
                                EditMode_TextBox_Title.Focus( );
                                Keyboard.Focus( EditMode_TextBox_Title );
                                EditMode_TextBox_Title.CaretIndex = EditMode_TextBox_Title.Text.Length + 1;
                            }));
                        }
                        else
                        {
                            // exit enable mode. We know the parent is a canvas because of the design
                            (EditMode_TextBox_Title.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Title );
                            (EditMode_TextBox_Speaker.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Speaker );
                            (EditMode_TextBox_Date.Parent as System.Windows.Controls.Canvas).Children.Remove( EditMode_TextBox_Date );
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

                public void HandleDelete( bool notifyParent )
                {
                    RemoveFromView( ParentEditingCanvas );

                    // notify our parent
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
                    string encodedTitle = HttpUtility.HtmlEncode( mTitle.Text );
                    string encodedSpeaker = HttpUtility.HtmlEncode( mSpeaker.Text );
                    string encodedDate = HttpUtility.HtmlEncode( mDate.Text );

                    return "<Header>" + 
                              "<Title>" + encodedTitle + "</Title>" + 
                              "<Speaker>" + encodedSpeaker + "</Speaker>" + 
                              "<Date>" + encodedDate + "</Date>" +
                           "</Header>";

                }
            }
        }
    }
}
#endif