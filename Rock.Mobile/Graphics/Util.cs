using System;

namespace Rock.Mobile.Graphics
{
    public class Util
    {
        public static uint ScaleRGBAColor( uint color, uint scale, bool scaleAlpha )
        {
            uint r = ( color & 0xFF000000) >> 24;
            uint g = ( color & 0x00FF0000) >> 16;
            uint b = ( color & 0x0000FF00) >> 8;
            uint a = ( color & 0x000000FF);

            r /= scale;
            g /= scale;
            b /= scale;

            if ( scaleAlpha )
            {
                a /= scale;
            }

            return r << 24 | g << 16 | b << 8 | a;
        }

        public static float UnitToPx( float unit )
        {
#if __ANDROID__
            return global::Android.Util.TypedValue.ApplyDimension(global::Android.Util.ComplexUnitType.Dip, unit, Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.DisplayMetrics);
#elif __IOS__
            return unit;
#elif __WIN__
            return unit;
#endif
        }
    }
}

