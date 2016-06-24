using System;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.Animation;
using App.Shared.Network;
using Rock.Mobile.Util.Strings;
using App.Shared.PrivateConfig;
using MobileApp;
using System.Net;
using Rock.Mobile.Network;
using System.IO;

namespace App.Shared.UI
{
    public class UIGroupInfo
    {
        public PlatformView View { get; set; }

        GroupFinder.GroupEntry GroupEntry { get; set; }

        PlatformLabel GroupTitle { get; set; }

        PlatformImageView LeaderImage { get; set; }
        bool LeaderImageValid { get; set; }

        PlatformLabel LeaderDescHeader { get; set; }
        PlatformLabel LeaderDesc { get; set; }
        PlatformView LeaderDescLayer { get; set; }

        PlatformLabel GroupDescHeader { get; set; }
        PlatformLabel GroupDesc { get; set; }
        PlatformView GroupDescLayer { get; set; }

        PlatformLabel MeetingTime { get; set; }
        PlatformView MeetingTimeLayer { get; set; }

        PlatformLabel ChildDescHeader { get; set; }
        PlatformLabel ChildDesc { get; set; }
        PlatformView ChildDescLayer { get; set; }

        PlatformButton JoinButton { get; set; }

        UIResultView ResultView { get; set; }

        UIBlockerView BlockerView { get; set; }

        bool IsDownloading { get; set; }

        MobileApp.MobileAppApi.OnGroupSummaryResult GroupSummaryResult { get; set; }

        public delegate void OnButtonClick( );
        OnButtonClick OnButtonClicked { get; set; }

        public UIGroupInfo( )
        {
        }

        public void Create( object masterView, RectangleF frame, OnButtonClick onJoinClicked )
        {
            OnButtonClicked = onJoinClicked;

            View = PlatformView.Create( );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.AddAsSubview( masterView );

            GroupTitle = PlatformLabel.Create( );
            GroupTitle.AddAsSubview( masterView );
            GroupTitle.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            GroupTitle.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GroupTitle.TextAlignment = TextAlignment.Center;

            LeaderImage = PlatformImageView.Create( true );
            LeaderImage.AddAsSubview( masterView );
            LeaderImage.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            LeaderImage.BackgroundColor = 0;

            // Group Desc
            GroupDescLayer = PlatformView.Create( );
            GroupDescLayer.AddAsSubview( masterView );
            GroupDescLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            GroupDescLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            GroupDescLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            GroupDesc = PlatformLabel.Create( );
            GroupDesc.AddAsSubview( masterView );
            GroupDesc.TextAlignment = TextAlignment.Center;
            GroupDesc.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            GroupDesc.TextColor = ControlStylingConfig.TextField_ActiveTextColor;

            // Group Desc Header
            GroupDescHeader = PlatformLabel.Create( );
            GroupDescHeader.AddAsSubview( masterView );
            GroupDescHeader.TextAlignment = TextAlignment.Center;
            GroupDescHeader.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
            GroupDescHeader.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GroupDescHeader.Text = ConnectStrings.GroupInfo_AboutGroup;


            // Leader Desc
            LeaderDescLayer = PlatformView.Create( );
            LeaderDescLayer.AddAsSubview( masterView );
            LeaderDescLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            LeaderDescLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            LeaderDescLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            LeaderDesc = PlatformLabel.Create( );
            LeaderDesc.AddAsSubview( masterView );
            LeaderDesc.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            LeaderDesc.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            LeaderDesc.TextAlignment = TextAlignment.Center;

            // Leader Desc Header
            LeaderDescHeader = PlatformLabel.Create( );
            LeaderDescHeader.AddAsSubview( masterView );
            LeaderDescHeader.TextAlignment = TextAlignment.Center;
            LeaderDescHeader.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
            LeaderDescHeader.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            LeaderDescHeader.Text = ConnectStrings.GroupInfo_AboutLeader;


            // Meeting Time
            MeetingTimeLayer = PlatformView.Create( );
            MeetingTimeLayer.AddAsSubview( masterView );
            MeetingTimeLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            MeetingTimeLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            MeetingTimeLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            MeetingTime = PlatformLabel.Create( );
            MeetingTime.AddAsSubview( masterView );
            MeetingTime.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            MeetingTime.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            MeetingTime.TextAlignment = TextAlignment.Center;


            // Childcare Desc
            ChildDescLayer = PlatformView.Create( );
            ChildDescLayer.AddAsSubview( masterView );
            ChildDescLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            ChildDescLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            ChildDescLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            ChildDesc = PlatformLabel.Create( );
            ChildDesc.AddAsSubview( masterView );
            ChildDesc.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            ChildDesc.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            ChildDesc.TextAlignment = TextAlignment.Center;

            // Child Desc Header
            ChildDescHeader = PlatformLabel.Create( );
            ChildDescHeader.AddAsSubview( masterView );
            ChildDescHeader.TextAlignment = TextAlignment.Center;
            ChildDescHeader.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Medium_FontSize );
            ChildDescHeader.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            ChildDescHeader.Text = ConnectStrings.GroupInfo_AboutChildcare;

