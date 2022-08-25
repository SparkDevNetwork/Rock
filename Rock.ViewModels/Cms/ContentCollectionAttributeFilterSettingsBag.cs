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

using Rock.Enums.Cms;

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// The settings for a single attribute filter configured on a content collection.
    /// </summary>
    public class ContentCollectionAttributeFilterSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating if this search filter is enabled.
        /// </summary>
        /// <value>
        /// A value indicating if this search filter is enabled.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the label to use for the filter.
        /// </summary>
        /// <value>
        /// The label to use for the Year search filter.
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the search filter control.
        /// </summary>
        /// <value>
        /// The search filter control.
        /// </value>
        public ContentCollectionFilterControl FilterControl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if multiple selection is allowed.
        /// </summary>
        /// <value>
        /// A value indicating if multiple selection is allowed.
        /// </value>
        public bool IsMultipleSelection { get; set; }
    }
}
