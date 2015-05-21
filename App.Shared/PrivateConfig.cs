using System;
using System.Drawing;

namespace App.Shared
{
    /// <summary>
    /// PrivateConfig simply refers to values that you probably do not want or
    /// need to change. Only change these settings if you really know what you're doing.
    /// </summary>
    namespace PrivateConfig
    {
        public class PrivateGeneralConfig
        {
            /// <summary>
            /// The size (in pixels) of the profile image to download from Rock
            /// </summary>
            public const uint ProfileImageSize = 200;

            /// <summary>
            /// The permissions the Facebook App should request. You probably shouldn't change this.
            /// </summary>
            public const string FBAppPermissions = "public_profile, user_friends, email";

            /// <summary>
            /// Defined in Rock, this should NEVER change, and is the key the mobile app uses so Rock
            /// knows who it's talking to.
            /// </summary>
            public const string RockMobileAppAuthorizationKey = "hWTaZ7buziBcJQH31KCm3Pzz";

            /// <summary>
            /// These are values that, while generated when the Rock database is created,
            /// are extremely unlikely to ever change. If they do change, simply update them here to match
            /// Rock.
            /// </summary>
            public const int CellPhoneValueId = 12;
            public const int NeighborhoodGroupGeoFenceValueId = 48;
            public const int NeighborhoodGroupValueId = 49;
            public const int GroupLocationTypeHomeValueId = 19;
            public const int GroupMemberStatus_Pending_ValueId = 2;
            public const int ApplicationGroup_PhotoRequest_ValueId = 1207885;
            public const int GroupMemberRole_Member_ValueId = 59;
            public const int GeneralDataTimeValueId = 2623;
            public const int UserLoginEntityTypeId = 27;
            public const int GroupRegistrationValueId = 52;
            public const int PersonConnectionStatusValueId = 146;
            public const int PersonRecordStatusValueId = 5;

            /// <summary>
            /// These are the names of placeholder images. They should not need to change.
            /// </summary>
            public const string NewsMainPlaceholder = "placeholder_news_main.png";
            public const string NewsDetailsPlaceholder = "placeholder_news_details.png";

            /// <summary>
            /// Actions sent via our super basic "event" system.
            /// </summary>
            public const string TaskAction_NewsReload = "News.Reload";
            public const string TaskAction_NotesDownloadImages = "Notes.DownloadImages";
            public const string TaskAction_NotesRead = "Page.Read";
        }

        public class PrivateSpringboardConfig
        {
            /// <summary>
            /// The text glyph to use as a symbol when the user does not have a profile.
            /// </summary>
            public const string NoProfileSymbol = "";

            /// <summary>
            /// The text glyph to use as a symbol when the user doesn't have a photo.
            /// </summary>
            public const string NoPhotoSymbol = "";

            /// <summary>
            /// The size of font to use for the no photo symbol
            /// </summary>
            public const float ProfileSymbolFontSize = 48;

            /// <summary>
            /// When we store their profile pic, this is what it's called.
            /// When the HasProfileImage flag is true, we'll load it from this file.
            /// </summary>
            public const string ProfilePic = "userPhoto.jpg";

            /// <summary>
            /// The symbol to use representing the settings button.
            /// </summary>
            public const string CampusSelectSymbol = "";

            /// <summary>
            /// The size of the symbol representing the settings button.
            /// </summary>
            public const float CampusSelectSymbolSize = 14;

            /// <summary>
            /// The size of font to use for the element's logo.
            /// </summary>
            public const int Element_FontSize = 23;

            /// <summary>
            /// The X offset to place the CENTER of the element's logo.
            /// </summary>
            public const int Element_LogoOffsetX_iOS = 20;

            /// <summary>
            /// The X offset to place the LEFT EDGE of the element's text.
            /// </summary>
            public const int Element_LabelOffsetX_iOS = 40;
        }

