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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// Defines the settings used by the Content Collection filters as stored
    /// in the database.
    /// </summary>
    public class ContentCollectionFilterSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether full text search should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if full text search should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool FullTextSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether searching content by year
        /// should be enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if searching content by year is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool YearSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the filter that allows an
        /// individual to search for content by a specific year.
        /// </summary>
        /// <value>
        /// The label to use for the Year search filter.
        /// </value>
        public string YearSearchLabel { get; set; }

        /// <summary>
        /// Gets or sets the year search filter control.
        /// </summary>
        /// <value>
        /// The year search filter control.
        /// </value>
        public ContentCollectionFilterControl YearSearchFilterControl { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if multiple selection is
        /// used by the year search filter.
        /// </summary>
        /// <value>
        /// A value that indicates if multiple selection is used by the year
        /// search filter.
        /// </value>
        public bool YearSearchFilterIsMultipleSelection { get; set; }

        /// <summary>
        /// Gets or sets the attributes that are enabled for filtering
        /// and indexing on the content collection.
        /// </summary>
        /// <value>
        /// The attributes that are enable for filtering.
        /// </value>
        public Dictionary<string, ContentCollectionAttributeFilterSettingsBag> AttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the field values that have been learned. These
        /// are used to display the known values in the filter panel.
        /// </summary>
        /// <value>The field values that have been learned.</value>
        public Dictionary<string, List<ListItemBag>> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the attribute values that have been learned. These
        /// are used to display the known values in the filter panel.
        /// </summary>
        /// <value>The attribute values that have been learned.</value>
        public Dictionary<string, List<ListItemBag>> AttributeValues { get; set; }
    }
}
