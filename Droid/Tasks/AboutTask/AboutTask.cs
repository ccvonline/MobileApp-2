using System;
using Android.App;
using Android.Views;
using App.Shared.Config;

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
                    MainPage = new TaskWebFragment( );
                    MainPage.ParentTask = this;
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
                        MainPage.DisplayUrl( AboutConfig.Url );
                    }
                }
            }
        }
    }
}

