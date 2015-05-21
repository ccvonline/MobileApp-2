using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using AVFoundation;
using MediaPlayer;
using App.Shared.Config;

namespace iOS
{
	partial class GIveMainPageUIViewController : TaskUIViewController
	{
        UIButton GiveButton { get; set; }

		public GIveMainPageUIViewController (IntPtr handle) : base (handle)
		{

		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ControlStyling.StyleBGLayer( GiveBannerLayer );

            GiveBanner.Text = App.Shared.Strings.GiveStrings.Header;
            ControlStyling.StyleUILabel( GiveBanner, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

            GiveButton = UIButton.FromType( UIButtonType.Custom );
            GiveButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                UIApplication.SharedApplication.OpenUrl( new NSUrl( GiveConfig.GiveUrl ) );
            };
            ControlStyling.StyleButton( GiveButton, App.Shared.Strings.GiveStrings.ButtonLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

            GiveButton.SizeToFit( );
            GiveButton.Frame = new CoreGraphics.CGRect( ( View.Bounds.Width - GiveButton.Bounds.Width ) / 2, ( View.Bounds.Height - GiveButton.Bounds.Height ) / 2, GiveButton.Bounds.Width, GiveButton.Bounds.Height );
            View.AddSubview( GiveButton );
        }
	}
}
