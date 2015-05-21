using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using App.Shared.UI;
using App.Shared.Config;
using Rock.Mobile.PlatformSpecific.Util;
using System.Drawing;
using CoreGraphics;
using Rock.Mobile.Animation;

namespace iOS
{
	class OOBEViewController : UIViewController
	{
        UIOOBE OOBEView { get; set; }

        public SpringboardViewController Springboard { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // this is totally a hack, but in order to seamlessly transition from the splash screen
            // to our logo, we need to use a PER-DEVICE image. Sigh.
            string imageName = GetSplashLogo( UIKit.UIScreen.MainScreen.ApplicationFrame.Size, UIKit.UIScreen.MainScreen.Scale );

            OOBEView = new UIOOBE();
            OOBEView.Create( View, "oobe_splash_bg.png", imageName, View.Frame.ToRectF( ), delegate(int index) 
                {
                    Springboard.OOBEOnClick( index );
                } );
        }

        public static string GetSplashLogo( CGSize screenSize, nfloat scalar )
        {
            nfloat nativeWidth = screenSize.Width * scalar;
            nfloat nativeHeight = screenSize.Height * scalar;

            // default to iphone4s, cause..why not.
            string imageName = "splash_logo_iphone4s.png";

            // compare the dimensions with the known iDevice sizes, and return the appropriate string.
            if ( nativeWidth == 640 && nativeHeight == 960 )
            {
                imageName = "splash_logo_iphone4s.png";
            }
            else if ( nativeWidth == 640 && nativeHeight == 1136 )
            {
                imageName = "splash_logo_iphone5.png";
            }
            else if ( nativeWidth == 750 && nativeHeight == 1334 )
            {
                imageName = "splash_logo_iphone6.png";
            }
            else if ( nativeWidth == 1242 && nativeHeight == 2208 )
            {
                imageName = "splash_logo_iphone6p.png";
            }
            else if ( nativeWidth == 2048 && nativeHeight == 1536 )
            {
                imageName = "splash_logo_ipad_landscape_2x.png";
            }
            else if ( nativeWidth == 1024 && nativeHeight == 768 )
            {
                imageName = "splash_logo_ipad_landscape.png";
            }
            else if ( nativeWidth == 1536 && nativeHeight == 2048 )
            {
                imageName = "splash_logo_ipad_portrait_2x.png";
            }
            else if ( nativeWidth == 768 && nativeHeight == 1024 )
            {
                imageName = "splash_logo_ipad_portrait.png";
            }

            return imageName;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            OOBEView.PerformStartup( );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            OOBEView.LayoutChanged( View.Bounds.ToRectF( ) );
        }
	}
}
