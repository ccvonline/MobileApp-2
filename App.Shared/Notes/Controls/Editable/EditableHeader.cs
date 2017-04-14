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

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableHeader: Header, IEditableUIControl
            {
                bool EditMode_Enabled = false;
                EditModeTextBox EditMode_TextBox_Title = null;
                EditModeTextBox EditMode_TextBox_Date = null;
                EditModeTextBox EditMode_TextBox_Speaker = null;
                                
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
                
                public EditableHeader( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // create our textbox that will display the text being edited.
                    EditMode_TextBox_Title = new EditModeTextBox( );
                    EditMode_TextBox_Title.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Speaker = new EditModeTextBox( );
                    EditMode_TextBox_Speaker.KeyUp += EditMode_TextBox_KeyUp;

                    EditMode_TextBox_Date = new EditModeTextBox( );
                    EditMode_TextBox_Date.KeyUp += EditMode_TextBox_KeyUp;
                    
                    ParentNote = parentParams.Parent as Note;

                    // take the max width / height we'll be allowed to fit in (and because it's the header, ignore note padding)
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
                            // only allow editing to end if there's text in all text boxes
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Title.Text ) == false && 
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Speaker.Text ) == false && 
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Date.Text ) == false )
                            {
                                // if they press return, commit the changed text.
                                mTitle.Text = EditMode_TextBox_Title.Text;
                                mTitle.Frame = new RectangleF( 0, 0, ParentSize.Width - Padding.Left - Padding.Width - (BorderPaddingPx * 2), 0 );
                                mTitle.SizeToFit( );

                                mSpeaker.Text = EditMode_TextBox_Speaker.Text;
                                mSpeaker.Frame = new RectangleF( );
                                mSpeaker.SizeToFit( );

                                mDate.Text = EditMode_TextBox_Date.Text;
                                mDate.Frame = new RectangleF( );
                                mDate.SizeToFit( );
                                
                                EnableEditMode( false );
                                    

                                // offset the controls according to our layout
                                mTitle.Position = new PointF( Frame.X + Padding.Left + BorderPaddingPx, 
                                                              Frame.Y + Padding.Top + BorderPaddingPx );

                                // guarantee date and speaker are below title.
                                float titleDetailsSpacing = Rock.Mobile.Graphics.Util.UnitToPx( -9 );
                                mDate.Position = new PointF( Frame.X + Padding.Left + BorderPaddingPx, 
                                                             mTitle.Frame.Bottom + titleDetailsSpacing );

                                mSpeaker.Position = new PointF( Frame.Right - (mSpeaker.Frame.Width + Padding.Left + BorderPaddingPx), 
                                                                mTitle.Frame.Bottom + titleDetailsSpacing );


                                // verify that the speaker won't overlap date. if it will, left justify them under each other beneath the title.
                                if ( mSpeaker.Position.X < mDate.Frame.Right )
                                {
                                    mDate.Position = new PointF( mTitle.Position.X, mTitle.Frame.Bottom + titleDetailsSpacing);
                                    mSpeaker.Position = new PointF( mTitle.Position.X, mDate.Frame.Bottom );
                                }

                                // determine the lowest control
                                float bottomY = mSpeaker.Frame.Bottom > mTitle.Frame.Bottom ? mSpeaker.Frame.Bottom : mTitle.Frame.Bottom;
                                bottomY = bottomY > mDate.Frame.Bottom ? bottomY : mDate.Frame.Bottom;

                                // set our bounds
                                Frame = new RectangleF( Frame.X, Frame.Y, Frame.Width, (bottomY + Padding.Height + BorderPaddingPx) - Frame.Y);
                                BorderView.Frame = Frame;
                                SetDebugFrame( Frame );
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
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Title );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Speaker );
                            ParentEditingCanvas.Children.Add( EditMode_TextBox_Date );

                            // hide the regular text
                            mTitle.Hidden = true;
                            mSpeaker.Hidden = true;
                            mDate.Hidden = true;

                            // position and size the textboxes
                            float availableWidth = ParentSize.Width - Padding.Left - Padding.Width - (BorderPaddingPx * 2);

                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Title, mTitle.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Title, mTitle.Frame.Top );

                            EditMode_TextBox_Title.Width = availableWidth;
                            EditMode_TextBox_Title.Height = mTitle.Frame.Height;

                            
                             // date
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Date, mDate.Frame.Left );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Date, mDate.Frame.Top + 15 );

                            EditMode_TextBox_Date.Width = availableWidth / 2;
                            EditMode_TextBox_Date.Height = mDate.Frame.Height;

                            
                            // speaker
                            System.Windows.Controls.Canvas.SetLeft( EditMode_TextBox_Speaker, mDate.Frame.Left + (availableWidth / 2) );
                            System.Windows.Controls.Canvas.SetTop( EditMode_TextBox_Speaker, mSpeaker.Frame.Top + 15 );

                            EditMode_TextBox_Speaker.Width = availableWidth / 2;
                            EditMode_TextBox_Speaker.Height = mSpeaker.Frame.Height;
                            
                            
                            // assign each text box
                            EditMode_TextBox_Title.Text = mTitle.Text.Trim( ' ' );
                            EditMode_TextBox_Speaker.Text = mSpeaker.Text.Trim( ' ' );
                            EditMode_TextBox_Date.Text = mDate.Text.Trim( ' ' );

                            Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Input, new Action( delegate() 
                            { 
                                EditMode_TextBox_Title.Focus( );
                                Keyboard.Focus( EditMode_TextBox_Title );
                                EditMode_TextBox_Title.CaretIndex = EditMode_TextBox_Title.Text.Length + 1;
                            }));
                        }
                        else
                        {
                            // unhide the regular controls
                            mTitle.Hidden = false;
                            mSpeaker.Hidden = false;
                            mDate.Hidden = false;

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
                }

                public void HandleDelete( bool notifyParent )
                {
                    RemoveFromView( ParentEditingCanvas );

                    // notify our parent
                    if( notifyParent )
                    {
                        ParentNote.HandleChildDeleted( this );
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
                        // clamp the left/top to 0, 0 within the parent (headers can ignore padding)
                        yPos = Math.Max( yPos, 0 );
                        xPos = Math.Max( xPos, 0 );

                        float xOffset = xPos - Frame.Left;
                        float yOffset = yPos - Frame.Top;

                        base.AddOffset( xOffset, yOffset );
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
                            if ( string.IsNullOrWhiteSpace( EditMode_TextBox_Title.Text ) == false && 
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Speaker.Text ) == false && 
                                 string.IsNullOrWhiteSpace( EditMode_TextBox_Date.Text ) == false )
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
                    string encodedTitle = HttpUtility.HtmlEncode( mTitle.Text );
                    string encodedSpeaker = HttpUtility.HtmlEncode( mSpeaker.Text );
                    string encodedDate = HttpUtility.HtmlEncode( mDate.Text );

                    // remove margin from the header, because it simply doesn't matter for a visual editor
                    return "<Header Margin=\"0\">" + 
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