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
    /// Service/Data access class for <see cref="Sequence"/> entity objects.
    /// </summary>
    public partial class SequenceService
    {
        #region Constants

        /// <summary>
        /// Days per week
        /// </summary>
        private const int DaysPerWeek = 7;

        /// <summary>
        /// Bits per byte
        /// </summary>
        private const int BitsPerByte = 8;        

        /// <summary>
        /// The minimum size of a byte array for a sequence related map. The point is to reduce memory reallocations which
        /// are costly, by starting and growing by this size. 128 bytes is trivial memory, but has 1024 bits, which is enough to represent
        /// almost 3 years worth of daily sequence data.
        /// </summary>
        private const int MapByteGrowthCount = 128;

        /// <summary>
        /// The number of hex chars per byte
        /// </summary>
        private const int HexDigitsPerByte = 2;

        /// <summary>
        /// The number of possible values per single hex digit
        /// </summary>
        private const int Base16 = 16;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Get the most recent engagement bits for the person
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="personId"></param>
        /// <param name="unitCount"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool[] GetRecentEngagementBits( int sequenceId, int personId, int unitCount, out string errorMessage )
        {
            errorMessage = string.Empty;

            var sequence = SequenceCache.Get( sequenceId );

            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return null;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return null;
            }

            if ( unitCount < 1 )
            {
                unitCount = 24;
            }

            var rockContext = Context as RockContext;
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );

            var enrollmentMaps = sequenceEnrollmentService.GetBySequenceAndPerson( sequenceId, personId )
                .AsNoTracking()
                .Select( se => se.EngagementMap )
                .ToArray();

            var enrollmentMap = GetAggregateMap( enrollmentMaps );
            return GetMostRecentBits( enrollmentMap, sequence.StartDate, sequence.OccurrenceFrequency, unitCount );
        }

        /// <summary>
        /// Enroll the person into the sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="personId"></param>
        /// <param name="locationId"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public SequenceEnrollment Enroll( SequenceCache sequence, int personId, out string errorMessage,
            DateTime? enrollmentDate = null, int? locationId = null )
        {
            errorMessage = string.Empty;

            // Validate the parameters
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return null;
            }

            if ( personId == default( int ) )
            {
                errorMessage = "A valid personId is required";
                return null;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return null;
            }

            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var maxDate = RockDateTime.Today;
            var minDate = sequence.StartDate;
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
                errorMessage = "The enrollmentDate cannot be before the sequence began";
                return null;
            }

            // Make sure the enrollment does not already exist for the person
            var rockContext = Context as RockContext;
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
            var alreadyEnrolled = sequenceEnrollmentService.IsEnrolled( sequence.Id, personId );

            if ( alreadyEnrolled )
            {
                errorMessage = "The enrollment already exists";
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
            var sequenceEnrollment = new SequenceEnrollment
            {
                SequenceId = sequence.Id,
                PersonAliasId = personAliasId.Value,
                LocationId = locationId,
                EnrollmentDate = enrollmentDate.Value,
                EngagementMap = AllocateNewByteArray( sequence.OccurrenceMap?.Length )
            };

            sequenceEnrollmentService.Add( sequenceEnrollment );
            return sequenceEnrollment;
        }

        /// <summary>
        /// Return the locations associated with the sequence structure type
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public IQueryable<Location> GetLocations( SequenceCache sequence, out string errorMessage )
        {
            errorMessage = string.Empty;

            var defaultReturnValue = new List<Location>().AsQueryable();
            var errorReturnValue = ( IQueryable<Location> ) null;

            // Validate the parameters
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return errorReturnValue;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return errorReturnValue;
            }

            // If the structure information is not complete, it is not possible to get locations
            if ( !sequence.StructureEntityId.HasValue || !sequence.StructureType.HasValue )
            {
                return defaultReturnValue;
            }

            // Calculate the group locations depending on the structure type
            var groupLocationsQuery = GetGroupLocationsQuery( sequence.StructureType.Value, sequence.StructureEntityId.Value );
            return groupLocationsQuery.Select( gl => gl.Location )
                .DistinctBy( l => l.Id )
                .AsQueryable();
        }

        /// <summary>
        /// Get the schedules for the sequence at the given location
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="locationId"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public IQueryable<Schedule> GetLocationSchedules( SequenceCache sequence, int locationId, out string errorMessage )
        {
            errorMessage = string.Empty;

            var defaultReturnValue = new List<Schedule>().AsQueryable();
            var errorReturnValue = ( IQueryable<Schedule> ) null;

            // Validate the parameters
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return errorReturnValue;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return errorReturnValue;
            }

            // If the structure information is not complete, it is not possible to get locations
            if ( !sequence.StructureEntityId.HasValue || !sequence.StructureType.HasValue )
            {
                return defaultReturnValue;
            }

            // Calculate the schedules for the group locations within the structure
            var groupLocationsQuery = GetGroupLocationsQuery( sequence.StructureType.Value, sequence.StructureEntityId.Value );
            return groupLocationsQuery.Where( gl => gl.LocationId == locationId )
                .SelectMany( gl => gl.Schedules )
                .Where( s => s.IsActive )
                .DistinctBy( s => s.Id )
                .AsQueryable();
        }

        /// <summary>
        /// Rebuild the sequence occurrence map and enrollment maps from the attendance structure of the sequence.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="errorMessage"></param>
        public static void RebuildSequenceAndEnrollmentsFromAttendance( int sequenceId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();
            var sequenceService = new SequenceService( rockContext );
            var sequence = sequenceService.Get( sequenceId );

            // Validate the parameters
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return;
            }

            // Get the occurrences that did occur
            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var occurrenceQuery = occurrenceService.Queryable()
                .AsNoTracking()
                .Where( ao => ao.DidNotOccur != true );

            // If the structure information is set, then limit the occurrences by the matching groups
            if ( sequence.StructureType.HasValue && sequence.StructureEntityId.HasValue )
            {
                var groupQuery = GetGroupsQuery( rockContext, sequence.StructureType.Value, sequence.StructureEntityId.Value );
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

            // Set the sequence occurrence map according to the dates returned
            sequence.StartDate = occurrenceDates.First();
            var occurrenceMap = AllocateNewByteArray();

            for ( var i = 0; i < numberOfOccurrences; i++ )
            {
                occurrenceMap = SetBit( occurrenceMap, sequence.StartDate, occurrenceDates[i], sequence.OccurrenceFrequency, true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            sequence.OccurrenceMap = occurrenceMap;
            rockContext.SaveChanges();

            // Get all of the attendees for the sequence
            var personAliasIds = occurrenceQuery
                .SelectMany( ao => ao.Attendees.Where( a => a.DidAttend == true && a.PersonAliasId.HasValue ) )
                .Select( a => a.PersonAliasId.Value )
                .Distinct();

            foreach ( var personAliasId in personAliasIds )
            {
                RebuildEnrollmentFromAttendance( sequenceId, personAliasId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Rebuild the enrollment map from the attendance structure of the sequence.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="errorMessage"></param>
        public static void RebuildEnrollmentFromAttendance( int sequenceId, int personAliasId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var sequenceService = new SequenceService( rockContext );
            var enrollmentService = new SequenceEnrollmentService( rockContext );
            var attendanceService = new AttendanceService( rockContext );

            // Validate the parameters
            var sequence = sequenceService.Get( sequenceId );

            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return;
            }

            // Get the attendance that did occur
            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var startDate = isDaily ? sequence.StartDate.Date : sequence.StartDate.SundayDate();

            var attendanceQuery = attendanceService.Queryable().AsNoTracking().Where( a =>
                a.PersonAliasId == personAliasId &&
                a.DidAttend == true &&
                a.Occurrence.DidNotOccur != true && (
                    ( isDaily && a.Occurrence.OccurrenceDate >= startDate ) ||
                    ( !isDaily && a.Occurrence.SundayDate >= startDate )
                ) );

            // If the structure information is set, then limit the attendances by the matching groups
            if ( sequence.StructureType.HasValue && sequence.StructureEntityId.HasValue )
            {
                var groupQuery = GetGroupsQuery( rockContext, sequence.StructureType.Value, sequence.StructureEntityId.Value );
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
            var enrollment = enrollmentService.Queryable().FirstOrDefault( se =>
                se.SequenceId == sequenceId
                && se.PersonAliasId == personAliasId );

            // Create the enrollment if needed
            if ( enrollment == null )
            {
                enrollment = new SequenceEnrollment
                {
                    SequenceId = sequenceId,
                    PersonAliasId = personAliasId,
                    EnrollmentDate = enrollmentDate
                };

                enrollmentService.Add( enrollment );
            }

            // Create a new map matching the length of the occurrence map
            var occurrenceMapLength = sequence.OccurrenceMap == null ? 0 : sequence.OccurrenceMap.Length;
            var engagementMap = new byte[occurrenceMapLength];

            // Loop over each date attended and set the bit corresponding to that date
            for ( var dateIndex = 0; dateIndex < attendanceDateCount; dateIndex++ )
            {
                engagementMap = SetBit( engagementMap, sequence.StartDate, datesAttended[dateIndex], sequence.OccurrenceFrequency, true,
                    out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            enrollment.EngagementMap = engagementMap;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Calculate sequence enrollment data and the streaks it represents
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="personId"></param>
        /// <param name="startDate">Defaults to the sequence start date</param>
        /// <param name="endDate">Defaults to now</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public SequenceStreakData GetSequenceStreakData( SequenceCache sequence, int personId, out string errorMessage,
        DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false )
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            errorMessage = string.Empty;

            // Validate the sequence
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return null;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return null;
            }

            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var maxDate = isDaily ? RockDateTime.Today : RockDateTime.Today.SundayDate();
            var minDate = isDaily ? sequence.StartDate.Date : sequence.StartDate.SundayDate();

            // Apply default values to parameters
            if ( !startDate.HasValue )
            {
                startDate = minDate;
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

            if ( startDate < minDate )
            {
                errorMessage = "StartDate cannot be before the sequence began";
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
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
            var sequenceEnrollments = sequenceEnrollmentService.GetBySequenceAndPerson( sequence.Id, personId ).AsNoTracking().ToList();
            var locationId = sequenceEnrollments.FirstOrDefault()?.LocationId;
            var enrollmentDate = sequenceEnrollments.Any() ? sequenceEnrollments.Min( se => se.EnrollmentDate ) : ( DateTime? ) null;

            if ( !enrollmentDate.HasValue )
            {
                startDate = maxDate;
            }
            else if ( enrollmentDate > startDate )
            {
                startDate = enrollmentDate;
            }

            // Calculate the difference in start dates from the parameter to what these maps are based upon
            var slideStartUnitsToFuture = GetFrequencyUnitDifference( minDate, startDate.Value, sequence.OccurrenceFrequency, false );

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( startDate.Value, endDate.Value, sequence.OccurrenceFrequency, true );

            // Calculate the aggregate engagement map, which are all of the engagement maps ORed together
            var engagementMaps = sequenceEnrollments.Where( se => se.EngagementMap != null ).Select( se => se.EngagementMap ).ToArray();
            var aggregateEngagementMap = GetAggregateMap( engagementMaps );

            // Calculate the aggregate exclusion map, which are all of the exclusion maps ORed together
            var exclusionMaps = sequence.SequenceOccurrenceExclusions
                .Where( soe => soe.LocationId == locationId && soe.ExclusionMap != null )
                .Select( soe => soe.ExclusionMap )
                .ToArray();

            var aggregateExclusionMap = GetAggregateMap( exclusionMaps );

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

            var objectArray = createObjectArray ? new List<SequenceStreakData.FrequencyUnitData>( numberOfFrequencyUnits ) : null;

            // Prepare to iterate over the bytes
            var currentUnit = 0;
            var initialByteOffset = slideStartUnitsToFuture / BitsPerByte + 1;
            var currentByteBitValue = 1 << ( slideStartUnitsToFuture % BitsPerByte );

            var occurrenceMap = sequence.OccurrenceMap ?? new byte[0];
            var occurrenceMapLength = occurrenceMap.Length;
            var currentOccurrenceByteIndex = occurrenceMapLength - initialByteOffset;
            var currentOccurrenceByte = GetByteFromMap( occurrenceMap, currentOccurrenceByteIndex );

            var engagementMapLength = aggregateEngagementMap.Length;
            var currentEngagementByteIndex = engagementMapLength - initialByteOffset;
            var currentEngagementByte = GetByteFromMap( aggregateEngagementMap, currentEngagementByteIndex );

            var exclusionMapLength = aggregateExclusionMap.Length;
            var currentExclusionByteIndex = exclusionMapLength - initialByteOffset;
            var currentExclusionByte = GetByteFromMap( aggregateExclusionMap, currentExclusionByteIndex );

            // Loop for each unit expected in the date range
            while ( currentUnit < numberOfFrequencyUnits )
            {
                var hasEngagement = ( currentEngagementByte & currentByteBitValue ) == currentByteBitValue;
                var hasExclusion = ( currentExclusionByte & currentByteBitValue ) == currentByteBitValue;
                var hasOccurrence = ( currentOccurrenceByte & currentByteBitValue ) == currentByteBitValue;

                if ( hasOccurrence )
                {
                    occurrenceCount++;

                    if ( hasEngagement )
                    {
                        engagementCount++;

                        // If starting a new streak, record the date
                        if ( currentStreak == 0 )
                        {
                            currentStreakStartDate = CalculateDateFromOffset( startDate.Value, currentUnit, sequence.OccurrenceFrequency );
                        }

                        // Count this engagement toward the current streak
                        currentStreak++;

                        // If this is now the longest streak, update the longest counters
                        if ( currentStreak > longestStreak )
                        {
                            longestStreak = currentStreak;
                            longestStreakStartDate = currentStreakStartDate;
                            longestStreakEndDate = CalculateDateFromOffset( startDate.Value, currentUnit, sequence.OccurrenceFrequency );
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
                    objectArray.Add( new SequenceStreakData.FrequencyUnitData
                    {
                        DateTime = CalculateDateFromOffset( startDate.Value, currentUnit, sequence.OccurrenceFrequency ),
                        HasEngagement = hasEngagement,
                        HasExclusion = hasExclusion,
                        HasOccurrence = hasOccurrence
                    } );
                }

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
                    currentEngagementByte = GetByteFromMap( aggregateEngagementMap, currentEngagementByteIndex );

                    currentExclusionByteIndex--;
                    currentExclusionByte = GetByteFromMap( aggregateExclusionMap, currentExclusionByteIndex );
                }
            }

            // Create the return object
            var data = new SequenceStreakData
            {
                EnrollmentCount = sequenceEnrollments.Count,
                FirstEnrollmentDate = enrollmentDate,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                OccurrenceMap = includeBitMaps ? GetHexDigitStringFromMap( occurrenceMap ) : null,
                ExclusionMap = includeBitMaps ? GetHexDigitStringFromMap( aggregateExclusionMap ) : null,
                EngagementMap = includeBitMaps ? GetHexDigitStringFromMap( aggregateEngagementMap ) : null,
                OccurrenceFrequency = sequence.OccurrenceFrequency,
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
        /// Calculate sequence enrollment data and the streaks it represents
        /// </summary>
        /// <param name="sequenceEnrollmentId"></param>
        /// <param name="startDate">Defaults to the sequence start date</param>
        /// <param name="endDate">Defaults to now</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public SequenceStreakData GetSequenceStreakData( int sequenceEnrollmentId, out string errorMessage,
        DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false )
        {
            errorMessage = string.Empty;
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );

            var sequenceEnrollment = sequenceEnrollmentService.Get( sequenceEnrollmentId );
            if ( sequenceEnrollment == null )
            {
                errorMessage = "A valid sequence enrollment is required";
                return null;
            }

            var personId = personAliasService.GetPersonId( sequenceEnrollment.PersonAliasId );
            if ( !personId.HasValue )
            {
                errorMessage = "A valid person ID is required";
                return null;
            }

            var sequenceCache = SequenceCache.Get( sequenceEnrollment.SequenceId );
            return GetSequenceStreakData( sequenceCache, personId.Value, out errorMessage, startDate, endDate, createObjectArray, includeBitMaps );
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the SequenceEnrollment map and also
        /// Attendance (if enabled).
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="personId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement">Defaults to today</param>
        /// <param name="groupId">This is required for marking attendance unless the sequence is a group structure type</param>
        /// <param name="locationId"></param>
        /// <param name="scheduleId"></param>
        /// <param name="addOrUpdateAttendanceRecord">Should this method add or create <see cref="Attendance"/> models?</param>
        public void MarkEngagement( SequenceCache sequence, int personId, out string errorMessage,
            DateTime? dateOfEngagement = null, int? groupId = null, int? locationId = null, int? scheduleId = null,
            bool addOrUpdateAttendanceRecord = true )
        {
            errorMessage = string.Empty;

            // Validate the sequence
            if ( sequence == null )
            {
                errorMessage = "A valid sequence is required";
                return;
            }

            if ( !sequence.IsActive )
            {
                errorMessage = "An active sequence is required";
                return;
            }

            // Override the group id if the sequence is explicit about the group
            if ( sequence.StructureType == SequenceStructureType.Group && sequence.StructureEntityId.HasValue )
            {
                groupId = sequence.StructureEntityId;
            }

            // Apply default values to parameters
            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            dateOfEngagement = ( dateOfEngagement ?? RockDateTime.Now ).Date;
            var minDate = sequence.StartDate.Date;

            if ( !isDaily )
            {
                dateOfEngagement = dateOfEngagement.Value.SundayDate();
                minDate = minDate.SundayDate();
            }

            // Validate the engagement date
            if ( dateOfEngagement < minDate )
            {
                errorMessage = "Cannot mark engagement before the sequence began";
                return;
            }

            // Get the enrollment if it exists. The first enrollment is fine, since when streaks are calculated the maps are combined
            var rockContext = Context as RockContext;
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
            var sequenceEnrollment = sequenceEnrollmentService.GetBySequenceAndPerson( sequence.Id, personId ).FirstOrDefault();

            if ( sequenceEnrollment == null && sequence.RequiresEnrollment )
            {
                errorMessage = "This sequence requires enrollment";
                return;
            }

            if ( sequenceEnrollment == null )
            {
                // Enroll the person since they are marking engagement and enrollment is not required
                sequenceEnrollment = Enroll( sequence, personId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                if ( sequenceEnrollment == null )
                {
                    errorMessage = "The enrollment was not successful but no error was specified";
                    return;
                }
            }

            // Mark engagement on the enrollment map
            sequenceEnrollment.EngagementMap = SetBit( sequenceEnrollment.EngagementMap, sequence.StartDate, dateOfEngagement.Value,
                sequence.OccurrenceFrequency, true, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            // Ensure the occurrence bit is set on the sequence model
            var sequenceService = new SequenceService( rockContext );
            var sequenceModel = sequenceService.Get( sequence.Id );

            sequenceModel.OccurrenceMap = SetBit( sequenceModel.OccurrenceMap, sequenceModel.StartDate, dateOfEngagement.Value,
                sequenceModel.OccurrenceFrequency, true, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            // Entity Framework cannot detect in-place changes to byte arrays, so force set the properties to modified state
            rockContext.Entry( sequenceEnrollment ).Property( se => se.EngagementMap ).IsModified = true;
            rockContext.Entry( sequenceModel ).Property( s => s.OccurrenceMap ).IsModified = true;

            // If attendance is enabled then update attendance models
            if ( sequence.EnableAttendance && addOrUpdateAttendanceRecord )
            {
                Task.Run( () =>
                {
                    var asyncRockContext = new RockContext();
                    var attendanceService = new AttendanceService( asyncRockContext );

                    // Add or update the attendance, but don't sync sequences since that would create a logic loop
                    attendanceService.AddOrUpdate( sequenceEnrollment.PersonAliasId, dateOfEngagement.Value, groupId, locationId,
                        scheduleId, null, null, null, null, null, null, null,
                        syncMatchingSequences: false );

                    asyncRockContext.SaveChanges();
                } );
            }
        }

        /// <summary>
        /// When an attendance record is created or modified (example: check-in), the method should be called to synchronize that
        /// attendance to any matching sequences and enrollments using this method.
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
                // If DidAttend is false, then don't do anything for the sequence. We should not unset the bit for the day/week because
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

            // Get the person's sequence enrollments
            var personId = person.Id;
            var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
            var enrolledInSequenceIdQuery = sequenceEnrollmentService.Queryable()
                .AsNoTracking()
                .Where( se => se.PersonAlias.PersonId == personId )
                .Select( se => se.SequenceId );
            var enrolledInSequenceIds = new HashSet<int>( enrolledInSequenceIdQuery );

            // Calculate the attendance group details
            var groupId = occurrence.GroupId;
            var groupTypeId = occurrence.Group?.GroupTypeId;
            var purposeId = occurrence.Group?.GroupType.GroupTypePurposeValueId;

            var checkInConfigIdList = occurrence.Group?.GroupType.ParentGroupTypes.Select( pgt => pgt.Id );
            var checkInConfigIds = checkInConfigIdList == null ? new HashSet<int>() : new HashSet<int>( checkInConfigIdList );

            // Loop through each active sequence that has attendance enabled and mark engagement for it if the person
            // is enrolled or the sequence does not require enrollment
            var matchedSequences = SequenceCache.All().Where( s =>
                s.IsActive &&
                s.EnableAttendance &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInSequenceIds.Contains( s.Id )
                ) &&
                (
                    !s.StructureType.HasValue ||
                    (
                        s.StructureEntityId.HasValue &&
                        (
                            ( s.StructureType == SequenceStructureType.Group && s.StructureEntityId.Value == groupId ) ||
                            ( s.StructureType == SequenceStructureType.GroupType && s.StructureEntityId.Value == groupTypeId ) ||
                            ( s.StructureType == SequenceStructureType.GroupTypePurpose && s.StructureEntityId.Value == purposeId ) ||
                            ( s.StructureType == SequenceStructureType.CheckInConfig && checkInConfigIds.Contains( s.StructureEntityId.Value ) )
                        )
                    )
                ) );

            foreach ( var sequence in matchedSequences )
            {
                MarkEngagement( sequence, personId, out errorMessage, occurrence.OccurrenceDate, addOrUpdateAttendanceRecord: false );
            }
        }

        /// <summary>
        /// This convenience method calls <see cref="HandleAttendanceRecord"/> in an asynchronous fashion such that the calling
        /// process can continue uninhibited. Use this where the sequence and enrollments should be synchronized, but the calling
        /// process should continue quickly and without regard to the success of this operation. This method creates it's own data
        /// context and saves the changes when complete.
        /// </summary>
        /// <param name="attendance"></param>
        public static void HandleAttendanceRecordAsync( Attendance attendance )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var service = new SequenceService( rockContext );
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
        /// in an asynchronous fashion such that the calling process can continue uninhibited. Use this where the sequence and enrollments 
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
        /// Get the name of the sequence's attendance structure
        /// </summary>
        /// <returns></returns>
        public string GetStructureName( SequenceStructureType? structureType, int? structureEntityId )
        {
            if ( !structureType.HasValue || !structureEntityId.HasValue )
            {
                return null;
            }

            var rockContext = Context as RockContext;

            switch ( structureType )
            {
                case SequenceStructureType.GroupType:
                case SequenceStructureType.CheckInConfig:
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.Get( structureEntityId.Value );
                    return groupType?.Name;
                case SequenceStructureType.Group:
                    var groupService = new GroupService( rockContext );
                    var group = groupService.Get( structureEntityId.Value );
                    return group?.Name;
                case SequenceStructureType.GroupTypePurpose:
                    var groupTypePurpose = DefinedTypeCache.Get( SystemGuid.DefinedType.GROUPTYPE_PURPOSE );
                    var purposeValue = groupTypePurpose?.DefinedValues.FirstOrDefault( dv => dv.Id == structureEntityId.Value );
                    return purposeValue?.Value;
                default:
                    throw new NotImplementedException( string.Format( "Getting structure name for the SequenceStructureType '{0}' is not implemented", structureType ) );
            }
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Get the most recent bits from a map (7 units (days/weeks) ago to today)
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mapStartDate"></param>
        /// <param name="sequenceOccurrenceFrequency"></param>
        /// <param name="unitCount"></param>
        /// <returns></returns>
        public static bool[] GetMostRecentBits( byte[] map, DateTime mapStartDate, SequenceOccurrenceFrequency sequenceOccurrenceFrequency, int unitCount = 24 )
        {
            if ( unitCount < 1 )
            {
                return null;
            }

            // If there is no data then return as if all unset bits
            if ( map == null || map.Length == 0 )
            {
                return new bool[unitCount];
            }

            var isDaily = sequenceOccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var currentDate = isDaily ? RockDateTime.Today : RockDateTime.Today.SundayDate();
            var bits = new bool[ unitCount ];

            for ( var i = 0; i < unitCount; i++ )
            {
                var isBitSet = IsBitSet( map, mapStartDate, currentDate, sequenceOccurrenceFrequency, out var errorMessage );
                bits[i] = isBitSet;
                currentDate = currentDate.AddDays( 0 - ( isDaily ? 1 : DaysPerWeek ) );
            }

            return bits;
        }

        /// <summary>
        /// Start an async task to calculate steak data and then copy it to the enrollment model
        /// </summary>
        /// <param name="sequenceId"></param>
        public static void UpdateEnrollmentStreakPropertiesAsync( int sequenceId )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
                var enrollmentIds = sequenceEnrollmentService.Queryable().AsNoTracking()
                    .Where( se => se.SequenceId == sequenceId )
                    .Select( se => se.Id );

                foreach ( var enrollmentId in enrollmentIds )
                {
                    SequenceEnrollmentService.UpdateStreakPropertiesAsync( enrollmentId );
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
        public static bool IsBitSet( byte[] map, DateTime mapStartDate, DateTime bitDate, SequenceOccurrenceFrequency occurrenceFrequency, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( map == null || map.Length == 0 )
            {
                return false;
            }

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the sequence begins";
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
        /// rockContext.Entry( sequenceModel ).Property( s => s.OccurrenceMap ).IsModified = true;
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mapStartDate"></param>
        /// <param name="bitDate"></param>
        /// <param name="occurrenceFrequency"></param>
        /// <param name="errorMessage"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static byte[] SetBit( byte[] map, DateTime mapStartDate, DateTime bitDate, SequenceOccurrenceFrequency occurrenceFrequency, bool newValue, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( occurrenceFrequency == SequenceOccurrenceFrequency.Weekly )
            {
                mapStartDate = mapStartDate.SundayDate();
                bitDate = bitDate.SundayDate();
            }

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the sequence begins";
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
        /// <param name="sequenceOccurrenceFrequency"></param>
        /// <returns></returns>
        private DateTime CalculateDateFromOffset( DateTime startDate, int frequencyUnits, SequenceOccurrenceFrequency sequenceOccurrenceFrequency )
        {
            var isDaily = sequenceOccurrenceFrequency == SequenceOccurrenceFrequency.Daily;

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
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="occurrenceFrequency"></param>
        /// <returns></returns>
        public static int GetFrequencyUnitDifference( DateTime startDate, DateTime endDate, SequenceOccurrenceFrequency occurrenceFrequency, bool isInclusive )
        {
            var isDaily = occurrenceFrequency == SequenceOccurrenceFrequency.Daily;

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
        private IQueryable<GroupLocation> GetGroupLocationsQuery( SequenceStructureType structureType, int structureEntityId )
        {
            var groupsQuery = GetGroupsQuery( Context as RockContext, structureType, structureEntityId );
            return groupsQuery.SelectMany( g => g.GroupLocations ).Where( gl => gl.Location.IsActive );
        }

        /// <summary>
        /// Get the groups that are contained with the structure
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="structureEntityId"></param>
        /// <returns></returns>
        private static IQueryable<Group> GetGroupsQuery( RockContext rockContext, SequenceStructureType structureType, int structureEntityId )
        {
            var groupService = new GroupService( rockContext );

            var query = groupService.Queryable()
                .AsNoTracking()
                .Where( g => g.IsActive );

            switch ( structureType )
            {
                case SequenceStructureType.CheckInConfig:
                case SequenceStructureType.GroupType:
                    return query.Where( g =>
                        g.GroupTypeId == structureEntityId ||
                        g.GroupType.ParentGroupTypes.Any( pgt => pgt.Id == structureEntityId ) );
                case SequenceStructureType.Group:
                    return query.Where( g =>
                        g.Id == structureEntityId ||
                        g.ParentGroupId == structureEntityId );
                case SequenceStructureType.GroupTypePurpose:
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
        private byte[] GetAggregateMap( byte[][] maps )
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
        private void OrBitOperation( byte[] a, byte[] b )
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
        private byte GetByteFromMap( byte[] map, int byteIndex )
        {
            if ( byteIndex < 0 || byteIndex >= map.Length )
            {
                return 0;
            }

            return map[byteIndex];
        }

        #endregion Bit Manipulation
    }

    /// <summary>
    /// This data transfer object conveys information about a single sequence enrollment as well as streak information that is
    /// calculated based on the sequence occurrence map and sequence exclusion maps.
    /// </summary>
    public class SequenceStreakData
    {
        /// <summary>
        /// The number of enrollments the person has in the sequence (because of person aliases)
        /// </summary>
        public int EnrollmentCount { get; set; }

        /// <summary>
        /// The date that the person enrolled into the sequence
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
        /// Gets or sets the timespan that each map bit represents (<see cref="Rock.Model.SequenceOccurrenceFrequency"/>).
        /// </summary>
        public SequenceOccurrenceFrequency OccurrenceFrequency { get; set; }

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
            /// Did the sequence have an occurrence?
            /// </summary>
            public bool HasOccurrence { get; set; }

            /// <summary>
            /// Did the sequence have an exclusion?
            /// </summary>
            public bool HasExclusion { get; set; }
        }
    }
}