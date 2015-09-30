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
    [Description( "Activates a new workflow. The attributes from this current activity, and workflow will be passed as attributes to the next workflow." )]
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

            CopyAttributes( newWorkflow, currentActivity, rockContext );

            SaveForProcessingLater( newWorkflow, rockContext );

            return true;
            // Kick off processing of new Workflow
            /*if(newWorkflow.Process( rockContext, entity, out errorMessages )) 
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
            }*/
        }
        private void SaveForProcessingLater(Rock.Model.Workflow newWorkflow, RockContext rockContext)
        {
            newWorkflow.IsPersisted = true;
            var service = new WorkflowService( rockContext );
            if (newWorkflow.Id == 0)
            {
                service.Add( newWorkflow );
            }

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

        private void CopyAttributes(Rock.Model.Workflow newWorkflow, Rock.Model.WorkflowActivity currentActivity, RockContext rockContext)
        {
            if (currentActivity.Attributes == null)
            {
                currentActivity.LoadAttributes( rockContext );
            }

            if (currentActivity.Workflow.Attributes == null)
            {
                currentActivity.Workflow.LoadAttributes( rockContext );
            }

            // Pass attributes from current Workflow to new Workflow.
            foreach (string key in currentActivity.Workflow.AttributeValues.Keys)
            {
                newWorkflow.SetAttributeValue( key, currentActivity.Workflow.GetAttributeValue( key ) );
            }

            // Pass attributes from current Activity to new Workflow.
            foreach (string key in currentActivity.AttributeValues.Keys)
            {
                newWorkflow.SetAttributeValue( key, currentActivity.GetAttributeValue(key) );
            }
        }
    }
}
