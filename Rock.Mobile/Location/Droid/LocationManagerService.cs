#if __ANDROID__
#if USE_LOCATION_SERVICES
using System;
using Android.OS;
using Android.Content;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Android.Support.V4.App;
using Java.Lang;

// HOW THIS WORKS:
//  LocationManagerService (the one in this file) is the core service that interacts with Android's FusedLocationApi and gets our location.
//  This service is Sticky and runs for the life of the main process that launched it. 

//  It communicates using LocationDelegateService, which is a service defined in "LocationDelegateService.cs". If that service is unloaded by Android (for memory)
//  this service can still work, it'll just be sending to nobody. (See note about service being killed at the bottom)

// Usage:
//  Use Location.OnCreate() which will create the Delegage service, this service, and bind them together.
//  Wait for the OnReady callback.
//  Call BeginModifyLocations
//  Add your locations
//  Call CommitLocations
//  Call Start() when you want to begin scanning
//  Call Stop() when you're finished.
//  
//  Note: Calling Stop() WILL stop the CoreLocation scanning (good), but will NOT kill/destroy/end either the DelegateService or LocationManagerService (also good.)
//        That way, if you want to scan again later, you can by calling Start().

//  Note on Service being Stopped
//      The only reason this service or LocationDelegateService would be stopped is the user tapping "Force Stop" in the Task Manager, or
//      Android killing them for memory purposes. Both of these should be extremely rare, and restarting the application will fix it.
//      As a "bonus", both services are set to "Sticky", so they SHOULD be restarted, and in theory the application can continue to use them.
//      At some point this should be more closely tested.

namespace DroidLocationServices
{
    public class TrackedLocation
    {
        public string Name { get; protected set; }
        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
        public float Radius { get; protected set; }

