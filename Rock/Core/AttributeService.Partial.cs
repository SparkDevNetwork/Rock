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
    public partial class AttributeService : Service<Attribute, AttributeDTO>
    {
		/// <summary>
		/// Gets Attributes by Entity
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <returns>An enumerable list of Attribute objects.</returns>
	    public IEnumerable<Attribute> GetByEntity( string entity )
        {
            return Repository.Find( t => ( t.Entity == entity || ( entity == null && t.Entity == null ) ) ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Attribute by Entity And Entity Qualifier Column And Entity Qualifier Value And Key
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="entityQualifierColumn">Entity Qualifier Column.</param>
		/// <param name="entityQualifierValue">Entity Qualifier Value.</param>
		/// <param name="key">Key.</param>
		/// <returns>Attribute object.</returns>
	    public Attribute GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey( string entity, string entityQualifierColumn, string entityQualifierValue, string key )
        {
            return Repository.FirstOrDefault( t => ( t.Entity == entity || ( entity == null && t.Entity == null ) ) && ( t.EntityQualifierColumn == entityQualifierColumn || ( entityQualifierColumn == null && t.EntityQualifierColumn == null ) ) && ( t.EntityQualifierValue == entityQualifierValue || ( entityQualifierValue == null && t.EntityQualifierValue == null ) ) && t.Key == key );
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
        /// Gets the attributes by entity qualifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetAttributesByEntityQualifier( string entity, string entityQualifierColumn, string entityQualifierValue )
        {
            return Queryable().Where( t =>
                t.Entity == entity &&
                t.EntityQualifierColumn == entityQualifierColumn &&
                t.EntityQualifierValue == entityQualifierValue );
        }

        /// <summary>
        /// Gets a global attribute.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Attribute GetGlobalAttribute( string key )
        {
            return this.GetByEntityAndEntityQualifierColumnAndEntityQualifierValueAndKey( string.Empty, string.Empty, string.Empty, key );
        }
    }
}
