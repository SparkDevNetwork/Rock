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

namespace Rock.ViewModels.Blocks.Engagement.StepBulkEntry
{
    /// <summary>
    /// The request bag for the SaveSteps block action, containing all
    /// data needed to create steps for the selected people.
    /// </summary>
    public class StepBulkEntrySaveRequestBag
    {
        /// <summary>
        /// Gets or sets the list of person alias unique identifiers for whom
        /// steps should be created. Only valid people should be included.
        /// </summary>
        public List<Guid> PersonAliasGuids { get; set; }

        /// <summary>
        /// Gets or sets the step type unique identifier.
        /// </summary>
        public Guid StepTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the step status unique identifier.
        /// </summary>
        public Guid StepStatusGuid { get; set; }

        /// <summary>
        /// Gets or sets the start date in ISO 8601 format.
        /// May be null if the step type does not require a date.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date in ISO 8601 format.
        /// Only applicable when the step type has an end date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// Null when no campus is selected.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the attribute values to apply to each step.
        /// Keys are attribute keys, values are the public edit values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
