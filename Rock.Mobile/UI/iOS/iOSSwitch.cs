#if __IOS__
using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;
using Rock.Mobile.UI.iOSNative;
using Rock.Mobile.PlatformSpecific.Util;
using Rock.Mobile.Animation;
using Rock.Mobile.PlatformSpecific.iOS.Graphics;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The iOS implementation of a ui switch
        /// </summary>
        public class iOSSwitch : PlatformSwitch
        {
            UISwitch Switch { get; set; }

            public iOSSwitch( )
            {
                Switch = new UISwitch( );
                Switch.Layer.AnchorPoint = new PointF( 0, 0 );
                Switch.ClipsToBounds = true;

                // setup a callback that will use any provided delegate the user gives us.
                Switch.TouchUpInside += (object sender, EventArgs e ) =>
                    {
                        if( OnCheckChangedDelegate != null )
                        {
                            OnCheckChangedDelegate( this );
                        }
                    };
            }

            // Properties
            protected override void setBorderColor( uint borderColor )
            {
                Switch.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Switch.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) Switch.Layer.BorderWidth;
            }

            protected override void setBorderWidth( float width )
            {
                Switch.Layer.BorderWidth = width;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                Switch.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( Switch.Layer.BackgroundColor );
            }

            protected override float getOpacity( )
            {
                return Switch.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                Switch.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float)Switch.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                Switch.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return Switch.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                Switch.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return Switch.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                Switch.Frame = frame;
            }

            protected override  PointF getPosition( )
            {
                return Switch.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                Switch.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return Switch.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                Switch.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return Switch.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Switch.UserInteractionEnabled = enabled;
            }

            protected override OnCheckChanged getCheckChanged( )
            {
                return OnCheckChangedDelegate;
            }

            protected override void setCheckChanged( OnCheckChanged checkChanged )
            {
                // store the callback they want. When TouchUpInside is called,
                // it will invoke this delegate
                OnCheckChangedDelegate = checkChanged;
            }

            protected override bool getChecked( )
            {
                return Switch.On;
            }

            protected override void setSwitchedOnColor( uint switchedOnColor )
            {
                Switch.TintColor = Rock.Mobile.UI.Util.GetUIColor( switchedOnColor );
            }

            protected override uint getSwitchedOnColor( )
            {
            	return Rock.Mobile.UI.Util.UIColorToInt( Switch.TintColor );
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( Switch );
            }

            public override void RemoveAsSubview( object obj )
            {
                // Obj is only needed by Android, so we ignore it
                Switch.RemoveFromSuperview( );
            }
        }
    }
}
#endif
