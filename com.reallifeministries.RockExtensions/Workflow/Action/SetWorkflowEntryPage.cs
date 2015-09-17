using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.reallifeministries.RockExtensions.Workflow.Action
{
    /// <summary>
    /// Sends user to a specific page to continue the workflow 
    /// </summary>
    [Description( "Sets up the workflow entry form page (to be uses with the corresponding block" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "SetWorkflowEntryPage" )]

    [LinkedPage("EntryFormPage","A page to send the user to to continue the workflow",true)]
    [BooleanField("Redirect", "Redirect user to entry page now", true)]
    public class SetWorkflowEntryPage : ActionComponent
    {
        private RockContext _rockContext;
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

            _rockContext = rockContext;
            
            var AttrKey = "EntryFormPage";
            var workflow = action.Activity.Workflow;

            var entryPage = GetAttributeValue( action, AttrKey );

            if (workflow.IsPersisted && !action.Activity.Workflow.Attributes.ContainsKey( AttrKey ))
            {
                if (workflow.Id == 0)
                {
                    PersistWorkflow( workflow ); //ensure we get the id back from the DB;
                }

                // If workflow attribute doesn't exist, create it 
                // ( should only happen on first workflow using this action for the current activity)
                var attribute = new Rock.Model.Attribute();
                attribute.EntityTypeId = workflow.TypeId;
                attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                attribute.EntityTypeQualifierValue = workflow.WorkflowTypeId.ToString();
                attribute.Name = AttrKey;
                attribute.Key = AttrKey;
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.PAGE_REFERENCE.AsGuid() ).Id;

                // Set the value for this action's instance to the current time
                var attributeValue = new Rock.Model.AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = entryPage;
                new AttributeValueService( rockContext ).Add( attributeValue );

                action.AddLogEntry( string.Format( "Attribute ({0}) added to Workflow with value: {1}", AttrKey, entryPage), true );
            }
            else
            {
                workflow.SetAttributeValue( AttrKey, entryPage );
                action.AddLogEntry( string.Format( "Attribute ({0}) set on Workflow with value: {1}", AttrKey, entryPage ), true );
            }

            var doRedirect = GetAttributeValue( action, "Redirect" ).AsBoolean();

            if (doRedirect)
            {
                if (workflow.IsPersisted)
                { 
                    PersistWorkflow(workflow); //ensure any changes are saved before redirecting.
                }

                if (HttpContext.Current != null)
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "WorkflowTypeId", action.Activity.Workflow.WorkflowTypeId.ToString() );

                    if (workflow.Id != 0)
                    {
                        queryParams.Add( "WorkflowId", action.Activity.WorkflowId.ToString() );
                    }

                    var pageReference = new Rock.Web.PageReference( entryPage, queryParams );

                    HttpContext.Current.Response.Redirect(pageReference.BuildUrl(),false);
                }
            }

            return true;
        }
        
        private void PersistWorkflow(Rock.Model.Workflow workflow )
        {
            if (workflow.Id == 0)
            {
                var workflowService = new WorkflowService( _rockContext );
                workflowService.Add( workflow );
            }

            _rockContext.WrapTransaction( () =>
            {
                _rockContext.SaveChanges();
                workflow.SaveAttributeValues( _rockContext );
                foreach (var activity in workflow.Activities)
                {
                    activity.SaveAttributeValues( _rockContext );
                }
            } );
        }
    }
}
