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
	class SplashViewController : UIViewController
	{
        UISplash SplashView { get; set; }

        public SpringboardViewController Springboard { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // this is totally a hack, but in order to seamlessly transition from the splash screen
            // to our logo, we need to use a PER-DEVICE image. Sigh.
            string backgroundName = GetSplashBackground( UIKit.UIScreen.MainScreen.ApplicationFrame.Size, UIKit.UIScreen.MainScreen.Scale );
            string logoName = OOBEViewController.GetSplashLogo( UIKit.UIScreen.MainScreen.ApplicationFrame.Size, UIKit.UIScreen.MainScreen.Scale );

            SplashView = new UISplash();
            SplashView.Create( View, backgroundName, logoName, View.Frame.ToRectF( ), delegate
                {
                    Springboard.SplashComplete( );
                } );
        }

        string GetSplashBackground( CGSize screenSize, nfloat scalar )
        {
            nfloat nativeWidth = screenSize.Width * scalar;
            nfloat nativeHeight = screenSize.Height * scalar;

            // default to iphone4s, cause..why not.
            string imageName = "splash_bg_iphone4s.png";

            // compare the dimensions with the known iDevice sizes, and return the appropriate string.
            if ( nativeWidth == 640 && nativeHeight == 960 )
            {
                imageName = "splash_bg_iphone4s.png";
            }
            else if ( nativeWidth == 640 && nativeHeight == 1136 )
            {
                imageName = "splash_bg_iphone5.png";
            }
            else if ( nativeWidth == 750 && nativeHeight == 1334 )
            {
                imageName = "splash_bg_iphone6.png";
            }
            else if ( nativeWidth == 1242 && nativeHeight == 2208 )
            {
                imageName = "splash_bg_iphone6p.png";
            }
            else if ( nativeWidth == 2048 && nativeHeight == 1536 )
            {
                imageName = "splash_bg_ipad_landscape_2x.png";
            }
            else if ( nativeWidth == 1024 && nativeHeight == 768 )
            {
                imageName = "splash_bg_ipad_landscape.png";
            }
            else if ( nativeWidth == 1536 && nativeHeight == 2048 )
            {
                imageName = "splash_bg_ipad_portrait_2x.png";
            }
            else if ( nativeWidth == 768 && nativeHeight == 1024 )
            {
                imageName = "splash_bg_ipad_portrait.png";
            }

            return imageName;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            SplashView.PerformStartup( );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            SplashView.LayoutChanged( View.Bounds.ToRectF( ) );
        }
	}
}
