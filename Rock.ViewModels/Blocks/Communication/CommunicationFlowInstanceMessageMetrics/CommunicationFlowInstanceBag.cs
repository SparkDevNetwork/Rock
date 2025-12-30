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

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// A bag that contains information about a Communication Flow Instance for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class CommunicationFlowInstanceBag
    {
        /// <summary>
        /// Gets or sets the identifier key for the Communication Flow Instance.
        /// </summary>
        public string CommunicationFlowInstanceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the count of unique people in this Communication Flow Instance.
        /// </summary>
        public int UniquePersonCount { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }
    }
}