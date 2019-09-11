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

namespace Rock.Store
{
    /// <summary>
    /// Represents a store promo.
    /// </summary>
    public class InstalledPackage : StoreModel
    {
        /// <summary>
        /// Gets or sets the PackageId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PackageId.
        /// </value>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the package label of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the package label.
        /// </value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the VersionId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the VersionId.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the version label of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the version label.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the VendorId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the VendorId.
        /// </value>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor name of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the vendor name.
        /// </value>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets the Name of the person who installed the package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the installer.
        /// </value>
        public string InstalledBy { get; set; }

        /// <summary>
        /// Gets or sets the install date/time. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the install date/time.
        /// </value>
        public DateTime InstallDateTime { get; set; }
       
    }
}
