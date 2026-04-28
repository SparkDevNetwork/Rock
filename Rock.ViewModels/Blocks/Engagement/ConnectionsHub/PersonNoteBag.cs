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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a person note associated with the requester of a connection request, for display in the request detail panel.
    /// </summary>
    public class PersonNoteBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of this note.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the URL of the profile photo of the person who created this note.
        /// </summary>
        public string CreatedByPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the note type this note belongs to.
        /// </summary>
        public string NoteTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who created this note.
        /// </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets or sets the date and time this note was created.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

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
        /// Gets or sets the text of the note.
        /// </summary>
        /// <value>The text of the note.</value>
        public string Text { get; set; }
    }
}
