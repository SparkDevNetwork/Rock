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
    /// Attribute POCO Service class
    /// </summary>
    public partial class AttributeService : Service<Attribute, AttributeDto>
    {
        /// <summary>
        /// Gets Attributes by Entity Type Id
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <returns>
        /// An enumerable list of Attribute objects.
        /// </returns>
        public IEnumerable<Attribute> GetByEntityTypeId( int? entityTypeId )
        {
            return Repository.Find( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue && !t.EntityTypeId.HasValue ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets Attributes by Entity Type Id, Entity Qualifier Column, Entity Qualifier Value
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        public IQueryable<Attribute> Get( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue )
        {
            return Queryable().Where( t =>
                ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue && !t.EntityTypeId.HasValue ) ) &&
                t.EntityTypeQualifierColumn == entityQualifierColumn &&
                t.EntityTypeQualifierValue == entityQualifierValue );
        }

        /// <summary>
        /// Gets Attribute by Entity Type Id, Entity Qualifier Column, Entity Qualifier Value, And Key
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityQualifierColumn">Entity Qualifier Column.</param>
        /// <param name="entityQualifierValue">Entity Qualifier Value.</param>
        /// <param name="key">Key.</param>
        /// <returns>
        /// Attribute object.
        /// </returns>
        public Attribute Get( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, string key )
        {
            return Repository.FirstOrDefault( t => 
                ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue && !t.EntityTypeId.HasValue ) ) && 
                ( t.EntityTypeQualifierColumn == entityQualifierColumn || ( entityQualifierColumn == null && t.EntityTypeQualifierColumn == null ) ) && 
                ( t.EntityTypeQualifierValue == entityQualifierValue || ( entityQualifierValue == null && t.EntityTypeQualifierValue == null ) ) && 
                t.Key == key );
        }
        
        /// <summary>
        /// Gets Attributes by Field Type Id
        /// </summary>
        /// <param name="fieldTypeId">Field Type Id.</param>
        /// <returns>An enumerable list of Attribute objects.</returns>
        public IEnumerable<Attribute> GetByFieldTypeId( int fieldTypeId )
        {
            return Repository.Find( t => t.FieldTypeId == fieldTypeId ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets the global attributes.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Attribute> GetGlobalAttributes()
        {
            return this.Get( null, string.Empty, string.Empty );
        }

        /// <summary>
        /// Gets a global attribute.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Attribute GetGlobalAttribute( string key )
        {
            return this.Get( null, string.Empty, string.Empty, key );
        }
    }
}
