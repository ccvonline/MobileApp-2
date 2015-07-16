using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.UI;
using App.Shared.Strings;
using System.Collections.Generic;
using CoreGraphics;
using App.Shared.Config;
using App.Shared.Network;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using Rock.Mobile.Animation;
using App.Shared;
using Rock.Mobile.IO;
using App.Shared.UI;
using Rock.Mobile.PlatformSpecific.Util;

namespace iOS
{
    partial class Prayer_CreateUIViewController : TaskUIViewController, IUIGestureRecognizerDelegate
	{
        /// <summary>
        /// List of the handles for our NSNotifications
        /// </summary>
        List<NSObject> ObserverHandles { get; set; }

        /// <summary>
        /// The keyboard manager that will adjust the UIView to not be obscured by the software keyboard
        /// </summary>
        KeyboardAdjustManager KeyboardAdjustManager { get; set; }

        /// <summary>
        /// The starting position of the scrollView so we can restore after the user uses the UIPicker
        /// </summary>
        /// <value>The starting scroll position.</value>
        CGPoint StartingScrollPos { get; set; }

        PickerAdjustManager PickerAdjustManager { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

        UIView CategoryLayer { get; set; }
        UIButton CategoryButton { get; set; }

        StyledTextField FirstName { get; set; }

        StyledTextField LastName { get; set; }

        UILabel MakePublicLabel { get; set; }
        UILabel PostAnonymouslyLabel { get; set; }

        UIView PrayerRequestLayer { get; set; }
        UILabel PrayerRequestPlaceholder { get; set; }
        UITextView PrayerRequest { get; set; }

        UIView SwitchBackground { get; set; }
        UISwitch UIPublicSwitch { get; set; }
        UISwitch UISwitchAnonymous { get; set; }

        UIButton SubmitButton { get; set; }

        public Prayer_CreateUIViewController ( )
        {
            ObserverHandles = new List<NSObject>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // set the background view to black so we don't get white aliasing flicker during
            // the pan
            View.BackgroundColor = UIColor.Black;
            View.Layer.AnchorPoint = CGPoint.Empty;


            // scroll view
            ScrollView = new UIScrollViewWrapper( );
            ScrollView.Layer.AnchorPoint = CGPoint.Empty;
            ScrollView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            ScrollView.Parent = this;
            View.AddSubview( ScrollView );



            // create our keyboard adjustment manager, which works to make sure text fields scroll into visible
            // range when a keyboard appears
            KeyboardAdjustManager = new Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager( View );


            // setup the First Name field
            FirstName = new StyledTextField();
            ScrollView.AddSubview( FirstName.Background );
            ControlStyling.StyleTextField( FirstName.Field, PrayerStrings.CreatePrayer_FirstNamePlaceholderText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( FirstName.Background );

            LastName = new StyledTextField();
            ScrollView.AddSubview( LastName.Background );
            ControlStyling.StyleTextField( LastName.Field, PrayerStrings.CreatePrayer_LastNamePlaceholderText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastName.Background );


            PrayerRequestLayer = new UIView();
            ScrollView.AddSubview( PrayerRequestLayer );

            PrayerRequestPlaceholder = new UILabel();
            PrayerRequestLayer.AddSubview( PrayerRequestPlaceholder );

            PrayerRequest = new UITextView();
            PrayerRequestLayer.AddSubview( PrayerRequest );

            // setup the prayer request field, which requires a fake "placeholder" text field
            PrayerRequest.Delegate = new KeyboardAdjustManager.TextViewDelegate( );
            PrayerRequest.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            PrayerRequest.TextContainerInset = UIEdgeInsets.Zero;
            PrayerRequest.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            PrayerRequest.TextContainer.LineFragmentPadding = 0;
            PrayerRequest.BackgroundColor = UIColor.Clear;
            PrayerRequest.Editable = true;
            PrayerRequest.KeyboardAppearance = UIKeyboardAppearance.Dark;
            PrayerRequestPlaceholder.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            PrayerRequestPlaceholder.BackgroundColor = UIColor.Clear;
            PrayerRequestPlaceholder.Text = PrayerStrings.CreatePrayer_PrayerRequest;
            PrayerRequestPlaceholder.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            //PrayerRequestPlaceholder.SizeToFit( );
            ControlStyling.StyleBGLayer( PrayerRequestLayer );


            // category layer
            CategoryLayer = new UIView();
            ScrollView.AddSubview( CategoryLayer );

            CategoryButton = new UIButton();
            CategoryLayer.AddSubview( CategoryButton );

            // setup the category picker and selector button
            UILabel categoryLabel = new UILabel( );
            ControlStyling.StyleUILabel( categoryLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            categoryLabel.Text = PrayerStrings.CreatePrayer_SelectCategoryLabel;

            PickerAdjustManager = new PickerAdjustManager( View, ScrollView, categoryLabel, CategoryLayer );
            UIPickerView pickerView = new UIPickerView();
            pickerView.Model = new CategoryPickerModel() { Parent = this };
            pickerView.UserInteractionEnabled = true;
            PickerAdjustManager.SetPicker( pickerView );


            // setup a tap gesture for the picker
            Action action = ( ) =>
                {
                    OnToggleCategoryPicker( false );
                };
            UITapGestureRecognizer uiTap = new UITapGestureRecognizer( action );
            uiTap.NumberOfTapsRequired = 1;
            pickerView.AddGestureRecognizer( uiTap );
            uiTap.Delegate = this;


            CategoryButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    OnToggleCategoryPicker( true );
                };
            CategoryButton.SetTitle( PrayerStrings.CreatePrayer_CategoryButtonText, UIControlState.Normal );
            CategoryButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CategoryButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            CategoryButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            ControlStyling.StyleBGLayer( CategoryLayer );


            // preference switches
            SwitchBackground = new UIView();
            ScrollView.AddSubview( SwitchBackground );
            ControlStyling.StyleBGLayer( SwitchBackground );

            UIPublicSwitch = new UISwitch();
            SwitchBackground.AddSubview( UIPublicSwitch );

            MakePublicLabel = new UILabel();
            SwitchBackground.AddSubview( MakePublicLabel );
            //MakePublicLabel.TextColor = UIColor.White;


            UISwitchAnonymous = new UISwitch();
            SwitchBackground.AddSubview( UISwitchAnonymous );

            PostAnonymouslyLabel = new UILabel();
            SwitchBackground.AddSubview( PostAnonymouslyLabel );
            //PostAnonymouslyLabel.TextColor = UIColor.White;


            // Setup the anonymous switch
            PostAnonymouslyLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            PostAnonymouslyLabel.Text = PrayerStrings.CreatePrayer_PostAnonymously;
            PostAnonymouslyLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            UISwitchAnonymous.OnTintColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Switch_OnColor );
            UISwitchAnonymous.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    OnToggleCategoryPicker( false );

                    if( UISwitchAnonymous.On == true )
                    {
                        FirstName.Field.Enabled = false;
                        FirstName.Field.Text = PrayerStrings.CreatePrayer_Anonymous;

                        LastName.Field.Enabled = false;
                        LastName.Field.Text = PrayerStrings.CreatePrayer_Anonymous;
                    }
                    else
                    {
                        FirstName.Field.Enabled = true;
                        FirstName.Field.Text = string.Empty;

                        LastName.Field.Enabled = true;
                        LastName.Field.Text = string.Empty;
                    }

                    // reset the background colors
                    Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( ControlStylingConfig.BG_Layer_Color, FirstName.Background );
                    Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( ControlStylingConfig.BG_Layer_Color, LastName.Background );
                };

            // setup the public switch
            MakePublicLabel.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            MakePublicLabel.Text = PrayerStrings.CreatePrayer_MakePublic;
            MakePublicLabel.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            UIPublicSwitch.OnTintColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Switch_OnColor );
            //UIPublicSwitch.ThumbTintColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );


