using System;
using UIKit;
using CoreGraphics;
using Foundation;
using App.Shared.Config;

namespace iOS
{
    public class AboutTask : Task
    {
        TaskWebViewController MainPageVC { get; set; }

        public AboutTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = new TaskWebViewController( AboutConfig.Url, this );
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainPageVC.View.Bounds = containerBounds;

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
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

        public override void OnActivated( )
        {
            base.OnActivated( );

            MainPageVC.OnActivated( );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            MainPageVC.WillEnterForeground( );
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );

            MainPageVC.AppOnResignActive( );
        }

        public override void AppDidEnterBackground()
        {
            base.AppDidEnterBackground();

            MainPageVC.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            MainPageVC.AppWillTerminate( );
        }
    }
}
