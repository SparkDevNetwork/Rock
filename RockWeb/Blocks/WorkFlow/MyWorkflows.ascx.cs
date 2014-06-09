// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Workflows" )]
    [Category( "WorkFlow" )]
    [Description( "Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user." )]

    [LinkedPage( "Entry Page", "Page used to entery form information for a workflow." )]
    public partial class MyWorkflows : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        protected int? SelectedWorkflowTypeId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SelectedWorkflowTypeId = ViewState["SelectedWorkflowTypeId"] as int?;

            GetData();
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptWorkflowTypes.ItemCommand += rptWorkflowTypes_ItemCommand;

            gWorkflows.DataKeyNames = new string[] { "id" };
            gWorkflows.Actions.ShowAdd = false;
            gWorkflows.IsDeleteEnabled = false;
            gWorkflows.GridRebind += gWorkflows_GridRebind;
             
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
                GetData();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["SelectedWorkflowTypeId"] = SelectedWorkflowTypeId;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglDisplay_CheckedChanged( object sender, EventArgs e )
        {
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptWorkflowTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptWorkflowTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? workflowTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if (workflowTypeId.HasValue)
            {
                SelectedWorkflowTypeId = workflowTypeId.Value;
            }

            GetData();
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            var workflow = new WorkflowService( new RockContext() ).Get( e.RowKeyId );
            if ( workflow != null )
            {
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                NavigateToLinkedPage( "EntryPage", qryParam );
            }
        }
        
        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            var rockContext = new RockContext();

            // Get all of the workflow types
            var allWorkflowTypes = new WorkflowTypeService( rockContext ).Queryable( "ActivityTypes" )
                .OrderBy( w => w.Name )
                .ToList();

            // Get the authorized activities in all workflow types
            var authorizedActivityTypes = AuthorizedActivityTypes( allWorkflowTypes );

            // Get all the active forms for any of the authorized activities
            var activeForms = new WorkflowActionService( rockContext ).Queryable( "ActionType.ActivityType.WorkflowType, Activity.Workflow" )
                .Where( a =>
                    a.ActionType.WorkflowFormId.HasValue &&
                    !a.CompletedDateTime.HasValue &&
                    a.Activity.ActivatedDateTime.HasValue &&
                    !a.Activity.CompletedDateTime.HasValue &&
                    a.Activity.Workflow.ActivatedDateTime.HasValue &&
                    !a.Activity.Workflow.CompletedDateTime.HasValue &&
                    authorizedActivityTypes.Contains( a.ActionType.ActivityTypeId ) )
                .ToList();


            // Limit the forms to those assigned to current user
            var userFormActions = activeForms
                .Where( a => a.Activity.IsAssigned( CurrentPerson, false ) )
                .ToList();

            // Create variable for storing authorized types and the count of active form actions
            var workflowTypeCounts = new Dictionary<int, int>();

            // Get any workflow types that have authorized activites and get the form count
            allWorkflowTypes
                .Where( w => w.ActivityTypes.Any( a => authorizedActivityTypes.Contains( a.Id ) ) )
                .Select( w => w.Id )
                .Distinct()
                .ToList()
                .ForEach( w => workflowTypeCounts.Add( w, userFormActions.Where( a => a.Activity.Workflow.WorkflowTypeId == w ).Count() ) );

            // Create a query to return workflow type, the count of active action forms, and the selected class
            var qry = allWorkflowTypes
                .Where( w => workflowTypeCounts.Keys.Contains( w.Id ) )
                .Select( w => new
                {
                    WorkflowType = w,
                    Count = workflowTypeCounts[w.Id],
                    Class = ( SelectedWorkflowTypeId.HasValue && SelectedWorkflowTypeId.Value == w.Id ) ? "active" : ""
                } );

            // If displaying active only, update query to exclude those workflow types without any active form actions
            if ( tglDisplay.Checked )
            {
                qry = qry.Where( q => q.Count > 0 );
            }

            rptWorkflowTypes.DataSource = qry.ToList();
            rptWorkflowTypes.DataBind();

            WorkflowType selectedWorkflowType = null;
            if ( SelectedWorkflowTypeId.HasValue )
            {
                selectedWorkflowType = allWorkflowTypes
                    .Where( w => 
                        w.Id == SelectedWorkflowTypeId.Value &&
                        workflowTypeCounts.Keys.Contains( SelectedWorkflowTypeId.Value ) )
                    .FirstOrDefault();
            }

            if ( selectedWorkflowType != null )
            {

                AddAttributeColumns( selectedWorkflowType );

                gWorkflows.DataSource = userFormActions
                    .Select( a => a.Activity.Workflow )
                    .Distinct()
                    .Where( w => w.WorkflowTypeId == selectedWorkflowType.Id )
                    .ToList();
                gWorkflows.DataBind();
                gWorkflows.Visible = true;

            }
            else
            {
                gWorkflows.Visible = false;
            }

        }

        private List<int> AuthorizedActivityTypes( List<WorkflowType> allWorkflowTypes )
        {
            var authorizedActivityTypes = new List<int>();

            foreach ( var workflowType in allWorkflowTypes )
            {
                if ( ( workflowType.IsActive ?? true ) && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    foreach ( var activityType in workflowType.ActivityTypes.Where( a => a.ActionTypes.Any( f => f.WorkflowFormId.HasValue ) ) )
                    {
                        if ( ( activityType.IsActive ?? true ) && activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            authorizedActivityTypes.Add( activityType.Id );
                        }
                    }
                }
            }

            return authorizedActivityTypes;
        }

        protected void AddAttributeColumns( WorkflowType workflowType)
        {
            // Remove attribute columns
            foreach ( var column in gWorkflows.Columns.OfType<AttributeField>().ToList() )
            {
                gWorkflows.Columns.Remove( column );
            }

            if ( workflowType != null )
            {
                // Add attribute columns
                int entityTypeId = new Workflow().TypeId;
                string qualifier = workflowType.Id.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gWorkflows.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gWorkflows.Columns.Add( boundField );
                    }
                }
            }
        }

        #endregion

    }

}