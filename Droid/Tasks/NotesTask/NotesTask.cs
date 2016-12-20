using System;
using Android.Views;
using MobileApp.Shared.Network;
using MobileApp.Shared.Config;
using MobileApp.Shared.PrivateConfig;
using Android.App;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesTask : Task
            {
                NotesFragment NotesPage { get; set; }
                NotesPrimaryFragment MainPage { get; set; }
                NotesDetailsFragment DetailsPage { get; set; }
                NotesWatchFragment WatchPage { get; set; }
                NotesListenFragment ListenPage { get; set; }
                NotesDiscGuideFragment DiscGuidePage { get; set; }
                TaskWebFragment WebViewPage { get; set; }

                public NotesTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)

                    // Note: Fragment Tags must be the fully qualified name of the class, including its namespaces.
                    // This is how Android will find it when searching.
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesPrimaryFragment" ) as NotesPrimaryFragment;
                    if ( MainPage == null )
                    {
                        MainPage = new NotesPrimaryFragment();
                    }
                    MainPage.ParentTask = this;

                    DetailsPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesDetailsFragment" ) as NotesDetailsFragment;
                    if ( DetailsPage == null )
                    {
                        DetailsPage = new NotesDetailsFragment();
                    }
                    DetailsPage.ParentTask = this;

                    NotesPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesFragment" ) as NotesFragment;
                    if ( NotesPage == null )
                    {
                        NotesPage = new NotesFragment( );
                    }
                    NotesPage.ParentTask = this;

                    WatchPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesWatchFragment" ) as NotesWatchFragment;
                    if ( WatchPage == null )
                    {
                        WatchPage = new NotesWatchFragment();
                    }
                    WatchPage.ParentTask = this;

                    ListenPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesListenFragment" ) as NotesListenFragment;
                    if ( ListenPage == null )
                    {
                        ListenPage = new NotesListenFragment();
                    }
                    ListenPage.ParentTask = this;

                    DiscGuidePage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesDiscGuideFragment" ) as NotesDiscGuideFragment;
                    if( DiscGuidePage == null )
                    {
                        DiscGuidePage = new NotesDiscGuideFragment();
                    }
                    DiscGuidePage.ParentTask = this;

                    WebViewPage = new TaskWebFragment( );
                    WebViewPage.ParentTask = this;
                }

                public override string Command_Keyword ()
                {
                    return PrivateGeneralConfig.App_URL_Task_Notes;
                }

                public override void PerformAction( string command, string[] arguments )
                {
                    base.PerformAction( command, arguments );

                    switch ( command )
                    {
                        case PrivateGeneralConfig.App_URL_Commands_Goto:
                        {
                            // make sure the argument is for us (and it wants more than just our root page)
                            if( arguments[ 0 ] == Command_Keyword( ) && arguments.Length > 1 )
                            {
                                // if they want a "read" page, we support that.
                                if( arguments[ 1 ] == PrivateGeneralConfig.App_URL_Page_Read )
                                {
                                    if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                                    {
                                        NotesPage.NoteName = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].Name;
                                        NotesPage.NoteUrl = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl;
                                        NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                        PresentFragment( NotesPage, true );
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                public override bool CanContainerPan()
                {
                    if ( NotesPage.IsVisible == true )
                    {
                        return NotesPage.MovingUserNote ? false : true;
                    }
                    else
                    {
                        return true;
                    }
                }

                public override bool OnBackPressed( )
                {
                    if ( WebViewPage.IsVisible == true )
                    {
                        return WebViewPage.OnBackPressed( );
                    }
                    return false;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public bool IsReadingNotes( )
                {
                    // if we're taking notes or in the web view, return true.
                    return (NotesPage.IsVisible || WebViewPage.IsVisible);
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null)
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            // on the main page, if the buttonId was -1, the user tapped the header,
                            // so we need to either go to the Watch or Take Notes page
                            if ( buttonId == -1 )
                            {
                                // the context is the button they clicked (watch or take notes)
                                int buttonChoice = (int)context;

                                // 0 is listen
                                if ( buttonChoice == 0 )
                                {
                                    ListenPage.MediaUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].AudioUrl;
                                    ListenPage.ShareUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;
                                    ListenPage.Name = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    PresentFragment( ListenPage, true );
                                }
                                // 1 is watch
                                else if ( buttonChoice == 1 )
                                {
                                    WatchPage.MediaUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl;
                                    WatchPage.ShareUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;
                                    WatchPage.Name = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    PresentFragment( WatchPage, true );
                                }
                                // 2 is read
                                else if ( buttonChoice == 2 )
                                {
                                    NotesPage.NoteUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl;
                                    NotesPage.NoteName = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                    PresentFragment( NotesPage, true );
                                }
                                // 3 is discussion guide
                                else if ( buttonChoice == 3 )
                                {
                                    DiscGuidePage.DiscGuideURL = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].DiscussionGuideUrl;
                                    PresentFragment( DiscGuidePage, true );
                                }
                            }
                            else
                            {
                                DetailsPage.Series = MainPage.SeriesEntries[ buttonId ].Series;
                                PresentFragment( DetailsPage, true );
                            }
                        }
                        else if ( source == DetailsPage )
                        {
                            // the context is the button they clicked (watch or take notes)
                            int buttonChoice = (int)context;

                            // 0 is listen
                            if ( buttonChoice == 0 )
                            {
                                ListenPage.MediaUrl = DetailsPage.Series.Messages[ buttonId ].AudioUrl;
                                ListenPage.ShareUrl = DetailsPage.Series.Messages[ buttonId ].ShareUrl;
                                ListenPage.Name = DetailsPage.Series.Messages[ buttonId ].Name;
                                PresentFragment( ListenPage, true );
                            }
                            // 1 is watch
                            else if ( buttonChoice == 1 )
                            {
                                WatchPage.MediaUrl = DetailsPage.Series.Messages[ buttonId ].WatchUrl;
                                WatchPage.ShareUrl = DetailsPage.Series.Messages[ buttonId ].ShareUrl;
                                WatchPage.Name = DetailsPage.Series.Messages[ buttonId ].Name;
                                PresentFragment( WatchPage, true );
                            }
                            // 2 is read
                            else if ( buttonChoice == 2 )
                            {
                                NotesPage.NoteUrl = DetailsPage.Series.Messages[ buttonId ].NoteUrl;
                                NotesPage.NoteName = DetailsPage.Series.Messages[ buttonId ].Name;
                                NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                PresentFragment( NotesPage, true );
                            }
                            // 3 is discussion guide
                            else if ( buttonChoice == 3 )
                            {
                                DiscGuidePage.DiscGuideURL = DetailsPage.Series.Messages[ buttonId ].DiscussionGuideUrl;
                                PresentFragment( DiscGuidePage, true );
                            }
                        }
                        else if ( source == NotesPage )
                        {
                            NotesFragment.UrlClickParams clickParams = (NotesFragment.UrlClickParams)context;
                            
                            // the context is the activeURL to visit.
                            WebViewPage.DisableIdleTimer = true;
                            TaskWebFragment.HandleUrl( clickParams.UseExternalBrowser, 
                                                       clickParams.UseImpersonationToken, 
                                                       clickParams.Url,
                                                       this, 
                                                       WebViewPage );
                        }
                        else if ( source == DiscGuidePage )
                        {
                            // Discussion Guide page only has one button, so if it was clicked,
                            // let them view the guide.

                            WebViewPage.DisableIdleTimer = true;
                            TaskWebFragment.HandleUrl( true, 
                                                       false, 
                                                       DiscGuidePage.DiscGuideURL,
                                                       this, 
                                                       WebViewPage );
                        }
                    }
                }

                public static string FormatBillboardImageName( string seriesName )
                {
                    return seriesName + "_bb";
                }

                public static string FormatThumbImageName( string seriesName )
                {
                    return seriesName + "_thumb";
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );
                }
            }
        }
    }
}
