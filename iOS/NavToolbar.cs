using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;
using App.Shared.Config;
using Rock.Mobile.UI;
using App.Shared.PrivateConfig;

namespace iOS
{
    /// <summary>
    /// A custom toolbar used for navigation within activities.
    /// Activities may change buttons according to their needs.
    /// </summary>
    public class NavToolbar : UIToolbar
    {
        /// <summary>
        /// The button used to go back a page in an task.
        /// </summary>
        /// <value>The back button.</value>
        UIButton BackButton { get; set; }
        EventHandler BackButtonHandler { get; set; }

        /// <summary>
        /// Button used when an task wishes to let the user share something
        /// </summary>
        /// <value>The share button.</value>
        UIButton ShareButton { get; set; }
        EventHandler ShareButtonHandler { get; set; }

        /// <summary>
        /// Button used when a task wishes to let the user create content (like a new prayer)
        /// </summary>
        /// <value>The create button.</value>
        UIButton CreateButton { get; set; }
        EventHandler CreateButtonHandler { get; set; }

        /// <summary>
        /// True when this toolbar is showing. False when it is hidden.
        /// </summary>
        /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
        bool Revealed { get; set; }

        /// <summary>
        /// True when the toolbar is in the process of sliding up or down.
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        bool Animating { get; set; }

        /// <summary>
        /// Timer monitoring the time the toolbar should be shown before auto-hiding.
        /// </summary>
        /// <value>The nav bar timer.</value>
        protected System.Timers.Timer NavBarTimer { get; set; }

        public NavToolbar( ) : base()
        {
            // create a timer that can be used to autohide this toolbar.
            NavBarTimer = new System.Timers.Timer();
            NavBarTimer.AutoReset = false;
            NavBarTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {
                    // when the timer fires, hide the toolbar.
                    // Although the timer fires on a seperate thread, because we queue the reveal
                    // on the main (UI) thread, we don't have to worry about race conditions.
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate { Reveal( false ); } );
                };

            uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );

            // create the back button
            NSString backLabel = new NSString(PrivateSubNavToolbarConfig.BackButton_Text);

