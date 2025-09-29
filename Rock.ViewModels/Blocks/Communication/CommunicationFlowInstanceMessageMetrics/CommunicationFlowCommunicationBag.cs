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
    /// Bag containing the high-level flow communication (blueprint) information needed for the Communication Flow Instance Message Metrics block.
    /// </summary>
    public class CommunicationFlowCommunicationBag
    {
        /// <summary>
        /// Gets or sets the identifier key for this high-level flow communication.
        /// </summary>
        public string CommunicationFlowCommunicationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the flow communication (blueprint).
        /// </summary>
        public string CommunicationFlowCommunicationName { get; set; }

        /// <summary>
        /// Gets or sets the collection of flow instance communications that are associated with this high-level flow communication (blueprint).
        /// </summary>
        /// <remarks></remarks>
        public List<CommunicationFlowInstanceCommunicationBag> CommunicationFlowInstanceCommunications { get; set; }
    }
}
