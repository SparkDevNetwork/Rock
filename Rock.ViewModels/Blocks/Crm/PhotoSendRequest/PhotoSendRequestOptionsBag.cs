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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PhotoSendRequest
{
    /// <summary>
    /// The option lists and settings the Send Photo Request block needs to render its criteria form.
    /// </summary>
    public class PhotoSendRequestOptionsBag
    {
        /// <summary>
        /// Gets or sets the selectable family group-type roles (value is the role's unique identifier).
        /// </summary>
        public List<ListItemBag> FamilyRoleOptions { get; set; }

        /// <summary>
        /// Gets or sets the selectable person connection statuses (value is the defined value's unique identifier).
        /// </summary>
        public List<ListItemBag> ConnectionStatusOptions { get; set; }

        /// <summary>
        /// Gets or sets the recipient count above which the communication requires approval before sending.
        /// </summary>
        public int MaximumRecipients { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can approve communications,
        /// which determines whether a large send is queued directly or submitted for approval.
        /// </summary>
        public bool IsApproveAuthorized { get; set; }
    }
}
