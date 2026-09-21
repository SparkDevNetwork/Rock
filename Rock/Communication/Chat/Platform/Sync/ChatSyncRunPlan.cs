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
    /// What a sync run decided to do before it did any work.
    /// </summary>
    internal sealed class ChatSyncRunPlan
    {
        /// <summary>
        /// Whether this run reads the church's data and submits it.
        /// </summary>
        public bool ShouldSubmit { get; set; }

        /// <summary>
        /// Why this run did nothing. Null where it went ahead.
        /// </summary>
        public string SkipMessage { get; set; }

        /// <summary>
        /// What is worth saying about the schedule this run was started on. Null where there is
        /// nothing to say. Carried whether or not the run went ahead.
        /// </summary>
        public string CadenceWarning { get; set; }
    }
}
