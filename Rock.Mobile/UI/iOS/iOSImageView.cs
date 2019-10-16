#if __IOS__
using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using CoreText;
using Rock.Mobile.PlatformSpecific.Util;
using System.IO;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The iOS implementation of a view (don't confuse this with overriding an actual UIView, it contains one.)
        /// </summary>
        public class iOSImageView : PlatformImageView
        {
            protected UIImageView ImageView { get; set; }

            public iOSImageView( )
            {
                ImageView = new UIImageView( );
                ImageView.Layer.AnchorPoint = new PointF( 0, 0 );
            }

            protected override void setImageScaleType(ScaleType scaleType)
            {
                //Note: we don't implement a "GetScaleType" because we just don't need it.

                // take the scaleType provided, and convert it to the appropriate android version.
                UIViewContentMode contentMode = UIViewContentMode.Center;
                switch( scaleType )
                {
                    case ScaleType.Center:
                    {
                        contentMode = UIViewContentMode.Center;
                        break;
                    }

                    case ScaleType.ScaleAspectFill:
                    {
                        contentMode = UIViewContentMode.ScaleAspectFill;
                        break;
                    }

                    case ScaleType.ScaleAspectFit:
                    {
                        contentMode = UIViewContentMode.ScaleAspectFit;
                        break;
                    }
                }

                ImageView.ContentMode = contentMode;
            }

            public override void Destroy()
            {
                // nothing needs to happen on iOS.
            }

            protected override void setImage( MemoryStream imageStream )
            {
                if( imageStream != null )
                {
                    NSData imageData = NSData.FromStream( imageStream );
                    ImageView.Image = new UIImage( imageData, UIKit.UIScreen.MainScreen.Scale );
                    ImageView.Frame = new CGRect( 0, 0, ImageView.Image.Size.Width, ImageView.Image.Size.Height );
                }
                else
                {
                    ImageView.Image = null;
                }
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                ImageView.Layer.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( backgroundColor ).CGColor;
            }

            protected override uint getBackgroundColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( ImageView.Layer.BackgroundColor );
            }

            protected override void setBorderColor( uint borderColor )
            {
                ImageView.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( borderColor ).CGColor;
            }

            protected override uint getBorderColor( )
            {
                return Rock.Mobile.UI.Util.UIColorToInt( ImageView.Layer.BorderColor );
            }

            protected override float getBorderWidth()
            {
                return (float) ImageView.Layer.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                ImageView.Layer.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return (float) ImageView.Layer.CornerRadius;
            }
            protected override void setCornerRadius( float radius )
            {
                ImageView.Layer.CornerRadius = radius;
            }

            protected override float getOpacity( )
            {
                return ImageView.Layer.Opacity;
            }

            protected override void setOpacity( float opacity )
            {
                ImageView.Layer.Opacity = opacity;
            }

            protected override float getZPosition( )
            {
                return (float) ImageView.Layer.ZPosition;
            }

            protected override void setZPosition( float zPosition )
            {
                ImageView.Layer.ZPosition = zPosition;
            }

            protected override RectangleF getBounds( )
            {
                return ImageView.Bounds.ToRectF( );
            }

            protected override void setBounds( RectangleF bounds )
            {
                ImageView.Bounds = bounds;
            }

            protected override RectangleF getFrame( )
            {
                return ImageView.Frame.ToRectF( );
            }

            protected override void setFrame( RectangleF frame )
            {
                ImageView.Frame = frame;
            }

            protected override PointF getPosition( )
            {
                return ImageView.Layer.Position.ToPointF( );
            }

            protected override void setPosition( PointF position )
            {
                ImageView.Layer.Position = position;
            }

            protected override bool getHidden( )
            {
                return ImageView.Hidden;
            }

            protected override void setHidden( bool hidden )
            {
                ImageView.Hidden = hidden;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return ImageView.UserInteractionEnabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                ImageView.UserInteractionEnabled = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                // we know that masterView will be an iOS View.
                UIView view = masterView as UIView;
                if( view == null )
                {
                    throw new Exception( "Object passed to iOS AddAsSubview must be a UIView." );
                }

                view.AddSubview( ImageView );
            }

            public override void RemoveAsSubview( object obj )
            {
                //obj is only for Android, so we don't use it.
                ImageView.RemoveFromSuperview( );
            }

            protected override object getPlatformNativeObject( )
            {
                return ImageView;
            }
        }
    }
}
#endif
