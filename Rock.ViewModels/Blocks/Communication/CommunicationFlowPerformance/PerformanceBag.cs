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

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// Bag containing information about the performance of a communication flow.
    /// </summary>
    public class PerformanceBag
    {
        /// <summary>
        /// Gets or sets the communication flow identifier key.
        /// </summary>
        public string CommunicationFlowIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication flow.
        /// </summary>
        public string CommunicationFlowName { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger that kicks off new communication flow instances.
        /// </summary>
        public CommunicationFlowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the type of the conversion goal.
        /// </summary>
        public ConversionGoalType? ConversionGoalType { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal target percent.
        /// </summary>
        public decimal? ConversionGoalTargetPercent { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal timeframe in days.
        /// </summary>
        public int? ConversionGoalTimeframeInDays { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings.
        /// </summary>
        public ConversionGoalSettingsBag ConversionGoalSettings { get; set; }

        /// <summary>
        /// Gets or sets the messages that are part of this communication flow's instances.
        /// </summary>
        public List<MessageBag> Messages { get; set; }

        /// <summary>
        /// Gets or sets the start date of the flow.
        /// </summary>
        /// <value>
        /// This is the schedule start date used in creating instances.
        /// Depending on trigger type and schedule, an instance may or may not be started on this start date time.
        /// This is primarily used for display purposes.
        /// </value>
        public DateTime? CommunicationFlowStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of communications in each flow instance.
        /// </summary>
        public int CommunicationCount { get; set; }

        /// <summary>
        /// Gets or sets the communication flow instances in this communication flow.
        /// </summary>
        public List<CommunicationFlowInstanceBag> Instances { get; set; }
    }
}
