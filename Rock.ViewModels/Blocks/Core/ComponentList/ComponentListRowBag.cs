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

namespace Rock.ViewModels.Blocks.Core.ComponentList
{
    /// <summary>
    /// A single row in the Component List grid representing a MEF component.
    /// </summary>
    public class ComponentListRowBag
    {
        /// <summary>
        /// Gets or sets the EntityType GUID for this component. Used as
        /// the row identifier for the grid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the EntityType identifier for this component,
        /// used to display per-component security.
        /// </summary>
        public int EntityTypeId { get; set; }
    }
}
