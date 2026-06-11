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

using Rock.Utility;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.CampaignConfiguration
{
    /// <summary>
    /// The item details for the Campaign Configuration block.
    /// </summary>
    public class CampaignConfigurationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the campaign item.
        /// Used to identify the item in the system settings store.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the campaign.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this campaign is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the connection type associated with this campaign.
        /// </summary>
        public string ConnectionTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the connection opportunity associated with this campaign.
        /// </summary>
        public string ConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used to generate comments on connection requests.
        /// Merge fields available are [Person] and [Family].
        /// </summary>
        public string RequestCommentsLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the data view used to select individuals for connection.
        /// </summary>
        public ListItemBag DataView { get; set; }

        /// <summary>
        /// Gets or sets the family limits setting, determining whether the campaign
        /// creates requests for each person or only for the head of household.
        /// Corresponds to the FamilyLimits enum value.
        /// </summary>
        public FamilyLimits? FamilyLimits { get; set; }

        /// <summary>
        /// Gets or sets the opt-out group. Individuals in this group will not receive
        /// a connection request from this campaign.
        /// </summary>
        public ListItemBag OptOutGroup { get; set; }

        /// <summary>
        /// Gets or sets whether connection requests are created all at once or as needed.
        /// Corresponds to the CreateConnectionRequestOptions enum value.
        /// </summary>
        public CreateConnectionRequestOptions? CreateConnectionRequestOption { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of connection requests each connector will be
        /// assigned per day. Leave null to disable auto-assignment.
        /// </summary>
        public int? DailyLimitAssigned { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of days that must pass since the last connection
        /// request was modified before a new one is created for the same person.
        /// </summary>
        public int? DaysBetweenConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campaign should prefer assigning
        /// the previous connector when creating a new request for a person.
        /// </summary>
        public bool PreferPreviousConnector { get; set; }
    }
}
