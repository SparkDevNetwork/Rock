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
    /// Represents a store category for packages.
    /// </summary>
    public class PurchaseResponse : StoreModel
    {
        /// <summary>
        /// Gets or sets the result of the purchase. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> representing the result of the purchase.
        /// </value>
        public PurchaseResult PurchaseResult { get; set; }

        /// <summary>
        /// Gets or sets the message of the purchase result. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing message of the purchase result.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package.
        /// </value>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Package.
        /// </value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Vendor.
        /// </value>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Vendor. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Vendor.
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
        /// Gets or sets the package install steps.
        /// </summary>
        /// <value>
        /// The package install steps.
        /// </value>
        public List<PackageInstallStep> PackageInstallSteps { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PurchaseResult
    {
        /// <summary>
        /// The success
        /// </summary>
        Success,

        /// <summary>
        /// The authenication failed
        /// </summary>
        AuthenicationFailed,

        /// <summary>
        /// The not authorized
        /// </summary>
        NotAuthorized,

        /// <summary>
        /// The no card on file
        /// </summary>
        NoCardOnFile,

        /// <summary>
        /// The payment failed
        /// </summary>
        PaymentFailed,

        /// <summary>
        /// The error
        /// </summary>
        Error
    }
}
