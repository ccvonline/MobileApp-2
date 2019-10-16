using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base Platform View that provides an interface to platform specific views.
        /// </summary>
        public abstract class PlatformView : PlatformBaseUI
        {
            public static PlatformView Create( )
            {
#if __IOS__
                return new iOSView( );
#endif

#if __ANDROID__
                return new DroidView( );
#endif

#if __WIN__
                return new WinView( );
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

            public float CornerRadius
            {
                get { return getCornerRadius( ); }
                set { setCornerRadius( value ); }
            }
            protected abstract float getCornerRadius( );
            protected abstract void setCornerRadius( float width );
        }
    }
}