            // Join Button
            JoinButton = PlatformButton.Create( );
            JoinButton.AddAsSubview( masterView );
            JoinButton.ClickEvent = JoinClicked;
            JoinButton.BackgroundColor = ControlStylingConfig.Button_BGColor;
            JoinButton.TextColor = ControlStylingConfig.Button_TextColor;
            JoinButton.CornerRadius = ControlStylingConfig.Button_CornerRadius;
            JoinButton.Text = ConnectStrings.JoinGroup_JoinButtonLabel;
            JoinButton.SizeToFit( );
            JoinButton.UserInteractionEnabled = true;

            // Create our results view overlay
            ResultView = new UIResultView( masterView, View.Frame, OnResultViewDone );

            // Create our blocker view
            BlockerView = new UIBlockerView( masterView, View.Frame );
        }

        public float GetControlBottom( )
        {
            return JoinButton.Frame.Bottom;
        }

        public void DisplayView( GroupFinder.GroupEntry groupEntry, MobileApp.MobileAppApi.OnGroupSummaryResult resultHandler )
        {
            // store the group and callback so that if it fails we can let them tap 'retry' and try again
            GroupEntry = groupEntry;
            GroupSummaryResult = resultHandler;

            Internal_DisplayView( );
        }

        void Internal_DisplayView( )
        {
            // default all controls to hidden, and we'll figure out what to show in the layout method.
            HideControls( true );

            IsDownloading = true;

            BlockerView.Show( delegate 
                {
                    MobileAppApi.GetGroupSummary( GroupEntry.Id, GroupEntry.GroupTypeId,
                        delegate( Rock.Client.Group resultGroup, System.IO.MemoryStream imageStream ) 
                        {
                            try
                            {
                                IsDownloading = false;

                                LeaderImage.Image = imageStream;
                                LeaderImage.SizeToFit( );

                                // if we had a valid image stream, dispose of it now
                                if( imageStream != null )
                                {
                                    LeaderImageValid = true;
                                    imageStream.Dispose( );
                                }
                                else
                                {
                                    LeaderImageValid = false;
                                }

                                // set the group title
                                GroupTitle.Text = GroupEntry.Title;

                                // set the details for the group (distance, meeting time, etc)
                                string currKey = "GroupDescription";
                                if( resultGroup.AttributeValues.ContainsKey( currKey ) )
                                {
                                    GroupDesc.Text = resultGroup.AttributeValues[ currKey ].Value;
                                }

                                currKey = "LeaderInformation";
                                if( resultGroup.AttributeValues.ContainsKey( currKey ) )
                                {
                                    LeaderDesc.Text = resultGroup.AttributeValues[ currKey ].Value;
                                }

                                currKey = "Children";
                                if( resultGroup.AttributeValues.ContainsKey( currKey ) )
                                {
                                    ChildDesc.Text = resultGroup.AttributeValues[ currKey ].Value;
                                }

                                if( string.IsNullOrEmpty( GroupEntry.MeetingTime ) == false )
                                {
                                    MeetingTime.Text = GroupEntry.MeetingTime;
                                }
                                else
                                {
                                    MeetingTime.Text = ConnectStrings.GroupFinder_ContactForTime;
                                }

                                // add the distance
                                MeetingTime.Text += "\n" + string.Format( "{0:##.0} {1}", GroupEntry.Distance, ConnectStrings.GroupFinder_MilesSuffix );

                                BlockerView.Hide( );

                                GroupSummaryResult( resultGroup, null );
                            }
                            catch
                            {
                                BlockerView.Hide( );

                                ResultView.Show( ConnectStrings.GroupInfo_Failed, 
                                                 PrivateControlStylingConfig.Result_Symbol_Failed, 
                                                 ConnectStrings.GroupInfoResult_Failed,
                                                 GeneralStrings.Retry );

                                GroupSummaryResult( null, null );
                            }
                        });
                });
        }

