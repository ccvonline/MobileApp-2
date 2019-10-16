using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base Platform View that provides an interface to platform specific views.
        /// </summary>
        public abstract class PlatformBusyIndicator : PlatformBaseUI
        {
            public static PlatformBusyIndicator Create( )
            {
                #if __IOS__
                return new iOSBusyIndicator( );
                #endif

                #if __ANDROID__
                return new DroidBusyIndicator( );
                #endif

                #if __WIN__
                return null;
                #endif
            }

            public uint Color
            {
                get { return getColor( ); }
                set { setColor( value ); }
            }
            protected abstract uint getColor( );
            protected abstract void setColor( uint value );
        }
    }
}

