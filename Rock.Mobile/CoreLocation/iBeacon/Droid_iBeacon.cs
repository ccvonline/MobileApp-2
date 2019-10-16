#if __ANDROID_18__ && _USE_IBEACON_
using System;
using System.Collections.Generic;

using Android.App;
using RadiusNetworks.IBeaconAndroid;

namespace Rock.Mobile
{
    namespace CoreLocation
    {
        namespace iBeacon
        {
            public class DroidBeacon : Beacon
            {
                #region MemberAccessors //Accessor Implementations

                protected override string getUUID( )
                {
                    return IBeacon != null ? IBeacon.ProximityUuid : string.Empty;
                }

                protected override Proximity getProximity( )
                {
                    // cast to our proximity, which is the same mapping
                    return IBeacon != null ? ( Proximity )IBeacon.Proximity : Proximity.Unknown;
                }

                protected override double getAccuracy( )
                {
                    return IBeacon != null ? IBeacon.Accuracy : 0.00;
                }

                protected override ushort getMajor( )
                {
                    return IBeacon != null ? ( ushort )IBeacon.Major : ( ushort )0;
                }

                protected override ushort getMinor( )
                {
                    return IBeacon != null ? ( ushort )IBeacon.Minor : ( ushort )0;
                }

                #endregion MemberAccessors //End Accessor Implementations

                // Platform implementation
                public IBeacon IBeacon { get; set; }

                public DroidBeacon( IBeacon iBeacon )
                {
                    IBeacon = iBeacon;
                }
            }

            public class DroidBeaconRegion : BeaconRegion
            {
                #region MemberAccessors //Accessor Implementations

                protected override string getUUID( )
                {
                    return Region != null ? Region.ProximityUuid : string.Empty;
                }

                //NotifyOnEntry
                protected override bool getNotifyOnEntry( )
                {
                    return true;
                }

                protected override void setNotifyOnEntry( bool value )
                {
                    if( Region != null )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "Warning: NotifyOnEntry being set, but not supported on Android" );
                    }
                }

                //NotifyonExit
                protected override bool getNotifyOnExit( )
                {
                    return true;
                }

