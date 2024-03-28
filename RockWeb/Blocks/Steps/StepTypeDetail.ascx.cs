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
using System.Data.Entity.SqlServer;
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
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Type Detail" )]
    [Category( "Steps" )]
    [Description( "Displays the details of the given Step Type for editing." )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [BooleanField
        ( "Show Chart",
          Key = AttributeKey.ShowChart,
          DefaultValue = "true",
          Order = 0 )]
    [DefinedValueField
        ( Rock.SystemGuid.DefinedType.CHART_STYLES,
         "Chart Style",
         Key = AttributeKey.ChartStyle,
         DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK,
         Order = 1 )]
    [SlidingDateRangeField
        ( "Default Chart Date Range",
          Key = AttributeKey.SlidingDateRange,
          DefaultValue = "Current||Year||",
          EnabledSlidingDateRangeTypes = "Last,Previous,Current,DateRange",
          Order = 2 )]
    [CategoryField(
        "Data View Categories",
        Key = AttributeKey.DataViewCategories,
        Description = "The categories from which the Audience and Autocomplete data view options can be selected. If empty, all data views will be available.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.DataView",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 7 )]

    [LinkedPage(
        name: "Bulk Entry Page",
        description: "The page to use for bulk entry of steps data",
        required: false,
        order: 8,
        key: AttributeKey.BulkEntryPage )]

    [CodeEditorField(
        "Key Performance Indicator Lava",
        IsRequired = false,
        DefaultValue = DefaultValue.KpiLava,
        Key = AttributeKey.KpiLava,
        EditorMode = CodeEditorMode.Lava,
        Description = "The Lava used to render the Key Performance Indicators bar. <span class='tip tip-lava'></span>",
        Order = 9 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "84DEAB14-70B3-4DA4-9CC2-0E0A301EE0FD" )]
    public partial class StepTypeDetail : ContextEntityBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The show chart
            /// </summary>
            public const string ShowChart = "ShowChart";

            /// <summary>
            /// The chart style
            /// </summary>
            public const string ChartStyle = "ChartStyle";

            /// <summary>
            /// The sliding date range
            /// </summary>
            public const string SlidingDateRange = "SlidingDateRange";

            /// <summary>
            /// The data view categories
            /// </summary>
            public const string DataViewCategories = "DataViewCategories";

            /// <summary>
            /// The bulk entry page
            /// </summary>
            public const string BulkEntryPage = "BulkEntryPage";

            /// <summary>
            /// The kpi lava
            /// </summary>
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
@"{[kpis style:'card' iconbackground:'true' columncount:'4']}
    [[ kpi icon:'fa-user' value:'{{IndividualsCompleting | Format:'N0'}}' label:'Individuals Completing' color:'blue-700']][[ endkpi ]]
    {% if StepType.HasEndDate %}
        [[ kpi icon:'fa-calendar' value:'{{AvgDaysToComplete | Format:'N0'}}' label:'Average Days to Complete' color:'green-600']][[ endkpi ]]
        [[ kpi icon:'fa-map-marker' value:'{{StepsStarted | Format:'N0'}}' label:'Steps Started' color:'#FF385C']][[ endkpi ]]
    {% endif %}
    [[ kpi icon:'fa-check-square' value:'{{StepsCompleted | Format:'N0'}}' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step type identifier
            /// </summary>
            public const string StepTypeId = "StepTypeId";

            /// <summary>
            /// The step program identifier
            /// </summary>
            public const string StepProgramId = "ProgramId";
        }

        #endregion Page Parameter Keys

        #region Properties

        private List<Attribute> AttributesState { get; set; }

        private List<StepWorkflowTriggerViewModel> WorkflowsState { get; set; }

        #endregion

        #region Private Variables

        private StepType _stepType = null;
        private StepProgram _program = null;
        private int _stepProgramId = 0;
        private int _stepTypeId = 0;
        private RockContext _dataContext = null;
        private bool _blockContextIsValid = false;

        #endregion Private Variables

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeBlockNotification( nbBlockStatus, pnlDetails );
            InitializeSettingsNotification( upStepType );

            _blockContextIsValid = InitializeBlockContext();

            if ( !_blockContextIsValid )
            {
                return;
            }

            InitializeChartScripts();
            InitializeChartFilter();

            dvpAutocomplete.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvpAutocomplete.CategoryGuids = GetAttributeValue( AttributeKey.DataViewCategories ).SplitDelimitedValues().AsGuidList();

            dvpAudience.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvpAudience.CategoryGuids = GetAttributeValue( AttributeKey.DataViewCategories ).SplitDelimitedValues().AsGuidList();

            bool editAllowed = false;
            if ( _stepType != null )
            {
                editAllowed = _stepType.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else if ( _program != null )
            {
                // Till this point, Step Type may not be initialized. That is the reason we are looking for authorization for Step Program.
                editAllowed = _program.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            InitializeAttributesGrid( editAllowed );
            InitializeWorkflowGrid( editAllowed );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete the associated step participants.');", StepType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.StepType ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !_blockContextIsValid )
            {
                return;
            }

            if ( !Page.IsPostBack )
            {
                ShowDetail( _stepTypeId );
            }
            else
            {
                RefreshChart();
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            LoadAttributesViewState();

            var json = ViewState["WorkflowsState"] as string ?? string.Empty;
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject( WorkflowsState, Formatting.None, jsonSetting );
            SaveAttributesViewState( jsonSetting );
            ViewState["WorkflowsState"] = json;

            return base.SaveViewState();
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

            int? stepTypeId = PageParameter( pageReference, PageParameterKey.StepTypeId ).AsIntegerOrNull();
            if ( stepTypeId != null )
            {
                var dataContext = GetDataContext();

                var stepType = new StepTypeService( dataContext ).Get( stepTypeId.Value );

                if ( stepType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( stepType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Step Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a Page Parameter to work with.
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Navigate to the step program page
        /// </summary>
        private void GoToStepProgramPage()
        {
            NavigateToParentPage( new Dictionary<string, string> { { PageParameterKey.StepProgramId, _stepProgramId.ToString() } } );
        }

        #endregion

        #region Events

        #region Control Events

        /// <summary>
        /// Handles the Click event of the btnBulkEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBulkEntry_Click( object sender, EventArgs e )
        {
            var stepType = GetStepType();
            var queryParams = new Dictionary<string, string>();

            if ( stepType != null )
            {
                queryParams[PageParameterKey.StepTypeId] = stepType.Id.ToString();
            }

            NavigateToLinkedPage( AttributeKey.BulkEntryPage, queryParams );
        }

        /// <summary>
        /// Refresh the Steps Activity Chart.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            var stepType = GetStepType();
            ShowEditDetails( stepType );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            DeleteRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var recordId = SaveRecord();

            if ( recordId <= 0 )
            {
                return;
            }

            // Update the query string for this page and reload.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.StepTypeId] = recordId.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns>The Id of the new record, or -1 if the process could not be completed.</returns>
        private int SaveRecord()
        {
            StepType stepType;

            var rockContext = GetDataContext();
            var stepTypeService = new StepTypeService( rockContext );
            var stepWorkflowService = new StepWorkflowService( rockContext );
            var stepWorkflowTriggerService = new StepWorkflowTriggerService( rockContext );

            int stepTypeId = int.Parse( hfStepTypeId.Value );
            bool isNew = false;
            if ( stepTypeId == 0 )
            {
                stepType = new StepType();
                stepType.StepProgramId = _stepProgramId;
                stepTypeService.Add( stepType );
                isNew = true;
            }
            else
            {
                stepType = stepTypeService.Queryable()
                                          .Include( x => x.StepWorkflowTriggers )
                                          .Where( c => c.Id == stepTypeId )
                                          .FirstOrDefault();
                _stepProgramId = stepType.StepProgramId;
            }

            // Workflow Triggers: Remove deleted triggers.
            var uiWorkflows = WorkflowsState.Select( l => l.Guid );
            var deletedTriggers = stepType.StepWorkflowTriggers.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList();

            foreach ( var trigger in deletedTriggers )
            {
                // Remove the Step workflows associated with this trigger.
                var stepWorkflows = stepWorkflowService.Queryable().Where( w => w.StepWorkflowTriggerId == trigger.Id );

                foreach ( var requestWorkflow in stepWorkflows )
                {
                    stepWorkflowService.Delete( requestWorkflow );
                }

                // Remove the trigger.
                stepType.StepWorkflowTriggers.Remove( trigger );
                stepWorkflowTriggerService.Delete( trigger );
            }

            // Workflow Triggers: Update modified triggers.
            foreach ( var stateTrigger in WorkflowsState )
            {
                var workflowTrigger = stepType.StepWorkflowTriggers.Where( a => a.Guid == stateTrigger.Guid ).FirstOrDefault();

                if ( workflowTrigger == null )
                {
                    workflowTrigger = new StepWorkflowTrigger();
                    workflowTrigger.StepProgramId = stepType.StepProgramId;
                    stepType.StepWorkflowTriggers.Add( workflowTrigger );
                }

                workflowTrigger.Guid = stateTrigger.Guid;
                workflowTrigger.WorkflowTypeId = stateTrigger.WorkflowTypeId;
                workflowTrigger.TriggerType = stateTrigger.TriggerType;
                workflowTrigger.TypeQualifier = stateTrigger.TypeQualifier;
                workflowTrigger.WorkflowTypeId = stateTrigger.WorkflowTypeId;
                workflowTrigger.WorkflowName = stateTrigger.WorkflowTypeName;
            }

            // Update Basic properties
            stepType.Name = tbName.Text;
            stepType.IsActive = cbIsActive.Checked;
            stepType.Description = tbDescription.Text;
            stepType.IconCssClass = tbIconCssClass.Text;
            stepType.HighlightColor = cpHighlight.Value;
            stepType.ShowCountOnBadge = cbShowBadgeCount.Checked;
            stepType.HasEndDate = cbHasDuration.Checked;
            stepType.AllowMultiple = cbAllowMultiple.Checked;
            stepType.IsDateRequired = cbRequireDate.Checked;

            // Update Prerequisites
            var uiPrerequisiteStepTypeIds = cblPrerequsities.SelectedValuesAsInt;
            var stepTypes = stepTypeService.Queryable().Where( x => x.StepProgramId == _stepProgramId && x.IsActive ).ToList();
            var removePrerequisiteStepTypes = stepType.StepTypePrerequisites.Where( x => !uiPrerequisiteStepTypeIds.Contains( x.PrerequisiteStepTypeId ) ).ToList();
            var prerequisiteService = new StepTypePrerequisiteService( rockContext );

            foreach ( var prerequisiteStepType in removePrerequisiteStepTypes )
            {
                stepType.StepTypePrerequisites.Remove( prerequisiteStepType );
                prerequisiteService.Delete( prerequisiteStepType );
            }

            var existingPrerequisiteStepTypeIds = stepType.StepTypePrerequisites.Select( x => x.PrerequisiteStepTypeId ).ToList();
            var addPrerequisiteStepTypeIds = stepTypes.Where( x => uiPrerequisiteStepTypeIds.Contains( x.Id )
                                                                 && !existingPrerequisiteStepTypeIds.Contains( x.Id ) )
                                                      .Select( x => x.Id )
                                                      .ToList();

            foreach ( var prerequisiteStepTypeId in addPrerequisiteStepTypeIds )
            {
                var newPrerequisite = new StepTypePrerequisite();
                newPrerequisite.StepTypeId = stepType.Id;
                newPrerequisite.PrerequisiteStepTypeId = prerequisiteStepTypeId;
                stepType.StepTypePrerequisites.Add( newPrerequisite );
            }

            // Validate Prerequisites.
            // This is necessary because other Step Types may have been modified after this record edit was started.
            if ( _stepTypeId > 0 )
            {
                var eligibleStepTypeIdList = stepTypeService.GetEligiblePrerequisiteStepTypes( _stepTypeId ).Select( x => x.Id ).ToList();

                foreach ( var prerequisite in stepType.StepTypePrerequisites )
                {
                    if ( !eligibleStepTypeIdList.Contains( prerequisite.PrerequisiteStepTypeId ) )
                    {
                        var prerequisiteStepType = stepTypeService.Get( prerequisite.PrerequisiteStepTypeId );
                        cvStepType.IsValid = false;
                        cvStepType.ErrorMessage = string.Format( "This Step Type cannot have prerequisite \"{0}\" because it is already a prerequisite of that Step Type.", prerequisiteStepType.Name );
                        return 0;
                    }
                }
            }

            if ( isNew )
            {
                // If there are any other step types, either:
                // Find out the maximum Order value for the steps, and set this new Step's Order value one higher than that.
                // If there are NOT any other step Types, set Order as 0.
                stepType.Order = stepTypes.Any() ? stepTypes.Max( st => st.Order ) + 1 : 0;

            }

            // Update Advanced Settings
            stepType.AutoCompleteDataViewId = dvpAutocomplete.SelectedValueAsId();
            stepType.AudienceDataViewId = dvpAudience.SelectedValueAsId();
            stepType.AllowManualEditing = cbAllowEdit.Checked;
            stepType.CardLavaTemplate = ceCardTemplate.Text;

            if ( !stepType.IsValid )
            {
                // Controls will render the error messages
                return -1;
            }

            // Save the Step Type and the associated Attributes.
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                avcStepProgramAttributes.GetEditValues( stepType );
                stepType.SaveAttributeValues( rockContext );

                Helper.SaveAttributeEdits( AttributesState, new Step().TypeId, "StepTypeId", stepType.Id.ToString(), rockContext );
            } );

            return stepType.Id;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfStepTypeId.Value.Equals( "0" ) )
            {
                GoToStepProgramPage();
            }
            else
            {
                ShowReadonlyDetails( GetStepType() );
            }
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

        /// <summary>
        /// Updates the trigger qualifiers.
        /// </summary>
        private void UpdateTriggerQualifiers()
        {
            var dataContext = GetDataContext();

            var workflowTrigger = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfAddStepWorkflowGuid.Value.AsGuid() ) );

            var sStepWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<StepWorkflowTrigger.WorkflowTriggerCondition>();

            if ( sStepWorkflowTriggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                // Populate the selection lists for "To Status" and "From Status".
                var stepType = GetStepType();

                var statusList = new StepStatusService( dataContext ).Queryable().Where( s => s.StepProgramId == stepType.StepProgramId ).ToList();

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
                var stepService = new StepWorkflowTriggerService( new RockContext() );

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
        /// Configure the Workflow grid control.
        /// </summary>
        private void InitializeWorkflowGrid( bool showAdd )
        {
            gWorkflows.DataKeyNames = new string[] { "Guid" };
            gWorkflows.Actions.ShowAdd = showAdd;
            gWorkflows.Actions.AddClick += gWorkflows_Add;
            gWorkflows.GridRebind += gWorkflows_GridRebind;
        }

        #endregion

        #endregion

        #region Attributes Grid and Picker (Custom)

        /// <summary>
        /// Get the implementing type of the Attribute Definition.
        /// This is the type to which the attribute definition is attached, not the type with which the attribute values are associated.
        /// </summary>
        /// <returns></returns>
        private Type GetAttributeParentEntityType()
        {
            return typeof( StepType );
        }

        /// <summary>
        /// Get the prompt shown in the Attribute Definition dialog for the current parent entity.
        /// </summary>
        /// <returns></returns>
        private string GetAttributeDefinitionDialogPrompt()
        {
            return string.Format( "Edit Attribute for Participants in Step Type \"{0}\"", tbName.Text );
        }

        #endregion

        #region Attributes Grid and Picker (Common)

        // Code in this region should be capable of being reused in other blocks without modification.

        /// <summary>
        /// Save the Attribute Definition currently displayed in the properties dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveAttributeDefinition()
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();

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
            HideDialog();
        }

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
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
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
                             FieldType = FieldTypeCache.GetName( a.FieldTypeId ),
                             a.IsRequired,
                             a.IsGridColumn,
                             a.AllowSearch
                         } )
                         .ToList();
            gAttributes.DataBind();
        }

        /// <summary>
        /// Build the dynamic controls based on the attributes
        /// </summary>
        private void StepTypeAttributeValueContainer( bool editMode )
        {
            var stepProgramId = GetStepProgramId();
            var stepType = GetStepType() ?? new StepType { StepProgramId = stepProgramId };

            stepType.LoadAttributes();

            if ( editMode )
            {
                avcStepProgramAttributes.AddEditControls( stepType );
            }
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

        #region Internal Methods

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

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;

            AddConfigurationUpdateTrigger( triggerPanel );
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool InitializeBlockContext()
        {
            _stepType = null;

            _stepProgramId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();
            _stepTypeId = PageParameter( PageParameterKey.StepTypeId ).AsInteger();

            if ( _stepProgramId == 0
                 && _stepTypeId == 0 )
            {
                ShowNotification( "A new Step cannot be added because there is no Step Program available in this context.", NotificationBoxType.Danger, true );

                return false;
            }

            var dataContext = this.GetDataContext();
            if ( _stepTypeId != 0 )
            {
                var stepTypeService = new StepTypeService( dataContext );
                _stepType = stepTypeService.Queryable().Where( g => g.Id == _stepTypeId ).FirstOrDefault();
            }
            else
            {
                var stepProgramService = new StepProgramService( dataContext );
                _program = stepProgramService.Queryable().Where( g => g.Id == _stepProgramId ).FirstOrDefault();
            }

            return true;
        }

        /// <summary>
        /// Populate the selection list for Workflow Trigger Types.
        /// </summary>
        private void LoadWorkflowTriggerTypesSelectionList()
        {
            ddlTriggerType.Items.Add( new ListItem( "Step Completed", StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete.ToString() ) );
            ddlTriggerType.Items.Add( new ListItem( "Status Changed", StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged.ToString() ) );
            ddlTriggerType.Items.Add( new ListItem( "Manual", StepWorkflowTrigger.WorkflowTriggerCondition.Manual.ToString() ) );
        }

        /// <summary>
        /// Populate the selection list for Prerequisite Steps.
        /// </summary>
        private void LoadPrerequisiteStepsList()
        {
            var dataContext = GetDataContext();

            // Load available Prerequisite Steps.
            var programId = GetStepProgramId();

            var stepsService = new StepTypeService( dataContext );

            List<StepType> prerequisiteStepTypes;

            if ( _stepTypeId == 0 )
            {
                prerequisiteStepTypes = stepsService.Queryable().Where( x => x.StepProgramId == programId && x.IsActive ).ToList();
            }
            else
            {
                prerequisiteStepTypes = stepsService.GetEligiblePrerequisiteStepTypes( _stepTypeId ).ToList();
            }

            cblPrerequsities.DataSource = prerequisiteStepTypes;
            cblPrerequsities.DataBind();
            cblPrerequsities.Visible = prerequisiteStepTypes.Count > 0;
        }

        /// <summary>
        /// Gets the step program identifier.
        /// </summary>
        /// <returns></returns>
        private int GetStepProgramId()
        {
            var stepType = GetStepType();

            int programId = 0;

            if ( stepType != null )
            {
                programId = stepType.StepProgramId;
            }

            if ( programId == 0 )
            {
                programId = _stepProgramId;
            }

            return programId;
        }

        /// <summary>
        /// Shows the detail panel containing the main content of the block.
        /// </summary>
        /// <param name="stepTypeId">The entity id of the item to be shown.</param>
        public void ShowDetail( int stepTypeId )
        {
            pnlDetails.Visible = false;

            var dataContext = GetDataContext();

            // Get the Step Type data model
            var stepType = GetStepType( stepTypeId );

            if ( stepType.Id != 0 )
            {
                pdAuditDetails.SetEntity( stepType, ResolveRockUrl( "~" ) );
            }
            else
            {
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            /*
             SK - 10/28/2021
             Earlier only Person with admin rights were allowed edit the block. That was changed to look for Edit after the Parent Authority for Step Type and Program is set.
             */
            bool editAllowed = stepType.IsAuthorized( Authorization.EDIT, CurrentPerson );
            pnlDetails.Visible = true;
            hfStepTypeId.Value = stepType.Id.ToString();
            lIcon.Text = string.Format( "<i class='{0}'></i>", stepType.IconCssClass );
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( StepProgram.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnSecurity.Visible = false;
                ShowReadonlyDetails( stepType );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                btnSecurity.Visible = true;
                btnSecurity.Title = "Secure " + stepType.Name;
                btnSecurity.EntityId = stepType.Id;

                if ( !stepTypeId.Equals( 0 ) )
                {
                    ShowReadonlyDetails( stepType );
                }
                else
                {
                    ShowEditDetails( stepType );
                }
            }

            // Set availability of Bulk Entry action.
            var showBulkEntry = GetAttributeValue( AttributeKey.BulkEntryPage ).IsNotNullOrWhiteSpace()
                && this.UserCanEdit
                && stepType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            btnBulkEntry.Visible = showBulkEntry;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="stepType">The entity instance to be displayed.</param>
        private void ShowEditDetails( StepType stepType )
        {
            if ( stepType == null )
            {
                stepType = new StepType
                {
                    IconCssClass = "fa fa-compress",
                    IsDateRequired = true
                };
            }

            if ( stepType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( StepType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = stepType.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            LoadAttributeDefinitions( new Step().TypeId, "StepTypeId", stepType.Id );

            LoadPrerequisiteStepsList();
            LoadWorkflowTriggerTypesSelectionList();

            // General properties
            tbName.Text = stepType.Name;
            cbIsActive.Checked = stepType.IsActive;
            tbDescription.Text = stepType.Description;
            tbIconCssClass.Text = stepType.IconCssClass;
            cpHighlight.Text = stepType.HighlightColor;
            cbAllowMultiple.Checked = stepType.AllowMultiple;
            cbHasDuration.Checked = stepType.HasEndDate;
            cbShowBadgeCount.Checked = stepType.ShowCountOnBadge;
            cbRequireDate.Checked = stepType.IsDateRequired;

            // Set the values of any Step Type Prerequisites.
            if ( stepType.StepTypePrerequisites != null )
            {
                cblPrerequsities.SetValues( stepType.StepTypePrerequisites.Select( x => x.PrerequisiteStepTypeId ) );
            }

            // Advanced Settings
            dvpAutocomplete.SetValue( stepType.AutoCompleteDataViewId );
            dvpAudience.SetValue( stepType.AudienceDataViewId );
            cbAllowEdit.Checked = stepType.AllowManualEditing;
            ceCardTemplate.Text = stepType.CardLavaTemplate;

            // Workflow Triggers
            WorkflowsState = new List<StepWorkflowTriggerViewModel>();

            foreach ( var trigger in stepType.StepWorkflowTriggers )
            {
                var newItem = new StepWorkflowTriggerViewModel( trigger );
                WorkflowsState.Add( newItem );
            }

            BindAttributesGrid();
            BindStepWorkflowsGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="stepType">The entity instance to be displayed.</param>
        private void ShowReadonlyDetails( StepType stepType )
        {
            SetEditMode( false );

            hfStepTypeId.SetValue( stepType.Id );

            WorkflowsState = null;

            lReadOnlyTitle.Text = stepType.Name.FormatAsHtmlTitle();

            lStepTypeDescription.Text = stepType.Description.ScrubHtmlAndConvertCrLfToBr();;
            lStepTypeName.Text = stepType.Name;

            // Configure Label: Inactive
            hlInactive.Visible = !stepType.IsActive;

            RefreshChart();
            RefreshKpi();
        }

        /// <summary>
        /// Refreshes the kpi.
        /// </summary>
        private void RefreshKpi()
        {
            var stepType = GetStepType();
            var template = GetAttributeValue( AttributeKey.KpiLava );

            if ( template.IsNullOrWhiteSpace() || stepType == null )
            {
                return;
            }

            var startedQuery = GetStartedStepQuery();
            var completedQuery = GetCompletedStepQuery();

            var individualsCompleting = completedQuery.Select( s => s.PersonAlias.PersonId ).Distinct().Count();
            var stepsStarted = startedQuery.Count();
            var stepsCompleted = completedQuery.Count();

            var daysToCompleteList = completedQuery
                .Select( s => SqlFunctions.DateDiff( "DAY", s.StartDateTime, s.CompletedDateTime ) )
                .Where( i => i.HasValue )
                .Select( i => i.Value )
                .ToList();

            var avgDaysToComplete = daysToCompleteList.Any() ? ( int ) daysToCompleteList.Average() : 0;

            lKpi.Text = template.ResolveMergeFields( new Dictionary<string, object>
            {
                { "IndividualsCompleting", individualsCompleting },
                { "AvgDaysToComplete", avgDaysToComplete },
                { "StepsStarted", stepsStarted },
                { "StepsCompleted", stepsCompleted },
                { "StepType", stepType }
            } );
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var rockContext = GetDataContext();

            var stepTypeService = new StepTypeService( rockContext );

            var stepType = GetStepType( forceLoadFromContext: true );

            if ( stepType != null )
            {
                // Earlier only Person with admin rights were allowed edit the block.That was changed to look for Edit after the Parent Authority for Step Type and Program is set.
                if ( !stepType.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;

                if ( !stepTypeService.CanDelete( stepType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                stepTypeService.Delete( stepType );
                rockContext.SaveChanges();
            }

            GoToStepProgramPage();
        }

        /// <summary>
        /// Gets the specified Step Type data model, or the current model if none is specified.
        /// </summary>
        /// <param name="stepType">The entity id of the instance to be retrieved.</param>
        /// <returns></returns>
        private StepType GetStepType( int? stepTypeId = null, bool forceLoadFromContext = false )
        {
            if ( stepTypeId == null )
            {
                stepTypeId = hfStepTypeId.ValueAsInt();
            }

            string key = string.Format( "StepType:{0}", stepTypeId );

            StepType stepType = null;

            if ( !forceLoadFromContext )
            {
                stepType = RockPage.GetSharedItem( key ) as StepType;
            }

            if ( stepType == null )
            {
                var dataContext = GetDataContext();

                stepType = new StepTypeService( dataContext ).Queryable()
                    .Where( c => c.Id == stepTypeId )
                    .FirstOrDefault();

                if ( stepType == null )
                {
                    stepType = new StepType
                    {
                        Id = 0,
                        IsDateRequired = true,
                        StepProgramId = _stepProgramId
                    };
                }

                RockPage.SaveSharedItem( key, stepType );
            }

            if ( _stepProgramId == default( int ) )
            {
                _stepProgramId = stepType.StepProgramId;
            }

            return stepType;
        }

        private int GetActiveStepTypeId()
        {
            return hfStepTypeId.ValueAsInt();
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
            StepTypeAttributeValueContainer( editable );
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
                case "STEPWORKFLOWS":
                    dlgStepWorkflow.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
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
            // Set the visibility of the Activity Summary chart.
            bool showActivitySummary = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean( true );

            if ( showActivitySummary )
            {
                // If the Step Type does not have any activity, hide the Activity Summary.
                var dataContext = GetDataContext();
                var stepService = new StepService( dataContext );
                var stepsQuery = stepService.Queryable().AsNoTracking()
                                    .Where( x => x.StepTypeId == _stepTypeId );
                showActivitySummary = stepsQuery.Any();
            }

            pnlActivitySummary.Visible = showActivitySummary;

            if ( !showActivitySummary )
            {
                return;
            }

            // Get chart data and set visibility of related elements.
            var chartFactory = this.GetChartJsFactory();

            chartCanvas.Visible = chartFactory.HasData;
            nbActivityChartMessage.Visible = !chartFactory.HasData;

            if ( !chartFactory.HasData )
            {
                // If no data, show a notification.
                nbActivityChartMessage.Text = "There are no Steps matching the current filter.";
                return;
            }

            // Add client script to construct the chart.
            var chartDataJson = chartFactory.GetJson( new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                SizeToFitContainerWidth = true,
                MaintainAspectRatio = false,
                LineTension = 0.4m
            } );

            string script = string.Format(
            @"var barCtx = $('#{0}')[0].getContext('2d');
            var barChart = new Chart(barCtx, {1});",
                                            chartCanvas.ClientID,
                                            chartDataJson );

            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "stepTypeActivityChartScript", script, true );
        }

        /// <summary>
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetCompletedStepQuery()
        {
            var dataContext = GetDataContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.StepTypeId == _stepTypeId &&
                    x.StepType.IsActive &&
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
        /// Gets the completed step query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Step> GetStartedStepQuery()
        {
            var dataContext = GetDataContext();
            var stepService = new StepService( dataContext );

            var query = stepService.Queryable()
                .AsNoTracking()
                .Where( x =>
                    x.StepTypeId == _stepTypeId &&
                    x.StepType.IsActive &&
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
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <returns></returns>
        public ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory()
        {
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
            }

            // Initialize a new Chart Factory.
            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

            if ( reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year )
            {
                factory.TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }
            else
            {
                factory.TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Day;
            }

            factory.StartDateTime = startDate;
            factory.EndDateTime = endDate;
            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;

            // Determine the appropriate date grouping for the chart data points.
            Func<int, int> groupKeySelector;
            var groupByDay = factory.TimeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day;

            if ( groupByDay )
            {
                // Group Steps by Start Date.
                groupKeySelector = x => x;
            }
            else
            {
                // Group Steps by Start Date rounded to beginning of the month.
                groupKeySelector = x => x / 100;
            }

            // Add data series for Steps started.
            var startedSeriesDataPoints = GetStartedStepQuery()
                .Select( x => x.StartDateKey.Value )
                .ToList()
                .GroupBy( groupKeySelector )
                .Select( x => new ChartDatasetInfo
                {
                    DatasetName = "Started",
                    DateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count(),
                    SortKey = "1"
                } );

            // Add data series for Steps completed.
            var completedSeriesDataPoints = GetCompletedStepQuery()
                .Select( x => x.CompletedDateKey.Value )
                .ToList()
                .GroupBy( groupKeySelector )
                .Select( x => new ChartDatasetInfo
                {
                    DatasetName = "Completed",
                    DateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count(),
                    SortKey = "2"
                } );

            var allDataPoints = startedSeriesDataPoints.Union( completedSeriesDataPoints ).OrderBy( x => x.SortKey ).ThenBy( x => x.DateTime );
            var dataSetNames = allDataPoints.Select( x => x.DatasetName ).Distinct().ToList();

            // Add Dataset for Steps Started.
            var colorStarted = new RockColor( ChartJsConstants.Colors.Blue );
            var startedDataset = this.CreateDataSet( allDataPoints, "Started", colorStarted.ToHex() );

            factory.Datasets.Add( startedDataset );

            // Add Dataset for Steps Completed.
            var colorCompleted = new RockColor( ChartJsConstants.Colors.Green );
            var completedDataset = this.CreateDataSet( allDataPoints, "Completed", colorCompleted.ToHex() );

            factory.Datasets.Add( completedDataset );

            return factory;
        }

        private ChartJsTimeSeriesDataset CreateDataSet( IOrderedEnumerable<ChartDatasetInfo> allDataPoints, string datasetName, string colorString )
        {
            var dataset = new ChartJsTimeSeriesDataset();
            dataset.Name = datasetName;
            dataset.DataPoints = allDataPoints
                                    .Where( x => x.DatasetName == datasetName )
                                    .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.Value } )
                                    .Cast<IChartJsTimeSeriesDataPoint>()
                                    .ToList();
            dataset.BorderColor = colorString;

            return dataset;
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

        #region Support Classes

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
        /// Stores information about a dataset to be displayed on a chart.
        /// </summary>
        private class ChartDatasetInfo
        {
            public string DatasetName { get; set; }

            public DateTime DateTime { get; set; }

            public int Value { get; set; }

            public string SortKey { get; set; }
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
        public void ShowNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        {
            _notificationControl.Text = message;
            _notificationControl.NotificationBoxType = notificationType;
            _notificationControl.Visible = true;
            _detailContainerControl.Visible = !hideBlockContent;
        }

        #endregion
    }
}