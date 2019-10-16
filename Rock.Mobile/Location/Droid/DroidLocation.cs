#if __ANDROID__
#if USE_LOCATION_SERVICES
using System;
using Android.App;
using Android.Content;
using Android.OS;
using DroidLocationServices;
using System.Collections.Generic;

namespace DroidLocationServices
{    
    public class DroidLocation : Location.Location, ILocationDelegateHandler
    {
        // our connection to the "delegate service" that is the go-between for us
        // and the actual locationService
        public LocationDelegateConnection LocationDelegateConnection { get; protected set; }
        public LocationDelegateBinder DelegateBinder { get; protected set; }

        public override void Create(object context)
        {
            // start and bind to the delegate service
            ((Android.Content.Context)context).BindService( new Intent( (Android.Content.Context)context, typeof( DroidLocationServices.LocationDelegateService )  ), 
                LocationDelegateConnection, Bind.AutoCreate );
        }

        public override void BeginModifyLocationsForTrack()
        {
            DelegateBinder.Service.BeginModifyLocationsForTrack( );
        }

        public override void AddLocation( string name, double latitude, double longitude, float radius )
        {
            DelegateBinder.Service.AddLocationForTrack( name, latitude, longitude, radius );
        }

        public override void CommitLocationsForTrack()
        {
            DelegateBinder.Service.CommitLocationsForTrack( );
        }

        public override void Start( )
        {
            DelegateBinder.Service.Start( );
        }

        public override void Stop( )
        {
            DelegateBinder.Service.Stop( );
        }

        public DroidLocation( ) : base( )
        {
            // establish our connection object
            LocationDelegateConnection = new LocationDelegateConnection( this );
        }

        public void ServiceConnected( IBinder binder )
        {
            General.Util.WriteToLog( "DroidLocation::ServiceConnected() - We are bound to LocationDelegateService" );

            DelegateBinder = (LocationDelegateBinder)binder;

            // give the service an instance of ourselves so it can notify us of events
            DelegateBinder.Service.SetHandler( this );

            // now wait for "OnReady", which means it can being processing our requests
        }

        public void ServiceDisconnected( )
        {
            General.Util.WriteToLog( "DroidLocation::ServiceDisconnected() - We are UNBOUND from LocationDelegateService" );

            if ( DelegateBinder != null )
            {
                DelegateBinder.Service.SetHandler( null );
                DelegateBinder = null;
            }
        }

        public void OnReady( )
        {
            General.Util.WriteToLog( "OnReady() - We are bound to LocationDelegate, and it is ready for us to give it work." );

            if ( OnReadyDelegate != null )
            {
                OnReadyDelegate( );
            }
        }

        public void OnLocationUpdate( List<string> locations )
        {
            Console.WriteLine( string.Format( "DroidLocation::On LOCATION UPDATE" ) );

            if( OnLocationUpdateDelegate != null )
            {
                OnLocationUpdateDelegate( locations );
            }
        }

        public void OnAuthChanged( )
        {
            // todo: implement
            if( OnLocationServiceAuthChangeDelegate != null )
            {
                OnLocationServiceAuthChangeDelegate( true );
            }
        }
    }
}
#endif
#endif