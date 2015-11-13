using System;
using Android.App;
using Android.Content;
using Android.Views;
using App.Shared.Network;
using System.Collections.Generic;
using App.Shared.Config;
using App.Shared.PrivateConfig;
using MobileApp;

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
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.NewsPrimaryFragment" ) as NewsPrimaryFragment;
                    if ( MainPage == null )
                    {
                        MainPage = new NewsPrimaryFragment();
                    }
                    MainPage.ParentTask = this;

                    DetailsPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.NewsDetailsFragment" ) as NewsDetailsFragment;
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
                            Rock.Client.Campus campus = RockGeneralData.Instance.Data.CampusFromId( RockMobileUser.Instance.ViewingCampus );
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
                                    if( News.Count < PrivateNewsConfig.MaxNews || App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled == true )
                                    {
                                        News.Add( new RockNews( newsItem ) );
                                    }
                                }
                            }
                        } );
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void PerformTaskAction(string action)
                {
                    base.PerformTaskAction(action);

                    switch ( action )
                    {
                        case PrivateGeneralConfig.TaskAction_NewsReload:
                        {
                            // for this action, we want to reload our news,
                            ReloadNews( );

                            break;
                        }

                        case PrivateGeneralConfig.TaskAction_CampusChanged:
                        {
                            ReloadNews( );

                            // let the page have the latest news
                            MainPage.UpdateNews( News );
                            break;
                        }
                    }
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null )
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // if the main page had a VALID news item clicked, go to it
                        if ( source == MainPage && buttonId < News.Count )
                        {
                            // either take them to the details page, or skip it and go straight to Learn More.
                            if ( MainPage.News[ buttonId ].News.SkipDetailsPage == true && string.IsNullOrEmpty( MainPage.News[ buttonId ].News.ReferenceURL ) == false )
                            {
                                TaskWebFragment.HandleUrl( MainPage.News[ buttonId ].News.ReferenceUrlLaunchesBrowser, 
                                    MainPage.News[ buttonId ].News.IncludeImpersonationToken, 
                                    MainPage.News[ buttonId ].News.ReferenceURL,
                                    this, 
                                    WebFragment );
                            }
                            else
                            {
                                DetailsPage.NewsItem = MainPage.News[ buttonId ].News;
                                PresentFragment( DetailsPage, true );
                            }
                        }
                        else if ( source == DetailsPage )
                        {
                            // otherwise visit the reference URL
                            if ( buttonId == Resource.Id.news_details_launch_url )
                            {
                                TaskWebFragment.HandleUrl( DetailsPage.NewsItem.ReferenceUrlLaunchesBrowser, 
                                                           DetailsPage.NewsItem.IncludeImpersonationToken, 
                                                           DetailsPage.NewsItem.ReferenceURL,
                                                           this, 
                                                           WebFragment );
                            }
                        }
                    }
                }
            }
        }
    }
}
