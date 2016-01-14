
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.PlatformSpecific.Android.UI;
using App.Shared.UI;
using App.Shared.PrivateConfig;
using MobileApp;

namespace Droid
{
    namespace Tasks
    {
        public class TaskWebFragment : TaskFragment
        {
            /// <summary>
            /// Our wake lock that will keep the device from sleeping while notes are up.
            /// </summary>
            /// <value>The wake lock.</value>
            PowerManager.WakeLock WakeLock { get; set; }

            /// <summary>
            /// Can be set to true or false depending on a desire to prevent the phone from sleeping
            /// </summary>
            public bool DisableIdleTimer { get; set; }

            WebLayout WebLayout { get; set; }
            String Url { get; set; }
            bool IncludeImpersonationToken { get; set; }

            bool IsActive { get; set; }

            UIResultView ResultView { get; set; }

            /// <summary>
            /// Utility function to simplify handling a URL with or without an impersonation token, in an embedded webView OR external browser.
            /// </summary>
            /// <param name="launchesExternalBrowser">If set to <c>true</c> launches external browser.</param>
            /// <param name="usesImpersonationToken">If set to <c>true</c> uses impersonation token.</param>
            /// <param name="url">URL.</param>
            /// <param name="currTask">Curr task.</param>
            /// <param name="webFragment">Web fragment.</param>
            public static void HandleUrl( bool launchesExternalBrowser, bool usesImpersonationToken, string url, Task currTask, TaskWebFragment webFragment )
            {
                // are we launching a seperate browser?
                if ( launchesExternalBrowser == true )
                {
                    // do they also want the impersonation token?
                    if ( usesImpersonationToken )
                    {
                        // try to get it
                        MobileAppApi.TryGetImpersonationToken(
                            delegate( string impersonationToken )
                            {
                                // append the mobile platform
                                string fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( url, PrivateGeneralConfig.MobilePlatform );

                                // append the campus (this is part of their identity)
                                fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( fullUrl, string.Format( PrivateGeneralConfig.RockCampusContext, App.Shared.Network.RockMobileUser.Instance.GetRelevantCampus( ) ) );

                                // if we got the token, append it
                                if( string.IsNullOrEmpty( impersonationToken ) == false )
                                {
                                    fullUrl += "&" + impersonationToken;
                                }

                                // now fire off an intent.
                                Android.Net.Uri uri = Android.Net.Uri.Parse( fullUrl );

                                var intent = new Intent( Intent.ActionView, uri ); 
                                ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).StartActivity( intent );
                            } );

                    }
                    else
                    {
                        // pretty easy, just fire off an intent.
                        Android.Net.Uri uri = Android.Net.Uri.Parse( url );

                        var intent = new Intent( Intent.ActionView, uri ); 
                        ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).StartActivity( intent );
                    }
                }
                else
                {
                    // otherwise we're not, so its simpler
                    webFragment.DisplayUrl( url, usesImpersonationToken );
                    currTask.PresentFragment( webFragment, true );
                }
            }

            public TaskWebFragment( ) : base( )
            {
            }

            public override void OnCreate( Bundle savedInstanceState )
            {
                base.OnCreate( savedInstanceState );
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                if (container == null)
                {
                    // Currently in a layout without a container, so no reason to create our view.
                    return null;
                }

                RelativeLayout view = inflater.Inflate(Resource.Layout.TaskWebView, container, false) as RelativeLayout;
                view.SetOnTouchListener( this );

                WebLayout = new WebLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                WebLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                WebLayout.SetBackgroundColor( Android.Graphics.Color.Black );

                view.AddView( WebLayout );

                ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 
                    delegate 
                    { 
                        ResultView.Hide( );

                        if ( string.IsNullOrEmpty( Url ) == false )
                        {
                            ProcessUrl( );
                        }
                    } );

                // get our power management control
                PowerManager pm = PowerManager.FromContext( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                WakeLock = pm.NewWakeLock(WakeLockFlags.Full, "TaskWeb");

                return view;
            }

            public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
            {
                base.OnConfigurationChanged(newConfig);

                ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
            }

            void DisplayUrl( string url, bool includeImpersonationToken )
            {
                Url = url;
                IncludeImpersonationToken = includeImpersonationToken;

                // if we're active, we can go ahead and display the url.
                // Otherwise, OnResume will take care of it.
                if ( IsActive == true )
                {
                    Activity.RunOnUiThread( delegate
                        {
                            ProcessUrl( );
                        } );
                }
            }

            void ProcessUrl( )
            {
                if ( string.IsNullOrEmpty( Url ) == false )
                {
                    // do they want the impersonation token?
                    if ( IncludeImpersonationToken )
                    {
                        // try to get it
                        MobileAppApi.TryGetImpersonationToken( 
                            delegate( string impersonationToken )
                            {
                                // append the mobile platform
                                string fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( Url, PrivateGeneralConfig.MobilePlatform );

                                // also include their campus. this is personal data as well.
                                fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( fullUrl, string.Format( PrivateGeneralConfig.RockCampusContext, App.Shared.Network.RockMobileUser.Instance.GetRelevantCampus( ) ) );

                                // if we got it, append it and load
                                if ( string.IsNullOrEmpty( impersonationToken ) == false )
                                {
                                    fullUrl += "&" + impersonationToken;
                                }

                                Console.WriteLine( "Browsing to {0}", fullUrl );
                                WebLayout.LoadUrl( fullUrl, PrivateGeneralConfig.ExternalUrlToken, PageLoaded );
                            });
                    }
                    else
                    {
                        // no impersonation token requested. just load.
                        Console.WriteLine( "Browsing to {0}", Url );
                        WebLayout.LoadUrl( Url, PrivateGeneralConfig.ExternalUrlToken, PageLoaded );
                    }
                }
            }

            void PageLoaded( bool result, string forwardUrl )
            {
                if ( IsActive == true )
                {
                    if ( result == false )
                    {
                        ResultView.Show( GeneralStrings.Network_Status_FailedText, PrivateControlStylingConfig.Result_Symbol_Failed, GeneralStrings.Network_Result_FailedText, GeneralStrings.Retry );
                    }
                }
            }

            public bool OnBackPressed( )
            {
                if ( WebLayout != null )
                {
                    return WebLayout.OnBackPressed( );
                }
                return false;
            }

            public override void OnResume()
            {
                base.OnResume();

                ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                IsActive = true;

                if ( DisableIdleTimer == true )
                {
                    WakeLock.Acquire( );
                }

                if ( string.IsNullOrEmpty( Url ) == false )
                {
                    ProcessUrl( );
                }
            }

            public override void OnPause()
            {
                base.OnPause();

                if ( WakeLock.IsHeld == true )
                {
                    WakeLock.Release( );
                }

                IsActive = false;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                WebLayout.Destroy( );
            }
        }
    }
}

