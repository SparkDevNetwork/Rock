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
    /// Turns a recorded submission status into the answers a job needs from it.
    /// </summary>
    internal static class ChatSyncOutcomeMapper
    {
        /// <summary>
        /// The HTTP status the chat platform answers with for this recorded status.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>The HTTP status code.</returns>
        public static int HttpStatusFor( ChatSyncSubmissionStatus status )
        {
            return status == ChatSyncSubmissionStatus.Refused ? 422 : 200;
        }

        /// <summary>
        /// Whether a job run that saw this status worked.
        /// </summary>
        /// <param name="status">The recorded status.</param>
        /// <returns>True where the run worked.</returns>
        public static bool IsJobSuccess( ChatSyncSubmissionStatus status )
        {
            return status != ChatSyncSubmissionStatus.Refused;
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

            return parsed;
        }
    }
}
