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
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Rock.Bus.Locking;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tasks;
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

        private static readonly string NeverEnabledMessage = "Chat is not set up for this church, so there is nothing to sync.";

        private static readonly string UnreadableKeyMessage = "Chat is set up for this church, but this installation cannot read the signing key it was given, so it cannot sync.";

        private static readonly string ForbiddenMessage = "You are not authorized to sync chat from here.";

        private static readonly string MissingJobMessage = "The Chat Platform Sync job is missing, so there is nothing to run.";

        private static readonly string AlreadyRunningMessage = "A sync is already running. Press Sync Now again when it has finished.";

        private static readonly string WaitingMessage = "Waiting for the sync to start.";

        private static readonly string RunningMessage = "The sync is running.";

        // What Rock records for a run that ended well. Anything else it records for an ended run, a
        // warning, an exception or a job that could not be loaded, is a run that did not.
        private const string SuccessStatus = "Success";

        #endregion Messages

        /// <summary>
        /// Answers a press.
        /// </summary>
        /// <param name="configuration">The church's chat settings as stored.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="readJob">Reads what the job tables say about the sync job, or null when its row is missing.</param>
        /// <param name="queueRunNow">Asks Rock to run the job with the given id now.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        /// <remarks>
        /// Authority is asked first, so a caller who may not press the button learns nothing about the
        /// state of chat from pressing it, and the job is read last, only for a press that may go ahead.
        /// </remarks>
        public static Result Request( ChatPlatformConfiguration configuration, bool isAuthorized, Func<JobSnapshot> readJob, Action<int> queueRunNow )
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

            // Read only now. Reading the job probes its lock, and a probe on the instant the schedule
            // fires takes the lock first and costs the church that run, which a press about to be
            // refused has no business doing.
            var job = readJob();
            if ( job == null )
            {
                return new Result { RefusalMessage = MissingJobMessage };
            }

            if ( job.IsRunning )
            {
                // Refused rather than followed. Rock would drop a second run without a word while this
                // one holds the lock, and whatever holds it read the church before this press, so its
                // result would answer an older question: often one asked before the Save this press
                // exists to send. The lock is also held after a run's record has ended, while Rock
                // sends the job's notification, so following "the next record" could wait on nothing.
                return new Result { RefusalMessage = AlreadyRunningMessage };
            }

            queueRunNow( job.JobId );

            return Waiting( job.LatestRunId ?? 0 );
        }

        /// <summary>
        /// Answers a check on a press already made.
        /// </summary>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="runMarker">The marker the press returned.</param>
        /// <param name="readRun">Reads the first run of the sync job recorded after that marker, or null when there is none yet.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        public static Result Status( bool isAuthorized, int runMarker, Func<RunSnapshot> readRun )
        {
            if ( !isAuthorized )
            {
                return new Result { RefusalMessage = ForbiddenMessage, IsForbidden = true };
            }

            var run = readRun();

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

        #region Reading Rock's job tables

        /// <summary>
        /// Reads what a press needs to know about the sync job.
        /// </summary>
        /// <param name="rockContext">The context to read through.</param>
        /// <returns>The job as it stands, or null when its row is missing.</returns>
        public static JobSnapshot ReadJob( RockContext rockContext )
        {
            var jobId = ReadJobId( rockContext );
            if ( !jobId.HasValue )
            {
                return null;
            }

            var latest = new ServiceJobHistoryService( rockContext ).Queryable()
                .Where( history => history.ServiceJobId == jobId.Value )
                .OrderByDescending( history => history.Id )
                .Select( history => new { history.Id, history.StopDateTime } )
                .FirstOrDefault();

            return new JobSnapshot
            {
                JobId = jobId.Value,
                IsRunning = IsJobLocked( jobId.Value ),
                LatestRunId = latest?.Id,
                IsLatestRunEnded = latest == null || latest.StopDateTime.HasValue
            };
        }

        /// <summary>
        /// Reads the first run of the sync job recorded after the given marker.
        /// </summary>
        /// <param name="rockContext">The context to read through.</param>
        /// <param name="runMarker">The marker a press returned.</param>
        /// <returns>The run, or null when none has been recorded since, or the job's row is missing.</returns>
        public static RunSnapshot ReadRunAfter( RockContext rockContext, int runMarker )
        {
            var jobId = ReadJobId( rockContext );
            if ( !jobId.HasValue )
            {
                return null;
            }

            return new ServiceJobHistoryService( rockContext ).Queryable()
                .Where( history => history.ServiceJobId == jobId.Value && history.Id > runMarker )
                .OrderBy( history => history.Id )
                .Select( history => new RunSnapshot
                {
                    Id = history.Id,
                    HasEnded = history.StopDateTime.HasValue,
                    Status = history.Status,
                    StatusMessage = history.StatusMessage
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Asks Rock to run the job now, exactly as the Jobs Administration page does.
        /// </summary>
        /// <param name="jobId">The job's id.</param>
        /// <remarks>
        /// That page's request is what gives the run its own scheduler, and the sync job reads the
        /// scheduler's name to know a person is waiting on it, so it marks the submission urgent and
        /// does not hold it back for the platform's backoff. Running the job any other way would lose both.
        /// </remarks>
        public static void QueueRunNow( int jobId )
        {
            new ProcessRunJobNow.Message { JobId = jobId }.Send();
        }

        /// <summary>
        /// The sync job's id, or null when its row is missing.
        /// </summary>
        /// <param name="rockContext">The context to read through.</param>
        /// <returns>The id.</returns>
        private static int? ReadJobId( RockContext rockContext )
        {
            var jobGuid = Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC_JOB.AsGuid();

            return new ServiceJobService( rockContext ).Queryable()
                .Where( job => job.Guid == jobGuid )
                .Select( job => ( int? ) job.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Whether a run of the job holds its lock right now, on any server.
        /// </summary>
        /// <param name="jobId">The job's id.</param>
        /// <returns>True when a run holds it.</returns>
        /// <remarks>
        /// The lock rather than the history record, because a run cut off by a restart leaves its record
        /// open forever, and a press that trusted the record would follow that dead run and never start
        /// another. This is the same probe Rock's own Run Now makes before it starts a job, held for no
        /// longer than it takes to ask.
        /// </remarks>
        private static bool IsJobLocked( int jobId )
        {
            var lockProvider = RockApp.Current.GetRequiredService<IDistributedLockProvider>();

            using ( var probe = lockProvider.TryAcquire( typeof( RockTriggerListener ), jobId.ToString(), TimeSpan.Zero ) )
            {
                return !probe.IsAcquired;
            }
        }

        #endregion Reading Rock's job tables

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
