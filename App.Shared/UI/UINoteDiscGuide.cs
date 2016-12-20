using System;
using Rock.Mobile.UI;
using System.Drawing;
using MobileApp.Shared.Config;
using MobileApp.Shared.PrivateConfig;

namespace MobileApp.Shared.UI
{
    /// <summary>
    /// Used to display a result to a user, including a status message
    /// </summary>
    public class UINoteDiscGuideView
    {
        PlatformView View { get; set; }

        PlatformLabel GuideDescHeader { get; set; }
        PlatformLabel GuideDesc { get; set; }
        PlatformView GuideDescLayer { get; set; }

        PlatformButton ViewGuideButton { get; set; }

        public delegate void DoneClickDelegate( );

        public UINoteDiscGuideView( object parentView, RectangleF frame, DoneClickDelegate onClick )
        {
            View = PlatformView.Create( );
            View.AddAsSubview( parentView );
            View.BackgroundColor = ControlStylingConfig.BackgroundColor;
            View.UserInteractionEnabled = false;

            // Guide Desc Header
            GuideDescHeader = PlatformLabel.Create( );
            GuideDescHeader.AddAsSubview( parentView );
            GuideDescHeader.TextAlignment = TextAlignment.Center;
            GuideDescHeader.SetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            GuideDescHeader.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GuideDescHeader.Text = Strings.MessagesStrings.DiscussionGuide_Header;

            // Guide Desc
            GuideDescLayer = PlatformView.Create( );
            GuideDescLayer.AddAsSubview( parentView );
            GuideDescLayer.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
            GuideDescLayer.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
            GuideDescLayer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

            GuideDesc = PlatformLabel.Create( );
            GuideDesc.AddAsSubview( parentView );
            GuideDesc.SetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Medium_FontSize );
            GuideDesc.TextColor = ControlStylingConfig.TextField_ActiveTextColor;
            GuideDesc.TextAlignment = TextAlignment.Center;
            GuideDesc.Text = Strings.MessagesStrings.DiscussionGuide_Desc;

            // View Guide Button
            ViewGuideButton = PlatformButton.Create( );
            ViewGuideButton.CornerRadius = 4;
            ViewGuideButton.BackgroundColor = ControlStylingConfig.Button_BGColor;
            ViewGuideButton.TextColor = ControlStylingConfig.Button_TextColor;
            ViewGuideButton.UserInteractionEnabled = true;
            ViewGuideButton.AddAsSubview( parentView );
            ViewGuideButton.ClickEvent = ( PlatformButton button ) =>
            {
                if( onClick != null )
                {
                    onClick( );
                }
            };
            ViewGuideButton.Text = "View Guide";

            SetBounds( frame );
        }

        public void SetBounds( RectangleF containerBounds )
        {
            float startingYPos = Rock.Mobile.Graphics.Util.UnitToPx( 125 );

            float sectionSpacing = Rock.Mobile.Graphics.Util.UnitToPx( 25 );
            float textLeftInset = Rock.Mobile.Graphics.Util.UnitToPx( 10 );
            float textTopInset = Rock.Mobile.Graphics.Util.UnitToPx( 2 );
            float textRightInset = textLeftInset * 2;
            float textBotInset = textTopInset * 2;

            float buttonWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );
            float buttonHeight = Rock.Mobile.Graphics.Util.UnitToPx( 44 );

            View.Bounds = containerBounds;

            // display and position the header
            GuideDescHeader.Hidden = false;
            GuideDescHeader.Frame = new RectangleF( textLeftInset, startingYPos, View.Frame.Width - textRightInset, 0 );
            GuideDescHeader.SizeToFit( );
            GuideDescHeader.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GuideDescHeader.Bounds.Height );
            float nextYPos = GuideDescHeader.Frame.Bottom;

            GuideDesc.Hidden = false;
            GuideDesc.Frame = new RectangleF( textLeftInset, nextYPos + textTopInset, View.Frame.Width - textRightInset, 0 );
            GuideDesc.SizeToFit( );
            GuideDesc.Bounds = new RectangleF( 0, 0, View.Frame.Width - textRightInset, GuideDesc.Bounds.Height );

            GuideDescLayer.Hidden = false;
            GuideDescLayer.Frame = new RectangleF( 0, nextYPos, View.Frame.Width, GuideDesc.Frame.Height + textBotInset );
            nextYPos = GuideDescLayer.Frame.Bottom + sectionSpacing;

            // lastly the button
            ViewGuideButton.Frame = new RectangleF( (View.Frame.Width - buttonWidth) / 2, nextYPos + sectionSpacing, buttonWidth, buttonHeight );
        }
    }
}
