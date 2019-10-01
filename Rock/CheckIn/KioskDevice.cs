// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of a check-in device.  
    /// </summary>
    [DataContract]
    public class KioskDevice : ItemCache<KioskDevice>
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
            Device.LoadAttributes();
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
        /// Gets the campus identifier based on the Device's Location(s)
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Registration Mode is enabled for the device
        /// </summary>
        /// <value>
        ///   <c>true</c> if [registration mode enabled]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RegistrationModeEnabled => Device.GetAttributeValue( "core_device_RegistrationMode" ).AsBoolean();

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
            return ActiveGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckInActive && l.IsActiveAndNotFull ) );
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
            foreach ( var groupLocation in new GroupLocationService( rockContext ).GetActiveByLocations( allLocations ).OrderBy( l => l.Order ) )
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
        /// Reads the device by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get( int id, List<int> configuredGroupTypes ) instead." )]
        public static KioskDevice Read( int id, List<int> configuredGroupTypes )
        {
            return Get( id, configuredGroupTypes );
        }

        /// <summary>
        /// Reads the device by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        /// <returns></returns>
        public static KioskDevice Get( int id, List<int> configuredGroupTypes )
        {
            var now = RockDateTime.Now;
            var timespan = now.Date.AddDays( 1 ).Subtract( now );
            return GetOrAddExisting( id, () => Create( id ), timespan );
        }

        private static KioskDevice Create( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                var campusLocations = new Dictionary<int, int>();
                CampusCache.All()
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
                    var device = new KioskDevice( deviceModel );
                    foreach ( Location location in deviceModel.Locations )
                    {
                        LoadKioskLocations( device, location, campusLocations, rockContext );
                    }

                    return device;
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove( int id ) instead.")]
        public static void Flush( int id )
        {
            Remove( id );
        }

        /// <summary>
        /// Flushes all.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Clear() instead." )]
        public static void FlushAll()
        {
            Clear();
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

            DateTime currentDateTime = RockDateTime.Now;
            if ( campusId.HasValue )
            {
                currentDateTime = CampusCache.Get( campusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
            }

            kioskDevice.CampusId = campusId;

            var activeSchedules = new Dictionary<int, KioskSchedule>();

            foreach ( var groupLocation in new GroupLocationService( rockContext ).GetActiveByLocations( allLocations ).OrderBy( l => l.Order ).AsNoTracking() )
            {
                var kioskLocation = new KioskLocation( groupLocation.Location );
                kioskLocation.CampusId = campusId;
                kioskLocation.Order = groupLocation.Order;

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
                        kioskSchedule.CampusId = kioskLocation.CampusId;
                        kioskSchedule.CheckInTimes = schedule.GetCheckInTimes( currentDateTime );
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

