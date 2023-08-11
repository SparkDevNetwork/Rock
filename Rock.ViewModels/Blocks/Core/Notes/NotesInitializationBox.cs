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

namespace Rock.ViewModels.Blocks.Core.Notes
{
    /// <summary>
    /// 
    /// </summary>
    public class NotesInitializationBox
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the context entity for this block.
        /// </summary>
        /// <value>The identifier key of the context entity for this block.</value>
        public string EntityIdKey { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier of the context entity for this block.
        /// </summary>
        /// <value>The entity type identifier of the context entity for this block.</value>
        public string EntityTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the notes to display.
        /// </summary>
        /// <value>The notes to display.</value>
        public List<NoteBag> Notes { get; set; }

        /// <summary>
        /// Gets or sets the note types configured on the block.
        /// </summary>
        /// <value>The note types configured on the block.</value>
        public List<NoteTypeBag> NoteTypes { get; set; }

        /// <summary>
        /// Gets or sets the panel title.
        /// </summary>
        /// <value>The panel title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the title icon CSS class.
        /// </summary>
        /// <value>The title icon CSS class.</value>
        public string TitleIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether adding notes is allowed.
        /// </summary>
        /// <value><c>true</c> if adding notes is allowed; otherwise, <c>false</c>.</value>
        public bool ShowAdd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notes should be displayed in descending order.
        /// </summary>
        /// <value><c>true</c> if notes should be displayed in descending order; otherwise, <c>false</c>.</value>
        public bool IsDescending { get; set; }

        /// <summary>
        /// Gets or sets the note label that describes a single note.
        /// </summary>
        /// <value>The note label that describes a single note.</value>
        public string NoteLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this block is in light mode.
        /// </summary>
        /// <value><c>true</c> if this block is in light mode; otherwise, <c>false</c>.</value>
        public bool IsLightMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the alert checkbox should be shown.
        /// </summary>
        /// <value><c>true</c> if the alert checkbox should be shown; otherwise, <c>false</c>.</value>
        public bool ShowAlertCheckBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether private checkbox should be shown.
        /// </summary>
        /// <value><c>true</c> if private checkbox should be shown; otherwise, <c>false</c>.</value>
        public bool ShowPrivateCheckBox { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether security button should be shown.
        /// </summary>
        /// <value><c>true</c> if security button should be shown; otherwise, <c>false</c>.</value>
        public bool ShowSecurityButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the add note editor should always be visible.
        /// </summary>
        /// <value><c>true</c> if the add note editor should always be visible; otherwise, <c>false</c>.</value>
        public bool AddAlwaysVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the created date override should be visible.
        /// </summary>
        /// <value><c>true</c> if the created date override should be visible; otherwise, <c>false</c>.</value>
        public bool ShowCreateDateInput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note type heading should be visible.
        /// </summary>
        /// <value><c>true</c> if the note type heading should be visible; otherwise, <c>false</c>.</value>
        public bool DisplayNoteTypeHeading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether person icons should be shown.
        /// </summary>
        /// <value><c>true</c> if person icons should be shown; otherwise, <c>false</c>.</value>
        public bool UsePersonIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether replies should be automatically expanded.
        /// </summary>
        /// <value><c>true</c> if replies should be automatically expanded; otherwise, <c>false</c>.</value>
        public bool ExpandReplies { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL of the current person.
        /// </summary>
        /// <value>The avatar URL of the current person.</value>
        public string PersonAvatarUrl { get; set; }
    }
}
