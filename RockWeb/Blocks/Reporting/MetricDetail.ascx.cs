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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
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
    [SlidingDateRangeField( "Chart Date Range", key: "SlidingDateRange", defaultValue: "-1||||", enabledSlidingDateRangeTypes: "Last,Previous,Current,DateRange" )]
    [BooleanField( "Combine Chart Series" )]
    public partial class MetricDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private List<MetricPartition> MetricPartitionsState { get; set; }

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

            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Metric ) ).Id;

            ddlDataView.Help = @"NOTE: When using DataView to populate Metrics, multiple partitions is not supported.
<br />
<br />
When using a DataView as the Source Type, the Metric Values will based on the number of records returned by the DataView when the Calculate Metrics job processes this metric.
<br />
<br />
Example: Let's say you have a DataView called 'Small Group Attendance for Last Week', and schedule this metric for every Monday. When the Calculate Metrics job runs this metric, the Metric Value for that week will be number of records returned by the Dataview.
";

            // Metric supports 0 or more Categories, so the entityType is actually MetricCategory, not Metric
            cpMetricCategories.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.MetricCategory ) ).Id;

            lcMetricsChart.Options.SetChartStyle( GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );

            gMetricPartitions.Actions.ShowAdd = true;
            gMetricPartitions.GridReorder += gMetricPartitions_GridReorder;
            gMetricPartitions.Actions.AddClick += gMetricPartitions_AddClick;
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

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["MetricPartitionsState"] as string;
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

            ViewState["MetricPartitionsState"] = JsonConvert.SerializeObject( MetricPartitionsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

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

            if ( MetricPartitionsState.Count() > 1 && MetricPartitionsState.Any(a => !a.EntityTypeId.HasValue ))
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
                var metricPartition = new MetricPartition();
                metricPartition.EntityTypeId = null;
                metricPartition.IsRequired = true;
                metricPartition.Order = 0;
                metric.MetricPartitions.Add( metricPartition );
            }
            
            metric.Title = tbTitle.Text;
            metric.Subtitle = tbSubtitle.Text;
            metric.Description = tbDescription.Text;
            metric.IconCssClass = tbIconCssClass.Text;
            metric.SourceValueTypeId = ddlSourceType.SelectedValueAsId();
            metric.YAxisLabel = tbYAxisLabel.Text;
            metric.IsCumulative = cbIsCumulative.Checked;
            metric.EnableAnalytics = cbEnableAnalytics.Checked;

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
                    schedule = new Schedule();

                    // make it an "Unnamed" metrics schedule
                    schedule.Name = string.Empty;
                    schedule.CategoryId = metricScheduleCategoryId;
                }

                // if the schedule was a unique schedule (configured in the Metric UI, set the schedule's ical content to the schedule builder UI's value
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
            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

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

                // since we deleted the metric, sweep thru and make sure Metric Analytics Views are OK
                metricService.EnsureMetricAnalyticsViews();
            }

            var qryParams = new Dictionary<string, string>();
            if ( parentCategoryId != null )
            {
                qryParams["CategoryId"] = parentCategoryId.ToString();
            }

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
            var sourceValueType = DefinedValueCache.Read( sourceValueTypeId ?? 0 );
            pnlSQLSourceType.Visible = false;
            pnlDataviewSourceType.Visible = false;
            if ( sourceValueType != null )
            {
                pnlSQLSourceType.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL.AsGuid();
                pnlDataviewSourceType.Visible = sourceValueType.Guid == Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_DATAVIEW.AsGuid();

                // only show LastRun label if SourceValueType is not Manual
                bool isManual = sourceValueType.Guid != Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid();
                ltLastRunDateTime.Visible = isManual;
                rcwSchedule.Visible = isManual;
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
                pdAuditDetails.SetEntity( metric, ResolveRockUrl( "~" ) );
            }

            if ( metric == null )
            {
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
                metric = new Metric { Id = 0, IsSystem = false };
                metric.SourceValueTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
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
            }
            else
            {
                btnSecurity.Visible = false;
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

            int manualSourceType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;

            ddlSourceType.SetValue( metric.SourceValueTypeId ?? manualSourceType );
            tbYAxisLabel.Text = metric.YAxisLabel;
            cbIsCumulative.Checked = metric.IsCumulative;
            cbEnableAnalytics.Checked = metric.EnableAnalytics;
            ppMetricChampionPerson.SetValue( metric.MetricChampionPersonAlias != null ? metric.MetricChampionPersonAlias.Person : null );
            ppAdminPerson.SetValue( metric.AdminPersonAlias != null ? metric.AdminPersonAlias.Person : null );
            ceSourceSql.Text = metric.SourceSql;
            ceSourceSql.Help = @"There are several ways to design your SQL to populate the Metric Values. If you use the 'SQL' source type option, the results will be stored with the date of the date when the Calculation is scheduled. To specify a specific date of the metric value, include a [MetricValueDate] column in the result set (see Example #4).
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
SELECT COUNT(*), [GroupId]
FROM [Attendance] 
WHERE DidAttend = 1
  AND StartDateTime >= '{{ RunDateTime | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ RunDateTime | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}' 
GROUP BY [GroupId]
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
SELECT COUNT(*), [GroupId], [CampusId], [ScheduleId]
FROM [Attendance] 
WHERE DidAttend = 1
  AND StartDateTime >= '{{ RunDateTime | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ RunDateTime | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}' 
GROUP BY [GroupId], [CampusId], [ScheduleId]
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

SELECT COUNT(*), '{{ weekEndDate }}' [MetricValueDateTime], [GroupId], [CampusId], [ScheduleId]
FROM [Attendance] 
WHERE DidAttend = 1
  AND StartDateTime >= '{{ weekEndDate | DateAdd:-6,'d' | Date:'MM/dd/yyyy' }}' 
  AND StartDateTime < '{{ weekEndDate | DateAdd:1,'d' | Date:'MM/dd/yyyy' }}'
GROUP BY [GroupId], [CampusId], [ScheduleId]
</pre>

NOTE: If a [MetricValueDateTime] is specified and there is already a metric value, the value will get updated. This is handy if you have a weekly metric, but schedule it to calculate every day.
<hr>
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

            MetricPartitionsState = new List<MetricPartition>();
            foreach ( var item in metric.MetricPartitions )
            {
                MetricPartitionsState.Add( item );
            }

            BindMetricPartitionsGrid();
        }

        /// <summary>
        /// Binds the metric partitions grid.
        /// </summary>
        private void BindMetricPartitionsGrid()
        {
            SetMetricPartitionsListOrder( MetricPartitionsState );

            var partitionList = MetricPartitionsState.OrderBy( a => a.Order ).ThenBy( a => a.Label ).Select( a =>
            {
                var entityTypeCache = EntityTypeCache.Read( a.EntityTypeId ?? 0 );
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

            if ( metric.MetricPartitions.Count() == 1 )
            {
                var singlePartition = metric.MetricPartitions.First();
                if ( singlePartition.EntityTypeId.HasValue )
                {
                    var entityTypeCache = EntityTypeCache.Read( singlePartition.EntityTypeId.Value );
                    if ( entityTypeCache != null )
                    {
                        descriptionListMain.Add( "Partitioned by ", singlePartition.Label ?? entityTypeCache.FriendlyName );
                    }
                }
            }
            else if ( metric.MetricPartitions.Count() > 1 )
            {
                var partitionNameList = metric.MetricPartitions.OrderBy( a => a.Order ).ThenBy( a => a.Label ).Where( a => a.EntityTypeId.HasValue ).ToList().Select( a => {
                    var entityTypeCache = EntityTypeCache.Read( a.EntityTypeId.Value );
                    return new
                    {
                        Label = a.Label,
                        EntityTypeFriendlyName = entityTypeCache != null ? entityTypeCache.FriendlyName : string.Empty
                    };
                });

                descriptionListMain.Add( "Partitioned by ", partitionNameList.Where( a => a != null ).Select( a => a.Label ?? a.EntityTypeFriendlyName ).ToList().AsDelimited( ", ", " and " ) );
            }

            // only show LastRun and Schedule label if SourceValueType is not Manual
            int manualSourceType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_MANUAL.AsGuid() ).Id;
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
            MetricPartition metricPartition = metricPartitionService.Get( (Guid)e.RowKeyValue );

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

            Guid rowGuid = (Guid)e.RowKeyValue;
            MetricPartitionsState.RemoveEntity( rowGuid );

            BindMetricPartitionsGrid();
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
            gMetricPartitions_ShowEdit( (Guid)e.RowKeyValue );
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
        /// Updates the type of the metric partion detail for entity.
        /// </summary>
        private void UpdateMetricPartionDetailForEntityType()
        {
            ddlMetricPartitionDefinedTypePicker.Visible = false;
            tbMetricPartitionEntityTypeQualifierColumn.ReadOnly = false;
            tbMetricPartitionEntityTypeQualifierValue.Visible = true;
            pwMetricPartitionAdvanced.Visible = etpMetricPartitionEntityType.SelectedEntityTypeId.HasValue;
            
            if ( etpMetricPartitionEntityType.SelectedEntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Read( etpMetricPartitionEntityType.SelectedEntityTypeId.Value );
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