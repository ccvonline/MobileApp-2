using System;
using Android.App;
using Android.Views;
using MobileApp.Shared.Strings;
using MobileApp.Shared;
using MobileApp.Shared.PrivateConfig;
using System.Collections.Generic;
using System.Linq;
using MobileApp;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class ConnectTask : Task
            {
                ConnectPrimaryFragment MainPage { get; set; }
                GroupFinderFragment GroupFinder { get; set; }
                GroupInfoFragment GroupInfo { get; set; }
                JoinGroupFragment JoinGroup { get; set; }
                TaskWebFragment WebFragment { get; set; }

                public ConnectTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)

                    // Note: Fragment Tags must be the fully qualified name of the class, including its namespaces.
                    // This is how Android will find it when searching.
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.ConnectPrimaryFragment" ) as ConnectPrimaryFragment;
                    if ( MainPage == null )
                    {
                        MainPage = new ConnectPrimaryFragment();
                    }
                    MainPage.ParentTask = this;

                    GroupFinder = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.GroupFinderFragment" ) as GroupFinderFragment;
                    if ( GroupFinder == null )
                    {
                        GroupFinder = new GroupFinderFragment();
                    }
                    GroupFinder.ParentTask = this;

                    GroupInfo = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.GroupInfoFragment" ) as GroupInfoFragment;
                    if ( GroupInfo == null )
                    {
                        GroupInfo = new GroupInfoFragment();
                    }
                    GroupInfo.ParentTask = this;

                    JoinGroup = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.JoinGroupFragment" ) as JoinGroupFragment;
                    if ( JoinGroup == null )
                    {
                        JoinGroup = new JoinGroupFragment();
                    }
                    JoinGroup.ParentTask = this;

                    WebFragment = new TaskWebFragment( );
                    WebFragment.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override string Command_Keyword ()
                {
                    return PrivateGeneralConfig.App_URL_Task_Connect; 
                }

                public override bool OnBackPressed( )
                {
                    if ( WebFragment.IsVisible == true )
                    {
                        return WebFragment.OnBackPressed( );
                    }
                    return false;
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );
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
                                    PresentFragment( GroupFinder, true );
                                }
                                else
                                {
                                    List<ConnectLink> engagedEntries = ConnectLink.BuildGetEngagedList( );

                                    ConnectLink connectLink = engagedEntries.Where( e => e.Command_Keyword == arguments[ 1 ] ).SingleOrDefault( );
                                    if( connectLink != null )
                                    {
                                        // now go to the requested URL
                                        TaskWebFragment.HandleUrl( false, true, connectLink.Url, this, WebFragment );
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                public override void OnClick(Fragment source, int buttonId, object context)
                {
                    base.OnClick(source, buttonId, context);

                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            ConnectLink linkEntry = (ConnectLink)context;

                            // group finder is the only connect link that doesn't use an embedded webView.
                            if ( linkEntry.Title == ConnectStrings.Main_Connect_GroupFinder )
                            {
                                // launch group finder (and have it auto-show the search)
                                GroupFinder.ShowSearchOnAppear = true;

                                // if we're logged in, give it a starting address
                                if ( MobileApp.Shared.Network.RockMobileUser.Instance.LoggedIn == true && MobileApp.Shared.Network.RockMobileUser.Instance.HasFullAddress( ) )
                                {
                                    GroupFinder.SetSearchAddress( MobileApp.Shared.Network.RockMobileUser.Instance.Street1( ), 
                                        MobileApp.Shared.Network.RockMobileUser.Instance.City( ),
                                        MobileApp.Shared.Network.RockMobileUser.Instance.State( ),
                                        MobileApp.Shared.Network.RockMobileUser.Instance.Zip( ) );
                                }

                                PresentFragment( GroupFinder, true );
                            }
                            else
                            {
                                // launch the ConnectWebFragment.
                                TaskWebFragment.HandleUrl( false, true, linkEntry.Url, this, WebFragment );
                            }
                        }
                        else if ( source == GroupFinder )
                        {
                            // turn off auto-show search so that if the user presses 'back', we don't pop it up again.
                            GroupFinder.ShowSearchOnAppear = false;
                            
                            MobileAppApi.GroupSearchResult entry = (MobileAppApi.GroupSearchResult )context;

                            GroupInfo.GroupEntry = entry;

                            PresentFragment( GroupInfo, true );
                        }
                        else if ( source == GroupInfo )
                        {
                            MobileAppApi.GroupSearchResult entry = (MobileAppApi.GroupSearchResult )context;

                            JoinGroup.GroupTitle = entry.Name;
                            JoinGroup.Distance = string.Format( "{0:##.0} {1}", entry.DistanceFromSource, ConnectStrings.GroupFinder_MilesSuffix );
                            JoinGroup.GroupID = entry.Id;
                            JoinGroup.MeetingTime = string.IsNullOrEmpty( entry.MeetingTime) == false ? entry.MeetingTime : ConnectStrings.GroupFinder_ContactForTime;

                            PresentFragment( JoinGroup, true );
                        }
                    }
                }
            }
        }
    }
}

