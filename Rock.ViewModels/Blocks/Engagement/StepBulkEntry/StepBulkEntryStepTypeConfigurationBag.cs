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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.StepBulkEntry
{
    /// <summary>
    /// Configuration for a specific step type, including metadata and attributes.
    /// Returned by both block initialization and the GetStepTypeConfiguration action.
    /// </summary>
    public class StepBulkEntryStepTypeConfigurationBag
    {
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
        /// Gets or sets the step program unique identifier, used to filter the status picker.
        /// </summary>
        public Guid StepProgramGuid { get; set; }

        /// <summary>
        /// Gets or sets the label for the start date picker.
        /// Returns "Date" when the step type has no end date, or "Start Date" when it does.
        /// </summary>
        public string StartDateLabel { get; set; }

        /// <summary>
        /// Gets or sets the step type attribute definitions available for bulk entry.
        /// Only includes attributes where ShowOnBulk is true. Values are not included
        /// because this block only creates new steps (no existing values to load).
        /// </summary>
        public Dictionary<string, PublicAttributeBag> StepAttributes { get; set; }
    }
}
