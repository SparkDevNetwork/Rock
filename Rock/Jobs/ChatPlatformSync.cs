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
using System.ComponentModel;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends this church's people, channels, memberships and badges to the chat platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every run sends the whole picture rather than what changed, so a run that does not
    ///         happen costs nothing the next one cannot put right, and a run that happens twice
    ///         writes the same thing twice. That is what lets this job give up early, skip itself
    ///         when the platform asks for quiet, and be pressed by hand as often as anyone likes.
    ///     </para>
    ///     <para>
    ///         Named apart from the Chat Sync job, which belongs to the other chat provider. The two
    ///         appear side by side on the Jobs Administration page and do entirely different things,
    ///         so they must not be read as one job under two names.
    ///     </para>
    /// </remarks>
    [DisplayName( "Chat Platform Sync" )]
    [Description( "Sends this church's people, channels, memberships and badges to the chat platform, as a whole picture each time." )]
    public class ChatPlatformSync : RockJob
    {
        #region Constants

        /// <summary>
        /// A run started by hand gets its own scheduler, named this way by the page that starts it.
        /// It is the only thing here that tells a person's run from the schedule's, and the
        /// difference matters twice: a person is waiting, so the platform's backoff does not hold
        /// them up, and their submission is marked urgent so it is drained ahead of the queue.
        /// </summary>
        private const string ManualRunSchedulerPrefix = "RunNow:";

        #endregion Constants

        #region Methods

        /// <inheritdoc />
        public override void Execute()
        {
            var configuration = ChatPlatformConfigurationService.Read();

            if ( !configuration.IsConfigured )
            {
                Result = configuration.HasBeenEnabled
                    ? "Nothing was sent. Chat is set up for this church, but this installation cannot read the signing key it was given."
                    : "Nothing was sent. Chat is not set up for this church.";
                return;
            }

            var isManualRun = IsManualRun();

            // The instant, not the organisation's wall clock. The backoff the gate compares against
            // is an instant the platform named with its offset, and a wall-clock reading handed to
            // the gate would be stamped with this server's offset, which on a hosted server is not
            // the organisation's, and be wrong by the difference.
            var plan = ChatSyncRunGate.Plan( ServiceJob, isManualRun, configuration.SyncBackoffUntil, DateTimeOffset.UtcNow );

            if ( !plan.ShouldSubmit )
            {
                Result = Join( plan.SkipMessage, plan.CadenceWarning );
                return;
            }

            ChatSyncRunResult outcome;
            using ( var rockContext = new RockContext() )
            {
                outcome = new ChatSyncRunner( configuration, isManualRun ).Run( rockContext );
            }

            Result = Join( outcome.Message, plan.CadenceWarning );

            if ( outcome.IsFailure )
            {
                // Thrown rather than returned, because the scheduler is what records a run as
                // failed and it only learns that from an exception. The message is already on the
                // result, so this carries no detail the church has not been shown.
                throw new RockJobWarningException( Result );
            }
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Whether a person started this run.
        /// </summary>
        private bool IsManualRun()
        {
            var schedulerName = Scheduler?.SchedulerName;

            return schedulerName != null
                && schedulerName.StartsWith( ManualRunSchedulerPrefix, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Puts the run's own sentence and the remark about its schedule together, in that order,
        /// skipping whichever is absent.
        /// </summary>
        private static string Join( string outcome, string cadenceWarning )
        {
            if ( cadenceWarning.IsNullOrWhiteSpace() )
            {
                return outcome;
            }

            return outcome.IsNullOrWhiteSpace() ? cadenceWarning : outcome + " " + cadenceWarning;
        }

        #endregion Private Methods
    }
}
