using System;
using Rock.Mobile.UI;
using System.Drawing;
using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;
using Rock.Mobile.Animation;
using MobileApp.Shared.Network;
using Rock.Mobile.Util.Strings;
using MobileApp.Shared.PrivateConfig;
using MobileApp;
using System.Net;
using Rock.Mobile.Network;
using System.IO;

namespace MobileApp.Shared.UI
{
    public class UIGroupInfo
    {
        public PlatformView View { get; set; }

        MobileAppApi.GroupSearchResult GroupEntry { get; set; }

        PlatformLabel GroupTitle { get; set; }
        PlatformLabel MeetingTime { get; set; }
        PlatformLabel ChildcareProvided { get; set; }

        PlatformImageView FamilyImage { get; set; }
        PlatformView FamilyImageLayer { get; set; }
        bool FamilyImageValid { get; set; }

        PlatformLabel GroupDescHeader { get; set; }
        PlatformLabel GroupDesc { get; set; }
        PlatformView GroupDescLayer { get; set; }

		PlatformLabel ChildDescHeader { get; set; }
		PlatformLabel ChildDesc { get; set; }
		PlatformView ChildDescLayer { get; set; }

		PlatformImageView GroupImage { get; set; }
		PlatformView GroupImageLayer { get; set; }
		bool GroupImageValid { get; set; }

        PlatformButton JoinButton { get; set; }

        UIResultView ResultView { get; set; }

        UIBlockerView BlockerView { get; set; }

        bool IsDownloading { get; set; }

        HttpRequest.RequestResult GroupSummaryResult { get; set; }

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

            // Group Title
            GroupTitle = PlatformLabel.Create( );
            GroupTitle.AddAsSubview( masterView );
            GroupTitle.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            GroupTitle.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GroupTitle.TextAlignment = TextAlignment.Center;

			// Meeting Time
			MeetingTime = PlatformLabel.Create( );
			MeetingTime.AddAsSubview( masterView );
			MeetingTime.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
			MeetingTime.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
			MeetingTime.TextAlignment = TextAlignment.Center;

			// Childcare Provided
			ChildcareProvided = PlatformLabel.Create( );
			ChildcareProvided.AddAsSubview( masterView );
			ChildcareProvided.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
			ChildcareProvided.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
			ChildcareProvided.TextAlignment = TextAlignment.Center;



            // Family Image and Layer
            FamilyImageLayer = PlatformView.Create( );
            FamilyImageLayer.AddAsSubview( masterView );
            FamilyImageLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            FamilyImageLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            FamilyImageLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            FamilyImage = PlatformImageView.Create( );
            FamilyImage.AddAsSubview( masterView );
            FamilyImage.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
            FamilyImage.BackgroundColor = 0;


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


			// Group Image and Layer
			GroupImageLayer = PlatformView.Create( );
			GroupImageLayer.AddAsSubview( masterView );
			GroupImageLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
			GroupImageLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
			GroupImageLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

			GroupImage = PlatformImageView.Create( );
			GroupImage.AddAsSubview( masterView );
			GroupImage.ImageScaleType = PlatformImageView.ScaleType.ScaleAspectFit;
			GroupImage.BackgroundColor = 0;


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

        public void Destroy( )
        {
            // free image resources for Android's sake.
            FamilyImage.Destroy( );
            GroupImage.Destroy( );
        }

        public float GetControlBottom( )
        {
            return JoinButton.Frame.Bottom;
        }

