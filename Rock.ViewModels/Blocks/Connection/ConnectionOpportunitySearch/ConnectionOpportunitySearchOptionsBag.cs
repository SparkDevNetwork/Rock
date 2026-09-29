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

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySearch
{
    /// <summary>
    /// The configuration options for the Connection Opportunity Search block.
    /// </summary>
    public class ConnectionOpportunitySearchOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the search panel is shown.
        /// </summary>
        public bool IsSearchShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the name filter is shown.
        /// </summary>
        public bool IsNameFilterShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus filter is shown.
        /// </summary>
        public bool IsCampusFilterShown { get; set; }

        /// <summary>
        /// Gets or sets the label displayed above the campus filter.
        /// </summary>
        public string CampusLabel { get; set; }

        /// <summary>
        /// Gets or sets the campuses available in the campus filter.
        /// </summary>
        public List<ListItemBag> CampusItems { get; set; }

        /// <summary>
        /// Gets or sets the searchable attributes that are displayed as filters.
        /// </summary>
        public List<PublicAttributeBag> AttributeFilters { get; set; }
    }
}