        void HideControls( bool hidden )
        {
            GroupTitle.Hidden = hidden;

            GroupDescHeader.Hidden = hidden;
            GroupDescLayer.Hidden = hidden;
            GroupDesc.Hidden = hidden;

            LeaderDescHeader.Hidden = hidden;
            LeaderDescLayer.Hidden = hidden;
            LeaderDesc.Hidden = hidden;

            MeetingTimeLayer.Hidden = hidden;
            MeetingTime.Hidden = hidden;

            ChildDescHeader.Hidden = hidden;
            ChildDescLayer.Hidden = hidden;
            ChildDesc.Hidden = hidden;

            JoinButton.Hidden = hidden;    
        }

        public void LayoutChanged( RectangleF containerBounds )
        {
            View.Frame = new RectangleF( containerBounds.Left, containerBounds.Top, containerBounds.Width, containerBounds.Height );

            BlockerView.SetBounds( containerBounds );
            ResultView.SetBounds( containerBounds );

            if( IsDownloading == false )
            {
                float sectionSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 25 );
                float textLeftInset = Rock.Mobile.Graphics.Util.UnitToPx( 10 );
                float textTopInset = Rock.Mobile.Graphics.Util.UnitToPx( 2 );
                float textRightInset = textLeftInset * 2;
                float textBotInset = textTopInset * 2;

                float buttonWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );
                float buttonHeight = Rock.Mobile.Graphics.Util.UnitToPx( 44 );

