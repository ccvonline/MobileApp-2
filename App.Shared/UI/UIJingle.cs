using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;
using System.Collections.Generic;
using Rock.Mobile.Audio;

namespace App.Shared.UI
{
    public class UIJingle
    {
        public PlatformView View { get; set; }
        public PlatformImageView Jingle_Image { get; set; }
        public PlatformButton JingleButton { get; set; }
        PlatformSoundEffect.SoundEffectHandle JingleHandle;

        string ImageName { get; set; }

        public void Create( object masterView, string imageName, RectangleF frame )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.Frame = frame;
            View.AddAsSubview( masterView );

            ImageName = imageName;

            Jingle_Image = PlatformImageView.Create( );
            Jingle_Image.AddAsSubview( View.PlatformNativeObject );
            Jingle_Image.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFill;


            JingleButton = PlatformButton.Create( );
            JingleButton.AddAsSubview( View.PlatformNativeObject );
            JingleButton.Text = "Got It!";
            JingleButton.BackgroundColor = 0;
            JingleButton.TextColor = 0;
            JingleButton.CornerRadius = 0;


            bool jingleBellsPlaying = false;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 1000;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
            {
                jingleBellsPlaying = false;
            };

            JingleButton.ClickEvent = delegate(PlatformButton button) 
            {
                if( jingleBellsPlaying == false )
                {
                    jingleBellsPlaying = true;
                    timer.Start( );
                    PlatformSoundEffect.Instance.Play( JingleHandle );
                }
            };
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            Jingle_Image.Frame = View.Frame;
            JingleButton.Frame = View.Frame;
        }

        public void LoadResources( )
        {
            MemoryStream imageStream = Rock.Mobile.IO.AssetConvert.AssetToStream( ImageName );
            imageStream.Position = 0;
            Jingle_Image.Image = imageStream;
            imageStream.Dispose( );

            JingleHandle = PlatformSoundEffect.Instance.LoadSoundEffectAsset( "bell.wav" );
        }

        public void FreeResources( )
        {
            // clean up resources (looking at you, Android)
            Jingle_Image.Destroy( );

            if( JingleHandle != null )
            {
                PlatformSoundEffect.Instance.ReleaseSoundEffect( JingleHandle );
            }
        }
    }
}
