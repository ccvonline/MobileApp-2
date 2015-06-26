using System;
using System.Reflection;
using System.Diagnostics;

namespace App.Shared
{
    namespace Strings
    {
        public class BuildStrings
        {
            public static string Version
            {
                get
                {
                    return "Beta (Build 24)";
                }
            }
        }

        public class OOBEStrings
        {
            public const string Welcome = "WELCOME";
            public const string HaveAccount = "I have a {0} Account";
            public const string WantAccount = "Create a {0} Account";
            public const string SkipAccount = "Do this Later";
        }

        public class GeneralStrings
        {
            public const string Yes = "Yes";
            public const string No = "No";
            public const string Ok = "Ok";
            public const string Cancel = "Cancel";
            public const string Retry = "Retry";
            public const string Done = "Done";
            public const string Search = "Search";

            public static string[ ] Days =
            {
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday"
            };

            public const string Network_Status_FailedText = "Oops";
            public const string Network_Result_FailedText = "There was a problem communicating with the internet. Check your network settings and try again";
        }

        public class SpringboardStrings
        {
            public const string ProfilePicture_SourceTitle = "Profile Picture";
            public const string ProfilePicture_SourceDescription = "Select a source for your profile picture.";
            public const string ProfilePicture_SourcePhotoLibrary = "Photo Library";
            public const string ProfilePicture_SourceCamera = "Take Photo";

            public const string ProfilePicture_Error_Title = "Profile Picture";
            public const string ProfilePicture_Error_Message = "There was a problem saving your profile picture. Please try again.";

            public const string Camera_Error_Title = "No Camera";
            public const string Camera_Error_Message = "This device does not have a camera.";

            public const string LoggedIn_Prefix = "Welcome ";
            public const string LoggedOut_Label = "Log In";
            public const string LoggedOut_Promo = "Tap to Personalize";

            public const string ViewProfile = "View Profile";

            public const string Element_News_Title = "News";
            public const string Element_Connect_Title = "Connect";
            public const string Element_Messages_Title = "Messages";
            public const string Element_Prayer_Title = "Prayer";
            public const string Element_Give_Title = "Give";
            public const string Element_More_Title = "More";

            public const string SelectCampus_SourceTitle = "Campus Selection";
            public const string SelectCampus_SourceDescription = "Select the campus to view.";
            public const string Viewing_Campus = "{0} Campus"; 

            public const string TakeNotesNotificationIcon = "";
            public const string TakeNotesNotificationLabel = "Tap Here To Take Notes";
        }

        public class LoginStrings
        {
            public const string UsernamePlaceholder = "Username";
            public const string PasswordPlaceholder = "Password";

            public const string LoginButton = "Login";
            public const string RegisterButton = "Register";

            public const string LoginWithFacebook = "LoginWithFacebook";

            public const string Error_Credentials = "Invalid Username or Password";
            public const string Error_Unknown = "Unable to Login. Try Again";
            public const string Success = "Welcome back, {0}.";
        }

        public class RegisterStrings
        {
            public const string RegisterButton = "Register";
            public const string ConfirmCancelReg = "Cancel Registration?";

            public const string UsernamePlaceholder = "Username";
            public const string PasswordPlaceholder = "Password";
            public const string ConfirmPasswordPlaceholder = "Confirm Password";

            public const string NickNamePlaceholder = "First Name";
            public const string LastNamePlaceholder = "Last Name";

            public const string CellPhonePlaceholder = "Cell Phone";
            public const string EmailPlaceholder = "Email";

            public const string RegisterStatus_Success = "Registration Successful";
            public const string RegisterStatus_Failed = "Small Issue";

            public const string RegisterResult_Success = "All set! You're now registered with CCV.";
            public const string RegisterResult_Failed = "Looks like there was a problem registering. Make sure you're connected to the internet and try again.";
            public const string RegisterResult_LoginUsed = "This username is already taken. Please try a different one.";
            public const string RegisterResult_BadLogin = "CreateLoginError";
        }

