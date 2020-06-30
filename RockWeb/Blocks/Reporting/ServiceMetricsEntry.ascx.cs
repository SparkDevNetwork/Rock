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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block for easily adding/editing metric values for any metric that has partitions of campus and service time.
    /// </summary>
    [DisplayName( "Service Metrics Entry" )]
    [Category( "Reporting" )]
    [Description( "Block for easily adding/editing metric values for any metric that has partitions of campus and service time." )]

    [CategoryField(
        "Schedule Category",
        Description = "The schedule category to use for list of service times.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.ScheduleCategory )]
    [IntegerField(
        "Weeks Back",
        Description = "The number of weeks back to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 8,
        Order = 1,
        Key = AttributeKey.WeeksBack )]
    [IntegerField(
        "Weeks Ahead",
        Description = "The number of weeks ahead to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 2,
        Key = AttributeKey.WeeksAhead )]
    [MetricCategoriesField(
        "Metric Categories",
        Description = "Select the metric categories to display (note: only metrics in those categories with a campus and schedule partition will displayed).",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.MetricCategories )]
    [CampusesField( "Campuses", "Select the campuses you want to limit this block to.", false, "", "", 4, AttributeKey.Campuses )]
    [BooleanField(
        "Insert 0 for Blank Items",
        Description = "If enabled, a zero will be added to any metrics that are left empty when entering data.",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.DefaultToZero )]
    [CustomDropdownListField(
        "Metric Date Determined By",
        Description = "This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.",
        DefaultValue = "0",
        ListSource = "0^Sunday Date,1^Day from Schedule",
        Order = 6,
        Key = AttributeKey.MetricDateDeterminedBy )]
    public partial class ServiceMetricsEntry : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ScheduleCategory = "ScheduleCategory";
            public const string WeeksBack = "WeeksBack";
            public const string WeeksAhead = "WeeksAhead";
            public const string MetricCategories = "MetricCategories";
            public const string Campuses = "Campuses";
            public const string DefaultToZero = "DefaultToZero";
            public const string MetricDateDeterminedBy = "MetricDateDeterminedBy";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string CampusId = "CampusId";
        }

        #endregion PageParameterKeys

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string SelectedCampusId = "SelectedCampusId";
            public const string SelectedWeekend = "SelectedWeekend";
            public const string SelectedServiceId = "SelectedServiceId";
        }

        #endregion ViewStateKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string CampusId = "CampusId";
            public const string ScheduleId = "ScheduleId";
        }

        #endregion UserPreferanceKeys

        #region Fields

        private int? _selectedCampusId { get; set; }
        private DateTime? _selectedWeekend { get; set; }
        private int? _selectedServiceId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _selectedCampusId = ViewState[ViewStateKey.SelectedCampusId] as int?;
            _selectedWeekend = ViewState[ViewStateKey.SelectedWeekend] as DateTime?;
            _selectedServiceId = ViewState[ViewStateKey.SelectedServiceId] as int?;
        }

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMetricsSaved.Visible = false;

            if ( !Page.IsPostBack )
            {
                var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    if ( GetCampuses().Any( b => b.Id == campusId.Value ) )
                    {
                        _selectedCampusId = campusId.Value;
                    }
                    else
                    {
                        DeleteBlockUserPreference( UserPreferenceKey.ScheduleId );
                    }
                }
                else
                {
                    _selectedCampusId = GetBlockUserPreference( UserPreferenceKey.CampusId ).AsIntegerOrNull();
                }
                _selectedServiceId = GetBlockUserPreference( UserPreferenceKey.ScheduleId ).AsIntegerOrNull();

                if ( CheckSelection() )
                {
                    LoadDropDowns();
                    BindMetrics();
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
            ViewState[ViewStateKey.SelectedCampusId] = _selectedCampusId;
            ViewState[ViewStateKey.SelectedWeekend] = _selectedWeekend;
            ViewState[ViewStateKey.SelectedServiceId] = _selectedServiceId;
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
            if ( CheckSelection() )
            {
                LoadDropDowns();
                BindMetrics();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            switch ( e.CommandName )
            {
                case "Campus":
                    _selectedCampusId = e.CommandArgument.ToString().AsIntegerOrNull();
                    break;
                case "Weekend":
                    _selectedWeekend = e.CommandArgument.ToString().AsDateTime();
                    break;
                case "Service":
                    _selectedServiceId = e.CommandArgument.ToString().AsIntegerOrNull();
                    break;
            }

            if ( CheckSelection() )
            {
                LoadDropDowns();
                BindMetrics();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMetric control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMetric_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item )
            {
                var nbMetricValue = e.Item.FindControl( "nbMetricValue" ) as NumberBox;
                if ( nbMetricValue != null )
                {
                    nbMetricValue.ValidationGroup = BlockValidationGroup;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            int? scheduleId = bddlService.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            if ( campusId.HasValue && scheduleId.HasValue && weekend.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var metricService = new MetricService( rockContext );
                    var metricValueService = new MetricValueService( rockContext );

                    weekend = GetWeekendDate( scheduleId, weekend, rockContext );

                    foreach ( RepeaterItem item in rptrMetric.Items )
                    {
                        var hfMetricIId = item.FindControl( "hfMetricId" ) as HiddenField;
                        var nbMetricValue = item.FindControl( "nbMetricValue" ) as NumberBox;

                        if ( hfMetricIId != null && nbMetricValue != null )
                        {
                            var metricYValue = nbMetricValue.Text.AsDecimalOrNull();

                            // If no value was provided and the block is not configured to default to "0" then just skip this metric.
                            if ( metricYValue == null && !GetAttributeValue( AttributeKey.DefaultToZero ).AsBoolean() )
                            {
                                continue;
                            }

                            int metricId = hfMetricIId.ValueAsInt();
                            var metric = new MetricService( rockContext ).Get( metricId );

                            if ( metric != null )
                            {
                                int campusPartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
                                int schedulePartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault();

                                var metricValue = metricValueService
                                    .Queryable()
                                    .Where( v =>
                                        v.MetricId == metric.Id &&
                                        v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                        v.MetricValuePartitions.Count == 2 &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == campusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == schedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                    .FirstOrDefault();

                                if ( metricValue == null )
                                {
                                    metricValue = new MetricValue();
                                    metricValue.MetricValueType = MetricValueType.Measure;
                                    metricValue.MetricId = metric.Id;
                                    metricValue.MetricValueDateTime = weekend.Value;
                                    metricValueService.Add( metricValue );

                                    var campusValuePartition = new MetricValuePartition();
                                    campusValuePartition.MetricPartitionId = campusPartitionId;
                                    campusValuePartition.EntityId = campusId.Value;
                                    metricValue.MetricValuePartitions.Add( campusValuePartition );

                                    var scheduleValuePartition = new MetricValuePartition();
                                    scheduleValuePartition.MetricPartitionId = schedulePartitionId;
                                    scheduleValuePartition.EntityId = scheduleId.Value;
                                    metricValue.MetricValuePartitions.Add( scheduleValuePartition );
                                }

                                if ( metricYValue == null )
                                {
                                    metricValue.YValue = 0;
                                }
                                else
                                {
                                    metricValue.YValue = metricYValue;
                                }

                                metricValue.Note = tbNote.Text;
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                nbMetricsSaved.Text = string.Format( "Your metrics for the {0} service on {1} at the {2} Campus have been saved.",
                    bddlService.SelectedItem.Text, bddlWeekend.SelectedItem.Text, bddlCampus.SelectedItem.Text );
                nbMetricsSaved.Visible = true;

                BindMetrics();
            }
        }

        private static DateTime? GetFirstScheduledDate( DateTime? weekend, Schedule schedule )
        {
            var date = schedule.GetNextStartDateTime( weekend.Value );
            if ( date.Value.Date > weekend.Value )
            {
                date = schedule.GetNextStartDateTime( weekend.Value.AddDays( -7 ) );
            }

            return date;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the filter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddl_SelectionChanged( object sender, EventArgs e )
        {
            BindMetrics();
        }

        #endregion

        #region Methods

        private bool CheckSelection()
        {
            // If campus and schedule have been selected before, assume current weekend
            if ( _selectedCampusId.HasValue && _selectedServiceId.HasValue && !_selectedWeekend.HasValue )
            {
                _selectedWeekend = RockDateTime.Today.SundayDate();
            }

            var options = new List<ServiceMetricSelectItem>();

            if ( !_selectedCampusId.HasValue )
            {
                var campuses = GetCampuses();
                if ( campuses.Count == 1 )
                {
                    _selectedCampusId = campuses.First().Id;
                }
                else
                {
                    lSelection.Text = "Select Location:";
                    foreach ( var campus in GetCampuses() )
                    {
                        options.Add( new ServiceMetricSelectItem( "Campus", campus.Id.ToString(), campus.Name ) );
                    }
                }
            }

            if ( !options.Any() && !_selectedWeekend.HasValue )
            {
                lSelection.Text = "Select Week of:";
                foreach ( var weekend in GetWeekendDates( 1, 0 ) )
                {
                    options.Add( new ServiceMetricSelectItem( "Weekend", weekend.ToString( "o" ), "Sunday " + weekend.ToShortDateString() ) );
                }
            }

            if ( !options.Any() && !_selectedServiceId.HasValue )
            {
                lSelection.Text = "Select Service Time:";
                foreach ( var service in GetServices() )
                {
                    options.Add( new ServiceMetricSelectItem( "Service", service.Id.ToString(), service.Name ) );
                }
            }

            if ( options.Any() )
            {
                rptrSelection.DataSource = options;
                rptrSelection.DataBind();

                pnlSelection.Visible = true;
                pnlMetrics.Visible = false;

                return false;
            }
            else
            {
                pnlSelection.Visible = false;
                pnlMetrics.Visible = true;

                return true;
            }
        }

        private void BuildCampusSelection()
        {
            foreach ( var campus in CampusCache.All()
                .Where( c => c.IsActive.HasValue && c.IsActive.Value )
                .OrderBy( c => c.Name ) )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            bddlCampus.Items.Clear();
            bddlWeekend.Items.Clear();
            bddlService.Items.Clear();

            // Load Campuses
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
            var campuses = GetCampuses();

            bool isCampusInvalid = false;
            if ( campusId.HasValue && !campuses.Any( a => a.Id == campusId.Value ) )
            {
                isCampusInvalid = true;
                nbWarning.Visible = true;
                nbWarning.Text = "The campus associated with request is not valid.";
            }

            foreach ( var campus in campuses.Where( a => !campusId.HasValue || isCampusInvalid || a.Id == campusId.Value ) )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }

            bddlCampus.SetValue( _selectedCampusId );

            // Load Weeks
            var weeksBack = GetAttributeValue( AttributeKey.WeeksBack ).AsInteger();
            var weeksAhead = GetAttributeValue( AttributeKey.WeeksAhead ).AsInteger();
            foreach ( var date in GetWeekendDates( weeksBack, weeksAhead ) )
            {
                bddlWeekend.Items.Add( new ListItem( "Sunday " + date.ToShortDateString(), date.ToString( "o" ) ) );
            }
            bddlWeekend.SetValue( _selectedWeekend.HasValue ? _selectedWeekend.Value.ToString( "o" ) : null );

            // Load service times
            foreach ( var service in GetServices() )
            {
                var listItemText = service.Name;

                if ( _selectedWeekend != null && GetAttributeValue( AttributeKey.MetricDateDeterminedBy ).AsInteger() == 1 )
                {
                    var date = GetFirstScheduledDate( _selectedWeekend, service );
                    listItemText = string.Format( "{0} ({1})", service.Name, date.ToShortDateString() );
                }

                bddlService.Items.Add( new ListItem( listItemText, service.Id.ToString() ) );
            }
            bddlService.SetValue( _selectedServiceId );
        }

        /// <summary>
        /// Gets the campuses.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampuses()
        {
            var campuses = new List<CampusCache>();
            var allowedCampuses = GetAttributeValue( "Campuses" ).SplitDelimitedValues().AsGuidList();

            foreach ( var campus in CampusCache.All()
                .Where( c => c.IsActive.HasValue && c.IsActive.Value )
                .Where( c => !allowedCampuses.Any() || allowedCampuses.Contains( c.Guid ) )
                .OrderBy( c => c.Name ) )
            {
                campuses.Add( campus );
            }

            return campuses;
        }

        /// <summary>
        /// Gets the weekend dates.
        /// </summary>
        /// <returns></returns>
        private List<DateTime> GetWeekendDates( int weeksBack, int weeksAhead )
        {
            var dates = new List<DateTime>();

            // Load Weeks
            var sundayDate = RockDateTime.Today.SundayDate();
            var daysBack = weeksBack * 7;
            var daysAhead = weeksAhead * 7;
            var startDate = sundayDate.AddDays( 0 - daysBack );
            var date = sundayDate.AddDays( daysAhead );
            while ( date >= startDate )
            {
                dates.Add( date );
                date = date.AddDays( -7 );
            }

            return dates;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetServices()
        {
            var services = new List<Schedule>();

            var scheduleCategory = CategoryCache.Get( GetAttributeValue( AttributeKey.ScheduleCategory ).AsGuid() );
            if ( scheduleCategory != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var schedule in new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s =>
                            s.IsActive &&
                            s.CategoryId.HasValue &&
                            s.CategoryId.Value == scheduleCategory.Id )
                        .OrderBy( s => s.Name ) )
                    {
                        services.Add( schedule );
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Binds the metrics.
        /// </summary>
        private void BindMetrics()
        {
            var serviceMetricValues = new List<ServiceMetric>();

            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            int? scheduleId = bddlService.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            var notes = new List<string>();

            if ( campusId.HasValue && scheduleId.HasValue && weekend.HasValue )
            {

                SetBlockUserPreference( UserPreferenceKey.CampusId, campusId.HasValue ? campusId.Value.ToString() : "" );
                SetBlockUserPreference( UserPreferenceKey.ScheduleId, scheduleId.HasValue ? scheduleId.Value.ToString() : "" );

                var metricCategories = MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( AttributeKey.MetricCategories ) );
                var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
                using ( var rockContext = new RockContext() )
                {
                    weekend = GetWeekendDate( scheduleId, weekend, rockContext );

                    var metricValueService = new MetricValueService( rockContext );
                    foreach ( var metric in new MetricService( rockContext )
                        .GetByGuids( metricGuids )
                        .Where( m =>
                            m.MetricPartitions.Count == 2 &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ) &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ) )
                        .OrderBy( m => m.Title )
                        .Select( m => new
                        {
                            m.Id,
                            m.Title,
                            CampusPartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                            SchedulePartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                        } ) )
                    {
                        var serviceMetric = new ServiceMetric( metric.Id, metric.Title );

                        if ( campusId.HasValue && weekend.HasValue && scheduleId.HasValue )
                        {
                            var metricValue = metricValueService
                                .Queryable().AsNoTracking()
                                .Where( v =>
                                    v.MetricId == metric.Id &&
                                    v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                    v.MetricValuePartitions.Count == 2 &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.CampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.SchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                .FirstOrDefault();

                            if ( metricValue != null )
                            {
                                serviceMetric.Value = metricValue.YValue;

                                if ( !string.IsNullOrWhiteSpace( metricValue.Note ) &&
                                    !notes.Contains( metricValue.Note ) )
                                {
                                    notes.Add( metricValue.Note );
                                }

                            }
                        }

                        serviceMetricValues.Add( serviceMetric );
                    }
                }
            }

            rptrMetric.DataSource = serviceMetricValues;
            rptrMetric.DataBind();

            tbNote.Text = notes.AsDelimited( Environment.NewLine + Environment.NewLine );
        }

        private DateTime? GetWeekendDate( int? scheduleId, DateTime? weekend, RockContext rockContext )
        {
            if ( GetAttributeValue( AttributeKey.MetricDateDeterminedBy ).AsInteger() == 1 )
            {
                var scheduleService = new ScheduleService( rockContext );
                var schedule = scheduleService.Get( scheduleId.Value );
                weekend = GetFirstScheduledDate( weekend, schedule );
            }

            return weekend;
        }

        #endregion

    }

    public class ServiceMetricSelectItem
    {
        public string CommandName { get; set; }
        public string CommandArg { get; set; }
        public string OptionText { get; set; }
        public ServiceMetricSelectItem( string commandName, string commandArg, string optionText )
        {
            CommandName = commandName;
            CommandArg = commandArg;
            OptionText = optionText;
        }
    }

    public class ServiceMetric
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Value { get; set; }

        public ServiceMetric( int id, string name )
        {
            Id = id;
            Name = name;
        }
    }
}
