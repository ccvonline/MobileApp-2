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

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableList: List, IEditableUIControl
            {
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
                IEditableUIControl ParentControl { get; set; }
                
                public EditableList( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    ParentEditingCanvas = null;

                    // this will be null if the parent is the actual note
                    ParentControl = parentParams.Parent as IEditableUIControl;

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
                    float currX = Frame.Left;
                    float currY = Frame.Top;

                    Frame = new RectangleF( xPos, yPos, Frame.Width, Frame.Height );

                    float xOffset = Frame.Left - currX;
                    float yOffset = Frame.Top - currY;

                    // position each interactive label relative to ourselves
                    foreach( IUIControl control in ChildControls )
                    {
                        control.AddOffset( xOffset, yOffset );
                    }

                    BorderView.Position = new PointF( BorderView.Position.X + xOffset,
                                                      BorderView.Position.Y + yOffset );

                    SetDebugFrame( Frame );
                }
                
                public IEditableUIControl HandleMouseDoubleClick( PointF point )
                {
                    // see if any of our child controls contain the point
                    foreach ( IEditableUIControl control in ChildControls )
                    {
                        IEditableUIControl consumingControl = control.HandleMouseDoubleClick( point );
                        if ( consumingControl != null )
                        {
                            return consumingControl;
                        }
                    }
                    
                    // we don't need to support double click on ourselves
                    return null;
                }
                
                public IEditableUIControl HandleMouseDown( PointF point )
                {
                    // see if any of our child controls contain the point
                    foreach ( IEditableUIControl control in ChildControls )
                    {
                        IEditableUIControl consumingControl = control.HandleMouseDown( point );
                        if ( consumingControl != null )
                        {
                            return consumingControl;
                        }
                    }
                    
                    // also don't need to support mouse down
                    return null;
                }

                public void HandleKeyUp( KeyEventArgs e )
                {
                    // ignore
                }

                public bool IsEditing( )
                {
                    return false;
                }

                public IEditableUIControl ContainerForControl( System.Type controlType, PointF mousePos )
                {
                    // we can return ourselves, as we store any control type, but first see if any of our children want this
                    // see if any of our child controls contain the point
                    foreach ( IEditableUIControl control in ChildControls )
                    {
                        IEditableUIControl consumingControl = control.ContainerForControl( controlType, mousePos );
                        if ( consumingControl != null )
                        {
                            return consumingControl;
                        }
                    }

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
                    // create the control and add it to our immediate children
                    IUIControl newControl = Parser.CreateEditableControl( controlType, new BaseControl.CreateParams( this, Frame.Width, Frame.Height, ref mStyle ) );
                    ChildControls.Add( newControl );
                    
                    // add it to our renderable canvas
                    newControl.AddToView( ParentEditingCanvas );

                    // get the position relative to us
                    //PointF localPos = new PointF( mousePos.X - Frame.Left, mousePos.Y - Frame.Top );

                    // default it to where the click occurred
                    newControl.AddOffset( mousePos.X, mousePos.Y );

                    // force our position to update
                    SetPosition( Frame.Left, Frame.Top );
                    
                    // return the editable interface for the caller
                    return newControl;
                }

                public void HandleDeleteControl( )
                {
                    // todo: handle deleting ourselves and any child controls

                    // notify our parent
                    IEditableUIControl editableParent = ParentControl as IEditableUIControl;
                    if( editableParent != null )
                    {
                        editableParent.HandleChildDeleted( this );
                    }
                }

                public void HandleChildDeleted( IEditableUIControl childControl )
                {
                }
                
                public IEditableUIControl HandleMouseHover( PointF mousePos )
                {
                    IEditableUIControl consumingControl = null;
                    bool hoveringChildControl = false;
                                        
                    // create a position outside the canvas. As soon as we find an item we're hovering over,
                    // we'll send the rest of the controls this position to force them to discontinue their hover state
                    PointF oobPos = new PointF( -100, -100 );

                    // see if any of our child controls contain the point
                    int i;
                    for( i = 0; i < ChildControls.Count; i++ )
                    {
                        IEditableUIControl editControl = ChildControls[ i ] as IEditableUIControl;
                        if ( editControl != null )
                        {
                            // if we're hovering over any control, flag it, but don't stop checking
                            consumingControl = editControl.HandleMouseHover( mousePos );
                            if( consumingControl != null )
                            {
                                hoveringChildControl = true;
                                break;
                            }
                        }
                    }

                    // now let the remainig children turn off
                    i++;
                    for( ; i < ChildControls.Count; i++ )
                    {
                        IEditableUIControl editableControl = ChildControls[ i ] as IEditableUIControl;
                        if( editableControl != null )
                        {
                            editableControl.HandleMouseHover( oobPos );
                        }
                    }
                    
                    // if we're over a child
                    if( hoveringChildControl == true )
                    {
                        // restore the color
                        BorderView.BackgroundColor = OrigBackgroundColor;
                    }
                    else
                    {
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
                    }

                    return consumingControl;
                }
            }
        }
    }
}
#endif