        public class ProfileStrings
        {
            public const string SubmitChangesTitle = "Submit Changes?";
            public const string LogoutTitle = "Log Out?";

            public const string NickNamePlaceholder = "First Name";
            public const string LastNamePlaceholder = "Last Name";

            public const string CellPhonePlaceholder = "Cell Phone";
            public const string EmailPlaceholder = "Email";

            public const string StreetPlaceholder = "Street";
            public const string CityPlaceholder = "City";
            public const string StatePlaceholder = "State";
            public const string ZipPlaceholder = "Zip";

            public const string GenderPlaceholder = "Gender";
            public const string BirthdatePlaceholder = "Birthdate";
            public const string CampusPlaceholder = "My CCV Campus";

            public const string SelectCampus_SourceTitle = "Home Campus Selection";
            public const string SelectCampus_SourceDescription = "Select your home campus.";
            public const string Viewing_Campus = "{0} Campus"; 

            public const string DoneButtonTitle = "Done";
            public const string LogoutButtonTitle = "Logout";

            public const string SelectBirthdateLabel = "Select Birthday";
            public const string SelectGenderLabel = "Select a Gender";
        }

        public class NewsStrings
        {
            public const string LearnMore = "Learn More";
        }

        public class GiveStrings
        {
            public const string Header = "Giving at CCV";
            public const string ButtonLabel = "Give Online";
        }

        public class ConnectStrings
        {
            public const string GroupFinder_StreetPlaceholder = "Street";
            public const string GroupFinder_CityPlaceholder = "City";
            public const string GroupFinder_StatePlaceholder = "State";
            public const string GroupFinder_ZipPlaceholder = "Zip";

            public const string GroupFinder_DefaultState = "AZ";

            public const string GroupFinder_GroupsFound = "Groups Nearest Your Location";
            public const string GroupFinder_NoGroupsFound = "No Groups Found Near Your Location";
            public const string GroupFinder_NetworkError = "Network problem. Please try again.";

            public const string GroupFinder_SearchButtonLabel = "Touch to Search an Address";

            public const string GroupFinder_SearchPageHeader = "Group Finder";
            public const string GroupFinder_SearchPageDetails = "Enter an address to find nearby neighborhood groups";

            public const string GroupFinder_MeetingTime = "Meets on {0} at {1}";
            public const string GroupFinder_MilesSuffix = "Miles";
            public const string GroupFinder_ClosestTag = "(Closest group to you)";
            public const string GroupFinder_ContactForTime = "Contact for Meeting Time";

            public const string GroupFinder_Neighborhood = "Your neighborhood is:  ";
            public const string GroupFinder_JoinLabel = "Join";
            public const string GroupFinder_DetailsLabel = "Details";

            public const string Main_Connect_Header = "Connect";
            public const string Main_Connect_GroupFinder = "Group Finder";

            public const string JoinGroup_FirstNamePlaceholder = "First Name";
            public const string JoinGroup_LastNamePlaceholder = "Last Name";
            public const string JoinGroup_SpouseNamePlaceholder = "Spouse Name";
            public const string JoinGroup_EmailPlaceholder = "Email";
            public const string JoinGroup_CellPhonePlaceholder = "Cell Phone";
            public const string JoinGroup_JoinButtonLabel = "Join Group";

            public const string JoinGroup_RegisterSuccess = "You're registered with {0}. Expect to be contacted in the next few days.";
            public const string JoinGroup_RegisterFailed = "There was a problem registering for {0}. Check your network settings and try again.";
        }

        public class MessagesStrings
        {
            public const string Series_Error_Title = "Messages";
            public const string Series_Error_Message = "There was a problem downloading message series. Check your network settings and try again.";

            public const string Error_Title = "Messages";
            public const string Error_Message = "There was a problem downloading the message. Check your network settings and try again.";

