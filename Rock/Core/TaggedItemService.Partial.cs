//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// MetricValue POCO Service class
    /// </summary>
    public partial class TaggedItemService
    {
        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="ownerId">The owner id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public IQueryable<TaggedItem> GetByEntity( string entity, string entityQualifierColumn, string entityQualifierValue, int? ownerId, int entityId )
        {
            return Repository.AsQueryable()
                .Where( t => t.Tag.Entity == entity &&
                    ( t.Tag.EntityTypeQualifierColumn == entityQualifierColumn || (t.Tag.EntityTypeQualifierColumn == null && entityQualifierColumn == null)) &&
                    ( t.Tag.EntityTypeQualifierValue == entityQualifierValue || (t.Tag.EntityTypeQualifierValue == null && entityQualifierValue == null)) &&
                    ( t.Tag.OwnerId == null || ( ownerId.HasValue && t.Tag.OwnerId == ownerId ) ) &&
                    t.EntityId == entityId
                    )
                .OrderBy( t => t.Tag.Name);
        }

        /// <summary>
        /// Gets the by tag.
        /// </summary>
        /// <param name="tagId">The tag id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public TaggedItem GetByTag( int tagId, int entityId )
        {
            return Repository.AsQueryable()
                .Where( t => t.TagId == tagId && t.EntityId == entityId)
                .FirstOrDefault();
        }

    }
}
