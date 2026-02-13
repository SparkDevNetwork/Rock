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

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// POCO result for a note.
    /// </summary>
    internal class NoteResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the type of note.
        /// </summary>
        public NoteTypeResult NoteType { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity (If it is a Note on a Person, it would be the person's name, etc)
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is an alert.
        /// </summary>
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this note is pinned.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets the author of the note.
        /// </summary>
        public PersonResult Author { get; set; }
    }
}
