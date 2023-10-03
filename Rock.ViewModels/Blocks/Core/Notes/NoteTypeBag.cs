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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.Notes
{
    /// <summary>
    /// Describes a single type of note that might be included in the list
    /// of notes.
    /// </summary>
    public class NoteTypeBag
    {
        /// <summary>
        /// Gets or sets the identifier of the note type.
        /// </summary>
        /// <value>The identifier of the note type.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class that represents the note type.
        /// </summary>
        /// <value>The icon CSS class that represents the note type.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person can select this note type.
        /// </summary>
        /// <value><c>true</c> if the person can select this note type; otherwise, <c>false</c>.</value>
        public bool UserSelectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether replies to this note type are allowed.
        /// </summary>
        /// <value><c>true</c> if replies to this note type are allowed; otherwise, <c>false</c>.</value>
        public bool AllowsReplies { get; set; }

        /// <summary>
        /// Gets or sets the maximum reply depth.
        /// </summary>
        /// <value>The maximum reply depth.</value>
        public int MaxReplyDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notes of this type can be watched.
        /// </summary>
        /// <value><c>true</c> if notes of this type can be watched; otherwise, <c>false</c>.</value>
        public bool AllowsWatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notes of this type can use mentions.
        /// </summary>
        /// <value><c>true</c> if notes of this type can use mentions; otherwise, <c>false</c>.</value>
        public bool IsMentionEnabled { get; set; }

        /// <summary>
        /// Gets or sets the attributes defined on this note type.
        /// </summary>
        /// <value>The attributes defined on this note type.</value>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
