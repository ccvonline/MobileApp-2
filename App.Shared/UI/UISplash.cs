using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;

namespace App.Shared.UI
{
    public class UISplash
    {
        public PlatformView View { get; set; }
        public PlatformImageView ImageBG { get; set; }
        public PlatformImageView ImageLogo { get; set; }

        public UISplash( )
        {
        }

        public delegate void OnCompletion( );
        OnCompletion OnCompletionCallback;

        public void Create( object masterView, string backgroundImageName, string logoImageName, RectangleF frame, OnCompletion onCompletion )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = 0;
            View.AddAsSubview( masterView );

            ImageBG = PlatformImageView.Create( true );
            ImageBG.BackgroundColor = ControlStylingConfig.OOBE_Splash_BG_Color;
            ImageBG.AddAsSubview( masterView );

            // if a background image was provided, use that.
            if ( string.IsNullOrEmpty( backgroundImageName ) == false )
            {
                MemoryStream stream = Rock.Mobile.IO.AssetConvert.AssetToStream( backgroundImageName );
                stream.Position = 0;
                ImageBG.Image = stream;
                ImageBG.SizeToFit( );
                ImageBG.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
                stream.Dispose( );
            }

            MemoryStream logoStream = Rock.Mobile.IO.AssetConvert.AssetToStream( logoImageName );
            logoStream.Position = 0;
            ImageLogo = PlatformImageView.Create( true );
            ImageLogo.AddAsSubview( masterView );
            ImageLogo.Image = logoStream;
            ImageLogo.SizeToFit( );
            ImageLogo.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            logoStream.Dispose( );

            OnCompletionCallback = onCompletion;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            ImageLogo.Destroy( );
            ImageBG.Destroy( );
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            ImageBG.Frame = View.Frame;

            ImageLogo.Frame = new RectangleF( (( View.Frame.Width - ImageLogo.Frame.Width ) / 2), (( View.Frame.Height - ImageLogo.Frame.Height ) / 2) + 2, ImageLogo.Frame.Width, ImageLogo.Frame.Height );
        }

        public void PerformStartup( )
        {
            // Fade OUT the logo
            SimpleAnimator_Float imageAlphaAnim = new SimpleAnimator_Float( ImageLogo.Opacity, 0.00f, .13f, delegate(float percent, object value )
                {
                    ImageLogo.Opacity = (float)value;
                },
                null );
            imageAlphaAnim.Start( );

            // Scale UP the logo
            SimpleAnimator_SizeF imageSizeAnim = new SimpleAnimator_SizeF( ImageLogo.Frame.Size, new SizeF( View.Frame.Width, View.Frame.Height ), .25f, delegate(float percent, object value )
                {
                    SizeF imageSize = (SizeF)value;
                    ImageLogo.Frame = new RectangleF( ( View.Frame.Width - imageSize.Width ) / 2, ( View.Frame.Height - imageSize.Height ) / 2, imageSize.Width, imageSize.Height );
                },
                delegate 
                {
                    // do this ON the UI thread
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            OnCompletionCallback( );
                        });
                } );
            imageSizeAnim.Start( );
        }
    }
}
