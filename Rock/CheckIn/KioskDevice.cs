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
using DotLiquid;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

namespace Rock.CheckIn
{
    /// <summary>
    /// The status of a check-in device.  
    /// </summary>
    [DataContract]
    public class KioskDevice : ItemCache<KioskDevice>, ILavaDataDictionary, Lava.ILiquidizable
    {
        #region Constructors

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

        #endregion Constructors

        #region Lifespan

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. This is set to the time from now
        /// until midnight for each cache item instance.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                // Expire cache items at midnight
                var now = RockDateTime.Now;
                var timespan = now.Date.AddDays( 1 ).Subtract( now );
                return timespan;
            }
        }

        #endregion Lifespan

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
        /// The group types (Checkin Areas) associated with this kiosk
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        [DataMember]
        public List<KioskGroupType> KioskGroupTypes { get; set; }

        /// <summary>
        /// Subset of the KioskGroupTypes (Checkin Areas)
        /// </summary>
        /// <param name="configuredGroupTypes">The current group types (Checkin Areas).</param>
        /// <returns></returns>
        public List<KioskGroupType> FilteredGroupTypes( List<int> configuredGroupTypes )
        {
            if ( configuredGroupTypes != null )
            {
                return KioskGroupTypes
                    .Where( g => configuredGroupTypes.Contains( g.GroupTypeId ) )
                    .ToList();
            }
            else
            {
                return KioskGroupTypes;
            }
        }

        /// <summary>
        /// Subset of the KioskGroupTypes (Checkin Areas) that are configured and currently active for check in.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns></returns>
        public List<KioskGroupType> ActiveGroupTypes( List<int> configuredGroupTypes )
        {
            return FilteredGroupTypes( configuredGroupTypes ).Where( t => t.IsCheckInActive ).ToList();
        }

        /// <summary>
        /// Subset of the KioskGroupTypes (Checkin Areas) that are configured and currently active for check out.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns></returns>
        public List<KioskGroupType> ActiveForCheckOutGroupTypes( List<int> configuredGroupTypes )
        {
            return FilteredGroupTypes( configuredGroupTypes ).Where( t => t.IsCheckOutActive ).ToList();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns>
        ///   <c>true</c> if the specified configured group types has locations; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///   <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocations( List<int> configuredGroupTypes )
        {
            return ActiveGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckInActive ) );
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns>
        ///   <c>true</c> if [has active locations] [the specified configured group types]; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///   <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveLocations( List<int> configuredGroupTypes )
        {
            return ActiveGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckInActive && l.IsActiveAndNotFull ) );
        }

        /// <summary>
        /// Gets a value indicating whether this instance has active locations for check-out.
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns>
        ///   <c>true</c> if [has active check out locations] [the specified configured group types]; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///   <c>true</c> if this instance has active locations; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveCheckOutLocations( List<int> configuredGroupTypes )
        {
            return ActiveForCheckOutGroupTypes( configuredGroupTypes ).SelectMany( t => t.KioskGroups ).Any( g => g.KioskLocations.Any( l => l.IsCheckOutActive ) );
        }

        /// <summary>
        /// returns the locations for this Kiosk for the configured group types
        /// </summary>
        /// <param name="configuredGroupTypes">The configured group types. (Checkin Areas)</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEnumerable<Location> Locations( List<int> configuredGroupTypes, RockContext rockContext )
        {
            var result = new List<Rock.Model.Location>();

            Rock.Model.Device currentDevice = new DeviceService( rockContext ).Get( this.Device.Id );

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
        /// <param name="configuredGroupTypes">The configured group types (Checkin Areas).</param>
        /// <returns></returns>
        public static KioskDevice Get( int id, List<int> configuredGroupTypes )
        {
            return GetOrAddExisting( id, () => Create( id ), () => GetAllIds() );
        }

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
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
        /// Gets a list of IDs for all devices of type CheckinKiosk
        /// </summary>
        /// <returns></returns>
        private static List<string> GetAllIds()
        {
            int? kioskDeviceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );

            using ( var rockContext = new RockContext() )
            {
                var deviceService = new DeviceService( rockContext );
                var ids = deviceService.Queryable().Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId ).Select( d => d.Id ).ToList().ConvertAll( d => d.ToString() );

                if ( ids.Any() )
                {
                    return ids;
                }
            }

            return null;
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

            LoadKioskLocations( kioskDevice, location, ( campusId > 0 ? campusId : ( int? ) null ), rockContext );
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
            /*  11-09-2023 DSH

              Performance tuning was done on this method to handle extremely
              large and complex check-in configurations. Previously a number of
              additional queries were being executed (hundreds). So now we try
              to load data in bulk in fewer queries. This shows a pretty massive
              performance boost.

              There is the possiblity the new code makes it slightly slower for
              smaller configurations, but because those were so fast already it
              should not be a noticeable difference.

              Testing was performed with 50 kiosks, 1,250 locations and 2,500 groups.
              Each kiosk had 5 random locations.
              Each group had 8 random locations.
              Each group location had 3 random schedules.
            */

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

            var groupLocationList = new GroupLocationService( rockContext )
                .GetActiveByLocations( allLocations )
                .Include( x => x.Location )
                .Include( x => x.Group )
                .OrderBy( l => l.Order ).AsNoTracking()
                .ToList();

            // Load all the attributes for the groups now in one query instead
            // of a bunch of individual queries later.
            groupLocationList
                .Select( l => l.Group )
                .DistinctBy( g => g.Id )
                .LoadAttributes( rockContext );

            // The above call only loaded Attributes for the first Group of a given ID.
            // We need to copy the attributes to all other Groups whose IDs match.
            var groupsWithAttributes = groupLocationList
                .Select( g => g.Group )
                .Where( g => g.Attributes != null && g.AttributeValues != null );

            foreach ( var groupWithAttributes in groupsWithAttributes )
            {
                var groupsMissingAttributes = groupLocationList
                    .Select( g => g.Group )
                    .Where( g => g.Id == groupWithAttributes.Id
                        && ( g.Attributes == null || g.AttributeValues == null )
                    );

                foreach ( var groupMissingAttributes in groupsMissingAttributes )
                {
                    groupMissingAttributes.Attributes = groupWithAttributes.Attributes;
                    groupMissingAttributes.AttributeValues = groupWithAttributes.AttributeValues;
                }
            }

            // Load all the schedules in one shot. This is faster than adding
            // Schedules as an Include above. Because we are already including
            // the Location and Group objects, adding in Schedules duplicates
            // all that data by however many schedules we have.
            var groupLocationIds = groupLocationList.Select( x => x.LocationId ).ToList();
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .Include( gl => gl.Schedules )
                .Where( gl => groupLocationIds.Contains( gl.LocationId ) )
                .ToList()
                .ToDictionary( gl => gl.Id, gl => gl.Schedules.ToList() );

            foreach ( var groupLocation in groupLocationList )
            {
                var kioskLocation = new KioskLocation( groupLocation.Location );
                kioskLocation.CampusId = campusId;
                kioskLocation.Order = groupLocation.Order;

                // Snag the schedules from our single query above. If we can't
                // find it then fall back to the old slow way. Shouldn't happen
                // but just makes us a bit safer.
                if ( !groupLocationSchedules.TryGetValue( groupLocation.Id, out var schedules ) )
                {
                    schedules = groupLocation.Schedules.ToList();
                }

                // Populate each kioskLocation with its schedules (kioskSchedules)
                foreach ( var schedule in schedules.Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
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
                        if ( kioskSchedule.IsCheckInActive || kioskSchedule.IsCheckOutActive || kioskSchedule.NextActiveDateTime.HasValue )
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

                        // KioskGroup will call Clone() on the group. This
                        // currently does not copy Attributes or AttributeValues
                        // properties to the new object.
                        if ( kioskGroup.Group.Attributes == null )
                        {
                            kioskGroup.Group.Attributes = groupLocation.Group.Attributes;
                            kioskGroup.Group.AttributeValues = groupLocation.Group.AttributeValues;
                        }

                        kioskGroupType.KioskGroups.Add( kioskGroup );
                    }

                    kioskGroup.KioskLocations.Add( kioskLocation );
                }
            }
        }
        #endregion

        #region Lava

        /// <summary>
        /// Gets a list of the keys defined by this data object.
        /// </summary>
        public List<string> AvailableKeys { get; } = new List<string> { "CampusId", "Device", "KioskGroupTypes" };

        /// <summary>
        /// Returns the data value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object GetValue( string key )
        {
            switch ( key )
            {
                case "CampusId":
                    return CampusId;
                case "Device":
                    return Device;
                case "KioskGroupTypes":
                    return KioskGroupTypes;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a flag indicating if this data object contains a value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool ContainsKey( string key )
        {
            return AvailableKeys.Contains( key );
        }

        #endregion

        #region ILiquidizable

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[object key]
        {
            get => GetValue( key.ToStringSafe() );
        }

        /// <summary>
        /// Converts to liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>()
            {
              { "CampusId", CampusId },
              { "Device", Device },
              { "KioskGroupTypes", KioskGroupTypes },
            };
            return dictionary;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(object key)
        {
            return AvailableKeys.Contains( key.ToStringSafe() );
        }

        #endregion
    }
}

