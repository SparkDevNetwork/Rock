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

using Rock.Enums.Core;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.NoteTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class NoteTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether attachments are allowed for this note type.
        /// </summary>
        public bool AllowsAttachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allows replies].
        /// </summary>
        public bool AllowsReplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allows watching].
        /// </summary>
        public bool AllowsWatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic watch authors].
        /// </summary>
        public bool AutoWatchAuthors { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BinaryFileType that will be used for attachments.
        /// </summary>
        public ListItemBag BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the color of each note.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of the entities that Notes of this NoteType 
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of formatting used by notes of this type.
        /// </summary>
        public NoteFormatType FormatType { get; set; }

        /// <summary>
        /// Gets or sets the name of an icon CSS class. 
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if mentions are supported on this note type.
        /// </summary>
        public bool IsMentionEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that this NoteType is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum reply depth.
        /// </summary>
        public string MaxReplyDepth { get; set; }

        /// <summary>
        /// Gets or sets the Name of the NoteType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        public bool UserSelectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the entity type picker to allow user selection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show entity type picker]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowEntityTypePicker { get; set; }
    }
}
