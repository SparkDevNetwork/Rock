//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
            return Repository.FirstOrDefault( t => t.Name == entityName );
        }

        /// <summary>
        /// Gets the specified <see cref="Rock.Model.EntityType"/> by the object type. If a match is not found, it can optionally create a new <see cref="Rock.Model.EntityType"/> for the object.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> to search for.</param>
        /// <param name="createIfNotFound">A <see cref="System.Boolean"/> value that indicates if a new <see cref="Rock.Model.EntityType"/> should be created if a match is not found. This value
        /// will be <c>true</c> if a new <see cref="Rock.Model.EntityType"/> should be created if there is not a match; otherwise <c>false</c>/</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is searching for and possibly creating a new EntityType.  This value can be
        /// null if the logged in person is not known (i.e. an anonymous user).</param>
        /// <returns>A <see cref="Rock.Model.EntityType"/> matching the provided type. If a match is not found and createIfNotFound is false this value will be null.</returns>
        public EntityType Get( Type type, bool createIfNotFound, int? personId )
        {
            var entityType = Get( type.FullName );
            if ( entityType != null )
                return entityType;

            if ( createIfNotFound )
            {
                entityType = new EntityType();
                entityType.Name = type.FullName;
                entityType.FriendlyName = type.Name.SplitCase();
                entityType.AssemblyName = type.AssemblyQualifiedName;

                this.Add( entityType, personId );
                this.Save( entityType, personId );

                return entityType;
            }

            return null;
        }

        /// <summary>
        /// Gets an <see cref="Rock.Model.EntityType" /> by it's name. If a match is not found, a new <see cref="Rock.Model.EntityType" /> can optionally be created.
        /// </summary>
        /// <param name="name">A <see cref="System.String" /> representing the name of the object/entity type to search for.</param>
        /// <param name="createIfNotFound">A <see cref="System.Boolean"/> value that indicates if a new <see cref="Rock.Model.EntityType"/> should be created if a match is not found. This value
        /// will be <c>true</c> if a new <see cref="Rock.Model.EntityType"/> should be created if there is not a match; otherwise <c>false</c>/</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is searching for and possibly creating a new EntityType.  This value can be
        /// null if the logged in person is not known (i.e. an anonymous user).</param>
        /// <returns></returns>
        public EntityType Get( string name, bool createIfNotFound, int? personId )
        {
            var entityType = Get( name );
            if ( entityType != null )
                return entityType;

            if ( createIfNotFound )
            {
                entityType = new EntityType();
                entityType.Name = name;

                this.Add( entityType, personId );
                this.Save( entityType, personId );

                return entityType;
            }

            return null;
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> where the IsEntity flag is set to true.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Rock.Model.EntityType"/> where the IsEntity flag is set tot true.</returns>
        public IEnumerable<EntityType> GetEntities()
        {
            return Repository.AsQueryable()
                .Where( e => e.IsEntity );
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
        public void RegisterEntityTypes( string physWebAppPath )
        {
            var entityTypes = new Dictionary<string, EntityType>();

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Data.IEntity ) ) )
            {
                var entityType = new EntityType();
                entityType.Name = type.Key;
                entityType.FriendlyName = type.Value.Name.SplitCase();
                entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                entityType.IsEntity = true;
                entityType.IsSecured = false;
                entityTypes.Add( type.Key, entityType );
            }

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Security.ISecured ) ) )
            {
                if ( entityTypes.ContainsKey( type.Key ) )
                {
                    entityTypes[type.Key].IsSecured = true;
                }
                else
                {
                    var entityType = new EntityType();
                    entityType.Name = type.Key;
                    entityType.FriendlyName = type.Value.Name.SplitCase();
                    entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                    entityType.IsEntity = false;
                    entityType.IsSecured = true;
                    entityTypes.Add( type.Key, entityType );
                }
            }

            // Find any existing EntityTypes marked as an entity or secured that are no longer an entity or secured
            foreach ( var oldEntityType in Repository.AsQueryable()
                .Where( e => !entityTypes.Keys.Contains( e.Name ) && ( e.IsEntity || e.IsSecured ) )
                .ToList() )
            {
                oldEntityType.IsSecured = false;
                oldEntityType.IsEntity = false;
                oldEntityType.AssemblyName = null;
                Save( oldEntityType, null );
            }

            // Update any existing entities
            foreach ( var existingEntityType in Repository.AsQueryable()
                .Where( e => entityTypes.Keys.Contains( e.Name ) )
                .ToList() )
            {
                var entityType = entityTypes[existingEntityType.Name];

                if ( existingEntityType.IsEntity != entityType.IsEntity ||
                    existingEntityType.IsSecured != entityType.IsSecured ||
                    existingEntityType.FriendlyName != ( existingEntityType.FriendlyName ?? entityType.FriendlyName ) ||
                    existingEntityType.AssemblyName != entityType.AssemblyName )
                {
                    existingEntityType.IsEntity = entityType.IsEntity;
                    existingEntityType.IsSecured = entityType.IsSecured;
                    existingEntityType.FriendlyName = existingEntityType.FriendlyName ?? entityType.FriendlyName;
                    existingEntityType.AssemblyName = entityType.AssemblyName;
                    Save( existingEntityType, null );
                }
                entityTypes.Remove( entityType.Name );
            }

            // Add the newly discovered entities 
            foreach ( var entityTypeInfo in entityTypes )
            {
                // Don't add the EntityType entity as it will probably have been automatically 
                // added by the audit on a previous save in this method.
                if ( entityTypeInfo.Value.Name != "Rock.Model.EntityType" )
                {
                    this.Add( entityTypeInfo.Value, null );
                    this.Save( entityTypeInfo.Value, null );
                }
            }
        }
    }
}
