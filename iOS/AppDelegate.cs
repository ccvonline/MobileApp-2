using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using AVFoundation;
using MobileApp.Shared.Config;
using MobileApp.Shared.PrivateConfig;
using HockeyApp.iOS;

namespace iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register( "AppDelegate" )]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;

        //protected static UIStoryboard Storyboard = UIStoryboard.FromName ("MainStoryboard", null);
        private SpringboardViewController Springboard { get; set; }

        public AppDelegate( ) : base()
        {
        }

        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching( UIApplication app, NSDictionary options )
        {
            
            // Register HockeyApp
            #if !DEBUG

                var manager = BITHockeyManager.SharedHockeyManager;
                manager.Configure( App.Shared.SecuredValues.iOS_HockeyApp_Id );
                manager.CrashManager.CrashManagerStatus = BITCrashManagerStatus.AutoSend;
                manager.StartManager();
                manager.Authenticator.AuthenticateInstallation();

            #endif

            // create a new window instance based on the screen size. If we're a phone launched in landscape (only possible on the iPhone 6+), 
            // force a portait layout.
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && UIScreen.MainScreen.Bounds.Height < UIScreen.MainScreen.Bounds.Width )
            {
                window = new UIWindow( new CoreGraphics.CGRect( 0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width ) );
            }
            else
            {
                // for ipads or portait phones, use the default
                window = new UIWindow( UIScreen.MainScreen.Bounds );
            }
			
            // If you have defined a root view controller, set it here:
            Springboard = new SpringboardViewController( );
            window.RootViewController = Springboard;

            Rock.Mobile.Util.URL.Override.SetAppUrlOverrides( PrivateGeneralConfig.App_URL_Overrides );

            // make the window visible
            window.MakeKeyAndVisible( );

            // request the Playback category session
            NSError error;
            AVAudioSession instance = AVAudioSession.SharedInstance();
            instance.SetCategory(new NSString("AVAudioSessionCategoryPlayback"), AVAudioSessionCategoryOptions.MixWithOthers, out error);
            instance.SetMode(new NSString("AVAudioSessionModeDefault"), out error);
            instance.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);
			
            return true;
        }


        public override void OnActivated(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("OnActivated called, App is active.");

            Springboard.OnActivated( );

        }
        public override void WillEnterForeground(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("App will enter foreground");

            Springboard.WillEnterForeground( );

        }
        public override void OnResignActivation(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("OnResignActivation called, App moving to inactive state.");

            Springboard.OnResignActive( );
        }
        public override void DidEnterBackground(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("App entering background state.");

            Springboard.DidEnterBackground( );

        }

        // not guaranteed that this will run
        public override void WillTerminate(UIApplication application)
        {   
            Rock.Mobile.Util.Debug.WriteLine("App is terminating.");
            Springboard.WillTerminate( );
        }
    }
}

