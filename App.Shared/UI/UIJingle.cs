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
        public PlatformImageView Jingle_Pre_Image { get; set; }
        public PlatformImageView Jingle_Post_Image { get; set; }
        public PlatformButton JingleButton { get; set; }
        PlatformSoundEffect.SoundEffectHandle JingleHandle;

        public UIJingle( )
        {
        }

        public delegate void OnButtonTap( );
        OnButtonTap OnButtonTapCallback;

        public void Create( object masterView, string imagePreName, string imagePostName, RectangleF frame, OnButtonTap onButtonTapCallback )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.Frame = frame;
            View.AddAsSubview( masterView );

            MemoryStream preStream = Rock.Mobile.IO.AssetConvert.AssetToStream( imagePreName );
            preStream.Position = 0;
            Jingle_Pre_Image = PlatformImageView.Create( );
            Jingle_Pre_Image.AddAsSubview( View.PlatformNativeObject );
            Jingle_Pre_Image.Image = preStream;
            Jingle_Pre_Image.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFill;
            preStream.Dispose( );


            MemoryStream postStream = Rock.Mobile.IO.AssetConvert.AssetToStream( imagePostName );
            postStream.Position = 0;
            Jingle_Post_Image = PlatformImageView.Create( );
            Jingle_Post_Image.AddAsSubview( View.PlatformNativeObject );
            Jingle_Post_Image.Image = postStream;
            Jingle_Post_Image.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFill;
            postStream.Dispose( );


            JingleButton = PlatformButton.Create( );
            JingleButton.AddAsSubview( View.PlatformNativeObject );
            JingleButton.Text = "Got It!";
            JingleButton.BackgroundColor = 0;
            JingleButton.TextColor = 0;
            JingleButton.CornerRadius = 0;

            Jingle_Pre_Image.Hidden = false;
            Jingle_Post_Image.Hidden = true;

            OnButtonTapCallback = onButtonTapCallback;

            JingleHandle = PlatformSoundEffect.Instance.LoadSoundEffectAsset( "bell.wav" );

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
                Jingle_Post_Image.Hidden = false;

                if( jingleBellsPlaying == false )
                {
                    jingleBellsPlaying = true;
                    timer.Start( );
                    PlatformSoundEffect.Instance.Play( JingleHandle );
                }

                OnButtonTapCallback( );
            };
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            Jingle_Pre_Image.Destroy( );
            Jingle_Post_Image.Destroy( );

            PlatformSoundEffect.Instance.ReleaseSoundEffect( JingleHandle );
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            Jingle_Pre_Image.Frame = View.Frame;
            Jingle_Post_Image.Frame = View.Frame;
            JingleButton.Frame = View.Frame;
        }
    }
}
