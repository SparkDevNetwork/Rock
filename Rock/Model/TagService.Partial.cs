//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
            return Repository.AsQueryable()
                .Where( t => t.EntityTypeId == entityTypeId &&
                    ( t.EntityTypeQualifierColumn == entityQualifierColumn || ( t.EntityTypeQualifierColumn == null && entityQualifierColumn == null ) ) &&
                    ( t.EntityTypeQualifierValue == entityQualifierValue || ( t.EntityTypeQualifierValue == null && entityQualifierValue == null ) ) &&
                    ( t.OwnerId == null || (ownerId.HasValue && t.OwnerId == ownerId) ) 
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
            return Repository.AsQueryable()
                .Where( t => t.EntityTypeId == entityTypeId &&
                    ( t.EntityTypeQualifierColumn == entityQualifierColumn || ( t.EntityTypeQualifierColumn == null && entityQualifierColumn == null ) ) &&
                    ( t.EntityTypeQualifierValue == entityQualifierValue || ( t.EntityTypeQualifierValue == null && entityQualifierValue == null ) ) && 
                    ( t.OwnerId == null || ( ownerId.HasValue && t.OwnerId == ownerId )) &&
                    ( t.Name == name)
                    )
                .FirstOrDefault();
        }
    }
}
