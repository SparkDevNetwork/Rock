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

using Rock.ViewModels.Utility;
using Rock.Enums.Communication;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// The item details for the Communication Flow Detail block.
    /// </summary>
    public class CommunicationFlowBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the communications for this communication flow.
        /// </summary>
        public List<CommunicationFlowCommunicationBag> Communications { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients expected to complete the conversion goal.
        /// </summary>
        /// <value>
        /// The target conversion percent which should be a value between 0 and 100, inclusively.
        /// </value>
        public decimal? ConversionGoalTargetPercent { get; set; }

        /// <summary>
        /// Gets or sets the timeframe (in days) for achieving the conversion goal.
        /// </summary>
        public int? ConversionGoalTimeframeInDays { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal type for this Communication Flow.
        /// </summary>
        public ConversionGoalType? ConversionGoalType { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the condition for when a recipient no longer receives messages from this Communication Flow.
        /// </summary>
        public ExitConditionType ExitConditionType { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active Communication Flow.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the Communication Flow (maximum 100 characters).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Data View used to define the initial target audience for this Communication Flow.
        /// </summary>
        public ListItemBag TargetAudienceDataView { get; set; }

        /// <summary>
        /// Gets or sets the iCalendar content that describes the schedule for this Communication Flow.
        /// </summary>
        public string iCalendarContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how this Communication Flow is triggered.
        /// </summary>
        public CommunicationFlowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe message (maximum 500 characters).
        /// </summary>
        public string UnsubscribeMessage { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings.
        /// </summary>
        public CommunicationFlowDetailConversionGoalSettingsBag ConversionGoalSettings { get; set; }
    }
}
