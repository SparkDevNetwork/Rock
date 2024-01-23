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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.Notes
{
    /// <summary>
    /// Describes a note that is in edit mode.
    /// </summary>
    public class NoteEditBag
    {
        /// <summary>
        /// Gets or sets the identifier of the note.
        /// </summary>
        /// <value>The identifier of the note.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the note type identifier.
        /// </summary>
        /// <value>The note type identifier.</value>
        public string NoteTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the parent note identifier.
        /// </summary>
        /// <value>The parent note identifier.</value>
        public string ParentNoteIdKey { get; set; }

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        /// <value>The text of the note.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is an alert note.
        /// </summary>
        /// <value><c>true</c> if this note is an alert note; otherwise, <c>false</c>.</value>
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is private.
        /// </summary>
        /// <value><c>true</c> if this note is private; otherwise, <c>false</c>.</value>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note should be pinned to the top of the list.
        /// </summary>
        /// <value><c>true</c> if this note is to be pinned to the top; otherwise, <c>false</c>.</value>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>The created date time.</value>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
