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
    /// Data access/service for <see cref="Rock.Model.AttributeValue"/> entity objects.
    /// </summary>
    public partial class AttributeValueService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <param name="attributeId">A <see cref="System.Int32" /> that represents the AttributeId of the <see cref="Rock.Model.Attribute"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by the specified <see cref="Rock.Model.Attribute"/>.</returns>
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
        /// Returns an enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.
        /// </summary>
        /// <param name="entityId">A <see cref="System.Int32"/> representing the EntityId to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.AttributeValue">AttributeValues</see> by EntityId.</returns>
        public IEnumerable<AttributeValue> GetByEntityId( int? entityId )
        {
            return Repository.Find( t => ( t.EntityId == entityId || ( entityId == null && t.EntityId == null ) ) );
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.AttributeValue"/> for a <see cref="Rock.Model.Attribute"/> by Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Global <see cref="Rock.Model.Attribute">Attribute's</see> key value.</param>
        /// <returns>The <see cref="Rock.Model.AttributeValue" /> of the global <see cref="Rock.Model.Attribute"/>.</returns>
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

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Save( AttributeValue item, int? personId )
        {
            if ( item.Attribute != null )
            {
                // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
                var fieldTypeImage = Rock.Web.Cache.FieldTypeCache.Read( Rock.SystemGuid.FieldType.IMAGE.AsGuid() );
                var fieldTypeBinaryFile = Rock.Web.Cache.FieldTypeCache.Read( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() );

                if ( item.Attribute.FieldTypeId == fieldTypeImage.Id || item.Attribute.FieldTypeId == fieldTypeBinaryFile.Id )
                {
                    int? binaryFileId = item.Value.AsInteger();
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
            }

            return base.Save( item, personId );
        }
    }
}
