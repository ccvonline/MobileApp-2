
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
using App.Shared.Analytics;

namespace Droid
{
    public class ProfileFragment : Fragment, DatePicker.IOnDateChangedListener
    {
        public Springboard SpringboardParent { get; set; }

        EditText NickNameField { get; set; }
        EditText LastNameField { get; set; }

        EditText CellPhoneField { get; set; }

        EditText EmailField { get; set; }
        RelativeLayout EmailLayer { get; set; }
        uint EmailBGColor { get; set; }

        EditText StreetField { get; set; }
        EditText CityField { get; set; }
        EditText StateField { get; set; }
        EditText ZipField { get; set; }

        EditText BirthdateField { get; set; }
        EditText GenderField { get; set; }
        EditText CampusField { get; set; }

        Button DoneButton { get; set; }
        Button LogoutButton { get; set; }

        bool Dirty { get; set; }


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

            View view = inflater.Inflate(Resource.Layout.Profile, container, false);
            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            RelativeLayout navBar = view.FindViewById<RelativeLayout>( Resource.Id.navbar_relative_layout );
            navBar.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            // setup the name section
            RelativeLayout backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.name_background );
            ControlStyling.StyleBGLayer( backgroundView );

            NickNameField = backgroundView.FindViewById<EditText>( Resource.Id.nickNameText );
            ControlStyling.StyleTextField( NickNameField, ProfileStrings.NickNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            NickNameField.AfterTextChanged += (sender, e) => { Dirty = true; };
            NickNameField.InputType |= InputTypes.TextFlagCapWords;

            View borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            LastNameField = backgroundView.FindViewById<EditText>( Resource.Id.lastNameText );
            ControlStyling.StyleTextField( LastNameField, ProfileStrings.LastNamePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            LastNameField.AfterTextChanged += (sender, e) => { Dirty = true; };
            LastNameField.InputType |= InputTypes.TextFlagCapWords;


            // setup the contact section
            EmailLayer = view.FindViewById<RelativeLayout>( Resource.Id.email_background );
            ControlStyling.StyleBGLayer( EmailLayer );
            EmailBGColor = ControlStylingConfig.BG_Layer_Color;

            EmailField = EmailLayer.FindViewById<EditText>( Resource.Id.emailAddressText );
            ControlStyling.StyleTextField( EmailField, ProfileStrings.EmailPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            EmailField.AfterTextChanged += (sender, e) => { Dirty = true; };

            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.cellphone_background );
            ControlStyling.StyleBGLayer( backgroundView );

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            CellPhoneField = backgroundView.FindViewById<EditText>( Resource.Id.cellPhoneText );
            ControlStyling.StyleTextField( CellPhoneField, ProfileStrings.CellPhonePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            CellPhoneField.AfterTextChanged += (sender, e) => { Dirty = true; };
            CellPhoneField.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());


            // setup the address section
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.address_background );
            ControlStyling.StyleBGLayer( backgroundView );

            StreetField = backgroundView.FindViewById<EditText>( Resource.Id.streetAddressText );
            ControlStyling.StyleTextField( StreetField, ProfileStrings.StreetPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            StreetField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.street_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            CityField = backgroundView.FindViewById<EditText>( Resource.Id.cityAddressText );
            ControlStyling.StyleTextField( CityField, ProfileStrings.CityPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            CityField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.city_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            StateField = backgroundView.FindViewById<EditText>( Resource.Id.stateAddressText );
            ControlStyling.StyleTextField( StateField, ProfileStrings.StatePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            StateField.AfterTextChanged += (sender, e) => { Dirty = true; };

            borderView = backgroundView.FindViewById<View>( Resource.Id.state_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            ZipField = backgroundView.FindViewById<EditText>( Resource.Id.zipAddressText );
            ControlStyling.StyleTextField( ZipField, ProfileStrings.ZipPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            ZipField.AfterTextChanged += (sender, e) => { Dirty = true; };


            // personal
            backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.personal_background );
            ControlStyling.StyleBGLayer( backgroundView );

            BirthdateField = backgroundView.FindViewById<EditText>( Resource.Id.birthdateText );
            ControlStyling.StyleTextField( BirthdateField, ProfileStrings.BirthdatePlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            BirthdateField.FocusableInTouchMode = false;
            BirthdateField.Focusable = false;
            Button birthdateButton = backgroundView.FindViewById<Button>( Resource.Id.birthdateButton );
            birthdateButton.Click += (object sender, EventArgs e ) =>
            {
                    // setup the initial date to use ( either now, or the date in the field )
                    DateTime initialDateTime = DateTime.Now;
                    if( string.IsNullOrEmpty( BirthdateField.Text ) == false )
                    {
                        initialDateTime = DateTime.Parse( BirthdateField.Text );
                    }

                    // build our 
                    LayoutInflater dateInflate = LayoutInflater.From( Activity );
                    DatePicker newPicker = (DatePicker)dateInflate.Inflate( Resource.Layout.DatePicker, null );
                    newPicker.Init( initialDateTime.Year, initialDateTime.Month - 1, initialDateTime.Day, this );

                    Dialog dialog = new Dialog( Activity );
                    dialog.AddContentView( newPicker, new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent ) );
                    dialog.Show( );
            };

            borderView = backgroundView.FindViewById<View>( Resource.Id.middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            // Gender
            GenderField = view.FindViewById<EditText>( Resource.Id.genderText );
            ControlStyling.StyleTextField( GenderField, ProfileStrings.GenderPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            GenderField.FocusableInTouchMode = false;
            GenderField.Focusable = false;
            Button genderButton = backgroundView.FindViewById<Button>( Resource.Id.genderButton );
            genderButton.Click += (object sender, EventArgs e ) =>
            {
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                        {
                            new Java.Lang.String( RockGeneralData.Instance.Data.Genders[ 1 ] ),
                            new Java.Lang.String( RockGeneralData.Instance.Data.Genders[ 2 ] ),
                        };

                    builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    GenderField.Text = RockGeneralData.Instance.Data.Genders[ clickArgs.Which + 1 ];
                                    Dirty = true;
                                });
                        });

                    builder.Show( );
            };

            borderView = backgroundView.FindViewById<View>( Resource.Id.campus_middle_border );
            borderView.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            // Campus
            CampusField = view.FindViewById<EditText>( Resource.Id.campusText );
            ControlStyling.StyleTextField( CampusField, ProfileStrings.CampusPlaceholder, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
            CampusField.FocusableInTouchMode = false;
            CampusField.Focusable = false;
            Button campusButton = backgroundView.FindViewById<Button>( Resource.Id.campusButton );
            campusButton.Click += (object sender, EventArgs e ) =>
                {
                    // build an alert dialog containing all the campus choices
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( ProfileStrings.SelectCampus_SourceTitle );
                    Java.Lang.ICharSequence [] campusStrings = new Java.Lang.ICharSequence[ RockGeneralData.Instance.Data.Campuses.Count ];
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        campusStrings[ i ] = new Java.Lang.String( RockGeneralData.Instance.Data.Campuses[ i ].Name );
                    }

                    // launch the dialog, and on selection, update the viewing campus text.
                    builder.SetItems( campusStrings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    int campusIndex = clickArgs.Which;
                                    CampusField.Text = RockGeneralData.Instance.Data.Campuses[ campusIndex ].Name;
                                    Dirty = true;
                                });
                        });

                    builder.Show( );
                };


