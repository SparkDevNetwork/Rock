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
    /// How a submit or status response turns into a job result. Status values
    /// match the platform's recorded status: accepted, refused, applied, failed.
    /// </summary>
    internal sealed class ChatSyncOutcome
    {
        /// <summary>
        /// The submission this outcome is about, when the body named one.
        /// </summary>
        public Guid? SubmissionId { get; set; }

        /// <summary>
        /// The recorded status, or null when the body was not an acknowledgement.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The stable error code, when the platform named one.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Earliest time a further scheduled submit is useful, when the platform
        /// sent one. Manual runs ignore it.
        /// </summary>
        public DateTime? SyncBackoffUntil { get; set; }

        /// <summary>
        /// HTTP status of the call that produced this.
        /// </summary>
        public int HttpStatus { get; set; }

        /// <summary>
        /// Raw body, for the job result text when parsing failed.
        /// </summary>
        public string RawBody { get; set; }

        /// <summary>
        /// True when the HTTP status is 200. Accepted and applied travel this way.
        /// </summary>
        public bool IsHttpSuccess
        {
            get { return HttpStatus >= 200 && HttpStatus < 300; }
        }

        /// <summary>
        /// True when the HTTP status is 422. Refused and failed travel this way.
        /// </summary>
        public bool IsHttpRefusal
        {
            get { return HttpStatus == 422; }
        }

        /// <summary>
        /// True when ingest accepted the payload and the drain has not finished it.
        /// </summary>
        public bool IsPending
        {
            get { return string.Equals( Status, "accepted", StringComparison.Ordinal ); }
        }

        /// <summary>
        /// True when the drain applied the payload.
        /// </summary>
        public bool IsApplied
        {
            get { return string.Equals( Status, "applied", StringComparison.Ordinal ); }
        }

        /// <summary>
        /// True when ingest refused or the drain marked failed. Both are 422.
        /// </summary>
        public bool IsFailure
        {
            get
            {
                return string.Equals( Status, "refused", StringComparison.Ordinal )
                    || string.Equals( Status, "failed", StringComparison.Ordinal )
                    || ( IsHttpRefusal && !IsPending && !IsApplied );
            }
        }

        /// <summary>
        /// One line for the ServiceJob result.
        /// </summary>
        public string ToJobResult()
        {
            if ( string.IsNullOrWhiteSpace( Status ) )
            {
                return string.Format( "Chat sync returned HTTP {0}.", HttpStatus );
            }

            if ( string.IsNullOrWhiteSpace( ErrorCode ) )
            {
                return string.Format( "Chat sync {0}.", Status );
            }

            return string.Format( "Chat sync {0} ({1}).", Status, ErrorCode );
        }
    }
}