        public class PrivateImageCropConfig
        {
            /// <summary>
            /// The text to display for the 'ok to crop' button.
            /// </summary>
            public const string CropOkButton_Text = "";

            /// <summary>
            /// The size (in font points) of the 'ok to crop' button.
            /// </summary>
            public const int CropOkButton_Size = 36;

            /// <summary>
            /// The text to display for the 'cancel crop' button.
            /// </summary>
            public const string CropCancelButton_Text = "";

            /// <summary>
            /// The size (in font points) of the 'cancel crop' button.
            /// </summary>
            public const int CropCancelButton_Size = 36;
        }

        public class PrivateSubNavToolbarConfig
        {
            /// <summary>
            /// The height of the subNavigation toolbar (the one at the bottom)
            /// </summary>
            public const float Height_iOS = 44;

            /// <summary>
            /// The color of the subNavigation toolbar (the one at the bottom)
            /// </summary>
            public const uint BackgroundColor = 0x505050FF;

            /// <summary>
            /// The amount of opacity (see throughyness) of the subNavigation toolbar (the one at the bottom)
            /// 1 = fully opaque, 0 = fully transparent.
            /// </summary>
            public const float Opacity = .75f;

            /// <summary>
            /// The amount of time (in seconds) it takes for the subNavigation toolbar (the one at the bottom)
            /// to slide up or down.
            /// </summary>
            public const float SlideRate = .30f;

            /// <summary>
            /// On iOS only, the amount of space between buttons on the subNavigation toolbar (the one at the bottom)
            /// </summary>
            public const float iOS_ButtonSpacing = 5.0f;

            /// <summary>
            /// The text to display for the subNavigation toolbar's back button. (the one at the bottom)
            /// </summary>
            public const string BackButton_Text = "";

            /// <summary>
            /// The size (in font points) of the sub nav toolbar back button. (the one at the bottom)
            /// </summary>
            public const int BackButton_Size = 36;

            /// <summary>
            /// The text to display for the subNavigation toolbar's share button. (the one at the bottom)
            /// </summary>
            public const string ShareButton_Text = "";

            /// <summary>
            /// The size (in font points) of the sub nav toolbar share button. (the one at the bottom)
            /// </summary>
            public const int ShareButton_Size = 36;

            /// <summary>
            /// The text to display for the subNavigation toolbar's create button. (the one at the bottom)
            /// </summary>
            public const string CreateButton_Text = "";

            /// <summary>
            /// The size (in font points) of the sub nav toolbar create button. (the one at the bottom)
            /// </summary>
            public const int CreateButton_Size = 36;
        }

        /// <summary>
        /// Settings for the primary nav bar (the one at the top)
        /// </summary>
        public class PrivatePrimaryNavBarConfig
        {
            /// <summary>
            /// The character to be displayed representing the reveal button.
            /// </summary>
            public const string RevealButton_Text = "";

            /// <summary>
            /// The percentage of the navbar width to slide over when revealing the Springboard in Portrait. (Android Only)
            /// </summary>
            public const float Portrait_RevealPercentage_Android = .65f;

            /// <summary>
            /// The percentage of the navbar width to slide over when revealing the Springboard in Landscape Wide. (Android Only)
            /// </summary>
            public const float Landscape_RevealPercentage_Android = .35f;

            /// <summary>
            /// The size of the character representing the reveal button.
            /// </summary>
            public const int RevealButton_Size = 36;

            /// <summary>
            /// The logo to be displayed on the primary nav bar.
            /// </summary>
            public const string LogoFile_iOS = "navbarLogo.png";
        }

        /// <summary>
        /// Settings for the primary container that all activities lie within.
        /// </summary>
        public class PrivatePrimaryContainerConfig
        {
            /// <summary>
            /// Time (in seconds) it takes for the primary container to slide in/out to reveal the springboard.
            /// </summary>
            public const float SlideRate = .50f;

