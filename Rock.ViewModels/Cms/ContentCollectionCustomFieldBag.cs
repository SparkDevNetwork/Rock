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

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// Defines a custom field that will be used to store custom data in the
    /// index for an item.
    /// </summary>
    public class ContentCollectionCustomFieldBag
    {
        /// <summary>
        /// Gets or sets the key used in the item index.
        /// </summary>
        /// <value>The index key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the friendly display title.
        /// </summary>
        /// <value>The friendly display title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value will be treated
        /// as a multi-value field. When enabled the final value will be split
        /// by comma and turned into multiple values.
        /// </summary>
        /// <value><c>true</c> if this field renders multiple values; otherwise, <c>false</c>.</value>
        public bool IsMultiple { get; set; }

        /// <summary>
        /// Gets or sets the lava template to use when generating custom content.
        /// </summary>
        /// <value>The lava template to use when generating custom content.</value>
        public string Template { get; set; }
    }
}
