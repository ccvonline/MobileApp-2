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

namespace DroidLocationServices
{
    public interface ILocationDelegateHandler
    {
        // Called when the service is first connected to this ILocationDelegateHandler via a binder
        void ServiceConnected( IBinder serviceBinder );

        // Called when the service is disconnected
        void ServiceDisconnected( );

        // Called when the LocationDelegateService binds to the LocationService and is
        // ready for the activity to start making requests.
        void OnReady( );

        // Called when the LocationManagerService has locations for us.
        void OnLocationUpdate( List<string> locations );
    }

    // define the type of class the binder will be
    public class LocationDelegateBinder : Binder
    {
        public LocationDelegateService Service { get; protected set; }
        public LocationDelegateBinder( LocationDelegateService service )
        {
            Service = service;
        }
    }

    public class LocationDelegateConnection : Java.Lang.Object, IServiceConnection
    {
        ILocationDelegateHandler DelegateHandler { get; set; }

        public LocationDelegateConnection( ILocationDelegateHandler serviceHandler )
        {
            DelegateHandler = serviceHandler;
        }

        public void OnServiceConnected( ComponentName name, IBinder serviceBinder )
        {
            LocationDelegateBinder binder = serviceBinder as LocationDelegateBinder;
            if ( binder != null )
            {
                DelegateHandler.ServiceConnected( binder );
            }
        }

        public void OnServiceDisconnected( ComponentName name )
        {
            DelegateHandler.ServiceDisconnected( );
        }
    }

    [Service( Label = "LocationDelegateService" )]
    public partial class LocationDelegateService : Service, ILocationServiceHandler
    {
        // The connection and binder used to bind THIS delegateService to the LocationService
        LocationManagerConnection DroidLocationManagerConnection { get; set; }
        LocationManagerBinder ServiceBinder { get; set; }

        // OUR binder that will be used to bind us to the activity
        IBinder Binder;

        // The handler that wants to manage events we receive.
        ILocationDelegateHandler ILocationDelegateHandler { get; set; }

        public LocationDelegateService( ) : base(  )
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            General.Util.WriteToLog( "LocationDelegateService::OnCreate()" );

            DroidLocationManagerConnection = new LocationManagerConnection( this );

            Binder = new LocationDelegateBinder( this );

            ILocationDelegateHandler = null;

            // start the location manager service.
            StartService( new Intent( this, typeof( DroidLocationServices.LocationManagerService ) ) );
        }

        public override void OnDestroy()
        {
            General.Util.WriteToLog( "LocationDelegateService::OnDestroy() - Android must need memory. Unbinding from DroidLocationService." );

            ServiceBinder.Service.SetLocationServiceHandler( null );
            UnbindService( DroidLocationManagerConnection );

            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            General.Util.WriteToLog( "LocationDelegateService::OnBind() - Someone is binding to us. (Probably the front-end activity.)" );
            return Binder;
        }

        public void SetHandler( ILocationDelegateHandler iLocationDelegateHandler )
        {
            General.Util.WriteToLog( "LocationDelegateService::SetHandler() - Accepting a handler for our events (probably the front-end activity.)" );

            ILocationDelegateHandler = iLocationDelegateHandler;

            if ( ILocationDelegateHandler != null )
            {
                // we need to ensure we're bound to the location service before allowing
                // our handler to make any calls
                if ( ServiceBinder != null )
                {
                    ILocationDelegateHandler.OnReady( );
                }
                // we're not bound, so first bind, then we'll notify our handler
                else
                {
                    TryBindLocationService( );
                }
            }
        }

        void TryBindLocationService( )
        {
            // if we aren't bound, bind.
            if ( ServiceBinder == null )
            {
                General.Util.WriteToLog( "LocationDelegateService::TryBindLocationService() - Requesting BIND to DroidLocationService." );
                BindService( new Intent( this, typeof( DroidLocationServices.LocationManagerService ) ), DroidLocationManagerConnection, Bind.AutoCreate );
            }
            else
            {
                General.Util.WriteToLog( "LocationDelegateService::TryBindLocationService() - Do nothing. already bound to DroidLocationService." );
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            General.Util.WriteToLog( "LocationDelegateService::OnStartCommand()" );

            TryBindLocationService( );

            if ( intent.Data != null )
            {
                // if there's a handler, call it so the normal application flow can occur
                if ( ILocationDelegateHandler != null )
                {
                    General.Util.WriteToLog( "LocationDelegateService::Calling HandleForegroundLocationEvent." );
                    HandleLocationEvent( intent.Data.Scheme, intent.Data.SchemeSpecificPart, intent.Data.Fragment );
                }
            }

            return StartCommandResult.Sticky;
        }

        void HandleLocationEvent( string eventStr, string major, string minor )
        {
            switch ( eventStr )
            {
                case LocationManagerService.LocationEvent_LocationUpdate:
                {
                    Console.WriteLine( string.Format( "LocationDelegateService: Entered Locations: {0}", major ) );

                    // split the locations based on ','
                    string[] locations = major.Split( ',' );

                    ILocationDelegateHandler.OnLocationUpdate( locations.ToList( ) );
                    break;
                }
            }
        }

        public bool Start( )
        {
            if ( ServiceBinder != null )
            {
                ServiceBinder.Service.Start( );
                return true;
            }

            General.Util.WriteToLog( "LocationDelegateService::Start() failed. Not bound to DroidLocationService." );
            return false;
        }

        public bool Stop( )
        {
            if ( ServiceBinder != null )
            {
                ServiceBinder.Service.Stop( );
                return true;
            }

            General.Util.WriteToLog( "LocationDelegateService::Stop() failed. Not bound to DroidLocationService." );
            return false;
        }

        public bool BeginModifyLocationsForTrack( )
        {
            if ( ServiceBinder != null )
            {
                return ServiceBinder.Service.BeginModifyLocationsForTrack( );
            }

            General.Util.WriteToLog( "LocationDelegateService::BeginModifyLocationsForTrack() failed. Not bound to DroidLocationService." );
            return false;
        }

        public bool AddLocationForTrack( string name, double latitude, double longitude, float radius )
        {
            if ( ServiceBinder != null )
            {
                return ServiceBinder.Service.AddLocationForTrack( name, latitude, longitude, radius );
            }

            General.Util.WriteToLog( "LocationDelegateService::AddLocationForTrack() failed. Not bound to DroidLocationService." );
            return false;
        }

        public bool CommitLocationsForTrack( )
        {
            if ( ServiceBinder != null )
            {
                ServiceBinder.Service.CommitLocationsForTrack( );
                return true;
            }

            General.Util.WriteToLog( "LocationDelegateService::CommitLocationsForTrack() failed. Not bound to DroidLocationService." );
            return false;
        }

        public void ServiceConnected( IBinder binder )
        {
            General.Util.WriteToLog( "LocationDelegateService::ServiceConnected(). Now BOUND to DroidLocationService." );

            ServiceBinder = (LocationManagerBinder)binder;

            // give the service an instance of ourselves so it can notify us of events
            ServiceBinder.Service.SetLocationServiceHandler( this );

            // if we have a handler, notify it we're ready
            if ( ILocationDelegateHandler != null )
            {
                ILocationDelegateHandler.OnReady( );
            }
        }

        public void ServiceDisconnected( )
        {
            General.Util.WriteToLog( "LocationDelegateService::ServiceDisconnected(). UNBOUND from DroidLocationService." );

            // we were disconnected. Null our binder so we can re-obtain it when we need it.
            ServiceBinder = null;
        }
    }
}
#endif
#endif