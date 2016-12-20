using System;
using MobileApp.Shared;
using Rock.Mobile.PlatformSpecific.Util;
using MobileApp.Shared.Config;
using UIKit;
using Foundation;
using MobileApp.Shared.UI;
using CoreGraphics;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using MobileApp.Shared.Strings;

namespace iOS
{
    public class NotesDiscGuideViewController : TaskUIViewController
    {
        public string DiscGuideURL { get; set; }

        UINoteDiscGuideView DiscGuideView { get; set; }

        public NotesDiscGuideViewController( Task parentTask )
        {
            Task = parentTask;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            DiscGuideView = new UINoteDiscGuideView( View, View.Frame.ToRectF( ), OnViewClicked );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            LayoutChanged( );

            Task.NavToolbar.Reveal( true );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            DiscGuideView.SetBounds( View.Bounds.ToRectF( ) );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }

        void OnViewClicked( )
        {
            // launch the view
            TaskWebViewController.HandleUrl( false, false, DiscGuideURL, Task, this, true, false, false );
        }
    }
}