            /// <summary>
            /// The amount to slide when revelaing the springboard.
            /// </summary>
            public const float SlideAmount_iOS = 230;

            /// <summary>
            /// The max amount to darken the panel when revealing the springboard. ( 0 - 1 )
            /// </summary>
            public const float SlideDarkenAmount = .75f;

            /// <summary>
            /// The darkness of the shadow cast by the primary container on top of the springboard.
            /// 1 = fully opaque, 0 = fully transparent.
            /// </summary>
            public const float ShadowOpacity_iOS = .60f;

            /// <summary>
            /// The offset of the shadow cast by the primary container on top of the springboard.
            /// </summary>
            public static SizeF ShadowOffset_iOS = new SizeF( 0.0f, 5.0f );
        }

        public class PrivateNewsConfig
        {
            /// <summary>
            /// The height of news image banners
            /// </summary>
            public const float NewsBannerWidth = 1242;
            public const float NewsBannerHeight = 540;

            public const float NewsBannerAspectRatio = NewsBannerHeight / NewsBannerWidth;


            public const float NewsMainWidth = 1242;
            public const float NewsMainHeight = 801;

            public const float NewsMainAspectRatio = NewsMainHeight / NewsMainWidth;
        }

        public class PrivateNoteConfig
        {
            public const string NotesMainPlaceholder = "placeholder_notes_main.png";
            public const string NotesThumbPlaceholder = "placeholder_notes_thumb.png";

            /// <summary>
            /// The image to display for the tutorial screen.
            /// </summary>
            public const string TutorialOverlayImage = "note_tutorial_portrait.png";
            public const string TutorialOverlayImageIPadLS = "note_tutorial_ipad_ls.png";

            public const float NotesMainPlaceholderWidth = 750;
            public const float NotesMainPlaceholderHeight = 422;
            public const float NotesMainPlaceholderAspectRatio = NotesMainPlaceholderHeight / NotesMainPlaceholderWidth;


            public const float NotesThumbPlaceholderWidth = 210;
            public const float NotesThumbPlaceholderHeight = 210;
            public const float NotesThumbPlaceholderAspectRatio = NotesMainPlaceholderHeight / NotesMainPlaceholderWidth;

            /// <summary>
            /// The number of times to show the user the "double tap to take notes" thing.
            /// </summary>
            public const float MaxTutorialDisplayCount = 3;

            /// <summary>
            /// The suffix to use for the user note filename.
            /// </summary>
            public const string UserNoteSuffix = "_user_note.dat";

            /// <summary>
            /// The space between the border of a view and the contents.
            /// Only applies when a border is rendered.
            /// </summary>
            public const int BorderPadding = 2;

            /// <summary>
            /// The icon to use for displaying the citation icon.
            /// </summary>
            public const string CitationUrl_Icon = "";

            /// <summary>
            /// The size of the font/icon when displaying the citation icon.
            /// </summary>
            public const int CitationUrl_IconSize = 24;

            /// <summary>
            /// The icon to use for displaying the user note icon.
            /// </summary>
            public const string UserNote_Icon = "";

            /// <summary>
            /// The size of the font/icon when the usernote is OPEN.
            /// </summary>
            public const int UserNote_IconOpenSize = 30;

            /// <summary>
            /// The size of the font/icon when the usernote is CLOSED.
            /// </summary>
            public const int UserNote_IconClosedSize = 46;

            /// <summary>
            /// The icon to use for displaying the delete icon on user notes.
            /// </summary>
            public const string UserNote_DeleteIcon = "";//"";

            /// <summary>
            /// The size of the font/icon when displaying the delete icon on user notes.
            /// </summary>
            /// 
            public const int UserNote_DeleteIconSize = 40;

            /// <summary>
            /// The icon to use for displaying the close icon on user notes.
            /// </summary>
            public const string UserNote_CloseIcon = "";