            // Done buttons
            DoneButton = view.FindViewById<Button>( Resource.Id.doneButton );
            ControlStyling.StyleButton( DoneButton, ProfileStrings.DoneButtonTitle, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );

            LogoutButton = view.FindViewById<Button>( Resource.Id.logoutButton );
            ControlStyling.StyleButton( LogoutButton, ProfileStrings.LogoutButtonTitle, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
            LogoutButton.Background = null;

            DoneButton.Click += (object sender, EventArgs e) => 
                {
                    if( Dirty == true )
                    {
                        if ( ValidateInput( ) )
                        {
                            // Since they made changes, confirm they want to save them.
                            AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                            builder.SetTitle( ProfileStrings.SubmitChangesTitle );

                            Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                                {
                                    new Java.Lang.String( GeneralStrings.Yes ),
                                    new Java.Lang.String( GeneralStrings.No ),
                                    new Java.Lang.String( GeneralStrings.Cancel )
                                };

                            builder.SetItems( strings, delegate(object s, DialogClickEventArgs clickArgs) 
                                {
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            switch( clickArgs.Which )
                                            {
                                                case 0: SubmitChanges( ); SpringboardParent.ModalFragmentDone( null ); break;
                                                case 1: SpringboardParent.ModalFragmentDone( null ); break;
                                                case 2: break;
                                            }
                                        });
                                });

                            builder.Show( );
                        }
                    }
                    else
                    {
                        SpringboardParent.ModalFragmentDone( null );
                    }
                };

            LogoutButton.Click += (object sender, EventArgs e) => 
                {
                    // Since they made changes, confirm they want to save them.
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    builder.SetTitle( ProfileStrings.LogoutTitle );

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
                                        case 0: RockMobileUser.Instance.LogoutAndUnbind( ); SpringboardParent.ModalFragmentDone( null ); break;
                                        case 1: break;
                                    }
                                });
                        });

                    builder.Show( );
                };

            return view;
        }

        bool ValidateInput( )
        {
            bool result = true;

            // the only one we really care about is email, to ensure they put a valid address
            uint targetColor = ControlStylingConfig.BG_Layer_Color;
            if ( string.IsNullOrEmpty( EmailField.Text ) == false && EmailField.Text.IsEmailFormat( ) == false )
            {
                // if failure, only color email
                targetColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                result = false;
            }

            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( EmailBGColor, targetColor, EmailLayer, delegate { EmailBGColor = targetColor; } );

            return result;
        }

        public void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    BirthdateField.Text = string.Format( "{0:MMMMM dd yyyy}", new DateTime( year, monthOfYear + 1, dayOfMonth ) );
                    Dirty = true;
                });
        }

        public override void OnResume()
        {
            base.OnResume();

            // logged in sanity check.
            if( RockMobileUser.Instance.LoggedIn == false ) throw new Exception("A user must be logged in before viewing a profile. How did you do this?" );

            NickNameField.Text = RockMobileUser.Instance.Person.NickName;
            LastNameField.Text = RockMobileUser.Instance.Person.LastName;

            EmailField.Text = RockMobileUser.Instance.Person.Email;

            // cellphone
            CellPhoneField.Text = RockMobileUser.Instance.CellPhoneNumberDigits( );

            // address
            StreetField.Text = RockMobileUser.Instance.Street1( );
            CityField.Text = RockMobileUser.Instance.City( );
            StateField.Text = RockMobileUser.Instance.State( );
            ZipField.Text = RockMobileUser.Instance.Zip( );

            // gender
            if ( RockMobileUser.Instance.Person.Gender > 0 )
            {
                GenderField.Text = RockGeneralData.Instance.Data.Genders[ RockMobileUser.Instance.Person.Gender ];
            }
            else
            {
                GenderField.Text = string.Empty;
            }

            // birthdate
            if ( RockMobileUser.Instance.Person.BirthDate.HasValue == true )
            {
                BirthdateField.Text = string.Format( "{0:MMMMM dd yyyy}", RockMobileUser.Instance.Person.BirthDate );
            }
            else
            {
                BirthdateField.Text = string.Empty;
            }

            // campus
            if ( RockMobileUser.Instance.PrimaryFamily.CampusId.HasValue == true )
            {
                CampusField.Text = RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.PrimaryFamily.CampusId.Value );
            }
            else
            {
                CampusField.Text = string.Empty;
            }

            // clear the dirty flag AFTER setting all values so the initial setup
            // doesn't get flagged as dirty
            Dirty = false;

            SpringboardParent.ModalFragmentOpened( this );
        }

        public override void OnStop()
        {
            base.OnStop();

            SpringboardParent.ModalFragmentDone( null );
        }

        void SubmitChanges()
        {
            // copy all the edited fields into the person object
            RockMobileUser.Instance.Person.Email = EmailField.Text;

            RockMobileUser.Instance.Person.NickName = NickNameField.Text;
            RockMobileUser.Instance.Person.LastName = LastNameField.Text;

            // Update their cell phone. 
            if ( string.IsNullOrEmpty( CellPhoneField.Text ) == false )
            {
                // update the phone number
                RockMobileUser.Instance.SetPhoneNumberDigits( CellPhoneField.Text );
            }

            // Gender
            if ( string.IsNullOrEmpty( GenderField.Text ) == false )
            {
                RockMobileUser.Instance.Person.Gender = RockGeneralData.Instance.Data.Genders.IndexOf( GenderField.Text );
            }

            // Birthdate
            if ( string.IsNullOrEmpty( BirthdateField.Text ) == false )
            {
                RockMobileUser.Instance.SetBirthday( DateTime.Parse( BirthdateField.Text ) );
            }

            // Campus
            if ( string.IsNullOrEmpty( CampusField.Text ) == false )
            {
                RockMobileUser.Instance.PrimaryFamily.CampusId = RockGeneralData.Instance.Data.CampusNameToId( CampusField.Text );
                RockMobileUser.Instance.ViewingCampus = RockMobileUser.Instance.PrimaryFamily.CampusId.Value;
            }

            if ( string.IsNullOrEmpty( StreetField.Text ) == false &&
                 string.IsNullOrEmpty( CityField.Text ) == false &&
                 string.IsNullOrEmpty( StateField.Text ) == false &&
                 string.IsNullOrEmpty( ZipField.Text ) == false )
            {
                RockMobileUser.Instance.SetAddress( StreetField.Text, CityField.Text, StateField.Text, ZipField.Text );
            }

            // request the person object be sync'd with the server. because we save the object locally,
            // if the sync fails, the profile will try again at the next login
            RockMobileUser.Instance.UpdateProfile( null );
            RockMobileUser.Instance.UpdateAddress( null );
            RockMobileUser.Instance.UpdateHomeCampus( null );
            RockMobileUser.Instance.UpdateOrAddPhoneNumber( null );

            ProfileAnalytic.Instance.Trigger( ProfileAnalytic.Update );
        }
    }
}

