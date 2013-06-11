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
    /// Category POCO Service class
    /// </summary>
    public partial class CategoryService
    {
        /// <summary>
        /// Gets the specified parent id.
        /// </summary>
        /// <param name="ParentId">The parent id.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <returns></returns>
        public IQueryable<Category> Get( int? ParentId, int? entityTypeId )
        {
            var query = Repository.AsQueryable()
                .Where( c => (c.ParentCategoryId ?? 0) == (ParentId ?? 0) );

            if ( entityTypeId.HasValue )
            {
                query = query.Where( c => c.EntityTypeId == entityTypeId.Value );
            }

            return query;
        }

        /// <summary>
        /// Gets the category by name and entity type
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public IQueryable<Category> Get( string name, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Repository.AsQueryable()
                .Where( c =>
                    string.Compare( c.Name, name, true ) == 0 &&
                    c.EntityTypeId == entityTypeId &&
                    c.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
                    c.EntityTypeQualifierValue == entityTypeQualifierValue );
        }

        /// <summary>
        /// Gets Categories for the given Entity Type
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <returns></returns>
        public IEnumerable<Category> GetByEntityTypeId( int? entityTypeId )
        {
            return Repository.Find( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue ) ) );// TODO - do categories need an order? as in: .OrderBy( t => t.Order );
        }
    }
}
