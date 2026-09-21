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
    internal sealed class ChatSyncAcknowledgement
    {
        public Guid SubmissionId { get; set; }

        public ChatSyncSubmissionStatus? Status { get; set; }

        public string ErrorCode { get; set; }

        public int? HttpStatusCode { get; set; }

        public ChatSyncOutcome PreviousOutcome { get; set; }

        public DateTimeOffset? SyncBackoffUntil { get; set; }

        public string TransportDetail { get; set; }

        public bool IsTransportFailure => !HttpStatusCode.HasValue;

        public bool CarriesBackoffAdvice => !IsTransportFailure && Status.HasValue;

        public bool IsJobSuccess => Status.HasValue && ChatSyncOutcomeMapper.IsJobSuccess( Status.Value );

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