        public void DisplayView( MobileAppApi.GroupSearchResult groupEntry, HttpRequest.RequestResult resultHandler )
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
                    MobileAppApi.GetGroupSummary( GroupEntry.Id,
                        delegate( MobileAppApi.GroupInfo groupInfo, System.IO.MemoryStream familyPhoto, System.IO.MemoryStream groupPhoto ) 
                        {
                            try
                            {
                                IsDownloading = false;

                                // setup the family image
                                FamilyImage.Image = familyPhoto;

                                float imageSize = Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.GroupInfo_Leader_ImageSize );
                                FamilyImage.Bounds = new RectangleF( 0, 0, imageSize, imageSize );

                                // if we had a valid image stream, dispose of it now
                                if( familyPhoto != null )
                                {
                                    FamilyImageValid = true;
                                    familyPhoto.Dispose( );
                                }
                                else
                                {
                                    FamilyImageValid = false;
                                }

                                // setup the group image
                                GroupImage.Image = groupPhoto;

                                imageSize = Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.GroupInfo_Group_ImageSize );
                                GroupImage.Bounds = new RectangleF( 0, 0, imageSize, imageSize );

                                // if we had a valid image stream, dispose of it now
                                if( groupPhoto != null )
                                {
                                    GroupImageValid = true;
                                    groupPhoto.Dispose( );
                                }
                                else
                                {
                                    GroupImageValid = false;
                                }

								// set the details for the group (distance, meeting time, etc)

								// set the group title
								GroupTitle.Text = GroupEntry.Name;

								if( string.IsNullOrWhiteSpace( GroupEntry.MeetingTime ) == false )
								{
									MeetingTime.Text = GroupEntry.MeetingTime;
								}
								else
								{
									MeetingTime.Text = ConnectStrings.GroupFinder_ContactForTime;
								}

								// add the distance
								MeetingTime.Text += "\n" + string.Format( "{0:##.0} {1}", GroupEntry.DistanceFromSource, ConnectStrings.GroupFinder_MilesSuffix );

                                // childcare provided header
                                if( string.IsNullOrWhiteSpace( groupInfo.Filters ) == false && groupInfo.Filters.Contains( PrivateConnectConfig.GroupFinder_Childcare_Filter ) )
                                {
                                    ChildcareProvided.Text = ConnectStrings.GroupFinder_OffersChildcare;
                                }
                                else
                                {
                                    ChildcareProvided.Text = string.Empty;
                                }
                                
                                // childcare description (if its blank, it'll be hidden)
                                ChildDesc.Text = groupInfo.ChildcareDesc;

                                // group description (if its blank, it'll be hidden)
                                GroupDesc.Text = groupInfo.Description;
                                

                                BlockerView.Hide( );

                                GroupSummaryResult( HttpStatusCode.OK, string.Empty );
                            }
                            catch
                            {
                                BlockerView.Hide( );

                                ResultView.Show( ConnectStrings.GroupInfo_Failed, 
                                                 PrivateControlStylingConfig.Result_Symbol_Failed, 
                                                 ConnectStrings.GroupInfoResult_Failed,
                                                 GeneralStrings.Retry );

                                GroupSummaryResult( HttpStatusCode.NotFound, string.Empty );
                            }
                        });
                });
        }

        void HideControls( bool hidden )
        {
            GroupTitle.Hidden = hidden;

            GroupImage.Hidden = hidden;
            GroupImageLayer.Hidden = hidden;

            FamilyImage.Hidden = hidden;
            FamilyImageLayer.Hidden = hidden;

            GroupDescHeader.Hidden = hidden;
            GroupDescLayer.Hidden = hidden;
            GroupDesc.Hidden = hidden;

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

                float nextYPos = MeetingTime.Frame.Bottom;

                // layout the childcare banner
                if( string.IsNullOrWhiteSpace( ChildcareProvided.Text ) == false )
                {
                    ChildcareProvided.Hidden = false;
                    ChildcareProvided.Frame = new RectangleF( textLeftInset, MeetingTime.Frame.Bottom, View.Frame.Width - textRightInset, 0 );
                    ChildcareProvided.SizeToFit( );
                    ChildcareProvided.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, ChildcareProvided.Bounds.Height );

                    nextYPos = ChildcareProvided.Frame.Bottom;
                }
                else
                {
                    ChildcareProvided.Hidden = true;
                }
                nextYPos += sectionSpacing;

                // layout the group description header (IF there's a description or group photo)
                if( string.IsNullOrWhiteSpace( GroupDesc.Text ) == false || FamilyImageValid == true )
                {
                    // display and position the header
                    GroupDescHeader.Hidden = false;
                    GroupDescHeader.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
                    GroupDescHeader.SizeToFit( );
                    GroupDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GroupDescHeader.Bounds.Height );
                    nextYPos = GroupDescHeader.Frame.Bottom;

					// now try the image
					if( FamilyImageValid == true )
					{
						// setup padding for the image
						float imageTopPadding = textTopInset * 2;
						float imageBotPadding = textBotInset * 2;
						float leaderImageLayerHeight = Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.GroupInfo_Leader_ImageSize );

						FamilyImage.Hidden = false;
						FamilyImage.Position = new PointF( ( View.Frame.Width - FamilyImage.Frame.Width ) / 2, nextYPos + imageTopPadding );

						FamilyImageLayer.Hidden = false;
						FamilyImageLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, leaderImageLayerHeight + imageBotPadding );

						nextYPos = FamilyImageLayer.Frame.Bottom + sectionSpacing;
					}

                    // try to layout the group description
                    if( string.IsNullOrWhiteSpace( GroupDesc.Text ) == false )
                    {
                        GroupDesc.Hidden = false;
                        GroupDesc.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
                        GroupDesc.SizeToFit( );
                        GroupDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GroupDesc.Bounds.Height );

                        GroupDescLayer.Hidden = false;
                        GroupDescLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, GroupDesc.Frame.Height + textBotInset );

                        nextYPos = GroupDescLayer.Frame.Bottom + textBotInset + sectionSpacing;
                    }

					// now try the image
					if( GroupImageValid == true )
					{
						// setup padding for the image
						float imageTopPadding = textTopInset * 2;
						float imageBotPadding = textBotInset * 2;
						float groupImageLayerHeight = Rock.Mobile.Graphics.Util.UnitToPx( PrivateConnectConfig.GroupInfo_Group_ImageSize );

						GroupImage.Hidden = false;
						GroupImage.Position = new PointF( ( View.Frame.Width - GroupImage.Frame.Width ) / 2, nextYPos + imageTopPadding );

						GroupImageLayer.Hidden = false;
						GroupImageLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, groupImageLayerHeight + imageBotPadding );

						nextYPos = GroupImageLayer.Frame.Bottom + sectionSpacing;
					}

                    // regardless of which (or both) of the above displayed, add an additional sectionSpacing
                    // so that the next major section, ChildDesc, has more spacing.
                    nextYPos += sectionSpacing;
                }

                // layout the child info header
                if( string.IsNullOrWhiteSpace( ChildDesc.Text ) == false )
                {
                    ChildDescHeader.Hidden = false;
                    ChildDescHeader.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
                    ChildDescHeader.SizeToFit( );
                    ChildDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, ChildDescHeader.Bounds.Height );
                    nextYPos = ChildDescHeader.Frame.Bottom;

                    // layout child info
                    ChildDesc.Hidden = false;
                    ChildDesc.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
                    ChildDesc.SizeToFit( );
                    ChildDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, ChildDesc.Bounds.Height );

                    ChildDescLayer.Hidden = false;
                    ChildDescLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, ChildDesc.Frame.Height + textBotInset );

                    nextYPos = ChildDescLayer.Frame.Bottom + sectionSpacing;
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
