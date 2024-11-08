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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    /// Describes the step program to be displayed to the user
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class StepProgramBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.Category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a description of the program.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the program. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default ListView.
        /// </summary>
        /// <value>
        /// The default ListView.
        /// </value>
        public int DefaultListView { get; set; }

        /// <summary>
        /// Gets or sets the step program attributes.
        /// </summary>
        /// <value>
        /// The step program attributes.
        /// </value>
        public List<PublicEditableAttributeBag> StepProgramAttributes { get; set; }

        /// <summary>
        /// Gets or sets the workflow triggers.
        /// </summary>
        /// <value>
        /// The workflow triggers.
        /// </value>
        public List<StepProgramWorkflowTriggerBag> WorkflowTriggers { get; set; }

        /// <summary>
        /// Gets or sets the statuses.
        /// </summary>
        /// <value>
        /// The statuses.
        /// </value>
        public List<StepStatusBag> Statuses { get; set; }

        /// <summary>
        /// Gets or sets the chart data.
        /// </summary>
        /// <value>
        /// The chart data.
        /// </value>
        public string ChartData { get; set; }

        /// <summary>
        /// Gets or sets the kpi.
        /// </summary>
        /// <value>
        /// The kpi.
        /// </value>
        public string Kpi { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show chart].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show chart]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowChart { get; set; }

        /// <summary>
        /// Gets or sets the default date range.
        /// </summary>
        /// <value>
        /// The default date range.
        /// </value>
        public SlidingDateRangeBag DefaultDateRange { get; set; }

        /// <summary>
        /// Gets or sets the step flow page URL.
        /// </summary>
        /// <value>
        /// The step flow page URL.
        /// </value>
        public string StepFlowPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has the can administrate authorization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current user has the can administrate authorization; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets the status options.
        /// </summary>
        /// <value>
        /// The status options.
        /// </value>
        public List<ListItemBag> StatusOptions { get; set; }
    }
}
