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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Store.PackageDetail
{
    /// <summary>
    /// The item details for the Package Detail block.
    /// </summary>
    public class PackageBag : EntityBagBase
    {
        /// <summary>
        /// Organization store key used when generating Rock postback URLs
        /// (e.g., rating links).
        /// </summary>
        public string StoreKey { get; set; }

        /// <summary>
        /// The display name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Long-form description shown on the package detail page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL to the vendor's support page for this package.
        /// Used for both the main package link and support link.
        /// </summary>
        public string SupportUrl { get; set; }

        /// <summary>
        /// Image URL for the package icon.
        /// </summary>
        public string PackageIconImageUrl { get; set; }

        /// <summary>
        /// Indicates whether the package is free.
        /// Determines pricing label and install/buy behavior.
        /// </summary>
        public bool IsFree { get; set; }

        /// <summary>
        /// Price of the package, if not free.
        /// Null means price is unknown or unavailable.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Indicates whether the current organization has purchased the package.
        /// </summary>
        public bool IsPurchased { get; set; }

        /// <summary>
        /// status of package installation, such as "Installed", "Update", or "Not Installed".
        /// </summary>
        public string InstalledStatusMessage { get; set; }

        /// <summary>
        /// Date the package was purchased, if applicable.
        /// Used for "Purchased on" messaging.
        /// </summary>
        public string PurchasedDate { get; set; }

        /// <summary>
        /// rating for the package (0–5).
        /// Used to render star icons.
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Ratings and reviews for the latest compatible version.
        /// </summary>
        public List<PackageVersionRatingBag> LatestVersionRatings { get; set; }

        /// <summary>
        /// The current Rock semantic version running on the system.
        /// Used to determine compatibility.
        /// </summary>
        public string CurrentRockSemanticVersion { get; set; }

        /// <summary>
        /// All versions of the package, ordered or filtered as needed.
        /// </summary>
        public List<PackageVersionBag> Versions { get; set; }

        /// <summary>
        /// alled version identifier, if the package is currently installed.
        /// </summary>
        public int? InstalledVersionId { get; set; }

        /// <summary>
        /// installed version label, if the package is currently installed.
        /// </summary>
        public string InstalledVersionLabel { get; set; }

        /// <summary>
        /// Rock-generated postback URL used for rating submissions.
        /// </summary>
        public string RatePostBackUrl { get; set; }

        /// <summary>
        /// Pre-selected and formatted information about the
        /// latest compatible version of the package.
        /// </summary>
        public PackageVersionBag LatestVersion { get; set; }

        /// <summary>
        /// Vendor display name.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Vendor website or profile URL.
        /// </summary>
        public string VendorUrl { get; set; }

        /// <summary>
        /// Indicates whether the package requires a Rock update to install or use.
        /// </summary>
        public bool RequiresRockUpdate { get; set; }

        /// <summary>
        /// Will store the value of the rock version requirement if RequiresRockUpdate is true.
        /// </summary>
        public string RockUpdateRequirement { get; set; }

    }
}

