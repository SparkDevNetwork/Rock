// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;

using Rock;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// EntityType POCO Service class
    /// </summary>
    public partial class EntityTypeService
    {
        /// <summary>
        /// Gets an <see cref="Rock.Model.EntityType"/> by it's name / type name.
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
                    var EntityTypeService = new EntityTypeService( rockContext );
                    entityType = new EntityType();
                    entityType.Name = type.FullName;
                    entityType.FriendlyName = type.Name.SplitCase();
                    entityType.AssemblyName = type.AssemblyQualifiedName;
                    EntityTypeService.Add( entityType );
                    rockContext.SaveChanges();
                }

                // Read type using current context
                return this.Get( entityType.Id );
            }

            return null;
        }

        /// <summary>
        /// Gets an <see cref="Rock.Model.EntityType" /> by it's name. If a match is not found, a new <see cref="Rock.Model.EntityType" /> can optionally be created.
        /// </summary>
        /// <param name="name">A <see cref="System.String" /> representing the name of the object/entity type to search for.</param>
        /// <param name="createIfNotFound">A <see cref="System.Boolean" /> value that indicates if a new <see cref="Rock.Model.EntityType" /> should be created if a match is not found. This value
        /// will be <c>true</c> if a new <see cref="Rock.Model.EntityType" /> should be created if there is not a match; otherwise <c>false</c>/</param>
        /// <returns></returns>
        public EntityType Get( string name, bool createIfNotFound )
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
                var EntityTypeService = new EntityTypeService( rockContext );
                entityType = new EntityType();
                entityType.Name = name;
                EntityTypeService.Add( entityType );
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
        public IEnumerable<EntityType> GetReportableEntities(Person currentPerson)
        {
            return this.GetEntities()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( a => !Rock.Web.Cache.EntityTypeCache.Read( a ).GetEntityType().GetCustomAttributes( typeof( HideFromReportingAttribute ), true ).Any() );
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
        /// Gets a list of ISecured and IEntity entities (all models) that have not yet been registered and adds them
        /// as an <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String"/> that represents the physical path of the web application</param>
        public static void RegisterEntityTypes( string physWebAppPath )
        {
            var entityTypes = new Dictionary<string, EntityType>();

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Data.IEntity ) ) )
            {
                var entityType = new EntityType();
                entityType.Name = type.Key;
                entityType.FriendlyName = type.Value.Name.SplitCase();
                entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                entityType.IsEntity = !type.Value.GetCustomAttributes( typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), false ).Any();
                entityType.IsSecured = false;
                entityTypes.Add( type.Key.ToLower(), entityType );
            }

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Security.ISecured ) ) )
            {
                string key = type.Key.ToLower();

                if ( entityTypes.ContainsKey( key ) )
                {
                    entityTypes[key].IsSecured = true;
                }
                else
                {
                    var entityType = new EntityType();
                    entityType.Name = type.Key;
                    entityType.FriendlyName = type.Value.Name.SplitCase();
                    entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                    entityType.IsEntity = false;
                    entityType.IsSecured = true;
                    entityTypes.Add( key, entityType );
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );

                // Find any existing EntityTypes marked as an entity or secured that are no longer an entity or secured
                foreach ( var oldEntityType in entityTypeService.Queryable()
                    .Where( e => e.IsEntity || e.IsSecured )
                    .ToList() )
                {
                    if ( !entityTypes.Keys.Contains( oldEntityType.Name.ToLower() ) )
                    {
                        oldEntityType.IsSecured = false;
                        oldEntityType.IsEntity = false;
                        oldEntityType.AssemblyName = null;
                    }
                }

                // Update any existing entities
                foreach ( var existingEntityType in entityTypeService.Queryable()
                    .Where( e => entityTypes.Keys.Contains( e.Name ) )
                    .ToList() )
                {
                    var key = existingEntityType.Name.ToLower();

                    var entityType = entityTypes[key];

                    if ( existingEntityType.Name != entityType.Name ||
                        existingEntityType.IsEntity != entityType.IsEntity ||
                        existingEntityType.IsSecured != entityType.IsSecured ||
                        existingEntityType.FriendlyName != ( existingEntityType.FriendlyName ?? entityType.FriendlyName ) ||
                        existingEntityType.AssemblyName != entityType.AssemblyName )
                    {
                        existingEntityType.Name = entityType.Name;
                        existingEntityType.IsEntity = entityType.IsEntity;
                        existingEntityType.IsSecured = entityType.IsSecured;
                        existingEntityType.FriendlyName = existingEntityType.FriendlyName ?? entityType.FriendlyName;
                        existingEntityType.AssemblyName = entityType.AssemblyName;
                    }
                    entityTypes.Remove( key );
                }

                // Add the newly discovered entities 
                foreach ( var entityTypeInfo in entityTypes )
                {
                    // Don't add the EntityType entity as it will probably have been automatically 
                    // added by the audit on a previous save in this method.
                    if ( entityTypeInfo.Value.Name != "Rock.Model.EntityType" )
                    {
                        entityTypeService.Add( entityTypeInfo.Value );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the Guid for the EntityType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.EntityTypeCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }
}
