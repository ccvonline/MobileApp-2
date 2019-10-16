using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base text field that provides an interface to platform specific text fields.
        /// </summary>
        public abstract class PlatformButton : PlatformBaseUI
        {
            public static PlatformButton Create( )
            {
                #if __IOS__
                return new iOSButton( );
                #endif

                #if __ANDROID__
                return new DroidButton( );
                #endif

                #if __WIN__
                return null;
                #endif
            }

            public delegate void OnClick( PlatformButton button );
            protected OnClick ClickCallbackDelegate;

            public OnClick ClickEvent
            {
                get { return getClickEvent( ); }
                set { setClickEvent( value ); }
            }
            protected abstract OnClick getClickEvent( );
            protected abstract void setClickEvent( OnClick clickEvent );

            public abstract void SetFont( string fontName, float fontSize );

            public uint TextColor
            {
                get { return getTextColor(); }
                set { setTextColor( value ); }
            }
            protected abstract uint getTextColor();
            protected abstract void setTextColor( uint color );

            public string Text
            {
                get { return getText( ); }
                set { setText( value ); }
            }
            protected abstract string getText( );
            protected abstract void setText( string text );

            public abstract void SizeToFit( );

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
