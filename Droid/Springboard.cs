
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
using Java.IO;
using Droid.Tasks;
using System.IO;
using App.Shared;
using App.Shared.Config;
using App.Shared.Strings;
using Rock.Mobile.UI;
using Android.Graphics.Drawables;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Rock.Mobile.PlatformSpecific.Android.UI;
using Rock.Mobile.Animation;
using Droid.Tasks.Give;
using App.Shared.Analytics;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;


namespace Droid
{
    class SpringboardElement
    {
        public Tasks.Task Task { get; set; }

        RelativeLayout Layout { get; set; }
        int LayoutId { get; set; }

        public Button Button { get; set; }

        TextView Icon { get; set; }
        string IconStr { get; set; }

        string ElementLabel { get; set; }
        TextView Text { get; set; }

        public SpringboardElement( Tasks.Task task, int layoutId, string iconStr, string elementLabel )
        {
            Task = task;
            LayoutId = layoutId;
            IconStr = iconStr;
            ElementLabel = elementLabel;
        }

        public void OnCreateView( View parentView )
        {
            Layout = parentView.FindViewById<RelativeLayout>( LayoutId );
            Icon = Layout.FindViewById<TextView>( Resource.Id.icon );
            Button = Layout.FindViewById<Button>( Resource.Id.button );
            Text = Layout.FindViewById<TextView>( Resource.Id.text );

            Typeface fontFace = FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Primary );
            Icon.SetTypeface( fontFace, TypefaceStyle.Normal );
            Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSpringboardConfig.Element_FontSize );
            Icon.SetX( Icon.GetX() - Icon.Width / 2 );
            Icon.Text = IconStr;

            Text.Text = ElementLabel;
            Text.SetTypeface( FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
            Text.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );

            Button.Background = null;

