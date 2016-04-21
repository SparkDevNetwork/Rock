﻿// <copyright>
// Copyright by the Spark Development Network
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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Workflows" )]
    [Category( "WorkFlow" )]
    [Description( "Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user." )]
    [LinkedPage( "Entry Page", "Page used to enter form information for a workflow." )]
    [LinkedPage( "Detail Page", "Page used to view status of a workflow." )]
    public partial class MyWorkflows : Rock.Web.UI.RockBlock
    {
        #region Fields

        private const string ROLE_TOGGLE_SETTING = "MyWorkflows_RoleToggle";
        private const string DISPLAY_TOGGLE_SETTING = "MyWorkflows_DisplayToggle";

        #endregion

        #region Properties

        protected bool? StatusFilter { get; set; }

        protected bool? RoleFilter { get; set; }

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

            StatusFilter = ViewState["StatusFilter"] as bool?;
            RoleFilter = ViewState["RoleFilter"] as bool?;
            SelectedWorkflowTypeId = ViewState["SelectedWorkflowTypeId"] as int?;
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptWorkflowTypes.ItemCommand += rptWorkflowTypes_ItemCommand;
            rptWorkflowTypes.ItemCreated += rptWorkflowTypes_ItemCreated;
            gWorkflows.DataKeyNames = new string[] { "Id" };
            gWorkflows.Actions.ShowAdd = false;
            gWorkflows.IsDeleteEnabled = false;
            gWorkflows.GridRebind += gWorkflows_GridRebind;

            if ( SelectedWorkflowTypeId.HasValue )
            {
                WorkflowType workflowType = new WorkflowTypeService( new RockContext() ).Get( SelectedWorkflowTypeId.Value );
                AddAttributeColumns( workflowType );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// Allows you to set query strings to activate filters on My Workflows block
        /// Will returns the filter values based on query string
        ///
        /// Role Filter
        /// ---------------------
        /// Initiated By Me = true
        /// Assigned To Me =  false
        ///
        /// Status Filter
        /// -------------------
        /// Active Types = true
        /// All Types = false
        /// </remarks>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.visible.min.js" ) );

            if ( !Page.IsPostBack )
            {
                bool? queryStatusFilter = Request.QueryString["StatusFilter"].AsBooleanOrNull();
                bool? queryRoleFilter = Request.QueryString["RoleFilter"].AsBooleanOrNull();

                /// If query string values exist then set them
                if ( queryStatusFilter.HasValue || queryRoleFilter.HasValue )
                {
                    tglDisplay.Checked = queryStatusFilter.GetValueOrDefault();
                    tglRole.Checked = queryRoleFilter.GetValueOrDefault();
                }
                else
                {
                    tglDisplay.Checked = GetUserPreference( DISPLAY_TOGGLE_SETTING ).AsBoolean();
                    tglRole.Checked = GetUserPreference( ROLE_TOGGLE_SETTING ).AsBoolean();
                }

                StatusFilter = tglDisplay.Checked;
                RoleFilter = tglRole.Checked;

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
            ViewState["StatusFilter"] = StatusFilter;
            ViewState["RoleFilter"] = RoleFilter;
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
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            StatusFilter = tglDisplay.Checked;
            RoleFilter = tglRole.Checked;

            SetUserPreference( DISPLAY_TOGGLE_SETTING, tglDisplay.Checked.ToString() );
            SetUserPreference( ROLE_TOGGLE_SETTING, tglRole.Checked.ToString() );

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
            if ( workflowTypeId.HasValue )
            {
                SelectedWorkflowTypeId = workflowTypeId.Value;
            }

            GetData();

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToGrid",
                "scrollToGrid();",
                true );
        }

        /// <summary>
        /// Handles the ItemCreated event of the rptWorkflowTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptWorkflowTypes_ItemCreated( object sender, RepeaterItemEventArgs e )
        {
            var lbWorkflowType = e.Item.FindControl( "lbWorkflowType" ) as LinkButton;
            if ( lbWorkflowType != null )
            {
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbWorkflowType );
            }
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
                qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                if ( tglRole.Checked )
                {
                    NavigateToLinkedPage( "DetailPage", qryParam );
                }
                else
                {
                    qryParam.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                    NavigateToLinkedPage( "EntryPage", qryParam );
                }
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
            GetData();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            var rockContext = new RockContext();

            int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

            // Get all of the workflow types
            var allWorkflowTypes = new WorkflowTypeService( rockContext ).Queryable( "ActivityTypes" )
                .OrderBy( w => w.Name )
                .ToList();

            // Get the authorized activities in all workflow types
            var authorizedActivityTypes = AuthorizedActivityTypes( allWorkflowTypes );

            // Get the workflow types that contain authorized activity types
            var workflowTypeIds = allWorkflowTypes
                .Where( w => w.ActivityTypes.Any( a => authorizedActivityTypes.Contains( a.Id ) ) )
                .Select( w => w.Id )
                .Distinct()
                .ToList();

            // Create variable for storing authorized types and the count of active form actions
            var workflowTypeCounts = new Dictionary<int, int>();

            List<Workflow> workflows = null;

            if ( RoleFilter.HasValue && RoleFilter.Value )
            {
                workflows = new WorkflowService( rockContext ).Queryable()
                    .Where( w =>
                        w.ActivatedDateTime.HasValue &&
                        !w.CompletedDateTime.HasValue &&
                        w.InitiatorPersonAlias.PersonId == personId )
                    .ToList();

                workflowTypeIds.ForEach( id =>
                    workflowTypeCounts.Add( id, workflows.Where( w => w.WorkflowTypeId == id ).Count() ) );
            }
            else
            {
                // Get all the active forms for any of the authorized activities
                var activeForms = new WorkflowActionService( rockContext ).Queryable( "ActionType.ActivityType.WorkflowType, Activity.Workflow" )
                    .Where( a =>
                        a.ActionType.WorkflowFormId.HasValue &&
                        !a.CompletedDateTime.HasValue &&
                        a.Activity.ActivatedDateTime.HasValue &&
                        !a.Activity.CompletedDateTime.HasValue &&
                        a.Activity.Workflow.ActivatedDateTime.HasValue &&
                        !a.Activity.Workflow.CompletedDateTime.HasValue &&
                        authorizedActivityTypes.Contains( a.ActionType.ActivityTypeId ) &&
                        (
                            ( a.Activity.AssignedPersonAlias != null && a.Activity.AssignedPersonAlias.PersonId == personId ) ||
                            ( a.Activity.AssignedGroup != null && a.Activity.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                        )
                    )
                    .ToList();

                // Get any workflow types that have authorized activites and get the form count
                workflowTypeIds.ForEach( w =>
                    workflowTypeCounts.Add( w, activeForms.Where( a => a.Activity.Workflow.WorkflowTypeId == w ).Count() ) );

                workflows = activeForms
                    .Select( a => a.Activity.Workflow )
                    .Distinct()
                    .ToList();
            }

            var displayedTypes = new List<WorkflowType>();
            foreach ( var workflowType in allWorkflowTypes.Where( w => workflowTypeCounts.Keys.Contains( w.Id ) ) )
            {
                if ( workflowTypeCounts[workflowType.Id] > 0 )
                {
                    // Always show any types that have active assignments assigned to user
                    displayedTypes.Add( workflowType );
                }
                else
                {
                    // If there are not any active assigned activities, and not filtering by active, then also
                    // show any types that user is authorized to edit
                    if ( ( !StatusFilter.HasValue || !StatusFilter.Value ) &&
                        workflowType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        displayedTypes.Add( workflowType );
                    }
                }
            }

            // Create a query to return workflow type, the count of active action forms, and the selected class
            var qry = displayedTypes
                .Select( w => new
                {
                    WorkflowType = w,
                    Count = workflowTypeCounts[w.Id],
                    Class = ( SelectedWorkflowTypeId.HasValue && SelectedWorkflowTypeId.Value == w.Id ) ? "active" : ""
                } );

            rptWorkflowTypes.DataSource = qry.ToList();
            rptWorkflowTypes.DataBind();

            WorkflowType selectedWorkflowType = null;
            if ( SelectedWorkflowTypeId.HasValue )
            {
                selectedWorkflowType = allWorkflowTypes
                    .Where( w => w.Id == SelectedWorkflowTypeId.Value )
                    .FirstOrDefault();

                AddAttributeColumns( selectedWorkflowType );
            }

            if ( selectedWorkflowType != null && workflowTypeCounts.Keys.Contains( selectedWorkflowType.Id ) )
            {
                var workflowQry = workflows.Where( w => w.WorkflowTypeId == selectedWorkflowType.Id ).AsQueryable();

                var sortProperty = gWorkflows.SortProperty;
                if ( sortProperty != null )
                {
                    gWorkflows.DataSource = workflowQry.Sort( sortProperty ).ToList();
                }
                else
                {
                    gWorkflows.DataSource = workflowQry.OrderByDescending( s => s.CreatedDateTime ).ToList();
                }

                gWorkflows.DataBind();
                gWorkflows.Visible = true;

                lWorkflow.Text = workflows.Where( w => w.WorkflowTypeId == selectedWorkflowType.Id ).Select( w => w.WorkflowType.Name ).FirstOrDefault() + " Workflows";
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

        protected void AddAttributeColumns( WorkflowType workflowType )
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
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

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