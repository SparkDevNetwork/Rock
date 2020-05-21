using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.bemaservices.DoorControl.DSX.Models;
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.DoorControl.DSX.Utility
{
    public class Helpers
    {
        /// <summary>
        /// Processes the unlocks for day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        public List<string> ProcessUnlocksForDay( UpdateDoorLocks_Config config )
        {
            List<string> results = new List<string>();

            RockContext rockContext = new RockContext();
            ReservationService reservationService = new ReservationService( rockContext );
            AttributeCache processDoorLock = AttributeCache.Get( config.ProcessDoorLockAttribute );
            var qry = reservationService.Queryable().AsNoTracking().Where( r => r.ApprovalState == ReservationApprovalState.Approved );

            // Getting all reservations that will happen during our window
            var reservationSummaries = reservationService.GetReservationSummaries( qry, config.DateToSync, config.DateToSync.AddDays( 1 ).AddMinutes( -1 ) ).ToList();
            var reservations = reservationService.GetByIds( reservationSummaries.Select( x => x.Id ).ToList() ).ToList();

            List<DoorLockOverride> allLocks = new List<DoorLockOverride>();
            foreach ( var reservation in reservations )
            {
                reservation.LoadAttributes();
                var isHvacOnly = !reservation.GetAttributeValue( processDoorLock.Key ).AsBoolean();

                List<DoorLockOverride> scheduledLocks = GetScheduleLocks( reservation, config, isHvacOnly );
                List<DoorLockOverride> overrideLocks = GetOverrideLocksForAReservation( reservation, config, rockContext, isHvacOnly );
                allLocks.AddRange( scheduledLocks );
                allLocks.AddRange( overrideLocks );
            }

            //List<DoorLockOverride> overrideLocks = GetOverrideLocks( config, rockContext, reservationService );
            //allLocks.AddRange( overrideLocks );

            // Process Campus Level Unlocks
            var campusUnlocks = GetCampusUnlocks( config );
            allLocks.AddRange( campusUnlocks );

            // Processing Shared Rooms
            var sharedRooms = GetSharedRooms( allLocks, config );
            allLocks.AddRange( sharedRooms );

            // Ordering by Start Time
            allLocks.OrderBy( x => x.StartTime ).ToList();

            // Pushing data into Rock DoorLock "Cache" table
            var loadRockTable = LoadDoorLockTable( allLocks, config );
            results.Add( loadRockTable );

            // Syncing Rock "Cache" table with DSX
            var syncWithDSX = SyncRockWithDSX( config );
            results.Add( syncWithDSX );

            return results;
        }

        private static List<DoorLockOverride> GetOverrideLocks( UpdateDoorLocks_Config config, RockContext rockContext, ReservationService reservationService )
        {
            AttributeMatrixService attributeMatrixService = new AttributeMatrixService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );

            List<DoorLockOverride> overrideLocks = new List<DoorLockOverride>();
            var reservationDoorOverridesTemplateGuid = Guid.Parse( com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTEMATRIX );
            var attributeMatrixList = attributeMatrixService.Queryable().AsNoTracking().Where( am => am.AttributeMatrixTemplate.Guid == reservationDoorOverridesTemplateGuid ).ToList();
            if ( attributeMatrixList.Any() )
            {
                foreach ( var attributeMatrix in attributeMatrixList )
                {
                    var matchingReservations = reservationService.Queryable().AsNoTracking().WhereAttributeValue( rockContext, "DoorOverrides", attributeMatrix.Guid.ToString() ).Where( r => r.ApprovalState == ReservationApprovalState.Approved ).ToList();

                    if ( matchingReservations.Any() )
                    {
                        foreach ( var attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                        {
                            List<ReservationDateTime> dateTimes = new List<ReservationDateTime>();
                            attributeMatrixItem.LoadAttributes();
                            var scheduleGuid = attributeMatrixItem.AttributeValues.Where( x => x.Key == "Schedule" ).FirstOrDefault().Value.Value;
                            var startDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartDateTime" ).FirstOrDefault().Value.Value;
                            var endDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndDateTime" ).FirstOrDefault().Value.Value;

                            var startDateTime = startDate.AsDateTime();
                            var endDateTime = endDate.AsDateTime();
                            if ( startDateTime != null && endDateTime != null )
                            {
                                var dateTime = new ReservationDateTime();
                                dateTime.StartDateTime = startDateTime.Value;
                                dateTime.EndDateTime = endDateTime.Value;
                                dateTimes.Add( dateTime );
                            }

                            var schedule = scheduleService.Get( scheduleGuid.AsGuid() );
                            if ( schedule != null )
                            {
                                DDay.iCal.Event calEvent = schedule.GetCalenderEvent();
                                if ( calEvent != null && calEvent.DTStart != null )
                                {
                                    var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, config.DateToSync, config.DateToSync.AddDays( 1 ).AddMinutes( -1 ) );
                                    var result = occurrences
                                        .Where( a =>
                                            a.Period != null &&
                                            a.Period.StartTime != null &&
                                            a.Period.EndTime != null )
                                        .Select( a => new ReservationDateTime
                                        {
                                            StartDateTime = DateTime.SpecifyKind( a.Period.StartTime.Value, DateTimeKind.Local ),
                                            EndDateTime = DateTime.SpecifyKind( a.Period.EndTime.Value, DateTimeKind.Local )
                                        } )
                                        .ToList();

                                    dateTimes.AddRange( result );
                                }
                            }

                            if ( dateTimes.Any() )
                            {
                                var startAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartAction" ).FirstOrDefault().Value.Value;
                                var endAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndAction" ).FirstOrDefault().Value.Value;

                                // Fetching Actions from DefinedValue Cache
                                startAction = DefinedValueCache.Get( startAction ).Value;
                                endAction = DefinedValueCache.Get( endAction ).Value;

                                // Looping through locations
                                foreach ( var matchingReservation in matchingReservations )
                                {
                                    foreach ( var reservationLocation in matchingReservation.ReservationLocations )
                                    {
                                        var location = reservationLocation.Location;
                                        location.LoadAttributes();
                                        var overrideGroup = location.GetAttributeValue( AttributeCache.Get( config.OverrideLocationAttribute ).Key ).AsIntegerOrNull();
                                        var roomName = location.GetAttributeValue( AttributeCache.Get( config.RoomNameLocationAttribute ).Key );

                                        if ( overrideGroup == null )
                                        {
                                            // Can't do anything if no Override Group is specified
                                            var newException = new Exception(
                                                string.Format( @"The Location '{0} ({1})' does not have an Override Group specified.
                                            Please configure this to allow Rock to manipulate your door locks.",
                                                                location.Name,
                                                                location.Id
                                                )
                                            );
                                            ExceptionLogService.LogException( newException );
                                        }
                                        else
                                        {
                                            foreach ( var dateTime in dateTimes )
                                            {
                                                var newDoorOverride = new DoorLockOverride
                                                {
                                                    StartTime = dateTime.StartDateTime.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                                    StartTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), startAction ),
                                                    EndTime = dateTime.EndDateTime.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                                    EndTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), endAction ),
                                                    ReservationId = matchingReservation.Id,
                                                    LocationId = location.Id,
                                                    OverrideGroup = overrideGroup.Value,
                                                    RoomName = roomName
                                                };

                                                overrideLocks.Add( newDoorOverride );
                                            }

                                        }
                                    }

                                }
                            }
                        }

                    }
                    // Looping through configured overrides
                }
            }

            return overrideLocks;
        }
        private static List<DoorLockOverride> GetOverrideLocksForAReservation( Reservation reservation, UpdateDoorLocks_Config config, RockContext rockContext, bool isHvacOnly )
        {
            AttributeMatrixService attributeMatrixService = new AttributeMatrixService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );

            List<DoorLockOverride> overrideLocks = new List<DoorLockOverride>();
            var reservationDoorOverridesTemplateGuid = Guid.Parse( com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTEMATRIX );

            reservation.LoadAttributes();
            var attributeMatrixGuid = reservation.GetAttributeValue( "DoorOverrides" ).AsGuidOrNull();
            if ( attributeMatrixGuid.HasValue )
            {
                var attributeMatrix = attributeMatrixService.Get( attributeMatrixGuid.Value );

                foreach ( var attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                {
                    List<ReservationDateTime> dateTimes = new List<ReservationDateTime>();
                    attributeMatrixItem.LoadAttributes();
                    var scheduleGuid = attributeMatrixItem.AttributeValues.Where( x => x.Key == "Schedule" ).FirstOrDefault().Value.Value;
                    var startDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartDateTime" ).FirstOrDefault().Value.Value;
                    var endDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndDateTime" ).FirstOrDefault().Value.Value;

                    var startDateTime = startDate.AsDateTime();
                    var endDateTime = endDate.AsDateTime();
                    if ( startDateTime != null && endDateTime != null )
                    {
                        var dateTime = new ReservationDateTime();
                        dateTime.StartDateTime = startDateTime.Value;
                        dateTime.EndDateTime = endDateTime.Value;
                        dateTimes.Add( dateTime );
                    }

                    var schedule = scheduleService.Get( scheduleGuid.AsGuid() );
                    if ( schedule != null )
                    {
                        DDay.iCal.Event calEvent = schedule.GetCalendarEvent();
                        if ( calEvent != null && calEvent.DTStart != null )
                        {
                            var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, config.DateToSync, config.DateToSync.AddDays( 1 ).AddMinutes( -1 ) );
                            var result = occurrences
                                .Where( a =>
                                    a.Period != null &&
                                    a.Period.StartTime != null &&
                                    a.Period.EndTime != null )
                                .Select( a => new ReservationDateTime
                                {
                                    StartDateTime = DateTime.SpecifyKind( a.Period.StartTime.Value, DateTimeKind.Local ),
                                    EndDateTime = DateTime.SpecifyKind( a.Period.EndTime.Value, DateTimeKind.Local )
                                } )
                                .ToList();

                            dateTimes.AddRange( result );
                        }
                    }

                    if ( dateTimes.Any() )
                    {
                        var startAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartAction" ).FirstOrDefault().Value.Value;
                        var endAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndAction" ).FirstOrDefault().Value.Value;

                        // Fetching Actions from DefinedValue Cache
                        startAction = DefinedValueCache.Get( startAction ).Value;
                        endAction = DefinedValueCache.Get( endAction ).Value;

                        // Looping through locations

                        foreach ( var reservationLocation in reservation.ReservationLocations )
                        {
                            var location = reservationLocation.Location;
                            location.LoadAttributes();
                            var overrideGroup = location.GetAttributeValue( AttributeCache.Get( config.OverrideLocationAttribute ).Key ).AsIntegerOrNull();
                            var roomName = location.GetAttributeValue( AttributeCache.Get( config.RoomNameLocationAttribute ).Key );

                            if ( overrideGroup == null )
                            {
                                // Can't do anything if no Override Group is specified
                                var newException = new Exception(
                                    string.Format( @"The Location '{0} ({1})' does not have an Override Group specified.
                                            Please configure this to allow Rock to manipulate your door locks.",
                                                    location.Name,
                                                    location.Id
                                    )
                                );
                                ExceptionLogService.LogException( newException );
                            }
                            else
                            {
                                foreach ( var dateTime in dateTimes )
                                {
                                    var newDoorOverride = new DoorLockOverride
                                    {
                                        StartTime = dateTime.StartDateTime.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                        StartTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), startAction ),
                                        EndTime = dateTime.EndDateTime.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                        EndTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), endAction ),
                                        ReservationId = reservation.Id,
                                        LocationId = location.Id,
                                        OverrideGroup = overrideGroup.Value,
                                        RoomName = roomName,
                                        IsHvacOnly = isHvacOnly
                                    };

                                    overrideLocks.Add( newDoorOverride );
                                }
                            }
                        }
                    }
                }
            }

            return overrideLocks;
        }


        /// <summary>
        /// Gets all of the schedule based door locks from Room Management
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="config">The job configuration.</param>
        /// <returns></returns>
        private List<DoorLockOverride> GetScheduleLocks( Reservation reservation, UpdateDoorLocks_Config config, bool isHvacOnly )
        {
            List<DoorLockOverride> output = new List<DoorLockOverride>();

            var reservationDateTimes = reservation.GetReservationTimes( config.DateToSync, config.DateToSync.AddDays( 1 ).AddMinutes( -1 ) );

            int setupTime = reservation.SetupTime != null ? reservation.SetupTime.Value : 30;
            int cleanupTime = reservation.CleanupTime != null ? reservation.CleanupTime.Value : 30;

            // Looping through reservations
            foreach ( var reservationDateTime in reservationDateTimes )
            {
                // Looping through locations
                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    var location = reservationLocation.Location;
                    location.LoadAttributes();
                    var overrideGroup = location.GetAttributeValue( AttributeCache.Get( config.OverrideLocationAttribute ).Key ).AsIntegerOrNull();
                    var roomName = location.GetAttributeValue( AttributeCache.Get( config.RoomNameLocationAttribute ).Key );

                    if ( overrideGroup == null )
                    {
                        // Can't do anything if no Override Group is specified
                        var newException = new Exception(
                            string.Format( @"The Location '{0} ({1})' does not have an Override Group specified.
                                            Please configure this to allow Rock to manipulate your door locks.",
                                            location.Name,
                                            location.Id
                            )
                        );
                        ExceptionLogService.LogException( newException );
                    }
                    else
                    {

                        var newDoorOverride = new DoorLockOverride
                        {
                            StartTime = reservationDateTime.StartDateTime.AddMinutes( setupTime * -1 ).RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                            StartTimeAction = DoorLockActions.Unlock,
                            EndTime = reservationDateTime.EndDateTime.AddMinutes( cleanupTime ).RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                            EndTimeAction = DoorLockActions.TimeZone,
                            ReservationId = reservation.Id,
                            LocationId = location.Id,
                            OverrideGroup = overrideGroup.Value,
                            RoomName = roomName,
                            IsHvacOnly = isHvacOnly
                        };

                        output.Add( newDoorOverride );
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Gets all of the campus based door locks from Campuses
        /// </summary>
        /// <param name="config">The job configuration.</param>
        /// <returns></returns>
        private List<DoorLockOverride> GetCampusUnlocks( UpdateDoorLocks_Config config )
        {
            RockContext rockContext = new RockContext();
            CampusService campusService = new CampusService( rockContext );

            List<DoorLockOverride> output = new List<DoorLockOverride>();

            var campuses = campusService.Queryable().AsNoTracking().Where( x => x.IsActive == true ).ToList();
            foreach ( var campus in campuses )
            {
                campus.LoadAttributes();

                var attributeMatrixGuid = campus.GetAttributeValue( "DoorOverrides" ).AsGuidOrNull();
                if ( attributeMatrixGuid != null )
                {
                    AttributeMatrixService amService = new AttributeMatrixService( rockContext );
                    var attributeMatrix = amService.Get( attributeMatrixGuid.Value );
                    if ( attributeMatrix != null )
                    {
                        var attributeMatrixItems = attributeMatrix.AttributeMatrixItems.ToList();
                        if ( attributeMatrixItems.Count > 0 )
                        {
                            // Looping through configured overrides
                            foreach ( var attributeMatrixItem in attributeMatrixItems )
                            {
                                attributeMatrixItem.LoadAttributes();

                                DateTime startDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartDateTime" ).FirstOrDefault().Value.Value.AsDateTime().Value;
                                var location = attributeMatrixItem.AttributeValues.Where( x => x.Key == "Location" ).FirstOrDefault().Value.Value;

                                // Making sure items are occuring during the window
                                if (
                                    DateTime.Compare( startDate, config.DateToSync ) >= 0 &&
                                    DateTime.Compare( startDate, config.DateToSync.AddDays( 1 ).AddMinutes( -1 ) ) <= 0 &&
                                    location.AsGuidOrNull() != null
                                )
                                {
                                    startDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartDateTime" ).FirstOrDefault().Value.Value.AsDateTime().Value;
                                    DateTime endDate = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndDateTime" ).FirstOrDefault().Value.Value.AsDateTime().Value;

                                    var startAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "StartAction" ).FirstOrDefault().Value.Value;
                                    var endAction = attributeMatrixItem.AttributeValues.Where( x => x.Key == "EndAction" ).FirstOrDefault().Value.Value;

                                    // Fetching Actions from DefinedValue Cache
                                    startAction = DefinedValueCache.Get( startAction ).Value;
                                    endAction = DefinedValueCache.Get( endAction ).Value;

                                    // Fetching location from Location Service
                                    LocationService locationService = new LocationService( rockContext );
                                    var locationObject = locationService.GetNoTracking( location.AsGuid() );

                                    if ( locationObject != null )
                                    {
                                        locationObject.LoadAttributes();
                                        var attribute = AttributeCache.Get( config.OverrideLocationAttribute );
                                        var overrideGroup = locationObject.GetAttributeValue( attribute.Key ).AsIntegerOrNull();
                                        var roomName = locationObject.GetAttributeValue( AttributeCache.Get( config.RoomNameLocationAttribute ).Key );

                                        if ( overrideGroup == null )
                                        {
                                            // Can't do anything if no Override Group is specified
                                            var newException = new Exception(
                                                string.Format( @"The Location '{0} ({1})' does not have an Override Group specified.
                                            Please configure this to allow Rock to manipulate your door locks.",
                                                                locationObject.Name,
                                                                locationObject.Id
                                                )
                                            );
                                            ExceptionLogService.LogException( newException );
                                        }
                                        else
                                        {
                                            var newDoorOverride = new DoorLockOverride
                                            {
                                                StartTime = startDate.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                                StartTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), startAction ),
                                                EndTime = endDate.RoundToNearest( TimeSpan.FromMinutes( 15 ) ),
                                                EndTimeAction = ( DoorLockActions ) Enum.Parse( typeof( DoorLockActions ), endAction ),
                                                ReservationId = null,
                                                LocationId = locationObject.Id,
                                                OverrideGroup = overrideGroup.Value,
                                                RoomName = roomName,
                                                IsHvacOnly = false
                                            };

                                            output.Add( newDoorOverride );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Checks the list of Door Locks for Shared Rooms
        /// </summary>
        /// <param name="doorLocks">The door locks.</param>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        private List<DoorLockOverride> GetSharedRooms( List<DoorLockOverride> doorLocks, UpdateDoorLocks_Config config )
        {
            List<DoorLockOverride> output = new List<DoorLockOverride>();
            RockContext rockContext = new RockContext();
            LocationService locationService = new LocationService( rockContext );

            if ( config.SharedLocationAttribute == Guid.Empty )
            {
                var exception = new Exception( "To process Shared Rooms, please configure the Shared Room Attribute" );
                ExceptionLogService.LogException( exception );
                return output;
            }

            var sharedLocationAttribute = AttributeCache.Get( config.SharedLocationAttribute );

            if ( sharedLocationAttribute != null )
            {
                // Looping through door configs
                foreach ( var doorLock in doorLocks )
                {
                    if ( doorLock.LocationId > 0 )
                    {
                        var location = locationService.Get( doorLock.LocationId );
                        if ( location != null )
                        {
                            location.LoadAttributes();

                            var sharedLocationItems = location.GetAttributeValue( sharedLocationAttribute.Key ).AsDictionaryOrNull();

                            if ( sharedLocationItems != null )
                            {
                                foreach ( var sharedLocation in sharedLocationItems )
                                {
                                    var overrideGroup = sharedLocation.Key.AsIntegerOrNull();
                                    var sharedLocationId = sharedLocation.Value.AsIntegerOrNull();

                                    if ( sharedLocationId != null && overrideGroup != null )
                                    {
                                        // Checking to see if location is in the list
                                        if ( doorLocks.Where( x => x.LocationId == sharedLocationId.Value ).Any() )
                                        {
                                            var sharedRooms = doorLocks.Where( x => x.LocationId == sharedLocationId.Value ).ToList();

                                            foreach ( var sharedRoom in sharedRooms )
                                            {
                                                if ( DateTime.Compare( sharedRoom.StartTime, doorLock.StartTime ) >= 0 &&
                                                    DateTime.Compare( sharedRoom.StartTime, doorLock.EndTime ) <= 0 )
                                                {
                                                    var startDate = DateTime.Compare( sharedRoom.StartTime, doorLock.StartTime ) >= 0 ? sharedRoom.StartTime : doorLock.StartTime;
                                                    var endDate = DateTime.Compare( sharedRoom.EndTime, doorLock.EndTime ) <= 0 ? sharedRoom.EndTime : doorLock.EndTime;

                                                    var isHvacOnly = false;
                                                    if ( sharedRoom.IsHvacOnly.HasValue && doorLock.IsHvacOnly.HasValue )
                                                    {
                                                        isHvacOnly = sharedRoom.IsHvacOnly.Value && doorLock.IsHvacOnly.Value;
                                                    }
                                                    else if ( sharedRoom.IsHvacOnly.HasValue )
                                                    {
                                                        isHvacOnly = sharedRoom.IsHvacOnly.Value;
                                                    }
                                                    else if ( doorLock.IsHvacOnly.HasValue )
                                                    {
                                                        isHvacOnly = doorLock.IsHvacOnly.Value;
                                                    }
                                                    else
                                                    {
                                                        isHvacOnly = true;
                                                    }
                                                    var newDoorOverride = new DoorLockOverride
                                                    {
                                                        StartTime = startDate,
                                                        StartTimeAction = DoorLockActions.Unlock,
                                                        EndTime = endDate,
                                                        EndTimeAction = DoorLockActions.Lock,
                                                        ReservationId = null,
                                                        LocationId = sharedLocationId.Value,
                                                        OverrideGroup = overrideGroup.Value,
                                                        IsHvacOnly = isHvacOnly
                                                    };

                                                    if ( !output.Contains( newDoorOverride ) )
                                                    {
                                                        output.Add( newDoorOverride );
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var exception = new Exception( "To process Shared Rooms, please configure the Shared Room Attribute" );
                ExceptionLogService.LogException( exception );
                return output;
            }

            return output;
        }


        /// <summary>
        /// Pushes door locks into the Rock Door Lock "Cache" table
        /// </summary>
        /// <param name="doorLocks">The door locks.</param>
        private string LoadDoorLockTable( List<DoorLockOverride> doorLocks, UpdateDoorLocks_Config config )
        {
            RockContext rockContext = new RockContext();
            DoorLockService doorLockService = new DoorLockService( rockContext );

            // Clearing Locks that have not happened yet
            var itemsToBeRemoved = doorLockService.Queryable().Where( x => DbFunctions.TruncateTime( x.StartDateTime ) == config.DateToSync.Date && x.StartDateTime > RockDateTime.Now ).ToList();
            var futureDoorLocks = doorLocks.Where( x => x.StartTime.Date == config.DateToSync ).ToList();

            int addedCount = 0;

            if ( !config.TestMode )
            {
                if ( itemsToBeRemoved.Count > 0 )
                {
                    doorLockService.DeleteRange( itemsToBeRemoved );
                    rockContext.SaveChanges();
                }

                if ( futureDoorLocks.Count > 0 )
                {
                    // Adding Values that are happening in the future.
                    foreach ( var doorLock in futureDoorLocks )
                    {
                        var item = new DoorLock
                        {
                            StartDateTime = doorLock.StartTime,
                            StartAction = doorLock.StartTimeAction,
                            EndDateTime = doorLock.EndTime,
                            EndAction = doorLock.EndTimeAction,
                            ReservationId = doorLock.ReservationId,
                            LocationId = doorLock.LocationId,
                            OverrideGroup = doorLock.OverrideGroup,
                            RoomName = doorLock.RoomName,
                            IsHvacOnly = doorLock.IsHvacOnly
                        };

                        // Adding if not in DB
                        if ( !doorLockService.Exists( item ) )
                        {
                            doorLockService.Add( item );
                            rockContext.SaveChanges();

                            addedCount += 1;
                        }
                    }


                }

                return string.Format( "ROCK: Purged {0} record(s) and added {1}", itemsToBeRemoved.Count, addedCount );
            }
            else
            {
                return string.Format( "ROCK: Would have Purged {0} records and potentially added {1}", itemsToBeRemoved.Count, futureDoorLocks.Count );
            }
        }

        /// <summary>
        /// Synchronizes the Rock "Cache" table with DSX OvrSchedule table
        /// </summary>
        /// <param name="config">The job configuration.</param>
        /// <returns></returns>
        private string SyncRockWithDSX( UpdateDoorLocks_Config config )
        {
            DateTime startTime = config.DateToSync;
            DateTime endTime = config.DateToSync.AddDays( 1 ).AddMinutes( -1 );
            DateTime now = RockDateTime.Now;
            int errorCount = 0;
            int totalRecords = 0;
            int rowsRemoved = 0;

            if ( config.TestMode )
            {
                return "DSX: Nothing has been modified";
            }
            else
            {
                SqlCommand sql = new SqlCommand();
                try
                {
                    sql.Connection = new SqlConnection( config.DSXConnectionString );
                    sql.Connection.Open();

                    // Purging all events between start and end date
                    sql.CommandText = @"
                    DELETE FROM OvrSchedule
                    WHERE [Opr] LIKE 'Rock%'
                    AND [StartDate] BETWEEN @StartDate AND @EndDate
                    AND [StopDate] > @NowDate";

                    sql.Parameters.AddWithValue( "@StartDate", startTime.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                    sql.Parameters.AddWithValue( "@EndDate", endTime.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                    sql.Parameters.AddWithValue( "@NowDate", now.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );

                    // Getting count of Rows deleted
                    rowsRemoved = sql.ExecuteNonQuery();

                    PushDoorLocks( startTime, now, ref errorCount, ref totalRecords, sql );
                    PushHvacOnlyRecords( startTime, now, ref errorCount, ref totalRecords, sql );
                }
                catch ( Exception ex )
                {
                    // Oh No! Something bad happened.
                    sql.Connection.Close();

                    ExceptionLogService.LogException( ex );
                    return "DSX: [ERROR] An error occured while connecting/querying DSX. Please check the Rock Exception Log for details";
                }

                // Closing DB Connection
                sql.Connection.Close();

                // Returning string with DSX results
                if ( errorCount > 0 )
                {
                    return "DSX: [ERROR] Threw " + errorCount + " exception(s). Please check the Rock Exception Log for details";
                }
                else
                {
                    return "DSX: [SUCCESS] Purged " + rowsRemoved + " records and added " + totalRecords + " record into DSX";
                }
            }
        }

        private static void PushDoorLocks( DateTime startTime, DateTime now, ref int errorCount, ref int totalRecords, SqlCommand sql )
        {
            // Clearing parameters so we can use the connection again
            sql.Parameters.Clear();

            // Setting query we will use for inserting records
            sql.CommandText = @"
                        DECLARE @NewId int;
                        INSERT INTO [dbo].[OvrSchedule] (
                            [PointID],
                            [PointType],
                            [StartCmd],
                            [StartDate],
                            [StopCmd],
                            [StopDate],
                            [Opr],
                            [OprID],
                            [Status]
                        ) VALUES (
                            @PointID,
                            @PointType,
                            @StartCmd,
                            @StartDateUTC,
                            @StopCmd,
                            @StopDateUTC,
                            @Opr,
                            @OprID,
                            @Status
                        )

                        -- Getting Newly create row Id
                        SET @NewId = SCOPE_IDENTITY();

                        INSERT INTO [dbo].[BacTalkRoomXref] (
                            [FKScheduleID],
                            [RoomNumber],
                            [StartDate],
                            [StopDate]
                        ) VALUES (
                            @NewId,
                            @LocationName,
                            @StartDate,
                            @StopDate
                        )";

            RockContext rockContext = new RockContext();
            DoorLockService doorLockService = new DoorLockService( rockContext );

            var items = doorLockService.Queryable().AsNoTracking().Where( x => x.IsHvacOnly == false && DbFunctions.TruncateTime( x.StartDateTime ) == startTime.Date ).ToList();
            // Updating counter
            totalRecords += items.Count;

            // Debugging
            //ExceptionLogService.LogException( new Exception( "DEBUG: Total Records to be processed are: " + totalRecords ) );

            // Looping through items and writing to DSX DB
            foreach ( var item in items )
            {
                // Starting with clean paramerters
                sql.Parameters.Clear();

                int startAction = ( int ) item.StartAction;
                int endAction = ( int ) item.EndAction;

                sql.Parameters.AddWithValue( "@PointID", item.OverrideGroup );
                sql.Parameters.AddWithValue( "@PointType", 2 );
                sql.Parameters.AddWithValue( "@StartCmd", startAction );
                sql.Parameters.AddWithValue( "@StartDateUTC", item.StartDateTime.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                sql.Parameters.AddWithValue( "@StopCmd", endAction );
                sql.Parameters.AddWithValue( "@StopDateUTC", item.EndDateTime.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                sql.Parameters.AddWithValue( "@Opr", "Rock " + RockDateTime.Now.ToShortDateTimeString() );
                sql.Parameters.AddWithValue( "@OprID", 29 );
                sql.Parameters.AddWithValue( "@Status", 1 );
                sql.Parameters.AddWithValue( "@LocationName", item.RoomName );

                sql.Parameters.AddWithValue( "@StartDate", item.StartDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                sql.Parameters.AddWithValue( "@StopDate", item.EndDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );

                // Validating record was added
                if ( sql.ExecuteNonQuery() < 1 )
                {
                    var innerException = new Exception( string.Format( "PointID: {0} @StartDate: {1} @StopDate: {2}", item.OverrideGroup, item.StartDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ), item.EndDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) ) );
                    var exception = new Exception( "A potiential error occurred while pushing configuration to DSX", innerException );
                    ExceptionLogService.LogException( exception );

                    errorCount++;
                }
            }
        }
        private static void PushHvacOnlyRecords( DateTime startTime, DateTime now, ref int errorCount, ref int totalRecords, SqlCommand sql )
        {
            // Clearing parameters so we can use the connection again
            sql.Parameters.Clear();

            // Setting query we will use for inserting records
            sql.CommandText = @"
                        INSERT INTO [dbo].[BacTalkRoomXref] (
                            [FKScheduleID],
                            [RoomNumber],
                            [StartDate],
                            [StopDate]
                        ) VALUES (
                            0,
                            @LocationName,
                            @StartDate,
                            @StopDate
                        )";

            RockContext rockContext = new RockContext();
            DoorLockService doorLockService = new DoorLockService( rockContext );

            var items = doorLockService.Queryable().AsNoTracking().Where( x => x.IsHvacOnly == true && DbFunctions.TruncateTime( x.StartDateTime ) == startTime.Date ).ToList();
            // Updating counter
            totalRecords += items.Count;

            // Debugging
            //ExceptionLogService.LogException( new Exception( "DEBUG: Total Records to be processed are: " + totalRecords ) );

            // Looping through items and writing to DSX DB
            foreach ( var item in items )
            {
                // Starting with clean paramerters
                sql.Parameters.Clear();

                sql.Parameters.AddWithValue( "@LocationName", item.RoomName );
                sql.Parameters.AddWithValue( "@StartDate", item.StartDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                sql.Parameters.AddWithValue( "@StopDate", item.EndDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
                // Validating record was added
                if ( sql.ExecuteNonQuery() < 1 )
                {
                    var innerException = new Exception( string.Format( "LocationName: {0} @StartDate: {1} @StopDate: {2}", item.RoomName, item.StartDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ), item.EndDateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) ) );
                    var exception = new Exception( "A potiential error occurred while pushing configuration to DSX", innerException );
                    ExceptionLogService.LogException( exception );

                    errorCount++;
                }
            }
        }
    }

    public class UpdateDoorLocks_Config
    {
        public DateTime DateToSync { get; set; }
        public Guid OverrideLocationAttribute { get; set; }
        public Guid ProcessDoorLockAttribute { get; set; }
        public Guid OverrideGroupAttribute { get; set; }
        public Guid SharedLocationAttribute { get; set; }
        public string DSXConnectionString { get; set; }
        public bool TestMode { get; set; }
        public Guid RoomNameLocationAttribute { get; set; }
    }
}

public static class DateTimeHelper
{
    public static DateTime RoundUp( this DateTime dt, TimeSpan d )
    {
        var modTicks = dt.Ticks % d.Ticks;
        var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
        return new DateTime( dt.Ticks + delta, dt.Kind );
    }

    public static DateTime RoundDown( this DateTime dt, TimeSpan d )
    {
        var delta = dt.Ticks % d.Ticks;
        return new DateTime( dt.Ticks - delta, dt.Kind );
    }

    public static DateTime RoundToNearest( this DateTime dt, TimeSpan d )
    {
        var delta = dt.Ticks % d.Ticks;
        bool roundUp = delta > d.Ticks / 2;
        var offset = roundUp ? d.Ticks : 0;

        return new DateTime( dt.Ticks + offset - delta, dt.Kind );
    }
}