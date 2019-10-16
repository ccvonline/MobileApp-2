#if __IOS__
using AudioToolbox;
#endif

namespace Rock.Mobile
{
    namespace Audio
    {
        public abstract class PlatformSoundEffect
        {
            /// <summary>
            /// Wrapper class that contains the platform-specific type of file handle
            /// </summary>
            public class SoundEffectHandle
            {
                #if __ANDROID__
                public SoundEffectHandle( int handle ) { Handle = handle; }
                public int Handle { get; private set; }
                #endif

                #if __IOS__
                public SoundEffectHandle( SystemSound handle ) { Handle = handle; }
                public SystemSound Handle { get; private set; }
                #endif
            }
            
            public static PlatformSoundEffect _Instance = null;
            public static PlatformSoundEffect Instance
            {
                get
                {
                    // if we haven't yet declared, create the platform correct audio
                    if( _Instance == null )
                    {
                        #if __ANDROID__
                        _Instance = new DroidSoundEffect( );
                        #endif

                        #if  __IOS__
                        _Instance = new iOSSoundEffect( );
                        #endif
                    }

                    return _Instance;
                }
            }

            public abstract SoundEffectHandle LoadSoundEffectAsset( string assetName );
            public abstract SoundEffectHandle LoadSoundEffectFile( string fileName );

            public abstract void ReleaseSoundEffect( SoundEffectHandle sfxHandle );

            public abstract void Play( SoundEffectHandle soundEffectHandle );
        }
    }
}