            // setup the submit button
            SubmitButton = UIButton.FromType( UIButtonType.Custom );
            ScrollView.AddSubview( SubmitButton );
            ControlStyling.StyleButton( SubmitButton, PrayerStrings.CreatePrayer_SubmitButtonText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            SubmitButton.SizeToFit( );
            SubmitButton.TouchUpInside += SubmitPrayerRequest;
        }

        [Export( "gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:" )]
        public bool ShouldRecognizeSimultaneously( UIKit.UIGestureRecognizer gestureRecognizer, UIKit.UIGestureRecognizer otherGestureRecognizer )
        {
            return true;
        }

        /// <summary>
        /// Builds a prayer request from data in the UI Fields and kicks off the post UI Control
        /// </summary>
        void SubmitPrayerRequest(object sender, EventArgs e)
        {
            if ( PickerAdjustManager.Revealed == true )
            {
                OnToggleCategoryPicker( false );
            }
            else
            {
                if( ValidateInput( ) )
                {
                    Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                    EnableControls( false );

                    prayerRequest.FirstName = FirstName.Field.Text;
                    prayerRequest.LastName = LastName.Field.Text;

                    // see if there's a person alias ID to use.
                    int? personAliasId = null;
                    if ( App.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId.HasValue )
                    {
                        personAliasId = App.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId;
                    }

                    prayerRequest.Text = PrayerRequest.Text;
                    prayerRequest.EnteredDateTime = DateTime.Now;
                    prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                    prayerRequest.CategoryId = RockGeneralData.Instance.Data.PrayerCategoryToId( CategoryButton.Title( UIControlState.Normal ) );
                    prayerRequest.IsActive = true;
                    prayerRequest.IsPublic = UIPublicSwitch.On; // use the public switch's state to determine whether it's a public prayer or not.
                    prayerRequest.Guid = Guid.NewGuid( );
                    prayerRequest.IsApproved = false;
                    prayerRequest.CreatedByPersonAliasId = UISwitchAnonymous.On == true ? null : personAliasId;

                    // launch the post view controller
                    Prayer_PostUIViewController postPrayerVC = new Prayer_PostUIViewController();
                    postPrayerVC.PrayerRequest = prayerRequest;
                    Task.PerformSegue( this, postPrayerVC );
                }
                else
                {
                    // check for debug features
                    CheckDebug( );
                }
            }
        }