            BackButton = new UIButton(UIButtonType.System);
            BackButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateSubNavToolbarConfig.BackButton_Size );
            BackButton.SetTitle( backLabel.ToString( ), UIControlState.Normal );
            BackButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            BackButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ), UIControlState.Disabled );

            CGSize buttonSize = backLabel.StringSize( BackButton.Font );
            BackButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );
            //BackButton.BackgroundColor = UIColor.White;

            // create the share button
            NSString shareLabel = new NSString(PrivateSubNavToolbarConfig.ShareButton_Text);

            ShareButton = new UIButton(UIButtonType.System);
            ShareButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateSubNavToolbarConfig.ShareButton_Size );
            ShareButton.SetTitle( shareLabel.ToString( ), UIControlState.Normal );
            ShareButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            ShareButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ), UIControlState.Disabled );

            // determine its dimensions
            buttonSize = shareLabel.StringSize( ShareButton.Font );
            ShareButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );
            //ShareButton.BackgroundColor = UIColor.White;


            // create the create button
            NSString createLabel = new NSString(PrivateSubNavToolbarConfig.CreateButton_Text);

            CreateButton = new UIButton(UIButtonType.System);
            CreateButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Secondary, PrivateSubNavToolbarConfig.CreateButton_Size );
            CreateButton.SetTitle( createLabel.ToString( ), UIControlState.Normal );
            CreateButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            CreateButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( disabledColor ), UIControlState.Disabled );

            // determine its dimensions
            buttonSize = createLabel.StringSize( CreateButton.Font );
            CreateButton.Bounds = new CGRect( 0, 0, buttonSize.Width, buttonSize.Height );
            //CreateButton.BackgroundColor = UIColor.White;

            UpdateButtons( );

            TintColor = UIColor.Clear;
            Translucent = false;
        }

        public void SetBackButtonAction( EventHandler handler )
        {
            if ( BackButtonHandler != null )
            {
                BackButton.TouchUpInside -= BackButtonHandler;
            }

            BackButton.TouchUpInside += handler;

            BackButtonHandler = handler;
        }

        public void SetBackButtonEnabled( bool enabled )
        {
            BackButton.Enabled = enabled;
        }

        public void SetShareButtonEnabled( bool enabled, EventHandler handler = null )
        {
            ShareButton.Enabled = enabled;

            if ( ShareButtonHandler != null )
            {
                ShareButton.TouchUpInside -= ShareButtonHandler; 
            }

            if( handler != null )
            {
                ShareButton.TouchUpInside += handler;
            }

            ShareButtonHandler = handler;
        }

        public void SetCreateButtonEnabled( bool enabled, EventHandler handler = null )
        {
            CreateButton.Enabled = enabled;

            if ( CreateButtonHandler != null )
            {
                CreateButton.TouchUpInside -= CreateButtonHandler; 
            }

            if( handler != null )
            {
                CreateButton.TouchUpInside += handler;
            }

            CreateButtonHandler = handler;
        }

        void UpdateButtons( )
        {
            // This sets the valid buttons TO the toolbar.
            // Since an task could request one, the other, or both,
            // we build a list and then add that list to the toolbar.
            List<UIBarButtonItem> itemList = new List<UIBarButtonItem>( );

            UIBarButtonItem spacer = new UIBarButtonItem( UIBarButtonSystemItem.FixedSpace );
            spacer.Width = PrivateSubNavToolbarConfig.iOS_ButtonSpacing;

            itemList.Add( new UIBarButtonItem( BackButton ) );
            itemList.Add( spacer );
            itemList.Add( new UIBarButtonItem( ShareButton ) );
            itemList.Add( spacer );
            itemList.Add( new UIBarButtonItem( CreateButton ) );

            // for some reason, it will not accept a new array of items
            // until we clear the existing.
            SetItems( new UIBarButtonItem[0], false );

            SetItems( itemList.ToArray( ), false );
        }

        public void RevealForTime( float timeToShow )
        {
            // stop (reset) any current timer
            NavBarTimer.Stop( );

            // convert to milliseconds
            NavBarTimer.Interval = timeToShow * 1000;

            // start the timer
            NavBarTimer.Start( );

            // reveal the toolbar, and when the timer ticks, it will be hidden again.
            InternalReveal( true );
        }

        public void Reveal( bool revealed )
        {
            // since they're calling reveal with no time, stop any pending timer.
            NavBarTimer.Stop( );

            InternalReveal( revealed );
        }

        bool ViewChangedDuringAnimation { get; set; }
        void InternalReveal( bool revealed )
        {
            // don't allow double requests of the same type
            if( Revealed != revealed )
            {
                Revealed = revealed;

                Animating = true;

                // Animate the front panel out
                UIView.Animate( PrivateSubNavToolbarConfig.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                    new Action( 
                        delegate 
                        { 
                            float deltaPosition = (float)  (revealed ? -Frame.Height : Frame.Height);

                            Layer.Position = new CGPoint( Layer.Position.X, Layer.Position.Y + deltaPosition);
                        })

                    , new Action(
                        delegate
                        {
                            Animating = false;

                            // if the view changed while we were animating, update 
                            // our position on completion
                            if ( ViewChangedDuringAnimation == true )
                            {
                                ViewChangedDuringAnimation = false;
                                ViewDidLayoutSubviews( );
                            }
                        })
                );
            }
        }

        public void ViewDidLayoutSubviews( )
        {
            CGRect parentFrame = Superview.Frame;

            // if we're not animating, we can easily update our position
            if ( Animating == false )
            {
                if ( Revealed == true )
                {
                    Frame = new CGRect( 0, parentFrame.Height - Frame.Height, parentFrame.Width, PrivateSubNavToolbarConfig.Height_iOS );
                }
                else
                {
                    Frame = new CGRect( 0, parentFrame.Height, parentFrame.Width, PrivateSubNavToolbarConfig.Height_iOS );
                }
            }
            else
            {
                // if we ARE, we need to have it correct itself when it finishes.
                ViewChangedDuringAnimation = true;
            }
        }
    }
}
    