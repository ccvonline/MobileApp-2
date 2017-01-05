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

namespace MobileApp
{
    namespace Shared
    {
        namespace Notes
        {
            public class EditableStackPanel : StackPanel, IEditableUIControl
            {
                // store our parent so we know our bound restrictions
                RectangleF ParentFrame { get; set; }

                BaseControl ParentControl { get; set; }
                
                SizeF ParentSize;
                RectangleF Padding;

                public EditableStackPanel( CreateParams parentParams, XmlReader reader ) : base( parentParams, reader )
                {
                    // this will be null if the parent is the actual note
                    ParentControl = parentParams.Parent as BaseControl;

                    // take the max width / height we'll be allowed to fit in
                    ParentSize = new SizeF( parentParams.Width, parentParams.Height );

                    // get our margin / padding
                    // note - declare a temp margin on the stack that we'll throw out. We store this in our BaseControl.
                    RectangleF tempMargin;
                    RectangleF tempBounds = new RectangleF( );
                    GetMarginsAndPadding( ref mStyle, ref ParentSize, ref tempBounds, out tempMargin, out Padding );
                    ApplyImmediateMargins( ref tempBounds, ref tempMargin, ref ParentSize );
                }

                public void UpdatePosition( float deltaX, float deltaY )
                {
                    AddOffset( deltaX, deltaY );

                    foreach ( IEditableUIControl control in ChildControls )
                    {
                        if ( control != null )
                        {
                            control.UpdatePosition( 0, 0 );
                        }
                    }
                }

                public IEditableUIControl ControlAtPoint( PointF point )
                {
                    // see if any of our child controls contain the point
                    foreach ( IEditableUIControl control in ChildControls )
                    {
                        IEditableUIControl consumingControl = control.ControlAtPoint( point );
                        if ( consumingControl != null )
                        {
                            return consumingControl;
                        }
                    }

                    if ( GetFrame( ).Contains( point ) )
                    {
                        return this;
                    }

                    return null;
                }
                

                public void ToggleHighlight( object masterView )
                {
                    ToggleDebug( masterView );
                }
            }
        }
    }
}
#endif