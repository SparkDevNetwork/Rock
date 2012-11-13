//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Util;

namespace Rock.Core
{
    /// <summary>
    /// EntityTypeWorkflowTrigger POCO Service class
    /// </summary>
    public partial class EntityTypeWorkflowTriggerService : Service<EntityTypeWorkflowTrigger, EntityTypeWorkflowTriggerDto>
    {
        /// <summary>
        /// Gets the workflow types.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <returns></returns>
        public IQueryable<EntityTypeWorkflowTrigger> Get( string entityTypeName, EntityTriggerType triggerType )
        {
            return Repository.AsQueryable()
                .Where( t =>
                    t.EntityType.Name == entityTypeName &&
                    t.EntityTriggerType == triggerType );
        }
    }
}
