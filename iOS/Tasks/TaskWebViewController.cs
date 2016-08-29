using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using CoreGraphics;
using App.Shared.UI;
using App.Shared.Strings;
using App.Shared.Config;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.PrivateConfig;
using MobileApp;

namespace iOS
{
	partial class TaskWebViewController : TaskUIViewController
	{
        class CustomWebViewDelegate : UIWebViewDelegate
        {
            public TaskWebViewController Parent { get; set; }
            public CustomWebViewDelegate( TaskWebViewController parent ) : base( )
            {
                Parent = parent;
            }

            public override void LoadFailed(UIWebView webView, NSError error)
            {
                // if it's within the URL domain
                if ( error.Domain == NSError.NSUrlErrorDomain )
                {
                    // make sure it isn't simply a cancelled resource load.
                    var urlError = default(NSUrlError);
                    if(!Enum.TryParse<NSUrlError>(error.Code.ToString(), out urlError))
                    {
                        urlError = NSUrlError.Unknown;
                    }

                    // if it ISN'T cancelled, show the error page.
                    if ( urlError != NSUrlError.Cancelled )
                    {
                        Parent.HandleLoadError( );
                    }
                }
            }

            public override void LoadingFinished(UIWebView webView)
            {
                Parent.HandleLoadFinished( );
            }

            public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
            {
                string urlString = request.Url.AbsoluteString.ToString( );
                if( urlString.StartsWith( PrivateGeneralConfig.ExternalUrlToken ) )
                {
                    // strip off the PrivateGeneralConfig.ExternalUrlToken and forward it
                    urlString = urlString.Substring( PrivateGeneralConfig.ExternalUrlToken.Length );
                    
                    UIApplication.SharedApplication.OpenUrl( new NSUrl( urlString ) );
                    return false;
                }

                return true;
            }
        }
        
        class WebScrollDelegate : NavBarRevealHelperDelegate
        {
            UIWebView ParentWebView { get; set; }
            IUIScrollViewDelegate SourceDelegate { get; set; }

            public WebScrollDelegate( UIWebView parentWebView, NavToolbar toolbar ) : base( toolbar )
            {
                ParentWebView = parentWebView;

                SourceDelegate = ParentWebView.ScrollView.Delegate;
                ParentWebView.ScrollView.Delegate = this;
            }

            public override void DraggingStarted(UIScrollView scrollView)
            {
                base.DraggingStarted(scrollView);

                if ( SourceDelegate != null )
                {
                    SourceDelegate.DraggingStarted( scrollView );
                }
            }

            public override void Scrolled(UIScrollView scrollView)
            {
                base.Scrolled(scrollView);

                if ( SourceDelegate != null )
                {
                    SourceDelegate.Scrolled( scrollView );
                }
            }

            // All the below methods are simple passthroughs
            public override void DecelerationEnded( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.DecelerationEnded( scrollView );
                }
            }

            public override void DecelerationStarted( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.DecelerationStarted( scrollView );
                }
            }

