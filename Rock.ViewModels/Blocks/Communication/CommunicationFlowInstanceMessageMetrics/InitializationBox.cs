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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
{
    /// <summary>
    /// Box containing the data needed to initialize the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class InitializationBox
    {
        /// <summary>
        /// Gets or sets the high-level flow communication details.
        /// </summary>
        public CommunicationFlowCommunicationBag CommunicationFlowCommunication { get; set; }

        /// <summary>
        /// Gets or sets the collection of communication flow instances associated with the parent communication flow.
        /// </summary>
        public List<CommunicationFlowInstanceBag> CommunicationFlowInstances { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether conversion goal tracking is enabled.
        /// </summary>
        public bool IsConversionGoalTrackingEnabled { get; set; }

        /// <summary>
        /// Gets the number of unique people included in the message metrics.
        /// </summary>
        public int UniquePersonCount { get; set; }

        /// <summary>
        /// Gets the number of unique people in the entire flow.
        /// </summary>
        public int AllUniquePersonCount { get; set; }
    }
}
