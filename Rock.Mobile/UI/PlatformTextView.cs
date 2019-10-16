using System;

namespace Rock.Mobile
{
    namespace UI
    {
        // put common utility things here (enums, etc)
        public enum TextAlignment
        {
            Left,
            Center,
            Right,
            Justified,
            Natural
        }

        // This is only for iOS, Android's can't be configured.
        // Enums match iOS.
        public enum KeyboardAppearanceStyle
        {
            Light,
            Dark
        }

        public enum AutoCapitalizationType
        {
            None,
            Words,
            Sentences,
            All
        }

        public enum AutoCorrectionType
        {
            Default,
            No,
            Yes
        }

        /// <summary>
        /// The base text field that provides an interface to platform specific text fields.
        /// </summary>
        public abstract class PlatformTextView : PlatformBaseLabelUI
        {
            public static PlatformTextView Create( )
            {
                #if __IOS__
                return new iOSTextView( );
                #endif

                #if __ANDROID__
                return new DroidTextView( );
                #endif

                #if __WIN__
                return new WinTextView( );
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

            public bool ScaleHeightForText
            {
                get { return getScaleHeightForText( ); }
                set { setScaleHeightForText( value ); }
            }
            protected abstract bool getScaleHeightForText( );
            protected abstract void setScaleHeightForText( bool scale );

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

            public abstract void BecomeFirstResponder( );
            public abstract void ResignFirstResponder( );

            public abstract void AnimateOpen( bool becomeFirstResponder );
            public abstract void AnimateClosed( );

            public delegate void EditCallback( PlatformTextView textView );

            protected EditCallback OnEditCallback { get; set; }
            public void SetOnEditCallback( EditCallback onEditCallback )
            {
                OnEditCallback = onEditCallback;
            }
        }
    }
}
