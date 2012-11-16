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
using Rock.Util;

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
        public WorkflowTriggerDto Trigger { get; set; }

        /// <summary>
        /// Gets or sets the dto entity.
        /// </summary>
        /// <value>
        /// The dto.
        /// </value>
        public IDto Dto { get; set; }

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
                    var workflow = Rock.Util.Workflow.Activate( workflowType, Trigger.WorkflowName );

                    List<string> workflowErrors;
                    if ( workflow.Process( Dto, out workflowErrors ) )
                    {
                        if ( workflowType.IsPersisted )
                        {
                            var workflowService = new Rock.Util.WorkflowService();
                            workflowService.Add( workflow, PersonId );
                            workflowService.Save( workflow, PersonId );
                        }
                    }
                }
            }
        }
    }
}