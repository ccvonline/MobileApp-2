using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using AVFoundation;
using App.Shared.Config;
using LocalyticsBinding;
using App.Shared.PrivateConfig;
using CoreMotion;
using Rock.Mobile.Math;
using Rock.Mobile.Audio;

namespace iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register( "AppDelegate" )]
    public partial class AppDelegate : UIApplicationDelegate
    {
        //HACK: JINGLE BELLS
        public static bool JingleBellsEnabled = false;
        CMMotionManager MotionManager { get; set; }

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
#if !DEBUG
            LocalyticsBinding.Localytics.Integrate( GeneralConfig.iOS_Localytics_Key );
            if( app.ApplicationState != UIApplicationState.Background )
            {
                LocalyticsBinding.Localytics.OpenSession( );
            }
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

            // HACK: JINGLE BELLS
            bool jingleBellsPlaying = false;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 1000;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
            {
                jingleBellsPlaying = false;
            };

            PlatformSoundEffect.SoundEffectHandle JingleHandle;
            JingleHandle = PlatformSoundEffect.Instance.LoadSoundEffectAsset( "bell.wav" );
            Vector3 LastPhonePosition = null;

            MotionManager = new CMMotionManager( );
            if( MotionManager.AccelerometerAvailable )
            {
                MotionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, (data, motionError) =>
                {
                    if( LastPhonePosition == null )
                    {
                        LastPhonePosition = new Vector3( (float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z );
                    }
                    else
                    {
                        Vector3 currPos = new Vector3( (float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z );

                        Vector3 delta = LastPhonePosition - currPos;

                        float changeRate = Vector3.Magnitude( delta );

                        if( changeRate > 2.0f )
                        {
                            LastPhonePosition = currPos;
                            Console.WriteLine( "Phone Shook: {0}\r\n", changeRate );

                            if( jingleBellsPlaying == false && JingleBellsEnabled == true )
                            {
                                PlatformSoundEffect.Instance.Play( JingleHandle );
                                jingleBellsPlaying = true;

                                timer.Start( );
                            }
                        }
                    }
                });
                ///
            }
			
            return true;
        }

        public override void OnActivated(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("OnActivated called, App is active.");

            Springboard.OnActivated( );
#if !DEBUG
            LocalyticsBinding.Localytics.OpenSession( );
            LocalyticsBinding.Localytics.Upload( );
#endif
        }
        public override void WillEnterForeground(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("App will enter foreground");

            Springboard.WillEnterForeground( );

#if !DEBUG
            LocalyticsBinding.Localytics.OpenSession( );
            LocalyticsBinding.Localytics.Upload( );
#endif
        }
        public override void OnResignActivation(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("OnResignActivation called, App moving to inactive state.");

            // HACK: JINGLE BELLS
            JingleBellsEnabled = false;

            Springboard.OnResignActive( );
        }
        public override void DidEnterBackground(UIApplication application)
        {
            Rock.Mobile.Util.Debug.WriteLine("App entering background state.");

            // HACK: JINGLE BELLS
            JingleBellsEnabled = false;

            Springboard.DidEnterBackground( );

#if !DEBUG
            LocalyticsBinding.Localytics.CloseSession( );
            LocalyticsBinding.Localytics.Upload( );
#endif
        }

        // not guaranteed that this will run
        public override void WillTerminate(UIApplication application)
        {   
            Rock.Mobile.Util.Debug.WriteLine("App is terminating.");
            Springboard.WillTerminate( );
        }
    }
}
