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
        /// Android implementation of a text field.
        /// </summary>
        public class DroidButton : PlatformButton
        {
            protected BorderedRectButton Button { get; set; }
            protected uint _BackgroundColor { get; set; }
            protected uint _BorderColor { get; set; }
            protected uint _TextColor { get; set; }

            public DroidButton( )
            {
                Button = new BorderedRectButton( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                Button.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                Button.SetAllCaps( false );

                Button.Click += (object sender, EventArgs e ) =>
                    {
                        if( ClickCallbackDelegate != null )
                        {
                            ClickCallbackDelegate( this );
                        }
                    };
            }

            // Properties
            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( fontName );
                    Button.SetTypeface( fontFace, TypefaceStyle.Normal );
                    Button.SetTextSize( Android.Util.ComplexUnitType.Dip, fontSize );
                    Button.SetAllCaps( false );
                } 
                catch
                {
                    throw new System.Exception( string.Format( "Unable to load font: {0}", fontName ) );
                }
            }

            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                Button.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                Button.Invalidate( );
            }

            protected override void setBorderColor( uint borderColor )
            {
                _BorderColor = borderColor;
                Button.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( borderColor ) );
                Button.Invalidate( );
            }

            protected override uint getBorderColor( )
            {
                return _BorderColor;
            }

            protected override float getBorderWidth()
            {
                return Button.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                Button.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return Button.Radius;
            }
            protected override void setCornerRadius( float radius )
            {
                Button.Radius = radius;
            }

            protected override float getOpacity( )
            {
                return Button.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                Button.Alpha = opacity;
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
                return new RectangleF( 0, 0, Button.LayoutParameters.Width, Button.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                Button.SetMinWidth( (int)bounds.Width );
                Button.SetMaxWidth( (int)bounds.Width );
                Button.LayoutParameters.Width = (int)bounds.Width;
                Button.LayoutParameters.Height = (int)bounds.Height;
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( Button.GetX( ), Button.GetY( ), Button.LayoutParameters.Width, Button.LayoutParameters.Height );
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
                return new System.Drawing.PointF( Button.GetX( ), Button.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                Button.SetX( position.X );
                Button.SetY( position.Y );
            }

            protected override bool getHidden( )
            {
                return Button.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                Button.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                return Button.Enabled;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Button.Enabled = enabled;
            }

            protected override uint getTextColor( )
            {
                return _TextColor;
            }

            protected override void setTextColor( uint color )
            {
                _TextColor = color;
                Button.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( color ) );
            }

            protected override string getText( )
            {
                return Button.Text;
            }

            protected override void setText( string text )
            {
                Button.Text = text;
            }

            protected override OnClick getClickEvent( )
            {
                return ClickCallbackDelegate;
            }

            protected override void setClickEvent( OnClick clickEvent )
            {
                // store the callback they want. When Click is called,
                // it will invoke this delegate
                ClickCallbackDelegate = clickEvent;
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( Button );
            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new System.Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                view.RemoveView( Button );
            }

            public override void SizeToFit( )
            {
                // create the specs we want for measurement
                int widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec( Button.LayoutParameters.Width, MeasureSpecMode.Unspecified );
                int heightMeasureSpec = View.MeasureSpec.MakeMeasureSpec( 0, MeasureSpecMode.Unspecified );

                // measure the label given the current width/height/text
                Button.Measure( widthMeasureSpec, heightMeasureSpec );

                // update its width
                Button.SetMinWidth( Button.MeasuredWidth );
                Button.SetMaxWidth( Button.MeasuredWidth );

                Button.LayoutParameters.Width = Button.MeasuredWidth;
                Button.LayoutParameters.Height = Button.MeasuredHeight;
            }
        }
    }
}
#endif
