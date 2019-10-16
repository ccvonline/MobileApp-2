#if __ANDROID__
using System;

namespace Rock.Mobile.PlatformSpecific.Android
{
    public class Core
    {
        public static global::Android.Content.Context Context = null;

        public static bool IsOrientationUnlocked( global::Android.Content.Context context = null )
        {
            // if no context was provieded, simply use the one we presume to have.
            if ( context == null )
            {
                context = Core.Context;
            }

            // if the acceleromter rotation value is 1, they have NOT locked their orientation.
            if ( global::Android.Provider.Settings.System.GetInt( context.ContentResolver, global::Android.Provider.Settings.System.AccelerometerRotation ) == 1 )
            {
                return true;
            }

            // otherwise, they have.
            return false;
        }
    }
}
#endif
