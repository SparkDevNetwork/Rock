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

namespace Rock.ViewModels.Blocks.Engagement.StepParticipantList
{
    /// <summary>
    /// The additional configuration options for the Step List block.
    /// </summary>
    public class StepParticipantListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the campus column should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the campus column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsCampusColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note column should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the note column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsNoteColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Date Started column should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Date Started column should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsDateStartedColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the step type.
        /// </summary>
        /// <value>
        /// The step type.
        /// </value>
        public ListItemBag StepType { get; set; }

        /// <summary>
        /// Gets or sets the person profile page URL.
        /// </summary>
        /// <value>
        /// The person profile page URL.
        /// </value>
        public string PersonProfilePageUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of available step statuses for the step status filter.
        /// </summary>
        /// <value>
        /// The step status items.
        /// </value>
        public List<ListItemBag> StepStatusItems { get; set; }

        /// <summary>
        /// Gets or sets the step status background colors.
        /// </summary>
        /// <value>
        /// The step status background colors.
        /// </value>
        public Dictionary<string, string> StepStatusBackgroundColors { get; set; }
    }
}
