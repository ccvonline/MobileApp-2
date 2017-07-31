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
            public class EditableTextInput: TextInput, IEditableUIControl
            {
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
                SizeF ParentSize;
                
                public EditableTextInput( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;
                    
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
                        TextView.BorderWidth = mStyle.mBorderWidth.Value;
                    }

                    OrigBackgroundColor = TextView.BackgroundColor;
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
                }

                private void EnableEditMode( bool enabled )
                {
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
                    SetPosition( TextView.Frame.Left, TextView.Frame.Top );
                }

                public PointF GetPosition( )
                {
                    return new PointF( TextView.Frame.Left, TextView.Frame.Top );
                }

                public void SetPosition( float xPos, float yPos )
                {
                    // clamp the yPos to the vertical bounds of our parent
                    yPos = Math.Max( yPos, ParentNote.Padding.Top );

                    // now left, which is easy
                    xPos = Math.Max( xPos, ParentNote.Padding.Left );

                    // Now do the right edge. This is tricky because we will allow this control to move forward
                    // until it can't wrap any more. Then, we'll clamp its movement to the parent's edge.

                    // Get the width the textview needs
                    float minRequiredWidth = TextView.Frame.Width + Padding.Left + Padding.Width;
                            
                    // now, if the control cannot wrap any further, we want to clamp its movement
                    // to the parent's right edge

                    // Right Edge Check
                    float adjustedXPos = Math.Min( xPos, ParentSize.Width - ParentNote.Padding.Right - minRequiredWidth );

                    float xOffset = adjustedXPos - TextView.Frame.Left;
                    float yOffset = yPos - TextView.Frame.Top;

                    base.AddOffset( xOffset, yOffset );
                    
                    // now update the actual width and height of the Quote based on the available width remaining
                    // our width remaining is the parent's right edge minus the control's left edge minus all padding.
                    float availableWidth = (ParentSize.Width - ParentNote.Padding.Right) - TextView.Frame.Left - Padding.Left - Padding.Width;
                        
                    TextView.Frame = new RectangleF( TextView.Frame.Left, TextView.Frame.Top, Math.Max( minRequiredWidth, availableWidth ), TextView.Frame.Height );
                    SetDebugFrame( TextView.Frame );
                }
                
                public IEditableUIControl HandleMouseDoubleClick( PointF point )
                {
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
                    return true;
                }

                public bool IsEditing( )
                {
                    return false;
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
                        TextView.BackgroundColor = 0xFFFFFF77;
                    }
                    // we're NOT hovering
                    else
                    {
                        // so revert the color and turn the Hovering flag off
                        TextView.BackgroundColor = OrigBackgroundColor;
                    }

                    return consumingControl;
                }

                public string Export( RectangleF parentPadding, float currYPos )
                {
                    // start by setting our position to our global position, and then we'll translate.
                    float controlLeftPos = TextView.Frame.Left;
                    float controlTopPos = TextView.Frame.Top;

                    // for vertical, it's relative to the control above it, so just make it relative to that
                    IUIControl logicalParent = ParentNote.GetLogicalVerticalParent( this );
                    if ( logicalParent != null )
                    {
                        controlTopPos -= logicalParent.GetFrame( ).Bottom;
                    }
                    else
                    {
                        controlTopPos -= currYPos;
                    }

                    // for horizontal, it just needs to remove padding, since it'll be re-applied on load
                    controlLeftPos -= parentPadding.Left;
                                        
                    // Add the tag and attribs
                    // Note: remove margin, because the default_style includes it, and that makes no sense when we will visually place it
                    string xml = string.Format( "<TI Top=\"{0}\" Height=\"{1}\"", controlTopPos, TextView.Frame.Height );

                    controlLeftPos /= (ParentSize.Width - parentPadding.Left - parentPadding.Right);
                    xml += string.Format( " Left=\"{0:#0.00}%\"", controlLeftPos * 100 );
                  
                    xml += ">";

                    // and the content
                    xml += "</TI>";
                    return xml;
                }

                public void ToggleDebugRect( bool enabled )
                {
                    // set the hover appearance
                    if( enabled && OrigBackgroundColor != EditableConfig.sDebugColor )
                    {
                        OrigBackgroundColor = TextView.BackgroundColor;
                        TextView.BackgroundColor = EditableConfig.sDebugColor;
                    }
                    else if ( enabled == false )
                    {
                        TextView.BackgroundColor = OrigBackgroundColor;
                    }
                }
            }
        }
    }
}
#endif