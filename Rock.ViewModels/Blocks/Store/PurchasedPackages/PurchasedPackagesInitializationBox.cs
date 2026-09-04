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

namespace Rock.ViewModels.Blocks.Store.PurchasedPackages
{
    /// <summary>
    /// The box that contains the initialization information for the Purchased Packages block.
    /// </summary>
    public class PurchasedPackagesInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the store has been linked to an
        /// organization. When <c>false</c>, the component redirects to the Link
        /// Organization page using <see cref="LinkOrganizationPageUrl"/>.
        /// </summary>
        public bool IsStoreConfigured { get; set; }

        /// <summary>
        /// Gets or sets the Link Organization page URL the component redirects to
        /// when the store is not configured.
        /// </summary>
        public string LinkOrganizationPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the purchased packages to display.
        /// </summary>
        public List<PurchasedPackageBag> PurchasedPackages { get; set; }

        /// <summary>
        /// Gets or sets the store error message. When non-empty, the store could not be
        /// reached and the component renders the "Store Currently Not Available" panel
        /// instead of the package list.
        /// </summary>
        public string StoreErrorMessage { get; set; }
    }
}
