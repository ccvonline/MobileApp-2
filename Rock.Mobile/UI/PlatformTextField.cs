using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base text field that provides an interface to platform specific text fields.
        /// Text Fields are subtly different in that they aren't designed to be multi-line.
        /// </summary>
        public abstract class PlatformTextField : PlatformBaseLabelUI
        {
            public static PlatformTextField Create( )
            {
                #if __IOS__
                return new iOSTextField( );
                #endif

                #if __ANDROID__
                return new DroidTextField( );
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

            public string Placeholder
            {
                get { return getPlaceholder( ); }
                set { setPlaceholder( value ); }
            }
            protected abstract string getPlaceholder( );
            protected abstract void setPlaceholder( string placeholder );

            public uint PlaceholderTextColor
            {
                get { return getPlaceholderTextColor( ); }
                set { setPlaceholderTextColor( value ); }
            }
            protected abstract uint getPlaceholderTextColor( );
            protected abstract void setPlaceholderTextColor( uint color );

            public KeyboardAppearanceStyle KeyboardAppearance
            {
                get { return getKeyboardAppearance( ); }
                set { setKeyboardAppearance( value ); }
            }
            protected abstract KeyboardAppearanceStyle getKeyboardAppearance( );
            protected abstract void setKeyboardAppearance( KeyboardAppearanceStyle style );

            public AutoCorrectionType AutoCorrectionType
            {
                get { return getAutoCorrectionType( ); }
                set { setAutoCorrectionType( value ); }
            }
            protected abstract AutoCorrectionType getAutoCorrectionType( );
            protected abstract void setAutoCorrectionType( AutoCorrectionType style );

            public AutoCapitalizationType AutoCapitalizationType
            {
                get { return getAutoCapitalizationType( ); }
                set { setAutoCapitalizationType( value ); }
            }
            protected abstract AutoCapitalizationType getAutoCapitalizationType( );
            protected abstract void setAutoCapitalizationType( AutoCapitalizationType style );

            public abstract void ResignFirstResponder( );
        }
    }
}
