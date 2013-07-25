//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Static object for caching kiosk configuration
    /// </summary>
    public static class KioskCache
    {
        // Locking object
        private static readonly Object obj = new object();

        private static int _cacheSeconds = 60;
        private static DateTimeOffset _lastCached { get; set; }
        private static Dictionary<int, KioskStatus> _kiosks;
        private static Dictionary<int, KioskLocationAttendance> _locations;
        private static Dictionary<int, LabelCache> _labels;

        /// <summary>
        /// Initializes the <see cref="KioskCache" /> class.
        /// </summary>
        static KioskCache()
        {
            lock ( obj )
            {
                var globalAttributes = GlobalAttributesCache.Read();
                string value = globalAttributes.GetValue( "KioskCacheExpiration" );

                if ( !Int32.TryParse( value, out _cacheSeconds ) )
                    _cacheSeconds = 60;

                RefreshCache();
            }
        }

        /// <summary>
        /// Gets the kiosk status.
        /// </summary>
        /// <param name="kioskId">The kiosk id.</param>
        /// <returns></returns>
        public static KioskStatus GetKiosk( int kioskId )
        {
            lock ( obj )
            {
                if ( _lastCached.AddSeconds( _cacheSeconds ).CompareTo( DateTimeOffset.Now ) < 0 )
                {
                    RefreshCache();
                }
                else
                {
                    // Get the inactive kiosks
                    var inactiveKiosk = _kiosks.Where( k => k.Value.KioskGroupTypes.Count > 0 && !k.Value.HasLocations ).Select( k => k.Value );

                    // If any of the currently inactive kiosks have a next active time prior to the current time, force a refresh the cache.
                    if (inactiveKiosk.Any( k => k.KioskGroupTypes.Select( g => g.NextActiveTime).Min().CompareTo(DateTimeOffset.Now) < 0))
                    {
                        RefreshCache();
                    }
                }

                if ( _kiosks.ContainsKey( kioskId ) )
                {
                    // Clone the object so that a reference to the static object is not maintaned (or updated)
                    return JsonConvert.DeserializeObject( _kiosks[kioskId].ToJson(), typeof( KioskStatus ) ) as KioskStatus;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a  matching device kiosk by it's latitude and longitude.
        /// </summary>
        /// <param name="latitude">the latitude of the device</param>
        /// <param name="longitude">the longitude of the device</param>
        /// <returns></returns>
        public static KioskStatus GetKiosk( double latitude, double longitude )
        {
            DbGeography aLocation = DbGeography.FromText( string.Format( "POINT({0} {1})", longitude, latitude ) );

            var kioskStatus = _kiosks
                .Where( d => 
                    d.Value.Device.Location != null &&
                    aLocation.Intersects( d.Value.Device.Location.GeoFence ) )
                .Select( k => k.Value ).FirstOrDefault();

            if ( kioskStatus != null )
            {
                // Now call the other GetKiosk method which will lock and refresh the cache as needed.
                return GetKiosk( kioskStatus.Device.Id );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the kiosk based on it's name (if the given IP can be reverse looked up into a fully qualified name)
        /// or by the given IP.
        /// </summary>
        /// <param name="ipAddress">The IP address (from Request server variable "REMOTE_ADDR")</param>
        /// <param name="skipReverseLookup">Flag to indicate whether or not the ipAddress should attempt to be looked up (converted to a FQHN).</param>
        /// <returns></returns>
        public static KioskStatus GetKiosk( string ipAddress, bool skipReverseLookup = true )
        {
            string hostValue = ipAddress;
            KioskStatus kioskStatus;

            if ( !skipReverseLookup )
            {
                // Lookup the system's "name" (as seen in the DNS) using the given IP
                // address because when using DHCP the kiosk may have a different IP from time to time
                // -- however the fully qualified name should always be the same.
                try
                {
                    hostValue = System.Net.Dns.GetHostEntry( ipAddress ).HostName;
                }
                catch ( SocketException )
                {
                    // TODO: consider whether we want to log the IP address that caused this error.
                    // As per http://msdn.microsoft.com/en-us/library/ms143998.aspx it *may* mean 
                    // a stale DNS record for an IPv4 address that actually belongs to a
                    // different host was going to be returned (there is a DNS PTR record for
                    // the IPv4 address, but no DNS A record for the IPv4 address).
                    hostValue = ipAddress;
                }
            }

            // If we still have an IPv4 address then try to find it based on IP
            if ( Regex.IsMatch( hostValue, @"\d+\.\d+\.\d+\.\d+" ) )
            {
                // find by IP
                kioskStatus = _kiosks.Where( k => k.Value.Device.IPAddress == hostValue ).Select( k => k.Value ).FirstOrDefault();
            }
            else
            {
                // find by name
                kioskStatus = _kiosks.Where( k => k.Value.Device.Name == hostValue ).Select( k => k.Value ).FirstOrDefault();
            }

            if ( kioskStatus != null )
            {
                // Now call the other GetKiosk method which will lock and refresh the cache as needed.
                return GetKiosk( kioskStatus.Device.Id );
            }
            else
            {
                return null;
            }
        }
 
        /// <summary>
        /// Gets the location attendance.
        /// </summary>
        /// <param name="locationId">The location id.</param>
        /// <returns></returns>
        public static KioskLocationAttendance GetLocationAttendance (int locationId)
        {
            lock ( obj )
            {
                if ( _locations.ContainsKey( locationId ) )
                {
                    // Clone the object so that a reference to the static object is not maintaned (or updated)
                    return JsonConvert.DeserializeObject( _locations[locationId].ToJson(), typeof( KioskLocationAttendance ) ) as KioskLocationAttendance;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static LabelCache GetLabel(int id)
        {
            lock ( obj )
            {
                if ( _labels.ContainsKey( id ) )
                {
                    return JsonConvert.DeserializeObject( _labels[id].ToJson(), typeof( LabelCache ) ) as LabelCache;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds an attendance item to location attendance cache so that it is accurate between refreshes
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        public static void AddAttendance( Attendance attendance )
        {
            lock ( obj )
            {
                if (_locations != null && attendance.LocationId.HasValue)
                {
                    if ( !_locations.ContainsKey( attendance.LocationId.Value ) )
                    {
                        var locationAttendance = new KioskLocationAttendance();
                        locationAttendance.LocationId = attendance.Location.Id;
                        locationAttendance.LocationName = attendance.Location.Name;
                        locationAttendance.Groups = new List<KioskGroupAttendance>();
                    }
                    AddAttendanceRecord( _locations[attendance.LocationId.Value], attendance );
                }
            }
        }

        # region Private Methods
        /// <summary>
        /// Refreshes the cache.
        /// </summary>
        private static void RefreshCache()
        {
            _kiosks = new Dictionary<int, KioskStatus>();
            _locations = new Dictionary<int, KioskLocationAttendance>();
            _labels = new Dictionary<int, LabelCache>();

            var checkInDeviceTypeId = DefinedValueCache.Read( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            foreach ( var kiosk in new DeviceService().Queryable()
                .Where( d => d.DeviceTypeValueId == checkInDeviceTypeId )
                .ToList() )
            {
                var kioskStatus = new KioskStatus( kiosk );

                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    foreach ( Location location in kiosk.Locations )
                    {
                        LoadKioskLocations( kioskStatus, location );
                    }
                }
                _kiosks.Add( kiosk.Id, kioskStatus );
            }

            foreach ( var file in new BinaryFileService()
                .Queryable()
                .Where( f => f.BinaryFileType.Guid == new Guid(SystemGuid.BinaryFiletype.CHECKIN_LABEL )))
            {
                var label = new LabelCache();

                label.Guid = file.Guid;
                label.Url = string.Format( "{0}File.ashx?{1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), file.Id );
                label.MergeFields = new Dictionary<string, string>();
                label.FileContent = System.Text.Encoding.Default.GetString( file.Data.Content );

                file.LoadAttributes();
                string attributeValue = file.GetAttributeValue( "MergeCodes" );
                if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                {
                    string[] nameValues = attributeValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                    foreach ( string nameValue in nameValues )
                    {
                        string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( nameAndValue.Length == 2 && !label.MergeFields.ContainsKey( nameAndValue[0] ) )
                        {
                            label.MergeFields.Add( nameAndValue[0], nameAndValue[1] );

                            int definedValueId = int.MinValue;
                            if ( int.TryParse( nameAndValue[1], out definedValueId ) )
                            {
                                var definedValue = DefinedValueCache.Read( definedValueId );
                                if ( definedValue != null )
                                {
                                    string mergeField = definedValue.GetAttributeValue( "MergeField" );
                                    if ( mergeField != null )
                                    {
                                        label.MergeFields[nameAndValue[0]] = mergeField;
                                    }
                                }
                            }
                        }
                    }
                }

                _labels.Add( file.Id, label );
            }

            _lastCached = DateTimeOffset.Now;
        }

        private static void LoadKioskLocations( KioskStatus kioskStatus, Location location )
        {
            var groupLocationService = new GroupLocationService();
            foreach ( var groupLocation in groupLocationService.GetActiveByLocation( location.Id ) )
            {
                DateTimeOffset nextGroupActiveTime = DateTimeOffset.MaxValue;

                var kioskGroup = new KioskGroup( groupLocation.Group );

                foreach ( var schedule in groupLocation.Schedules.Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                {
                    var nextScheduleActiveTime = schedule.GetNextCheckInStartTime( DateTimeOffset.Now );
                    if ( nextScheduleActiveTime.HasValue && nextScheduleActiveTime.Value.CompareTo( nextGroupActiveTime.DateTime ) < 0 )
                    {
                        nextGroupActiveTime = nextScheduleActiveTime.Value;
                    }

                    if ( schedule.IsCheckInActive )
                    {
                        kioskGroup.KioskSchedules.Add( new KioskSchedule( schedule ) );
                    }
                }

                // If the group has any active or future schedules, add the group's group type to the kiosk's 
                // list of group types
                if ( kioskGroup.KioskSchedules.Count > 0 || nextGroupActiveTime < DateTimeOffset.MaxValue )
                {
                    kioskGroup.Group.LoadAttributes();

                    KioskGroupType kioskGroupType = kioskStatus.KioskGroupTypes.Where( g => g.GroupType.Id == kioskGroup.Group.GroupTypeId ).FirstOrDefault();
                    if ( kioskGroupType == null )
                    {
                        kioskGroupType = new KioskGroupType( groupLocation.Group.GroupType );
                        kioskGroupType.GroupType.LoadAttributes();
                        kioskGroupType.NextActiveTime = DateTimeOffset.MaxValue;
                        kioskStatus.KioskGroupTypes.Add( kioskGroupType );
                    }

                    if ( nextGroupActiveTime.CompareTo( kioskGroupType.NextActiveTime ) < 0 )
                    {
                        kioskGroupType.NextActiveTime = nextGroupActiveTime;
                    }

                    // If there are active schedules, add the locations to the group type locations
                    if ( kioskGroup.KioskSchedules.Count > 0 )
                    {
                        KioskLocation kioskLocation = kioskGroupType.KioskLocations.Where( l => l.Location.Id == location.Id ).FirstOrDefault();
                        if ( kioskLocation == null )
                        {
                            kioskLocation = new KioskLocation( location );
                            kioskLocation.Location.LoadAttributes();
                            kioskGroupType.KioskLocations.Add( kioskLocation );
                        }

                        kioskLocation.KioskGroups.Add( kioskGroup );
                    }
                }
            }

            if ( !_locations.ContainsKey( location.Id ) )
            {
                var locationAttendance = new KioskLocationAttendance();
                locationAttendance.LocationId = location.Id;
                locationAttendance.LocationName = location.Name;
                locationAttendance.Groups = new List<KioskGroupAttendance>();

                var attendanceService = new AttendanceService();
                foreach ( var attendance in attendanceService.GetByDateAndLocation( DateTime.Today, location.Id ) )
                {
                    AddAttendanceRecord( locationAttendance, attendance );
                }

                _locations.Add(location.Id, locationAttendance );
            }

            foreach ( var childLocation in location.ChildLocations )
            {
                LoadKioskLocations( kioskStatus, childLocation );
            }
        }

        private static void AddAttendanceRecord( KioskLocationAttendance kioskLocationAttendance, Attendance attendance )
        {
            if ( attendance.GroupId.HasValue && attendance.ScheduleId.HasValue && attendance.PersonId.HasValue )
            {
                var groupAttendance = kioskLocationAttendance.Groups.Where( g => g.GroupId == attendance.GroupId ).FirstOrDefault();
                if ( groupAttendance == null )
                {
                    groupAttendance = new KioskGroupAttendance();
                    groupAttendance.GroupId = attendance.GroupId.Value;
                    groupAttendance.GroupName = attendance.Group.Name;
                    groupAttendance.Schedules = new List<KioskScheduleAttendance>();
                    kioskLocationAttendance.Groups.Add( groupAttendance );
                }

                var scheduleAttendance = groupAttendance.Schedules.Where( s => s.ScheduleId == attendance.ScheduleId ).FirstOrDefault();
                if ( scheduleAttendance == null )
                {
                    scheduleAttendance = new KioskScheduleAttendance();
                    scheduleAttendance.ScheduleId = attendance.ScheduleId.Value;
                    scheduleAttendance.ScheduleName = attendance.Schedule.Name;
                    scheduleAttendance.PersonIds = new List<int>();
                    groupAttendance.Schedules.Add( scheduleAttendance );
                }

                scheduleAttendance.PersonIds.Add( attendance.PersonId.Value );
            }
        }
        # endregion
    }
}