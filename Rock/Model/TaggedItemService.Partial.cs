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
            return Repository.AsQueryable()
                .Where( t => t.Tag.EntityTypeId == entityTypeId &&
                    ( t.Tag.EntityTypeQualifierColumn == entityQualifierColumn || (t.Tag.EntityTypeQualifierColumn == null && entityQualifierColumn == null)) &&
                    ( t.Tag.EntityTypeQualifierValue == entityQualifierValue || (t.Tag.EntityTypeQualifierValue == null && entityQualifierValue == null)) &&
                    ( t.Tag.OwnerId == null || ( ownerId.HasValue && t.Tag.OwnerId == ownerId ) ) &&
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
            return Repository.AsQueryable()
                .Where( t => t.TagId == tagId && t.EntityGuid == entityGuid )
                .FirstOrDefault();
        }

    }
}
