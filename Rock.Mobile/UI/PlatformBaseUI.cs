using System;
using System.Drawing;

namespace Rock.Mobile
{
    namespace UI
    {
        /// <summary>
        /// The base platformUI that provides an interface to platform specific UI controls. All our platform wrappers derive from this.
        /// </summary>
        public abstract class PlatformBaseUI
        {
            #if __ANDROID__
            /// <summary>
            /// This should be called once at startup to allow the device to init anything
            /// that can be done before actually creating / rendering UI.
            /// </summary>
            public static void Init( )
            {
                // Preload the alpha mask image.
                DroidNative.FadeTextView.CreateAlphaMask( Rock.Mobile.PlatformSpecific.Android.Core.Context, "spot_mask.png" );
            }
            #endif

            #if __IOS__
            /// <summary>
            /// This should be called once at startup to allow the device to init anything
            /// that can be done before actually creating / rendering UI.
            /// </summary>
            public static void Init( )
            {
            }
            #endif



            // Properties
            public uint BackgroundColor
            {
                set { setBackgroundColor( value ); }
                get { return getBackgroundColor( ); }
            }
            protected abstract void setBackgroundColor( uint backgroundColor );
            protected abstract uint getBackgroundColor( );

            public uint BorderColor
            {
                set { setBorderColor( value ); }
                get { return getBorderColor( ); }
            }
            protected abstract void setBorderColor( uint borderColor );
            protected abstract uint getBorderColor( );

            public float BorderWidth
            {
                get { return getBorderWidth( ); }
                set { setBorderWidth( value ); }
            }
            protected abstract float getBorderWidth( );
            protected abstract void setBorderWidth( float width );

            public float Opacity
            {
                get { return getOpacity( ); }
                set { setOpacity( value ); }
            }
            protected abstract float getOpacity( );
            protected abstract void setOpacity( float opacity );

            public float ZPosition
            {
                get { return getZPosition( ); }
                set { setZPosition( value ); }
            }
            protected abstract float getZPosition( );
            protected abstract void setZPosition( float zPosition );

            public RectangleF Bounds
            {
                get { return getBounds( ); }
                set { setBounds( value ); }
            }
            protected abstract RectangleF getBounds( );
            protected abstract void setBounds( RectangleF bounds );

            public RectangleF Frame
            {
                get { return getFrame( ); }
                set { setFrame( value ); }
            }
            protected abstract RectangleF getFrame( );
            protected abstract void setFrame( RectangleF frame );

            public PointF Position
            {
                get { return getPosition( ); }
                set { setPosition( value ); }
            }
            protected abstract PointF getPosition( );
            protected abstract void setPosition( PointF position );

            public bool Hidden
            {
                get { return getHidden( ); }
                set { setHidden( value ); }
            }
            protected abstract bool getHidden( );
            protected abstract void setHidden( bool hidden );

            public bool UserInteractionEnabled
            {
                get { return getUserInteractionEnabled( ); }
                set { setUserInteractionEnabled( value ); }
            }
            protected abstract bool getUserInteractionEnabled( );
            protected abstract void setUserInteractionEnabled( bool hidden );

            /// <summary>
            /// Adds _THIS_ object AS a subView of the masterView.
            /// Don't confuse this with adding a subView TO this object.
            /// </summary>
            public abstract void AddAsSubview( object masterView );

            /// <summary>
            /// Removes _THIS_ object AS a subView of the masterView.
            /// Don't confuse this with removing a subView FROM this object.
            /// </summary>
            public abstract void RemoveAsSubview( object masterView );
        }
    }
}

