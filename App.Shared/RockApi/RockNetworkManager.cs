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
                    RockApi.SetRockURL( Config.GeneralConfig.RockBaseUrl );
                    RockApi.SetAuthorizationKey( Config.GeneralConfig.RockMobileAppAuthorizationKey );
                }

                public void SyncRockData( SeriesDownloaded seriesCallback, HttpRequest.RequestResult resultCallback )
                {
                    ResultCallback = resultCallback;

                    // have the launch data request the series before it does anything else.
                    RockLaunchData.Instance.GetNoteDB( delegate
                        {
                            if( seriesCallback != null )
                            {
                                seriesCallback( );
                            }

                            // if we're logged in, sync any changes we've made with the server.
                            if( RockMobileUser.Instance.LoggedIn == true )
                            {
                                Rock.Mobile.Util.Debug.WriteLine( "Logged in. Syncing out-of-sync data." );

                                // now get their profile. This will download
                                // their latest profile. That way if someone made a change directly in Rock, it'll be reflected here.
                                RockMobileUser.Instance.GetPersonData( delegate 
                                    {
                                        // if they have a profile picture, grab it.
                                        RockMobileUser.Instance.TryDownloadProfilePicture( PrivateGeneralConfig.ProfileImageSize, delegate 
                                            {
                                                // failure or not, server syncing is finished, so let's go ahead and 
                                                // get launch data.
                                                RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                                            });
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
                    if ( ResultCallback != null )
                    {
                        ResultCallback( statusCode, statusDescription );
                    }
                }

                public void SaveObjectsToDevice( )
                {
                    RockGeneralData.Instance.SaveToDevice( );
                    RockLaunchData.Instance.SaveToDevice( );
                    RockMobileUser.Instance.SaveToDevice( );
                }

                public void LoadObjectsFromDevice( )
                {
                    RockGeneralData.Instance.LoadFromDevice( );
                    RockLaunchData.Instance.LoadFromDevice( );
                    RockMobileUser.Instance.LoadFromDevice( );
                }
            }
        }
    }
}
