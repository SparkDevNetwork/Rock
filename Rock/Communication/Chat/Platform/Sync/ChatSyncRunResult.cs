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
    /// What one sync run has to say for itself.
    /// </summary>
    internal sealed class ChatSyncRunResult
    {
        /// <summary>
        /// Whether the run should be recorded as a failure.
        /// </summary>
        public bool IsFailure { get; set; }

        /// <summary>
        /// What the run puts on its own result line, in words an administrator can act on.
        /// </summary>
        public string Message { get; set; }
    }
}
