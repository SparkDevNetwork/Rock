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
namespace Rock.ViewModels.Blocks.Communication.Chat.ChatSyncNow
{
    /// <summary>
    /// Where a Sync Now press has got to, as the chat blocks that offer it are told. The same shape
    /// answers the press and every check on it afterwards.
    /// </summary>
    public class ChatSyncNowStatusBag
    {
        /// <summary>
        /// Gets or sets the point in the sync job's run history this press is reported from. The run
        /// being reported is the first one recorded after it, and the screen hands it back on each
        /// check so that a run which finished before the press is never shown as its result.
        /// </summary>
        public int RunMarker { get; set; }

        /// <summary>
        /// Gets or sets whether the run has ended, successfully or not.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// Gets or sets whether the run that ended reported a failure.
        /// </summary>
        public bool IsFailure { get; set; }

        /// <summary>
        /// Gets or sets what to show: the run's own result once it has ended, and where it has got
        /// to until then.
        /// </summary>
        public string Message { get; set; }
    }
}
