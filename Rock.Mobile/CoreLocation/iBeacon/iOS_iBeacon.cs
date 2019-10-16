#if __IOS__
using System;
using CoreLocation;
using Foundation;

namespace Rock.Mobile
{
    namespace CoreLocation
    {
        namespace iBeacon
        {
            public class iOSBeacon : Beacon
            {
                #region MemberAccessors //Accessor Implementations

                protected override string getUUID( )
                {
                    return CLBeacon != null ? CLBeacon.ProximityUuid.AsString( ) : string.Empty;
                }

                protected override Proximity getProximity( )
                {
                    // cast to our proximity, which is the same mapping
                    return CLBeacon != null ? ( Proximity )CLBeacon.Proximity : Proximity.Unknown;
                }

                protected override double getAccuracy( )
                {
                    return CLBeacon != null ? CLBeacon.Accuracy : 0.00;
                }

                protected override ushort getMajor( )
                {
                    return CLBeacon != null ? CLBeacon.Major.UInt16Value : ( ushort )0;
                }

                protected override ushort getMinor( )
                {
                    return CLBeacon != null ? CLBeacon.Minor.UInt16Value : ( ushort )0;
                }

                #endregion MemberAccessors //End Accessor Implementations

                // Platform implementation
                public CLBeacon CLBeacon { get; set; }

                public iOSBeacon( CLBeacon clBeacon )
                {
                    CLBeacon = clBeacon;
                }
            }

            public class iOSBeaconRegion : BeaconRegion
            {
                #region MemberAccessors //Accessor Implementations

                protected override string getUUID( )
                {
                    return CLBeaconRegion != null ? CLBeaconRegion.ProximityUuid.AsString( ) : string.Empty;
                }

                //NotifyOnEntry
                protected override bool getNotifyOnEntry( )
                {
                    return CLBeaconRegion != null ? CLBeaconRegion.NotifyOnEntry : false;
                }

                protected override void setNotifyOnEntry( bool value )
                {
                    if( CLBeaconRegion != null )
                    {
                        CLBeaconRegion.NotifyOnEntry = value;
                    }
                }

                //NotifyonExit
                protected override bool getNotifyOnExit( )
                {
                    return CLBeaconRegion != null ? CLBeaconRegion.NotifyOnExit : false;
                }

                protected override void setNotifyOnExit( bool value )
                {
                    if( CLBeaconRegion != null )
                    {
                        CLBeaconRegion.NotifyOnExit = value;
                    }
                }

                //NotifyEntryStateOnDisplay
                protected override bool getNotifyEntryStateOnDisplay( )
                {
                    return CLBeaconRegion != null ? CLBeaconRegion.NotifyEntryStateOnDisplay : false;
                }

                protected override void setNotifyEntryStateOnDisplay( bool value )
                {
                    if( CLBeaconRegion != null )
                    {
                        CLBeaconRegion.NotifyEntryStateOnDisplay = value;
                    }
                }

                #endregion //End Accessor Implementations

                // Platform Implementation
                public CLBeaconRegion CLBeaconRegion { get; set; }

                public iOSBeaconRegion( string uuid, string regionTag )
                {
                    // this constructor is called by the user, so validate uuid
                    if( string.IsNullOrEmpty( uuid ) || string.IsNullOrEmpty( regionTag ) )
                    {
                        throw new Exception( "Cannot create an iOSBeaconRegion with a blank UUID or regionTag" );
                    }

                    //TODO: Support major/minor

                    // this should be called by all constructors
                    CLBeaconRegion = null;

                    if( uuid != string.Empty )
                    {
                        //CLBeaconRegion = new CLBeaconRegion(new NSUuid(uuid), major, minor, string.Empty);
                        CLBeaconRegion = new CLBeaconRegion( new NSUuid( uuid ), regionTag );
                    }
                }

                public iOSBeaconRegion( CLBeaconRegion region )
                {
                    CLBeaconRegion = region;
                }
            }

            public class iOSRegionEventArgs : RegionEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override BeaconRegion getRegion( )
                {
                    return new iOSBeaconRegion( CLRegionEventArgs.Region as CLBeaconRegion ) as BeaconRegion;
                }

                #endregion //End Accessor Implementations

                //Platform Implementation
                public CLRegionEventArgs CLRegionEventArgs { get; set; }

                public iOSRegionEventArgs( CLRegionEventArgs e )
                {
                    CLRegionEventArgs = e;
                }
            }

            public class iOSRegionBeaconsRangedEventArgs : RegionBeaconsRangedEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override Beacon [] getBeacons( )
                {
                    // create an array of iOSBeacons and fill it with the CLBeacons of the event args
                    iOSBeacon[] beacons = new iOSBeacon[CLRegionBeaconsRangedEventArgs.Beacons.Length];
                    int index = 0;
                    foreach( CLBeacon clBeacon in CLRegionBeaconsRangedEventArgs.Beacons )
                    {
                        beacons[ index++ ] = new iOSBeacon( clBeacon );
                    }

                    return beacons as Beacon[];
                }

                protected override BeaconRegion getRegion( )
                {
                    return new iOSBeaconRegion( CLRegionBeaconsRangedEventArgs.Region ) as BeaconRegion;
                }

