
using System;
using System.Drawing;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;

using UIKit;
using CoreText;
using CoreGraphics;
using Foundation;
using RestSharp;

using Rock.Mobile.PlatformSpecific.Util;
using MobileApp.Shared.Config;
using MobileApp.Shared.UI;
using MobileApp.Shared.Strings;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.Network;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Shared.Notes.Model;

using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using App.Shared;

namespace iOS
{
	public class BiblePassageViewController : TaskUIViewController
	{
        /// <summary>
		/// The Web View that will display the HTML-formatted Bible passage.
		/// </summary>
		/// <value>The bible web view.</value>
		UIWebView BibleWebView { get; set; }

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
		/// This is the fully formatted Bible passage text.
		/// </summary>
		/// <value>The passage HTML string.</value>
		string PassageHTML { get; set; }

		/// <summary>
		/// The base address of the HTTP request.
		/// </summary>
		/// <value>The address for the HTTP request.</value>
		//string BibliaAddress { get; set; }

        string BibleAddress { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:iOS.BiblePassageViewController"/> requesting bible passage.
		/// </summary>
		/// <value><c>true</c> if requesting bible passage; otherwise, <c>false</c>.</value>
		bool RequestingBiblePassage { get; set; }

		/// <summary>
		/// The current orientation of the device. We track this
		/// so we can know when it changes and only rebuild the notes then.
		/// </summary>
		/// <value>The orientation.</value>
		int OrientationState { get; set; }

		/// <summary>
		///  The Bible Passage View's constructor takes a url, pulls the passage
		///  citation token out of it, and runs an HTTP request to the Biblia
		///  API for that passage. The Biblia API responds with HTML that is
		///  then processed into an NSAttributedString.
		/// </summary>
		/// <param name="activeUrl">Active URL.</param>
		/// <param name="parentTask">Parent task.</param>
		public BiblePassageViewController( string activeUrl, Task parentTask )
		{
			Task = parentTask;

            // Finds the passage citation in the Bible Gateway URL and extracts it
            // (always comes after the "?search=" token, which is 8 characters long, hence the offset).
            //string passageCitation = activeUrl.Substring( activeUrl.IndexOf( PrivateNoteConfig.Biblia_Prefix, StringComparison.CurrentCulture ) + 8 );

            /*BibliaAddress = string.Format( "https://api.biblia.com/v1/bible/content/LEB.html?passage={0}&style=fullyFormatted&key={1}",
										  passageCitation, App.Shared.SecuredValues.Biblia_API_Key );*/
            BibleAddress = activeUrl;
		}

		public override void ViewDidLoad( )
		{
			OrientationState = -1;

			//Instantiate the Bible Web View first.
			BibleWebView = new UIWebView( new CGRect( 10, 10, View.Bounds.Width - 20, View.Bounds.Height - 20 ) );
			BibleWebView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( 0x1C1C1CFF );
			BibleWebView.ScrollView.ContentInset = new UIEdgeInsets( 10, 0, 40, 0 );
			BibleWebView.Opaque = false;

			View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
			View.AddSubview( BibleWebView );

			//Then instantiate the blocker view.
			BlockerView = new UIBlockerView( View, View.Bounds.ToRectF( ) );

			ResultView = new UIResultView( View, View.Bounds.ToRectF( ),
				delegate
				{
					if( RequestingBiblePassage == false )
					{
						RetrieveBiblePassage( );
					}
				} );
		}

        public override void ViewDidLayoutSubviews( )
        {
            base.ViewDidLayoutSubviews( );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
            Rock.Mobile.Util.Debug.WriteLine( "Turning idle timer OFF" );
        }

		public override void ViewWillAppear( bool animated )
		{
			base.ViewWillAppear( animated );

			View.Bounds = Task.ContainerBounds;

			View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

			LayoutChanged( );

			if( RequestingBiblePassage == false )
            {
                RetrieveBiblePassage( );
            }
		}

		public override void LayoutChanged( )
		{
			base.LayoutChanged( );

			// get the orientation state. WE consider unknown- 1, profile 0, landscape 1,
			int orientationState = SpringboardViewController.IsDeviceLandscape( ) == true ? 1 : 0;

			// if the states are in disagreement, correct it
			if( OrientationState != orientationState )
			{
				OrientationState = orientationState;

				BibleWebView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
				BibleWebView.Layer.Position = new CGPoint( BibleWebView.Layer.Position.X, BibleWebView.Layer.Position.Y );

				BlockerView.SetBounds( View.Bounds.ToRectF( ) );
				ResultView.SetBounds( View.Bounds.ToRectF( ) );
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
                            BibleWebView.LoadHtmlString( PassageHTML, NSBundle.MainBundle.BundleUrl );
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

        public override void ViewDidAppear( bool animated )
        {
            base.ViewDidAppear( animated );
            UIApplication.SharedApplication.IdleTimerDisabled = true;
        }


		public override void OnActivated( )
		{
			base.OnActivated( );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
			LayoutChanged( );
		}

		public override void WillEnterForeground( )
		{
			base.OnActivated( );

            UIApplication.SharedApplication.IdleTimerDisabled = true;
			LayoutChanged( );
		}

        public override void ViewWillDisappear( bool animated )
        {
            base.ViewWillDisappear( animated );
            UIApplication.SharedApplication.IdleTimerDisabled = false;
        }

		public override void AppOnResignActive( )
		{
            base.AppOnResignActive();

            UIApplication.SharedApplication.IdleTimerDisabled = false;
		}

		public override void AppWillTerminate( )
		{
            base.AppWillTerminate();

            UIApplication.SharedApplication.IdleTimerDisabled = false;
		}
	}
}
