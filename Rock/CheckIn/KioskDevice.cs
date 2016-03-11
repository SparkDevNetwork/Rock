﻿// <copyright>
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
                return KioskGroupTypes
                    .Where( g => configuredGroupTypes.Contains( g.GroupType.Id ) )
                    .ToList();
            }
            else
            {
                return KioskGroupTypes;
            }
        }

        /// <summary>
        /// Subset of the KioskGroupTypes that are configured and currently active or check in.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        /// <returns></returns>
        public List<KioskGroupType> ActiveGroupTypes( List<int> configuredGroupTypes )
        {
            return FilteredGroupTypes( configuredGroupTypes ).Where( t => t.IsCheckInActive ).ToList();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocations(List<int> configuredGroupTypes)
        {
            return ActiveGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckInActive ) );
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveLocations(List<int> configuredGroupTypes)
        {
            return ActiveGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckInActive && l.Location.IsActive ) );
        }

        /// <summary>
        /// returns the locations for this Kiosk for the configured group types
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEnumerable<Location> Locations( List<int> configuredGroupTypes, RockContext rockContext )
        {
            var result = new List<Rock.Model.Location>();

            Rock.Model.Device currentDevice = new DeviceService( rockContext ).Get(this.Device.Id);

            // first, get all the possible locations for this device including child locations
            var allLocations = new List<int>();
            foreach ( Rock.Model.Location location in currentDevice.Locations )
            {
                // add the location to the locations for this device
                allLocations.Add( location.Id );

                // Get all the child locations also
                new LocationService( rockContext )
                    .GetAllDescendents( location.Id )
                    .Select( l => l.Id )
                    .ToList()
                    .ForEach( l => allLocations.Add( l ) );
            }

            // now, narrow it down to only locations that are active group locations for the configured group types
            foreach ( var groupLocation in new GroupLocationService( rockContext ).GetActiveByLocations( allLocations ) )
            {
                if ( configuredGroupTypes.Contains( groupLocation.Group.GroupTypeId ) )
                {
                    if ( !result.Any( a => a.Id == groupLocation.LocationId ) )
                    {
                        result.Add( groupLocation.Location );
                    }
                }
            }

            return result;
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
                            .Queryable().AsNoTracking()
                            .Where( d => d.Id == id )
                            .FirstOrDefault();

                        if ( deviceModel != null )
                        {
                            device = new KioskDevice( deviceModel );
                            foreach ( Location location in deviceModel.Locations )
                            {
                                LoadKioskLocations( device, location, campusLocations, rockContext );
                            }

                            cache.Set( cacheKey, device, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.Date.AddDays( 1 ) } );

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
        /// Flushes all.
        /// </summary>
        public static void FlushAll()
        {
            ObjectCache cache = Rock.Web.Cache.RockMemoryCache.Default;
            var keysToRemove = cache
                .Where( c =>
                    c.Key.StartsWith( "Rock:CheckIn:KioskDevice:" ) )
                .Select( c => c.Key )
                .ToList();

            foreach ( var key in keysToRemove )
            {
                cache.Remove( key );
            }
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
                    .GetAllAncestorIds( location.Id ) )
                {
                    campusId = campusLocations
                        .Where( c => c.Value == parentLocationId )
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
            // Get the child locations and the selected location
            var allLocations = new LocationService( rockContext ).GetAllDescendentIds( location.Id ).ToList();
            allLocations.Add( location.Id );

            var activeSchedules = new Dictionary<int, KioskSchedule>();

            foreach ( var groupLocation in new GroupLocationService( rockContext ).GetActiveByLocations( allLocations ).AsNoTracking() )
            {
                var kioskLocation = new KioskLocation( groupLocation.Location );
                kioskLocation.CampusId = campusId;

                // Populate each kioskLocation with its schedules (kioskSchedules)
                foreach ( var schedule in groupLocation.Schedules.Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                {
                    if ( activeSchedules.Keys.Contains( schedule.Id ) )
                    {
                        kioskLocation.KioskSchedules.Add( activeSchedules[schedule.Id] );
                    }
                    else
                    {
                        var kioskSchedule = new KioskSchedule( schedule );
                        kioskSchedule.CheckInTimes = schedule.GetCheckInTimes( RockDateTime.Now );
                        if ( kioskSchedule.IsCheckInActive || kioskSchedule.NextActiveDateTime.HasValue )
                        {
                            kioskLocation.KioskSchedules.Add( kioskSchedule );
                            activeSchedules.Add( schedule.Id, kioskSchedule );
                        }
                    }
                }

                // If the group location has any active OR future schedules, add the group's group type to the kiosk's 
                // list of group types
                if ( kioskLocation.KioskSchedules.Count > 0 )
                {
                    KioskGroupType kioskGroupType = kioskDevice.KioskGroupTypes.Where( g => g.GroupType.Id == groupLocation.Group.GroupTypeId ).FirstOrDefault();
                    if ( kioskGroupType == null )
                    {
                        kioskGroupType = new KioskGroupType( groupLocation.Group.GroupTypeId );
                        kioskDevice.KioskGroupTypes.Add( kioskGroupType );
                    }

                    KioskGroup kioskGroup = kioskGroupType.KioskGroups.Where( g => g.Group.Id == groupLocation.GroupId ).FirstOrDefault();
                    if ( kioskGroup == null )
                    {
                        kioskGroup = new KioskGroup( groupLocation.Group );
                        kioskGroup.Group.LoadAttributes( rockContext );
                        kioskGroupType.KioskGroups.Add( kioskGroup );
                    }

                    kioskGroup.KioskLocations.Add( kioskLocation );
                }
            }
        }

        #endregion

    }
}

