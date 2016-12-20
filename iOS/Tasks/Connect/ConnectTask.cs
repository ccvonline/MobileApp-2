using System;
using UIKit;
using CoreGraphics;
using Foundation;
using MobileApp.Shared.PrivateConfig;
using System.Collections.Generic;
using MobileApp.Shared;
using System.Linq;

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

        public override string Command_Keyword( )
        {
            return PrivateGeneralConfig.App_URL_Task_Connect;
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

        public override bool OnBackPressed( )
        {
            // if we're displaying a taskViewController, let it handle back.
            TaskWebViewController webViewController = ActiveViewController as TaskWebViewController;
            if( webViewController != null )
            {
                return webViewController.OnBackPressed( );
            }

            return false;
        }

        public override void PerformAction( string command, string[] arguments )
        {
            base.PerformAction( command, arguments );

            switch( command )
            {
                // is this a goto command?
                case PrivateGeneralConfig.App_URL_Commands_Goto:
                {
                    // make sure the argument is for us
                    if( arguments[ 0 ] == Command_Keyword( ) && arguments.Length > 1 )
                    {
                        // check for groupfinder, because we support that one.
                        if( PrivateGeneralConfig.App_URL_Page_GroupFinder == arguments[ 1 ] )
                        {
                            // since we're switching to the read notes VC, pop to the main page root and 
                            // remove it, because we dont' want back history (where would they go back to?)
                            ParentViewController.ClearViewControllerStack( );

                            // create and launch the group finder. It's fine to create it here because we always dynamically create this controller.
                            TaskUIViewController viewController = Storyboard.InstantiateViewController( "GroupFinderViewController" ) as TaskUIViewController;
                            ParentViewController.PushViewController( viewController, false );
                        }
                        else
                        {
                            List<ConnectLink> engagedEntries = ConnectLink.BuildGetEngagedList( );

                            ConnectLink connectLink = engagedEntries.Where( e => e.Command_Keyword == arguments[ 1 ] ).SingleOrDefault( );
                            if( connectLink != null )
                            {
                                // clear out the stack and push the main connect page onto the stack
                                ParentViewController.ClearViewControllerStack( );
                                ParentViewController.PushViewController( MainPageVC, false );

                                // now go to the requested URL
                                TaskWebViewController.HandleUrl( false, true, connectLink.Url, this, MainPageVC, false, false, false );
                            }
                        }
                    }
                    break;
                }
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

