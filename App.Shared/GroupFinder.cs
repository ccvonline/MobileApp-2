using System;
using System.Collections.Generic;
using Rock.Mobile.Network;


namespace MobileApp.Shared
{
    public class GroupFinder
    {
        public GroupFinder( )
        {
        }

        public delegate void GetGroupsComplete( MobileAppApi.GroupSearchResult sourceLocation, List<MobileAppApi.GroupSearchResult> groupEntry, bool result );
        public static void GetGroups( int groupTypeId, string street, string city, string state, string zip, int skip, int top, GetGroupsComplete onCompletion )
        {
            MobileAppApi.GroupSearchResult sourceLocation = new MobileAppApi.GroupSearchResult( );

            // first convert the address into a location object
            RockApi.Get_Locations_FromAddress( street, city, state, zip, 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription, Rock.Client.Location model )
                {
                    // verify the call was successful AND it resulted in a valid location
                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && 
                        model != null &&
                        model.Latitude.HasValue == true && 
                        model.Longitude.HasValue == true )
                    { 
                        // take the source location so we can provide it to the caller
                        sourceLocation.Latitude = model.Latitude.Value;
                        sourceLocation.Longitude = model.Longitude.Value;

                        MobileAppApi.GetPublicGroupsByLocation( groupTypeId, model.Id, skip, top, delegate ( List<MobileAppApi.GroupSearchResult> searchResults )
                            {
                                if( searchResults != null )
                                {
                                    onCompletion( sourceLocation, searchResults, true );
                                }
                                else
                                {
                                    // pass on empty list on failure
                                    onCompletion( sourceLocation, new List<MobileAppApi.GroupSearchResult>(), false );
                                }
                            } );
                    }
                    else
                    {
                        onCompletion( sourceLocation, new List<MobileAppApi.GroupSearchResult>( ), Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true ? true : false );
                    }
                } );
        }

        static DateTime ParseMilitaryTime(string time )
        {
            //
            // Convert hour part of string to integer.
            //
            string hour = time.Substring(0, 2);
            int hourInt = int.Parse(hour);
            if (hourInt >= 24)
            {
                throw new ArgumentOutOfRangeException("Invalid hour");
            }
            //
            // Convert minute part of string to integer.
            //
            string minute = time.Substring(3, 2);
            int minuteInt = int.Parse(minute);
            if (minuteInt >= 60)
            {
                throw new ArgumentOutOfRangeException("Invalid minute");
            }
            //
            // Return the DateTime.
            //
            return new DateTime(2000, 1, 1, hourInt, minuteInt, 0);
        }
    }
}

