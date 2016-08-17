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

                bool Requesting { get; set; }

                public RockNetworkManager( )
                {
                    RockApi.SetRockURL( Config.GeneralConfig.RockBaseUrl );
                    RockApi.SetAuthorizationKey( Config.GeneralConfig.RockMobileAppAuthorizationKey );

                    Requesting = false;
                }

                public void SyncRockData( SeriesDownloaded seriesCallback, HttpRequest.RequestResult resultCallback )
                {
                    // if a request is already being made, don't handle a second one.
                    if( Requesting == false )
                    {
                        Requesting = true;

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
                }

                void LaunchDataReceived(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // New general data received. Save it, update fields, etc.
                    if ( ResultCallback != null )
                    {
                        ResultCallback( statusCode, statusDescription );

                        ResultCallback = null;
                    }

                    Requesting = false;
                }

                public void SaveObjectsToDevice( )
                {
                    RockLaunchData.Instance.SaveToDevice( );
                    RockMobileUser.Instance.SaveToDevice( );
                }

                public void LoadObjectsFromDevice( )
                {
                    RockLaunchData.Instance.LoadFromDevice( );
                    RockMobileUser.Instance.LoadFromDevice( );
                }
            }
        }
    }
}
