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
        /// Gets Attributes by EntityName
        /// </summary>
        /// <param name="entityName">Entity.</param>
        /// <returns>An enumerable list of Attribute objects.</returns>
        public IEnumerable<Attribute> Get( string entityName )
        {
            return Repository
                .Find( t =>
                    t.EntityType.Name == entityName || ( entityName == null && !t.EntityTypeId.HasValue )
                 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets the attributes by EntityName and qualifier value.
        /// </summary>
        /// <param name="entityName">The entity.</param>
        /// <param name="entityTypeQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        public IQueryable<Attribute> Get( string entityName, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Queryable().Where( t =>
                t.EntityType.Name == entityName &&
                t.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
                t.EntityTypeQualifierValue == entityTypeQualifierValue );
        }

        /// <summary>
        /// Gets Attribute by EntityName, Qualifier value, and Key
        /// </summary>
        /// <param name="entityName">Entity.</param>
        /// <param name="entityTypeQualifierColumn">Entity Qualifier Column.</param>
        /// <param name="entityTypeQualifierValue">Entity Qualifier Value.</param>
        /// <param name="key">Key.</param>
        /// <returns>Attribute object.</returns>
        public Attribute Get( string entityName, string entityTypeQualifierColumn, string entityTypeQualifierValue, string key )
        {
            return Repository.FirstOrDefault( t => 
                ( t.EntityType.Name == entityName || ( entityName == null && !t.EntityTypeId.HasValue ) ) && 
                ( t.EntityTypeQualifierColumn == entityTypeQualifierColumn || ( entityTypeQualifierColumn == null && t.EntityTypeQualifierColumn == null ) ) && 
                ( t.EntityTypeQualifierValue == entityTypeQualifierValue || ( entityTypeQualifierValue == null && t.EntityTypeQualifierValue == null ) ) && 
                t.Key == key 
            );
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
