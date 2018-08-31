using System;
using Rock.Mobile.Network;
using System.Net;
using System.Collections.Generic;
using Rock.Mobile;
using MobileApp.Shared.Network;
using MobileApp.Shared.PrivateConfig;
using System.Linq;
using Rock.Client;
using MobileApp.Shared.Strings;
using Rock.Mobile.Util.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MobileApp
{
    /// <summary>
    /// Implements api methods that are specific to MobileApp.
    /// </summary>
    public static class MobileAppApi
    {
        const string EndPoint_LaunchData = "api/MobileApp/LaunchData";
        public class LaunchData
        {
            public List<Rock.Client.Campus> Campuses { get; set; }
            public List<KeyValuePair<string, int>> PrayerCategories { get; set; }
            public int MobileAppVersion { get; set; }
        }
        public static void Get_LaunchData( HttpRequest.RequestResult<LaunchData> resultHandler )
        {
            // pull down launch data for the mobile app
            RockApi.Get_CustomEndPoint<LaunchData>( RockApi.BaseUrl + EndPoint_LaunchData, resultHandler );
        }

        const string EndPoint_Campaign = "api/PersonalizationEngine/Campaign/?";
        public static void GetPECampaign( int? personId, HttpRequest.RequestResult<JArray> resultHandler )
        {
            // if they're logged in, add their personId
            string endPoint = EndPoint_Campaign;
            if( personId.HasValue )
            {
                endPoint += "personId=" + personId.Value + "&";
            }

            RockApi.Get_CustomEndPoint<JArray>( RockApi.BaseUrl + endPoint + "campaignTypeList=MobileApp", resultHandler );
        }

        public static void GetNews( HttpRequest.RequestResult< List<Rock.Client.ContentChannelItem> > resultHandler )
        {
            string oDataFilter = "";

            // if they're a developer, pull down pending and future start date items as well.
            if( RockLaunchData.Instance.Data.DeveloperModeEnabled == true )
            {
                oDataFilter = string.Format( "?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637' and " +
                    "(Status eq '2' or Status eq '1') and " +
                    "(ExpireDateTime ge DateTime'{0}' or ExpireDateTime eq null)&LoadAttributes=True", 
                    DateTime.Now.ToString( "s" ) );
            }
            else
            {
                oDataFilter = string.Format( "?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637' and " +
                    "Status eq '2' and (StartDateTime le DateTime'{0}' or StartDateTime eq null) and " +
                    "(ExpireDateTime ge DateTime'{0}' or ExpireDateTime eq null)&LoadAttributes=True", 
                    DateTime.Now.ToString( "s" ) );
            }

            RockApi.Get_ContentChannelItems( oDataFilter, resultHandler );
        }

        public static void GetPrayerCategories( HttpRequest.RequestResult<List<Rock.Client.Category>> resultHandler )
        {
            RockApi.Get_Categories_GetChildren_1( resultHandler );
        }


        const string EndPoint_GroupsByLocation = "api/MobileApp/GroupsByLocation";
        public delegate void OnGroupsByLocationResult( List<GroupSearchResult> groupResults );
        public class GroupSearchResult
        {
            public int Id;
            public double Latitude;
            public double Longitude;
            public string Name;
            public double DistanceFromSource;
            public string MeetingTime;
            public string Filters;
        };
        public static void GetPublicGroupsByLocation( int groupTypeId, int locationId, int skip, int top, OnGroupsByLocationResult resultHandler )
        {
            string queryParams = string.Format( "?locationId={0}&groupTypeId={1}&skip={2}&top={3}&publicOnly=true", locationId, groupTypeId, skip, top );
            RockApi.Get_CustomEndPoint<JArray>( RockApi.BaseUrl + EndPoint_GroupsByLocation + queryParams, delegate( HttpStatusCode statusCode, string statusDescription, JArray model )
             {
				 if( Util.StatusInSuccessRange( statusCode ) == true )
				 {
                    resultHandler( model.ToObject<List<GroupSearchResult>>( ) );
				 }
				 else
				 {
					resultHandler( null );
				 }
             } );
        }

        const string EndPoint_Login = "api/MobileApp/Login";
        public static void Login( string username, string password, HttpRequest.RequestResult resultHandler )
        {
            var loginObj = new
            {
                Username = username,
                Password = password
            };

            RockApi.Post_CustomEndPoint( RockApi.BaseUrl + EndPoint_Login, loginObj, resultHandler );
        }

        const string EndPoint_PersonData = "api/MobileApp/PersonData?userID=";
        public delegate void OnPersonDataResult( PersonData personData, HttpStatusCode statusCode );
        public class PersonData
        {
            public Person Person { get; set; }
            public PhoneNumber CellPhoneNumber { get; set; }
            public Group Family { get; set; }
            public GroupLocation FamilyAddress { get; set; }

            public bool IsBaptised { get; set; }
            public bool IsERA { get; set; }
            public bool IsGiving { get; set; }
            public bool TakenStartingPoint { get; set; }
            public bool IsMember { get; set; }
            public bool IsServing { get; set; }
            public bool IsPeerLearning { get; set; }
            public bool IsMentored { get; set; }
            public bool IsTeaching { get; set; }
        }

        public static void GetPersonData( string userID, OnPersonDataResult onResultHandler )
        {
            RockApi.Get_CustomEndPoint<PersonData>( RockApi.BaseUrl + EndPoint_PersonData + System.Net.WebUtility.UrlEncode( userID ), delegate ( HttpStatusCode statusCode, string statusDescription, PersonData model )
            {
                if( Util.StatusInSuccessRange( statusCode ) == true )
                {
                    onResultHandler( model, statusCode );
                }
                else
                {
                    onResultHandler( null, statusCode );
                }
            } );
        }


        const string EndPoint_GroupInfo = "api/MobileApp/GroupInfo?groupId=";
        public delegate void OnGroupSummaryResult( GroupInfo groupInfo, System.IO.MemoryStream leaderPhoto, System.IO.MemoryStream groupPhoto );
        public class GroupInfo
        {
            public string Description { get; set; }
            public string ChildcareDesc { get; set; }

            public Guid FamilyPhotoGuid { get; set; }
            public Guid GroupPhotoGuid { get; set; }

            public string Filters { get; set; }
        }

        public static void GetGroupSummary( int groupId, OnGroupSummaryResult onResultHandler )
        {
            System.IO.MemoryStream familyPhoto = null;

            // first, get the group info
            RockApi.Get_CustomEndPoint<GroupInfo>( RockApi.BaseUrl + EndPoint_GroupInfo + groupId, delegate( HttpStatusCode statusCode, string statusDescription, GroupInfo model ) 
                {
                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                    {
                        // get an image size appropriate for the device.
                        uint imageRes = (uint)Rock.Mobile.Graphics.Util.UnitToPx( 512 );
                        RockApi.Get_GetImage( model.FamilyPhotoGuid, imageRes, null, delegate(HttpStatusCode imageCode, string imageDescription, System.IO.MemoryStream imageStream ) 
                            {
                                // if the image didn't return successfully, just null it out.
                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( imageCode ) == false )
                                {
                                    imageStream = null;
                                }
                                else
                                {
                                    // otherwise, copy it. We must copy it because the imageStream
                                    // will be going out of scope when we make the next Get_GetImage async call.
                                    familyPhoto = new System.IO.MemoryStream( );

                                    imageStream.CopyTo( familyPhoto );
                                    familyPhoto.Position = 0;
                                }

                                // now try for the group photo
                                RockApi.Get_GetImage( model.GroupPhotoGuid, imageRes, null, delegate(HttpStatusCode groupImageCode, string groupImageDesc, System.IO.MemoryStream groupImageStream )
                                    {
                                        // if the image didn't return successfully, just null it out.
                                        if( Rock.Mobile.Network.Util.StatusInSuccessRange( groupImageCode ) == false )
                                        {
                                            groupImageStream = null;
                                        }

                                        // JHM Note: Enable this to debug the image size issue that's on certain devices.
                                        //groupImageStream.CopyTo( leaderPhoto );
                                        //groupImageStream.Position = 0;
                                        //leaderPhoto.Position = 0;

                                        // return ok whether they have a images or not (since they're not required)
                                        onResultHandler( model, familyPhoto, groupImageStream );
                                    });

                                
                            });
                    }
                    // GROUP INFO fail
                    else
                    {
                        // return ok whether they have an image or not (since it's not required)
                        onResultHandler( null, null, null );
                    }
                });
        }

        class GroupRegModel
        {
            public int RequestedGroupId;

            public string FirstName;
            public string LastName;
            public string SpouseName;
            public string Email;
            public string Phone;
        }

        const string EndPoint_GroupRegistration = "api/MobileApp/GroupRegistration/";
        public static void JoinGroup( int groupId, string firstName, string lastName, string spouseName, string email, string phone, HttpRequest.RequestResult resultHandler )
        {
            // build the object we'll send
            GroupRegModel groupRegModel = new GroupRegModel( )
            {
                RequestedGroupId = groupId,
                FirstName = firstName,
                LastName = lastName,
                SpouseName = spouseName,
                Email = email,
                Phone = phone
            };

            RockApi.Post_CustomEndPoint( RockApi.BaseUrl + EndPoint_GroupRegistration, groupRegModel, resultHandler );
        }

        class RegAccountData
        {
            public string Username;
            public string Password;
            
            public string FirstName;
            public string LastName;
            public string Email;

            public string CellPhoneNumber;
        }

        const string EndPoint_Register = "api/MobileApp/RegisterNewUser/";
        public static void RegisterNewUser( string username, string password, string firstName, string lastName, string email, string phoneNumberText, HttpRequest.RequestResult resultHandler)
        {
            //JHM 2-28-17 WOAH TWO YEARS LATER!?! This was in the old crappy RegisterNewUser, replaced by this slick one. But I gotta keep the comment below, cause I gotta be me!
                //JHM 3-11-15 ALMOST FRIDAY THE 13th SIX YEARS LATER YAAAHH!!!!!
                //Until we release, or at least are nearly done testing, do not allow actual user registrations.
                //resultHandler( HttpStatusCode.OK, "" );
                //return;
            //

            RegAccountData regAccountModel = new RegAccountData()
            {
                Username = username,
                Password = password,
                
                FirstName = firstName,
                LastName = lastName,
                Email = email,

                CellPhoneNumber = phoneNumberText
            };

            RockApi.Post_CustomEndPoint(RockApi.BaseUrl + EndPoint_Register, regAccountModel, 
                delegate( HttpStatusCode statusCode, string statusDescription )
                {
                    // Everything worked
                    if (Rock.Mobile.Network.Util.StatusInSuccessRange(statusCode) == true)
                    {
                        resultHandler(statusCode, statusDescription);
                    }
                    // Login already exists
                    else if (HttpStatusCode.Unauthorized == statusCode)
                    {
                        resultHandler(statusCode, RegisterStrings.RegisterResult_LoginUsed );
                    }
                    // Person already exists in Rock (with no login)
                    else if (HttpStatusCode.Conflict == statusCode)
                    {
                        resultHandler(statusCode, RegisterStrings.RegisterResult_PersonAlreadyExistsNoLogin );
                    }
                    // Person already exists in Rock (WITH a login)
                    else if (HttpStatusCode.NotAcceptable == statusCode)
                    {
                        resultHandler(statusCode, RegisterStrings.RegisterResult_PersonAlreadyExistsWithLogin );
                    }
                    // Random failure
                    else
                    {
                        resultHandler(statusCode, GeneralStrings.Network_Result_FailedText );
                    }
                }
           );
        }

        public static void UpdateOrAddPhoneNumber( Rock.Client.Person person, Rock.Client.PhoneNumber phoneNumber, bool isNew, HttpRequest.RequestResult<Rock.Client.PhoneNumber> resultHandler )
        {
            // is it blank?
            if ( string.IsNullOrEmpty( phoneNumber.Number ) == true )
            {
                // if it's not new, we should delete an existing
                if ( isNew == false )
                {
                    ApplicationApi.DeleteCellPhoneNumber( phoneNumber, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // return the blank number back to them
                                resultHandler( statusCode, statusDescription, phoneNumber );
                            }
                        } );
                }
                // otherwise, simply ignore it and say we're done.
                else
                {
                    resultHandler( System.Net.HttpStatusCode.OK, "", phoneNumber );
                }
            }
            // not blank, so we're adding or updating
            else
            {
                // send it to the server
                ApplicationApi.AddOrUpdateCellPhoneNumber( person, phoneNumber, isNew, 0, 
                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                    {
                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                        {
                            // if it was new, get it so we have its ID
                            if( isNew == true )
                            {
                                ApplicationApi.GetPhoneNumberByGuid( phoneNumber.Guid, resultHandler );
                            }
                            else
                            {
                                // it's updated, so now just return what we updated.
                                resultHandler( statusCode, statusDescription, phoneNumber );
                            }
                        }
                        // something went wrong, so return.
                        else
                        {
                            resultHandler( statusCode, statusDescription, null );
                        }
                    } );
            }
        }

        public delegate void ImpersonationTokenResponse( string impersonationToken );
        public static void TryGetImpersonationToken( ImpersonationTokenResponse response )
        {
            // if the user is logged in and has an alias ID, try to get it
            if ( MobileApp.Shared.Network.RockMobileUser.Instance.LoggedIn == true && MobileApp.Shared.Network.RockMobileUser.Instance.Person.PrimaryAliasId.HasValue == true )
            {
                // make the request
                ApplicationApi.GetImpersonationToken( MobileApp.Shared.Network.RockMobileUser.Instance.Person.Id, 
                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription, string impersonationToken )
                    {
                        // whether it succeeded or not, hand them the response
                        response( impersonationToken );
                    } );
            }
            else
            {
                // they didn't pass requirements, so hand back an empty string.
                response( string.Empty );
            }
        }
    }
}
