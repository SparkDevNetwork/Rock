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
    /// Data access/service class for <see cref="Rock.Model.Category"/> objects.
    /// </summary>
    public partial class CategoryService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by parent <see cref="Rock.Model.Category"/> and <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="ParentId">A <see cref="System.Int32"/> representing the CategoryID of the parent <see cref="Rock.Model.Category"/> to search by. To find <see cref="Rock.Model.Category">Categories</see>
        /// that do not inherit from a parent category, this value will be null.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the specified criteria. </returns>
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
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by Name, <see cref="Rock.Model.EntityType"/>, Qualifier Column and Qualifier Value.
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> representing the name to search by.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityType of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityTypeQualifierColumn">A <see cref="System.String"/> representing the name of the Qualifier Column to search by.</param>
        /// <param name="entityTypeQualifierValue">A <see cref="System.String"/> representing the name of the Qualifier Value to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the search criteria.</returns>
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
        /// Returns a enumerable collection of <see cref="Rock.Model.Category">Categories</see> by <see cref="Rock.Model.EntityType"/>
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Category">Categories</see> are used for the specified <see cref="Rock.Model.Category"/>.</returns>
        public IEnumerable<Category> GetByEntityTypeId( int? entityTypeId )
        {
            return Repository.Find( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue ) ) );// TODO - do categories need an order? as in: .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Save( Category item, int? personId )
        {
            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            if ( item.IconLargeFileId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
                var binaryFile = binaryFileService.Get( item.IconLargeFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            if ( item.IconSmallFileId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
                var binaryFile = binaryFileService.Get( item.IconSmallFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
            
            return base.Save( item, personId );
        }
    }
}
