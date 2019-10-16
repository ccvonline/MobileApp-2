using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base Platform View that provides an interface to platform specific views.
        /// </summary>
        public abstract class PlatformCircleView : PlatformBaseUI
        {
            public static PlatformCircleView Create( )
            {
                #if __IOS__
                return new iOSCircleView( );
                #endif

                #if __ANDROID__
                return new DroidCircleView( );
                #endif

                #if __WIN__
                return null;
                #endif
            }

            /// <summary>
            /// If we want to use UI broadly, one concession
            /// that needs to be made is the ability to get the 
            /// native view so that features that aren't implemented can
            /// be performed in native code.
            /// </summary>
            /// <value>The platform native object.</value>
            public object PlatformNativeObject
            {
                get { return getPlatformNativeObject( ); }
            }
            protected abstract object getPlatformNativeObject( );
        }
    }
}

