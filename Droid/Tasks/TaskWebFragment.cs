
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

namespace Droid
{
    namespace Tasks
    {
        public class TaskWebFragment : TaskFragment
        {
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

                return view;
            }

            public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
            {
                base.OnConfigurationChanged(newConfig);

                ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
            }

            public void DisplayUrl( string url )
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
                                WebLayout.LoadUrl( Url, PageLoaded );
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

                if ( string.IsNullOrEmpty( Url ) == false )
                {
                    WebLayout.LoadUrl( Url, PageLoaded );
                }
            }

            public override void OnPause()
            {
                base.OnPause();

                IsActive = false;
            }
        }
    }
}

