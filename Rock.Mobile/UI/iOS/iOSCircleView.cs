#if __IOS__
using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;
using Rock.Mobile.PlatformSpecific.Util;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The iOS implementation of a circle view (don't confuse this with overriding an actual UIView, it contains one.)
        /// </summary>
        public class iOSCircleView : PlatformCircleView
        {
            protected UIView View { get; set; }

            public iOSCircleView( )
            {
                View = new UIView( );
                View.Layer.AnchorPoint = new PointF( 0, 0 );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                View.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( View.Layer.BackgroundColor );
            }

            protected override void setBorderColor( uint borderColor )
            {
                View.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( View.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) View.Layer.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                View.Layer.BorderWidth = width;
            }

            protected override float getOpacity( )
            {
                return View.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                View.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float) View.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                View.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return View.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                View.Bounds = bounds;
                View.Layer.CornerRadius = bounds.Width / 2;
            }

            protected override RectangleF getFrame( )
            {
                return View.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                View.Frame = frame;
                View.Layer.CornerRadius = frame.Width / 2;
            }

            protected override PointF getPosition( )
            {
                return View.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                View.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return View.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                View.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return View.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                View.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( View );
            }

            public override void RemoveAsSubview( object obj )
            {
                //obj is only for Android, so we don't use it.
                View.RemoveFromSuperview( );
            }

            protected override object getPlatformNativeObject( )
            {
                return View;
            }
        }
    }
}
#endif
