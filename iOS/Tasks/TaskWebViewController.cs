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

namespace iOS
{
	partial class TaskWebViewController : TaskUIViewController
	{
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

        public TaskWebViewController ( string displayUrl, Task parentTask ) : base ( )
		{
            DisplayUrl = displayUrl;
            Task = parentTask;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // setup our web view
            WebView = new UIWebView( );
            WebView.Layer.AnchorPoint = CGPoint.Empty;
            WebView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            WebView.Hidden = true;
            View.AddSubview( WebView );

            // add an activity indicator
            ActivityIndicator = new UIActivityIndicatorView();
            ActivityIndicator.Layer.AnchorPoint = CGPoint.Empty;
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            ActivityIndicator.StartAnimating( );
            View.AddSubview( ActivityIndicator );

            // setup a result view in the case of failure
            ResultView = new UIResultView( View, View.Bounds.ToRectF( ), 
                delegate 
                { 
                    ResultView.Hide( );
                    ActivityIndicator.Hidden = false;
                    WebView.LoadRequest( new NSUrlRequest( new NSUrl( DisplayUrl ) ) ); 
                } );
            

            // kick off our initial request
            ActivityIndicator.Hidden = false;
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( DisplayUrl ) ) );

            // if it fails, display the result view
            WebView.LoadError += (object sender, UIWebErrorArgs e ) =>
            {
                ResultView.Show( GeneralStrings.Network_Status_FailedText, PrivateControlStylingConfig.Result_Symbol_Failed, GeneralStrings.Network_Result_FailedText, GeneralStrings.Retry );
                ActivityIndicator.Hidden = true;
            };

            // if it succeeds, reveal the webView
            WebView.LoadFinished += (object sender, EventArgs e ) =>
            {
                ResultView.Hide( );
                WebView.Hidden = false;
                ActivityIndicator.Hidden = true;
            };

            // not 100% sure that this is safe. If WebView sets the scrollView delegate and doesn't back ours up
            // (which it SHOULD) we won't get our calls
            WebView.ScrollView.Delegate = new WebScrollDelegate( WebView, Task.NavToolbar );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // by default, show the toolbar
            Task.NavToolbar.Reveal( true );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            WebView.Bounds = View.Bounds;

            ResultView.SetBounds( WebView.Bounds.ToRectF( ) );

            ActivityIndicator.Frame = new CGRect( ( View.Bounds.Width - ActivityIndicator.Bounds.Width ) / 2, 
                                                  ( View.Bounds.Height - ActivityIndicator.Bounds.Height ) / 2,
                                                  ActivityIndicator.Bounds.Width, 
                                                  ActivityIndicator.Bounds.Height );
        }
	}
}
