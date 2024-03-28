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
using System;

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    /// The workflow trigger details for the Step Program Detail workflow trigger grid.
    /// </summary>
    public class StepProgramWorkflowTriggerBag
    {
        /// <summary>
        /// Gets or sets the identifier key. Used primarily by the grid for updates and deletes.
        /// The idKey is used by the grid over the Guid because new additions might be added and
        /// simple increasing int values will be used as identifiers for the new entries so they can be
        /// by the grid when updating or deleting these new entries.
        /// </summary>
        /// <value>
        /// The identifier key.
        /// </value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the workflow trigger.
        /// </summary>
        /// <value>
        /// The workflow trigger.
        /// </value>
        public ListItemBag WorkflowTrigger { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the primary qualifier.
        /// </summary>
        /// <value>
        /// The primary qualifier.
        /// </value>
        public string PrimaryQualifier { get; set; }

        /// <summary>
        /// Gets or sets the secondary qualifier.
        /// </summary>
        /// <value>
        /// The secondary qualifier.
        /// </value>
        public string SecondaryQualifier { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid? Guid { get; set; }
    }
}