                protected override void setNotifyOnExit( bool value )
                {
                    if( Region != null )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "Warning: NotifyOnExit being set, but not supported on Android" );
                    }
                }

                //NotifyEntryStateOnDisplay
                protected override bool getNotifyEntryStateOnDisplay( )
                {
                    return true;
                }

                protected override void setNotifyEntryStateOnDisplay( bool value )
                {
                    if( Region != null )
                    {
                        Rock.Mobile.Util.Debug.WriteLine( "Warning: NotifyEntryStateOnDisplay being set, but not supported on Android" );
                    }
                }

                #endregion //End Accessor Implementations

                // Platform Implementation
                public Region Region { get; set; }

                public DroidBeaconRegion( string uuid, string regionTag )
                {
                    // this constructor is called by the user, so validate uuid
                    if( string.IsNullOrEmpty( uuid ) || string.IsNullOrEmpty( regionTag ) )
                    {
                        throw new Exception( "Cannot create a Region with a blank UUID" );
                    }
                    Init( uuid, 0, 0, regionTag );
                }

                public DroidBeaconRegion( Region region )
                {
                    Region = region;
                }

                public DroidBeaconRegion( string uuid, ushort major, ushort minor, string regionTag )
                {
                    // this constructor will only be called by THIS implementation,
                    // so we don't need to validate uuid
                    Init( uuid, major, minor, regionTag );
                }

                public void Init( string uuid, ushort major, ushort minor, string regionTag )
                {
                    //TODO: Support major/minor

                    // this should be called by all constructors
                    Region = null;

                    if( !string.IsNullOrEmpty( uuid ) && !string.IsNullOrEmpty( regionTag ) )
                    {
                        Region = new Region( regionTag, uuid, null, null );
                    }
                }
            }

            public class DroidRegionEventArgs : RegionEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override BeaconRegion getRegion( )
                {
                    return new DroidBeaconRegion( DroidRegion ) as BeaconRegion;
                }

                #endregion //End Accessor Implementations

                //Platform Implementation
                public Region DroidRegion { get; set; }

                public DroidRegionEventArgs( BeaconRegion region )
                {
                    DroidRegion = ( region as DroidBeaconRegion ).Region;
                }
            }

            public class DroidRegionBeaconsRangedEventArgs : RegionBeaconsRangedEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override Beacon [] getBeacons( )
                {
                    // create an array of DroidBeacons and fill it with the Beacons of the event args
                    DroidBeacon[] beacons = new DroidBeacon[IBeacons.Count];
                    int index = 0;
                    foreach( IBeacon ibeacon in IBeacons )
                    {
                        beacons[ index++ ] = new DroidBeacon( ibeacon );
                    }

                    return beacons as Beacon[];
                }

                protected override BeaconRegion getRegion( )
                {
                    return new DroidBeaconRegion( DroidRegion ) as BeaconRegion;
                }

                #endregion //End Accessor Implementations

                // Platform Implementation
                public ICollection<IBeacon> IBeacons { get; set; }

                public Region DroidRegion { get; set; }

                public DroidRegionBeaconsRangedEventArgs( ICollection<IBeacon> ibeacons, Region region )
                {
                    IBeacons = ibeacons;
                    DroidRegion = region;
                }
            }

            public class DroidRegionStateDeterminedEventArgs : RegionStateDeterminedEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override RegionState getRegionState( )
                {
                    return ( RegionState )State;
                }

                protected override BeaconRegion getRegion( )
                {
                    return BeaconRegion;
                }

                #endregion //End Accessor Implementations

                public int State { get; set; }

                public BeaconRegion BeaconRegion { get; set; }

                public DroidRegionStateDeterminedEventArgs( int state, Region droidRegion )
                {
                    State = state;
                    BeaconRegion = new DroidBeaconRegion( droidRegion );
                }
            }

            public class DroidLocationManager : LocationManager, IMonitorNotifier, IRangeNotifier
            {
                //Platform Implementation
                protected IBeaconManager IBeaconManager = null;
                protected bool IBeaconManagerBound = false;

                // This enables us to queue requests to start/stop monitoring/ranging
                // in the case that IBeaconManager hasn't been bound by the first call to LocationManager.
                protected List<BeaconRegion> PendingStartMonitor = new List<BeaconRegion>( );
                protected List<BeaconRegion> PendingStopMonitor = new List<BeaconRegion>( );

                protected List<BeaconRegion> PendingStartRanging = new List<BeaconRegion>( );
                protected List<BeaconRegion> PendingStopRanging = new List<BeaconRegion>( );
                //

                protected List<BeaconRegion> BeaconRegionsRequestingState = new List<BeaconRegion>( );

                public DroidLocationManager( )
                {
                }

                ~DroidLocationManager()
                {
                }

                public void BindIBeaconManager( Activity mainActivity )
                {
                    IBeaconManager = IBeaconManager.GetInstanceForApplication( mainActivity );
                    IBeaconManager.Bind( mainActivity as IBeaconConsumer );
                }

                public void UnBindIBeaconManager( Activity mainActivity )
                {
                    if( IBeaconManager != null )
                    {
                        IBeaconManager.UnBind( mainActivity as IBeaconConsumer );
                    }
                }

                public void EnterForegroundMode( Activity mainActivity )
                {
                    if( IBeaconManager != null )
                    {
                        IBeaconManager.SetBackgroundMode( mainActivity as IBeaconConsumer, false );
                    }
                }

                public void EnterBackgroundMode( Activity mainActivity )
                {
                    if( IBeaconManager != null )
                    {
                        IBeaconManager.SetBackgroundMode( mainActivity as IBeaconConsumer, true );
                    }
                }

                //IRangeNotifier
                public void DidRangeBeaconsInRegion( ICollection<IBeacon> beacons, Region region )
                {
                    // build our event notifications
                    if( beacons.Count > 0 )
                    {
                        DroidRegionBeaconsRangedEventArgs eventArgs = new DroidRegionBeaconsRangedEventArgs( beacons, region );

                        OnRegionBeaconsRangedEvent( this, eventArgs as RegionBeaconsRangedEventArgs );
                    }
                }

                //IMonitorNotifier
                public void DidDetermineStateForRegion( int p0, Region p1 )
                {
                    BeaconRegion region = BeaconRegionsRequestingState.Find( br => br.UUID == p1.ProximityUuid );
                    if( region != null )
                    {
                        DroidRegionStateDeterminedEventArgs args = new DroidRegionStateDeterminedEventArgs( p0, p1 );

                        // notify people that care
                        OnDidDetermineStateEvent( this, args );

                        BeaconRegionsRequestingState.Remove( region );
                    }
                }

                public void DidEnterRegion( Region p0 )
                {
                    DroidBeaconRegion region = new DroidBeaconRegion( p0 );
                    DroidRegionEventArgs eventArgs = new DroidRegionEventArgs( region );

                    OnRegionEnteredEvent( this, eventArgs as RegionEventArgs );
                }

                public void DidExitRegion( Region p0 )
                {
                    DroidBeaconRegion region = new DroidBeaconRegion( p0 );
                    DroidRegionEventArgs eventArgs = new DroidRegionEventArgs( region );

                    OnRegionExitedEvent( this, eventArgs as RegionEventArgs );
                }
                //

                // Handle any pending start/stop requests
                public void OnIBeaconServiceConnect( IBeaconConsumer consumer )
                {
                    // become the listener
                    IBeaconManager.SetMonitorNotifier( this );
                    IBeaconManager.SetRangeNotifier( this );

                    // protect against multiple calls
                    if( IBeaconManagerBound == false )
                    {
                        IBeaconManagerBound = true;

                        //Start Monitoring
                        foreach( BeaconRegion region in PendingStartMonitor )
                        {
                            StartMonitoring( region );
                        }

                        //Stop Monitoring
                        foreach( BeaconRegion region in PendingStopMonitor )
                        {
                            StopMonitoring( region );
                        }

                        //Start Ranging
                        foreach( BeaconRegion region in PendingStartRanging )
                        {
                            StartRangingBeacons( region );
                        }

                        //Stop Ranging
                        foreach( BeaconRegion region in PendingStopRanging )
                        {
                            StopRangingBeacons( region );
                        }

                        // clear lists
                        PendingStartMonitor.Clear( );
                        PendingStopMonitor.Clear( );
                        PendingStartRanging.Clear( );
                        PendingStopRanging.Clear( );
                    }
                }

                public override void StartMonitoring( BeaconRegion region )
                {
                    if( IBeaconManagerBound == true )
                    {
                        DroidBeaconRegion droidRegion = region as DroidBeaconRegion;
                        IBeaconManager.StartMonitoringBeaconsInRegion( droidRegion.Region );
                    }
                    else
                    {
                        // queue it for when we ARE bound
                        PendingStartMonitor.Add( region );
                    }
                }

                public override void StopMonitoring( BeaconRegion region )
                {
                    if( IBeaconManagerBound == true )
                    {
                        DroidBeaconRegion droidRegion = region as DroidBeaconRegion;
                        IBeaconManager.StopMonitoringBeaconsInRegion( droidRegion.Region );
                    }
                    else
                    {
                        // queue it for when we ARE bound
                        PendingStopMonitor.Add( region );
                    }
                }

                public override void RequestStateForRegion( BeaconRegion region )
                {
                    // add to a list the region they requested
                    BeaconRegionsRequestingState.Add( region );
                }

                public override void StartRangingBeacons( BeaconRegion region )
                {
                    if( IBeaconManagerBound == true )
                    {
                        DroidBeaconRegion droidRegion = region as DroidBeaconRegion;

                        Rock.Mobile.Util.Debug.WriteLine( "START ranging beacons with UUID: " + droidRegion.Region.ProximityUuid );
                        IBeaconManager.StartRangingBeaconsInRegion( droidRegion.Region );
                    }
                    else
                    {
                        // queue it for when we ARE bound
                        PendingStartRanging.Add( region );
                    }
                }

                public override void StopRangingBeacons( BeaconRegion region )
                {
                    if( IBeaconManagerBound == true )
                    {
                        DroidBeaconRegion droidRegion = region as DroidBeaconRegion;

                        Rock.Mobile.Util.Debug.WriteLine( "Stop ranging beacons with UUID: " + droidRegion.Region.ProximityUuid );
                        IBeaconManager.StopRangingBeaconsInRegion( droidRegion.Region );
                    }
                    else
                    {
                        // queue it for when we ARE bound
                        PendingStopRanging.Add( region );
                    }
                }

                public override bool IsAvailable( )
                {
                    if( IBeaconManager != null )
                    {
                        return IBeaconManager.CheckAvailability( );
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
#elif __ANDROID__
using System;
using System.Collections.Generic;

using Android.App;

namespace Rock.Mobile
{
    namespace CoreLocation
    {
        namespace iBeacon
        {
            public class DroidLocationManager : LocationManager
            {
                public DroidLocationManager( )
                {
                }

                ~DroidLocationManager()
                {
                }

                public override void StartMonitoring( BeaconRegion region )
                {
                }

                public override void StopMonitoring( BeaconRegion region )
                {
                }

                public override void RequestStateForRegion( BeaconRegion region )
                {
                }

                public override void StartRangingBeacons( BeaconRegion region )
                {
                }

                public override void StopRangingBeacons( BeaconRegion region )
                {
                }

                public void BindIBeaconManager( Activity mainActivity )
                {
                }

                public void UnBindIBeaconManager( Activity mainActivity )
                {
                }

                public void EnterForegroundMode( Activity mainActivity )
                {
                }

                public void EnterBackgroundMode( Activity mainActivity )
                {
                }

                public override bool IsAvailable( )
                {
                    return false;
                }
            }
        }
    }
}
#endif