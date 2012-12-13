//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Static object for caching kiosk configuration
    /// TODO: Need to figure out thread concurency issues with static object and lock/unlock appropriately
    /// </summary>
    public static class KioskCache
    {
        private static int _cacheSeconds = 30;
        private static DateTimeOffset _lastCached { get; set; }
        private static List<KioskStatus> _kiosks;

        /// <summary>
        /// Gets the kiosks.
        /// </summary>
        /// <value>
        /// The kiosks.
        /// </value>
        public static List<KioskStatus> Kiosks
        {
            get
            {
                if ( _lastCached.AddSeconds( _cacheSeconds ).CompareTo( DateTimeOffset.Now ) < 0 )
                {
                    RefreshCache();
                }

                return _kiosks;
            }
        }

        /// <summary>
        /// Initializes the <see cref="KioskCache" /> class.
        /// </summary>
        static KioskCache()
        {
            var globalAttributes = GlobalAttributesCache.Read();
            string value = globalAttributes.GetValue( "KioskCacheExpiration" );

            if ( !Int32.TryParse( value, out _cacheSeconds ) )
                _cacheSeconds = 30;

            RefreshCache();
        }

        /// <summary>
        /// Refreshes the cache.
        /// </summary>
	    private static void RefreshCache()
	    {
            _kiosks = new List<KioskStatus>();
		
            var checkInDeviceTypeId = DefinedValueCache.Read(SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK).Id;
		    foreach(var kiosk in new DeviceService().Queryable()
                .Where( d => d.DeviceTypeValueId == checkInDeviceTypeId)
                .ToList())
		    {
			    var kioskStatus = new KioskStatus(kiosk);
			
			    foreach(Location location in kiosk.Locations)
			    {
                    LoadKioskLocations( kioskStatus, location );
			    }
			
			    _kiosks.Add(kioskStatus);
		    }

            _lastCached = DateTimeOffset.Now;
	    }

        private static void LoadKioskLocations(KioskStatus kioskStatus, Location location)
        {
            foreach ( var groupLocation in new GroupLocationService().GetActiveByLocation( location.Id ) )
            {
                DateTimeOffset nextGroupActiveTime = DateTimeOffset.MaxValue;

                var kioskGroup = new KioskGroup( groupLocation.Group );

                foreach ( var schedule in groupLocation.Schedules.Where( s => s.CheckInStartTime.HasValue ) )
                {
                    var nextScheduleActiveTime = schedule.GetNextCheckInStartTime( DateTimeOffset.Now );
                    if ( nextScheduleActiveTime.HasValue && nextScheduleActiveTime.Value.CompareTo( nextGroupActiveTime.DateTime ) < 0 )
                    {
                        nextGroupActiveTime = nextScheduleActiveTime.Value;
                    }

                    if ( schedule.IsCheckInActive )
                    {
                        kioskGroup.Schedules.Add( new KioskSchedule( schedule ) );
                    }
                }

                // If the group has any active or future schedules, add the group's group type to the kiosk's 
                // list of group types
                if ( kioskGroup.Schedules.Count > 0 || nextGroupActiveTime < DateTimeOffset.MaxValue )
                {
                    KioskGroupType kioskGroupType = kioskStatus.GroupTypes.Where( g => g.Id == kioskGroup.GroupTypeId ).FirstOrDefault();
                    if (kioskGroupType == null)
                    {
                        kioskGroupType = new KioskGroupType( groupLocation.Group.GroupType );
                        kioskGroupType.NextActiveTime = DateTimeOffset.MaxValue;
                        kioskStatus.GroupTypes.Add( kioskGroupType );
                    }

                    if ( nextGroupActiveTime.CompareTo( kioskGroupType.NextActiveTime ) < 0 )
                    {
                        kioskGroupType.NextActiveTime = nextGroupActiveTime;
                    }

                    // If there are active schedules, add the locations to the group type locations
                    if ( kioskGroup.Schedules.Count > 0 )
                    {
                        KioskLocation kioskLocation = kioskGroupType.Locations.Where( l => l.Id == location.Id ).FirstOrDefault();
                        if ( kioskLocation == null)
                        {
                            kioskLocation = new KioskLocation( location );
                            kioskGroupType.Locations.Add( kioskLocation );
                        }

                        kioskLocation.Groups.Add( kioskGroup );
                    }
                }
            }

            foreach(var childLocation in location.ChildLocations)
            {
                LoadKioskLocations( kioskStatus, childLocation );
            }
        }
    }
}