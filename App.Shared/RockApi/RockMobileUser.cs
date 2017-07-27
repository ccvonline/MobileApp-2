using System;
using Rock.Client;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using MobileApp.Shared.Config;
using System.Collections.Generic;
using RestSharp;
using Rock.Mobile.Util.Strings;
using System.Runtime.Serialization.Formatters.Binary;
using MobileApp.Shared.PrivateConfig;
using Rock.Mobile.IO;
using MobileApp;
using System.Linq;

namespace MobileApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Basically a wrapper for Rock.Models that make up the "user" of this mobile app.
            /// </summary>
            public sealed class RockMobileUser
            {
                /// <summary>
                /// Defines what ACCOUNT is used to login to Rock. They are always logged in via Rock,
                /// but the account used to log in could be a Rock Account or their Facebook Account
                /// </summary>
                public enum BoundAccountType
                {
                    Facebook,
                    Rock,
                    None
                }
                public BoundAccountType AccountType { get; set; }

                /// <summary>
                /// Instance for MobileUser. We only allow single logins, so force a static instance.
                /// </summary>
                private static RockMobileUser _Instance = new RockMobileUser();
                public static RockMobileUser Instance { get { return _Instance; } }

                const string MOBILEUSER_DATA_FILENAME = "mobileuser.dat";

                // versioning to protect against old data. Since everything is stored in Rock,
                // we're ok to wipe it if data is old. This must be incremented if
                // the mobile user is changed.
                int Version = 0;

                /// <summary>
                /// Account - The ID representing the user. If they logged in via a Rock Account, it's a Username. If a social service,
                /// it might be their social service account ID.
                /// </summary>
                public string UserID { get; set; }

                /// <summary>
                /// Account - Rock Password. Only valid if they are logged in with a Rock Account. If they logged in via a social service,
                /// this will be empty.
                /// </summary>
                public string RockPassword { get; set; }

                /// <summary>
                /// Account - Access Token for Social Service. If they're logged in via a Rock Account we don't need this or care about it.
                /// If they are logged in via a Social Service we will.
                /// </summary>
                public string AccessToken { get; set; }

                /// <summary>
                /// True when logged in.
                /// </summary>
                public bool LoggedIn { get; set; }

                /// <summary>
                /// Person object representing this user's core personal data.
                /// </summary>
                /// <value>The person.</value>
                public Person Person;

                /// <summary>
                /// If true, the user has created a custom note and won't need to see the tutorial again.
                /// </summary>
                /// <value><c>true</c> if user note created; otherwise, <c>false</c>.</value>
                public bool UserNoteCreated { get; set; }

                /// <summary>
                /// True if this is the first time the user has run the app
                /// </summary>
                /// <value><c>true</c> if this instance is first run; otherwise, <c>false</c>.</value>
                public bool OOBEComplete { get; set; }

                /// <summary>
                /// Used to know whether we need to sync the profile image or not.
                /// </summary>
                /// <value><c>true</c> if profile image dirty; otherwise, <c>false</c>.</value>
                public bool ProfileImageDirty { get; set; }

                /// <summary>
                /// The primary family associated with this person, which contains their home campus
                /// </summary>
                public Rock.Client.Group PrimaryFamily;

                /// <summary>
                /// GroupLocation representing the address of their primary residence
                /// </summary>
                public Rock.Client.GroupLocation PrimaryAddress;

                // the state of the actions someone can (and should!) take at CCV
                public bool IsBaptised { get; set; }
                public bool IsERA { get; set; }
                public bool IsGiving { get; set; }
                public bool TakenStartingPoint { get; set; }
                public bool IsMember { get; set; }
                public bool IsServing { get; set; }
                public bool IsPeerLearning { get; set; }
                public bool IsMentored { get; set; }
                public bool IsTeaching { get; set; }

                // This will return either their HOME campus if they're logged in, or their
                // VIEWING campus if they're not.
                // JHM 10-7: seems like this confuses people more. It now just returns the viewing campus.
                public int GetRelevantCampus( )
                {
                    // JHM 10-7: seems like this confuses people more.
                    /*if ( LoggedIn && PrimaryFamily.CampusId.HasValue )
                    {
                        return PrimaryFamily.CampusId.Value;
                    }*/

                    return ViewingCampus;
                }

                // make the address getters methods, not properties, so json doesn't try to serialize them.
                public string Street1( )
                {
                    return string.IsNullOrEmpty( PrimaryAddress.Location.Street1 ) == true ? "" : PrimaryAddress.Location.Street1;
                }

                public string Street2( )
                {
                    return string.IsNullOrEmpty( PrimaryAddress.Location.Street2 ) == true ? "" : PrimaryAddress.Location.Street2;
                }

                public string City( )
                {
                    return string.IsNullOrEmpty( PrimaryAddress.Location.City ) == true ? "" : PrimaryAddress.Location.City;
                }

                public string State( )
                {
                    return string.IsNullOrEmpty( PrimaryAddress.Location.State ) == true ? "" : PrimaryAddress.Location.State;
                }

                public string Zip( )
                {
                    if ( string.IsNullOrEmpty( PrimaryAddress.Location.PostalCode ) == false )
                    {
                        // guard against there not being a postal code suffix
                        int length = PrimaryAddress.Location.PostalCode.IndexOf( '-' );
                        if ( length == -1 )
                        {
                            length = PrimaryAddress.Location.PostalCode.Length;
                        }

                        return PrimaryAddress.Location.PostalCode.Substring( 0, length );
                    }

                    return "";
                }

                public void SetAddress( string street, string city, string state, string zip )
                {
                    // make sure they DO have a location
                    PrimaryAddress.Location.Street1 = street;
                    PrimaryAddress.Location.City = city;
                    PrimaryAddress.Location.State = state;
                    PrimaryAddress.Location.PostalCode = zip;
                }

                public void SetBirthday( DateTime birthday )
                {
                    // update the birthdate field
                    Person.BirthDate = birthday;

                    // and the day/month/year fields.
                    Person.BirthDay = Person.BirthDate.Value.Day;
                    Person.BirthMonth = Person.BirthDate.Value.Month;
                    Person.BirthYear = Person.BirthDate.Value.Year;
                }

                public bool HasFullAddress( )
                {
                    // by full address, we mean street, city, state, zip
                    if ( string.IsNullOrEmpty( Street1( ) ) == false &&
                         string.IsNullOrEmpty( City( ) ) == false &&
                         string.IsNullOrEmpty( State( ) ) == false &&
                         string.IsNullOrEmpty( Zip( ) ) == false )
                    {
                        return true;
                    }
                    return false;
                }

                /// <summary>
                /// The URL of the last media streamed, used so we can know whether
                /// to resume it or not.
                /// </summary>
                /// <value>The last streaming mediaURL.</value>
                public string LastStreamingMediaUrl { get; set; }

                /// <summary>
                /// The left off position of the last streaming media, so we can
                /// resume if desired.
                /// </summary>
                /// <value>The last streaming video position.</value>
                public double LastStreamingMediaPos { get; set; }

                /// <summary>
                /// If true they have a profile image, so we should look for it in our defined spot.
                /// The way profile images work is, Rock will tell us they have one via a url.
                /// We'll request it and retrieve it, and then store it locally.
                /// </summary>
                /// <value><c>true</c> if this instance has profile image; otherwise, <c>false</c>.</value>
                public bool HasProfileImage { get; set; }

                /// <summary>
                /// The index of the campus the user is viewing in the mobile app.
                /// This may or may NOT be their "home campus" as defined in the Person object.
                /// </summary>
                /// <value>The viewing campus.</value>
                public int ViewingCampus { get; set; }

                /// <summary>
                /// A json version of the person at the last point it was sync'd with the server.
                /// This allows us to update Person and save it, and in the case of a server sync failing,
                /// know that we need to try again.
                /// </summary>
                /// <value>The person json.</value>
                [JsonProperty]
                string LastSyncdPersonJson { get; set; }

                [JsonProperty]
                string LastSyncdFamilyJson { get; set; }

                [JsonProperty]
                string LastSyncdAddressJson { get; set; }

                [JsonProperty]
                string LastSyncdCellPhoneNumberJson { get; set; }

                [JsonProperty]
                bool HasCellNumber { get; set; }

                [JsonProperty]
                Rock.Client.PhoneNumber _CellPhoneNumber { get; set; }

                public string CellPhoneNumberDigits( )
                {
                    return _CellPhoneNumber.Number;
                }

                public void SetPhoneNumberDigits( string digits )
                { 
                    _CellPhoneNumber.Number = digits;
                    _CellPhoneNumber.NumberFormatted = digits.AsPhoneNumber( );
                }

                private RockMobileUser( )
                {
                    SetDefaultPersonValues( );
                }

                /// <summary>
                /// Resets all the values related to the Person, but won't reset things like
                /// IsFirstRun
                /// </summary>
                void SetDefaultPersonValues( )
                {
                    Person = new Person();

                    PrimaryFamily = new Group();

                    PrimaryAddress = new GroupLocation();
                    PrimaryAddress.GroupLocationTypeValueId = PrivateGeneralConfig.GroupLocationTypeHomeValueId;
                    PrimaryAddress.IsMappedLocation = true;

                    // for the address location, default the country to the built in country code.
                    PrimaryAddress.Location = new Rock.Client.Location();
                    PrimaryAddress.Location.Country = MobileApp.Shared.Config.GeneralConfig.CountryCode;

                    _CellPhoneNumber = new PhoneNumber( );
                    SetPhoneNumberDigits( "" );

                    IsBaptised = false;
                    IsERA = false;
                    IsGiving = false;
                    TakenStartingPoint = false;
                    IsMember = false;
                    IsServing = false;
                    IsPeerLearning = false;
                    IsMentored = false;
                    IsTeaching = false;

                    // Don't reset the viewing campus, because we force them to pick one at
                    // first run, and don't want to lose their choice.
                    //ViewingCampus = 0;
                }

                public string PreferredName( )
                {
                    if( string.IsNullOrEmpty( Person.NickName ) == false )
                    {
                        return Person.NickName;
                    }
                    else
                    {
                        return Person.FirstName;
                    }
                }

                /// <summary>
                /// Attempts to login with whatever account type is bound.
                /// </summary>
                public void Login( HttpRequest.RequestResult loginResult )
                {
                    switch ( AccountType )
                    {
                        case BoundAccountType.Rock:
                        {
                            LoggedIn = true;

                            loginResult( System.Net.HttpStatusCode.NoContent, "" );
                            break;
                        }

                        case BoundAccountType.Facebook:
                        {
                            LoggedIn = true;

                            loginResult( System.Net.HttpStatusCode.NoContent, "" );
                            break;
                        }

                        default:
                        {
                            throw new Exception( "No account type bound, so I don't know how to log you in to Rock. Call Bind*Account first." );
                        }
                    }
                }

                /// <summary>
                /// Called by us when done attempting to bind an account to Rock. For example,
                /// if a user wants to login via Facebook, we first have to get authorization FROM facebook,
                /// which means that could fail, and thus BindResult will return false.
                /// </summary>
                public delegate void BindResult( bool success );

                public void BindRockAccount( string username, string password, BindResult bindResult )
                {
                    MobileAppApi.Login( username, password, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                    {
                        // if we received Ok (nocontent), we're logged in.
                        if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                        {
                            UserID = username;
                            RockPassword = password;

                            AccessToken = "";

                            AccountType = BoundAccountType.Rock;

                            // save!
                            SaveToDevice( );

                            bindResult( true);
                        }
                        else
                        {
                            bindResult( false );
                        }
                    });
                }

#if !__WIN__
                public delegate void GetUserCredentials( string fromUri, Facebook.FacebookClient session );
                public void BindFacebookAccount( GetUserCredentials getCredentials )
                {
                    Dictionary<string, object> loginRequest = FacebookManager.Instance.CreateLoginRequest( );

                    Facebook.FacebookClient fbSession = new Facebook.FacebookClient( );
                    string requestUri = fbSession.GetLoginUrl( loginRequest ).AbsoluteUri;

                    getCredentials( requestUri, fbSession );
                }

                public bool HasFacebookResponse( string response, Facebook.FacebookClient session )
                {
                    // if true is returned, there IS a response, so the caller can call the below FacebookCredentialResult
                    Facebook.FacebookOAuthResult oauthResult;
                    return session.TryParseOAuthCallbackUrl( new Uri( response ), out oauthResult );
                }

                public void FacebookCredentialResult( string response, Facebook.FacebookClient session, BindResult result )
                {
                    // make sure we got a valid access token
                    Facebook.FacebookOAuthResult oauthResult;
                    if( session.TryParseOAuthCallbackUrl (new Uri ( response ), out oauthResult) == true )
                    {
                        if ( oauthResult.IsSuccess )
                        {
                            // now attempt to get their basic info
                            Facebook.FacebookClient fbSession = new Facebook.FacebookClient( oauthResult.AccessToken );

                            string infoRequest = FacebookManager.Instance.CreateInfoRequest( );

                            fbSession.GetTaskAsync( infoRequest ).ContinueWith( t =>
                                {
                                    // if there was no problem, we are logged in and can send this up to Rock
                                    if ( t.IsFaulted == false && t.Exception == null )
                                    {
                                        // now login via rock with the facebook credentials to verify we're good
                                        RockApi.Post_Auth_FacebookLogin( t.Result, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                                            {
                                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                                {
                                                    UserID = "facebook_" + FacebookManager.Instance.GetUserID( t.Result );
                                                    RockPassword = "";

                                                    AccessToken = oauthResult.AccessToken;

                                                    AccountType = BoundAccountType.Facebook;

                                                    // save!
                                                    SaveToDevice( );

                                                    result( true );
                                                }
                                                else
                                                {
                                                    result( false );
                                                }
                                            });
                                    }
                                    else
                                    {
                                        // didn't work out.
                                        result( false );
                                    }
                                } );
                        }
                        else
                        {
                            result( false );
                        }
                    }
                    else
                    {
                        // didn't work out.
                        result( false );
                    }
                }

                void SyncFacebookInfoToPerson( object infoObj )
                {
                    Person.FirstName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.NickName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.LastName = FacebookManager.Instance.GetLastName( infoObj );
                    Person.Email = FacebookManager.Instance.GetEmail( infoObj );
                }
#endif

                public void LogoutAndUnbind( )
                {
                    // clear the person and take a blank copy
                    SetDefaultPersonValues( );

                    // remove their profile picture
                    FileCache.Instance.RemoveFile( PrivateSpringboardConfig.ProfilePic );

                    LoggedIn = false;
                    AccountType = BoundAccountType.None;

                    UserID = "";
                    RockPassword = "";
                    AccessToken = "";

                    // save!
                    SaveToDevice( );
                }

                public void GetPersonData( HttpRequest.RequestResult onResult )
                {
                    MobileAppApi.GetPersonData( UserID,
                        delegate ( MobileAppApi.PersonData personData )
                        {
                            if( personData != null )
                            {
                                // take the person (clear out the cell numbers since we manage those seperately)
                                Person = personData.Person;
                                Person.PhoneNumbers = null;

                                // search for a phone number (which should match whatever we already have, unless this is a new login)
                                if( personData.CellPhoneNumber != null )
                                {
                                    _CellPhoneNumber = personData.CellPhoneNumber;

                                    HasCellNumber = true;
                                }
                                else
                                {
                                    // no longer has a phone number, so clear it
                                    _CellPhoneNumber = new PhoneNumber( );

                                    SetPhoneNumberDigits( "" );

                                    HasCellNumber = false;
                                }

                                // we're always safe to take family--it cannot be null
                                PrimaryFamily = personData.Family;

                                // only take the address if it's valid. otherwise, we want
                                // to use the default, empty one.
                                if( personData.FamilyAddress != null )
                                {
                                    PrimaryAddress = personData.FamilyAddress;
                                }

                                // set the person's current actions
                                IsBaptised = personData.IsBaptised;
                                IsERA = personData.IsERA;
                                IsGiving = personData.IsGiving;
                                TakenStartingPoint = personData.TakenStartingPoint;
                                IsMember = personData.IsMember;
                                IsServing = personData.IsServing;
                                IsPeerLearning = personData.IsPeerLearning;
                                IsMentored = personData.IsMentored;
                                IsTeaching = personData.IsTeaching;

                                // save!
                                SaveToDevice( );

                                if( onResult != null )
                                {
                                    onResult( System.Net.HttpStatusCode.OK, "" );
                                }
                            }
                            else
                            {
                                if( onResult != null )
                                {
                                    onResult( System.Net.HttpStatusCode.BadRequest, "" );
                                }
                            }
                        });
                }

                public void UpdateProfile( HttpRequest.RequestResult profileResult )
                {
                    ApplicationApi.UpdatePerson( Person, 0, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                SaveToDevice( );
                            }

                            if( profileResult != null )
                            {
                                profileResult( statusCode, statusDescription );
                            }
                        });
                }

                public void UpdateOrAddPhoneNumber( HttpRequest.RequestResult phoneResult )
                {   
                    // if they currently have a cell number, OR their number is non-empty
                    if( HasCellNumber == true || string.IsNullOrEmpty( _CellPhoneNumber.Number ) == false )
                    {
                        // if they DON'T have a cell number, then yes, this will be a new number.
                        bool addNewPhoneNumber = !HasCellNumber;

                        MobileAppApi.UpdateOrAddPhoneNumber( Person, _CellPhoneNumber, addNewPhoneNumber, 
                            delegate(System.Net.HttpStatusCode statusCode, string statusDescription, PhoneNumber model )
                            {
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                {
                                    // if we got back a model with an actual number, it's updated. 
                                    if( model != null && string.IsNullOrEmpty( model.Number ) == false )
                                    {
                                        _CellPhoneNumber = model;

                                        HasCellNumber = true;
                                    }
                                    else
                                    {
                                        // otherwise the user is deleting it
                                        _CellPhoneNumber = new PhoneNumber( );
                                        SetPhoneNumberDigits( "" );

                                        HasCellNumber = false;
                                    }

                                    SaveToDevice( );
                                }

                                if ( phoneResult != null )
                                {
                                    phoneResult( statusCode, statusDescription );
                                }
                            } );
                    }
                    else
                    {
                        if ( phoneResult != null )
                        {
                            phoneResult( System.Net.HttpStatusCode.OK, "" );
                        }
                    }
                }

                public void UpdateHomeCampus( HttpRequest.RequestResult result )
                {
                    // unlike Profile and Address, it's possible they haven't selected a campus, in which
                    // case we don't want to update it.
                    ApplicationApi.UpdateHomeCampus( PrimaryFamily, 0,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                SaveToDevice( );
                            }

                            if ( result != null )
                            {
                                result( statusCode, statusDescription );
                            }
                        } );
                }

                public void UpdateAddress( HttpRequest.RequestResult addressResult )
                {
                    // fire it off
                    ApplicationApi.UpdateFamilyAddress( PrimaryFamily, PrimaryAddress,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                SaveToDevice( );
                            }

                            if( addressResult != null )
                            {
                                addressResult( statusCode, statusDescription );
                            }
                        });
                }

                public void UploadSavedProfilePicture( HttpRequest.RequestResult result )
                {
                    // first open the image.
                    MemoryStream imageStream = (MemoryStream) FileCache.Instance.LoadFile( PrivateSpringboardConfig.ProfilePic );

                    // verify it's valid and not corrupt, or otherwise unable to load. If it is, we'll stop here.
                    if ( imageStream != null )
                    {
                        // if upload is called, the profile image implicitely becomes dirty.
                        // that way if it fails, we can know to sync it on next run.
                        ProfileImageDirty = true;

                        ApplicationApi.UploadSavedProfilePicture( Person, imageStream, 0, 
                            delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                            {
                                // now we know that the profile image group was updated correctly, and that's the last step
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                {
                                    // so now we can finally flag everything as good
                                    ProfileImageDirty = false;

                                    SaveToDevice( );
                                }

                                if ( result != null )
                                {
                                    result( statusCode, statusDescription );
                                }
                            } );
                    }
                    else
                    {
                        // the picture failed to save, so all we can do is say it was fine and
                        // that it is no longer dirty. Both things are true.
                        ProfileImageDirty = false;
                        result( System.Net.HttpStatusCode.OK, "" );
                    }
                }

                public void SaveProfilePicture( MemoryStream imageStream )
                {
                    FileCache.Instance.SaveFile( imageStream, PrivateSpringboardConfig.ProfilePic, FileCache.CacheFileNoExpiration );

                    // now we have a picture!
                    HasProfileImage = true;

                    SaveToDevice( );
                }

                public void TryDownloadProfilePicture( uint dimensionSize, HttpRequest.RequestResult profilePictureResult )
                {
                    switch ( AccountType )
                    {
                        case BoundAccountType.Facebook:
                        {
                            // grab the actual image
                            string facebookID = UserID.Substring( UserID.IndexOf( "_" ) + 1 ); //chop off the "facebook_" prefix we add.
                            string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", facebookID, "large", AccessToken);
                            RestRequest request = new RestRequest( Method.GET );

                            // get the raw response
                            HttpRequest webRequest = new HttpRequest();
                            webRequest.ExecuteAsync( profilePictureUrl, request, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, byte[] model )
                                {
                                    // it worked out ok!
                                    if ( Util.StatusInSuccessRange( statusCode ) == true )
                                    {
                                        MemoryStream imageStream = new MemoryStream( model );
                                        SaveProfilePicture( imageStream );
                                        imageStream.Dispose( );
                                    }

                                    // notify the caller
                                    if ( profilePictureResult != null )
                                    {
                                        profilePictureResult( statusCode, statusDescription );
                                    }

                                } );
                            break;
                        }

                        case BoundAccountType.Rock:
                        {
                            if ( Person.PhotoId != null )
                            {
                                RockApi.Get_GetImage( Person.PhotoId.Value, dimensionSize, dimensionSize, 
                                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription, MemoryStream imageStream )
                                    {
                                        if ( Util.StatusInSuccessRange( statusCode ) == true )
                                        {
                                            // if successful, update the file on disk.
                                            SaveProfilePicture( imageStream );
                                        }

                                        // notify the caller
                                        if ( profilePictureResult != null )
                                        {
                                            profilePictureResult( statusCode, statusDescription );
                                        }
                                    } );
                            }
                            // they might not have a photo ID, and that's ok. In that case, just callback with finished.
                            else
                            {
                                // notify the caller
                                if ( profilePictureResult != null )
                                {
                                    profilePictureResult( System.Net.HttpStatusCode.NotFound, string.Empty );
                                }
                            }
                            break;
                        }
                    }
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), MOBILEUSER_DATA_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        string json = JsonConvert.SerializeObject( this );
                        writer.WriteLine( json );
                    }
                }

                public void LoadFromDevice(  )
                {
                    // at startup, this should be called to allow current objects to be restored.
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), MOBILEUSER_DATA_FILENAME);

                    // if the file exists
                    if(System.IO.File.Exists(filePath) == true)
                    {
                        // read it
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            // guard against corrupt data
                            string json = reader.ReadLine();
                            if ( json != null )
                            {
                                try
                                {
                                    // guard against the mobile user model changing and the user having old data
                                    RockMobileUser loadedInstance = JsonConvert.DeserializeObject<RockMobileUser>( json ) as RockMobileUser;
                                    if( _Instance.Version == loadedInstance.Version )
                                    {
                                        // temp upgrade: check for their cell phone number so we can set the HasCellPhoneNumber flag.
                                        if( string.IsNullOrEmpty( loadedInstance.CellPhoneNumberDigits( ) ) == false )
                                        {
                                            loadedInstance.HasCellNumber = true;
                                        }
                                        
                                        // as long as the versions match, we can take this.
                                        // We really don't want to bump up the version unless we HAVE to,
                                        // because it'll force them to re-sign in.
                                        _Instance = loadedInstance;
                                    }
                                }
                                catch( Exception e )
                                {
                                    Rock.Mobile.Util.Debug.WriteLine( string.Format( "{0}", e ) );
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
