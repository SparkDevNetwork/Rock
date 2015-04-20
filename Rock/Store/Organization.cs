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
    public class Organization : StoreModel
    {
        /// <summary>
        /// Gets or sets the key for the organization. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the key of the organization.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Organization. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Organization.
        /// </value>
        public string Name { get; set; }
    }
}
