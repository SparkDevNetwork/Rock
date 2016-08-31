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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Tag"/> entity objects.
    /// </summary>
    public partial class TagService
    {

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Tag">Tags</see> by EntityType, Qualifier Column, Qualifier Value and Owner.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32" /> representing the EntityTypeID of the <see cref="Rock.Model.EntityType"/> of the entities that are eligible for the <see cref="Rock.Model.Tag"/>.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String"/> that represents the EntityQualifierColumn of the <see cref="Rock.Model.Tag"/>. This value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String"/> that represents the EntityQualifierValue of the <see cref="Rock.Model.Tag"/>. This value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32"/> representing the <see cref="Rock.Model.Tag"/> owner's PersonId. If the <see cref="Rock.Model.Tag"/> is public this value can be null.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Tag">Tags</see> that match the provided criteria.</returns>
        public IQueryable<Tag> Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId )
        {
            return Queryable()
                .Where( t => t.EntityTypeId == entityTypeId &&
                    ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == "" || t.EntityTypeQualifierColumn == entityQualifierColumn ) &&
                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == "" || t.EntityTypeQualifierValue == entityQualifierValue ) &&
                    ( t.OwnerPersonAlias == null || (ownerId.HasValue && t.OwnerPersonAlias.PersonId == ownerId) ) 
                    )
                .OrderBy( t => t.Name );
        }

        /// <summary>
        /// Returns an <see cref="Rock.Model.Tag"/> by EntityType, Qualifier Column, Qualifier Value, Owner and Tag Name.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeID of the <see cref="Rock.Model.EntityType"/> of entities that are eligible for the <see cref="Rock.Model.Tag"/>.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String"/> representing the EntityQualifierColumn of the <see cref="Rock.Model.Tag"/>. 
        /// If the <see cref="Rock.Model.Tag"/> does not have a EntityQualifierColumn associated with it, this value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String"/> representing the EntityQualifierValue of the <see cref="Rock.Model.Tag"/>.
        /// If the <see cref="Rock.Model.Tag"/> does not have a EntityQualifierValue associated with it, this value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32"/> representing the owner's PersonId.</param>
        /// <param name="name">A <see cref="System.String"/> representing the Name of the <see cref="Rock.Model.Tag"/>.</param>
        /// <returns>The <see cref="Rock.Model.Tag"/> that matches the provided criteria. If a match is not found, null will be returned.</returns>
        public Tag Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId, string name )
        {
            return Queryable()
                .Where( t => t.EntityTypeId == entityTypeId &&
                    ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == "" || t.EntityTypeQualifierColumn == entityQualifierColumn ) &&
                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == "" || t.EntityTypeQualifierValue == entityQualifierValue ) &&
                    ( t.OwnerPersonAlias == null || ( ownerId.HasValue && t.OwnerPersonAlias.PersonId == ownerId ) ) &&
                    ( t.Name == name)
                    )
                .FirstOrDefault();
        }
    }
}
