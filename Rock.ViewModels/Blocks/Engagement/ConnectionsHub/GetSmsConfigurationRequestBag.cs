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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a request to retrieve SMS configuration for sending communications to a set of connection requests.
    /// </summary>
    public class GetSmsConfigurationRequestBag
    {
        /// <summary>
        /// Gets or sets the communication type ID key associated with the selected connection requests being sent an SMS communication.
        /// </summary>
        public string ConnectionTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the list of connection request ID keys to retrieve communication recipients for.
        /// </summary>
        /// <value>Each request is associated with a single person. These identifiers are used to find the people who will be sent the communication.</value>
        public List<string> ConnectionRequestIdKeys { get; set; }
    }
}
