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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a note update operation in a bulk update save request.
    /// </summary>
    public class BulkUpdateNoteBag
    {
        /// <summary>
        /// Gets or sets the note type unique identifier.
        /// </summary>
        public ListItemBag NoteType { get; set; }

        /// <summary>
        /// Gets or sets the text for the note.
        /// </summary>
        public string NoteText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note is an alert.
        /// </summary>
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note is private.
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
