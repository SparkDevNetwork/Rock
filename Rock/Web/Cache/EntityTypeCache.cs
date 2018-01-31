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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a entityType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class EntityTypeCache : CachedEntity<EntityType>
    {
        #region Static Fields

        private static ConcurrentDictionary<string, int> _entityTypes = new ConcurrentDictionary<string, int>();

        #endregion

        #region Constructors

        private EntityTypeCache()
        {
        }

        private EntityTypeCache( EntityType model )
        {
            CopyFromModel( model );
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
            Type type = this.GetEntityType();

            if ( type != null )
            {
                return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                        .Any( x =>
                            ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) )
                                || ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                            );
            }

            return false;
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
            Type type = this.GetEntityType();

            if ( type != null )
            {
                return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                        .Where( a => a.SupportsHistory )
                        .Any( x =>
                            ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) )
                                || ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                            );
            }

            return false;
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
            Type type = this.GetEntityType();

            if ( type != null )
            {
                return type.GetCustomAttributes( true ).OfType<AnalyticsAttribute>()
                        .Where( a => a.SupportsAttributes )
                        .Any( x =>
                            ( string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( entityTypeQualifierColumn ) )
                                || ( x.EntityTypeQualifierColumn == entityTypeQualifierColumn && x.EntityTypeQualifierValue == entityTypeQualifierValue )
                            );
            }

            return false;
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
            if ( !string.IsNullOrWhiteSpace( this.AssemblyName ) )
            {
                return Type.GetType( this.AssemblyName );
            }

            return null;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is EntityType )
            {
                var entityType = (EntityType)model;
                this.Name = entityType.Name;
                this.AssemblyName = entityType.AssemblyName;
                this.FriendlyName = entityType.FriendlyName;
                this.IsEntity = entityType.IsEntity;
                this.IsSecured = entityType.IsSecured;
                this.SingleValueFieldTypeId = entityType.SingleValueFieldTypeId;
                this.MultiValueFieldTypeId = entityType.MultiValueFieldTypeId;
                this.IsIndexingEnabled = entityType.IsIndexingEnabled;
                this.IsIndexingSupported = entityType.IsIndexingSupported;
                this.IndexResultTemplate = entityType.IndexResultTemplate;
                this.IndexDocumentUrl = entityType.IndexDocumentUrl;
                this.LinkUrlLavaTemplate = entityType.LinkUrlLavaTemplate;

                _entityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:EntityType:{0}", id );
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static int? GetId( Type type )
        {
            return Read( type ).Id;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int? GetId<T>()
        {
            return GetId( typeof( T ) );
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return null;
            }

            return Read( name ).Id;
        }

        /// <summary>
        /// Reads the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Type type, bool createIfNotFound = true, RockContext rockContext = null )
        {
            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            int entityTypeId = 0;
            if ( _entityTypes.TryGetValue( type.FullName, out entityTypeId ) )
            {
                return Read( entityTypeId );
            }

            EntityTypeCache entityType = null;

            if ( rockContext != null )
            {
                var entityTypeModel = new EntityTypeService( rockContext ).Get( type, createIfNotFound, null );
                if ( entityTypeModel != null )
                {
                    entityType = Read( entityTypeModel );
                }
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    var entityTypeModel = new EntityTypeService( myRockContext ).Get( type, createIfNotFound, null );
                    if ( entityTypeModel != null )
                    {
                        entityType = Read( entityTypeModel );
                    }
                }
            }

            if ( entityType != null )
            {
                _entityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
            }

            return entityType;
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
            return EntityTypeCache.Read( typeof( T ), createIfNotFound, rockContext );
        }

        /// <summary>
        /// Returns EntityType object from cache.  If entityBlockType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( string name )
        {
            return Read( name, true );
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
            int entityTypeId = 0;
            if ( _entityTypes.TryGetValue( name, out entityTypeId ) )
            {
                return Read( entityTypeId );
            }

            EntityTypeCache entityType = null;

            if ( rockContext != null )
            {
                var entityTypeModel = new EntityTypeService( rockContext ).Get( name, createNew );
                if ( entityTypeModel != null )
                {
                    entityType = Read( entityTypeModel );
                }
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    var entityTypeModel = new EntityTypeService( myRockContext ).Get( name, createNew );
                    if ( entityTypeModel != null )
                    {
                        entityType =  Read( entityTypeModel );
                    }
                }
            }

            if ( entityType != null )
            {
                _entityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
            }

            return entityType;
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
            return GetOrAddExisting( EntityTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static EntityTypeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static EntityTypeCache LoadById2( int id, RockContext rockContext )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            var entityTypeModel = entityTypeService.Get( id );
            if ( entityTypeModel != null )
            {
                return new EntityTypeCache( entityTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            return entityTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="entityTypeModel">The field type model.</param>
        /// <returns></returns>
        public static EntityTypeCache Read( EntityType entityTypeModel )
        {
            return GetOrAddExisting( EntityTypeCache.CacheKey( entityTypeModel.Id ),
                () => LoadByModel( entityTypeModel ) );
        }

        private static EntityTypeCache LoadByModel( EntityType entityTypeModel )
        {
            if ( entityTypeModel != null )
            {
                return new EntityTypeCache( entityTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Alls this instance.
        /// </summary>
        /// <returns></returns>
        public static List<EntityTypeCache> All()
        {
            List<EntityTypeCache> entityTypes = new List<EntityTypeCache>();
            var entityTypeIds = GetOrAddExisting( "Rock:EntityType:All", () => LoadAll() );
            if ( entityTypeIds != null )
            {
                foreach ( int entityTypeId in entityTypeIds )
                {
                    var entityTypeCache = EntityTypeCache.Read( entityTypeId );
                    entityTypes.Add( entityTypeCache );
                }
            }
            return entityTypes;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new EntityTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes entityType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( EntityTypeCache.CacheKey( id ) );
            if ( _entityTypes == null )
            {
                // shouldn't happen, but just in case
                _entityTypes = new ConcurrentDictionary<string, int>();
            }

            // rebuild the _entityTypes dictionary 
            var _keepEntityTypes = _entityTypes.Where( a => a.Value != id );
            _entityTypes = new ConcurrentDictionary<string, int>( _keepEntityTypes );

            FlushCache( "Rock:EntityType:All" );
        }

        #endregion
    }
}