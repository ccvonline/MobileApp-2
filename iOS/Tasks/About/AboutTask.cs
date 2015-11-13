using System;
using UIKit;
using CoreGraphics;
using Foundation;
using App.Shared.Config;

namespace iOS
{
    public class AboutTask : Task
    {
        TaskUIViewController MainPageVC { get; set; }

        public AboutTask( string storyboardName ) : base( storyboardName )
        {
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
            TaskWebViewController.HandleUrl( false, true, AboutConfig.Url, this, MainPageVC, false, false );
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // turn off the share & create buttons
            NavToolbar.SetShareButtonEnabled( false, null );
            NavToolbar.SetCreateButtonEnabled( false, null );
            NavToolbar.Reveal( false );
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
