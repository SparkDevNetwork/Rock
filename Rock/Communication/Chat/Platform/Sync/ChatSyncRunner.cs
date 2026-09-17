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
using System.Threading.Tasks;

using Quartz;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Cadence, backpressure and poll rules for the chat restatement job.
    /// Extracted from the job class so <c>Rock.Tests</c> can pin them; that
    /// project does not reference the blocks assembly and a rule that lived
    /// only there could be reverted with every gate green.
    /// </summary>
    internal static class ChatSyncRunner
    {
        #region Constants

        /// <summary>
        /// Daily at midnight, Quartz 6-field form. Used when the stored cron
        /// would wait more than a day between runs.
        /// </summary>
        internal const string DailyCron = "0 0 0 1/1 * ? *";

        /// <summary>
        /// First poll wait.
        /// </summary>
        internal static readonly TimeSpan InitialPollDelay = TimeSpan.FromSeconds( 2 );

        /// <summary>
        /// Longest single poll wait.
        /// </summary>
        internal static readonly TimeSpan MaxPollDelay = TimeSpan.FromSeconds( 8 );

        /// <summary>
        /// How long a run will wait for the drain before warning.
        /// </summary>
        internal static readonly TimeSpan PollBudget = TimeSpan.FromSeconds( 60 );

        #endregion

        #region Methods

        /// <summary>
        /// If the cron's next two fires are more than 24 hours apart, returns
        /// the daily cron; otherwise returns the input. Unparseable input is
        /// left alone so an administrator's typo stays visible on the job.
        /// </summary>
        internal static string ClampCronExpression( string cronExpression, DateTimeOffset now )
        {
            if ( string.IsNullOrWhiteSpace( cronExpression ) )
            {
                return DailyCron;
            }

            try
            {
                var cron = new CronExpression( cronExpression );
                var first = cron.GetNextValidTimeAfter( now );
                if ( !first.HasValue )
                {
                    return DailyCron;
                }

                var second = cron.GetNextValidTimeAfter( first.Value );
                if ( !second.HasValue )
                {
                    return DailyCron;
                }

                if ( second.Value - first.Value > TimeSpan.FromHours( 24 ) )
                {
                    return DailyCron;
                }
            }
            catch ( Exception )
            {
                return cronExpression;
            }

            return cronExpression;
        }

        /// <summary>
        /// True when a scheduled run should wait. Manual runs never wait.
        /// </summary>
        internal static bool ShouldDelay( DateTime? backoffUntilUtc, bool isManual, DateTime utcNow )
        {
            if ( isManual || !backoffUntilUtc.HasValue )
            {
                return false;
            }

            return backoffUntilUtc.Value > utcNow.ToUniversalTime();
        }

        /// <summary>
        /// Polls until applied, failed, or the budget elapses. Still accepted
        /// at the budget is a warning rather than a failure: the payload is
        /// queued and the next cycle will see it.
        /// </summary>
        internal static async Task<ChatSyncOutcome> PollUntilSettledAsync(
            Func<Task<ChatSyncOutcome>> status,
            Action<TimeSpan> sleep,
            Func<DateTime> utcNow )
        {
            var delay = InitialPollDelay;
            var deadline = utcNow().ToUniversalTime() + PollBudget;
            ChatSyncOutcome latest = null;

            while ( utcNow().ToUniversalTime() < deadline )
            {
                sleep( delay );
                latest = await status().ConfigureAwait( false );
                if ( latest != null && ( latest.IsApplied || latest.IsFailure ) )
                {
                    return latest;
                }

                if ( delay < MaxPollDelay )
                {
                    var doubled = TimeSpan.FromTicks( delay.Ticks * 2 );
                    delay = doubled > MaxPollDelay ? MaxPollDelay : doubled;
                }
            }

            return latest;
        }

        #endregion
    }
}
