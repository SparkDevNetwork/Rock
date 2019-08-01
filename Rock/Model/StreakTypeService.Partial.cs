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
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="StreakType"/> entity objects.
    /// </summary>
    public partial class StreakTypeService
    {
        #region Constants

        /// <summary>
        /// Days per week
        /// </summary>
        private static int DaysPerWeek = 7;

        /// <summary>
        /// Bits per byte
        /// </summary>
        private static int BitsPerByte = 8;

        /// <summary>
        /// The minimum size of a byte array for a streak related map. The point is to reduce memory reallocations which
        /// are costly, by starting and growing by this size. 128 bytes is trivial memory, but has 1024 bits, which is enough to represent
        /// almost 3 years worth of daily streak data.
        /// </summary>
        private static int MapByteGrowthCount = 128;

        /// <summary>
        /// The number of hex chars per byte
        /// </summary>
        private static int HexDigitsPerByte = 2;

        /// <summary>
        /// The number of possible values per single hex digit
        /// </summary>
        private static int Base16 = 16;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Get the most recent engagement bits where there were occurrences for the person
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="personId"></param>
        /// <param name="unitCount"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public OccurrenceEngagement[] GetRecentEngagementBits( int streakTypeId, int personId, int unitCount, out string errorMessage )
        {
            errorMessage = string.Empty;

            var streakType = StreakTypeCache.Get( streakTypeId );

            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return null;
            }

            if ( !streakType.IsActive )
            {
                errorMessage = "An active streak type is required";
                return null;
            }

            if ( unitCount < 1 )
            {
                unitCount = 24;
            }

            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );

            var enrollmentMaps = streakService.GetByStreakTypeAndPerson( streakTypeId, personId )
                .AsNoTracking()
                .Select( se => se.EngagementMap )
                .ToArray();

            var enrollmentMap = GetAggregateMap( enrollmentMaps );
            return GetMostRecentEngagementBits( enrollmentMap, streakType.OccurrenceMap, streakType.StartDate, streakType.OccurrenceFrequency, unitCount );
        }

        /// <summary>
        /// Enroll the person into the streak type
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="enrollmentDate">The enrollment date.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public Streak Enroll( StreakTypeCache streakTypeCache, int personId, out string errorMessage, DateTime? enrollmentDate = null, int? locationId = null )
        {
            errorMessage = string.Empty;

            // Validate the parameters
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type is required";
                return null;
            }

            if ( personId == default( int ) )
            {
                errorMessage = "A valid personId is required";
                return null;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return null;
            }

            var isDaily = streakTypeCache.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var maxDate = RockDateTime.Today;
            var minDate = streakTypeCache.StartDate;
            enrollmentDate = enrollmentDate ?? maxDate;

            if ( !isDaily )
            {
                enrollmentDate = enrollmentDate.Value.SundayDate();
                maxDate = maxDate.SundayDate();
                minDate = minDate.SundayDate();
            }

            if ( enrollmentDate.Value > maxDate )
            {
                errorMessage = "The enrollmentDate cannot be in the future";
                return null;
            }

            if ( enrollmentDate.Value < minDate )
            {
                errorMessage = "The enrollmentDate cannot be before the streak type start date";
                return null;
            }

            // Make sure the enrollment does not already exist for the person
            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );
            var alreadyEnrolled = streakService.IsEnrolled( streakTypeCache.Id, personId );

            if ( alreadyEnrolled )
            {
                errorMessage = "The streak already exists";
                return null;
            }

            // Get the person alias id
            var personAliasService = new PersonAliasService( rockContext );
            var personAliasId = personAliasService.Queryable().AsNoTracking().FirstOrDefault( pa => pa.PersonId == personId )?.Id;

            if ( !personAliasId.HasValue )
            {
                errorMessage = "The person does not have a person alias id";
                return null;
            }

            // Add the enrollment, matching the occurrence map's length with the same length array of 0/false bits
            var streak = new Streak
            {
                StreakTypeId = streakTypeCache.Id,
                PersonAliasId = personAliasId.Value,
                LocationId = locationId,
                EnrollmentDate = enrollmentDate.Value,
                EngagementMap = AllocateNewByteArray( streakTypeCache.OccurrenceMap?.Length )
            };

            streakService.Add( streak );
            return streak;
        }

        /// <summary>
        /// Return the locations associated with the streak type structure
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public IQueryable<Location> GetLocations( StreakTypeCache streakTypeCache, out string errorMessage )
        {
            errorMessage = string.Empty;

            var defaultReturnValue = new List<Location>().AsQueryable();
            var errorReturnValue = ( IQueryable<Location> ) null;

            // Validate the parameters
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type is required";
                return errorReturnValue;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return errorReturnValue;
            }

            // If the structure information is not complete, it is not possible to get locations
            if ( !streakTypeCache.StructureEntityId.HasValue || !streakTypeCache.StructureType.HasValue )
            {
                return defaultReturnValue;
            }

            // Calculate the group locations depending on the structure type
            var groupLocationsQuery = GetGroupLocationsQuery( streakTypeCache.StructureType.Value, streakTypeCache.StructureEntityId.Value );
            return groupLocationsQuery.Select( gl => gl.Location )
                .DistinctBy( l => l.Id )
                .AsQueryable();
        }

        /// <summary>
        /// Get the schedules for the streak type at the given location
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="locationId"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public IQueryable<Schedule> GetLocationSchedules( StreakTypeCache streakTypeCache, int locationId, out string errorMessage )
        {
            errorMessage = string.Empty;

            var defaultReturnValue = new List<Schedule>().AsQueryable();
            var errorReturnValue = ( IQueryable<Schedule> ) null;

            // Validate the parameters
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type is required";
                return errorReturnValue;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return errorReturnValue;
            }

            // If the structure information is not complete, it is not possible to get locations
            if ( !streakTypeCache.StructureEntityId.HasValue || !streakTypeCache.StructureType.HasValue )
            {
                return defaultReturnValue;
            }

            // Calculate the schedules for the group locations within the structure
            var groupLocationsQuery = GetGroupLocationsQuery( streakTypeCache.StructureType.Value, streakTypeCache.StructureEntityId.Value );
            return groupLocationsQuery.Where( gl => gl.LocationId == locationId )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsActive )
                .DistinctBy( s => s.Id )
                .AsQueryable();
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the attendance structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="errorMessage"></param>
        public static void RebuildStreakTypeFromAttendance( int streakTypeId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeId );

            // Validate the parameters
            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            if ( !streakType.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            // Get the occurrences that did occur
            var isDaily = streakType.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var occurrenceQuery = occurrenceService.Queryable()
                .AsNoTracking()
                .Where( ao => ao.DidNotOccur != true );

            // If the structure information is set, then limit the occurrences by the matching groups
            if ( streakType.StructureType.HasValue && streakType.StructureEntityId.HasValue )
            {
                var groupQuery = GetGroupsQuery( rockContext, streakType.StructureType.Value, streakType.StructureEntityId.Value );
                occurrenceQuery = occurrenceQuery.Where( ao => groupQuery.Any( g => g.Id == ao.GroupId ) );
            }

            // Limit the output to the dates according to the frequency
            var occurrenceDates = occurrenceQuery
                .Select( ao => isDaily ? ao.OccurrenceDate : ao.SundayDate )
                .Distinct()
                .OrderBy( d => d )
                .ToArray();

            var numberOfOccurrences = occurrenceDates.Length;
            if ( numberOfOccurrences < 1 )
            {
                errorMessage = "No attendance occurrences were found.";
                return;
            }

            // Set the streak type occurrence map according to the dates returned
            streakType.StartDate = occurrenceDates.First();
            var occurrenceMap = AllocateNewByteArray();

            for ( var i = 0; i < numberOfOccurrences; i++ )
            {
                occurrenceMap = SetBit( occurrenceMap, streakType.StartDate, occurrenceDates[i], streakType.OccurrenceFrequency, true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            streakType.OccurrenceMap = occurrenceMap;
            rockContext.SaveChanges();

            // Get all of the attendees for the streak type
            var personAliasIds = occurrenceQuery
                .SelectMany( ao => ao.Attendees.Where( a => a.DidAttend == true && a.PersonAliasId.HasValue ) )
                .Select( a => a.PersonAliasId.Value )
                .Distinct();

            foreach ( var personAliasId in personAliasIds )
            {
                RebuildStreakFromAttendance( streakTypeId, personAliasId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Rebuild the streak map from the attendance structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void RebuildStreakFromAttendance( int streakTypeId, int personAliasId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var streakTypeService = new StreakTypeService( rockContext );
            var streakService = new StreakService( rockContext );
            var attendanceService = new AttendanceService( rockContext );

            // Validate the parameters
            var streakType = streakTypeService.Get( streakTypeId );

            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            if ( !streakType.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            // Get the attendance that did occur
            var isDaily = streakType.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var startDate = isDaily ? streakType.StartDate.Date : streakType.StartDate.SundayDate();

            var attendanceQuery = attendanceService.Queryable().AsNoTracking().Where( a =>
                a.PersonAliasId == personAliasId &&
                a.DidAttend == true &&
                a.Occurrence.DidNotOccur != true && (
                    ( isDaily && a.Occurrence.OccurrenceDate >= startDate ) ||
                    ( !isDaily && a.Occurrence.SundayDate >= startDate )
                ) );

            // If the structure information is set, then limit the attendances by the matching groups
            if ( streakType.StructureType.HasValue && streakType.StructureEntityId.HasValue )
            {
                var groupQuery = GetGroupsQuery( rockContext, streakType.StructureType.Value, streakType.StructureEntityId.Value );
                attendanceQuery = attendanceQuery.Where( a => groupQuery.Any( g => g.Id == a.Occurrence.GroupId ) );
            }

            // Get the attended dates
            var datesAttended = attendanceQuery
                .Select( a => isDaily ? a.Occurrence.OccurrenceDate : a.Occurrence.SundayDate )
                .Distinct()
                .OrderBy( d => d )
                .ToArray();

            var attendanceDateCount = datesAttended.Length;
            var enrollmentDate = attendanceDateCount == 0 ?
                ( isDaily ? RockDateTime.Today : RockDateTime.Today.SundayDate() ) :
                datesAttended.First();

            // Get the enrollment
            var streak = streakService.Queryable().FirstOrDefault( se =>
                se.StreakTypeId == streakTypeId
                && se.PersonAliasId == personAliasId );

            // Create the enrollment if needed
            if ( streak == null )
            {
                streak = new Streak
                {
                    StreakTypeId = streakTypeId,
                    PersonAliasId = personAliasId,
                    EnrollmentDate = enrollmentDate
                };

                streakService.Add( streak );
            }

            // Create a new map matching the length of the occurrence map
            var occurrenceMapLength = streakType.OccurrenceMap == null ? 0 : streakType.OccurrenceMap.Length;
            var engagementMap = new byte[occurrenceMapLength];

            // Loop over each date attended and set the bit corresponding to that date
            for ( var dateIndex = 0; dateIndex < attendanceDateCount; dateIndex++ )
            {
                engagementMap = SetBit( engagementMap, streakType.StartDate, datesAttended[dateIndex], streakType.OccurrenceFrequency, true,
                    out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            streak.EngagementMap = engagementMap;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Calculate streak data (things like current streak count and longest streak)
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="startDate">Defaults to the streak type start date</param>
        /// <param name="endDate">Defaults to now</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public StreakData GetStreakData( StreakTypeCache streakTypeCache, int personId, out string errorMessage,
        DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false )
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            errorMessage = string.Empty;

            // Validate the streak type
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type is required";
                return null;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return null;
            }

            var isDaily = streakTypeCache.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var maxDate = isDaily ? RockDateTime.Today : RockDateTime.Today.SundayDate();
            var streakTypeMapStartDate = isDaily ? streakTypeCache.StartDate.Date : streakTypeCache.StartDate.SundayDate();

            // Apply default values to parameters
            if ( !startDate.HasValue )
            {
                startDate = streakTypeMapStartDate;
            }

            if ( !endDate.HasValue )
            {
                endDate = maxDate;
            }

            // Adjust the start and stop dates based on the selected frequency
            if ( !isDaily )
            {
                startDate = startDate.Value.SundayDate();
                endDate = endDate.Value.SundayDate();
            }

            // Validate the parameters
            if ( startDate > maxDate )
            {
                errorMessage = "StartDate cannot be in the future";
                return null;
            }

            if ( startDate < streakTypeMapStartDate )
            {
                errorMessage = "StartDate cannot be before the streak type start date";
                return null;
            }

            if ( endDate > maxDate )
            {
                errorMessage = "EndDate cannot be in the future";
                return null;
            }

            if ( startDate >= endDate )
            {
                errorMessage = "EndDate must be after the StartDate";
                return null;
            }

            // Get the enrollment if it exists
            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );
            var streaks = streakService.GetByStreakTypeAndPerson( streakTypeCache.Id, personId ).AsNoTracking().ToList();
            var locationId = streaks.FirstOrDefault()?.LocationId;
            var enrollmentDate = streaks.Any() ? streaks.Min( se => se.EnrollmentDate ) : ( DateTime? ) null;

            if ( !enrollmentDate.HasValue )
            {
                startDate = maxDate;
            }
            else if ( enrollmentDate > startDate )
            {
                startDate = enrollmentDate;
            }

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( startDate.Value, endDate.Value, streakTypeCache.OccurrenceFrequency, true );

            // Calculate the aggregate engagement map, which are all of the engagement maps ORed together
            var engagementMaps = streaks.Where( se => se.EngagementMap != null ).Select( se => se.EngagementMap ).ToArray();
            var aggregateEngagementMap = GetAggregateMap( engagementMaps );

            // Calculate the streak specific exclusion map
            var streakExclusionMaps = streaks.Where( se => se.ExclusionMap != null ).Select( se => se.ExclusionMap ).ToArray();
            var aggregateStreakExclusionMap = GetAggregateMap( streakExclusionMaps );

            // Calculate the aggregate exclusion map, which are all of the exclusion maps ORed together
            var exclusionMaps = streakTypeCache.StreakTypeExclusions
                .Where( soe => soe.LocationId == locationId && soe.ExclusionMap != null )
                .Select( soe => soe.ExclusionMap )
                .ToList();

            exclusionMaps.Add( aggregateStreakExclusionMap );
            var aggregateExclusionMap = GetAggregateMap( exclusionMaps.ToArray() );

            // Calculate streaks and object array if requested
            var currentStreak = 0;
            var currentStreakStartDate = ( DateTime? ) null;

            var longestStreak = 0;
            var longestStreakStartDate = ( DateTime? ) null;
            var longestStreakEndDate = ( DateTime? ) null;

            var occurrenceCount = 0;
            var engagementCount = 0;
            var absenceCount = 0;
            var excludedAbsenceCount = 0;

            var objectArray = createObjectArray ? new List<StreakData.FrequencyUnitData>( numberOfFrequencyUnits ) : null;

            // Iterate over the maps from the start to the end date
            void iterationAction( int currentUnit, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                if ( hasOccurrence )
                {
                    occurrenceCount++;

                    if ( hasEngagement )
                    {
                        engagementCount++;

                        // If starting a new streak, record the date
                        if ( currentStreak == 0 )
                        {
                            currentStreakStartDate = CalculateDateFromOffset( startDate.Value, currentUnit, streakTypeCache.OccurrenceFrequency );
                        }

                        // Count this engagement toward the current streak
                        currentStreak++;

                        // If this is now the longest streak, update the longest counters
                        if ( currentStreak > longestStreak )
                        {
                            longestStreak = currentStreak;
                            longestStreakStartDate = currentStreakStartDate;
                            longestStreakEndDate = CalculateDateFromOffset( startDate.Value, currentUnit, streakTypeCache.OccurrenceFrequency );
                        }
                    }
                    else if ( hasExclusion )
                    {
                        // Excluded/excused absences don't count toward streaks in a positive nor a negative manner, just ignore other
                        // than this count
                        excludedAbsenceCount++;
                    }
                    else
                    {
                        absenceCount++;

                        // Break the current streak
                        currentStreak = 0;
                        currentStreakStartDate = null;
                    }
                }

                if ( createObjectArray )
                {
                    objectArray.Add( new StreakData.FrequencyUnitData
                    {
                        DateTime = CalculateDateFromOffset( startDate.Value, currentUnit, streakTypeCache.OccurrenceFrequency ),
                        HasEngagement = hasEngagement,
                        HasExclusion = hasExclusion,
                        HasOccurrence = hasOccurrence
                    } );
                }
            }

            IterateMaps( streakTypeMapStartDate, startDate.Value, endDate.Value, streakTypeCache.OccurrenceFrequency, streakTypeCache.OccurrenceMap,
                aggregateEngagementMap, aggregateExclusionMap, iterationAction, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Create the return object
            var data = new StreakData
            {
                StreakTypeId = streakTypeCache.Id,
                StreakIds = streaks.Select( s => s.Id ).ToList(),
                EnrollmentCount = streaks.Count,
                FirstEnrollmentDate = enrollmentDate,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                OccurrenceMap = includeBitMaps ? GetHexDigitStringFromMap( streakTypeCache.OccurrenceMap ) : null,
                ExclusionMap = includeBitMaps ? GetHexDigitStringFromMap( aggregateExclusionMap ) : null,
                EngagementMap = includeBitMaps ? GetHexDigitStringFromMap( aggregateEngagementMap ) : null,
                OccurrenceFrequency = streakTypeCache.OccurrenceFrequency,
                CurrentStreakCount = currentStreak,
                CurrentStreakStartDate = currentStreakStartDate,
                LongestStreakCount = longestStreak,
                LongestStreakStartDate = longestStreakStartDate,
                LongestStreakEndDate = longestStreakEndDate,
                PerFrequencyUnit = objectArray,
                AbsenceCount = absenceCount,
                EngagementCount = engagementCount,
                ExcludedAbsenceCount = excludedAbsenceCount,
                OccurrenceCount = occurrenceCount
            };

            stopwatch.Stop();
            data.ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            return data;
        }

        /// <summary>
        /// Calculate streak data (like the current streak and longest streak count)
        /// </summary>
        /// <param name="streakId"></param>
        /// <param name="startDate">Defaults to the streak type start date</param>
        /// <param name="endDate">Defaults to now</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public StreakData GetStreakData( int streakId, out string errorMessage,
        DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false )
        {
            errorMessage = string.Empty;
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var streakService = new StreakService( rockContext );

            var streak = streakService.Get( streakId );
            if ( streak == null )
            {
                errorMessage = "A valid streak is required";
                return null;
            }

            var personId = personAliasService.GetPersonId( streak.PersonAliasId );
            if ( !personId.HasValue )
            {
                errorMessage = "A valid person ID is required";
                return null;
            }

            var streakTypeCache = StreakTypeCache.Get( streak.StreakTypeId );
            return GetStreakData( streakTypeCache, personId.Value, out errorMessage, startDate, endDate, createObjectArray, includeBitMaps );
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the Streak Engagement map and also
        /// Attendance (if enabled).
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement">Defaults to today</param>
        /// <param name="groupId">This is required for marking attendance unless the streak type is a group structure type</param>
        /// <param name="locationId"></param>
        /// <param name="scheduleId"></param>
        /// <param name="addOrUpdateAttendanceRecord">Should this method add or create <see cref="Attendance"/> models?</param>
        public void MarkEngagement( StreakTypeCache streakTypeCache, int personId, out string errorMessage,
            DateTime? dateOfEngagement = null, int? groupId = null, int? locationId = null, int? scheduleId = null,
            bool addOrUpdateAttendanceRecord = true )
        {
            errorMessage = string.Empty;

            // Validate the streak type
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            // Override the group id if the streak type is explicit about the group
            if ( streakTypeCache.StructureType == StreakStructureType.Group && streakTypeCache.StructureEntityId.HasValue )
            {
                groupId = streakTypeCache.StructureEntityId;
            }

            // Apply default values to parameters
            var isDaily = streakTypeCache.OccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            dateOfEngagement = ( dateOfEngagement ?? RockDateTime.Now ).Date;
            var minDate = streakTypeCache.StartDate.Date;

            if ( !isDaily )
            {
                dateOfEngagement = dateOfEngagement.Value.SundayDate();
                minDate = minDate.SundayDate();
            }

            // Validate the engagement date
            if ( dateOfEngagement < minDate )
            {
                errorMessage = "Cannot mark engagement before the streak type start date";
                return;
            }

            // Get the streak if it exists. The first streak is fine, since when streaks are calculated the maps are combined
            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );
            var streak = streakService.GetByStreakTypeAndPerson( streakTypeCache.Id, personId ).FirstOrDefault();

            if ( streak == null && streakTypeCache.RequiresEnrollment )
            {
                errorMessage = "This streak type requires enrollment";
                return;
            }

            if ( streak == null )
            {
                // Enroll the person since they are marking engagement and enrollment is not required
                streak = Enroll( streakTypeCache, personId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                if ( streak == null )
                {
                    errorMessage = "The enrollment was not successful but no error was specified";
                    return;
                }
            }

            // Mark engagement on the enrollment map
            streak.EngagementMap = SetBit( streak.EngagementMap, streakTypeCache.StartDate, dateOfEngagement.Value,
                streakTypeCache.OccurrenceFrequency, true, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            // Ensure the occurrence bit is set on the streak type model
            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeCache.Id );

            streakType.OccurrenceMap = SetBit( streakType.OccurrenceMap, streakType.StartDate, dateOfEngagement.Value,
                streakType.OccurrenceFrequency, true, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            // Entity Framework cannot detect in-place changes to byte arrays, so force set the properties to modified state
            rockContext.Entry( streak ).Property( se => se.EngagementMap ).IsModified = true;
            rockContext.Entry( streakType ).Property( s => s.OccurrenceMap ).IsModified = true;

            // If attendance is enabled then update attendance models
            if ( streakTypeCache.EnableAttendance && addOrUpdateAttendanceRecord )
            {
                Task.Run( () =>
                {
                    var asyncRockContext = new RockContext();
                    var attendanceService = new AttendanceService( asyncRockContext );

                    // Add or update the attendance, but don't sync streaks since that would create a logic loop
                    attendanceService.AddOrUpdate( streak.PersonAliasId, dateOfEngagement.Value, groupId, locationId,
                        scheduleId, null, null, null, null, null, null, null,
                        syncMatchingStreaks: false );

                    asyncRockContext.SaveChanges();
                } );
            }
        }

        /// <summary>
        /// When an attendance record is created or modified (example: check-in), the method should be called to synchronize that
        /// attendance to any matching streak types and streaks using this method.
        /// </summary>
        /// <param name="attendance"></param>
        /// <param name="errorMessage"></param>
        public void HandleAttendanceRecord( Attendance attendance, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = Context as RockContext;

            if ( attendance == null )
            {
                errorMessage = "The attendance model is required.";
                return;
            }

            if ( attendance.DidAttend != true )
            {
                // If DidAttend is false, then don't do anything for the streak type. We should not unset the bit for the day/week because
                // we don't know if they had some other engagement besides this and cannot assume it should be unset.
                return;
            }

            if ( !attendance.PersonAliasId.HasValue )
            {
                errorMessage = "The person alias ID is required.";
                return;
            }

            // Get the occurrence to ensure all of the virtual properties are included since it's possible the incoming
            // attendance model does not have all of this data populated
            var occurrenceService = new AttendanceOccurrenceService( rockContext );
            var occurrence = occurrenceService.Get( attendance.OccurrenceId );

            if ( occurrence == null )
            {
                errorMessage = "The occurrence model is required.";
                return;
            }

            // Get the person since it's possible the incoming attendance model does not have the virtual PersonAlias loaded
            var personAliasService = new PersonAliasService( rockContext );
            var person = personAliasService.GetPerson( attendance.PersonAliasId.Value );

            if ( person == null )
            {
                errorMessage = "The person model is required.";
                return;
            }

            // Get the person's streaks
            var personId = person.Id;
            var streakService = new StreakService( rockContext );
            var enrolledInStreakTypeIdQuery = streakService.Queryable()
                .AsNoTracking()
                .Where( se => se.PersonAlias.PersonId == personId )
                .Select( se => se.StreakTypeId );
            var enrolledInStreakTypeIds = new HashSet<int>( enrolledInStreakTypeIdQuery );

            // Calculate the attendance group details
            var groupId = occurrence.GroupId;
            var groupTypeId = occurrence.Group?.GroupTypeId;
            var purposeId = occurrence.Group?.GroupType.GroupTypePurposeValueId;

            var checkInConfigIdList = occurrence.Group?.GroupType.ParentGroupTypes.Select( pgt => pgt.Id );
            var checkInConfigIds = checkInConfigIdList == null ? new HashSet<int>() : new HashSet<int>( checkInConfigIdList );

            // Loop through each active streak types that has attendance enabled and mark engagement for it if the person
            // is enrolled or the streak type does not require enrollment
            var matchesStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.EnableAttendance &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    !s.StructureType.HasValue ||
                    (
                        s.StructureEntityId.HasValue &&
                        (
                            ( s.StructureType == StreakStructureType.Group && s.StructureEntityId.Value == groupId ) ||
                            ( s.StructureType == StreakStructureType.GroupType && s.StructureEntityId.Value == groupTypeId ) ||
                            ( s.StructureType == StreakStructureType.GroupTypePurpose && s.StructureEntityId.Value == purposeId ) ||
                            ( s.StructureType == StreakStructureType.CheckInConfig && checkInConfigIds.Contains( s.StructureEntityId.Value ) )
                        )
                    )
                ) );

            foreach ( var streakType in matchesStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, occurrence.OccurrenceDate, addOrUpdateAttendanceRecord: false );
            }
        }

        /// <summary>
        /// This convenience method calls <see cref="HandleAttendanceRecord"/> in an asynchronous fashion such that the calling
        /// process can continue uninhibited. Use this where the streak type and streaks should be synchronized, but the calling
        /// process should continue quickly and without regard to the success of this operation. This method creates it's own data
        /// context and saves the changes when complete.
        /// </summary>
        /// <param name="attendance"></param>
        public static void HandleAttendanceRecordAsync( Attendance attendance )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var service = new StreakTypeService( rockContext );
                service.HandleAttendanceRecord( attendance, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    ExceptionLogService.LogException( errorMessage );
                    return;
                }

                rockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// This convenience method calls <see cref="HandleAttendanceRecord"/> for all attendance records associated the occurrence 
        /// in an asynchronous fashion such that the calling process can continue uninhibited. Use this where the streak type and streaks 
        /// should be synchronized, but the calling process should continue quickly and without regard to the success of this operation.
        /// This method creates it's own data context and any changes will be saved automatically.
        /// </summary>
        /// <param name="occurrenceId"></param>
        public static void HandleAttendanceRecordsAsync( int occurrenceId )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var service = new AttendanceService( rockContext );
                var attendances = service.Queryable().AsNoTracking().Where( a => a.OccurrenceId == occurrenceId );

                foreach ( var attendance in attendances )
                {
                    HandleAttendanceRecordAsync( attendance );
                }
            } );
        }

        /// <summary>
        /// Get the name of the streak type's attendance structure
        /// </summary>
        /// <returns></returns>
        public string GetStructureName( StreakStructureType? structureType, int? structureEntityId )
        {
            if ( !structureType.HasValue || !structureEntityId.HasValue )
            {
                return null;
            }

            var rockContext = Context as RockContext;

            switch ( structureType )
            {
                case StreakStructureType.GroupType:
                case StreakStructureType.CheckInConfig:
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.Get( structureEntityId.Value );
                    return groupType?.Name;
                case StreakStructureType.Group:
                    var groupService = new GroupService( rockContext );
                    var group = groupService.Get( structureEntityId.Value );
                    return group?.Name;
                case StreakStructureType.GroupTypePurpose:
                    var groupTypePurpose = DefinedTypeCache.Get( SystemGuid.DefinedType.GROUPTYPE_PURPOSE );
                    var purposeValue = groupTypePurpose?.DefinedValues.FirstOrDefault( dv => dv.Id == structureEntityId.Value );
                    return purposeValue?.Value;
                default:
                    throw new NotImplementedException( string.Format( "Getting structure name for the StreakStructureType '{0}' is not implemented", structureType ) );
            }
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Get the most recent bits from a map where there was an occurrence
        /// </summary>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="occurrenceMap">The occurrence map.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="streakOccurrenceFrequency">The streak occurrence frequency.</param>
        /// <param name="unitCount">The unit count.</param>
        /// <returns></returns>
        public static OccurrenceEngagement[] GetMostRecentEngagementBits( byte[] engagementMap, byte[] occurrenceMap, DateTime startDate,
            StreakOccurrenceFrequency streakOccurrenceFrequency, int unitCount = 24 )
        {
            if ( unitCount < 1 )
            {
                return null;
            }

            var isDaily = streakOccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var maxDate = isDaily ? RockDateTime.Today : RockDateTime.Now.SundayDate();
            var minDate = isDaily ? startDate : startDate.SundayDate();
            var occurrenceEngagements = new OccurrenceEngagement[unitCount];
            var occurrencesFound = 0;

            void iterationAction( int currentUnit, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                if ( hasOccurrence && occurrencesFound < unitCount )
                {
                    occurrenceEngagements[occurrencesFound] = new OccurrenceEngagement
                    {
                        DateTime = CalculateDateFromOffset( minDate, currentUnit, streakOccurrenceFrequency ),
                        HasEngagement = hasEngagement
                    };

                    occurrencesFound++;
                }
            }

            ReverseIterateMaps( startDate, minDate, maxDate, streakOccurrenceFrequency, occurrenceMap, engagementMap, null, iterationAction, out var errorMessage );
            return occurrenceEngagements;
        }

        /// <summary>
        /// Start an async task to calculate steak data and then copy it to the streak model
        /// </summary>
        /// <param name="streakTypeId"></param>
        public static void UpdateEnrollmentStreakPropertiesAsync( int streakTypeId )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var streakService = new StreakService( rockContext );
                var streakIds = streakService.Queryable().AsNoTracking()
                    .Where( se => se.StreakTypeId == streakTypeId )
                    .Select( se => se.Id );

                foreach ( var streakId in streakIds )
                {
                    StreakService.RefreshStreakDenormalizedPropertiesAsync( streakId );
                }
            } );
        }

        /// <summary>
        /// Determines if the bit at the bitDate in the map is set.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mapStartDate"></param>
        /// <param name="bitDate"></param>
        /// <param name="occurrenceFrequency"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool IsBitSet( byte[] map, DateTime mapStartDate, DateTime bitDate, StreakOccurrenceFrequency occurrenceFrequency, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( map == null || map.Length == 0 )
            {
                return false;
            }

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the streak type start date";
                return false;
            }

            var unitsFromStart = GetFrequencyUnitDifference( mapStartDate, bitDate, occurrenceFrequency, false );
            var bytesNeeded = unitsFromStart / BitsPerByte + 1;
            var byteIndex = map.Length - bytesNeeded;
            var byteBitValue = ( byte ) ( 1 << ( unitsFromStart % BitsPerByte ) );

            if ( byteIndex < 0 )
            {
                return false;
            }

            return ( map[byteIndex] & byteBitValue ) == byteBitValue;
        }

        /// <summary>
        /// Set the bit that corresponds to bitDate. This method works in-place unless the array has to grow. Note that if the array does not
        /// grow and get reallocated, then Entity Framework will not track the change. If needed, force the property state to Modified:
        /// rockContext.Entry( streakModel ).Property( s => s.OccurrenceMap ).IsModified = true;
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mapStartDate"></param>
        /// <param name="bitDate"></param>
        /// <param name="occurrenceFrequency"></param>
        /// <param name="errorMessage"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static byte[] SetBit( byte[] map, DateTime mapStartDate, DateTime bitDate, StreakOccurrenceFrequency occurrenceFrequency, bool newValue, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( occurrenceFrequency == StreakOccurrenceFrequency.Weekly )
            {
                mapStartDate = mapStartDate.SundayDate();
                bitDate = bitDate.SundayDate();
            }

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the streak type start date";
                return map;
            }

            var unitsFromStart = GetFrequencyUnitDifference( mapStartDate, bitDate, occurrenceFrequency, false );
            var bytesNeeded = unitsFromStart / BitsPerByte + 1;

            if ( map == null )
            {
                map = AllocateNewByteArray( bytesNeeded );
            }
            else if ( bytesNeeded > map.Length )
            {
                // Grow the map to accommodate the new value
                map = PadLeft( map, bytesNeeded );
            }

            // Set the target bit within it's byte
            var byteIndex = map.Length - bytesNeeded;
            var byteBitValue = ( byte ) ( 1 << ( unitsFromStart % BitsPerByte ) );

            if ( newValue )
            {
                map[byteIndex] |= byteBitValue;
            }
            else
            {
                map[byteIndex] &= ( byte ) ~byteBitValue;
            }

            return map;
        }

        /// <summary>
        /// Get a byte array from a string of hex digits
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] GetMapFromHexDigitString( string hexString )
        {
            var numberChars = hexString.Length;
            var map = new byte[numberChars / HexDigitsPerByte];

            for ( int i = 0; i < numberChars; i += HexDigitsPerByte )
            {
                map[i / HexDigitsPerByte] = Convert.ToByte( hexString.Substring( i, HexDigitsPerByte ), Base16 );
            }

            return map;
        }

        /// <summary>
        /// Get a string of hex digits from a byte array
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string GetHexDigitStringFromMap( byte[] map )
        {
            if ( map == null || map.Length == 0 )
            {
                return string.Empty;
            }

            return BitConverter.ToString( map ).Replace( "-", "" );
        }

        #endregion Static Methods

        #region Date Calculation Helpers

        /// <summary>
        /// Calculate the date represented by a map bit
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="frequencyUnits"></param>
        /// <param name="streakOccurrenceFrequency"></param>
        /// <returns></returns>
        private static DateTime CalculateDateFromOffset( DateTime startDate, int frequencyUnits, StreakOccurrenceFrequency streakOccurrenceFrequency )
        {
            var isDaily = streakOccurrenceFrequency == StreakOccurrenceFrequency.Daily;

            if ( !isDaily )
            {
                startDate = startDate.SundayDate();
            }

            var daysAfterStart = frequencyUnits * ( isDaily ? 1 : DaysPerWeek );
            var date = startDate.AddDays( daysAfterStart );
            return isDaily ? date.Date : date.SundayDate().Date;
        }

        /// <summary>
        /// Get the number of frequency units (days or weeks) between the two dates
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="occurrenceFrequency">The occurrence frequency.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public static int GetFrequencyUnitDifference( DateTime startDate, DateTime endDate, StreakOccurrenceFrequency occurrenceFrequency, bool isInclusive )
        {
            var isDaily = occurrenceFrequency == StreakOccurrenceFrequency.Daily;

            if ( !isDaily )
            {
                startDate = startDate.SundayDate();
                endDate = endDate.SundayDate();
            }

            // Calculate the difference in days
            var numberOfDays = endDate.Date.Subtract( startDate.Date ).Days;
            var oneFrequencyUnitOfDays = isDaily ? 1 : DaysPerWeek;

            // Adjust to be inclusive if needed
            if ( isInclusive && numberOfDays >= 0 )
            {
                numberOfDays += oneFrequencyUnitOfDays;
            }
            else if ( isInclusive )
            {
                numberOfDays -= oneFrequencyUnitOfDays;
            }

            // Convert from days to the frequency units
            return isDaily ? numberOfDays : ( numberOfDays / DaysPerWeek );
        }

        #endregion Date Helpers

        #region Groups Helpers

        /// <summary>
        /// Get the group locations that are contained with the structure
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="structureEntityId"></param>
        /// <returns></returns>
        private IQueryable<GroupLocation> GetGroupLocationsQuery( StreakStructureType structureType, int structureEntityId )
        {
            var groupsQuery = GetGroupsQuery( Context as RockContext, structureType, structureEntityId );
            return groupsQuery.SelectMany( g => g.GroupLocations ).Where( gl => gl.Location.IsActive );
        }

        /// <summary>
        /// Get the groups that are contained with the structure
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="structureType">Type of the structure.</param>
        /// <param name="structureEntityId">The structure entity identifier.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static IQueryable<Group> GetGroupsQuery( RockContext rockContext, StreakStructureType structureType, int structureEntityId )
        {
            var groupService = new GroupService( rockContext );

            var query = groupService.Queryable()
                .AsNoTracking()
                .Where( g => g.IsActive );

            switch ( structureType )
            {
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.GroupType:
                    return query.Where( g =>
                        g.GroupTypeId == structureEntityId ||
                        g.GroupType.ParentGroupTypes.Any( pgt => pgt.Id == structureEntityId ) );
                case StreakStructureType.Group:
                    return query.Where( g =>
                        g.Id == structureEntityId ||
                        g.ParentGroupId == structureEntityId );
                case StreakStructureType.GroupTypePurpose:
                    return query.Where( g =>
                        g.GroupType.GroupTypePurposeValueId == structureEntityId ||
                        g.GroupType.ParentGroupTypes.Any( pgt => pgt.GroupTypePurposeValueId == structureEntityId ) );
                default:
                    throw new NotImplementedException( string.Format( "Getting groups for the SequenceStructureType '{0}' is not implemented", structureType ) );
            }
        }

        #endregion Groups Helpers

        #region Bit Manipulation

        /// <summary>
        /// Take several maps and combine them (OR bit operation) into a single map
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        public static byte[] GetAggregateMap( byte[][] maps )
        {
            if ( maps == null )
            {
                return null;
            }

            var numberOfMaps = maps.Length;

            if ( numberOfMaps == 0 )
            {
                return new byte[0];
            }

            if ( numberOfMaps == 1 )
            {
                return maps[0];
            }

            var maxMapLength = maps.Max( em => em.Length );
            var aggregateMap = AllocateNewByteArray( maxMapLength );

            for ( var i = 0; i < numberOfMaps; i++ )
            {
                OrBitOperation( aggregateMap, maps[i] );
            }

            return aggregateMap;
        }

        /// <summary>
        /// ORs the bits in-place on top of array a. If b is longer than a, those bits are not ORed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void OrBitOperation( byte[] a, byte[] b )
        {
            // Start at the right side (least significant bit) of each array
            var aIndex = a.Length - 1;
            var bIndex = b.Length - 1;

            while ( aIndex >= 0 && bIndex >= 0 )
            {
                a[aIndex] |= b[bIndex];

                // Move left
                aIndex--;
                bIndex--;
            }
        }

        /// <summary>
        /// Adds 0's to the start or left side of the map
        /// </summary>
        /// <param name="map"></param>
        /// <param name="newSize"></param>
        /// <returns></returns>
        private static byte[] PadLeft( byte[] map, int newSize )
        {
            if ( newSize <= map.Length )
            {
                return map;
            }

            var newMap = AllocateNewByteArray( newSize );
            var offset = newMap.Length - map.Length;

            for ( var i = 0; i < map.Length; i++ )
            {
                newMap[i + offset] = map[i];
            }

            return newMap;
        }

        /// <summary>
        /// Always declare new byte arrays by the <see cref="MapByteGrowthCount"/> so as to limit
        /// memory reallocations to enhance performance.
        /// </summary>
        /// <param name="bytesNeeded"></param>
        /// <returns></returns>
        private static byte[] AllocateNewByteArray( int? bytesNeeded = null )
        {
            var growthUnits = ( bytesNeeded ?? 0 ) / MapByteGrowthCount + 1;
            return new byte[growthUnits * MapByteGrowthCount];
        }

        /// <summary>
        /// Get a byte from a map. This safely returns a zero if the index is outside the array.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="byteIndex"></param>
        /// <returns></returns>
        private static byte GetByteFromMap( byte[] map, int byteIndex )
        {
            if ( byteIndex < 0 || byteIndex >= map.Length )
            {
                return 0;
            }

            return map[byteIndex];
        }

        /// <summary>
        /// Iterates over each bit in the maps over the given timeframe from min to max. The actionPerIteration is called for each bit.
        /// actionPerIteration( currentUnit, hasOccurrence, hasEngagement, hasExclusion );
        /// </summary>
        /// <param name="mapStartDate">The map start date.</param>
        /// <param name="iterationStartDate">The iteration start date.</param>
        /// <param name="iterationEndDate">The iteration end date.</param>
        /// <param name="streakOccurrenceFrequency">The streak occurrence frequency.</param>
        /// <param name="occurrenceMap">The occurrence map.</param>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="exclusionMap">The exclusion map.</param>
        /// <param name="actionPerIteration">The action per iteration.</param>
        /// <param name="errorMessage"></param>
        private static void IterateMaps( DateTime mapStartDate, DateTime iterationStartDate, DateTime iterationEndDate,
            StreakOccurrenceFrequency streakOccurrenceFrequency, byte[] occurrenceMap, byte[] engagementMap, byte[] exclusionMap,
            Action<int, bool, bool, bool> actionPerIteration, out string errorMessage )
        {
            errorMessage = string.Empty;

            var isDaily = streakOccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var currentDate = isDaily ? RockDateTime.Today : RockDateTime.Now.SundayDate();
            var maxDate = isDaily ? iterationEndDate : iterationEndDate.SundayDate();
            var minDate = isDaily ? iterationStartDate : iterationStartDate.SundayDate();

            if ( maxDate > currentDate )
            {
                errorMessage = "The max date cannot be in the future";
                return;
            }

            if ( minDate < mapStartDate )
            {
                errorMessage = "The min date cannot be before the map start date";
                return;
            }

            if ( minDate > maxDate )
            {
                errorMessage = "The max date must be after the min date";
                return;
            }

            // Calculate the difference in min date to the map start date
            var slideStartUnitsToFuture = GetFrequencyUnitDifference( mapStartDate, minDate, streakOccurrenceFrequency, false );

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( minDate, maxDate, streakOccurrenceFrequency, true );

            // Prepare to iterate over the bytes
            var currentUnit = 0;
            var initialByteOffset = slideStartUnitsToFuture / BitsPerByte + 1;
            var currentByteBitValue = 1 << ( slideStartUnitsToFuture % BitsPerByte );

            occurrenceMap = occurrenceMap ?? new byte[0];
            var occurrenceMapLength = occurrenceMap.Length;
            var currentOccurrenceByteIndex = occurrenceMapLength - initialByteOffset;
            var currentOccurrenceByte = GetByteFromMap( occurrenceMap, currentOccurrenceByteIndex );

            engagementMap = engagementMap ?? new byte[0];
            var engagementMapLength = engagementMap.Length;
            var currentEngagementByteIndex = engagementMapLength - initialByteOffset;
            var currentEngagementByte = GetByteFromMap( engagementMap, currentEngagementByteIndex );

            exclusionMap = exclusionMap ?? new byte[0];
            var exclusionMapLength = exclusionMap.Length;
            var currentExclusionByteIndex = exclusionMapLength - initialByteOffset;
            var currentExclusionByte = GetByteFromMap( exclusionMap, currentExclusionByteIndex );

            // Loop for each unit expected in the date range
            while ( currentUnit < numberOfFrequencyUnits )
            {
                var hasEngagement = ( currentEngagementByte & currentByteBitValue ) == currentByteBitValue;
                var hasExclusion = ( currentExclusionByte & currentByteBitValue ) == currentByteBitValue;
                var hasOccurrence = ( currentOccurrenceByte & currentByteBitValue ) == currentByteBitValue;

                // Call the action with the iteration variables
                actionPerIteration( currentUnit, hasOccurrence, hasEngagement, hasExclusion );

                // Iterate to the next bit
                currentUnit++;
                currentByteBitValue = currentByteBitValue << 1;

                // If the bit value is beyond the current byte, then increment to the next byte
                if ( currentByteBitValue > byte.MaxValue )
                {
                    currentByteBitValue = 1;

                    currentOccurrenceByteIndex--;
                    currentOccurrenceByte = GetByteFromMap( occurrenceMap, currentOccurrenceByteIndex );

                    currentEngagementByteIndex--;
                    currentEngagementByte = GetByteFromMap( engagementMap, currentEngagementByteIndex );

                    currentExclusionByteIndex--;
                    currentExclusionByte = GetByteFromMap( exclusionMap, currentExclusionByteIndex );
                }
            }
        }

        /// <summary>
        /// Iterates over each bit in the maps over the given timeframe from max to min. The actionPerIteration is called for each bit.
        /// actionPerIteration( currentUnit, hasOccurrence, hasEngagement, hasExclusion );
        /// </summary>
        /// <param name="mapStartDate">The map start date.</param>
        /// <param name="iterationStartDate">The iteration start date.</param>
        /// <param name="iterationEndDate">The iteration end date.</param>
        /// <param name="streakOccurrenceFrequency">The streak occurrence frequency.</param>
        /// <param name="occurrenceMap">The occurrence map.</param>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="exclusionMap">The exclusion map.</param>
        /// <param name="actionPerIteration">The action per iteration.</param>
        /// <param name="errorMessage"></param>
        private static void ReverseIterateMaps( DateTime mapStartDate, DateTime iterationStartDate, DateTime iterationEndDate,
            StreakOccurrenceFrequency streakOccurrenceFrequency, byte[] occurrenceMap, byte[] engagementMap, byte[] exclusionMap,
            Action<int, bool, bool, bool> actionPerIteration, out string errorMessage )
        {
            errorMessage = string.Empty;

            var isDaily = streakOccurrenceFrequency == StreakOccurrenceFrequency.Daily;
            var currentDate = isDaily ? RockDateTime.Today : RockDateTime.Now.SundayDate();
            var maxDate = isDaily ? iterationEndDate : iterationEndDate.SundayDate();
            var minDate = isDaily ? iterationStartDate : iterationStartDate.SundayDate();

            if ( maxDate > currentDate )
            {
                errorMessage = "The max date cannot be in the future";
                return;
            }

            if ( minDate < mapStartDate )
            {
                errorMessage = "The min date cannot be before the map start date";
                return;
            }

            if ( minDate >= maxDate )
            {
                errorMessage = "The max date must be after the min date";
                return;
            }

            // Calculate the difference in min date to the map start date
            var slideStartUnitsToFuture = GetFrequencyUnitDifference( mapStartDate, maxDate, streakOccurrenceFrequency, false );

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( minDate, maxDate, streakOccurrenceFrequency, true );

            // Prepare to iterate over the bytes
            var currentUnit = numberOfFrequencyUnits - 1;
            var initialByteOffset = slideStartUnitsToFuture / BitsPerByte + 1;
            var currentByteBitValue = 1 << ( slideStartUnitsToFuture % BitsPerByte );

            occurrenceMap = occurrenceMap ?? new byte[0];
            var occurrenceMapLength = occurrenceMap.Length;
            var currentOccurrenceByteIndex = occurrenceMapLength - initialByteOffset;
            var currentOccurrenceByte = GetByteFromMap( occurrenceMap, currentOccurrenceByteIndex );

            engagementMap = engagementMap ?? new byte[0];
            var engagementMapLength = engagementMap.Length;
            var currentEngagementByteIndex = engagementMapLength - initialByteOffset;
            var currentEngagementByte = GetByteFromMap( engagementMap, currentEngagementByteIndex );

            exclusionMap = exclusionMap ?? new byte[0];
            var exclusionMapLength = exclusionMap.Length;
            var currentExclusionByteIndex = exclusionMapLength - initialByteOffset;
            var currentExclusionByte = GetByteFromMap( exclusionMap, currentExclusionByteIndex );

            // Loop for each unit expected in the date range
            while ( currentUnit >= 0 )
            {
                var hasEngagement = ( currentEngagementByte & currentByteBitValue ) == currentByteBitValue;
                var hasExclusion = ( currentExclusionByte & currentByteBitValue ) == currentByteBitValue;
                var hasOccurrence = ( currentOccurrenceByte & currentByteBitValue ) == currentByteBitValue;

                // Call the action with the iteration variables
                actionPerIteration( currentUnit, hasOccurrence, hasEngagement, hasExclusion );

                // Iterate to the next bit
                currentUnit--;
                currentByteBitValue = currentByteBitValue >> 1;

                // If the bit value is beyond the current byte, then increment to the next byte
                if ( currentByteBitValue == 0 )
                {
                    currentByteBitValue = 1 << 7;

                    currentOccurrenceByteIndex++;
                    currentOccurrenceByte = GetByteFromMap( occurrenceMap, currentOccurrenceByteIndex );

                    currentEngagementByteIndex++;
                    currentEngagementByte = GetByteFromMap( engagementMap, currentEngagementByteIndex );

                    currentExclusionByteIndex++;
                    currentExclusionByte = GetByteFromMap( exclusionMap, currentExclusionByteIndex );
                }
            }
        }

        #endregion Bit Manipulation
    }

    /// <summary>
    /// This data transfer object conveys information about a single streak enrollment as well as streak information that is
    /// calculated based on the streak type occurrence map and streak type exclusion maps.
    /// </summary>
    public class StreakData
    {
        /// <summary>
        /// Gets or sets the streak type identifier.
        /// </summary>
        public int StreakTypeId { get; set; }

        /// <summary>
        /// Gets or sets the streak ids.
        /// </summary>
        public List<int> StreakIds { get; set; }

        /// <summary>
        /// The number of enrollments the person has in the streak type (because of person aliases)
        /// </summary>
        public int EnrollmentCount { get; set; }

        /// <summary>
        /// The date that the person enrolled into the streak type
        /// </summary>
        public DateTime? FirstEnrollmentDate { get; set; }

        /// <summary>
        /// The earliest date represented by this data
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The latest date represented by this data
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The sequence of bits that represent engagement. The least significant bit (rightmost) represents the start date.
        /// </summary>
        public string EngagementMap { get; set; }

        /// <summary>
        /// The sequence of bits that represent occurrences where engagement was possible.  The least significant bit (rightmost)
        /// represents the start date.
        /// </summary>
        public string OccurrenceMap { get; set; }

        /// <summary>
        /// The sequence of bits that represent exclusions.  The least significant bit (rightmost) represents the start date.
        /// </summary>
        public string ExclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (<see cref="Rock.Model.StreakOccurrenceFrequency"/>).
        /// </summary>
        public StreakOccurrenceFrequency OccurrenceFrequency { get; set; }

        /// <summary>
        /// The date that the current streak began
        /// </summary>
        public DateTime? CurrentStreakStartDate { get; set; }

        /// <summary>
        /// The current number of non excluded occurrences attended in a row
        /// </summary>
        public int CurrentStreakCount { get; set; }

        /// <summary>
        /// The date the longest streak began
        /// </summary>
        public DateTime? LongestStreakStartDate { get; set; }

        /// <summary>
        /// The date the longest streak ended
        /// </summary>
        public DateTime? LongestStreakEndDate { get; set; }

        /// <summary>
        /// The longest number of non excluded occurrences attended in a row
        /// </summary>
        public int LongestStreakCount { get; set; }

        /// <summary>
        /// The number of occurrences within the date range
        /// </summary>
        public int OccurrenceCount { get; set; }

        /// <summary>
        /// The number of engagements on occurrences within the date range
        /// </summary>
        public int EngagementCount { get; set; }

        /// <summary>
        /// The number of absences on occurrences within the date range
        /// </summary>
        public int AbsenceCount { get; set; }

        /// <summary>
        /// The number of excluded absences on occurrences within the date range
        /// </summary>
        public int ExcludedAbsenceCount { get; set; }

        /// <summary>
        /// A list of object representing days or weeks from start to end containing the date and its engagement, occurrence, and
        /// exclusion data.
        /// </summary>
        public List<FrequencyUnitData> PerFrequencyUnit { get; set; }

        /// <summary>
        /// The number of milliseconds that this calculation took
        /// </summary>
        public double? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// The object representing a single day or week (frequency unit) in a streak
        /// </summary>
        public class FrequencyUnitData
        {
            /// <summary>
            /// The day or week represented
            /// </summary>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// Did the person have engagement?
            /// </summary>
            public bool HasEngagement { get; set; }

            /// <summary>
            /// Did the streak type have an occurrence?
            /// </summary>
            public bool HasOccurrence { get; set; }

            /// <summary>
            /// Did the streak type have an exclusion?
            /// </summary>
            public bool HasExclusion { get; set; }
        }
    }

    /// <summary>
    /// Class representing an occurrence within a streak type to determine the date of that occurrence and also if the person had engagement at that time.
    /// Lists of this class are useful for driving a binary state graph.
    /// </summary>
    public class OccurrenceEngagement
    {
        /// <summary>
        /// The day or week represented
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Did the person have engagement?
        /// </summary>
        public bool HasEngagement { get; set; }
    }
}