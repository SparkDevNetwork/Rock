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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.NoteTypeDetail
{
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
        /// A optional Lava Template that can be used to general a URL where Notes of this type can be approved
        /// If this is left blank, the Approval URL will be a URL to the page (including a hash anchor to the note) where the note was originally created
        /// </summary>
        public string ApprovalUrlTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic watch authors].
        /// </summary>
        public bool AutoWatchAuthors { get; set; }

        /// <summary>
        /// Gets or sets the background color of each note
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BinaryFileType that will be used for attachments.
        /// </summary>
        public ListItemBag BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the border color of each note
        /// </summary>
        public string BorderColor { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of the entities that Notes of this NoteType 
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the font color of the note text
        /// </summary>
        public string FontColor { get; set; }

        /// <summary>
        /// Gets or sets the name of an icon CSS class. 
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that this NoteType is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum reply depth.
        /// </summary>
        public int? MaxReplyDepth { get; set; }

        /// <summary>
        /// Gets or sets the Name of the NoteType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approvals].
        /// </summary>
        public bool RequiresApprovals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send approval notifications].
        /// </summary>
        public bool SendApprovalNotifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        public bool UserSelectable { get; set; }
    }
}
