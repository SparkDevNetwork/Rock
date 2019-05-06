// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

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
            return GetByEntityTypeId( entityTypeId, true );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetByEntityTypeId( int? entityTypeId, bool includeInactive )
        {
            var query = Queryable();

            if ( entityTypeId.HasValue )
            {
                query = query.Where( t => t.EntityTypeId == entityTypeId );
            }
            else
            {
                query = query.Where( t => !t.EntityTypeId.HasValue );
            }

            if ( !includeInactive )
            {
                query = query.Where( a => a.IsActive == true );
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
            return GetByCategoryId( categoryId, true );
        }

        /// <summary>
        /// Gets the by category identifier.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetByCategoryId( int categoryId, bool includeInactive )
        {
            var queryable = Queryable().Where( a => a.Categories.Any( c => c.Id == categoryId ) );
            if ( !includeInactive )
            {
                queryable = queryable.Where( a => a.IsActive == true );
            }

            return queryable;
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
            return GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, true );
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Attribute">Attributes</see> by <see cref="Rock.Model.EntityType"/>, EntityQualifierColumn and EntityQualifierValue.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetByEntityTypeQualifier( int? entityTypeId, string entityQualifierColumn, string entityQualifierValue, bool includeInactive )
        {
            var query = Queryable();

            if ( entityTypeId.HasValue )
            {
                query = query.Where( t => t.EntityTypeId == entityTypeId );
            }
            else
            {
                query = query.Where( t => !t.EntityTypeId.HasValue );
            }

            if ( string.IsNullOrWhiteSpace(entityQualifierColumn ) )
            {
                query = query.Where( t => t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == "" );
            }
            else
            {
                query = query.Where( t => t.EntityTypeQualifierColumn == entityQualifierColumn );
            }

            if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
            {
                query = query.Where( t => t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == "" );
            }
            else
            {
                query = query.Where( t => t.EntityTypeQualifierValue == entityQualifierValue );
            }

            if ( !includeInactive )
            {
                query = query.Where( a => a.IsActive == true );
            }

            return query;

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
            var query = GetByEntityTypeQualifier( entityTypeId, entityQualifierColumn, entityQualifierValue, true);
            return query.Where( t => t.Key == key ).FirstOrDefault();
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Attribute">Attributes</see> that uses the provided <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <param name="fieldTypeId">A <see cref="System.Int32"/> that represents the FileTypeId of the <see cref="Rock.Model.BinaryFileType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Attribute">Attributes</see> that uses the specified <see cref="Rock.Model.BinaryFileType"/>.</returns>
        public IQueryable<Attribute> GetByFieldTypeId( int fieldTypeId )
        {
            return Queryable().Where( t => t.FieldTypeId == fieldTypeId ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.
        /// </summary>
        /// <returns>A queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.</returns>
        public IQueryable<Attribute> GetGlobalAttributes()
        {
            var query = Queryable( "Categories,AttributeQualifiers" );
            query = query.Where( t => !t.EntityTypeId.HasValue);

            return query
                .Where( t =>
                    ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == string.Empty ) && 
                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == string.Empty ) );
        }

        /// <summary>
        /// Gets the group member attributes combined with the inherited group type's group member attributes.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetGroupMemberAttributesCombined( int groupId, int groupTypeId )
        {
            return GetGroupMemberAttributesCombined( groupId, groupTypeId, true );
        }

        /// <summary>
        /// Gets the group member attributes combined with the inherited group type's group member attributes.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public IQueryable<Attribute> GetGroupMemberAttributesCombined( int groupId, int groupTypeId, bool includeInactive )
        {
            var queryInherited = GetByEntityTypeQualifier( new GroupMember().TypeId, "GroupTypeId", groupTypeId.ToString(), includeInactive );
            queryInherited.OrderBy( a => a.Order )
                .ThenBy( a => a.Name );

            var query = GetByEntityTypeQualifier( new GroupMember().TypeId, "GroupId", groupId.ToString(), includeInactive );
            query.OrderBy( a => a.Order )
                .ThenBy( a => a.Name );

            return queryInherited.Concat( query );
        }

        /// <summary>
        /// Returns a global <see cref="Rock.Model.Attribute"/> by its Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Attribute key.</param>
        /// <returns>A global <see cref="Rock.Model.Attribute"/> by its key.</returns>
        public Attribute GetGlobalAttribute( string key )
        {
            return this.Get( null, string.Empty, string.Empty, key );
        }


        /// <summary>
        /// Returns a queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.
        /// </summary>
        /// <returns>A queryable collection containing the Global <see cref="Rock.Model.Attribute">Attributes</see>.</returns>
        public IQueryable<Attribute> GetSystemSettings()
        {
            return this.GetByEntityTypeQualifier( null, Attribute.SYSTEM_SETTING_QUALIFIER, string.Empty, true );
        }

        /// <summary>
        /// Returns a global <see cref="Rock.Model.Attribute"/> by its Key.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the name of the Attribute key.</param>
        /// <returns>A global <see cref="Rock.Model.Attribute"/> by its key.</returns>
        public Attribute GetSystemSetting( string key )
        {
            return this.Get( null, Attribute.SYSTEM_SETTING_QUALIFIER, string.Empty, key );
        }

        /// <summary>
        /// Gets the system setting value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetSystemSettingValue(string key)
        {
            return this.GetByEntityTypeQualifier( null, Attribute.SYSTEM_SETTING_QUALIFIER, string.Empty, true ).Where( t => t.Key == key ).Select( a => a.DefaultValue ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the Guid for the Attribute that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.AttributeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }
    }
}
