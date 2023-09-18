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
using Rock.Model;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Workflow.WorkflowTriggerDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class WorkflowTriggerBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.EntityType that contains the entities that are affected by this WorkflowTrigger.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the Entity Qualifier Column that contains the value that filters the scope of the WorkflowTrigger. This
        /// property must be used in conjunction with the Rock.Model.WorkflowTrigger.EntityTypeQualifierValue property.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierValue in the Rock.Model.WorkflowTrigger.EntityTypeQualifierColumn that is used to filter the scope of the WorkflowTrigger.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value alt.
        /// </summary>
        /// <value>
        /// The entity type qualifier value alt.
        /// </value>
        public string EntityTypeQualifierValueAlt { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierValuePrevious in the Rock.Model.WorkflowTrigger.EntityTypeQualifierColumn that is used to filter the scope of the WorkflowTrigger.
        /// </summary>
        public string EntityTypeQualifierValuePrevious { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the WorkflowTrigger is active.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this WorkflowTrigger is part of Rock core system/framework.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow trigger. Indicates the type of change and  the timing the trigger.
        /// </summary>
        public string WorkflowTriggerType { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.WorkflowType that is executed by this WorkflowTrigger.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the workflow types.
        /// </summary>
        /// <value>
        /// The workflow types.
        /// </value>
        public List<ListItemBag> WorkflowTriggerTypes { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value label.
        /// </summary>
        /// <value>
        /// The entity type qualifier value label.
        /// </value>
        public string EntityTypeQualifierValueLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show previous and alt qualifier value text boxes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show previous and alt qualifier value text boxes]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPreviousAndAltQualifierValueTextBoxes { get; set; }

        /// <summary>
        /// Gets or sets the qualifier columns.
        /// </summary>
        /// <value>
        /// The qualifier columns.
        /// </value>
        public List<ListItemBag> QualifierColumns { get; set; }
    }
}
