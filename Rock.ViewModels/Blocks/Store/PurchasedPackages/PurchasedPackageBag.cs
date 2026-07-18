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

namespace Rock.ViewModels.Blocks.Store.PurchasedPackages
{
    /// <summary>
    /// A single purchased package displayed by the Purchased Packages block.
    /// </summary>
    public class PurchasedPackageBag
    {
        /// <summary>
        /// Gets or sets the package identifier.
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the package description (may contain HTML).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a plain-text truncated version of the description for
        /// the collapsed card view. <c>null</c> when the full description fits
        /// within the truncation limit, meaning no "Show more" button is needed.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the pre-sized package icon URL.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the date the package was purchased.
        /// </summary>
        public DateTimeOffset? PurchasedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who purchased the package.
        /// </summary>
        public string Purchaser { get; set; }

        /// <summary>
        /// Gets or sets the install state of the package, by name. Mirrors the
        /// client-side <c>PurchasedPackageInstallState</c> enum values
        /// ("NotAvailable", "Install", "Update", "Installed").
        /// </summary>
        public string InstallState { get; set; }

        /// <summary>
        /// Gets or sets the label of the currently installed version, when the
        /// package is installed.
        /// </summary>
        public string InstalledVersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the label of the latest version compatible with this
        /// Rock version, when one is available.
        /// </summary>
        public string LatestVersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the package detail page URL.
        /// </summary>
        public string DetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the install / update page URL.
        /// </summary>
        public string InstallPageUrl { get; set; }
    }
}
