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

using System.Collections.Generic;

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Describes the structure of a grid. This should be passed to
    /// the grid.
    /// </summary>
    public class GridDefinitionBag
    {
        /// <summary>
        /// Gets or sets the fields that have been defined on the grid. Each
        /// field has a corresponding entry in the row data object.
        /// </summary>
        /// <value>The fields that have been defined on the grid.</value>
        public List<FieldDefinitionBag> Fields { get; set; }

        /// <summary>
        /// Gets or sets the dynamic fields that have been defined on the
        /// grid. Each dynamic field has a corresponding entry in the row
        /// data object.
        /// </summary>
        /// <value>The dynamic fields.</value>
        public List<DynamicFieldDefinitionBag> DynamicFields { get; set; }

        /// <summary>
        /// Gets or sets the attribute fields that have been defined on the
        /// grid. Each attribute field has a corresponding entry in the row
        /// data object.
        /// </summary>
        /// <value>The attribute fields.</value>
        public List<AttributeFieldDefinitionBag> AttributeFields { get; set; }

        /// <summary>
        /// Gets or sets the action urls. These are used as a lookup table for
        /// when a standard action is executed on a set of items. The key is
        /// something known by the Grid internally.
        /// </summary>
        /// <value>The action urls.</value>
        public Dictionary<string, string> ActionUrls { get; set; }

        /// <summary>
        /// Gets or sets the definitions of any custom columns that should be
        /// added to the grid. These are defined by the administrator.
        /// </summary>
        /// <value>The definitions of any custom columns.</value>
        public List<CustomColumnDefinitionBag> CustomColumns { get; set; }

        /// <summary>
        /// Gets or sets the custom actions that should be added to the grid.
        /// </summary>
        /// <value>The custom actions.</value>
        public List<CustomActionBag> CustomActions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sticky header should
        /// be enabled. This is additive, if the developer has explicitly turned
        /// off sticky headers then they will not be enabled by this.
        /// </summary>
        /// <value><c>true</c> if sticky headers should be enabled; otherwise, <c>false</c>.</value>
        public bool EnableStickyHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the launch workflow action
        /// should be included. This is additive, if the developer has explicitly
        /// turned off launch workflows then they will not be enabled by this.
        /// </summary>
        /// <value><c>true</c> if the launch workflow action should be enabled; otherwise, <c>false</c>.</value>
        public bool EnableLaunchWorkflow { get; set; }
    }
}
