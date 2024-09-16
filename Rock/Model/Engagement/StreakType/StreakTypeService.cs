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
using System.Threading.Tasks;

using Rock.Data;
using Rock.Field.Types;
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
        private static readonly int DaysPerWeek = 7;

        /// <summary>
        /// Bits per byte
        /// </summary>
        private static readonly int BitsPerByte = 8;

        /// <summary>
        /// The minimum size of a byte array for a streak related map. The point is to reduce memory reallocations which
        /// are costly, by starting and growing by this size. 128 bytes is trivial memory, but has 1024 bits, which is enough to represent
        /// almost 3 years worth of daily streak data.
        /// </summary>
        private static readonly int MapByteGrowthCount = 128;

        /// <summary>
        /// The number of hex chars per byte
        /// </summary>
        private static readonly int HexDigitsPerByte = 2;

        /// <summary>
        /// The number of possible values per single hex digit
        /// </summary>
        private static readonly int Base16 = 16;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="checkIfUsedByAchievementType">if set to <c>true</c> [check if used by achievement type].</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( StreakType item, out string errorMessage, bool checkIfUsedByAchievementType )
        {
            if ( !this.CanDelete( item, out errorMessage ) )
            {
                return false;
            }

            if ( checkIfUsedByAchievementType )
            {
                // check if any MatrixAttributes are using this AttributeMatrixTemplate
                var streakTypeFieldTypeId = FieldTypeCache.Get<StreakTypeFieldType>().Id;
                var entityTypeIdAchievementType = EntityTypeCache.GetId<Rock.Model.AchievementType>();

                var streakTypeGuid = item.Guid.ToString();
                var usedAsAchievementType = new AttributeValueService( new RockContext() ).Queryable()
                    .Any( av =>
                        av.Attribute.FieldTypeId == streakTypeFieldTypeId
                        && av.Value == streakTypeGuid
                        && av.Attribute.EntityTypeId == entityTypeIdAchievementType );
                if ( usedAsAchievementType )
                {
                    errorMessage = string.Format( "This {0} is assigned to an {1}.", StreakType.FriendlyTypeName, Rock.Model.AchievementType.FriendlyTypeName );
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

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

            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

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

            if ( unitCount < 1 )
            {
                unitCount = 24;
            }

            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );

            var streaks = streakService.GetByStreakTypeAndPerson( streakTypeId, personId ).AsNoTracking().ToList();
            var engagementMaps = streaks.Select( se => se.EngagementMap ).ToArray();
            var locationId = streaks.FirstOrDefault()?.LocationId;
            var aggregateExclusionMap = GetAggregateExclusionMap( streaks, streakTypeCache, locationId );
            var engagementMap = GetAggregateMap( engagementMaps );

            return GetMostRecentOccurrences( streakTypeCache, engagementMap, aggregateExclusionMap, unitCount );
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

            if ( personId == default )
            {
                errorMessage = "A valid personId is required";
                return null;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return null;
            }

            var maxDate = AlignDate( RockDateTime.Today, streakTypeCache );
            var minDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );
            enrollmentDate = AlignDate( enrollmentDate ?? maxDate, streakTypeCache );

            if ( enrollmentDate.Value > maxDate )
            {
                errorMessage = "The enrollment date cannot be in the future.";
                return null;
            }

            if ( enrollmentDate.Value < minDate )
            {
                errorMessage = $"The enrollment date cannot be before the streak type start date, {minDate.ToShortDateString()}.";
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
            var personAliasId = personAliasService.GetPrimaryAliasId( personId );

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

            // If the structure information is not complete, open to anything, or interaction related, it is not possible to get locations
            if ( !streakTypeCache.StructureEntityId.HasValue ||
                !streakTypeCache.StructureType.HasValue ||
                streakTypeCache.StructureType == StreakStructureType.AnyAttendance ||
                streakTypeCache.StructureType == StreakStructureType.InteractionChannel ||
                streakTypeCache.StructureType == StreakStructureType.InteractionComponent ||
                streakTypeCache.StructureType == StreakStructureType.InteractionMedium )
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

            // If the structure information is not complete, open to anything, or interaction related, it is not possible to get locations
            if ( !streakTypeCache.StructureEntityId.HasValue ||
                !streakTypeCache.StructureType.HasValue ||
                streakTypeCache.StructureType == StreakStructureType.AnyAttendance ||
                streakTypeCache.StructureType == StreakStructureType.InteractionChannel ||
                streakTypeCache.StructureType == StreakStructureType.InteractionComponent ||
                streakTypeCache.StructureType == StreakStructureType.InteractionMedium )
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
        [Obsolete( "Use the overload with Progress instead", true )]
        [RockObsolete( "1.10" )]
        public static void RebuildStreakTypeFromAttendance( int streakTypeId, out string errorMessage )
        {
            RebuildStreakTypeFromAttendance( null, streakTypeId, out errorMessage );
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the linked activity structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="progress">Optional (using null is fine)</param>
        /// <param name="streakTypeId"></param>
        /// <param name="errorMessage"></param>
        public static void RebuildStreakType( IProgress<int?> progress, int streakTypeId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            // Validate the parameters
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type cache is required";
                return;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            if ( !streakTypeCache.StructureType.HasValue )
            {
                errorMessage = "A streak type linked activity structure is required";
                return;
            }

            if ( streakTypeCache.StructureType != StreakStructureType.AnyAttendance && !streakTypeCache.StructureEntityId.HasValue )
            {
                errorMessage = "A streak type linked activity entity id is required";
                return;
            }

            switch ( streakTypeCache.StructureType.Value )
            {
                case StreakStructureType.AnyAttendance:
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.Group:
                case StreakStructureType.GroupType:
                case StreakStructureType.GroupTypePurpose:
                    RebuildStreakTypeFromAttendance( progress, streakTypeCache, out errorMessage );
                    break;
                case StreakStructureType.InteractionChannel:
                case StreakStructureType.InteractionComponent:
                case StreakStructureType.InteractionMedium:
                    RebuildStreakTypeFromInteraction( progress, streakTypeCache, out errorMessage );
                    break;
                case StreakStructureType.FinancialTransaction:
                    RebuildStreakTypeFromFinancialTransaction( progress, streakTypeCache, out errorMessage );
                    break;
                default:
                    errorMessage = $"The streak type structure {streakTypeCache.StructureType.Value} is not supported";
                    break;
            }
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the attendance structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        [Obsolete( "Use the RebuildStreakType method instead", true )]
        [RockObsolete( "1.10" )]
        public static void RebuildStreakTypeFromAttendance( IProgress<int?> progress, int streakTypeId, out string errorMessage )
        {
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            // Validate the parameters
            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type cache is required";
                return;
            }

            if ( !streakTypeCache.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            if ( !streakTypeCache.StructureType.HasValue )
            {
                errorMessage = "A streak type linked activity structure is required";
                return;
            }

            if ( streakTypeCache.StructureType != StreakStructureType.AnyAttendance && !streakTypeCache.StructureEntityId.HasValue )
            {
                errorMessage = "A streak type linked activity entity id is required";
                return;
            }

            RebuildStreakTypeFromAttendance( progress, streakTypeCache, out errorMessage );
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the attendance structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="progress">The progress. Optional (using null is fine)</param>
        /// <param name="streakTypeCache">The streak type.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void RebuildStreakTypeFromAttendance( IProgress<int?> progress, StreakTypeCache streakTypeCache, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeCache.Id );

            // Get the occurrences that did occur
            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var occurrenceQuery = occurrenceService.Queryable()
                .AsNoTracking()
                .Where( ao => ao.DidNotOccur != true &&
                    ao.OccurrenceDate >= streakType.StartDate );

            // If the structure information is set, then limit the occurrences by the matching groups
            if ( streakType.StructureType.HasValue &&
                streakType.StructureEntityId.HasValue &&
                streakType.StructureType != StreakStructureType.AnyAttendance )
            {
                var groupQuery = GetGroupsQuery( rockContext, streakType.StructureType.Value, streakType.StructureEntityId.Value );
                occurrenceQuery = occurrenceQuery.Where( ao => groupQuery.Any( g => g.Id == ao.GroupId ) );
            }

            // Limit the output to the dates according to the frequency
            var occurrenceDates = occurrenceQuery
                .Select( o => o.OccurrenceDate )
                .Distinct()
                .OrderBy( d => d )
                .ToArray();

            var numberOfOccurrences = occurrenceDates.Length;

            if ( numberOfOccurrences < 1 )
            {
                errorMessage = "No attendance occurrences were found.";
                return;
            }

            for ( var i = 0; i < numberOfOccurrences; i++ )
            {
                occurrenceDates[i] = AlignDate( occurrenceDates[i], streakTypeCache );
            }

            // Set the streak type occurrence map according to the dates returned
            var firstOccurrenceDate = occurrenceDates.First();
            streakTypeCache.SetFromEntity( streakType );
            var occurrenceMap = AllocateNewByteArray();

            for ( var i = 0; i < numberOfOccurrences; i++ )
            {
                occurrenceMap = SetBit( streakTypeCache, occurrenceMap, occurrenceDates[i], true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            streakType.OccurrenceMap = occurrenceMap;
            rockContext.SaveChanges();
            streakTypeCache = StreakTypeCache.Get( streakTypeCache.Id );

            // Get all of the attendees for the streak type
            var personIds = occurrenceQuery
                .SelectMany( ao => ao.Attendees.Where( a => a.DidAttend == true && a.PersonAlias != null ) )
                .Select( a => a.PersonAlias.PersonId )
                .Distinct()
                .ToList();

            var totalCount = personIds.LongCount();
            var batchCounter = 0;
            var totalCounter = 0L;

            foreach ( var personId in personIds )
            {
                if ( batchCounter == 0 )
                {
                    rockContext = new RockContext();
                }

                RebuildStreak( rockContext, streakTypeCache, streakType, streakType.StartDate, personId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                batchCounter++;
                totalCounter++;

                if ( batchCounter == 100 )
                {
                    rockContext.SaveChanges();
                    rockContext.Dispose();
                    batchCounter = 0;

                    progress?.Report( ( int ) ( decimal.Divide( totalCounter, totalCount ) * 100 ) );
                }
            }

            if ( batchCounter > 0 )
            {
                rockContext.SaveChanges();
                rockContext.Dispose();
            }
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the linked activity structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="progress">The progress. Optional (using null is fine)</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void RebuildStreakTypeFromInteraction( IProgress<int?> progress, StreakTypeCache streakTypeCache, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeCache.Id );

            // Validate the parameters
            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            // Get the interaction start date
            var interactionQuery = GetInteractionQuery( rockContext, streakType.StructureType.Value, streakType.StructureEntityId.Value )
                .Where( i => i.InteractionDateTime >= streakType.StartDate );

            // Set the streak type occurrence map according to the dates returned
            var firstOccurrenceDate = interactionQuery
                .Select( i => i.InteractionDateTime )
                .OrderBy( d => d )
                .FirstOrDefault();

            streakTypeCache.SetFromEntity( streakType );
            var occurrenceMap = AllocateNewByteArray();

            // Iterate over the map from the start to the end and set all units to 1 because there is an opportunity for an interaction
            // to occur at any time
            var currentDate = streakType.StartDate;
            var maxDate = AlignDate( RockDateTime.Today, streakTypeCache );

            while ( currentDate <= maxDate )
            {
                occurrenceMap = SetBit( streakTypeCache, occurrenceMap, currentDate, true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                currentDate = IncrementDateTime( currentDate, streakTypeCache.OccurrenceFrequency );
            }

            streakType.OccurrenceMap = occurrenceMap;
            rockContext.SaveChanges();
            streakTypeCache = StreakTypeCache.Get( streakTypeCache.Id );

            // Get all of the attendees for the streak type
            var personIds = interactionQuery
                .Select( i => i.PersonAlias.PersonId )
                .Distinct()
                .ToList();

            var totalCount = personIds.LongCount();
            var batchCounter = 0;
            var totalCounter = 0L;

            foreach ( var personId in personIds )
            {
                if ( batchCounter == 0 )
                {
                    rockContext = new RockContext();
                }

                RebuildStreak( rockContext, streakTypeCache, streakType, streakType.StartDate, personId, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                batchCounter++;
                totalCounter++;

                if ( batchCounter == 100 )
                {
                    rockContext.SaveChanges();
                    rockContext.Dispose();
                    batchCounter = 0;

                    progress?.Report( ( int ) ( decimal.Divide( totalCounter, totalCount ) * 100 ) );
                }
            }

            if ( batchCounter > 0 )
            {
                rockContext.SaveChanges();
                rockContext.Dispose();
            }
        }

        /// <summary>
        /// Rebuild the streak map from the attendance structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void RebuildStreakFromAttendance( int streakTypeId, int personId, out string errorMessage )
        {
            var rockContext = new RockContext();

            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeId );
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            if ( streakTypeCache == null )
            {
                errorMessage = "A valid streak type cache is required";
                return;
            }

            if ( !streakType.IsActive )
            {
                errorMessage = "An active streak type is required";
                return;
            }

            // Get the attendance that did occur
            var startDate = AlignDate( streakType.StartDate, streakTypeCache );

            RebuildStreak( rockContext, streakTypeCache, streakType, startDate, personId, out errorMessage );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Rebuild the streak type occurrence map and streak maps from the linked activity structure of the streak type.
        /// This method makes it's own Rock Context and saves changes.
        /// </summary>
        /// <param name="progress">The progress. Optional (using null is fine)</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void RebuildStreakTypeFromFinancialTransaction( IProgress<int?> progress, StreakTypeCache streakTypeCache, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var streakType = streakTypeService.Get( streakTypeCache.Id );

            // Validate the parameters
            if ( streakType == null )
            {
                errorMessage = "A valid streak type is required";
                return;
            }

            // Get the transactions
            var transactionQuery = GetFinancialTransactionQuery( rockContext,
                streakType.StructureType.Value,
                streakType.StructureEntityId.Value,
                streakType.StructureSettings.IncludeChildAccounts )
                .Where( t => t.TransactionDateTime >= streakType.StartDate );

            // Set the streak type occurrence map according to the dates returned
            var firstOccurrenceDate = transactionQuery.Select( t => t.TransactionDateTime ).Min();
            if ( !firstOccurrenceDate.HasValue )
            {
                // No data to work with
                return;
            }

            streakTypeCache.SetFromEntity( streakType );
            var occurrenceMap = AllocateNewByteArray();

            // Iterate over the map from the start to the end and set all units to 1 because there is an opportunity for a transaction
            // to occur at any time
            var currentDate = streakType.StartDate;
            var maxDate = AlignDate( RockDateTime.Today, streakTypeCache );

            while ( currentDate <= maxDate )
            {
                occurrenceMap = SetBit( streakTypeCache, occurrenceMap, currentDate, true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }

                currentDate = IncrementDateTime( currentDate, streakTypeCache.OccurrenceFrequency );
            }

            streakType.OccurrenceMap = occurrenceMap;
            rockContext.SaveChanges();
            streakTypeCache = StreakTypeCache.Get( streakTypeCache.Id );

            // Get all of the attendees for the streak type
            var personIds = transactionQuery.Select( t => t.AuthorizedPersonAlias.PersonId ).Distinct().ToList();

            var totalCount = personIds.LongCount();
            var batchCounter = 0;
            var totalCounter = 0L;

            foreach ( var personId in personIds )
            {
                // Create a new context every 100 persons to keep the change tracker from getting bogged down.
                if ( batchCounter == 0 )
                {
                    rockContext = new RockContext();
                }

                // Get the Person's Giving ID
                var givingId = new PersonService( rockContext ).GetSelect( personId, p => p.GivingId );

                // fetch all of the Person.Id that use the givingId of the current person
                var givingPersonIds = new PersonService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( p => p.GivingId == givingId )
                    .Select( a => a.Id )
                    .ToList();

                foreach ( var pId in givingPersonIds )
                {
                    RebuildStreak( rockContext, streakTypeCache, streakType, streakType.StartDate, pId, out errorMessage );

                    /*
                    * 2023-05-30 ETD
                    * Save changes here in case the this person has not inserted a row into Streak yet.
                    * Otherwise the Context could try to insert the same person for the same StreakType multiple times which will violate unique key index "IX_StreakTypeId_PersonAliasId".
                    * This can happen when a new StreakType is created for a past date where one or more persons has multiple streaks to process (e.g. weekly giving starting 3 weeks ago)
                    */
                    if ( errorMessage.IsNullOrWhiteSpace() )
                    {
                        rockContext.SaveChanges();
                    }
                }

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                batchCounter++;
                totalCounter++;

                if ( batchCounter == 100 )
                {
                    batchCounter = 0;
                    rockContext.Dispose();
                    progress?.Report( ( int ) ( decimal.Divide( totalCounter, totalCount ) * 100 ) );
                }
            }
        }

        /// <summary>
        /// Rebuilds the streak from linked activity (attendance or interactions).
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="streakType">Type of the streak.</param>
        /// <param name="alignedStreakTypeStartDate">The aligned streak type start date.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        private static void RebuildStreak( RockContext rockContext, StreakTypeCache streakTypeCache, StreakType streakType, DateTime alignedStreakTypeStartDate, int personId, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( !streakTypeCache.StructureType.HasValue )
            {
                errorMessage = "A linked activity structure is required";
                return;
            }

            if ( !streakTypeCache.StructureEntityId.HasValue && streakTypeCache.StructureType != StreakStructureType.AnyAttendance )
            {
                errorMessage = "A structure entity id is required";
                return;
            }

            var streakTypeService = new StreakTypeService( rockContext );
            var streakService = new StreakService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
            var personService = new PersonService( rockContext );

            var person = personService.Queryable( "Aliases" ).AsNoTracking().FirstOrDefault( p => p.Id == personId );

            // Validate the parameters
            if ( person == null )
            {
                // This probably happened because the person is deceased
                streakService.DeleteRange(
                    streakService.Queryable().Where( se =>
                        se.StreakTypeId == streakTypeCache.Id &&
                        se.PersonAlias.PersonId == personId ) );
                return;
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                errorMessage = $"The person with id {personId} did not have a primary alias id";
                return;
            }

            // Get the attendance that did occur
            var minDate = alignedStreakTypeStartDate;

            // Walk back the min date to see what actual minimum date is that still aligns to the start date
            // This ensures that all dates that round to a weekly Sunday date are included even if they may be
            // less than the actual start date
            while ( AlignDate( minDate.AddDays( -1 ), streakTypeCache ) == alignedStreakTypeStartDate )
            {
                minDate = minDate.AddDays( -1 );
            }

            // Get the attended dates
            var datesAttended = GetLinkedActivityEngagementDatesQuery( rockContext, streakTypeCache, personId, minDate )
                .OrderBy( d => d )
                .Distinct()
                .ToArray();

            var attendanceDateCount = datesAttended.Length;

            for ( var i = 0; i < attendanceDateCount; i++ )
            {
                datesAttended[i] = AlignDate( datesAttended[i], streakTypeCache );
            }

            var enrollmentDate = attendanceDateCount == 0 ?
                AlignDate( RockDateTime.Now, streakTypeCache ) :
                datesAttended.First();

            // Get the enrollments
            var streaks = streakService.Queryable().Where( se =>
                se.StreakTypeId == streakType.Id
                && se.PersonAlias.PersonId == personId ).ToList();

            // Keep the record that belongs to the person's primary alias. Delete the others.
            var streakToKeep = streaks.FirstOrDefault( s => s.PersonAliasId == person.PrimaryAliasId );
            var streaksToDelete = streaks.Where( s => s.Id != streakToKeep?.Id );

            // Create the enrollment if needed
            if ( streakToKeep == null )
            {
                streakToKeep = new Streak
                {
                    StreakTypeId = streakType.Id,
                    PersonAliasId = person.PrimaryAliasId.Value,
                    EnrollmentDate = enrollmentDate
                };

                streakService.Add( streakToKeep );
            }
            else
            {
                streakToKeep.EnrollmentDate = enrollmentDate;
            }

            if ( streaksToDelete.Any() )
            {
                // Keep all attempts by pointing them to the streak that will be kept
                var streaksToDeleteIds = streaksToDelete.Select( s => s.Id ).ToList();

                // Delete the streaks
                streakService.DeleteRange( streaksToDelete );
            }

            // Create a new map matching the length of the occurrence map
            var occurrenceMapLength = streakType.OccurrenceMap == null ? 0 : streakType.OccurrenceMap.Length;
            var engagementMap = new byte[occurrenceMapLength];

            // Loop over each date attended and set the bit corresponding to that date
            for ( var dateIndex = 0; dateIndex < attendanceDateCount; dateIndex++ )
            {
                engagementMap = SetBit( streakTypeCache, engagementMap, datesAttended[dateIndex], true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }

            streakToKeep.EngagementMap = engagementMap;
        }

        /// <summary>
        /// Gets the linked activity dates query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="minDate"></param>
        /// <returns></returns>
        private static IQueryable<DateTime> GetLinkedActivityEngagementDatesQuery( RockContext rockContext, StreakTypeCache streakTypeCache, int personId, DateTime minDate )
        {
            // Not enough info in these cases to get any dates
            if ( !streakTypeCache.StructureType.HasValue || (
                    streakTypeCache.StructureType != StreakStructureType.AnyAttendance &&
                    !streakTypeCache.StructureEntityId.HasValue ) )
            {
                return new List<DateTime>() as IQueryable<DateTime>;
            }

            // Financial Transactions based query
            if ( streakTypeCache.StructureType == StreakStructureType.FinancialTransaction )
            {
                // Get the Person's GivingID
                var givingId = new PersonService( rockContext ).GetSelect( personId, p => p.GivingId );

                // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
                var personAliasIds = new PersonAliasService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.Person.GivingId == givingId )
                    .Select( a => a.Id ).ToList();

                return GetFinancialTransactionQuery( rockContext,
                    streakTypeCache.StructureType.Value,
                    streakTypeCache.StructureEntityId.Value,
                    streakTypeCache.StructureSettings.IncludeChildAccounts )
                    .Where( t =>
                       personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) &&
                       t.TransactionDateTime.HasValue &&
                       t.TransactionDateTime >= minDate )
                    .Select( t => t.TransactionDateTime.Value );
            }

            // Interactions based query
            if ( streakTypeCache.StructureType == StreakStructureType.InteractionChannel ||
                streakTypeCache.StructureType == StreakStructureType.InteractionComponent ||
                streakTypeCache.StructureType == StreakStructureType.InteractionMedium )
            {
                return GetInteractionQuery( rockContext, streakTypeCache.StructureType.Value, streakTypeCache.StructureEntityId.Value )
                    .Where( i =>
                        i.PersonAlias.PersonId == personId &&
                        i.InteractionDateTime >= minDate )
                    .Select( i => i.InteractionDateTime );
            }

            // Attendance based query
            var attendanceService = new AttendanceService( rockContext );

            var attendanceQuery = attendanceService.Queryable().AsNoTracking().Where( a =>
                a.PersonAlias.PersonId == personId &&
                a.DidAttend == true &&
                a.Occurrence.DidNotOccur != true &&
                a.Occurrence.OccurrenceDate >= minDate );

            // If the structure information is set, then limit the attendances by the matching groups
            if ( streakTypeCache.StructureType != StreakStructureType.AnyAttendance )
            {
                var groupQuery = GetGroupsQuery( rockContext, streakTypeCache.StructureType.Value, streakTypeCache.StructureEntityId.Value );
                attendanceQuery = attendanceQuery.Where( a => groupQuery.Any( g => g.Id == a.Occurrence.GroupId ) );
            }

            // Get the attended dates
            return attendanceQuery.Select( a => a.Occurrence.OccurrenceDate );
        }

        /// <summary>
        /// Iterate over streak related data by calling the iterationAction for each bit between the start and end dates.
        /// The iteration action should return a bool (isDone) indicating if the iteration process can stop early.
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="iterationAction">The iteration action.</param>
        /// <param name="errorMessage">The error message.</param>
        public void IterateStreakMap( StreakTypeCache streakTypeCache, int personAliasId, DateTime startDate, DateTime endDate, Func<int, DateTime, bool, bool, bool, bool> iterationAction, out string errorMessage )
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

            // Calculate the aggregate engagement map, which are all of the engagement maps ORed together
            var streakService = new StreakService( Context as RockContext );
            var streaks = streakService.Queryable().AsNoTracking()
                .Where( s =>
                    s.StreakTypeId == streakTypeCache.Id
                    && s.PersonAlias.Person.Aliases.Any( a => a.Id == personAliasId ) )
                .ToList();

            var engagementMaps = streaks.Where( se => se.EngagementMap != null ).Select( se => se.EngagementMap ).ToArray();
            var aggregateEngagementMap = GetAggregateMap( engagementMaps );
            var locationId = streaks.Where( s => s.LocationId.HasValue ).Select( s => s.LocationId ).FirstOrDefault();

            // Make sure there are no engagements where occurrences do not exist
            AndBitOperation( aggregateEngagementMap, streakTypeCache.OccurrenceMap ?? new byte[aggregateEngagementMap.Length] );

            // Validate and adjust the dates
            var maxDate = AlignDate( RockDateTime.Now, streakTypeCache );
            var streakTypeMapStartDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );

            // Apply default values to parameters
            if ( startDate < streakTypeMapStartDate )
            {
                startDate = streakTypeMapStartDate;
            }

            if ( endDate > RockDateTime.Today )
            {
                endDate = RockDateTime.Today;
            }

            startDate = AlignDate( startDate, streakTypeCache );
            endDate = AlignDate( endDate, streakTypeCache );

            if ( startDate > endDate )
            {
                /*
                 * 2021-06-25 BJW
                 * We were previously setting the startDate = endDate in this conditional. However, this caused a bug because
                 * a start equal to an end is actually going to result in an iteration over that single day/week. The expected
                 * result would be to not iterate at all since there is no timespan (even inclusively) between the start and
                 * end dates.
                 *
                 * The combination of setting the end date to today if in the future (couple lines up), and then setting the
                 * start equal to the end, caused a single iteration over the current day or week. If the person had engagement
                 * today/this week, they got an attempt record even though the start date was in the future.
                 */

                // There is nothing to be iterated over
                return;
            }

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, true );

            // Calculate the aggregate exclusion map, which are all of the exclusion maps ORed together
            var exclusionMaps = streakTypeCache.StreakTypeExclusions
                .Where( soe => soe.LocationId == locationId && soe.ExclusionMap != null )
                .Select( soe => soe.ExclusionMap )
                .ToList();

            exclusionMaps.AddRange( streaks.Where( s => s.ExclusionMap != null ).Select( s => s.ExclusionMap ) );
            var aggregateExclusionMap = GetAggregateMap( exclusionMaps.ToArray() );

            IterateMaps( streakTypeCache, startDate, endDate, aggregateEngagementMap, aggregateExclusionMap, iterationAction, out errorMessage );
        }

        /// <summary>
        /// Calculate streak data (things like current streak count and longest streak)
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="startDate">Defaults to the streak type start date</param>
        /// <param name="endDate">Defaults to the last elapsed frequency unit (yesterday or last week)</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="maxStreaksToReturn">Specify the maximum number of streak objects "ComputedStreaks" to include in the response</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public StreakData GetStreakData( StreakTypeCache streakTypeCache, int personId, out string errorMessage, DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false, int? maxStreaksToReturn = null )
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

            var maxDate = AlignDate( RockDateTime.Now, streakTypeCache );
            var streakTypeMapStartDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );

            // Apply default values to parameters
            if ( !startDate.HasValue )
            {
                startDate = streakTypeMapStartDate;
            }

            if ( !endDate.HasValue )
            {
                endDate = RockDateTime.Today;

                if ( endDate < startDate )
                {
                    endDate = startDate;
                }
            }

            // Adjust the start and stop dates based on the selected frequency
            startDate = AlignDate( startDate.Value, streakTypeCache );
            endDate = AlignDate( endDate.Value, streakTypeCache );

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

            if ( startDate > endDate )
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

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( startDate.Value, endDate.Value, streakTypeCache, true );

            // Calculate the aggregate engagement map, which are all of the engagement maps ORed together
            var engagementMaps = streaks.Where( se => se.EngagementMap != null ).Select( se => se.EngagementMap ).ToArray();
            var aggregateEngagementMap = GetAggregateMap( engagementMaps );

            // Make sure there are no engagements where occurrences do not exist
            AndBitOperation( aggregateEngagementMap, streakTypeCache.OccurrenceMap ?? new byte[aggregateEngagementMap.Length] );

            // Calculate the exclusion map
            var aggregateExclusionMap = GetAggregateExclusionMap( streaks, streakTypeCache, locationId );

            // Calculate streaks and object array if requested
            var computedStreaks = new List<ComputedStreak>();
            ComputedStreak currentStreak = null;
            ComputedStreak longestStreak = null;
            DateTime? lastOccurrenceDate = null;

            var occurrenceCount = 0;
            var engagementCount = 0;
            var absenceCount = 0;
            var excludedAbsenceCount = 0;

            var objectArray = createObjectArray ? new List<StreakData.FrequencyUnitData>( numberOfFrequencyUnits ) : null;

            // Get the max date that streaks can be broken. This is to avoid breaking streaks while people still have time to
            // engage in that day or week (because it is the current day or week)
            var maxDateForStreakBreaking = GetMaxDateForStreakBreaking( streakTypeCache );

            // Iterate over the maps from the start to the end date
            bool iterationAction( int currentUnit, DateTime currentDate, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                if ( hasOccurrence )
                {
                    occurrenceCount++;

                    if ( hasEngagement )
                    {
                        engagementCount++;

                        // If starting a new streak, record the date
                        if ( currentStreak == null )
                        {
                            currentStreak = new ComputedStreak( currentDate );
                            computedStreaks.Add( currentStreak );

                            // If the user wants a cap on the number of streaks calculated, then we remove them from the front
                            // and thus keep the more recent streaks
                            if ( maxStreaksToReturn.HasValue && computedStreaks.Count > maxStreaksToReturn.Value )
                            {
                                computedStreaks.RemoveAt( 0 );
                            }
                        }

                        // Count this engagement toward the current streak
                        currentStreak.Count++;

                        // If this is now the longest streak, update the longest counters
                        if ( longestStreak == null || currentStreak.Count > longestStreak.Count )
                        {
                            longestStreak = new ComputedStreak( currentStreak.StartDate )
                            {
                                EndDate = currentDate,
                                Count = currentStreak.Count
                            };
                        }
                    }
                    else if ( hasExclusion )
                    {
                        // Excluded/excused absences don't count toward streaks in a positive nor a negative manner, just ignore other
                        // than this count
                        excludedAbsenceCount++;
                    }
                    else if ( currentDate <= maxDateForStreakBreaking )
                    {
                        absenceCount++;

                        // Break the current streak
                        if ( currentStreak != null )
                        {
                            currentStreak.EndDate = lastOccurrenceDate;
                            currentStreak = null;
                        }
                    }

                    lastOccurrenceDate = currentDate;
                }

                if ( createObjectArray )
                {
                    objectArray.Add( new StreakData.FrequencyUnitData
                    {
                        DateTime = currentDate,
                        HasEngagement = hasEngagement,
                        HasExclusion = hasExclusion,
                        HasOccurrence = hasOccurrence
                    } );
                }

                return false;
            }

            IterateMaps( streakTypeCache, startDate.Value, endDate.Value, aggregateEngagementMap, aggregateExclusionMap, iterationAction, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Check if the person had engagement at the most recent occurrence
            var recentOccurrences = GetMostRecentOccurrences( streakTypeCache, aggregateEngagementMap, aggregateExclusionMap, 1, true );
            var mostRecentOccurrence = recentOccurrences?.FirstOrDefault();

            // Get the date of the most recent engagement
            var mostRecentEngagementDate = GetDateOfMostRecentSetBit( streakTypeCache, aggregateEngagementMap, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Calculate the count of engagements at occurrences this year
            var today = RockDateTime.Today;
            var beginningOfYear = new DateTime( today.Year, 1, 1 );

            if ( beginningOfYear < streakTypeCache.StartDate )
            {
                beginningOfYear = streakTypeCache.StartDate;
            }

            var engagementsThisYear = CountSetBits( streakTypeCache, aggregateEngagementMap, beginningOfYear, today, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Calculate the count of engagements at occurrences this month
            var beginningOfMonth = new DateTime( today.Year, today.Month, 1 );

            if ( beginningOfMonth < streakTypeCache.StartDate )
            {
                beginningOfMonth = streakTypeCache.StartDate;
            }

            var engagementsThisMonth = CountSetBits( streakTypeCache, aggregateEngagementMap, beginningOfMonth, today, out errorMessage );

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
                CurrentStreakCount = currentStreak?.Count ?? 0,
                CurrentStreakStartDate = currentStreak?.StartDate,
                LongestStreakCount = longestStreak?.Count ?? 0,
                LongestStreakStartDate = longestStreak?.StartDate,
                LongestStreakEndDate = longestStreak?.EndDate,
                PerFrequencyUnit = objectArray,
                AbsenceCount = absenceCount,
                EngagementCount = engagementCount,
                ExcludedAbsenceCount = excludedAbsenceCount,
                OccurrenceCount = occurrenceCount,
                ComputedStreaks = computedStreaks,
                EngagedAtMostRecentOccurrence = mostRecentOccurrence?.HasEngagement ?? false,
                MostRecentOccurrenceDate = mostRecentOccurrence?.DateTime,
                EngagementsThisMonth = engagementsThisMonth,
                EngagementsThisYear = engagementsThisYear,
                MostRecentEngagementDate = mostRecentEngagementDate
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
        /// <param name="endDate">Defaults to the last elapsed frequency unit (yesterday or last week)</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public StreakData GetStreakData( int streakId, out string errorMessage, DateTime? startDate = null, DateTime? endDate = null, bool createObjectArray = false, bool includeBitMaps = false )
        {
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
        [Obsolete( "Use the new simpler MarkEngagement, MarkAttendanceEngagement, or MarkInteractionEngagement methods instead" )]
        [RockObsolete( "1.12" )]
        public void MarkEngagement( StreakTypeCache streakTypeCache, int personId, out string errorMessage, DateTime? dateOfEngagement = null, int? groupId = null, int? locationId = null, int? scheduleId = null, bool addOrUpdateAttendanceRecord = true )
        {
            var attendanceEngagementArgs = new AttendanceEngagementArgs
            {
                GroupId = groupId,
                LocationId = locationId,
                ScheduleId = scheduleId
            };

            MarkAttendanceEngagement( streakTypeCache, personId, attendanceEngagementArgs, out errorMessage, dateOfEngagement );
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the Streak Engagement map and also add an
        /// Interaction (if enabled).
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="interactionEngagementArgs"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement">Defaults to today</param>
        public void MarkInteractionEngagement( StreakTypeCache streakTypeCache, int personId, InteractionEngagementArgs interactionEngagementArgs, out string errorMessage, DateTime? dateOfEngagement = null )
        {
            // Apply default values to parameters
            if ( !dateOfEngagement.HasValue )
            {
                dateOfEngagement = RockDateTime.Today;
            }

            // Override the component id if the streak type is explicit about it
            if ( streakTypeCache.StructureType == StreakStructureType.InteractionComponent &&
                streakTypeCache.StructureEntityId.HasValue &&
                interactionEngagementArgs != null )
            {
                interactionEngagementArgs.InteractionComponentId = streakTypeCache.StructureEntityId.Value;
            }

            var streak = MarkEngagement( streakTypeCache, personId, out errorMessage, dateOfEngagement.Value, null );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( streak == null )
            {
                errorMessage = "The streak was not returned from Mark Engagement, but no error was specified";
                return;
            }

            // If interaction link is enabled then update add an interaction model
            if ( streakTypeCache.EnableAttendance && interactionEngagementArgs != null && streakTypeCache.StructureType.HasValue && (
                    streakTypeCache.StructureType == StreakStructureType.InteractionChannel ||
                    streakTypeCache.StructureType == StreakStructureType.InteractionComponent ||
                    streakTypeCache.StructureType == StreakStructureType.InteractionMedium ) )
            {
                var rockContext = Context as RockContext;
                var interactionService = new InteractionService( rockContext );

                interactionService.AddInteraction(
                    interactionEngagementArgs.InteractionComponentId,
                    interactionEngagementArgs.EntityId,
                    interactionEngagementArgs.Operation,
                    interactionEngagementArgs.InteractionData,
                    streak.PersonAliasId,
                    dateOfEngagement.Value,
                    null,
                    null,
                    null,
                    null,
                    null );
            }
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the Streak Engagement map and also add an
        /// Attendance (if enabled).
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="attendanceEngagementArgs"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement">Defaults to today</param>
        public void MarkAttendanceEngagement( StreakTypeCache streakTypeCache, int personId, AttendanceEngagementArgs attendanceEngagementArgs, out string errorMessage, DateTime? dateOfEngagement = null )
        {
            // Apply default values to parameters
            if ( !dateOfEngagement.HasValue )
            {
                dateOfEngagement = RockDateTime.Today;
            }

            // Override the group id if the streak type is explicit about the group
            if ( streakTypeCache.StructureType == StreakStructureType.Group &&
                streakTypeCache.StructureEntityId.HasValue &&
                attendanceEngagementArgs != null )
            {
                attendanceEngagementArgs.GroupId = streakTypeCache.StructureEntityId;
            }

            var streak = MarkEngagement( streakTypeCache, personId, out errorMessage, dateOfEngagement.Value, attendanceEngagementArgs?.LocationId );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( streak == null )
            {
                errorMessage = "The streak was not returned from Mark Engagement, but no error was specified";
                return;
            }

            // If attendance is enabled then update attendance models
            if ( streakTypeCache.EnableAttendance && attendanceEngagementArgs != null && streakTypeCache.StructureType.HasValue && (
                    streakTypeCache.StructureType == StreakStructureType.AnyAttendance ||
                    streakTypeCache.StructureType == StreakStructureType.CheckInConfig ||
                    streakTypeCache.StructureType == StreakStructureType.Group ||
                    streakTypeCache.StructureType == StreakStructureType.GroupType ||
                    streakTypeCache.StructureType == StreakStructureType.GroupTypePurpose ) )
            {
                var rockContext = Context as RockContext;
                var attendanceService = new AttendanceService( rockContext );

                // Add or update the attendance, but don't sync streaks since that would create a logic loop
                attendanceService.AddOrUpdate( streak.PersonAliasId, dateOfEngagement.Value, attendanceEngagementArgs.GroupId, attendanceEngagementArgs.LocationId, attendanceEngagementArgs.ScheduleId, null, null, null, null, null, null, null );
            }
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the Streak Engagement map.
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement">Defaults to today</param>
        /// <param name="locationId"></param>
        public void MarkEngagement( StreakTypeCache streakTypeCache, int personId, out string errorMessage, DateTime? dateOfEngagement = null, int? locationId = null )
        {
            // Apply default values to parameters
            if ( !dateOfEngagement.HasValue )
            {
                dateOfEngagement = RockDateTime.Today;
            }

            MarkEngagement( streakTypeCache, personId, out errorMessage, dateOfEngagement.Value, locationId );
        }

        /// <summary>
        /// Notes that the currently logged in person is present. This will update the Streak Engagement map
        /// </summary>
        /// <param name="streakTypeCache"></param>
        /// <param name="personId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="dateOfEngagement"></param>
        /// <param name="locationId"></param>
        private Streak MarkEngagement( StreakTypeCache streakTypeCache, int personId, out string errorMessage, DateTime dateOfEngagement, int? locationId )
        {
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

            var maxDate = AlignDate( RockDateTime.Today, streakTypeCache );
            var minDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );
            var alignedDateOfEngagement = AlignDate( dateOfEngagement, streakTypeCache );

            // Validate the engagement date
            if ( alignedDateOfEngagement < minDate )
            {
                errorMessage = "Cannot mark engagement before the streak type start date";
                return null;
            }

            if ( alignedDateOfEngagement > maxDate )
            {
                errorMessage = "Cannot mark engagement in the future";
                return null;
            }

            // Get the streak if it exists. The first streak is fine, since when streaks are calculated the maps are combined
            var rockContext = Context as RockContext;
            var streakService = new StreakService( rockContext );
            var streak = streakService.GetByStreakTypeAndPerson( streakTypeCache.Id, personId ).FirstOrDefault();

            if ( streak == null && streakTypeCache.RequiresEnrollment )
            {
                errorMessage = "This streak type requires enrollment";
                return null;
            }

            if ( streak == null )
            {
                // Enroll the person since they are marking engagement and enrollment is not required
                streak = Enroll( streakTypeCache, personId, out errorMessage, alignedDateOfEngagement, locationId );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                if ( streak == null )
                {
                    errorMessage = "The enrollment was not successful but no error was specified";
                    return null;
                }
            }

            // Mark engagement on the enrollment map
            streak.EngagementMap = SetBit( streakTypeCache, streak.EngagementMap, alignedDateOfEngagement, true, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Entity Framework cannot detect in-place changes to byte arrays, so force set the properties to modified state
            var streakContextEntry = rockContext.Entry( streak );

            if ( streakContextEntry.State == EntityState.Unchanged )
            {
                streakContextEntry.State = EntityState.Modified;
                streakContextEntry.Property( se => se.EngagementMap ).IsModified = true;
            }

            // Ensure the occurrence bit is set on the streak type model. Check first if it is already set because updating the streak type
            // occurrence map means that all streaks need to be recalculated, so it's expensive
            var streakType = Get( streakTypeCache.Id );
            var isOccurrenceSet = IsBitSet( streakTypeCache, streakType.OccurrenceMap, alignedDateOfEngagement, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !isOccurrenceSet )
            {
                streakType.OccurrenceMap = SetBit( streakTypeCache, streakType.OccurrenceMap, alignedDateOfEngagement, true, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                // Entity Framework cannot detect in-place changes to byte arrays, so force set the properties to modified state
                var streakTypeContextEntry = rockContext.Entry( streakType );

                if ( streakTypeContextEntry.State == EntityState.Unchanged )
                {
                    streakTypeContextEntry.State = EntityState.Modified;
                    streakTypeContextEntry.Property( s => s.OccurrenceMap ).IsModified = true;
                }
            }

            return streak;
        }

        /// <summary>
        /// Handles the interaction record.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use the override with the Interaction Id instead of the Interaction object." )]
        public static void HandleInteractionRecord( Interaction interaction )
        {
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            streakTypeService.HandleInteractionRecord( interaction, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Error while handling interaction record for streaks: {errorMessage}" );
            }
            else
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// When an attendance record is created or modified (example: check-in), the method should be called to synchronize that
        /// attendance to any matching streak types and streaks using this method.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        public static void HandleAttendanceRecord( Attendance attendance )
        {
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            streakTypeService.HandleAttendanceRecordForStreak( attendance?.Id, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Error while handling attendance record for streaks: {errorMessage}" );
            }
            else
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the attendance record.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public static void HandleAttendanceRecord( int attendanceId )
        {
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );

            streakTypeService.HandleAttendanceRecordForStreak( attendanceId, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Error while handling attendance record for streaks: {errorMessage}" );
            }
            else
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the attendance record for streak. This is used when the Attendance object is known to have an active DbContext and can safely use navigation properties.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        private void HandleAttendanceRecordForStreak( int? attendanceId, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( attendanceId == null )
            {
                // No streak data can be marked in this case. Do not throw an error since this operation is chained to the post save event
                // of an attendance model. We don't even know if this attendance was supposed to be related to a streak type.
                return;
            }

            var attendanceInfo = new AttendanceService( new RockContext() ).Queryable()
                .Where( a => a.Id == attendanceId.Value && a.PersonAliasId.HasValue && a.Occurrence.GroupId.HasValue )
                .Select( s => new
                {
                    Id = s.Id,
                    DidAttend = s.DidAttend,
                    PersonId = s.PersonAlias.PersonId,
                    OccurrenceId = s.OccurrenceId,
                    OccurrenceGroupId = s.Occurrence.GroupId.Value,
                    OccurrenceGroupTypeId = s.Occurrence.Group.GroupTypeId,
                    OccurrenceOccurrenceDate = s.Occurrence.OccurrenceDate,
                    CheckInStatus = s.CheckInStatus
                } ).FirstOrDefault();

            if ( attendanceInfo == null )
            {
                return;
            }

            if ( attendanceInfo.DidAttend != true || attendanceInfo.CheckInStatus == Enums.Event.CheckInStatus.Pending )
            {
                // If DidAttend is false or the check-in status is pending, then don't do anything for the streak type. We should not unset the bit for the day/week because
                // we don't know if they had some other engagement besides this and cannot assume it should be unset.
                return;
            }

            // Get the person's streaks
            var personId = attendanceInfo.PersonId;
            var streakService = new StreakService( new RockContext() );
            var enrolledInStreakTypeIdQuery = streakService.Queryable()
                .AsNoTracking()
                .Where( se => se.PersonAlias.PersonId == personId )
                .Select( se => se.StreakTypeId );
            var enrolledInStreakTypeIds = new HashSet<int>( enrolledInStreakTypeIdQuery );

            // Calculate the attendance group details
            var groupId = attendanceInfo.OccurrenceGroupId;
            var groupTypeId = attendanceInfo.OccurrenceGroupTypeId;
            var groupType = GroupTypeCache.Get( groupTypeId );

            var purposeId = groupType.GroupTypePurposeValueId;

            var checkInConfigIdList = groupType.ParentGroupTypes.Select( pgt => pgt.Id );
            var checkInConfigIds = checkInConfigIdList == null ? new HashSet<int>() : new HashSet<int>( checkInConfigIdList );

            // Loop through each active streak types that has attendance enabled and mark engagement for it if the person
            // is enrolled or the streak type does not require enrollment
            var matchedStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.StructureType.HasValue &&
                s.EnableAttendance &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    s.StructureType == StreakStructureType.AnyAttendance ||
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

            foreach ( var streakType in matchedStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, attendanceInfo.OccurrenceOccurrenceDate, null );
            }
        }

        /// <summary>
        /// Handles the interaction record for streaks. Use this method with the ID instead of the whole object if there is
        /// a chance the context for the interaction could be disposed. e.g. if this method is being run in a new Task.
        /// </summary>
        /// <param name="interactionId">The interaction identifier.</param>
        public static void HandleInteractionRecord( int interactionId )
        {
            try
            {
                HandleInteractionRecordInternal( interactionId );
            }
            catch ( System.Data.Entity.Infrastructure.DbUpdateException )
            {
                /*
                    5/10/2022 - DSH

                    A DbUpdateException almost certainly means we had a race condition
                    between two calls to this method. Both tried to create a new Streak
                    object. We are in the latter call which triggered a unique key
                    constraint violation.

                    Try it one more time, this time without catching the exception.
                 */
                HandleInteractionRecordInternal( interactionId );
            }
        }

        /// <summary>
        /// Handles the interaction record for streaks. Use this method with the ID instead of the whole object if there is
        /// a chance the context for the interaction could be disposed. e.g. if this method is being run in a new Task.
        /// </summary>
        /// <param name="interactionId">The interaction identifier.</param>
        private static void HandleInteractionRecordInternal( int interactionId )
        {
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var safeInteraction = new InteractionService( rockContext ).Get( interactionId );

            streakTypeService.HandleInteractionRecordForStreak( safeInteraction, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Error while handling interaction record for streaks: {errorMessage}" );
            }
            else
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the interaction record for streak. This is used when the Interaction object is known to have an active DbContext and can safely use navigation properties.
        /// </summary>
        /// <param name="interaction">The interaction.</param>
        /// <param name="errorMessage">The error message.</param>
        private void HandleInteractionRecordForStreak( Interaction interaction, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( interaction == null )
            {
                // No streak data can be marked in this case. Do not throw an error since this operation is chained to the post save event
                // of an interaction model. We don't even know if this interaction was supposed to be related to a streak type.
                return;
            }

            if ( !interaction.PersonAliasId.HasValue )
            {
                // If we don't know what person this interaction is tied to then it is impossible to mark engagement in a streak. This is not
                // an error because a null PersonAliasId is a valid state for the interaction model.
                return;
            }

            if ( interaction.PersonAlias.Person == null )
            {
                // This is an error state because it is an invalid data scenario.
                errorMessage = $"The person alias {interaction.PersonAliasId.Value} did not produce a valid person record.";
                return;
            }

            // Get the person's streaks
            var personId = interaction.PersonAlias.Person.Id;
            var streakService = new StreakService( new RockContext() );
            var enrolledInStreakTypeIdQuery = streakService.Queryable()
                .AsNoTracking()
                .Where( se => se.PersonAlias.PersonId == personId )
                .Select( se => se.StreakTypeId );
            var enrolledInStreakTypeIds = new HashSet<int>( enrolledInStreakTypeIdQuery );

            // Calculate the interaction hierarchy details
            var componentId = interaction.InteractionComponentId;
            var channelId = interaction.InteractionComponent?.InteractionChannelId;
            var mediumId = interaction.InteractionComponent?.InteractionChannel?.ChannelTypeMediumValueId;

            // Query each active streak type and mark engagement for it if the person
            // is enrolled or the streak type does not require enrollment
            var matchedStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.StructureType.HasValue &&
                s.StructureEntityId.HasValue &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    ( s.StructureType == StreakStructureType.InteractionChannel && s.StructureEntityId.Value == channelId ) ||
                    ( s.StructureType == StreakStructureType.InteractionComponent && s.StructureEntityId.Value == componentId ) ||
                    ( s.StructureType == StreakStructureType.InteractionMedium && s.StructureEntityId.Value == mediumId )
                ) );

            foreach ( var streakType in matchedStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, interaction.InteractionDateTime, null );
            }
        }

        /// <summary>
        /// Handles the financial transaction record for streaks. Use this method with the ID instead of the whole object if there is
        /// a chance the context for the transaction could be disposed. e.g. if this method is being run in a new Task.
        /// </summary>
        /// <param name="transactionId">The financial transaction identifier.</param>
        public static void HandleFinancialTransactionRecord( int transactionId )
        {
            var rockContext = new RockContext();
            var streakTypeService = new StreakTypeService( rockContext );
            var safeTransaction = new FinancialTransactionService( rockContext ).Get( transactionId );

            streakTypeService.HandleFinancialTransactionRecordForStreak( safeTransaction, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Error while handling financial transaction record for streaks: {errorMessage}" );
            }
            else
            {
                rockContext.SaveChanges();
            }

        }

        /// <summary>
        /// Handles the financial transaction record for streak. This is used when the FinancialTransaction object is known to have an active DbContext and can safely use navigation properties.
        /// </summary>
        /// <param name="transaction">The financial transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        private void HandleFinancialTransactionRecordForStreak( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( null == transaction )
            {
                // No streak data can be marked in this case. Do not throw an error since this operation is chained to the post save event
                // of a transaction model. We don't even know if this transaction was supposed to be related to a streak type.
                return;
            }

            if ( !transaction.AuthorizedPersonAliasId.HasValue )
            {
                // If we don't know what person this transaction is tied to then it is impossible to mark engagement in a streak. This is not
                // an error because a null PersonAliasId is a valid state for the transaction model.
                return;
            }

            if ( transaction.AuthorizedPersonAlias.PersonId == 0 )
            {
                // This is an error state because it is an invalid data scenario.
                errorMessage = $"The person alias {transaction.AuthorizedPersonAliasId.Value} does not have a valid person identifier.";
                return;
            }

            // Exclude the following transactions from streaks
            // Refunds, negative/zero amounts
            bool isRefund = null != transaction.RefundDetails || transaction.TotalAmount <= 0;

            if ( isRefund )
            {
                // No error message, we just don't want to track these for streaks
                return;
            }

            // Get the person's streaks
            var personId = transaction.AuthorizedPersonAlias.PersonId;
            var rockContext = new RockContext();
            var streakService = new StreakService( rockContext );

            var enrolledInStreakTypeIdQuery = streakService.Queryable()
                .AsNoTracking()
                .Where( se => se.PersonAlias.PersonId == personId )
                .Select( se => se.StreakTypeId );
            var enrolledInStreakTypeIds = new HashSet<int>( enrolledInStreakTypeIdQuery );

            // Get the account identifier(s) for this transaction
            var transactionAccountIds = transaction.TransactionDetails.Where( a => a.Amount > 0.00M ).Select( t => t.AccountId ).ToList();
            var accountAncestorIds = FinancialAccountCache.GetByIds( transactionAccountIds ).SelectMany( s => s.GetAncestorFinancialAccountIds() ).Distinct().ToList();

            // Query each active streak type and mark engagement for it if the person
            // is enrolled or the streak type does not require enrollment
            var matchedStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.StructureType.HasValue &&
                s.StructureEntityId.HasValue &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    // Try to match the Financial Account ID first
                    ( s.StructureType == StreakStructureType.FinancialTransaction
                        && transactionAccountIds.Contains( s.StructureEntityId.Value ) ||
                    // If include children, see if the Streak's defined AccountId is one of the Ancestors of the account(s) that the transaction was posted to
                    ( s.StructureType == StreakStructureType.FinancialTransaction &&
                        s.StructureSettings.IncludeChildAccounts && accountAncestorIds.Contains( s.StructureEntityId.Value ) ) )
                ) );

            foreach ( var streakType in matchedStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, transaction.TransactionDateTime, null );
            }
        }

        /// <summary>
        /// When an interaction record is created or modified (example: page view), the method should be called to synchronize that
        /// interaction to any matching streak types and streaks using this method.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="errorMessage"></param>
        [RockObsolete( "1.11" )]
        [Obsolete( "This method is only being used internally and is being replaced with a private method. Use the override with the Interaction Id for public access." )]
        public void HandleInteractionRecord( Interaction interaction, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( interaction == null )
            {
                // No streak data can be marked in this case. Do not throw an error since this operation is chained to the post save event
                // of an interaction model. We don't even know if this interaction was supposed to be related to a streak type.
                return;
            }

            if ( !interaction.PersonAliasId.HasValue )
            {
                // If we don't know what person this interaction is tied to then it is impossible to mark engagement in a streak. This is not
                // an error because a null PersonAliasId is a valid state for the interaction model.
                return;
            }

            // Get the person since it's possible the incoming model does not have the virtual PersonAlias loaded
            var rockContext = Context as RockContext;
            var personAliasService = new PersonAliasService( rockContext );
            var person = personAliasService.GetPerson( interaction.PersonAliasId.Value );

            if ( person == null )
            {
                // This is an error state because it is an invalid data scenario.
                errorMessage = $"The person alias {interaction.PersonAliasId.Value} did not produce a valid person record.";
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

            // Calculate the interaction hierarchy details
            var componentId = interaction.InteractionComponentId;

            // Get the InteractionComponent since it's possible the incoming model does not have the virtual InteractionComponent loaded
            var interactionComponent = new InteractionComponentService( rockContext ).Get( componentId );
            var channelId = interactionComponent.InteractionChannelId;
            var mediumId = interactionComponent.InteractionChannel?.ChannelTypeMediumValueId;

            // Query each active streak type and mark engagement for it if the person
            // is enrolled or the streak type does not require enrollment
            var matchedStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.StructureType.HasValue &&
                s.StructureEntityId.HasValue &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    ( s.StructureType == StreakStructureType.InteractionChannel && s.StructureEntityId.Value == channelId ) ||
                    ( s.StructureType == StreakStructureType.InteractionComponent && s.StructureEntityId.Value == componentId ) ||
                    ( s.StructureType == StreakStructureType.InteractionMedium && s.StructureEntityId.Value == mediumId )
                ) );

            foreach ( var streakType in matchedStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, interaction.InteractionDateTime, null );
            }
        }

        /// <summary>
        /// When an attendance record is created or modified (example: check-in), the method should be called to synchronize that
        /// attendance to any matching streak types and streaks using this method.
        /// </summary>
        /// <param name="attendance"></param>
        /// <param name="errorMessage"></param>
        [RockObsolete( "1.11" )]
        [Obsolete( "This method is only being used internally and is being replaced with a private method. Use the override with the Interaction Id for public access." )]
        public void HandleAttendanceRecord( Attendance attendance, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( attendance == null )
            {
                // No streak data can be marked in this case. Do not throw an error since this operation is chained to the post save event
                // of an attendance model. We don't even know if this attendance was supposed to be related to a streak type.
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
                // If we don't know what person this attendance is tied to then it is impossible to mark engagement in a streak. This is not
                // an error because a null PersonAliasId is a valid state for the attendance model.
                return;
            }

            // Get the occurrence to ensure all of the virtual properties are included since it's possible the incoming
            // attendance model does not have all of this data populated
            var rockContext = Context as RockContext;
            var occurrenceService = new AttendanceOccurrenceService( rockContext );
            var occurrence = occurrenceService.Get( attendance.OccurrenceId );

            if ( occurrence == null )
            {
                // This is an error state because it is an invalid data scenario.
                errorMessage = $"The attendance record {attendance.Id} does not have a valid occurrence model.";
                return;
            }

            // Get the person since it's possible the incoming attendance model does not have the virtual PersonAlias loaded
            var personAliasService = new PersonAliasService( rockContext );
            var person = personAliasService.GetPerson( attendance.PersonAliasId.Value );

            if ( person == null )
            {
                // This is an error state because it is an invalid data scenario.
                errorMessage = $"The person alias {attendance.PersonAliasId.Value} did not produce a valid person record.";
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
            var matchedStreakTypes = StreakTypeCache.All().Where( s =>
                s.IsActive &&
                s.StructureType.HasValue &&
                s.EnableAttendance &&
                (
                    !s.RequiresEnrollment ||
                    enrolledInStreakTypeIds.Contains( s.Id )
                ) &&
                (
                    s.StructureType == StreakStructureType.AnyAttendance ||
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

            foreach ( var streakType in matchedStreakTypes )
            {
                MarkEngagement( streakType, personId, out errorMessage, occurrence.OccurrenceDate, null );
            }
        }

        /// <summary>
        /// This convenience method calls <see cref="HandleAttendanceRecord(Attendance)"/> in an asynchronous fashion such that the calling
        /// process can continue uninhibited. Use this where the streak type and streaks should be synchronized, but the calling
        /// process should continue quickly and without regard to the success of this operation. This method creates it's own data
        /// context and saves the changes when complete.
        /// </summary>
        /// <param name="attendance"></param>
        [Obsolete( "Use the HandleAttendanceRecord method instead", true )]
        [RockObsolete( "1.10" )]
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
        /// This convenience method calls <see cref="HandleAttendanceRecord(Attendance)"/> for all attendance records associated the occurrence
        /// in an asynchronous fashion such that the calling process can continue uninhibited. Use this where the streak type and streaks
        /// should be synchronized, but the calling process should continue quickly and without regard to the success of this operation.
        /// This method creates it's own data context and any changes will be saved automatically.
        /// </summary>
        /// <param name="occurrenceId"></param>
        [Obsolete( "Use the HandleAttendanceRecord method instead", true )]
        [RockObsolete( "1.10" )]
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
        /// Synchronize streaks for all attendance records associated with the occurrence
        /// </summary>
        /// <param name="occurrenceId"></param>
        /// <param name="errorMessage"></param>
        [Obsolete( "Use HandleAttendanceRecord method instead", true )]
        [RockObsolete( "1.10" )]
        public void HandleAttendanceRecords( int occurrenceId, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = Context as RockContext;
            var service = new AttendanceService( rockContext );
            var attendances = service.Queryable().AsNoTracking().Where( a => a.OccurrenceId == occurrenceId );

            foreach ( var attendance in attendances )
            {
                HandleAttendanceRecord( attendance, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return;
                }
            }
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
                case StreakStructureType.AnyAttendance:
                    return StreakStructureType.AnyAttendance.GetDescription();
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
                case StreakStructureType.InteractionMedium:
                    var mediumType = DefinedTypeCache.Get( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM );
                    var mediumValue = mediumType?.DefinedValues.FirstOrDefault( dv => dv.Id == structureEntityId.Value );
                    return mediumValue?.Value;
                case StreakStructureType.InteractionChannel:
                    var interactionChannelService = new InteractionChannelService( rockContext );
                    var interactionChannel = interactionChannelService.Get( structureEntityId.Value );
                    return interactionChannel?.Name;
                case StreakStructureType.InteractionComponent:
                    var interactionComponentService = new InteractionComponentService( rockContext );
                    var interactionComponent = interactionComponentService.Queryable().AsNoTracking()
                        .Include( ic => ic.InteractionChannel )
                        .FirstOrDefault( ic => ic.Id == structureEntityId.Value );
                    return $"{interactionComponent?.InteractionChannel?.Name} / {interactionComponent?.Name}";
                case StreakStructureType.FinancialTransaction:
                    return FinancialAccountCache.Get( structureEntityId.Value )?.Name;
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
        /// <param name="mapStartDate">The start date.</param>
        /// <param name="streakOccurrenceFrequency">The streak occurrence frequency.</param>
        /// <param name="unitCount">The unit count.</param>
        /// <returns></returns>
        [Obsolete( "Downgrading the visibility of this method and renaming to GetMostRecentOccurrences", true )]
        [RockObsolete( "1.10" )]
        public static OccurrenceEngagement[] GetMostRecentEngagementBits( byte[] engagementMap, byte[] occurrenceMap, DateTime mapStartDate, StreakOccurrenceFrequency streakOccurrenceFrequency, int unitCount = 24 )
        {
            // Try to accommodate this without knowing the streak type id until this method is removed since it is obsolete
            var streakTypeCache = StreakTypeCache.All().FirstOrDefault( st =>
                st.StartDate == mapStartDate &&
                st.OccurrenceFrequency == streakOccurrenceFrequency &&
                st.OccurrenceMap.Length == occurrenceMap.Length );

            if ( streakTypeCache == null )
            {
                return null;
            }

            return GetMostRecentOccurrences( streakTypeCache, engagementMap, null, unitCount );
        }

        /// <summary>
        /// Get the most recent bits from a map where there was an occurrence
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="exclusionMap">The exclusion map.</param>
        /// <param name="unitCount">The unit count.</param>
        /// <param name="unconditionallyIncludeCurrentUnit">if set to <c>true</c> [unconditionally include current unit].</param>
        /// <returns></returns>
        private static OccurrenceEngagement[] GetMostRecentOccurrences( StreakTypeCache streakTypeCache, byte[] engagementMap, byte[] exclusionMap, int unitCount = 24, bool unconditionallyIncludeCurrentUnit = false )
        {
            if ( unitCount < 1 )
            {
                return null;
            }

            var mapStartDate = streakTypeCache.StartDate;
            var occurrenceMap = streakTypeCache.OccurrenceMap;

            var maxDate = AlignDate( RockDateTime.Now, streakTypeCache );
            var minDate = AlignDate( mapStartDate, streakTypeCache );
            var occurrenceEngagements = new OccurrenceEngagement[unitCount];
            var occurrencesFound = 0;

            var maxDateForStreakBreaking = GetMaxDateForStreakBreaking( streakTypeCache );

            if ( maxDate < minDate )
            {
                maxDate = minDate;
            }

            bool iterationAction( int currentUnit, DateTime currentDate, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                // Don't include dates where there was an absence, but it's after the max date allowed for streak breaking.
                if ( !unconditionallyIncludeCurrentUnit && hasOccurrence && !hasEngagement && currentDate > maxDateForStreakBreaking )
                {
                    return occurrencesFound >= unitCount;
                }

                if ( hasOccurrence && occurrencesFound < unitCount )
                {
                    occurrenceEngagements[occurrencesFound] = new OccurrenceEngagement
                    {
                        DateTime = currentDate,
                        HasEngagement = hasEngagement,
                        HasExclusion = hasExclusion
                    };

                    occurrencesFound++;
                }

                return occurrencesFound >= unitCount;
            }

            ReverseIterateMaps( streakTypeCache, minDate, maxDate, engagementMap, exclusionMap, iterationAction, out var errorMessage );
            return occurrenceEngagements;
        }

        /// <summary>
        /// Start an async task to calculate steak data and then copy it to the streak model
        /// </summary>
        /// <param name="streakTypeId"></param>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the HandlePostSaveChanges method instead.", true )]
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
                    StreakService.HandlePostSaveChanges( streakId );
                }
            } );
        }

        /// <summary>
        /// Updates the enrollments within by calling <see cref="StreakService.HandlePostSaveChanges(int)"/> for each streak.
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <returns>The number of streaks updated</returns>
        public static int HandlePostSaveChanges( int streakTypeId )
        {
            var rockContext = new RockContext();
            var streakService = new StreakService( rockContext );
            var streakIds = streakService.Queryable().AsNoTracking()
                .Where( se => se.StreakTypeId == streakTypeId )
                .Select( se => se.Id )
                .ToList();

            foreach ( var streakId in streakIds )
            {
                StreakService.HandlePostSaveChanges( streakId );
            }

            return streakIds.Count;
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
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the override with StreakTypeCache instead.", true )]
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
            var bytesNeeded = ( unitsFromStart / BitsPerByte ) + 1;
            var byteIndex = map.Length - bytesNeeded;
            var byteBitValue = ( byte ) ( 1 << ( unitsFromStart % BitsPerByte ) );

            if ( byteIndex < 0 )
            {
                return false;
            }

            return ( map[byteIndex] & byteBitValue ) == byteBitValue;
        }

        /// <summary>
        /// Determines if the bit at the bitDate in the map is set.
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="map">The map.</param>
        /// <param name="bitDate">The bit date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [is bit set] [the specified streak type cache]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBitSet( StreakTypeCache streakTypeCache, byte[] map, DateTime bitDate, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( map == null || map.Length == 0 )
            {
                return false;
            }

            var mapStartDate = streakTypeCache.StartDate;

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the streak type start date";
                return false;
            }

            var unitsFromStart = GetFrequencyUnitDifference( mapStartDate, bitDate, streakTypeCache, false );
            var bytesNeeded = ( unitsFromStart / BitsPerByte ) + 1;
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
        [Obsolete( "Use the override with StreakTypeCache param instead", true )]
        [RockObsolete( "1.10" )]
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
            var bytesNeeded = ( unitsFromStart / BitsPerByte ) + 1;

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
        /// Set the bit that corresponds to bitDate. This method works in-place unless the array has to grow. Note that if the array does not
        /// grow and get reallocated, then Entity Framework will not track the change. If needed, force the property state to Modified:
        /// rockContext.Entry( streakModel ).Property( s => s.OccurrenceMap ).IsModified = true;
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="map">The map.</param>
        /// <param name="bitDate">The bit date.</param>
        /// <param name="newValue">if set to <c>true</c> [new value].</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public static byte[] SetBit( StreakTypeCache streakTypeCache, byte[] map, DateTime bitDate, bool newValue, out string errorMessage )
        {
            errorMessage = string.Empty;
            var mapStartDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );
            bitDate = AlignDate( bitDate, streakTypeCache );

            if ( bitDate < mapStartDate )
            {
                errorMessage = "The specified date occurs before the streak type start date";
                return map;
            }

            var unitsFromStart = GetFrequencyUnitDifference( mapStartDate, bitDate, streakTypeCache, false );
            var bytesNeeded = ( unitsFromStart / BitsPerByte ) + 1;

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

            return BitConverter.ToString( map ).Replace( "-", string.Empty );
        }

        /// <summary>
        /// Gets the aggregate exclusion map.
        /// </summary>
        /// <param name="streaks">The streaks.</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        private static byte[] GetAggregateExclusionMap( List<Streak> streaks, StreakTypeCache streakTypeCache, int? locationId )
        {
            // Calculate the streak specific exclusion map
            var streakExclusionMaps = streaks.Where( se => se.ExclusionMap != null ).Select( se => se.ExclusionMap ).ToArray();
            var aggregateStreakExclusionMap = GetAggregateMap( streakExclusionMaps );

            // Calculate the aggregate exclusion map, which are all of the exclusion maps ORed together
            var exclusionMaps = streakTypeCache.StreakTypeExclusions
                .Where( soe => soe.LocationId == locationId && soe.ExclusionMap != null )
                .Select( soe => soe.ExclusionMap )
                .ToList();

            exclusionMaps.Add( aggregateStreakExclusionMap );
            return GetAggregateMap( exclusionMaps.ToArray() );
        }

        #endregion Static Methods

        #region Date Calculation Helpers

        /// <summary>
        /// Aligns the date. For daily streaks, this is the date portion only.  For weekly, this is the Sunday date calculated
        /// based on the first day of the week (streak type or system setting).
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <returns></returns>
        public static DateTime AlignDate( DateTime dateTime, StreakTypeCache streakTypeCache )
        {
            switch ( streakTypeCache.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                case StreakOccurrenceFrequency.Monthly:
                case StreakOccurrenceFrequency.Yearly:
                    return dateTime.Date;
                case StreakOccurrenceFrequency.Weekly:
                    if ( streakTypeCache.FirstDayOfWeek.HasValue )
                    {
                        return RockDateTime.GetSundayDate( dateTime, streakTypeCache.FirstDayOfWeek.Value );
                    }
                    else
                    {
                        return dateTime.SundayDate();
                    }
                default:
                    throw new NotImplementedException( string.Format( "Getting aligned date for the StreakOccurrenceFrequency '{0}' is not implemented", streakTypeCache.OccurrenceFrequency ) );
            }
        }

        /// <summary>
        /// Increments the date time according to the frequency.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="streakOccurrenceFrequency">The streak occurrence frequency.</param>
        /// <param name="isReverse">if set to <c>true</c> [is reverse].</param>
        /// <returns></returns>
        private static DateTime IncrementDateTime( DateTime dateTime, StreakOccurrenceFrequency streakOccurrenceFrequency, bool isReverse = false )
        {
            var incrementBy = ( isReverse ) ? -1 : 1;

            switch ( streakOccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    return dateTime.AddDays( incrementBy );
                case StreakOccurrenceFrequency.Weekly:
                    incrementBy = ( isReverse ) ? -1 * DaysPerWeek : DaysPerWeek;
                    return dateTime.AddDays( incrementBy );
                case StreakOccurrenceFrequency.Monthly:
                    return dateTime.AddMonths( incrementBy );
                case StreakOccurrenceFrequency.Yearly:
                    return dateTime.AddYears( incrementBy );
                default:
                    throw new NotImplementedException( string.Format( "Increment date/time for the StreakOccurrenceFrequency '{0}' is not implemented", streakOccurrenceFrequency ) );
            }
        }

        /// <summary>
        /// Get the number of frequency units (days or weeks) between the two dates
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="occurrenceFrequency">The occurrence frequency.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        [Obsolete( "Use the override with StreakTypeCache param instead", true )]
        [RockObsolete( "1.10" )]
        public static int GetFrequencyUnitDifference( DateTime startDate, DateTime endDate, StreakOccurrenceFrequency occurrenceFrequency, bool isInclusive )
        {
            if ( occurrenceFrequency == StreakOccurrenceFrequency.Monthly ||
                 occurrenceFrequency == StreakOccurrenceFrequency.Yearly )
            {
                throw new NotImplementedException( string.Format( "Get Frequency Unit Difference for the StreakOccurrenceFrequency '{0}' is not implemented. Use the override with StreakTypeCache param instead.", occurrenceFrequency ) );
            }

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

        /// <summary>
        /// Get the number of frequency units (days, weeks, months, or years) between the two dates
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="isInclusive">if set to <c>true</c> [is inclusive].</param>
        /// <returns></returns>
        public static int GetFrequencyUnitDifference( DateTime startDate, DateTime endDate, StreakTypeCache streakTypeCache, bool isInclusive )
        {
            startDate = AlignDate( startDate, streakTypeCache );
            endDate = AlignDate( endDate, streakTypeCache );

            // Calculate the difference in days
            var timeSpan = endDate.Subtract( startDate );
            var numberOfDays = timeSpan.Days;
            var numberOfMonths = 0;
            var numberOfYears = 0;

            switch ( streakTypeCache.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    // Adjust to be inclusive if needed
                    if ( isInclusive && numberOfDays >= 0 )
                    {
                        numberOfDays += 1;
                    }
                    else if ( isInclusive )
                    {
                        numberOfDays -= 1;
                    }

                    // Convert from days to the frequency units
                    return numberOfDays;
                case StreakOccurrenceFrequency.Weekly:
                    if ( isInclusive && numberOfDays >= 0 )
                    {
                        numberOfDays += DaysPerWeek;
                    }
                    else if ( isInclusive )
                    {
                        numberOfDays -= DaysPerWeek;
                    }

                    // Convert from days to the frequency units
                    return ( numberOfDays / DaysPerWeek );
                case StreakOccurrenceFrequency.Monthly:
                    numberOfMonths = ( ( endDate.Year - startDate.Year ) * 12 ) + endDate.Month - startDate.Month;
                    if ( isInclusive )
                    {
                        numberOfMonths += 1;
                    }
                    return numberOfMonths;
                case StreakOccurrenceFrequency.Yearly:
                    numberOfYears = endDate.Year - startDate.Year;
                    if ( isInclusive )
                    {
                        numberOfYears += 1;
                    }
                    return numberOfYears;
                default:
                    throw new NotImplementedException( string.Format( "Get Frequency Unit Difference for the StreakOccurrenceFrequency '{0}' is not implemented", streakTypeCache.OccurrenceFrequency ) );
            }
        }

        /// <summary>
        /// Gets the maximum date for allowing streaks toe be broken. This is the end of the last fully elapsed frequency unit (day or week).
        /// The idea is that streaks should not be broken until the period for engagement has fully elapsed. Until that time period has
        /// elapsed, people still have time to engage and it isn't fair to show their streak as broken.
        /// </summary>
        /// <param name="streakOccurrenceFrequency"></param>
        /// <returns></returns>
        [Obsolete( "Use the override with StreakTypeCache param instead", true )]
        [RockObsolete( "1.10" )]
        public static DateTime GetMaxDateForStreakBreaking( StreakOccurrenceFrequency streakOccurrenceFrequency )
        {
            if ( streakOccurrenceFrequency == StreakOccurrenceFrequency.Monthly ||
                 streakOccurrenceFrequency == StreakOccurrenceFrequency.Yearly )
            {
                throw new NotImplementedException( string.Format( "Get Max Date For Streak Breaking for the StreakOccurrenceFrequency '{0}' is not implemented. Use the override with StreakTypeCache param instead.", streakOccurrenceFrequency ) );
            }

            if ( streakOccurrenceFrequency == StreakOccurrenceFrequency.Daily )
            {
                return RockDateTime.Today.AddDays( -1 );
            }

            // Weekly - this will need to be adjusted when the SundayDate method is replaced the with configurable start/end of week
            return RockDateTime.Now.SundayDate().AddDays( -1 * DaysPerWeek );
        }

        /// <summary>
        /// Gets the maximum date for allowing streaks to be broken. This is the end of the last fully elapsed frequency unit.
        /// The idea is that streaks should not be broken until the period for engagement has fully elapsed. Until that time period has
        /// elapsed, people still have time to engage and it isn't fair to show their streak as broken.
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <returns></returns>
        public static DateTime GetMaxDateForStreakBreaking( StreakTypeCache streakTypeCache )
        {
            var currentBitDate = AlignDate( RockDateTime.Now, streakTypeCache );

            switch ( streakTypeCache.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    return currentBitDate.AddDays( -1 );
                case StreakOccurrenceFrequency.Weekly:
                    return currentBitDate.AddDays( -1 * DaysPerWeek );
                case StreakOccurrenceFrequency.Monthly:
                    return currentBitDate.AddMonths( -1 );
                case StreakOccurrenceFrequency.Yearly:
                    return currentBitDate.AddYears( -1 );
                default:
                    throw new NotImplementedException( string.Format( "Get Max Date For Streak Breaking for the StreakOccurrenceFrequency '{0}' is not implemented", streakTypeCache.OccurrenceFrequency ) );
            }
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
            switch ( structureType )
            {
                case StreakStructureType.AnyAttendance:
                case StreakStructureType.InteractionChannel:
                case StreakStructureType.InteractionComponent:
                case StreakStructureType.InteractionMedium:
                    return new List<Group>() as IQueryable<Group>;
            }

            var groupService = new GroupService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );

            var query = groupService.Queryable()
                .AsNoTracking()
                .Where( g => g.IsActive );

            switch ( structureType )
            {
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.GroupType:
                    var groupTypeIds = groupTypeService.GetCheckinAreaDescendants( structureEntityId ).ConvertAll( x => x.Id );
                    groupTypeIds.Add( structureEntityId );
                    return query.Where( g =>
                        groupTypeIds.Contains( g.GroupTypeId ) );
                case StreakStructureType.Group:
                    return query.Where( g =>
                        g.Id == structureEntityId ||
                        g.ParentGroupId == structureEntityId );
                case StreakStructureType.GroupTypePurpose:
                    var groupTypes = groupTypeService.GetCheckinAreaDescendants( structureEntityId );
                    groupTypes.Add( GroupTypeCache.Get( structureEntityId ) );
                    var groupTypePurposeValueIds = groupTypes.ConvertAll( x => x.GroupTypePurposeValueId );
                    return query.Where( g =>
                        groupTypePurposeValueIds.Contains( g.GroupType.GroupTypePurposeValueId ) );
                default:
                    throw new NotImplementedException( string.Format( "Getting groups for the Structure Type '{0}' is not implemented", structureType ) );
            }
        }

        #endregion Groups Helpers

        #region Interactions Helpers

        /// <summary>
        /// Gets the interaction components query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="structureType">Type of the structure.</param>
        /// <param name="structureEntityId">The structure entity identifier.</param>
        /// <returns></returns>
        private static IQueryable<Interaction> GetInteractionQuery( RockContext rockContext, StreakStructureType structureType, int structureEntityId )
        {
            switch ( structureType )
            {
                case StreakStructureType.AnyAttendance:
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.Group:
                case StreakStructureType.GroupType:
                case StreakStructureType.GroupTypePurpose:
                    return new List<Interaction>() as IQueryable<Interaction>;
            }

            var interactionService = new InteractionService( rockContext );

            var query = interactionService.Queryable()
                .AsNoTracking()
                .Where( i => i.PersonAliasId.HasValue );

            switch ( structureType )
            {
                case StreakStructureType.InteractionMedium:
                    return query.Where( i => i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == structureEntityId );
                case StreakStructureType.InteractionChannel:
                    return query.Where( i => i.InteractionComponent.InteractionChannelId == structureEntityId );
                case StreakStructureType.InteractionComponent:
                    return query.Where( i => i.InteractionComponentId == structureEntityId );
                default:
                    throw new NotImplementedException( string.Format( "Getting interactions for the Structure Type '{0}' is not implemented", structureType ) );
            }
        }

        #endregion Interactions Helpers

        #region Financial Transaction Helpers

        /// <summary>
        /// Gets the financial transaction components query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="structureType">Type of the structure.</param>
        /// <param name="structureEntityId">The structure entity identifier that represents a financial account assigned to the streak type.</param>
        /// <param name="includeChildAccounts">Determines whether the financial account hierarchy is considered when pulling the transaction records.</param>
        /// <returns></returns>
        private static IQueryable<FinancialTransaction> GetFinancialTransactionQuery( RockContext rockContext,
            StreakStructureType structureType, int structureEntityId, bool includeChildAccounts )
        {
            switch ( structureType )
            {
                case StreakStructureType.AnyAttendance:
                case StreakStructureType.CheckInConfig:
                case StreakStructureType.Group:
                case StreakStructureType.GroupType:
                case StreakStructureType.GroupTypePurpose:
                case StreakStructureType.InteractionChannel:
                case StreakStructureType.InteractionComponent:
                case StreakStructureType.InteractionMedium:
                    return new List<FinancialTransaction>() as IQueryable<FinancialTransaction>;
            }

            // Get the account identifier for this transaction
            var accountDescendentIds = includeChildAccounts
                ? FinancialAccountCache.Get( structureEntityId )?.GetDescendentFinancialAccountIds() ?? new int[0]
                : new int[0];

            var transactionService = new FinancialTransactionService( rockContext );
            var query = transactionService.Queryable()
                .AsNoTracking()
                .Where( t => t.AuthorizedPersonAliasId.HasValue &&
                    t.RefundDetails == null &&
                    t.TransactionDetails.Any( a =>
                        a.Amount > 0 &&
                        ( a.AccountId == structureEntityId ||
                        accountDescendentIds.Contains( a.AccountId ) ) ) );

            return query;
        }

        #endregion

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
        /// ANDs the bits in-place on top of array a. If b is longer than a, those bits are not ANDed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void AndBitOperation( byte[] a, byte[] b )
        {
            // Start at the right side (least significant bit) of each array
            var aIndex = a.Length - 1;
            var bIndex = b.Length - 1;

            while ( aIndex >= 0 && bIndex >= 0 )
            {
                a[aIndex] &= b[bIndex];

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
            var growthUnits = ( ( bytesNeeded ?? 0 ) / MapByteGrowthCount ) + 1;
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
        /// Count the number of bits that are set in the map within the specified inclusive date range
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="map">The map.</param>
        /// <param name="rangeMin">The range minimum.</param>
        /// <param name="rangeMax">The range maximum.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private int CountSetBits( StreakTypeCache streakTypeCache, byte[] map, DateTime rangeMin, DateTime rangeMax, out string errorMessage )
        {
            var count = 0;

            // Iterate over the map from the start to the end of the date range and do this logic
            bool iterationAction( int currentUnit, DateTime currentDate, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                if ( hasEngagement )
                {
                    count++;
                }

                return false;
            }

            IterateMaps( streakTypeCache, rangeMin, rangeMax, map, null, iterationAction, out errorMessage );
            return count;
        }

        /// <summary>
        /// Get the date of the most recent bit that is set in the map
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="map">The map.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private static DateTime? GetDateOfMostRecentSetBit( StreakTypeCache streakTypeCache, byte[] map, out string errorMessage )
        {
            DateTime? mostRecentBitDate = null;

            // Iterate over the map from the today backwards and do this logic
            bool iterationAction( int currentUnit, DateTime currentDate, bool hasOccurrence, bool hasEngagement, bool hasExclusion )
            {
                if ( hasEngagement )
                {
                    mostRecentBitDate = currentDate;
                }

                return mostRecentBitDate.HasValue;
            }

            ReverseIterateMaps( streakTypeCache, streakTypeCache.StartDate, RockDateTime.Today, map, null, iterationAction, out errorMessage );
            return mostRecentBitDate;
        }

        /// <summary>
        /// Iterates over each bit in the maps over the given timeframe from min to max. The actionPerIteration is called for each bit.
        /// actionPerIteration( currentUnit, hasOccurrence, hasEngagement, hasExclusion );
        /// </summary>
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="iterationStartDate">The iteration start date.</param>
        /// <param name="iterationEndDate">The iteration end date.</param>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="exclusionMap">The exclusion map.</param>
        /// <param name="actionPerIteration">The action per iteration. Returns a bool indicating if the iteration should stop early (isDone).</param>
        /// <param name="errorMessage">The error message.</param>
        private static void IterateMaps( StreakTypeCache streakTypeCache, DateTime iterationStartDate, DateTime iterationEndDate, byte[] engagementMap, byte[] exclusionMap, Func<int, DateTime, bool, bool, bool, bool> actionPerIteration, out string errorMessage )
        {
            errorMessage = string.Empty;

            var today = AlignDate( RockDateTime.Now, streakTypeCache );
            var maxDate = AlignDate( iterationEndDate, streakTypeCache );
            var minDate = AlignDate( iterationStartDate, streakTypeCache );
            var mapStartDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );

            if ( maxDate > today )
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
            var slideStartUnitsToFuture = GetFrequencyUnitDifference( mapStartDate, minDate, streakTypeCache, false );

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( minDate, maxDate, streakTypeCache, true );

            // Prepare to iterate over the bytes
            var currentUnit = 0;
            var currentDate = minDate;
            var initialByteOffset = ( slideStartUnitsToFuture / BitsPerByte ) + 1;
            var currentByteBitValue = 1 << ( slideStartUnitsToFuture % BitsPerByte );

            var occurrenceMap = streakTypeCache.OccurrenceMap ?? new byte[0];
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
                var isDone = actionPerIteration( currentUnit, currentDate, hasOccurrence, hasEngagement, hasExclusion );

                if ( isDone )
                {
                    break;
                }

                // Iterate to the next bit
                currentUnit++;
                currentByteBitValue = currentByteBitValue << 1;
                currentDate = IncrementDateTime( currentDate, streakTypeCache.OccurrenceFrequency, false );

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
        /// <param name="streakTypeCache">The streak type cache.</param>
        /// <param name="iterationStartDate">The iteration start date.</param>
        /// <param name="iterationEndDate">The iteration end date.</param>
        /// <param name="engagementMap">The engagement map.</param>
        /// <param name="exclusionMap">The exclusion map.</param>
        /// <param name="actionPerIteration">The action per iteration. Returns a bool indicating if the iteration should stop early (isDone).</param>
        /// <param name="errorMessage">The error message.</param>
        private static void ReverseIterateMaps( StreakTypeCache streakTypeCache, DateTime iterationStartDate, DateTime iterationEndDate, byte[] engagementMap, byte[] exclusionMap, Func<int, DateTime, bool, bool, bool, bool> actionPerIteration, out string errorMessage )
        {
            errorMessage = string.Empty;

            var today = AlignDate( RockDateTime.Now, streakTypeCache );
            var maxDate = AlignDate( iterationEndDate, streakTypeCache );
            var minDate = AlignDate( iterationStartDate, streakTypeCache );
            var mapStartDate = AlignDate( streakTypeCache.StartDate, streakTypeCache );

            if ( maxDate > today )
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
            var slideStartUnitsToFuture = GetFrequencyUnitDifference( mapStartDate, maxDate, streakTypeCache, false );

            // Calculate the number of frequency units that the results are based upon (inclusive)
            var numberOfFrequencyUnits = GetFrequencyUnitDifference( minDate, maxDate, streakTypeCache, true );

            // Prepare to iterate over the bytes
            var currentUnit = numberOfFrequencyUnits - 1;
            var currentDate = maxDate;
            var initialByteOffset = ( slideStartUnitsToFuture / BitsPerByte ) + 1;
            var currentByteBitValue = 1 << ( slideStartUnitsToFuture % BitsPerByte );

            var occurrenceMap = streakTypeCache.OccurrenceMap ?? new byte[0];
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
                var isDone = actionPerIteration( currentUnit, currentDate, hasOccurrence, hasEngagement, hasExclusion );

                if ( isDone )
                {
                    break;
                }

                // Iterate to the next bit
                currentUnit--;
                currentDate = IncrementDateTime( currentDate, streakTypeCache.OccurrenceFrequency, true );
                currentByteBitValue = currentByteBitValue >> 1;

                // If the bit value is beyond the current byte, then increment to the next byte
                if ( currentByteBitValue == 0 )
                {
                    currentByteBitValue = 1 << ( BitsPerByte - 1 );

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
        /// Gets or sets the streaks.
        /// </summary>
        public List<ComputedStreak> ComputedStreaks { get; set; }

        /// <summary>
        /// Indicates if the person engaged at the most recent occurrence.
        /// </summary>
        public bool EngagedAtMostRecentOccurrence { get; set; }

        /// <summary>
        /// The date of the most recent occurrence
        /// </summary>
        public DateTime? MostRecentOccurrenceDate { get; set; }

        /// <summary>
        /// The date of the most recent engagement
        /// </summary>
        public DateTime? MostRecentEngagementDate { get; set; }

        /// <summary>
        /// Gets or sets the count of engagements this month.
        /// </summary>
        public int EngagementsThisMonth { get; set; }

        /// <summary>
        /// Gets or sets the count of engagements this year.
        /// </summary>
        public int EngagementsThisYear { get; set; }

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
    /// Object representing a streak over time
    /// </summary>
    public class ComputedStreak
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedStreak"/> class.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        public ComputedStreak( DateTime startDate )
        {
            StartDate = startDate;
        }

        /// <summary>
        /// Gets the start date time.
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        public int Count { get; set; }
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

        /// <summary>
        /// Did the person have an exclusion?
        /// </summary>
        public bool HasExclusion { get; set; }
    }

    /// <summary>
    /// Data needed to create an interaction via <see cref="StreakTypeService.MarkInteractionEngagement" />
    /// </summary>
    public class InteractionEngagementArgs
    {
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionComponent"/> Component that is associated with this Interaction.
        /// </summary>
        public int InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is related to.
        /// For example:
        ///  if this is a Page View:
        ///     Interaction.EntityId is the Page.Id of the page that was viewed
        ///  if this is a Communication Recipient activity:
        ///     Interaction.EntityId is the CommunicationRecipient.Id that did the click or open
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        public string InteractionData { get; set; }
    }

    /// <summary>
    /// Data needed to create an attendance via <see cref="StreakTypeService.MarkAttendanceEngagement" />
    /// </summary>
    public class AttendanceEngagementArgs
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; set; }
    }
}