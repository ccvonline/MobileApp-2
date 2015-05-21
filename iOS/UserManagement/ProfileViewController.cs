using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using Rock.Mobile.Network;
using App.Shared.Network;
using CoreAnimation;
using CoreGraphics;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.UI;
using System.Collections.Generic;
using Rock.Mobile.Util.Strings;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using Rock.Mobile.PlatformSpecific.Util;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class ProfileViewController : UIViewController
	{
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// True when a change to the profile was made and the user should be prompted
        /// to submit changes.
        /// </summary>
        /// <value><c>true</c> if dirty; otherwise, <c>false</c>.</value>
        protected bool Dirty { get; set; }

        /// <summary>
        /// View for displaying the logo in the header
        /// </summary>
        /// <value>The logo view.</value>
        UIImageView LogoView { get; set; }

        UIView HeaderView { get; set; }

        PickerAdjustManager GenderPicker { get; set; }

        PickerAdjustManager BirthdatePicker { get; set; }

        StyledTextField NickName { get; set; }
        StyledTextField LastName { get; set; }

        StyledTextField Email { get; set; }
        StyledTextField CellPhone { get; set; }

        StyledTextField Street { get; set; }
        StyledTextField City { get; set; }
        StyledTextField State { get; set; }
        StyledTextField Zip { get; set; }

        StyledTextField Gender { get; set; }
        StyledTextField Birthdate { get; set; }
        StyledTextField HomeCampus { get; set; }

        UIButton GenderButton { get; set; }
        UIButton BirthdayButton { get; set; }
        UIButton HomeCampusButton { get; set; }

        UIButton DoneButton { get; set; }
        UIButton LogoutButton { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

		public ProfileViewController (IntPtr handle) : base (handle)
		{
		}

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // insist they stay in portait on iPhones
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // setup the fake header
            HeaderView = new UIView( );
            View.AddSubview( HeaderView );

            HeaderView.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivatePrimaryNavBarConfig.LogoFile_iOS;
            LogoView = new UIImageView( new UIImage( imagePath ) );
            HeaderView.AddSubview( LogoView );


            ScrollView = new UIScrollViewWrapper();

            View.AddSubview( ScrollView );
            ScrollView.Parent = this;

            //setup styles
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            NickName = new StyledTextField();
            ScrollView.AddSubview( NickName.Background );

            ControlStyling.StyleTextField( NickName.Field, ProfileStrings.NickNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( NickName.Background );
            NickName.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            NickName.Field.AutocorrectionType = UITextAutocorrectionType.No;
            NickName.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            LastName = new StyledTextField();

            LastName.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            LastName.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( LastName.Field, ProfileStrings.LastNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastName.Background );
            LastName.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            Email = new StyledTextField();
            ScrollView.AddSubview( Email.Background );
            Email.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            Email.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ScrollView.AddSubview( LastName.Background );
            ControlStyling.StyleTextField( Email.Field, ProfileStrings.EmailPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( Email.Background );
            Email.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            CellPhone = new StyledTextField();
            ScrollView.AddSubview( CellPhone.Background );
            CellPhone.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            CellPhone.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( CellPhone.Field, ProfileStrings.CellPhonePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( CellPhone.Background );
            CellPhone.Field.EditingDidBegin += (sender, e) => { Dirty = true; };


            Street = new StyledTextField();
            ScrollView.AddSubview( Street.Background );
            Street.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            Street.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( Street.Field, ProfileStrings.StreetPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( Street.Background );
            Street.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            City = new StyledTextField();
            ScrollView.AddSubview( City.Background );
            City.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            City.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( City.Field, ProfileStrings.CityPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( City.Background );
            City.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            State = new StyledTextField();
            ScrollView.AddSubview( State.Background );
            State.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            State.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( State.Field, ProfileStrings.StatePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( State.Background );
            State.Field.EditingDidBegin += (sender, e) => { Dirty = true; };

            Zip = new StyledTextField();
            ScrollView.AddSubview( Zip.Background );
            Zip.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            Zip.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( Zip.Field, ProfileStrings.ZipPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( Zip.Background );
            Zip.Field.EditingDidBegin += (sender, e) => { Dirty = true; };


            // Gender
            Gender = new StyledTextField();
            ScrollView.AddSubview( Gender.Background );
            Gender.Field.UserInteractionEnabled = false;
            ControlStyling.StyleTextField( Gender.Field, ProfileStrings.GenderPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( Gender.Background );

            GenderButton = new UIButton( );
            ScrollView.AddSubview( GenderButton );
            GenderButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    // don't allow multiple pickers
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false )
                    {
                        // if they have a gender selected, default to that.
                        if( string.IsNullOrEmpty( Gender.Field.Text ) == false )
                        {
                            ((UIPickerView)GenderPicker.Picker).Select( RockGeneralData.Instance.Data.Genders.IndexOf( Gender.Field.Text ) - 1, 0, false );
                        }

                        GenderPicker.TogglePicker( true );
                    }
                };
            //

            // Birthday
            Birthdate = new StyledTextField( );
            ScrollView.AddSubview( Birthdate.Background );
            Birthdate.Field.UserInteractionEnabled = false;
            ControlStyling.StyleTextField( Birthdate.Field, ProfileStrings.BirthdatePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( Birthdate.Background );

            BirthdayButton = new UIButton( );
            ScrollView.AddSubview( BirthdayButton );
            BirthdayButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    // don't allow multiple pickers
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false )
                    {
                        // setup the default date time to display
                        DateTime initialDate = DateTime.Now;
                        if( string.IsNullOrEmpty( Birthdate.Field.Text ) == false )
                        {
                            initialDate = DateTime.Parse( Birthdate.Field.Text );
                        }

                        ((UIDatePicker)BirthdatePicker.Picker).Date = initialDate.DateTimeToNSDate( );
                        BirthdatePicker.TogglePicker( true );
                    }
                };
            //


            // setup the home campus chooser
            HomeCampus = new StyledTextField( );
            ScrollView.AddSubview( HomeCampus.Background );
            HomeCampus.Field.UserInteractionEnabled = false;
            ControlStyling.StyleTextField( HomeCampus.Field, ProfileStrings.CampusPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( HomeCampus.Background );

            HomeCampusButton = new UIButton( );
            ScrollView.AddSubview( HomeCampusButton );
            HomeCampusButton.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    UIAlertController actionSheet = UIAlertController.Create( ProfileStrings.SelectCampus_SourceTitle, 
                                                                              ProfileStrings.SelectCampus_SourceDescription, 
                                                                              UIAlertControllerStyle.ActionSheet );

                    // if the device is a tablet, anchor the menu
                    if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad )
                    {
                        actionSheet.PopoverPresentationController.SourceView = HomeCampusButton;
                        actionSheet.PopoverPresentationController.SourceRect = HomeCampusButton.Bounds;
                    }

                    // for each campus, create an entry in the action sheet, and its callback will assign
                    // that campus index to the user's viewing preference
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        UIAlertAction campusAction = UIAlertAction.Create( RockGeneralData.Instance.Data.Campuses[ i ].Name, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                            {
                                // update the home campus text and flag as dirty
                                HomeCampus.Field.Text = obj.Title;
                                Dirty = true;
                            } );

                        actionSheet.AddAction( campusAction );
                    }

                    // let them cancel, too
                    UIAlertAction cancelAction = UIAlertAction.Create( GeneralStrings.Cancel, UIAlertActionStyle.Cancel, delegate { });
                    actionSheet.AddAction( cancelAction );

                    PresentViewController( actionSheet, true, null );
                };

            DoneButton = new UIButton( );
            ScrollView.AddSubview( DoneButton );
            ControlStyling.StyleButton( DoneButton, ProfileStrings.DoneButtonTitle, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            DoneButton.SizeToFit( );

            LogoutButton = new UIButton( );
            ScrollView.AddSubview( LogoutButton );
            LogoutButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            LogoutButton.SetTitle( ProfileStrings.LogoutButtonTitle, UIControlState.Normal );
            LogoutButton.SizeToFit( );


            // setup the pickers
            UILabel genderPickerLabel = new UILabel( );
            ControlStyling.StyleUILabel( genderPickerLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            genderPickerLabel.Text = ProfileStrings.SelectGenderLabel;

            GenderPicker = new PickerAdjustManager( View, ScrollView, genderPickerLabel, Gender.Background );
            UIPickerView genderPicker = new UIPickerView();
            genderPicker.Model = new GenderPickerModel() { Parent = this };
            GenderPicker.SetPicker( genderPicker );


            UILabel birthdatePickerLabel = new UILabel( );
            ControlStyling.StyleUILabel( birthdatePickerLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            birthdatePickerLabel.Text = ProfileStrings.SelectBirthdateLabel;
            BirthdatePicker = new PickerAdjustManager( View, ScrollView, birthdatePickerLabel, Birthdate.Background );

            UIDatePicker datePicker = new UIDatePicker();
            datePicker.SetValueForKey( UIColor.White, new NSString( "textColor" ) );
            datePicker.Mode = UIDatePickerMode.Date;
            datePicker.MinimumDate = new DateTime( 1900, 1, 1 ).DateTimeToNSDate( );
            datePicker.MaximumDate = DateTime.Now.DateTimeToNSDate( );
            datePicker.ValueChanged += (object sender, EventArgs e ) =>
            {
                NSDate pickerDate = ((UIDatePicker) sender).Date;
                Birthdate.Field.Text = string.Format( "{0:MMMMM dd yyyy}", pickerDate.NSDateToDateTime( ) );
            };
            BirthdatePicker.SetPicker( datePicker );


            // Allow the return on username and password to start
            // the login process
            NickName.Field.ShouldReturn += TextFieldShouldReturn;
            LastName.Field.ShouldReturn += TextFieldShouldReturn;

            Email.Field.ShouldReturn += TextFieldShouldReturn;

            // If submit is pressed with dirty changes, prompt the user to save them.
            DoneButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false)
                    {
                        if( Dirty == true )
                        {
                            // make sure the input is valid before asking them what they want to do.
                            if ( ValidateInput( ) )
                            {
                                // if there were changes, create an action sheet for them to confirm.
                                var actionSheet = new UIActionSheet( ProfileStrings.SubmitChangesTitle );
                                actionSheet.AddButton( GeneralStrings.Yes );
                                actionSheet.AddButton( GeneralStrings.No );
                                actionSheet.AddButton( GeneralStrings.Cancel );

                                actionSheet.CancelButtonIndex = 2;

                                actionSheet.Clicked += SubmitActionSheetClicked;

                                actionSheet.ShowInView( View );
                            }
                        }
                        else
                        {
                            Springboard.ResignModelViewController( this, null );
                        }
                    }
                    else
                    {
                        GenderPicker.TogglePicker( false );
                        BirthdatePicker.TogglePicker( false );
                        Dirty = true;
                    }
                };

            // On logout, make sure the user really wants to log out.
            LogoutButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( GenderPicker.Revealed == false && BirthdatePicker.Revealed == false)
                    {
                        // if they tap logout, and confirm it
                        var actionSheet = new UIActionSheet( ProfileStrings.LogoutTitle, null, GeneralStrings.Cancel, GeneralStrings.Yes, null );

                        actionSheet.ShowInView( View );

                        actionSheet.Clicked += (object s, UIButtonEventArgs ev) => 
                            {
                                if( ev.ButtonIndex == actionSheet.DestructiveButtonIndex )
                                {
                                    // then log them out.
                                    RockMobileUser.Instance.LogoutAndUnbind( );

                                    Springboard.ResignModelViewController( this, null );
                                }
                            };
                    }
                    else
                    {
                        GenderPicker.TogglePicker( false );
                        BirthdatePicker.TogglePicker( false );
                        Dirty = true;
                    }
                };

            Dirty = false;

            // logged in sanity check.
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            HeaderView.Frame = new CGRect( View.Frame.Left, View.Frame.Top, View.Frame.Width, StyledTextField.StyledFieldHeight );
            ScrollView.Frame = new CGRect( View.Frame.Left, HeaderView.Frame.Bottom, View.Frame.Width, View.Frame.Height - HeaderView.Frame.Height );
            NickName.SetFrame( new CGRect( -10, View.Frame.Height * .05f, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            LastName.SetFrame( new CGRect( -10, NickName.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            Email.SetFrame( new CGRect( -10, LastName.Background.Frame.Bottom + 20, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            CellPhone.SetFrame( new CGRect( -10, Email.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            Street.SetFrame( new CGRect( -10, CellPhone.Background.Frame.Bottom + 20, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            City.SetFrame( new CGRect( -10, Street.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            State.SetFrame( new CGRect( -10, City.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            Zip.SetFrame( new CGRect( -10, State.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            Gender.SetFrame( new CGRect( -10, Zip.Background.Frame.Bottom + 20, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            GenderButton.Frame = Gender.Background.Frame;
            Birthdate.SetFrame( new CGRect( -10, Gender.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            BirthdayButton.Frame = Birthdate.Background.Frame;
            HomeCampus.SetFrame( new CGRect( -10, Birthdate.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            HomeCampusButton.Frame = HomeCampus.Background.Frame;

            DoneButton.Frame = new CGRect( View.Frame.Left + 10, HomeCampus.Background.Frame.Bottom + 20, View.Bounds.Width - 20, ControlStyling.ButtonHeight );
            LogoutButton.Frame = new CGRect( ( View.Frame.Width - ControlStyling.ButtonWidth) / 2, DoneButton.Frame.Bottom + 20, ControlStyling.ButtonWidth, ControlStyling.ButtonHeight );

            nfloat controlBottom = LogoutButton.Frame.Bottom + ( View.Bounds.Height * .25f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new CGPoint( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );

            BirthdatePicker.LayoutChanged( );
            GenderPicker.LayoutChanged( );

            GenderPicker.TogglePicker( false, false );
            BirthdatePicker.TogglePicker( false, false );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ScrollView.ContentOffset = CGPoint.Empty;

            // set values
            NickName.Field.Text = RockMobileUser.Instance.Person.NickName;
            LastName.Field.Text = RockMobileUser.Instance.Person.LastName;
            Email.Field.Text = RockMobileUser.Instance.Person.Email;

            // setup the phone number
            CellPhone.Field.Delegate = new Rock.Mobile.PlatformSpecific.iOS.UI.PhoneNumberFormatterDelegate();
            CellPhone.Field.Text = RockMobileUser.Instance.CellPhoneNumberDigits( );
            CellPhone.Field.Delegate.ShouldChangeCharacters( CellPhone.Field, new NSRange( CellPhone.Field.Text.Length, 0 ), "" );

            // address
            Street.Field.Text = RockMobileUser.Instance.Street1( );
            City.Field.Text = RockMobileUser.Instance.City( );
            State.Field.Text = RockMobileUser.Instance.State( );
            Zip.Field.Text = RockMobileUser.Instance.Zip( );

            // gender
            if ( RockMobileUser.Instance.Person.Gender > 0 )
            {
                Gender.Field.Text = RockGeneralData.Instance.Data.Genders[ RockMobileUser.Instance.Person.Gender ];
            }
            else
            {
                Gender.Field.Text = string.Empty;
            }

            if ( RockMobileUser.Instance.Person.BirthDate.HasValue == true )
            {
                Birthdate.Field.Text = string.Format( "{0:MMMMM dd yyyy}", RockMobileUser.Instance.Person.BirthDate );
            }
            else
            {
                Birthdate.Field.Text = string.Empty;
            }

            if ( RockMobileUser.Instance.PrimaryFamily.CampusId.HasValue )
            {
                HomeCampus.Field.Text = RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.PrimaryFamily.CampusId.Value );
            }
            else
            {
                HomeCampus.Field.Text = string.Empty;
            }
        }

        public void SubmitActionSheetClicked(object sender, UIButtonEventArgs e)
        {
            switch( e.ButtonIndex )
            {
                // submit
                case 0: Dirty = false; SubmitChanges( ); Springboard.ResignModelViewController( this, null ); break;

                // No, don't submit
                case 1: Dirty = false; Springboard.ResignModelViewController( this, null ); break;

                // cancel
                case 2: break;
            }
        }

        public bool TextFieldShouldReturn( UITextField textField )
        {
            if( textField.IsFirstResponder == true )
            {
                textField.ResignFirstResponder();
                return true;
            }

            return false;
        }

        bool ValidateInput( )
        {
            bool result = true;

            // the only one we really care about is email, to ensure they put a valid address
            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( Email.Field.Text ) == false && Email.Field.Text.IsEmailFormat( ) == false )
            {
                // if failure, only color email
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, Email.Background );

            return result;
        }

        void SubmitChanges()
        {
            // copy all the edited fields into the person object
            RockMobileUser.Instance.Person.Email = Email.Field.Text;

            RockMobileUser.Instance.Person.NickName = NickName.Field.Text;
            RockMobileUser.Instance.Person.LastName = LastName.Field.Text;

            // Update their cell phone. 
            if ( string.IsNullOrEmpty( CellPhone.Field.Text ) == false )
            {
                // update the phone number
                RockMobileUser.Instance.SetPhoneNumberDigits( CellPhone.Field.Text.AsNumeric( ) );
            }

            // Gender
            if ( string.IsNullOrEmpty( Gender.Field.Text ) == false )
            {
                RockMobileUser.Instance.Person.Gender = RockGeneralData.Instance.Data.Genders.IndexOf( Gender.Field.Text );
            }

            // Birthdate
            if ( string.IsNullOrEmpty( Birthdate.Field.Text ) == false )
            {
                RockMobileUser.Instance.SetBirthday( DateTime.Parse( Birthdate.Field.Text ) );
            }

            // Campus
            if ( string.IsNullOrEmpty( HomeCampus.Field.Text ) == false )
            {
                RockMobileUser.Instance.PrimaryFamily.CampusId = RockGeneralData.Instance.Data.CampusNameToId( HomeCampus.Field.Text );
                RockMobileUser.Instance.ViewingCampus = RockMobileUser.Instance.PrimaryFamily.CampusId.Value;
            }

            // address (make sure that all fields are set)
            if ( string.IsNullOrEmpty( Street.Field.Text ) == false &&
                 string.IsNullOrEmpty( City.Field.Text ) == false &&
                 string.IsNullOrEmpty( State.Field.Text ) == false &&
                 string.IsNullOrEmpty( Zip.Field.Text ) == false )
            {
                RockMobileUser.Instance.SetAddress( Street.Field.Text, City.Field.Text, State.Field.Text, Zip.Field.Text );
            }

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
            RockMobileUser.Instance.UpdateAddress( null );
            RockMobileUser.Instance.UpdateHomeCampus( null );
            RockMobileUser.Instance.UpdateOrAddPhoneNumber( null );

            ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Update );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            // if we're picking a gender, don't allow anything else.
            if ( GenderPicker.Revealed == true )
            {
                GenderPicker.TogglePicker( false );
                Dirty = true;
            }
            else if ( BirthdatePicker.Revealed == true )
            {
                BirthdatePicker.TogglePicker( false );
                Dirty = true;
            }
            else
            {
                base.TouchesEnded( touches, evt );
            
                // if they tap somewhere outside of the text fields, 
                // hide the keyboard
                TextFieldShouldReturn( NickName.Field );
                TextFieldShouldReturn( LastName.Field );

                TextFieldShouldReturn( CellPhone.Field );
                TextFieldShouldReturn( Email.Field );

                TextFieldShouldReturn( Street.Field );
                TextFieldShouldReturn( City.Field );
                TextFieldShouldReturn( State.Field );
                TextFieldShouldReturn( Zip.Field );

                TextFieldShouldReturn( Birthdate.Field );
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        /// <summary>
        /// Called when the user selects something in the UIPicker
        /// </summary>
        public void PickerSelected( int row, int component )
        {
            // set the button's text to be the item they selected. Note that we now change the color to Active from the original Placeholder
            Gender.Field.Text = RockGeneralData.Instance.Data.Genders[ row ];
            Gender.Field.TextColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
        }

        /// <summary>
        /// The model that defines the object that will be selected in the UI Picker
        /// </summary>
        public class GenderPickerModel : UIPickerViewModel
        {
            public ProfileViewController Parent { get; set; }

            public override nint GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView picker, nint component)
            {
                return RockGeneralData.Instance.Data.Genders.Count - 1;
            }

            public override string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return RockGeneralData.Instance.Data.Genders[ (int) (row + 1) ];
            }

            public override void Selected(UIPickerView picker, nint row, nint component)
            {
                Parent.PickerSelected( (int) (row + 1), (int) component );
            }

            public override UIView GetView(UIPickerView picker, nint row, nint component, UIView view)
            {
                UILabel label = view as UILabel;
                if ( label == null )
                {
                    label = new UILabel();
                    label.TextColor = UIColor.White;
                    label.Text = RockGeneralData.Instance.Data.Genders[ (int) (row + 1) ];
                    label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    label.SizeToFit( );
                }

                return label;
            }
        }
	}
}
