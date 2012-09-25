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
		public IQueryable<TaggedItem> GetByEntity( string entity, string entityQualifierColumn, string entityQualifierValue, int? ownerId, int entityId )
        {
			return Repository.AsQueryable()
				.Where( t => t.Tag.Entity == entity &&
					( t.Tag.EntityQualifierColumn == entityQualifierColumn || (t.Tag.EntityQualifierColumn == null && entityQualifierColumn == null)) &&
					( t.Tag.EntityQualifierValue == entityQualifierValue || (t.Tag.EntityQualifierValue == null && entityQualifierValue == null)) &&
					( t.Tag.OwnerId == null || ( ownerId.HasValue && t.Tag.OwnerId == ownerId ) ) &&
					t.EntityId == entityId
					)
				.OrderBy( t => t.Tag.Name);
        }

		public TaggedItem GetByTag( int tagId, int entityId )
		{
			return Repository.AsQueryable()
				.Where( t => t.TagId == tagId && t.EntityId == entityId)
				.FirstOrDefault();
		}

    }
}
