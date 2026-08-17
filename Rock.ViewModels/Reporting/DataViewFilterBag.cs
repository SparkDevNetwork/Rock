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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// A serializable representation of a DataViewFilter
    /// tree used by Obsidian controls.
    /// </summary>
    public class DataViewFilterBag
    {
        /// <summary>
        /// The unique identifier for this filter/group node.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The expression type for this node.
        /// </summary>
        public FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// The unique identifier of the selected filter type entity.
        /// </summary>
        public Guid? FilterTypeGuid { get; set; }

        /// <summary>
        /// The persisted selection string for this filter.
        /// </summary>
        public string Selection { get; set; }

        /// <summary>
        /// The component data currently being edited by the Obsidian UI.
        /// </summary>
        public Dictionary<string, string> ComponentData { get; set; }

        /// <summary>
        /// The child filters for group nodes.
        /// </summary>
        public List<DataViewFilterBag> ChildFilters { get; set; }
    }
}
