// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service for <see cref="Rock.Model.AttributeValue"/> entity objects.
    /// </summary>
    public partial class AttributeValueService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <param name="attributeId">A <see cref="System.Int32" /> that represents the AttributeId of the <see cref="Rock.Model.Attribute"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by the specified <see cref="Rock.Model.Attribute"/>.</returns>
        public IQueryable<AttributeValue> GetByAttributeId( int attributeId )
        {
            return Queryable().Where( t => t.AttributeId == attributeId );
        }

        /// <summary>
        /// Gets an Attribute Value by Attribute Id And Entity Id
        /// </summary>
        /// <param name="attributeId">Attribute Id.</param>
        /// <param name="entityId">Entity Id.</param>
        /// <returns></returns>
        public AttributeValue GetByAttributeIdAndEntityId( int attributeId, int? entityId )
        {
            return Queryable()
                .Where( t =>
                    t.AttributeId == attributeId &&
                    ( 
                        ( !t.EntityId.HasValue && !entityId.HasValue ) || 
                        ( t.EntityId.HasValue && entityId.HasValue && t.EntityId.Value == entityId.Value )
                    )
                )
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.
        /// </summary>
        /// <param name="entityId">A <see cref="System.Int32"/> representing the EntityId to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.</returns>
        public IQueryable<AttributeValue> GetByEntityId( int? entityId )
        {
            return Queryable().Where( t => ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.AttributeValue"/> for a <see cref="Rock.Model.Attribute"/> by Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Global <see cref="Rock.Model.Attribute">Attribute's</see> key value.</param>
        /// <returns>The <see cref="Rock.Model.AttributeValue" /> of the global <see cref="Rock.Model.Attribute"/>.</returns>
        public AttributeValue GetGlobalAttributeValue( string key )
        {
            return Queryable()
                .Where( v =>
                    v.Attribute.Key == key &&
                    !v.Attribute.EntityTypeId.HasValue &&
                    ( v.Attribute.EntityTypeQualifierColumn == null || v.Attribute.EntityTypeQualifierColumn == string.Empty ) &&
                    ( v.Attribute.EntityTypeQualifierColumn == null || v.Attribute.EntityTypeQualifierColumn == string.Empty ) &&
                    !v.EntityId.HasValue )
                .FirstOrDefault();
        }

    }
}
