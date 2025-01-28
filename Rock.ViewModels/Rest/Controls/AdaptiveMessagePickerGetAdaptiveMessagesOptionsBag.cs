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

using System;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetAdaptiveMessages API action of
    /// the AdaptiveMessagePicker control.
    /// </summary>
    public class AdaptiveMessagePickerGetAdaptiveMessagesOptionsBag
    {
        /// <summary>
        /// Gets or sets the parent unique identifier whose children are to
        /// be retrieved. If null then the root items are being requested.
        /// </summary>
        /// <value>The parent unique identifier.</value>
        public Guid? ParentGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items should be loaded
        /// or only categories.
        /// </summary>
        /// <value><c>true</c> if items should be loaded; otherwise, <c>false</c>.</value>
        public bool GetCategorizedItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether entity items without a name
        /// should be included in the results. Only applies if
        /// <see cref="GetCategorizedItems"/> is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unnamed entity items should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeUnnamedEntityItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether categories that have no
        /// child categories and no items should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if categories with no children should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeCategoriesWithoutChildren { get; set; } = false;

        /// <summary>
        /// Gets or sets the default icon CSS class to use for items that do not
        /// specify their own IconCssClass value.
        /// </summary>
        /// <value>The default icon CSS class.</value>
        public string DefaultIconCssClass { get; set; } = "fa fa-list-ol";

        /// <summary>
        /// Gets or sets a value indicating whether child categories and items
        /// are loaded automatically. If <c>true</c> then all descendant categories
        /// will be loaded along with the items if <see cref="GetCategorizedItems"/>
        /// is also true. This results in the Children property of the results
        /// being null to indicate they must be loaded on demand.
        /// </summary>
        /// <value><c>true</c> if child items should not be loaded eagerly; otherwise, <c>false</c>.</value>
        public bool LazyLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}