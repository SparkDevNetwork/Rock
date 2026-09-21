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
    /// What came back from a submission.
    /// </summary>
    /// <remarks>
    /// Returned for a refusal exactly as for an acceptance, because the chat platform answers a
    /// refusal with a body and a status rather than an error. Nothing here is a reason to throw: a
    /// job has a result message to write and a backoff to honour, and it can do neither from a
    /// stack trace.
    /// </remarks>
    internal sealed class ChatSyncAcknowledgement
    {
        /// <summary>
        /// The submission this answers.
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// What was recorded, or null where nothing readable came back.
        /// </summary>
        public ChatSyncSubmissionStatus? Status { get; set; }

        /// <summary>
        /// The named reason a refusal carries.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// The HTTP status the answer arrived with, or null where no answer arrived.
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// The church's previous submission and what became of it.
        /// </summary>
        public ChatSyncOutcome PreviousOutcome { get; set; }

        /// <summary>
        /// The time the platform would rather not hear from this church before.
        /// </summary>
        public DateTimeOffset? SyncBackoffUntil { get; set; }

        /// <summary>
        /// Why the submission never got an answer. Null where one arrived.
        /// </summary>
        public string TransportDetail { get; set; }

        /// <summary>
        /// True where no answer arrived at all.
        /// </summary>
        public bool IsTransportFailure => !HttpStatusCode.HasValue;

        /// <summary>
        /// Whether what came back is the platform's own word, and so whether the backoff it
        /// carries, or the absence of one, is worth recording.
        /// </summary>
        /// <remarks>
        /// A submission that never arrived, or that was answered by something other than the
        /// platform, says nothing about the platform's load. Recording its silence as "no backoff"
        /// would lift advice the platform had given.
        /// </remarks>
        public bool CarriesBackoffAdvice => !IsTransportFailure && Status.HasValue;

        /// <summary>
        /// Whether the run that made this submission worked.
        /// </summary>
        public bool IsJobSuccess => Status.HasValue && ChatSyncOutcomeMapper.IsJobSuccess( Status.Value );

        /// <summary>
        /// A submission that never reached the platform.
        /// </summary>
        /// <param name="submissionId">The submission that was attempted.</param>
        /// <param name="detail">What the transport said.</param>
        /// <returns>The acknowledgement.</returns>
        public static ChatSyncAcknowledgement Unreachable( Guid submissionId, string detail )
        {
            return new ChatSyncAcknowledgement
            {
                SubmissionId = submissionId,
                TransportDetail = detail
            };
        }
    }
}
