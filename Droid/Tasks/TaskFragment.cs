
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Java.Lang.Reflect;

namespace Droid
{
    namespace Tasks
    {
        /// <summary>
        /// A task fragment is simply a fragment for a page of a task.
        /// This provides a common interface that allows us
        /// to work with the fragments of tasks in an abstract manner.
        /// </summary>
        public class TaskFragment : Fragment, View.IOnTouchListener
        {            
            /// <summary>
            /// Manages forwarding gestures to the carousel
            /// </summary>
            public class TaskFragmentGestureDetector : GestureDetector.SimpleOnGestureListener
            {
                public TaskFragment Parent { get; set; }

                public TaskFragmentGestureDetector( TaskFragment parent )
                {
                    Parent = parent;
                }

                public override bool OnDown(MotionEvent e)
                {
                    // Make the TaskFragment handle this
                    Parent.OnDownGesture( e );
                    return true;
                }

                public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
                {
                    Parent.OnFlingGesture( e1, e2, velocityX, velocityY );
                    return base.OnFling(e1, e2, velocityX, velocityY);
                }

                public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
                {
                    Parent.OnScrollGesture( e1, e2, distanceX, distanceY );
                    return base.OnScroll(e1, e2, distanceX, distanceY);
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    Parent.OnDoubleTap( e );
                    return base.OnDoubleTap( e );
                }
            }

            public Task ParentTask { get; set; }

            GestureDetector GestureDetector { get; set; }

            public TaskFragment( ) : base( )
            {
                GestureDetector = new GestureDetector( Rock.Mobile.PlatformSpecific.Android.Core.Context, new TaskFragmentGestureDetector( this ) );
            }

            public virtual bool OnDownGesture( MotionEvent e )
            {
                ParentTask.NavbarFragment.OnDown( e );
                return false;
            }

            public virtual bool OnDoubleTap(MotionEvent e)
            {
                return false;
            }

            public virtual bool OnFlingGesture( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
            {
                // let the navbar know we're flicking
                ParentTask.NavbarFragment.OnFlick( e1, e2, velocityX, velocityY );
                return false;
            }

            public virtual bool OnScrollGesture( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
            {
                // let the navbar know we're scrolling
                ParentTask.NavbarFragment.OnScroll( e1, e2, distanceX, distanceY );
                return false;
            }

            public override void OnDetach()
            {
                base.OnDetach();

                // See http://stackoverflow.com/questions/15207305/getting-the-error-java-lang-illegalstateexception-activity-has-been-destroyed
                // This seems to be a bug in the newly added support for nested fragments. 
                // Basically, the child FragmentManager ends up with a broken internal state when it is detached from the activity. 
                // A short-term workaround that fixed it for me is to add the following to onDetach() of every Fragment which you call getChildFragmentManager() on:
                // If you look at the implementation of Fragment, you'll see that when moving to the detached state, it'll reset its internal state.
                // However, it doesn't reset mChildFragmentManager (this is a bug in the current version of the support library). 
                // This causes it to not reattach the child fragment manager when the Fragment is reattached, causing the exception you saw. 
                try
                {
                    Field childFragmentManager = Java.Lang.Class.ForName( "android.app.Fragment" ).GetDeclaredField( "mChildFragmentManager" );
                    childFragmentManager.Accessible = true;
                    childFragmentManager.Set( this, null );
                }
                catch
                {
                }
                Rock.Mobile.Util.Debug.WriteLine( "Detaching" );
            }

            /// <summary>
            /// Called by the parent task when it's ok with the fragment
            /// displaying itself. Prior to this, it's ok to do basic UI setup,
            /// but expensive operations should wait until this is called.
            /// </summary>
            public virtual void TaskReadyForFragmentDisplay( )
            {
            }

            /// <summary>
            /// Called by the OnTouchListener. This is the only method OnTouch calls.
            /// If you override this, you need to acknowledge it returning true and
            /// return true as well
            /// </summary>
            /// <param name="v">V.</param>
            /// <param name="e">E.</param>
            public virtual bool OnTouch( View v, MotionEvent e )
            {
                if ( GestureDetector.OnTouchEvent( e ) == true )
                {
                    return true;
                }
                else
                {
                    switch ( e.Action )
                    {
                        case MotionEventActions.Up:
                        {
                            ParentTask.NavbarFragment.OnUp( e );
                            break;
                        }
                    }

                    return false;
                }
            }
        }
    }
}
