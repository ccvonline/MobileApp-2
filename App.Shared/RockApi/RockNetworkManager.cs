using System;
using App.Shared.Network;
using Rock.Mobile.Network;
using App.Shared.Config;
using App.Shared.PrivateConfig;

namespace App
{
    namespace Shared
    {
        namespace Network
        {
            public sealed class RockNetworkManager
            {
                private static RockNetworkManager _Instance = new RockNetworkManager( );
                public static RockNetworkManager Instance { get { return _Instance; } }

                /// <summary>
                /// Callback used to let the system know the series info has been downloaded.
                /// </summary>
                public delegate void SeriesDownloaded( );

                HttpRequest.RequestResult ResultCallback;

                public RockNetworkManager( )
                {
                }

                public void SyncRockData( SeriesDownloaded seriesCallback, HttpRequest.RequestResult resultCallback )
                {
                    ResultCallback = resultCallback;

                    // have the launch data request the series before it does anything else.
                    RockLaunchData.Instance.GetNoteDB( delegate
                        {
                            seriesCallback( );

                            // if we're logged in, sync any changes we've made with the server.
                            if( RockMobileUser.Instance.LoggedIn == true )
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "Logged in. Syncing out-of-sync data." );

                                //( this includes notes, profile changes, etc.)
                                RockApi.Instance.SyncWithServer( 
                                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                                    {
                                        // IF THERE WAS A PROBLEM SYNCING, DO NOT PULL DOWN THE LATEST PROFILE.
                                        // That would cause pending changes to be lost.
                                        if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                        {
                                            Rock.Mobile.Util.Debug.WriteLine( "Syncing with server worked. Pulling down latest data." );
                                            // now get their profile. Assuming there weren't any profile changes, this will download
                                            // their latest profile. That way if someone made a change directly in Rock, it'll be reflected here.
                                            RockMobileUser.Instance.GetProfileAndCellPhone( delegate 
                                                {
                                                    // get the address, which is certainly part
                                                    RockMobileUser.Instance.GetFamilyAndAddress( delegate 
                                                        {
                                                            // and hey, why not their profile picture too
                                                            // if they have a profile picture, grab it.
                                                            RockMobileUser.Instance.TryDownloadProfilePicture( PrivateGeneralConfig.ProfileImageSize, delegate 
                                                                {
                                                                    // failure or not, server syncing is finished, so let's go ahead and 
                                                                    // get launch data.
                                                                    RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                                                                });
                                                        });
                                                });
                                        }
                                        else
                                        {
                                            Rock.Mobile.Util.Debug.WriteLine( "Syncing with server FAILED. Skipping profile download to protect dirty data." );

                                            // failure or not, server syncing is finished, so let's go ahead and 
                                            // get launch data.
                                            RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                                        }
                                    });
                            }
                            else
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "Not Logged In. Skipping sync." );
                                RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                            }
                        } );
                }

                void LaunchDataReceived(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // launch data is done. Now see if we should update general data.

                    // first, take the latest delta of GeneralDataServerTime and what we have for ServerTime
                    TimeSpan deltaTime = RockLaunchData.Instance.Data.GeneralDataServerTime - RockGeneralData.Instance.Data.ServerTime;

                    // if that is > 0, it means the server has newer general data than us, so we should update.

                    // Alternatively, if GeneralData's serverTime is min value, we should update, because for whatever reason
                    // launch data isn't getting a time from Rock, so we want to ensure we have latest.
                    if( deltaTime > TimeSpan.Zero || RockGeneralData.Instance.Data.ServerTime == DateTime.MinValue )
                    {
                        RockGeneralData.Instance.GetGeneralData( RockLaunchData.Instance.Data.GeneralDataServerTime, GeneralDataReceived );
                    }
                    else
                    {
                        // there was no need to update general data, so just report done.
                        GeneralDataReceived( statusCode, statusDescription );
                    }
                }

                void GeneralDataReceived(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // New general data received. Save it, update fields, etc.
                    ResultCallback( statusCode, statusDescription );
                }
            }
        }
    }
}
