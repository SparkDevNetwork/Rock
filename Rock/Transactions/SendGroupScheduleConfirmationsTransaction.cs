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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends Group Scheduler confirmation communications for a snapshotted set of
    /// <see cref="Attendance"/> records on the standard transaction queue. The
    /// caller (typically the Group Scheduler block's <c>SendConfirmations</c>
    /// action) is expected to perform pre-flight validation synchronously and
    /// enqueue this transaction so the HTTP request can return immediately,
    /// avoiding reverse-proxy timeouts on large sends.
    /// </summary>
    public class SendGroupScheduleConfirmationsTransaction : ITransaction
    {
        #region Cache Coordination

        /// <summary>
        /// The <see cref="RockCache"/> region used to coordinate "currently queued"
        /// attendance identifiers between the block action (which writes claims)
        /// and this transaction (which releases them after drain).
        /// </summary>
        internal const string CacheRegion = "GroupScheduler";

        /// <summary>
        /// How long a queued claim is held in cache. The cache release in
        /// <see cref="Execute"/> normally clears entries well before this TTL;
        /// the TTL is the safety net for the case where a worker terminates
        /// before reaching the release in the <c>finally</c> block.
        /// </summary>
        internal static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes( 10 );

        /// <summary>
        /// Builds the <see cref="RockCache"/> key for tracking a single queued
        /// <see cref="Attendance"/> identifier.
        /// </summary>
        /// <param name="attendanceId">The Attendance identifier.</param>
        /// <returns>The cache key string for this attendance identifier.</returns>
        internal static string GetCacheKey( int attendanceId )
        {
            return $"AttendanceConfirmationQueued:{attendanceId}";
        }

        #endregion

        /// <summary>
        /// Gets or sets the identifiers of the <see cref="Attendance"/> records
        /// for which confirmations should be sent. The transaction re-filters to
        /// records that have not yet had
        /// <see cref="Attendance.ScheduleConfirmationSent"/> set, so it is safe
        /// (and idempotent) if multiple instances are enqueued for the same IDs.
        /// </summary>
        /// <value>
        /// The list of <see cref="Attendance"/> identifiers to send confirmations for.
        /// </value>
        public List<int> AttendanceIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupScheduleConfirmationsTransaction"/> class.
        /// </summary>
        /// <param name="attendanceIds">The identifiers of the Attendance records to send confirmations for.</param>
        public SendGroupScheduleConfirmationsTransaction( IEnumerable<int> attendanceIds )
        {
            AttendanceIds = attendanceIds?.ToList() ?? new List<int>();
        }

        /// <summary>
        /// Executes the send on the background queue.
        /// </summary>
        public void Execute()
        {
            if ( AttendanceIds == null || !AttendanceIds.Any() )
            {
                return;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var attendanceService = new AttendanceService( rockContext );

                    // Re-apply the same base predicates the caller's snapshot relied on
                    // (RequestedToAttend, DeclineReasonValueId, DidAttend, future
                    // OccurrenceDate, RSVP / ScheduledToAttend / AutoAccept rules) so an
                    // Attendance whose state changed between snapshot and drain (e.g.,
                    // the volunteer declined) is skipped rather than receiving a stale
                    // confirmation.
                    var sendConfirmationAttendancesQuery = attendanceService.GetPendingAndAutoAcceptScheduledConfirmations()
                        .Where( a => AttendanceIds.Contains( a.Id ) )
                        .Where( a => a.ScheduleConfirmationSent != true );

                    var sendMessageResult = attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery, true );

                    rockContext.SaveChanges();

                    // Per-recipient errors would normally surface in the UI for a
                    // synchronous send. Because this transaction runs after the HTTP
                    // response has returned, log them so they remain discoverable via
                    // the Exception List page.
                    if ( sendMessageResult?.Errors != null && sendMessageResult.Errors.Any() )
                    {
                        ExceptionLogService.LogException(
                            new Exception( $"Group Scheduler background confirmation send completed with {sendMessageResult.Errors.Count} error(s): {string.Join( " | ", sendMessageResult.Errors )}" )
                        );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Exception in {nameof( SendGroupScheduleConfirmationsTransaction )}.Execute() for {AttendanceIds.Count} attendance id(s).", ex ) );
            }
            finally
            {
                // Release the cache claims regardless of success or failure. The TTL
                // will eventually clear stale entries if this is skipped (e.g. process
                // crash), but explicit release keeps the cache footprint small under
                // normal load.
                foreach ( var id in AttendanceIds )
                {
                    try
                    {
                        RockCache.Remove( GetCacheKey( id ), CacheRegion );
                    }
                    catch
                    {
                        // Intentionally swallowed -- cache cleanup must not propagate failures.
                    }
                }
            }
        }
    }
}
