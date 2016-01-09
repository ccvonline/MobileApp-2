using System;
using App.Shared;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.Config;
using UIKit;
using Foundation;
using App.Shared.UI;
using CoreGraphics;
using System.Drawing;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using App.Shared.Strings;

namespace iOS
{
    public class GroupInfoViewController : TaskUIViewController
    {
        public GroupFinder.GroupEntry GroupEntry { get; set; }

        UIGroupInfo GroupInfoView { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

        public GroupInfoViewController( )
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ScrollView = new UIScrollViewWrapper();
            ScrollView.Layer.AnchorPoint = CGPoint.Empty;
            ScrollView.Parent = this;
            ScrollView.Bounds = View.Bounds;
            View.AddSubview( ScrollView );

            GroupInfoView = new UIGroupInfo();
            GroupInfoView.Create( ScrollView, View.Frame.ToRectF( ), OnJoinClicked );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // setup the values
            GroupInfoView.DisplayView( GroupEntry, delegate 
                {
                    LayoutChanged( );
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            // default the scrollview to match the screen
            ScrollView.Bounds = View.Bounds;
            ScrollView.ContentSize = View.Bounds.Size;

            // now, let the actual view perform its layout
            RectangleF joinBounds = new RectangleF( 0, 0, (float)View.Bounds.Width, (float)ScrollView.ContentSize.Height );
            GroupInfoView.LayoutChanged( joinBounds );

            // and finally update the scroll content
            nfloat controlBottom = GroupInfoView.GetControlBottom( ) + ( View.Bounds.Height * .25f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }

        void OnJoinClicked( )
        {
            // launch the join group UI
            GroupFinderJoinViewController joinController = new GroupFinderJoinViewController();

            // set the group info
            GroupFinder.GroupEntry currGroup = GroupEntry;
            joinController.GroupTitle = currGroup.Title;
            joinController.MeetingTime = string.IsNullOrEmpty( currGroup.MeetingTime ) == false ? currGroup.MeetingTime : ConnectStrings.GroupFinder_ContactForTime;
            joinController.GroupID = currGroup.Id;

            joinController.Distance = string.Format( "{0:##.0} {1}", currGroup.Distance, ConnectStrings.GroupFinder_MilesSuffix );
            /*if ( row == 0 )
            {
                joinController.Distance += " " + ConnectStrings.GroupFinder_ClosestTag;
            }*/

            // launch the view
            Task.PerformSegue( this, joinController );
        }
    }
}
