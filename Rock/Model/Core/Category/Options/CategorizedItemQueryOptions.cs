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

namespace Rock.Model.Core.Category.Options
{
    /// <summary>
    /// Options for retrieving the items associated with a category.
    /// </summary>
    public class CategorizedItemQueryOptions
    {
        /// <summary>
        /// Gets or sets the category unique identifier. If <c>null</c> then
        /// only items without a category will be included.
        /// </summary>
        /// <value>The category unique identifier.</value>
        public Guid? CategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive items should be
        /// included in the results. If the entity type does not support the
        /// IsActive property then this value will be ignored.
        /// </summary>
        /// <value><c>true</c> if inactive items should be included; otherwise, <c>false</c>.</value>
        public bool IncludeInactiveItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether entity items without a name
        /// should be included in the results.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unnamed entity items should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeUnnamedEntityItems { get; set; }

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
    }
}
