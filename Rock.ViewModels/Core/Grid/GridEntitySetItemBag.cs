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

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Identifies a single entity set item that should be created when a Grid
    /// needs to create an entity set.
    /// </summary>
    public class GridEntitySetItemBag
    {
        /// <summary>
        /// Gets or sets the entity key. This should either be a Guid, IdKey
        /// or Id property value.
        /// </summary>
        /// <value>The entity key.</value>
        public string EntityKey { get; set; }

        /// <summary>
        /// Gets or sets the order of the item in the set.
        /// </summary>
        /// <value>The order of the item in the set.</value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the additional merge values.
        /// </summary>
        /// <value>The additional merge values.</value>
        public Dictionary<string, object> AdditionalMergeValues { get; set; }
    }
}
