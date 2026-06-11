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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to manually update the fulfillment state of a group member requirement.
    /// </summary>
    public class UpdateGroupMemberRequirementBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the group requirement definition.
        /// </summary>
        public string GroupRequirementIdKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the group member requirement record to update.
        /// </summary>
        public string GroupMemberRequirementIdKey { get; set; }

        /// <summary>
        /// The Group Member IdKey of the placed group member.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group member requirement should be marked as met.
        /// </summary>
        public bool IsMet { get; set; }
    }
}
