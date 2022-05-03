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

using Rock.ClientService.Core.Category.Options;

namespace Rock.Rest.v2.Options
{
    /// <summary>
    /// The options for retrieving child tree items for the category picker.
    /// </summary>
    public class CategoryPickerChildTreeItemsOptions
    {
        /// <inheritdoc cref="CategoryItemTreeOptions.ParentGuid"/>
        public Guid? ParentGuid { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.GetCategorizedItems"/>
        public bool GetCategorizedItems { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.EntityTypeGuid"/>
        public Guid? EntityTypeGuid { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.EntityTypeQualifierColumn"/>
        public string EntityTypeQualifierColumn { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.EntityTypeQualifierValue"/>
        public string EntityTypeQualifierValue { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.IncludeUnnamedEntityItems"/>
        public bool IncludeUnnamedEntityItems { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.IncludeCategoriesWithoutChildren"/>
        public bool IncludeCategoriesWithoutChildren { get; set; } = true;

        /// <inheritdoc cref="CategoryItemTreeOptions.DefaultIconCssClass"/>
        public string DefaultIconCssClass { get; set; } = "fa fa-list-ol";

        /// <inheritdoc cref="CategoryItemTreeOptions.IncludeInactiveItems"/>
        public bool IncludeInactiveItems { get; set; }

        /// <inheritdoc cref="CategoryItemTreeOptions.LazyLoad"/>
        public bool LazyLoad { get; set; } = true;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
