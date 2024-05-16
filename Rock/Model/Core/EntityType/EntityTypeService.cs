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
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// EntityType POCO Service class
    /// </summary>
    public partial class EntityTypeService
    {
        /// <summary>
        /// Gets an <see cref="Rock.Model.EntityType"/> by its name / type name.
        /// </summary>
        /// <param name="entityName">A <see cref="System.String"/> representing the name of the EntityType to search for.</param>
        /// <returns>The first <see cref="Rock.Model.EntityType"/> with a name that matches the provided value.</returns>
        public EntityType Get( string entityName )
        {
            return Queryable().FirstOrDefault( t => t.Name == entityName );
        }

        /// <summary>
        /// Gets the specified <see cref="Rock.Model.EntityType"/> by the object type. If a match is not found, it can optionally create a new <see cref="Rock.Model.EntityType"/> for the object.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> to search for.</param>
        /// <param name="createIfNotFound">A <see cref="System.Boolean"/> value that indicates if a new <see cref="Rock.Model.EntityType"/> should be created if a match is not found. This value
        /// will be <c>true</c> if a new <see cref="Rock.Model.EntityType"/> should be created if there is not a match; otherwise <c>false</c>/</param>
        /// <param name="personAlias">A <see cref="Rock.Model.PersonAlias"/> representing the alias of the <see cref="Rock.Model.Person"/> who is searching for and possibly creating a new EntityType.  This value can be
        /// null if the logged in person is not known (i.e. an anonymous user).</param>
        /// <returns>A <see cref="Rock.Model.EntityType"/> matching the provided type. If a match is not found and createIfNotFound is false this value will be null.</returns>
        public EntityType Get( Type type, bool createIfNotFound, PersonAlias personAlias )
        {
            var entityType = Get( type.FullName );
            if ( entityType != null )
            {
                return entityType;
            }

            if ( createIfNotFound )
            {
                // Create a new context so type can be saved independing of current context
                using ( var rockContext = new RockContext() )
                {
                    var entityTypeService = new EntityTypeService( rockContext );
                    entityType = CreateFromType( type );
                    entityTypeService.Add( entityType );
                    try
                    {
                        rockContext.SaveChanges();
                    }
                    catch ( Exception thrownException )
                    {
                        // if the exception was due to a duplicate Guid, throw a duplicateGuidException. That'll make it easier to troubleshoot.
                        var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, type.FullName );
                        if ( duplicateGuidException != null )
                        {
                            throw duplicateGuidException;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Read type using current context
                return this.Get( entityType.Id );
            }

            return null;
        }

        private static EntityType CreateFromType( Type type )
        {
            EntityType entityType = new EntityType();
            entityType.Name = type.FullName;
            entityType.FriendlyName = type.Name.SplitCase();
            entityType.AssemblyName = type.AssemblyQualifiedName;

            if ( typeof( IEntity ).IsAssignableFrom( type ) )
            {
                entityType.IsEntity = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() == null;
            }
            else
            {
                entityType.IsEntity = false;
            }

            entityType.IsSecured = typeof( Rock.Security.ISecured ).IsAssignableFrom( type );

            var entityTypeGuidAttributeValue = type.GetCustomAttribute<Rock.SystemGuid.EntityTypeGuidAttribute>( inherit: false )?.Guid;
            if ( entityTypeGuidAttributeValue.HasValue )
            {
                entityType.Guid = entityTypeGuidAttributeValue.Value;
            }

            return entityType;
        }

        /// <summary>
        /// Gets an <see cref="Rock.Model.EntityType" /> by its name. If a match is not found, a new <see cref="Rock.Model.EntityType" /> can optionally be created.
        /// </summary>
        /// <param name="name">A <see cref="System.String" /> representing the name of the object/entity type to search for.</param>
        /// <param name="createIfNotFound">A <see cref="System.Boolean" /> value that indicates if a new <see cref="Rock.Model.EntityType" /> should be created if a match is not found. This value
        /// will be <c>true</c> if a new <see cref="Rock.Model.EntityType" /> should be created if there is not a match; otherwise <c>false</c>/</param>
        /// <returns></returns>
        public EntityType GetByName( string name, bool createIfNotFound )
        {
            var entityType = Get( name );
            if ( entityType != null )
            {
                return entityType;
            }

            if ( createIfNotFound )
            {
                // Create a new context so type can be saved independing of current context
                var rockContext = new RockContext();
                var entityTypeService = new EntityTypeService( rockContext );
                entityType = new EntityType();
                entityType.Name = name;
                entityTypeService.Add( entityType );
                rockContext.SaveChanges();

                // Read type using current context
                return this.Get( entityType.Id );
            }

            return null;
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> where the IsEntity flag is set to true.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Rock.Model.EntityType"/> where the IsEntity flag is set tot true.</returns>
        public IEnumerable<EntityType> GetEntities()
        {
            return Queryable().Where( e => e.IsEntity );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> where the IsEntity flag is set to true, person is Authorized to View, and EntityType isn't HideFromReporting
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public IEnumerable<EntityType> GetReportableEntities( Person currentPerson )
        {
            return this.GetEntities()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( s => new
                {
                    EntityTypeCache = EntityTypeCache.Get( s ),
                    Entity = s,
                } )
                .Where( a => a.EntityTypeCache != null && a.EntityTypeCache.GetEntityType() != null && !a.EntityTypeCache.GetEntityType().GetCustomAttributes( typeof( HideFromReportingAttribute ), true ).Any() )
                .Select( s => s.Entity );
        }

        /// <summary>
        /// Returns the <see cref="Rock.Model.EntityType">EntityTypes</see> as a grouped collection of <see cref="System.Web.UI.WebControls.ListItem">ListItems</see> with the 
        /// "Common" flag set to true.
        /// </summary>
        /// <returns>A list of <see cref="Rock.Model.EntityType"/> <see cref="System.Web.UI.WebControls.ListItem">ListItems</see> ordered by their "Common" flag and FriendlyName</returns>
        public List<System.Web.UI.WebControls.ListItem> GetEntityListItems()
        {
            var items = new List<System.Web.UI.WebControls.ListItem>();

            var entities = GetEntities().OrderBy( e => e.FriendlyName ).ToList();

            foreach ( var entity in entities.Where( t => t.IsCommon ) )
            {
                var li = new System.Web.UI.WebControls.ListItem( entity.FriendlyName, entity.Id.ToString() );
                li.Attributes.Add( "optiongroup", "Common" );
                items.Add( li );
            }

            foreach ( var entity in entities )
            {
                var li = new System.Web.UI.WebControls.ListItem( entity.FriendlyName, entity.Id.ToString() );
                li.Attributes.Add( "optiongroup", "All Entities" );
                items.Add( li );
            }

            return items;
        }

        /// <summary>
        /// Gets an Entity by type and entity Id.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public IEntity GetEntity( int entityTypeId, int entityId )
        {
            var entityQry = GetQueryable( entityTypeId );
            if ( entityQry != null )
            {
                return entityQry.Where( i => i.Id == entityId ).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets an Entity by type and entity Id, without loading the entity into EF ChangeTracking
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public IEntity GetEntityNoTracking( int entityTypeId, int entityId )
        {
            var entityQry = GetQueryable( entityTypeId );
            if ( entityQry != null )
            {
                return entityQry.Where( i => i.Id == entityId ).AsNoTracking().FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets an Entity by type and entity Guid.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns></returns>
        public IEntity GetEntity( int entityTypeId, Guid entityGuid )
        {
            var entityQry = GetQueryable( entityTypeId );
            if ( entityQry != null )
            {
                return entityQry.Where( i => i.Guid == entityGuid ).FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets a Service Queryable method for a particular entity type
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        private IQueryable<IEntity> GetQueryable( int entityTypeId )
        {
            EntityTypeCache itemEntityType = EntityTypeCache.Get( entityTypeId );
            if ( itemEntityType != null )
            {
                Type entityType = itemEntityType.GetEntityType();
                if ( entityType != null )
                {
                    Type[] modelType = { entityType };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { ( RockContext ) this.Context } ) as IService;

                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    return qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;
                }
            }

            return null;
        }

        /// <summary>
        /// Uses Reflection to find all IEntity entities (all models), ISecured Types (could be a model or a component), and IRockBlockTypes.
        /// Then ensures that the <seealso cref="Rock.Model.EntityType" /> table is synced up to match.
        /// </summary>
        [Obsolete( "Use the RegisterEntityTypes() that doesn't have any parameters (physWebAppPath is never used)" )]
        [RockObsolete( "1.11" )]
        public static void RegisterEntityTypes( string physWebAppPath )
        {
            RegisterEntityTypes();
        }

        /// <summary>
        /// Uses Reflection to find all IEntity entities (all models), ISecured Types (could be a model or a component), and IRockBlockTypes.
        /// Then ensures that the <seealso cref="Rock.Model.EntityType" /> table is synced up to match.
        /// </summary>
        public static void RegisterEntityTypes()
        {
            List<Type> reflectedTypes = new List<Type>();

            // we'll auto-register anything that implements IEntity, ISecured or IRockBlockType
            reflectedTypes.AddRange( Rock.Reflection.FindTypes( typeof( Rock.Data.IEntity ) ).Values );
            reflectedTypes.AddRange( Rock.Reflection.FindTypes( typeof( Rock.Security.ISecured ) ).Values );
            reflectedTypes.AddRange( Rock.Reflection.FindTypes( typeof( Rock.Blocks.IRockBlockType ) ).Values );

            // do a distinct since some of the types implement both IEntity and ISecured
            reflectedTypes = reflectedTypes.Distinct().OrderBy( a => a.FullName ).ToList();

            var reflectedTypeLookupByName = reflectedTypes.ToDictionary( k => k.FullName );

            Dictionary<string, EntityType> entityTypesFromReflection = new Dictionary<string, EntityType>( StringComparer.OrdinalIgnoreCase );
            foreach ( var reflectedType in reflectedTypes )
            {
                var entityType = CreateFromType( reflectedType );
                entityTypesFromReflection.TryAdd( reflectedType.FullName, entityType );
            };

            using ( var rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );

                var reflectedTypeNames = reflectedTypes.Select( a => a.FullName ).ToArray();
                var reflectedTypeGuids = entityTypesFromReflection.Values.Select( a => a.Guid ).ToArray();

                var duplicateGuids = reflectedTypeGuids
                    .GroupBy( g => g )
                    .Where( g => g.Count() > 1 )
                    .Select( g => g.Key.ToString() )
                    .ToList();

                // Throw an error if duplicate guids were detected in the code.
                // This will intentionally prevent Rock from starting so the
                // developer can quickly find and fix the error.
                if ( duplicateGuids.Any() )
                {
                    throw new Exception( $"Duplicate EntityType system guids detected: {duplicateGuids.JoinStrings( ", " )}" );
                }

                // Get all the EntityType records from the Database without filtering them (we'll have to deal with them all)
                // Then we'll split them into a list of ones that don't exist and ones that still exist
                var entityTypeInDatabaseList = entityTypeService.Queryable().ToList();

                // Find any existing self-discovered EntityType records that no longer exist in reflectedTypes
                // but have C# narrow it down to ones that aren't in the reflectedTypeNames list
                var reflectedEntityTypesThatNoLongerExist = entityTypeInDatabaseList
                    .Where( e => !string.IsNullOrEmpty( e.AssemblyName ) )
                    .ToList()
                    .Where( e => !reflectedTypeNames.Contains( e.Name ) && !reflectedTypeGuids.Contains( e.Guid ) )
                    .OrderBy( a => a.Name )
                    .ToList();

                // clean up entitytypes that don't have an Assembly, but somehow have IsEntity or IsSecured set
                foreach ( var entityTypesWithoutAssembliesButIsEntity in entityTypeInDatabaseList.Where( e => string.IsNullOrEmpty( e.AssemblyName ) && ( e.IsEntity || e.IsSecured ) ) )
                {
                    if ( entityTypesWithoutAssembliesButIsEntity.AssemblyName.IsNullOrWhiteSpace() )
                    {
                        entityTypesWithoutAssembliesButIsEntity.IsEntity = false;
                        entityTypesWithoutAssembliesButIsEntity.IsSecured = false;
                    }
                }

                foreach ( var oldEntityType in reflectedEntityTypesThatNoLongerExist )
                {

                    Type foundType = null;
                    // if this isn't one of the EntityTypes that we self-register,
                    // see if it was manually registered first (with EntityTypeCache.Get(Type type, bool createIfNotExists))
                    try
                    {
                        foundType = Type.GetType( oldEntityType.AssemblyName, false );
                    }
                    catch
                    {
                        /* 2020-05-22 MDP
                         * GetType (string typeName, bool throwOnError) can throw exceptions even if throwOnError is false!
                         * see https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype?view=netframework-4.5.2#System_Type_GetType_System_String_System_Boolean_

                          so, if this happens, we'll ignore any error it returns in those cases too
                         */
                    }

                    if ( foundType == null )
                    {
                        // it was manually registered but we can't create a Type from it
                        // so we'll update the EntityType.AssemblyName to null
                        // and set IsSecured and IsEntity to False (since a NULL type doesn't implement ISecured or IEntity)
                        oldEntityType.AssemblyName = null;
                        oldEntityType.IsSecured = false;
                        oldEntityType.IsEntity = false;
                    }
                }

                // Now get the entityType records that are still in the list of types we found thru reflection
                // but we'll have C# narrow it down to ones that aren't in the reflectedTypeNames list
                var reflectedEntityTypesThatStillExist = entityTypeInDatabaseList
                    .Where( e => reflectedTypeNames.Contains( e.Name ) || reflectedTypeGuids.Contains( e.Guid ) )
                    .ToList();

                // Update any existing entities
                foreach ( var existingEntityType in reflectedEntityTypesThatStillExist )
                {
                    var entityTypeFromReflection = entityTypesFromReflection.GetValueOrNull( existingEntityType.Name );
                    if ( entityTypeFromReflection == null )
                    {
                        // Check if the entity type had its class name change by
                        // seeing if we already have one with the same guid.
                        entityTypeFromReflection = entityTypesFromReflection.Values.FirstOrDefault( e => e.Guid == existingEntityType.Guid );

                        if ( entityTypeFromReflection == null )
                        {
                            continue;
                        }
                    }

                    if ( existingEntityType.Name != entityTypeFromReflection.Name ||
                        existingEntityType.IsEntity != entityTypeFromReflection.IsEntity ||
                        existingEntityType.IsSecured != entityTypeFromReflection.IsSecured ||
                        existingEntityType.FriendlyName != ( existingEntityType.FriendlyName ?? entityTypeFromReflection.FriendlyName ) ||
                        existingEntityType.AssemblyName != entityTypeFromReflection.AssemblyName )
                    {
                        existingEntityType.Name = entityTypeFromReflection.Name;
                        existingEntityType.IsEntity = entityTypeFromReflection.IsEntity;
                        existingEntityType.IsSecured = entityTypeFromReflection.IsSecured;
                        existingEntityType.FriendlyName = existingEntityType.FriendlyName ?? entityTypeFromReflection.FriendlyName;
                        existingEntityType.AssemblyName = entityTypeFromReflection.AssemblyName;
                    }

                    var reflectedType = reflectedTypeLookupByName.GetValueOrNull( existingEntityType.Name );
                    var reflectedTypeGuid = reflectedType?.GetCustomAttribute<Rock.SystemGuid.EntityTypeGuidAttribute>( inherit: false )?.Guid;
                    if ( reflectedTypeGuid != null && reflectedTypeGuid.Value != existingEntityType.Guid )
                    {
                        /*
                         * 2022-07-11 ETD
                         * Since the GUID has changed we need to check for AttributeValues that use the old GUID and update it to the new one.
                         * Do this on the same context since the two changes should occur at the same time.
                         * Limit to attributes that are chosen by EntityType, Component, and Components FieldTypes since those are what store the GUIDs as attribute values.
                         * The filtered attributes list should be less than 300, even on very large DBs.
                         */

                        if ( existingEntityType.Guid != null )
                        {
                            var attributeService = new AttributeService( rockContext );
                            var attributeValueService = new AttributeValueService( rockContext );
                            var entityTypeFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.ENTITYTYPE ).Id;
                            var componentFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.COMPONENT ).Id;
                            var componentsFieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.COMPONENTS ).Id;
                            var existingEntityTypeString = existingEntityType.Guid.ToString().ToLower();
                            var reflectedTypeGuidString = reflectedTypeGuid.Value.ToString().ToLower();

                            var attributeIdsUsingFieldType = attributeService.Queryable()
                                .Where( a => a.FieldTypeId == entityTypeFieldTypeId
                                    || a.FieldTypeId == componentFieldTypeId
                                    || a.FieldTypeId == componentsFieldTypeId )
                                .Select( a => a.Id )
                                .ToList();

                            rockContext.Database.CommandTimeout = 150;

                            foreach ( var attributeIdUsingFieldType in attributeIdsUsingFieldType )
                            {
                                var attributeValues = attributeValueService.Queryable().Where( av => av.AttributeId == attributeIdUsingFieldType && av.Value.Contains( existingEntityTypeString ) ).ToList();

                                foreach ( var attributeValue in attributeValues )
                                {
                                    attributeValue.Value = attributeValue.Value.ToLower().Replace( existingEntityTypeString, reflectedTypeGuidString );
                                }
                            }
                        }

                        // Now update the Guid of the EntityType
                        existingEntityType.Guid = reflectedTypeGuid.Value;
                    }

                    entityTypesFromReflection.Remove( existingEntityType.Name );
                }

                // Add the newly discovered entities 
                foreach ( var entityType in entityTypesFromReflection.Values )
                {
                    // Don't add the EntityType entity as it will probably have been automatically 
                    // added by the audit on a previous save in this method.
                    if ( entityType.Name != "Rock.Model.EntityType" )
                    {
                        // double check that another thread didn't add this EntityType.
                        if ( !entityTypeService.AlreadyExists( entityType.Name ) )
                        {
                            entityTypeService.Add( entityType );
                        }
                    }
                }

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception thrownException )
                {
                    // if the exception was due to a duplicate Guid, throw as a duplicateGuidException. That'll make it easier to troubleshoot.
                    var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, null );
                    if ( duplicateGuidException != null )
                    {
                        throw duplicateGuidException;
                    }
                    else
                    {
                        throw;
                    }
                }

                // make sure the EntityTypeCache is synced up with any changes that were made
                foreach ( var entityTypeModel in entityTypeService.Queryable().AsNoTracking() )
                {
                    EntityTypeCache.Get( entityTypeModel );
                }
            }
        }

        /// <summary>
        /// Gets the Guid for the EntityType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = EntityTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }

        /// <summary>
        /// Returns true if an EntityType with the same Name already exists in the database.
        /// This can be used this to help prevent duplicates.
        /// </summary>
        /// <param name="name">The name.</param>
        private bool AlreadyExists( string name )
        {
            return this.Queryable().Where( a => a.Name == name ).Any();
        }

    }
}
