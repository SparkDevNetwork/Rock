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
using System.Collections.Generic;

using Rock.Security;

namespace Rock.ClientService.Core.Category.Options
{
    /// <summary>
    /// Specifies the options when loading category tree items.
    /// </summary>
    public class CategoryItemTreeOptions
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
        /// Gets or sets the entity type unique identifier to limit the
        /// results to.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier that is used to filter category
        /// results. If not blank, then the category EntityTypeQualifierColumn
        /// property must match this value.
        /// </summary>
        /// <value>The entity qualifier used to filter results.</value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier value that is used to filter
        /// category results. If both this and <see cref="EntityTypeQualifierColumn"/>
        /// are not blank, then the category EntityTypeQualifierValue property
        /// must match this value.
        /// </summary>
        /// <value>The entity qualifier value used to filter results.</value>
        public string EntityTypeQualifierValue { get; set; }

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
        public bool IncludeCategoriesWithoutChildren { get; set; } = true;

        /// <summary>
        /// Gets or sets the category unique identifiers to be included in the
        /// results. If this contains any values then any categories that do
        /// not match one of these values will be excluded. If this value is set
        /// then <see cref="ExcludeCategoryGuids"/> is ignored.
        /// </summary>
        /// <value>The unique identifiers to limit category results to.</value>
        public List<Guid> IncludeCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the unique category identifiers to be excluded from
        /// the results. If this contains any values then any categories that
        /// match one of these values will be excluded. Only takes effect if
        /// <see cref="IncludeCategoryGuids"/> is null or empty.
        /// </summary>
        /// <value>The unique category identifiers to exclude from the results.</value>
        public List<Guid> ExcludeCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the default icon CSS class to use for items that do not
        /// specify their own IconCssClass value.
        /// </summary>
        /// <value>The default icon CSS class.</value>
        public string DefaultIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive items should be
        /// included in the results. If the entity type does not support the
        /// IsActive property then this value will be ignored.
        /// </summary>
        /// <value><c>true</c> if inactive items should be included; otherwise, <c>false</c>.</value>
        public bool IncludeInactiveItems { get; set; }

        /// <summary>
        /// Gets or sets the name of the item property to use for comparison
        /// with <see cref="ItemFilterPropertyValue"/>. When set the item query
        /// will attempt to filter on this property name. If it does not exist
        /// then an exception will be thrown.
        /// </summary>
        /// <value>The name of the item property to use for custom filtering.</value>
        public string ItemFilterPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the item property value to compare against. This should
        /// be either an integer-string, Guid-string or plain string for
        /// comparison. And must match the database property type.
        /// </summary>
        /// <value>The item property value to compare against.</value>
        public string ItemFilterPropertyValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child categories and items
        /// are loaded automatically. If <c>true</c> then all descendant categories
        /// will be loaded along with the items if <see cref="GetCategorizedItems"/>
        /// is also true. This results in the Children property of the results
        /// being null to indicate they must be loaded on demand.
        /// </summary>
        /// <value><c>true</c> if child items should not be loaded eagerly; otherwise, <c>false</c>.</value>
        public bool LazyLoad { get; set; }

        /// <summary>
        /// Gets or sets the security grant which provides additional authorization
        /// checks when not null.
        /// </summary>
        /// <value>The security grant to use for additional authorization.</value>
        internal SecurityGrant SecurityGrant { get; set; }
    }
}
