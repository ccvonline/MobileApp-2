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

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Android implementation of a view
        /// </summary>
        public class DroidBusyIndicator : PlatformBusyIndicator
        {
            protected ProgressBar ProgressBar { get; set; }
            protected uint _BackgroundColor { get; set; }

            public DroidBusyIndicator( )
            {
                ProgressBar = new ProgressBar( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                ProgressBar.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                ProgressBar.Indeterminate = true;
            }

            // Properties
            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                ProgressBar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                ProgressBar.Invalidate( );
            }

            protected override uint getColor( )
            {
                // not supported for android progress bars
                return 0;
            }

            protected override void setColor( uint value )
            {
                // not supported for android progress bars
            }

            protected override void setBorderColor( uint borderColor )
            {
                // not supported for progress bars
            }

            protected override uint getBorderColor( )
            {
                return 0;
            }

            protected override float getBorderWidth()
            {
                // not supported for progress bars
                return 0;
            }
            protected override void setBorderWidth( float width )
            {
                // not supported for progress bars
            }

            protected override float getOpacity( )
            {
                return ProgressBar.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                ProgressBar.Alpha = opacity;
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
                return new RectangleF( 0, 0, ProgressBar.LayoutParameters.Width, ProgressBar.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                ProgressBar.LayoutParameters.Width = ( int )bounds.Width;
                ProgressBar.LayoutParameters.Height = ( int )bounds.Height;
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( ProgressBar.GetX( ), ProgressBar.GetY( ), ProgressBar.LayoutParameters.Width, ProgressBar.LayoutParameters.Height );
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
                return new System.Drawing.PointF( ProgressBar.GetX( ), ProgressBar.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                ProgressBar.SetX( position.X );
                ProgressBar.SetY( position.Y );
            }

            protected override bool getHidden( )
            {
                return ProgressBar.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                ProgressBar.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                // doesn't matter if we return this or regular Focusable,
                // because we set them both, guaranteeing the same value.
                return ProgressBar.FocusableInTouchMode;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                ProgressBar.FocusableInTouchMode = enabled;
                ProgressBar.Focusable = enabled;
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( ProgressBar );
            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                view.RemoveView( ProgressBar );
            }
        }
    }
}
#endif
