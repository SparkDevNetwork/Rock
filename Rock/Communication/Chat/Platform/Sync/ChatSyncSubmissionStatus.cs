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
namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What the chat platform recorded about one submission.
    /// </summary>
    /// <remarks>
    /// The platform's own vocabulary, mirrored here so the four are a type rather than four
    /// strings. A submission is accepted when it passed the checks and was queued, refused when a
    /// check turned it away, applied when the queue drained it, and failed when draining it did not
    /// work often enough to give up. A value Rock does not know is never guessed at.
    /// </remarks>
    internal enum ChatSyncSubmissionStatus
    {
        /// <summary>
        /// Stored and queued. Nothing has been written to the church's chat tables yet.
        /// </summary>
        Accepted,

        /// <summary>
        /// Turned away at submission, with a named reason. Nothing was queued.
        /// </summary>
        Refused,

        /// <summary>
        /// Drained into the church's chat tables.
        /// </summary>
        Applied,

        /// <summary>
        /// Drained repeatedly and never worked. The payload is kept for whoever looks into it.
        /// </summary>
        Failed
    }
}
