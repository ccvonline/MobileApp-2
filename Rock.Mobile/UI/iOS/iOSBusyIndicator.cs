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
        /// The iOS implementation of a view (don't confuse this with overriding an actual UIView, it contains one.)
        /// </summary>
        public class iOSBusyIndicator : PlatformBusyIndicator
        {
            protected UIActivityIndicatorView BusyIndicator { get; set; }

            public iOSBusyIndicator( )
            {
                BusyIndicator = new UIActivityIndicatorView( );
                BusyIndicator.Layer.AnchorPoint = new PointF( 0, 0 );
                BusyIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
                BusyIndicator.SizeToFit( );
                BusyIndicator.StartAnimating( );
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                BusyIndicator.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( BusyIndicator.Layer.BackgroundColor );
            }

            protected override void setBorderColor( uint borderColor )
            {
                BusyIndicator.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( BusyIndicator.Layer.BorderColor );
            }

            protected override uint getColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( BusyIndicator.Color );
            }

            protected override void setColor( uint value )
            {
                BusyIndicator.Color = Rock.Mobile.UI.Util.GetUIColor( value );
            }

            protected override float getBorderWidth()
            {
                // not supported for progress bars
                return (float) 0.00f;
            }
            protected override void setBorderWidth( float width )
            {
                // not supported for progress bars
            }

            protected override float getOpacity( )
            {
                return BusyIndicator.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                BusyIndicator.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float) BusyIndicator.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                BusyIndicator.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return BusyIndicator.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                BusyIndicator.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return BusyIndicator.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                BusyIndicator.Frame = frame;
            }

            protected override PointF getPosition( )
            {
                return BusyIndicator.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                BusyIndicator.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return BusyIndicator.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                BusyIndicator.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return BusyIndicator.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                BusyIndicator.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( BusyIndicator );
            }

            public override void RemoveAsSubview( object obj )
            {
                //obj is only for Android, so we don't use it.
                BusyIndicator.RemoveFromSuperview( );
            }
        }
    }
}
#endif
