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
using Rock.Utility;

namespace Rock.Store
{
    /// <summary>
    /// Represents a store category for packages.
    /// </summary>
    public class PackageVersion : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package Category.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Package Category.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Package Category.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the version's label. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the versions lable.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the version's rock semantic version. 
        /// </summary>
        /// <value>
        /// A <see cref="RockSemanticVersion"/> representing the versions rock semantic version.
        /// </value>
        public RockSemanticVersion RequiredRockSemanticVersion { 
            get {
                return RockSemanticVersion.Parse( this.RequiredRockVersion );
            } 
        }

        /// <summary>
        /// Gets or sets the required Rock version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> required Rock version.
        /// </value>
        public string RequiredRockVersion { get; set; }

        /// <summary>
        /// Gets or sets the version description. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the version description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the documentation URL. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing URL to the version's documentation.
        /// </value>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Gets or sets the date added. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the version was added.
        /// </value>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Gets or sets the ratings.
        /// </summary>
        /// <value>
        /// The ratings.
        /// </value>
        public List<PackageVersionRating> Ratings { get; set; }

        /// <summary>
        /// Gets or sets the screenshots.
        /// </summary>
        /// <value>
        /// The screenshots.
        /// </value>
        public List<StoreImage> Screenshots { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersion" /> class.
        /// </summary>
        public PackageVersion()
        {
            this.Screenshots = new List<StoreImage>();
            this.Ratings = new List<PackageVersionRating>();
        }
    }
}
