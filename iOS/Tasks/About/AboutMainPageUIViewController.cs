using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using App.Shared.Config;
using App.Shared.Strings;

namespace iOS
{
	partial class AboutMainPageUIViewController : TaskUIViewController
	{
        UIWebView WebView { get; set; }
        
		public AboutMainPageUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WebView = new UIWebView( );
            View.AddSubview( WebView );

            string aboutUrl = string.Format( AboutConfig.Url, App.Shared.Network.RockMobileUser.Instance.GetRelevantCampus( ) );
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( aboutUrl ) ) );
        }

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            CGRect webViewFrame;
            webViewFrame = new CGRect( 0, 
                                       AboutVersionText.Frame.Height, 
                                       View.Frame.Width, 
                                       View.Frame.Height - AboutVersionText.Frame.Height );

            WebView.Frame = webViewFrame;
        }
	}
}
