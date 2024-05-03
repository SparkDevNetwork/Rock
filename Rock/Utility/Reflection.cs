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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;

using Rock.Data;
using Rock.Utility.ExtensionMethods;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Static helper methods for using Reflection
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Gets the namespaces that start with the given root.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="rootNamespace">The root namespace.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetNamespacesThatStartWith( Assembly assembly, string rootNamespace )
        {
            return assembly.GetTypes()
                .Select( t => t.Namespace )
                .Where( ns => ns != null && ns.StartsWith( rootNamespace ) )
                .Distinct();
        }

        /// <summary>
        /// Finds the first matching type in Rock or any of the assemblies that reference Rock
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static Type FindType( Type baseType, string typeName )
        {
            return FindTypes( baseType, typeName ).Select( a => a.Value ).FirstOrDefault();
        }

        /// <summary>
        /// Finds the all the types that implement or inherit from the baseType. NOTE: It will only search the Rock.dll and also in assemblies that reference Rock.dll. The baseType
        /// will not be included in the result
        /// </summary>
        /// <param name="baseType">base type.</param>
        /// <param name="typeName">typeName can be specified to filter it to a specific type name</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType, string typeName = null )
        {
            SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();
            var assemblies = GetRockAndPluginAssemblies();

            foreach ( var assemblyEntry in assemblies )
            {
                var typeEntries = SearchAssembly( assemblyEntry, baseType );
                foreach ( KeyValuePair<string, Type> typeEntry in typeEntries )
                {
                    if ( string.IsNullOrWhiteSpace( typeName ) || typeEntry.Key == typeName )
                    {
                        types.TryAdd( typeEntry.Key, typeEntry.Value );
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// Searches the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <returns></returns>
        public static Dictionary<string, Type> SearchAssembly( Assembly assembly, Type baseType )
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            try
            {
                foreach ( Type type in assembly.GetTypesSafe() )
                {
                    if ( !type.IsAbstract )
                    {
                        if ( baseType.IsInterface )
                        {
                            foreach ( Type typeInterface in type.GetInterfaces() )
                            {
                                if ( typeInterface == baseType )
                                {
                                    types.Add( type.FullName, type );
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Type parentType = type.BaseType;
                            while ( parentType != null )
                            {
                                if ( parentType == baseType )
                                {
                                    types.Add( type.FullName, type );
                                    break;
                                }
                                else if ( baseType.IsGenericType && parentType.IsGenericType && parentType.GetGenericTypeDefinition() == baseType )
                                {
                                    types.Add( type.FullName, type );
                                    break;
                                }
                                parentType = parentType.BaseType;
                            }
                        }
                    }
                }
            }
            catch ( ReflectionTypeLoadException )
            {
                // Just continue on
            }

            return types;
        }

        /// <summary>
        /// Returns the DisplayName Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDisplayName( Type type )
        {
            return type.GetCustomAttribute<DisplayNameAttribute>( true )?.DisplayName;
        }

        /// <summary>
        /// Returns the Category Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCategory( Type type )
        {
            return type.GetCustomAttribute<CategoryAttribute>( true )?.Category;
        }

        /// <summary>
        /// Returns the Description Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDescription( Type type )
        {
            return type.GetCustomAttribute<DescriptionAttribute>( true )?.Description;
        }

        /// <summary>
        /// Gets the <see cref="DescriptionAttribute" /> value of first matching string const property with the specified value.
        /// For example:
        /// <code>
        /// GetDescriptionOfStringConstant( typeof(RelatedEntityPurposeKey), RelatedEntityPurposeKey.FinancialAccountGivingAlert)</code> would return "Giving Alerts"
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constantValue">The constant value.</param>
        /// <returns>System.String.</returns>
        public static string GetDescriptionOfStringConstant( Type type, string constantValue )
        {
            var fieldInfo = type.GetFields().Where( a => a.IsLiteral && ( string ) a.GetValue( null ) == constantValue ).FirstOrDefault();
            return fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// Gets the appropriate DbContext Type based on the entity type
        /// </summary>
        /// <param name="entityType">Type of the Entity.</param>
        /// <returns>A <see cref="Type"/> object.</returns>
        public static Type GetDbContextTypeForEntityType( Type entityType )
        {
            Type contextType = typeof( Rock.Data.RockContext );
            if ( entityType.Assembly != contextType.Assembly )
            {
                var contextTypeLookup = Reflection.SearchAssembly( entityType.Assembly, typeof( System.Data.Entity.DbContext ) );

                if ( contextTypeLookup.Any() )
                {
                    contextType = contextTypeLookup.First().Value;
                }
            }

            return contextType;
        }

        /// <summary>
        /// Gets the appropriate DbContext based on the entity type
        /// </summary>
        /// <param name="entityType">Type of the Entity.</param>
        /// <returns></returns>
        public static System.Data.Entity.DbContext GetDbContextForEntityType( Type entityType )
        {
            Type contextType = GetDbContextTypeForEntityType( entityType );

            if ( contextType == typeof( Rock.Data.RockContext ) )
            {
                return new Rock.Data.RockContext();
            }
            else
            {
                System.Data.Entity.DbContext dbContext = Activator.CreateInstance( contextType ) as System.Data.Entity.DbContext;
                return dbContext;
            }
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> that corresponds to the entity type and
        /// identifier key specified.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="key">The key that identifies the entity.</param>
        /// <returns>An instance of the entity or null if not found.</returns>
        public static Rock.Data.IEntity GetIEntityForEntityType( Type entityType, string key )
        {
            var dbContext = Reflection.GetDbContextForEntityType( entityType );
            Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( string ), typeof( bool ) } );
                return getMethod.Invoke( serviceInstance, new object[] { key, true } ) as Rock.Data.IEntity;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> that corresponds to the entity type and
        /// identifier key specified.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="key">The key that identifies the entity.</param>
        /// <param name="dbContext">The database context to use when accessing the database.</param>
        /// <returns>An instance of the entity or null if not found.</returns>
        public static Rock.Data.IEntity GetIEntityForEntityType( Type entityType, string key, Data.DbContext dbContext = null )
        {
            Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );

            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( string ), typeof( bool ) } );
                return getMethod.Invoke( serviceInstance, new object[] { key, true } ) as Rock.Data.IEntity;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> that corresponds to the entity type and
        /// identifier specified.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static Rock.Data.IEntity GetIEntityForEntityType( Type entityType, int id )
        {
            var dbContext = Reflection.GetDbContextForEntityType( entityType );
            Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                return getMethod.Invoke( serviceInstance, new object[] { id } ) as Rock.Data.IEntity;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> that corresponds to the entity type and
        /// unique identifier specified.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static Rock.Data.IEntity GetIEntityForEntityType( Type entityType, Guid guid )
        {
            var dbContext = Reflection.GetDbContextForEntityType( entityType );
            Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
                return getMethod.Invoke( serviceInstance, new object[] { guid } ) as Rock.Data.IEntity;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="IEntity"/> that corresponds to the entity type and
        /// public key specified.
        /// <para>
        /// Note that this key is NOT the hashed IEntity.IdKey, but rather the "Id>Guid" format used in some
        /// legacy areas of Rock, such as the identifiers encrypted within context cookies.
        /// </para>
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        internal static Rock.Data.IEntity GetIEntityForEntityTypeAndPublicKey( Type entityType, string publicKey )
        {
            var dbContext = Reflection.GetDbContextForEntityType( entityType );
            Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( entityType, dbContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "GetByPublicKey", new Type[] { typeof( string ) } );
                return getMethod.Invoke( serviceInstance, new object[] { publicKey } ) as Rock.Data.IEntity;
            }

            return null;
        }

        /// <summary>
        /// Gets the specified entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        public static IEntity GetIEntityForEntityType( int entityTypeId, Guid entityGuid, Data.DbContext dbContext = null )
        {
            var type = EntityTypeCache.Get( entityTypeId )?.GetEntityType();

            if ( type == null )
            {
                return null;
            }

            var serviceInstance = GetServiceForEntityType( type, dbContext ?? new RockContext() );
            var getMethod = serviceInstance?.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
            var entity = getMethod?.Invoke( serviceInstance, new object[] { entityGuid } ) as IEntity;
            return entity;
        }

        /// <summary>
        /// Gets the type of the entity identifier for entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        public static int? GetEntityIdForEntityType( int entityTypeId, Guid entityGuid, Data.DbContext dbContext = null )
        {
            var entityTypeGuid = EntityTypeCache.Get( entityTypeId ).Guid;

            return GetEntityIdForEntityType( entityTypeGuid, entityGuid, dbContext );
        }

        /// <summary>
        /// Gets the specified entity identifier given the entity type unique
        /// identifier and the entity unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier, this represents the model to use when mapping the <paramref name="entityGuid"/> to an identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>The integer identifier of the entity.</returns>
        public static int? GetEntityIdForEntityType( Guid entityTypeGuid, Guid entityGuid, Data.DbContext dbContext = null )
        {
            var type = EntityTypeCache.Get( entityTypeGuid )?.GetEntityType();

            if ( type == null )
            {
                return null;
            }

            int? entityId = null;

            /*
             * 1/11/2022 - DSH
             * 
             * This should be reworked at some point to provide mapping support
             * on models or ICachable so they can either specify the class that
             * handles the cache for it or a GetIdFromCache method.
             */

            /*
             * 1/14/2022 -DSH
             * 
             * This should also be updated to build a new Guid<=>Id cache map that
             * is fairly short lived (5-15 minutes). This cache would be used
             * to improve performance further for items that would not normally
             * be cached. It should then be tested if the ICachable logic is
             * noticably slower than this new method and whichever is faster should
             * be used as the primary source of truth.
             */

            // Check to see if we might have this item in cache. This is unholy
            // but it will catch probably 95% of the cases where we have a cache
            // available for a model.
            // Performance is good, 0.07ms for positive cache hit.
            if ( typeof( ICacheable ).IsAssignableFrom( type ) && type.Namespace == "Rock.Model" )
            {
                var cacheType = Type.GetType( $"Rock.Web.Cache.{type.Name}Cache" );

                // Make sure the base type inherits from ModelCache<,>
                if ( cacheType != null && cacheType.BaseType.IsGenericType && cacheType.BaseType.GetGenericTypeDefinition() == typeof( ModelCache<,> ) )
                {
                    // Make sure the base type is the expected type, e.g. ModelCache<CampusCache, Campus>
                    if ( cacheType.BaseType.GenericTypeArguments[1] == type )
                    {
                        var cacheGetIdMethod = cacheType.GetMethod( "GetId", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof( Guid ) }, null );

                        if ( cacheGetIdMethod != null )
                        {
                            entityId = ( int? ) cacheGetIdMethod.Invoke( null, new object[] { entityGuid } );
                        }
                    }
                }
            }

            // If we didn't find the entity id in cache, look it up in the database.
            if ( !entityId.HasValue )
            {
                var serviceInstance = GetServiceForEntityType( type, dbContext ?? new RockContext() );
                var getIdMethod = serviceInstance?.GetType().GetMethod( "GetId", new Type[] { typeof( Guid ) } );

                entityId = getIdMethod?.Invoke( serviceInstance, new object[] { entityGuid } ) as int?;
            }

            return entityId;
        }

        /// <summary>
        /// Gets the specified entity identifier given the entity type unique
        /// identifier and the entity unique identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityKey">The entity identifier key.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise <c>null</c> will be returned if an integer identifier is provided.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>The unique identifier of the entity.</returns>
        internal static Guid? GetEntityGuidForEntityType( int entityTypeId, string entityKey, bool allowIntegerIdentifier = true, Data.DbContext dbContext = null )
        {
            var entityTypeGuid = EntityTypeCache.Get( entityTypeId ).Guid;

            return GetEntityGuidForEntityType( entityTypeGuid, entityKey, allowIntegerIdentifier, dbContext );
        }

        /// <summary>
        /// Gets the specified entity identifier given the entity type unique
        /// identifier and the entity unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier, this represents the model to use when mapping the <paramref name="entityKey"/> to an unique identifier.</param>
        /// <param name="entityKey">The entity identifier key.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise <c>null</c> will be returned if an integer identifier is provided.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>The unique identifier of the entity.</returns>
        internal static Guid? GetEntityGuidForEntityType( Guid entityTypeGuid, string entityKey, bool allowIntegerIdentifier = true, Data.DbContext dbContext = null )
        {
            var type = EntityTypeCache.Get( entityTypeGuid )?.GetEntityType();

            if ( type == null )
            {
                return null;
            }

            // Check if the key is already in the Guid format.
            Guid? entityGuid = entityKey.AsGuidOrNull();

            if ( entityGuid.HasValue )
            {
                return entityGuid.Value;
            }

            // Get the integer identifier from the key or the hasher.
            var entityId = allowIntegerIdentifier ? entityKey.AsIntegerOrNull() : null;

            if ( !entityId.HasValue )
            {
                entityId = Rock.Utility.IdHasher.Instance.GetId( entityKey );
            }

            // If no integer identifier found, we can't proceed.
            if ( !entityId.HasValue )
            {
                return null;
            }

            /*
             * 1/11/2022 - DSH
             * 
             * This should be reworked at some point to provide mapping support
             * on models or ICachable so they can either specify the class that
             * handles the cache for it or a GetIdFromCache method.
             */

            /*
             * 1/14/2022 -DSH
             * 
             * This should also be updated to build a new Guid<=>Id cache map that
             * is fairly short lived (5-15 minutes). This cache would be used
             * to improve performance further for items that would not normally
             * be cached. It should then be tested if the ICachable logic is
             * noticably slower than this new method and whichever is faster should
             * be used as the primary source of truth.
             */

            // Check to see if we might have this item in cache. This is unholy
            // but it will catch probably 95% of the cases where we have a cache
            // available for a model.
            // Performance is good, 0.07ms for positive cache hit.
            if ( typeof( ICacheable ).IsAssignableFrom( type ) && type.Namespace == "Rock.Model" )
            {
                var cacheType = Type.GetType( $"Rock.Web.Cache.{type.Name}Cache" );

                // Make sure the base type inherits from ModelCache<,>
                if ( cacheType != null && cacheType.BaseType.IsGenericType && cacheType.BaseType.GetGenericTypeDefinition() == typeof( ModelCache<,> ) )
                {
                    // Make sure the base type is the expected type, e.g. ModelCache<CampusCache, Campus>
                    if ( cacheType.BaseType.GenericTypeArguments[1] == type )
                    {
                        var cacheGetIdMethod = cacheType.GetMethod( "GetGuid", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof( int ) }, null );

                        if ( cacheGetIdMethod != null )
                        {
                            entityGuid = ( Guid? ) cacheGetIdMethod.Invoke( null, new object[] { entityId.Value } );
                        }
                    }
                }
            }

            // If we didn't find the entity id in cache, look it up in the database.
            if ( !entityGuid.HasValue )
            {
                var serviceInstance = GetServiceForEntityType( type, dbContext ?? new RockContext() );
                var getIdMethod = serviceInstance?.GetType().GetMethod( "GetGuid", new Type[] { typeof( int ) } );

                entityGuid = getIdMethod?.Invoke( serviceInstance, new object[] { entityId } ) as Guid?;
            }

            return entityGuid;
        }

        /// <summary>
        /// Gets the entity ids for a entity type from a list of entity guids, idkeys or ids as strings.
        /// </summary>
        /// <param name="entityType">The entity type cache, this represents the model to use when mapping the <paramref name="entityKeys"/> to a identifiers.</param>
        /// <param name="entityKeys">The entity identifier keys to be converted to integer identifiers.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise the integer identifier will not be returned.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>A dictionary whose key is the original entityKey and value is the integer identifier. If an identifier was not found then its key will not be present in this dictionary.</returns>
        internal static Dictionary<string, int> GetEntityIdsForEntityType( EntityTypeCache entityType, List<string> entityKeys, bool allowIntegerIdentifier = true, Data.DbContext dbContext = null )
        {
            if ( entityType == null )
            {
                throw new ArgumentNullException( nameof( entityType ) );
            }

            if ( entityKeys == null )
            {
                throw new ArgumentNullException( nameof( entityKeys ) );
            }

            if ( entityKeys.Count == 0 )
            {
                return new Dictionary<string, int>();
            }

            var guidsToLookup = new List<Guid>();
            var entityIds = new Dictionary<string, int>();

            // Go over each entity key and check if it is already an integer or
            // a hashed integer. Otherwise add it to a list of guids to lookup.
            foreach ( var entityKey in entityKeys )
            {
                var entityId = allowIntegerIdentifier ? entityKey.AsIntegerOrNull() : null;

                if ( !entityId.HasValue )
                {
                    entityId = Rock.Utility.IdHasher.Instance.GetId( entityKey );
                }

                if ( entityId.HasValue )
                {
                    entityIds.TryAdd( entityKey, entityId.Value );
                }
                else if ( Guid.TryParse( entityKey, out var guid ) )
                {
                    guidsToLookup.Add( guid );
                }
            }

            // If we have any guids to look up in the database, then do so in bulk.
            if ( guidsToLookup.Any() )
            {
                var disposeOfContext = false;

                if ( dbContext == null )
                {
                    dbContext = new RockContext();
                    disposeOfContext = true;
                }

                // Get a queryable for the IEntity type.
                var entityQry = GetQueryableForEntityType( entityType.GetEntityType(), dbContext );

                while ( guidsToLookup.Any() )
                {
                    // Load at most 1,000 entities at a time since it performs better than loading all of them at once.
                    var guidsToProcess = guidsToLookup.Take( 1_000 ).ToList();
                    guidsToLookup = guidsToLookup.Skip( 1_000 ).ToList();

                    // Load all the entities from the GUIDs.
                    var ids = entityQry
                        .AsNoTracking()
                        .Where( e => guidsToProcess.Contains( e.Guid ) )
                        .Select( e => new
                        {
                            e.Id,
                            e.Guid
                        } )
                        .ToList();

                    foreach ( var id in ids )
                    {
                        var key = entityKeys.FirstOrDefault( k => k.ToLower() == id.Guid.ToString().ToLower() );

                        if ( key != null )
                        {
                            entityIds.TryAdd( key, id.Id );
                        }
                    }
                }

                if ( disposeOfContext )
                {
                    dbContext.Dispose();
                }
            }

            return entityIds;
        }

        /// <summary>
        /// Gets the queryable for the entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>An <see cref="IQueryable{IEntity}"/> that can be used to load entities; or <c>null</c> if the operation was not supported.</returns>
        internal static IQueryable<IEntity> GetQueryableForEntityType( Type entityType, Data.DbContext dbContext )
        {
            // Dynamically get the IService for the entity type and then get a queryable to load the entities.
            var entityService = Rock.Reflection.GetServiceForEntityType( entityType, dbContext );
            var asQueryableMethod = entityService?.GetType().GetMethod( "Queryable", Array.Empty<Type>() );

            // If the entity service is null, then the entity type is not a valid IEntity type.
            if ( asQueryableMethod == null )
            {
                return null;
            }

            // Get a queryable for the IEntity type.
            return ( IQueryable<IEntity> ) asQueryableMethod.Invoke( entityService, Array.Empty<object>() );
        }

        // Cache the ServiceType that we found when doing reflection. Doing reflection each time could take a few milliseconds,
        // but once we cache it, it'll only a few microseconds to get the type
        private static ConcurrentDictionary<Type, Type> serviceTypesByEntityTypeLookup = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Gets the specified entity identifier given the entity type unique
        /// identifier and the entity key.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityKey">The entity identifier key.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise <c>null</c> will be returned if an integer identifier is provided.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>The identifier of the entity.</returns>
        internal static int? GetEntityIdForEntityType( int entityTypeId, string entityKey, bool allowIntegerIdentifier = true, Data.DbContext dbContext = null )
        {
            var entityTypeGuid = EntityTypeCache.Get( entityTypeId ).Guid;

            return GetEntityIdForEntityType( entityTypeGuid, entityKey, allowIntegerIdentifier, dbContext );
        }

        /// <summary>
        /// Gets the specified entity identifier given the entity type unique
        /// identifier and the entity unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier, this represents the model to use when mapping the <paramref name="entityKey"/> to an unique identifier.</param>
        /// <param name="entityKey">The entity identifier key.</param>
        /// <param name="allowIntegerIdentifier">if set to <c>true</c> integer identifiers will be allowed; otherwise <c>null</c> will be returned if an integer identifier is provided.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>The unique identifier of the entity.</returns>
        internal static int? GetEntityIdForEntityType( Guid entityTypeGuid, string entityKey, bool allowIntegerIdentifier = true, Data.DbContext dbContext = null )
        {
            var type = EntityTypeCache.Get( entityTypeGuid )?.GetEntityType();

            if ( type == null )
            {
                return null;
            }

            // Get the integer identifier from the key or the hasher.
            var entityId = allowIntegerIdentifier ? entityKey.AsIntegerOrNull() : null;

            if ( !entityId.HasValue )
            {
                entityId = Rock.Utility.IdHasher.Instance.GetId( entityKey );
            }

            if ( entityId.HasValue )
            {
                return entityId.Value;
            }

            // Check if the key is already in the Guid format.
            Guid? entityGuid = entityKey.AsGuidOrNull();

            // If no unique identifier found, we can't proceed.
            if ( !entityGuid.HasValue )
            {
                return null;
            }

            /*
             * 1/11/2022 - DSH
             * 
             * This should be reworked at some point to provide mapping support
             * on models or ICachable so they can either specify the class that
             * handles the cache for it or a GetIdFromCache method.
             */

            /*
             * 1/14/2022 -DSH
             * 
             * This should also be updated to build a new Guid<=>Id cache map that
             * is fairly short lived (5-15 minutes). This cache would be used
             * to improve performance further for items that would not normally
             * be cached. It should then be tested if the ICachable logic is
             * noticably slower than this new method and whichever is faster should
             * be used as the primary source of truth.
             */

            // Check to see if we might have this item in cache. This is unholy
            // but it will catch probably 95% of the cases where we have a cache
            // available for a model.
            // Performance is good, 0.07ms for positive cache hit.
            if ( typeof( ICacheable ).IsAssignableFrom( type ) && type.Namespace == "Rock.Model" )
            {
                var cacheType = Type.GetType( $"Rock.Web.Cache.{type.Name}Cache" );

                // Make sure the base type inherits from ModelCache<,>
                if ( cacheType != null && cacheType.BaseType.IsGenericType && cacheType.BaseType.GetGenericTypeDefinition() == typeof( ModelCache<,> ) )
                {
                    // Make sure the base type is the expected type, e.g. ModelCache<CampusCache, Campus>
                    if ( cacheType.BaseType.GenericTypeArguments[1] == type )
                    {
                        var cacheGetIdMethod = cacheType.GetMethod( "GetId", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof( Guid ) }, null );

                        if ( cacheGetIdMethod != null )
                        {
                            entityId = ( int? ) cacheGetIdMethod.Invoke( null, new object[] { entityGuid.Value } );
                        }
                    }
                }
            }

            // If we didn't find the entity id in cache, look it up in the database.
            if ( !entityId.HasValue )
            {
                var serviceInstance = GetServiceForEntityType( type, dbContext ?? new RockContext() );
                var getIdMethod = serviceInstance?.GetType().GetMethod( "GetId", new Type[] { typeof( Guid ) } );

                entityId = getIdMethod?.Invoke( serviceInstance, new object[] { entityGuid.Value } ) as int?;
            }

            return entityId;
        }

        /// <summary>
        /// Gets the appropriate Rock.Data.IService based on the entity type
        /// </summary>
        /// <param name="entityType">Type of the Entity.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        public static Rock.Data.IService GetServiceForEntityType( Type entityType, System.Data.Entity.DbContext dbContext )
        {
            Type serviceTypeForEntityType;

            serviceTypesByEntityTypeLookup.TryGetValue( entityType, out serviceTypeForEntityType );
            if ( serviceTypeForEntityType == null )
            {
                /* 05/25/2022 MDP

                  To find the appropriate Service class for the entity type, we'll
                  use reflection to find the class that was code-generated (or manually created) for the EntityType.
                  For example, PersonService is the derived class for Service<Person> and GroupService is the derived class for Group

                  Just in case we can't find it a derived class, we'll just do the Service<T>.
                  For example, if the entityType is plugin.Model.PotLuck, there might not be a derived service for that.
                  So, we'll use Service<plugin.Model.PotLuck> in that case

                */

                // Find service that has this entity type's base class of Service<T>
                serviceTypeForEntityType = Reflection.FindTypes( typeof( IService ) ).Values.Where( a => a.GetGenericArgumentsOfBaseType( typeof( Service<> ) ).Any( g => g == entityType ) ).FirstOrDefault();
                if ( serviceTypeForEntityType == null)
                {
                    // No derived class, so use Service<T>
                    // This could happen if this is a plugin IEntity, for example: plugin.Model.PotLuck
                    Type serviceType = typeof( Rock.Data.Service<> );
                    serviceTypeForEntityType = serviceType.MakeGenericType( new Type[] { entityType } );
                }

                if ( serviceTypeForEntityType != null )
                {
                    serviceTypesByEntityTypeLookup.TryAdd( entityType, serviceTypeForEntityType );
                }
            }

            Rock.Data.IService serviceInstance = Activator.CreateInstance( serviceTypeForEntityType, dbContext ) as Rock.Data.IService;
            return serviceInstance;
        }

        /// <summary>
        /// Determines whether the specified property of an IEntity is mapped to a real database field
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>
        ///   <c>true</c> if [is mapped database property] [the specified property information]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMappedDatabaseProperty( PropertyInfo propertyInfo )
        {
            // if marked as NotMapped, it isn't a database property
            var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;

            if ( notMapped )
            {
                return false;
            }

            // if the property is marked virtual (unless it is 'virtual final'), don't include it since it isn't a real database field
            var getter = propertyInfo.GetGetMethod();
            var isVirtual = getter?.IsVirtual == true;
            if ( isVirtual )
            {
                // NOTE: Properties that implement interface members (for example Rock.Data.IOrder) will also be marked as 'virtual final' by the compiler, so check IsFinal to determine if it was the compiler that did it.
                // See https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isfinal?redirectedfrom=MSDN&view=netframework-4.7.2#System_Reflection_MethodBase_IsFinal
                bool isVirtualDueToInterface = getter?.IsFinal == true;
                if ( !isVirtualDueToInterface )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The Plugin assemblies
        /// </summary>
        private static List<Assembly> _pluginAssemblies = null;


        /// <summary>
        /// The RockWeb app_code assembly
        /// </summary>
        private static Assembly _appCodeAssembly = null;

        /// <summary>
        /// Sets the RockWeb.App_Code assembly so that the Reflection methods can search for types in it
        /// </summary>
        /// <param name="appCodeAssembly">The application code assembly.</param>
        public static void SetAppCodeAssembly( Assembly appCodeAssembly )
        {
            _appCodeAssembly = appCodeAssembly;
            if ( _pluginAssemblies != null && _appCodeAssembly != null )
            {
                _pluginAssemblies.Add( _appCodeAssembly );
            }
        }

        /// <summary>
        /// Gets a list of Assemblies, including Rock and all those in the ~/Bin and ~/Plugins folders as well as the RockWeb.App_Code assembly that are
        /// assemblies that might have plugins
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetRockAndPluginAssemblies()
        {
            var assemblies = GetPluginAssemblies();
            var executingAssembly = Assembly.GetExecutingAssembly();
            assemblies.Add( executingAssembly );

            return assemblies;
        }

        /// <summary>
        /// Gets a list of Assemblies in the ~/Bin and ~/Plugins folders as well as the RockWeb.App_Code assembly that are assemblies that might have plugins
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetPluginAssemblies()
        {
            if ( _pluginAssemblies != null )
            {
                return _pluginAssemblies.ToList();
            }

            // Add executing assembly's directory
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            string binDirectory = Path.GetDirectoryName( path );

            // Add all the assemblies in the 'Plugins' subdirectory
            string pluginsFolder = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Plugins" );

            // blacklist of files that would never have Rock MEF components or Rock types
            string[] ignoredFileStart = { "Lucene.", "Microsoft.", "msvcr100.", "System.", "JavaScriptEngineSwitcher.", "React.", "CacheManager." };

            // get all *.dll in the bin and plugin directories except for blacklisted ones
            var assemblyFileNames = Directory.EnumerateFiles( binDirectory, "*.dll", SearchOption.AllDirectories ).ToList();

            if ( Directory.Exists( pluginsFolder ) )
            {
                assemblyFileNames.AddRange( Directory.EnumerateFiles( pluginsFolder, "*.dll", SearchOption.AllDirectories ) );
            }

            assemblyFileNames = assemblyFileNames.Where( a => !a.EndsWith( ".resources.dll", StringComparison.OrdinalIgnoreCase )
                                        && !ignoredFileStart.Any( i => Path.GetFileName( a ).StartsWith( i, StringComparison.OrdinalIgnoreCase ) ) ).ToList();

            // get a lookup of already loaded assemblies so that we don't have to load it unnecessarily
            var loadedAssembliesDictionary = AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && !a.GlobalAssemblyCache && !string.IsNullOrWhiteSpace( a.Location ) )
                .DistinctBy( k => new Uri( k.CodeBase ).LocalPath )
                .ToDictionary( k => new Uri( k.CodeBase ).LocalPath, v => v, StringComparer.OrdinalIgnoreCase );

            List<Assembly> pluginAssemblies = new List<Assembly>();
            if ( _appCodeAssembly != null )
            {
                pluginAssemblies.Add( _appCodeAssembly );
            }

            foreach ( var assemblyFileName in assemblyFileNames )
            {
                Assembly assembly = loadedAssembliesDictionary.GetValueOrNull( assemblyFileName );
                if ( assembly == null )
                {
                    try
                    {
                        // if an assembly is found that isn't loaded yet, load it into the CurrentDomain
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName( assemblyFileName );
                        assembly = AppDomain.CurrentDomain.Load( assemblyName );
                    }
                    catch ( BadImageFormatException )
                    {
                        // BadImageFormatException means the dll isn't a managed dll (not a .NET dll), so we can safely ignore
                    }
                    catch ( Exception ex )
                    {
                        Rock.Model.ExceptionLogService.LogException( new Exception( $"Unable to load assembly from {assemblyFileName}", ex ) );
                    }
                }

                if ( assembly != null )
                {
                    bool isRockAssembly = false;

                    // only search inside dlls that are Rock.dll or reference Rock.dll
                    if ( assemblyFileName.Equals( "Rock.dll", StringComparison.OrdinalIgnoreCase ) )
                    {
                        isRockAssembly = true;
                    }
                    else
                    {
                        List<AssemblyName> referencedAssemblies = assembly.GetReferencedAssemblies().ToList();

                        if ( referencedAssemblies.Any( a => a.Name.Equals( "Rock", StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            isRockAssembly = true;
                        }
                    }

                    if ( isRockAssembly )
                    {
                        pluginAssemblies.Add( assembly );
                    }
                }
            }

            _pluginAssemblies = pluginAssemblies;

            return _pluginAssemblies.ToList();
        }


        /// <summary>
        /// Gets the name of the type in a "friendly" manner.
        /// Nested types are returned as "A.B.C."
        /// Generic types are returned as "A&lt;B,C&gt;"
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetFriendlyName( Type type )
        {
            if ( type == null )
            {
                return string.Empty;
            }

            var rawTypeName = type.Name;

            if ( type.IsGenericType && type.IsNested )
            {
                var nameWithoutParamCount = rawTypeName.Substring( 0, rawTypeName.IndexOf( "`" ) );
                var nestedName = string.Format( "{0}.{1}", GetFriendlyName( type.DeclaringType ), nameWithoutParamCount );

                return string.Format(
                    "{0}<{1}>",
                    nestedName,
                    type.GetGenericArguments().Select( GetFriendlyName ).JoinStrings( ", " ) );
            }

            if ( type.IsGenericType )
            {
                return string.Format(
                    "{0}<{1}>",
                    rawTypeName.Substring( 0, rawTypeName.IndexOf( "`" ) ),
                    type.GetGenericArguments().Select( GetFriendlyName ).JoinStrings( ", " ) );
            }

            if ( type.IsNested )
            {
                return string.Format( "{0}.{1}", GetFriendlyName( type.DeclaringType ), type.Name );
            }

            return rawTypeName;
        }

        /// <summary>
        /// Gets the types with attribute.
        /// https://stackoverflow.com/a/720171/13215483
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>( bool inherit )
            where TAttribute : System.Attribute
        {
            var assemblies = GetRockAndPluginAssemblies();

            return
                from a in assemblies
                from t in a.GetTypes()
                where t.IsDefined( typeof( TAttribute ), inherit )
                select t;
        }
    }
}