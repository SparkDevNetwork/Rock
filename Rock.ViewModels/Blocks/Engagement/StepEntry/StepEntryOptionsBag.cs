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

namespace Rock.ViewModels.Blocks.Engagement.StepEntry
{
    /// <summary>
    /// The additional configuration options for the Step Entry block.
    /// </summary>
    public class StepEntryOptionsBag
    {
        /// <summary>
        /// Gets or sets the step program unique identifier used to configure the step status picker.
        /// </summary>
        public Guid? StepProgramGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the step type supports an end date.
        /// When false, the end date picker should be hidden.
        /// </summary>
        public bool HasEndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the start date is required for this step type.
        /// </summary>
        public bool IsDateRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person picker should be shown and enabled.
        /// This is true when adding a new step and no person context or page parameter is provided.
        /// </summary>
        public bool IsPersonSelectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Rock has only a single campus.
        /// When true, campus information should be hidden in the UI.
        /// </summary>
        public bool IsSingleCampus { get; set; }

        /// <summary>
        /// Gets or sets the name of the step type for display in the block title.
        /// </summary>
        public string StepTypeName { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the step type for display in the block title.
        /// </summary>
        public string StepTypeIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the available manual workflow triggers for this step.
        /// </summary>
        public List<StepEntryWorkflowBag> AvailableWorkflows { get; set; }
    }
}
