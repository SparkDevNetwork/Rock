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
using System.Reflection;
using System.Threading.Tasks;
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

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Metric Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of the given metric." )]

    #region Block Attributes

    [BooleanField(
        "Show Chart",
        Key = AttributeKey.ShowChart,
        DefaultValue = "true",
        Order = 0 )]

    [DefinedValueField(
        "Chart Style",
        Key = AttributeKey.ChartStyle,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CHART_STYLES,
        DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK,
        Order = 1 )]

    [SlidingDateRangeField(
        "Chart Date Range",
        Key = AttributeKey.SlidingDateRange,
        DefaultValue = "-1||||",
        EnabledSlidingDateRangeTypes = "Last,Previous,Current,DateRange",
        Order = 2 )]

    [BooleanField(
        "Combine Chart Series",
        Key = AttributeKey.CombineChartSeries,
        Order = 3 )]

    [LinkedPage( "Data View Page",
        Key = AttributeKey.DataViewPage,
        Description = "The page to edit data views",
        IsRequired = true,
        Order = 4 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for any SQL based operations to complete. Leave blank to use the default for this metric (300). Note, some metrics do not use SQL so this timeout will only apply to metrics that are SQL based.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 5,
        Category = "General",
        Order = 5 )]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "D77341B9-BA38-4693-884E-E5C1D908CEC4" )]
    public partial class MetricDetail : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ShowChart = "Show Chart";
            public const string SlidingDateRange = "SlidingDateRange";
            public const string CombineChartSeries = "CombineChartSeries";
            public const string ChartStyle = "ChartStyle";
            public const string DataViewPage = "DataViewPage";
            public const string CommandTimeout = "CommandTimeout";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string MetricId = "MetricId";
            public const string MetricCategoryId = "MetricCategoryId";
            public const string ParentCategoryId = "ParentCategoryId";
            public const string ExpandedIds = "ExpandedIds";
            public const string CategoryId = "CategoryId";
        }

        #endregion Page Parameter Keys

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string MetricPartitionsState = "MetricPartitionsState";
        }

        #endregion ViewStateKeys

        #region Properties

        private List<MetricPartition> MetricPartitionsState { get; set; } = new List<MetricPartition>();

        #endregion

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

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Metric ) ).Id;

            dvpDataView.Help = @"NOTE: When using DataView to populate Metrics, multiple partitions is not supported.

When using a DataView as the Source Type, the Metric Values will based on the number of records returned by the DataView when the Calculate Metrics job processes this metric.