        bool ValidateInput( )
        {
            bool result = true;

            // Update the first name background color
            // if they left the name field blank and didn't turn on Anonymous, flag the field.
            uint targetNameColor = ControlStylingConfig.BG_Layer_Color; 
            if ( string.IsNullOrEmpty( FirstName.Field.Text ) && UISwitchAnonymous.On == false )
            {
                targetNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetNameColor, FirstName.Background );


            // Update the LAST name background color
            // if they left the name field blank and didn't turn on Anonymous, flag the field.
            uint targetLastNameColor = ControlStylingConfig.BG_Layer_Color; 
            if ( string.IsNullOrEmpty( LastName.Field.Text ) && UISwitchAnonymous.On == false )
            {
                targetLastNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetLastNameColor, LastName.Background );


            // Update the prayer background color
            uint targetPrayerColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( PrayerRequest.Text ) == true )
            {
                targetPrayerColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetPrayerColor, PrayerRequestLayer );


            // update the category
            int categoryId = RockGeneralData.Instance.Data.PrayerCategoryToId( CategoryButton.Title( UIControlState.Normal ) );

            uint targetCategoryColor = ControlStylingConfig.BG_Layer_Color;
            if ( categoryId == -1 )
            {
                targetCategoryColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetCategoryColor, CategoryLayer );

            return result;
        }

