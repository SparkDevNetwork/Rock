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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains information needed to select a connection request.
    /// </summary>
    public class ConnectionRequestBoardSelectRequestBag
    {
        /// <summary>
        /// Gets or sets the connection request identifier.
        /// </summary>
        public int ConnectionRequestId { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity identifier, which will be used if the connection request
        /// identifier is 0; this indicates the individual is attempting to add a new connection request.
        /// </summary>
        public int ConnectionOpportunityId { get; set; }
    }
}
