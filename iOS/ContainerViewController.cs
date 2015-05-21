using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using System.Collections.Generic;
using System.Threading;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.PrivateConfig;

namespace iOS
{
    /// <summary>
    /// A delegate managing the navBar owned by ContainerViewController.
    /// ContainerViewController needs to know when a user changes controllers via the navBar.
    /// </summary>
    public class NavBarDelegate : UINavigationControllerDelegate
    {
        public ContainerViewController ParentController { get; set; }
       
        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            // notify our parent
            ParentController.NavWillShowViewController( viewController );
        }

        public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            // notify our parent
            ParentController.NavDidShowViewController( viewController );
        }
    }

    /// <summary>
    /// The "frame" surrounding all activities. This manages the main navigation bar
    /// that contains the Springboard Reveal button so that it can be in one place rather
    /// than making every single view controller do it.
    /// </summary>
	public partial class ContainerViewController : UIViewController
	{
        /// <summary>
        /// The task currently being displayed.
        /// </summary>
        Task _CurrentTask;
        public Task CurrentTask { get { return _CurrentTask; } }

        /// <summary>
        /// Each task is placed as a child within this SubNavigation controller.
        /// Instead of using a NavigationBar to go back, however, they use the SubNavToolbar.
        /// It's basically an invisible container so that we can use the iOS View Controller stack to
        /// manage our view controllers
        /// </summary>
        /// <value>The sub navigation controller.</value>
        public TaskUINavigationController SubNavigationController { get; set; }
        public NavToolbar SubNavToolbar { get; set; }

        /// <summary>
        /// True when the controller within an task is animating (from a navigate forward/backward)
        /// This is tracked so we don't allow multiple navigation requests at once (like if a user spammed the back button)
        /// </summary>
        /// <value><c>true</c> if task controller animating; otherwise, <c>false</c>.</value>
        bool TaskControllerAnimating { get; set; }

        protected UIButton SpringboardRevealButton { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
            TaskTransition.ContainerViewController = this;
		}

        public ContainerViewController () : base ()
        {
            TaskTransition.ContainerViewController = this;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // container view must have a black background so that the ticks
            // before the task displays don't cause a flash
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            View.Layer.AnchorPoint = CGPoint.Empty;
            View.Layer.Position = CGPoint.Empty;

            // First setup the SpringboardReveal button, which rests in the upper left
            // of the MainNavigationUI. (We must do it here because the ContainerViewController's
            // NavBar is the active one.)
            NSString buttonLabel = new NSString(PrivatePrimaryNavBarConfig.RevealButton_Text);

            SpringboardRevealButton = new UIButton(UIButtonType.System);
            SpringboardRevealButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivatePrimaryNavBarConfig.RevealButton_Size );
            SpringboardRevealButton.SetTitle( buttonLabel.ToString( ), UIControlState.Normal );

            // determine its dimensions
            CGSize buttonSize = buttonLabel.StringSize( SpringboardRevealButton.Font );
            SpringboardRevealButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );

            // set its callback
            SpringboardRevealButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    (ParentViewController as MainUINavigationController).SpringboardRevealButtonTouchUp( );
                };
            this.NavigationItem.SetLeftBarButtonItem( new UIBarButtonItem( SpringboardRevealButton ), false );
            //

            // set the title image for the bar
            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivatePrimaryNavBarConfig.LogoFile_iOS;
            this.NavigationItem.TitleView = new UIImageView( new UIImage( imagePath ) );


            // Now create the sub-navigation, which includes
            // the NavToolbar used to let the user navigate
            CreateSubNavigationController( );
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            if( SpringboardRevealButton != null )
            {
                SpringboardRevealButton.Enabled = enabled;
            }
        }

        protected void CreateSubNavigationController( )
        {
            // Create the sub navigation controller
            SubNavigationController = new TaskUINavigationController();
            SubNavigationController.NavigationBarHidden = true;
            SubNavigationController.Delegate = new NavBarDelegate( ) { ParentController = this };

            // setup the toolbar that will manage task navigation and any other tasks the task needs
            SubNavToolbar = new NavToolbar();

            SubNavToolbar.BarTintColor = Rock.Mobile.UI.Util.GetUIColor( PrivateSubNavToolbarConfig.BackgroundColor );
            SubNavToolbar.Layer.Opacity = PrivateSubNavToolbarConfig.Opacity;
            SubNavigationController.View.AddSubview( SubNavToolbar );

            // add the back button
            SubNavToolbar.SetBackButtonAction( delegate
                {
                    // don't allow multiple back presses at once
                    if( TaskControllerAnimating == false )
                    {
                        TaskControllerAnimating = true;
                        SubNavigationController.PopViewController( true );
                    }
                });

            // add this navigation controller (and its toolbar) as a child
            // of this ContainerViewController, which will effectively make it a child
            // of the primary navigation controller.
            AddChildViewController( SubNavigationController );
            View.AddSubview( SubNavigationController.View );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            SubNavToolbar.ViewDidLayoutSubviews( );
        }

        public void LayoutChanging( )
        {
            UpdateBackButton( );

            if ( SpringboardViewController.IsDeviceLandscape( ) == true )
            {
                EnableSpringboardRevealButton( false );
            }
            else
            {
                EnableSpringboardRevealButton( true );
            }

            if ( CurrentTask != null )
            {
                CurrentTask.LayoutChanging( );
            }
        }

        public void LayoutChanged( )
        {
            if ( SubNavigationController != null )
            {
                SubNavigationController.View.Bounds = View.Bounds;
            }

            if ( CurrentTask != null )
            {
                CurrentTask.LayoutChanged( View.Bounds );
            }
        }

        public bool SupportsLandscape( )
        {
            if( CurrentTask != null )
            {
                return CurrentTask.SupportsLandscape( );
            }

            return false;
        }

        public void UpdateBackButton( )
        {
            // if there are VCs in the stack
            if ( SubNavigationController.ViewControllers.Length > 1 )
            {
                // only allow back if we're in regular landscape or portrait mode
                bool allowBack = false;
                if ( SpringboardViewController.IsLandscapeWide( ) == true || SpringboardViewController.IsDevicePortrait( ) == true )
                {
                    allowBack = true;
                }

                // OR, if there's a current task, ask it if there should be an override
                if ( CurrentTask != null )
                {
                    if ( CurrentTask.ShouldForceBackButtonEnabled( ) )
                    {
                        allowBack = true;
                    }
                }
                SubNavToolbar.SetBackButtonEnabled( allowBack );
            }
            else
            {
                SubNavToolbar.SetBackButtonEnabled( false );
            }
        }

        public void NavWillShowViewController( UIViewController viewController )
        {
            // let the current task know which of its view controllers was just shown.
            if( CurrentTask != null )
            {
                TaskControllerAnimating = true;
                CurrentTask.WillShowViewController( (TaskUIViewController) viewController );
            }

            UpdateBackButton( );
        }

        public void NavDidShowViewController( UIViewController viewController )
        {
            // once the animation is COMPLETE, we can turn off the flag
            // and allow another back press.
            TaskControllerAnimating = false;
        }

        public void ActivateTask( Task task )
        {
            // reset our stack and remove all current view controllers 
            // before changing activities
            SubNavigationController.ClearViewControllerStack( );

            if( CurrentTask != null )
            {
                CurrentTask.MakeInActive( );
            }

            _CurrentTask = task;

            CurrentTask.MakeActive( SubNavigationController, SubNavToolbar, View.Bounds );
        }

        public void PerformSegue( UIViewController sourceViewController, UIViewController destinationViewController )
        {
            // notify the active task regarding the change.
            if( CurrentTask != null )
            {
                CurrentTask.PerformSegue( sourceViewController, destinationViewController );
            }
        }

        public void OnActivated( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.OnActivated( );
            }
        }

        public void WillEnterForeground( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.WillEnterForeground( );
            }
        }

        public void OnResignActive()
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppOnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppDidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentTask != null )
            {
                CurrentTask.AppWillTerminate( );
            }
        }
	}

    //NOTE: To perform a transition between ViewControllers that are part of a task,
    // simply create a custom Segue in the Storyboard, and set it to "TaskTransition"

    // Define our task transition class that notifies the container
    // about the transition, so it can ensure the next view controller receives
    // a reference to the active task.
    [Register("TaskTransition")]
    class TaskTransition : UIStoryboardSegue
    {
        public static ContainerViewController ContainerViewController { get; set; }

        public TaskTransition( IntPtr handle ) : base( handle )
        {
        }

        public override void Perform()
        {
            ContainerViewController.PerformSegue( SourceViewController, DestinationViewController );
        }
    }
}
