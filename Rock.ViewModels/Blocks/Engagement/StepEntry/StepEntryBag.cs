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

namespace Rock.ViewModels.Blocks.Engagement.StepEntry
{
    /// <summary>
    /// The item details for the Step Entry block.
    /// </summary>
    public class StepEntryBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the person alias as a list item bag reference.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's photo for display in the view panel.
        /// </summary>
        public string PersonPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the person's connection status text for display in the view panel.
        /// </summary>
        public string PersonConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the selected campus as a list item bag.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the start date of the step in ISO 8601 format.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date of the step in ISO 8601 format.
        /// </summary>
        public string EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date of the step in ISO 8601 format.
        /// </summary>
        public string CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the step status as a list item bag for the status picker.
        /// </summary>
        public ListItemBag StepStatus { get; set; }

        /// <summary>
        /// Gets or sets the step status color hex value for display in the view panel.
        /// </summary>
        public string StepStatusColor { get; set; }

        /// <summary>
        /// Gets or sets the note text for the step.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the sanitized HTML representation of the note for display in view mode.
        /// </summary>
        public string NoteHtml { get; set; }
    }
}
