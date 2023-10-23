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

namespace Rock.ViewModels.Blocks.Core.Notes
{
    /// <summary>
    /// Describes a request to watch or unwatch an existing note.
    /// </summary>
    public class WatchNoteRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier of the note to be watched or unwatched.
        /// </summary>
        /// <value>The identifier of the note to be watched or unwatched.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the note should be watched or not.
        /// </summary>
        /// <value><c>true</c> if the note should be watched; otherwise <c>false</c>.</value>
        public bool IsWatching { get; set; }
    }
}
