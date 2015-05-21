
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
using App.Shared.Network;
using Android.Graphics;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Strings;
using App.Shared.Config;
using App.Shared.Analytics;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using App.Shared.UI;
using App.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerPostFragment : TaskFragment
            {
                public Rock.Client.PrayerRequest PrayerRequest { get; set; }
                public bool Posting { get; set; }
                bool Success { get; set; }
                bool IsActive { get; set; }

                UIBlockerView BlockerView { get; set; }
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

                    View view = inflater.Inflate(Resource.Layout.Prayer_Post, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    ResultView = new UIResultView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), 
                        delegate 
                        { 
                            if( Success == true )
                            {
                                // leave
                                ParentTask.OnClick( this, 0 );
                            }
                            else
                            {
                                // retry
                                SubmitPrayerRequest( );
                            }
                        } );

                    BlockerView = new UIBlockerView( view, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );


                    return view;
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                    BlockerView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetContainerDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    IsActive = true;

                    SubmitPrayerRequest( );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsActive = false;
                }

                void SubmitPrayerRequest( )
                {
                    ResultView.Show( PrayerStrings.PostPrayer_Status_Submitting,
                        "", 
                        "", 
                        "" );

                    Success = false;
                    Posting = true;

                    // fade in our blocker, and when it's complete, send our request off
                    BlockerView.Show( delegate
                        {
                            // submit the request
                            App.Shared.Network.RockApi.Instance.PutPrayer( PrayerRequest, 
                                delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                                {
                                    Posting = false;

                                    // if they left while posting, screw em.
                                    if ( IsActive == true )
                                    {
                                        BlockerView.Hide( null );

                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                        {
                                            Success = true;

                                            ResultView.Show( PrayerStrings.PostPrayer_Status_SuccessText,
                                                PrivateControlStylingConfig.Result_Symbol_Success, 
                                                PrayerStrings.PostPrayer_Result_SuccessText, 
                                                GeneralStrings.Done );

                                            PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Create );
                                        }
                                        else
                                        {
                                            Success = false;

                                            ResultView.Show( PrayerStrings.PostPrayer_Status_FailedText,
                                                PrivateControlStylingConfig.Result_Symbol_Failed, 
                                                PrayerStrings.PostPrayer_Result_FailedText, 
                                                GeneralStrings.Retry );
                                        }
                                    }
                                } );
                        } );
                }
            }
        }
    }
}
