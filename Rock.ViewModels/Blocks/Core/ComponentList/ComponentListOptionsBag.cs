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

namespace Rock.ViewModels.Blocks.Core.ComponentList
{
    /// <summary>
    /// The options bag for the Component List block.
    /// </summary>
    public class ComponentListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether ordering is supported.
        /// When <c>true</c> and the individual has Administrate access, a
        /// reorder column is displayed in the grid.
        /// </summary>
        public bool IsSupportOrdering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per-component security is supported.
        /// When <c>true</c> and the individual has Administrate access, a
        /// security button is displayed for each row.
        /// </summary>
        public bool IsSupportSecurity { get; set; }
    }
}
