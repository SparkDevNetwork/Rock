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

using Rock.Enums.Cms;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionDetail
{
    /// <summary>
    /// Class FilterSettingsBag.
    /// </summary>
    public class FilterSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether full text search should be enabled.
        /// </summary>
        public bool FullTextSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether searching content by year
        /// should be enabled.
        /// </summary>
        public bool YearSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the filter that allows an
        /// individual to search for content by a specific year.
        /// </summary>
        public string YearSearchLabel { get; set; }

        /// <summary>
        /// Gets or sets the year search filter control.
        /// </summary>
        public ContentCollectionFilterControl YearSearchFilterControl { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if multiple selection is
        /// used by the year search filter.
        /// </summary>
        public bool YearSearchFilterIsMultipleSelection { get; set; }

        /// <summary>
        /// Gets or sets the attributes that are enabled for filtering
        /// and indexing on the content collection.
        /// </summary>
        public List<AttributeFilterBag> AttributeFilters { get; set; }
    }
}
