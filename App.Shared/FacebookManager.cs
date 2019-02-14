#if !__WIN__
using System;
using System.Collections.Generic;
using System.Net;
using MobileApp.Shared.PrivateConfig;
using RestSharp;
using Rock.Mobile.Network;

namespace MobileApp.Shared
{
    public class FacebookManager
    {
        static FacebookManager _Instance = new FacebookManager();
        public static FacebookManager Instance { get { return _Instance; } }

        const string FacebookLoginUrl = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&response_type={2}&display={3}&scope={4}";

        const string FacebookGraphUrl = "https://graph.facebook.com/v{0}/me?access_token={1}&fields=id,first_name,last_name,email";

        public string AccessToken { get; set; }

        public class FBBasicInfo
        {
            public string id;
            public string first_name;
            public string last_name;
            public string email;
        }
        public FBBasicInfo BasicInfo { get; protected set; }

        FacebookManager( )
        {
        }

        public Dictionary<string, object> CreateLoginRequest( )
        {
            // setup the login dictionary
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[ "client_id" ] = App.Shared.SecuredValues.FBAppID;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the permissions we want
            parameters["scope"] = PrivateGeneralConfig.FBAppPermissions;

            return parameters;
        }

        public string GetLoginUrl( Dictionary<string, object> loginRequest )
        {
            string generatedUrl = string.Format( FacebookLoginUrl, System.Net.WebUtility.UrlEncode( loginRequest[ "client_id" ].ToString() ) ,
                                                                   System.Net.WebUtility.UrlEncode( loginRequest[ "redirect_uri" ].ToString( ) ),
                                                                   System.Net.WebUtility.UrlEncode( loginRequest[ "response_type" ].ToString( ) ),
                                                                   System.Net.WebUtility.UrlEncode( loginRequest[ "display" ].ToString( ) ),
                                                                   System.Net.WebUtility.UrlEncode( loginRequest[ "scope" ].ToString( ) ) );

            return generatedUrl;
        }

        public bool HasFacebookResponse( Uri facebookUri)
        {
            // if there's an error in the query parameter, we have a failed response.
            var queryParams = System.Web.HttpUtility.ParseQueryString( facebookUri.Query );
            if( queryParams != null && queryParams[ "error" ] != null )
            {
                return true;
            }
            else
            {
                // otherwise see if there's a response with an access code.

                // first split the fragments
                string[ ] fragmentList = facebookUri.Fragment.Split( '&' );

                // now find the access token
                foreach( string param in fragmentList )
                {
                    // we use 'contains' because if it's the first element of the fragment, it'll have # on it.
                    if( param.ToLower( ).Contains( "access_token" ) )
                    {
                        string[ ] keyValue = param.Split( '=' );
                        AccessToken = keyValue[ 1 ] ?? string.Empty;
                    }
                }

                // if we got an access token, we got a successful response
                if( string.IsNullOrWhiteSpace( AccessToken ) == false )
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAuthenticated( )
        {
            // Note - This class was designed for quick Login access, so this access code shouldn't be preserved
            return string.IsNullOrWhiteSpace( AccessToken ) == false ? true : false;
        }

        public delegate void OnBasicInfoResponse( bool result );
        public void FetchBasicInfo( OnBasicInfoResponse onResultHandler )
        {
            // retrieves basic data about the user

            // first make sure we have an access token; otherwise any data request will fail
            if( string.IsNullOrWhiteSpace( AccessToken ) == false )
            {
                string fbRequestUrl = string.Format( FacebookGraphUrl, PrivateConfig.PrivateGeneralConfig.FBGraphAPIVersion, AccessToken );

                RestRequest request = new RestRequest( Method.GET );
                request.RequestFormat = DataFormat.Json;

                HttpRequest httpRequest = new HttpRequest( );
                httpRequest.ExecuteAsync( fbRequestUrl, request, delegate ( HttpStatusCode statusCode, string statusDescription, FBBasicInfo model )
                {
                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                    {
                        BasicInfo = model;
                        onResultHandler( true );
                    }
                    else
                    {
                        BasicInfo = null;
                        onResultHandler( false );
                    }
                } );
            }
            else
            {
                // we're not authenticated, so simply fail
                onResultHandler( false );
            }
        }

        public string GetUserID( )
        {
            return BasicInfo.id;
        }

        public string GetFirstName( )
        {
            return BasicInfo.first_name;
        }

        public string GetLastName( )
        {
            return BasicInfo.last_name;
        }

        public string GetEmail( )
        {
            return BasicInfo.email;
        }
    }
}
#endif
