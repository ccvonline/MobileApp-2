using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace iOS
{
    public class Task
    {
        public CGRect ContainerBounds { get; set; }
        protected TaskUINavigationController ParentViewController { get; set; }
        public NavToolbar NavToolbar { get; set; }
        protected UIStoryboard Storyboard { get; set; }
        protected TaskUIViewController ActiveViewController { get; set; }

        public Task( string storyboardName )
        {
            // activities don't HAVE to have a storyboard
            if( false == string.IsNullOrEmpty( storyboardName ) )
            {
                Storyboard = UIStoryboard.FromName( storyboardName, null );
            }
        }

        /// <summary>
        /// The root function to call when changing view controllers within a task. If changing in code,
        /// directly call this from the view controller. If using a storyboard, hook the segue to TaskTransition
        /// in ContainerViewController.cs, which will cause this function to be called.
        /// </summary>
        /// <param name="sourceViewController">Source view controller.</param>
        /// <param name="destinationViewController">Destination view controller.</param>
        public void PerformSegue( UIViewController sourceViewController, UIViewController destinationViewController )
        {
            // take this opportunity to give the presenting view controller a pointer to the active task
            // so it can receive callbacks.
            TaskUIViewController viewController = destinationViewController as TaskUIViewController;
            if( viewController == null )
            {
                throw new InvalidCastException( "View Controllers used by Activities must be of type TaskUIViewController" );
            }

            ParentViewController.PushViewController( destinationViewController, true );
        }

        /// <summary>
        /// Called when the task is going to be the forefront task.
        /// Allows it to do any work necessary before being interacted with.
        /// Ex: Notes might disable the phone's sleep
        /// This is NOT called when the application comes into the foreground.
        /// </summary>
        /// <param name="parentViewController">Parent view controller.</param>
        public virtual void MakeActive( TaskUINavigationController parentViewController, NavToolbar navToolbar, CGRect containerBounds )
        {
            ContainerBounds = containerBounds;

            ParentViewController = parentViewController;

            NavToolbar = navToolbar;
        }

        /// <summary>
        /// This acts as a sort of "event" based system, where actions can be performed on tasks
        /// without the caller knowing specifically what task is running.
        /// The primary example would be the "Take Notes" billboard launching the Notes
        /// Task and presenting the sermon notes VC.
        /// </summary>
        /// <param name="action">Action.</param>
        public virtual void PerformAction( string action )
        {
        }

        /// <summary>
        /// Called when the task is going away so another task can be interacted with.
        /// Allows it to undo any work done in MakeActive.
        /// Ex: Notes might RE-enable the phone's sleep.
        /// This is NOT called when the application goes into the background.
        /// </summary>
        public virtual void MakeInActive( )
        {
            // always clear our parent view controller when going inactive
            ActiveViewController = null;
            ParentViewController = null;
            NavToolbar = null;
        }

        /// <summary>
        /// Called when a new view controller is shown by the parent navigation controller.
        /// This is useful so the task can evaluate what viewcontroller was just shown
        /// and update itself or the toolbar accordingly.
        /// It is ALWAYS called, whether a view controller is being pushed, or revealed because the stack is being popped.
        /// </summary>
        /// <param name="viewController">View controller.</param>
        public virtual void WillShowViewController( TaskUIViewController viewController )
        {
            ActiveViewController = viewController;

            // ensure this controller has the parent task
            viewController.Task = this;

            // and that it knows what its dimensions can be.
            viewController.View.Bounds = ContainerBounds;

            viewController.LayoutChanged( );
        }

        /// <summary>
        /// Called when the container wants to allow panning (typically to allow the springboard to be revealed).
        /// Return false to disallow.
        /// </summary>
        /// <returns><c>true</c>, if wants pan was containered, <c>false</c> otherwise.</returns>
        public virtual bool CanContainerPan( NSSet touches, UIEvent evt )
        {
            return true;
        }

        // if a given task wants all or some of its view controllers to support landscape, it should return true here.
        public virtual bool SupportsLandscape( )
        {
            return false;
        }

        public virtual void LayoutChanging( )
        {
            ActiveViewController.LayoutChanging( );
        }

        public virtual void LayoutChanged( CGRect containerBounds )
        {
            // store the new container bounds so we can notify any view controllers we show
            ContainerBounds = containerBounds;

            // give the current view controller the new bounds
            ActiveViewController.View.Bounds = containerBounds;

            // and notify it.
            ActiveViewController.LayoutChanged( );
        }

        /// <summary>
        /// Called by the active view controller when touches ended. Allows the task to perform any
        /// necessary actions, like revealing the nav bar.
        /// </summary>
        /// <param name="TaskUIViewController">Task user interface view controller.</param>
        /// <param name="touches">Touches.</param>
        /// <param name="evt">Evt.</param>
        public virtual void TouchesEnded( TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt )
        {
        }

        /// <summary>
        /// Called by the active view controller when a user scrolls.
        /// This allows the task to perform any scroll dependent actions, like revealing the nav bar.
        /// </summary>
        /// <param name"scrollDelta">The change in scroll since the last call.</param>
        public virtual void ViewDidScroll( float scrollDelta )
        {
        }

        /// <summary>
        /// Called when the app is coming active, like from the Task Switcher. This
        /// can be called without WillEnterForeground being called, if the Task Switcher is 
        /// invoked while the app is active, and the app is picked.
        /// This is probably the more important of the two.
        /// </summary>
        public virtual void OnActivated( )
        {
        }

        /// <summary>
        /// Called then when app comes back from being backgrounded. Good opportunity
        /// to re-init things like Idle Timer disabling.
        /// </summary>
        public virtual void WillEnterForeground( )
        {
        }

        /// <summary>
        /// Called when the application will go into the background.
        /// This is NOT called when the task goes into the background.
        /// </summary>
        public virtual void AppOnResignActive( )
        {
        }

        public virtual void AppDidEnterBackground( )
        {
        }

        public virtual void AppWillTerminate( )
        {
        }

        /// <summary>
        /// 99% of the time, the main container can decide if the back button should be enabled.
        /// Every now and then, a task might need to force it on. (Like the notes webview)
        /// </summary>
        public virtual bool ShouldForceBackButtonEnabled( )
        {
            return false;
        }
    }
}