        public TrackedLocation( string name, double latitude, double longitude, float radius )
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            Radius = radius;
        }
    }

    // define the type of class the binder will be
    public class LocationManagerBinder : Binder
    {
        public LocationManagerService Service { get; protected set; }
        public LocationManagerBinder( LocationManagerService  service )
        {
            Service = service;
        }
    }

    public interface ILocationServiceHandler
    {
        // Called when the service is first connected to this ILocationServiceHandler via a binder
        void ServiceConnected( IBinder serviceBinder );

        // Called when the service is disconnected
        void ServiceDisconnected( );
    }

    public class LocationManagerConnection : Java.Lang.Object, IServiceConnection
    {
        ILocationServiceHandler ServiceHandler { get; set; }

        public LocationManagerConnection( ILocationServiceHandler serviceHandler )
        {
            ServiceHandler = serviceHandler;
        }

        public void OnServiceConnected( ComponentName name, IBinder serviceBinder )
        {
            LocationManagerBinder binder = serviceBinder as LocationManagerBinder;
            if ( binder != null )
            {
                ServiceHandler.ServiceConnected( binder );
            }
        }

        public void OnServiceDisconnected( ComponentName name )
        {
            ServiceHandler.ServiceDisconnected( );
        }
    }

    [Service( Label="LocationManagerService" )]
    public class LocationManagerService  : Service, Android.Gms.Common.Apis.IGoogleApiClientConnectionCallbacks, Android.Gms.Common.Apis.IGoogleApiClientOnConnectionFailedListener, Android.Gms.Location.ILocationListener, Android.Gms.Common.Apis.IResultCallback
    {
        // These are the events we'll send to the LocationDelegateService. Currently there's just one.
        public const string LocationEvent_LocationUpdate = "location_update";

        List<TrackedLocation> TrackedLocations { get; set; }

        // create our binder object that will be used to pass an instance of ourselves around
        IBinder Binder;

        // Reference to the interface for the location service.
        Android.Gms.Common.Apis.IGoogleApiClient ILocationServiceApi { get; set; }

        // The handler that receives callbacks on events defined in the interface.
        ILocationServiceHandler LocationServiceHandler { get; set; }

        public enum RunningState
        {
            Stopped,
            WantsStop,
            WantsStart,
            Started
        };

        RunningState State { get; set; }

        // Adding locations for tracking is an asynchronous operation. Because of this,
        // we define these states to manage the process.
        enum LocationCommitState
        {
            // No region track changes happening
            None,

            // After BeginModifyLocationsForTrack, so we know the user is adding regions.
            QueuingLocations,
        };
        LocationCommitState CommitState;

        // Stores the regions the user adds with AddRegionForTrack so that we can add them when GooglePlay is ready for us to.
        List<TrackedLocation> PendingLocationsForAdd { get; set; }

        // Stores the regions we should remove before we add new ones.
        List<string> PendingLocationsForRemove { get; set; }

        // If we receive a region add while disconnected from Google Services, we need to
        // store it and prcess it after we connect
        bool PendingLocationCommitEvent { get; set; }

        // True if since service creation we've connected to the google API.
        // This prevents us from running 'first time connection' stuff more than once.
        bool GoogleServicesFirstConnectionComplete { get; set; }

        // The filename for the region cache.
        static string CachedRegionsFileName 
        {
            get
            {
                string filePath = System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal );
                return filePath + "/" + "regions.bin";
            }
        }

        public void OnResult( Java.Lang.Object status )
        {
            //Status code 1000 means user has disallowed location tracking.
            Android.Gms.Common.Apis.IResult result = status as Android.Gms.Common.Apis.IResult;
            if ( result != null && result.Status.StatusCode == 1000  )
            {
                General.Util.WriteToLog( string.Format( "LOCATION SERVICE PERMISSION DISABLED BY USER. WE WILL NOT GET ANY UPDATES." ) );
            }
            else
            {
                General.Util.WriteToLog( string.Format( "LocationManagerService::OnResult() - (From GooglePlay) iPendingResult: {0} for Commit State: {1}", status, CommitState ) );
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();

            General.Util.WriteToLog( string.Format( "LocationManagerService::OnCreate" ) );

            GoogleServicesFirstConnectionComplete = false;
            PendingLocationCommitEvent = false;

            Binder = new LocationManagerBinder( this );

            // Build our interface to the google play location services.
            Android.Gms.Common.Apis.GoogleApiClientBuilder apiBuilder = new Android.Gms.Common.Apis.GoogleApiClientBuilder( this, this, this );
            apiBuilder.AddApi( LocationServices.Api );
            apiBuilder.AddConnectionCallbacks( this );
            apiBuilder.AddOnConnectionFailedListener( this );

            ILocationServiceApi = apiBuilder.Build( );
            //

            // establish a connection
            ILocationServiceApi.Connect( );

            // setup our regions
            TrackedLocations = new List<TrackedLocation>( );
        }

        public void SetLocationServiceHandler( ILocationServiceHandler locationServiceHandler )
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService::Set LocationServiceHandler" ) );
            LocationServiceHandler = locationServiceHandler;
        }


        public bool BeginModifyLocationsForTrack( )
        {
            // make sure the commit state is none so the user doesn't call this WHILE we're trying to add/remove locations.
            if( CommitState == LocationCommitState.None )
            {
                CommitState = LocationCommitState.QueuingLocations;

                // Because the service can be restarted, we need to cache a list of the locations they want to track.
                // By calling BeginModify, we can safely know that when they're done and call "Commit", we can
                // overwrite the cached locations.

                // we're ready for the user to begin adding locations
                return true;
            }

            return false;
        }

        public bool AddLocationForTrack( string name, double latitude, double longitude, float radius )
        {
            // make sure the user called 'BeginModifyLocationsForTrack'
            if ( CommitState == LocationCommitState.QueuingLocations )
            {
                General.Util.WriteToLog( string.Format( "LocationManagerService::Adding Location For Tracking: {0}", name ) );

                TrackedLocation newLocation = new TrackedLocation( name, latitude, longitude, radius );
                TrackedLocations.Add( newLocation );

                return true;
            }

            return false;
        }

        public void CommitLocationsForTrack( )
        {
            if ( CommitState == LocationCommitState.QueuingLocations )
            {
                General.Util.WriteToLog( string.Format( "LocationManagerService::CommitLocationsForTrack - Storing to cache file." ) );

                // immediately cache the regions to disk, so that if this service is restarted, we can restore them.
                SaveLocationsToDisk( );

                CommitState = LocationCommitState.None;
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService::OnStartCommand received. Returning Sticky" ) );

            // establish our API connection
            ILocationServiceApi.Connect( );

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            ILocationServiceApi.Disconnect( );
            General.Util.WriteToLog( string.Format( "LocationManagerService::OnDestroy" ) );
        }

        // when bound, return the Binder object containing our instance
        public override IBinder OnBind(Intent intent)
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService::OnBind (Someone requested to bind to us. Probably LocationDelegateService)" ) );
            return Binder;
        }

        // Google Play Services Callbacks
        public void OnConnected (Bundle bundle)
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService::OnConnected (Google Play Services ready)" ) );

            // only setup our scanning, load the cache and notify our handler if
            // this is our first connection since being created.
            if ( GoogleServicesFirstConnectionComplete == false )
            {
                GoogleServicesFirstConnectionComplete = true;

                // now either restore any necessary regions
                if ( File.Exists( CachedRegionsFileName ) == true )
                {
                    General.Util.WriteToLog( string.Format( "LocationManagerService - Found cached regions: restoring" ) );
                    LoadLocationsFromDisk( );
                }
            }

            // handle any pending requests
            if( State == RunningState.WantsStart )
            {
                Start( );
            }
            else if ( State == RunningState.WantsStop )
            {
                Stop( );
            }
        }

        public void Start( )
        {
            if ( ILocationServiceApi.IsConnected )
            {
                State = RunningState.Started;

                LocationRequest locationRequest = CreateLocationRequest( 10000 );
                LocationServices.FusedLocationApi.RequestLocationUpdates( ILocationServiceApi, locationRequest, this );
            }
            else
            {
                // establish our API connection, and then this will be called again connected.
                ILocationServiceApi.Connect( );

                State = RunningState.WantsStart;
            }
        }

        public void Stop( )
        {
            if( ILocationServiceApi.IsConnected )
            {
                State = RunningState.Stopped;

                // stop and disconnect
                LocationServices.FusedLocationApi.RemoveLocationUpdates( ILocationServiceApi, this );
                ILocationServiceApi.Disconnect( );
            }
            else
            {
                // it seems weird, but we need to connect in order to remove updates and disconnect.
                ILocationServiceApi.Connect( );

                State = RunningState.WantsStop;
            }
        }

        LocationRequest CreateLocationRequest( long interval )
        {
            LocationRequest locationRequest = new LocationRequest();
            locationRequest.SetPriority( 100 );
            locationRequest.SetFastestInterval( interval );
            locationRequest.SetInterval( interval );
            return locationRequest;
        }

        public void OnDisconnected (Bundle bundle)
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService: OnDisconnected" ) );
        }

        public void OnConnectionFailed (Bundle bundle)
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService: OnConnectionFailed" ) );
        }

        public void OnConnectionSuspended( int cause )
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService: OnConnectionSuspended Cause: {0}", cause ) );
        }

        public void OnConnectionFailed( ConnectionResult result )
        {
            General.Util.WriteToLog( string.Format( "LocationManagerService: OnConnectionFailed Cause: {0}", result ) );
        }

        public void OnLocationChanged( Android.Locations.Location location )
        {
            General.Util.WriteToLog( string.Format( "START LOCATION RECEIVED" ) );
            General.Util.WriteToLog( string.Format( "-----------------" ) );
            General.Util.WriteToLog( string.Format( "Longitude: " + location.Longitude ) );
            General.Util.WriteToLog( string.Format( "Latitude: " + location.Latitude ) );
            General.Util.WriteToLog( string.Format( "" ) );

            // see if the found location is within any of our tracked location radiuses (radii?)
            string foundLocations = string.Empty;

            foreach ( TrackedLocation trackedLocation in TrackedLocations )
            {
                Android.Locations.Location droidLocation = new Android.Locations.Location( trackedLocation.Name ) { Latitude = trackedLocation.Latitude, Longitude = trackedLocation.Longitude };
                float locationDist = droidLocation.DistanceTo( location );

                if( locationDist < trackedLocation.Radius )
                {
                    // create a comma dilimited list of locations found
                    foundLocations += trackedLocation.Name + ",";
                }
            }

            // remove trailing ","
            foundLocations = foundLocations.TrimEnd( new char[] { ',' } );

            SendLocationUpdateIntent( LocationEvent_LocationUpdate, foundLocations, string.Empty );

            General.Util.WriteToLog( string.Format( "-----------------" ) );
            General.Util.WriteToLog( string.Format( "END LOCATION RECEIVED\n" ) );
        }

        void SendLocationUpdateIntent( string action, string location, string extra )
        {
            Android.Net.Uri uri = Android.Net.Uri.FromParts( action, location, extra );
            StartService( new Intent("Location", uri, this, typeof( DroidLocationServices.LocationDelegateService ) ) );
        }

        private object locker = new object();
        void SaveLocationsToDisk( )
        {
            // first lock to make this asynchronous
            lock ( locker )
            {
                try
                {
                    // open the file
                    FileStream fileStream = new FileStream( CachedRegionsFileName, FileMode.Create );
                    BinaryWriter writer = new BinaryWriter( fileStream );

                    // write the number of regions
                    writer.Write( TrackedLocations.Count );

                    foreach ( TrackedLocation location in TrackedLocations )
                    {
                        // write the location properties
                        writer.Write( location.Name );
                        writer.Write( location.Latitude );
                        writer.Write( location.Longitude );
                        writer.Write( location.Radius );
                    }

                    // done
                    writer.Close( );
                    fileStream.Close( );
                }
                catch
                {
                    General.Util.WriteToLog( string.Format( "Failed to save locations to disk. If the service is restarted, these are gonna be gone" ) );
                }
            }
        }

        void LoadLocationsFromDisk( )
        {
            // first lock to make this synchronous
            lock ( locker )
            {
                try
                {
                    FileStream fileStream = new FileStream( CachedRegionsFileName, FileMode.Open );
                    BinaryReader binaryReader = new BinaryReader( fileStream );

                    // reset our region list
                    TrackedLocations = new List<TrackedLocation>( );

                    // read the number of cached regions
                    int locationCount = binaryReader.ReadInt32( );
                    for ( int i = 0; i < locationCount; i++ )
                    {
                        // read the region properties
                        string name = binaryReader.ReadString( );
                        double locationLatitude = binaryReader.ReadDouble( );
                        double locationLongitude = binaryReader.ReadDouble( );
                        float locationRadius = binaryReader.ReadSingle( );

                        // create and add the location
                        TrackedLocation location = new TrackedLocation( name, locationLatitude, locationLongitude, locationRadius );

                        TrackedLocations.Add( location );
                    }

                    // done
                    binaryReader.Close( );
                    fileStream.Close( );
                }
                catch
                {
                    General.Util.WriteToLog( string.Format( "Failed to load locations from disk. Until the activity runs and feeds us some, we can't scan for anything." ) );
                }
            }
        }

    }

}
#endif
#endif