            public const string Error_Watch_Playback = "There was a problem playing this content. Check your network settings and try again.";

            public const string Watch_Share_Subject = "Check out this Video";

            /// <summary>
            /// Defines the share email as an html doc. Please don't change this.
            /// </summary>
            public const string Watch_Share_Header_Html = "<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n<HTML><Body>\n";

            /// <summary>
            /// The HTML that should be used in the body of the Share Video email.
            /// </summary>
            public const string Watch_Share_Body_Html = "<p>Check out this message from CCV.\n<a href={0}>Click here to watch online.</a></p>";

            /// <summary>
            /// Set this to the URL for downloading the mobile app. It should probably point to YOUR website, where you can
            /// determine if the user is on Android or iOS.
            /// If you do not wish to advertise your mobile app in the share video email, leave this as an empty string.
            /// </summary>
            public const string Watch_Mobile_App_Url = "appstore://download.com";

            /// <summary>
            /// The text that will be used in the Share Video email to let users know they can download an app to watch the video.
            /// Leave the above "Watch_Mobile_App_Url" EMPTY if you do not wish to include this.
            /// </summary>
            public const string Watch_Share_DownloadApp_Html = "<p><a href={0}>Watch it in the CCV mobile app instead.</a></p>";

            public const string Series_TopBanner = "This Week's Message";

            public const string Series_Table_Watch = "Watch";
            public const string Series_Table_TakeNotes = "Take Notes";

            public const string Series_Table_PreviousMessages = "Previous Message Series";

            public const string UserNote_Placeholder = "Enter note";

            public const string Read_Share_Notes = "Message - {0}";

            public const string TooManyNotes = "Looks like you're taking a lot of notes. Creating any more will cause this app to really slow down.\nTry consolidating some of them so there aren't quite so many.";

            public const string UserNote_DeleteTitle = "Delete Note";
            public const string UserNote_DeleteMessage = "Are you sure you want to delete this note?";
            public const string UserNote_Prefix = "My Note - ";

        }

        public class PrayerStrings
        {
            public const string Error_Title = "Prayer";

            public const string Error_Retrieve_Message = "There was a problem getting prayer requests. Check your network settings and try again.";
            public const string Error_Submit_Message = "There was a problem submitting your prayer request. Check your network settings and try again.";

            public const string ViewPrayer_StatusText_Retrieving = "Getting Prayer Requests";
            public const string ViewPrayer_StatusText_Failed = "Oops";
            public const string ViewPrayer_StatusText_NoPrayers = "Prayer Reqests";
            public const string ViewPrayer_Result_NoPrayersText = "There don't currently appear to be any public prayer requests. Why not add one?";
            public const string Prayer_Before = "Pray";
            public const string Prayer_After = "Prayed";

            public const string CreatePrayer_FirstNamePlaceholderText = "First Name";
            public const string CreatePrayer_LastNamePlaceholderText = "Last Name";
            public const string CreatePrayer_SubmitButtonText = "Submit";
            public const string CreatePrayer_CategoryButtonText = "Category";
            public const string CreatePrayer_AnonymousSwitchLabel = "";
            public const string CreatePrayer_SelectCategoryLabel = "Select a Category";
            public const string CreatePrayer_PostAnonymously = "Post Anonymously";
            public const string CreatePrayer_Anonymous = "Anonymous";
            public const string CreatePrayer_MakePublic = "Make Public";
            public const string CreatePrayer_PrayerRequest = "Prayer Request";

            public const string PostPrayer_Status_Submitting = "Submitting";
            public const string PostPrayer_Status_SuccessText = "Submitted";
            public const string PostPrayer_Status_FailedText = "Oops";
            public const string PostPrayer_Result_SuccessText = "Prayer posted successfully. As soon as it is approved you'll see it in the Prayer Requests.";
            public const string PostPrayer_Result_FailedText = "Looks like there was a problem submitting your request. Tap below to try again, or go back to make changes.";
        }
    }
}
