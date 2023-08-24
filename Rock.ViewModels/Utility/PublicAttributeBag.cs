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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// A slimmed down version of an attribute that includes enough detail to
    /// view and edit a value, but not enough to edit the attribute itself.
    /// </summary>
    public class PublicAttributeBag
    {
        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>
        /// The field type unique identifier.
        /// </value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>
        /// The attribute unique identifier.
        /// </value>
        public Guid AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the attribute on the entity.
        /// </summary>
        /// <value>
        /// The key that identifies the attribute on the entity.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute value is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this attribute value is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<PublicAttributeCategoryBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the configuration values.
        /// </summary>
        /// <value>The configuration values.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value> The HTML string the is prepended before the attribute's markup. </value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value> The HTML string the is appended after the attribute's markup. </value>
        public string PostHtml { get; set; }
    }
}
