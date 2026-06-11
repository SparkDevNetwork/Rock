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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

using Ical.Net;
using Ical.Net.DataTypes;

using Rock.Configuration;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Schedule"/> entity. This inherits from the Service class
    /// </summary>
    public partial class ScheduleService
    {
        #region Methods

        /// <summary>
        /// Creates the Preview for all the occurrences of the schedule which can be viewed in HTML
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string CreatePreviewHTML( Schedule entity )
        {
            var sbPreviewHtml = new System.Text.StringBuilder();
            sbPreviewHtml.Append( $@"<strong>iCalendar Content</strong><div style='white-space: pre' Font-Names='Consolas' Font-Size='9'><br />{entity.iCalendarContent}</div>" );

            var calendarList = CalendarCollection.Load( new System.IO.StringReader( entity.iCalendarContent ) );
            Calendar calendar = null;
            if ( calendarList.Count > 0 )
            {
                calendar = calendarList[0] as Calendar;
            }

            var calendarEvent = calendar?.Events?[0];

            if ( calendarEvent?.DtStart != null )
            {
                var nextOccurrences = calendar.GetOccurrences( RockDateTime.Now, RockDateTime.Now.AddYears( 1 ) ).Take( 26 ).ToList();
                var sbOccurrenceItems = new System.Text.StringBuilder();
                if ( nextOccurrences.Any() )
                {
                    foreach ( var occurrence in nextOccurrences )
                    {
                        sbOccurrenceItems.Append( $"<li>{GetOccurrenceText( occurrence )}</li>" );
                    }
                }
                else
                {
                    sbOccurrenceItems.Append( "<li>No future occurrences</l1>" );
                }

                sbPreviewHtml.Append( $"<hr /><strong>Occurrences Preview</strong><ul>{sbOccurrenceItems}</ul>" );
            }

            return sbPreviewHtml.ToString();
        }

        /// <summary>
        /// Gets the occurrence text.
        /// Moved over from ScheduleDetails Blocks in Webforms
        /// </summary>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns></returns>
        private static string GetOccurrenceText( Occurrence occurrence )
        {
            string occurrenceText;
            if ( occurrence.Period.Duration <= new TimeSpan( 0, 0, 1 ) )
            {
                // no or very short duration. Probably a schedule for starting something that doesn't care about duration, like Metrics
                occurrenceText = string.Format( "{0}", occurrence.Period.StartTime.Value.ToString( "g" ) );
            }
            else if ( occurrence.Period.StartTime.Value.Date.Equals( occurrence.Period.EndTime.Value.Date ) )
            {
                // same day for start and end time
                occurrenceText = string.Format( "{0} - {1} to {2} ( {3} hours) ", occurrence.Period.StartTime.Value.Date.ToShortDateString(), occurrence.Period.StartTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.EndTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
            }
            else
            {
                // spans over midnight
                occurrenceText = string.Format( "{0} to {1} ( {2} hours) ", occurrence.Period.StartTime.Value.ToString( "g" ), occurrence.Period.EndTime.Value.ToString( "g" ), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
            }
            return occurrenceText;
        }

        /// <summary>
        /// Clones a schedule given the id.
        /// </summary>
        /// <param name="id"> The idkey of the Schedule to be copied</param>
        /// <returns></returns>
        public Schedule Copy( string id )
        {
            var schedule = Get( id );
            var newSchedule = schedule.CloneWithoutIdentity();
            newSchedule.Name += " - Copy";
            this.Add( newSchedule );
            schedule.LoadAttributes();
            newSchedule.LoadAttributes();
            newSchedule.CopyAttributesFrom( schedule );

            var rockContext = this.Context as RockContext;

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                newSchedule.SaveAttributeValues( rockContext );
            } );
            return newSchedule;
        }

        #endregion

        #region Schedule Date Methods

        /// <summary>
        /// Updates all the <see cref="ScheduleDate"/> entries for the specified
        /// schedule. This will add and remove entries as needed to ensure
        /// that there is an entry for each occurrence.
        /// </summary>
        /// <param name="scheduleId">The identifier of the schedule to be updated.</param>
        /// <returns>The total number of <see cref="ScheduleDate"/> records that were added or deleted.</returns>
        internal static int UpdateScheduleDates( int scheduleId )
        {
            var startDate = RockDateTime.Now.AddYears( -2 );
            var endDate = RockDateTime.Now.AddYears( 2 );
            Schedule schedule;

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                schedule = new ScheduleService( rockContext ).Get( scheduleId );
            }

            if ( schedule == null )
            {
                return 0;
            }

            return UpdateScheduleDates( new[] { schedule }, startDate, endDate, true, CancellationToken.None );
        }

        /// <summary>
        /// Updates all the <see cref="ScheduleDate"/> entries for all schedules
        /// in the system. This will add and remove entries as needed to ensure
        /// that there is an entry for each occurrence of each active schedule.
        /// </summary>
        /// <param name="bulkProcess">If <c>true</c> then the bulk add and bulk remove methods will be used instead of calling SaveChanges().</param>
        /// <param name="cancellationToken">Signals if this process should be aborted.</param>
        /// <returns>The total number of <see cref="ScheduleDate"/> records that were added or deleted.</returns>
        internal static int UpdateScheduleDates( bool bulkProcess, CancellationToken cancellationToken )
        {
            var startDate = RockDateTime.Now.AddYears( -2 );
            var endDate = RockDateTime.Now.AddYears( 2 );

            return UpdateScheduleDates( startDate, endDate, bulkProcess, cancellationToken );
        }

        /// <summary>
        /// Updates all the <see cref="ScheduleDate"/> entries for all schedules
        /// in the system. This will add and remove entries as needed to ensure
        /// that there is an entry for each occurrence of each active schedule
        /// between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="startDate">The starting date. If any <see cref="ScheduleDate"/> entries exist prior to this date, they will be removed.</param>
        /// <param name="endDate">The ending date. If any <see cref="ScheduleDate"/> entries exist after this date, they will be removed.</param>
        /// <param name="bulkProcess">If <c>true</c> then the bulk add and bulk remove methods will be used instead of calling SaveChanges().</param>
        /// <param name="cancellationToken">Signals if this process should be aborted.</param>
        /// <returns>The total number of <see cref="ScheduleDate"/> records that were added or deleted.</returns>
        internal static int UpdateScheduleDates( DateTime startDate, DateTime endDate, bool bulkProcess, CancellationToken cancellationToken )
        {
            List<Schedule> schedules;
            var modifiedDateCount = 0;

            using ( var readOnlyRockContext = RockApp.Current.CreateRockContext() )
            {
                schedules = new ScheduleService( readOnlyRockContext )
                    .Queryable()
                    .AsNoTracking()
                    .ToList();
            }

            // We usually work with a +/- multi-year window. This means at
            // worst (+/- 5-year window), each schedule could add and delete
            // up to 3,650 date entries. Chunking by 100 schedules at a time
            // should reduce the number of queries and still keep the
            // transaction time reasonable.
            foreach ( var chunk in schedules.Chunk( 100 ) )
            {
                modifiedDateCount += UpdateScheduleDates( chunk, startDate, endDate, bulkProcess, cancellationToken );
            }

            return modifiedDateCount;
        }

        /// <summary>
        /// Updates all the <see cref="ScheduleDate"/> entries for specified
        /// schedules. This will add and remove entries as needed to ensure
        /// that there is an entry for each occurrence of each active schedule
        /// between <paramref name="startDate"/> and <paramref name="endDate"/>.
        /// </summary>
        /// <param name="schedules">The schedules to be processed.</param>
        /// <param name="startDate">The starting date. If any <see cref="ScheduleDate"/> entries exist prior to this date, they will be removed.</param>
        /// <param name="endDate">The ending date. If any <see cref="ScheduleDate"/> entries exist after this date, they will be removed.</param>
        /// <param name="bulkProcess">If <c>true</c> then the bulk add and bulk remove methods will be used instead of calling SaveChanges().</param>
        /// <param name="cancellationToken">Signals if this process should be aborted.</param>
        /// <returns>The total number of <see cref="ScheduleDate"/> records that were added or deleted.</returns>
        internal static int UpdateScheduleDates( ICollection<Schedule> schedules, DateTime startDate, DateTime endDate, bool bulkProcess, CancellationToken cancellationToken )
        {
            var rockContext = RockApp.Current.CreateRockContext();
            var scheduleDateSet = rockContext.Set<ScheduleDate>();
            var scheduleIds = schedules.Select( s => s.Id ).ToList();
            var existingScheduleDates = scheduleDateSet
                .AsNoTracking()
                .Where( sd => scheduleIds.Contains( sd.ScheduleId ) )
                .GroupBy( sd => sd.ScheduleId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var modifiedDateCount = 0;
            var scheduleDatesToAdd = new List<ScheduleDate>();
            var scheduleDatesToRemove = new List<ScheduleDate>();

            foreach ( var schedule in schedules )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( !existingScheduleDates.TryGetValue( schedule.Id, out var existingDates ) )
                {
                    existingDates = new List<ScheduleDate>();
                }

                var existingDateHashSet = new HashSet<(DateTime, DateTime)>( existingDates.Select( sd => (sd.StartDateTime, sd.EndDateTime) ) );

                // Get the current start and end times of each occurrence.
                var scheduleDates = schedule.IsActive
                    ? schedule.GetScheduledStartTimes( startDate, endDate )
                        .Select( sd => (StartDateTime: sd, EndDateTime: sd.AddMinutes( schedule.DurationInMinutes )) )
                    : new List<(DateTime StartDateTime, DateTime EndDateTime)>();
                var scheduleDateHashSet = new HashSet<(DateTime, DateTime)>( scheduleDates );

                // Look for dates that need to be removed.
                foreach ( var existingDate in existingDates )
                {
                    if ( !scheduleDateHashSet.Contains( (existingDate.StartDateTime, existingDate.EndDateTime) ) )
                    {
                        scheduleDatesToRemove.Add( existingDate );

                        modifiedDateCount++;
                    }
                }

                // Look for dates that need to be added.
                foreach ( var scheduleDate in scheduleDates )
                {
                    if ( !existingDateHashSet.Contains( scheduleDate ) )
                    {
                        scheduleDatesToAdd.Add( new ScheduleDate
                        {
                            ScheduleId = schedule.Id,
                            StartDateTime = scheduleDate.StartDateTime,
                            EndDateTime = scheduleDate.EndDateTime,
                            StartDateKey = scheduleDate.StartDateTime.AsDateKey(),
                        } );

                        modifiedDateCount++;
                    }
                }
            }

            if ( modifiedDateCount > 0 )
            {
                if ( bulkProcess )
                {
                    // Delete old records first to prevent primary key violations.
                    if ( scheduleDatesToRemove.Any() )
                    {
                        BulkDeleteScheduleDates( rockContext, scheduleDatesToRemove );
                    }

                    if ( scheduleDatesToAdd.Any() )
                    {
                        rockContext.BulkInsert( scheduleDatesToAdd );
                    }
                }
                else
                {
                    // Non-bulk processing is used by unit tests so that the
                    // tests can track the changes and verify expected results.
                    scheduleDateSet.AddRange( scheduleDatesToAdd );
                    scheduleDateSet.RemoveRange( scheduleDatesToRemove );
                    rockContext.SaveChanges( new SaveChangesArgs
                    {
                        DisablePrePostProcessing = true,
                    } );
                }
            }

            return modifiedDateCount;
        }

        /// <summary>
        /// Performs a manual bulk delete operation. The only standard bulk
        /// delete method we have requires a queryable, but in this case that
        /// would mean manually building a queryable with a bunch of OR
        /// statements to match the specific items to be deleted.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="scheduleDatesToRemove">The scheduled dates to be removed.</param>
        /// <returns>The number of items that were deleted.</returns>
        private static int BulkDeleteScheduleDates( RockContext rockContext, List<ScheduleDate> scheduleDatesToRemove )
        {
            // Build a DataTable of keys to delete
            var table = new DataTable();
            table.Columns.Add( "ScheduleId", typeof( int ) );
            table.Columns.Add( "StartDateTime", typeof( DateTime ) );

            foreach ( var sd in scheduleDatesToRemove )
            {
                table.Rows.Add( sd.ScheduleId, sd.StartDateTime );
            }

            // Get the underlying SQL connection
            var connection = ( SqlConnection ) rockContext.Database.Connection;
            var closeConnection = false;

            if ( connection.State != System.Data.ConnectionState.Open )
            {
                connection.Open();
                closeConnection = true;
            }

            using ( var command = connection.CreateCommand() )
            {
                using ( var transaction = connection.BeginTransaction() )
                {
                    command.Transaction = transaction;

                    // 1. Create a temp table
                    command.CommandText = @"
CREATE TABLE #KeysToDelete (
    ScheduleId INT NOT NULL,
    StartDateTime DATETIME NOT NULL
);";
                    command.ExecuteNonQuery();

                    // 2. Bulk copy into the temp table
                    using ( var bulk = new SqlBulkCopy( connection, SqlBulkCopyOptions.Default, transaction ) )
                    {
                        bulk.DestinationTableName = "#KeysToDelete";
                        bulk.WriteToServer( table );
                    }

                    // 3. Delete using a join
                    command.CommandText = @"
DELETE [sd]
FROM [ScheduleDate] [sd]
INNER JOIN #KeysToDelete [t]
    ON [sd].[ScheduleId] = [t].[ScheduleId]
    AND [sd].[StartDateTime] = [t].[StartDateTime];";

                    var deleted = command.ExecuteNonQuery();

                    // 4. Commit
                    transaction.Commit();

                    if ( closeConnection )
                    {
                        connection.Close();
                    }

                    return deleted;
                }
            }
        }

        #endregion
    }
}
