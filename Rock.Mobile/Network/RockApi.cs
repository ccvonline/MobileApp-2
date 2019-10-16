using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Rock.Mobile
{
    namespace Network
    {
        /// <summary>
        /// Implements the core API REST layer for Rock.
        /// </summary>
        public static class RockApi
        {
            /// <summary>
            /// The header key used for passing up the mobile app authorization token.
            /// </summary>
            const string AuthorizationTokenHeaderKey = "Authorization-Token";

            public static string BaseUrl { get; private set; }
            static string AuthorizationKey { get; set; }
            static HttpRequest Request { get; set; }

            static RockApi( )
            {
                Request = new HttpRequest();
            }

            public static void SetRockURL( string rockUrl )
            {
                BaseUrl = rockUrl;

                // append the "/" if necessary.
                if ( string.IsNullOrEmpty( BaseUrl ) == false && BaseUrl.EndsWith( "/" ) == false )
                {
                    BaseUrl += "/";
                }
            }

            public static void SetAuthorizationKey( string authKey )
            {
                AuthorizationKey = authKey;
            }

            static RestRequest GetRockRestRequest( Method method )
            {
                RestRequest request = new RestRequest( method );
                request.RequestFormat = DataFormat.Json;
                request.AddHeader( AuthorizationTokenHeaderKey, AuthorizationKey );

                return request;
            }


            // This allows hitting custom endpoints in Rock that don't necessarily map to a specific endpoint.
            // An example would be checking to see if Rock exists. We do that by querying the default person. Since
            // that is a workaround, we don't put an endpoint for that in here. Instead, the ApplicationApi implements it
            // using this method.
            public static void Get_CustomEndPoint<T>( string urlWithQuery, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                Request.ExecuteAsync<T>( urlWithQuery, request, resultHandler );
            }

            public static void Post_CustomEndPoint( string urlWithQuery, object postBody, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                if( postBody != null )
                {
                    request.AddJsonBody( postBody );
                }

                Request.ExecuteAsync( urlWithQuery, request, resultHandler );
            }

            public static void Post_CustomEndPoint<T>( string urlWithQuery, object postBody, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                if( postBody != null )
                {
                    request.AddJsonBody( postBody );
                }

                Request.ExecuteAsync<T>( urlWithQuery, request, resultHandler );
            }


            const string EndPoint_Auth_FacebookLogin = "api/Auth/FacebookLogin";
            public static void Post_Auth_FacebookLogin( object facebookUser, HttpRequest.RequestResult resultHandler )
            {
                // give the facebook auth login extra time, as it can be a lengthy process in Rock
                RestRequest request = GetRockRestRequest( Method.POST );
                request.Timeout = 60000;

                request.AddJsonBody( facebookUser );

                Request.ExecuteAsync( BaseUrl + EndPoint_Auth_FacebookLogin, request, resultHandler );
            }



            const string EndPoint_Auth_Login = "api/Auth/Login";
            public static void Post_Auth_Login( string username, string password, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                request.AddParameter( "Username", username );
                request.AddParameter( "Password", password );
                request.AddParameter( "Persisted", true );

                Request.ExecuteAsync( BaseUrl + EndPoint_Auth_Login, request, resultHandler );
            }



            const string EndPoint_People = "api/People";
            public static void Get_People<T>( string oDataFilter, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestUrl = BaseUrl + EndPoint_People + oDataFilter;
                Request.ExecuteAsync<T>( requestUrl, request, resultHandler);
            }

            const int RecordTypeValueId = 1; //This should never change, and represents "Person"
            const int RecordStatusTypeValueId = 5; //This represents "Pending", and is the safest default to use.
            const int ConnectionStatusTypeValueId = 67; // This represents "Web Prospect", and is the safest default to use.
            static Rock.Client.PersonEntity PackagePersonForUpload( Rock.Client.Person person )
            {
                // there are certain values that cannot be sent up to Rock, so
                // this will ensure that the 'person' object is setup for a clean upload to Rock.
                Rock.Client.PersonEntity newPerson = new Rock.Client.PersonEntity( );
                newPerson.CopyPropertiesFrom( person );
                newPerson.FirstName = person.NickName; //ALWAYS SET THE FIRST NAME TO NICK NAME
                newPerson.RecordTypeValueId = RecordTypeValueId; //ALWAYS SET THE RECORD TYPE TO PERSON

                // set the record / connection status values only if they aren't already set.
                if ( newPerson.RecordStatusValueId == null )
                {
                    newPerson.RecordStatusValueId = RecordStatusTypeValueId;
                }

                if ( newPerson.ConnectionStatusValueId == null )
                {
                    newPerson.ConnectionStatusValueId = ConnectionStatusTypeValueId;
                }

                return newPerson;
            }
            public static void Put_People( Rock.Client.Person person, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                // create a person object that can go up to rock, and copy the relavant data from the passed in arg
                Rock.Client.PersonEntity personEntity = PackagePersonForUpload( person );

                if ( modifiedById > 0 )
                {
                    personEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    personEntity.CreatedByPersonAliasId = modifiedById;
                    personEntity.ModifiedByPersonAliasId = modifiedById;
                }

                RestRequest request = GetRockRestRequest( Method.PUT );
                request.AddJsonBody( personEntity );

                Request.ExecuteAsync( BaseUrl + EndPoint_People + "/" + personEntity.Id.ToString( ), request, resultHandler );
            }

            public static void Post_People( Rock.Client.Person person, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                // create a person object that can go up to rock, and copy the relavant data from the passed in arg
                Rock.Client.PersonEntity personEntity = PackagePersonForUpload( person );

                if ( modifiedById > 0 )
                {
                    personEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    personEntity.CreatedByPersonAliasId = modifiedById;
                    personEntity.ModifiedByPersonAliasId = modifiedById;
                }
                    
                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( personEntity );

                Request.ExecuteAsync( BaseUrl + EndPoint_People, request, resultHandler);
            }



            const string EndPoint_People_GetByUserName = "api/People/GetByUserName/";
            public static void Get_People_ByUserName( string userName, HttpRequest.RequestResult<Rock.Client.Person> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestUrl = BaseUrl + EndPoint_People_GetByUserName;
                requestUrl += userName;

                Request.ExecuteAsync<Rock.Client.Person>( requestUrl, request, resultHandler);
            }



            const string EndPoint_People_GetGraduationYear = "api/People/GetGraduationYear/";
            public static void Get_People_GraduationYear( int gradeOffset, HttpRequest.RequestResult<int> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestUrl = BaseUrl + EndPoint_People_GetGraduationYear;
                requestUrl += gradeOffset;

                Request.ExecuteAsync<int>( requestUrl, request, resultHandler);
            }



            const string EndPoint_People_AddExistingPersonToFamily = "api/People/AddExistingPersonToFamily";
            public static void Post_People_AddExistingPersonToFamily( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                string requestString = BaseUrl + EndPoint_People_AddExistingPersonToFamily + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );    
            }



            const string EndPoint_People_AttributeValue = "api/People/AttributeValue";
            public static void Delete_People_AttributeValue( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.DELETE );

                string requestString = BaseUrl + EndPoint_People_AttributeValue + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );
            }

            public static void Post_People_AttributeValue( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                string requestString = BaseUrl + EndPoint_People_AttributeValue + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );
            }



            const string EndPoint_People_GetSearchDetails = "api/People/GetSearchDetails/";
            public static void Get_People_GetSearchDetails( string oDataFilter, HttpRequest.RequestResult<string> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_People_GetSearchDetails + oDataFilter;
                Request.ExecuteAsync<object>( requestString, request, delegate(HttpStatusCode statusCode, string statusDescription, object model )
                    {
                        // we know this endpoint returns string data, so intercept and cast to a string.
                        // we have to do this because string doesn't have a parameterless constructor

                        // use dynamic casting so that if there's a problem with the return value we handle it.
                        if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                        {
                            string result = model as string;
                            resultHandler( statusCode, statusDescription, result );
                        }
                        else
                        {
                            resultHandler( statusCode, statusDescription, null );
                        }
                    } );
            }



            const string EndPoint_Campuses = "api/Campuses";
            public static void Get_Campuses( string queryString, HttpRequest.RequestResult< List<Rock.Client.Campus> > resultHandler )
            {
                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_Campuses + queryString;

                // get the raw response
                Request.ExecuteAsync< List<Rock.Client.Campus> >( requestUrl, request, resultHandler);
            }



            const string EndPoint_Attributes = "api/Attributes";
            public static void Get_Attributes( string oDataFilter, HttpRequest.RequestResult<List<Rock.Client.Attribute>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_Attributes + oDataFilter;

                Request.ExecuteAsync<List<Rock.Client.Attribute>>( requestString, request, resultHandler);
            }



            const string EndPoint_GetImage = "GetImage.ashx?";
            public static void Get_GetImage( Guid photoGuid, uint? width, uint? height, HttpRequest.RequestResult<MemoryStream> resultHandler )
            {
                if ( photoGuid != Guid.Empty )
                {
                    string requestUrl = BaseUrl + EndPoint_GetImage + "guid=" + photoGuid;
                    Get_GetImage_Internal( requestUrl, width, height, resultHandler );
                }
                else 
                {
                    resultHandler( HttpStatusCode.NotFound, "Image Guid is Empty", null );
                }
            }

            public static void Get_GetImage( int photoId, uint? width, uint? height, HttpRequest.RequestResult<MemoryStream> resultHandler )
            {
                string requestUrl = BaseUrl + EndPoint_GetImage + "id=" + photoId;
                Get_GetImage_Internal( requestUrl, width, height, resultHandler );
            }

            public static void Get_GetImage_Internal( string requestUrl, uint? width, uint? height, HttpRequest.RequestResult<MemoryStream> resultHandler )
            {
                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );


                // add the requested dimensions
                if( width.HasValue )
                {
                    requestUrl += string.Format( "&width={0}", width );
                }

                // add the requested dimensions
                if( height.HasValue )
                {
                    requestUrl += string.Format( "&height={0}", height );
                }

                // get the raw response
                Request.ExecuteAsync( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, byte[] model) 
                    {
                        if ( model != null )
                        {
                            MemoryStream memoryStream = new MemoryStream( model );

                            resultHandler( statusCode, statusDescription, memoryStream );

                            memoryStream.Dispose( );
                        }
                        else
                        {
                            resultHandler( statusCode, statusDescription, null );
                        }
                    });
            }



            const string EndPoint_FileUploader = "FileUploader.ashx?isBinaryFile={0}&fileTypeGuid={1}&isTemporary={2}";
            public static void Post_FileUploader( MemoryStream fileBuffer, bool isBinary, string fileTypeGuid, bool isTemporary, HttpRequest.RequestResult<byte[]> resultHandler )
            {
                // send up the file
                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddFile( "file0", fileBuffer.ToArray( ), "image.jpg" );

                string requestUrl = string.Format( BaseUrl + EndPoint_FileUploader, isBinary, fileTypeGuid, isTemporary );

                Request.ExecuteAsync( requestUrl, request, resultHandler );
            }



            const string EndPoint_GroupMembers = "api/groupmembers/";
            public static void Get_GroupMembers( string oDataFilter, HttpRequest.RequestResult<List<Rock.Client.GroupMember>> requestHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_GroupMembers + oDataFilter;

                Request.ExecuteAsync< List<Rock.Client.GroupMember> >( requestUrl, request, requestHandler );
            }

            public static void Put_GroupMembers( Rock.Client.GroupMember groupMember, int modifiedById, HttpRequest.RequestResult requestHandler )
            {
                Rock.Client.GroupMemberEntity groupMemberEntity = new Rock.Client.GroupMemberEntity();
                groupMemberEntity.CopyPropertiesFrom( groupMember );

                if ( modifiedById > 0 )
                {
                    groupMemberEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    groupMemberEntity.CreatedByPersonAliasId = modifiedById;
                    groupMemberEntity.ModifiedByPersonAliasId = modifiedById;
                }
                
                RestRequest request = GetRockRestRequest( Method.PUT );
                request.AddJsonBody( groupMemberEntity );

                string requestUrl = BaseUrl + EndPoint_GroupMembers + groupMemberEntity.Id.ToString( );
                Request.ExecuteAsync( requestUrl, request, requestHandler );
            }

            public static void Post_GroupMembers( Rock.Client.GroupMember groupMember, int modifiedById, HttpRequest.RequestResult requestHandler )
            {
                Rock.Client.GroupMemberEntity groupMemberEntity = new Rock.Client.GroupMemberEntity();
                groupMemberEntity.CopyPropertiesFrom( groupMember );

                if ( modifiedById > 0 )
                {
                    groupMemberEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    groupMemberEntity.CreatedByPersonAliasId = modifiedById;
                    groupMemberEntity.ModifiedByPersonAliasId = modifiedById;
                }

                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( groupMemberEntity );

                string requestUrl = BaseUrl + EndPoint_GroupMembers;
                Request.ExecuteAsync( requestUrl, request, requestHandler );
            }



            const string EndPoint_Groups_GetFamiliesByPersonNameSearch = "api/Groups/GetFamiliesByPersonNameSearch/";
            public static void Get_Groups_FamiliesByPersonNameSearch( string personName, HttpRequest.RequestResult<List<Rock.Client.Family>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_Groups_GetFamiliesByPersonNameSearch + personName;

                Request.ExecuteAsync<List<Rock.Client.Family>>( requestString, request, resultHandler);
            }



            const string EndPoint_Groups_GetFamily = "api/Groups/GetFamily/{0}";
            public static void Get_Groups_GetFamily( int familyID, HttpRequest.RequestResult<Rock.Client.Family> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + string.Format( EndPoint_Groups_GetFamily, familyID );

                Request.ExecuteAsync<Rock.Client.Family>( requestString, request, resultHandler);
            }



            const string EndPoint_Groups_AttributeValue = "api/Groups/AttributeValue";
            public static void Post_Groups_AttributeValue( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                string requestString = BaseUrl + EndPoint_Groups_AttributeValue + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );
            }



            const string EndPoint_PrayerRequests_Public = "api/prayerrequests/public";
            public static void Get_PrayerRequests_Public( HttpRequest.RequestResult< List<Rock.Client.PrayerRequest> > resultHandler )
            {
                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );

                // insert the expiration limit
                string requestString = BaseUrl + EndPoint_PrayerRequests_Public;

                Request.ExecuteAsync< List<Rock.Client.PrayerRequest> >( requestString, request, resultHandler);
            }



            const string EndPoint_PrayerRequests = "api/prayerrequests";
            public static void Post_PrayerRequests( Rock.Client.PrayerRequest prayer, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.PrayerRequestEntity prayerRequestEntity = new Rock.Client.PrayerRequestEntity();
                prayerRequestEntity.CopyPropertiesFrom( prayer );
                
                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( prayerRequestEntity );

                Request.ExecuteAsync( BaseUrl + EndPoint_PrayerRequests, request, resultHandler);
            }



            const string EndPoint_PrayerRequests_Prayed = "api/prayerrequests/prayed/";
            public static void Put_PrayerRequests_Prayed( int prayerId, HttpRequest.RequestResult resultHandler )
            {
                // build a URL that contains the ID for the prayer that is getting another prayer
                RestRequest request = GetRockRestRequest( Method.PUT );

                string requestUrl = BaseUrl + EndPoint_PrayerRequests_Prayed;
                requestUrl += prayerId.ToString( );

                Request.ExecuteAsync( requestUrl, request, resultHandler);
            }



            const string EndPoint_Categories_GetChildren_1 = "api/categories/getChildren/1";
            public static void Get_Categories_GetChildren_1( HttpRequest.RequestResult<List<Rock.Client.Category>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_Categories_GetChildren_1;

                // get the resonse
                Request.ExecuteAsync< List<Rock.Client.Category> >( requestUrl, request, resultHandler );
            }



            const string EndPoint_ContentChannelItems = "api/ContentChannelItems";
            public static void Get_ContentChannelItems( string oDataFilter, HttpRequest.RequestResult< List<Rock.Client.ContentChannelItem> > resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_ContentChannelItems + oDataFilter;

                Request.ExecuteAsync< List<Rock.Client.ContentChannelItem> >( requestUrl, request, resultHandler );
            }



            const string EndPoint_Groups_GetFamilies = "api/Groups/GetFamilies/";
            public static void Get_FamiliesOfPerson( int personId, string oDataFilter, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_Groups_GetFamilies + personId.ToString( );
                requestUrl += oDataFilter;

                // get the raw response
                Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler);
            }



            const string EndPoint_Groups_GuestsForFamily = "api/Groups/GetGuestsForFamily/";
            public static void Get_Groups_GuestsForFamily( int familyId, HttpRequest.RequestResult<List<Rock.Client.GuestFamily>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_Groups_GuestsForFamily + familyId.ToString( );

                Request.ExecuteAsync<List<Rock.Client.GuestFamily>>( requestString, request, resultHandler);
            }



            const string EndPoint_Groups = "api/Groups";
            public static void Get_Groups<T>( string oDataFilter, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_Groups + oDataFilter;

                Request.ExecuteAsync<T>( requestUrl, request, resultHandler );
            }

            public static void Put_Groups( Rock.Client.Group group, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.GroupEntity groupEntity = new Rock.Client.GroupEntity();
                groupEntity.CopyPropertiesFrom( group );

                if ( modifiedById > 0 )
                {
                    groupEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    groupEntity.CreatedByPersonAliasId = modifiedById;
                    groupEntity.ModifiedByPersonAliasId = modifiedById;
                }
                
                RestRequest request = GetRockRestRequest( Method.PUT );
                request.AddJsonBody( groupEntity );

                Request.ExecuteAsync( BaseUrl + EndPoint_Groups + "/" + groupEntity.Id, request, resultHandler);
            }

            public static void Post_Groups( Rock.Client.Group group, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.GroupEntity groupEntity = new Rock.Client.GroupEntity();
                groupEntity.CopyPropertiesFrom( group );

                if ( modifiedById > 0 )
                {
                    groupEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    groupEntity.CreatedByPersonAliasId = modifiedById;
                    groupEntity.ModifiedByPersonAliasId = modifiedById;
                }

                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( groupEntity );

                Request.ExecuteAsync( BaseUrl + EndPoint_Groups, request, resultHandler);
            }



            const string EndPoint_AttributeValues = "api/AttributeValues";
            public static void Get_AttributeValues<T>( string oDataFilter, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_AttributeValues + oDataFilter;

                Request.ExecuteAsync<T>( requestUrl, request, resultHandler );
            }



            const string EndPoint_Groups_ByLocation = "api/Groups/ByLocation";
            public static void Get_Groups_ByLocation( string oDataFilter, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
            {
                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + EndPoint_Groups_ByLocation + oDataFilter;

                Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler );
            }



            /*const string EndPoint_Groups_SaveAddress = "api/Groups/SaveAddress/{0}/{1}/{2}/{3}/{4}/{5}/{6}";
            public static void Put_Groups_SaveAddress( Rock.Client.Group group, Rock.Client.GroupLocation groupLocation, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.PUT );

                string requestUrl = string.Format( BaseUrl + EndPoint_Groups_SaveAddress, group.Id, 
                    groupLocation.GroupLocationTypeValueId, 
                    System.Net.WebUtility.UrlEncode( groupLocation.Location.Street1 ), 
                    System.Net.WebUtility.UrlEncode( groupLocation.Location.City ), 
                    System.Net.WebUtility.UrlEncode( groupLocation.Location.State ), 
                    System.Net.WebUtility.UrlEncode( groupLocation.Location.PostalCode ), 
                    System.Net.WebUtility.UrlEncode( groupLocation.Location.Country ) );

                // make sure the URL pieces are fully encoded, but still use spaces
                requestUrl = requestUrl.Replace( "+", " " );

                Request.ExecuteAsync( requestUrl, request, resultHandler );
            }*/

            const string EndPoint_Groups_SaveAddress = "api/Groups/SaveAddress/{0}/{1}";
            public static void Put_Groups_SaveAddress( Rock.Client.Group group, Rock.Client.GroupLocation groupLocation, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.PUT );

                string requestUrl = string.Format( BaseUrl + EndPoint_Groups_SaveAddress, group.Id, 
                                        groupLocation.GroupLocationTypeValueId );


                // append all valid params (since they're not all required)
                if ( string.IsNullOrWhiteSpace( groupLocation.Location.Street1 ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "street1=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.Street1 ) ) );
                }

                if ( string.IsNullOrWhiteSpace( groupLocation.Location.Street2 ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "street2=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.Street2 ) ) );
                }

                if ( string.IsNullOrWhiteSpace( groupLocation.Location.City ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "city=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.City ) ) );
                }

                if ( string.IsNullOrWhiteSpace( groupLocation.Location.State ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "state=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.State ) ) );
                }

                if ( string.IsNullOrWhiteSpace( groupLocation.Location.PostalCode ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "postalCode=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.PostalCode ) ) );
                }

                if ( string.IsNullOrWhiteSpace( groupLocation.Location.Country ) == false )
                {
                    requestUrl = Rock.Mobile.Util.Strings.Parsers.AddParamToURL( requestUrl, string.Format( "country=" + System.Net.WebUtility.UrlEncode( groupLocation.Location.Country ) ) );
                }

                // make sure the URL pieces are fully encoded, but still use spaces
                requestUrl = requestUrl.Replace( "+", " " );

                Request.ExecuteAsync( requestUrl, request, resultHandler );
            }

            const string EndPoint_GroupMembers_KnownRelationships = "api/GroupMembers/KnownRelationship";
            public static void Get_GroupMembers_KnownRelationships( string query, HttpRequest.RequestResult<List<Rock.Client.GroupMember>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_GroupMembers_KnownRelationships + query;

                Request.ExecuteAsync<List<Rock.Client.GroupMember>>( requestString, request, resultHandler );
            }

            public static void Post_GroupMembers_KnownRelationships( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                string requestString = BaseUrl + EndPoint_GroupMembers_KnownRelationships + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );
            }

            public static void Delete_GroupMembers_KnownRelationships( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.DELETE );

                string requestString = BaseUrl + EndPoint_GroupMembers_KnownRelationships + oDataFilter;

                Request.ExecuteAsync( requestString, request, resultHandler );
            }



            const string EndPoint_PhoneNumbers = "api/PhoneNumbers/";
            public static void Get_PhoneNumbers<T>( string oDataFilter, HttpRequest.RequestResult<T> resultHandler ) where T : new( )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestUrl = BaseUrl + EndPoint_PhoneNumbers + oDataFilter;
                Request.ExecuteAsync<T>( requestUrl, request, resultHandler);
            }

            public static void Put_PhoneNumbers( Rock.Client.PhoneNumber phoneNumber, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.PhoneNumberEntity phoneNumberEntity = new Rock.Client.PhoneNumberEntity();
                phoneNumberEntity.CopyPropertiesFrom( phoneNumber );

                if ( modifiedById > 0 )
                {
                    phoneNumberEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    phoneNumberEntity.CreatedByPersonAliasId = modifiedById;
                    phoneNumberEntity.ModifiedByPersonAliasId = modifiedById;
                }
                
                RestRequest request = GetRockRestRequest( Method.PUT );
                request.AddJsonBody( phoneNumberEntity );

                // since we're updating an existing number, put the ID
                string requestUrl = EndPoint_PhoneNumbers + phoneNumberEntity.Id;

                // fire off the request
                Request.ExecuteAsync( BaseUrl + requestUrl, request, resultHandler );
            }

            public static void Post_PhoneNumbers( Rock.Client.PhoneNumber phoneNumber, int modifiedById, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.PhoneNumberEntity phoneNumberEntity = new Rock.Client.PhoneNumberEntity();
                phoneNumberEntity.CopyPropertiesFrom( phoneNumber );

                if ( modifiedById > 0 )
                {
                    phoneNumberEntity.ModifiedAuditValuesAlreadyUpdated = true;
                    phoneNumberEntity.CreatedByPersonAliasId = modifiedById;
                    phoneNumberEntity.ModifiedByPersonAliasId = modifiedById;
                }

                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( phoneNumberEntity );

                // fire off the request
                Request.ExecuteAsync( BaseUrl + EndPoint_PhoneNumbers, request, resultHandler );
            }

            public static void Delete_PhoneNumbers( int phoneNumberId, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.DELETE );

                string requestUrl = EndPoint_PhoneNumbers + phoneNumberId;

                // fire off the request
                Request.ExecuteAsync( BaseUrl + requestUrl, request, resultHandler );
            }



            const string EndPoint_Locations_FromAddress = "api/locations/{0}/{1}/{2}/{3}";
            public static void Get_Locations_FromAddress( string street, string city, string state, string zip, HttpRequest.RequestResult<Rock.Client.Location> resultHandler )
            {
                // url encode the address values
                street = System.Net.WebUtility.UrlEncode( street );
                city = System.Net.WebUtility.UrlEncode( city );
                state = System.Net.WebUtility.UrlEncode( state );
                zip = System.Net.WebUtility.UrlEncode( zip );

                // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                RestRequest request = GetRockRestRequest( Method.GET );
                string requestUrl = BaseUrl + string.Format( EndPoint_Locations_FromAddress, street, city, state, zip );

                // restore white space
                requestUrl = requestUrl.Replace( "+", " " );

                // first get the location based on the info passed up
                Request.ExecuteAsync<Rock.Client.Location>( requestUrl, request, resultHandler );
            }



            const string EndPoint_UserLogins = "api/UserLogins";
            public static void Post_UserLogins( int personId, string username, string password, int loginEntityTypeId, HttpRequest.RequestResult resultHandler )
            {
                Rock.Client.UserLoginWithPlainTextPassword newLogin = new Rock.Client.UserLoginWithPlainTextPassword();
                newLogin.UserName = username;
                newLogin.PlainTextPassword = password;
                newLogin.PersonId = personId;
                newLogin.Guid = Guid.NewGuid( );
                newLogin.EntityTypeId = loginEntityTypeId;

                RestRequest request = GetRockRestRequest( Method.POST );
                request.AddJsonBody( newLogin );

                Request.ExecuteAsync( BaseUrl + EndPoint_UserLogins, request, resultHandler);
            }



            const string EndPoint_DefinedTypes = "api/DefinedTypes";
            public static void Get_DefinedTypes( string oDataFilter, HttpRequest.RequestResult<List<Rock.Client.DefinedType>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_DefinedTypes + oDataFilter;

                Request.ExecuteAsync<List<Rock.Client.DefinedType>>( requestString, request, resultHandler);
            }



            const string EndPoint_Workflows_WorkflowEntry = "api/Workflows/WorkflowEntry";
            public static void Post_Workflows_WorkflowEntry( string oDataFilter, HttpRequest.RequestResult resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.POST );

                string requestString = EndPoint_Workflows_WorkflowEntry + oDataFilter;
                Request.ExecuteAsync( BaseUrl + requestString, request, resultHandler );    
            }



            const string EndPoint_GroupTypeRoles = "api/GroupTypeRoles";
            public static void Get_GroupTypeRoles( string oDataFilter, HttpRequest.RequestResult<List<Rock.Client.GroupTypeRole>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestString = BaseUrl + EndPoint_GroupTypeRoles + oDataFilter;

                Request.ExecuteAsync<List<Rock.Client.GroupTypeRole>>( requestString, request, resultHandler);
            }



            const string EndPoint_DefinedValues = "api/DefinedValues";
            public static void Get_DefinedValues( string oDataFilter, HttpRequest.RequestResult<List<Rock.Client.DefinedValue>> resultHandler )
            {
                RestRequest request = GetRockRestRequest( Method.GET );

                string requestUrl = BaseUrl + EndPoint_DefinedValues + oDataFilter;

                Request.ExecuteAsync<List<Rock.Client.DefinedValue>>( requestUrl, request, resultHandler );
            }

            /// <summary>
            /// Core function that must be used before calling ANY endpoint requiring a personId
            /// </summary>
            public class PersonIdObj
            {
                public int PersonId { get; set; }
            }

            const string EndPoint_PersonAlias = "api/PersonAlias/";
            public static void Get_PersonAliasIdToPersonId( int personAliasId, HttpRequest.RequestResult<PersonIdObj> resultHandler )
            {
                // For debugging, enable the below
                //onComplete( person.Id );
                //return;

                // make the request for the ID
                RestRequest request = GetRockRestRequest( Method.GET );
                Request.ExecuteAsync<PersonIdObj>( BaseUrl + EndPoint_PersonAlias + personAliasId, request, resultHandler ); 
            }
        }
    }
}
