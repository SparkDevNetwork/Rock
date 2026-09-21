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

using Rock.Model;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Decides whether a sync run goes ahead, before it reads anything.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The backoff is the chat platform's advice about its own load, and it binds the
    ///         schedule but not a person. Someone who pressed Sync Now is at a screen waiting for an
    ///         answer and would otherwise be told only that the run declined to happen; the cost of
    ///         letting them through is one submission the platform would rather have had later.
    ///     </para>
    ///     <para>
    ///         The cadence is the church's own setting and is remarked on rather than corrected.
    ///         Nothing in this file writes to the job.
    ///     </para>
    /// </remarks>
    internal static class ChatSyncRunGate
    {
        /// <summary>
        /// Plans one run.
        /// </summary>
        /// <param name="job">The scheduled job this run belongs to. Read, never written.</param>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <param name="backoffUntil">The time the chat platform last asked not to be called before.</param>
        /// <param name="now">The current time.</param>
        /// <returns>The plan. Never null.</returns>
        public static ChatSyncRunPlan Plan( ServiceJob job, bool isManualRun, DateTimeOffset? backoffUntil, DateTimeOffset now )
        {
            var plan = new ChatSyncRunPlan
            {
                ShouldSubmit = true,
                CadenceWarning = ChatSyncCadence.Warning( job?.CronExpression, now )
            };

            if ( isManualRun || !backoffUntil.HasValue || backoffUntil.Value <= now )
            {
                return plan;
            }

            plan.ShouldSubmit = false;
            plan.SkipMessage = string.Format(
                CultureInfo.InvariantCulture,
                "Nothing was submitted. The chat platform asked for a backoff until {0}, and this run was started by the schedule rather than by a person. Sync Now ignores the backoff.",
                backoffUntil.Value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss'Z'", CultureInfo.InvariantCulture ) );

            return plan;
        }
    }
}
