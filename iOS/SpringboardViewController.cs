using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreAnimation;
using CoreGraphics;
using System.Collections.Generic;
using Rock.Mobile.Network;
using MobileApp.Shared.Network;
using AssetsLibrary;
using System.IO;
using MobileApp.Shared.Config;
using MobileApp.Shared.Strings;
using Rock.Mobile.UI;
using Rock.Mobile.PlatformSpecific.iOS.Graphics;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using MobileApp.Shared;
using Rock.Mobile.Animation;
using MobileApp.Shared.Analytics;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;
using MobileApp;
using Rock.Mobile.PlatformSpecific.Util;
using System.Linq;

namespace iOS
{
    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
	public class SpringboardViewController : UIViewController
	{
        /// <summary>
        /// Represents a selectable element on the springboard.
        /// Contains its button and the associated task.
        /// </summary>
        protected class SpringboardElement
        {
            /// <summary>
            /// The task that is launched by this element.
            /// </summary>
            /// <value>The task.</value>
            public Task Task { get; set; }

            /// <summary>
            /// The view that rests behind the button, graphic and text, and is colored when 
            /// the task is active. It is the parent for the button, text logo and seperator
            /// </summary>
            /// <value>The backing view.</value>
            UIView BackingView { get; set; }

            /// <summary>
            /// The button itself. Because we have special display needs, we
            /// break the button apart, and this ends up being an empty container that lies
            /// on top of the BackingView, LogoView and TextView.
            /// </summary>
            /// <value>The button.</value>
            UIButton Button { get; set; }

            UILabel TextLabel { get; set; }

            UILabel LogoView { get; set; }

            UIView Seperator { get; set; }

            public SpringboardElement( SpringboardViewController controller, Task task, UIView backingView, string imageChar, string labelStr )
            {
                Task = task;

                // setup the backing view
                BackingView = backingView;
                BackingView.BackgroundColor = UIColor.Clear;

                //The button should look as follows:
                // [ X Text ]
                // To make sure the icons and text are all aligned vertically,
                // we will actually create a backing view that can highlight (the []s)
                // and place a logo view (the X), and a text view (the Text) on top.
                // Finally, we'll make the button clear with no text and place it over the
                // backing view.

                // Create the logo view containing the image.
                LogoView = new UILabel();
                LogoView.Text = imageChar;
                LogoView.Font = FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Primary, PrivateSpringboardConfig.Element_FontSize );
                LogoView.SizeToFit( );
                LogoView.BackgroundColor = UIColor.Clear;
                BackingView.AddSubview( LogoView );

                // Create the text, and populate it with the button's requested text, color and font.
                TextLabel = new UILabel();
                TextLabel.Text = labelStr;
                TextLabel.Font = FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                TextLabel.BackgroundColor = UIColor.Clear;
                TextLabel.SizeToFit( );
                BackingView.AddSubview( TextLabel );

                // Create the seperator
                Seperator = new UIView( );
                Seperator.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                BackingView.AddSubview( Seperator );

                // Create the button
                Button = new UIButton( UIButtonType.Custom );
                Button.Layer.AnchorPoint = CGPoint.Empty;
                Button.BackgroundColor = UIColor.Clear;
                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        controller.ActivateElement( this );
                    };
                BackingView.AddSubview( Button );


                // position the controls
                Button.Bounds = BackingView.Bounds;

                LogoView.Layer.Position = new CGPoint( PrivateSpringboardConfig.Element_LogoOffsetX_iOS, BackingView.Frame.Height / 2 );

                TextLabel.Layer.Position = new CGPoint( PrivateSpringboardConfig.Element_LabelOffsetX_iOS + ( TextLabel.Frame.Width / 2 ), BackingView.Frame.Height / 2 );

                Seperator.Frame = new CGRect( 0, 0, Button.Frame.Width, 1.0f );

                Deactivate( );
            }

