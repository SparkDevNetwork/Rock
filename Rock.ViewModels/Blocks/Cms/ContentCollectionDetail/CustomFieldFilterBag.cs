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
    /// Represents a single custom field filter for a content collection.
    /// </summary>
    public class CustomFieldFilterBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the field.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the internal identification key of the field.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the enabled state of this filter.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the names of the sources that make up this filter.
        /// </summary>
        public List<string> SourceNames { get; set; }

        /// <summary>
        /// Gets or sets the friendly label to use when displaying this filter.
        /// </summary>
        public string FilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the type of control to use when displaying this filter.
        /// </summary>
        public ContentCollectionFilterControl FilterControl { get; set; }

        /// <summary>
        /// Gets or sets if multiple selections are allowed.
        /// </summary>
        public bool IsMultipleSelection { get; set; }
    }
}