        void CheckDebug( )
        {
            if( string.IsNullOrEmpty( FirstName.Field.Text ) == true && string.IsNullOrEmpty( LastName.Field.Text ) == true )
            {
                if ( PrayerRequest.Text.ToLower( ) == "clear cache" )
                {
                    FileCache.Instance.CleanUp( true );
                    SpringboardViewController.DisplayError( "Cache Cleared", "All cached items have been deleted" );
                }
                else if ( PrayerRequest.Text.ToLower( ) == "developer" )
                {
                    App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled = !App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled;
                    SpringboardViewController.DisplayError( "Developer Mode", 
                        string.Format( "Developer Mode has been toggled: {0}", App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled == true ? "ON" : "OFF" ) );
                }
                else if ( PrayerRequest.Text.ToLower( ) == "version" )
                {
                    SpringboardViewController.DisplayError( "Current Version", BuildStrings.Version );
                }
                else if ( PrayerRequest.Text.ToLower( ) == "upload dumps" )
                {
                    Xamarin.Insights.PurgePendingCrashReports( ).Wait( );
                    SpringboardViewController.DisplayError( "Crash Dumps Sent", "Just uploaded all pending crash dumps." );
                }
                // fun bonus!
                else if ( PrayerRequest.Text.ToLower( ) == "up up down down left right left right b a start" )
                {
                    UISpecial special = new UISpecial();
                    special.Create( ScrollView, "me.png", true, View.Frame.ToRectF( ), delegate { special.View.RemoveAsSubview( ScrollView ); });
                    special.LayoutChanged( new System.Drawing.RectangleF( 0, 0, (float)View.Bounds.Width, (float)ScrollView.ContentSize.Height ) );
                    ScrollView.ContentOffset = CGPoint.Empty;
                }
            }
        }
       
        /// <summary>
        /// Shows / Hides the category picker by animating the picker onto the screen and scrolling
        /// the ScrollView to reveal the category field.
        /// </summary>
        public void OnToggleCategoryPicker( bool enabled )
        {
            if ( enabled == true )
            {
                // we're going to show it, so hide the nav bar
                Task.NavToolbar.Reveal( false );

                // force the keyboard to hide.
                PrayerRequest.ResignFirstResponder( );
                FirstName.Field.ResignFirstResponder( );
                LastName.Field.ResignFirstResponder( );
            }

            PickerAdjustManager.TogglePicker( enabled );
        }

        /// <summary>
        /// Called when the user selects something in the UIPicker
        /// </summary>
        public void PickerSelected( int row )
        {
            // set the category's text to be the item they selected. Note that we now change the color to Active from the original Placeholder
            CategoryButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
            CategoryButton.SetTitle( RockGeneralData.Instance.Data.PrayerCategories[ row ].Name, UIControlState.Normal );

            //PickerAdjustManager.TogglePicker( false );
        }

        /// <summary>
        /// The model that defines the object that will be selected in the UI Picker
        /// </summary>
        public class CategoryPickerModel : UIPickerViewModel
        {
            public Prayer_CreateUIViewController Parent { get; set; }

            public override nint GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView picker, nint component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories.Count;
            }

