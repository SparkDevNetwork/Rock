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

using Rock.Configuration;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks;

/// <summary>
/// Updates <see cref="PersonSession.LastActivityDateTime"/> based on user
/// activity. Replaces <see cref="UpdateUserLastActivity"/> as the
/// authoritative activity-tracking task; see the PersonSession spec.
/// </summary>
/// <remarks>
/// Writes are throttled to once per
/// <see cref="ThrottleWindowMinutes"/> minutes per
/// <see cref="PersonSession"/>. The throttle is intentionally longer
/// than the legacy <see cref="UpdateUserLastActivity"/> two-minute window
/// to further reduce database contention during heavy load (page hits,
/// check-in, etc.). A small loss of recency on
/// <c>LastActivityDateTime</c> is acceptable for the read use cases
/// (Active Users block, Data Automation idle-detection job).
/// </remarks>
public sealed class UpdatePersonSessionLastActivity : BusStartedTask<UpdatePersonSessionLastActivity.Message>
{
    /// <summary>
    /// The minimum number of minutes that must elapse between
    /// <see cref="PersonSession.LastActivityDateTime"/> writes for a given
    /// session.
    /// </summary>
    private const int ThrottleWindowMinutes = 5;

    /// <summary>
    /// The cache key to use for the in-process dictionary that tracks the most
    /// recent <see cref="PersonSession.LastActivityDateTime"/> for each session.
    /// </summary>
    private const string CACHE_KEY = "UpdatePersonSessionLastActivity_PreviousLastActivityDateTimeBySessionId";

    /// <summary>
    /// Per-process record of the most recent <c>LastActivityDateTime</c>
    /// observed for each <see cref="PersonSession"/>. Used by
    /// <see cref="Message.SendIfNeeded"/> to short-circuit the bus send
    /// when the window has not elapsed since the last write attempt from
    /// this process.
    /// </summary>
    private static ConcurrentDictionary<int, DateTime> PreviousLastActivityDateTimeBySessionId => ( ConcurrentDictionary<int, DateTime> ) RockCache.GetOrAddExisting( CACHE_KEY, () => new ConcurrentDictionary<int, DateTime>() );

    /// <summary>
    /// Determines if the LastActivityDateTime on this session needs to be
    /// updated based on how recently it was last updated.
    /// </summary>
    /// <param name="previousLastActivityDateTime">The previously observed last-activity date time, or <c>null</c> if none has been recorded yet.</param>
    /// <param name="lastActivityDateTime">The candidate last-activity date time.</param>
    /// <returns><c>true</c> if a write is warranted; otherwise, <c>false</c>.</returns>
    private static bool NeedsToBeUpdated( DateTime? previousLastActivityDateTime, DateTime lastActivityDateTime )
    {
        if ( !previousLastActivityDateTime.HasValue )
        {
            return true;
        }

        var timeSinceLastUpdate = lastActivityDateTime - previousLastActivityDateTime.Value;
        return timeSinceLastUpdate.TotalMinutes > ThrottleWindowMinutes;
    }

    /// <summary>
    /// Executes the activity write. Performs a second throttle check
    /// against the database value (the in-process dictionary may be stale
    /// if another node already wrote, or if the message was queued before
    /// a more recent write landed).
    /// </summary>
    /// <param name="message">The bus message.</param>
    public override void Execute( Message message )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var personSessionService = new PersonSessionService( rockContext );
        var personSession = personSessionService.Get( message.PersonSessionId );

        if ( personSession == null )
        {
            return;
        }

        // Double check that the database value is still inside the
        // throttle window before writing. Avoids an UPDATE when a
        // concurrent message from this or another node already
        // advanced the row.
        if ( NeedsToBeUpdated( personSession.LastActivityDateTime, message.LastActivityDateTime ) )
        {
            personSession.LastActivityDateTime = message.LastActivityDateTime;
            rockContext.SaveChanges();
        }
    }

    /// <summary>
    /// Message Class
    /// </summary>
    public sealed class Message : BusStartedTaskMessage
    {
        /// <summary>
        /// Gets or sets the <see cref="PersonSession"/> identifier whose
        /// <see cref="PersonSession.LastActivityDateTime"/> should be
        /// updated.
        /// </summary>
        public int PersonSessionId { get; set; }

        /// <summary>
        /// Gets or sets the last-activity date time to record.
        /// </summary>
        public DateTime LastActivityDateTime { get; set; }

        /// <summary>
        /// If this <see cref="LastActivityDateTime"/> hasn't been recently
        /// updated for the target <see cref="PersonSessionId"/>, sends
        /// this message and returns <c>true</c>; otherwise returns
        /// <c>false</c> without touching the bus.
        /// </summary>
        /// <returns><c>true</c> when the message was sent; otherwise, <c>false</c>.</returns>
        public bool SendIfNeeded()
        {
            var previousLastActivityDateTime = PreviousLastActivityDateTimeBySessionId.GetValueOrNull( this.PersonSessionId );

            if ( !NeedsToBeUpdated( previousLastActivityDateTime, this.LastActivityDateTime ) )
            {
                return false;
            }

            this.Send();
            PreviousLastActivityDateTimeBySessionId[this.PersonSessionId] = this.LastActivityDateTime;

            return true;
        }
    }
}
