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
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.StepTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class StepTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a flag indicating if this item can be edited by a person.
        /// </summary>
        public bool AllowManualEditing { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this step type allows multiple step records per person.
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DataView.  The data view reveals the people that are allowed to be
        /// considered for this step type.
        /// </summary>
        public ListItemBag AudienceDataView { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DataView.  The data view reveals the people that should be considered
        /// as having completed this step.
        /// </summary>
        public ListItemBag AutoCompleteDataView { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render custom card details.
        /// </summary>
        public string CardLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a description of the step type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this step type happens over time (like being in a group) or is it achievement based (like attended a class).
        /// </summary>
        public bool HasEndDate { get; set; }

        /// <summary>
        /// Gets or sets the highlight color for badges and cards.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this step requires a date.
        /// </summary>
        public bool IsDateRequired { get; set; }

        /// <summary>
        /// Gets or sets the name of the step type. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the number of occurrences should be shown on the badge.
        /// </summary>
        public bool ShowCountOnBadge { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the available pre requisites.
        /// </summary>
        /// <value>
        /// The available pre requisites.
        /// </value>
        public List<ListItemBag> AvailablePreRequisites { get; set; }

        /// <summary>
        /// Gets or sets the pre requisites.
        /// </summary>
        /// <value>
        /// The pre requisites.
        /// </value>
        public List<string> PreRequisites { get; set; }

        /// <summary>
        /// Gets or sets the step attributes.
        /// </summary>
        /// <value>
        /// The step attributes.
        /// </value>
        public List<StepAttributeBag> StepAttributes { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        public List<StepTypeWorkflowTriggerBag> Workflows { get; set; }

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
        /// Gets or sets the step type attributes grid data.
        /// </summary>
        /// <value>
        /// The step type attributes grid data.
        /// </value>
        public GridDataBag StepTypeAttributesGridData { get; set; }

        /// <summary>
        /// Gets or sets the step type attributes grid definition.
        /// </summary>
        /// <value>
        /// The step type attributes grid definition.
        /// </value>
        public GridDefinitionBag StepTypeAttributesGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the workflow trigger grid data.
        /// </summary>
        /// <value>
        /// The workflow trigger grid data.
        /// </value>
        public GridDataBag WorkflowTriggerGridData { get; set; }

        /// <summary>
        /// Gets or sets the workflow trigger grid definition.
        /// </summary>
        /// <value>
        /// The workflow trigger grid definition.
        /// </value>
        public GridDefinitionBag WorkflowTriggerGridDefinition { get; set; }
    }
}
