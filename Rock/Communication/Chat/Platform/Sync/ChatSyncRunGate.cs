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

using Rock.Model;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Decides whether a sync run goes ahead, before it reads anything.
    /// </summary>
    internal static class ChatSyncRunGate
    {
        /// <summary>
        /// Plans one run.
        /// </summary>
        /// <param name="job">The scheduled job this run belongs to.</param>
        /// <param name="isManualRun">Whether a person started this run rather than the schedule.</param>
        /// <param name="backoffUntil">The time the chat platform last asked not to be called before.</param>
        /// <param name="now">The current time.</param>
        /// <returns>The plan.</returns>
        public static ChatSyncRunPlan Plan( ServiceJob job, bool isManualRun, DateTimeOffset? backoffUntil, DateTimeOffset now )
        {
            return new ChatSyncRunPlan { ShouldSubmit = true };
        }
    }
}
