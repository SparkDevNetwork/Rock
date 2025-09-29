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

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// A bag that contains information about a Communication Flow Instance for the Communication Flow Performance block.
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
        /// Gets a list of the unique person alias identifier keys.
        /// </summary>
        public List<string> UniquePersonAliasIdKeys { get; set; }

        /// <summary>
        /// Gets or sets the Communication Flow Instance start date.
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}