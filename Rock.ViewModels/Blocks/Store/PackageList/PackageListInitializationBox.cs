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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Store.PackageList
{
    /// <summary>
    /// The box that contains the initialization information for the Package List block.
    /// </summary>
    public class PackageListInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the packages to display.
        /// </summary>
        public List<PackageListItemBag> Packages { get; set; }

        /// <summary>
        /// Gets or sets the store error message. When non-empty, the store could not be
        /// reached and the component renders the "Store Currently Not Available" panel
        /// instead of the package list.
        /// </summary>
        public string StoreErrorMessage { get; set; }
    }
}
