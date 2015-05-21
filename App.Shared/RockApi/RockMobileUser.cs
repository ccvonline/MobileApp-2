using System;
using Rock.Client;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using App.Shared.Config;
using System.Collections.Generic;
using Facebook;
using RestSharp;
using Rock.Mobile.Util.Strings;
using System.Runtime.Serialization.Formatters.Binary;
using App.Shared.PrivateConfig;
using Rock.Mobile.IO;

namespace App
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
                /// If true, they've already seen the note tutorial and don't need to see it again.
                /// </summary>
                /// <value><c>true</c> if note tutorial shown; otherwise, <c>false</c>.</value>
                public int NoteTutorialShownCount { get; set; }

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
                Rock.Client.PhoneNumber _CellPhoneNumber = null;

                public string CellPhoneNumberDigits( )
                {
                    return _CellPhoneNumber != null ? _CellPhoneNumber.Number : "";
                }

                public void SetPhoneNumberDigits( string digits )
                { 
                    if ( _CellPhoneNumber == null )
                    {
                        _CellPhoneNumber = new Rock.Client.PhoneNumber();
                    }

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

                    _CellPhoneNumber = null;

                    ViewingCampus = RockGeneralData.Instance.Data.Campuses[ 0 ].Id;

                    // for the address location, default the country to the built in country code.
                    PrimaryAddress.Location = new Location();
                    PrimaryAddress.Location.Country = App.Shared.Config.GeneralConfig.CountryCode;

                    LastSyncdPersonJson = JsonConvert.SerializeObject( Person );
                    LastSyncdFamilyJson = JsonConvert.SerializeObject( PrimaryFamily );
                    LastSyncdAddressJson = JsonConvert.SerializeObject( PrimaryAddress );
                    LastSyncdCellPhoneNumberJson = "";
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
                    RockApi.Instance.Login( username, password, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
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

                public delegate void GetUserCredentials( string fromUri, FacebookClient session );
                public void BindFacebookAccount( GetUserCredentials getCredentials )
                {
                    Dictionary<string, object> loginRequest = FacebookManager.Instance.CreateLoginRequest( );

                    FacebookClient fbSession = new FacebookClient( );
                    string requestUri = fbSession.GetLoginUrl( loginRequest ).AbsoluteUri;

                    getCredentials( requestUri, fbSession );
                }

                public bool HasFacebookResponse( string response, FacebookClient session )
                {
                    // if true is returned, there IS a response, so the caller can call the below FacebookCredentialResult
                    FacebookOAuthResult oauthResult;
                    return session.TryParseOAuthCallbackUrl( new Uri( response ), out oauthResult );
                }

                public void FacebookCredentialResult( string response, FacebookClient session, BindResult result )
                {
                    // make sure we got a valid access token
                    FacebookOAuthResult oauthResult;
                    if( session.TryParseOAuthCallbackUrl (new Uri ( response ), out oauthResult) == true )
                    {
                        if ( oauthResult.IsSuccess )
                        {
                            // now attempt to get their basic info
                            FacebookClient fbSession = new FacebookClient( oauthResult.AccessToken );
                            string infoRequest = FacebookManager.Instance.CreateInfoRequest( );

                            fbSession.GetTaskAsync( infoRequest ).ContinueWith( t =>
                                {
                                    // if there was no problem, we are logged in and can send this up to Rock
                                    if ( t.IsFaulted == false || t.Exception == null )
                                    {
                                        // now login via rock with the facebook credentials to verify we're good
                                        RockApi.Instance.LoginFacebook( t.Result, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
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

                public void LogoutAndUnbind( )
                {
                    // clear the person and take a blank copy
                    SetDefaultPersonValues( );

                    LoggedIn = false;
                    AccountType = BoundAccountType.None;

                    UserID = "";
                    RockPassword = "";
                    AccessToken = "";

                    RockApi.Instance.Logout( );

                    // save!
                    SaveToDevice( );
                }

                void SyncFacebookInfoToPerson( object infoObj )
                {
                    Person.FirstName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.NickName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.LastName = FacebookManager.Instance.GetLastName( infoObj );
                    Person.Email = FacebookManager.Instance.GetEmail( infoObj );
                }

                public void GetProfileAndCellPhone( HttpRequest.RequestResult<Rock.Client.Person> profileResult )
                {
                    RockApi.Instance.GetProfile( UserID, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, Rock.Client.Person model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // on retrieval, convert this version for dirty compares later
                                Person = model;

                                // store their contact phone number seperately so we don't
                                // get conflicts with syncing.
                                _CellPhoneNumber = TryGetPhoneNumber( PrivateGeneralConfig.CellPhoneValueId );
                                Person.PhoneNumbers = null;

                                LastSyncdCellPhoneNumberJson = _CellPhoneNumber != null ? JsonConvert.SerializeObject( _CellPhoneNumber ) : "";
                                LastSyncdPersonJson = JsonConvert.SerializeObject( Person );

                                // save!
                                SaveToDevice( );
                            }

                            // notify the caller
                            if( profileResult != null )
                            {
                                profileResult( statusCode, statusDescription, model );
                            }
                        });
                }

                public void UpdateProfile( HttpRequest.RequestResult profileResult )
                {
                    RockApi.Instance.UpdateProfile( Person, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update our json so we have a match and don't try to update again later.
                                LastSyncdPersonJson = JsonConvert.SerializeObject( Person );
                            }

                            // whether we succeeded in updating with the server or not, save to disk.
                            SaveToDevice( );

                            if( profileResult != null )
                            {
                                profileResult( statusCode, statusDescription );
                            }
                        });
                }

                /// <summary>
                /// Returns the phone number matching phoneTypeId, or an empty one if no match is found.
                /// </summary>
                /// <returns>The phone number.</returns>
                /// <param name="phoneTypeId">Phone type identifier.</param>
                Rock.Client.PhoneNumber TryGetPhoneNumber( int phoneTypeId )
                {
                    Rock.Client.PhoneNumber requestedNumber = null;

                    // if the user has phone numbers
                    if ( Person.PhoneNumbers != null )
                    {
                        // get an enumerator
                        IEnumerator<Rock.Client.PhoneNumber> enumerator = Person.PhoneNumbers.GetEnumerator( );
                        enumerator.MoveNext( );

                        // search for the phone number type requested
                        while ( enumerator.Current != null )
                        {
                            Rock.Client.PhoneNumber phoneNumber = enumerator.Current as Rock.Client.PhoneNumber;

                            // is this the right type?
                            if ( phoneNumber.NumberTypeValueId == phoneTypeId )
                            {
                                requestedNumber = phoneNumber;
                                break;
                            }
                            enumerator.MoveNext( );
                        }
                    }

                    return requestedNumber;
                }

                public void UpdateOrAddPhoneNumber( HttpRequest.RequestResult phoneResult )
                {
                    // we know if it's a new phone number based on whether our sync'd cell number is blank or not.
                    bool addNewPhoneNumber = string.IsNullOrEmpty( LastSyncdCellPhoneNumberJson ) ? true : false;

                    // send it to the server
                    RockApi.Instance.UpdatePhoneNumber( Person, _CellPhoneNumber, addNewPhoneNumber, 

                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // since the phone number is tied to the person, if this was successful, consider our 
                                LastSyncdCellPhoneNumberJson = JsonConvert.SerializeObject( _CellPhoneNumber );
                            }

                            // save either way!
                            SaveToDevice( );

                            if( phoneResult != null )
                            {
                                phoneResult( statusCode, statusDescription );
                            }

                        } );
                }

                public void GetFamilyAndAddress( HttpRequest.RequestResult< List<Rock.Client.Group> > addressResult )
                {
                    // for the address (which implicitly is their primary residence address), first get all group locations associated with them
                    RockApi.Instance.GetFamiliesOfPerson( Person, 

                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Group> model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // find what we'll consider their primary address.

                                // look thru each group (family)
                                foreach( Rock.Client.Group personGroup in model )
                                {
                                    // If we find a groupType of family, that should be their primary family.
                                    if( personGroup.GroupType.Guid.ToString( ).ToLower( ) == Rock.Client.SystemGuid.GroupType.GROUPTYPE_FAMILY.ToLower( ) )
                                    {
                                        // store the family
                                        PrimaryFamily = personGroup;

                                        // look at each location within the family
                                        foreach( Rock.Client.GroupLocation groupLocation in personGroup.GroupLocations )
                                        {
                                            // find their "Home Location" within the family group type.
                                            if( groupLocation.GroupLocationTypeValue.Guid.ToString( ).ToLower( ) == Rock.Client.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.ToLower( ) )
                                            {
                                                PrimaryAddress = groupLocation;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // on retrieval, convert this version for dirty compares later
                                LastSyncdAddressJson = JsonConvert.SerializeObject( PrimaryAddress );
                                LastSyncdFamilyJson = JsonConvert.SerializeObject( PrimaryFamily );

                                // save!
                                SaveToDevice( );
                            }

                            // notify the caller
                            if( addressResult != null )
                            {
                                addressResult( statusCode, statusDescription, model );
                            }
                        });
                }

                public void UpdateHomeCampus( HttpRequest.RequestResult result )
                {
                    // unlike Profile and Address, it's possible they haven't selected a campus, in which
                    // case we don't want to update it.
                    RockApi.Instance.UpdateHomeCampus( PrimaryFamily, 

                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update our json so we have a match and don't try to update again later.
                                LastSyncdFamilyJson = JsonConvert.SerializeObject( PrimaryFamily );
                            }

                            // whether we succeeded in updating with the server or not, save to disk.
                            SaveToDevice( );

                            if ( result != null )
                            {
                                result( statusCode, statusDescription );
                            }
                        } );
                }

                public void UpdateAddress( HttpRequest.RequestResult addressResult )
                {
                    // fire it off
                    RockApi.Instance.UpdateAddress( PrimaryFamily, PrimaryAddress, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update our json so we have a match and don't try to update again later.
                                LastSyncdAddressJson = JsonConvert.SerializeObject( PrimaryAddress );
                            }

                            // whether we succeeded in updating with the server or not, save to disk.
                            SaveToDevice( );

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
                        // this is a big process. The profile picture being updated also requires the user's
                        // profile be updated AND they need to be placed into a special group.
                        // So, until ALL THOSE succeed in order, we will not consider the profile image "clean"


                        // if upload is called, the profile image implicitely becomes dirty.
                        // that way if it fails, we can know to sync it on next run.
                        ProfileImageDirty = true;

                        // attempt to upload it
                        RockApi.Instance.UpdateProfilePicture( imageStream, 

                            delegate( System.Net.HttpStatusCode statusCode, string statusDesc, int photoId )
                            {
                                // free the stream
                                imageStream.Dispose( );

                                // if the upload went ok
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                {
                                    // now update the profile
                                    Person.PhotoId = photoId;

                                    // attempt to sync the profile
                                    UpdateProfile( 
                                        delegate ( System.Net.HttpStatusCode profileStatusCode, string profileStatusDesc )
                                        {
                                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( profileStatusCode ) == true )
                                            {
                                                // now (and only now) that we know the profile was updated correctly,
                                                // we can update the image group.
                                                RockApi.Instance.UpdateProfileImageGroup( Person, delegate ( System.Net.HttpStatusCode resultCode, string resultDesc )
                                                    {
                                                        // now we know that the profile image group was updated correctly, and that's the last step
                                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( resultCode ) == true )
                                                        {
                                                            // so now we can finally flag everything as good
                                                            ProfileImageDirty = false;
                                                        }

                                                        if ( result != null )
                                                        {
                                                            result( statusCode, statusDesc );
                                                        }
                                                    } );
                                            }
                                            else
                                            {
                                                if ( result != null )
                                                {
                                                    result( statusCode, statusDesc );
                                                }
                                            }
                                        } );
                                }
                                else
                                {
                                    if ( result != null )
                                    {
                                        result( statusCode, statusDesc );
                                    }
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
                                RockApi.Instance.GetProfilePicture( Person.PhotoId.ToString( ), dimensionSize, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, MemoryStream imageStream )
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
                            break;
                        }
                    }
                }

                public void SyncDirtyObjects( HttpRequest.RequestResult resultCallback )
                {
                    // check to see if our person object OR address object changed. If our original json
                    // created at a point when we know we were sync'd with the server
                    // no longer matches our object, we should update it.
                    string currPersonJson = JsonConvert.SerializeObject( Person );
                    string currPhoneNumbersJson = _CellPhoneNumber != null ? JsonConvert.SerializeObject( _CellPhoneNumber ) : "";
                    string currFamilyJson = JsonConvert.SerializeObject( PrimaryFamily );
                    string currAddressJson = JsonConvert.SerializeObject( PrimaryAddress );

                    // assume things will work
                    System.Net.HttpStatusCode returnCode = System.Net.HttpStatusCode.OK;

                    if( string.Compare( LastSyncdPersonJson, currPersonJson ) != 0 || 
                        string.Compare( LastSyncdCellPhoneNumberJson, currPhoneNumbersJson ) != 0 ||
                        string.Compare( LastSyncdFamilyJson, currFamilyJson ) != 0 ||
                        string.Compare( LastSyncdAddressJson, currAddressJson ) != 0 ||
                        ProfileImageDirty == true )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "RockMobileUser: Syncing Profile" );

                        // PROFILE
                        UpdateProfile( delegate( System.Net.HttpStatusCode profileCode, string profileResult )
                            {
                                // if there's a failure, flag it and continue so the caller can know there was a problem.
                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( profileCode ) == false )
                                {
                                    returnCode = System.Net.HttpStatusCode.BadRequest;
                                }

                                // PHONE NUMBER
                                UpdateOrAddPhoneNumber( delegate( System.Net.HttpStatusCode phoneCode, string phoneResult )
                                {
                                    // if there's a failure, flag it and continue so the caller can know there was a problem.
                                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( phoneCode ) == false )
                                    {
                                        returnCode = System.Net.HttpStatusCode.BadRequest;
                                    }

                                    // ADDRESS
                                    UpdateAddress( delegate( System.Net.HttpStatusCode addressCode, string addressResult )
                                        {
                                            // if there's a failure, flag it and continue so the caller can know there was a problem.
                                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( addressCode ) == false )
                                            {
                                                returnCode = System.Net.HttpStatusCode.BadRequest;
                                            }

                                            // HOME CAMPUS
                                            UpdateHomeCampus( delegate( System.Net.HttpStatusCode campusCode, string campusResult )
                                                {
                                                    // if there's a failure, flag it and continue so the caller can know there was a problem.
                                                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( campusCode ) == false )
                                                    {
                                                        returnCode = System.Net.HttpStatusCode.BadRequest;
                                                    }
                                                    // If needed, make other calls here, chained, and finally.


                                                    // PROFILE PICTURE SYNCING SHOULD ALWAYS BE LAST.
                                                    // add a sanity check for profile image. it's too large to simply send up
                                                    // just because the rest of the stuff needs updating.
                                                    if ( ProfileImageDirty == true )
                                                    {
                                                            UploadSavedProfilePicture( 
                                                                delegate( System.Net.HttpStatusCode pictureCode, string pictureDesc )
                                                                {
                                                                    // if there's a failure, flag it and continue so the caller can know there was a problem.
                                                                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( pictureCode ) == false )
                                                                    {
                                                                        returnCode = System.Net.HttpStatusCode.BadRequest;
                                                                    }

                                                                    // return finished. just tell them OK, because it really doesn't matter if it worked or not.
                                                                    resultCallback( returnCode, "" );
                                                                } );
                                                        
                                                    }
                                                    else
                                                    {
                                                        // return finished. just tell them OK, because it really doesn't matter if it worked or not.
                                                            resultCallback( returnCode, "" );
                                                    }
                                                });
                                        });
                                });
                            });
                    }
                    else
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "RockMobileUser: No sync needed." );

                        // nothing need be sync'd, call back with ok.
                        resultCallback( System.Net.HttpStatusCode.OK, "Success" );
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
