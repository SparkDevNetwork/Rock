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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow Detail" )]
    [Category( "WorkFlow" )]
    [Description( "Displays the details of a workflow instance." )]
    public partial class WorkflowDetail : RockBlock
    {

        #region Fields

        private bool _canEdit = false;
        private PersonAliasService _personAliasService = null;
        private GroupService _groupService = null;

        #endregion

        #region Properties

        private Rock.Model.Workflow Workflow { get; set; }
        private List<string> LogEntries { get; set; } 
        private List<Guid> ExpandedActivities { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["Workflow"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                Workflow = new Workflow();
            }
            else
            {
                Workflow = JsonConvert.DeserializeObject<Workflow>( json );
            }

            // Wire up type objects since they are not serialized
            var rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var activityTypeService = new WorkflowActivityTypeService( rockContext );
            var actionTypeService = new WorkflowActionTypeService( rockContext );
            Workflow.WorkflowType = workflowTypeService.Get( Workflow.WorkflowTypeId );
            foreach ( var activity in Workflow.Activities )
            {
                activity.ActivityType = activityTypeService.Get( activity.ActivityTypeId );
                foreach ( var action in activity.Actions )
                {
                    action.ActionType = actionTypeService.Get( action.ActionTypeId );
                }
            }

            // Add new log entries since they are not serialized
            LogEntries = ViewState["LogEntries"] as List<string>;
            if ( LogEntries == null )
            {
                LogEntries = new List<string>();
            }
            LogEntries.ForEach( l => Workflow.AddLogEntry( l ) );

            ExpandedActivities = ViewState["ExpandedActivities"] as List<Guid>;
            if (ExpandedActivities == null)
            {
                ExpandedActivities = new List<Guid>();
            }

            _canEdit = UserCanEdit || Workflow.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLog.DataKeyNames = new string[] { "Id" };
            gLog.Actions.ShowAdd = false;
            gLog.IsDeleteEnabled = false;
            gLog.GridRebind += gLog_GridRebind;

            rptrActivities.ItemDataBound += rptrActivities_ItemDataBound;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbNotAuthorized.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "workflowId" ).AsInteger() );
            }
            else
            {
                if (hfMode.Value == "Edit")
                {
                    BuildControls( false );
                }
                else
                {
                    ShowAttributeValues();
                }
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
            var jsonSetting = new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            if ( Workflow != null )
            {
                ViewState["Workflow"] = JsonConvert.SerializeObject( Workflow, Formatting.None, jsonSetting );
                ViewState["LogEntries"] = Workflow.GetUnsavedLogEntries().Select( l => l.LogText ).ToList();
                ViewState["ExpandedActivities"] = ExpandedActivities;
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            bool editMode = hfMode.Value == "Edit";

            liDetails.Visible = _canEdit;
            liActivities.Visible = _canEdit;
            liLog.Visible = _canEdit && !editMode;
            divLog.Visible = _canEdit && !editMode;

            pnlDetailsView.Visible = !editMode;
            pnlDetailsEdit.Visible = editMode;
            pnlActivitesView.Visible = !editMode;
            pnlActivitesEdit.Visible = editMode;

            string activeTab = hfActiveTab.Value;
            ShowHideTab( activeTab == "Details" || activeTab == string.Empty, liDetails );
            ShowHideTab( activeTab == "Details" || activeTab == string.Empty, divDetails );
            ShowHideTab( activeTab == "Activities", liActivities );
            ShowHideTab( activeTab == "Activities", divActivities );
            ShowHideTab( activeTab == "Log", liLog );
            ShowHideTab( activeTab == "Log", divLog );

            btnEdit.Visible = _canEdit && !editMode;
            btnSave.Visible = _canEdit && editMode;
        }

        #endregion

        #region Events

        #region Edit / Save / Cancel events

        protected void btnEdit_Click( object sender, EventArgs e )
        {
            if ( new List<string> { "Notes", "Log" }.Contains( hfActiveTab.Value ) )
            {
                hfActiveTab.Value = "Details";
            }

            ShowEditDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new WorkflowService( rockContext );

            ParseControls( rockContext, true );

            Workflow dbWorkflow = null;

            if ( Workflow != null )
            {
                dbWorkflow = service.Get( Workflow.Id );

                if ( dbWorkflow != null )
                {
                    if ( !dbWorkflow.CompletedDateTime.HasValue && Workflow.CompletedDateTime.HasValue )
                    {
                        dbWorkflow.AddLogEntry( "Workflow Manually Completed." );
                        dbWorkflow.CompletedDateTime = Workflow.CompletedDateTime;
                    }
                    else if ( dbWorkflow.CompletedDateTime.HasValue && !Workflow.CompletedDateTime.HasValue )
                    {
                        dbWorkflow.AddLogEntry( "Workflow Manually Re-Activated." );
                        dbWorkflow.CompletedDateTime = null;
                    }

                    if ( dbWorkflow.Name.Trim() != Workflow.Name.Trim() )
                    {
                        dbWorkflow.AddLogEntry( string.Format( "Workflow name manually changed from '{0}' to '{0}'.", dbWorkflow.Name, tbName.Text ) );
                        dbWorkflow.Name = Workflow.Name;
                    }

                    if ( dbWorkflow.Status.Trim() != Workflow.Status.Trim() )
                    {
                        dbWorkflow.AddLogEntry( string.Format( "Workflow status manually changed from '{0}' to '{0}'.", dbWorkflow.Status, tbStatus.Text ) );
                        dbWorkflow.Status = Workflow.Status;
                    }

                    if ( !dbWorkflow.InitiatorPersonAliasId.Equals(Workflow.InitiatorPersonAliasId))
                    {
                        dbWorkflow.AddLogEntry( string.Format( "Workflow status manually changed from '{0}' to '{0}'.",
                            dbWorkflow.InitiatorPersonAlias != null ? dbWorkflow.InitiatorPersonAlias.Person.FullName : "",
                            Workflow.InitiatorPersonAlias != null ? Workflow.InitiatorPersonAlias.Person.FullName : "" ) );
                        dbWorkflow.InitiatorPersonAlias = Workflow.InitiatorPersonAlias;
                        dbWorkflow.InitiatorPersonAliasId = Workflow.InitiatorPersonAliasId;
                    }

                    if ( !Page.IsValid || !dbWorkflow.IsValid )
                    {
                        return;
                    }

                    foreach ( var activity in Workflow.Activities )
                    {
                        if ( !activity.IsValid )
                        {
                            return;
                        }
                        foreach ( var action in activity.Actions )
                        {
                            if ( !action.IsValid )
                            {
                                return;
                            }
                        }
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        dbWorkflow.LoadAttributes( rockContext );
                        foreach ( var attributeValue in Workflow.AttributeValues )
                        {
                            dbWorkflow.SetAttributeValue( attributeValue.Key, Workflow.GetAttributeValue( attributeValue.Key ) );
                        }
                        dbWorkflow.SaveAttributeValues( rockContext );

                        WorkflowActivityService workflowActivityService = new WorkflowActivityService( rockContext );
                        WorkflowActionService workflowActionService = new WorkflowActionService( rockContext );

                        var activitiesInUi = new List<Guid>();
                        var actionsInUI = new List<Guid>();
                        foreach ( var activity in Workflow.Activities )
                        {
                            activitiesInUi.Add( activity.Guid );
                            foreach ( var action in activity.Actions )
                            {
                                actionsInUI.Add( action.Guid );
                            }
                        }

                        // delete WorkflowActions that were removed in the UI
                        foreach ( var action in workflowActionService.Queryable()
                            .Where( a => 
                                a.Activity.WorkflowId.Equals( dbWorkflow.Id ) &&
                                !actionsInUI.Contains( a.Guid ) ) )
                        {
                            workflowActionService.Delete( action );
                        }

                        // delete WorkflowActivities that aren't assigned in the UI anymore
                        foreach ( var activity in workflowActivityService.Queryable()
                            .Where( a => 
                                a.WorkflowId.Equals( dbWorkflow.Id ) &&
                                !activitiesInUi.Contains( a.Guid ) ) )
                        {
                            workflowActivityService.Delete( activity );
                        }

                        rockContext.SaveChanges();

                        // add or update WorkflowActivities(and Actions) that are assigned in the UI
                        foreach ( var editorWorkflowActivity in Workflow.Activities )
                        {
                            // Add or Update the activity type
                            WorkflowActivity workflowActivity = dbWorkflow.Activities.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActivity.Guid ) );
                            if ( workflowActivity == null )
                            {
                                workflowActivity = new WorkflowActivity();
                                workflowActivity.ActivityTypeId = editorWorkflowActivity.ActivityTypeId;
                                dbWorkflow.Activities.Add( workflowActivity );
                            }

                            workflowActivity.AssignedPersonAliasId = editorWorkflowActivity.AssignedPersonAliasId;
                            workflowActivity.AssignedGroupId = editorWorkflowActivity.AssignedGroupId;
                            workflowActivity.ActivatedDateTime = editorWorkflowActivity.ActivatedDateTime;
                            workflowActivity.LastProcessedDateTime = editorWorkflowActivity.LastProcessedDateTime;

                            if ( !workflowActivity.CompletedDateTime.HasValue && editorWorkflowActivity.CompletedDateTime.HasValue )
                            {
                                workflowActivity.AddLogEntry( "Activity Manually Completed." );
                                workflowActivity.CompletedDateTime = RockDateTime.Now;
                            }
                            if ( workflowActivity.CompletedDateTime.HasValue && !editorWorkflowActivity.CompletedDateTime.HasValue )
                            {
                                workflowActivity.AddLogEntry( "Activity Manually Re-Activated." );
                                workflowActivity.CompletedDateTime = null;
                            }


                            // Save Activity Type
                            rockContext.SaveChanges();

                            // Save ActivityType Attributes
                            workflowActivity.LoadAttributes( rockContext );
                            foreach ( var attributeValue in editorWorkflowActivity.AttributeValues )
                            {
                                workflowActivity.SetAttributeValue( attributeValue.Key, editorWorkflowActivity.GetAttributeValue( attributeValue.Key ) );
                            }
                            workflowActivity.SaveAttributeValues( rockContext );

                            foreach ( var editorWorkflowAction in editorWorkflowActivity.Actions )
                            {
                                WorkflowAction workflowAction = workflowActivity.Actions.FirstOrDefault( a => a.Guid.Equals( editorWorkflowAction.Guid ) );
                                if ( workflowAction == null )
                                {
                                    // New action
                                    workflowAction = new WorkflowAction();
                                    workflowAction.ActionTypeId = editorWorkflowAction.ActionTypeId;
                                    workflowActivity.Actions.Add( workflowAction );
                                }

                                workflowAction.LastProcessedDateTime = editorWorkflowAction.LastProcessedDateTime;
                                workflowAction.FormAction = editorWorkflowAction.FormAction;

                                if ( !workflowAction.CompletedDateTime.HasValue && editorWorkflowAction.CompletedDateTime.HasValue )
                                {
                                    workflowAction.AddLogEntry( "Action Manually Completed." );
                                    workflowAction.CompletedDateTime = RockDateTime.Now;
                                }
                                if ( workflowAction.CompletedDateTime.HasValue && !editorWorkflowAction.CompletedDateTime.HasValue )
                                {
                                    workflowAction.AddLogEntry( "Action Manually Re-Activated." );
                                    workflowAction.CompletedDateTime = null;
                                }
                            }

                            // Save action updates
                            rockContext.SaveChanges();

                        }

                    } );

                }

                Workflow = service
                    .Queryable("WorkflowType,Activities.ActivityType,Activities.Actions.ActionType")
                    .FirstOrDefault( w => w.Id == Workflow.Id );

                var errorMessages = new List<string>();
                service.Process( Workflow, out errorMessages );

            }

            ShowReadonlyDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( Workflow != null )
            {
                if ( hfMode.Value == "View" )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams["WorkflowTypeId"] = Workflow.WorkflowTypeId.ToString();
                    NavigateToParentPage( qryParams );
                }
                else
                {
                    ShowDetail( Workflow.Id );
                }
            }
        }

        #endregion

        void rptrActivities_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var activity = e.Item.DataItem as WorkflowActivity;
            if (activity != null)
            {
                var lblComplete = e.Item.FindControl( "lblComplete" ) as Label;
                if ( lblComplete != null )
                {
                    lblComplete.Visible = activity.CompletedDateTime.HasValue;
                }

                var lblActive = e.Item.FindControl( "lblActive" ) as Label;
                if ( lblActive != null )
                {
                    lblActive.Visible = !activity.CompletedDateTime.HasValue;
                }

                var tdAssignedToPerson = e.Item.FindControl( "tdAssignedToPerson" ) as TermDescription;
                if ( tdAssignedToPerson != null && activity.AssignedPersonAliasId.HasValue )
                {
                    var person = _personAliasService.Queryable()
                        .Where( a => a.Id == activity.AssignedPersonAliasId.Value )
                        .Select( a => a.Person )
                        .FirstOrDefault();
                    if ( person != null )
                    {
                        tdAssignedToPerson.Description = string.Format( "<a href='~/Person/{0}'>{1}</a>", person.Id, person.FullName );
                    }
                }

                var tdAssignedToGroup = e.Item.FindControl( "tdAssignedToGroup" ) as TermDescription;
                if ( tdAssignedToGroup != null && activity.AssignedGroupId.HasValue )
                {
                    var group = _groupService.Get( activity.AssignedGroupId.Value);
                    if ( group != null )
                    {
                        tdAssignedToGroup.Description = group.Name;
                    }
                }

                var tdActivated = e.Item.FindControl( "tdActivated" ) as TermDescription;
                if (tdActivated != null && activity.ActivatedDateTime.HasValue )
                {
                    tdActivated.Description = string.Format( "{0} {1} ({2})",
                        activity.ActivatedDateTime.Value.ToShortDateString(),
                        activity.ActivatedDateTime.Value.ToShortTimeString(),
                        activity.ActivatedDateTime.Value.ToRelativeDateString() );
                }

                var tdCompleted = e.Item.FindControl("tdCompleted") as TermDescription;
                if (tdCompleted != null && activity.CompletedDateTime.HasValue )
                {
                    tdCompleted.Description = string.Format( "{0} {1} ({2})",
                        activity.CompletedDateTime.Value.ToShortDateString(),
                        activity.CompletedDateTime.Value.ToShortTimeString(),
                        activity.CompletedDateTime.Value.ToRelativeDateString() );
                }

                var phActivityAttributes = e.Item.FindControl("phActivityAttributes") as PlaceHolder;
                if (phActivityAttributes != null )
                {
                    foreach ( var attribute in activity.Attributes.OrderBy( a => a.Value.Order ).Select( a => a.Value ) )
                    {
                        var td = new TermDescription();
                        phActivityAttributes.Controls.Add( td );
                        td.Term = attribute.Name;

                        string value = activity.GetAttributeValue( attribute.Key );

                        var field = attribute.FieldType.Field;
                        string formattedValue = field.FormatValueAsHtml( phActivityAttributes, value, attribute.QualifierValues );

                        if ( field is Rock.Field.ILinkableFieldType )
                        {
                            var linkableField = field as Rock.Field.ILinkableFieldType;
                            td.Description = string.Format( "<a href='{0}{1}'>{2}</a>",
                                ResolveRockUrl( "~" ), linkableField.UrlLink( value, attribute.QualifierValues ), formattedValue );
                        }
                        else
                        {
                            td.Description = formattedValue;
                        }
                        phActivityAttributes.Controls.Add( td );
                    }
                }

                var gridActions = e.Item.FindControl("gridActions") as Grid;
                if ( gridActions != null )
                {
                    gridActions.DataSource = activity.Actions.OrderBy( a => a.ActionType.Order )
                        .Select( a => new
                        {
                            Name = a.ActionType.Name,
                            LastProcessed = a.LastProcessedDateTime.HasValue ?
                                string.Format( "{0} {1} ({2})",
                                    a.LastProcessedDateTime.Value.ToShortDateString(),
                                    a.LastProcessedDateTime.Value.ToShortTimeString(),
                                    a.LastProcessedDateTime.Value.ToRelativeDateString() ) :
                                "",
                            Completed = a.CompletedDateTime.HasValue,
                            CompletedWhen = a.CompletedDateTime.HasValue ?
                                string.Format( "{0} {1} ({2})",
                                    a.CompletedDateTime.Value.ToShortDateString(),
                                    a.CompletedDateTime.Value.ToShortTimeString(),
                                    a.CompletedDateTime.Value.ToRelativeDateString() ) :
                                ""
                        } )
                        .ToList();
                    gridActions.DataBind();
                }
            }
        }

        #region Activity/Action Events

        /// <summary>
        /// Handles the DeleteActivityClick event of the workflowActivityEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void workflowActivityEditor_DeleteActivityClick( object sender, EventArgs e )
        {
            ParseControls();

            var activityEditor = sender as WorkflowActivityEditor;
            if ( activityEditor != null )
            {
                var activity = Workflow.Activities.Where( a => a.Guid == activityEditor.ActivityGuid ).FirstOrDefault();
                if ( activity != null )
                {
                    if ( ExpandedActivities.Contains( activity.Guid ) )
                    {
                        ExpandedActivities.Remove( activity.Guid );
                    }

                    Workflow.Activities.Remove( activity );
                }

                BuildControls( true );
            }
        }

        protected void ddlActivateNewActivity_SelectedIndexChanged( object sender, EventArgs e )
        {
            ParseControls();

            int? activityTypeId = ddlActivateNewActivity.SelectedValueAsId();
            if (activityTypeId.HasValue)
            {
                var activityType = new WorkflowActivityTypeService(new RockContext()).Get(activityTypeId.Value);
                if (activityType != null)
                {
                    var activity = WorkflowActivity.Activate( activityType, Workflow );
                    activity.ActivityTypeId = activity.ActivityType.Id;
                    activity.Guid = Guid.NewGuid();

                    foreach( var action in activity.Actions)
                    {
                        action.ActionTypeId = action.ActionType.Id;
                        action.Guid = Guid.NewGuid();
                    }

                    Workflow.AddLogEntry( string.Format( "Manually Activated new '{0}' activity", activityType.ToString() ) );

                    ExpandedActivities.Add( activity.Guid ); 

                    BuildControls( true, activity.Guid );
                }
            }

            ddlActivateNewActivity.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the GridRebind event of the glog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLog_GridRebind( object sender, EventArgs e )
        {
            BindLog();
        }

        #endregion

        #endregion

        #region Methods

        #region Show Details

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail( int workflowId )
        {
            var rockContext = new RockContext();

            Workflow = new WorkflowService( rockContext )
                    .Queryable( "WorkflowType, Activities")
                    .Where( w => w.Id == workflowId )
                    .FirstOrDefault();

            if ( Workflow == null )
            {
                pnlContent.Visible = false;
                return;
            }

            _canEdit = UserCanEdit || Workflow.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );

            Workflow.LoadAttributes( rockContext );
            foreach ( var activity in Workflow.Activities )
            {
                activity.LoadAttributes();
            }

            lReadOnlyTitle.Text = Workflow.Name.FormatAsHtmlTitle();
            if ( Workflow.CompletedDateTime.HasValue )
            {
                hlState.LabelType = LabelType.Default;
                hlState.Text = "Complete";
            }
            else
            {
                hlState.LabelType = LabelType.Success;
                hlState.Text = "Active";
            }
            hlType.Text = Workflow.WorkflowType.Name;

            ShowReadonlyDetails();
        }

        private void ShowReadonlyDetails()
        {
            hfMode.Value = "View";

            if ( Workflow != null )
            {
                if ( _canEdit || Workflow.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    tdName.Description = Workflow.Name;
                    tdStatus.Description = Workflow.Status;

                    if ( Workflow.InitiatorPersonAlias != null && Workflow.InitiatorPersonAlias.Person != null )
                    {
                        var person = Workflow.InitiatorPersonAlias.Person;
                        tdInitiator.Description = string.Format( "<a href='{0}{1}'>{2}</a>", ResolveRockUrl("~/Person/"), person.Id, person.FullName );
                    }
                    else
                    {
                        tdInitiator.Description = string.Empty;
                    }

                    if ( Workflow.ActivatedDateTime.HasValue )
                    {
                        tdActivatedWhen.Description = string.Format( "{0} {1} ({2})",
                            Workflow.ActivatedDateTime.Value.ToShortDateString(),
                            Workflow.ActivatedDateTime.Value.ToShortTimeString(),
                            Workflow.ActivatedDateTime.Value.ToRelativeDateString() );
                    }

                    if ( Workflow.LastProcessedDateTime.HasValue )
                    {
                        tdLastProcessed.Description = string.Format( "{0} {1} ({2})",
                            Workflow.LastProcessedDateTime.Value.ToShortDateString(),
                            Workflow.LastProcessedDateTime.Value.ToShortTimeString(),
                            Workflow.LastProcessedDateTime.Value.ToRelativeDateString() );
                    }

                    if ( Workflow.CompletedDateTime.HasValue )
                    {
                        tdCompletedWhen.Description = string.Format( "{0} {1} ({2})",
                            Workflow.CompletedDateTime.Value.ToShortDateString(),
                            Workflow.CompletedDateTime.Value.ToShortTimeString(),
                            Workflow.CompletedDateTime.Value.ToRelativeDateString() );
                    }

                    ShowAttributeValues();

                    var rockContext = new RockContext();
                    _personAliasService = new PersonAliasService( rockContext );
                    _groupService = new GroupService( rockContext );
                    rptrActivities.DataSource = Workflow.Activities.OrderBy( a => a.ActivatedDateTime ).ToList();
                    rptrActivities.DataBind();

                    BindLog();
                }
                else
                {
                    nbNotAuthorized.Visible = true;
                    pnlContent.Visible = false;
                }
            }

            HideSecondaryBlocks( false );
        }

        private void ShowEditDetails()
        {
            hfMode.Value = "Edit";

            if ( _canEdit )
            {
                ddlActivateNewActivity.Items.Clear();
                ddlActivateNewActivity.Items.Add( new ListItem( "Activate New Activity", "0" ) );
                foreach ( var activityType in Workflow.WorkflowType.ActivityTypes.OrderBy( a => a.Order ) )
                {
                    ddlActivateNewActivity.Items.Add( new ListItem( activityType.Name, activityType.Id.ToString() ) );
                }
            }

            ExpandedActivities = new List<Guid>();

            tbName.Text = Workflow.Name;
            tbStatus.Text = Workflow.Status;

            if ( Workflow.InitiatorPersonAlias != null && Workflow.InitiatorPersonAlias.Person != null)
            {
                ppInitiator.SetValue( Workflow.InitiatorPersonAlias.Person );
            }
            else
            {
                ppInitiator.SetValue( null );
            }

            cbIsCompleted.Checked = Workflow.CompletedDateTime.HasValue;

            var sbState = new StringBuilder();
            if ( Workflow.ActivatedDateTime.HasValue )
            {
                sbState.AppendFormat( "<strong>Activated:</strong> {0} {1} ({2})<br/>",
                    Workflow.ActivatedDateTime.Value.ToShortDateString(),
                    Workflow.ActivatedDateTime.Value.ToShortTimeString(),
                    Workflow.ActivatedDateTime.Value.ToRelativeDateString() );
            }
            if ( Workflow.CompletedDateTime.HasValue )
            {
                sbState.AppendFormat( "<strong>Completed:</strong> {0} {1} ({2})",
                    Workflow.CompletedDateTime.Value.ToShortDateString(),
                    Workflow.CompletedDateTime.Value.ToShortTimeString(),
                    Workflow.CompletedDateTime.Value.ToRelativeDateString() );
            }
            lState.Text = sbState.ToString();

            BuildControls( true );

            HideSecondaryBlocks( true );
        }

        private void ShowAttributeValues()
        {
            phViewAttributes.Controls.Clear();
            foreach ( var attribute in Workflow.Attributes.OrderBy( a => a.Value.Order ).Select( a => a.Value ) )
            {
                var td = new TermDescription();
                td.ID = "tdViewAttribute_" + attribute.Key;
                td.Term = attribute.Name;

                string value = Workflow.GetAttributeValue( attribute.Key );

                var field = attribute.FieldType.Field;
                string formattedValue = field.FormatValueAsHtml( phViewAttributes, value, attribute.QualifierValues );

                if ( field is Rock.Field.ILinkableFieldType )
                {
                    var linkableField = field as Rock.Field.ILinkableFieldType;
                    td.Description = string.Format( "<a href='{0}{1}'>{2}</a>",
                        ResolveRockUrl( "~" ), linkableField.UrlLink( value, attribute.QualifierValues ), formattedValue );
                }
                else
                {
                    td.Description = formattedValue;
                }
                phViewAttributes.Controls.Add( td );
            }
        }

        private void BindLog()
        {
            var logEntries = new WorkflowLogService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person" )
                .Where( l => l.WorkflowId == Workflow.Id )
                .OrderBy( l => l.LogDateTime)
                .ToList();

            gLog.DataSource = logEntries;
            gLog.DataBind();
        }

        private void ShowHideTab( bool show, System.Web.UI.HtmlControls.HtmlGenericControl control )
        {
            if ( show )
            {
                control.AddCssClass( "active" );
            }
            else
            {
                control.RemoveCssClass( "active" );
            }
        }

        #endregion

        #region Build/Parse controls

        /// <summary>
        /// Builds the controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="activeActivityGuid">The active activity unique identifier.</param>
        private void BuildControls( bool setValues = false, Guid? activeActivityGuid = null, bool showInvalid = false )
        {
            phAttributes.Controls.Clear();
            if ( _canEdit )
            {
                Helper.AddEditControls( Workflow, phAttributes, setValues, btnSave.ValidationGroup );
            }
            else
            {
                Helper.AddDisplayControls( Workflow, phAttributes );
            }

            var rockContext = new RockContext();

            phActivities.Controls.Clear();
            foreach ( var activity in Workflow.Activities.OrderBy( a => a.ActivatedDateTime ) )
            {
                var activityEditor = new WorkflowActivityEditor();
                activityEditor.ID = "WorkflowActivityEditor_" + activity.Guid.ToString( "N" );
                phActivities.Controls.Add( activityEditor );
                activityEditor.CanEdit = _canEdit;
                activityEditor.ValidationGroup = btnSave.ValidationGroup;
                activityEditor.IsDeleteEnabled = !activity.LastProcessedDateTime.HasValue;
                activityEditor.DeleteActivityTypeClick += workflowActivityEditor_DeleteActivityClick;
                activityEditor.SetWorkflowActivity( activity, rockContext, setValues);

                foreach ( WorkflowAction action in activity.Actions.OrderBy( a => a.ActionType.Order ) )
                {
                    var actionEditor = new WorkflowActionEditor();
                    actionEditor.ID = "WorkflowActionEditor_" + action.Guid.ToString( "N" );
                    activityEditor.Controls.Add( actionEditor );
                    actionEditor.CanEdit = _canEdit;
                    actionEditor.ValidationGroup = activityEditor.ValidationGroup;
                    actionEditor.SetWorkflowAction( action, setValues );
                }

                if ( _canEdit && setValues )
                {
                    activityEditor.Expanded = ExpandedActivities.Contains( activity.Guid );

                    if ( !activityEditor.Expanded && showInvalid && !activity.IsValid )
                    {
                        activityEditor.Expanded = true;
                    }

                    if ( !activityEditor.Expanded )
                    {
                        activityEditor.Expanded = activeActivityGuid.HasValue && activeActivityGuid.Equals( activity.Guid );
                    }
                }

            }
        }

        /// <summary>
        /// Parses the controls.
        /// </summary>
        private void ParseControls( RockContext rockContext = null, bool expandInvalid = false )
        {
            if (rockContext == null)
            {
                rockContext = new RockContext();
            }

            if ( Workflow.CompletedDateTime.HasValue && !cbIsCompleted.Checked )
            {
                Workflow.CompletedDateTime = null;
            }
            else if ( !Workflow.CompletedDateTime.HasValue && cbIsCompleted.Checked )
            {
                Workflow.CompletedDateTime = RockDateTime.Now;
            }

            Workflow.Name = tbName.Text;
            Workflow.Status = tbStatus.Text;

            int? initiatorPersonId = ppInitiator.PersonId;
            if ( initiatorPersonId.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).GetByAliasId( initiatorPersonId.Value );
                if ( personAlias != null )
                {
                    Workflow.InitiatorPersonAlias = personAlias;
                    Workflow.InitiatorPersonAliasId = personAlias.Id;
                }
                else
                {
                    Workflow.InitiatorPersonAlias = null;
                    Workflow.InitiatorPersonAliasId = null;
                }
            }

            Helper.GetEditValues( phAttributes, Workflow );

            ExpandedActivities = new List<Guid>();

            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                var activity = Workflow.Activities.Where( a => a.Guid.Equals(activityEditor.ActivityGuid)).FirstOrDefault();
                if (activity != null)
                {
                    activityEditor.GetWorkflowActivity(activity, expandInvalid);
                }

                if (activityEditor.Expanded)
                {
                    ExpandedActivities.Add( activity.Guid );
                }
            }
        }


        #endregion

        #endregion

}
}