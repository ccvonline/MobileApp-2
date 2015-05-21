using System;
using Android.App;
using Android.Content;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerTask : Task
            {
                PrayerPrimaryFragment MainPage { get; set; }
                PrayerCreateFragment CreatePage { get; set; }
                PrayerPostFragment PostPage { get; set; }

                public PrayerTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = new PrayerPrimaryFragment( );
                    MainPage.ParentTask = this;

                    CreatePage = new PrayerCreateFragment();
                    CreatePage.ParentTask = this;

                    PostPage = new PrayerPostFragment();
                    PostPage.ParentTask = this;
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );
                }

                public override void Deactivate(bool forPause)
                {
                    base.Deactivate(forPause);

                    // if we're deactivating because they navigated away,
                    // reset the prayer state
                    if ( forPause == false )
                    {
                        MainPage.ResetPrayerStatus( );
                    }
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null)
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // if the main page is the source
                        if ( source == MainPage )
                        {
                            // and it's button id 0, goto the create page
                            if ( buttonId == 0 )
                            {
                                PresentFragment( CreatePage, true );
                            }
                        }
                        else if ( source == CreatePage )
                        {
                            if ( buttonId == 0 )
                            {
                                PostPage.PrayerRequest = (Rock.Client.PrayerRequest)context;
                                PresentFragment( PostPage, true );
                            }
                        }
                        else if ( source == PostPage )
                        {
                            // this is our first / only "circular" navigation, as we're returning to the main page after 
                            // having posted a prayer. In which case, clear the back stack.
                            NavbarFragment.FragmentManager.PopBackStack( null, PopBackStackFlags.Inclusive );
                            PresentFragment( MainPage, false );
                        }
                    }
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );

                    // ignore Up gestures. Do not reveal the nav toolbar.
                }
            }
        }
    }
}

