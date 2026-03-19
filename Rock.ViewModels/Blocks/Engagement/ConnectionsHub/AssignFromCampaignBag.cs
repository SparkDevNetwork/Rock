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

using Rock.Enums.Connection;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the parameters for assigning connection requests to a connector from a campaign.
    /// </summary>
    public class AssignFromCampaignBag
    {
        /// <summary>
        /// Gets or sets the GUID of the connection opportunity to assign requests for.
        /// </summary>
        public string ConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection campaign to pull requests from.
        /// </summary>
        public string ConnectionCampaignGuid { get; set; }

        /// <summary>
        /// Gets or sets the number of connection requests to assign from the campaign.
        /// </summary>
        public int NumberOfRequests { get; set; }
    }
}
