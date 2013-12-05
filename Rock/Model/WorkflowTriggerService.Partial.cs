//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.WorkflowTrigger"/> entity objects.
    /// </summary>
    public partial class WorkflowTriggerService 
    {
        /// <summary>
        /// Returns a queryable collection of  the <see cref="Rock.Model.WorkflowTrigger"/> by <see cref="Rock.Model.EntityType"/> name
        /// and <see cref="Rock.Model.WorkflowTriggerType"/>
        /// </summary>
        /// <param name="entityTypeName">A <see cref="System.String"/> representing the name of the <see cref="Rock.Model.EntityType"/> to filter by.</param>
        /// <param name="triggerType">The <see cref="Rock.Model.WorkflowTriggerType"/> to filter by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.WorkflowTrigger"/> entities that meet the specified criteria.</returns>
        public IQueryable<WorkflowTrigger> Get( string entityTypeName, WorkflowTriggerType triggerType )
        {
            return Repository.AsQueryable()
                .Where( t =>
                    t.EntityType.Name == entityTypeName &&
                    t.WorkflowTriggerType == triggerType );
        }
    }
}
