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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Metric Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of the given metric." )]

    [BooleanField( "Show Chart", DefaultValue = "true" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [SlidingDateRangeField( "Chart Date Range", Key = "SlidingDateRange", DefaultValue = "-1||||" )]
    [BooleanField( "Combine Chart Series" )]
    public partial class MetricDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Metric.FriendlyTypeName );

            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Metric ) ).Id;

            // Metric supports 0 or more Categories, so the entityType is actually MetricCategory, not Metric
            cpMetricCategories.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.MetricCategory ) ).Id;

            lcMetricsChart.Options.SetChartStyle( GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
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
                // in case called normally
                int? metricId = PageParameter( "MetricId" ).AsIntegerOrNull();

                // in case called from CategoryTreeView
                int? metricCategoryId = PageParameter( "MetricCategoryId" ).AsIntegerOrNull();
                MetricCategory metricCategory = null;
                if ( metricCategoryId.HasValue )
                {
                    if ( metricCategoryId.Value > 0 )
                    {
                        // editing a metric, but get the metricId from the metricCategory
                        metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId.Value );
                        if ( metricCategory != null )
                        {
                            hfMetricCategoryId.Value = metricCategory.Id.ToString();
                            metricId = metricCategory.MetricId;
                        }
                    }
                    else
                    {
                        if ( !metricId.HasValue )
                        {
                            // adding a new metric
                            metricId = 0;
                        }
                    }
                }

                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();

                if ( metricId.HasValue )
                {
                    ShowDetail( metricId.Value, parentCategoryId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ////
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Metric metric;

            var rockContext = new RockContext();
            MetricService metricService = new MetricService( rockContext );
            MetricCategoryService metricCategoryService = new MetricCategoryService( rockContext );
            MetricValueService metricValueService = new MetricValueService( rockContext );
            bool deleteValuesOnSave = sender == btnDeleteValuesAndSave;

            int metricId = hfMetricId.Value.AsInteger();

            if ( metricId == 0 )
            {
                metric = new Metric();
            }
            else
            {
                metric = metricService.Get( metricId );
            }

            metric.Title = tbTitle.Text;
            metric.Subtitle = tbSubtitle.Text;
            metric.Description = tbDescription.Text;
            metric.IconCssClass = tbIconCssClass.Text;
            metric.SourceValueTypeId = ddlSourceType.SelectedValueAsId();
            metric.XAxisLabel = tbXAxisLabel.Text;
            metric.YAxisLabel = tbYAxisLabel.Text;
            metric.IsCumulative = cbIsCumulative.Checked;

            var origEntityType = metric.EntityTypeId.HasValue ? EntityTypeCache.Read( metric.EntityTypeId.Value ) : null;
            var newEntityType = etpEntityType.SelectedEntityTypeId.HasValue ? EntityTypeCache.Read( etpEntityType.SelectedEntityTypeId.Value ) : null;
            if ( origEntityType != null && !deleteValuesOnSave )
            {
                if ( newEntityType == null || newEntityType.Id != origEntityType.Id )
                {
                    // if the EntityTypeId of this metric has changed to NULL or to another EntityType, warn about the EntityId values being wrong
                    bool hasEntityValues = metricValueService.Queryable().Any( a => a.MetricId == metric.Id && a.EntityId.HasValue );

                    if ( hasEntityValues )
                    {
                        nbEntityTypeChanged.Text = string.Format(
                            "Warning: You can't change the series partition to {0} when there are values associated with {1}. Do you want to delete existing values?",
                            newEntityType != null ? newEntityType.FriendlyName : "<none>",
                            origEntityType.FriendlyName );
                        mdEntityTypeChanged.Show();
                        nbEntityTypeChanged.Visible = true;
                        return;
                    }
                }
            }

            metric.EntityTypeId = etpEntityType.SelectedEntityTypeId;

            int sourceTypeDataView = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid() ).Id;
            int sourceTypeSQL = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid() ).Id;

            var personService = new PersonService( rockContext );
            var metricChampionPerson = personService.Get( ppMetricChampionPerson.SelectedValue ?? 0 );
            metric.MetricChampionPersonAliasId = metricChampionPerson != null ? metricChampionPerson.PrimaryAliasId : null;
            var adminPerson = personService.Get( ppAdminPerson.SelectedValue ?? 0 );
            metric.AdminPersonAliasId = adminPerson != null ? adminPerson.PrimaryAliasId : null;

            if ( metric.SourceValueTypeId == sourceTypeSQL )
            {
                metric.SourceSql = ceSourceSql.Text;
            }
            else
            {
                metric.SourceSql = string.Empty;
            }

            if ( metric.SourceValueTypeId == sourceTypeDataView )
            {
                metric.DataViewId = ddlDataView.SelectedValueAsId();
            }
            else
            {
                metric.DataViewId = null;
            }

            if ( rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.NamedSchedule )
            {
                metric.ScheduleId = ddlSchedule.SelectedValueAsId();
            }
            else
            {
                metric.ScheduleId = hfUniqueScheduleId.ValueAsInt();
            }

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !metric.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            if ( !cpMetricCategories.SelectedValuesAsInt().Any() )
            {
                cpMetricCategories.ShowErrorMessage( "Must select at least one category" );
                return;
            }

            // do a WrapTransaction since we are doing multiple SaveChanges()
            rockContext.WrapTransaction( () =>
            {
                var scheduleService = new ScheduleService( rockContext );
                var schedule = scheduleService.Get( metric.ScheduleId ?? 0 );
                int metricScheduleCategoryId = new CategoryService( rockContext ).Get( Rock.SystemGuid.Category.SCHEDULE_METRICS.AsGuid() ).Id;
                if ( schedule == null )
                {
                    schedule = new Schedule();

                    // make it an "Unnamed" metrics schedule
                    schedule.Name = string.Empty;
                    schedule.CategoryId = metricScheduleCategoryId;
                }

                schedule.iCalendarContent = sbSchedule.iCalendarContent;

                if ( schedule.IsEmptySchedule() )
                {
                    schedule = null;
                }
                else
                {
                    if ( schedule.Id == 0 )
                    {
                        scheduleService.Add( schedule );

                        // save to make sure we have a scheduleId
                        rockContext.SaveChanges();
                    }
                }

                if ( schedule != null )
                {
                    metric.ScheduleId = schedule.Id;
                }
                else
                {
                    metric.ScheduleId = null;
                }

                if ( metric.Id == 0 )
                {
                    metricService.Add( metric );

                    // save to make sure we have a metricId
                    rockContext.SaveChanges();
                }

                // update MetricCategories for Metric            
                metric.MetricCategories = metric.MetricCategories ?? new List<MetricCategory>();
                var selectedCategoryIds = cpMetricCategories.SelectedValuesAsInt();

                // delete any categories that were removed
                foreach ( var metricCategory in metric.MetricCategories.ToList() )
                {
                    if ( !selectedCategoryIds.Contains( metricCategory.CategoryId ) )
                    {
                        metricCategoryService.Delete( metricCategory );
                    }
                }

                // add any categories that were added
                foreach ( int categoryId in selectedCategoryIds )
                {
                    if ( !metric.MetricCategories.Any( a => a.CategoryId == categoryId ) )
                    {
                        metricCategoryService.Add( new MetricCategory { CategoryId = categoryId, MetricId = metric.Id } );
                    }
                }

                rockContext.SaveChanges();

                // delete MetricValues associated with the old entityType if they confirmed the EntityType change
                if ( deleteValuesOnSave )
                {
                    metricValueService.DeleteRange( metricValueService.Queryable().Where( a => a.MetricId == metric.Id && a.EntityId.HasValue ) );

                    // since there could be 1000s of values that got deleted, do a SaveChanges that skips PrePostProcessing
                    rockContext.SaveChanges( true );
                }

                // delete any orphaned Unnamed metric schedules
                var metricIdSchedulesQry = metricService.Queryable().Select( a => a.ScheduleId );
                int? metricScheduleId = schedule != null ? schedule.Id : (int?)null;
                var orphanedSchedules = scheduleService.Queryable()
                    .Where( a => a.CategoryId == metricScheduleCategoryId && a.Name == string.Empty && a.Id != ( metricScheduleId ?? 0 ) )
                    .Where( s => !metricIdSchedulesQry.Any( m => m == s.Id ) );
                foreach ( var item in orphanedSchedules )
                {
                    scheduleService.Delete( item );
                }

                if ( orphanedSchedules.Any() )
                {
                    rockContext.SaveChanges();
                }
            } );

            var qryParams = new Dictionary<string, string>();
            qryParams["MetricId"] = metric.Id.ToString();
            if ( hfMetricCategoryId.ValueAsInt() == 0 )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                int? metricCategoryId = new MetricCategoryService( new RockContext() ).Queryable().Where( a => a.MetricId == metric.Id && a.CategoryId == parentCategoryId ).Select( a => a.Id ).FirstOrDefault();
                hfMetricCategoryId.Value = metricCategoryId.ToString();
            }

            qryParams["MetricCategoryId"] = hfMetricCategoryId.Value;

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfMetricId.Value.Equals( "0" ) )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                MetricService metricService = new MetricService( new RockContext() );
                Metric metric = metricService.Get( hfMetricId.Value.AsInteger() );
                ShowReadonlyDetails( metric );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            MetricService metricService = new MetricService( new RockContext() );
            Metric metric = metricService.Get( hfMetricId.Value.AsInteger() );
            ShowEditDetails( metric );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            MetricService metricService = new MetricService( rockContext );
            Metric metric = metricService.Get( hfMetricId.Value.AsInteger() );

            // intentionally get metricCategory with new RockContext() so we don't confuse SaveChanges()
            int? parentCategoryId = null;
            var metricCategory = new MetricCategoryService( new RockContext() ).Get( hfMetricCategoryId.ValueAsInt() );
            if ( metricCategory != null )
            {
                parentCategoryId = metricCategory.CategoryId;
            }

            if ( metric != null )
            {
                string errorMessage;
                if ( !metricService.CanDelete( metric, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                metricService.Delete( metric );
                rockContext.SaveChanges();
            }

            var qryParams = new Dictionary<string, string>();
            if ( parentCategoryId != null )
            {
                qryParams["CategoryId"] = parentCategoryId.ToString();
            }

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelForEntityTypeChange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelForEntityTypeChange_Click( object sender, EventArgs e )
        {
            mdEntityTypeChanged.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSourceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSourceType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? sourceValueTypeId = ddlSourceType.SelectedValueAsId();
            var sourceValueType = DefinedValueCache.Read( sourceValueTypeId ?? 0 );
            ceSourceSql.Visible = false;
            ddlDataView.Visible = false;
            if ( sourceValueType != null )
            {
                ceSourceSql.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();
                ddlDataView.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();

                // only show LastRun label if SourceValueType is not Manual
                ltLastRunDateTime.Visible = sourceValueType.Guid != Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScheduleSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblScheduleSelect_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlSchedule.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.NamedSchedule;
            sbSchedule.Visible = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>() == ScheduleSelectionType.Unique;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        public void ShowDetail( int metricId )
        {
            ShowDetail( metricId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( int metricId, int? parentCategoryId )
        {
            pnlDetails.Visible = false;

            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            Metric metric = null;

            if ( !metricId.Equals( 0 ) )
            {
                metric = metricService.Get( metricId );
            }

            if ( metric == null )
            {
                metric = new Metric { Id = 0, IsSystem = false };
                metric.SourceValueTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
                metric.MetricCategories = new List<MetricCategory>();
                if ( parentCategoryId.HasValue )
                {
                    var metricCategory = new MetricCategory { CategoryId = parentCategoryId.Value };
                    metricCategory.Category = metricCategory.Category ?? new CategoryService( rockContext ).Get( metricCategory.CategoryId );
                    metric.MetricCategories.Add( metricCategory );
                }
            }

            if ( !metric.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfMetricId.Value = metric.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( metric.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Metric.FriendlyTypeName );
            }

            btnSecurity.Visible = metric.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = metric.Title;
            btnSecurity.EntityId = metric.Id;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( metric );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Visible = metricService.CanDelete( metric, out errorMessage );
                if ( metric.Id > 0 )
                {
                    ShowReadonlyDetails( metric );
                }
                else
                {
                    ShowEditDetails( metric );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="metric">The metric.</param>
        public void ShowEditDetails( Metric metric )
        {
            if ( metric.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Metric.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = ActionTitle.Edit( metric.Title ).FormatAsHtmlTitle();
            }

            SetEditMode( true );
            LoadDropDowns();

            tbTitle.Text = metric.Title;
            tbSubtitle.Text = metric.Subtitle;
            tbDescription.Text = metric.Description;
            tbIconCssClass.Text = metric.IconCssClass;
            cpMetricCategories.SetValues( metric.MetricCategories.Select( a => a.Category ) );

            int manualSourceType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;

            ddlSourceType.SetValue( metric.SourceValueTypeId ?? manualSourceType );
            tbXAxisLabel.Text = metric.XAxisLabel;
            tbYAxisLabel.Text = metric.YAxisLabel;
            cbIsCumulative.Checked = metric.IsCumulative;
            etpEntityType.SelectedEntityTypeId = metric.EntityTypeId;
            ppMetricChampionPerson.SetValue( metric.MetricChampionPersonAlias != null ? metric.MetricChampionPersonAlias.Person : null );
            ppAdminPerson.SetValue( metric.AdminPersonAlias != null ? metric.AdminPersonAlias.Person : null );
            ceSourceSql.Text = metric.SourceSql;
            ceSourceSql.Help = @"Specify the SQL that will return the Y Value for each scheduled Date. Example: <pre>SELECT COUNT(*) FROM [Attendance] </pre> 
If this is a metric is partitioned, the 2nd column will be the EntityId value. Example: <pre>SELECT COUNT(*), [CampusId] FROM [Attendance] GROUP BY [CampusId] </pre>
The SQL can include Lava merge fields:";
            ceSourceSql.Help += metric.GetMergeObjects( RockDateTime.Now ).lavaDebugInfo();

            if ( metric.Schedule != null )
            {
                sbSchedule.iCalendarContent = metric.Schedule.iCalendarContent;
                if ( metric.Schedule.Name != string.Empty )
                {
                    // Named Schedule
                    rblScheduleSelect.SelectedValue = ScheduleSelectionType.NamedSchedule.ConvertToInt().ToString();
                    sbSchedule.iCalendarContent = null;
                    ddlSchedule.SelectedValue = metric.ScheduleId.ToString();
                }
                else
                {
                    // Unique Schedule (specific to this Metric)
                    rblScheduleSelect.SelectedValue = ScheduleSelectionType.Unique.ConvertToInt().ToString();
                    sbSchedule.iCalendarContent = metric.Schedule.iCalendarContent;

                    // set the hidden field for the scheduleId of the Unique schedule so we don't overwrite a named schedule
                    hfUniqueScheduleId.Value = metric.ScheduleId.ToString();
                }
            }
            else
            {
                sbSchedule.iCalendarContent = null;
                rblScheduleSelect.SelectedValue = rblScheduleSelect.SelectedValue = ScheduleSelectionType.Unique.ConvertToInt().ToString();
            }

            if ( metric.LastRunDateTime != null )
            {
                ltLastRunDateTime.LabelType = LabelType.Success;
                ltLastRunDateTime.Text = "Last Run: " + metric.LastRunDateTime.ToString();
            }
            else
            {
                ltLastRunDateTime.LabelType = LabelType.Warning;
                ltLastRunDateTime.Text = "Never Run";
            }

            ddlDataView.SetValue( metric.DataViewId );

            // make sure the control visibility is set based on SourceType
            ddlSourceType_SelectedIndexChanged( null, new EventArgs() );

            // make sure the control visibility is set based on Schedule Selection Type
            rblScheduleSelect_SelectedIndexChanged( null, new EventArgs() );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="metric">The metric.</param>
        private void ShowReadonlyDetails( Metric metric )
        {
            SetEditMode( false );
            hfMetricId.SetValue( metric.Id );

            lcMetricsChart.Visible = GetAttributeValue( "ShowChart" ).AsBooleanOrNull() ?? true;

            var chartDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "SlidingDateRange" ) ?? "-1||" );
            lcMetricsChart.StartDate = chartDateRange.Start;
            lcMetricsChart.EndDate = chartDateRange.End;
            lcMetricsChart.MetricId = metric.Id;
            lcMetricsChart.CombineValues = GetAttributeValue( "CombineChartSeries" ).AsBooleanOrNull() ?? false;

            lReadOnlyTitle.Text = metric.Title.FormatAsHtmlTitle();

            DescriptionList descriptionListMain = new DescriptionList();

            descriptionListMain.Add( "Subtitle", metric.Subtitle );
            descriptionListMain.Add( "Description", metric.Description );

            if ( metric.MetricCategories != null && metric.MetricCategories.Any() )
            {
                descriptionListMain.Add( "Categories", metric.MetricCategories.Select( s => s.Category.ToString() ).OrderBy( o => o ).ToList().AsDelimited( "," ) );
            }

            // only show LastRun label if SourceValueType is not Manual
            ltLastRunDateTime.Visible = metric.SourceValueTypeId != DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;

            if ( metric.LastRunDateTime != null )
            {
                ltLastRunDateTime.LabelType = LabelType.Success;
                ltLastRunDateTime.Text = "Last Run: " + metric.LastRunDateTime.ToString();
            }
            else
            {
                ltLastRunDateTime.LabelType = LabelType.Warning;
                ltLastRunDateTime.Text = "Never Run";
            }

            lblMainDetails.Text = descriptionListMain.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            RockContext rockContext = new RockContext();
            ddlDataView.Items.Clear();
            var dataviewList = new DataViewService( rockContext ).Queryable().Select(
                s => new
                {
                    s.Id,
                    s.Name
                } ).OrderBy( a => a.Name ).ToList();

            foreach ( var item in dataviewList )
            {
                ddlDataView.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            ddlSourceType.Items.Clear();
            foreach ( var item in new DefinedValueService( rockContext ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.METRIC_SOURCE_TYPE.AsGuid() ) )
            {
                ddlSourceType.Items.Add( new ListItem( item.Value, item.Id.ToString() ) );
            }

            rblScheduleSelect.Items.Clear();
            rblScheduleSelect.Items.Add( new ListItem( ScheduleSelectionType.Unique.ConvertToString(), ScheduleSelectionType.Unique.ConvertToInt().ToString() ) );
            rblScheduleSelect.Items.Add( new ListItem( ScheduleSelectionType.NamedSchedule.ConvertToString(), ScheduleSelectionType.NamedSchedule.ConvertToInt().ToString() ) );

            var scheduleCategoryId = new CategoryService( rockContext ).Get( Rock.SystemGuid.Category.SCHEDULE_METRICS.AsGuid() ).Id;
            var scheduleCategories = new ScheduleService( rockContext ).Queryable()
                .Where( a => a.CategoryId == scheduleCategoryId && a.Name != string.Empty )
                .OrderBy( a => a.Name ).ToList();

            ddlSchedule.Items.Clear();
            foreach ( var item in scheduleCategories )
            {
                ddlSchedule.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            // limit to EntityTypes that support picking a Value with a picker
            etpEntityType.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().OrderBy( t => t.FriendlyName ).Where( a => a.SingleValueFieldTypeId.HasValue ).ToList();
        }

        #endregion

        #region block specific enums

        /// <summary>
        /// 
        /// </summary>
        public enum ScheduleSelectionType
        {
            Unique = 0,
            NamedSchedule = 1
        }

        #endregion

    }
}