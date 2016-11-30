using System;
using Android.App;
using Android.Content;
using Android.Views;
using App.Shared.Network;
using System.Collections.Generic;
using App.Shared.Config;
using App.Shared.PrivateConfig;
using MobileApp;
using App.Shared.Analytics;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsTask : Task
            {
                NewsPrimaryFragment MainPage { get; set; }
                NewsDetailsFragment DetailsPage { get; set; }
                TaskWebFragment WebFragment { get; set; }

                List<RockNews> News { get; set; }

                public NewsTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.News.NewsPrimaryFragment" ) as NewsPrimaryFragment;
                    if ( MainPage == null )
                    {
                        MainPage = new NewsPrimaryFragment();
                    }
                    MainPage.ParentTask = this;

                    DetailsPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.News.NewsDetailsFragment" ) as NewsDetailsFragment;
                    if ( DetailsPage == null )
                    {
                        DetailsPage = new NewsDetailsFragment();
                    }
                    DetailsPage.ParentTask = this;

                    WebFragment = new TaskWebFragment( );
                    WebFragment.ParentTask = this;

                    // setup a list we can use to cache the news, so should it update we don't use the wrong set.
                    News = new List<RockNews>();
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    if ( forResume == false )
                    {                            
                        ReloadNews( );

                        // let the page have the latest news
                        MainPage.UpdateNews( News );
                    }
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
                                // only add news for "all campuses" and their selected campus.
                                if ( newsItem.CampusGuids.Contains( viewingCampusGuid ) || newsItem.CampusGuids.Count == 0 )
                                {
                                    // Limit the amount of news to display to MaxNews so we don't show so many we
                                    // run out of memory. If DEVELOPER MODE is on, show them all.
                                    if( News.Count < PrivateNewsConfig.MaxNews || App.Shared.Network.RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
                                    {
                                        News.Add( new RockNews( newsItem ) );
                                    }
                                }
                            }

                            // HACK: JINGLE BELLS
                            if ( (DateTime.Now >= NewsConfig.JingleBellsHack_StartTime && DateTime.Now <= NewsConfig.JingleBellsHack_EndTime) || RockLaunchData.Instance.Data.EnableJingleBells == true )
                            {
                                News.Insert( 0, RockLaunchData.Instance.Data.JingleBellsNewsItem );
                            }

                            // if they need to upgrade, push that news item to the top
                            if( RockLaunchData.Instance.Data.NeedsUpgrade )
                            {
                                News.Insert( 0, RockLaunchData.Instance.Data.UpgradeNewsItem );
                            }
                        } );
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override string Command_Keyword ()
                {
                    return PrivateGeneralConfig.App_URL_Task_News;
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
                                    // for either action, we want to reload our news,
                                    // and then update the page
                                    ReloadNews( );
                                    MainPage.UpdateNews( News );
                                }
                            }

                            break;
                        }
                    }
                }

                public override bool OnBackPressed( )
                {
                    if ( WebFragment.IsVisible == true )
                    {
                        return WebFragment.OnBackPressed( );
                    }
                    return false;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null )
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // if the main page had a VALID news item clicked, go to it
                        if ( source == MainPage && buttonId < News.Count )
                        {
                            // mark that they tapped this item.
                            NewsAnalytic.Instance.Trigger( NewsAnalytic.Read, MainPage.News[ buttonId ].News.Title );

                            // either take them to the details page, or skip it and go straight to Learn More.
                            if ( MainPage.News[ buttonId ].News.SkipDetailsPage == true && string.IsNullOrEmpty( MainPage.News[ buttonId ].News.ReferenceURL ) == false )
                            {
                                if( Springboard.IsAppURL( MainPage.News[ buttonId ].News.ReferenceURL ) == true )
                                {
                                    NavbarFragment.HandleAppURL( MainPage.News[ buttonId ].News.ReferenceURL );
                                }
                                else
                                {
                                    // copy the news item's relevant members. That way, if we're running in debug,
                                    // and they want to override the news item, we can do that below.
                                    string newsUrl = MainPage.News[ buttonId ].News.ReferenceURL;
                                    bool newsImpersonation = MainPage.News[ buttonId ].News.IncludeImpersonationToken;
                                    bool newsExternalBrowser = MainPage.News[ buttonId ].News.ReferenceUrlLaunchesBrowser;

                                    // If we're running a debug build, see if we should override the news
                                    #if DEBUG
                                    if( DebugConfig.News_Override_Item == true )
                                    {
                                        newsUrl = DebugConfig.News_Override_ReferenceURL;
                                        newsImpersonation = DebugConfig.News_Override_IncludeImpersonationToken;
                                        newsExternalBrowser = DebugConfig.News_Override_ReferenceUrlLaunchesBrowser;
                                    }
                                    #endif

                                    TaskWebFragment.HandleUrl( newsExternalBrowser, newsImpersonation, newsUrl, this, WebFragment );
                                }
                            }
                            else
                            {
                                // store the news index they chose so we can manage the 'tap details' page.
                                DetailsPage.SetNewsInfo( MainPage.News[ buttonId ].News.Title,
                                                         MainPage.News[ buttonId ].News.Description,
                                                         MainPage.News[ buttonId ].News.GetDeveloperInfo( ),
                                                         MainPage.News[ buttonId ].News.ReferenceURL,
                                                         MainPage.News[ buttonId ].News.ReferenceUrlLaunchesBrowser,
                                                         MainPage.News[ buttonId ].News.IncludeImpersonationToken,
                                                         MainPage.News[ buttonId ].News.HeaderImageName,
                                                         MainPage.News[ buttonId ].News.HeaderImageURL );

                                PresentFragment( DetailsPage, true );
                            }
                        }
                        else if ( source == DetailsPage )
                        {
                            // otherwise visit the reference URL
                            if ( buttonId == Resource.Id.news_details_launch_url )
                            {
                                // if this is an app url, handle it internally
                                if( Springboard.IsAppURL( DetailsPage.ReferenceURL ) == true )
                                {
                                    NavbarFragment.HandleAppURL( DetailsPage.ReferenceURL );
                                }
                                else
                                {
                                    // copy the news item's relevant members. That way, if we're running in debug,
                                    // and they want to override the news item, we can do that below.
                                    string newsUrl = DetailsPage.ReferenceURL;
                                    bool newsImpersonation = DetailsPage.IncludeImpersonationToken;
                                    bool newsExternalBrowser = DetailsPage.ReferenceURLLaunchesBrowser;

                                    // If we're running a debug build, see if we should override the news
                                    #if DEBUG
                                    if( DebugConfig.News_Override_Item == true )
                                    {
                                        newsUrl = DebugConfig.News_Override_ReferenceURL;
                                        newsImpersonation = DebugConfig.News_Override_IncludeImpersonationToken;
                                        newsExternalBrowser = DebugConfig.News_Override_ReferenceUrlLaunchesBrowser;
                                    }
                                    #endif

                                    TaskWebFragment.HandleUrl( newsExternalBrowser, newsImpersonation, newsUrl, this, WebFragment );
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
