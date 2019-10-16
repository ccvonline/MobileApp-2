
#if __ANDROID__
using System;
using Android.Content.Res;
using Android.Media;

namespace Rock.Mobile
{
    namespace Audio
    {
        public class DroidSoundEffect : PlatformSoundEffect
        {
            SoundPool SoundPool;
            const int sMaxStreams = 4;

            public DroidSoundEffect()
            {
                // setup a soundpool suitable for game-style sound effects
                if( (int)Android.OS.Build.VERSION.SdkInt >= 21 )
                {
                    AudioAttributes attribs = new AudioAttributes.Builder().SetUsage( AudioUsageKind.Game ).SetContentType( AudioContentType.Sonification ).Build( );
                    SoundPool = new SoundPool.Builder().SetMaxStreams( sMaxStreams ).SetAudioAttributes( attribs ).Build( );
                }
                else
                {
                    SoundPool = new SoundPool( sMaxStreams, Stream.Music, 0 );
                }
            }

            public override SoundEffectHandle LoadSoundEffectAsset(string assetName )
            {
                // find this asset in the Assets resource
                AssetFileDescriptor fileDesc = Rock.Mobile.PlatformSpecific.Android.Core.Context.Assets.OpenFd( assetName );
                int handle = SoundPool.Load( fileDesc, 1 );

                SoundEffectHandle sfxHandle = new SoundEffectHandle( handle );
                return sfxHandle;
            }

            public override SoundEffectHandle LoadSoundEffectFile(string fileName )
            {
                return null;
            }

            public override void ReleaseSoundEffect( SoundEffectHandle sfxHandle)
            {
                SoundPool.Unload( sfxHandle.Handle );
            }

            public override void Play( SoundEffectHandle soundEffectHandle )
            {
                SoundPool.Play( soundEffectHandle.Handle, 1, 1, 1, 0, 1.0f );
            }
        }
    }
}
#endif