// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of a check-in device.  
    /// </summary>
    [DataContract]
    public class KioskDevice
    {
        private static ConcurrentDictionary<int, object> _locks = new ConcurrentDictionary<int,object>();

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
            object obj = new object();
            obj =_locks.GetOrAdd( id, obj );

            lock ( obj )
            {
                string cacheKey = KioskDevice.CacheKey( id );

                ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
                KioskDevice device = cache[cacheKey] as KioskDevice;

                // If the kioskdevice is currently inactive, but has a next active time prior to now, force a refresh
                if ( device != null && device.FilteredGroupTypes(configuredGroupTypes).Count > 0 && !device.HasLocations( configuredGroupTypes ) )
                {
                    if ( device.KioskGroupTypes.Select( g => g.NextActiveTime ).Min().CompareTo( RockDateTime.Now ) < 0 )
                    {
                        device = null;
                    }
                }

                if ( device != null )
                {
                    return device;
                }
                else
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var campusLocations = new Dictionary<int, int>();
                        Rock.Web.Cache.CampusCache.All()
                            .Where( c => c.LocationId.HasValue )
                            .Select( c => new
                            {
                                CampusId = c.Id,
                                LocationId = c.LocationId.Value
                            } )
                            .ToList()
                            .ForEach( c => campusLocations.Add( c.CampusId, c.LocationId ) );

                        var deviceModel = new DeviceService( rockContext )
                            .Queryable( "Locations" ).AsNoTracking()
                            .Where( d => d.Id == id )
                            .FirstOrDefault();

                        if ( deviceModel != null )
                        {
                            device = new KioskDevice( deviceModel );
                            foreach ( Location location in deviceModel.Locations )
                            {
                                LoadKioskLocations( device, location, campusLocations, rockContext );
                            }

                            var cachePolicy = new CacheItemPolicy();
                            cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 60 );
                            cache.Set( cacheKey, device, cachePolicy );

                            return device;
                        }
                    }
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
            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            cache.Remove( KioskDevice.CacheKey( id ) );
        }

        /// <summary>
        /// Loads the kiosk locations.
        /// </summary>
        /// <param name="kioskDevice">The kiosk device.</param>
        /// <param name="location">The location.</param>
        /// <param name="campusLocations">The campus locations.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void LoadKioskLocations( KioskDevice kioskDevice, Location location, Dictionary<int, int> campusLocations, RockContext rockContext )
        {
            // First check to see if this is a campus location
            int campusId = campusLocations
                .Where( c => c.Value == location.Id )
                .Select( c => c.Key )
                .FirstOrDefault();

            // If location is not a campus, check the location's parent locations to see if any of them are a campus
            if ( campusId == 0 )
            {
                foreach ( var parentLocationId in new LocationService( rockContext )
                    .GetAllAncestors( location.Id )
                    .Select( l => l.Id ) )
                {
                    campusId = campusLocations
                        .Where( c => c.Value == location.Id )
                        .Select( c => c.Key )
                        .FirstOrDefault();
                    if ( campusId != 0 )
                    {
                        break;
                    }
                }
            }

            LoadKioskLocations( kioskDevice, location, ( campusId > 0 ? campusId : (int?)null ), rockContext );
        }

        /// <summary>
        /// Loads the kiosk locations.
        /// </summary>
        /// <param name="kioskDevice">The kiosk device.</param>
        /// <param name="location">The location.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void LoadKioskLocations( KioskDevice kioskDevice, Location location, int? campusId, RockContext rockContext )
        {
            // Get all the child locations also
            var allLocations = new List<int> { location.Id };
            new LocationService( rockContext )
                .GetAllDescendents( location.Id )
                .Select( l => l.Id )
                .ToList()
                .ForEach( l => allLocations.Add( l ) );

            foreach ( var groupLocation in new GroupLocationService( rockContext ).GetActiveByLocations( allLocations ) )
            {
                DateTime nextGroupActiveTime = DateTime.MaxValue;

                var kioskLocation = new KioskLocation( groupLocation.Location );
                kioskLocation.CampusId = campusId;

                // Populate each kioskLocation with it's schedules (kioskSchedules)
                foreach ( var schedule in groupLocation.Schedules.Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                {
                    var nextScheduleActiveTime = schedule.GetNextCheckInStartTime( RockDateTime.Now );
                    if ( nextScheduleActiveTime.HasValue &&
                        ( schedule.IsCheckInActive || nextScheduleActiveTime.Value.CompareTo( RockDateTime.Now ) >= 0 ) &&
                        nextScheduleActiveTime.Value.CompareTo( nextGroupActiveTime ) < 0 )
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
                if ( kioskLocation.KioskSchedules.Count > 0 || nextGroupActiveTime < DateTime.MaxValue )
                {
                    KioskGroupType kioskGroupType = kioskDevice.KioskGroupTypes.Where( g => g.GroupType.Id == groupLocation.Group.GroupTypeId ).FirstOrDefault();
                    if ( kioskGroupType == null )
                    {
                        kioskGroupType = new KioskGroupType( groupLocation.Group.GroupTypeId );
                        kioskGroupType.NextActiveTime = DateTime.MaxValue;
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
                            kioskGroup.Group.LoadAttributes( rockContext );
                            kioskGroupType.KioskGroups.Add( kioskGroup );
                        }

                        //kioskLocation.Location.LoadAttributes( rockContext ); // Locations don't have UI for attributes
                        kioskGroup.KioskLocations.Add( kioskLocation );
                    }
                }
            }
        }

        #endregion

    }
}

