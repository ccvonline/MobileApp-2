using System;
using Rock.Mobile.Network;
using System.Net;
using System.Collections.Generic;
using Rock.Mobile;

namespace MobileApp
{
    /// <summary>
    /// Implements api methods that are specific to MobileApp.
    /// </summary>
    public static class MobileAppApi
    {
        public static void GetNews( HttpRequest.RequestResult< List<Rock.Client.ContentChannelItem> > resultHandler )
        {
            string oDataFilter = string.Format( "?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637' and " +
                "Status eq '2' and (StartDateTime le DateTime'{0}' or StartDateTime eq null) and " +
                "(ExpireDateTime ge DateTime'{1}' or ExpireDateTime eq null)&LoadAttributes=True", 
                DateTime.Now.ToString( "s" ), DateTime.Now.ToString( "s" ) );

            RockApi.Get_ContentChannelItems( oDataFilter, resultHandler );
        }

        class DateTimeModel
        {
            public string ValueAsDateTime { get; set; }
        }

        const int GeneralDataTimeValueId = 2623;
        public static void GetGeneralDataTime( HttpRequest.RequestResult<DateTime> resultHandler )
        {
            string oDataFilter = string.Format( "?$filter=AttributeId eq {0}", GeneralDataTimeValueId );
            RockApi.Get_AttributeValues<List<DateTimeModel>>( oDataFilter,
                delegate(HttpStatusCode statusCode, string statusDescription, List<DateTimeModel> dateTimeList) 
                {
                    DateTime dateTime = DateTime.MinValue;
                    if( dateTimeList != null && dateTimeList.Count > 0 )
                    {
                        dateTime = DateTime.Parse( dateTimeList[ 0 ].ValueAsDateTime );
                    }

                    resultHandler( statusCode, statusDescription, dateTime );
                } );
        }

        public static void GetPrayerCategories( HttpRequest.RequestResult<List<Rock.Client.Category>> resultHandler )
        {
            RockApi.Get_Categories_GetChildren_1( resultHandler );
        }

        const int GroupRegistrationValueId = 52;
        public static void JoinGroup( Rock.Client.Person person, string firstName, string lastName, string spouseName, string email, string phone, int groupId, string groupName, HttpRequest.RequestResult resultHandler )
        {
            if ( person.PrimaryAliasId.HasValue && person.PrimaryAliasId.Value != 0 )
            {
                // resolve the alias ID
                ApplicationApi.ResolvePersonAliasId( person, 
                    delegate(int personId )
                    {
                        string oDataFilter = string.Format( "/{0}?PersonAliasId={1}&FirstName={2}&LastName={3}&SpouseName={4}&Email={5}&MobilePhone={6}&GroupId={7}&GroupName={8}", 
                            GroupRegistrationValueId, personId, firstName, lastName, spouseName, email, phone, groupId, groupName );

                        RockApi.Post_Workflows_WorkflowEntry( oDataFilter, resultHandler );
                    } );
            }
            else
            {
                // no ID, so just send the info
                string oDataFilter = string.Format( "/{0}?PersonAliasId={1}&FirstName={2}&LastName={3}&SpouseName={4}&Email={5}&MobilePhone={6}&GroupId={7}&GroupName={8}", 
                    GroupRegistrationValueId, 0, firstName, lastName, spouseName, email, phone, groupId, groupName );

                RockApi.Post_Workflows_WorkflowEntry( oDataFilter, resultHandler );
            }
        }

        const string RegisterResult_BadLogin = "CreateLoginError";
        const int UserLoginEntityTypeId = 27;
        public static void RegisterNewUser( Rock.Client.Person person, Rock.Client.PhoneNumber phoneNumber, string username, string password, HttpRequest.RequestResult resultHandler )
        {
            //JHM 3-11-15 ALMOST FRIDAY THE 13th SIX YEARS LATER YAAAHH!!!!!
            //Until we release, or at least are nearly done testing, do not allow actual user registrations.
            //resultHandler( HttpStatusCode.OK, "" );
            //return;

            // this is a complex end point. To register a user is a multiple step process.
            //1. Create a new Person on Rock
            //2. Request that Person back
            //3. Create a new Login for that person
            //4. Post the location, phone number and home campus
            person.Guid = Guid.NewGuid( );

            RockApi.Post_People( person, 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                {
                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                    {
                        ApplicationApi.GetPersonByGuid( person.Guid,
                            delegate(System.Net.HttpStatusCode personStatusCode, string personStatusDescription, Rock.Client.Person createdPerson ) 
                            {
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( personStatusCode ) )
                                {
                                    // it worked. now put their login info.
                                    RockApi.Post_UserLogins( createdPerson.Id, username, password, UserLoginEntityTypeId,
                                        delegate(System.Net.HttpStatusCode loginStatusCode, string loginStatusDescription) 
                                        {
                                            // if this worked, we are home free
                                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( loginStatusCode ) )
                                            {
                                                // now update their phone number, if valid
                                                if( phoneNumber != null )
                                                {
                                                    ApplicationApi.AddOrUpdateCellPhoneNumber( createdPerson, phoneNumber, true,
                                                        delegate(HttpStatusCode phoneStatusCode, string phoneStatusDescription) 
                                                        {
                                                            // NOTICE: We are passing back the loginStatus, not the phone status.
                                                            // This is because if the login was created, we're DONE and they can login.
                                                            // Updating their phone number is purely a bonus.
                                                            if( resultHandler != null )
                                                            {
                                                                resultHandler( loginStatusCode, loginStatusDescription );
                                                            }
                                                        });
                                                }
                                                else
                                                {
                                                    // phone number not provided, go with the result of the login creation
                                                    if( resultHandler != null )
                                                    {
                                                        resultHandler( loginStatusCode, RegisterResult_BadLogin );
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Failed
                                                if( resultHandler != null )
                                                {
                                                    resultHandler( loginStatusCode, RegisterResult_BadLogin );
                                                }
                                            }
                                        });
                                }
                                else
                                {
                                    // Failed
                                    if( resultHandler != null )
                                    {
                                        resultHandler( personStatusCode, personStatusDescription );
                                    }
                                }
                            });
                    }
                    else
                    {
                        // Failed
                        if( resultHandler != null )
                        {
                            resultHandler( statusCode, statusDescription );
                        }
                    }
                } );
        }
    }
}
