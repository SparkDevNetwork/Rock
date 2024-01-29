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

using Rock.Enums.Blocks.Group.Scheduling;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about the outcome of a schedule row action request for the group schedule toolbox block.
    /// </summary>
    public class PerformScheduleRowActionResponseBag
    {
        /// <summary>
        /// Gets or sets the new status for the row as a result of the action request.
        /// <para>
        /// If this value is not defined, the row should be removed from the current list of rows.
        /// </para>
        /// </summary>
        public ToolboxScheduleRowConfirmationStatus? NewStatus { get; set; }

        /// <summary>
        /// Gets or sets whether this schedule row was previously confirmed.
        /// Will be true if this response represents a cancellation.
        /// </summary>
        public bool WasSchedulePreviouslyConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the selected person's group schedule feed URL.
        /// </summary>
        public string PersonGroupScheduleFeedUrl { get; set; }

        /// <summary>
        /// Gets or sets whether a decline reason is required.
        /// </summary>
        public bool IsDeclineReasonRequired { get; set; }

        /// <summary>
        /// Gets or sets whether a decline elaboration note field should be visible.
        /// </summary>
        public bool IsDeclineNoteVisible { get; set; }

        /// <summary>
        /// Gets or sets whether a decline elaboration note is required.
        /// </summary>
        public bool IsDeclineNoteRequired { get; set; }
    }
}
