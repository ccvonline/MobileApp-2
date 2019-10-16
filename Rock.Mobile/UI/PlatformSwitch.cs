using System;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// Platform agnostic "Switch" control, which toggles on / off.
        /// </summary>
        public abstract class PlatformSwitch : PlatformBaseUI
        {
            public static PlatformSwitch Create( )
            {
                #if __IOS__
                return new iOSSwitch( );
                #endif

                #if __ANDROID__
                return new DroidSwitch( );
                #endif

                #if __WIN__
                return null;
                #endif
            }

            public delegate void OnCheckChanged( PlatformSwitch switchObj );
            protected OnCheckChanged OnCheckChangedDelegate;

            public OnCheckChanged CheckedChanged
            {
                get { return getCheckChanged( ); }
                set { setCheckChanged( value ); }
            }
            protected abstract OnCheckChanged getCheckChanged( );
            protected abstract void setCheckChanged( OnCheckChanged checkChanged );

            public bool Checked { get { return getChecked( ); } }
            protected abstract bool getChecked( );

            // Properties
            public uint SwitchedOnColor
            {
            	set { setSwitchedOnColor( value ); }
            	get { return getSwitchedOnColor( ); }
            }
            protected abstract void setSwitchedOnColor( uint switchedOnColor );
            protected abstract uint getSwitchedOnColor( );
        }
    }
}
