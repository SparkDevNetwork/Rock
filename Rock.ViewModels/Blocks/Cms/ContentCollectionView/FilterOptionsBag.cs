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

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// Information about a single filter that may be shown in the filter panel.
    /// </summary>
    public class FilterOptionsBag
    {
        /// <summary>
        /// The unique key that identifies the filter source. If backed by
        /// an attribute then it will be the value "attr_" followed by the
        /// attribute key.
        /// </summary>
        /// <value>
        /// The unique key that identifies the filter source.
        /// </value>
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this filter is shown.
        /// </summary>
        /// <value><c>true</c> if filter is shown; otherwise, <c>false</c>.</value>
        public bool Show { get; set; }

        /// <summary>
        /// Gets or sets the display name of the filter.
        /// </summary>
        /// <value>The display name of the filter.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the markup that will be displayed before the filter.
        /// </summary>
        /// <value>The markup that will be displayed before the filter.</value>
        public string HeaderMarkup { get; set; }
    }
}