            public void Activate( )
            {
                LogoView.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementTextColor );
                TextLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementTextColor );
                BackingView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_Element_SelectedColor );
            }

            public void Deactivate( )
            {
                LogoView.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
                TextLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
                BackingView.BackgroundColor = UIColor.Clear;
            }
        };

        /// <summary>
        /// A list of all the elements on the springboard page.
        /// </summary>
        /// <value>The elements.</value>
        protected List<SpringboardElement> Elements { get; set; }

        /// <summary>
        /// The primary navigation for activities.
        /// </summary>
        /// <value>The nav view controller.</value>
        protected MainUINavigationController NavViewController { get; set; }

        /// <summary>
        /// Controller managing a user logging in or out
        /// </summary>
        /// <value>The login view controller.</value>
        LoginViewController LoginViewController { get; set; }

        /// <summary>
        /// Controller for managing user registration
        /// </summary>
        /// <value>The register view controller.</value>
        RegisterViewController RegisterViewController { get; set; }

        /// <summary>
        /// Controller managing the user's profile. Lets a user view or edit their profile.
        /// </summary>
        /// <value>The profile view controller.</value>
        ProfileViewController ProfileViewController { get; set; }

        /// <summary>
        /// Controller used for copping an image to our requirements (1:1 aspect ratio)
        /// </summary>
        /// <value>The image crop view controller.</value>
        ImageCropViewController ImageCropViewController { get; set; }

        /// <summary>
        /// The out of box experience view controller, used for first time setup.
        /// </summary>
        /// <value>The OOBE view controller.</value>
        OOBEViewController OOBEViewController { get; set; }

        SplashViewController SplashViewController { get; set; }

        /// <summary>
        /// True while the user is still being guided through the OOBE. This includes
        /// the view controllers the OOBE launches, like Login and Register
        /// </summary>
        /// <value><c>true</c> if this instance is OOBE running; otherwise, <c>false</c>.</value>
        bool IsOOBERunning { get; set; }

        /// <summary>
        /// When true, we are doing something else, like logging in, editing the profile, etc.
        /// </summary>
        /// <value><c>true</c> if modal controller visible; otherwise, <c>false</c>.</value>
        protected bool ModalControllerVisible { get; set; } 

        /// <summary>
        /// When true, we need to launch the image cropper. We have to wait
        /// until the NavBar and all sub-fragments have been pushed to the stack.
        /// </summary>
        UIImage ImageCropperPendingImage { get; set; }

        /// <summary>
        /// Stores the profile picture that is placed on the "Login Button" when the user is logged in.
        /// We use this because setting an image on a button via SetImage causes the button to size to the image,
        /// even with ContentMode set.
        /// </summary>
        /// <value>The profile image view.</value>
        UIImageView ProfileImageView { get; set; }

        /// <summary>
        /// A seperator that goes at the bottom of the Springboard Element List
        /// </summary>
        /// <value>The bottom seperator.</value>
        UIView BottomSeperator { get; set; }

        UILabel CampusSelectionText { get; set; }
        UILabel CampusSelectionIcon { get; set; }
        UIButton CampusSelectionButton { get; set; }

        NotificationBillboard Billboard { get; set; }

        /// <summary>
        /// True when the series info has been downloaded and it's safe to show the notification billboard.
        /// </summary>
        bool SeriesInfoDownloaded { get; set; }

		public SpringboardViewController ( ) : base()
		{
            NavViewController = new MainUINavigationController();
            NavViewController.ParentSpringboard = this;

            Elements = new List<SpringboardElement>( );
		}

        public override bool ShouldAutorotate()
        {
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone )
            {
                if ( NavViewController.SupportsLandscape( ) )
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // we have a choice here to support rotation on an iPhone 6+, but it just doesn't work well enough.
            // so limit rotation to tablets.
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone )
            {
                if ( NavViewController.SupportsLandscape( ) )
                {
                    return UIInterfaceOrientationMask.All;
                }
                else
                {
                    return UIInterfaceOrientationMask.Portrait;
                }
            }
            else
            {
                return UIInterfaceOrientationMask.All;
            }
        }

        static UITraitCollection CurrentTraitCollection { get; set; }
        static public CGSize TraitSize { get; protected set; }

        public override void WillTransitionToTraitCollection(UITraitCollection traitCollection, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.WillTransitionToTraitCollection(traitCollection, coordinator);

            CurrentTraitCollection = traitCollection;
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize( toSize, coordinator );

            TraitSize = toSize;

            if ( NavViewController != null )
            {
                NavViewController.LayoutChanging( );
                NavViewController.LayoutChanged( );
            }
        }

        public static bool SupportsLandscapeWide( )
        {
            // note: this is not my favorite way to do this, 
            // because we eliminate devices like the iPhone 6+. But,
            // it works ok since we don't want LandscapeWide on that device.
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone )
            {
                return true;
            }

            return false;
        }

        public static bool IsLandscapeWide( )
        {
            // we have a choice here to support rotation on an iPhone 6+, but it just doesn't work well enough.
            // so limit rotation to tablets.
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone )
            {
                if ( IsDeviceLandscape( ) && CurrentTraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDeviceLandscape( )
        {
            if ( TraitSize.Width > TraitSize.Height )
            {
                return true;
            }
            return false;
        }

        public static bool IsDevicePortrait( )
        {
            if ( TraitSize.Width < TraitSize.Height )
            {
                return true;
            }

            return false;
        }

        public void RevealButtonClicked( )
        {
            // this will be called by the Navbar (which owns the reveal button) when
            // it's clicked. We want to make sure we alwas hide the billboard.
            Billboard.Hide( );
        }

        UIView NewsElement { get; set; }
        UIView ConnectElement { get; set; }
        UIView MessagesElement { get; set; }
        UIView PrayerElement{ get; set; }
        UIView GiveElement { get; set; }
        UIView MoreElement { get; set; }

        UIButton EditPictureButton { get; set; }
        UILabel WelcomeField { get; set; }
        UILabel UserNameField { get; set; }

        UIButton ViewProfileButton { get; set; }
        UILabel ViewProfileLabel { get; set; }

		UIButton ViewToolboxButton { get; set; }
		UILabel ViewToolboxLabel { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            TraitSize = UIScreen.MainScreen.Bounds.Size;
            CurrentTraitCollection = TraitCollection;

            // if we're on an iphone and they're holding it landscape, force a portrait traitsize
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && IsDeviceLandscape( ) )
            {
                UITraitCollection horzTrait = UITraitCollection.FromHorizontalSizeClass( UIUserInterfaceSizeClass.Compact );
                UITraitCollection vertTrait = UITraitCollection.FromVerticalSizeClass( UIUserInterfaceSizeClass.Regular );
                CurrentTraitCollection = UITraitCollection.FromTraitsFromCollections( new UITraitCollection[] { horzTrait, vertTrait } );

                TraitSize = new CGSize( TraitSize.Height, TraitSize.Width );
            }

            View.Layer.AnchorPoint = CGPoint.Empty;
            View.Bounds = new CGRect( View.Bounds.Left, View.Bounds.Top, TraitSize.Width, TraitSize.Height );

            // load our objects from disk
            Rock.Mobile.Util.Debug.WriteLine( "Loading objects from device." );
            RockNetworkManager.Instance.LoadObjectsFromDevice( );
            Rock.Mobile.Util.Debug.WriteLine( "Loading objects done." );

            // create the login controller / profile view controllers
            LoginViewController = new LoginViewController( );
            LoginViewController.Springboard = this;

            ProfileViewController = new ProfileViewController( );
            ProfileViewController.Springboard = this;

            ImageCropViewController = new ImageCropViewController( );
            ImageCropViewController.Springboard = this;

            RegisterViewController = new RegisterViewController( );
            RegisterViewController.Springboard = this;

            OOBEViewController = new OOBEViewController( );
            OOBEViewController.Springboard = this;
            OOBEViewController.View.Layer.Position = CGPoint.Empty;

            SplashViewController = new SplashViewController( );
            SplashViewController.Springboard = this;
            SplashViewController.View.Layer.Position = CGPoint.Empty;


            ScrollView = new UIScrollViewWrapper( );
            ScrollView.Layer.AnchorPoint = CGPoint.Empty;
            ScrollView.Parent = this;
            View.AddSubview( ScrollView );

            // Instantiate all activities
            float elementWidth = 250;
            float elementHeight = 45;

            NewsElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( NewsElement );

            MessagesElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( MessagesElement );

            GiveElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( GiveElement );

            ConnectElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( ConnectElement );

            PrayerElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( PrayerElement );

            MoreElement = new UIView( new CGRect( 0, 0, elementWidth, elementHeight ) );
            ScrollView.AddSubview( MoreElement );

            EditPictureButton = new UIButton( new CGRect( 0, 0, 112, 112 )  );
            ScrollView.AddSubview( EditPictureButton );

            WelcomeField = new UILabel();
            ScrollView.AddSubview( WelcomeField );

            UserNameField = new UILabel();
            ScrollView.AddSubview( UserNameField );

            ViewProfileButton = new UIButton();
            ScrollView.AddSubview( ViewProfileButton );

            ViewProfileLabel = new UILabel();
            ScrollView.AddSubview( ViewProfileLabel );


			ViewToolboxButton = new UIButton( );
			ScrollView.AddSubview( ViewToolboxButton );

			ViewToolboxLabel = new UILabel( );
			ScrollView.AddSubview( ViewToolboxLabel );
            ViewToolboxLabel.Text = SpringboardStrings.AccessToolbox;
        

            Elements.Add( new SpringboardElement( this, new NewsTask( "NewsStoryboard_iPhone" )      , NewsElement    , SpringboardConfig.Element_News_Icon    , SpringboardStrings.Element_News_Title ) );
            Elements.Add( new SpringboardElement( this, new NotesTask( "" )                          , MessagesElement, SpringboardConfig.Element_Messages_Icon, SpringboardStrings.Element_Messages_Title ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )      , GiveElement    , SpringboardConfig.Element_Give_Icon    , SpringboardStrings.Element_Give_Title ) );
            Elements.Add( new SpringboardElement( this, new ConnectTask( "ConnectStoryboard_iPhone" ), ConnectElement , SpringboardConfig.Element_Connect_Icon , SpringboardStrings.Element_Connect_Title ) );
            Elements.Add( new SpringboardElement( this, new PrayerTask( "" )                         , PrayerElement  , SpringboardConfig.Element_Prayer_Icon  , SpringboardStrings.Element_Prayer_Title ) );
            Elements.Add( new SpringboardElement( this, new AboutTask( "" )                          , MoreElement    , SpringboardConfig.Element_More_Icon    , SpringboardStrings.Element_More_Title ) );

            // add a bottom seperator for the final element
            BottomSeperator = new UIView();
            BottomSeperator.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            ScrollView.AddSubview( BottomSeperator );
            BottomSeperator.Frame = new CGRect( 0, 0, View.Frame.Width, 1.0f );


            // set the profile image mask so it's circular
            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new CGPoint( 0, 0 );
            maskLayer.Bounds = EditPictureButton.Layer.Bounds;
            maskLayer.CornerRadius = EditPictureButton.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            EditPictureButton.Layer.Mask = maskLayer;
            //

            // setup the campus selector and settings button
            CampusSelectionText = new UILabel();
            ControlStyling.StyleUILabel( CampusSelectionText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            CampusSelectionText.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
            CampusSelectionText.LineBreakMode = UILineBreakMode.TailTruncation;
            ScrollView.AddSubview( CampusSelectionText );

            CampusSelectionIcon = new UILabel();
            ControlStyling.StyleUILabel( CampusSelectionIcon, PrivateControlStylingConfig.Icon_Font_Primary, ControlStylingConfig.Small_FontSize );
            CampusSelectionIcon.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
            CampusSelectionIcon.Text = PrivateSpringboardConfig.CampusSelectSymbol;
            CampusSelectionIcon.SizeToFit( );
            ScrollView.AddSubview( CampusSelectionIcon );

            CampusSelectionButton = new UIButton();
            ScrollView.AddSubview( CampusSelectionButton );
            CampusSelectionButton.TouchUpInside += SelectCampus;


            // setup the image that will display when the user is logged in
            ProfileImageView = new UIImageView( );
            ProfileImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            ProfileImageView.Layer.AnchorPoint = CGPoint.Empty;
            ProfileImageView.Bounds = EditPictureButton.Bounds;
            ProfileImageView.Layer.Position = CGPoint.Empty;
            EditPictureButton.AddSubview( ProfileImageView );

            EditPictureButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( PrivateControlStylingConfig.Icon_Font_Primary, PrivateSpringboardConfig.ProfileSymbolFontSize );
            EditPictureButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ), UIControlState.Normal );
            EditPictureButton.Layer.BorderColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ).CGColor;
            EditPictureButton.Layer.CornerRadius = EditPictureButton.Bounds.Width / 2;
            EditPictureButton.Layer.BorderWidth = 4;

            WelcomeField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Large_FontSize );
            WelcomeField.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );

            UserNameField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Bold, ControlStylingConfig.Large_FontSize );
            UserNameField.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );

            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_BackgroundColor );

            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );

            EditPictureButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow launching a model view controller unless the springboard is open.
                    if ( NavViewController.IsSpringboardOpen( ) )
                    {
                        if( RockMobileUser.Instance.LoggedIn == true )
                        {
                            // they're logged in, so let them set their profile pic
                            ManageProfilePic( );
                        }
                        else
                        {
                            //otherwise this button can double as a login button.
                            PresentModalViewController( LoginViewController );
                        }
                    }
                };
            
            ViewProfileButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow launching a model view controller unless the springboard is open.
                    if ( NavViewController.IsSpringboardOpen( ) )
                    {
                        if( RockMobileUser.Instance.LoggedIn == true )
                        {
                            // Because we aren't syncing RIGHT HERE, Rock data could technically be overwritten.
                            // If WHILE they're running the app, their data is updated in Rock, those changes will
                            // be lost when they submit their profile changes.
                            // But, the odds that Rock data will update WHILE THE APP IS RUNNING, and they then decide to
                            // update their profile without having even backgrounded the app, are extremely low.
                            PresentModalViewController( ProfileViewController );
                        }
                        else
                        {
                            PresentModalViewController( LoginViewController );
                        }
                    }
                };

            // Setup coach toolbox access
            ViewToolboxButton.TouchUpInside += ( object sender, EventArgs e ) =>
                {
                    HandleAppURL( PrivateGeneralConfig.App_URL_Scheme +
                                      PrivateGeneralConfig.App_URL_Commands_Goto + "/" +
                                      PrivateGeneralConfig.App_URL_Task_Connect + "/" +
                                      PrivateGeneralConfig.App_URL_Page_Toolbox );
                };

            // set the viewing campus now that their profile has loaded (if they have already done the OOBE)
            CampusSelectionText.Text = string.Format( SpringboardStrings.Viewing_Campus, RockLaunchData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) ).ToUpper( );

            // setup the Notification Banner for Taking Notes
            Billboard = new NotificationBillboard( View.Bounds.Width, View.Bounds.Height );
            Billboard.SetLabel( SpringboardStrings.TakeNotesNotificationIcon, 
                                PrivateControlStylingConfig.Icon_Font_Primary,
                                ControlStylingConfig.Small_FontSize,
                                SpringboardStrings.TakeNotesNotificationLabel, 
                                ControlStylingConfig.Font_Light,
                                ControlStylingConfig.Small_FontSize,
                                ControlStylingConfig.TextField_ActiveTextColor, 
                                ControlStylingConfig.Springboard_Element_SelectedColor, 
                delegate 
                {
                    // tell the app to launch notes
                    HandleAppURL( PrivateGeneralConfig.App_URL_Scheme +
                                  PrivateGeneralConfig.App_URL_Commands_Goto + "/" + 
                                  PrivateGeneralConfig.App_URL_Task_Notes + "/" + 
                                  PrivateGeneralConfig.App_URL_Page_Read );
                    return true;
                } 
            );

            Billboard.Layer.Position = new CGPoint( Billboard.Layer.Position.X, NavViewController.NavigationBar.Frame.Height );

            // only do the OOBE if the user hasn't seen it yet
            if ( RockMobileUser.Instance.OOBEComplete == false )
                //if( RanOOBE == false )
            {
                // sanity check for testers that didn't listen to me and delete / reinstall.
                // This will force them to be logged out so they experience the OOBE properly.
                RockMobileUser.Instance.LogoutAndUnbind( );

                //RanOOBE = true;
                IsOOBERunning = true;
                AddChildViewController( OOBEViewController );
                View.AddSubview( OOBEViewController.View );

                // before we do anything else, force a rock sync. Then we can trust we have good solid launch data.
                RockNetworkManager.Instance.SyncRockData( null, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // for the OOBE we very much care if syncing worked, and can't let them continue if it didn't.
                    bool success = Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true;
                    OOBEViewController.PerformStartup( success );
                });
            }
            else
            {
                // prepare the splash screen animation
                AddChildViewController( SplashViewController );
                View.AddSubview( SplashViewController.View );
                SplashViewController.PerformStartup( );

                // now let the normal activation go
                OnActivated_SyncRockData( );
            }
        }
        //static bool RanOOBE { get; set; }

        public void SelectCampus(object sender, EventArgs e )
        {
            UIAlertController actionSheet = UIAlertController.Create( SpringboardStrings.SelectCampus_SourceTitle, 
                SpringboardStrings.SelectCampus_SourceDescription, 
                UIAlertControllerStyle.ActionSheet );

            // if the device is a tablet, anchor the menu
            if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad )
            {
                actionSheet.PopoverPresentationController.SourceView = CampusSelectionButton;
                actionSheet.PopoverPresentationController.SourceRect = CampusSelectionButton.Bounds;
            }

            // for each campus, create an entry in the action sheet, and its callback will assign
            // that campus index to the user's viewing preference
            for( int i = 0; i < RockLaunchData.Instance.Data.Campuses.Count; i++ )
            {
                UIAlertAction campusAction = UIAlertAction.Create( RockLaunchData.Instance.Data.Campuses[ i ].Name, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                    {
                        //get the index of the campus based on the selection's title, and then set that campus title as the string
                        RockMobileUser.Instance.ViewingCampus = RockLaunchData.Instance.Data.CampusNameToId( obj.Title );

                        RefreshCampusSelection( );
                    } );

                actionSheet.AddAction( campusAction );
            }

            // let them cancel, too
            UIAlertAction cancelAction = UIAlertAction.Create( GeneralStrings.Cancel, UIAlertActionStyle.Cancel, delegate { });
            actionSheet.AddAction( cancelAction );

            PresentViewController( actionSheet, true, null );
        }

        public void SplashComplete( )
        {
            SimpleAnimator_Float splashFadeOutAnim = new SimpleAnimator_Float( 1.00f, 0.00f, .33f, delegate(float percent, object value )
                {
                    SplashViewController.View.Layer.Opacity = (float)value;
                },
                delegate
                {
                    // remove the splash screen
                    SplashViewController.RemoveFromParentViewController( );
                    SplashViewController.View.RemoveFromSuperview( );
                } );
            splashFadeOutAnim.Start( );
        }

        //static bool RanOOBE = false;
        public void OOBEOnClick( int index, bool isCampusSelection )
        {
            // -1 means retry because the network connection failed, so we didn't get launch data
            if( -1 == index )
            {
                // before we do anything else, force a rock sync. Then we can trust we have good solid launch data.
                RockNetworkManager.Instance.SyncRockData( null, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                    {
                        OOBEViewController.HandleNetworkFixed( );
                    }
                });
            }
            else if ( isCampusSelection )
            {
                MobileApp.Shared.Network.RockMobileUser.Instance.ViewingCampus = index;
            }
            else
            {
                // fade out the OOBE
                SimpleAnimator_Float oobeFadeOutAnim = new SimpleAnimator_Float( 1.00f, 0.00f, .33f, delegate(float percent, object value )
                    {
                        OOBEViewController.View.Layer.Opacity = (float)value;
                    },
                                                           delegate
                    {
                        // if they chose register, present it
                        if ( index == 0 )
                        {
                            PresentModalViewController( RegisterViewController );
                        }
                        // if they chose login, present it!
                        else if ( index == 1 )
                        {
                            PresentModalViewController( LoginViewController );
                        }
                        else
                        {
                            // don't present anything. Instead, just wrap up the OOBE.
                            CompleteOOBE( );
                        }

                        OOBEViewController.RemoveFromParentViewController( );
                        OOBEViewController.View.RemoveFromSuperview( );
                    } );
                oobeFadeOutAnim.Start( );
            }
        }

        void CompleteOOBE( )
        {
            // kick off a timer to allow the user to see the news before revealing the springboard.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 500;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            IsOOBERunning = false;
                            RockMobileUser.Instance.OOBEComplete = true;

                            // force a final refresh so we see the login info change (if it did)
                            ViewDidLayoutSubviews( );

                            // if the series billboard will NOT show up,
                            if( TryDisplaySeriesBillboard( ) == false )
                            {
                                // reveal the springboard
                                NavViewController.RevealSpringboard( true );
                            }

                            // NOW go ahead and start downloads by forcing a campus selection (since they just picked it in the OOBE)
                            RefreshCampusSelection( true );
                        } );
                };
            timer.Start( );
        }

        void OnActivated_SyncRockData( )
        {
            SeriesInfoDownloaded = false;

            RockNetworkManager.Instance.SyncRockData( 
                // first delegate is for completion of the series download. At that point we can show the notification billboard.
                delegate 
                {
                    SeriesInfoDownloaded = true;

                    TryDisplaySeriesBillboard( );
                },
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    //If the OOBE isn't running
                    if( IsOOBERunning == false )
                    {
                        // Allow the news to update, and begin downloading all
                        // news and note images we need.
                        PerformTaskAction( PrivateGeneralConfig.App_URL_Commands_Execute, new string[] { PrivateGeneralConfig.App_URL_Task_News, PrivateGeneralConfig.App_URL_Execute_ReloadNews } );

                        View.SetNeedsLayout( );
                    }
                });
        }

        public static bool IsAppURL( string url )
        {
            // make sure the host is for the app. If not, it's not an App Url
            if( url.Substring( 0, PrivateGeneralConfig.App_URL_Scheme.Length ) == PrivateGeneralConfig.App_URL_Scheme )
            {
                return true;
            }

            return false;
        }

        public void HandleAppURL( string url )
        {
            if( IsAppURL( url ) )
            {
                // skip past the scheme
                url = url.Substring( PrivateGeneralConfig.App_URL_Scheme.Length );

                // GOTO COMMAND
                if( url.StartsWith( PrivateGeneralConfig.App_URL_Commands_Goto, StringComparison.InvariantCultureIgnoreCase ) == true )
                {
                    // strip the command off
                    url = url.Substring( PrivateGeneralConfig.App_URL_Commands_Goto.Length + 1 );

                    string[] arguments = url.Split( '/' );

                    foreach( SpringboardElement element in Elements )
                    {
                        if( element.Task.Command_Keyword( ) == arguments[ 0 ] )
                        {
                            ActivateElement( element, true );
                        }
                    }

                    // provide the full argument URL so that only the task that cares about this will use it.
                    // (it's possible two tasks might have a "read" argument, so by sending "messages/read" all other tasks will know to ignore it) 
                    PerformTaskAction( PrivateGeneralConfig.App_URL_Commands_Goto, arguments );
                }
                // EXECUTE COMMAND
                else if ( url.StartsWith( PrivateGeneralConfig.App_URL_Commands_Execute, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    // strip the command off
                    url = url.Substring( PrivateGeneralConfig.App_URL_Commands_Execute.Length + 1 );

                    string[] arguments = url.Split( '/' );

                    // and handle it
                    PerformTaskAction( PrivateGeneralConfig.App_URL_Commands_Execute, arguments );
                }
            }
        }

        void PerformTaskAction( string command, string[] arguments )
        {
            // notify all elements
            foreach ( SpringboardElement element in Elements )
            {
                element.Task.PerformAction( command, arguments );
            }
        }

        void ManageProfilePic( )
        {
            UIAlertController actionSheet = UIAlertController.Create( SpringboardStrings.ProfilePicture_SourceTitle, 
                                                                      SpringboardStrings.ProfilePicture_SourceDescription, 
                                                                      UIAlertControllerStyle.ActionSheet );

            // if the device is a tablet, anchor the menu
            if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad )
            {
                actionSheet.PopoverPresentationController.SourceView = EditPictureButton;
                actionSheet.PopoverPresentationController.SourceRect = EditPictureButton.Bounds;
            }

            // setup the camera
            UIAlertAction cameraAction = UIAlertAction.Create( SpringboardStrings.ProfilePicture_SourceCamera, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                {
                    // only allow the camera if they HAVE one
                    if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                    {
                        ModalControllerVisible = true;

                        // launch the camera
                        string jpgFilename = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), "cameraTemp.jpg" );
                        Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( jpgFilename, this, delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                            {
                                ModalControllerVisible = false;

                                // if the result is true, they either got a picture or pressed cancel
                                bool success = false;
                                if( args.Result == true )
                                {
                                    // either way, no need for an error
                                    success = true;

                                    // if the image path is valid, they didn't cancel
                                    if ( string.IsNullOrEmpty( args.ImagePath ) == false )
                                    {
                                        // load the image for cropping
                                        ImageCropperPendingImage = UIImage.FromFile( args.ImagePath );
                                    }
                                }

                                if( success == false )
                                {
                                    DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                                }
                            });
                    }
                    else
                    {
                        // notify them they don't have a camera
                        DisplayError( SpringboardStrings.Camera_Error_Title, SpringboardStrings.Camera_Error_Message );
                    }
                } );

            // setup the photo library
            UIAlertAction photoLibraryAction = UIAlertAction.Create( SpringboardStrings.ProfilePicture_SourcePhotoLibrary, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                {
                    ModalControllerVisible = true;

                    Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( this, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
                        {
                            ModalControllerVisible = false;

                            if( args.Result == true )
                            {
                                ImageCropperPendingImage = (UIImage) args.Image;
                            }
                            else
                            {
                                DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                            }
                        } );
                } );

            //setup cancel
            UIAlertAction cancelAction = UIAlertAction.Create( GeneralStrings.Cancel, UIAlertActionStyle.Cancel, delegate{ } );

            actionSheet.AddAction( cameraAction );
            actionSheet.AddAction( photoLibraryAction );
            actionSheet.AddAction( cancelAction );
            PresentViewController( actionSheet, true, null );
        }

        void PresentModalViewController( UIViewController modelViewController )
        {
            PresentViewController( modelViewController, true, null );
            ModalControllerVisible = true;
        }

        public void ResignModelViewController( UIViewController modelViewController, object context )
        {
            // if the image cropper is resigning
            if ( modelViewController == ImageCropViewController )
            {
                // if croppedImage is null, they simply cancelled
                UIImage croppedImage = (UIImage)context;
                if ( croppedImage != null )
                {
                    NSData croppedImageData = croppedImage.AsJPEG( );

                    // if the image converts, we're good.
                    if ( croppedImageData != null )
                    {
                        MemoryStream memStream = new MemoryStream();

                        Stream nsDataStream = croppedImageData.AsStream( );

                        nsDataStream.CopyTo( memStream );
                        memStream.Position = 0;

                        RockMobileUser.Instance.SaveProfilePicture( memStream );
                        RockMobileUser.Instance.UploadSavedProfilePicture( null ); // we don't care about the response. just do it.

                        nsDataStream.Dispose( );
                    }
                    else
                    {
                        // notify them about a problem saving the profile picture
                        DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                    }
                }
            }
            else if ( modelViewController == ProfileViewController )
            {
                // make sure we update the UI if they just finished editing their profile
                View.SetNeedsLayout( );
            }

            modelViewController.DismissViewController( true, delegate
                    {
                        // if this resign is while the OOBE is running, it was the register or login finishing up, 
                        // so wrap up the OOBE
                        if ( IsOOBERunning == true )
                        {
                            CompleteOOBE( );
                        }
                        ModalControllerVisible = false;
                        View.SetNeedsLayout( );
                    } );
        }

        public void RegisterNewUser( )
        {
            LoginViewController.PresentViewController( RegisterViewController, true, null );
        }

        public override bool PrefersStatusBarHidden()
        {
            // don't show the status bar when running this app.
            return true;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // only needed when we were showing the status bar. Causes
            // the status bar text to be white.
            return UIStatusBarStyle.LightContent;
        }

        protected void ActivateElement( SpringboardElement activeElement, bool forceActivate = false )
        {
            // total hack - If they tap Give, we'll kick them out to the give URL, leaving the app
            // in this state.
            if ( activeElement.Task as GiveTask != null )
            {
                // trigger the Give analytic
                GiveAnalytic.Instance.Trigger( GiveAnalytic.Give );

                TaskWebViewController.HandleUrl( true, true, GiveConfig.GiveUrl, null, null, false, false, false );
            }
            else
            {
                // don't allow any navigation while the login controller is active.
                // If forceOpen is enabled, we'll allow it regardless.
                if ( ( ModalControllerVisible == false && NavViewController.IsSpringboardOpen( ) == true ) || forceActivate == true )
                {
                    // make sure we're allowed to switch activities
                    if ( NavViewController.ActivateTask( activeElement.Task ) == true )
                    {
                        // first turn "off" the backingView selection for all but the element
                        // becoming active.
                        foreach ( SpringboardElement element in Elements )
                        {
                            if ( element != activeElement )
                            {
                                element.Deactivate( );
                            }
                        }

                        // activate the element and its associated task
                        activeElement.Activate( );
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // don't allow any navigation while the login controller is active
            if( ModalControllerVisible == false && IsLandscapeWide( ) == false )
            {
                NavViewController.RevealSpringboard( false );
            }
        }

        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            // refresh the viewing campus
            RefreshCampusSelection( );
        }

        void RefreshCampusSelection( bool forceRefresh = false )
        {
            string newCampusText = string.Format( SpringboardStrings.Viewing_Campus, 
                RockLaunchData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) ).ToUpper( );

            if ( CampusSelectionText.Text != newCampusText || forceRefresh == true )
            {
                CampusSelectionText.Text = newCampusText;

                UpdateCampusViews( );

                // notify the system the campus changed
                PerformTaskAction( PrivateGeneralConfig.App_URL_Commands_Execute, new string[] { PrivateGeneralConfig.App_URL_Task_News, PrivateGeneralConfig.App_URL_Execute_CampusChanged } );
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // if the image cropper is pending, launch it now.
            if( ImageCropperPendingImage != null )
            {
                ImageCropViewController.Begin( ImageCropperPendingImage, 1.0f );
                PresentModalViewController( ImageCropViewController );

                ImageCropperPendingImage = null;
            }
            else
            {
                // if we're appearing and no task is active, start one.
                // (this will only happen when the app is first launched)
                if( NavViewController.CurrentTask == null )
                {
                    // don't use the ActivateElement method because
                    // it verifies the springboard is closed, and we don't
                    // care on first run.
                    NavViewController.ActivateTask( Elements[ 0 ].Task );
                    Elements[0].Activate( );
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            View.Bounds = new CGRect( View.Bounds.Left, View.Bounds.Top, TraitSize.Width, TraitSize.Height );

            ScrollView.Frame = View.Frame;

            // if the OOBE isn't running, do everything normal
            if ( IsOOBERunning == false )
            {
                UpdateLoginState( );

                AdjustSpringboardLayout( );

                // add the billboard now that we're ready
                if ( Billboard != null && Billboard.Superview == null )
                {
                    View.AddSubview( Billboard );

                    TryDisplaySeriesBillboard( );
                }
            }
            // if the OOBE is running, only do this!
            else
            {
                AdjustSpringboardLayout( );
            }
        }

        /// <summary>
        /// Displays the "Tap to take notes" series billboard
        /// </summary>
        bool TryDisplaySeriesBillboard( )
        {
            // first make sure all initial setup is done.
            if ( SeriesInfoDownloaded == true && IsOOBERunning == false && Billboard != null && Billboard.Superview != null )
            {
                // should we advertise the notes?

                // make sure we're not already showing the notes.
                NotesTask noteTask = NavViewController.CurrentTask as NotesTask;
                if( noteTask == null || noteTask.IsReading( ) == false )
                {
                    // yes, if it's a weekend
                    if ( DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday )
                    {
                        if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 && RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ] != null )
                        {
                            // lastly, ensure there's a valid note for the message
                            if ( string.IsNullOrEmpty( RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl ) == false )
                            {
                                // kick off a timer to reveal the billboard, because we 
                                // don't want to do it the MOMENT the view appears.
                                System.Timers.Timer timer = new System.Timers.Timer();
                                timer.AutoReset = false;
                                timer.Interval = 1;
                                timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                                {
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( 
                                        delegate
                                        {
                                            Billboard.Reveal( );
                                        } );
                                };
                                timer.Start( );

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adjusts the positioning of the springboard elements to be spaced out consistently
        /// across ios devices
        /// </summary>
        void AdjustSpringboardLayout( )
        {
            nfloat availableHeight = View.Frame.Height;
            if ( SpringboardViewController.IsLandscapeWide( ) == true )
            {
                availableHeight = View.Frame.Height / 2;
            }

            EditPictureButton.Layer.AnchorPoint = CGPoint.Empty;
            EditPictureButton.Layer.Position = new CGPoint( ( PrivatePrimaryContainerConfig.SlideAmount_iOS - EditPictureButton.Bounds.Width ) / 2, availableHeight * .02f );


            // center the welcome and name labels within the available Springboard width
            float totalNameWidth = (float) (WelcomeField.Bounds.Width + UserNameField.Bounds.Width);
            totalNameWidth = Math.Min( totalNameWidth, PrivatePrimaryContainerConfig.SlideAmount_iOS - 10 );

            float totalNameHeight = Math.Max( (float) WelcomeField.Bounds.Height, (float) UserNameField.Bounds.Height );

            WelcomeField.Layer.AnchorPoint = CGPoint.Empty;
            WelcomeField.Layer.Position = new CGPoint( ( PrivatePrimaryContainerConfig.SlideAmount_iOS - totalNameWidth ) / 2, EditPictureButton.Frame.Bottom + 10 );
            WelcomeField.Bounds = new CGRect( 0, 0, WelcomeField.Bounds.Width, totalNameHeight );


            nfloat availNameWidth = totalNameWidth - WelcomeField.Bounds.Width + 5;
            UserNameField.Layer.AnchorPoint = CGPoint.Empty;
            UserNameField.Layer.Position = new CGPoint( WelcomeField.Frame.Right, WelcomeField.Frame.Y );
            UserNameField.Bounds = new CGRect( 0, 0, availNameWidth, totalNameHeight );
            UserNameField.LineBreakMode = UILineBreakMode.TailTruncation;
            UserNameField.AdjustsFontSizeToFitWidth = false;

            ViewProfileLabel.Layer.AnchorPoint = CGPoint.Empty;
            ViewProfileLabel.Font = FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
            ViewProfileLabel.SizeToFit( );
            ViewProfileLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
            ViewProfileLabel.Layer.Position = new CGPoint( EditPictureButton.Layer.Position.X + ((EditPictureButton.Bounds.Width - ViewProfileLabel.Bounds.Width) / 2), WelcomeField.Frame.Bottom );

            float totalHeight = (float) (totalNameHeight + ViewProfileLabel.Bounds.Height);

            // wrap the view profile button around the entire "Welcome: Name" phrase
            ViewProfileButton.SetTitle( "", UIControlState.Normal );
            ViewProfileButton.Layer.AnchorPoint = CGPoint.Empty;
            ViewProfileButton.Layer.Position = new CGPoint( 0, WelcomeField.Frame.Y );
            ViewProfileButton.Bounds = new CGRect( 0, 0, View.Frame.Width, totalHeight );
            //ViewProfileButton.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( 0xFF000077 );


			// wrap the view toolbox button around the "View Toolbox" label only
			ViewToolboxButton.SetTitle( "", UIControlState.Normal );
			ViewToolboxButton.Layer.AnchorPoint = CGPoint.Empty;
			ViewToolboxButton.Layer.Position = new CGPoint( 0, ViewProfileButton.Frame.Bottom + 10 );
			ViewToolboxButton.Bounds = new CGRect( 0, 0, View.Frame.Width, ViewToolboxLabel.Frame.Height * 2 );
			//ViewToolboxButton.BackgroundColor = UIColor.Green;

			ViewToolboxLabel.Layer.AnchorPoint = CGPoint.Empty;
			ViewToolboxLabel.Font = FontManager.GetFont( ControlStylingConfig.Font_Light, ControlStylingConfig.Small_FontSize );
			ViewToolboxLabel.SizeToFit( );
			ViewToolboxLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
			ViewToolboxLabel.Layer.Position = new CGPoint( EditPictureButton.Layer.Position.X + ( ( EditPictureButton.Bounds.Width - ViewToolboxLabel.Bounds.Width ) / 2 ), ViewProfileButton.Frame.Bottom + 10 );

            
            // HERE IS WHERE WE ORDER THE SPRINGBOARD ITEMS 
            NewsElement.Layer.AnchorPoint = CGPoint.Empty;
            NewsElement.Layer.Position = new CGPoint( 0, ViewToolboxButton.Frame.Bottom + 10 );

            MessagesElement.Layer.AnchorPoint = CGPoint.Empty;
            MessagesElement.Layer.Position = new CGPoint( 0, NewsElement.Frame.Bottom );

            GiveElement.Layer.AnchorPoint = CGPoint.Empty;
            GiveElement.Layer.Position = new CGPoint( 0, MessagesElement.Frame.Bottom );

            ConnectElement.Layer.AnchorPoint = CGPoint.Empty;
            ConnectElement.Layer.Position = new CGPoint( 0, GiveElement.Frame.Bottom );

            PrayerElement.Layer.AnchorPoint = CGPoint.Empty;
            PrayerElement.Layer.Position = new CGPoint( 0, ConnectElement.Frame.Bottom );

            MoreElement.Layer.AnchorPoint = CGPoint.Empty;
            MoreElement.Layer.Position = new CGPoint( 0, PrayerElement.Frame.Bottom );

            BottomSeperator.Layer.AnchorPoint = CGPoint.Empty;
            BottomSeperator.Layer.Position = new CGPoint( 0, MoreElement.Frame.Bottom );

            // so basically, if the springboard is too large for the device and needs to scroll,
            // put campus selection under the "More" element.
            CampusSelectionText.Layer.AnchorPoint = CGPoint.Empty;
            if ( BottomSeperator.Frame.Bottom >= View.Frame.Height )
            {
                CampusSelectionText.Layer.Position = new CGPoint( 10, BottomSeperator.Frame.Bottom + 25 );
            }
            // if it is NOT too large, place it at the bottom of the screen
            else
            {
                CampusSelectionText.Layer.Position = new CGPoint( 10, View.Frame.Height - CampusSelectionText.Frame.Height - 10 );
            }

            UpdateCampusViews( );

            nfloat controlBottom = CampusSelectionButton.Frame.Bottom + ( View.Bounds.Height * .05f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );
        }

        void UpdateCampusViews( )
        {
            CampusSelectionText.SizeToFit( );
            CampusSelectionText.Frame = new CGRect( CampusSelectionText.Frame.Left, 
                                                    CampusSelectionText.Frame.Top, 
                                                    Math.Min( CampusSelectionText.Frame.Width, PrivatePrimaryContainerConfig.SlideAmount_iOS - PrivateSpringboardConfig.CampusTextMaxSizeOffset_iOS ), 
                                                    CampusSelectionText.Frame.Height );

            CampusSelectionIcon.SizeToFit( );

            CampusSelectionIcon.Layer.AnchorPoint = CGPoint.Empty;

            nfloat halfPoint = ( CampusSelectionText.Frame.Height / 2 ) - ( CampusSelectionIcon.Frame.Height / 2 );
            CampusSelectionIcon.Layer.Position = new CGPoint( CampusSelectionText.Frame.Right + 4, CampusSelectionText.Frame.Top + halfPoint );

            // overlay the button across the campus text and icon
            CampusSelectionButton.Frame = new CGRect( CampusSelectionText.Frame.Left, CampusSelectionText.Frame.Top, CampusSelectionIcon.Frame.Right, CampusSelectionText.Frame.Height );
        }

        protected void UpdateLoginState( )
        {
            // assume they aren't coaching
            ViewToolboxLabel.Hidden = true;
            ViewToolboxButton.Enabled = false;

            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                WelcomeField.Text = SpringboardStrings.LoggedIn_Prefix;
                UserNameField.Text = RockMobileUser.Instance.PreferredName( );
                ViewProfileLabel.Text = SpringboardStrings.ViewProfile;

                // if they're teaching, let them access their toolbox
                if( RockMobileUser.Instance.IsTeaching )
                {
                    ViewToolboxLabel.Hidden = false;
                    ViewToolboxButton.Enabled = true;
                }
            }
            else
            {
                WelcomeField.Text = SpringboardStrings.LoggedOut_Label;
                UserNameField.Text = "";
                ViewProfileLabel.Text = SpringboardStrings.LoggedOut_Promo;
            }

            // update the positioning of the "Welcome: Name"
            WelcomeField.SizeToFit( );
            UserNameField.SizeToFit( );

            UpdateProfilePic( );

            RefreshCampusSelection( );
        }

        public void UpdateProfilePic( )
        {
            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                bool useNoPhotoImage = true;

                // if they have a profile image
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
                    // attempt to load it, but
                    // because the profile picture is dynamic, make sure it loads correctly.
                    try
                    {
                        MemoryStream imageStream = (MemoryStream) FileCache.Instance.LoadFile( PrivateSpringboardConfig.ProfilePic );

                        NSData imageData = NSData.FromStream( imageStream );
                        UIImage image = new UIImage( imageData );

                        ProfileImageView.Image = image;

                        useNoPhotoImage = false;
                    }
                    catch(Exception)
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "Bad Pic! Defaulting to No Photo" );
                    }
                }

                // if we made it here and useNoPhoto is true, well, use no photo
                if ( useNoPhotoImage == true )
                {
                    ProfileImageView.Image = null;
                    EditPictureButton.SetTitle( PrivateSpringboardConfig.NoPhotoSymbol, UIControlState.Normal );
                }
                else
                {
                    EditPictureButton.SetTitle( "", UIControlState.Normal );
                }
            }
            else
            {
                // otherwise display the no profile image.
                ProfileImageView.Image = null;
                EditPictureButton.SetTitle( PrivateSpringboardConfig.NoProfileSymbol, UIControlState.Normal );
            }
        }

        public static void DisplayError( string title, string message )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIAlertView alert = new UIAlertView();
                    alert.Title = title;
                    alert.Message = message;
                    alert.AddButton( GeneralStrings.Ok );
                    alert.Show( ); 
                } );
        }

        public void OnActivated( )
        {
            NavViewController.OnActivated( );

            // resync data
            OnActivated_SyncRockData( );
            UpdateLoginState( );
        }

        public void WillEnterForeground( )
        {
            NavViewController.WillEnterForeground( );
        }

        public void OnResignActive( )
        {
            NavViewController.OnResignActive( );
        }

        public void DidEnterBackground( )
        {
            NavViewController.DidEnterBackground( );

            // request quick backgrounding so we can save objects
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            RockNetworkManager.Instance.SaveObjectsToDevice( );

            FileCache.Instance.SaveCacheMap( );

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        public void WillTerminate( )
        {
            NavViewController.WillTerminate( );

            // request quick backgrounding so we can save objects
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            RockNetworkManager.Instance.SaveObjectsToDevice( );

            FileCache.Instance.SaveCacheMap( );

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }
	}
}
