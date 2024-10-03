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

namespace Rock.Data
{
    /// <summary>
    /// Specifies that the entity supports the fast queryable entity attributes
    /// backed by SQL views.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, Inherited = false )]
    internal class HasQueryableAttributesAttribute: System.Attribute
    {
        /// <summary>
        /// The type that represents the attribute values, this should be a
        /// type that inherits from <see cref="QueryableAttributeValue"/>.
        /// </summary>
        public Type AttributeValueType { get; }

        /// <summary>
        /// The name of the navigation property. Must be defined as an
        /// <see cref="ICollection{T}"/> with an inner type of
        /// <see cref="AttributeValueType"/>.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Creates a new instance of <see cref="HasQueryableAttributesAttribute"/>.
        /// </summary>
        /// <param name="attributeValueType">The type that represents the attribute values, which should inherit from <see cref="QueryableAttributeValue"/>.</param>
        /// <param name="propertyName">The name of the navigation property. Must be defined as an <see cref="ICollection{T}"/> with an inner type of <paramref name="attributeValueType"/>.</param>
        public HasQueryableAttributesAttribute( Type attributeValueType, string propertyName )
        {
            AttributeValueType = attributeValueType;
            PropertyName = propertyName;
        }
    }
}
