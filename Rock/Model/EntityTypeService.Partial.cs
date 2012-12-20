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
        /// Gets EntityTypes by EntityName
        /// </summary>
        /// <param name="entityName">Entity.</param>
        /// <returns>An enumerable list of EntityType objects.</returns>
        public EntityType Get( string entityName )
        {
            return Repository.FirstOrDefault( t => t.Name == entityName );
        }

        /// <summary>
        /// Gets the specified type, and optionally creates new type if not found.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="createIfNotFound">if set to <c>true</c> [create if not found].</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public EntityType Get(string entityName, bool createIfNotFound, int? personId )
        {
            var entityType = Get( entityName );
            if ( entityType != null )
                return entityType;

            if ( createIfNotFound )
            {
                entityType = new EntityType();
                entityType.Name = entityName;
                this.Add(entityType, personId);
                this.Save(entityType, personId);

                return entityType;
            }

            return null;
        }

        /// <summary>
        /// Gets a list of ISecured entities (all models) that have not yet been registered and adds them
        /// as an entity type.
        /// </summary>
        /// <param name="physWebAppPath">the physical path of the web application</param>
        public void RegisterEntityTypes( string physWebAppPath )
        {
            var entityTypes = new Dictionary<string, EntityType>();

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Data.IEntity ),
                new DirectoryInfo[] { 
                    new DirectoryInfo( physicalPath( physWebAppPath, "bin" ) ), 
                    new DirectoryInfo( physicalPath( physWebAppPath, "Plugins" ) ) } ) )
            {
                var entityType = new EntityType();
                entityType.Name = type.Value.FullName;
                entityType.FriendlyName = type.Value.Name.SplitCase();
                entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                entityType.IsEntity = true;
                entityType.IsSecured = false;
                entityTypes.Add( type.Value.FullName, entityType );
            }

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Security.ISecured ),
                new DirectoryInfo[] { 
                    new DirectoryInfo( physicalPath( physWebAppPath, "bin" ) ), 
                    new DirectoryInfo( physicalPath( physWebAppPath, "Plugins" ) ) } ) )
            {
                if ( entityTypes.ContainsKey( type.Value.FullName ) )
                {
                    entityTypes[type.Value.FullName].IsSecured = true;
                }
                else
                {
                    var entityType = new EntityType();
                    entityType.Name = type.Value.FullName;
                    entityType.FriendlyName = type.Value.Name.SplitCase();
                    entityType.AssemblyName = type.Value.AssemblyQualifiedName;
                    entityType.IsEntity = false;
                    entityType.IsSecured = true;
                    entityTypes.Add( type.Value.FullName, entityType);
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

                existingEntityType.IsEntity = entityType.IsEntity;
                existingEntityType.IsSecured = entityType.IsSecured;
                existingEntityType.FriendlyName = existingEntityType.FriendlyName ?? entityType.FriendlyName;
                existingEntityType.AssemblyName = entityType.AssemblyName;
                Save( existingEntityType, null );
                entityTypes.Remove( entityType.Name );
            }

            // Add the newly discovered entities
            foreach ( var entityTypeInfo in entityTypes )
            {
                this.Add( entityTypeInfo.Value, null );
                this.Save( entityTypeInfo.Value, null );
            }
        }

        private string physicalPath( string physWebAppPath, string folder )
        {
            return string.Format( @"{0}{1}{2}\", physWebAppPath, ( physWebAppPath.EndsWith( @"\" ) ) ? "" : @"\", folder );
        }

    }
}
