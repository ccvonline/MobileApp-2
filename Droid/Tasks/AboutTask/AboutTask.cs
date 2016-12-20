using System;
using Android.App;
using Android.Views;
using MobileApp.Shared.Config;
using MobileApp.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace About
        {
            public class AboutTask : Task
            {
                TaskWebFragment MainPage { get; set; }

                public AboutTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)

                    // Note: Fragment Tags must be the fully qualified name of the class, including its namespaces.
                    // This is how Android will find it when searching.
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.TaskWebFragment" ) as TaskWebFragment;
                    if( MainPage == null )
                    {
                        MainPage = new TaskWebFragment( );
                    }
                    MainPage.ParentTask = this;
                }

                public override string Command_Keyword ()
                {
                    return PrivateGeneralConfig.App_URL_Task_About; 
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void Activate(bool forResume)
                {
                    base.Activate(forResume);

                    if ( forResume == false )
                    {
                        TaskWebFragment.HandleUrl( false, 
                            true, 
                            AboutConfig.Url,
                            this, 
                            MainPage );
                    }
                }

                public override bool OnBackPressed( )
                {
                    if ( MainPage.IsVisible == true )
                    {
                        return MainPage.OnBackPressed( );
                    }
                    return false;
                }
            }
        }
    }
}

