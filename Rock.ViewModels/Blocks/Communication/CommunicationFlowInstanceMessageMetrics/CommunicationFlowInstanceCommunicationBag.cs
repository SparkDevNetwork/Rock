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

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// Bag containing information about a communication flow instance communication for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class CommunicationFlowInstanceCommunicationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the communication flow instance.
        /// </summary>
        public string CommunicationFlowInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the communication flow instance communication.
        /// </summary>
        public string CommunicationFlowInstanceCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the list of recipient metrics associated with this communication flow instance communication.
        /// </summary>
        public List<RecipientMetricsBag> RecipientMetrics { get; set; }
    }
}
