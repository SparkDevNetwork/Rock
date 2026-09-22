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
        /// <summary>
        /// Answers a press.
        /// </summary>
        /// <param name="configuration">The church's chat settings as stored.</param>
        /// <param name="isAuthorized">Whether the caller may save on the block the button sits on.</param>
        /// <param name="job">What the job tables say about the sync job, or null when its row is missing.</param>
        /// <param name="queueRunNow">Asks Rock to run the job with the given id now.</param>
        /// <returns>A refusal, or where the press has got to.</returns>
        public static Result Request( ChatPlatformConfiguration configuration, bool isAuthorized, JobSnapshot job, Action<int> queueRunNow )
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Whether a group type's chat settings are shown at all.
        /// </summary>
        /// <param name="isPreviousProviderEnabled">Whether the chat provider Rock shipped before this one is configured.</param>
        /// <param name="configuration">The church's settings for this chat platform.</param>
        /// <returns>True when either provider is configured.</returns>
        public static bool IsChatSectionShown( bool isPreviousProviderEnabled, ChatPlatformConfiguration configuration )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Whether this chat platform can run here, which is when Sync Now and its wording are shown.
        /// </summary>
        /// <param name="configuration">The church's settings for this chat platform.</param>
        /// <returns>True when this platform is configured.</returns>
        public static bool IsChatPlatformConfigured( ChatPlatformConfiguration configuration )
        {
            throw new NotImplementedException();
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
