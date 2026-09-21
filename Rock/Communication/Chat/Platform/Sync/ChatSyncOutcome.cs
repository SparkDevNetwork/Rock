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

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What became of one submission.
    /// </summary>
    /// <remarks>
    /// One shape for two readings: the answer to a direct status read, and the previous
    /// submission's result carried on every acknowledgement. They are the same four fields because
    /// the platform builds them from the same history row, and a job that had to hold two shapes
    /// for one fact would have two ways of describing it.
    /// </remarks>
    internal sealed class ChatSyncOutcome
    {
        /// <summary>
        /// The submission this describes.
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// What was recorded, or null where the platform named a status this build of Rock does not
        /// know.
        /// </summary>
        public ChatSyncSubmissionStatus? Status { get; set; }

        /// <summary>
        /// The named reason a refused or failed submission carries. Null on the other two.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// When the queue drained it. Null until it is applied.
        /// </summary>
        public DateTimeOffset? DrainedAt { get; set; }
    }
}
