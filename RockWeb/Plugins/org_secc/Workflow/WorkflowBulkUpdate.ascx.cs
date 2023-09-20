// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.WorkFlowUpdate
{
    [DisplayName( "Workflow Bulk Update" )]
    [Category( "SECC > WorkFlow" )]
    [Description( "Tool for updating workflows in bulk." )]


    public partial class WorkflowBulkUpdate : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                DisplaySettings();
            }


        }

        private void DisplaySettings()
        {
            RockContext rockContext = new RockContext();
            EntitySetService entitySetService = new EntitySetService( rockContext );
            var entitySet = entitySetService.Get( PageParameter( "EntitySetId" ).AsInteger() );
            var workflowEntityId = EntityTypeCache.GetId( typeof( Rock.Model.Workflow ) );

            if ( entitySet == null || entitySet.EntityTypeId != workflowEntityId )
            {
                return;
            }

            WorkflowService workflowService = new WorkflowService( rockContext );

            var workflowQueryable = workflowService.Queryable();

            var workflows = entitySet.Items
                .Join( workflowQueryable,
                    i => i.EntityId,
                    w => w.Id,
                    ( i, w ) => w );

            var firstWorkflow = workflows.FirstOrDefault();
            if ( firstWorkflow == null )
            {
                return;
            }

            var workflowTypeId = firstWorkflow.WorkflowTypeId;

            AttributeService attributeService = new AttributeService( rockContext );

            var attributes = attributeService.Queryable()
              .Where( a => a.EntityTypeId == workflowEntityId
                && a.EntityTypeQualifierColumn == "WorkflowTypeId"
                && a.EntityTypeQualifierValue == workflowTypeId.ToString() )
              .OrderBy( a => a.Order )
              .ToList();

            rAttributes.DataSource = attributes;
            rAttributes.DataBind();

            var workflowActivityTypes = firstWorkflow.WorkflowType.ActivityTypes;
            ddlActivities.DataSource = workflowActivityTypes;
            ddlActivities.DataBind();
            ddlActivities.Items.Insert( 0, new ListItem() );

            hfCount.Value = workflows.Count().ToString();
            hfWorkflowTypeName.Value = firstWorkflow.WorkflowType.Name;
            nbNotification.Text = string.Format( "Updating {0} {1} workflows.", workflows.Count(), firstWorkflow.WorkflowType.Name );
        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            var errors = new List<string>();
            RockContext rockContext = new RockContext();
            var workflows = GetWorkflows( rockContext );

            //Get the new attribute values from the repeater
            Dictionary<string, string> attributeUpdates = GetNewAttributeValues();

            if ( attributeUpdates.Any() )
            {
                var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                foreach ( var workflow in workflows )
                {
                    //Reuse the mergefields with new workflow
                    mergeFields["Workflow"] = workflow;

                    //We will store the attribute value changes and apply them all at once
                    Dictionary<string, string> toUpdate = new Dictionary<string, string>();
                    foreach ( var update in attributeUpdates )
                    {
                        toUpdate[update.Key] = update.Value.ResolveMergeFields( mergeFields );
                        //if (toUpdate[update.Key])
                        //{

                        //}
                    }

                    foreach ( var update in toUpdate )
                    {
                        workflow.SetAttributeValue( update.Key, update.Value );
                    }

                    workflow.SaveAttributeValues();
                }
            }

            //Workflow settings

            //Activate the workflows if requested or if a new activity is activated
            if ( ddlState.SelectedValue == "NotComplete" || ddlActivities.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var workflow in workflows )
                {
                    workflow.CompletedDateTime = null;
                }
            }
            else if ( ddlState.SelectedValue == "Complete" )
            {
                foreach ( var workflow in workflows )
                {
                    workflow.MarkComplete();
                }
            }

            //Update Status message
            if ( tbStatus.Text.IsNotNullOrWhiteSpace() )
            {
                var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                foreach ( var workflow in workflows )
                {
                    mergeFields["Workflow"] = workflow;
                    workflow.Status = tbStatus.Text.ResolveMergeFields( mergeFields );
                }
            }

            //Activate New Activity

            int? activityTypeId = ddlActivities.SelectedValueAsId();
            if ( activityTypeId.HasValue )
            {

                var activityType = WorkflowActivityTypeCache.Get( activityTypeId.Value );
                if ( activityType != null )
                {
                    foreach ( var workflow in workflows )
                    {
                        var activity = WorkflowActivity.Activate( activityType, workflow, rockContext );
                        activity.Guid = Guid.NewGuid();

                        foreach ( var action in activity.Actions )
                        {
                            action.Guid = Guid.NewGuid();
                        }
                    }
                }
            }
            rockContext.SaveChanges();

            //Process workflows
            List<string> errorMessages = new List<string>();
            if ( workflows.Where( w => w.IsActive ).Any() )
            {
                foreach ( var workflow in workflows )
                {
                    WorkflowService workflowService = new WorkflowService( new RockContext() );
                    workflowService.Process( workflow, out errorMessages );
                }
            }

            pnlConfirmation.Visible = false;
            pnlDisplay.Visible = false;
            pnlDone.Visible = true;
        }

        private Dictionary<string, string> GetNewAttributeValues()
        {
            Dictionary<string, string> attributeUpdates = new Dictionary<string, string>();
            foreach ( var item in rAttributes.Items )
            {
                var repeaterItem = ( RepeaterItem ) item;
                var cbUpdate = repeaterItem.FindControl( "cbUpdate" ) as CheckBox;
                if ( cbUpdate.Checked )
                {
                    var hfAttributeKey = repeaterItem.FindControl( "hfAttributeKey" ) as HiddenField;
                    var tbValue = repeaterItem.FindControl( "tbValue" ) as RockTextBox;
                    attributeUpdates.Add( hfAttributeKey.Value, tbValue.Text );
                }
            }
            return attributeUpdates;
        }

        private List<Rock.Model.Workflow> GetWorkflows( RockContext rockContext )
        {
            EntitySetService entitySetService = new EntitySetService( rockContext );
            var entitySet = entitySetService.Get( PageParameter( "EntitySetId" ).AsInteger() );
            var workflowEntityId = EntityTypeCache.GetId( typeof( Rock.Model.Workflow ) );

            if ( entitySet == null || entitySet.EntityTypeId != workflowEntityId )
            {
                return null;
            }

            WorkflowService workflowService = new WorkflowService( rockContext );

            var workflowQueryable = workflowService.Queryable();

            var qry = entitySet.Items
                .Join( workflowQueryable,
                    i => i.EntityId,
                    w => w.Id,
                    ( i, w ) => w );

            var firstWorkflow = qry.FirstOrDefault();
            if ( firstWorkflow == null )
            {
                return null;
            }

            var workflowTypeId = firstWorkflow.WorkflowTypeId;

            AttributeService attributeService = new AttributeService( rockContext );
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );

            var attributeQueryable = attributeService.Queryable()
              .Where( a => a.EntityTypeId == workflowEntityId
                && a.EntityTypeQualifierColumn == "WorkflowTypeId"
                && a.EntityTypeQualifierValue == workflowTypeId.ToString() )
              .Select( a => a.Id );

            var attributeValueQueryable = attributeValueService.Queryable()
                .Where( av => attributeQueryable.Contains( av.AttributeId ) );

            var mixedItems = qry.GroupJoin( attributeValueQueryable,
            w => w.Id,
            av => av.EntityId,
            ( w, av ) => new { Workflow = w, AttributeValues = av } )
            .ToList();

            Dictionary<string, AttributeCache> attributes = new Dictionary<string, AttributeCache>();

            foreach ( var id in attributeQueryable.ToList() )
            {
                var attributeCache = AttributeCache.Get( id );
                if ( attributeCache != null )
                {
                    attributes[attributeCache.Key] = attributeCache;
                }
            }

            var workflows = new List<Rock.Model.Workflow>();
            foreach ( var item in mixedItems )
            {
                var workflow = item.Workflow;
                workflow.Attributes = attributes;
                workflow.AttributeValues = new Dictionary<string, AttributeValueCache>();
                foreach ( var attribute in attributes )
                {
                    var attributeValue = item.AttributeValues.Where( av => av.AttributeKey == attribute.Key ).FirstOrDefault();
                    workflow.AttributeValues[attribute.Key] = attributeValue != null ? new AttributeValueCache( attributeValue ) : new AttributeValueCache();
                }
                workflows.Add( workflow );
            }
            return workflows;
        }

        protected void btnContinue_Click( object sender, EventArgs e )
        {
            var text = "<br>You are about to update" + hfCount.Value + " workflows with the following information:";

            var attributeChanges = GetNewAttributeValues();
            if ( attributeChanges.Any() )
            {
                text += "<h5>Attributes:</h5><ul>";
                foreach ( var attribute in attributeChanges )
                {
                    text += "<li>" + attribute.Key + "</li>";
                }
                text += "</ul>";
            }

            List<string> workflowChanges = new List<string>();

            if ( ddlState.SelectedValue == "NotComplete" || ddlActivities.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                workflowChanges.Add( "Reactivate Inactive Workflows" );
            }
            else if ( ddlState.SelectedValue == "Complete" )
            {
                workflowChanges.Add( "Complete Active Workflows" );
            }

            if ( tbStatus.Text.IsNotNullOrWhiteSpace() )
            {
                workflowChanges.Add( "Update Workflow Statuses" );
            }

            if ( ddlActivities.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                workflowChanges.Add( "Activate Activity: " + ddlActivities.SelectedItem.Text );
            }

            if ( workflowChanges.Any() )
            {
                text += "<h5>Workflow:</h5><ul>";
                foreach ( var change in workflowChanges )
                {
                    text += "<li>" + change + "</li>";
                }
                text += "</ul>";
            }

            text += "<h5>Other:</h5><ul><li>Process Workflows</li></ul>" +
                "<i>Note: If your changes are complex or there are a large number of workflows this may take a few minutes.</i>";

            nbConfirmation.Text = text;
            pnlConfirmation.Visible = true;
            pnlDisplay.Visible = false;
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlConfirmation.Visible = false;
            pnlDisplay.Visible = true;
        }

        protected void btnDone_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
    }
}