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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.UniversalSearch
{
    /// <summary>
    /// Describes a single refine search filter option.
    /// </summary>
    public class UniversalSearchFilterBag
    {
        /// <summary>
        /// Gets or sets the entity type identifier that owns this filter.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the field name used by the search index.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the label shown for this filter.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the selectable items for this filter.
        /// </summary>
        public List<ListItemBag> Items { get; set; }
    }
}