                #endregion //End Accessor Implementations

                // Platform Implementation
                public CLRegionBeaconsRangedEventArgs CLRegionBeaconsRangedEventArgs { get; set; }

                public iOSRegionBeaconsRangedEventArgs( CLRegionBeaconsRangedEventArgs eventArgs )
                {
                    CLRegionBeaconsRangedEventArgs = eventArgs;
                }
            }

            public class iOSRegionStateDeterminedEventArgs : RegionStateDeterminedEventArgs
            {
                #region MemberAccessors //Accessor Implementations

                protected override RegionState getRegionState( )
                {
                    return ( RegionState )CLRegionStateDeterminedEventArgs.State;
                }

                protected override BeaconRegion getRegion( )
                {
                    return new iOSBeaconRegion( CLRegionStateDeterminedEventArgs.Region as CLBeaconRegion ) as BeaconRegion;
                }

                #endregion //End Accessor Implementations

                public CLRegionStateDeterminedEventArgs CLRegionStateDeterminedEventArgs { get; set; }

                public iOSRegionStateDeterminedEventArgs( CLRegionStateDeterminedEventArgs eventArgs )
                {
                    CLRegionStateDeterminedEventArgs = eventArgs;
                }
            }

            public class iOSLocationManager : LocationManager
            {
                //Platform Implementation
                public CLLocationManager CLLocationManager { get; set; }

                public iOSLocationManager( )
                {
                    CLLocationManager = new CLLocationManager( );

                    // Create our own local delegate, and in that, call the user-provided regionEvent.
                    // Basically a wrapper for their delegate
                    CLLocationManager.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e ) =>
                    {
                        if( e.Beacons.Length > 0 )
                        {
                            iOSRegionBeaconsRangedEventArgs eventArg = new iOSRegionBeaconsRangedEventArgs( e );

                            // call their delegate
                            OnRegionBeaconsRangedEvent( sender, eventArg as RegionBeaconsRangedEventArgs );
                        }
                    };

                    // Create our own local delegate, and in that, call the user-provided regionEvent.
                    // Basically a wrapper for their delegate
                    CLLocationManager.RegionEntered += (object sender, CLRegionEventArgs e ) =>
                    {
                        // we need to convert the iOS arguments into our platform-agnostic object.
                        iOSRegionEventArgs eventArgs = new iOSRegionEventArgs( e );

                        // call their delegate
                        OnRegionEnteredEvent( sender, eventArgs as RegionEventArgs );
                    };

                    // Create our own local delegate, and in that, call the user-provided regionEvent.
                    // Basically a wrapper for their delegate
                    CLLocationManager.RegionLeft += (object sender, CLRegionEventArgs e ) =>
                    {
                        // we need to convert the iOS arguments into our platform-agnostic object.
                        iOSRegionEventArgs eventArgs = new iOSRegionEventArgs( e );

                        // call their delegate
                        OnRegionExitedEvent( sender, eventArgs as RegionEventArgs );
                    };

                    CLLocationManager.DidDetermineState += (object sender, CLRegionStateDeterminedEventArgs e ) =>
                    {
                        // we need to convert the iOS arguments into our platform-agnostic object.
                        iOSRegionStateDeterminedEventArgs eventArgs = new iOSRegionStateDeterminedEventArgs( e );

                        // call their delegate
                        OnDidDetermineStateEvent( sender, eventArgs as RegionStateDeterminedEventArgs );
                    };
                }

                public override void StartMonitoring( BeaconRegion region )
                {
                    iOSBeaconRegion iosRegion = region as iOSBeaconRegion;
                    CLLocationManager.StartMonitoring( iosRegion.CLBeaconRegion );
                }

                public override void StopMonitoring( BeaconRegion region )
                {
                    iOSBeaconRegion iosRegion = region as iOSBeaconRegion;
                    CLLocationManager.StopMonitoring( iosRegion.CLBeaconRegion );
                }

                public override void RequestStateForRegion( BeaconRegion region )
                {
                    iOSBeaconRegion iOSRegion = region as iOSBeaconRegion;
                    CLLocationManager.RequestState( iOSRegion.CLBeaconRegion );
                }

                public override void StartRangingBeacons( BeaconRegion region )
                {
                    iOSBeaconRegion iosRegion = region as iOSBeaconRegion;

                    Rock.Mobile.Util.Debug.WriteLine( "START ranging beacons with UUID: " + iosRegion.CLBeaconRegion.ProximityUuid.AsString( ) );
                    CLLocationManager.StartRangingBeacons( iosRegion.CLBeaconRegion );
                }

                public override void StopRangingBeacons( BeaconRegion region )
                {
                    iOSBeaconRegion iosRegion = region as iOSBeaconRegion;

                    Rock.Mobile.Util.Debug.WriteLine( "STOP ranging beacons with UUID: " + iosRegion.CLBeaconRegion.ProximityUuid.AsString( ) );
                    CLLocationManager.StopRangingBeacons( iosRegion.CLBeaconRegion );
                }

                public override bool IsAvailable( )
                {
                    return true;
                }
            }
        }
    }
}
#endif
