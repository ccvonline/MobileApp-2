
#if __IOS__
using System;
using AudioToolbox;
using AVFoundation;
using Foundation;

namespace Rock.Mobile
{
    namespace Audio
    {
        public class iOSSoundEffect : PlatformSoundEffect
        {
            public iOSSoundEffect()
            {
                
            }

            public override SoundEffectHandle LoadSoundEffectAsset( string assetName )
            {
                // build the full path to the asset, then convert to NSURL
                string fullPath = Foundation.NSBundle.MainBundle.BundlePath + "/" + assetName;
                NSUrl audioUrl = new NSUrl( fullPath );

                SystemSound soundEffect = new SystemSound( audioUrl );
                SoundEffectHandle sfxHandle = new SoundEffectHandle( soundEffect );

                return sfxHandle;
            }

            public override SoundEffectHandle LoadSoundEffectFile( string fileName )
            {
                return null;
            }

            public override void ReleaseSoundEffect( SoundEffectHandle sfxHandle)
            {
                sfxHandle.Handle.Close( );
            }

            public override void Play( SoundEffectHandle soundEffectHandle )
            {
                soundEffectHandle.Handle.PlaySystemSound( );
            }
        }
    }
}
#endif