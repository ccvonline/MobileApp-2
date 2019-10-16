using System;
using Rock.Mobile.Animation;

namespace Rock.Mobile.UI
{
    public class Util
    {
        public static void AnimateBackgroundColor( PlatformBaseUI view, uint targetColor, SimpleAnimator.AnimationComplete onCompletion = null )
        {
            SimpleAnimator_Color animator = new SimpleAnimator_Color( view.BackgroundColor, targetColor, .15f, delegate(float percent, object value )
                {
                    view.BackgroundColor = (uint)value;
                }
                , onCompletion );
            animator.Start( );
        }

        public static void AnimateBackgroundOpacity( PlatformBaseUI view, float targetOpacity, SimpleAnimator.AnimationComplete onCompletion = null )
        {
            SimpleAnimator_Float animator = new SimpleAnimator_Float( view.Opacity, targetOpacity, .15f, delegate(float percent, object value )
                {
                    view.Opacity = (float)value;
                }
                , onCompletion );
            animator.Start( );
        }

        #if __ANDROID__
        public static Android.Graphics.Color GetUIColor( uint color )
        {
            // break out the colors as 255 components for android
            return new Android.Graphics.Color(
                ( byte )( ( color & 0xFF000000 ) >> 24 ),
                ( byte )( ( color & 0x00FF0000 ) >> 16 ), 
                ( byte )( ( color & 0x0000FF00 ) >> 8 ), 
                ( byte )( ( color & 0x000000FF ) ) );
        }

        public static uint UIColorToInt( Android.Graphics.Color color )
        {
            return (uint) (color.R << 24 | color.G << 16 | color.B << 8 | color.A);
        }
        #endif

        #if __WIN__
        public static System.Windows.Media.Color GetUIColor( uint color )
        {
            return new System.Windows.Media.Color( )
            {
                R = ( byte )( ( color & 0xFF000000 ) >> 24 ),
                G = ( byte )( ( color & 0x00FF0000 ) >> 16 ), 
                B = ( byte )( ( color & 0x0000FF00 ) >> 8 ), 
                A = ( byte )( ( color & 0x000000FF ) )
            };
        }

        public static uint UIColorToInt( System.Windows.Media.Color color )
        {
            return (uint) (color.R << 24 | color.G << 16 | color.B << 8 | color.A);
        }
        #endif

        #if __IOS__
        public static UIKit.UIColor GetUIColor( uint color )
        {
            // break out the colors and convert to 0-1 for iOS
            return new UIKit.UIColor(
            ( float )( ( color & 0xFF000000 ) >> 24 ) / 255,
            ( float )( ( color & 0x00FF0000 ) >> 16 ) / 255, 
            ( float )( ( color & 0x0000FF00 ) >> 8 ) / 255, 
            ( float )( ( color & 0x000000FF ) ) / 255 );
        }

        public static uint UIColorToInt( CoreGraphics.CGColor color )
        {
            nfloat colorR = 0, colorG = 0, colorB = 0, colorA = 0;

            nint numComponents = color.NumberOfComponents;
            if ( numComponents == 4 )
            {
                colorR = color.Components[ 0 ];
                colorG = color.Components[ 1 ];
                colorB = color.Components[ 2 ];
                colorA = color.Components[ 3 ];
            }

            return (uint)( colorR * 255.0f ) << 24 | (uint)( colorG * 255.0f ) << 16 | (uint)( colorB * 255.0f ) << 8 | (uint)( colorA * 255.0f );
        }

        public static uint UIColorToInt( UIKit.UIColor color )
        {
            nfloat colorR, colorG, colorB, colorA;
            color.GetRGBA( out colorR, out colorG, out colorB, out colorA );

            return (uint)( colorR * 255.0f ) << 24 | (uint)( colorG * 255.0f ) << 16 | (uint)( colorB * 255.0f ) << 8 | (uint)( colorA * 255.0f );
        }
        #endif
    }
}