            public override string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return RockGeneralData.Instance.Data.PrayerCategories[ (int)row ].Name;
            }

            public override void Selected(UIPickerView picker, nint row, nint component)
            {
                Parent.PickerSelected( (int)row );
            }

            public override UIView GetView(UIPickerView picker, nint row, nint component, UIView view)
            {
                UILabel label = view as UILabel;
                if ( label == null )
                {
                    label = new UILabel();
                    label.TextColor = UIColor.White;
                    label.Text = RockGeneralData.Instance.Data.PrayerCategories[ (int)row ].Name;
                    label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    label.SizeToFit( );
                }

                return label;
            }
        }

        void OnTextChanged( NSNotification notification )
        {
            TogglePlaceholderText( );
        }

        void TogglePlaceholderText( )
        {
            // toggle our fake placeholder text
            if ( PrayerRequest.Text == "" )
            {
                PrayerRequestPlaceholder.Hidden = false;
            }
            else
            {
                PrayerRequestPlaceholder.Hidden = true;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            foreach ( NSObject handle in ObserverHandles )
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver( handle );
            }

            ObserverHandles.Clear( );

            KeyboardAdjustManager.FreeObservers( );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // enable all controls
            EnableControls( true );

            // todo: determine if we're coming back from the post page.
            UISwitchAnonymous.SetState( false, false );
            UIPublicSwitch.SetState( true, false );

            TogglePlaceholderText( );

            // prepopulate the name fields if we have them
            if ( RockMobileUser.Instance.LoggedIn == true )
            {
                FirstName.Field.Text = RockMobileUser.Instance.Person.NickName;
                LastName.Field.Text = RockMobileUser.Instance.Person.LastName;
            }

            // monitor for text field being edited, and keyboard show/hide notitications
            NSObject handle = NSNotificationCenter.DefaultCenter.AddObserver( Rock.Mobile.PlatformSpecific.iOS.UI.KeyboardAdjustManager.TextControlChangedNotification, OnTextChanged);
            ObserverHandles.Add( handle );
        }

        public override void LayoutChanged( )
        {
            base.LayoutChanged( );

            PickerAdjustManager.LayoutChanged( );
            PickerAdjustManager.TogglePicker( false, false );

            ScrollView.Frame = View.Frame;

            FirstName.SetFrame( new CGRect( -10, 25, View.Bounds.Width + 20, 33 ) );
            LastName.SetFrame( new CGRect( -10, FirstName.Background.Frame.Bottom, View.Bounds.Width + 20, 33 ) );

            PrayerRequestLayer.Frame = new CGRect( -10, LastName.Background.Frame.Bottom + 20, View.Bounds.Width + 20, 200 );
            PrayerRequestPlaceholder.Frame = new CGRect( 20, 0, View.Bounds.Width - 20, 33 );
            PrayerRequest.Frame = new CGRect( 20, 3, View.Bounds.Width - 20, 200 );


            CategoryLayer.Frame = new CGRect( -10, PrayerRequestLayer.Frame.Bottom + 20, View.Bounds.Width + 20, 40 );
            CategoryButton.Frame = new CGRect( 20, 0, View.Bounds.Width - 20, 40 );


            SwitchBackground.Frame = new CGRect( -10, CategoryLayer.Frame.Bottom + 20, View.Bounds.Width + 20, 88 );
            PostAnonymouslyLabel.Frame = new CGRect( 20, 6, View.Bounds.Width - 10, 33 );

            UISwitchAnonymous.Frame = new CGRect( View.Bounds.Width- 10 - UISwitchAnonymous.Bounds.Width, 6, UISwitchAnonymous.Bounds.Width, UISwitchAnonymous.Bounds.Height );


            MakePublicLabel.Frame = new CGRect( 20, PostAnonymouslyLabel.Frame.Bottom + 10, View.Bounds.Width - 10, 33 );
            UIPublicSwitch.Frame = new CGRect( View.Bounds.Width- 10 - UIPublicSwitch.Bounds.Width, PostAnonymouslyLabel.Frame.Bottom + 10, UIPublicSwitch.Bounds.Width, UIPublicSwitch.Bounds.Height );

            SubmitButton.Frame = new CGRect( 20, SwitchBackground.Frame.Bottom + 20, View.Bounds.Width - 40, SubmitButton.Bounds.Height );

            // once all the controls are laid out, update the content size to provide a little "bounce"
            nfloat controlBottom = SubmitButton.Frame.Bottom + ( View.Bounds.Height * .25f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );
        }

        void EnableControls( bool enabled )
        {
            FirstName.Field.Enabled = enabled;
            LastName.Field.Enabled = enabled;

            PrayerRequest.Editable = enabled;

            UISwitchAnonymous.Enabled = enabled;
            UIPublicSwitch.Enabled = enabled;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // if we're picking a category, don't allow anything else.
            if ( PickerAdjustManager.Revealed == true )
            {
                OnToggleCategoryPicker( false );
            }
            else
            {
                base.TouchesEnded( touches, evt );

                // ensure that tapping anywhere outside a text field will hide the keyboard
                FirstName.Field.ResignFirstResponder( );
                LastName.Field.ResignFirstResponder( );
                PrayerRequest.ResignFirstResponder( );
            }
        }
	}
}
