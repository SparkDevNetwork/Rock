﻿// <copyright>
// Copyright by the Spark Development Network
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
    public class PackageInstallStep : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the package version.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the Label of the Version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Label of the Version.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Install Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing URL of the Install Package.
        /// </value>
        public string InstallPackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the post install instructions. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing post install instructions.
        /// </value>
        public string PostInstallInstructions { get; set; }
    }
}
