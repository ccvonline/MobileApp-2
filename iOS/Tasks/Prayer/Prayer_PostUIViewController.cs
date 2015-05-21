using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using App.Shared.Strings;
using Rock.Mobile.UI;
using CoreGraphics;
using App.Shared.Config;
using App.Shared.Analytics;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.PrivateConfig;

namespace iOS
{
    partial class Prayer_PostUIViewController : TaskUIViewController
	{
        public Rock.Client.PrayerRequest PrayerRequest { get; set; }
        public bool Posting { get; set; }
        bool Success { get; set; }
        bool IsActive { get; set; }

        UIBlockerView BlockerView { get; set; }
        UIResultView ResultView { get; set; }

		public Prayer_PostUIViewController ( ) : base ()
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ResultView = new UIResultView( View, View.Frame.ToRectF( ), 
                delegate 
                { 
                    if( Success == true )
                    {
                        NavigationController.PopToRootViewController( true );
                    }
                    else
                    {
                        SubmitPrayerRequest( );
                    }
                } );

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            //setup our appearance
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            Success = false;
            Posting = false;

            IsActive = true;

            if ( PrayerRequest == null )
            {
                throw new Exception( "Set a PrayerRequest before loading this view controller!" );
            }

            // immediately attempt to submit the request
            SubmitPrayerRequest( );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Note: Ideally i'd like to disable the springboard button until the post is finished.
            // However, that would give them NO WAY to get out of this page should they decide
            // they don't want the prayer to finish posting. Maybe it's fine to let them leave if they
            // want to.
            Task.NavToolbar.Reveal( false );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            ResultView.SetBounds( View.Bounds.ToRectF( ) );
            BlockerView.SetBounds( View.Bounds.ToRectF( ) );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

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
                    // sleep this thread for a second to give an appearance of submission
                    System.Threading.Thread.Sleep( 1000 );

                    // submit the request
                    App.Shared.Network.RockApi.Instance.PutPrayer( PrayerRequest, delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
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

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // don't allow touch input while we're posting
            if ( Posting == false )
            {
                base.TouchesEnded( touches, evt );
            }
        }
	}
}
