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
    /// MetricValue POCO Service class
    /// </summary>
    public partial class TaggedItemService
    {
        /// <summary>
        /// Gets the tag by entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="ownerId">The owner id.</param>
        /// <param name="entityGuid">The entity GUID.</param>
        /// <returns></returns>
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
        /// Gets tag by tagId and entityId.
        /// </summary>
        /// <param name="tagId">The tag id.</param>
        /// <param name="entityGuid">The entity GUID.</param>
        /// <returns></returns>
        public TaggedItem Get( int tagId, Guid entityGuid )
        {
            return Repository.AsQueryable()
                .Where( t => t.TagId == tagId && t.EntityGuid == entityGuid )
                .FirstOrDefault();
        }

    }
}
