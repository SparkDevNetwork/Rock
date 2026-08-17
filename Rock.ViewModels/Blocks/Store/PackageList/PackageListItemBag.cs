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

namespace Rock.ViewModels.Blocks.Store.PackageList
{
    /// <summary>
    /// Contains the data for a single package card rendered by the Package List block.
    /// All image URLs are resolved server-side from <c>Rock.Store.StoreImage.ImageUrl</c>
    /// before being placed in this bag.
    /// </summary>
    public class PackageListItemBag
    {
        /// <summary>
        /// Gets or sets the package identifier.
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the vendor of the package.
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Gets or sets the package rating.
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// Gets or sets the package price. A null value means the price is unknown
        /// (renders as "Paid"). Ignored when <see cref="IsFree"/> is <c>true</c>.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the package is free.
        /// </summary>
        public bool IsFree { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the package icon image.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the fully resolved detail page URL for this package,
        /// with the PackageId already substituted in by the server.
        /// </summary>
        public string DetailPageUrl { get; set; }
    }
}
