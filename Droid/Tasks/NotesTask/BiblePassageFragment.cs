using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using RestSharp;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using MobileApp.Shared.Config;
using MobileApp.Shared.UI;
using MobileApp.Shared.Strings;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.Network;
using App.Shared;

namespace Droid
{
    namespace Tasks
    {
        public class BiblePassageFragment : TaskFragment
        {

            /// <summary>
            /// The Web View that will display the HTML-formatted Bible passage.
            /// </summary>
            /// <value>The bible web view.</value>
            WebView PassageWebView { get; set; }

            /// <summary>
            /// Gets or sets the blocker view.
            /// </summary>
            /// <value>The blocker view.</value>
            UIBlockerView BlockerView { get; set; }

            /// <summary>
            /// Gets or sets the result view.
            /// </summary>
            /// <value>The result view.</value>
            UIResultView ResultView { get; set; }

            /// <summary>
            /// Our wake lock that will keep the device from sleeping while notes are up.
            /// </summary>
            /// <value>The wake lock.</value>
            PowerManager.WakeLock WakeLock { get; set; }

            /// <summary>
            /// This is the fully formatted Bible passage text.
            /// </summary>
            /// <value>The passage HTML string.</value>
            string PassageHTML { get; set; }

            string PassageCitation { get; set; }

            /// <summary>
            /// The base address of the HTTP request.
            /// </summary>
            /// <value>The address for the HTTP request.</value>
            string BibleAddress { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="T:iOS.BiblePassageViewController"/> requesting bible passage.
            /// </summary>
            /// <value><c>true</c> if requesting bible passage; otherwise, <c>false</c>.</value>
            bool RequestingBiblePassage { get; set; }

            /// <summary>
            /// True when WE are ready to create notes
            /// </summary>
            /// <value><c>true</c> if fragment ready; otherwise, <c>false</c>.</value>
            bool FragmentReady { get; set; }

            /// <summary>
            /// The current orientation of the device. We track this
            /// so we can know when it changes and only rebuild the notes then.
            /// </summary>
            /// <value>The orientation.</value>
            int OrientationState { get; set; }

            public BiblePassageFragment( string activeUrl )
            {
                BibleAddress = activeUrl;
            }

            public override void OnCreate( Bundle savedInstanceState )
            {
                base.OnCreate( savedInstanceState );
            }

            public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
            {

                if( container == null )
                {
                    // Currently in a layout without a container, so no reason to create our view.
                    return null;
                }

                RelativeLayout view = inflater.Inflate( Resource.Layout.BiblePassage, container, false ) as RelativeLayout;
                view.SetOnTouchListener( this );

                PassageWebView = new WebView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                PassageWebView.SetBackgroundColor( Android.Graphics.Color.Rgb( 28, 28, 28 ) );
                PassageWebView.SetPadding( 10, 0, 40, 0 );

                view.AddView( PassageWebView );
                view.SetBackgroundColor( Android.Graphics.Color.Rgb( 28, 28, 28 ) );

                // get our power management control
                PowerManager pm = PowerManager.FromContext( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                WakeLock = pm.NewWakeLock( WakeLockFlags.Full, "Bible Passage" );

                BlockerView = new UIBlockerView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

                ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ),
                    delegate
                    {
                        if( RequestingBiblePassage == false )
                        {
                            RetrieveBiblePassage( );
                        }
                    } );

                return view;

            }

            public override void OnStart( )
            {
                base.OnStart( );

                if( RequestingBiblePassage == false )
                {
                    RetrieveBiblePassage( );
                }
            }

            public override void OnConfigurationChanged( Android.Content.Res.Configuration newConfig )
            {
                base.OnConfigurationChanged( newConfig );

                ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetCurrentContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );

            }

            public override void OnResume( )
            {
                base.OnResume( );

                // make sure they're ok with rotation and didn't lock their phone's orientation
                if( Rock.Mobile.PlatformSpecific.Android.Core.IsOrientationUnlocked( ) )
                {
                    Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.FullSensor;
                }

                WakeLock.Acquire( );

                ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
            }

            public override void OnPause( )
            {
                base.OnPause( );

                FragmentReady = false;

                WakeLock.Release( );

                // if we don't support full widescreen, enable the reveal button
                if( MainActivity.SupportsLandscapeWide( ) == false )
                {
                    ParentTask.NavbarFragment.EnableSpringboardRevealButton( true );

                    // also force the orientation to portait
                    Activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                }
            }

            void RetrieveBiblePassage( )
            {
               ResultView.Hide( );

               BlockerView.Show( delegate
               {
                  RequestingBiblePassage = true;

                  BibleRenderer.RetrieveBiblePassage( BibleAddress, delegate( string htmlStream )
                  {
                      // if it worked, take the html stream and store it
                      if( string.IsNullOrWhiteSpace( htmlStream ) == false )
                      {
                          PassageHTML = htmlStream;
                          PassageWebView.LoadDataWithBaseURL( "", PassageHTML, "text/html", "UTF-8", "" );
                      }
                      else
                      {
                          // otherwise display an error
                          ResultView.Show( GeneralStrings.Network_Status_FailedText,
                                           PrivateControlStylingConfig.Result_Symbol_Failed,
                                           GeneralStrings.Network_Result_FailedText,
                                           GeneralStrings.Retry );
                      }

                      RequestingBiblePassage = false;
                      BlockerView.Hide( null );
                  });

               });
            }
        }
    }
}
