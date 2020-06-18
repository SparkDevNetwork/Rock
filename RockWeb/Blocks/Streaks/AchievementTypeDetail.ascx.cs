﻿// <copyright>
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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Achievement;
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

namespace RockWeb.Blocks.Streaks
{
    [DisplayName( "Achievement Type Detail" )]
    [Category( "Streaks" )]
    [Description( "Displays the details of the given Achievement Type for editing." )]

    public partial class AchievementTypeDetail : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// Key for the streak type Id
            /// </summary>
            public const string StreakTypeId = "StreakTypeId";

            /// <summary>
            /// The streak type achievement type identifier
            /// </summary>
            public const string StreakTypeAchievementTypeId = "StreakTypeAchievementTypeId";
        }

        #endregion Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeActionButtons();
            InitializeSettingsNotification();

            // NOTE: moment.js must be loaded before Chart.js
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
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
                RenderState();
            }
            else if ( IsViewMode() )
            {
                RefreshChart();
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
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Achievement Type" : GetAchievementTypeCache().Name, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnRebuild.Attributes["onclick"] = "javascript: return Rock.dialogs.confirmDelete(event, 'data', 'Attempt data that occurs after a person\\'s most recent successful attempt will be deleted and rebuilt from streak data. This process occurs real-time (not in a job).');";
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'All associated achievement attempts will also be deleted!');", StreakTypeAchievementType.FriendlyTypeName );
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upAchievementTypeDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnRebuild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRebuild_Click( object sender, EventArgs e )
        {
            var achievementTypeCache = GetAchievementTypeCache();
            StreakTypeAchievementTypeService.Process( achievementTypeCache.Id );
            NavigateToCurrentPage( new Dictionary<string, string> {
                { PageParameterKey.StreakTypeAchievementTypeId, achievementTypeCache.Id.ToString() }
            } );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAddStep control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAddStep_CheckedChanged( object sender, EventArgs e )
        {
            divStepControls.Visible = cbAddStep.Checked;
        }

        /// <summary>
        /// Refresh the Activity Chart.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRefreshChart_Click( object sender, EventArgs e )
        {
            RefreshChart();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            hfIsEditMode.Value = CanEdit() ? "true" : string.Empty;
            RenderState();
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
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( IsAddMode() )
            {
                NavigateToParentPage();
            }
            else
            {
                hfIsEditMode.Value = string.Empty;
                RenderState();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAllowOverachievement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAllowOverachievement_CheckedChanged( object sender, EventArgs e )
        {
            SyncOverAchievementAndMaxControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpAchievementComponent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpAchievementComponent_SelectedIndexChanged( object sender, EventArgs e )
        {
            RenderComponentAttributeControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the spstStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void spstStepType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncStepControls();
        }

        #endregion Events

        #region Block Notification Messages

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        private void ShowBlockNotification( NotificationBox notificationControl, string message, NotificationBoxType notificationType = NotificationBoxType.Info )
        {
            notificationControl.Text = message;
            notificationControl.NotificationBoxType = notificationType;
        }

        private void ShowBlockError( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Danger );
        }

        private void ShowBlockException( NotificationBox notificationControl, Exception ex, bool writeToLog = true )
        {
            ShowBlockNotification( notificationControl, ex.Message, NotificationBoxType.Danger );

            if ( writeToLog )
            {
                LogException( ex );
            }
        }

        private void ShowBlockSuccess( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Success );
        }

        #endregion Block Notification Messages

        #region Internal Methods

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        public ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory()
        {
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );
            var isYearly = reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year;
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            var successDateQuery = GetChartQuery().Select( saa => saa.AchievementAttemptEndDateTime.Value );

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
                successDateQuery = successDateQuery.Where( d => d >= startDate );
            }

            if ( endDate.HasValue )
            {
                endDate = endDate.Value.Date.AddDays( 1 );
                successDateQuery = successDateQuery.Where( d => d < endDate );
            }

            Func<DateTime, DateTime> groupByExpression;

            if ( isYearly )
            {
                groupByExpression = dt => new DateTime( dt.Year, dt.Month, 1 );
            }
            else
            {
                groupByExpression = dt => dt.Date;
            }

            var groupedSuccessData = successDateQuery.ToList().GroupBy( groupByExpression );

            // Initialize a new Chart Factory.
            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>
            {
                TimeScale = isYearly ? ChartJsTimeSeriesTimeScaleSpecifier.Month : ChartJsTimeSeriesTimeScaleSpecifier.Day,
                StartDateTime = startDate,
                EndDateTime = endDate,
                ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line
            };

            // Add data series for success
            factory.Datasets.Add( new ChartJsTimeSeriesDataset
            {
                Name = "Successful",
                BorderColor = ChartJsConstants.Colors.Green,
                DataPoints = groupedSuccessData.Select( g => ( IChartJsTimeSeriesDataPoint ) new ChartJsTimeSeriesDataPoint
                {
                    DateTime = g.Key,
                    Value = g.Count()
                } ).ToList()
            } );

            return factory;
        }

        /// <summary>
        /// Refresh the chart using the current filter settings.
        /// </summary>
        private void RefreshChart()
        {
            var query = GetChartQuery();
            var hasData = query != null && query.Any();
            pnlActivitySummary.Visible = hasData;

            if ( !hasData )
            {
                return;
            }

            // Get chart data and set visibility of related elements.
            var chartFactory = GetChartJsFactory();
            chartCanvas.Visible = chartFactory.HasData;
            nbActivityChartMessage.Visible = !chartFactory.HasData;

            if ( !chartFactory.HasData )
            {
                // If no data, show a notification.
                nbActivityChartMessage.Text = "There are no Attempts matching the current filter.";
                return;
            }

            // Add client script to construct the chart.
            var chartDataJson = chartFactory.GetJson(
                sizeToFitContainerWidth: true,
                maintainAspectRatio: false,
                displayLegend: false );

            string script = string.Format( @"
            var canvasElement = $('#{0}')[0];
            if (canvasElement) {{
                var barCtx = canvasElement.getContext('2d');
                new Chart(barCtx, {1});
            }}", chartCanvas.ClientID, chartDataJson );

            ScriptManager.RegisterStartupScript( Page, GetType(), "attemptActivityChartScript", script, true );
        }

        /// <summary>
        /// Renders the component attribute controls.
        /// </summary>
        private void RenderComponentAttributeControls()
        {
            var achievementType = GetAchievementType() ?? new StreakTypeAchievementType();

            if ( IsAddMode() )
            {
                achievementType.AchievementEntityTypeId = cpAchievementComponent.SelectedEntityTypeId ?? 0;
            }

            var componentEntityType = EntityTypeCache.Get( achievementType.AchievementEntityTypeId );

            if ( componentEntityType != null )
            {
                var component = AchievementContainer.GetComponent( componentEntityType.Name );

                if ( component != null )
                {
                    lComponentDescription.Text = component.Description;
                }
            }

            achievementType.LoadAttributes();
            avcComponentAttributes.ExcludedAttributes = achievementType.Attributes.Values
                .Where( a => a.Key == "Order" || a.Key == "Active" ).ToArray();
            avcComponentAttributes.AddEditControls( achievementType, true );
        }

        /// <summary>
        /// Synchronizes the step controls.
        /// </summary>
        private void SyncStepControls()
        {
            var stepProgramId = spstStepType.StepProgramId;

            cbAddStep.Checked = stepProgramId.HasValue;
            divStepControls.Visible = stepProgramId.HasValue;
            sspStepStatus.Visible = stepProgramId.HasValue;
            sspStepStatus.StepProgramId = stepProgramId;
        }

        /// <summary>
        /// Synchronizes the over achievement and maximum controls.
        /// </summary>
        private void SyncOverAchievementAndMaxControls()
        {
            if ( cbAllowOverachievement.Checked )
            {
                nbMaxAccomplishments.IntegerValue = 1;
                nbMaxAccomplishments.Visible = false;
            }
            else
            {
                nbMaxAccomplishments.Visible = true;
            }
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var achievementType = GetAchievementType();

            if ( achievementType != null )
            {
                if ( !achievementType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var achievementTypeService = GetAchievementTypeService();
                var errorMessage = string.Empty;

                if ( !achievementTypeService.CanDelete( achievementType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                achievementTypeService.Delete( achievementType );
                GetRockContext().SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            var rockContext = GetRockContext();
            var achievementType = GetAchievementType();
            var achievementTypeService = GetAchievementTypeService();
            var isNew = achievementType == null;

            if ( isNew )
            {
                achievementType = new StreakTypeAchievementType();
                achievementTypeService.Add( achievementType );

                var streakTypeCache = GetStreakTypeCache();
                achievementType.StreakTypeId = streakTypeCache != null ? streakTypeCache.Id : ( stpStreakType.SelectedValueAsInt() ?? 0 );
                achievementType.AchievementEntityTypeId = cpAchievementComponent.SelectedEntityTypeId ?? 0;
            }

            achievementType.Name = tbName.Text;
            achievementType.Description = tbDescription.Text;
            achievementType.IsActive = cbActive.Checked;
            achievementType.AchievementIconCssClass = tbIconCssClass.Text;
            achievementType.MaxAccomplishmentsAllowed = cbAllowOverachievement.Checked ? 1 : nbMaxAccomplishments.IntegerValue;
            achievementType.AllowOverAchievement = cbAllowOverachievement.Checked;
            achievementType.AchievementStartWorkflowTypeId = wtpStartWorkflowType.SelectedValueAsInt();
            achievementType.AchievementSuccessWorkflowTypeId = wtpSuccessWorkflowType.SelectedValueAsInt();
            achievementType.AchievementFailureWorkflowTypeId = wtpFailureWorkflowType.SelectedValueAsInt();
            achievementType.BadgeLavaTemplate = ceBadgeLava.Text;
            achievementType.ResultsLavaTemplate = ceResultsLava.Text;
            achievementType.CategoryId = cpCategory.SelectedValueAsInt();

            // Both step type and status are required together or neither can be set
            var stepTypeId = spstStepType.StepTypeId;
            var stepStatusId = sspStepStatus.SelectedValueAsInt();

            if ( cbAddStep.Checked && stepTypeId.HasValue && stepStatusId.HasValue )
            {
                achievementType.AchievementStepTypeId = stepTypeId;
                achievementType.AchievementStepStatusId = stepStatusId;
            }
            else
            {
                achievementType.AchievementStepTypeId = null;
                achievementType.AchievementStepStatusId = null;
            }

            // Upsert Prerequisites
            var prerequisiteService = new StreakTypeAchievementTypePrerequisiteService( rockContext );
            var selectedPrerequisiteAchievementTypeIds = cblPrerequsities.SelectedValuesAsInt;

            // Remove existing prerequisites that are not selected
            var removePrerequisiteAchievementTypes = achievementType.Prerequisites
                .Where( statp => !selectedPrerequisiteAchievementTypeIds.Contains( statp.PrerequisiteStreakTypeAchievementTypeId ) ).ToList();

            foreach ( var prerequisite in removePrerequisiteAchievementTypes )
            {
                achievementType.Prerequisites.Remove( prerequisite );
                prerequisiteService.Delete( prerequisite );
            }

            // Add selected achievement types prerequisites that are not existing
            var addPrerequisiteAchievementTypeIds = selectedPrerequisiteAchievementTypeIds
                .Where( statId => !achievementType.Prerequisites.Any( statp => statp.PrerequisiteStreakTypeAchievementTypeId == statId ) );

            foreach ( var prerequisiteAchievementTypeId in addPrerequisiteAchievementTypeIds )
            {
                achievementType.Prerequisites.Add( new StreakTypeAchievementTypePrerequisite
                {
                    StreakTypeAchievementTypeId = achievementType.Id,
                    PrerequisiteStreakTypeAchievementTypeId = prerequisiteAchievementTypeId
                } );
            }

            // Validate Prerequisites.
            // This is necessary because other Achievement Types may have been modified after this record edit was started.
            if ( !isNew )
            {
                var achievementTypeCache = GetAchievementTypeCache();
                var eligibleAchievementTypes = StreakTypeAchievementTypeService.GetEligiblePrerequisiteAchievementTypeCaches( achievementTypeCache );

                foreach ( var prerequisite in achievementType.Prerequisites )
                {
                    if ( !eligibleAchievementTypes.Any( stat => stat.Id == prerequisite.PrerequisiteStreakTypeAchievementTypeId ) )
                    {
                        cvCustomValidator.IsValid = false;
                        cvCustomValidator.ErrorMessage = string.Format(
                            "This achievement type cannot have prerequisite \"{0}\" because it would create a circular dependency.",
                            prerequisite.PrerequisiteStreakTypeAchievementType.Name );
                        return;
                    }
                }
            }

            if ( !achievementType.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            try
            {
                rockContext.SaveChanges();

                if ( !achievementType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    achievementType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !achievementType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    achievementType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !achievementType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    achievementType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }

            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            achievementType.LoadAttributes( rockContext );
            avcComponentAttributes.GetEditValues( achievementType );
            achievementType.SaveAttributeValues( rockContext );

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.StreakTypeAchievementTypeId, achievementType.Id.ToString() }
            } );
        }

        /// <summary>
        /// This method satisfies the IDetailBlock requirement
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            nbEditModeMessage.Text = string.Empty;
            SetIcon();
            SetLabels();

            if ( IsAddMode() )
            {
                ShowAddMode();
            }
            else if ( IsEditMode() )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
            else
            {
                nbEditModeMessage.Text = "The achievement type was not found";
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing achievement type
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode() )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            stpStreakType.Visible = false; // Cannot change the streak type
            cpAchievementComponent.Enabled = false; // Cannot change the component type

            var achievementTypeCache = GetAchievementTypeCache();
            lReadOnlyTitle.Text = achievementTypeCache.Name.FormatAsHtmlTitle();

            var achievementType = GetAchievementType();

            tbName.Text = achievementType.Name;
            tbDescription.Text = achievementType.Description;
            spstStepType.StepTypeId = achievementType.AchievementStepTypeId;
            cbActive.Checked = achievementType.IsActive;
            cbAllowOverachievement.Checked = achievementType.AllowOverAchievement;
            nbMaxAccomplishments.IntegerValue = achievementType.MaxAccomplishmentsAllowed;
            cpAchievementComponent.SetValue( achievementType.AchievementEntityType.Guid.ToString().ToUpper() );
            tbIconCssClass.Text = achievementType.AchievementIconCssClass;
            stpStreakType.SetValue( achievementType.StreakTypeId );
            ceResultsLava.Text = achievementType.ResultsLavaTemplate;
            ceBadgeLava.Text = achievementType.BadgeLavaTemplate;
            cpCategory.SetValue( achievementType.CategoryId );

            // Workflows
            wtpStartWorkflowType.SetValue( achievementType.AchievementStartWorkflowTypeId );
            wtpFailureWorkflowType.SetValue( achievementType.AchievementFailureWorkflowTypeId );
            wtpSuccessWorkflowType.SetValue( achievementType.AchievementSuccessWorkflowTypeId );

            // Steps
            spstStepType.StepTypeId = achievementType.AchievementStepTypeId;
            SyncStepControls();
            sspStepStatus.SetValue( achievementType.AchievementStepStatusId );

            /*
            achievementType.CategoryId = tbName.Text;
            */

            SyncOverAchievementAndMaxControls();
            RenderComponentAttributeControls();
            SyncPrerequisiteList();
        }

        /// <summary>
        /// Show the mode where a user can add a new achievement type
        /// </summary>
        private void ShowAddMode()
        {
            if ( !IsAddMode() )
            {
                return;
            }

            StreakTypePicker.LoadDropDownItems( stpStreakType, false );

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );
            spstStepType.Enabled = true; // Can only set this when adding a new achievement type

            lReadOnlyTitle.Text = ActionTitle.Add( StreakTypeAchievementType.FriendlyTypeName ).FormatAsHtmlTitle();

            nbMaxAccomplishments.IntegerValue = 1;
            cbActive.Checked = true;

            var streakTypeCache = GetStreakTypeCache();
            stpStreakType.Visible = streakTypeCache == null;

            if ( streakTypeCache != null )
            {
                stpStreakType.SetValue( streakTypeCache.Id.ToString() );
            }

            SyncStepControls();
            SyncOverAchievementAndMaxControls();
            RenderComponentAttributeControls();
            SyncPrerequisiteList();
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing achievement type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            btnEdit.Visible = canEdit;
            btnDelete.Visible = canEdit;

            var achievementTypeCache = GetAchievementTypeCache();
            lReadOnlyTitle.Text = achievementTypeCache.Name.FormatAsHtmlTitle();

            lDescription.Text = achievementTypeCache.Description;

            RefreshChart();
        }

        /// <summary>
        /// Sets the icon.
        /// </summary>
        private void SetIcon()
        {
            var achievementTypeCache = GetAchievementTypeCache();

            if ( achievementTypeCache == null || achievementTypeCache.AchievementIconCssClass.IsNullOrWhiteSpace() )
            {
                lIcon.Text = string.Format( "<i class='{0}'></i>", "fa fa-medal" );
            }
            else
            {
                lIcon.Text = string.Format( "<i class='{0}'></i>", achievementTypeCache.AchievementIconCssClass );
            }
        }

        /// <summary>
        /// Sets the labels.
        /// </summary>
        private void SetLabels()
        {
            var achievementTypeCache = GetAchievementTypeCache();
            var isActive = achievementTypeCache == null || achievementTypeCache.IsActive;
            hlInactive.Visible = !isActive;

            var categoryCache = achievementTypeCache == null ? null : achievementTypeCache.Category;

            if ( categoryCache != null)
            {
                var style = categoryCache.HighlightColor.IsNullOrWhiteSpace() ? string.Empty : string.Format( "style='background-color: {0};'", categoryCache.HighlightColor );
                var text = string.Format( "<i class='{0}'></i> {1}", categoryCache.IconCssClass, categoryCache.Name );
                lCategory.Text = string.Format( "<span class='label label-default' {0}>{1}</span>", style, text );
            }
        }

        /// <summary>
        /// Loads the prerequisite list.
        /// </summary>
        private void SyncPrerequisiteList()
        {
            var streakTypeCache = GetStreakTypeCache();
            var achievementTypeCache = GetAchievementTypeCache();
            var isNew = IsAddMode();

            var eligiblePrerequisites = isNew ?
                StreakTypeAchievementTypeService.GetEligiblePrerequisiteAchievementTypeCaches( streakTypeCache ) :
                StreakTypeAchievementTypeService.GetEligiblePrerequisiteAchievementTypeCaches( achievementTypeCache );

            cblPrerequsities.DataSource = eligiblePrerequisites;
            cblPrerequsities.DataBind();
            cblPrerequsities.Visible = eligiblePrerequisites.Any();

            if ( !isNew )
            {
                cblPrerequsities.SetValues( achievementTypeCache.Prerequisites.Select( statp => statp.PrerequisiteStreakTypeAchievementTypeId ) );
            }
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the achievement type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetAchievementType() != null;
        }

        /// <summary>
        /// Can the user add a new achievement type
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetAchievementType() == null;
        }

        /// <summary>
        /// Is this block currently adding a new achievement type
        /// </summary>
        /// <returns></returns>
        private bool IsAddMode()
        {
            return CanAdd();
        }

        /// <summary>
        /// Is this block currently editing an existing achievement type
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return CanEdit() && hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing information about a achievement type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetAchievementType() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual achievement type model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementType GetAchievementType()
        {
            if ( _achievementType == null )
            {
                var achievementTypeId = PageParameter( PageParameterKey.StreakTypeAchievementTypeId ).AsIntegerOrNull();

                if ( achievementTypeId.HasValue && achievementTypeId.Value > 0 )
                {
                    var achievementTypeService = GetAchievementTypeService();
                    _achievementType = achievementTypeService.Queryable( "AchievementEntityType, Prerequisites" )
                        .FirstOrDefault( stat => stat.Id == achievementTypeId.Value );
                }
            }

            return _achievementType;
        }
        private StreakTypeAchievementType _achievementType = null;

        /// <summary>
        /// Gets the achievement type cache.
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementTypeCache GetAchievementTypeCache()
        {
            return StreakTypeAchievementTypeCache.Get( PageParameter( PageParameterKey.StreakTypeAchievementTypeId ).AsInteger() );
        }

        /// <summary>
        /// Gets the achievement type cache.
        /// </summary>
        /// <returns></returns>
        private StreakTypeCache GetStreakTypeCache()
        {
            var achievementTypeCache = GetAchievementTypeCache();

            if ( achievementTypeCache != null )
            {
                return achievementTypeCache.StreakTypeCache;
            }

            return StreakTypeCache.Get( PageParameter( PageParameterKey.StreakTypeId ).AsInteger() );
        }

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the achievement type service
        /// </summary>
        /// <returns></returns>
        private StreakTypeAchievementTypeService GetAchievementTypeService()
        {
            if ( _achievementTypeService == null )
            {
                var rockContext = GetRockContext();
                _achievementTypeService = new StreakTypeAchievementTypeService( rockContext );
            }

            return _achievementTypeService;
        }
        private StreakTypeAchievementTypeService _achievementTypeService = null;

        /// <summary>
        /// Gets the attempt service.
        /// </summary>
        /// <returns></returns>
        private StreakAchievementAttemptService GetAttemptService()
        {
            if ( _streakAchievementAttemptService == null )
            {
                var rockContext = GetRockContext();
                _streakAchievementAttemptService = new StreakAchievementAttemptService( rockContext );
            }

            return _streakAchievementAttemptService;
        }
        private StreakAchievementAttemptService _streakAchievementAttemptService = null;

        /// <summary>
        /// Gets the attempts query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<StreakAchievementAttempt> GetAttemptsQuery()
        {
            var achievementType = GetAchievementType();
            var attemptService = GetAttemptService();
            var query = attemptService.Queryable().AsNoTracking().Where( saa => saa.StreakTypeAchievementTypeId == achievementType.Id );
            return query;
        }

        /// <summary>
        /// Gets the attempts query for the chart. Only successes.
        /// </summary>
        /// <returns></returns>
        private IQueryable<StreakAchievementAttempt> GetChartQuery()
        {
            var achievementType = GetAchievementType();
            var attemptService = GetAttemptService();
            var query = attemptService.Queryable().AsNoTracking().Where( saa =>
                saa.StreakTypeAchievementTypeId == achievementType.Id &&
                saa.IsSuccessful &&
                saa.AchievementAttemptEndDateTime.HasValue );
            return query;
        }

        #endregion Data Interface Methods
    }
}