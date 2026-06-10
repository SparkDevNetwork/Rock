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

namespace Rock.ViewModels.Blocks.Store.PromoList
{
    /// <summary>
    /// Contains the promo data sent to the Promo List block for rendering.
    /// All image URLs are resolved server-side from <c>Rock.Store.StoreImage.ImageUrl</c>
    /// before being placed in this bag.
    /// </summary>
    public class PromoBag
    {
        /// <summary>
        /// Gets or sets the package identifier.
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the vendor of the package.
        /// </summary>
        public string PackageVendor { get; set; }

        /// <summary>
        /// Gets or sets the package rating.
        /// </summary>
        public double? PackageRating { get; set; }

        /// <summary>
        /// Gets or sets the package price. A value of 0 means free. A null
        /// value means the price is unknown (renders as "Paid").
        /// </summary>
        public decimal? PackagePrice { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the package icon image.
        /// Used by the card list display style.
        /// </summary>
        public string PackageIconUrl { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the large promo image.
        /// Used by the rotator display style. Null when no large image exists.
        /// </summary>
        public string ImageLargeUrl { get; set; }

        /// <summary>
        /// Gets or sets the fully resolved detail page URL for this promo,
        /// with the PackageId already substituted in by the server.
        /// </summary>
        public string DetailPageUrl { get; set; }
    }
}
