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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityDetail
{
    /// <summary>
    /// Minimal bag describing a placement group configuration for a Connection Opportunity.
    /// </summary>
    public class PlacementGroupConfigBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public System.Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the GroupType as a ListItemBag.
        /// </summary>
        public ListItemBag GroupType { get; set; }

        /// <summary>
        /// Gets or sets the Group Member Role as a ListItemBag.
        /// </summary>
        public ListItemBag GroupMemberRole { get; set; }

        /// <summary>
        /// Gets or sets the group member status (enum value).
        /// </summary>
        public int GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all groups of the type are used.
        /// </summary>
        public bool UseAllGroupsOfType { get; set; }
    }
}