            /// <summary>
            /// The size of the font/icon when displaying the close icon on user notes.
            /// </summary>
            /// 
            public const int UserNote_CloseIconSize = 40;

            /// <summary>
            /// The icon to use representing "Listen to this Message"
            /// </summary>
            public const string Series_Table_Listen_Icon = "";

            /// <summary>
            /// The icon to use representing "Watch this Message"
            /// </summary>
            public const string Series_Table_Watch_Icon = "";

            /// <summary>
            /// The icon to use representing the action to "Take Notes"
            /// </summary>
            public const string Series_Table_TakeNotes_Icon = "";

            /// <summary>
            /// The icon to use representing that tapping the element will take you to a new page. (Like a > symbol)
            /// </summary>
            public const string Series_Table_Navigate_Icon = "";

            /// <summary>
            /// The size of icons in the Series table.
            /// </summary>
            public const uint Series_Table_IconSize = 36;

            /// <summary>
            /// The height that an image should be within the cell
            /// </summary>
            public const float Series_Main_CellHeight = 70;

            /// <summary>
            /// The width that an image should be within the cell
            /// </summary>
            public const float Series_Main_CellWidth = 70;

            /// <summary>
            /// The icon size for icons in the details table row.
            /// </summary>
            public const uint Details_Table_IconSize = 62;
        }

        public class PrivatePrayerConfig
        {
            /// <summary>
            /// The interval to download prayer requests. (Between this time they will be cached in memory)
            /// They WILL be redownloaded if the app is quit and re-run.
            /// </summary>
            public static TimeSpan PrayerDownloadFrequency = new TimeSpan( 1, 0, 0 );

            /// <summary>
            /// The length of the animation when a prayer card is animating.
            /// </summary>
            public const float Card_AnimationDuration = .25f;

            /// <summary>
            /// The size of the symbol used to representing a prayer post result.
            /// </summary>
            public const uint PostPrayer_ResultSymbolSize_Droid = 64;
        }

        public class PrivateConnectConfig
        {
            /// <summary>
            /// Image to use for the group finder thumbnail image
            /// </summary>
            public const string GroupFinder_IconImage = "groupfinder_thumb.png";

            /// <summary>
            /// Banner to display at the top of the Connect Page
            /// </summary>
            public const string MainPageHeaderImage = "connect_banner.png";

            public const float MainPageHeaderWidth = 2304;

            public const float MainPageHeaderHeight = 1296;

            public const float MainPageHeaderAspectRatio = MainPageHeaderHeight / MainPageHeaderWidth;

            /// <summary>
            /// The width/height of the image used as a thumbnail for each entry in the "Other ways to connect"
            /// </summary>
            public const float MainPage_ThumbnailDimension = 70;

            /// <summary>
            /// The icon to use representing that tapping the element will take you to a new page. (Like a > symbol)
            /// </summary>
            public const string MainPage_Table_Navigate_Icon = "";

            /// <summary>
            /// The size of icons navigate icon in each row.
            /// </summary>
            public const uint MainPage_Table_IconSize = 36;

            /// <summary>
            /// The icon to use representing the join button.
            /// </summary>
            public const string GroupFinder_JoinIcon = "";

            /// <summary>
            /// The size of the icon to use for the join button.
            /// </summary>
            public const uint GroupFinder_Join_IconSize = 64;
        }

        public class PrivateControlStylingConfig
        {
            /// <summary>
            /// The primary (most commonly used) icon font
            /// </summary>
            public const string Icon_Font_Primary = "FontAwesome";

            /// <summary>
            /// The secondary (used in occasional spots) icon font
            /// </summary>
            public const string Icon_Font_Secondary = "Bh";

            /// <summary>
            /// The symbol to use for a result that was successful.
            /// </summary>
            public const string Result_Symbol_Success = "";

            /// <summary>
            /// The symbol to use for a result that failed.
            /// </summary>
            public const string Result_Symbol_Failed = "";
        }
    }
}

