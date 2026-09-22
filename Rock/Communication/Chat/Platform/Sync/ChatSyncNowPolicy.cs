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

using Rock.Communication.Chat.Platform.Configuration;
using Rock.ViewModels.Blocks.Communication.Chat.ChatSyncNow;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What Sync Now on a chat block may do, and what it reports afterwards. Kept apart from the
    /// blocks so a test can reach both questions: a block supplies the stored settings, the caller's
    /// authority and what Rock's own job tables say, and this decides.
    /// </summary>
    /// <remarks>
    /// A press does not run the sync itself. It asks Rock to run the Chat Platform Sync job now, the
    /// same request the Jobs Administration page makes, so the run is the job's own: one run at a time
    /// across every server, recorded in the job's history, and treated as a person's run rather than
    /// the schedule's.
    /// </remarks>
    internal static class ChatSyncNowPolicy
    {
        #region Messages

        private const string NeverEnabledMessage = "Chat is not set up for this church, so there is nothing to sync.";

        private const string UnreadableKeyMessage = "Chat is set up for this church, but this installation cannot read the signing key it was given, so it cannot sync.";

        private const string ForbiddenMessage = "You are not authorized to sync chat from here.";

        private const string MissingJobMessage = "The Chat Platform Sync job is missing, so there is nothing to run.";

        private const string WaitingMessage = "Waiting for the sync to start.";

        private const string RunningMessage = "The sync is running.";

        // What Rock records for a run that ended well. Anything else it records for an ended run, a
        // warning, an exception or a job that could not be loaded, is a run that did not.
        private const string SuccessStatus = "Success";

        #endregion Messages

        /// <summary>
        /// Answers a press.
        /// </summary>
        /// <param name="configuration">The church's chat settings as stored.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="job">What the job tables say about the sync job, or null when its row is missing.</param>
        /// <param name="queueRunNow">Asks Rock to run the job with the given id now.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        /// <remarks>
        /// Authority is asked first, so a caller who may not press the button learns nothing about the
        /// state of chat from pressing it.
        /// </remarks>
        public static Result Request( ChatPlatformConfiguration configuration, bool isAuthorized, JobSnapshot job, Action<int> queueRunNow )
        {
            if ( !isAuthorized )
            {
                return new Result { RefusalMessage = ForbiddenMessage, IsForbidden = true };
            }

            var stored = configuration ?? new ChatPlatformConfiguration();
            if ( !stored.IsConfigured )
            {
                return new Result { RefusalMessage = stored.HasBeenEnabled ? UnreadableKeyMessage : NeverEnabledMessage };
            }

            if ( job == null )
            {
                return new Result { RefusalMessage = MissingJobMessage };
            }

            if ( job.IsRunning )
            {
                // Rock would refuse a second run while this one holds the lock, and refuse it without a
                // word, so the press follows the run already going instead. Where the newest record is
                // still open it is that run; where it has ended, the run holding the lock has not been
                // recorded yet and will be the next record.
                var isInFlightRunRecorded = job.LatestRunId.HasValue && !job.IsLatestRunEnded;

                return Waiting( isInFlightRunRecorded ? job.LatestRunId.Value - 1 : job.LatestRunId ?? 0 );
            }

            queueRunNow( job.JobId );

            return Waiting( job.LatestRunId ?? 0 );
        }

        /// <summary>
        /// Answers a check on a press already made.
        /// </summary>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="runMarker">The marker the press returned.</param>
        /// <param name="run">The first run of the sync job recorded after that marker, or null when there is none yet.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        public static Result Status( bool isAuthorized, int runMarker, RunSnapshot run )
        {
            if ( !isAuthorized )
            {
                return new Result { RefusalMessage = ForbiddenMessage, IsForbidden = true };
            }

            // Checked here as well as by whoever read the run, because the one thing this must never do
            // is report a run that ended before the press as the press's own result.
            if ( run == null || run.Id <= runMarker )
            {
                return Waiting( runMarker );
            }

            if ( !run.HasEnded )
            {
                return new Result
                {
                    Status = new ChatSyncNowStatusBag { RunMarker = runMarker, Message = RunningMessage }
                };
            }

            return new Result
            {
                Status = new ChatSyncNowStatusBag
                {
                    RunMarker = runMarker,
                    IsFinished = true,
                    IsFailure = !string.Equals( run.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase ),
                    Message = run.StatusMessage
                }
            };
        }

        /// <summary>
        /// Whether a group type's chat settings are shown at all.
        /// </summary>
        /// <param name="isPreviousProviderEnabled">Whether the chat provider Rock shipped before this one is configured.</param>
        /// <param name="configuration">The church's settings for this chat platform.</param>
        /// <returns>True when either provider is configured.</returns>
        public static bool IsChatSectionShown( bool isPreviousProviderEnabled, ChatPlatformConfiguration configuration )
        {
            return isPreviousProviderEnabled || IsChatPlatformConfigured( configuration );
        }

        /// <summary>
        /// Whether this chat platform can run here, which is when Sync Now and its wording are shown.
        /// </summary>
        /// <param name="configuration">The church's settings for this chat platform.</param>
        /// <returns>True when this platform is configured.</returns>
        public static bool IsChatPlatformConfigured( ChatPlatformConfiguration configuration )
        {
            return configuration != null && configuration.IsConfigured;
        }

        /// <summary>
        /// A press that has not yet reached a run that ended.
        /// </summary>
        /// <param name="runMarker">The marker the press is reported from.</param>
        /// <returns>The answer.</returns>
        private static Result Waiting( int runMarker )
        {
            return new Result
            {
                Status = new ChatSyncNowStatusBag { RunMarker = runMarker, Message = WaitingMessage }
            };
        }

        /// <summary>
        /// What a press or a check comes to.
        /// </summary>
        internal sealed class Result
        {
            /// <summary>
            /// Gets or sets why nothing was done, or null where the answer is a status.
            /// </summary>
            public string RefusalMessage { get; set; }

            /// <summary>
            /// Gets or sets whether the refusal is about the caller's authority rather than the state of chat.
            /// </summary>
            public bool IsForbidden { get; set; }

            /// <summary>
            /// Gets or sets where the press has got to, when it was not refused.
            /// </summary>
            public ChatSyncNowStatusBag Status { get; set; }

            /// <summary>
            /// Gets whether nothing was done.
            /// </summary>
            public bool IsRefused => RefusalMessage != null;
        }

        /// <summary>
        /// What Rock's job tables say about the sync job at the moment of a press.
        /// </summary>
        internal sealed class JobSnapshot
        {
            /// <summary>
            /// Gets or sets the job's id.
            /// </summary>
            public int JobId { get; set; }

            /// <summary>
            /// Gets or sets whether a run of the job holds its lock right now, on any server.
            /// </summary>
            public bool IsRunning { get; set; }

            /// <summary>
            /// Gets or sets the id of the newest run recorded in the job's history, or null when it has never run.
            /// </summary>
            public int? LatestRunId { get; set; }

            /// <summary>
            /// Gets or sets whether that newest run has ended.
            /// </summary>
            public bool IsLatestRunEnded { get; set; }
        }

        /// <summary>
        /// One run of the sync job as its history records it.
        /// </summary>
        internal sealed class RunSnapshot
        {
            /// <summary>
            /// Gets or sets the history record's id.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets whether the run has ended.
            /// </summary>
            public bool HasEnded { get; set; }

            /// <summary>
            /// Gets or sets the status Rock recorded for the run.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Gets or sets the result the run left.
            /// </summary>
            public string StatusMessage { get; set; }
        }
    }
}
