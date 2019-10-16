#if __ANDROID__
using System;
using System.Drawing;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Java.IO;
using Droid;
using Rock.Mobile.UI.DroidNative;
using System.IO;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Android implementation of a view
        /// </summary>
        public class DroidImageView : PlatformImageView
        {
            protected BorderedRectImageView ImageView { get; set; }
            protected Android.Graphics.Bitmap ImageRef { get; set; }
            protected uint _BackgroundColor { get; set; }
            protected uint _BorderColor { get; set; }

            // NOTE: For the image to display correctly, call setImageScaleType when creating the image.
            // Then, after the image is set, be sure to update its frame so it can redraw with the correct settings.
            public DroidImageView( )
            {
                ImageView = new BorderedRectImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                ImageView.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            }

            protected override void setImageScaleType( ScaleType scaleType )
            {
                //Note: we don't implement a "GetScaleType" because we just don't need it.
                
                // take the scaleType provided, and convert it to the appropriate android version.
                Android.Widget.ImageView.ScaleType type = Android.Widget.ImageView.ScaleType.FitCenter;
                switch( scaleType )
                {
                    case ScaleType.Center:
                    {
                        type = Android.Widget.ImageView.ScaleType.Center;
                        break;
                    }

                    case ScaleType.ScaleAspectFill:
                    {
                        type = Android.Widget.ImageView.ScaleType.CenterCrop;
                        break;
                    }

                    case ScaleType.ScaleAspectFit:
                    {
                        type = Android.Widget.ImageView.ScaleType.FitCenter;
                        break;
                    }
                }

                ImageView.SetScaleType( type );
            }

            public override void Destroy()
            {
                // free our C# reference so Dalvik can clean up 
                // the resources.
                if( ImageView != null && ImageView.Drawable != null )
                {
                    ImageView.Drawable.Dispose( );
                    ImageView.SetImageBitmap( null );
                }
                
                if ( ImageRef != null )
                {
                    ImageRef.Recycle( );
                    ImageRef.Dispose( );
                    ImageRef = null;
                }
            }

            protected override void setImage( MemoryStream imageStream )
            {
                if( imageStream != null )
                {
                    // if they requsted it, scale the image by the device's density so we get
                    // an image that isn't overly large.
                    BitmapFactory.Options decodeOptions = new BitmapFactory.Options( );
                    
                    ImageRef = BitmapFactory.DecodeStream( imageStream, null, decodeOptions );

                    ImageView.SetImageBitmap( ImageRef );
                    ImageView.LayoutParameters.Width = ImageRef.Width;
                    ImageView.LayoutParameters.Height = ImageRef.Height;
                }
                else
                {
                    ImageRef = null;
                    ImageView.SetImageBitmap( null );
                    ImageView.LayoutParameters.Width = 0;
                    ImageView.LayoutParameters.Height = 0;
                }
            }

            // Properties
            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                ImageView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                ImageView.Invalidate( );
            }

            protected override void setBorderColor( uint borderColor )
            {
                _BorderColor = borderColor;
                ImageView.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( borderColor ) );
                ImageView.Invalidate( );
            }

            protected override uint getBorderColor( )
            {
                return _BorderColor;
            }

            protected override float getBorderWidth()
            {
                return ImageView.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                ImageView.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return ImageView.Radius;
            }
            protected override void setCornerRadius( float radius )
            {
                ImageView.Radius = radius;
            }

            protected override float getOpacity( )
            {
                return ImageView.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                ImageView.Alpha = opacity;
            }

            protected override float getZPosition( )
            {
                //Android doesn't use/need a Z position for its layers. (It goes based on order added)
                return 0.0f;
            }

            protected override void setZPosition( float zPosition )
            {
                //Android doesn't use/need a Z position for its layers. (It goes based on order added)
            }

            protected override RectangleF getBounds( )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                return new RectangleF( 0, 0, ImageView.LayoutParameters.Width, ImageView.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                ImageView.LayoutParameters.Width = ( int )bounds.Width;
                ImageView.LayoutParameters.Height = ( int )bounds.Height;

                ImageView.RequestLayout( );
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( ImageView.GetX( ), ImageView.GetY( ), ImageView.LayoutParameters.Width, ImageView.LayoutParameters.Height );
                return frame;
            }

            protected override void setFrame( RectangleF frame )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                setPosition( new System.Drawing.PointF( frame.X, frame.Y ) );

                RectangleF bounds = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );
                setBounds( bounds );
            }

            protected override System.Drawing.PointF getPosition( )
            {
                return new System.Drawing.PointF( ImageView.GetX( ), ImageView.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                ImageView.SetX( position.X );
                ImageView.SetY( position.Y );
            }

            protected override bool getHidden( )
            {
                return ImageView.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                ImageView.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                // doesn't matter if we return this or regular Focusable,
                // because we set them both, guaranteeing the same value.
                return ImageView.FocusableInTouchMode;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                ImageView.FocusableInTouchMode = enabled;
                ImageView.Focusable = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( ImageView );
            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                view.RemoveView( ImageView );
            }

            protected override object getPlatformNativeObject( )
            {
                return ImageView;
            }
        }
    }
}
#endif
