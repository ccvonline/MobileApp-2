
/// <summary>
/// ****IF YOU WANT TO USE THIS, DEFINE THE PRE-PROCESSOR DIRECTIVE USE_LOCATION_SERVICES ****
/// </summary>
#if USE_LOCATION_SERVICES

using System.Collections.Generic;
using System;

namespace Location
{
    public abstract class Location
    {
        static Location _Instance;
        public static Location Instance 
        {
            get 
            { 
                if ( _Instance == null )
                {
                    #if __ANDROID__
                    _Instance = new DroidLocationServices.DroidLocation();
                    #elif __IOS__
                    _Instance = new iOSLocationServices.iOSLocation();
                    #endif
                }
                return _Instance; 
            } 
        }

        public Location( )
        {   
        }

        public abstract void Create( object context );

        public abstract void Start( );

        public abstract void Stop( );

        public abstract void BeginModifyLocationsForTrack( );

        public abstract void AddLocation( string name, double latitude, double longitude, float radius );

        public abstract void CommitLocationsForTrack( );

        public delegate void OnReadyCallback( );
        public virtual OnReadyCallback OnReadyDelegate { get; set; }

        public delegate void OnLocationUpdateCallback( List<string> locations );
        public OnLocationUpdateCallback OnLocationUpdateDelegate { get; set; }

        public delegate void OnLocationServiceAuthChangeCallback( bool enabled );
        public OnLocationServiceAuthChangeCallback OnLocationServiceAuthChangeDelegate { get; set; }
    }
}
#endif
