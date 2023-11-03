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
    /// Identifies the data used by a Grid to request a new entity set be
    /// created. 
    /// </summary>
    public class GridEntitySetBag
    {
        /// <summary>
        /// Gets or sets the entity type key. Currently this must be a Guid
        /// value.
        /// </summary>
        /// <value>The entity type key.</value>
        public string EntityTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the items that will be stored in the set.
        /// </summary>
        /// <value>The items that will be stored in the set.</value>
        public List<GridEntitySetItemBag> Items { get; set; }
    }
}
