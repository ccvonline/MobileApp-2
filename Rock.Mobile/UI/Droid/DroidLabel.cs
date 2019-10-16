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
using Android.Util;
using System.Collections.Generic;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Android implementation of a text label.
        /// </summary>
        public class DroidLabel : PlatformLabel
        {
            /// <summary>
            /// The amount to scale the border by relative to the text width.
            /// Useful if a using gradiants that fade out too early
            /// </summary>
            static float BORDER_WIDTH_SCALER = 0.99f;

            /// <summary>
            /// The view that draws the underline for the word.
            /// </summary>
            protected View UnderlineView { get; set; }

            protected BorderedRectTextView Label { get; set; }
            protected uint _BorderColor { get; set; }
            protected uint _BackgroundColor { get; set; }
            protected uint _TextColor { get; set; }

            public DroidLabel( )
            {
                Label = new BorderedRectTextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                Label.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            }

            // Properties
            public override void AddUnderline( )
            {
                if ( UnderlineView == null )
                {
                    // Define a gradiant underline that will be shown underneath the text
                    int[] colors = new int[] { int.MaxValue, int.MaxValue };
                    GradientDrawable border = new GradientDrawable( GradientDrawable.Orientation.LeftRight, colors );
                    border.SetGradientType( GradientType.LinearGradient );

                    UnderlineView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    UnderlineView.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    UnderlineView.Background = border;

                    UnderlineView.LayoutParameters.Height = 2;
                }
            }

            public override void SetFont( string fontName, float fontSize )
            {
                try
                {
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( fontName );
                    Label.SetTypeface( fontFace, TypefaceStyle.Normal );
                    Label.SetTextSize( Android.Util.ComplexUnitType.Dip, fontSize );
                } 
                catch
                {
                    throw new Exception( string.Format( "Unable to load font: {0}", fontName ) );
                }
            }

            protected override uint getBackgroundColor()
            {
                return _BackgroundColor;
            }

            protected override void setBackgroundColor( uint backgroundColor )
            {
                _BackgroundColor = backgroundColor;
                Label.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( backgroundColor ) );
                Label.Invalidate( );
            }

            protected override void setBorderColor( uint borderColor )
            {
                _BorderColor = borderColor;
                Label.SetBorderColor( Rock.Mobile.UI.Util.GetUIColor( borderColor ) );
                Label.Invalidate( );
            }

            protected override uint getBorderColor( )
            {
                return _BorderColor;
            }

            protected override float getBorderWidth()
            {
                return Label.BorderWidth;
            }
            protected override void setBorderWidth( float width )
            {
                Label.BorderWidth = width;
            }

            protected override float getCornerRadius()
            {
                return Label.Radius;
            }
            protected override void setCornerRadius( float radius )
            {
                Label.Radius = radius;
            }

            protected override float getOpacity( )
            {
                return Label.Alpha;
            }

            protected override void setOpacity( float opacity )
            {
                Label.Alpha = opacity;
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
                return new RectangleF( 0, 0, Label.LayoutParameters.Width, Label.LayoutParameters.Height );
            }

            protected override void setBounds( RectangleF bounds )
            {
                //Bounds is simply the localSpace coordinates of the edges.
                // NOTE: On android we're not supporting a non-0 left/top. I don't know why you'd EVER
                // want this, but it's possible to set on iOS.
                Label.LayoutParameters.Width = ( int )bounds.Width;
                Label.SetMaxWidth( Label.LayoutParameters.Width );
                Label.LayoutParameters.Height = ( int )bounds.Height;

                UpdateUnderline();
            }

            protected override RectangleF getFrame( )
            {
                //Frame is the transformed bounds to include position, so the Right/Bottom will be absolute.
                RectangleF frame = new RectangleF( Label.GetX( ), Label.GetY( ), Label.LayoutParameters.Width, Label.LayoutParameters.Height );
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
                return new System.Drawing.PointF( Label.GetX( ), Label.GetY( ) );
            }

            protected override void setPosition( System.Drawing.PointF position )
            {
                // to position the border, first get the amount we'll be moving
                float deltaX = position.X - Label.GetX();
                float deltaY = position.Y - Label.GetY();

                Label.SetX( position.X );
                Label.SetY( position.Y );

                // now adjust the border by only the difference
                if ( UnderlineView != null )
                {
                    UnderlineView.SetX( UnderlineView.GetX( ) + deltaX );
                    UnderlineView.SetY( UnderlineView.GetY( ) + deltaY );
                }
            }

            protected override uint getTextColor( )
            {
                return _TextColor;
            }

            protected override void setTextColor( uint color )
            {
                _TextColor = color;
                Label.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( color ) );
            }

            protected override string getText( )
            {
                return Label.Text;
            }

            protected override void setText( string text )
            {
                Label.Text = text;
            }

            protected override bool getHidden( )
            {
                return Label.Visibility == ViewStates.Gone ? true : false;
            }

            protected override void setHidden( bool hidden )
            {
                Label.Visibility = hidden == true ? ViewStates.Gone : ViewStates.Visible;
            }

            protected override bool getUserInteractionEnabled( )
            {
                // doesn't matter if we return this or regular Focusable,
                // because we set them both, guaranteeing the same value.
                return Label.FocusableInTouchMode;
            }

            protected override void setUserInteractionEnabled( bool enabled )
            {
                Label.FocusableInTouchMode = enabled;
                Label.Focusable = enabled;
            }

            protected override TextAlignment getTextAlignment( )
            {
                // gonna have to do a stupid transform
                switch( Label.Gravity )
                {
                    case GravityFlags.Center:
                    {
                        return TextAlignment.Center;
                    }
                    case GravityFlags.Left:
                    {
                        return TextAlignment.Left;
                    }
                    case GravityFlags.Right:
                    {
                        return TextAlignment.Right;
                    }
                    default:
                    {
                        return TextAlignment.Left;
                    }
                }
            }

            protected override void setTextAlignment( TextAlignment alignment )
            {
                // gonna have to do a stupid transform
                switch( alignment )
                {
                    case TextAlignment.Center:
                    {
                        Label.Gravity = GravityFlags.Center;
                        break;
                    }
                    case TextAlignment.Left:
                    {
                        Label.Gravity = GravityFlags.Left;
                        break;
                    }
                    case TextAlignment.Right:
                    {
                        Label.Gravity = GravityFlags.Right;
                        break;
                    }
                    default:
                    {
                        Label.Gravity = GravityFlags.Left;
                        break;
                    }
                }
            }

            public override void AddAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android AddAsSubview must be a ViewGroup or subclass." );
                }

                view.AddView( Label );

                if ( UnderlineView != null )
                {
                    view.AddView( UnderlineView );
                }

            }

            public override void RemoveAsSubview( object masterView )
            {
                ViewGroup view = masterView as ViewGroup;
                if( view == null )
                {
                    throw new Exception( "Object passed to Android RemoveAsSubview must be a ViewGroup or subclass." );
                }

                if ( UnderlineView != null )
                {
                    view.RemoveView( UnderlineView );
                }

                view.RemoveView( Label );
            }

            public override void SizeToFit( )
            {
                // create the specs we want for measurement
                int widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec( Label.LayoutParameters.Width, MeasureSpecMode.Unspecified );
                int heightMeasureSpec = View.MeasureSpec.MakeMeasureSpec( 0, MeasureSpecMode.Unspecified );

                // measure the label given the current width/height/text
                Label.Measure( widthMeasureSpec, heightMeasureSpec );

                Label.LayoutParameters.Width = Label.MeasuredWidth;
                Label.LayoutParameters.Height = Label.MeasuredHeight;

                Label.SetMaxWidth( Label.LayoutParameters.Width );

                UpdateUnderline();
            }

            void UpdateUnderline()
            {
                if ( UnderlineView != null )
                {
                    // first get the Y starting point of the font, relative to the control.
                    // (the control's top might start 5 pixels above the actual font, for example)
                    float fontYStart = ( Label.LayoutParameters.Height - Label.TextSize ) / 2;

                    // Update the Y position of the border here, because 
                    // if the HEIGHT of the label changed, our starting Y position must change
                    // so we stay at the bottom of the label.
                    float borderYOffset = ( fontYStart + Label.TextSize );

                    UnderlineView.SetY( (int)Label.GetY( ) + (int)borderYOffset );


                    // Same for X
                    UnderlineView.LayoutParameters.Width = (int)( (float)Label.LayoutParameters.Width * BORDER_WIDTH_SCALER );

                    float borderXOffset = ( Label.LayoutParameters.Width - UnderlineView.LayoutParameters.Width ) / 2;
                    UnderlineView.SetX( (int)Label.GetX( ) + (int)borderXOffset );
                }
            }

            public override float GetFade()
            {
                return 1.0f;
            }

            public override void SetFade( float fadeAmount )
            {
            }

            public override void AnimateToFade( float fadeAmount )
            {
            }
        }
    }
}
#endif
