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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to add or edit the note on a connection request status history
    /// record (the note explaining why a request moved from one status to another).
    /// </summary>
    public class SaveStatusHistoryNoteBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request status history record whose note is being saved.
        /// </summary>
        public string StatusHistoryIdKey { get; set; }

        /// <summary>
        /// Gets or sets the text content of the note. An empty value clears the note unless the ended status requires one.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the activity feed entry key of the status change this note belongs to. It is echoed back
        /// unchanged in the response so the client can locate and refresh the corresponding entry in the feed.
        /// </summary>
        public string Key { get; set; }
    }
}
