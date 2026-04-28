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
    /// Represents a connection campaign and its current pending request count for use in the assign-from-campaign flow.
    /// </summary>
    public class ConnectionCampaignBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this connection campaign.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of this connection campaign.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of connection requests currently pending assignment in this campaign.
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Gets or sets the default number of requests to assign per connector when pulling from this campaign.
        /// </summary>
        public int? DefaultNumberOfRequests { get; set; }
    }
}
