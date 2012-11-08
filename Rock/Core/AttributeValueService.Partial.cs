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
    /// Attribute Value POCO Service class
    /// </summary>
    public partial class AttributeValueService : Service<AttributeValue, AttributeValueDto>
    {
        /// <summary>
        /// Gets Attribute Values by Attribute Id
        /// </summary>
        /// <param name="attributeId">Attribute Id.</param>
        /// <returns>An enumerable list of AttributeValue objects.</returns>
        public IEnumerable<AttributeValue> GetByAttributeId( int attributeId )
        {
            return Repository.Find( t => t.AttributeId == attributeId );
        }
        
        /// <summary>
        /// Gets Attribute Values by Attribute Id And Entity Id
        /// </summary>
        /// <param name="attributeId">Attribute Id.</param>
        /// <param name="entityId">Entity Id.</param>
        /// <returns>An enumerable list of AttributeValue objects.</returns>
        public IEnumerable<AttributeValue> GetByAttributeIdAndEntityId( int attributeId, int? entityId )
        {
            return Repository.Find( t => t.AttributeId == attributeId && ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) );
        }
        
        /// <summary>
        /// Gets Attribute Values by Entity Id
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <returns>An enumerable list of AttributeValue objects.</returns>
        public IEnumerable<AttributeValue> GetByEntityId( int? entityId )
        {
            return Repository.Find( t => ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) );
        }

        /// <summary>
        /// Gets the by attribute id and entity id.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public IEnumerable<AttributeValue> GetByAttributeIdAndEntityId( int attributeId, int entityId )
        {
            return Repository.AsQueryable().
                Where( v => v.AttributeId == attributeId && v.EntityId == entityId ).
                OrderBy( v => v.Order );
        }

        /// <summary>
        /// Gets the global attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public AttributeValue GetGlobalAttributeValue( string key )
        {
            return Repository.AsQueryable()
                .Where( v =>
                    !v.Attribute.EntityTypeId.HasValue &&
                    v.Attribute.EntityTypeQualifierColumn == string.Empty &&
                    v.Attribute.EntityTypeQualifierColumn == string.Empty &&
                    v.Attribute.Key == key &&
                    !v.EntityId.HasValue )
                .FirstOrDefault();
        }
    }
}
