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
    /// Data access/service class for <see cref="Rock.Model.Attribute"/> entities.
    /// </summary>
    public partial class AttributeService 
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType" /> to search by.</param>
        /// <returns>
        /// A queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> that are related to the specified <see cref="Rock.Model.EntityType"/>.
        /// </returns>
        public IQueryable<Attribute> GetByEntityTypeId( int? entityTypeId )
        {
            var query = Repository.AsQueryable();

            if ( entityTypeId.HasValue )
            {
                query = query.Where( t => t.EntityTypeId == entityTypeId );
            }
            else
            {
                query = query.Where( t => !t.EntityTypeId.HasValue );
            }

            return query.OrderBy( t => t.Order ).ThenBy( t => t.Name );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.Category"/>.
        /// </summary>
        /// <param name="categoryId">A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> that are part of the specified <see cref="Rock.Model.Category"/></returns>
        public IQueryable<Attribute> GetByCategoryId( int categoryId )
        {
            return Repository.AsQueryable().Where( a => a.Categories.Any( c => c.Id == categoryId ) );
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.EntityType"/>, EntityQualifierColumn and EntityQualifierValue.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of a <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String" /> represents the name of the EntityQualifierColumn to search by.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String"/> that represents the qualifier value to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> that matches the specified value.</returns>
        public IQueryable<Attribute> Get( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue )
        {
            var query = Repository.AsQueryable();

            if ( entityTypeId.HasValue )
            {
                query = query.Where( t => t.EntityTypeId == entityTypeId );
            }
            else
            {
                query = query.Where( t => !t.EntityTypeId.HasValue );
            }

            return query.Where( t =>
                t.EntityTypeQualifierColumn == entityQualifierColumn &&
                t.EntityTypeQualifierValue == entityQualifierValue );
        }

        /// <summary>
        /// Returns an <see cref="Rock.Model.Attribute"/> by <see cref="Rock.Model.EntityType"/>, EntityQualifierColumn, EntityQualiferValue and Key name.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> that represents the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityQualifierColumn">A <see cref="System.String"/> that represents the name of the EntityQualifierColumn to search by.</param>
        /// <param name="entityQualifierValue">A <see cref="System.String"/> that represents the EntityQualifierValue to search by.</param>
        /// <param name="key">A <see cref="System.String"/> representing the key name of the attribute to search by.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Attribute"/> that matches the specified values. If a match is not found, a null value will be returned.
        /// </returns>
        public Attribute Get( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, string key )
        {
            var query = Get(entityTypeId, entityQualifierColumn, entityQualifierValue);
            return query.Where( t => t.Key == key ).FirstOrDefault();
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Attribute">Attributes</see> that uses the provided <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <param name="fieldTypeId">A <see cref="System.Int32"/> that represents the FileTypeId of the <see cref="Rock.Model.BinaryFileType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Attribute">Attributes</see> that uses the specified <see cref="Rock.Model.BinaryFileType"/>.</returns>
        public IEnumerable<Attribute> GetByFieldTypeId( int fieldTypeId )
        {
            return Repository.Find( t => t.FieldTypeId == fieldTypeId ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.
        /// </summary>
        /// <returns>A queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.</returns>
        public IQueryable<Attribute> GetGlobalAttributes()
        {
            return this.Get( null, string.Empty, string.Empty );
        }

        /// <summary>
        /// Returns a global <see cref="Rock.Model.Attribute"/> by it's Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Attribute key.</param>
        /// <returns>A global <see cref="Rock.Model.Attribute"/> by it's key.</returns>
        public Attribute GetGlobalAttribute( string key )
        {
            return this.Get( null, string.Empty, string.Empty, key );
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Save( Attribute item, int? personId )
        {
            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            var fieldTypeImage = Rock.Web.Cache.FieldTypeCache.Read( Rock.SystemGuid.FieldType.IMAGE.AsGuid() );
            var fieldTypeBinaryFile = Rock.Web.Cache.FieldTypeCache.Read( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() );

            if ( item.FieldTypeId == fieldTypeImage.Id || item.FieldTypeId == fieldTypeBinaryFile.Id )
            {
                int? binaryFileId = item.DefaultValue.AsInteger();
                if ( binaryFileId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
                    var binaryFile = binaryFileService.Get( binaryFileId.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                    }
                }
            }            
            
            return base.Save( item, personId );
        }
    }

    
}
