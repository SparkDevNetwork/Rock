//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes any entity chnages that are configured to be tracked
    /// </summary>
    public class WorkflowTriggerTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the workflow trigger.
        /// </summary>
        /// <value>
        /// The workflow trigger.
        /// </value>
        public WorkflowTrigger Trigger { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( Trigger != null )
            {
                var workflowTypeService = new WorkflowTypeService();
                var workflowType = workflowTypeService.Get( Trigger.WorkflowTypeId );

                if ( workflowType != null )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, Trigger.WorkflowName );

                    List<string> workflowErrors;
                    if ( workflow.Process( Entity, out workflowErrors ) )
                    {
                        if ( workflowType.IsPersisted )
                        {
                            var workflowService = new Rock.Model.WorkflowService();
                            workflowService.Add( workflow, PersonId );
                            workflowService.Save( workflow, PersonId );
                        }
                    }
                }
            }
        }
    }
}