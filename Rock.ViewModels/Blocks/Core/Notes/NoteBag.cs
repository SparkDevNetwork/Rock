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
    /// Describes an existing note that should be displayed to the person.
    /// </summary>
    public class NoteBag
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
        /// Gets or sets the caption text of the note.
        /// </summary>
        /// <value>The caption text of the note.</value>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        /// <value>The text of the note.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the unique anchor identifier used to link to this note.
        /// </summary>
        /// <value>The unique anchor identifier used to link to this note.</value>
        public string AnchorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is an alert note.
        /// </summary>
        /// <value><c>true</c> if this note is an alert note; otherwise, <c>false</c>.</value>
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note should be pinned to top
        /// </summary>
        /// <value><c>true</c> if this note should be pinned to top otherwise, <c>false</c>.</value>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is a private note.
        /// </summary>
        /// <value><c>true</c> if this note is a private note; otherwise, <c>false</c>.</value>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is currently being watched.
        /// </summary>
        /// <value><c>true</c> if this note is currently being watched; otherwise, <c>false</c>.</value>
        public bool IsWatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is editable.
        /// </summary>
        /// <value><c>true</c> if this note is editable; otherwise, <c>false</c>.</value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is deletable.
        /// </summary>
        /// <value><c>true</c> if this note is deletable; otherwise, <c>false</c>.</value>
        public bool IsDeletable { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>The created date time.</value>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the person that created this note.
        /// </summary>
        /// <value>The identifier of the person that created this note.</value>
        public string CreatedByIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that created this created.
        /// </summary>
        /// <value>The name of the person that created this created.</value>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL of the person that created this note.
        /// </summary>
        /// <value>The photo URL of the person that created this note.</value>
        public string CreatedByPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the edited date time.
        /// </summary>
        /// <value>The edited date time.</value>
        public DateTimeOffset? EditedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that edited this note.
        /// </summary>
        /// <value>The name of the person that edited this note.</value>
        public string EditedByName { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
