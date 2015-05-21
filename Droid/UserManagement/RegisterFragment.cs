
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Text;
using Android.Widget;
using App.Shared.Network;
using Android.Views.InputMethods;
using App.Shared.Strings;
using App.Shared.Config;
using Rock.Mobile.UI;
using Android.Telephony;
using Rock.Mobile.Util.Strings;
using Java.Lang.Reflect;
using App.Shared.UI;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;

namespace Droid
{
    public class RegisterFragment : Fragment, View.IOnTouchListener
    {
        public Springboard SpringboardParent { get; set; }

        View UserNameLayer { get; set; }
        EditText UserNameText { get; set; }
        uint UserNameBGColor { get; set; }

        View PasswordLayer { get; set; }
        EditText PasswordText { get; set; }
        uint PasswordBGColor { get; set; }

        View ConfirmPasswordLayer { get; set; }
        EditText ConfirmPasswordText { get; set; }
        uint ConfirmPasswordBGColor { get; set; }

        View NickNameLayer { get; set; }
        EditText NickNameText { get; set; }
        uint NickNameBGColor { get; set; }

        View LastNameLayer { get; set; }
        EditText LastNameText { get; set; }
        uint LastNameBGColor { get; set; }

        View EmailLayer { get; set; }
        EditText EmailText { get; set; }
        uint EmailBGColor { get; set; }

        View CellPhoneLayer { get; set; }
        EditText CellPhoneText { get; set; }

        Button RegisterButton { get; set; }
        Button CancelButton { get; set; }

        enum RegisterState
        {
            None,
            Trying,
            Success,
            Fail
        }

        RegisterState State { get; set; }

        UIResultView ResultView { get; set; }

        RelativeLayout ProgressBarBlocker { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            View view = inflater.Inflate(Resource.Layout.Register, container, false);
            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
            view.SetOnTouchListener( this );

            RelativeLayout layoutView = view.FindViewById<RelativeLayout>( Resource.Id.scroll_linear_background );

            ProgressBarBlocker = view.FindViewById<RelativeLayout>( Resource.Id.progressBarBlocker );
            ProgressBarBlocker.Visibility = ViewStates.Gone;
            ProgressBarBlocker.LayoutParameters = new RelativeLayout.LayoutParams( 0, 0 );
            ProgressBarBlocker.LayoutParameters.Width = NavbarFragment.GetFullDisplayWidth( );
            ProgressBarBlocker.LayoutParameters.Height = this.Resources.DisplayMetrics.HeightPixels;

            ResultView = new UIResultView( layoutView, new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ), OnResultViewDone );

            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );


            // setup the username 
            UserNameLayer = view.FindViewById<RelativeLayout>( Resource.Id.username_background );
            ControlStyling.StyleBGLayer( UserNameLayer );