Example: Let's say you have a DataView called 'Small Group Attendance for Last Week', and schedule this metric for every Monday. When the Calculate Metrics job runs this metric, the Metric Value for that week will be number of records returned by the Dataview.
";

            // Metric supports 0 or more Categories, so the entityType is actually MetricCategory, not Metric
            cpMetricCategories.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.MetricCategory ) ).Id;

            gMetricPartitions.Actions.ShowAdd = true;
            gMetricPartitions.GridReorder += gMetricPartitions_GridReorder;
            gMetricPartitions.Actions.AddClick += gMetricPartitions_AddClick;

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
                // in case called normally
                int? metricId = PageParameter( PageParameterKey.MetricId ).AsIntegerOrNull();

                // in case called from CategoryTreeView
                int? metricCategoryId = PageParameter( PageParameterKey.MetricCategoryId ).AsIntegerOrNull();

                if ( metricCategoryId.HasValue )
                {
                    if ( metricCategoryId.Value > 0 )
                    {
                        // editing a metric, but get the metricId from the metricCategory
                        var metricCategory = new MetricCategoryService( new RockContext() ).Get( metricCategoryId.Value );
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

                int? parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();

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

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[ViewStateKey.MetricPartitionsState] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MetricPartitionsState = new List<MetricPartition>();
            }
            else
            {
                MetricPartitionsState = JsonConvert.DeserializeObject<List<MetricPartition>>( json );
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

            ViewState[ViewStateKey.MetricPartitionsState] = JsonConvert.SerializeObject( MetricPartitionsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion Base Control Methods

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
            MetricPartitionService metricPartitionService = new MetricPartitionService( rockContext );

            int metricId = hfMetricId.Value.AsInteger();

            if ( metricId == 0 )
            {
                metric = new Metric();
            }
            else
            {
                metric = metricService.Get( metricId );

                // remove any metricPartitions that were removed in the UI
                var selectedMetricPartitionGuids = MetricPartitionsState.Select( r => r.Guid );
                foreach ( var item in metric.MetricPartitions.Where( r => !selectedMetricPartitionGuids.Contains( r.Guid ) ).ToList() )
                {
                    metric.MetricPartitions.Remove( item );
                    metricPartitionService.Delete( item );
                }
            }

            metric.MetricPartitions = metric.MetricPartitions ?? new List<MetricPartition>();

            if ( MetricPartitionsState.Count() > 1 && MetricPartitionsState.Any( a => !a.EntityTypeId.HasValue ) )
            {
                mdMetricPartitionsEntityTypeWarning.Text = "If multiple partitions are defined for a metric, all the partitions must have an EntityType assigned";
                mdMetricPartitionsEntityTypeWarning.Visible = true;
                pwMetricPartitions.Expanded = true;
                return;
            }

            mdMetricPartitionsEntityTypeWarning.Visible = false;

            foreach ( var metricPartitionState in MetricPartitionsState )
            {
                MetricPartition metricPartition = metric.MetricPartitions.Where( r => r.Guid == metricPartitionState.Guid ).FirstOrDefault();
                if ( metricPartition == null )
                {
                    metricPartition = new MetricPartition();
                    metric.MetricPartitions.Add( metricPartition );
                }
                else
                {
                    metricPartitionState.Id = metricPartition.Id;
                    metricPartitionState.Guid = metricPartition.Guid;
                }

                metricPartition.CopyPropertiesFrom( metricPartitionState );
            }

            // ensure there is at least one partition
            if ( !metric.MetricPartitions.Any() )
            {
                var metricPartition = new MetricPartition()
                {
                    EntityTypeId = null,
                    IsRequired = true,
                    Order = 0
                };

                metric.MetricPartitions.Add( metricPartition );
            }

            metric.Title = tbTitle.Text;
            metric.Subtitle = tbSubtitle.Text;
            metric.Description = tbDescription.Text;
            metric.IconCssClass = tbIconCssClass.Text;
            metric.SourceValueTypeId = ddlSourceType.SelectedValueAsId();
            metric.YAxisLabel = tbYAxisLabel.Text;
            metric.UnitType = rblUnitType.SelectedValueAsEnum<Rock.Model.UnitType>();
            metric.IsCumulative = cbIsCumulative.Checked;
            metric.EnableAnalytics = cbEnableAnalytics.Checked;

            int sourceTypeDataView = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid() ).Id;
            int sourceTypeSQL = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid() ).Id;
            int sourceTypeLava = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_LAVA.AsGuid() ).Id;

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

            if ( metric.SourceValueTypeId == sourceTypeLava )
            {
                metric.SourceLava = ceSourceLava.Text;
            }
            else
            {
                metric.SourceLava = string.Empty;
            }

            if ( metric.SourceValueTypeId == sourceTypeDataView )
            {
                metric.DataViewId = dvpDataView.SelectedValueAsId();
                metric.AutoPartitionOnPrimaryCampus = cbAutoPartionPrimaryCampus.Checked;
            }
            else
            {
                metric.DataViewId = null;
                metric.AutoPartitionOnPrimaryCampus = false;
            }

            var scheduleSelectionType = rblScheduleSelect.SelectedValueAsEnum<ScheduleSelectionType>();
            if ( scheduleSelectionType == ScheduleSelectionType.NamedSchedule )
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
                    schedule = new Schedule()
                    {
                        // make it an "Unnamed" metrics schedule
                        Name = string.Empty,
                        CategoryId = metricScheduleCategoryId
                    };
                }

                // if the schedule was a unique schedule (configured in the Metric UI, set the schedule's iCal content to the schedule builder UI's value
                if ( scheduleSelectionType == ScheduleSelectionType.Unique )
                {
                    schedule.iCalendarContent = sbSchedule.iCalendarContent;
                }

                if ( !schedule.HasSchedule() && scheduleSelectionType == ScheduleSelectionType.Unique )
                {
                    // don't save as a unique schedule if the schedule doesn't do anything
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

                // safely, add the attribute values.
                avcEditAttributeValues.GetEditValues( metric );

                // only save if everything saves:
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    metric.SaveAttributeValues();
                } );

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

                metricService.EnsureMetricAnalyticsViews();

                // delete any orphaned Unnamed metric schedules
                var metricIdSchedulesQry = metricService.Queryable().Select( a => a.ScheduleId );
                int? metricScheduleId = schedule != null ? schedule.Id : ( int? ) null;
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
            qryParams[PageParameterKey.MetricId] = metric.Id.ToString();
            if ( hfMetricCategoryId.ValueAsInt() == 0 )
            {
                int? parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
                int? metricCategoryId = new MetricCategoryService( new RockContext() ).Queryable().Where( a => a.MetricId == metric.Id && a.CategoryId == parentCategoryId ).Select( a => a.Id ).FirstOrDefault();
                hfMetricCategoryId.Value = metricCategoryId.ToString();
            }

            if ( hfMetricCategoryId.ValueAsInt() != 0 )
            {
                qryParams[PageParameterKey.MetricCategoryId] = hfMetricCategoryId.Value;
            }

            if ( PageParameter( PageParameterKey.ExpandedIds ).IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ExpandedIds] = PageParameter( PageParameterKey.ExpandedIds );
            }

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
                int? parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Canceling on Add, and we know the parentCategoryId, so we are probably in TreeView mode, so navigate to the current page.
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.CategoryId] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Canceling on Add.  Return to Grid.
                    NavigateToParentPage();
                }
            }
            else
            {
                // Canceling on Edit.  Return to Details.
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

                // since we deleted the metric, sweep thru and make sure Metric Analytics Views are OK
                metricService.EnsureMetricAnalyticsViews();
            }

            var qryParams = new Dictionary<string, string>();
            if ( parentCategoryId != null )
            {
                qryParams[PageParameterKey.CategoryId] = parentCategoryId.ToString();
            }

            qryParams[PageParameterKey.ExpandedIds] = PageParameter( PageParameterKey.ExpandedIds );

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSourceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSourceType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? sourceValueTypeId = ddlSourceType.SelectedValueAsId();
            var sourceValueType = DefinedValueCache.Get( sourceValueTypeId ?? 0 );
            pnlSQLSourceType.Visible = false;
            pnlDataviewSourceType.Visible = false;
            if ( sourceValueType != null )
            {
                pnlSQLSourceType.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();
                pnlLavaSourceType.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_LAVA.AsGuid();
                pnlDataviewSourceType.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();

                // only show LastRun label if SourceValueType is not Manual
                bool isManual = sourceValueType.Guid != Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid();
                ltLastRunDateTime.Visible = isManual;
                rcwSchedule.Visible = isManual;
            }

            ShowHideAutoPartitionPrimaryCampus();
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

        /// <summary>
        /// Handles the SelectItem event of the dvpDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpDataView_SelectItem( object sender, EventArgs e )
        {
            ShowHideAutoPartitionPrimaryCampus();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvTitle control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvTitle_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = IsTitleUnique();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnManualRun_Click( object sender, EventArgs e )
        {
            mdManualRunConfirm.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void mdManualRunConfirm_SaveClick( object sender, EventArgs e )
        {
            mdManualRunConfirm.Hide();
            mdManualRunInfo.Show( string.Format( "The manual metric for '{0}' has been started.", lReadOnlyTitle.Text ), ModalAlertType.Information );
            ManualMetricRun( hfMetricId.Value.AsInteger() );
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
                pdAuditDetails.SetEntity( metric, ResolveRockUrl( "~" ) );
            }

            if ( metric == null )
            {
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
                metric = new Metric { Id = 0, IsSystem = false };
                metric.SourceValueTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
                metric.MetricCategories = new List<MetricCategory>();
                if ( parentCategoryId.HasValue )
                {
                    var metricCategory = new MetricCategory { CategoryId = parentCategoryId.Value };
                    metricCategory.Category = metricCategory.Category ?? new CategoryService( rockContext ).Get( metricCategory.CategoryId );
                    metric.MetricCategories.Add( metricCategory );
                }

                metric.MetricPartitions = new List<MetricPartition>();
            }

            if ( !metric.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfMetricId.Value = metric.Id.ToString();
            avcDisplayAttributeValues.AddDisplayControls( metric, Rock.Security.Authorization.VIEW, CurrentPerson );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( metric.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Metric.FriendlyTypeName );
            }

            if ( !UserCanEdit && !metric.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( Metric.FriendlyTypeName );
            }

            bool canAdministrate = UserCanAdministrate || metric.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            if ( canAdministrate )
            {
                btnSecurity.Visible = true;
                btnSecurity.Title = metric.Title;
                btnSecurity.EntityId = metric.Id;

                // Manual run button is only available to Source Value Types that are NOT "Manual".
                bool sourceTypeIsManual = metric.SourceValueTypeId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
                btnManualRun.Enabled = !sourceTypeIsManual;
            }
            else
            {
                btnSecurity.Visible = false;
                btnManualRun.Enabled = false;
            }

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

            int manualSourceType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;

            ddlSourceType.SetValue( metric.SourceValueTypeId ?? manualSourceType );
            tbYAxisLabel.Text = metric.YAxisLabel;
            rblUnitType.SetValue( metric.UnitType.ConvertToInt() );
            cbIsCumulative.Checked = metric.IsCumulative;
            cbEnableAnalytics.Checked = metric.EnableAnalytics;
            ppMetricChampionPerson.SetValue( metric.MetricChampionPersonAlias != null ? metric.MetricChampionPersonAlias.Person : null );
            ppAdminPerson.SetValue( metric.AdminPersonAlias != null ? metric.AdminPersonAlias.Person : null );
            ceSourceSql.Text = metric.SourceSql;
            nbSQLHelp.InnerHtml = @"There are several ways to design your SQL to populate the Metric Values. If you use the 'SQL' source type option, the results will be stored with the date of the date when the Calculation is scheduled. To specify a specific date of the metric value, include a [MetricValueDate] column in the result set (see Example #4).
<br />
<br />
<h4>Example #1</h4> 
Simple metric with the default partition
<ul>
  <li>The 1st Column will be the YValue </li>
</ul>

<pre>
SELECT COUNT(*) 
FROM [Attendance] 
WHERE DidAttend = 1
  AND StartDateTime >= '{{ RunDateTime | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ RunDateTime | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}' 
</pre>

<br />
<h4>Example #2</h4> 
Simple metric with GroupId as the Partition.
<ul>
  <li>The 1st Column will be the YValue </li>
  <li>The 2nd Column will be the EntityId of the Partition </li>
</ul>

<pre>
SELECT COUNT(*), O.[GroupId]
FROM [Attendance] A
INNER JOIN [AttendanceOccurrence] O ON O.Id = A.OccurrenceId
WHERE DidAttend = 1
  AND StartDateTime >= '{{ RunDateTime | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ RunDateTime | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}' 
GROUP BY O.[GroupId]
</pre>

<br />
<h4>Example #3</h4> 
A metric with multiple partitions.
<ul>
    <li>The 1st Column will be the YValue </li>
    <li>The 2nd Column will be the EntityId of the 1st Partition </li>
    <li>The 3rd Column will be the EntityId of the 2nd Partition </li>
    <li>... </li>
    <li>The Nth Column will be the EntityId of the (N-1)th Partition </li>
</ul>
<pre>
SELECT COUNT(*), O.[GroupId], A.[CampusId], O.[ScheduleId]
FROM [Attendance] A
INNER JOIN [AttendanceOccurrence] O ON O.Id = A.OccurrenceId
WHERE DidAttend = 1
  AND StartDateTime >= '{{ RunDateTime | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ RunDateTime | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}' 
GROUP BY O.[GroupId], A.[CampusId], O.[ScheduleId]
</pre>

<br />
<h4>Example #4</h4> 
A metric with multiple partitions with a specific MetricValueDateTime specified. 
<ul>
    <li>The 1st Column will be the YValue </li>
    <li>If a [MetricValueDateTime] field is specified as the 2nd column, that will be the metric value date. NOTE: This should be a constant value for all rows
    <li>The 3rd Column will be the EntityId of the 1st Partition </li>
    <li>The 4th Column will be the EntityId of the 2nd Partition </li>
    <li>etc.</li>
</ul>
<pre>
-- get totals for the previous week
{% assign weekEndDate = RunDateTime | SundayDate | DateAdd:-7,'d' %}

SELECT COUNT(*), '{{ weekEndDate }}' [MetricValueDateTime], O.[GroupId], A.[CampusId], O.[ScheduleId]
FROM [Attendance] A
INNER JOIN [AttendanceOccurrence] O ON O.Id = A.OccurrenceId
WHERE DidAttend = 1
  AND StartDateTime >= '{{ weekEndDate | DateAdd:-6,'d' | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ weekEndDate | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}'
GROUP BY O.[GroupId], A.[CampusId], O.[ScheduleId]
</pre>

NOTE: If a [MetricValueDateTime] is specified and there is already a metric value, the value will get updated. This is handy if you have a weekly metric, but schedule it to calculate every day.
<hr>
The SQL can include Lava merge fields:";

            nbSQLHelp.InnerHtml += metric.GetMergeObjects( RockDateTime.Now ).lavaDebugInfo();

            ceSourceLava.Text = metric.SourceLava;
            nbLavaHelp.InnerHtml = @"There are several ways to design your Lava to populate the Metric Values. If you use the 'Lava' source type option, the results will be stored with the date of the date when the Calculation is scheduled. To specify a specific date of the metric value, include a [MetricValueDate] column in the result set.
<br />
<br />
<h4>Example #1</h4> 
Simple metric with the default partition
<ul>
  <li>The output will be the YValue </li>
</ul>
<br />
Lava Template:
<pre>{% attendance where:'DidAttend == true && GroupId == 56' count:'true' %}
  {{ count }}
{% endattendance %}</pre>

Lava Output:
<pre>15052</pre>

<br />
<h4>Example #2</h4> 
Simple metric with a MetricValueDateTime specified
<ul>
  <li>The 1st Column will be the YValue </li>
  <li>The 2nd Column will be the MetricValueDateTime </li>
</ul>
<br />
Lava Template:
<pre>{% webrequest url:'https://api.github.com/repos/SparkDevNetwork/Rock/subscribers' %}
    {{ results | Size }},{{ RunDateTime | SundayDate | DateAdd:-7 }} 
{% endwebrequest %}</pre>

Lava Output:
<pre>30, 5/7/2016</pre>

<br />
<h4>Example #3</h4>
Lava that returns a Count and EntityIds for each Partition
<ul>
    <li>The 1st Column will be the YValue </li>
    <li>The 2nd Column will be the EntityId of the 1st Partition </li>
    <li>The 3rd Column will be the EntityId of the 2nd Partition </li>
    <li>... </li>
    <li>The Nth Column will be the EntityId of the (N-1)th Partition </li>
</ul>

Lava Template:
<pre>{% webrequest url:'https://api.example.com/statsByGroupAndCampus' %}
     {% for item in results %}
	    item.Count,Item.GroupId,Item.CampusId
    {% endfor %}
{% endwebrequest %}</pre>

Lava Output:
<pre>30, 45, 2
34, 45, 3
36, 45, 4
30, 46, 2
34, 46, 3
36, 46, 4</pre>

<br />
<h4>Example #4</h4>
Lava that returns a Count, Date and EntityIds for each Partition
<br />
Lava Template:
<pre>{% assign weekEndDate = RunDateTime | SundayDate | DateAdd:-7,'d' | Date:'yyyy-MM-dd'  %}
{% webrequest url:'https://api.example.com/weeklyStatsByCampus?since={{ weekEndDate }}'  %}
     {% for item in results %}
	    item.Count, item.DateTime, Item.CampusId
    {% endfor %}
{% endwebrequest %}</pre>

Lava Output:
<pre>30, 5/7/2016, 2
34, 5/7/2016, 3
36, 5/7/2016, 4
30, 5/8/2016, 2
34, 5/8/2016, 3
36, 5/8/2016, 4</pre>

NOTE: If a [MetricValueDateTime] is specified and there is already a metric value, the value will get updated. This is handy if you have a weekly metric, but schedule it to calculate every day.
<hr>
The Lava can include Lava merge fields:";

            nbLavaHelp.InnerHtml += metric.GetMergeObjects( RockDateTime.Now ).lavaDebugInfo();

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

            dvpDataView.SetValue( metric.DataViewId );

            // make sure the control visibility is set based on SourceType
            ddlSourceType_SelectedIndexChanged( null, new EventArgs() );

            // make sure the control visibility is set based on Schedule Selection Type
            rblScheduleSelect_SelectedIndexChanged( null, new EventArgs() );

            MetricPartitionsState = new List<MetricPartition>();
            foreach ( var item in metric.MetricPartitions )
            {
                MetricPartitionsState.Add( item );
            }

            BindMetricPartitionsGrid();

            // and populate it's value, otherwise this prop is not available and should be set to false.
            ShowHideAutoPartitionPrimaryCampus();
            cbAutoPartionPrimaryCampus.Checked = cbAutoPartionPrimaryCampus.Visible && metric.AutoPartitionOnPrimaryCampus;

            metric.LoadAttributes();
            avcEditAttributeValues.AddEditControls( metric, Rock.Security.Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Binds the metric partitions grid.
        /// </summary>
        private void BindMetricPartitionsGrid()
        {
            SetMetricPartitionsListOrder( MetricPartitionsState );

            var partitionList = MetricPartitionsState.OrderBy( a => a.Order ).ThenBy( a => a.Label ).Select( a =>
            {
                var entityTypeCache = EntityTypeCache.Get( a.EntityTypeId ?? 0 );
                string label;
                if ( a.Order == 0 && !a.EntityTypeId.HasValue )
                {
                    label = a.Label ?? "Default";
                }
                else
                {
                    label = a.Label;
                }

                string entityTypeQualifierInfo = null;
                if ( !string.IsNullOrEmpty( a.EntityTypeQualifierColumn ) )
                {
                    entityTypeQualifierInfo = string.Format( "({0}: {1})", a.EntityTypeQualifierColumn, a.EntityTypeQualifierValue );
                }

                return new
                {
                    a.Guid,
                    Label = label,
                    EntityTypeName = entityTypeCache != null ? entityTypeCache.FriendlyName : string.Empty,
                    EntityTypeQualifier = entityTypeQualifierInfo,
                    a.IsRequired
                };
            } ).ToList();

            gMetricPartitions.DataSource = partitionList;
            gMetricPartitions.DataBind();

            int metricId = hfMetricId.Value.AsInteger();
            nbMetricValuesWarning.Visible = new MetricValueService( new RockContext() ).Queryable().Where( a => a.MetricId == metricId ).Any();
            nbMetricValuesWarning.Text = "This Metric already has some values.  If you are changing Metric Partitions, you might have to manually update the metric values to reflect the new partition arrangement.";
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="metric">The metric.</param>
        private void ShowReadonlyDetails( Metric metric )
        {
            SetEditMode( false );
            hfMetricId.SetValue( metric.Id );

            CreateChart( metric );

            lReadOnlyTitle.Text = metric.Title.FormatAsHtmlTitle();

            DescriptionList descriptionListMain = new DescriptionList();

            descriptionListMain.Add( "Subtitle", metric.Subtitle );
            descriptionListMain.Add( "Description", metric.Description );

            if ( metric.MetricCategories != null && metric.MetricCategories.Any() )
            {
                descriptionListMain.Add( "Categories", metric.MetricCategories.Select( s => s.Category.ToString() ).OrderBy( o => o ).ToList().AsDelimited( "," ) );
            }

            if ( metric.MetricPartitions.Count() == 1 )
            {
                var singlePartition = metric.MetricPartitions.First();
                if ( singlePartition.EntityTypeId.HasValue )
                {
                    var entityTypeCache = EntityTypeCache.Get( singlePartition.EntityTypeId.Value );
                    if ( entityTypeCache != null )
                    {
                        descriptionListMain.Add( "Partitioned by ", singlePartition.Label ?? entityTypeCache.FriendlyName );
                    }
                }
            }
            else if ( metric.MetricPartitions.Count() > 1 )
            {
                var partitionNameList = metric.MetricPartitions.OrderBy( a => a.Order ).ThenBy( a => a.Label ).Where( a => a.EntityTypeId.HasValue ).ToList().Select( a =>
                {
                    var entityTypeCache = EntityTypeCache.Get( a.EntityTypeId.Value );
                    return new
                    {
                        Label = a.Label,
                        EntityTypeFriendlyName = entityTypeCache != null ? entityTypeCache.FriendlyName : string.Empty
                    };
                } );

                descriptionListMain.Add( "Partitioned by ", partitionNameList.Where( a => a != null ).Select( a => a.Label ?? a.EntityTypeFriendlyName ).ToList().AsDelimited( ", ", " and " ) );
            }

            // only show LastRun and Schedule label if SourceValueType is not Manual
            int manualSourceType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
            ltLastRunDateTime.Visible = metric.SourceValueTypeId != manualSourceType;
            hlScheduleFriendlyText.Visible = metric.SourceValueTypeId != manualSourceType;

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

            if ( metric.Schedule != null )
            {
                string iconClass;
                if ( metric.Schedule.HasSchedule() )
                {
                    if ( metric.Schedule.HasScheduleWarning() )
                    {
                        hlScheduleFriendlyText.LabelType = LabelType.Warning;
                        iconClass = "fa fa-exclamation-triangle";
                    }
                    else
                    {
                        hlScheduleFriendlyText.LabelType = LabelType.Info;
                        iconClass = "fa fa-clock-o";
                    }
                }
                else
                {
                    hlScheduleFriendlyText.LabelType = LabelType.Danger;
                    iconClass = "fa fa-exclamation-triangle";
                }

                hlScheduleFriendlyText.Text = "<i class='" + iconClass + "'></i> " + metric.Schedule.ToFriendlyScheduleText( true );
            }
            else
            {
                hlScheduleFriendlyText.LabelType = LabelType.Danger;
                hlScheduleFriendlyText.Text = "<i class='fa fa-clock-o'></i> " + "Not Scheduled";
            }

            lblMainDetails.Text = descriptionListMain.Html;

            if ( metric.DataView != null )
            {
                hlDataView.Visible = UserCanEdit;

                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "DataViewId", metric.DataViewId.ToString() );
                hlDataView.Text = $"<a href='{LinkedPageUrl( AttributeKey.DataViewPage, queryParams )}'>Data View: {metric.DataView.Name.Truncate( 30, true )}</a>";
                hlDataView.ToolTip = ( metric.DataView.Name.Length > 30 ) ? metric.DataView.Name : null;
            }
            else
            {
                hlDataView.Visible = false;
            }
        }

        private void CreateChart( Metric metric )
        {
            if ( !GetAttributeValue( AttributeKey.ShowChart ).AsBoolean() )
            {
                pnlActivityChart.Visible = false;
                return;
            }

            var chartFactory = GetChartJsFactory( metric );
            pnlActivityChart.Visible = GetAttributeValue( AttributeKey.ShowChart ).AsBoolean();

            // Add client script to construct the chart.
            string formatString;
            if ( metric.UnitType == Rock.Model.UnitType.Currency )
            {
                formatString = "currency";
            }
            else if ( metric.UnitType == Rock.Model.UnitType.Percentage )
            {
                formatString = "percentage";
            }
            else
            {
                formatString = "numeric";
            }
            var chartDataJson = chartFactory.GetJson( new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                SizeToFitContainerWidth = true,
                MaintainAspectRatio = false,
                LineTension = 0m,
                YValueFormatString = formatString
            } );

            string script = string.Format(
                @"
            var barCtx = $('#{0}')[0].getContext('2d');
            var barChart = new Chart(barCtx, {1});",
                                    chartCanvas.ClientID,
                                    chartDataJson );

            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "stepProgramActivityChartScript", script, true );
        }

        private ChartStyle getChartStyle()
        {
            var chartStyleDefinedValueGuid = this.GetAttributeValue( AttributeKey.ChartStyle ).AsGuidOrNull();
            var definedValue = DefinedValueCache.Get( chartStyleDefinedValueGuid.Value );
            if ( definedValue == null )
            {
                return new ChartStyle();
            }
            var chartStyle = ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( "ChartStyle" ) );
            return chartStyle;
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <returns></returns>
        private ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory( Metric metric )
        {
            var reportPeriod = new TimePeriod( GetAttributeValue( AttributeKey.SlidingDateRange ) ?? "-1||" );

            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
            }

            List<MetricValue> metricValues = GetMetricValues( metric, startDate, endDate );

            // Initialize a new Chart Factory.
            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>
            {
                TimeScale = ChartJsTimeSeriesTimeScaleSpecifier.Auto
            };
            var dataPoints = metricValues
                .Where( a => a.YValue.HasValue && a.MetricValueDateKey.HasValue )
                .GroupBy( x => new
                {
                    DateKey = x.MetricValueDateKey.Value,
                    x.MetricValueType,
                    x.MetricValuePartitionEntityIds
                } )
                .Select( x => new
                {
                    x.Key,
                    Value = x.Select( a => a.YValue.Value )
                } )
               .Select( x => new ChartDatasetInfo
               {
                   MetricValuePartitionEntityIds = x.Key.MetricValuePartitionEntityIds,
                   MetricValueType = x.Key.MetricValueType,
                   DateTime = x.Key.DateKey.GetDateKeyDate(), // +1 to get first day of month
                   Value = x.Value.Sum()
               } )
               .ToList();

            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;

            var chartStyle = getChartStyle();

            var dataSeriesDatasets = dataPoints
                .Select( x => x.MetricValuePartitionEntityIds )
                .Distinct()
                .ToList();
            var hasMultiplePartitions = dataSeriesDatasets.Count > 1;

            var combineValues = GetAttributeValue( AttributeKey.CombineChartSeries ).AsBooleanOrNull() ?? false;
            var datapointsByMetricTypeValue = dataPoints
                                .GroupBy( d => d.MetricValueType )
                                .ToList();

            if ( combineValues )
            {
                foreach ( var datapoint in datapointsByMetricTypeValue )
                {
                    // Get the series name.
                    // If the metric has a specified label use it.
                    // Append "Total" if this is a combination of multiple partitions.
                    var name = metric.YAxisLabel;
                    if ( string.IsNullOrWhiteSpace(name) )
                    {
                        name = hasMultiplePartitions ? "Total" : "Value";
                    }

                    var borderColor = chartStyle.SeriesColors?[0] ?? null;

                    if ( datapoint.Key == MetricValueType.Goal )
                    {
                        name += $" Goal";
                        borderColor = chartStyle.GoalSeriesColor;
                    }

                    var dataset = new ChartJsTimeSeriesDataset
                    {
                        Name = name,
                        BorderColor = borderColor,
                        DataPoints = datapoint
                            .GroupBy( a => a.DateTime )
                            .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.Key, Value = x.Select( a => a.Value ).Sum() } )
                            .Cast<IChartJsTimeSeriesDataPoint>()
                            .ToList()
                    };

                    factory.Datasets.Add( dataset );
                }

            }
            else
            {
                var seriesNameKeyValue = new Dictionary<string, string>();
                foreach ( var dataSeriesDataset in dataSeriesDatasets )
                {
                    var seriesNameValue = GetSeriesPartitionName( dataSeriesDataset );
                    seriesNameKeyValue.Add( dataSeriesDataset, seriesNameValue );
                }

                int seriesIndex = 0;
                foreach ( var dataseriesKey in seriesNameKeyValue.Keys )
                {
                    foreach ( var datapoint in datapointsByMetricTypeValue )
                    {
                        // Set the series name.
                        var name = seriesNameKeyValue[dataseriesKey];
                        if ( string.IsNullOrWhiteSpace( name ) )
                        {
                            name = "Value";
                        }

                        // Set the series color. If the chart style defines a set of colors, assign the next in the set.
                        // If none are available, the chart component will assign a color from the default palette.
                        string fillColor = null;
                        if ( chartStyle.SeriesColors != null && chartStyle.SeriesColors.Length > seriesIndex )
                        {
                            fillColor = chartStyle.SeriesColors[seriesIndex];
                        }

                        if ( datapoint.Key == MetricValueType.Goal )
                        {
                            name += $" Goal";
                            fillColor = chartStyle.GoalSeriesColor;
                        }

                        var dataset = new ChartJsTimeSeriesDataset
                        {
                            Name = name,
                            BorderColor = fillColor,
                            DataPoints = datapoint
                                .Where( x => x.MetricValuePartitionEntityIds == dataseriesKey )
                                .Select( x => new ChartJsTimeSeriesDataPoint { DateTime = x.DateTime, Value = x.Value } )
                                .Cast<IChartJsTimeSeriesDataPoint>()
                                .ToList()
                        };

                        factory.Datasets.Add( dataset );
                    }

                    seriesIndex++;
                }
            }

            return factory;
        }

        private static List<MetricValue> GetMetricValues( Metric metric, DateTime? startDate, DateTime? endDate )
        {
            // include MetricValuePartitions and each MetricValuePartition's MetricPartition so that MetricValuePartitionEntityIds doesn't have to lazy load
            var metricValuesQry = new MetricValueService( new RockContext() )
                .Queryable()
                .Include( a => a.MetricValuePartitions.Select( b => b.MetricPartition ) )
                .Where( a => a.MetricId == metric.Id );

            if ( startDate.HasValue )
            {
                metricValuesQry = metricValuesQry.Where( a => a.MetricValueDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                metricValuesQry = metricValuesQry.Where( a => a.MetricValueDateTime <= endDate.Value );
            }

            metricValuesQry = metricValuesQry.OrderBy( a => a.MetricValueDateTime );

            var metricValues = metricValuesQry.ToList();
            return metricValues;
        }

        private string GetSeriesPartitionName( string dataSeriesDataset )
        {
            var seriesNamValue = string.Empty;

            var rockContext = new RockContext();
            List<string> seriesPartitionValues = new List<string>();

            var entityTypeEntityIdList = dataSeriesDataset
                .SplitDelimitedValues( "," )
                .Select( a => a.Split( '|' ) )
                .Select( a =>
                new
                {
                    EntityTypeId = a[0].AsIntegerOrNull(),
                    EntityId = a[1].AsIntegerOrNull()
                } );

            foreach ( var entityTypeEntity in entityTypeEntityIdList )
            {
                if ( entityTypeEntity.EntityTypeId.HasValue && entityTypeEntity.EntityId.HasValue )
                {
                    var entityTypeCache = EntityTypeCache.Get( entityTypeEntity.EntityTypeId.Value );
                    if ( entityTypeCache != null )
                    {
                        if ( entityTypeCache.Id == EntityTypeCache.GetId<Campus>() )
                        {
                            var campus = CampusCache.Get( entityTypeEntity.EntityId.Value );
                            if ( campus != null )
                            {
                                seriesPartitionValues.Add( campus.Name );
                            }
                        }
                        else if ( entityTypeCache.Id == EntityTypeCache.GetId<DefinedValue>() )
                        {
                            var definedValue = DefinedValueCache.Get( entityTypeEntity.EntityId.Value );
                            if ( definedValue != null )
                            {
                                seriesPartitionValues.Add( definedValue.ToString() );
                            }
                        }
                        else
                        {
                            Type[] modelType = { entityTypeCache.GetEntityType() };
                            Type genericServiceType = typeof( Rock.Data.Service<> );
                            Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                            var serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            var result = getMethod.Invoke( serviceInstance, new object[] { entityTypeEntity.EntityId } );
                            if ( result != null )
                            {
                                seriesPartitionValues.Add( result.ToString() );
                            }
                        }
                    }
                }
            }

            if ( seriesPartitionValues.Any() )
            {
                seriesNamValue = seriesPartitionValues.AsDelimited( "," );
            }
            else
            {
                seriesNamValue = null;
            }

            return seriesNamValue;
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

            ddlSourceType.Items.Clear();
            foreach ( var item in new DefinedValueService( rockContext ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.METRIC_SOURCE_TYPE.AsGuid() ) )
            {
                ddlSourceType.Items.Add( new ListItem( item.Value, item.Id.ToString() ) );
            }

            rblScheduleSelect.Items.Clear();
            rblScheduleSelect.Items.Add( new ListItem( ScheduleSelectionType.Unique.ConvertToString(), ScheduleSelectionType.Unique.ConvertToInt().ToString() ) );
            rblScheduleSelect.Items.Add( new ListItem( ScheduleSelectionType.NamedSchedule.ConvertToString(), ScheduleSelectionType.NamedSchedule.ConvertToInt().ToString() ) );

            var scheduleCategoryId = new CategoryService( rockContext ).Get( Rock.SystemGuid.Category.SCHEDULE_METRICS.AsGuid() ).Id;
            var metricSchedules = new ScheduleService( rockContext ).Queryable()
                .Where( a => a.CategoryId == scheduleCategoryId && a.Name != string.Empty && a.IsActive )
                .OrderBy( a => a.Name ).ToList();

            ddlSchedule.Items.Clear();
            foreach ( var item in metricSchedules )
            {
                ddlSchedule.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            // limit to EntityTypes that support picking a Value with a picker
            etpMetricPartitionEntityType.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().OrderBy( t => t.FriendlyName ).Where( a => a.SingleValueFieldTypeId.HasValue ).ToList();

            // just in case they select an EntityType that can be qualified by DefinedType...
            ddlMetricPartitionDefinedTypePicker.Items.Clear();
            ddlMetricPartitionDefinedTypePicker.Items.Add( new ListItem() );
            var definedTypesList = new DefinedTypeService( rockContext ).Queryable().OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            foreach ( var definedType in definedTypesList )
            {
                ddlMetricPartitionDefinedTypePicker.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            rblUnitType.BindToEnum<Rock.Model.UnitType>();
        }

        /// <summary>
        /// If the metric uses a DataView, there is one partition, and that partition is a campus then show the Auto Partition on Primary Campus checkbox
        /// </summary>
        private void ShowHideAutoPartitionPrimaryCampus()
        {
            // If the metric uses a DataView, there is one partition, and that partition is a campus then show the Auto Partition on Primary Campus checkbox and populate it's value, otherwise this prop is not available and should be set to false.
            cbAutoPartionPrimaryCampus.Visible = dvpDataView.Visible == true
                && dvpDataView.SelectedValueAsId() != null
                && MetricPartitionsState.Count == 1
                && MetricPartitionsState[0].EntityTypeId == EntityTypeCache.GetId( Rock.SystemGuid.EntityType.CAMPUS );
        }

        /// <summary>
        /// If "Enable Analytics" is checked this methods checks for a metric with the same name that also has "Enable Analytics" checked.
        /// </summary>
        /// <returns>
        ///   <c>false</c> If the name is already taken; otherwise, <c>true</c>.
        /// </returns>
        private bool IsTitleUnique()
        {
            // Only Analytics Metrics need to have a unique name.
            if ( !cbEnableAnalytics.Checked )
            {
                return true;
            }

            using ( var rockContext = new RockContext() )
            {
                var metricId = hfMetricId.ValueAsInt();

                var existingAnalyticMetricsWithTheSameName = new MetricService( rockContext )
                    .Queryable()
                    .Where( m => m.Title == tbTitle.Text && m.EnableAnalytics == true && m.Id != metricId )
                    .ToList();

                if ( existingAnalyticMetricsWithTheSameName.Any() )
                {
                    tbTitle.ShowErrorMessage( string.Empty );
                    return false;
                }

                return true;
            }
        }

        private void ManualMetricRun( int metricId )
        {
            MetricService metricService = new MetricService( new RockContext() );
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 300;

            var metricTask = new Task( () =>
            {
                metricService.CalculateMetric( metricId, commandTimeout, true );
            } );

            metricTask.Start();
        }

        private struct ResultValue
        {
            public List<ResultValuePartition> Partitions { get; set; }

            public decimal Value { get; set; }

            public DateTime MetricValueDateTime { get; set; }
        }

        private struct ResultValuePartition
        {
            // Zero-based partition position
            public int PartitionPosition { get; set; }

            public int? EntityId { get; set; }
        }

        #endregion

        #region Series Partitions

        /// <summary>
        /// Handles the GridReorder event of the gMetricPartitions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gMetricPartitions_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderMetricPartitionsList( MetricPartitionsState, e.OldIndex, e.NewIndex );
            BindMetricPartitionsGrid();
        }

        /// <summary>
        /// Sets the metric partitions list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetMetricPartitionsListOrder( List<MetricPartition> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the metric partitions list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderMetricPartitionsList( List<MetricPartition> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Handles the Delete event of the gMetricPartitions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetricPartitions_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            MetricPartitionService metricPartitionService = new MetricPartitionService( rockContext );
            MetricPartition metricPartition = metricPartitionService.Get( ( Guid ) e.RowKeyValue );

            if ( MetricPartitionsState.Count() == 1 )
            {
                mdMetricPartitionsGridWarning.Show( "Metric must have at least one partition defined", ModalAlertType.Warning );
                return;
            }

            if ( metricPartition != null )
            {
                string errorMessage;
                if ( !metricPartitionService.CanDelete( metricPartition, out errorMessage ) )
                {
                    mdMetricPartitionsGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }
            }

            Guid rowGuid = ( Guid ) e.RowKeyValue;
            MetricPartitionsState.RemoveEntity( rowGuid );

            BindMetricPartitionsGrid();
            ShowHideAutoPartitionPrimaryCampus();
        }

        /// <summary>
        /// Handles the AddClick event of the gMetricPartitions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gMetricPartitions_AddClick( object sender, EventArgs e )
        {
            gMetricPartitions_ShowEdit( null );
        }

        /// <summary>
        /// Handles the RowSelected event of the gMetricPartitions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetricPartitions_RowSelected( object sender, RowEventArgs e )
        {
            gMetricPartitions_ShowEdit( ( Guid ) e.RowKeyValue );
        }

        /// <summary>
        /// Shows the Metric Partition dialog for add/edit
        /// </summary>
        /// <param name="metricPartitionGuid">The metric partition unique identifier.</param>
        protected void gMetricPartitions_ShowEdit( Guid? metricPartitionGuid )
        {
            MetricPartition metricPartition;
            if ( !metricPartitionGuid.HasValue )
            {
                metricPartition = new MetricPartition();
                mdMetricPartitionDetail.Title = "Add Partition";
            }
            else
            {
                metricPartition = MetricPartitionsState.FirstOrDefault( a => a.Guid == metricPartitionGuid.Value );
                mdMetricPartitionDetail.Title = "Edit Partition";
            }

            var metricValueService = new MetricValueService( new RockContext() );

            hfMetricPartitionGuid.Value = metricPartition.Guid.ToString();
            tbMetricPartitionLabel.Text = metricPartition.Label;
            etpMetricPartitionEntityType.SetValue( metricPartition.EntityTypeId );
            cbMetricPartitionIsRequired.Checked = metricPartition.IsRequired;
            tbMetricPartitionEntityTypeQualifierColumn.Text = metricPartition.EntityTypeQualifierColumn;
            tbMetricPartitionEntityTypeQualifierValue.Text = metricPartition.EntityTypeQualifierValue;

            if ( metricPartition.EntityTypeQualifierColumn == "DefinedTypeId" )
            {
                ddlMetricPartitionDefinedTypePicker.SetValue( ( metricPartition.EntityTypeQualifierValue ?? string.Empty ).AsIntegerOrNull() );
            }

            UpdateMetricPartionDetailForEntityType();

            mdMetricPartitionDetail.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMetricPartitionDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMetricPartitionDetail_SaveClick( object sender, EventArgs e )
        {
            if ( !tbMetricPartitionLabel.IsValid )
            {
                return;
            }

            var metricPartition = new MetricPartition();
            var rockContext = new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var metricPartitionState = MetricPartitionsState.FirstOrDefault( a => a.Guid == hfMetricPartitionGuid.Value.AsGuid() );
            if ( metricPartitionState != null )
            {
                metricPartition.CopyPropertiesFrom( metricPartitionState );
            }
            else
            {
                metricPartition.Order = MetricPartitionsState.Any() ? MetricPartitionsState.Max( a => a.Order ) + 1 : 0;
                metricPartition.MetricId = hfMetricId.Value.AsInteger();
            }

            metricPartition.Label = tbMetricPartitionLabel.Text;

            metricPartition.EntityTypeId = etpMetricPartitionEntityType.SelectedEntityTypeId;
            metricPartition.IsRequired = cbMetricPartitionIsRequired.Checked;
            metricPartition.EntityTypeQualifierColumn = tbMetricPartitionEntityTypeQualifierColumn.Text;
            if ( ddlMetricPartitionDefinedTypePicker.Visible )
            {
                metricPartition.EntityTypeQualifierValue = ddlMetricPartitionDefinedTypePicker.SelectedValue;
            }
            else
            {
                metricPartition.EntityTypeQualifierValue = tbMetricPartitionEntityTypeQualifierValue.Text;
            }

            // Controls will show warnings
            if ( !metricPartition.IsValid )
            {
                return;
            }

            if ( metricPartitionState != null )
            {
                MetricPartitionsState.RemoveEntity( metricPartitionState.Guid );
            }

            MetricPartitionsState.Add( metricPartition );
            BindMetricPartitionsGrid();
            ShowHideAutoPartitionPrimaryCampus();
            mdMetricPartitionDetail.Hide();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpMetricPartitionEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpMetricPartitionEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateMetricPartionDetailForEntityType();
        }

        /// <summary>
        /// Updates the type of the metric partition detail for entity.
        /// </summary>
        private void UpdateMetricPartionDetailForEntityType()
        {
            ddlMetricPartitionDefinedTypePicker.Visible = false;
            tbMetricPartitionEntityTypeQualifierColumn.ReadOnly = false;
            tbMetricPartitionEntityTypeQualifierValue.Visible = true;
            pwMetricPartitionAdvanced.Visible = etpMetricPartitionEntityType.SelectedEntityTypeId.HasValue;

            if ( etpMetricPartitionEntityType.SelectedEntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Get( etpMetricPartitionEntityType.SelectedEntityTypeId.Value );
                if ( entityTypeCache != null )
                {
                    if ( entityTypeCache.Id == EntityTypeCache.GetId<Rock.Model.DefinedValue>() )
                    {
                        ddlMetricPartitionDefinedTypePicker.Visible = true;
                        tbMetricPartitionEntityTypeQualifierColumn.Text = "DefinedTypeId";
                        tbMetricPartitionEntityTypeQualifierColumn.ReadOnly = true;
                        tbMetricPartitionEntityTypeQualifierValue.Visible = false;
                    }
                }
            }

            pwMetricPartitionAdvanced.Expanded = !string.IsNullOrEmpty( tbMetricPartitionEntityTypeQualifierColumn.Text );
        }

        #endregion

        #region block specific enums

        /// <summary>
        /// Stores information about a dataset to be displayed on a chart.
        /// </summary>
        private class ChartDatasetInfo
        {
            public string MetricValuePartitionEntityIds { get; set; }

            public DateTime DateTime { get; set; }

            public decimal Value { get; set; }

            public MetricValueType MetricValueType { get; internal set; }
        }

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
