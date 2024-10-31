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

namespace Rock.ViewModels.Blocks.Reporting.PageParameterFilter
{
    /// <summary>
    /// A bag that contains information about updated filters for the page parameter filter block.
    /// </summary>
    public class GetUpdatedFiltersResponseBag
    {
        /// <summary>
        /// Gets or sets the public filters available for selection.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> PublicFilters { get; set; }

        /// <summary>
        /// Gets or sets the public filter values.
        /// </summary>
        public Dictionary<string, string> PublicFilterValues { get; set; }

        /// <summary>
        /// Gets or sets the filter page parameters.
        /// </summary>
        public Dictionary<string, string> FilterPageParameters { get; set; }
    }
}