            // setup the seperator color
            View seperator = Layout.FindViewById<View>( Resource.Id.seperator );
            seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
        }

        public void Deactivate( )
        {
            Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );

            Text.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );

            Layout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( 0x00000000 ) );
        }

        public void Activate( )
        {
            Icon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementTextColor ) );

            Text.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementTextColor ) );

            Layout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_Element_SelectedColor ) );
        }
    }

    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
    public class Springboard : Fragment, View.IOnTouchListener
    {
        List<SpringboardElement> Elements { get; set; }

        /// <summary>
        /// The top navigation bar that acts as the container for Tasks
        /// </summary>
        /// <value>The navbar fragment.</value>
        protected NavbarFragment NavbarFragment { get; set; }
        protected LoginFragment LoginFragment { get; set; }
        protected ProfileFragment ProfileFragment { get; set; }
        protected RegisterFragment RegisterFragment { get; set; }
        protected ImageCropFragment ImageCropFragment { get; set; }
        protected OOBEFragment OOBEFragment { get; set; }
        protected SplashFragment SplashFragment { get; set; }

        protected Button ProfileImageButton { get; set; }

        protected Button LoginProfileButton { get; set; }
        protected TextView ProfileName { get; set; }
        protected TextView ViewProfileLabel { get; set; }

        protected int ActiveElementIndex { get; set; }

        //bool DisplayingModalFragment { get; set; }
        Fragment VisibleModalFragment { get; set; }

        bool AppInForeground { get; set; }

        /// <summary>
        /// Get a pointer to the Fullscreen layout so we can hide it when a modal fragment isn't displaying
        /// </summary>
        /// <value>The full screen layout.</value>
        protected FrameLayout FullScreenLayout { get; set; }

        /// <summary>
        /// When true, we need to launch the image cropper. We have to wait
        /// until the NavBar and all sub-fragments have been pushed to the stack.
        /// </summary>
        /// <value><c>true</c> if image cropper pending launch; otherwise, <c>false</c>.</value>
        string ImageCropperPendingFilePath { get; set; }

        Bitmap ProfileMask { get; set; }

        Bitmap ProfileMaskedImage { get; set; }

        TextView ProfilePrefix { get; set; }

        TextView CampusText { get; set; }

        NotificationBillboard Billboard { get; set; }

        LinearLayout ProfileContainer { get; set; }

        View CampusContainer { get; set; }

        /// <summary>
        /// True when series info is downloaded and it's safe to start downloading other stuff.
        /// </summary>
        bool SeriesInfoDownloaded { get; set; }

        /// <summary>
        /// Allows fragments to control whether the device back button will work or not.
        /// </summary>
        /// <value><c>true</c> if enable back; otherwise, <c>false</c>.</value>
        public bool EnableBack { get; set; }

        /// <summary>
        /// Stores the time of the last rock sync.
        /// If the user has left our app running > 24 hours we'll redownload
        /// </summary>
        /// <value>The last rock sync.</value>
        DateTime LastRockSync { get; set; }

        /// <summary>
        /// True if we've run the splash intro.
        /// </summary>
        bool IsSplashDone { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            EnableBack = true;

            RetainInstance = true;

            // setup our fragments (checking for these to be created might be unnecessary, since we'll retain this fragment)
            NavbarFragment = new NavbarFragment( );
            NavbarFragment.SpringboardParent = this;

            LoginFragment = new LoginFragment( );
            LoginFragment.SpringboardParent = this;

            ProfileFragment = new ProfileFragment( );
            ProfileFragment.SpringboardParent = this;

            RegisterFragment = new RegisterFragment( );
            RegisterFragment.SpringboardParent = this;

            OOBEFragment = new OOBEFragment( );
            OOBEFragment.SpringboardParent = this;

            SplashFragment = new SplashFragment( );
            SplashFragment.SpringboardParent = this;

            ImageCropFragment = new ImageCropFragment( );
            ImageCropFragment.SpringboardParent = this;

            // get the mask used for the profile pic
            ProfileMask = BitmapFactory.DecodeResource( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources, Resource.Drawable.androidPhotoMask );

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navbar, NavbarFragment);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();

            // create our tasks
            Elements = new List<SpringboardElement>();
            Elements.Add( new SpringboardElement( new Droid.Tasks.News.NewsTask( NavbarFragment ), Resource.Id.springboard_news_frame, SpringboardConfig.Element_News_Icon, SpringboardStrings.Element_News_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Connect.ConnectTask( NavbarFragment ), Resource.Id.springboard_connect_frame, SpringboardConfig.Element_Connect_Icon, SpringboardStrings.Element_Connect_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Notes.NotesTask( NavbarFragment ), Resource.Id.springboard_notes_frame, SpringboardConfig.Element_Messages_Icon, SpringboardStrings.Element_Messages_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Prayer.PrayerTask( NavbarFragment ), Resource.Id.springboard_prayer_frame, SpringboardConfig.Element_Prayer_Icon, SpringboardStrings.Element_Prayer_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Give.GiveTask( NavbarFragment ), Resource.Id.springboard_give_frame, SpringboardConfig.Element_Give_Icon, SpringboardStrings.Element_Give_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.About.AboutTask( NavbarFragment ), Resource.Id.springboard_about_frame, SpringboardConfig.Element_More_Icon, SpringboardStrings.Element_More_Title ) );

            ActiveElementIndex = 0;
            if( savedInstanceState != null )
            {
                // grab the last active element
                ActiveElementIndex = savedInstanceState.GetInt( "LastActiveElement" );
            }

            // load our objects from disk
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Loading objects from device." ) );
            RockApi.Instance.LoadObjectsFromDevice( );
            Rock.Mobile.Util.Debug.WriteLine( string.Format( "Loading objects done." ) );

            // seed the last sync time with now, so that when OnResume gets called we don't do it again.
            LastRockSync = DateTime.Now;

            SyncRockData( );
        }

        void SyncRockData( )
        {
            SeriesInfoDownloaded = false;

            App.Shared.Network.RockNetworkManager.Instance.SyncRockData(
                delegate
                {
                    // here we know whether the initial handshake with Rock went ok or not
                    SeriesInfoDownloaded = true;

                    TryDisplaySeriesBillboard( );
                },
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    LastRockSync = DateTime.Now;

                    //If the OOBE isn't running
                    if( IsOOBERunning == false )
                    {
                        // Allow the news to update, and begin downloading all
                        // news and note images we need.
                        PerformTaskAction( PrivateGeneralConfig.TaskAction_NewsReload );
                        PerformTaskAction( PrivateGeneralConfig.TaskAction_NotesDownloadImages );
                    }
                });
        }

        void PerformTaskAction( string action )
        {
            foreach( SpringboardElement element in Elements )
            {
                element.Task.PerformTaskAction( action );
            }
        }

        public void RegisterNewUser( )
        {
            // we want to specially allow the registration to appear while Login is showing,
            // so pass true for 'forceShow'
            StartModalFragment( RegisterFragment, true );
        }

        public override void OnSaveInstanceState( Bundle outState )
        {
            base.OnSaveInstanceState( outState );

            // store the last activity we were in
            outState.PutInt( "LastActiveElement", ActiveElementIndex );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // grab our resource file
            View view = inflater.Inflate(Resource.Layout.Springboard, container, false);

            // let the springboard elements setup their buttons
            foreach( SpringboardElement element in Elements )
            {
                element.OnCreateView( view );

                element.Button.SetOnTouchListener( this );
            }

            view.SetOnTouchListener( this );
            view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_BackgroundColor ) );

            // set the task we wish to have active
            ActivateElement( Elements[ ActiveElementIndex ] );


            // setup our profile pic button, which displays either their profile picture or an icon if they're not logged in / don't have a pic
            ProfileImageButton = view.FindViewById<Button>( Resource.Id.springboard_profile_image );
            ProfileImageButton.Click += (object sender, EventArgs e) => 
                {
                    // if we're logged in, manage their profile pic
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        ManageProfilePic( );
                    }
                    else
                    {
                        // otherwise, use it to let them log in
                        StartModalFragment( LoginFragment );
                    }
                };
            Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Primary );
            ProfileImageButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            ProfileImageButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSpringboardConfig.ProfileSymbolFontSize );
            ProfileImageButton.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            ProfileImageButton.LayoutParameters.Width = (int)Rock.Mobile.Graphics.Util.UnitToPx( 140 );
            ProfileImageButton.LayoutParameters.Height = (int)Rock.Mobile.Graphics.Util.UnitToPx( 140 );
            ProfileImageButton.SetBackgroundColor( Color.Transparent );


            // create and add a simple circle to border the image
            RelativeLayout layout = view.FindViewById<RelativeLayout>( Resource.Id.springboard_profile_image_layout );
            layout.SetBackgroundColor( Color.Transparent );

            CircleView circle = new Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView( Activity.BaseContext );

            //note: these are converted from dp to pixels, so don't do it here.
            circle.StrokeWidth = 4;

            circle.Color = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor );
            circle.SetBackgroundColor( Color.Transparent );
            circle.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (RelativeLayout.LayoutParams)circle.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
            circle.LayoutParameters.Width = (int)Rock.Mobile.Graphics.Util.UnitToPx( 150 );
            circle.LayoutParameters.Height = (int)Rock.Mobile.Graphics.Util.UnitToPx( 150 );
            layout.AddView( circle );


            // setup our login button
            LoginProfileButton = view.FindViewById<Button>( Resource.Id.springboard_login_button );
            LoginProfileButton.Click += (object sender, EventArgs e) => 
                {
                    // if we're logged in, it'll be the profile one
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        StartModalFragment( ProfileFragment );
                    }
                    else
                    {
                        // else it'll be the login one
                        StartModalFragment( LoginFragment );
                    }
                };
            
            // setup the textView for rendering either "Tap to Personalize" or "View Profile"
            ViewProfileLabel = view.FindViewById<TextView>( Resource.Id.view_profile );
            ViewProfileLabel.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            ViewProfileLabel.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
            ViewProfileLabel.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );


            // get the size of the display. We will use this rather than Resources.DeviceManager because this
            // is absolute and won't change based on orientation
            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            float displayWidth = displaySize.X;

            float revealPercent = MainActivity.IsLandscapeWide( ) ? PrivatePrimaryNavBarConfig.Landscape_RevealPercentage_Android : PrivatePrimaryNavBarConfig.Portrait_RevealPercentage_Android;

            // setup the width of the springboard area and campus selector
            ProfileContainer = view.FindViewById<LinearLayout>( Resource.Id.springboard_profile_image_container );
            ProfileContainer.LayoutParameters.Width = (int) ( displayWidth * revealPercent );

            // setup the textView for rendering the user's name when they're logged in "Welcome: Jered"
            ProfilePrefix = view.FindViewById<TextView>( Resource.Id.profile_prefix );
            ProfilePrefix.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
            ProfilePrefix.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
            ProfilePrefix.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            ProfilePrefix.Text = SpringboardStrings.LoggedIn_Prefix;
            ProfilePrefix.Measure( 0, 0 );

            ProfileName = view.FindViewById<TextView>( Resource.Id.profile_name );
            ProfileName.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            ProfileName.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
            ProfileName.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
            ProfileName.SetMaxLines( 1 );
            ProfileName.Ellipsize = Android.Text.TextUtils.TruncateAt.End;


            CampusContainer = view.FindViewById<View>( Resource.Id.campus_container );
            CampusContainer.LayoutParameters.Width = (int) ( displayWidth * revealPercent );
            CampusContainer.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_BackgroundColor ) );

            View seperator = view.FindViewById<View>( Resource.Id.end_seperator );
            seperator.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );


            // setup the bottom campus / settings selector
            CampusText = CampusContainer.FindViewById<TextView>( Resource.Id.campus_selection_text );
            CampusText.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
            CampusText.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            CampusText.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
            CampusText.SetTextSize(Android.Util.ComplexUnitType.Dip,  ControlStylingConfig.Small_FontSize );
            CampusText.SetSingleLine( );

            TextView settingsIcon = CampusContainer.FindViewById<TextView>( Resource.Id.campus_selection_icon );
            settingsIcon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( PrivateControlStylingConfig.Icon_Font_Primary ), TypefaceStyle.Normal );
            settingsIcon.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementTextColor ) );
            settingsIcon.SetTextSize( Android.Util.ComplexUnitType.Dip, PrivateSpringboardConfig.CampusSelectSymbolSize );
            settingsIcon.Text = PrivateSpringboardConfig.CampusSelectSymbol;

            // set the campus text to whatever their profile has set for viewing.
            CampusText.Text = string.Format( SpringboardStrings.Viewing_Campus, RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) ).ToUpper( );

            // setup the campus selection button.
            Button campusSelectionButton = CampusContainer.FindViewById<Button>( Resource.Id.campus_selection_button );
            campusSelectionButton.Click += (object sender, EventArgs e ) =>
                {
                    // build an alert dialog containing all the campus choices
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    Java.Lang.ICharSequence [] campusStrings = new Java.Lang.ICharSequence[ RockGeneralData.Instance.Data.Campuses.Count ];
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        campusStrings[ i ] = new Java.Lang.String( App.Shared.Network.RockGeneralData.Instance.Data.Campuses[ i ].Name );
                    }

                    // launch the dialog, and on selection, update the viewing campus text.
                    builder.SetItems( campusStrings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    // get the ID for the campus they selected
                                    string campusTitle = campusStrings[ clickArgs.Which ].ToString( );
                                    RockMobileUser.Instance.ViewingCampus = RockGeneralData.Instance.Data.CampusNameToId( campusTitle );

                                    // build a label showing what they picked
                                    RefreshCampusSelection( );
                                });
                        });

                    builder.Show( );
                };

            Billboard = new NotificationBillboard( displayWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context );
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
                    // find the Notes task, activate it, and tell it to jump to the read page.
                    foreach( SpringboardElement element in Elements )
                    {
                        if ( element.Task as Droid.Tasks.Notes.NotesTask != null )
                        {
                            ActivateElement( element );
                            PerformTaskAction( PrivateGeneralConfig.TaskAction_NotesRead );
                        }
                    }
                } );
            Billboard.Hide( );

            return view;
        }

        void RefreshCampusSelection( )
        {
            string newCampusText = string.Format( SpringboardStrings.Viewing_Campus, 
                RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) ).ToUpper( );

            if ( CampusText.Text != newCampusText )
            {
                CampusText.Text = newCampusText;

                // let the news know it should reload
                PerformTaskAction( PrivateGeneralConfig.TaskAction_NewsReload );
            }
        }

        public void RevealButtonClicked( )
        {
            // this will be called by the Navbar (which owns the reveal button) when
            // it's clicked. We want to make sure we alwas hide the billboard.
            Billboard.Hide( );
        }

        public void StartModalFragment( Fragment fragment, bool forceShow = false )
        {
            // don't allow multiple modal fragments, or modal fragments when the springboard is closed.
            if (forceShow == true || ( VisibleModalFragment == null && NavbarFragment.ShouldSpringboardAllowInput( ) ) )
            {
                FullScreenLayout.Visibility = ViewStates.Visible;

                // replace the entire screen with a modal fragment
                var ft = FragmentManager.BeginTransaction( );
                ft.SetTransition( FragmentTransit.FragmentFade );

                ft.Replace( Resource.Id.fullscreen, fragment );

                ft.Commit( );
            }
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            // when the configuration changes, update the springboard profile and campus containers
            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            float displayWidth = displaySize.X;

            float revealPercent = MainActivity.IsLandscapeWide( ) ? PrivatePrimaryNavBarConfig.Landscape_RevealPercentage_Android : PrivatePrimaryNavBarConfig.Portrait_RevealPercentage_Android;

            ProfileContainer.LayoutParameters.Width = (int) ( displayWidth * revealPercent );
            CampusContainer.LayoutParameters.Width = (int) ( displayWidth * revealPercent );

            NavbarFragment.LayoutChanged( );
        }

        /// <summary>
        /// Returns the width of the springboard when revealed
        /// </summary>
        /// <returns>The container display width.</returns>
        public static int GetSpringboardDisplayWidth( )
        {
            Point displaySize = new Point( );
            ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).WindowManager.DefaultDisplay.GetSize( displaySize );
            float displayWidth = displaySize.X;

            if ( MainActivity.IsLandscapeWide( ) == true )
            {
                return (int)( displayWidth * PrivatePrimaryNavBarConfig.Landscape_RevealPercentage_Android );
            }
            else
            {
                return (int) (displayWidth * PrivatePrimaryNavBarConfig.Portrait_RevealPercentage_Android );
            }
        }

        /// <summary>
        /// Called by the fragment launched by StartModalFragment.
        /// Needed so that we don't take a pointer to it until any current fragment
        /// is closed.
        /// </summary>
        public void ModalFragmentOpened( Fragment fragment )
        {
            VisibleModalFragment = fragment;
        }

        public bool CanPressBack( )
        {
            // if a modal fragment is visible, end it.
            if ( VisibleModalFragment != null )
            {
                // the OOBE / Splash is special. If they hit back, ignore them.
                if ( VisibleModalFragment == OOBEFragment || VisibleModalFragment == SplashFragment )
                {
                    return false;
                }
                else
                {
                    ModalFragmentDone( null );
                    return false;
                }
            }
            // if they press back while the springboard is open, close it. (if we're not in landscape wide)
            else if ( NavbarFragment.SpringboardRevealed == true && MainActivity.IsLandscapeWide( ) == false )
            {
                // otherwise, close the springboard
                NavbarFragment.RevealSpringboard( false );
                return false;
            }
            else
            {
                // if there's no further back entry
                if ( FragmentManager.BackStackEntryCount == 0 )
                {
                    // take them to the first element. If they're there,
                    // allow the app to exit.
                    if ( Elements[ 0 ].Task != NavbarFragment.ActiveTask )
                    {
                        ActivateElement( Elements[ 0 ] );
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    // otherwise, let it be up to the discretion of
                    // the current fragment, which can toggle this.
                    return EnableBack;
                }
            }
        }

        public void ModalFragmentDone( object context )
        {
            if ( VisibleModalFragment != null && AppInForeground == true )
            {
                // remove the modal fragment
                FragmentTransaction ft = FragmentManager.BeginTransaction( );
                ft.SetTransition( FragmentTransit.FragmentFade );
                ft.Remove( VisibleModalFragment );
                ft.Commit( );

                // if login or profile are ending, update the login state
                if ( LoginFragment == VisibleModalFragment || ProfileFragment == VisibleModalFragment )
                {
                    UpdateLoginState( );

                    RefreshCampusSelection( );
                }
                // for the image cropper, store the picture
                else if ( ImageCropFragment == VisibleModalFragment )
                {
                    // take the newly cropped image and write it to disk
                    if ( context != null )
                    {
                        Bitmap croppedImage = (Bitmap)context;

                        bool success = false;
                        MemoryStream memStream = new MemoryStream();
                        try
                        {
                            // compress the image into our memory stream
                            if ( croppedImage.Compress( Bitmap.CompressFormat.Jpeg, 100, memStream ) )
                            {
                                memStream.Position = 0;

                                RockMobileUser.Instance.SaveProfilePicture( memStream );
                                RockMobileUser.Instance.UploadSavedProfilePicture( null );
                                success = true;

                                SetProfileImage( );
                            }
                        }
                        catch ( Exception )
                        {
                        }

                        if ( memStream != null )
                        {
                            memStream.Dispose( );
                        }

                        if ( success == false )
                        {
                            DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                        }
                    }
                }

                // hide the full screen layout and clear our modal fragment pointer
                FullScreenLayout.Visibility = ViewStates.Gone;
                VisibleModalFragment = null;

                // OOBE Check - If the OOBE is running, this will be the last step. 
                // We either launched the Register or Login dialog,
                // and now those are closing, so we want to show them the springboard.
                if ( IsOOBERunning == true )
                {
                    CompleteOOBE( );
                }
            }
        }

        void ManageProfilePic( )
        {
            // only allow picture taking if they're logged in
            if( RockMobileUser.Instance.LoggedIn && NavbarFragment.ShouldSpringboardAllowInput( ) )
            {
                // setup the chooser dialog so they can pick the photo source
                AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                builder.SetTitle( SpringboardStrings.ProfilePicture_SourceTitle );

                Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                    {
                        new Java.Lang.String( SpringboardStrings.ProfilePicture_SourcePhotoLibrary ),
                        new Java.Lang.String( SpringboardStrings.ProfilePicture_SourceCamera ),
                        new Java.Lang.String( GeneralStrings.Cancel )
                    };

                builder.SetItems( strings, delegate(object sender, DialogClickEventArgs clickArgs) 
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                switch( clickArgs.Which )
                                {
                                    // Photo Library
                                    case 0:
                                    {
                                        Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( Rock.Mobile.PlatformSpecific.Android.Core.Context, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
                                            {
                                                // android returns a path TO the image
                                                if( args.Image != null )
                                                {
                                                    ImageCropperPendingFilePath = (string) args.Image;
                                                }
                                            });
                                        break;
                                    }

                                    // Camera
                                    case 1:
                                    {
                                        if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                                        {
                                            // start up the camera and get our picture. 
                                            // JHM 4-24-15 - The camera requires an SD Card, so use that path for our temp file.
                                            string jpgFilename = Rock.Mobile.PlatformSpecific.Android.Core.Context.GetExternalFilesDir( null ).ToString( ) + "cameratemp.jpg";
                                            Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( new Java.IO.File( jpgFilename ), null, 

                                                delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                                                {
                                                    // flag that we want the cropper to start up on resume.
                                                    // we cannot launch it now because we need to wait for the camera
                                                    // activity to end and the navBar fragment to resume
                                                    if( args.Result == true )
                                                    {
                                                        // if the image path is valid, we have a picture.
                                                        // Otherwise, they pressed cancel, so don't do anything.
                                                        if( args.ImagePath != null )
                                                        {
                                                            ImageCropperPendingFilePath = args.ImagePath;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // couldn't get the picture
                                                        DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                                                    }
                                                });
                                        }
                                        else
                                        {
                                            // nope
                                            DisplayError( SpringboardStrings.Camera_Error_Title, SpringboardStrings.Camera_Error_Message );
                                        }
                                        break;
                                    }

                                    // Cancel
                                    case 2:
                                    {
                                        break;
                                    }
                                }
                            });
                    });

                builder.Show( );
            }
        }

        void LaunchImageCropper( string filePath )
        {
            // create the crop fragment
            ImageCropFragment.Begin( filePath, 1.00f );

            StartModalFragment( ImageCropFragment );
        }

        public override void OnPause()
        {
            base.OnPause();

            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnPause()" );

            AppInForeground = false;
        }

        bool IsOOBERunning { get; set; }
        //static bool oobeRan = false;
        public override void OnResume()
        {
            base.OnResume();

            AppInForeground = true;

            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnResume()" );

            // if it's been longer than N hours, resync rock.
            // JHM 4-27-15: Now we will always sync on resume. We do this to avoid issues like Christopher had,
            // where he ran the app early Saturday, and then didn't see the sermon notes Saturday evening.
            //if ( DateTime.Now.Subtract( LastRockSync ).TotalHours > SpringboardConfig.SyncRockHoursFrequency )
            {
                SyncRockData( );
            }
            /*else
            {
                RockLaunchData.Instance.GetNoteDB( null );
            }*/

            // refresh the viewing campus
            RefreshCampusSelection( );

            UpdateLoginState( );

            // Manage the notification billboard.
            // This is the only chance we have to kick it off. We have
            // to wait till onResume because we need all fragment views created.
            if ( Billboard.Parent == null )
            {
                // First add it 
                ( (FrameLayout)NavbarFragment.ActiveTaskFrame ).AddView( Billboard );

                TryDisplaySeriesBillboard( );
            }

            // if we haven't yet, get the fullscreen layout
            if ( FullScreenLayout == null )
            {
                FullScreenLayout = ( (Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context ).FindViewById<FrameLayout>( Resource.Id.fullscreen ) as FrameLayout;
                FullScreenLayout.Visibility = ViewStates.Gone;
                FullScreenLayout.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
            }

            // only do the OOBE if the user hasn't seen it yet
            if ( RockMobileUser.Instance.OOBEComplete == false && IsOOBERunning == false )
            //if( oobeRan == false && IsOOBERunning == false )
            {
                // sanity check for testers that didn't listen to me and delete / reinstall.
                // This will force them to be logged out so they experience the OOBE properly.
                RockMobileUser.Instance.LogoutAndUnbind( );

                //oobeRan = true;
                IsOOBERunning = true;
                StartModalFragment( OOBEFragment, true );
            }
            // otherwise if the splash hasn't been shown, show it.
            else if ( IsSplashDone == false )
            {
                IsSplashDone = true;
                SplashFragment.ContainerView = FullScreenLayout;
                StartModalFragment( SplashFragment, true );
            }
        }

        public void SplashComplete( )
        {
            ModalFragmentDone( null );

            FullScreenLayout.Alpha = 1.0f;
        }

        public void OOBEUserClick( int index )
        {
            if ( index == 0 )
            {
                StartModalFragment( RegisterFragment, true );
            }
            else if ( index == 1 )
            {
                StartModalFragment( LoginFragment, true );
            }
            else
            {
                // fade the OOBE out
                SimpleAnimator_Float viewAlphaAnim = new SimpleAnimator_Float( FullScreenLayout.Alpha, 0.00f, .13f, delegate(float percent, object value )
                {
                    FullScreenLayout.Alpha = (float)value;
                },
                delegate
                {
                       
                    // and launch the appropriate screen
                    ModalFragmentDone( null );

                    FullScreenLayout.Alpha = 1.0f;

                    CompleteOOBE( );
                } );
                viewAlphaAnim.Start( );
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

                            // if the series billboard will NOT show up,
                            if( TryDisplaySeriesBillboard( ) == false )
                            {
                                // reveal the springboard
                                NavbarFragment.RevealSpringboard( true );
                            }

                            // NOW go ahead and start downloads.
                            PerformTaskAction( PrivateGeneralConfig.TaskAction_NewsReload );
                            PerformTaskAction( PrivateGeneralConfig.TaskAction_NotesDownloadImages );
                        } );
                };
            timer.Start( );
        }

        /// <summary>
        /// Displays the "Tap to take notes" series billboard
        /// </summary>
        bool TryDisplaySeriesBillboard( )
        {
            // should we advertise the notes?
            if ( IsOOBERunning == false && Billboard.Parent != null && SeriesInfoDownloaded == true )
            {
                // uh, make sure they aren't already ON the notes.
                Droid.Tasks.Notes.NotesTask noteTask = Elements[ ActiveElementIndex ].Task as Droid.Tasks.Notes.NotesTask;
                if ( noteTask == null || noteTask.IsReadingNotes( ) == false )
                {
                    // yes, if it's a weekend and we're at church (that part will come later)
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
                                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                        {
                                            Billboard.Reveal( );
                                        } );
                                };
                                timer.Start( );

                                // let the caller know it's gonna show
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void NavbarWasResumed()
        {
            // once the navbar has resumed, we're safe to launch any pending
            // fullscreen activities.
            if( ImageCropperPendingFilePath != null )
            {
                LaunchImageCropper( ImageCropperPendingFilePath );
                ImageCropperPendingFilePath = null;
            }
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                ProfilePrefix.Text = SpringboardStrings.LoggedIn_Prefix;
                ProfileName.Text = RockMobileUser.Instance.PreferredName( );
                ViewProfileLabel.Text = SpringboardStrings.ViewProfile;

                // refresh the viewing campus
                RefreshCampusSelection( );
            }
            else
            {
                ProfilePrefix.Text = SpringboardStrings.LoggedOut_Label;
                ProfileName.Text = "";
                ViewProfileLabel.Text = SpringboardStrings.LoggedOut_Promo;
            }

            SetProfileImage( );
        }

        public void SetProfileImage( )
        {
            ProfileImageButton.Background = null;

            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                // default to displaying the "No Photo" icon
                ProfileImageButton.Text = PrivateSpringboardConfig.NoPhotoSymbol;

                // if they have an profile pic
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
                    // Load the profile pic
                    if ( FileCache.Instance.FileExists( PrivateSpringboardConfig.ProfilePic ) == true )
                    {
                        Rock.Mobile.PlatformSpecific.Android.Util.AsyncLoader.LoadImage( PrivateSpringboardConfig.ProfilePic, false, false,
                            delegate(Bitmap imageBmp )
                            {
                                // if for some reason it loaded corrupt, remove it.
                                if ( imageBmp == null )
                                {
                                    FileCache.Instance.RemoveFile( PrivateSpringboardConfig.ProfilePic );
                                }
                                else
                                {
                                    // scale the image to the size of the mask
                                    Bitmap scaledImage = Bitmap.CreateScaledBitmap( imageBmp, ProfileMask.Width, ProfileMask.Height, false );

                                    // dump the source image
                                    imageBmp.Dispose( );
                                    imageBmp = null;

                                    // if we already have a final image, dispose of it
                                    if ( ProfileMaskedImage != null )
                                    {
                                        ProfileMaskedImage.Dispose( );
                                        ProfileMaskedImage = null;
                                    }

                                    // generate the masked image
                                    ProfileMaskedImage = Rock.Mobile.PlatformSpecific.Android.Graphics.Util.ApplyMaskToBitmap( scaledImage, ProfileMask, 0, 0 );

                                    scaledImage.Dispose( );
                                    scaledImage = null;

                                    // set the final result
                                    ProfileImageButton.Text = "";
                                    ProfileImageButton.Background = new BitmapDrawable( ProfileMaskedImage );
                                }

                                return true;
                            });
                    }
                }
            }
            else
            {
                // display the "Not Logged In" icon
                ProfileImageButton.Text = PrivateSpringboardConfig.NoProfileSymbol;
            }
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            switch ( e.Action )
            {
                case MotionEventActions.Up:
                {
                    // only allow changing tasks via button press if the springboard is open 
                    // and we're not showing a modal fragment (like the Login screen)
                    if ( NavbarFragment.ShouldSpringboardAllowInput( ) == true && VisibleModalFragment == null )
                    {
                        // if we're not in landscape regular, close the springboard.
                        // we need this here as WELL as in NavbarFragment's ActivateTask so that
                        // if we tap an empty space of the springboard, we can close it.
                        if ( MainActivity.IsLandscapeWide( ) == false )
                        {
                            NavbarFragment.RevealSpringboard( false );
                        }

                        // did we tap a button?
                        SpringboardElement element = Elements.Where( el => el.Button == v ).SingleOrDefault( );
                        if ( element != null )
                        {
                            // did we tap within the revealed springboard area?
                            float visibleButtonWidth = NavbarFragment.View.Width * PrivatePrimaryNavBarConfig.Portrait_RevealPercentage_Android;
                            if ( e.GetX( ) < visibleButtonWidth )
                            {
                                // we did, so activate the element associated with that button
                                ActiveElementIndex = Elements.IndexOf( element ); 
                                ActivateElement( element );
                            }
                        }
                    }

                    break;
                }
            }

            // by default, consume the event.
            return true;
        }

        public void SetActiveTaskFrame( FrameLayout layout )
        {
            // once we receive the active task frame, we can start our task
            NavbarFragment.ActiveTaskFrame = layout;
        }

        void ActivateElement( SpringboardElement activeElement )
        {
            // total hack - If they tap Give, we'll kick them out to the give URL, leaving the app
            // in this state.
            if ( activeElement.Task as GiveTask != null )
            {
                GiveAnalytic.Instance.Trigger( GiveAnalytic.Give );

                var uri = Android.Net.Uri.Parse( GiveConfig.GiveUrl );
                var intent = new Intent( Intent.ActionView, uri ); 
                ((Activity)Rock.Mobile.PlatformSpecific.Android.Core.Context).StartActivity( intent );
            }
            else
            {
                foreach ( SpringboardElement element in Elements )
                {
                    if ( activeElement != element )
                    {
                        element.Deactivate( );
                    }
                }

                activeElement.Activate( );
                NavbarFragment.SetActiveTask( activeElement.Task );
            }
        }

        public override void OnStop()
        {
            base.OnStop();

            // save any final changes that may have been performed by the OnPause of other Fragments
            FileCache.Instance.SaveCacheMap( );
            RockApi.Instance.SaveObjectsToDevice( );

            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnStop()" );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnDestroy()" );
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnAtach()" );
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Rock.Mobile.Util.Debug.WriteLine( "Springboard OnDetach()" );
        }

        public static void DisplayError( string title, string message )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    AlertDialog.Builder dlgAlert = new AlertDialog.Builder( Rock.Mobile.PlatformSpecific.Android.Core.Context );                      
                    dlgAlert.SetTitle( title ); 
                    dlgAlert.SetMessage( message ); 
                    dlgAlert.SetPositiveButton( "Ok", delegate(object sender, DialogClickEventArgs ev )
                        {
                        } );
                    dlgAlert.Create( ).Show( );
                } );
        }
    }
}
