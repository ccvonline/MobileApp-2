using System;
using Android.App;
using Android.Widget;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        /// <summary>
        /// A task represents a "section" of the app, like the news, group finder,
        /// notes, etc. It contains all of the pages that make up that particular section.
        /// </summary>
        public abstract class Task
        {
            /// <summary>
            /// Reference to the parent navbar fragment
            /// </summary>
            /// <value>The navbar fragment.</value>
            public NavbarFragment NavbarFragment { get; set; }

            /// <summary>
            /// True when the task is ready for the fragment to display.
            /// This could be false if, say, the task wants the fragment to wait
            /// for the springboard to close.
            /// </summary>
            /// <value><c>true</c> if task ready; otherwise, <c>false</c>.</value>
            public bool TaskReadyForFragmentDisplay { get; protected set; }

            public Task( NavbarFragment navFragment )
            {
                NavbarFragment = navFragment;
            }

            public abstract TaskFragment StartingFragment( );

            public virtual void Activate( bool forResume )
            {
                // if we're simply resuming, we dont need to reset the fragment, the task is already setup.
                if ( forResume == false )
                {
                    Rock.Mobile.Util.Debug.WriteLine( "Popping back stack" );
                    NavbarFragment.FragmentManager.PopBackStack( null, PopBackStackFlags.Inclusive );

                    // present our starting fragment, and don't allow back navigation
                    PresentFragment( StartingFragment( ), false );
                }

                // if the springboard is already closed, set ourselves as ready.
                // This is always called before any fragment methods, so the fragment
                // will be able to know if it can display or not.

                // alternatively, if we're simply resuming from a pause, it's ok to allow the note to show.
                if( NavbarFragment.ShouldTaskAllowInput( ) || forResume == true)
                {
                    TaskReadyForFragmentDisplay = true;
                }
                else
                {
                    TaskReadyForFragmentDisplay = false;
                }
            }

            public virtual void Deactivate( bool forPause )
            {
                // let the fragment know we're NOT ok with it displaying
                TaskReadyForFragmentDisplay = false;
            }

            public virtual void PerformTaskAction( string action )
            {
            }

            protected void PresentFragment( TaskFragment fragment, bool allowBack )
            {
                // allow back means "can you return to the page you're LEAVING? Not, can the
                // fragment being passed in be returned to"

                // get the fragment manager
                var ft = NavbarFragment.FragmentManager.BeginTransaction( );

                // set this as the active visible fragment in the task frame.
                string typestr = fragment.GetType( ).ToString( );
                ft.Replace( Resource.Id.activetask, fragment, typestr );

                // do a nice crossfade
                ft.SetTransition( FragmentTransit.FragmentFade );

                // if back was requested, put it in our stack
                if ( allowBack )
                {
                    ft.AddToBackStack( fragment.ToString( ) );
                }

                // do the transaction
                ft.Commit( );
            }

            public virtual bool CanContainerPan()
            {
                return true;
            }

            public virtual void SpringboardDidAnimate( bool springboardRevealed )
            {
                // did the springboard just close?
                if( springboardRevealed == false )
                {
                    // if we weren't ready, let the notes know we now are.
                    if( TaskReadyForFragmentDisplay == false )
                    {
                        TaskReadyForFragmentDisplay = true;

                        StartingFragment( ).TaskReadyForFragmentDisplay( );
                    }
                }
            }

            public virtual void OnClick( Fragment source, int buttonId, object context = null )
            {
            }

            public virtual void OnUp( MotionEvent e )
            {
            }
        }
    }
}
