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
        string BibliaAddress { get; set; }

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
            string passageCitation = activeUrl.Substring( activeUrl.IndexOf( PrivateNoteConfig.Biblia_Prefix, StringComparison.CurrentCulture ) + 8 );

			BibliaAddress = string.Format( "https://api.biblia.com/v1/bible/content/LEB.html?passage={0}&style=fullyFormatted&key={1}",
			                              passageCitation, GeneralConfig.Biblia_API_Key );

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

        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            View.Bounds = Task.ContainerBounds;

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            LayoutChanged( );

            RetrieveBiblePassage( );

        }

        public override void ViewDidAppear( bool animated )
        {

            base.ViewDidAppear( animated );

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

                    RestRequest request = new RestRequest( Method.GET );
                    HttpRequest webRequest = new HttpRequest( );

                    webRequest.ExecuteAsync<RestResponse>( BibliaAddress, request, delegate ( System.Net.HttpStatusCode statusCode, string statusDescription, RestResponse passage )
                     {

                         if( Util.StatusInSuccessRange( statusCode ) == true )
                         {

                             PassageHTML = passage.Content;
                             string styleHeader = "<head>" +
                             "<style type=\"text/css\">" +
                                 "html {-webkit-text-size-adjust:none}" +
                                 "body {" +
                                    "font-family: Arial;" +
                                    "color: white;" +
                                    "background-color: #1C1C1C;" +
                                  "}" +
                                "</style>" +
                                "</head>";

                            // adds the CSS header to the HTML string
                            PassageHTML = "<html>" + styleHeader + "<body>" + PassageHTML + "</body></html>";

                            // removes the weird formatting quirks of LEB (scholarly notations) that might confuse the reader
                            PassageHTML = Regex.Replace( PassageHTML, "<sub>.*?</sub>", String.Empty );
                             PassageHTML = Regex.Replace( PassageHTML, "font-style:italic;", String.Empty );
                             PassageHTML = Regex.Replace( PassageHTML, "\\*", String.Empty );

                             BibleWebView.LoadHtmlString( PassageHTML, NSBundle.MainBundle.BundleUrl );
                         }
                         else
                         {
                             ResultView.Show( GeneralStrings.Network_Status_FailedText,
                                              PrivateControlStylingConfig.Result_Symbol_Failed,
                                              GeneralStrings.Network_Result_FailedText,
                                              GeneralStrings.Retry );
                         }

                         RequestingBiblePassage = false;

                         BlockerView.Hide( null );

                     } );

                } );

        }


        public override void OnActivated( )
        {
            base.OnActivated( );
            LayoutChanged( );
        }

        public override void WillEnterForeground( )
        {
            base.OnActivated( );
            LayoutChanged( );
        }

        public override void AppOnResignActive( )
        {
        }

        public override void AppDidEnterBackground( )
        {
        }

        public override void AppWillTerminate( )
        {
        }

        /// <summary>
        /// This helper function converts the HTML String we receive from the API
        /// and converts it into an appropriately formatted NSMutableAttributedString.
        /// </summary>
        static NSMutableAttributedString BuildHtmlAttributedStringFromString( string inputStr )
        {
            NSError error = null;

            // there is no option to turn HTML into a Mutable Attributed String, so we must
            // use the constructor provided for Attributed String
            NSAttributedString attributedString = new NSAttributedString( inputStr,
                new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.HTML, StringEncoding = NSStringEncoding.UTF8 },
                ref error );

            // then we just construct a mutable attributed string with the unmutable attributed HTML and return it
            NSMutableAttributedString mutableAttrString = new NSMutableAttributedString( attributedString );
            return mutableAttrString;
        }

    }
}
