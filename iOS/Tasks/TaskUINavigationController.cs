using System;
using UIKit;

namespace iOS
{
    /// <summary>
    /// A wrapper class that gives us some helper functions that Tasks can use in managing themselves.
    /// </summary>
    public class TaskUINavigationController : UINavigationController
    {
        public void ClearViewControllerStack( )
        {
            // This function basically removes everything in the Container View Controller,
            // allowing whatever is pushed onto it next to be the root view controller.

            // An example use case: A shortcut is tapped which takes a user directly
            // to a deep View Controller within a task. Well, we don't want any back history,
            // even tho that View Controller is normally a few levels deep within the task.
            PopToRootViewController( false );

            foreach ( UIViewController controller in ChildViewControllers )
            {
                controller.ViewWillDisappear( false );
                controller.RemoveFromParentViewController( );
                controller.ViewDidDisappear( false );
            }
        }
    }
}

