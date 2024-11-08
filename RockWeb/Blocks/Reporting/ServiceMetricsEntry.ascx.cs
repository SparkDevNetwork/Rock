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

    #region Block Attributes

    [CategoryField(
        "Schedule Category",
        Key = AttributeKey.ScheduleCategory,
        Description = "The schedule category to use for list of service times.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Weeks Back",
        Key = AttributeKey.WeeksBack,
        Description = "The number of weeks back to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 8,
        Order = 1 )]

    [IntegerField(
        "Weeks Ahead",
        Key = AttributeKey.WeeksAhead,
        Description = "The number of weeks ahead to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 2 )]

    [BooleanField(
        "Default to Current Week",
        Key = AttributeKey.DefaultToCurrentWeek,
        Description = "When enabled, the block will bypass the step to select a week from a list and will instead skip right to the entry page with the current week selected.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [MetricCategoriesField(
        "Metric Categories",
        Key = AttributeKey.MetricCategories,
        Description = "Select the metric categories to display (note: only metrics in those categories with a campus and schedule partition will displayed).",
        IsRequired = true,
        Order = 4 )]

    [CampusesField( "Campuses",
        description: "Select the campuses you want to limit this block to.",
        required: false,
        defaultCampusGuids: "",
        category: "",
        order: 5,
        key: AttributeKey.Campuses )]

    [BooleanField(
        "Insert 0 for Blank Items",
        Key = AttributeKey.DefaultToZero,
        Description = "If enabled, a zero will be added to any metrics that are left empty when entering data.",
        DefaultValue = "false",
        Order = 6 )]

    [CustomDropdownListField(
        "Metric Date Determined By",
        Key = AttributeKey.MetricDateDeterminedBy,
        Description = "This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.",
        DefaultValue = "0",
        ListSource = "0^Sunday Date,1^Day from Schedule",
        Order = 7 )]

    [BooleanField(
        "Limit Campus Selection to Campus Team Membership",
        Key = AttributeKey.LimitCampusByCampusTeam,
        Description = "When enabled, this would limit the campuses shown to only those where the individual was on the Campus Team.",
        DefaultBooleanValue = false,
        Order = 8 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Note: setting this can override the selected Campuses block setting.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Order = 9 )]

    [DefinedValueField(
        "Campus Status",
        Key = AttributeKey.CampusStatus,
        Description = "Note: setting this can override the selected Campuses block setting.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Order = 10 )]

    [BooleanField(
        "Filter Schedules by Campus",
        Key = AttributeKey.FilterByCampus,
        Description = "When enabled, only schedules that are included in the Campus Schedules will be included.",
        DefaultBooleanValue = false,
        Order = 11 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "535E1879-CD4C-432B-9312-B27B3A668D88" )]
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
            public const string LimitCampusByCampusTeam = "LimitCampusByCampusTeam";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatus = "CampusStatus";
            public const string FilterByCampus = "FilterByCampus";
            public const string DefaultToCurrentWeek = "DefaultToCurrentWeek";
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
            nbMetricsSaved.Visible = false;

            if ( !Page.IsPostBack )
            {
                var preferences = GetBlockPersonPreferences();
                var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    if ( GetCampuses().Any( b => b.Id == campusId.Value ) )
                    {
                        _selectedCampusId = campusId.Value;
                    }
                    else
                    {
                        preferences.SetValue( UserPreferenceKey.ScheduleId, string.Empty );
                    }
                }
                else
                {
                    _selectedCampusId = preferences.GetValue( UserPreferenceKey.CampusId ).AsIntegerOrNull();
                }
                _selectedServiceId = preferences.GetValue( UserPreferenceKey.ScheduleId ).AsIntegerOrNull();

                if ( CheckSelection() )
                {
                    LoadDropDowns();
                    BindMetrics();
                }
            }

            base.OnLoad( e );
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
            pnlConfigurationError.Visible = false;
            nbNoCampuses.Visible = false;

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
            if ( date != null && date.Value.Date > weekend.Value )
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
            if ( sender == bddlWeekend )
            {
                _selectedWeekend = bddlWeekend.SelectedValue.AsDateTime();
                LoadServicesDropDown();
            }
            else if ( sender == bddlCampus )
            {
                _selectedCampusId = bddlCampus.SelectedValueAsId();
                LoadServicesDropDown();
            }
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
                if ( campuses.Count == 0 )
                {
                    pnlConfigurationError.Visible = true;
                    nbNoCampuses.Visible = true;
                    pnlSelection.Visible = false;
                    pnlMetrics.Visible = false;
                    return false;
                }
                else if ( campuses.Count == 1 )
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

            var defaultToCurrentWeek = GetAttributeValue( AttributeKey.DefaultToCurrentWeek ).AsBoolean();
            if ( !options.Any() && !_selectedWeekend.HasValue && !defaultToCurrentWeek )
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

            if ( campuses.Count == 0 )
            {
                pnlConfigurationError.Visible = true;
                nbNoCampuses.Visible = true;
                pnlSelection.Visible = false;
                pnlMetrics.Visible = false;
                return;
            }

            bool isCampusInvalid = false;
            if ( campusId.HasValue && !campuses.Any( a => a.Id == campusId.Value ) )
            {
                isCampusInvalid = true;
                nbWarning.Visible = true;
                nbWarning.Text = "The campus associated with request is not valid.";
            }

            var filteredCampuses = campuses.Where( a => !campusId.HasValue || isCampusInvalid || a.Id == campusId.Value ).ToList();
            var activeCampuses = CampusCache.All( false );
            var availableCampuses = new List<CampusCache>();

            // If we have more than one campus bind the dropdown list.
            if ( filteredCampuses.Count > 1 )
            {
                bddlCampus.Visible = true;
                divCampusLabel.Visible = false;

                availableCampuses = filteredCampuses;
            }
            else if ( filteredCampuses.Count == 1 && activeCampuses.Count > 1 )
            {
                bddlCampus.Visible = false;
                divCampusLabel.Visible = true;

                lCampus.Text = filteredCampuses[0].Name;
                _selectedCampusId = filteredCampuses[0].Id;
                availableCampuses = filteredCampuses;
            }
            else if ( activeCampuses.Count == 1 )
            {
                bddlCampus.Visible = false;
                divCampusLabel.Visible = false;

                _selectedCampusId = activeCampuses[0].Id;
                availableCampuses = activeCampuses;
            }

            foreach ( var campus in availableCampuses )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
            bddlCampus.SetValue( _selectedCampusId );

            // Load Weeks
            var weeksBack = GetAttributeValue( AttributeKey.WeeksBack ).AsInteger();
            var weeksAhead = GetAttributeValue( AttributeKey.WeeksAhead ).AsInteger();
            var weekEndDates = GetWeekendDates( weeksBack, weeksAhead );

            foreach ( var date in weekEndDates )
            {
                bddlWeekend.Items.Add( new ListItem( "Sunday " + date.ToShortDateString(), date.ToString( "o" ) ) );
            }

            var defaultToCurrentWeek = GetAttributeValue( AttributeKey.DefaultToCurrentWeek ).AsBoolean();
            if ( defaultToCurrentWeek )
            {
                _selectedWeekend = RockDateTime.Today.SundayDate();
            }

            bddlWeekend.SetValue( _selectedWeekend.HasValue ? _selectedWeekend.Value.ToString( "o" ) : null );
            LoadServicesDropDown();
        }

        private void LoadServicesDropDown()
        {
            bddlService.Items.Clear();
            // Load service times
            foreach ( var service in GetServices() )
            {
                var listItemText = service.Name;

                if ( _selectedWeekend != null && GetAttributeValue( AttributeKey.MetricDateDeterminedBy ).AsInteger() == 1 )
                {
                    var date = GetFirstScheduledDate( _selectedWeekend, service );
                    if ( date == null )
                    {
                        listItemText = string.Empty;
                    }
                    else
                    {
                        listItemText = string.Format( "{0} ({1})", service.Name, date.ToShortDateString() );
                    }
                }

                if ( listItemText.IsNotNullOrWhiteSpace() )
                {
                    bddlService.Items.Add( new ListItem( listItemText, service.Id.ToString() ) );
                }
            }
            bddlService.SetValue( _selectedServiceId );

            var showServicesDropdown = bddlService.Items.Count != 0;

            bddlService.Visible = showServicesDropdown;
            pnlMetricEdit.Visible = showServicesDropdown;

            pnlNoServices.Visible = !showServicesDropdown;
            nbWarning.Visible = !showServicesDropdown;
            if ( !showServicesDropdown )
            {
                nbWarning.Text = "No services exist for the selected campus and date. Change the date or campus to find the desired service.";
            }

        }

        /// <summary>
        /// Gets the campuses.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampuses()
        {
            var campuses = new List<CampusCache>();
            var allowedCampuses = GetAttributeValue(  AttributeKey.Campuses ).SplitDelimitedValues().AsGuidList();
            var limitCampusByCampusTeam = GetAttributeValue( AttributeKey.LimitCampusByCampusTeam ).AsBoolean();
            var selectedCampusTypes = GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList();
            var selectedCampusStatuses = GetAttributeValue( AttributeKey.CampusStatus ).SplitDelimitedValues().AsGuidList();

            var campusQuery = CampusCache.All().Where( c => c.IsActive.HasValue && c.IsActive.Value );

            // If specific campuses are selected, filter by those campuses.
            if ( allowedCampuses.Any() )
            {
                campusQuery = campusQuery.Where( c => allowedCampuses.Contains( c.Guid ) );
            }

            if ( limitCampusByCampusTeam )
            {
                var campusTeamGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM.AsGuid() );
                var teamGroupIds = new GroupService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( g => g.GroupTypeId == campusTeamGroupTypeId )
                    .Where( g => g.Members.Where( gm => gm.PersonId == CurrentPersonId ).Any() )
                    .Select( g => g.Id ).ToList();

                campusQuery = campusQuery.Where( c => c.TeamGroupId.HasValue && teamGroupIds.Contains( c.TeamGroupId.Value ) );
            }

            // If campus types are selected, filter by those.
            if ( selectedCampusTypes.Any() )
            {
                var campusTypes = DefinedValueCache.All().Where( d => selectedCampusTypes.Contains( d.Guid ) ).Select( d => d.Id ).ToList();
                campusQuery = campusQuery.Where( c => c.CampusTypeValueId.HasValue && campusTypes.Contains( c.CampusTypeValueId.Value ) );
            }

            // If campus statuses are selected, filter by those.
            if ( selectedCampusStatuses.Any() )
            {
                var campusStatuses = DefinedValueCache.All().Where( d => selectedCampusStatuses.Contains( d.Guid ) ).Select( d => d.Id ).ToList();
                campusQuery = campusQuery.Where( c => c.CampusStatusValueId.HasValue && campusStatuses.Contains( c.CampusStatusValueId.Value ) );
            }

            // Sort by name.
            campusQuery = campusQuery.OrderBy( c => c.Name );

            foreach ( var campus in campusQuery )
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
            var scheduleCategoryGuids = GetAttributeValue( AttributeKey.ScheduleCategory ).SplitDelimitedValues().AsGuidList();
            foreach ( var scheduleCategoryGuid in scheduleCategoryGuids )
            {
                var scheduleCategory = CategoryCache.Get( scheduleCategoryGuid );
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
            }

            var filterByCampus = GetAttributeValue( AttributeKey.FilterByCampus ).AsBoolean();
            if ( filterByCampus )
            {
                var campus = CampusCache.Get( _selectedCampusId.Value );
                services = services.Where( s => campus.CampusScheduleIds.Contains( s.Id ) ).ToList();
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
                var preferences = GetBlockPersonPreferences();
                preferences.SetValue( UserPreferenceKey.CampusId, campusId.HasValue ? campusId.Value.ToString() : "" );
                preferences.SetValue( UserPreferenceKey.ScheduleId, scheduleId.HasValue ? scheduleId.Value.ToString() : "" );
                preferences.Save();

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