using System;
using UIKit;
using CoreGraphics;
using Foundation;
using App.Shared.Config;
using App.Shared.PrivateConfig;

namespace iOS
{
    public class AboutTask : Task
    {
        TaskUIViewController MainPageVC { get; set; }

        public AboutTask( string storyboardName ) : base( storyboardName )
        {
        }

        public override string Command_Keyword( )
        {
            return PrivateGeneralConfig.App_URL_Task_About;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainPageVC = new TaskUIViewController();
            MainPageVC.Task = this;
            MainPageVC.View.Bounds = containerBounds;

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);

            // and immediately handle the URL
            TaskWebViewController.HandleUrl( false, true, AboutConfig.Url, this, MainPageVC, false, true, false );
        }

        public override bool WantOverrideBackButton (ref bool enabled)
        {
            enabled = false;
            return true;
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // turn off the back, share & create buttons
            NavToolbar.SetBackButtonEnabled( false );
            NavToolbar.SetShareButtonEnabled( false, null );
            NavToolbar.SetCreateButtonEnabled( false, null );
            NavToolbar.Reveal( true );
        }

        public override bool OnBackPressed( )
        {
            // if we're displaying a taskViewController, let it handle back.
            TaskWebViewController webViewController = ActiveViewController as TaskWebViewController;
            if( webViewController != null )
            {
                webViewController.OnBackPressed( );
            }

            // let the container know we're handling it, so that it doesn't need to.
            return true;
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }
    }
}
