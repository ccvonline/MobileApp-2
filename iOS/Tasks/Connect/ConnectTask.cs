using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace iOS
{
    public class ConnectTask : Task
    {
        TaskUIViewController MainPageVC { get; set; }

        public ConnectTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "ConnectMainPageViewController" ) as TaskUIViewController;
            MainPageVC.Task = this;

            ActiveViewController = MainPageVC;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // turn off the share & create buttons
            NavToolbar.SetShareButtonEnabled( false, null );
            NavToolbar.SetCreateButtonEnabled( false, null );

            // if it's the main page, nide the nav toolbar
            if ( viewController == MainPageVC )
            {
                NavToolbar.Reveal( false );
            }
            // if it's the group finder, force the nav toolbar to always show
            else if ( viewController as GroupFinderViewController != null )
            {
                NavToolbar.Reveal( true );
            }
            // otherwise, as long as it IS NOT the webView, do the standard 3 seconds
            else if ( viewController as TaskWebViewController == null )
            {
                //NavToolbar.RevealForTime( 3.0f );
                NavToolbar.Reveal( true );
            }
        }

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they're not on the main page or the webView
            if ( ActiveViewController != MainPageVC && ( ActiveViewController as TaskWebViewController ) == null )
            {
                // let a dead space tap reveal the toolbar
                //NavToolbar.RevealForTime( 3.0f );
            }
        }

        public override void OnActivated( )
        {
            base.OnActivated( );

            ActiveViewController.OnActivated( );
        }

        public override void WillEnterForeground()
        {
            base.WillEnterForeground();

            ActiveViewController.WillEnterForeground( );
        }

        public override void AppOnResignActive()
        {
            base.AppOnResignActive( );

            ActiveViewController.AppOnResignActive( );
        }

        public override void AppDidEnterBackground()
        {
            base.AppDidEnterBackground();

            ActiveViewController.AppDidEnterBackground( );
        }

        public override void AppWillTerminate()
        {
            base.AppWillTerminate( );

            ActiveViewController.AppWillTerminate( );
        }
    }
}

