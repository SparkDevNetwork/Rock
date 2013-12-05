//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of a check-in device.  
    /// </summary>
    [DataContract]
    public class KioskDevice
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="KioskDevice" /> class from being created.
        /// </summary>
        private KioskDevice()
        {
            KioskGroupTypes = new List<KioskGroupType>();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="KioskDevice" /> class from being created.
        /// </summary>
        /// <param name="device">The device.</param>
        private KioskDevice( Device device )
            : base()
        {
            Device = device.Clone( false );
            KioskGroupTypes = new List<KioskGroupType>();
        }

        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        [DataMember]
        public Device Device { get; set; }

        /// <summary>
        /// The group types associated with this kiosk
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        [DataMember]
        public List<KioskGroupType> KioskGroupTypes { get; set; }

        /// <summary>
        /// Subset of the KioskGroupTypes
        /// </summary>
        /// <param name="configuredGroupTypes">The current group types.</param>
        /// <returns></returns>
        public List<KioskGroupType> FilteredGroupTypes( List<int> configuredGroupTypes )
        {
            if ( configuredGroupTypes != null )
            {
                return KioskGroupTypes.Where( g => configuredGroupTypes.Contains( g.GroupType.Id ) ).ToList();
            }
            else
            {
                return KioskGroupTypes;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocations(List<int> configuredGroupTypes)
        {
            return FilteredGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any() );
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveLocations(List<int> configuredGroupTypes)
        {
            return FilteredGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.Location.IsActive ) );
        }

        #region Static Methods

        /// <summary>
        /// Caches the key.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:CheckIn:KioskDevice:{0}", id );
        }

        /// <summary>
        /// Reads the device by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        /// <returns></returns>
        public static KioskDevice Read( int id, List<int> configuredGroupTypes )
        {
            string cacheKey = KioskDevice.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            KioskDevice device = cache[cacheKey] as KioskDevice;

            // If the kioskdevice is currently inactive, but has a next active time prior to now, force a refresh
            if ( device != null && device.FilteredGroupTypes(configuredGroupTypes).Count > 0 && !device.HasLocations( configuredGroupTypes ) )
            {
                if ( device.KioskGroupTypes.Select( g => g.NextActiveTime ).Min().CompareTo( DateTimeOffset.Now ) < 0 )
                {
                    device = null;
                }
            }

            if (device != null)
            {
                return device;
            }
            else
            {
                var deviceModel = new DeviceService().Get(id);

                if ( deviceModel != null )
                {
                    device = new KioskDevice( deviceModel );

                    using ( new Rock.Data.UnitOfWorkScope() )
                    {
                        foreach ( Location location in deviceModel.Locations )
                        {
                            LoadKioskLocations( device, location );
                        }
                    }

                    var cachePolicy = new CacheItemPolicy();
                    cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 60 );
                    cache.Set( cacheKey, device, cachePolicy );

                    return device;
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( KioskDevice.CacheKey( id ) );
        }

        /// <summary>
        /// Loads the kiosk locations.
        /// </summary>
        /// <param name="kioskDevice">The kiosk device.</param>
        /// <param name="location">The location.</param>
        private static void LoadKioskLocations( KioskDevice kioskDevice, Location location )
        {
            var groupLocationService = new GroupLocationService();
            foreach ( var groupLocation in groupLocationService.GetActiveByLocation( location.Id ) )
            {
                DateTimeOffset nextGroupActiveTime = DateTimeOffset.MaxValue;

                var kioskLocation = new KioskLocation( groupLocation.Location );

                // Populate each kioskLocation with it's schedules (kioskSchedules)
                foreach ( var schedule in groupLocation.Schedules.Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                {
                    var nextScheduleActiveTime = schedule.GetNextCheckInStartTime( DateTimeOffset.Now );
                    if ( nextScheduleActiveTime.HasValue && nextScheduleActiveTime.Value.CompareTo( nextGroupActiveTime.DateTime ) < 0 )
                    {
                        nextGroupActiveTime = nextScheduleActiveTime.Value;
                    }

                    if ( schedule.IsCheckInActive && kioskLocation != null )
                    {
                        kioskLocation.KioskSchedules.Add( new KioskSchedule( schedule ) );
                    }
                }

                // If the group location has any active OR future schedules, add the group's group type to the kiosk's 
                // list of group types
                if ( kioskLocation.KioskSchedules.Count > 0 || nextGroupActiveTime < DateTimeOffset.MaxValue )
                {

                    KioskGroupType kioskGroupType = kioskDevice.KioskGroupTypes.Where( g => g.GroupType.Id == groupLocation.Group.GroupTypeId ).FirstOrDefault();
                    if ( kioskGroupType == null )
                    {
                        kioskGroupType = new KioskGroupType( groupLocation.Group.GroupType );
                        kioskGroupType.GroupType.LoadAttributes();
                        kioskGroupType.NextActiveTime = DateTimeOffset.MaxValue;
                        kioskDevice.KioskGroupTypes.Add( kioskGroupType );
                    }

                    if ( nextGroupActiveTime.CompareTo( kioskGroupType.NextActiveTime ) < 0 )
                    {
                        kioskGroupType.NextActiveTime = nextGroupActiveTime;
                    }

                    // If there are active schedules, add the group to the group type groups
                    if ( kioskLocation.KioskSchedules.Count > 0 )
                    {
                        KioskGroup kioskGroup = kioskGroupType.KioskGroups.Where( g => g.Group.Id == groupLocation.GroupId ).FirstOrDefault();
                        if ( kioskGroup == null )
                        {
                            kioskGroup = new KioskGroup( groupLocation.Group );
                            kioskGroup.Group.LoadAttributes();
                            kioskGroupType.KioskGroups.Add( kioskGroup );
                        }

                        kioskLocation.Location.LoadAttributes();
                        kioskGroup.KioskLocations.Add( kioskLocation );
                    }
                }
            }

            foreach ( var childLocation in location.ChildLocations )
            {
                LoadKioskLocations( kioskDevice, childLocation );
            }
        }

        #endregion

    }
}

