using System;
using Android.App;
using Android.Views;
using MobileApp.Shared.Config;
using MobileApp.Shared.Analytics;
using MobileApp.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace Give
        {
            public class GiveTask : Task
            {
                GivePrimaryFragment MainPage { get; set; }

                public GiveTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = navFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Give.GivePrimaryFragment" ) as GivePrimaryFragment;
                    if ( MainPage == null )
                    {
                        MainPage = new GivePrimaryFragment();
                    }
                    MainPage.ParentTask = this;
                }

                public override string Command_Keyword ()
                {
                    return PrivateGeneralConfig.App_URL_Task_Give;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );
                }

                public override void Activate(bool forResume)
                {
                    base.Activate(forResume);
                }
            }
        }
    }
}