            UserNameText = UserNameLayer.FindViewById<EditText>( Resource.Id.userNameText );
            ControlStyling.StyleTextField( UserNameText, RegisterStrings.UsernamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            UserNameBGColor = ControlStylingConfig.BG_Layer_Color;
            UserNameText.InputType |= InputTypes.TextFlagCapWords;

            View borderView = UserNameLayer.FindViewById<View>( Resource.Id.username_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            // password
            PasswordLayer = view.FindViewById<RelativeLayout>( Resource.Id.password_background );
            ControlStyling.StyleBGLayer( PasswordLayer );

            PasswordText = PasswordLayer.FindViewById<EditText>( Resource.Id.passwordText );
            PasswordText.InputType |= InputTypes.TextVariationPassword;
            ControlStyling.StyleTextField( PasswordText, RegisterStrings.PasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            PasswordBGColor = ControlStylingConfig.BG_Layer_Color;

            borderView = PasswordLayer.FindViewById<View>( Resource.Id.password_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );



            ConfirmPasswordLayer = view.FindViewById<RelativeLayout>( Resource.Id.confirmPassword_background );
            ControlStyling.StyleBGLayer( ConfirmPasswordLayer );

            ConfirmPasswordText = ConfirmPasswordLayer.FindViewById<EditText>( Resource.Id.confirmPasswordText );
            ConfirmPasswordText.InputType |= InputTypes.TextVariationPassword;
            ControlStyling.StyleTextField( ConfirmPasswordText, RegisterStrings.ConfirmPasswordPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ConfirmPasswordBGColor = ControlStylingConfig.BG_Layer_Color;



            // setup the name section
            NickNameLayer = view.FindViewById<RelativeLayout>( Resource.Id.firstname_background );
            ControlStyling.StyleBGLayer( NickNameLayer );

            NickNameText = NickNameLayer.FindViewById<EditText>( Resource.Id.nickNameText );
            ControlStyling.StyleTextField( NickNameText, RegisterStrings.NickNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            NickNameBGColor = ControlStylingConfig.BG_Layer_Color;
            NickNameText.InputType |= InputTypes.TextFlagCapWords;

            borderView = NickNameLayer.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            LastNameLayer = view.FindViewById<RelativeLayout>( Resource.Id.lastname_background );
            ControlStyling.StyleBGLayer( LastNameLayer );

            LastNameText = LastNameLayer.FindViewById<EditText>( Resource.Id.lastNameText );
            ControlStyling.StyleTextField( LastNameText, RegisterStrings.LastNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            LastNameBGColor = ControlStylingConfig.BG_Layer_Color;
            LastNameText.InputType |= InputTypes.TextFlagCapWords;


            // setup the cell phone section
            CellPhoneLayer = view.FindViewById<RelativeLayout>( Resource.Id.cellphone_background );
            ControlStyling.StyleBGLayer( CellPhoneLayer );

            CellPhoneText = CellPhoneLayer.FindViewById<EditText>( Resource.Id.cellPhoneText );
            ControlStyling.StyleTextField( CellPhoneText, RegisterStrings.CellPhonePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            CellPhoneText.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());


            // email layer
            EmailLayer = view.FindViewById<RelativeLayout>( Resource.Id.email_background );
            ControlStyling.StyleBGLayer( EmailLayer );

            borderView = EmailLayer.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            EmailText = EmailLayer.FindViewById<EditText>( Resource.Id.emailAddressText );
            ControlStyling.StyleTextField( EmailText, RegisterStrings.EmailPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            EmailBGColor = ControlStylingConfig.BG_Layer_Color;


            // Register button
            RegisterButton = view.FindViewById<Button>( Resource.Id.registerButton );
            ControlStyling.StyleButton( RegisterButton, RegisterStrings.RegisterButton, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );

            CancelButton = view.FindViewById<Button>( Resource.Id.cancelButton );
            ControlStyling.StyleButton( CancelButton, GeneralStrings.Cancel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            CancelButton.Background = null;

            RegisterButton.Click += (object sender, EventArgs e) => 
                {
                    RegisterUser( );
                };

            CancelButton.Click += (object sender, EventArgs e) => 
                {
                    // Since they made changes, confirm they want to save them.
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( RegisterStrings.ConfirmCancelReg );

                    Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                        {
                            new Java.Lang.String( GeneralStrings.Yes ),
                            new Java.Lang.String( GeneralStrings.No )
                        };

                    builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    switch( clickArgs.Which )
                                    {
                                        case 0: SpringboardParent.ModalFragmentDone( null ); break;
                                        case 1: break;
                                    }
                                });
                        });

                    builder.Show( );
                };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            // logged in sanity check.
            //if( RockMobileUser.Instance.LoggedIn == true ) throw new Exception("A user cannot be logged in when registering. How did you do this?" );

            UserNameText.Text = string.Empty;
            PasswordText.Text = string.Empty;
            ConfirmPasswordText.Text = string.Empty;

            NickNameText.Text = string.Empty;
            LastNameText.Text = string.Empty;

            EmailText.Text = string.Empty;
            CellPhoneText.Text = string.Empty;

            SpringboardParent.ModalFragmentOpened( this );
        }

        public override void OnStop()
        {
            base.OnStop();

            SpringboardParent.ModalFragmentDone( null );

            State = RegisterState.None;
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            // consume all input so things tasks underneath don't respond
            return true;
        }

        void ToggleControls( bool enabled )
        {
            UserNameText.Enabled = enabled;
            PasswordText.Enabled = enabled;
            ConfirmPasswordText.Enabled = enabled;

            NickNameText.Enabled = enabled;
            LastNameText.Enabled = enabled;

            EmailText.Enabled = enabled;
            CellPhoneText.Enabled = enabled;
            RegisterButton.Enabled = enabled;
            CancelButton.Enabled = enabled;
        }

        void RegisterUser()
        {
            if ( State == RegisterState.None )
            {
                // make sure they entered all required fields
                if ( ValidateInput( ) )
                {
                    ToggleControls( false );

                    ProgressBarBlocker.Visibility = ViewStates.Visible;
                    State = RegisterState.Trying;

                    // create a new user and submit them
                    Rock.Client.Person newPerson = new Rock.Client.Person();
                    Rock.Client.PhoneNumber newPhoneNumber = null;

                    // copy all the edited fields into the person object
                    newPerson.Email = EmailText.Text;

                    newPerson.NickName = NickNameText.Text;
                    newPerson.LastName = LastNameText.Text;
                    newPerson.ConnectionStatusValueId = PrivateGeneralConfig.PersonConnectionStatusValueId;
                    newPerson.RecordStatusValueId = PrivateGeneralConfig.PersonRecordStatusValueId;

                    // Update their cell phone. 
                    if ( string.IsNullOrEmpty( CellPhoneText.Text ) == false )
                    {
                        // update the phone number
                        string digits = CellPhoneText.Text.AsNumeric( );
                        newPhoneNumber = new Rock.Client.PhoneNumber();
                        newPhoneNumber.Number = digits;
                        newPhoneNumber.NumberFormatted = digits.AsPhoneNumber( );
                        newPhoneNumber.NumberTypeValueId = PrivateGeneralConfig.CellPhoneValueId;
                    }

                    RockApi.Instance.RegisterNewUser( newPerson, newPhoneNumber, UserNameText.Text, PasswordText.Text,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            ProgressBarBlocker.Visibility = ViewStates.Gone;
                             ToggleControls( true );

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

                            ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
                        } );
                }
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            ProgressBarBlocker.LayoutParameters.Width = NavbarFragment.GetFullDisplayWidth( );
            ResultView.SetBounds( new System.Drawing.RectangleF( 0, 0, NavbarFragment.GetFullDisplayWidth( ), this.Resources.DisplayMetrics.HeightPixels ) );
        }

        /// <summary>
        /// Ensure all required fields have data
        /// </summary>
        public bool ValidateInput( )
        {
            bool result = true;

            // validate there's text in all required fields
            uint userNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( UserNameText.Text ) == true )
            {
                userNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( UserNameBGColor, userNameTargetColor, UserNameLayer, delegate { UserNameBGColor = userNameTargetColor; } );

            // for the password, if EITHER field is blank, that's not ok, OR if the passwords don't match, also not ok.
            uint passwordTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( (string.IsNullOrEmpty( PasswordText.Text ) == true || string.IsNullOrEmpty( ConfirmPasswordText.Text ) == true) ||
                 (PasswordText.Text != ConfirmPasswordText.Text) )
            {
                passwordTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( PasswordBGColor, passwordTargetColor, PasswordLayer, delegate { PasswordBGColor = passwordTargetColor; } );
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( ConfirmPasswordBGColor, passwordTargetColor, ConfirmPasswordLayer, delegate { ConfirmPasswordBGColor = passwordTargetColor; } );


            uint nickNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( NickNameText.Text ) == true )
            {
                nickNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( NickNameBGColor, nickNameTargetColor, NickNameLayer, delegate { NickNameBGColor = nickNameTargetColor; } );


            uint lastNameTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( LastNameText.Text ) == true )
            {
                lastNameTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( LastNameBGColor, lastNameTargetColor, LastNameLayer, delegate { LastNameBGColor = lastNameTargetColor; } );

            // cell phone OR email is fine
            uint emailTargetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( EmailText.Text ) == true && string.IsNullOrEmpty( CellPhoneText.Text ) == true )
            {
                emailTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }
            // otherwise, if they used email and didn't give it a proper format, they also fail.
            else if ( string.IsNullOrEmpty( EmailText.Text ) == false && EmailText.Text.IsEmailFormat( ) == false )
            {
                emailTargetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }

            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( EmailBGColor, emailTargetColor, CellPhoneLayer, delegate { EmailBGColor = emailTargetColor; } );
            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( EmailBGColor, emailTargetColor, EmailLayer, delegate { EmailBGColor = emailTargetColor; } );


            return result;
        }

        void OnResultViewDone( )
        {
            switch ( State )
            {
                case RegisterState.Success:
                {
                    SpringboardParent.ModalFragmentDone( null );
                    State = RegisterState.None;
                    break;
                }

                case RegisterState.Fail:
                {
                    ResultView.Hide( );
                    State = RegisterState.None;
                    break;
                }
            }
        }

    }
}

