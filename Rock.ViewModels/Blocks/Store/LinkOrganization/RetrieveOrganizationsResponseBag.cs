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

namespace Rock.ViewModels.Blocks.Store.LinkOrganization
{
    /// <summary>
    /// The response returned after attempting to retrieve the organizations
    /// tied to a store account.
    /// </summary>
    public class RetrieveOrganizationsResponseBag
    {
        /// <summary>
        /// Gets or sets the next step the block should display.
        /// </summary>
        public string NextStep { get; set; }

        /// <summary>
        /// Gets or sets the organizations available for selection. Populated
        /// when the account is tied to more than one organization.
        /// </summary>
        public List<ListItemBag> Organizations { get; set; }

        /// <summary>
        /// Gets or sets a warning message to display to the user, when
        /// applicable.
        /// </summary>
        public string WarningMessage { get; set; }
    }
}
