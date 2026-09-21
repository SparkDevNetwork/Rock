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
using System.Globalization;

using Quartz;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Reads a schedule and works out the longest quiet stretch it leaves.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The chat platform treats an absent restatement as a church that has gone quiet, and
    ///         its own liveness views are built on that expectation, so a schedule leaving more than
    ///         a day between runs is worth a church knowing about. Knowing about it is all this
    ///         does: the expression belongs to the administrator who typed it and nothing here
    ///         rewrites it.
    ///     </para>
    ///     <para>
    ///         The longest gap rather than the next one, because the schedules that go wrong quietly
    ///         are the ones that look frequent. A weekday morning schedule fires five times a week
    ///         and leaves seventy two hours over every weekend, and the gap after any given Monday
    ///         run is a reassuring twenty four hours.
    ///     </para>
    /// </remarks>
    internal static class ChatSyncCadence
    {
        #region Constants

        /// <summary>
        /// The longest quiet stretch a schedule may leave before this is worth saying. A design
        /// bound on how stale the platform's picture of a church may get, not a measurement, and
        /// provisional until the platform is measured at full scale.
        /// </summary>
        public static readonly TimeSpan Maximum = TimeSpan.FromHours( 24 );

        /// <summary>
        /// How many fire times to look at. Enough to walk a weekly pattern round to its own repeat,
        /// which is the longest shape that hides its gap; anything slower than weekly shows its gap
        /// on the first step. A bounded sample chosen by that reasoning rather than a proof, and
        /// cheap enough to take on every run.
        /// </summary>
        private const int SampleSize = 14;

        #endregion Constants

        #region Methods

        /// <summary>
        /// The longest gap between consecutive runs of this schedule, looking forward from a moment.
        /// </summary>
        /// <param name="cronExpression">The schedule.</param>
        /// <param name="after">The moment to look forward from.</param>
        /// <returns>The longest gap, or null where the schedule cannot be read or has no future runs.</returns>
        public static TimeSpan? LongestGap( string cronExpression, DateTimeOffset after )
        {
            if ( cronExpression.IsNullOrWhiteSpace() )
            {
                return null;
            }

            CronExpression expression;
            try
            {
                expression = new CronExpression( cronExpression );
            }
            catch ( Exception )
            {
                // A schedule this job cannot read is the scheduler's to complain about. It has
                // already refused to run on it, or it is running on something this does not
                // understand; either way a second opinion from here would only be noise.
                return null;
            }

            // Measured in elapsed time rather than wall clock. A daily schedule genuinely spans
            // twenty five hours on the morning the clocks go back, and reporting a church's
            // schedule as too slow once a year for that reason would be wrong every time.
            expression.TimeZone = TimeZoneInfo.Utc;

            var previous = after;
            TimeSpan? longest = null;

            for ( var step = 0; step < SampleSize; step++ )
            {
                var next = expression.GetNextValidTimeAfter( previous );
                if ( !next.HasValue )
                {
                    break;
                }

                var gap = next.Value - previous;
                if ( !longest.HasValue || gap > longest.Value )
                {
                    longest = gap;
                }

                previous = next.Value;
            }

            return longest;
        }

        /// <summary>
        /// What is worth saying about this schedule, or null where there is nothing.
        /// </summary>
        /// <param name="cronExpression">The schedule.</param>
        /// <param name="after">The moment to look forward from.</param>
        /// <returns>The warning, or null.</returns>
        public static string Warning( string cronExpression, DateTimeOffset after )
        {
            var longest = LongestGap( cronExpression, after );
            if ( !longest.HasValue || longest.Value <= Maximum )
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "This schedule leaves up to {0} between runs. Chat is meant to be restated at least every 24 hours, "
                    + "and a longer gap leaves the chat platform holding a stale picture of this church and reading it as offline. "
                    + "The schedule has not been changed.",
                Describe( longest.Value ) );
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// A gap in the roundest words it fits, because a church reads this on a job page.
        /// </summary>
        private static string Describe( TimeSpan gap )
        {
            if ( gap.TotalDays >= 2 )
            {
                return string.Format( CultureInfo.InvariantCulture, "{0:0.#} days", gap.TotalDays );
            }

            return string.Format( CultureInfo.InvariantCulture, "{0:0.#} hours", gap.TotalHours );
        }

        #endregion Private Methods
    }
}
