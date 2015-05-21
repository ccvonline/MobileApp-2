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
using Rock.Mobile.Animation;
using App.Shared.UI;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;

namespace iOS
{
    partial class RegisterViewController : UIViewController
    {
        /// <summary>
        /// Reference to the parent springboard for returning apon completion
        /// </summary>
        /// <value>The springboard.</value>
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// View for displaying the logo in the header
        /// </summary>
        /// <value>The logo view.</value>
        UIImageView LogoView { get; set; }

        enum RegisterState
        {
            None,
            Trying,
            Success,
            Fail
        }

        RegisterState State { get; set; }

        UIBlockerView BlockerView { get; set; }

        UIResultView ResultView { get; set; }

        StyledTextField UserNameText { get; set; }
        StyledTextField PasswordText { get; set; }
        StyledTextField ConfirmPasswordText { get; set; }

        StyledTextField NickNameText { get; set; }
        StyledTextField LastNameText { get; set; }

        StyledTextField EmailText { get; set; }
        StyledTextField CellPhoneText { get; set; }

        UIButton DoneButton { get; set; }
        UIButton CancelButton { get; set; }

        UIView HeaderView { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }

        public RegisterViewController (IntPtr handle) : base (handle)
        {
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( )
        {
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
            ScrollView.Frame = new CGRect( View.Frame.Left, HeaderView.Frame.Bottom, View.Frame.Width, View.Frame.Height - HeaderView.Frame.Height );
            View.AddSubview( ScrollView );
            ScrollView.Parent = this;

            // logged in sanity check.
            //if( RockMobileUser.Instance.LoggedIn == true ) throw new Exception("A user cannot be logged in when registering. How did you do this?" );

            BlockerView = new UIBlockerView( View, View.Frame.ToRectF( ) );

            //setup styles
            View.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            UserNameText = new StyledTextField();
            ScrollView.AddSubview( UserNameText.Background );
            UserNameText.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            UserNameText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( UserNameText.Field, RegisterStrings.UsernamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( UserNameText.Background );

            PasswordText = new StyledTextField();
            ScrollView.AddSubview( PasswordText.Background );
            PasswordText.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            PasswordText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            PasswordText.Field.SecureTextEntry = true;
            ControlStyling.StyleTextField( PasswordText.Field, RegisterStrings.PasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( PasswordText.Background );

            ConfirmPasswordText = new StyledTextField();
            ScrollView.AddSubview( ConfirmPasswordText.Background );
            ConfirmPasswordText.Field.SecureTextEntry = true;
            ControlStyling.StyleTextField( ConfirmPasswordText.Field, RegisterStrings.ConfirmPasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( ConfirmPasswordText.Background );

            NickNameText = new StyledTextField();
            ScrollView.AddSubview( NickNameText.Background );
            NickNameText.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            NickNameText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( NickNameText.Field, RegisterStrings.NickNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( NickNameText.Background );

            LastNameText = new StyledTextField();
            ScrollView.AddSubview( LastNameText.Background );
            LastNameText.Field.AutocapitalizationType = UITextAutocapitalizationType.Words;
            LastNameText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( LastNameText.Field, RegisterStrings.LastNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( LastNameText.Background );

            EmailText = new StyledTextField();
            ScrollView.AddSubview( EmailText.Background );
            EmailText.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            EmailText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( EmailText.Field, RegisterStrings.EmailPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( EmailText.Background );

            CellPhoneText = new StyledTextField();
            ScrollView.AddSubview( CellPhoneText.Background );
            CellPhoneText.Field.AutocapitalizationType = UITextAutocapitalizationType.None;
            CellPhoneText.Field.AutocorrectionType = UITextAutocorrectionType.No;
            ControlStyling.StyleTextField( CellPhoneText.Field, RegisterStrings.CellPhonePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ControlStyling.StyleBGLayer( CellPhoneText.Background );

            DoneButton = UIButton.FromType( UIButtonType.System );
            ScrollView.AddSubview( DoneButton );
            ControlStyling.StyleButton( DoneButton, RegisterStrings.RegisterButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            DoneButton.SizeToFit( );


            CancelButton = UIButton.FromType( UIButtonType.System );
            ScrollView.AddSubview( CancelButton );
            CancelButton.SetTitleColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            CancelButton.SetTitle( GeneralStrings.Cancel, UIControlState.Normal );
            CancelButton.SizeToFit( );


            // Allow the return on username and password to start
            // the login process
            NickNameText.Field.ShouldReturn += TextFieldShouldReturn;
            LastNameText.Field.ShouldReturn += TextFieldShouldReturn;

            EmailText.Field.ShouldReturn += TextFieldShouldReturn;

            // If submit is pressed with dirty changes, prompt the user to save them.
            DoneButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    RegisterUser( );
                };

            // On logout, make sure the user really wants to log out.
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // CONFIRM CANCEL
                    var actionSheet = new UIActionSheet( RegisterStrings.ConfirmCancelReg, null, GeneralStrings.Cancel, GeneralStrings.Yes, null );

                    actionSheet.ShowInView( View );

                    actionSheet.Clicked += (object s, UIButtonEventArgs ev) => 
                        {
                            if( ev.ButtonIndex == actionSheet.DestructiveButtonIndex )
                            {
                                Springboard.ResignModelViewController( this, null );
                            }
                        };
                };

            ResultView = new UIResultView( ScrollView, View.Frame.ToRectF( ), OnResultViewDone );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            HeaderView.Frame = new CGRect( View.Frame.Left, View.Frame.Top, View.Frame.Width, StyledTextField.StyledFieldHeight );

            ScrollView.Frame = new CGRect( View.Frame.Left, HeaderView.Frame.Bottom, View.Frame.Width, View.Frame.Height - HeaderView.Frame.Height );

            UserNameText.SetFrame( new CGRect( -10, View.Frame.Height * .05f, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            PasswordText.SetFrame( new CGRect( -10, UserNameText.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            ConfirmPasswordText.SetFrame( new CGRect( -10, PasswordText.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            NickNameText.SetFrame( new CGRect( -10, ConfirmPasswordText.Background.Frame.Bottom + 40, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            LastNameText.SetFrame( new CGRect( -10, NickNameText.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            EmailText.SetFrame( new CGRect( -10, LastNameText.Background.Frame.Bottom + 40, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );
            CellPhoneText.SetFrame( new CGRect( -10, EmailText.Background.Frame.Bottom, View.Frame.Width + 20, StyledTextField.StyledFieldHeight ) );

            DoneButton.Frame = new CGRect( View.Frame.Left + 10, CellPhoneText.Background.Frame.Bottom + 20, View.Bounds.Width - 20, ControlStyling.ButtonHeight );
            CancelButton.Frame = new CGRect( ( View.Frame.Width - ControlStyling.ButtonWidth) / 2, DoneButton.Frame.Bottom + 20, ControlStyling.ButtonWidth, ControlStyling.ButtonHeight );

            // for the scroll size, if the content is larger than the screen, we'll take the bottom
            // of the content plus some padding. Otherwise, we'll just use the window height plus a tiny bit so there's
            // a subtle scroll effect
            nfloat controlBottom = CancelButton.Frame.Bottom + ( View.Bounds.Height * .25f );
            ScrollView.ContentSize = new CGSize( 0, (nfloat) Math.Max( controlBottom, View.Bounds.Height * 1.05f ) );

            // setup the header shadow
            UIBezierPath shadowPath = UIBezierPath.FromRect( HeaderView.Bounds );
            HeaderView.Layer.MasksToBounds = false;
            HeaderView.Layer.ShadowColor = UIColor.Black.CGColor;
            HeaderView.Layer.ShadowOffset = new CGSize( 0.0f, .0f );
            HeaderView.Layer.ShadowOpacity = .23f;
            HeaderView.Layer.ShadowPath = shadowPath.CGPath;

            LogoView.Layer.Position = new CGPoint( HeaderView.Bounds.Width / 2, HeaderView.Bounds.Height / 2 );

            ResultView.SetBounds( View.Frame.ToRectF( ) );
            BlockerView.SetBounds( View.Frame.ToRectF( ) );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // reset the background colors
            UserNameText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            PasswordText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            ConfirmPasswordText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

            NickNameText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            LastNameText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            EmailText.Background.BackgroundColor = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

            ScrollView.ContentOffset = CGPoint.Empty;

            // set values
            UserNameText.Field.Text = string.Empty;

            PasswordText.Field.Text = string.Empty;
            ConfirmPasswordText.Field.Text = string.Empty;

            NickNameText.Field.Text = string.Empty;
            LastNameText.Field.Text = string.Empty;

            EmailText.Field.Text = string.Empty;

            // setup the phone number
            CellPhoneText.Field.Delegate = new Rock.Mobile.PlatformSpecific.iOS.UI.PhoneNumberFormatterDelegate();
            CellPhoneText.Field.Text = string.Empty;
            CellPhoneText.Field.Delegate.ShouldChangeCharacters( CellPhoneText.Field, new NSRange( CellPhoneText.Field.Text.Length, 0 ), "" );

            State = RegisterState.None;
            ResultView.Hide( );
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

        /// <summary>
        /// Ensure all required fields have data
        /// </summary>
        public bool ValidateInput( )
        {
            bool result = true;

            // validate there's text in all required fields
            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( UserNameText.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, UserNameText.Background );


            // for the password, if EITHER field is blank, that's not ok, OR if the passwords don't match, also not ok.
            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( (string.IsNullOrEmpty( PasswordText.Field.Text ) == true || string.IsNullOrEmpty( ConfirmPasswordText.Field.Text ) == true) ||
                ( PasswordText.Field.Text != ConfirmPasswordText.Field.Text ) )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, PasswordText.Background );
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, ConfirmPasswordText.Background );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( NickNameText.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, NickNameText.Background );


            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( LastNameText.Field.Text ) == true )
            {
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, LastNameText.Background );


            // cell phone OR email is fine
            targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( EmailText.Field.Text ) == true && string.IsNullOrEmpty( CellPhoneText.Field.Text ) == true )
            {
                // if failure, only color email
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            // otherwise, if they used email and didn't give it a proper format, they also fail.
            else if ( string.IsNullOrEmpty( EmailText.Field.Text ) == false && EmailText.Field.Text.IsEmailFormat( ) == false)
            {
                // if failure, only color email
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, EmailText.Background );
            Rock.Mobile.PlatformSpecific.iOS.UI.Util.AnimateViewColor( targetColor, CellPhoneText.Background );

            return result;
        }

        void RegisterUser()
        {
            if ( State == RegisterState.None )
            {
                // make sure they entered all required fields
                if ( ValidateInput( ) )
                {
                    BlockerView.Show( 
                        delegate
                        {
                            // force the UI to scroll back up
                            ScrollView.ContentOffset = CGPoint.Empty;
                            ScrollView.ScrollEnabled = false;

                            State = RegisterState.Trying;

                            // create a new user and submit them
                            Rock.Client.Person newPerson = new Rock.Client.Person();
                            Rock.Client.PhoneNumber newPhoneNumber = null;

                            // copy all the edited fields into the person object
                            newPerson.Email = EmailText.Field.Text;

                            newPerson.NickName = NickNameText.Field.Text;
                            newPerson.LastName = LastNameText.Field.Text;
                            newPerson.ConnectionStatusValueId = PrivateGeneralConfig.PersonConnectionStatusValueId;
                            newPerson.RecordStatusValueId = PrivateGeneralConfig.PersonRecordStatusValueId;

                            // Update their cell phone. 
                            if ( string.IsNullOrEmpty( CellPhoneText.Field.Text ) == false )
                            {
                                // update the phone number
                                string digits = CellPhoneText.Field.Text.AsNumeric( );
                                newPhoneNumber = new Rock.Client.PhoneNumber();
                                newPhoneNumber.Number = digits;
                                newPhoneNumber.NumberFormatted = digits.AsPhoneNumber( );
                            }

                            RockApi.Instance.RegisterNewUser( newPerson, newPhoneNumber, UserNameText.Field.Text, PasswordText.Field.Text,
                                delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                                {
                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                    {
                                        ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Register );

                                        State = RegisterState.Success;
                                        ResultView.Show( RegisterStrings.RegisterStatus_Success, 
                                            PrivateControlStylingConfig.Result_Symbol_Success, 
                                            RegisterStrings.RegisterResult_Success,
                                            GeneralStrings.Done );
                                    }
                                    else
                                    {
                                        State = RegisterState.Fail;
                                        ResultView.Show( RegisterStrings.RegisterStatus_Failed, 
                                            PrivateControlStylingConfig.Result_Symbol_Failed, 
                                            statusDescription == RegisterStrings.RegisterResult_BadLogin ? RegisterStrings.RegisterResult_LoginUsed : RegisterStrings.RegisterResult_Failed,
                                            GeneralStrings.Done );
                                    }

                                    BlockerView.Hide( null );
                                } );
                        } );
                }
            }
        }

        void OnResultViewDone( )
        {
            switch ( State )
            {
                case RegisterState.Success:
                {
                    Springboard.ResignModelViewController( this, null );
                    ScrollView.ScrollEnabled = true;
                    State = RegisterState.None;
                    break;
                }

                case RegisterState.Fail:
                {
                    ResultView.Hide( );
                    ScrollView.ScrollEnabled = true;
                    State = RegisterState.None;
                    break;
                }
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ResultView.Hide( );
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded( touches, evt );

            // if they tap somewhere outside of the text fields, 
            // hide the keyboard
            TextFieldShouldReturn( UserNameText.Field );
            TextFieldShouldReturn( PasswordText.Field );
            TextFieldShouldReturn( ConfirmPasswordText.Field );

            TextFieldShouldReturn( NickNameText.Field );
            TextFieldShouldReturn( LastNameText.Field );

            TextFieldShouldReturn( CellPhoneText.Field );
            TextFieldShouldReturn( EmailText.Field );
        }
    }
}
