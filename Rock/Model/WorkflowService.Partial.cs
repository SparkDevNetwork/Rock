//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Workflow"/> entity objects
    /// </summary>
    public partial class WorkflowService 
    {
        /// <summary>
        /// Activates a new <see cref="Rock.Model.Workflow"/> instance.
        /// </summary>
        /// <param name="workflowType">The <see cref="Rock.Model.WorkflowType"/> to be activated.</param>
        /// <param name="name">A <see cref="System.String"/> representing the name of the <see cref="Rock.Model.Workflow"/> instance.</param>
        /// <param name="currentPersonId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is activating the 
        /// <see cref="Rock.Model.Workflow"/> instance; this will be null if it was completed by the anonymous user.</param>
        /// <returns>The activated <see cref="Rock.Model.Workflow"/> instance</returns>
        public Workflow Activate( WorkflowType workflowType, string name, int? currentPersonId )
        {
            var workflow = Workflow.Activate( workflowType, name );
            
            this.Add( workflow, currentPersonId );
            this.Save( workflow, currentPersonId );

            return workflow;
        }

        /// <summary>
        /// Processes the specified <see cref="Rock.Model.Workflow"/>
        /// </summary>
        /// <param name="workflow">The <see cref="Rock.Model.Workflow"/> instance to process.</param>
        /// <param name="CurrentPersonId">A <see cref="System.String"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is processing the <see cref="Rock.Model.Workflow"/>.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> that contains any error messages that were returned while processing the <see cref="Rock.Model.Workflow"/>.</param>
        public void Process( Workflow workflow, int? CurrentPersonId, out List<string> errorMessages )
        {
            workflow.IsProcessing = true;
            this.Save( workflow, null );

            workflow.Process(out errorMessages); 

            workflow.IsProcessing = false;
            this.Save( workflow, null );
        }

        /// <summary>
        /// Gets the active <see cref="Rock.Model.Workflow">Workflows</see>.
        /// </summary>
        /// <returns>A queryable collection of active <see cref="Rock.Model.Workflow"/>entities ordered by LastProcessedDate.</returns>
        public IQueryable<Workflow> GetActive()
        {
            return Repository.AsQueryable()
                .Where( w =>
                    w.ActivatedDateTime.HasValue &&
                    !w.CompletedDateTime.HasValue )
                .OrderBy( w => w.LastProcessedDateTime );
        }

    }
}