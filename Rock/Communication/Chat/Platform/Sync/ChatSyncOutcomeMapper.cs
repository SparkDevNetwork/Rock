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
    /// Turns a recorded submission status into the answers a sync run needs from it.
    /// </summary>
    /// <remarks>
    /// Every arm is written out rather than left to a default, because the two statuses that are
    /// easy to forget, applied and failed, are the two a run sees least often and the two that
    /// matter most when it does. A default arm sends one of them to the wrong answer and nothing
    /// says so until a church's drain fails and its job reports a success.
    /// </remarks>
    internal static class ChatSyncOutcomeMapper
    {
        /// <summary>
        /// The HTTP status the chat platform answers with for this recorded status. The platform
        /// sets it from the status rather than from whether the call worked, so a refusal arrives
        /// looking like one.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>The HTTP status code.</returns>
        public static int HttpStatusFor( ChatSyncSubmissionStatus status )
        {
            switch ( status )
            {
                case ChatSyncSubmissionStatus.Accepted:
                case ChatSyncSubmissionStatus.Applied:
                    return 200;

                case ChatSyncSubmissionStatus.Refused:
                case ChatSyncSubmissionStatus.Failed:
                    return 422;

                default:
                    throw new ArgumentOutOfRangeException( nameof( status ), status, "no HTTP status is mapped for this recorded status" );
            }
        }

        /// <summary>
        /// Whether a sync run that saw this status worked.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>True where the run worked.</returns>
        /// <remarks>
        /// Accepted counts as working. The restatement is stored and queued, and whether the queue
        /// has reached it yet is the drain's business rather than a fault of the run that submitted
        /// it.
        /// </remarks>
        public static bool IsJobSuccess( ChatSyncSubmissionStatus status )
        {
            switch ( status )
            {
                case ChatSyncSubmissionStatus.Accepted:
                case ChatSyncSubmissionStatus.Applied:
                    return true;

                case ChatSyncSubmissionStatus.Refused:
                case ChatSyncSubmissionStatus.Failed:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException( nameof( status ), status, "no run outcome is mapped for this recorded status" );
            }
        }

        /// <summary>
        /// The label the wire carries for this status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>The wire label.</returns>
        public static string WireValueFor( ChatSyncSubmissionStatus status )
        {
            return status.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The status a wire label names, or null where this build of Rock does not know it.
        /// </summary>
        /// <param name="value">The wire label.</param>
        /// <returns>The status, or null.</returns>
        /// <remarks>
        /// A label added to the platform's vocabulary after this build shipped reads as nothing
        /// rather than as the nearest guess. A run that reports it does not know beats a run that
        /// reports a success it has no evidence for.
        /// </remarks>
        public static ChatSyncSubmissionStatus? ParseStatus( string value )
        {
            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            ChatSyncSubmissionStatus parsed;
            if ( !Enum.TryParse( value, true, out parsed ) || !Enum.IsDefined( typeof( ChatSyncSubmissionStatus ), parsed ) )
            {
                return null;
            }

            // Enum.TryParse takes a number as well as a name, and a number is not a label the wire
            // could ever have carried.
            return WireValueFor( parsed ) == value.ToLowerInvariant() ? parsed : ( ChatSyncSubmissionStatus? ) null;
        }
    }
}
