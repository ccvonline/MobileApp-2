using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using System.IO;

namespace App.Shared.UI
{
    public class UISpecial
    {
        public PlatformView View { get; set; }
        public PlatformImageView Image { get; set; }
        public PlatformButton CloseButton { get; set; }
        public PlatformLabel Label { get; set; }

        public UISpecial( )
        {
        }

        public delegate void OnCompletion( );
        OnCompletion OnCompletionCallback;

        public void Create( object masterView, string logoImageName, bool scaleImage, RectangleF frame, OnCompletion onCompletion )
        {
            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.Frame = frame;
            View.AddAsSubview( masterView );

            MemoryStream logoStream = Rock.Mobile.IO.AssetConvert.AssetToStream( logoImageName );
            logoStream.Position = 0;
            Image = PlatformImageView.Create( scaleImage );
            Image.AddAsSubview( View.PlatformNativeObject );
            Image.Image = logoStream;
            Image.SizeToFit( );
            Image.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            logoStream.Dispose( );

            Label = PlatformLabel.Create( );
            Label.Text = "Hey you found me! I'm Jered, the mobile app developer here at CCV. If you see me around campus, say hi!";
            Label.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            Label.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            Label.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;
            Label.TextColor = ControlStylingConfig.Label_TextColor;
            Label.Bounds = new RectangleF( 0, 0, frame.Width * .75f, 0 );
            Label.SizeToFit( );
            Label.AddAsSubview( View.PlatformNativeObject );

            CloseButton = PlatformButton.Create( );
            CloseButton.AddAsSubview( View.PlatformNativeObject );
            CloseButton.Text = "Got It!";
            CloseButton.BackgroundColor = PrayerConfig.PrayedForColor;
            CloseButton.TextColor = ControlStylingConfig.Button_TextColor;
            CloseButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            CloseButton.SizeToFit( );
            CloseButton.ClickEvent = delegate(PlatformButton button) 
                {
                    OnCompletionCallback( );
                };

            OnCompletionCallback = onCompletion;
        }

        public void Destroy( )
        {
            // clean up resources (looking at you, Android)
            Image.Destroy( );
        }

        public void LayoutChanged( RectangleF frame )
        {
            View.Frame = new RectangleF( frame.Left, frame.Top, frame.Width, frame.Height );

            Image.Frame = new RectangleF( (( View.Frame.Width - Image.Frame.Width ) / 2), Rock.Mobile.Graphics.Util.UnitToPx( 40 ), Image.Bounds.Width, Image.Frame.Height );

            Label.Frame = new RectangleF( ( ( View.Frame.Width - Label.Frame.Width ) / 2 ), Image.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 50 ), Label.Bounds.Width, Label.Bounds.Height );

            CloseButton.Frame = new RectangleF( ( ( View.Frame.Width - CloseButton.Frame.Width ) / 2 ), Label.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 25 ), CloseButton.Bounds.Width, CloseButton.Bounds.Height );
        }
    }
}
