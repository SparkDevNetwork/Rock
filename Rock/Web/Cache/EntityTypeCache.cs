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
using System.Collections.Generic;
using System.Linq;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a entityType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheEntityType instead" )]
    public class EntityTypeCache : CachedEntity<EntityType>
    {
        #region Constructors

        private EntityTypeCache()
        {
        }

        private EntityTypeCache( CacheEntityType cacheEntityType )
        {
            CopyFromNewCache( cacheEntityType );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is entity; otherwise, <c>false</c>.
        /// </value>
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is secured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecured { get; set; }

        /// <summary>
        /// Gets or sets the single value field type identifier.
        /// </summary>
        /// <value>
        /// The single value field type identifier.
        /// </value>
        private int? SingleValueFieldTypeId { get; set; }

        /// <summary>
        /// Gets the type of the single value field.
        /// </summary>
        /// <value>
        /// The type of the single value field.
        /// </value>
        public FieldTypeCache SingleValueFieldType
        {
            get
            {
                if ( SingleValueFieldTypeId.HasValue )
                {
                    return FieldTypeCache.Read( SingleValueFieldTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the multi value field type identifier.
        /// </summary>
        /// <value>
        /// The multi value field type identifier.
        /// </value>
        private int? MultiValueFieldTypeId { get; set; }

        /// <summary>
        /// Gets the type of the multi value field.
        /// </summary>
        /// <value>
        /// The type of the multi value field.
        /// </value>
        public FieldTypeCache MultiValueFieldType
        {
            get
            {
                if ( MultiValueFieldTypeId.HasValue )
                {
                    return FieldTypeCache.Read( MultiValueFieldTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexing enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexing enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexing supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexing supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexingSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic supported; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        public bool IsAnalyticSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic historical supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic historical supported; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        public bool IsAnalyticHistoricalSupported { get; set; }

        /// <summary>
        /// Determines whether [is analytics supported] [the specified entity type qualifier column].
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns>
        ///   <c>true</c> if [is analytics supported] [the specified entity type qualifier column]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAnalyticsSupported( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            var type = GetEntityType();
            if ( type == null ) return false;

            return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                .Any( x =>
                    ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) ) ||
                    ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                );
        }

        /// <summary>
        /// Determines whether [is analytics historical supported] [the specified entity type qualifier column].
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns>
        ///   <c>true</c> if [is analytics historical supported] [the specified entity type qualifier column]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAnalyticsHistoricalSupported( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            var type = GetEntityType();
            if ( type == null ) return false;

            return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                    .Where( a => a.SupportsHistory )
                    .Any( x =>
                        ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) ) ||
                        ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                    );
        }

        /// <summary>
        /// Gets a value indicating whether this instance support dynamically added Attributes for the Analytic Tables for the specified Attribute
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns>
        ///   <c>true</c> if [is analytic attributes supported] [the specified attribute]; otherwise, <c>false</c>.
        /// </returns>
        /// <value></value>
        public bool IsAnalyticAttributesSupported( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            var type = GetEntityType();
            if ( type == null ) return false;

            return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                    .Where( a => a.SupportsAttributes )
                    .Any( x =>
                        ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) ) ||
                        ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                    );
        }

        /// <summary>
        /// Gets or sets the index result template.
        /// </summary>
        /// <value>
        /// The index result template.
        /// </value>
        public string IndexResultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the index document URL.
        /// </summary>
        /// <value>
        /// The index document URL.
        /// </value>
        public string IndexDocumentUrl { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating a link to view details for this entity (i.e. "~/person/{{ Entity.Id }}").
        /// </summary>
        /// <value>
        /// The link URL.
        /// </value>
        public string LinkUrlLavaTemplate { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <returns></returns>
        public Type GetEntityType()
        {
            return !string.IsNullOrWhiteSpace( AssemblyName ) ? Type.GetType( AssemblyName ) : null;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );
            if ( !( model is EntityType ) ) return;

            var entityType = (EntityType)model;
            Name = entityType.Name;
            AssemblyName = entityType.AssemblyName;
            FriendlyName = entityType.FriendlyName;
            IsEntity = entityType.IsEntity;
            IsSecured = entityType.IsSecured;
            SingleValueFieldTypeId = entityType.SingleValueFieldTypeId;
            MultiValueFieldTypeId = entityType.MultiValueFieldTypeId;
            IsIndexingEnabled = entityType.IsIndexingEnabled;
            IsIndexingSupported = entityType.IsIndexingSupported;
            IndexResultTemplate = entityType.IndexResultTemplate;
            IndexDocumentUrl = entityType.IndexDocumentUrl;
            LinkUrlLavaTemplate = entityType.LinkUrlLavaTemplate;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );
            if ( !( cacheEntity is CacheEntityType ) ) return;

            var entityType = (CacheEntityType)cacheEntity;
            Name = entityType.Name;
            AssemblyName = entityType.AssemblyName;
            FriendlyName = entityType.FriendlyName;
            IsEntity = entityType.IsEntity;
            IsSecured = entityType.IsSecured;
            SingleValueFieldTypeId = entityType.SingleValueFieldTypeId;
            MultiValueFieldTypeId = entityType.MultiValueFieldTypeId;
            IsIndexingEnabled = entityType.IsIndexingEnabled;
            IsIndexingSupported = entityType.IsIndexingSupported;
            IndexResultTemplate = entityType.IndexResultTemplate;
            IndexDocumentUrl = entityType.IndexDocumentUrl;
            LinkUrlLavaTemplate = entityType.LinkUrlLavaTemplate;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static int? GetId( Type type )
        {
            return CacheEntityType.GetId( type );
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int? GetId<T>()
        {
            return CacheEntityType.GetId<T>();
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( string name )
        {
            return CacheEntityType.GetId( name );
        }

        /// <summary>
        /// Reads the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Cache.CacheEntityType.Get instead" )]
        public static EntityTypeCache Read( Type type, bool createIfNotFound = true, RockContext rockContext = null )
        {
            return new EntityTypeCache( CacheEntityType.Get( type, createIfNotFound, rockContext ) );
        }

        /// <summary>
        /// Reads the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read<T>( bool createIfNotFound = true, RockContext rockContext = null )
        {
            return new EntityTypeCache( CacheEntityType.Get<T>( createIfNotFound, rockContext ) );
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityBlockType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Cache.CacheEntityType.Get instead" )]
        public static EntityTypeCache Read( string name )
        {
            return new EntityTypeCache( CacheEntityType.Get( name ) );
        }

        /// <summary>
        /// Reads the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createNew">if set to <c>true</c> [create new].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( string name, bool createNew, RockContext rockContext = null )
        {
            return new EntityTypeCache( CacheEntityType.Get( name, createNew, rockContext ) );
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityBlockType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( int id, RockContext rockContext = null )
        {
            return new EntityTypeCache( CacheEntityType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new EntityTypeCache( CacheEntityType.Get( guid ) );
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="entityTypeModel">The field type model.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( EntityType entityTypeModel )
        {
            return new EntityTypeCache( CacheEntityType.Get( entityTypeModel ) );
        }

        /// <summary>
        /// Alls this instance.
        /// </summary>
        /// <returns></returns>
        public static List<EntityTypeCache> All()
        {
            var entityTypes = new List<EntityTypeCache>();

            var cacheEntityTypes = CacheEntityType.All();
            if ( cacheEntityTypes == null ) return entityTypes;

            foreach ( var cacheEntityType in cacheEntityTypes )
            {
                entityTypes.Add( new EntityTypeCache( cacheEntityType ) );
            }

            return entityTypes;
        }

        /// <summary>
        /// Removes entityType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheEntityType.Remove( id );
        }

        #endregion
    }
}