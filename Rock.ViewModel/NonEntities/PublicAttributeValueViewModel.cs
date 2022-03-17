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

namespace Rock.ViewModel.NonEntities
{
    /// <summary>
    /// A slimmed down version of <see cref="AttributeValueViewModel"/> designed
    /// for normal transmission of attribute values to the client when needed
    /// for display purposes.
    /// </summary>
    public class PublicAttributeValueViewModel : IViewModel
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
        public List<PublicAttributeValueCategoryViewModel> Categories { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly display text value.
        /// </summary>
        /// <value>
        /// The user-friendly display text value.
        /// </value>
        public string TextValue { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}