            public override void DidZoom( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.DidZoom( scrollView );
                }
            }

            public override void DraggingEnded( UIScrollView scrollView, bool willDecelerate )
            {
                if ( SourceDelegate != null )
                {   
                    SourceDelegate.DraggingEnded( scrollView, willDecelerate );
                }
            }

            public override void ScrollAnimationEnded( UIScrollView scrollView )
            {
                SourceDelegate.ScrollAnimationEnded( scrollView );
            }

            public override void ScrolledToTop( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.ScrolledToTop( scrollView );
                }
            }

            public override bool ShouldScrollToTop( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    return SourceDelegate.ShouldScrollToTop( scrollView );
                }

                return false;
            }

            public override UIView ViewForZoomingInScrollView( UIScrollView scrollView )
            {
                if ( SourceDelegate != null )
                {
                    return SourceDelegate.ViewForZoomingInScrollView( scrollView );
                }

                return null;
            }

            public override void WillEndDragging( UIScrollView scrollView, CGPoint velocity, ref CGPoint targetContentOffset )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.WillEndDragging( scrollView, velocity, ref targetContentOffset );
                }
            }

            public override void ZoomingEnded( UIScrollView scrollView, UIView withView, nfloat atScale )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.ZoomingEnded( scrollView, withView, atScale );
                }
            }

            public override void ZoomingStarted( UIScrollView scrollView, UIView view )
            {
                if ( SourceDelegate != null )
                {
                    SourceDelegate.ZoomingStarted( scrollView, view );
                }
            }
        }

        string DisplayUrl { get; set; }

        UIWebView WebView { get; set; }

        UIResultView ResultView { get; set; }

        WebScrollDelegate WebScrollDelegateOverride { get; set; }

        UIActivityIndicatorView ActivityIndicator { get; set; }

        bool DisableIdleTimer { get; set; }

        bool WebviewControlsNavbar { get; set; }
        bool WebviewHidesNavbarOnScroll { get; set; }

        bool IncludeImpersonationToken { get; set; }

        bool PreloadComplete { get; set; }

        const string BlackHtml = "<html><body style=\"background-color:black;\"></body></html>";

        /// <summary>
        /// Handles taking a URL and either launching it externally or within a webView, with or without impersonation token info. (Showing an embedded web page OR launching Safari)
        /// </summary>
        public static void HandleUrl( bool launchExternalBrowser, bool includeImpersonationToken, string url, Task parentTask, UIViewController currController, bool disableIdleTimer, bool webViewControlsNavbar, bool webviewHidesNavbarOnScroll )
        {
            // do we launch them out of the app?
            if ( launchExternalBrowser == true )
            {
                // should we get the impersonation token?
                if ( includeImpersonationToken == true )
                {
                    MobileAppApi.TryGetImpersonationToken( 
                        delegate(string impersonationToken )
                        {
                            // put the platform we're running
                            string fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( url, PrivateGeneralConfig.MobilePlatform );

                            // build the full url with their preferred campus (since thats personal data)
                            fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( fullUrl, string.Format( PrivateGeneralConfig.RockCampusContext, App.Shared.Network.RockMobileUser.Instance.GetRelevantCampus( ) ) );

                            // URL encode the value
                            NSString encodedUrlString = fullUrl.UrlEncode( );

                            // if we got a token, append it
                            NSUrl encodedUrl = null;
                            if ( string.IsNullOrEmpty( impersonationToken ) == false )
                            {
                                encodedUrl = new NSUrl( encodedUrlString + "&" + impersonationToken );
                            }
                            else
                            {
                                encodedUrl = new NSUrl( encodedUrlString );
                            }

                            UIApplication.SharedApplication.OpenUrl( encodedUrl );
                        } );
                }
                else
                {
                    // first encode the url
                    NSString encodedUrlString = url.UrlEncode( );

                    UIApplication.SharedApplication.OpenUrl( new NSUrl( encodedUrlString ) );
                }
            }
            // they are using an embedded browser, so this is pretty simple
            else
            {
                TaskWebViewController viewController = new TaskWebViewController( url, parentTask, includeImpersonationToken, disableIdleTimer, webViewControlsNavbar, webviewHidesNavbarOnScroll );
                parentTask.PerformSegue( currController, viewController );
            }
        }

        protected TaskWebViewController ( string displayUrl, Task parentTask, bool includeImpersonationToken, bool disableIdleTimer, bool webviewControlsNavbar, bool webviewHidesNavbarOnScroll ) : base ( )
		{
            DisplayUrl = displayUrl;
            Task = parentTask;
            DisableIdleTimer = disableIdleTimer;
            WebviewControlsNavbar = webviewControlsNavbar;
            WebviewHidesNavbarOnScroll = webviewHidesNavbarOnScroll;
            IncludeImpersonationToken = includeImpersonationToken;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // setup our web view
            WebView = new UIWebView( );
            WebView.Layer.AnchorPoint = CGPoint.Empty;
            WebView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            WebView.Delegate = new CustomWebViewDelegate( this );
            WebView.ScrollView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            WebView.Hidden = true;
            View.AddSubview( WebView );

            // add an activity indicator
            ActivityIndicator = new UIActivityIndicatorView();
            ActivityIndicator.Layer.AnchorPoint = CGPoint.Empty;
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            ActivityIndicator.StartAnimating( );
            View.AddSubview( ActivityIndicator );

            ActivityIndicator.Hidden = false;

            // begin by preloading a black html page that will cover the screen
            WebView.LoadHtmlString( BlackHtml, null );
        }

        public bool OnBackPressed( )
        {
            if ( WebView.CanGoBack )
            {
                WebView.GoBack( );
                return true;
            }

            return false;
        }

        void PerformRequest( )
        {
            // do we need an impersonation token?
            if ( IncludeImpersonationToken )
            {
                MobileAppApi.TryGetImpersonationToken( 
                    delegate( string impersonationToken )
                    {
                        // put the platform we're running
                        string fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( DisplayUrl, PrivateGeneralConfig.MobilePlatform );

                        // include their campus, since that's part of personal data.
                        fullUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( fullUrl, string.Format( PrivateGeneralConfig.RockCampusContext, App.Shared.Network.RockMobileUser.Instance.GetRelevantCampus( ) ) );

                        // URL encode the value
                        NSString encodedUrlString = fullUrl.UrlEncode( );

                        // if we got a token, append it
                        NSUrl encodedUrl = null;
                        if( string.IsNullOrEmpty( impersonationToken ) == false )
                        {
                            encodedUrl = new NSUrl( encodedUrlString + "&" + impersonationToken );
                        }
                        else
                        {
                            encodedUrl = new NSUrl( encodedUrlString );
                        }

                        LaunchWebview( encodedUrl );
                    } );
            }
            else
            {
                NSString encodedUrlString = DisplayUrl.UrlEncode( );
                NSUrl encodedUrl = new NSUrl( encodedUrlString );
                LaunchWebview( encodedUrl );
            }
        }

        void LaunchWebview( NSUrl encodedUrl )
        {
            // setup a result view in the case of failure
            ResultView = new UIResultView( View, View.Bounds.ToRectF( ), 
                delegate 
                { 
                    ResultView.Hide( );
                    ActivityIndicator.Hidden = false;
                    WebView.LoadRequest( new NSUrlRequest( encodedUrl ) ); 
                } );

            WebView.LoadRequest( new NSUrlRequest( encodedUrl ) );

            // if the webview controls the navbar
            if ( WebviewControlsNavbar == true  )
            {
                // then if we want hiding on scroll, change our delegate so we can do just that.
                if ( WebviewHidesNavbarOnScroll == true )
                {
                    WebView.ScrollView.Delegate = new WebScrollDelegate( WebView, Task.NavToolbar );
                }
                else
                {
                    // otherwise force the nav toolbar to show up, and we'll leave it up

                    // of course, make sure it's still valid. IT's possible our load request returned
                    // AFTER they left this page.
                    if( Task.NavToolbar != null )
                    {
                        Task.NavToolbar.Reveal( true );
                    }
                }
            }
        }

        public void HandleLoadError( )
        {
            ResultView.Show( GeneralStrings.Network_Status_FailedText, PrivateControlStylingConfig.Result_Symbol_Failed, GeneralStrings.Network_Result_FailedText, GeneralStrings.Retry );
            ActivityIndicator.Hidden = true;
        }

        public void HandleLoadFinished( )
        {
            if ( ResultView != null )
            {
                ResultView.Hide( );
            }

            // if we're preloading, then the black page just finished
            if ( PreloadComplete == false )
            {
                // now, seriously, kick off a timer so that the renderer has time to
                // display its default white screen, then render the html.
                // This will hide the white flicker and look a lot nicer to the end user.
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 500;
                timer.AutoReset = false;
                timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    // do this ON the UI thread
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            WebView.Hidden = false;
                            PreloadComplete = true;
                            PerformRequest( );
                        } );
                };
                timer.Start( );
            }
            else
            {
                ActivityIndicator.Hidden = true;

                // if we control the nav toolbar, we enable the back button only when
                // there's something to go "back" to.
                if( WebviewControlsNavbar == true )
                {
                    // of course, make sure it's still valid. IT's possible our load request returned
                    // AFTER they left this page.
                    if( Task.NavToolbar != null )
                    {
                        Task.NavToolbar.SetBackButtonEnabled( WebView.CanGoBack );
                    }
                }
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // by default, show the toolbar

            // of course, make sure it's still valid. IT's possible our load request returned
            // AFTER they left this page.
            if( Task.NavToolbar != null )
            {
                Task.NavToolbar.Reveal( true );
            }

            UIApplication.SharedApplication.IdleTimerDisabled = DisableIdleTimer;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer {0}", DisableIdleTimer == true ? "OFF " : "ON" ) );
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            UIApplication.SharedApplication.IdleTimerDisabled = DisableIdleTimer;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer {0}", DisableIdleTimer == true ? "OFF " : "ON" ) );
        }

        public override void OnActivated()
        {
            base.OnActivated();

            UIApplication.SharedApplication.IdleTimerDisabled = DisableIdleTimer;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer {0}", DisableIdleTimer == true ? "OFF " : "ON" ) );
        }

        public override void WillEnterForeground( )
        {
            base.WillEnterForeground( );

            UIApplication.SharedApplication.IdleTimerDisabled = DisableIdleTimer;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer {0}", DisableIdleTimer == true ? "OFF " : "ON" ) );

            // force a redraw so the notes are recreated
            LayoutChanged( );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive();

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer ON" ) );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate();

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer ON" ) );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UIApplication.SharedApplication.IdleTimerDisabled = false;
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Turning idle timer ON" ) );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            WebView.Bounds = View.Bounds;

            // this could be null if we are waiting for the impersonation token response
            if ( ResultView != null )
            {
                ResultView.SetBounds( WebView.Bounds.ToRectF( ) );
            }

            ActivityIndicator.Frame = new CGRect( ( View.Bounds.Width - ActivityIndicator.Bounds.Width ) / 2, 
                                                  ( View.Bounds.Height - ActivityIndicator.Bounds.Height ) / 2,
                                                  ActivityIndicator.Bounds.Width, 
                                                  ActivityIndicator.Bounds.Height );
        }
	}
}
