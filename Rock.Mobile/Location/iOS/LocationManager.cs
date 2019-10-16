#if __IOS__
#if USE_LOCATION_SERVICES
using System;
using CoreLocation;
using UIKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;

namespace iOSLocationServices
{
    public class LocationManager
    {
        public class TrackedLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Radius { get; set; }
            public string Name { get; set; }
        }

        List<TrackedLocation> TrackedLocations { get; set; }
        CLLocationManager CLLocationManager { get; set; }

        public delegate void LocationUpdateDelegate( List<TrackedLocation> trackedLocations );
        LocationUpdateDelegate LocationUpdate { get; set; }
             
        public delegate void AuthorizationChangedDelegate( CLAuthorizationStatus status );
        AuthorizationChangedDelegate AuthorizationChanged { get; set; }

        public CLAuthorizationStatus AuthorizationStatus { get; protected set; }

        // If true, they've called Start. We'll store this so that if
        // the caller wanted to start, but permission hadn't been granted, we can turn it on after it IS granted.
        bool UserRequestedTurnOn { get; set; }

        public LocationManager( LocationUpdateDelegate locationUpdate, AuthorizationChangedDelegate authorizationChanged )
        {
            TrackedLocations = new List<TrackedLocation>();

            LocationUpdate = locationUpdate;

            AuthorizationChanged = authorizationChanged;

            // create our location manager and request permission to use GPS tracking
            CLLocationManager = new CLLocationManager();
            CLLocationManager.ActivityType = CLActivityType.Other;

            // ask for permission
            CLLocationManager.RequestWhenInUseAuthorization( );

            CLLocationManager.AuthorizationChanged += (object sender, CLAuthorizationChangedEventArgs e ) =>
                {
                    AuthorizationStatus = e.Status;

                    AuthorizationChanged( e.Status );

                    switch( e.Status )
                    {
                        case CLAuthorizationStatus.AuthorizedAlways:
                        case CLAuthorizationStatus.AuthorizedWhenInUse:
                        {
                            // get a fix on our location
                            if( UserRequestedTurnOn )
                            {
                                CLLocationManager.StartUpdatingLocation( );
                                UserRequestedTurnOn = false;
                            }
                            break;
                        }

                        // if they just turned us down, shut it all off
                        // any services that can relaunch the app (region monitoring & significant change location)
                        case CLAuthorizationStatus.Denied:
                        case CLAuthorizationStatus.NotDetermined:
                        case CLAuthorizationStatus.Restricted:
                        {
                            CLLocationManager.StopUpdatingLocation( );
                            break;
                        }
                    }
              };

            // setup our callback used when intense (real time) scanning is on and we get a new location from the device.
            CLLocationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e ) =>
                {
                    LocationReceived( this, e.Locations );
                };
        }

        public void Start( )
        {
            UserRequestedTurnOn = true;
            CLLocationManager.StartUpdatingLocation( );
        }

        public void Stop( )
        {
            CLLocationManager.StopUpdatingLocation( );
            UserRequestedTurnOn = false;
        }

        public void AddLocation( string name, double latitude, double longitude, float radius )
        {
            TrackedLocations.Add( new TrackedLocation( ) { Longitude = longitude, Latitude = latitude, Radius = radius, Name = name } );
        }

        public void LocationReceived(object sender, CLLocation[] locationList )
        {
            // if they're tracking locations, then provide them with updates
            if( TrackedLocations.Count > 0 )
            {
                List<TrackedLocation> nearbyLocations = new List<TrackedLocation>( );

                // go thru each location it gave us, (likey just one)
                foreach ( CLLocation location in locationList )
                {
                    Console.WriteLine( "Longitude: " + location.Coordinate.Longitude );
                    Console.WriteLine( "Latitude: " + location.Coordinate.Latitude );
                    Console.WriteLine( "" );

                    // see if any of these locations are within the radius of requsted locations
                    foreach ( TrackedLocation trackedLocation in TrackedLocations )
                    {
                        double locationDist = location.DistanceFrom( new CLLocation( trackedLocation.Latitude, trackedLocation.Longitude ) );
                        if ( locationDist < trackedLocation.Radius )
                        {
                            nearbyLocations.Add( trackedLocation );
                        }
                    }
                }

                // it's possible this will be blank. That's ok, because it'd be saying "You're tracking locations but not near any of them."
                LocationUpdate( nearbyLocations );
            }

            Console.WriteLine( "--------------------------" );
            Console.WriteLine( "END NEW LOCATION CHANGES" );
        }
    }
}
#endif
#endif
