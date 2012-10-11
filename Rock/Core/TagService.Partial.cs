//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Core
    
    /// <summary>
    /// MetricValue POCO Service class
    /// </summary>
    public partial class TagService
        
        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="ownerId">The owner id.</param>
        /// <returns></returns>
        public IQueryable<Tag> GetByEntity( string entity, string entityQualifierColumn, string entityQualifierValue, int? ownerId )
            
            return Repository.AsQueryable()
                .Where( t => t.Entity == entity &&
                    ( t.EntityQualifierColumn == entityQualifierColumn || ( t.EntityQualifierColumn == null && entityQualifierColumn == null ) ) &&
                    ( t.EntityQualifierValue == entityQualifierValue || ( t.EntityQualifierValue == null && entityQualifierValue == null ) ) &&
                    ( t.OwnerId == null || (ownerId.HasValue && t.OwnerId == ownerId) ) 
                    )
                .OrderBy( t => t.Name );
        }

        /// <summary>
        /// Gets the name of the by entity and.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityQualifierColumn">The entity qualifier column.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="ownerId">The owner id.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Tag GetByEntityAndName( string entity, string entityQualifierColumn, string entityQualifierValue, int? ownerId, string name )
            
            return Repository.AsQueryable()
                .Where( t => t.Entity == entity &&
                    ( t.EntityQualifierColumn == entityQualifierColumn || ( t.EntityQualifierColumn == null && entityQualifierColumn == null ) ) &&
                    ( t.EntityQualifierValue == entityQualifierValue || ( t.EntityQualifierValue == null && entityQualifierValue == null ) ) &&
                    ( t.OwnerId == null || ( ownerId.HasValue && t.OwnerId == ownerId )) &&
                    ( t.Name == name)
                    )
                .FirstOrDefault();
        }
    }
}