                GroupTitle.Hidden = false;
                GroupTitle.Frame = new RectangleF( textLeftInset, 0, View.Frame.Width - textRightInset, 0 );
                GroupTitle.SizeToFit( );
                GroupTitle.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GroupTitle.Bounds.Height );

                // layout the meeting itme
                MeetingTime.Hidden = false;
                MeetingTime.Frame = new RectangleF( textLeftInset, GroupTitle.Frame.Bottom, View.Frame.Width - textRightInset, 0 );
                MeetingTime.SizeToFit( );
                MeetingTime.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, MeetingTime.Bounds.Height );

                //MeetingTimeLayer.Hidden = false;
                //MeetingTimeLayer.Frame = new RectangleF( 0, GroupTitle.Frame.Bottom + sectionSpacing, View.Frame.Width, MeetingTime.Frame.Height + textBotInset );

                float nextYPos = MeetingTime.Frame.Bottom;

                // layout the leader's description header (IF there's a description or image)
                if ( string.IsNullOrEmpty( LeaderDesc.Text ) == false || LeaderImageValid == true )
                {
                    // display and position the header
                    LeaderDescHeader.Hidden = false;
                    LeaderDescHeader.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset + sectionSpacing, View.Frame.Width - textRightInset, 0 );
                    LeaderDescHeader.SizeToFit( );
                    LeaderDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, LeaderDescHeader.Bounds.Height );
                    nextYPos = LeaderDescHeader.Frame.Bottom;

                    // now try the image
                    if( LeaderImageValid == true )
                    {
                        LeaderImage.Hidden = false;
                        LeaderImage.Position = new PointF( (View.Frame.Width - LeaderImage.Frame.Width) / 2, nextYPos );
                        nextYPos = LeaderImage.Frame.Bottom + textBotInset;
                    }

                    // finally try to layout the leader's description next
                    if( string.IsNullOrEmpty( LeaderDesc.Text ) == false )
                    {
                        LeaderDesc.Hidden = false;
                        LeaderDesc.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
                        LeaderDesc.SizeToFit( );
                        LeaderDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, LeaderDesc.Bounds.Height );

                        LeaderDescLayer.Hidden = false;
                        LeaderDescLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, LeaderDesc.Frame.Height + textBotInset );

                        nextYPos = LeaderDescLayer.Frame.Bottom;
                    }
                }

                // layout the group description header
                if( string.IsNullOrEmpty( GroupDesc.Text ) == false )
                {
                    GroupDescHeader.Hidden = false;
                    GroupDescHeader.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset + sectionSpacing, View.Frame.Width - textRightInset, 0 );
                    GroupDescHeader.SizeToFit( );
                    GroupDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GroupDescHeader.Bounds.Height );

                    // layout the group description
                    GroupDesc.Hidden = false;
                    GroupDesc.Frame = new RectangleF( textLeftInset, GroupDescHeader.Frame.Bottom + textTopInset, View.Frame.Width - textRightInset, 0 );
                    GroupDesc.SizeToFit( );
                    GroupDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GroupDesc.Bounds.Height );

                    GroupDescLayer.Hidden = false;
                    GroupDescLayer.Frame = new RectangleF( 0, GroupDescHeader.Frame.Bottom, View.Frame.Width, GroupDesc.Frame.Height + textBotInset );

                    nextYPos = GroupDescLayer.Frame.Bottom;
                }

                // layout the child info header
                if( string.IsNullOrEmpty( ChildDesc.Text ) == false )
                {
                    ChildDescHeader.Hidden = false;
                    ChildDescHeader.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset + sectionSpacing, View.Frame.Width - textRightInset, 0 );
                    ChildDescHeader.SizeToFit( );
                    ChildDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, ChildDescHeader.Bounds.Height );

                    // layout child info
                    ChildDesc.Hidden = false;
                    ChildDesc.Frame = new RectangleF( textLeftInset, ChildDescHeader.Frame.Bottom + textTopInset, View.Frame.Width - textRightInset, 0 );
                    ChildDesc.SizeToFit( );
                    ChildDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, ChildDesc.Bounds.Height );

                    ChildDescLayer.Hidden = false;
                    ChildDescLayer.Frame = new RectangleF( 0, ChildDescHeader.Frame.Bottom, View.Frame.Width, ChildDesc.Frame.Height + textBotInset );

                    nextYPos = ChildDescLayer.Frame.Bottom;
                }


                // Join Button
                JoinButton.Hidden = false;
                JoinButton.Frame = new RectangleF( (View.Frame.Width - buttonWidth) / 2, nextYPos + sectionSpacing, buttonWidth, buttonHeight );
            }
        }

        void OnResultViewDone( )
        {
            //hack - Can't figure out WHY the join button isn't in the proper Z order on the Nexus 7.
            // but I just don't care right now. So hide it and unhide it.
            JoinButton.Hidden = false;

            ResultView.Hide( );

            Internal_DisplayView( );
        }

        void JoinClicked( PlatformButton button )
        {
            // forward to our parent
            OnButtonClicked( );
        }
    }
}
