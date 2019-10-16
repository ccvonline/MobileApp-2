#if __IOS__
#if USE_LOCATION_SERVICES
using System.Collections.Generic;
using System;
using Location;
using CoreLocation;

namespace iOSLocationServices
{
    public class iOSLocation : Location.Location
    {
        public LocationManager LocationManager { get; protected set; }

        public iOSLocation( )
        {
        }

        // override the ready delegate so that if we've been created
        // before this was set, we can notify the caller that we're ready.
        public override OnReadyCallback OnReadyDelegate 
        { 
            get 
            { 
                return base.OnReadyDelegate; 
            }

            set
            {
                if ( LocationManager != null )
                {
                    base.OnReadyDelegate = value;
                    base.OnReadyDelegate( );
                }
            }
        }

        public override void Create( object context )
        {
            LocationManager = new LocationManager( LocationUpdateDelegate, AuthorizationChanged );

            // iOS is immediately ready
            if ( OnReadyDelegate != null )
            {
                OnReadyDelegate( );
            }
        }

        public override void BeginModifyLocationsForTrack()
        {
            // this is for Android. we don't need to do anything here
        }

        public override void CommitLocationsForTrack()
        {
            // just for android
        }

        public override void Start ()
        {
            LocationManager.Start( );
        }

        public override void Stop( )
        {
            LocationManager.Stop( );
        }

        public override void AddLocation( string name, double latitude, double longitude, float radius )
        {
            LocationManager.AddLocation( name, latitude, longitude, radius );
        }

        public void LocationUpdateDelegate( List<LocationManager.TrackedLocation> trackedLocations )
        {
            // build a list of just the names
            List<string> locations = new List<string>( );
            foreach( LocationManager.TrackedLocation location in trackedLocations )
            {
                locations.Add( location.Name );
            }

            if( OnLocationUpdateDelegate != null )
            {
                OnLocationUpdateDelegate( locations );
            }
        }

        public void AuthorizationChanged( CLAuthorizationStatus status )
        {
            if( OnLocationServiceAuthChangeDelegate != null )
            {
                switch( status )
                {
                    case CLAuthorizationStatus.AuthorizedAlways:
                    case CLAuthorizationStatus.AuthorizedWhenInUse:
                    {
                        OnLocationServiceAuthChangeDelegate( true );
                        break;
                    }

                    case CLAuthorizationStatus.Denied:
                    case CLAuthorizationStatus.Restricted:
                    {
                        OnLocationServiceAuthChangeDelegate( false );
                        break;
                    }

                    case CLAuthorizationStatus.NotDetermined:
                    {
                        // if we're not sure, don't issue an alert
                        break;
                    }
                }
            }
        }
    }
}
#endif
#endif