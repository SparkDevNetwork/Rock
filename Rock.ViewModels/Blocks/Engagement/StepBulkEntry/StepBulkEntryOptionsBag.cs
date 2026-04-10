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

namespace Rock.ViewModels.Blocks.Engagement.StepBulkEntry
{
    /// <summary>
    /// The initialization options for the Step Bulk Entry block.
    /// </summary>
    public class StepBulkEntryOptionsBag
    {
        /// <summary>
        /// Gets or sets the pre-selected step program.
        /// Null when the user should select the program.
        /// </summary>
        public ListItemBag StepProgram { get; set; }

        /// <summary>
        /// Gets or sets the pre-selected step type.
        /// Null when the user should select the step type.
        /// </summary>
        public ListItemBag StepType { get; set; }

        /// <summary>
        /// Gets or sets the pre-selected step status.
        /// Null when the user should select the status.
        /// </summary>
        public ListItemBag StepStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the step program picker should be visible.
        /// False when a program is pre-selected via block settings or page parameters.
        /// </summary>
        public bool IsProgramPickerVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the step type picker should be disabled.
        /// True when a step type is pre-selected via block settings or page parameters,
        /// so the picker displays the selected value but cannot be changed.
        /// </summary>
        public bool IsTypePickerDisabled { get; set; }

        /// <summary>
        /// Gets or sets the step type configuration. Included on initialization when a
        /// step type is pre-selected so the client does not need an extra round-trip.
        /// </summary>
        public StepBulkEntryStepTypeConfigurationBag StepTypeConfiguration { get; set; }
    }
}
