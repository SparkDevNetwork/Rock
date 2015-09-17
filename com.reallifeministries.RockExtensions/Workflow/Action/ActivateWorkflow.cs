using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.reallifeministries.RockExtensions.Workflow.Action
{
    /// <summary>
    /// Activates a new activity for a given activity type
    /// </summary>
    [Description( "Activates a new workflow. The attributes from this current activity will be passed as attributes to the workflow." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Activate Workflow" )]

    [WorkflowTypeField( "Workflow", "The workflow type to activate", false,true,"","",0)]
    [TextField("WorkflowName", "What to name this Workflow Instance",true)]
    public class ActivateWorkflow : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var guid = GetAttributeValue( action, "Workflow" ).AsGuid();
            if (guid.IsEmpty())
            {
                action.AddLogEntry( "Invalid Workflow Property", true );
                return false;
            }

            var currentActivity = action.Activity;
            var currentWorkflow = action.Activity.Workflow;
            var newWorkflowType = new WorkflowTypeService( rockContext ).Get( guid );
            var newWorkflowName = GetAttributeValue(action, "WorkflowName" );

            if (newWorkflowType == null)
            {
                action.AddLogEntry( "Invalid Workflow Property", true );
                return false;
            }
            
            var newWorkflow = Rock.Model.Workflow.Activate( newWorkflowType, newWorkflowName );
            if (newWorkflow == null)
            {
                action.AddLogEntry( "The Workflow could not be activated", true );
                return false;
            }

            if (currentActivity.Attributes == null)
            {
                currentActivity.LoadAttributes( rockContext );
            }

            if (currentWorkflow.Attributes == null)
            {
                currentWorkflow.LoadAttributes( rockContext );
            }

            // Pass attributes from current Workflow to new Workflow.
            foreach (string key in currentWorkflow.AttributeValues.Keys)
            {
                newWorkflow.SetAttributeValue( key, currentWorkflow.GetAttributeValue( key ) );
            }

            // Pass attributes from current Activity to new Workflow.
            foreach (string key in currentActivity.AttributeValues.Keys)
            {
                newWorkflow.SetAttributeValue( key, currentActivity.GetAttributeValue(key) );
            }
            
            // Kick off processing of new Workflow
            if(newWorkflow.Process( rockContext, entity, out errorMessages )) 
            {
                if (newWorkflow.IsPersisted || newWorkflowType.IsPersisted)
                {
                    var workflowService = new Rock.Model.WorkflowService( rockContext );
                    workflowService.Add( newWorkflow );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        newWorkflow.SaveAttributeValues( rockContext );
                        foreach (var activity in newWorkflow.Activities)
                        {
                            activity.SaveAttributeValues( rockContext );
                        }
                    } );
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
