
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
using Android.Widget;
using App.Shared.Network;
using Android.Graphics;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Strings;
using App.Shared.Config;
using App.Shared.Analytics;
using Rock.Mobile.Animation;
using App.Shared;
using Rock.Mobile.IO;
using App.Shared.UI;
using Droid.Tasks.Notes;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class SpinnerArrayAdapter : ArrayAdapter
            {
                int ResourceId { get; set; }
                public SpinnerArrayAdapter( Context context, int resourceId ) : base( context, resourceId )
                {
                    ResourceId = resourceId;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    if ( convertView as TextView == null )
                    {
                        convertView = ( Context as Activity ).LayoutInflater.Inflate( ResourceId, parent, false );
                        ControlStyling.StyleUILabel( (convertView as TextView), ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    }

                    ( convertView as TextView ).Text = this.GetItem( position ).ToString( );

                    return convertView;
                }
            }

            public class PrayerCreateFragment : TaskFragment
            {
                EditText FirstNameText { get; set; }
                RelativeLayout FirstNameBGLayer { get; set; }
                uint FirstNameBGColor { get; set; }

                EditText LastNameText { get; set; }
                RelativeLayout LastNameBGLayer { get; set; }
                uint LastNameBGColor { get; set; }

                EditText RequestText { get; set; }
                RelativeLayout RequestBGLayer { get; set; }
                uint RequestBGColor { get; set; }

                Switch AnonymousSwitch { get; set; }
                Switch PublicSwitch { get; set; }

                Spinner Spinner { get; set; }

                public PrayerCreateFragment(  ) : base( )
                {
                }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Prayer_Create, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // setup the first name background
                    FirstNameBGLayer = view.FindViewById<RelativeLayout>( Resource.Id.first_name_background );
                    ControlStyling.StyleBGLayer( FirstNameBGLayer );
                    //

                    LastNameBGLayer = view.FindViewById<RelativeLayout>( Resource.Id.last_name_background );
                    ControlStyling.StyleBGLayer( LastNameBGLayer );

                    // setup the prayer request background
                    RequestBGLayer = view.FindViewById<RelativeLayout>( Resource.Id.prayerRequest_background );
                    ControlStyling.StyleBGLayer( RequestBGLayer );
                    //

                    // setup the switch background
                    RelativeLayout backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.switch_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the category background
                    backgroundLayout = view.FindViewById<RelativeLayout>( Resource.Id.spinner_background );
                    ControlStyling.StyleBGLayer( backgroundLayout );

                    // setup the text views
                    FirstNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_firstNameText );
                    ControlStyling.StyleTextField( FirstNameText, PrayerStrings.CreatePrayer_FirstNamePlaceholderText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    FirstNameBGColor = ControlStylingConfig.BG_Layer_Color;
                    FirstNameText.InputType |= Android.Text.InputTypes.TextFlagCapWords;

                    LastNameText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_lastNameText );
                    ControlStyling.StyleTextField( LastNameText, PrayerStrings.CreatePrayer_LastNamePlaceholderText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    LastNameBGColor = ControlStylingConfig.BG_Layer_Color;
                    LastNameText.InputType |= Android.Text.InputTypes.TextFlagCapWords;

                    RequestText = (EditText)view.FindViewById<EditText>( Resource.Id.prayer_create_requestText );
                    ControlStyling.StyleTextField( RequestText, PrayerStrings.CreatePrayer_PrayerRequest, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );
                    RequestBGColor = ControlStylingConfig.BG_Layer_Color;
                    RequestText.InputType |= Android.Text.InputTypes.TextFlagCapSentences;


                    AnonymousSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.postAnonymousSwitch );
                    AnonymousSwitch.Checked = false;
                    AnonymousSwitch.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e ) =>
                    {
                            if( AnonymousSwitch.Checked == false )
                            {
                                FirstNameText.Enabled = true;
                                LastNameText.Enabled = true;

                                FirstNameText.Text = string.Empty;
                                LastNameText.Text = string.Empty;
                            }
                            else
                            {
                                FirstNameText.Enabled = false;
                                LastNameText.Enabled = false;

                                FirstNameText.Text = PrayerStrings.CreatePrayer_Anonymous;
                                LastNameText.Text = PrayerStrings.CreatePrayer_Anonymous;
                            }

                            // set the colors back to neutral
                            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( FirstNameBGColor, ControlStylingConfig.BG_Layer_Color, FirstNameBGLayer, delegate { FirstNameBGColor = ControlStylingConfig.BG_Layer_Color; } );
                            Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( LastNameBGColor, ControlStylingConfig.BG_Layer_Color, LastNameBGLayer, delegate { LastNameBGColor = ControlStylingConfig.BG_Layer_Color; } );
                    };

                    PublicSwitch = (Switch)view.FindViewById<Switch>( Resource.Id.makePublicSwitch );
                    PublicSwitch.Checked = true;

                    TextView postAnonymousLabel = view.FindViewById<TextView>( Resource.Id.postAnonymous );
                    ControlStyling.StyleUILabel( postAnonymousLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

                    TextView publicLabel = view.FindViewById<TextView>( Resource.Id.makePublic );
                    ControlStyling.StyleUILabel( publicLabel, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

                    // setup our category spinner
                    Spinner = (Spinner)view.FindViewById<Spinner>( Resource.Id.categorySpinner );
                    ArrayAdapter adapter = new SpinnerArrayAdapter( Rock.Mobile.PlatformSpecific.Android.Core.Context, Android.Resource.Layout.SimpleListItem1 );
                    adapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );
                    Spinner.Adapter = adapter;

                    // populate the category
                    foreach ( Rock.Client.Category category in App.Shared.Network.RockGeneralData.Instance.Data.PrayerCategories )
                    {
                        adapter.Add( category.Name );
                    }

                    Button submitButton = (Button)view.FindViewById<Button>( Resource.Id.prayer_create_submitButton );
                    ControlStyling.StyleButton( submitButton, PrayerStrings.CreatePrayer_SubmitButtonText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Small_FontSize );
                    submitButton.Click += (object sender, EventArgs e ) =>
                    {
                        SubmitPrayerRequest( );
                    };

                    return view;
                }

                void SubmitPrayerRequest( )
                {
                    // if first and last name are valid, OR anonymous is on
                    // and if there's text in the request field.
                    if ( ValidateInput( ) )
                    {
                        Rock.Client.PrayerRequest prayerRequest = new Rock.Client.PrayerRequest();

                        FirstNameText.Enabled = false;
                        LastNameText.Enabled = false;
                        RequestText.Enabled = false;

                        // setup the request
                        prayerRequest.FirstName = FirstNameText.Text;
                        prayerRequest.LastName = LastNameText.Text;

                        int? personAliasId = null;
                        if ( App.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId.HasValue )
                        {
                            personAliasId = App.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId;
                        }

                        prayerRequest.Text = RequestText.Text;
                        prayerRequest.EnteredDateTime = DateTime.Now;
                        prayerRequest.ExpirationDate = DateTime.Now.AddYears( 1 );
                        prayerRequest.CategoryId = App.Shared.Network.RockGeneralData.Instance.Data.PrayerCategoryToId( Spinner.SelectedItem.ToString( ) );
                        prayerRequest.IsActive = true;
                        prayerRequest.Guid = Guid.NewGuid( );
                        prayerRequest.IsPublic = PublicSwitch.Checked;
                        prayerRequest.IsApproved = false;
                        prayerRequest.CreatedByPersonAliasId = AnonymousSwitch.Checked == true ? null : personAliasId;

                        ParentTask.OnClick( this, 0, prayerRequest );
                    }
                    else
                    {
                        CheckDebug( );
                    }
                }

                bool ValidateInput( )
                {
                    bool result = true;

                    // Update the name background color
                    uint targetNameColor = ControlStylingConfig.BG_Layer_Color;
                    if( string.IsNullOrEmpty( FirstNameText.Text ) && AnonymousSwitch.Checked == false )
                    {
                        targetNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                        result = false;
                    }
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( FirstNameBGColor, targetNameColor, FirstNameBGLayer, delegate { FirstNameBGColor = targetNameColor; } );


                    // if they left the name field blank and didn't turn on Anonymous, flag the field.
                    uint targetLastNameColor = ControlStylingConfig.BG_Layer_Color; 
                    if( string.IsNullOrEmpty( LastNameText.Text ) && AnonymousSwitch.Checked == false )
                    {
                        targetLastNameColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                        result = false;
                    }
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( LastNameBGColor, targetLastNameColor, LastNameBGLayer, delegate { LastNameBGColor = targetLastNameColor; } );


                    // Update the prayer background color
                    uint targetPrayerColor = ControlStylingConfig.BG_Layer_Color;
                    if( string.IsNullOrEmpty( RequestText.Text ) == true )
                    {
                        targetPrayerColor = ControlStylingConfig.BadInput_BG_Layer_Color;
                        result = false;
                    }
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( RequestBGColor, targetPrayerColor, RequestBGLayer, delegate { RequestBGColor = targetPrayerColor; } );

                    return result;
                }

                void CheckDebug( )
                {
                    if( string.IsNullOrEmpty( FirstNameText.Text ) == true && string.IsNullOrEmpty( LastNameText.Text ) == true )
                    {
                        if ( RequestText.Text.ToLower( ) == "clear cache" )
                        {
                            FileCache.Instance.CleanUp( true );
                            Springboard.DisplayError( "Cache Cleared", "All cached items have been deleted" );
                        }
                        else if ( RequestText.Text.ToLower( ) == "developer" )
                        {
                            App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled = !App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled;
                            Springboard.DisplayError( "Developer Mode", 
                                string.Format( "Developer Mode has been toggled: {0}", App.Shared.Network.RockGeneralData.Instance.Data.DeveloperModeEnabled == true ? "ON" : "OFF" ) );
                        }
                        else if ( RequestText.Text.ToLower( ) == "version" )
                        {
                            Springboard.DisplayError( "Current Version", BuildStrings.Version );
                        }
                        else if ( RequestText.Text.ToLower( ) == "upload dumps" )
                        {
                            Xamarin.Insights.PurgePendingCrashReports( ).Wait( );
                            Springboard.DisplayError( "Crash Dumps Sent", "Just uploaded all pending crash dumps." );
                        }
                        else
                        {
                            UISpecial.Trigger( RequestText.Text.ToLower( ), View, this, ParentTask, null );
                        }
                    }
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // if they're logged in, pre-populate the name fields
                    if ( RockMobileUser.Instance.LoggedIn == true )
                    {
                        FirstNameText.Text = RockMobileUser.Instance.Person.NickName;
                        LastNameText.Text = RockMobileUser.Instance.Person.LastName;
                    }
                }
            }
        }
    }
}
