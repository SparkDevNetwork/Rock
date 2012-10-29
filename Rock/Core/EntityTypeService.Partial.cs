//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock;
using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// EntityType POCO Service class
    /// </summary>
    public partial class EntityTypeService : Service<EntityType, EntityTypeDto>
    {
        /// <summary>
        /// Gets EntityTypes by EntityName
        /// </summary>
        /// <param name="entityName">Entity.</param>
        /// <returns>An enumerable list of EntityType objects.</returns>
        public EntityType Get( string entityName )
        {
            return Repository.FirstOrDefault( t => t.EntityTypeName == entityName );
        }

        /// <summary>
        /// Gets the specified type, and optionally creates new type if not found.
        /// </summary>
        /// <param name="type">The type.</param>
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

    }
}
