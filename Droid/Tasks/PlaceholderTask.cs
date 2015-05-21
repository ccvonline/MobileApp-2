using System;
using Android.OS;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace Placeholder
        {
            public class PlaceholderFragment : TaskFragment
            {
                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    return view;
                }
            }

            public class PlaceholderTask : Task
            {
                PlaceholderFragment MainPage { get; set; }

                public PlaceholderTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (PlaceholderFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Placeholder.PlaceholderFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new PlaceholderFragment( );
                    }
                    MainPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

