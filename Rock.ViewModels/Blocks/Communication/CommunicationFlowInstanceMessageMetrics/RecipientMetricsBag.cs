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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// Bag containing the recipient metrics of a communication flow message for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class RecipientMetricsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the recipient person.
        /// </summary>
        public string PersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the parent Communication Flow Instance Communication.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the person associated with the recipient metrics.
        /// </summary>
        public PersonFieldBag Person { get; set; }

        /// <summary>
        /// Gets or sets the date the communication was sent to the recipient.
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Gets or sets the latest date the communication was opened by the recipient.
        /// </summary>
        public DateTime? OpenedDate { get; set; }

        /// <summary>
        /// Gets or sets the latest date the recipient clicked on a link in the communication.
        /// </summary>
        public DateTime? ClickedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the recipient achieved the conversion goal for the communication flow instance.
        /// </summary>
        public DateTime? ConversionDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the recipient unsubscribed from the communication.
        /// </summary>
        public DateTime? UnsubscribeDate { get; set; }
    }
}
