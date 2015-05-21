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

            AboutVersionText.Text = string.Format( "Version: {0}", BuildStrings.Version );

            WebView = new UIWebView( );
            View.AddSubview( WebView );
            WebView.LoadRequest( new NSUrlRequest( new NSUrl( AboutConfig.Url ) ) );
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
