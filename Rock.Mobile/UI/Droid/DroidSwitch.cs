#if __ANDROID__
using System;
using System.Drawing;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.App;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Rock.Mobile.UI.DroidNative;
using Android.Util;
using Android.Text;
using Java.Lang;
using Java.Lang.Reflect;
using Rock.Mobile.Animation;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Android implementation of a switch
        /// </summary>
        public class DroidSwitch : PlatformSwitch
        {
            protected BorderedRectSwitch Switch { get; set; }
            protected uint _BackgroundColor { get; set; }
            protected uint _BorderColor { get; set; }
            protected uint _TextColor { get; set; }

            public DroidSwitch( )
            {
                Switch = new BorderedRectSwitch( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                Switch.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                Switch.Checked = false;

                Switch.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e ) =>
                    {
                        if( OnCheckChangedDelegate != null )
                        {
                            OnCheckChangedDelegate( this );
                        }
                    };
            }

            // Properties
            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                Switch.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                Switch.Invalidate( );
            }

            protected override void setBorderColor( uint borderColor )
            {
                _BorderColor = borderColor;
                Switch.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( borderColor ) );
                Switch.Invalidate( );
            }

            protected override uint getBorderColor( )
            {
                return _BorderColor;
            }

            protected override float getBorderWidth()
            {
                return Switch.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                Switch.BorderWidth = width;
            }

            protected override float getOpacity( )
            {
                return Switch.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                Switch.Alpha = opacity;
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
                return new RectangleF( 0, 0, Switch.LayoutParameters.Width, Switch.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                Switch.SetMinWidth( (int)bounds.Width );
                Switch.SetMaxWidth( (int)bounds.Width );
                Switch.LayoutParameters.Width = (int)bounds.Width;
                Switch.LayoutParameters.Height = (int)bounds.Height;
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( Switch.GetX( ), Switch.GetY( ), Switch.LayoutParameters.Width, Switch.LayoutParameters.Height );
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
                return new System.Drawing.PointF( Switch.GetX( ), Switch.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                Switch.SetX( position.X );
                Switch.SetY( position.Y );
            }

            protected override bool getHidden( )
            {
                return Switch.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                Switch.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return Switch.Enabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Switch.Enabled = enabled;
            }

            protected override OnCheckChanged getCheckChanged( )
            {
                return OnCheckChangedDelegate;
            }

            protected override void setCheckChanged( OnCheckChanged checkChanged )
            {
                // store the callback they want. When checked is called,
                // it will invoke this delegate
                OnCheckChangedDelegate = checkChanged;
            }

            protected override bool getChecked( )
            {
            	return Switch.Checked;
            }

            protected override void setSwitchedOnColor( uint switchedOnColor )
            {
                // todo: implement; changing the color of a switch is not straight forward on android.
                // https://stackoverflow.com/questions/11253512/change-on-color-of-a-switch
                //Switch.ThumbDrawable.SetColorFilter( Android.Graphics.Color.Red, PorterDuff.Mode.Multiply );
                //Switch.TrackDrawable.SetColorFilter( Android.Graphics.Color.Red, PorterDuff.Mode.Multiply );
            }

            protected override uint getSwitchedOnColor( )
            {
                // todo: implement; changing the color of a switch is not straight forward on android.
                // https://stackoverflow.com/questions/11253512/change-on-color-of-a-switch
                //Switch.ThumbDrawable.SetColorFilter( Android.Graphics.Color.Red, PorterDuff.Mode.Multiply );
                //Switch.TrackDrawable.SetColorFilter( Android.Graphics.Color.Red, PorterDuff.Mode.Multiply );
                return 0;// Rock.Mobile.UI.Util.UIColorToInt( Switch.TintColor );
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( Switch );
            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                view.RemoveView( Switch );
            }
        }
    }
}
#endif
