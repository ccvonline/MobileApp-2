
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

            bool IsActive { get; set; }

            UIResultView ResultView { get; set; }

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

                ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 
                    delegate 
                    { 
                        ResultView.Hide( );

                        if ( string.IsNullOrEmpty( Url ) == false )
                        {
                            WebLayout.LoadUrl( Url, PageLoaded );
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

                ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
            }

            public void DisplayUrl( string url, bool includeImpersonationToken )
            {
                Url = url;

                // if we're active, we can go ahead and display the url.
                // Otherwise, OnResume will take care of it.
                if ( IsActive == true )
                {
                    Activity.RunOnUiThread( delegate
                        {
                            if ( string.IsNullOrEmpty( Url ) == false )
                            {
                                // do they want the impersonation token?
                                if ( includeImpersonationToken )
                                {
                                    // try to get it
                                    MobileAppApi.TryGetImpersonationToken( 
                                        delegate( string impersonationToken )
                                        {
                                            // if we got it, append it and load
                                            if ( string.IsNullOrEmpty( impersonationToken ) == false )
                                            {
                                                WebLayout.LoadUrl( Url + "&" + impersonationToken, PageLoaded );
                                            }
                                            else
                                            {
                                                // otherwise just load
                                                WebLayout.LoadUrl( Url, PageLoaded );
                                            }
                                        });
                                }
                                else
                                {
                                    // no impersonation token requested. just load.
                                    WebLayout.LoadUrl( Url, PageLoaded );
                                }
                            }
                        } );
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
                    WebLayout.LoadUrl( Url, PageLoaded );
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
        }
    }
}

