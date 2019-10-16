#if __IOS__ || __ANDROID__
using System;

namespace Rock.Mobile
{
    namespace CoreLocation
    {
        namespace iBeacon
        {
            public enum Proximity
            {
                Unknown,
                Immediate,
                Near,
                Far
            }

            public enum RegionState
            {
                Inside,
                Outside,
                Unknown
            }

            // Implements the Beacon object, which defines the actual beacon
            public abstract class Beacon
            {
                #region MemberAccessors // Abstracted accessors for beacon members

                public string UUID
                {
                    get { return getUUID( ); }
                }

                protected abstract string getUUID( );

                public Proximity Proximity
                {
                    get { return getProximity( ); }
                }

                protected abstract Proximity getProximity( );

                public double Accuracy
                {
                    get { return getAccuracy( ); }
                }

                protected abstract double getAccuracy( );

                public ushort Major
                {
                    get { return getMajor( ); }
                }

                protected abstract ushort getMajor( );

                public ushort Minor
                {
                    get { return getMinor( ); }
                }

                protected abstract ushort getMinor( );

                #endregion //End Abstracted accessors
            }

            // Implements a BeaconRegion object, which controls beacons based on a UUID/Major/Minor
            public abstract class BeaconRegion
            {
                #region MemberAccessors // Abstracted accessors for beacon members

                public string UUID
                {
                    get { return getUUID( ); }
                }

                protected abstract string getUUID( );

                public bool NotifyOnEntry
                { 
                    get { return getNotifyOnEntry( ); }

                    set
                    {
                        setNotifyOnEntry( value );
                    }
                }

                protected abstract bool getNotifyOnEntry( );

                protected abstract void setNotifyOnEntry( bool value );

                public bool NotifyOnExit
                { 
                    get { return getNotifyOnExit( ); }

                    set
                    {
                        setNotifyOnExit( value );
                    }
                }

                protected abstract bool getNotifyOnExit( );

                protected abstract void setNotifyOnExit( bool value );

                public bool NotifyEntryStateOnDisplay
                { 
                    get { return getNotifyEntryStateOnDisplay( ); }

                    set
                    {
                        setNotifyEntryStateOnDisplay( value );
                    }
                }

                protected abstract bool getNotifyEntryStateOnDisplay( );

                protected abstract void setNotifyEntryStateOnDisplay( bool value );

                #endregion //End Abstracted accessors

                public static BeaconRegion Create( string uuid, string regionTag )
                {
                    #if __IOS__
					return new iOSBeaconRegion(uuid, regionTag);
                    #endif

                    #if __ANDROID_18__ && _USE_IBEACON_
                    return new DroidBeaconRegion( uuid, regionTag );
                    #elif __ANDROID__
                    return null;
                    #endif
                }
            }

            // Implements RegionEventArgs, used for notifying Region Entered/Exit
            public abstract class RegionEventArgs
            {
                #region MemberAccessors // Abstracted accessors

                public BeaconRegion Region { get { return getRegion( ); } }

                protected abstract BeaconRegion getRegion( );

                #endregion //End Abstracted accessors
            }

            //Implements RegionBeaconsRangedEventArgs, used for notifying when beacons are successfully Ranged.
            public abstract class RegionBeaconsRangedEventArgs
            {
                #region MemberAccessors // Abstracted accessors

                public Beacon [] Beacons { get { return getBeacons( ); } }

                protected abstract Beacon [] getBeacons( );

                public BeaconRegion Region { get { return getRegion( ); } }

                protected abstract BeaconRegion getRegion( );

                #endregion //End Abstracted accessors
            }

            //Implements RegionStateDeterminedEventArgs, used for notifying when the state of a BeaconRegion is determined.
            public abstract class RegionStateDeterminedEventArgs
            {
                #region MemberAccessors //Abstracted accessors

                public RegionState RegionState { get { return getRegionState( ); } }

                protected abstract RegionState getRegionState( );

                public BeaconRegion Region { get { return getRegion( ); } }

                protected abstract BeaconRegion getRegion( );

                #endregion //End Abstracted accessors
            }

            //Implements the main LocationManager, which manages all Beacons and Ranges
    #if __IOS__
			public abstract class LocationManager
	#endif
            #if __ANDROID__
            public abstract class LocationManager : Java.Lang.Object
	#endif
			{
                // use a singleton so we don't create multiple LocationManagers.
                // That would be bad.
                protected static LocationManager instance;

                protected LocationManager( )
                {
                }

                public static LocationManager Instance
                {
                    get
                    {
                        if( instance == null )
                        {
                            #if __IOS__
							instance = new iOSLocationManager();
                            #endif

                            #if __ANDROID__
                            instance = new DroidLocationManager( );
                            #endif
                        }
                        return instance;
                    }
                }

                public delegate void RegionEvent( object s, RegionEventArgs args );

                public delegate void RegionBeaconsRangedEvent( object s, RegionBeaconsRangedEventArgs args );

                public delegate void DidDetermineStateEvent( object s, RegionStateDeterminedEventArgs args );

                public event RegionEvent RegionEnteredEvents;

                protected void OnRegionEnteredEvent( object s, RegionEventArgs args )
                {
                    if( RegionEnteredEvents != null )
                    {
                        RegionEnteredEvents( s, args );
                    }
                }

                public event RegionEvent RegionExitedEvents;

                protected void OnRegionExitedEvent( object s, RegionEventArgs args )
                {
                    if( RegionExitedEvents != null )
                    {
                        RegionExitedEvents( s, args );
                    }
                }

                public event RegionBeaconsRangedEvent RegionBeaconsRangedEvents;

                protected void OnRegionBeaconsRangedEvent( object s, RegionBeaconsRangedEventArgs args )
                {
                    if( RegionBeaconsRangedEvents != null )
                    {
                        RegionBeaconsRangedEvents( s, args );
                    }
                }

                public event DidDetermineStateEvent DidDetermineStateEvents;

                protected void OnDidDetermineStateEvent( object s, RegionStateDeterminedEventArgs args )
                {
                    if( DidDetermineStateEvents != null )
                    {
                        DidDetermineStateEvents( s, args );
                    }
                }

                public abstract void StartMonitoring( BeaconRegion region );

                public abstract void StopMonitoring( BeaconRegion region );

                public abstract void RequestStateForRegion( BeaconRegion region );

                public abstract void StartRangingBeacons( BeaconRegion region );

                public abstract void StopRangingBeacons( BeaconRegion region );

                public abstract bool IsAvailable( );
            }
        }
    }
}
#endif