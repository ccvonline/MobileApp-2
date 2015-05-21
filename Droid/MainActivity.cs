using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Gms.Maps;
using Com.Localytics.Android;
using App.Shared.Config;

namespace Droid
{
    [Activity( Label = GeneralConfig.AndroidAppName, NoHistory = true, MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class Splash : Activity
    {
        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            // see if this device will support wide landscape (like, if it's a tablet)
            if ( MainActivity.SupportsLandscapeWide( this ) )
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
            }
            else
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            Window.AddFlags( WindowManagerFlags.Fullscreen );


            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Splash );

            System.Timers.Timer splashTimer = new System.Timers.Timer();
            splashTimer.Interval = 500;
            splashTimer.AutoReset = false;
            splashTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {

                    RunOnUiThread( delegate
                        {
                            // launch create order intent, which should be a FORM
                            Intent intent = new Intent(this, typeof(MainActivity));
                            StartActivity(intent);
                        });
                };

            splashTimer.Start( );
        }
    }

    //JHM 4-28 - In case we need to change the Localytics key on a per-config basis.
    //[Application]
    //[MetaData ("LOCALYTICS_APP_KEY", Value="b5da9a8d5e23b54319b5903-4d60e47a-edc4-11e4-adb1-005cf8cbabd8")]
    [Activity( Label = GeneralConfig.AndroidAppName, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize )]
    public class MainActivity : Activity
    {
        Springboard Springboard { get; set; }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            #if !DEBUG
            LocalyticsActivityLifecycleCallbacks callback = new LocalyticsActivityLifecycleCallbacks( this );
            Application.RegisterActivityLifecycleCallbacks( callback );
            #endif

            Window.AddFlags(WindowManagerFlags.Fullscreen);

            Rock.Mobile.PlatformSpecific.Android.Core.Context = this;

            // default our app to protrait mode, and let the notes change it.
            if ( SupportsLandscapeWide( ) )
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
            }
            else
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            DisplayMetrics metrics = Resources.DisplayMetrics;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Android Device detected dpi: {0}", metrics.DensityDpi ) );

            // Set our view from the "main" layout resource
            SetContentView( Resource.Layout.Main );

            // get the active task frame and give it to the springboard
            FrameLayout layout = FindViewById<FrameLayout>(Resource.Id.activetask);

            Rock.Mobile.UI.PlatformBaseUI.Init( );
            MapsInitializer.Initialize( this );

            Springboard = FragmentManager.FindFragmentById(Resource.Id.springboard) as Springboard;
            Springboard.SetActiveTaskFrame( layout );
        }

        /// <summary>
        /// Returns true if the device CAN display in landscape wide mode. This doesn't
        /// necessarily mean it IS in landscape wide mode.
        /// </summary>
        public static bool SupportsLandscapeWide( Context contextArg = null )
        {
            Context currContext = contextArg == null ? Rock.Mobile.PlatformSpecific.Android.Core.Context : contextArg;
            
            // get the current device configuration
            Android.Content.Res.Configuration currConfig = currContext.Resources.Configuration;

            if ( ( currConfig.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask ) >= Android.Content.Res.ScreenLayout.SizeLarge )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the device is CURRENTLY IN landscape wide mode.
        /// </summary>
        public static bool IsLandscapeWide( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // if it has the capacity for landscape wide, and is currently in landscape
            if ( MainActivity.SupportsLandscapeWide( ) == true && (currConfig.ScreenWidthDp > currConfig.ScreenHeightDp) == true )
            {
                // then yes, we're in landscape wide.
                return true;
            }

            return false;
        }

        public static bool IsLandscape( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // is our width greater?
            if ( currConfig.ScreenWidthDp > currConfig.ScreenHeightDp == true )
            {
                
                return true;
            }

            return false;
        }

        public static bool IsPortrait( )
        {
            Android.Content.Res.Configuration currConfig = Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources.Configuration;

            // is our width less?
            if ( currConfig.ScreenWidthDp < currConfig.ScreenHeightDp == true )
            {

                return true;
            }

            return false;
        }

        protected override void OnResume()
        {
            base.OnResume();

            OverridePendingTransition( Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out );
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            Intent = intent;
        }

        public override void OnBackPressed()
        {
            // only allow Back if the springboard OKs it.
            if ( Springboard.CanPressBack( ) )
            {
                base.OnBackPressed( );
            }
        }
    }
}
