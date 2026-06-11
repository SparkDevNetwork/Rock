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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to bulk-update the connection state of one or more connection requests.
    /// </summary>
    public class UpdateConnectionRequestStatesBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier keys of the connection requests to update.
        /// </summary>
        public List<string> ConnectionRequestIdKeys { get; set; }

        /// <summary>
        /// Gets or sets the connection state (e.g., Active, Inactive, Future Follow-up) to apply to all specified requests.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date to assign when transitioning requests to the Future Follow-up state.
        /// </summary>
        public DateTimeOffset? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the group member requirements to update when completing requests that have placement groups.
        /// </summary>
        public List<GroupMemberRequirementBag> GroupMemberRequirements { get; set; }
    }
}
