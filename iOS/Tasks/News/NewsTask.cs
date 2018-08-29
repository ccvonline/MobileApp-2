using System;
using UIKit;
using CoreGraphics;
using Foundation;
using MobileApp.Shared.Network;
using System.Collections.Generic;
using MobileApp.Shared.Config;
using MobileApp.Shared.PrivateConfig;
using MobileApp;
using Rock.Mobile.PlatformSpecific.Util;

namespace iOS
{
    public class NewsTask : Task
    {
        NewsMainUIViewController MainPageVC { get; set; }

        List<RockNews> News { get; set; }

        public NewsTask( string storyboardName ) : base( storyboardName )
        {
            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as NewsMainUIViewController;
            MainPageVC.Task = this;

            ActiveViewController = MainPageVC;

            News = new List<RockNews>( );
        }

        public override string Command_Keyword( )
        {
            return PrivateGeneralConfig.App_URL_Task_News;
        }

        public override void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            base.MakeActive( parentViewController, navToolbar, containerBounds );

            MainPageVC.View.Bounds = containerBounds;

            // refresh our news from GeneralData
            ReloadNews( );

            // and provide it to the main page
            MainPageVC.UpdateNews( News );

            // set our current page as root
            parentViewController.PushViewController(MainPageVC, false);
        }

        /// <summary>
        /// Takes the news from LaunchData and populates the NewsPrimaryFragment with it.
        /// </summary>
        void ReloadNews( )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    Rock.Client.Campus campus = RockLaunchData.Instance.Data.CampusFromId( RockMobileUser.Instance.ViewingCampus );
                    Guid viewingCampusGuid = campus != null ? campus.Guid : Guid.Empty;

                    // provide the news to the viewer by COPYING it.
                    News.Clear( );
                    foreach ( RockNews newsItem in RockLaunchData.Instance.Data.News )
                    {
                        // if the list of campus guids contains the viewing campus, OR there are no guids set, allow it.
                        if ( newsItem.CampusGuids.Contains( viewingCampusGuid ) || newsItem.CampusGuids.Count == 0 )
                        {
                            // Limit the amount of news to display to MaxNews so we don't show so many we
                            // run out of memory. If DEVELOPER MODE is on, show them all.
                            if( News.Count < PrivateNewsConfig.MaxNews || MobileApp.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                            {
                                News.Add( new RockNews( newsItem ) );
                            }
                        }
                    }

                    // if a campaign is downloaded, display it to them.
                    if( RockLaunchData.Instance.Data.PECampaign != null )
                    {
                        News.Insert( 0, RockLaunchData.Instance.Data.PECampaign );
                    }

                    // if they need to upgrade, push that news item to the top
                    if( RockLaunchData.Instance.Data.NeedsUpgrade )
                    {
                        News.Insert( 0, RockLaunchData.Instance.Data.UpgradeNewsItem );
                    }
                } );
        }

        public override void WillShowViewController(TaskUIViewController viewController)
        {
            base.WillShowViewController( viewController );

            // turn off the share & create buttons
            NavToolbar.SetShareButtonEnabled( false, null );
            NavToolbar.SetCreateButtonEnabled( false, null );

            // if it's the main page, disable the back button on the toolbar
            if ( viewController == MainPageVC )
            {
                NavToolbar.Reveal( false );
            }
            else if ( viewController as TaskWebViewController == null )
            {
                NavToolbar.Reveal( true );
            }
        }

        public override void PerformAction( string command, string[] arguments )
        {
            base.PerformAction( command, arguments );

            switch ( command )
            {
                case PrivateGeneralConfig.App_URL_Commands_Execute:
                {
                    // is this for us?
                    if( arguments[ 0 ] == Command_Keyword( ) )
                    {
                        // whether we're changing campuses or explicitely reloading news,
                        // reload the news.
                        if ( arguments[ 1 ] == PrivateGeneralConfig.App_URL_Execute_CampusChanged || 
                             arguments[ 1 ] == PrivateGeneralConfig.App_URL_Execute_ReloadNews )
                        {
                            ReloadNews( );

                            MainPageVC.UpdateNews( News );

                            MainPageVC.LoadAndDownloadImages( );
                            MainPageVC.LayoutChanged( );
                        }
                    }
                    break;
                }
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

        public override void TouchesEnded(TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(taskUIViewController, touches, evt);

            // if they touched a dead area, reveal the nav toolbar again.
            //NavToolbar.RevealForTime( 3.0f );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
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
