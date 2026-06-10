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

using Rock.Enums.Crm;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a step update operation in a bulk update save request.
    /// </summary>
    public class BulkUpdateStepBag
    {
        /// <summary>
        /// Gets or sets the action to perform on the step.
        /// </summary>
        public BulkUpdateActionSpecifier Action { get; set; }

        /// <summary>
        /// Gets or sets the step type.
        /// </summary>
        public ListItemBag StepType { get; set; }

        /// <summary>
        /// Gets or sets the step status.
        /// </summary>
        public ListItemBag StepStatus { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the step attributes.
        /// </summary>
        public Dictionary<string, string> StepAttributes { get; set; }

        /// <summary>
        /// Gets or sets which step fields the user toggled on for update
        /// when the action is <see cref="BulkUpdateActionSpecifier.Update"/>.
        /// </summary>
        public Dictionary<string, bool> UpdatedFields { get; set; }
    }
}
