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

namespace Rock.Store
{
    /// <summary>
    /// Represents a package in the store.
    /// </summary>
    public class Package : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Package.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Description of the Package.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Price of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the Price of the Package.
        /// </value>
        public decimal? Price { get; set; }

        /// <summary>
        /// Gets or sets a flag determining if the package is free. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Package is free; otherwise <c>false</c>.
        /// </value>
        public bool IsFree { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Package.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Vendor of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.PackageVendor"/> representing the Vendor of the Package.
        /// </value>
        public PackageVendor Vendor { get; set; }

        /// <summary>
        /// Gets or sets the Primary Category of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.PackageCategory"/> representing the Primary Category of the Package.
        /// </value>
        public PackageCategory PrimaryCategory { get; set; }

        /// <summary>
        /// Gets or sets the Secondary Category of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.PackageCategory"/> representing the Secondary Category of the Package.
        /// </value>
        public PackageCategory SecondaryCategory { get; set; }

        /// <summary>
        /// Gets or sets the Rating of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> representing the Rating of the Package.
        /// </value>
        public double? Rating { get; set; }

        /// <summary>
        /// Gets or sets the Support URL of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Support URL of the Package.
        /// </value>
        public string SupportUrl { get; set; }

        /// <summary>
        /// Gets or sets the date the package was added. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the Package was added.
        /// </value>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Gets or sets the package icon 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.StoreImage"/> representing the icon of the package.
        /// </value>
        public StoreImage PackageIconBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        /// <value>
        /// The versions.
        /// </value>
        public List<PackageVersion> Versions { get; set; }

        /// <summary>
        /// Gets or sets the determination if the package is purchased 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> fact if the package is purchased or not.
        /// </value>
        public bool IsPurchased { get; set; }

        /// <summary>
        /// Gets or sets the purchase date 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> of the date the package was purchased.
        /// </value>
        public DateTime PurchasedDate { get; set; }

        /// <summary>
        /// Gets or sets the purchaser 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the person who purchased the package.
        /// </value>
        public string Purchaser { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> class.
        /// </summary>
        public Package()
        {
            this.PrimaryCategory = new PackageCategory();
            this.SecondaryCategory = new PackageCategory();
            this.Vendor = new PackageVendor();
            this.PackageIconBinaryFile = new StoreImage();
            this.Versions = new List<PackageVersion>();
        }
    }

    /// <summary>
    /// Represents a vendor that created the package.
    /// </summary>
    public class PackageVendor : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Vendor.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Vendor.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Vendor.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the URL of the Vendor.
        /// </value>
        public string Url { get; set; }
    }
}
