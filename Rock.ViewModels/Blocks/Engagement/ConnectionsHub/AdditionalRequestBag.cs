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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a summary of an additional connection request associated with the same requester person.
    /// </summary>
    public class AdditionalRequestBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of this connection request.
        /// </summary>
        public string RequestIdKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection opportunity this request belongs to.
        /// </summary>
        public string ConnectionOpportunityIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection opportunity this request belongs to.
        /// </summary>
        public string ConnectionOpportunityName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the current connection status for this request.
        /// </summary>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector assigned to this request.
        /// </summary>
        public string Connector { get; set; }

        /// <summary>
        /// Gets or sets the date and time this connection request was created.
        /// </summary>
        public DateTimeOffset? RequestCreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who submitted this connection request.
        /// </summary>
        public string Requester { get; set; }
    }
}
