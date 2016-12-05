using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using App.Shared.UI;
using App.Shared.Config;
using Rock.Mobile.PlatformSpecific.Util;
using System.Drawing;
using CoreGraphics;
using Rock.Mobile.Audio;

namespace iOS
{
	class JingleUIViewController : TaskUIViewController
	{
        UIJingle JingleView { get; set; }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Layer.AnchorPoint = CGPoint.Empty;
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            string jinglePreName = string.Format( "{0}/{1}.jpg", Foundation.NSBundle.MainBundle.BundlePath, "jingle_bells_pre" );
            string jinglePostName = string.Format( "{0}/{1}.jpg", Foundation.NSBundle.MainBundle.BundlePath, "jingle_bells_post" );

            JingleView = new UIJingle();
            JingleView.Create( View, jinglePreName, jinglePostName, View.Frame.ToRectF( ), 
                              delegate( ) 
                              {
                              });
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            AppDelegate.JingleBellsEnabled = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            JingleView.LayoutChanged( View.Bounds.ToRectF( ) );
        }

        public override void WillEnterForeground( )
        {
            base.WillEnterForeground( );

            AppDelegate.JingleBellsEnabled = true;
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);

            AppDelegate.JingleBellsEnabled = false;
        }
	}
}
