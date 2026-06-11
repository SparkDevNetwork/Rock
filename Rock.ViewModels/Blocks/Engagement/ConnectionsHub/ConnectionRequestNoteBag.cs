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
    /// Represents the data required to create or update a note on a connection request.
    /// </summary>
    public class ConnectionRequestNoteBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request to add or update the note on.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of an existing note to update, or <c>null</c> to create a new note.
        /// </summary>
        public string NoteIdKey { get; set; }

        /// <summary>
        /// Gets or sets the text content of the note.
        /// </summary>
        public string NoteText { get; set; }
    }
}