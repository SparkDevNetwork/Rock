﻿// <copyright>
// Copyright by the Spark Development Network
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
    /// Data access/service class for <see cref="Rock.Model.TaggedItem"/> entity objects.
    /// </summary>
    public partial class TaggedItemService
    {
        /// <summary>
        /// Returns a list of <see cref="Rock.Model.TaggedItem">TaggedItems</see> by EntityType, QualifierColumn, QualifierValue, OwnerId and EntityGuid.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeID of an <see cref="Rock.Model.EntityType"/> of the <see cref="Rock.Model.TaggedItem"/>. </param>
        /// <param name="entityQualifierColumn">A <see cref="System.String"/> representing the EntityQualifierColumn of the <see cref="Rock.Model.Tag"/> that the <see cref="Rock.Model.TaggedItem"/>
        /// belongs to. If a qualifier column was not used, this value can be null.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String"/> representing the EntityQualifierValue of the <see cref="Rock.Model.Tag"/> that the <see cref="Rock.Model.TaggedItem"/>
        /// belongs to. If a qualifier value was not used, this  value can be null.</param>
        /// <param name="ownerId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is the owner of the <see cref="Rock.Model.Tag"/> that 
        /// the <see cref="Rock.Model.TaggedItem"/> belongs to. </param>
        /// <param name="entityGuid">A <see cref="System.Guid"/> representing the entity Guid of the <see cref="Rock.Model.TaggedItem"/></param>
        /// <returns>A queryable collection of <see cref="Rock.Model.TaggedItem">TaggedItems</see> that match the provided criteria.</returns>
        public IQueryable<TaggedItem> Get( int entityTypeId, string entityQualifierColumn, string entityQualifierValue, int? ownerId, Guid entityGuid )
        {
            return Queryable("Tag")
                .Where( t => t.Tag.EntityTypeId == entityTypeId &&
                    ( t.Tag.EntityTypeQualifierColumn == entityQualifierColumn || (t.Tag.EntityTypeQualifierColumn == null && entityQualifierColumn == null)) &&
                    ( t.Tag.EntityTypeQualifierValue == entityQualifierValue || (t.Tag.EntityTypeQualifierValue == null && entityQualifierValue == null)) &&
                    ( t.Tag.OwnerPersonAlias == null || ( ownerId.HasValue && t.Tag.OwnerPersonAlias.PersonId == ownerId ) ) &&
                    t.EntityGuid == entityGuid
                    )
                .OrderBy( t => t.Tag.Name);
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.TaggedItem"/> by TagId and EntityGuid. 
        /// </summary>
        /// <param name="tagId">A <see cref="System.Int32"/> representing the TagId of the <see cref="Rock.Model.Tag" /> that the <see cref="Rock.Model.TaggedItem"/> belongs to.</param>
        /// <param name="entityGuid">A <see cref="System.Guid"/> representing the Guid identifier of an <see cref="Rock.Model.TaggedItem">TaggedItem's</see> Entity object.</param>
        /// <returns>The <see cref="Rock.Model.TaggedItem"/> that matches the provided criteria. If a match is not found, null will be returned.</returns>
        public TaggedItem Get( int tagId, Guid entityGuid )
        {
            return Queryable()
                .Where( t => t.TagId == tagId && t.EntityGuid == entityGuid )
                .FirstOrDefault();
        }

    }
}
