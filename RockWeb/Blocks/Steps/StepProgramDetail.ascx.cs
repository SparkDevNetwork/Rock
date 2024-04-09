// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.SystemGuid;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Program Detail" )]
    [Category( "Steps" )]
    [Description( "Displays the details of the given Step Program for editing." )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [BooleanField
        ( "Show Chart",
          Key = AttributeKey.ShowChart,
          DefaultValue = "true",
          Order = 0 )]

    [SlidingDateRangeField
        ( "Default Chart Date Range",
          Key = AttributeKey.SlidingDateRange,
          DefaultValue = "Current||Year||",
          EnabledSlidingDateRangeTypes = "Last,Previous,Current,DateRange",
          Order = 1 )]

    [CodeEditorField(
        "Key Performance Indicator Lava",
        IsRequired = false,
        DefaultValue = DefaultValue.KpiLava,
        Key = AttributeKey.KpiLava,
        EditorMode = CodeEditorMode.Lava,
        Description = "The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>",
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34" )]
    public partial class StepProgramDetail : ContextEntityBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ShowChart = "Show Chart";
            public const string SlidingDateRange = "SlidingDateRange";
            public const string KpiLava = "KpiLava";
        }

        /// <summary>
        /// Default Attribute Values
        /// </summary>
        private static class DefaultValue
        {
            /// <summary>
            /// The kpi lava
            /// </summary>
            public const string KpiLava =
@"{[kpis style:'card' iconbackground:'true']}
  [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing Program' color:'blue-700']][[ endkpi ]]
  [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete Program' color:'green-600']][[ endkpi ]]
  [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
  [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string StepProgramId = "ProgramId";
        }

        #endregion Page Parameter Keys

        #region Properties

        private List<StepStatus> StatusesState { get; set; }

        private List<StepWorkflowTriggerViewModel> WorkflowsState { get; set; }

        #endregion

        #region Private Variables

        private StepProgram _program = null;
        private int _stepProgramId = 0;

        #endregion Private Variables

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            LoadAttributesViewState();

            string json;

            json = ViewState["StatusesState"] as string ?? string.Empty;

            this.StatusesState = JsonConvert.DeserializeObject<List<StepStatus>>( json );

            this.StatusesState = this.StatusesState ?? new List<StepStatus>();

            json = ViewState["WorkflowsState"] as string ?? string.Empty;

            this.WorkflowsState = JsonConvert.DeserializeObject<List<StepWorkflowTriggerViewModel>>( json ) ?? new List<StepWorkflowTriggerViewModel>();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            // Create a custom contract resolver to specifically ignore some complex properties that add significant amounts of unnecessary ViewState data.
            var resolver = new Rock.Utility.DynamicPropertyMapContractResolver();

            resolver.IgnoreProperty( typeof( StepStatus ), "StepProgram", "Steps", "UrlEncodedKey" );

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = resolver,
            };

            ViewState["StatusesState"] = JsonConvert.SerializeObject( StatusesState, Formatting.None, jsonSetting );
            ViewState["WorkflowsState"] = JsonConvert.SerializeObject( WorkflowsState, Formatting.None, jsonSetting );

            SaveAttributesViewState( jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeBlockNotification( nbBlockStatus, pnlDetails );
            InitializeBlockContext();
            InitializeStatusesGrid();
            InitializeWorkflowsGrid();
            InitializeActionButtons();
            InitializeChartScripts();
            InitializeChartFilter();
            InitializeSettingsNotification( upStepProgram );
            hlStepFlow.NavigateUrl = ResolveUrl( string.Format( "~//steps/program/{0}/flow", _stepProgramId ) );

            var editAllowed = IsUserAuthorized( Authorization.EDIT );
            if ( !editAllowed && _program != null )
            {
                editAllowed = _program.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            InitializeAttributesGrid( editAllowed );
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
                var stepProgramId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();
                ShowDetail( stepProgramId );
            }
            else
            {
                RefreshChart();
                RefreshKpi();
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? stepProgramId = PageParameter( pageReference, PageParameterKey.StepProgramId ).AsIntegerOrNull();
            if ( stepProgramId != null )
            {
                var dataContext = GetDataContext();

                var stepProgram = new StepProgramService( dataContext ).Get( stepProgramId.Value );

                if ( stepProgram != null )
                {
                    breadCrumbs.Add( new BreadCrumb( stepProgram.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Step Program", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the Statuses grid.
        /// </summary>
        private void InitializeStatusesGrid()
        {
            gStatuses.DataKeyNames = new string[] { "Guid" };
            gStatuses.Actions.ShowAdd = true;
            gStatuses.Actions.AddClick += gStatuses_Add;
            gStatuses.GridRebind += gStatuses_GridRebind;
            gStatuses.GridReorder += gStatuses_GridReorder;
        }

        /// <summary>
        /// Initialize the Workflows grid.
        /// </summary>
        private void InitializeWorkflowsGrid()
        {
            gWorkflows.DataKeyNames = new string[] { "Guid" };
            gWorkflows.Actions.ShowAdd = true;
            gWorkflows.Actions.AddClick += gWorkflows_Add;
            gWorkflows.GridRebind += gWorkflows_GridRebind;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'All associated Step Types and Step Participants will also be deleted!');", StepProgram.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.StepProgram ) ).Id;
        }

        /// <summary>
        /// Set the initial value of controls
        /// </summary>
        private void IntializeChartFilter()
        {
            // Set the default Date Range from the block settings.
            drpSlidingDateRange.DelimitedValues = GetAttributeValue( AttributeKey.SlidingDateRange ) ?? "-1||";
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            this.BlockUpdated += Block_BlockUpdated;

            AddConfigurationUpdateTrigger( triggerPanel );
        }

        #endregion

        #region Events

        #region Control Events

        protected void btnRefreshChart_Click( object sender, EventArgs e )
        {
            RefreshChart();
            RefreshKpi();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            StartEditMode();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            CancelEditMode();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            this.DeleteRecord();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        #endregion

        #region StepStatus Events

        /// <summary>
        /// Handles the Delete event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatuses_Delete( object sender, RowEventArgs e )
        {
            var rowGuid = ( Guid ) e.RowKeyValue;

            StatusesState.RemoveEntity( rowGuid );

            BindStepStatusesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnAddStepStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddStepStatus_Click( object sender, EventArgs e )
        {
            StepStatus stepStatus = null;

            var guid = hfStepProgramAddStepStatusGuid.Value.AsGuid();

            if ( !guid.IsEmpty() )
            {
                stepStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( stepStatus == null )
            {
                stepStatus = new StepStatus();
            }

            stepStatus.Name = tbStepStatusName.Text;
            stepStatus.IsActive = cbIsActive.Checked;
            stepStatus.IsCompleteStatus = cbIsCompleted.Checked;
            stepStatus.StatusColor = cpStatus.Text;

            if ( !stepStatus.IsValid )
            {
                return;
            }

            if ( StatusesState.Any( a => a.Guid.Equals( stepStatus.Guid ) ) )
            {
                StatusesState.RemoveEntity( stepStatus.Guid );
            }

            StatusesState.Add( stepStatus );
            BindStepStatusesGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatuses_GridRebind( object sender, EventArgs e )
        {
            BindStepStatusesGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        protected void gStatuses_GridReorder( object sender, GridReorderEventArgs e )
        {
            var movedItem = StatusesState.Where( ss => ss.Order == e.OldIndex ).FirstOrDefault();

            if ( movedItem != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in StatusesState.Where( ss => ss.Order < e.OldIndex && ss.Order >= e.NewIndex ) )
                    {
                        otherItem.Order++;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in StatusesState.Where( ss => ss.Order > e.OldIndex && ss.Order <= e.NewIndex ) )
                    {
                        otherItem.Order--;
                    }
                }

                movedItem.Order = e.NewIndex;
            }

            BindStepStatusesGrid();
        }

        /// <summar>ymod
        /// Handles the Add event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatuses_Add( object sender, EventArgs e )
        {
            gStatuses_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatuses_Edit( object sender, RowEventArgs e )
        {
            Guid stepStatusGuid = ( Guid ) e.RowKeyValue;
            gStatuses_ShowEdit( stepStatusGuid );
        }

        /// <summary>
        /// Shows the edit dialog for the specified Step Status.
        /// </summary>
        /// <param name="stepStatusGuid">The step status unique identifier.</param>
        protected void gStatuses_ShowEdit( Guid stepStatusGuid )
        {
            var stepStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( stepStatusGuid ) );

            if ( stepStatus != null )
            {
                tbStepStatusName.Text = stepStatus.Name;
                cbIsActive.Checked = stepStatus.IsActive;
                cpStatus.Value = stepStatus.StatusColor;
                cbIsCompleted.Checked = stepStatus.IsCompleteStatus;
            }
            else
            {
                tbStepStatusName.Text = string.Empty;
                cbIsActive.Checked = true;
                cbIsCompleted.Checked = false;
            }

            hfStepProgramAddStepStatusGuid.Value = stepStatusGuid.ToString();

            ShowDialog( "StepStatuses", true );
        }

        /// <summary>
        /// Binds the step statuses grid.
        /// </summary>
        private void BindStepStatusesGrid()
        {
            SetStepStatusStateOrder();

            gStatuses.DataSource = StatusesState;

            gStatuses.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetStepStatusStateOrder()
        {
            if ( StatusesState != null )
            {
                if ( StatusesState.Any() )
                {
                    StatusesState = StatusesState.OrderBy( ss => ss.Order ).ThenBy( ss => ss.Name ).ToList();

                    for ( var i = 0; i < StatusesState.Count; i++ )
                    {
                        StatusesState[i].Order = i;
                    }

                    SaveViewState();
                }
            }
        }

        #endregion

        #region StepWorkflow Events

        /// <summary>
        /// Handles the SaveClick event of the dlgStepWorkflow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgStepWorkflow_SaveClick( object sender, EventArgs e )
        {
            SaveWorkflowProperties();
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;

            var workflowTypeStateObj = WorkflowsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( workflowTypeStateObj != null )
            {
                WorkflowsState.Remove( workflowTypeStateObj );
            }

            BindStepWorkflowsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindStepWorkflowsGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            Guid stepWorkflowGuid = ( Guid ) e.RowKeyValue;
            gWorkflows_ShowEdit( stepWorkflowGuid );
        }

        /// <summary>
        /// Show the edit dialog for the specified Workflow Trigger.
        /// </summary>
        /// <param name="triggerGuid">The workflow trigger unique identifier.</param>
        protected void gWorkflows_ShowEdit( Guid triggerGuid )
        {
            ShowWorkflowTriggerPropertiesDialog( triggerGuid );
        }

        /// <summary>
        /// Handles the Add event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflows_Add( object sender, EventArgs e )
        {
            gWorkflows_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTriggerType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTriggerType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateTriggerQualifiers();
        }

        /// <summary>
        /// Show the edit dialog for the specified Workflow Trigger.
        /// </summary>
        /// <param name="triggerGuid">The workflow trigger unique identifier.</param>
        private void ShowWorkflowTriggerPropertiesDialog( Guid triggerGuid )
        {
            var workflowTrigger = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( triggerGuid ) );

            if ( workflowTrigger != null )
            {
                wpWorkflowType.SetValue( workflowTrigger.WorkflowTypeId );
                ddlTriggerType.SelectedValue = workflowTrigger.TriggerType.ToString();
            }
            else
            {
                // Set default values
                wpWorkflowType.SetValue( null );
                ddlTriggerType.SelectedValue = StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete.ToString();
            }

            hfAddStepWorkflowGuid.Value = triggerGuid.ToString();
            ShowDialog( "StepWorkflows", true );
            UpdateTriggerQualifiers();
        }

        /// <summary>
        /// Updates the trigger qualifiers.
        /// </summary>
        private void UpdateTriggerQualifiers()
        {
            var dataContext = this.GetDataContext();

            var workflowTrigger = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfAddStepWorkflowGuid.Value.AsGuid() ) );

            var sStepWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();

            if ( sStepWorkflowTriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                // Populate the selection lists for "To Status" and "From Status".
                var stepProgram = GetStepProgram();

                var statusList = new StepStatusService( dataContext ).Queryable().Where( s => s.StepProgramId == stepProgram.Id ).ToList();

                ddlPrimaryQualifier.Label = "From";
                ddlPrimaryQualifier.Visible = true;
                ddlPrimaryQualifier.Items.Clear();
                ddlPrimaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var status in statusList )
                {
                    ddlPrimaryQualifier.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                }

                ddlSecondaryQualifier.Label = "To";
                ddlSecondaryQualifier.Visible = true;
                ddlSecondaryQualifier.Items.Clear();
                ddlSecondaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var status in statusList )
                {
                    ddlSecondaryQualifier.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                }
            }
            else
            {
                ddlPrimaryQualifier.Visible = false;
                ddlPrimaryQualifier.Items.Clear();
                ddlSecondaryQualifier.Visible = false;
                ddlSecondaryQualifier.Items.Clear();
            }

            // Set the qualifier values.
            if ( workflowTrigger != null )
            {
                if ( workflowTrigger.TriggerType == sStepWorkflowTriggerType )
                {
                    var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings( workflowTrigger.TypeQualifier );

                    ddlPrimaryQualifier.SelectedValue = qualifierSettings.FromStatusId.ToStringSafe();
                    ddlSecondaryQualifier.SelectedValue = qualifierSettings.ToStatusId.ToStringSafe();
                }
            }
        }

        /// <summary>
        /// Binds the workflow triggers grid.
        /// </summary>
        private void BindStepWorkflowsGrid()
        {
            if ( WorkflowsState != null )
            {
                SetStepWorkflowListOrder( WorkflowsState );

                // Set the description for the trigger.
                var dataContext = GetDataContext();

                var stepService = new StepWorkflowTriggerService( dataContext );

                foreach ( var workflowTrigger in WorkflowsState )
                {
                    var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings( workflowTrigger.TypeQualifier );

                    workflowTrigger.TriggerDescription = stepService.GetTriggerSettingsDescription( workflowTrigger.TriggerType, qualifierSettings );
                }

                gWorkflows.DataSource = WorkflowsState;
            }

            gWorkflows.DataBind();
        }

        /// <summary>
        /// Sets the workflow triggers list order.
        /// </summary>
        /// <param name="stepWorkflowList">The workflow trigger list.</param>
        private void SetStepWorkflowListOrder( List<StepWorkflowTriggerViewModel> stepWorkflowList )
        {
            if ( stepWorkflowList != null )
            {
                if ( stepWorkflowList.Any() )
                {
                    stepWorkflowList.OrderBy( c => c.WorkflowTypeName ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
            }
        }

        /// <summary>
        /// Populate the Workflow Trigger Type selection list.
        /// </summary>
        private void LoadWorkflowTriggerTypesSelectionList()
        {
            ddlTriggerType.Items.Add( new ListItem( "Step Completed", StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete.ToString() ) );
            ddlTriggerType.Items.Add( new ListItem( "Status Changed", StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged.ToString() ) );
            ddlTriggerType.Items.Add( new ListItem( "Manual", StepWorkflowTrigger.WorkflowTriggerCondition.Manual.ToString() ) );
        }

        /// <summary>
        /// Save changes to the Workflow Trigger currently displayed in the Workflow properties dialog.
        /// </summary>
        private void SaveWorkflowProperties()
        {
            StepWorkflowTriggerViewModel workflowTrigger = null;

            var guid = hfAddStepWorkflowGuid.Value.AsGuid();

            if ( !guid.IsEmpty() )
            {
                workflowTrigger = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( workflowTrigger == null )
            {
                workflowTrigger = new StepWorkflowTriggerViewModel();

                workflowTrigger.Guid = Guid.NewGuid();

                WorkflowsState.Add( workflowTrigger );
            }

            workflowTrigger.WorkflowTypeId = wpWorkflowType.SelectedValueAsId().Value;
            workflowTrigger.TriggerType = ddlTriggerType.SelectedValueAsEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();

            var qualifierSettings = new StepWorkflowTrigger.StatusChangeTriggerSettings
            {
                FromStatusId = ddlPrimaryQualifier.SelectedValue.AsIntegerOrNull(),
                ToStatusId = ddlSecondaryQualifier.SelectedValue.AsIntegerOrNull()
            };

            workflowTrigger.TypeQualifier = qualifierSettings.ToSelectionString();

            var dataContext = GetDataContext();

            var workflowTypeService = new WorkflowTypeService( dataContext );

            var workflowTypeId = wpWorkflowType.SelectedValueAsId().GetValueOrDefault( 0 );

            var workflowType = workflowTypeService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == workflowTypeId );

            workflowTrigger.WorkflowTypeName = ( workflowType == null ) ? "(Unknown)" : workflowType.Name;

            BindStepWorkflowsGrid();

            HideDialog();
        }

        #endregion

        #endregion

        #region Data Context

        private RockContext _dataContext;

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Cancel edit mode and return to read-only mode.
        /// </summary>
        private void CancelEditMode()
        {
            if ( hfStepProgramId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetStepProgram() );
            }
        }

        /// <summary>
        /// Enter record edit mode.
        /// </summary>
        private void StartEditMode()
        {
            var stepProgram = GetStepProgram();

            ShowEditDetails( stepProgram );
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var rockContext = GetDataContext();

            var stepProgramService = new StepProgramService( rockContext );
            var authService = new AuthService( rockContext );

            var stepProgram = GetStepProgram( null, rockContext );

            if ( stepProgram != null )
            {
                // Earlier only Person with admin rights were allowed edit the block.That was changed to look for Edit after the Parent Authority for Step Type and Program is set.
                if ( !stepProgram.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var stepTypes = stepProgram.StepTypes.ToList();
                var stepTypeService = new StepTypeService( rockContext );

                foreach ( var stepType in stepTypes )
                {
                    string errorMessageStepType;
                    if ( !stepTypeService.CanDelete( stepType, out errorMessageStepType ) )
                    {
                        mdDeleteWarning.Show( errorMessageStepType, ModalAlertType.Information );
                        return;
                    }

                    stepTypeService.Delete( stepType );
                }

                rockContext.SaveChanges();

                string errorMessage;
                if ( !stepProgramService.CanDelete( stepProgram, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                stepProgramService.Delete( stepProgram );
                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            StepProgram stepProgram;

            var rockContext = GetDataContext();

            var stepProgramService = new StepProgramService( rockContext );
            var stepStatusService = new StepStatusService( rockContext );
            var stepWorkflowService = new StepWorkflowService( rockContext );
            var stepWorkflowTriggerService = new StepWorkflowTriggerService( rockContext );

            int stepProgramId = int.Parse( hfStepProgramId.Value );

            if ( stepProgramId == 0 )
            {
                stepProgram = new StepProgram();

                stepProgramService.Add( stepProgram );
            }
            else
            {
                stepProgram = stepProgramService.Queryable()
                                                .Include( x => x.StepTypes )
                                                .Include( x => x.StepStatuses )
                                                .Include( x => x.StepWorkflowTriggers )
                                                .Where( c => c.Id == stepProgramId )
                                                .FirstOrDefault();
            }

            // Step Statuses: Remove deleted Statuses
            var uiStatuses = StatusesState.Select( r => r.Guid );

            var deletedStatuses = stepProgram.StepStatuses.Where( r => !uiStatuses.Contains( r.Guid ) ).ToList();

            foreach ( var stepStatus in deletedStatuses )
            {
                stepProgram.StepStatuses.Remove( stepStatus );
                stepStatusService.Delete( stepStatus );
            }

            // Step Statuses: Update modified Statuses
            foreach ( var stepStatusState in this.StatusesState )
            {
                var stepStatus = stepProgram.StepStatuses.Where( a => a.Guid == stepStatusState.Guid ).FirstOrDefault();

                if ( stepStatus == null )
                {
                    stepStatus = new StepStatus();
                    stepProgram.StepStatuses.Add( stepStatus );
                }

                stepStatus.CopyPropertiesFrom( stepStatusState );
                stepStatus.StepProgramId = stepProgram.Id;
            }

            // Workflow Triggers: Remove deleted triggers.
            // Note that we need to be careful not to remove triggers related to a specific Step Type here, because they are managed separately in the Step Type Detail block.
            var uiWorkflows = WorkflowsState.Select( l => l.Guid );

            var deletedTriggers = stepProgram.StepWorkflowTriggers.Where( l => l.StepTypeId == null && !uiWorkflows.Contains( l.Guid ) ).ToList();

            foreach ( var trigger in deletedTriggers )
            {
                // Remove the Step workflows associated with this trigger.
                var stepWorkflows = stepWorkflowService.Queryable().Where( w => w.StepWorkflowTriggerId == trigger.Id );

                foreach ( var requestWorkflow in stepWorkflows )
                {
                    stepWorkflowService.Delete( requestWorkflow );
                }

                // Remove the trigger.
                stepProgram.StepWorkflowTriggers.Remove( trigger );

                stepWorkflowTriggerService.Delete( trigger );
            }

            // Workflow Triggers: Update modified triggers.
            foreach ( var stateTrigger in WorkflowsState )
            {
                var workflowTrigger = stepProgram.StepWorkflowTriggers.Where( a => a.Guid == stateTrigger.Guid ).FirstOrDefault();

                if ( workflowTrigger == null )
                {
                    workflowTrigger = new StepWorkflowTrigger();

                    workflowTrigger.StepProgramId = stepProgramId;

                    stepProgram.StepWorkflowTriggers.Add( workflowTrigger );
                }

                workflowTrigger.Guid = stateTrigger.Guid;
                workflowTrigger.WorkflowTypeId = stateTrigger.WorkflowTypeId;
                workflowTrigger.TriggerType = stateTrigger.TriggerType;
                workflowTrigger.TypeQualifier = stateTrigger.TypeQualifier;
                workflowTrigger.WorkflowName = stateTrigger.WorkflowTypeName;
                workflowTrigger.StepTypeId = null;
            }

            // Update Basic properties
            stepProgram.Name = tbName.Text;
            stepProgram.IsActive = cbActive.Checked;
            stepProgram.Description = tbDescription.Text;
            stepProgram.IconCssClass = tbIconCssClass.Text;

            stepProgram.CategoryId = cpCategory.SelectedValueAsInt();

            stepProgram.DefaultListView = rblDefaultListView.SelectedValue.ConvertToEnum<StepProgram.ViewMode>( StepProgram.ViewMode.Cards );

            if ( !stepProgram.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                rockContext.SaveChanges();

                Helper.SaveAttributeEdits( AttributesState, new StepType().TypeId, "StepProgramId", stepProgram.Id.ToString(), rockContext );

                stepProgram = stepProgramService.Get( stepProgram.Id );

                if ( stepProgram == null )
                {
                    throw new Exception( "This record is no longer valid, please reload your data." );
                }

                if ( !stepProgram.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    stepProgram.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !stepProgram.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    stepProgram.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !stepProgram.IsAuthorized( Authorization.MANAGE_STEPS, CurrentPerson ) )
                {
                    stepProgram.AllowPerson( Authorization.MANAGE_STEPS, CurrentPerson, rockContext );
                }

                if ( !stepProgram.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    stepProgram.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }
            }
            catch ( Exception ex )
            {
                ShowBlockException( ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.StepProgramId] = stepProgram.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the controls needed for edit mode.
        /// </summary>
        /// <param name="stepProgramId">The Site Program identifier.</param>
        public void ShowDetail( int stepProgramId )
        {
            LoadWorkflowTriggerTypesSelectionList();

            pnlDetails.Visible = false;

            StepProgram stepProgram = null;

            var dataContext = GetDataContext();

            if ( !stepProgramId.Equals( 0 ) )
            {
                stepProgram = GetStepProgram( stepProgramId, dataContext );
                pdAuditDetails.SetEntity( stepProgram, ResolveRockUrl( "~" ) );
            }

            if ( stepProgram == null )
            {
                stepProgram = new StepProgram { Id = 0 };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            pnlDetails.Visible = true;
            if ( stepProgram != null && stepProgram.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                /*
                SK - 10/28/2021
                Earlier only Person with admin rights were allowed edit the block. That was changed to look for Edit after the Parent Authority for Step Type and Program is set.
                */
                bool editAllowed = stepProgram.IsAuthorized( Authorization.EDIT, CurrentPerson );
                hfStepProgramId.Value = stepProgram.Id.ToString();
                lIcon.Text = string.Format( "<i class='{0}'></i>", stepProgram.IconCssClass );
                bool readOnly = false;

                nbEditModeMessage.Visible = false;
                nbEditModeMessage.Text = string.Empty;
                if ( !editAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( StepProgram.FriendlyTypeName );
                }

                rblDefaultListView.Items.Clear();
                rblDefaultListView.Items.Add( new ListItem( "Cards", StepProgram.ViewMode.Cards.ToString() ) );
                rblDefaultListView.Items.Add( new ListItem( "Grid", StepProgram.ViewMode.Grid.ToString() ) );

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    btnSecurity.Visible = false;
                    ShowReadonlyDetails( stepProgram );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    btnSecurity.Visible = true;
                    btnSecurity.Visible = stepProgram.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                    btnSecurity.Title = "Secure " + stepProgram.Name;
                    btnSecurity.EntityId = stepProgram.Id;

                    if ( !stepProgramId.Equals( 0 ) )
                    {
                        ShowReadonlyDetails( stepProgram );
                    }
                    else
                    {
                        ShowEditDetails( stepProgram );
                    }
                }
            }
            else
            {
                nbEditModeMessage.Visible = true;
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( ContentChannel.FriendlyTypeName );
                pnlEditDetails.Visible = false;
                pnlViewDetails.Visible = false;
                this.HideSecondaryBlocks( true );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="stepProgram">The target entity.</param>
        private void ShowEditDetails( StepProgram stepProgram )
        {
            if ( stepProgram == null )
            {
                stepProgram = new StepProgram();
                stepProgram.IconCssClass = "fa fa-compress";
            }

            if ( stepProgram.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( StepProgram.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = stepProgram.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            // Hide the Inactive Tag because the Active state is editable.
            hlInactive.Visible = false;

            tbName.Text = stepProgram.Name;
            cbActive.Checked = stepProgram.IsActive;
            tbDescription.Text = stepProgram.Description;
            tbIconCssClass.Text = stepProgram.IconCssClass;

            cpCategory.SetValue( stepProgram.CategoryId );

            rblDefaultListView.SelectedValue = stepProgram.DefaultListView.ToString();

            // Step Statuses
            StatusesState = stepProgram.StepStatuses.ToList();

            LoadAttributeDefinitions( new StepType().TypeId, "StepProgramId", stepProgram.Id );

            BindStepStatusesGrid();
            BindAttributesGrid();

            // Workflow Triggers
            WorkflowsState = new List<StepWorkflowTriggerViewModel>();

            // Only show triggers that are not related to a specific Step Type.
            var stepTypeTriggers = stepProgram.StepWorkflowTriggers.Where( x => x.StepTypeId == null );

            foreach ( var trigger in stepTypeTriggers )
            {
                var newItem = new StepWorkflowTriggerViewModel( trigger );

                WorkflowsState.Add( newItem );
            }

            BindStepWorkflowsGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="stepProgram">The target entity.</param>
        private void ShowReadonlyDetails( StepProgram stepProgram )
        {
            SetEditMode( false );

            hfStepProgramId.SetValue( stepProgram.Id );

            WorkflowsState = null;
            StatusesState = null;

            lReadOnlyTitle.Text = stepProgram.Name.FormatAsHtmlTitle();
            lStepProgramName.Text = stepProgram.Name;

            lStepProgramDescription.Text = stepProgram.Description.ScrubHtmlAndConvertCrLfToBr();

            // Configure Label: Inactive
            hlInactive.Visible = !stepProgram.IsActive;

            // Configure Label: Category
            if ( stepProgram.Category != null )
            {
                hlCategory.Text = stepProgram.Category.Name;
            }

            RefreshChart();
            RefreshKpi();
        }

        /// <summary>
        /// Refreshes the kpi.
        /// </summary>
        private void RefreshKpi()
        {
            var stepProgram = GetStepProgram();
            var template = GetAttributeValue( AttributeKey.KpiLava );

            if ( template.IsNullOrWhiteSpace() || stepProgram == null )
            {
                return;
            }

            var startedQuery = GetStartedStepQuery();
            var completedStepQuery = GetCompletedStepQuery();
            var completedPrograms = GetCompletedProgramQuery().ToList();

            var individualsCompleting = completedPrograms.Count;
            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedStepQuery.Count();

            var daysToCompleteList = completedPrograms
                .Where( sps => sps.CompletedDateTime.HasValue && sps.StartedDateTime.HasValue )
                .Select( sps => ( sps.CompletedDateTime.Value - sps.StartedDateTime.Value ).Days );

            var avgDaysToComplete = daysToCompleteList.Any() ? ( int ) daysToCompleteList.Average() : 0;

            lKpi.Text = template.ResolveMergeFields( new Dictionary<string, object>
            {
                { "IndividualsCompleting", individualsCompleting },
                { "AvgDaysToComplete", avgDaysToComplete },
                { "StepsStarted", stepsStarted },
                { "StepsCompleted", stepsCompleted },
                { "StepProgram", stepProgram }
            } );
        }

        /// <summary>
        /// Gets the active step type ids.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetActiveStepTypeIds()
        {
            var stepProgramId = GetActiveStepProgramId();
            var stepProgram = StepProgramCache.Get( stepProgramId );

            if ( stepProgram == null )
            {
                return null;
            }

            var stepTypeIds = stepProgram.StepTypes.Where( st => st.IsActive ).Select( st => st.Id );
            return stepTypeIds;
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetCompletedStepQuery()
        {
            var stepTypeIds = GetActiveStepTypeIds();

            if ( stepTypeIds == null )
            {
                return null;
            }

            var dataContext = GetDataContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    stepTypeIds.Contains( x.StepTypeId ) &&
                    x.CompletedDateKey != null );

            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            // Apply date range
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                var startDateKey = startDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                query = query.Where( x => x.CompletedDateKey <= compareDateKey );
            }

            return query;
        }

        /// <summary>
        /// Gets the completed step program query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<StepProgramService.PersonStepProgramViewModel> GetCompletedProgramQuery()
        {
            var stepProgramId = GetActiveStepProgramId();
            var rockContext = GetDataContext();
            var service = new StepProgramService( rockContext );
            var query = service.GetPersonCompletingProgramQuery( stepProgramId );

            if ( query == null )
            {
                return null;
            }

            // Apply date range
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                query = query.Where( x => x.CompletedDateTime >= startDate.Value );
            }

            if ( endDate != null )
            {
                var compareDate = endDate.Value.AddDays( 1 );
                query = query.Where( x => x.CompletedDateTime < compareDate );
            }

            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            return query;
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStartedStepQuery()
        {
            var stepTypeIds = GetActiveStepTypeIds();

            if ( stepTypeIds == null )
            {
                return null;
            }

            var dataContext = GetDataContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    stepTypeIds.Contains( x.StepTypeId ) &&
                    x.StartDateKey != null );

            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                query = query.Where( s => s.CampusId == campusContext.Id );
            }

            // Apply date range
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                var startDateKey = startDate.Value.ToDateKey();
                query = query.Where( x => x.StartDateKey >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                query = query.Where( x => x.StartDateKey <= compareDateKey );
            }

            return query;
        }

        /// <summary>
        /// Gets the step program data model displayed by this page.
        /// </summary>
        /// <param name="stepProgramId">The step program identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private StepProgram GetStepProgram( int? stepProgramId = null, RockContext rockContext = null )
        {
            if ( stepProgramId == null )
            {
                stepProgramId = hfStepProgramId.ValueAsInt();
            }

            string key = string.Format( "StepProgram:{0}", stepProgramId );

            var stepProgram = RockPage.GetSharedItem( key ) as StepProgram;

            if ( stepProgram == null )
            {
                rockContext = rockContext ?? GetDataContext();

                stepProgram = new StepProgramService( rockContext ).Queryable()
                    .Where( c => c.Id == stepProgramId )
                    .FirstOrDefault();

                RockPage.SaveSharedItem( key, stepProgram );
            }

            return stepProgram;
        }

        /// <summary>
        /// Gets the active step program identifier.
        /// </summary>
        /// <returns></returns>
        private int GetActiveStepProgramId()
        {
            return hfStepProgramId.ValueAsInt();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "STEPSTATUSES":
                    dlgStepStatuses.Show();
                    break;
                case "STEPWORKFLOWS":
                    dlgStepWorkflow.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "STEPSTATUSES":
                    dlgStepStatuses.Hide();
                    break;
                case "STEPWORKFLOWS":
                    dlgStepWorkflow.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Gets the campus context, returns null if there is only no more than one active campus.
        /// This is to prevent to filtering out of Steps that are associated with currently inactive
        /// campuses or no campus at all.
        /// </summary>
        /// <returns></returns>
        private Campus GetCampusContextOrNull()
        {
            return ( CampusCache.All( false ).Count > 1 ) ? ContextEntity<Campus>() : null;
        }

        #endregion

        #region Step Activity Chart

        /// <summary>
        /// Add scripts for Chart.js components
        /// </summary>
        private void InitializeChartScripts()
        {
            // NOTE: moment.js must be loaded before Chart.js
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
        }

        /// <summary>
        /// Initialize the chart by applying block configuration settings.
        /// </summary>
        private void InitializeChartFilter()
        {
            // Set the default Date Range from the block settings.
            var dateRangeSettings = GetAttributeValue( AttributeKey.SlidingDateRange );

            if ( !string.IsNullOrEmpty( dateRangeSettings ) )
            {
                drpSlidingDateRange.DelimitedValues = dateRangeSettings;
            }

            if ( drpSlidingDateRange.SlidingDateRangeMode == SlidingDateRangePicker.SlidingDateRangeType.All )
            {
                // Default to current year
                drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;
            }
        }

        /// <summary>
        /// Refresh the chart using the current filter settings.
        /// </summary>
        private void RefreshChart()
        {
            var stepProgram = GetStepProgram();

            if ( stepProgram == null )
            {
                return;
            }

            // Set the visibility of the Activity Summary chart.
            bool showActivitySummary = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean( true );

            if ( showActivitySummary )
            {
                // If the Program does not have any Step activity, hide the Activity Summary.
                var dataContext = GetDataContext();
                showActivitySummary = GetStepsCompletedQuery( dataContext ).Any();
            }

            pnlActivitySummary.Visible = showActivitySummary;

            if ( !showActivitySummary )
            {
                return;
            }

            // Get chart data and set visibility of related elements.
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );

            var chartFactory = this.GetChartJsFactory( stepProgram, reportPeriod );

            pnlActivityChart.Visible = chartFactory.HasData;
            nbActivityChartMessage.Visible = !chartFactory.HasData;

            if ( !chartFactory.HasData )
            {
                nbActivityChartMessage.Text = "There are no completed Steps matching the current filter.";
                return;
            }

            // Add scripts for Chart.js components
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );

            // Add client script to construct the chart.
            var chartDataJson = chartFactory.GetJson( new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                SizeToFitContainerWidth = true,
                MaintainAspectRatio = false,
                LineTension = 0.4m
            } );

            string script = string.Format( @"var barCtx = $('#{0}')[0].getContext('2d'); var barChart = new Chart(barCtx, {1});", chartCanvas.ClientID, chartDataJson );

            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "stepProgramActivityChartScript", script, true );
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <returns></returns>
        private ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory( StepProgram program, TimePeriod reportPeriod )
        {
            var rockContext = new RockContext();
            var programId = GetActiveStepProgramId();

            // Get all of the completed Steps associated with the current program, grouped by Step Type.
            var stepsCompletedQuery = GetStepsCompletedQuery( rockContext );

            var dateRange = reportPeriod.GetDateRange();

            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate != null )
            {
                startDate = startDate.Value.Date;
                var startDateKey = startDate.ToDateKey();
                stepsCompletedQuery = stepsCompletedQuery.Where( x => x.CompletedDateKey.Value >= startDateKey );
            }

            if ( endDate != null )
            {
                var compareDateKey = endDate.Value.ToDateKey();
                stepsCompletedQuery = stepsCompletedQuery.Where( x => x.CompletedDateKey.Value <= compareDateKey );
            }

            List<StepTypeActivityDataPoint> stepTypeDataPoints;

            // Get the Data Points, scaled according to the currently selected range.
            ChartJsTimeSeriesTimeScaleSpecifier chartTimeScale;

            if ( reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year )
            {
                // Group by Month
                chartTimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;

                stepTypeDataPoints = stepsCompletedQuery
                    .GroupBy( x => new
                    {
                        MonthKey = x.CompletedDateKey.Value / 100,
                        DatasetName = x.StepType.Name,
                        SortKey1 = x.StepType.Order,
                        SortKey2 = x.StepTypeId
                    } )
                    .Select( x => new
                    {
                        x.Key,
                        Count = x.Count()
                    } )
                    .ToList()
                    .Select( x =>
                    {
                        // Add 1 for the first day of the month
                        var dateKey = ( x.Key.MonthKey * 100 ) + 1;

                        return new StepTypeActivityDataPoint
                        {
                            StepTypeName = x.Key.DatasetName,
                            DateTime = dateKey.GetDateKeyDate(),
                            SortKey1 = x.Key.SortKey1,
                            SortKey2 = x.Key.SortKey2,
                            CompletedCount = x.Count
                        };
                    } )
                    .OrderBy( x => x.SortKey1 )
                    .ThenBy( x => x.SortKey2 )
                    .ToList();
            }
            else
            {
                // Group by Day
                chartTimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day;

                stepTypeDataPoints = stepsCompletedQuery
                    .GroupBy( x => new
                    {
                        DateKey = x.CompletedDateKey.Value,
                        DatasetName = x.StepType.Name,
                        SortKey1 = x.StepType.Order,
                        SortKey2 = x.StepTypeId
                    } )
                    .Select( x => new
                    {
                        x.Key,
                        Count = x.Count()
                    } )
                    .ToList()
                    .Select( x => new StepTypeActivityDataPoint
                    {
                        StepTypeName = x.Key.DatasetName,
                        DateTime = x.Key.DateKey.GetDateKeyDate(),
                        SortKey1 = x.Key.SortKey1,
                        SortKey2 = x.Key.SortKey2,
                        CompletedCount = x.Count
                    } )
                    .OrderBy( x => x.SortKey1 )
                    .ThenBy( x => x.SortKey2 )
                    .ToList();
            }

            var stepTypeDatasets = stepTypeDataPoints
                .OrderBy( x => x.SortKey1 ).ThenBy( x => x.SortKey2 )
                .Select( x => x.StepTypeName )
                .Distinct()
                .ToList();

            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

            factory.TimeScale = chartTimeScale;
            factory.StartDateTime = startDate;
            factory.EndDateTime = endDate;
            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.StackedLine;

            foreach ( var stepTypeDataset in stepTypeDatasets )
            {
                var dataset = new ChartJsTimeSeriesDataset();

                // Set Line Color to Step Type Highlight Color.
                var step = program.StepTypes.FirstOrDefault( x => x.Name == stepTypeDataset );

                if ( step != null )
                {
                    dataset.BorderColor = step.HighlightColor;
                }

                dataset.Name = stepTypeDataset;

                dataset.DataPoints = stepTypeDataPoints
                                        .Where( x => x.StepTypeName == stepTypeDataset )
                                        .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.CompletedCount } )
                                        .Cast<IChartJsTimeSeriesDataPoint>()
                                        .ToList();

                factory.Datasets.Add( dataset );
            }

            return factory;
        }

        /// <summary>
        /// Returns a Step query filtered for active steps completed for the specified Program.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        private IQueryable<Step> GetStepsCompletedQuery( RockContext dataContext )
        {
            var stepService = new StepService( dataContext );

            var programId = GetActiveStepProgramId();

            // Get all of the completed Steps associated with the current program, grouped by Step Type.
            var stepsCompletedQuery = stepService.Queryable().AsNoTracking()
                .Where( x =>
                    x.StepType.StepProgramId == programId
                    && x.StepType.IsActive
                    && x.CompletedDateKey.HasValue );

            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                stepsCompletedQuery = stepsCompletedQuery.Where( s => s.CampusId == campusContext.Id );
            }

            return stepsCompletedQuery;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The view model for a workflow trigger associated with this Step Program.
        /// </summary>
        [Serializable]
        private class StepWorkflowTriggerViewModel
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public string WorkflowTypeName { get; set; }

            public int? StepTypeId { get; set; }

            public int WorkflowTypeId { get; set; }

            public StepWorkflowTrigger.WorkflowTriggerCondition TriggerType { get; set; }

            public string TypeQualifier { get; set; }

            public string TriggerDescription { get; set; }

            public StepWorkflowTriggerViewModel()
            {
                //
            }

            public StepWorkflowTriggerViewModel( StepWorkflowTrigger trigger )
            {
                Id = trigger.Id;
                Guid = trigger.Guid;
                StepTypeId = trigger.StepTypeId;
                TriggerType = trigger.TriggerType;
                TypeQualifier = trigger.TypeQualifier;

                if ( trigger.WorkflowType != null )
                {
                    WorkflowTypeId = trigger.WorkflowType.Id;
                    WorkflowTypeName = trigger.WorkflowType.Name;
                }
            }
        }

        /// <summary>
        /// A single data point in the result set of a Steps Activity query.
        /// </summary>
        private class StepTypeActivityDataPoint
        {
            /// <summary>
            /// The name of the Step Type to which this Step activity relates.
            /// </summary>
            public string StepTypeName { get; set; }

            /// <summary>
            /// The date and time represented by this data point.
            /// </summary>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// The number of completions represented by this data point.
            /// </summary>
            public int CompletedCount { get; set; }

            /// <summary>
            /// A value used to sort the datapoint within the set of values for this Step Type.
            /// </summary>
            public int SortKey1 { get; set; }

            /// <summary>
            /// A value used to sort the datapoint within the set of values for this Step Type.
            /// </summary>
            public int SortKey2 { get; set; }
        }

        #endregion

        #region Block Notifications

        private NotificationBox _notificationControl;
        private Control _detailContainerControl;

        /// <summary>
        /// Initialize block-level notification message handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeBlockNotification( NotificationBox notificationControl, Control detailContainerControl )
        {
            _notificationControl = notificationControl;
            _detailContainerControl = detailContainerControl;

            ClearBlockNotification();
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool InitializeBlockContext()
        {
            _stepProgramId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();
            if ( _stepProgramId != 0 )
            {
                var dataContext = this.GetDataContext();
                var stepProgramService = new StepProgramService( dataContext );
                _program = stepProgramService.Queryable().Where( g => g.Id == _stepProgramId ).FirstOrDefault();
            }

            return true;
        }

        /// <summary>
        /// Reset the notification message for the block.
        /// </summary>
        public void ClearBlockNotification()
        {
            _notificationControl.Visible = false;
            _detailContainerControl.Visible = true;
        }

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        public void ShowBlockNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        {
            _notificationControl.Text = message;
            _notificationControl.NotificationBoxType = notificationType;

            _notificationControl.Visible = true;
            _detailContainerControl.Visible = !hideBlockContent;
        }

        /// <summary>
        /// Show an error message for the block and log the associated exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="writeToLog"></param>
        public void ShowBlockException( Exception ex, bool writeToLog = true )
        {
            ShowBlockNotification( ex.Message, NotificationBoxType.Danger );

            if ( writeToLog )
            {
                LogException( ex );
            }
        }

        #endregion

        #region Attributes Grid
        private List<Attribute> AttributesState { get; set; }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadAttributeDefinitions( int targetEntityTypeId, string targetEntityParentForeignKeyName, int targetEntityParentId )
        {
            if ( targetEntityParentId == 0 )
            {
                // If this is a new step type, then there are no attributes to load
                AttributesState = new List<Attribute>();
                return;
            }

            var dataContext = this.GetDataContext();

            var attributeService = new AttributeService( dataContext );

            AttributesState = attributeService
                .GetByEntityTypeId( targetEntityTypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( targetEntityParentForeignKeyName, StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( targetEntityParentId.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Load the Attribute Definitions associated with the current record from ViewState.
        /// </summary>
        private void LoadAttributesViewState()
        {
            string json = ViewState["AttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Save the Attribute Definitions associated with the current record into ViewState.
        /// </summary>
        private void SaveAttributesViewState( JsonSerializerSettings jsonSetting )
        {
            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );
        }

        /// <summary>
        /// Get the implementing type of the Attribute Definition.
        /// This is the type to which the attribute definition is attached, not the type with which the attribute values are associated.
        /// </summary>
        /// <returns></returns>
        private Type GetAttributeParentEntityType()
        {
            return typeof( StepProgram );
        }

        /// <summary>
        /// Get the prompt shown in the Attribute Definition dialog for the current parent entity.
        /// </summary>
        /// <returns></returns>
        private string GetAttributeDefinitionDialogPrompt()
        {
            return string.Format( "Edit Attribute for Step Types in Step Program \"{0}\"", tbName.Text );
        }

        /// <summary>
        /// Set the properties of the Attributes grid.
        /// </summary>
        /// <param name="showAdd"></param>
        private void InitializeAttributesGrid( bool showAdd )
        {
            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.AllowPaging = false;
            gAttributes.DisplayType = GridDisplayType.Light;
            gAttributes.ShowConfirmDeleteDialog = false;
            gAttributes.EmptyDataText = Server.HtmlEncode( None.Text );

            gAttributes.Actions.ShowAdd = showAdd;
            gAttributes.Actions.AddClick += gAttributes_Add;

            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;
        }

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            gAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            var attributeGuid = ( Guid ) e.RowKeyValue;

            gAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Shows the edit attribute dialog.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gAttributes_ShowEdit( Guid attributeGuid )
        {
            var entityType = GetAttributeParentEntityType();
            var prompt = GetAttributeDefinitionDialogPrompt();

            this.ShowAttributeDefinitionDialog( attributeGuid, entityType, prompt );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( AttributesState, e.OldIndex, e.NewIndex );

            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            var attributeGuid = ( Guid ) e.RowKeyValue;

            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_GridRebind( object sender, EventArgs e )
        {
            BindAttributesGrid();
        }

        /// <summary>
        /// Show the Attribute Definition Properties Dialog.
        /// </summary>
        private void ShowAttributeDefinitionDialog( Guid attributeGuid, Type attachToEntityType, string title )
        {
            Attribute attribute;

            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
            }

            edtAttributes.ActionTitle = title;

            var reservedKeyNames = new List<string>();

            AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            edtAttributes.AllowSearchVisible = true;
            edtAttributes.ReservedKeyNames = reservedKeyNames.ToList();
            edtAttributes.SetAttributeProperties( attribute, attachToEntityType );

            hfActiveDialog.Value = "ATTRIBUTES";

            dlgAttribute.Show();
        }

        /// <summary>
        /// Hide the Attribute Definition Properties Dialog.
        /// </summary>
        private void HideAttributeDefinitionDialog()
        {
            dlgAttribute.Hide();

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgConnectionTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAttribute_SaveClick( object sender, EventArgs e )
        {
            var attribute = new Rock.Model.Attribute();
            edtAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                AttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
            }

            AttributesState.Add( attribute );
            ReOrderAttributes( AttributesState );
            BindAttributesGrid();

            HideAttributeDefinitionDialog();
        }

        /// <summary>
        /// Binds the Connection Type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.DataSource = AttributesState
                         .OrderBy( a => a.Order )
                         .ThenBy( a => a.Name )
                         .Select( a => new
                         {
                             a.Id,
                             a.Guid,
                             a.Name,
                             a.Description,
                             a.IsRequired,
                             a.IsGridColumn,
                             a.AllowSearch
                         } )
                         .ToList();
            gAttributes.DataBind();
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortAttributes( List<Attribute> attributeList, int oldIndex, int newIndex )
        {
            var movedItem = attributeList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in attributeList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in attributeList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorders the attributes.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void ReOrderAttributes( List<Attribute> attributeList )
        {
            attributeList = attributeList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            attributeList.ForEach( a => a.Order = order++ );
        }
        #endregion
    }
}