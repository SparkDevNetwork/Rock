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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Rock.Cms.ContentCollection;
using Rock.Cms.ContentCollection.Attributes;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Settings;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a entityType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityTypeCache : EntityCache<EntityTypeCache, EntityType>
    {
        #region Static Fields

        private static ConcurrentDictionary<string, int> EntityTypes
        {
            get
            {
                return ( ConcurrentDictionary<string, int> ) RockCache.GetOrAddExisting( "EntityTypeCache_LookupByName", () =>
                    new ConcurrentDictionary<string, int>( StringComparer.OrdinalIgnoreCase ) );
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The cached type this EntityTypeCache refers to.
        /// </summary>
        private Type _entityType = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        [DataMember]
        public string AssemblyName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the friendly.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        [DataMember]
        public string FriendlyName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is entity; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsEntity { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is secured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSecured { get; private set; }

        /// <summary>
        /// Gets a flag indicating whether this entity type is a commonly used entity.
        /// If so, it will grouped at the top by the entity type picker control
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommon { get; private set; }

        /// <summary>
        /// Gets or sets the single value field type identifier.
        /// </summary>
        /// <value>
        /// The single value field type identifier.
        /// </value>
        [DataMember]
        internal int? SingleValueFieldTypeId { get; private set; }

        /// <summary>
        /// The properties
        /// </summary>
        [IgnoreDataMember]
        private Dictionary<string, PropertyInfo> _properties = null;

        /// <summary>
        /// Gets a dictionary of the names of all the properties for this type of entity, along with <see cref="PropertyInfo"/> about the property
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [IgnoreDataMember]
        public Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                if ( _properties == null )
                {
                    _properties = this.GetEntityType().GetProperties().ToDictionary( k => k.Name, v => v );
                }

                return _properties;
            }
        }

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
                    return FieldTypeCache.Get( SingleValueFieldTypeId.Value );
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
        [DataMember]
        internal int? MultiValueFieldTypeId { get; private set; }

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
                    return FieldTypeCache.Get( MultiValueFieldTypeId.Value );
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
        [DataMember]
        public bool IsIndexingEnabled { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity type supports content
        /// collection indexing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this entity type supports content collection indexing; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        internal bool IsContentCollectionIndexingEnabled { get; private set; }

        /// <summary>
        /// Gets the type of the indexer used when storing in the content collection index.
        /// </summary>
        /// <value>
        /// The type of the indexer used when storing in the content collection index.
        /// </value>
        internal Type ContentCollectionIndexerType { get; private set; }

        /// <summary>
        /// Gets the type of the document used when storing in the content collection index.
        /// </summary>
        /// <value>
        /// he type of the document used when storing in the content collection index.
        /// </value>
        internal Type ContentCollectionDocumentType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has achievements enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is achievements enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAchievementsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is message bus event publish enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is message bus event publish enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMessageBusEventPublishEnabled { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether attributes of this entity type support a Pre-HTML and Post-HTML option.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attributes support pre post HTML]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AttributesSupportPrePostHtml { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexing supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexing supported; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexingSupported { get; private set; }

        /// <summary>
        /// Gets the name of the get index model.
        /// </summary>
        /// <value>
        /// The name of the get index model.
        /// </value>
        public Type IndexModelType { get; private set; }

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
            var type = GetEntityType();

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
            var type = GetEntityType();

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
        [DataMember]
        public string IndexResultTemplate { get; private set; }

        /// <summary>
        /// Gets or sets the index document URL.
        /// </summary>
        /// <value>
        /// The index document URL.
        /// </value>
        [DataMember]
        public string IndexDocumentUrl { get; private set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating a link to view details for this entity (i.e. "~/person/{{ Entity.Id }}").
        /// </summary>
        /// <value>
        /// The link URL.
        /// </value>
        [DataMember]
        public string LinkUrlLavaTemplate { get; private set; }

        #endregion

        #region Cache Related Methods

        private static Dictionary<string, Type> _cachedEntityTypeByEntityType = null;
        private System.Reflection.MethodInfo _cachedItemGetMethod = null;
        private System.Reflection.MethodInfo _cachedItemFlushItemMethod = null;
        private System.Reflection.MethodInfo _cachedItemClearMethod = null;

        /// <summary>
        /// Determines whether there is an IEntityCache associated with this EntityType
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has entity cache]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEntityCache()
        {
            return GetEntityCacheType() != null;
        }

        /// <summary>
        /// Gets the Rock Cached Item
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        internal IEntityCache GetCachedItem( int entityId )
        {
            _cachedItemGetMethod = _cachedItemGetMethod ?? GetEntityCacheType()?.GetMethod( "Get", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, new Type[] { typeof( Int32 ) }, null );

            return _cachedItemGetMethod?.Invoke( null, new object[] { entityId } ) as IEntityCache;
        }

        /// <summary>
        /// Flushes the cached item.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        internal void FlushCachedItem( int entityId )
        {
            _cachedItemFlushItemMethod = _cachedItemFlushItemMethod ?? GetEntityCacheType()?.GetMethod( "FlushItem", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, new Type[] { typeof( Int32 ) }, null );

            _cachedItemFlushItemMethod?.Invoke( null, new object[] { entityId } );
        }

        /// <summary>
        /// Clears all the cached items for this EntityType
        /// </summary>
        internal void ClearCachedItems()
        {
            _cachedItemClearMethod = _cachedItemClearMethod ?? GetEntityCacheType()?.GetMethod( "Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy, null, new Type[] {}, null );

            _cachedItemClearMethod?.Invoke( null, new object[] {} );
        }

        #endregion Cache Related Methods

        #region Methods

        /// <summary>
        /// Gets the type of the entity cache associated with this EntityType (if applicable)
        /// </summary>
        /// <returns></returns>
        public Type GetEntityCacheType()
        {
            if ( _cachedEntityTypeByEntityType == null )
            {
                _cachedEntityTypeByEntityType = Reflection.FindTypes( typeof( IEntityCache ) ).Values
                    .Select( a => new
                    {
                        CacheTypeType = a.BaseType.GenericTypeArguments[0],
                        EntityTypeType = a.BaseType.GenericTypeArguments[1]
                    } )
                    .ToDictionary( k => k.EntityTypeType.FullName, v => v.CacheTypeType );
            }

            var entityCacheType = _cachedEntityTypeByEntityType?.GetValueOrNull( this.Name );

            return entityCacheType;
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <returns></returns>
        public Type GetEntityType()
        {
            /*
             * 2022-06-24 - dsh
             * 
             * Constructing a type from the AssemblyName is fast, but still
             * takes 0.0024ms. It can also be called extremely often. On the
             * stock Rock instance this is called 86 times for the person
             * profile extended attributes page. Caching it can save 0.2ms
             * per page load on some pages.
             */
            if ( _entityType == null )
            {
                _entityType = !string.IsNullOrWhiteSpace( AssemblyName ) ? Type.GetType( AssemblyName ) : null;
            }

            return _entityType;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var entityType = entity as EntityType;
            if ( entityType == null ) return;

            Name = entityType.Name;
            AssemblyName = entityType.AssemblyName;
            FriendlyName = entityType.FriendlyName;
            IsEntity = entityType.IsEntity;
            IsSecured = entityType.IsSecured;
            IsCommon = entityType.IsCommon;
            SingleValueFieldTypeId = entityType.SingleValueFieldTypeId;
            MultiValueFieldTypeId = entityType.MultiValueFieldTypeId;
            IsIndexingEnabled = entityType.IsIndexingEnabled;
            IsAchievementsEnabled = entityType.IsAchievementsEnabled;
            IsMessageBusEventPublishEnabled = entityType.IsMessageBusEventPublishEnabled;
            AttributesSupportPrePostHtml = entityType.AttributesSupportPrePostHtml;
            IsIndexingSupported = entityType.IsIndexingSupported;
            IndexResultTemplate = entityType.IndexResultTemplate;
            IndexDocumentUrl = entityType.IndexDocumentUrl;
            LinkUrlLavaTemplate = entityType.LinkUrlLavaTemplate;

            IndexModelType = entityType.IndexModelType;

            // Detect support for content collection.
            var contentIndexableAttribute = GetEntityType()?.GetCustomAttribute<ContentCollectionIndexableAttribute>();
            if ( contentIndexableAttribute != null )
            {
                ContentCollectionIndexerType = contentIndexableAttribute.IndexerType;
                ContentCollectionDocumentType = contentIndexableAttribute.DocumentType;

                if ( !typeof( IContentCollectionIndexer ).IsAssignableFrom( ContentCollectionIndexerType ) )
                {
                    ContentCollectionIndexerType = null;
                }

                if ( !typeof( Rock.Cms.ContentCollection.IndexDocuments.IndexDocumentBase ).IsAssignableFrom( ContentCollectionDocumentType ) )
                {
                    ContentCollectionDocumentType = null;
                }

                IsContentCollectionIndexingEnabled = ContentCollectionIndexerType != null && ContentCollectionDocumentType != null;
            }

            EntityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
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
            return Get( type ).Id;
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
        /// Gets an EntityTypeId based on the specified type name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int? GetId( string name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return null;
            }

            return Get( name ).Id;
        }

        /// <summary>
        /// Gets an EntityType cache object based on the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Get<T>( bool createIfNotFound = true, RockContext rockContext = null )
        {
            return Get( typeof( T ), createIfNotFound, rockContext );
        }

        /// <summary>
        /// Gets an EntityType cache object based on the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Get( Type type, bool createIfNotFound = true, RockContext rockContext = null )
        {
            if ( type == null ) return null;

            if ( type.IsDynamicProxyType() && type.BaseType != null )
            {
                type = type.BaseType;
            }

            int entityTypeId;
            if ( EntityTypes.TryGetValue( type.FullName, out entityTypeId ) )
            {
                return Get( entityTypeId );
            }

            EntityTypeCache entityType = null;

            if ( rockContext != null )
            {
                var entityTypeModel = new EntityTypeService( rockContext ).Get( type, createIfNotFound, null );
                if ( entityTypeModel != null )
                {
                    entityType = Get( entityTypeModel );
                }
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    var entityTypeModel = new EntityTypeService( myRockContext ).Get( type, createIfNotFound, null );
                    if ( entityTypeModel != null )
                    {
                        entityType = Get( entityTypeModel );
                    }
                }
            }

            if ( entityType != null )
            {
                EntityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
            }

            return entityType;
        }

        /// <summary>
        /// Gets an EntityType cache object based on the specified type name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public new static EntityTypeCache Get( string name )
        {
            return Get( name, true );
        }

        /// <summary>
        /// Gets an EntityType cache object based on the specified type name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createNew">if set to <c>true</c> [create new].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityTypeCache Get( string name, bool createNew, RockContext rockContext = null )
        {
            // First try to find by guid
            var entityType = EntityCache<EntityTypeCache, EntityType>.Get( name );
            if ( entityType != null )
            {
                return entityType;
            }

            int entityTypeId;
            if ( EntityTypes.TryGetValue( name, out entityTypeId ) )
            {
                return Get( entityTypeId );
            }

            if ( rockContext != null )
            {
                var entityTypeModel = new EntityTypeService( rockContext ).GetByName( name, createNew );
                if ( entityTypeModel != null )
                {
                    entityType = Get( entityTypeModel );
                }
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    var entityTypeModel = new EntityTypeService( myRockContext ).GetByName( name, createNew );
                    if ( entityTypeModel != null )
                    {
                        entityType = Get( entityTypeModel );
                    }
                }
            }

            if ( entityType != null )
            {
                EntityTypes.AddOrUpdate( entityType.Name, entityType.Id, ( k, v ) => entityType.Id );
            }

            return entityType;
        }

        #endregion

    }
}