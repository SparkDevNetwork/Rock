// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Represents a store promo.
    /// </summary>
    public class Promo : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the promo. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the image large identifier.
        /// </summary>
        /// <value>
        /// The image large identifier.
        /// </value>
        public int ImageLargeId { get; set; }

        /// <summary>
        /// Gets or sets the image small identifier.
        /// </summary>
        /// <value>
        /// The image small identifier.
        /// </value>
        public int ImageSmallId { get; set; }

        /// <summary>
        /// Gets or sets the Package Category of the Promo. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.PackageCategory"/> representing the Package Category of the Promo.
        /// </value>
        public PackageCategory Category { get; set; }
        
        /// <summary>
        /// Gets or sets the Guid of the Promo. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Promo.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Package Vendor of the Promo. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Package Vendor of the Promo.
        /// </value>
        public string PackageVendor { get; set; }

        /// <summary>
        /// Gets or sets the Package Name of the Promo. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Package Name of the Promo.
        /// </value>
        public string PackageName { get; set; }

         /// <summary>
        /// Gets or sets the Id of the promo package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PackageId.
        /// </value>
        public int PackageId { get; set; }
        
        /// <summary>
        /// Gets or sets a flag determining if the promo is for one of the top paid packages. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this promo is for the top paid; otherwise <c>false</c>.
        /// </value>
        public bool IsTopPaid { get; set; }

        /// <summary>
        /// Gets or sets a flag determining if the promo is for one of the top free packages. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this promo is for the top free; otherwise <c>false</c>.
        /// </value>
        public bool IsTopFree { get; set; }

        /// <summary>
        /// Gets or sets a flag determining if the promo is for a featured package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this promo is featured; otherwise <c>false</c>.
        /// </value>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Gets or sets the promo's large image. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.StoreImage"/> for the promo's large image.
        /// </value>
        public StoreImage ImageLarge { get; set; }

        /// <summary>
        /// Gets or sets the promo's small image. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.StoreImage"/> for the promo's small image.
        /// </value>
        public StoreImage ImageSmall { get; set; }

        /// <summary>
        /// Gets or sets the promo's package image. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Store.StoreImage"/> for the promo's package image.
        /// </value>
        public StoreImage PackageIconBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the promo's package rating. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> for the promo's package rating.
        /// </value>
        public double? PackageRating { get; set; }

        /// <summary>
        /// Gets or sets the promo's package price. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> for the promo's package price.
        /// </value>
        public decimal? PackagePrice { get; set; }
    }
}
