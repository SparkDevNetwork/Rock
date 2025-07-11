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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.Extensions.Logging;

using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// Shows a graph of attendance statistics which can be configured for specific groups, date range, etc.
    /// </summary>
    [DisplayName( "Attendance Analytics" )]
    [Category( "Check-in" )]
    [Description( "Shows a graph of attendance statistics which can be configured for specific groups, date range, etc." )]

    [GroupTypesField(
        name: "Group Types",
        description: "Optional List of specific group types that should be included. If none are selected, an option to select an attendance type will be displayed and all of that attendance area's areas will be available.",
        required: false,
        defaultGroupTypeGuids: "",
        category: "",
        order: 0,
        key: AttributeKeys.GroupTypes )]

    [BooleanField(
        name: "Include Inactive Campuses",
        description: "Should campus filter include inactive campuses?",
        defaultValue: false,
        category: "",
        order: 1,
        key: AttributeKeys.IncludeInactiveCampuses )]

    [BooleanField(
        name: "Show All Groups",
        description: "Should all of the available groups be listed individually with checkboxes? If not, a group dropdown will be used instead for selecting the desired groups",
        defaultValue: true,
        category: "",
        order: 2,
        key: AttributeKeys.ShowAllGroups )]

    [BooleanField(
        name: "Show Group Ancestry",
        description: "By default the group ancestry path is shown.  Unselect this to show only the group name.",
        defaultValue: true,
        category: "",
        order: 3,
        key: AttributeKeys.ShowGroupAncestry )]

    [LinkedPage(
        name: "Detail Page",
        description: "Select the page to navigate to when the chart is clicked",
        required: false,
        defaultValue: "",
        category: "",
        order: 4,
        key: AttributeKeys.DetailPage )]

    [LinkedPage(
        name: "Check-in Detail Page",
        description: "Page that shows the user details for the check-in data.",
        required: false,
        defaultValue: "",
        category: "",
        order: 5,
        key: AttributeKeys.CheckinDetailPage )]

    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.CHART_STYLES,
        name: "Chart Style",
        description: "",
        required: true,
        allowMultiple: false,
        defaultValue: Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK,
        category: "",
        order: 6,
        key: AttributeKeys.ChartStyle )]

    [CategoryField(
        name: "Data View Category(s)",
        description: "The optional data view categories that should be included as an option to filter attendance for. If a category is not selected, all data views will be included.",
        allowMultiple: true,
        entityTypeName: "Rock.Model.DataView",
        entityTypeQualifierColumn: "",
        entityTypeQualifierValue: "",
        required: false,
        defaultValue: "",
        category: "",
        order: 7,
        key: AttributeKeys.DataViewCategories )]

    [BooleanField(
        name: "Group Specific",
        description: "Should this block display attendance only for the selected group?",
        defaultValue: false,
        category: "",
        order: 8,
        key: AttributeKeys.GroupSpecific )]

    [BooleanField(
        name: "Show Schedule Filter",
        description: "Should the Schedules filter be displayed",
        defaultValue: true,
        category: "",
        order: 9,
        key: AttributeKeys.ShowScheduleFilter )]

    [BooleanField( name: "Show Campus Filter",
        description: "Should the Campus filter be displayed?",
        defaultValue: true,
        category: "",
        order: 10,
        key: AttributeKeys.ShowCampusFilter )]

    [DefinedValueField( name: "Campus Types",
        description: "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        definedTypeGuid: Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        allowMultiple: true,
        order: 11,
        key: AttributeKeys.CampusTypes )]

    [DefinedValueField(
        name: "Campus Statuses",
        description: "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        definedTypeGuid: Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        allowMultiple: true,
        order: 12,
        key: AttributeKeys.CampusStatuses )]

    [BooleanField( name: "Show View By Option",
        description: "Should the option to view 'Attendees' vs 'Parents of Attendees' vs 'Children of Attendees' be displayed when viewing the grid? If not displayed, the grid will always show attendees.",
        defaultValue: true,
        category: "",
        order: 13,
        key: AttributeKeys.ShowViewByOption )]

    [BooleanField(
        name: "Show Bulk Update Option",
        description: "Should the Bulk Update option be allowed from the attendance grid?",
        defaultValue: true,
        category: "",
        order: 14,
        key: AttributeKeys.ShowBulkUpdateOption )]

    [CustomDropdownListField(
        name: "Filter Column Direction",
        description: "Choose the direction for the checkboxes for filter selections.",
        listSource: "vertical^Vertical,horizontal^Horizontal",
        required: true,
        defaultValue: "vertical",
        order: 15,
        key: AttributeKeys.FilterColumnDirection )]

    [IntegerField(
        name: "Filter Column Count",
        description: "The number of check boxes for each row.",
        required: false,
        defaultValue: 1,
        order: 16,
        key: AttributeKeys.FilterColumnCount )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKeys.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 17 )]

    [Rock.SystemGuid.BlockTypeGuid( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D" )]
    public partial class AttendanceAnalytics : RockBlock
    {
        private static class AttributeKeys
        {
            public const string GroupTypes = "GroupTypes";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string ShowAllGroups = "ShowAllGroups";
            public const string ShowGroupAncestry = "ShowGroupAncestry";
            public const string DetailPage = "DetailPage";
            public const string CheckinDetailPage = "Check-inDetailPage";
            public const string DataViewCategories = "DataViewCategories";
            public const string GroupSpecific = "GroupSpecific";
            public const string ShowScheduleFilter = "ShowScheduleFilter";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string ShowViewByOption = "ShowViewByOption";
            public const string ShowBulkUpdateOption = "ShowBulkUpdateOption";
            public const string FilterColumnCount = "FilterColumnCount";
            public const string FilterColumnDirection = "FilterColumnDirection";
            public const string ChartStyle = "ChartStyle";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        #region Fields

        private bool FilterIncludedInURL = false;
        private bool _isGroupSpecific = false;
        private Group _specificGroup = null;

        private List<DateTime> _possibleAttendances = null;
        private Dictionary<int, string> _scheduleNameLookup = null;
        private Dictionary<int, Location> _personLocations = null;
        private Dictionary<int, List<PhoneNumber>> _personPhoneNumbers = null;

        private bool _currentlyExporting = false;

        private int databaseTimeoutSeconds;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            databaseTimeoutSeconds = GetAttributeValue( AttributeKeys.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

            cbShowInactive.Checked = GetBlockPersonPreferences().GetValue( "show-inactive" ).AsBoolean();

            // Determine if the block should be for a specific group
            _isGroupSpecific = GetAttributeValue( AttributeKeys.GroupSpecific ).AsBoolean();
            if ( _isGroupSpecific )
            {
                int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    _specificGroup = new GroupService( rockContextAnalytics ).Get( groupId.Value );
                    if ( _specificGroup != null )
                    {
                        lSpecificGroupName.Text = string.Format( ": {0}", _specificGroup.Name );
                    }
                }

                if ( _specificGroup == null || ( !IsUserAuthorized( Rock.Security.Authorization.VIEW ) && !_specificGroup.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ) )
                {
                    nbInvalidGroup.Visible = true;
                    pnlContent.Visible = false;
                }

                btnCopyToClipboard.Visible = false;
            }
            else
            {
                btnCopyToClipboard.Visible = true;
                RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
                string script = string.Format( @"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
                ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gChartAttendance.GridRebind += gChartAttendance_GridRebind;
            gAttendeesAttendance.GridRebind += gAttendeesAttendance_GridRebind;

            gAttendeesAttendance.EntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            gAttendeesAttendance.Actions.ShowBulkUpdate = GetAttributeValue( AttributeKeys.ShowBulkUpdateOption ).AsBoolean( true );
            gAttendeesAttendance.Actions.ShowMergePerson = !_isGroupSpecific;
            gAttendeesAttendance.Actions.ShowMergeTemplate = !_isGroupSpecific;

            dvpDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            dvpDataView.CategoryGuids = GetAttributeValue( AttributeKeys.DataViewCategories ).SplitDelimitedValues().AsGuidList();

            // show / hide the checkin details page
            btnCheckinDetails.Visible = !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKeys.CheckinDetailPage ) );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // GroupTypesUI dynamically creates controls, so we need to rebuild it on every OnLoad()
            BuildGroupTypesUI( false );

            var chartStyle = GetChartStyle();
            lcAttendance.SetChartStyle( chartStyle );
            bcAttendance.SetChartStyle( chartStyle );

            if ( Page.IsPostBack )
            {
                // Assign event handlers to process the postback.
                if ( this.DetailPageGuid.HasValue )
                {
                    lcAttendance.ChartClick += ChartClickEventHandler;
                    bcAttendance.ChartClick += ChartClickEventHandler;
                }
            }
            else
            {
                lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );

                LoadDropDowns();
                try
                {
                    LoadSettings();
                    if ( ( !_isGroupSpecific && FilterIncludedInURL ) || ( _isGroupSpecific && _specificGroup != null ) )
                    {
                        LoadChartAndGrids();
                    }
                }
                catch ( Exception exception )
                {
                    LogAndShowException( exception );
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        private ChartStyle GetChartStyle()
        {
            var chartStyle = new ChartStyle();

            var chartStyleDefinedValueGuid = GetAttributeValue( AttributeKeys.ChartStyle ).AsGuidOrNull();

            if ( chartStyleDefinedValueGuid.HasValue )
            {
                var rockContext = new RockContext();
                var definedValue = DefinedValueCache.Get( chartStyleDefinedValueGuid.Value );
                if ( definedValue != null )
                {
                    try
                    {
                        definedValue.LoadAttributes( rockContext );

                        chartStyle = ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( AttributeKeys.ChartStyle ) );
                    }
                    catch
                    {
                        // intentionally ignore and default to basic style
                    }
                }
            }

            return chartStyle;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        public Guid? DetailPageGuid
        {
            get
            {
                return ( GetAttributeValue( AttributeKeys.DetailPage ) ?? string.Empty ).AsGuidOrNull();
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
            string repeatDirection = GetAttributeValue( AttributeKeys.FilterColumnDirection );
            int repeatColumns = GetAttributeValue( AttributeKeys.FilterColumnCount ).AsIntegerOrNull() ?? 0;
            clbCampuses.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            clbCampuses.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;

            BuildGroupTypesUI( true );

            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendeesAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gAttendeesAttendance_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAttendeesGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the GridRebind event of the gChartAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gChartAttendance_GridRebind( object sender, EventArgs e )
        {
            BindChartAttendanceGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            string repeatDirection = GetAttributeValue( AttributeKeys.FilterColumnDirection );
            int repeatColumns = GetAttributeValue( AttributeKeys.FilterColumnCount ).AsIntegerOrNull() ?? 0;

            bool includeInactiveCampuses = GetAttributeValue( AttributeKeys.IncludeInactiveCampuses ).AsBoolean();

            clbCampuses.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            clbCampuses.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;
            clbCampuses.Items.Clear();
            var noCampusListItem = new ListItem();
            noCampusListItem.Text = "<span title='Include records that are not associated with a campus'>No Campus</span>";
            noCampusListItem.Value = "null";
            clbCampuses.Items.Add( noCampusListItem );
            foreach ( var campus in GetCampuses( includeInactiveCampuses ) )
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                clbCampuses.Items.Add( listItem );
            }

            if ( !_isGroupSpecific )
            {
                var groupTypeGuids = this.GetAttributeValue( AttributeKeys.GroupTypes ).SplitDelimitedValues().AsGuidList();
                if ( !groupTypeGuids.Any() )
                {
                    // show the CheckinType control if there isn't a block setting for specific group types
                    ddlAttendanceArea.Visible = true;
                    var groupTypeService = new GroupTypeService( new RockContextAnalytics() );
                    Guid groupTypePurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
                    ddlAttendanceArea.GroupTypes = groupTypeService.Queryable()
                            .Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeGuid )
                            .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                }
                else
                {
                    // hide the CheckinType control if there is a block setting for group types
                    ddlAttendanceArea.Visible = false;
                }
            }
            else
            {
                ddlAttendanceArea.Visible = false;
            }
        }

        /// <summary>
        /// Gets the list of available campuses after filtering by any selected statuses or types.
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        private List<CampusCache> GetCampuses( bool includeInactive )
        {
            var campusTypeIds = GetAttributeValues( AttributeKeys.CampusTypes )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var campusStatusIds = GetAttributeValues( AttributeKeys.CampusStatuses )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            return CampusCache.All( includeInactive )
                .Where( c => ( !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                    && ( !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) ) )
                .ToList();
        }

        /// <summary>
        /// Builds the group types UI
        /// </summary>
        private void BuildGroupTypesUI( bool clearSelection )
        {
            if ( !_isGroupSpecific )
            {
                var groupTypes = this.GetSelectedGroupTypes();
                if ( groupTypes.Any() )
                {
                    nbGroupTypeWarning.Visible = false;

                    // only add each grouptype/group once in case they are a child of multiple parents
                    _addedGroupTypeIds = new List<int>();
                    _addedGroupIds = new List<int>();

                    var showAllGroups = GetAttributeValue( AttributeKeys.ShowAllGroups ).AsBoolean();
                    if ( showAllGroups )
                    {
                        rptGroupTypes.DataSource = groupTypes.ToList();
                        rptGroupTypes.DataBind();

                        pnlGroups.Visible = true;
                        gpGroups.Visible = false;
                    }
                    else
                    {
                        gpGroups.IncludedGroupTypeIds = groupTypes.Select( t => t.Id ).ToList();
                        if ( clearSelection )
                        {
                            gpGroups.SetValues( null );
                            BindSelectedGroups();
                        }

                        gpGroups.Visible = true;

                        pnlGroups.Visible = false;
                        gpGroups.Visible = true;
                    }

                    dvpDataView.Visible = true;
                }
                else
                {
                    pnlGroups.Visible = false;
                    gpGroups.Visible = false;
                    dvpDataView.Visible = false;

                    nbGroupTypeWarning.Text = "Please select a check-in type.";
                    nbGroupTypeWarning.Visible = true;
                }
            }
            else
            {
                nbGroupTypeWarning.Visible = false;

                pnlGroups.Visible = false;
                gpGroups.Visible = false;
                dvpDataView.Visible = false;
            }
        }

        private void BindSelectedGroups()
        {
            var selectedGroupIds = GetSelectedGroupIds();
            bool showGroupAncestry = GetAttributeValue( AttributeKeys.ShowGroupAncestry ).AsBoolean( true );

            using ( var rockContextAnalytics = new RockContextAnalytics() )
            {
                var groupService = new GroupService( rockContextAnalytics );
                var groups = groupService.Queryable().AsNoTracking()
                    .Where( g => selectedGroupIds.Contains( g.Id ) )
                    .ToList();

                var groupList = new List<string>();

                foreach ( int id in selectedGroupIds )
                {
                    var group = groups.FirstOrDefault( g => g.Id == id );
                    if ( group != null )
                    {
                        groupList.Add( showGroupAncestry ? groupService.GroupAncestorPathName( id ) : group.Name );
                    }
                }

                rptSelectedGroups.DataSource = groupList;
                rptSelectedGroups.DataBind();
                rcwSelectedGroups.Visible = groupList.Any();
            }
        }

        /// <summary>
        /// Gets the type of the selected template group (Check-In Type)
        /// </summary>
        /// <returns></returns>
        private List<GroupType> GetSelectedGroupTypes()
        {
            if ( !_isGroupSpecific )
            {
                var groupTypeGuids = this.GetAttributeValue( AttributeKeys.GroupTypes ).SplitDelimitedValues().AsGuidList();
                var groupTypeService = new GroupTypeService( new RockContextAnalytics() );
                if ( groupTypeGuids.Any() )
                {
                    var groupTypes = groupTypeGuids.Select( a => GroupTypeCache.Get( a ) ).Where( a => a != null )
                        .OrderBy( t => t.Order )
                        .ThenBy( t => t.Name )
                        .ToList();

                    foreach ( var groupType in groupTypes.ToList() )
                    {
                        foreach ( var childGroupType in groupTypeService.GetCheckinAreaDescendantsOrdered( groupType.Id ) )
                        {
                            if ( !groupTypes.Any( t => t.Id == childGroupType.Id ) )
                            {
                                groupTypes.Add( childGroupType );
                            }
                        }
                    }

                    var groupTypeIds = groupTypes.Select( a => a.Id ).ToList();
                    return groupTypeService.GetByIds( groupTypeIds ).ToList();
                }
                else
                {
                    if ( ddlAttendanceArea.SelectedGroupTypeId.HasValue )
                    {
                        var groupTypeIds = groupTypeService
                            .GetCheckinAreaDescendantsOrdered( ddlAttendanceArea.SelectedGroupTypeId.Value )
                            .Select( a => a.Id )
                            .ToList();

                        return groupTypeService.GetByIds( groupTypeIds ).ToList();
                    }
                }
            }

            return new List<GroupType>();
        }

        /// <summary>
        /// Loads the chart and any visible grids
        /// </summary>
        public void LoadChartAndGrids()
        {
            pnlUpdateMessage.Visible = false;
            pnlResults.Visible = true;

            lcAttendance.ShowTooltip = true;
            bcAttendance.ShowTooltip = true;
            if ( this.DetailPageGuid.HasValue )
            {
                lcAttendance.ChartClick += ChartClickEventHandler;
                bcAttendance.ChartClick += ChartClickEventHandler;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( !dateRange.Start.HasValue || !dateRange.End.HasValue )
            {
                nbDateRangeWarning.Visible = true;
                return;
            }
            nbDateRangeWarning.Visible = false;

            var selectedGroupIds = GetSelectedGroupIds();
            // if no Groups are selected show a warning since no data will show up
            nbGroupsWarning.Visible = false;
            if ( !selectedGroupIds.Any() )
            {
                nbGroupsWarning.Visible = true;
                return;
            }

            if ( pnlShowByChart.Visible )
            {
                var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;

                lcAttendance.TooltipContentScript = GetChartTooltipScript( groupBy );

                string intervalType = null;
                string intervalSize = null;
                switch ( groupBy )
                {
                    case ChartGroupBy.Week:
                        {
                            intervalType = "week";
                            intervalSize = "1";
                        }
                        break;

                    case ChartGroupBy.Month:
                        {
                            intervalType = "month";
                            intervalSize = "1";
                        }
                        break;

                    case ChartGroupBy.Year:
                        {
                            intervalType = "year";
                            intervalSize = "1";
                        }
                        break;
                }
                lcAttendance.SeriesGroupIntervalType = intervalType;
                lcAttendance.SeriesGroupIntervalSize = intervalSize;

                string groupByTextPlural = groupBy.ConvertToString().ToLower().Pluralize();
                lPatternXFor.Text = string.Format( " {0} for the selected date range", groupByTextPlural );
                lPatternAndMissedXBetween.Text = string.Format( " {0} between", groupByTextPlural );

                bcAttendance.TooltipContentScript = lcAttendance.TooltipContentScript;

                var chartData = this.GetAttendanceChartData().ToList();

                var singleDateTime = chartData.GroupBy( a => a.DateTimeStamp ).Count() == 1;
                if ( singleDateTime )
                {
                    var chartDataByCategory = ChartDataFactory.GetCategorySeriesFromChartData( chartData );
                    bcAttendance.SetChartDataItems( chartDataByCategory );
                }
                else
                {
                    lcAttendance.SetChartDataItems( chartData );
                }
                bcAttendance.Visible = singleDateTime;
                lcAttendance.Visible = !singleDateTime;

                if ( pnlChartAttendanceGrid.Visible )
                {
                    BindChartAttendanceGrid( chartData );
                }
            }

            if ( pnlShowByAttendees.Visible )
            {
                BindAttendeesGrid();
            }

            SaveSettings();
        }

        private string GetChartTooltipScript( ChartGroupBy groupBy )
        {
            var tooltipScriptTemplate = @"
function(tooltipModel)
{
    var colors = tooltipModel.labelColors[0];
    var style = 'background:' + colors.backgroundColor;
    style += '; border-color:' + colors.borderColor;
    style += '; border-width: 2px';
    style += '; width: 10px';
    style += '; height: 10px';
    style += '; margin-right: 10px';
    style += '; display: inline-block';
    var span = '<span style=""' + style + '""></span>';
<assignContent>
    var html = '<table><thead>';
    html += '<tr><th style=""text-align:center"">' + headerText + '</th></tr>';
    html += '</thead><tbody>';
    html += '<tr><td>' + span + bodyText  + '</td></tr>';
    html += '</tbody></table>';
    return html;
}
";

            string tooltipScript;
            switch ( groupBy )
            {
                case ChartGroupBy.Week:
                default:
                    {
                        var assignContentScript = @"
var bodyText = tooltipModel.body[0].lines[0];
var headerText = 'Weekend of <br />' + tooltipModel.title;
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );
                    }
                    break;

                case ChartGroupBy.Month:
                    {
                        var assignContentScript = @"
var bodyText = tooltipModel.body[0].lines[0];
var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
var dp = tooltipModel.dataPoints[0];
var dataValue = _chartData.data.datasets[dp.datasetIndex].data[dp.index];
var itemDate = new Date( dataValue.x );
var headerText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );

                    }
                    break;

                case ChartGroupBy.Year:
                    {
                        var assignContentScript = @"
var bodyText = tooltipModel.body[0].lines[0];
var dp = tooltipModel.dataPoints[0];
var dataValue = _chartData.data.datasets[dp.datasetIndex].data[dp.index];
var itemDate = new Date( dataValue.x );
var headerText = dp.label;
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );
                    }
                    break;
            }

            return tooltipScript;
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettings()
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "TemplateGroupTypeId", ddlAttendanceArea.SelectedGroupTypeId.ToString() );

            preferences.SetValue( "SlidingDateRange", drpSlidingDateRange.DelimitedValues );
            preferences.SetValue( "GroupBy", hfGroupBy.Value );
            preferences.SetValue( "GraphBy", hfGraphBy.Value );
            preferences.SetValue( "CampusIds", clbCampuses.SelectedValues.AsDelimited( "," ) );
            preferences.SetValue( "ScheduleIds", spSchedules.SelectedValues.ToList().AsDelimited( "," ) );
            preferences.SetValue( "DataView", dvpDataView.SelectedValue );

            var selectedGroupIds = GetSelectedGroupIds();
            preferences.SetValue( "GroupIds", selectedGroupIds.AsDelimited( "," ) );

            preferences.SetValue( "ShowBy", hfShowBy.Value );

            preferences.SetValue( "ViewBy", hfViewBy.Value );

            AttendeesFilterBy attendeesFilterBy;
            if ( radByVisit.Checked )
            {
                attendeesFilterBy = AttendeesFilterBy.ByVisit;
            }
            else if ( radByPattern.Checked )
            {
                attendeesFilterBy = AttendeesFilterBy.Pattern;
            }
            else
            {
                attendeesFilterBy = AttendeesFilterBy.All;
            }

            preferences.SetValue( "AttendeesFilterByType", attendeesFilterBy.ConvertToInt().ToString() );
            preferences.SetValue( "AttendeesFilterByVisit", ddlNthVisit.SelectedValue );
            preferences.SetValue( "AttendeesFilterByPattern", string.Format( "{0}|{1}|{2}|{3}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, tbPatternMissedXTimes.Text, drpPatternDateRange.DelimitedValues ) );

            preferences.Save();

            // Create URL for selected settings
            var pageReference = CurrentPageReference;
            foreach ( var key in preferences.GetKeys() )
            {
                pageReference.Parameters.AddOrReplace( key, preferences.GetValue( key ) );
            }

            Uri uri = new Uri( Request.UrlProxySafe().ToString() );
            btnCopyToClipboard.Attributes["data-clipboard-text"] = uri.GetLeftPart( UriPartial.Authority ) + pageReference.BuildUrl();
            btnCopyToClipboard.Disabled = false;
        }

        /// <summary>
        /// Gets the selected group ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var selectedGroupIds = new List<int>();

            if ( _isGroupSpecific )
            {
                if ( _specificGroup != null )
                {
                    selectedGroupIds.Add( _specificGroup.Id );
                }
            }
            else
            {
                var showAllGroups = GetAttributeValue( AttributeKeys.ShowAllGroups ).AsBoolean();
                if ( showAllGroups )
                {
                    var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
                    foreach ( var cblGroup in checkboxListControls )
                    {
                        selectedGroupIds.AddRange( cblGroup.SelectedValuesAsInt );
                    }
                }
                else
                {
                    selectedGroupIds = gpGroups.SelectedValuesAsInt().ToList();
                }
            }

            return selectedGroupIds;
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettings()
        {
            FilterIncludedInURL = false;

            var preferences = GetBlockPersonPreferences();

            if ( !_isGroupSpecific )
            {
                ddlAttendanceArea.SelectedGroupTypeId = GetSetting( preferences, "TemplateGroupTypeId" ).AsIntegerOrNull();
                cbIncludeGroupsWithoutSchedule.Checked = preferences.GetValue( "IncludeGroupsWithoutSchedule" ).AsBooleanOrNull() ?? true;
                BuildGroupTypesUI( false );
            }

            string slidingDateRangeSettings = GetSetting( preferences, "SlidingDateRange" );
            if ( string.IsNullOrWhiteSpace( slidingDateRangeSettings ) )
            {
                // default to current year
                drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;
            }
            else
            {
                drpSlidingDateRange.DelimitedValues = slidingDateRangeSettings;
            }

            dvpDataView.SetValue( GetSetting( preferences, "DataView" ).AsIntegerOrNull() );

            hfGroupBy.Value = GetSetting( preferences, "GroupBy" );
            hfGraphBy.Value = GetSetting( preferences, "GraphBy" );

            if ( GetAttributeValue( AttributeKeys.ShowCampusFilter ).AsBoolean() )
            {
                clbCampuses.Visible = true;

                var campusIdList = new List<string>();
                string campusQryString = Request.QueryString["CampusIds"];
                if ( campusQryString != null )
                {
                    FilterIncludedInURL = true;
                    campusIdList = campusQryString.Split( ',' ).ToList();
                    clbCampuses.SetValues( campusIdList );
                }
                else
                {
                    if ( preferences.ContainsKey( "CampusIds" ) )
                    {
                        campusIdList = preferences.GetValue( "CampusIds" ).Split( ',' ).ToList();
                        clbCampuses.SetValues( campusIdList );
                    }
                    else
                    {
                        // if previous campus selection has never been made, default to showing all of them
                        foreach ( ListItem item in clbCampuses.Items )
                        {
                            item.Selected = true;
                        }
                    }
                }
            }
            else
            {
                clbCampuses.Visible = false;
            }

            if ( GetAttributeValue( AttributeKeys.ShowScheduleFilter ).AsBoolean() )
            {
                spSchedules.Visible = true;

                var scheduleIdList = GetSetting( preferences, "ScheduleIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList();
                if ( scheduleIdList.Any() )
                {
                    var schedules = new ScheduleService( new RockContextAnalytics() )
                        .Queryable().AsNoTracking()
                        .Where( s => scheduleIdList.Contains( s.Id ) )
                        .ToList();
                    spSchedules.SetValues( schedules );
                }
            }
            else
            {
                spSchedules.Visible = false;
            }

            var groupIdList = GetSetting( preferences, "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            // if no groups are selected, and option to show all groups is configured, default to showing all of them

            var showAllGroups = GetAttributeValue( AttributeKeys.ShowAllGroups ).AsBoolean();
            if ( showAllGroups )
            {
                var selectAll = groupIdList.Count == 0;
                var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
                foreach ( var cblGroup in checkboxListControls )
                {
                    foreach ( ListItem item in cblGroup.Items )
                    {
                        item.Selected = selectAll || groupIdList.Contains( item.Value );
                    }
                }
            }
            else
            {
                gpGroups.SetValues( groupIdList.AsIntegerList() );
                BindSelectedGroups();
            }

            ShowBy showBy = GetSetting( preferences, "ShowBy" ).ConvertToEnumOrNull<ShowBy>() ?? ShowBy.Chart;
            DisplayShowBy( showBy );

            if ( GetAttributeValue( AttributeKeys.ShowViewByOption ).AsBoolean() )
            {
                pnlViewBy.Visible = true;
                ViewBy viewBy = GetSetting( preferences, "ViewBy" ).ConvertToEnumOrNull<ViewBy>() ?? ViewBy.Attendees;
                hfViewBy.Value = viewBy.ConvertToInt().ToString();
            }
            else
            {
                pnlViewBy.Visible = false;
                hfViewBy.Value = ViewBy.Attendees.ConvertToInt().ToString();
            }

            AttendeesFilterBy attendeesFilterBy = GetSetting( preferences, "AttendeesFilterByType" ).ConvertToEnumOrNull<AttendeesFilterBy>() ?? AttendeesFilterBy.All;

            switch ( attendeesFilterBy )
            {
                case AttendeesFilterBy.All:
                    radAllAttendees.Checked = true;
                    break;

                case AttendeesFilterBy.ByVisit:
                    radByVisit.Checked = true;
                    break;

                case AttendeesFilterBy.Pattern:
                    radByPattern.Checked = true;
                    break;

                default:
                    radAllAttendees.Checked = true;
                    break;
            }

            ddlNthVisit.SelectedValue = GetSetting( preferences, "AttendeesFilterByVisit" );
            string attendeesFilterByPattern = GetSetting( preferences, "AttendeesFilterByPattern" );
            string[] attendeesFilterByPatternValues = attendeesFilterByPattern.Split( '|' );
            if ( attendeesFilterByPatternValues.Length == 4 )
            {
                tbPatternXTimes.Text = attendeesFilterByPatternValues[0];
                cbPatternAndMissed.Checked = attendeesFilterByPatternValues[1].AsBooleanOrNull() ?? false;
                tbPatternMissedXTimes.Text = attendeesFilterByPatternValues[2];
                drpPatternDateRange.DelimitedValues = attendeesFilterByPatternValues[3];
            }
        }

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSetting( PersonPreferenceCollection preferences, string key )
        {
            string setting = Request.QueryString[key];
            if ( setting != null )
            {
                FilterIncludedInURL = true;
                return setting;
            }

            return preferences.GetValue( key );
        }

        /// <summary>
        /// Handles the chart click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void ChartClickEventHandler( object sender, ChartClickArgs e )
        {
            if ( this.DetailPageGuid.HasValue )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString.Add( "YValue", e.YValue.ToString() );
                qryString.Add( "DateTimeValue", e.DateTimeValue.ToString( "o" ) );
                NavigateToPage( this.DetailPageGuid.Value, qryString );
            }
        }

        /// <summary>
        /// Handles the Click event of the lShowGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lShowChartAttendanceGrid_Click( object sender, EventArgs e )
        {
            if ( pnlChartAttendanceGrid.Visible )
            {
                pnlChartAttendanceGrid.Visible = false;
                lShowChartAttendanceGrid.Text = "Show Data <i class='fa fa-chevron-down'></i>";
                lShowChartAttendanceGrid.ToolTip = "Show Data";
            }
            else
            {
                pnlChartAttendanceGrid.Visible = true;
                lShowChartAttendanceGrid.Text = "Hide Data <i class='fa fa-chevron-up'></i>";
                lShowChartAttendanceGrid.ToolTip = "Hide Data";
                BindChartAttendanceGrid();
            }
        }

        /// <summary>
        /// Binds the chart attendance grid.
        /// </summary>
        private void BindChartAttendanceGrid()
        {
            var chartData = GetAttendanceChartData();

            BindChartAttendanceGrid( chartData );
        }

        /// <summary>
        /// Binds the chart attendance grid.
        /// </summary>
        /// <param name="chartData">The chart data.</param>
        private void BindChartAttendanceGrid( IEnumerable<Rock.Chart.IChartData> chartData )
        {
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<AttendanceGraphBy>() ?? AttendanceGraphBy.Total;
            gChartAttendance.Columns[1].Visible = graphBy != AttendanceGraphBy.Total;
            switch ( graphBy )
            {
                case AttendanceGraphBy.Group:
                    gChartAttendance.Columns[1].HeaderText = "Group";
                    break;
                case AttendanceGraphBy.Campus:
                    gChartAttendance.Columns[1].HeaderText = "Campus";
                    break;
                case AttendanceGraphBy.Location:
                    gChartAttendance.Columns[1].HeaderText = "Location";
                    break;
                case AttendanceGraphBy.Schedule:
                    gChartAttendance.Columns[1].HeaderText = "Schedule";
                    break;
            }

            SortProperty sortProperty = gChartAttendance.SortProperty;

            if ( sortProperty != null )
            {
                gChartAttendance.DataSource = chartData.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gChartAttendance.DataSource = chartData.OrderBy( a => a.DateTimeStamp ).ToList();
            }

            gChartAttendance.DataBind();
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Rock.Chart.IChartData> GetAttendanceChartData()
        {
            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<AttendanceGraphBy>() ?? AttendanceGraphBy.Total;

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.End == null )
            {
                dateRange.End = RockDateTime.Now;
            }

            // Adjust the start/end times to reflect the attendance dates who's SundayDate value would fall between the date range selected
            var start = dateRange.Start.HasValue ?
                dateRange.Start.Value.Date.AddDays( 0 - ( dateRange.Start.Value.DayOfWeek == DayOfWeek.Sunday ? 6 : ( int ) dateRange.Start.Value.DayOfWeek - 1 ) ) :
                new DateTime( 1900, 1, 1 );

            var end = dateRange.End.HasValue ?
                dateRange.End.Value.AddDays( 0 - ( int ) dateRange.End.Value.DayOfWeek ) :
                new DateTime( 2100, 1, 1, 23, 59, 59 );

            if ( end < start )
            {
                end = end.AddDays( start.Subtract( end ).Days + 6 );
            }

            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            string campusIds = GetAttributeValue( AttributeKeys.ShowCampusFilter ).AsBoolean() ? clbCampuses.SelectedValues.AsDelimited( "," ) : string.Empty;
            var dataView = dvpDataView.SelectedValueAsInt();
            var scheduleIds = GetAttributeValue( AttributeKeys.ShowScheduleFilter ).AsBoolean() ? spSchedules.SelectedValues.ToList().AsDelimited( "," ) : string.Empty;

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

            var chartData = new AttendanceService( rockContextAnalytics ).GetChartData( groupBy, graphBy, start, end, groupIds, campusIds, dataView, scheduleIds );

            return chartData;
        }

        /// <summary>
        /// Binds the attendees grid.
        /// </summary>
        private void BindAttendeesGrid( bool isExporting = false )
        {
            // Get Group Type filter
            var groupTypeIdList = new List<int>();
            if ( !_isGroupSpecific )
            {
                var groupTypes = this.GetSelectedGroupTypes();
                if ( groupTypes == null || !groupTypes.Any() )
                {
                    return;
                }
                groupTypeIdList = groupTypes.Select( t => t.Id ).ToList();
            }
            else
            {
                if ( _specificGroup != null )
                {
                    groupTypeIdList.Add( _specificGroup.GroupTypeId );
                }
                else
                {
                    groupTypeIdList.Add( 0 );
                }
            }

            // Get the daterange filter
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.End == null )
            {
                dateRange.End = RockDateTime.Now;
            }
            var start = dateRange.Start;

            var endTime = dateRange.End.Value;
            // We need to pass '11/09/2020 11:59:59.000', because SQL Server rounds '11/09/2020 11:59:59.999' up to '11/10/2020 12:00:00.000'.
            // Since this is used for Sunday Date the milliseconds don't matter
            var end = new DateTime( endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second );

            // Get the group filter
            var groupIdList = new List<int>();
            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            if ( !string.IsNullOrWhiteSpace( groupIds ) )
            {
                groupIdList = groupIds.Split( ',' ).AsIntegerList();
            }

            // If campuses were included, filter attendances by those that have selected campuses
            // if 'null' is one of the campuses, treat that as a 'CampusId is Null'
            var includeNullCampus = true;
            List<int> campusIdList = null;
            if ( GetAttributeValue( AttributeKeys.ShowCampusFilter ).AsBoolean() )
            {
                includeNullCampus = clbCampuses.SelectedValues.Any( a => a.Equals( "null", StringComparison.OrdinalIgnoreCase ) );
                campusIdList = clbCampuses.SelectedValues.AsIntegerList();
                campusIdList.Remove( 0 ); // remove 0 from the list, just in case it is there
                if ( !includeNullCampus && !campusIdList.Any() )
                {
                    campusIdList = null;
                }
            }

            var scheduleIdList = GetAttributeValue( AttributeKeys.ShowScheduleFilter ).AsBoolean() ? spSchedules.SelectedValues.AsIntegerList() : new List<int>();
            scheduleIdList.Remove( 0 );
            if ( !scheduleIdList.Any() )
            {
                scheduleIdList = null;
            }

            // we want to get the first 2 visits at a minimum so we can show the dates in the grid
            int nthVisitsTake = 2;
            int? byNthVisit = null;
            if ( radByVisit.Checked )
            {
                // If we are filtering by nth visit, we might want to get up to first 5
                byNthVisit = ddlNthVisit.SelectedValue.AsIntegerOrNull();
                if ( byNthVisit.HasValue && byNthVisit > 2 )
                {
                    nthVisitsTake = byNthVisit.Value;
                }
            }
            bool showNonAttenders = byNthVisit.HasValue && byNthVisit.Value == 0;

            // Get any attendance pattern filters
            int? attendedMinCount = null;
            int? attendedMissedCount = null;
            DateRange attendedMissedDateRange = new DateRange();
            if ( radByPattern.Checked )
            {
                attendedMinCount = tbPatternXTimes.Text.AsIntegerOrNull();
                if ( cbPatternAndMissed.Checked )
                {
                    attendedMissedCount = tbPatternMissedXTimes.Text.AsIntegerOrNull();
                    attendedMissedDateRange = new DateRange( drpPatternDateRange.LowerValue, drpPatternDateRange.UpperValue );
                    if ( !attendedMissedDateRange.Start.HasValue || !attendedMissedDateRange.End.HasValue )
                    {
                        nbMissedDateRangeRequired.Visible = true;
                        return;
                    }
                }
            }
            nbMissedDateRangeRequired.Visible = false;

            // Determine how dates should be grouped
            ChartGroupBy groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;

            // Determine if parents or children are being included with results
            var includeParents = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ParentsOfAttendees;
            var includeChildren = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ChildrenOfAttendees;

            // Attendance results
            var allAttendeeVisits = new Dictionary<int, AttendeeVisits>();
            var allResults = new List<AttendeeResult>();

            // Collection of async queries to run before assembling data
            var qryTasks = new ConcurrentBag<Task>();
            var taskInfos = new ConcurrentBag<TaskInfo>();

            DataTable dtAttendeeLastAttendance = null;
            DataTable dtAttendees = null;
            DataTable dtAttendeeFirstDates = null;
            List<int> personIdsWhoDidNotMiss = null;

            if ( !showNonAttenders )
            {
                // Call the stored procedure to get all the person ids and their attendance dates for anyone
                // with attendance that matches the selected criteria.
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get Attendee Dates", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    DataTable dtAttendeeDates = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsAttendeeDatesDataSet(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                    foreach ( DataRow row in dtAttendeeDates.Rows )
                    {
                        int personId = ( int ) row["PersonId"];
                        allAttendeeVisits.TryAdd( personId, new AttendeeVisits() );
                        var result = allAttendeeVisits[personId];
                        result.PersonId = personId;

                        var summaryDate = DateTime.MinValue;
                        switch ( groupBy )
                        {
                            case ChartGroupBy.Week:
                                summaryDate = ( DateTime ) row["SundayDate"];
                                break;
                            case ChartGroupBy.Month:
                                summaryDate = ( DateTime ) row["MonthDate"];
                                break;
                            case ChartGroupBy.Year:
                                summaryDate = ( DateTime ) row["YearDate"];
                                break;
                        }
                        if ( !result.AttendanceSummary.Contains( summaryDate ) )
                        {
                            result.AttendanceSummary.Add( summaryDate );
                        }
                    }

                    ti.end = RockDateTime.Now;

                } ) );

                // Call the stored procedure to get the last attendance
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get Last Attendance", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    dtAttendeeLastAttendance = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsAttendeeLastAttendanceDataSet(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                    ti.end = RockDateTime.Now;

                } ) );

                // Call the stored procedure to get the names/demographic info for attendees
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get Name/Demographic Data", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    dtAttendees = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsAttendeesDataSet(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList, includeParents, includeChildren ).Tables[0];

                    ti.end = RockDateTime.Now;

                } ) );

                // If checking for missed attendance, get the people who missed that number of dates during the missed date range
                if ( attendedMissedCount.HasValue &&
                    attendedMissedDateRange.Start.HasValue &&
                    attendedMissedDateRange.End.HasValue )
                {
                    qryTasks.Add( Task.Run( () =>
                    {
                        var ti = new TaskInfo { name = "Get Missed Attendee Dates", start = RockDateTime.Now };
                        taskInfos.Add( ti );

                        personIdsWhoDidNotMiss = new List<int>();

                        var threadRockContextAnalytics = new RockContextAnalytics();
                        threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                        DataTable dtAttendeeDatesMissed = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsAttendeeDatesDataSet(
                            groupIdList, attendedMissedDateRange.Start.Value, attendedMissedDateRange.End.Value,
                            campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                        var missedResults = new Dictionary<int, AttendeeResult>();
                        foreach ( DataRow row in dtAttendeeDatesMissed.Rows )
                        {
                            int personId = ( int ) row["PersonId"];
                            missedResults.TryAdd( personId, new AttendeeResult() );
                            var missedResult = missedResults[personId];
                            missedResult.PersonId = personId;

                            var summaryDate = DateTime.MinValue;
                            switch ( groupBy )
                            {
                                case ChartGroupBy.Week:
                                    summaryDate = ( DateTime ) row["SundayDate"];
                                    break;
                                case ChartGroupBy.Month:
                                    summaryDate = ( DateTime ) row["MonthDate"];
                                    break;
                                case ChartGroupBy.Year:
                                    summaryDate = ( DateTime ) row["YearDate"];
                                    break;
                            }

                            if ( !missedResult.AttendanceSummary.Contains( summaryDate ) )
                            {
                                missedResult.AttendanceSummary.Add( summaryDate );
                            }
                        }

                        var missedPossibleDates = GetPossibleAttendancesForDateRange( attendedMissedDateRange, groupBy );
                        int missedPossibleCount = missedPossibleDates.Count();

                        personIdsWhoDidNotMiss = missedResults
                            .Where( m => missedPossibleCount - m.Value.AttendanceSummary.Count < attendedMissedCount.Value )
                            .Select( m => m.Key )
                            .ToList();

                        ti.end = RockDateTime.Now;

                    } ) );
                }

                // Call the stored procedure to get the first five dates that any person attended this group type
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get First Five Dates", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    dtAttendeeFirstDates = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsAttendeeFirstDatesDataSet(
                        groupTypeIdList, groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                    ti.end = RockDateTime.Now;

                } ) );
            }
            else
            {
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get Non-Attendees", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    DataSet ds = new AttendanceService( threadRockContextAnalytics ).GetAttendanceAnalyticsNonAttendeesDataSet(
                        groupTypeIdList, groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList, includeParents, includeChildren );

                    DataTable dtNonAttenders = ds.Tables[0];
                    dtAttendeeFirstDates = ds.Tables[1];
                    dtAttendeeLastAttendance = ds.Tables[2];

                    foreach ( DataRow row in dtNonAttenders.Rows )
                    {
                        int personId = ( int ) row["Id"];

                        var result = new AttendeeResult();
                        result.PersonId = personId;

                        var person = new PersonInfo();
                        person.NickName = row["NickName"].ToString();
                        person.LastName = row["LastName"].ToString();
                        person.Gender = row["Gender"].ToString().ConvertToEnum<Gender>();
                        person.Email = row["Email"].ToString();
                        person.GivingId = row["GivingId"].ToString();
                        person.Birthdate = row["BirthDate"] as DateTime?;
                        person.DeceasedDate = row["DeceasedDate"] as DateTime?;
                        person.Gender = row["Gender"].ToString().ConvertToEnum<Gender>();
                        person.Age = Person.GetAge( person.Birthdate, person.DeceasedDate );
                        person.Grade = dtNonAttenders.Columns.Contains( "GraduationYear" ) ? Person.GradeFormattedFromGraduationYear( row["GraduationYear"] as int? ) : null;

                        person.ConnectionStatusValueId = row["ConnectionStatusValueId"] as int?;
                        result.Person = person;

                        if ( includeParents )
                        {
                            result.ParentId = ( int ) row["ParentId"];
                            var parent = new PersonInfo();
                            parent.NickName = row["ParentNickName"].ToString();
                            parent.LastName = row["ParentLastName"].ToString();
                            parent.Email = row["ParentEmail"].ToString();
                            parent.GivingId = row["ParentGivingId"].ToString();
                            parent.Birthdate = row["ParentBirthDate"] as DateTime?;
                            parent.DeceasedDate = row["ParentDeceasedDate"] as DateTime?;
                            parent.Gender = row["ParentGender"].ToString().ConvertToEnum<Gender>();
                            parent.Age = Person.GetAge( parent.Birthdate, parent.DeceasedDate );
                            result.Parent = parent;
                        }

                        if ( includeChildren )
                        {
                            var child = new PersonInfo();
                            result.ChildId = ( int ) row["ChildId"];
                            child.NickName = row["ChildNickName"].ToString();
                            child.LastName = row["ChildLastName"].ToString();
                            child.Email = row["ChildEmail"].ToString();
                            child.GivingId = row["ChildGivingId"].ToString();
                            child.Birthdate = row["ChildBirthDate"] as DateTime?;
                            child.DeceasedDate = row["ChildDeceasedDate"] as DateTime?;
                            child.Gender = row["ChildGender"].ToString().ConvertToEnum<Gender>();
                            child.Grade = Person.GradeFormattedFromGraduationYear( row["ChildGraduationYear"] as int? );
                            child.Age = Person.GetAge( child.Birthdate, child.DeceasedDate );
                            result.Child = child;
                        }

                        allResults.Add( result );
                    }

                    ti.end = RockDateTime.Now;

                } ) );
            }

            // If a dataview filter was included, find the people who match that criteria
            List<int> dataViewPersonIds = null;
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Get DataView People", start = RockDateTime.Now };
                taskInfos.Add( ti );

                var dataViewId = dvpDataView.SelectedValueAsInt();
                if ( dataViewId.HasValue )
                {
                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    dataViewPersonIds = new List<int>();
                    var dataView = new DataViewService( threadRockContextAnalytics ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var dvPersonService = new PersonService( threadRockContextAnalytics );
                        ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression );

                        SortProperty sort = null;
                        var dataViewPersonIdQry = dvPersonService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sort )
                            .Select( p => p.Id );
                        dataViewPersonIds = dataViewPersonIdQry.ToList();
                    }
                }

                ti.end = RockDateTime.Now;

            } ) );

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

            // Log the task time and parameters information.
            string taskLogMessage = "";
            foreach ( var info in taskInfos )
            {
                taskLogMessage += $"Task Name: {info.name}, Time To Run: {info.TimeToRun()}. ";
            }
            Logger.LogInformation( "Attendance Analytics Task Times: " + taskLogMessage );
            Logger.LogInformation( "Attendance Analytics Parameters: " +
                "groupTypeIdList: {@groupTypeIdList}, groupIdList: {@groupIdList}, start: {@start}, end: {@end}, " +
                "campusIdList: {@campusIdList}, includeNullCampus: {@includeNullCampus}, scheduleIdList: {@scheduleIdList}",
                groupTypeIdList, groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList );

            if ( !showNonAttenders )
            {
                var attendees = allAttendeeVisits.AsQueryable();

                // If dataview filter was included remove anyone not in that dataview
                if ( dataViewPersonIds != null )
                {
                    attendees = attendees.Where( p => dataViewPersonIds.Contains( p.Key ) );
                }

                // If filter for number missed was included, remove anyone who did not match that filter
                if ( personIdsWhoDidNotMiss != null )
                {
                    attendees = attendees.Where( p => !personIdsWhoDidNotMiss.Contains( p.Key ) );
                }

                // If filtering by minimum times attended
                if ( attendedMinCount.HasValue )
                {
                    attendees = attendees.Where( p => p.Value.AttendanceSummary.Count() >= attendedMinCount );
                }

                // Force filter application
                allAttendeeVisits = attendees.ToDictionary( k => k.Key, v => v.Value );

                // Add the First Visit information
                foreach ( DataRow row in dtAttendeeFirstDates.Rows )
                {
                    int personId = ( int ) row["PersonId"];
                    if ( allAttendeeVisits.ContainsKey( personId ) )
                    {
                        allAttendeeVisits[personId].FirstVisits.Add( ( DateTime ) row["StartDate"] );
                    }
                }

                // If filtering based on visit time, only include those who visited the selected time during the date range
                if ( byNthVisit.HasValue )
                {
                    int skipCount = byNthVisit.Value - 1;
                    allAttendeeVisits = allAttendeeVisits
                        .Where( p => p.Value.FirstVisits.Skip( skipCount ).Take( 1 ).Any( d => d >= start && d < end ) )
                        .ToDictionary( k => k.Key, v => v.Value );
                }

                // Add the Last Attended information
                if ( dtAttendeeLastAttendance != null )
                {
                    foreach ( DataRow row in dtAttendeeLastAttendance.Rows )
                    {
                        int personId = ( int ) row["PersonId"];
                        if ( allAttendeeVisits.ContainsKey( personId ) )
                        {
                            var result = allAttendeeVisits[personId];
                            if ( result.LastVisit == null )
                            {
                                var lastAttendance = new PersonLastAttendance();
                                lastAttendance.CampusId = row["CampusId"] as int?;
                                lastAttendance.CampusName = row["CampusName"].ToString();
                                lastAttendance.GroupId = row["GroupId"] as int?;
                                lastAttendance.GroupName = row["GroupName"].ToString();
                                lastAttendance.RoleName = row["RoleName"].ToString();
                                lastAttendance.InGroup = !string.IsNullOrWhiteSpace( lastAttendance.RoleName );
                                lastAttendance.ScheduleId = row["ScheduleId"] as int?;
                                lastAttendance.StartDateTime = ( DateTime ) row["StartDateTime"];
                                lastAttendance.LocationId = row["LocationId"] as int?;
                                lastAttendance.LocationName = row["LocationName"].ToString();
                                result.LastVisit = lastAttendance;
                            }
                        }
                    }
                }

                // Add the Demographic information
                if ( dtAttendees != null )
                {
                    var newResults = new Dictionary<int, AttendeeResult>();

                    foreach ( DataRow row in dtAttendees.Rows )
                    {
                        int personId = ( int ) row["Id"];
                        if ( allAttendeeVisits.ContainsKey( personId ) )
                        {
                            var result = new AttendeeResult( allAttendeeVisits[personId] );

                            var person = new PersonInfo();
                            person.NickName = row["NickName"].ToString();
                            person.LastName = row["LastName"].ToString();
                            person.Gender = row["Gender"].ToString().ConvertToEnum<Gender>();
                            person.Email = row["Email"].ToString();
                            person.GivingId = row["GivingId"].ToString();
                            person.Birthdate = row["BirthDate"] as DateTime?;
                            person.DeceasedDate = row["DeceasedDate"] as DateTime?;
                            person.Gender = row["Gender"].ToString().ConvertToEnum<Gender>();
                            person.Age = Person.GetAge( person.Birthdate, person.DeceasedDate );
                            person.ConnectionStatusValueId = row["ConnectionStatusValueId"] as int?;
                            person.Grade = dtAttendees.Columns.Contains( "GraduationYear" ) ? Person.GradeFormattedFromGraduationYear( row["GraduationYear"] as int? ) : null;
                            result.Person = person;

                            if ( includeParents )
                            {
                                result.ParentId = ( int ) row["ParentId"];
                                var parent = new PersonInfo();
                                parent.NickName = row["ParentNickName"].ToString();
                                parent.LastName = row["ParentLastName"].ToString();
                                parent.Email = row["ParentEmail"].ToString();
                                parent.GivingId = row["ParentGivingId"].ToString();
                                parent.Birthdate = row["ParentBirthDate"] as DateTime?;
                                person.DeceasedDate = row["ParentDeceasedDate"] as DateTime?;
                                parent.Gender = row["ParentGender"].ToString().ConvertToEnum<Gender>();
                                parent.Age = Person.GetAge( parent.Birthdate, parent.DeceasedDate );
                                result.Parent = parent;
                            }

                            if ( includeChildren )
                            {
                                var child = new PersonInfo();
                                result.ChildId = ( int ) row["ChildId"];
                                child.NickName = row["ChildNickName"].ToString();
                                child.LastName = row["ChildLastName"].ToString();
                                child.Email = row["ChildEmail"].ToString();
                                child.GivingId = row["ChildGivingId"].ToString();
                                child.Birthdate = row["ChildBirthDate"] as DateTime?;
                                child.DeceasedDate = row["ChildDeceasedDate"] as DateTime?;
                                child.Gender = row["ChildGender"].ToString().ConvertToEnum<Gender>();
                                child.Grade = Person.GradeFormattedFromGraduationYear( row["ChildGraduationYear"] as int? );
                                child.Age = Person.GetAge( child.Birthdate, child.DeceasedDate );
                                result.Child = child;
                            }

                            allResults.Add( result );
                        }
                    }
                }
            }
            else
            {
                // If dataview filter was included remove anyone not in that dataview
                if ( dataViewPersonIds != null )
                {
                    allResults = allResults
                        .Where( p => dataViewPersonIds.Contains( p.PersonId ) )
                        .ToList();
                }

                // Add the first visit dates for people
                foreach ( DataRow row in dtAttendeeFirstDates.Rows )
                {
                    int personId = ( int ) row["PersonId"];
                    foreach ( var result in allResults.Where( r => r.PersonId == personId ) )
                    {
                        result.FirstVisits.Add( ( DateTime ) row["StartDate"] );
                    }
                }

                // Add the Last Attended information
                if ( dtAttendeeLastAttendance != null )
                {
                    foreach ( DataRow row in dtAttendeeLastAttendance.Rows )
                    {
                        int personId = ( int ) row["PersonId"];
                        foreach ( var result in allResults.Where( r => r.PersonId == personId ) )
                        {
                            if ( result.LastVisit == null )
                            {
                                var lastAttendance = new PersonLastAttendance();
                                lastAttendance.CampusId = row["CampusId"] as int?;
                                lastAttendance.CampusName = row["CampusName"].ToString();
                                lastAttendance.GroupId = row["GroupId"] as int?;
                                lastAttendance.GroupName = row["GroupName"].ToString();
                                lastAttendance.RoleName = row["RoleName"].ToString();
                                lastAttendance.InGroup = !string.IsNullOrWhiteSpace( lastAttendance.RoleName );
                                lastAttendance.ScheduleId = row["ScheduleId"] as int?;
                                lastAttendance.StartDateTime = ( DateTime ) row["StartDateTime"];
                                lastAttendance.LocationId = row["LocationId"] as int?;
                                lastAttendance.LocationName = row["LocationName"].ToString();
                                result.LastVisit = lastAttendance;
                            }
                        }
                    }
                }
            }

            // Begin formatting the columns
            var qryResult = allResults.AsQueryable();

            var personUrlFormatString = ( ( RockPage ) this.Page ).ResolveRockUrl( "~/Person/{0}" );

            var personGradeField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Grade" );
            if ( personGradeField != null )
            {
                personGradeField.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.NeverInclude : ExcelExportBehavior.AlwaysInclude;
            }

            var personHyperLinkField = gAttendeesAttendance.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Name" );
            if ( personHyperLinkField != null )
            {
                personHyperLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }

            var parentHyperLinkField = gAttendeesAttendance.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Parent" );
            if ( parentHyperLinkField != null )
            {
                parentHyperLinkField.Visible = includeParents;
                parentHyperLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }

            var parentField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Parent" );
            if ( parentField != null )
            {
                parentField.ExcelExportBehavior = includeParents ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var parentGenderField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Parent Gender" );
            if ( parentGenderField != null )
            {
                parentGenderField.ExcelExportBehavior = includeParents ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var parentEmailField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Parent Email" );
            if ( parentEmailField != null )
            {
                parentEmailField.ExcelExportBehavior = includeParents ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var parentGivingId = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Parent GivingId" );
            if ( parentGivingId != null )
            {
                parentGivingId.ExcelExportBehavior = includeParents ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childHyperLinkField = gAttendeesAttendance.Columns.OfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Child" );
            if ( childHyperLinkField != null )
            {
                childHyperLinkField.Visible = includeChildren;
                childHyperLinkField.DataNavigateUrlFormatString = personUrlFormatString;
            }

            var childfield = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child" );
            if ( childfield != null )
            {
                childfield.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childGenderlField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child Gender" );
            if ( childGenderlField != null )
            {
                childGenderlField.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childGradeField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child Grade" );
            if ( childGradeField != null )
            {
                childGradeField.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childEmailField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child Email" );
            if ( childEmailField != null )
            {
                childEmailField.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childAgeField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child Age" );
            if ( childAgeField != null )
            {
                childAgeField.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            var childGivingId = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Child GivingId" );
            if ( childGivingId != null )
            {
                childGivingId.ExcelExportBehavior = includeChildren ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
            }

            SortProperty sortProperty = gAttendeesAttendance.SortProperty;

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "AttendanceSummary.Count" )
                {
                    if ( sortProperty.Direction == SortDirection.Descending )
                    {
                        qryResult = qryResult.OrderByDescending( a => a.AttendanceSummary.Count() );
                    }
                    else
                    {
                        qryResult = qryResult.OrderBy( a => a.AttendanceSummary.Count() );
                    }
                }
                else if ( sortProperty.Property == "FirstVisit.StartDateTime" )
                {
                    if ( sortProperty.Direction == SortDirection.Descending )
                    {
                        qryResult = qryResult.OrderByDescending( a => a.FirstVisits.Min() );
                    }
                    else
                    {
                        qryResult = qryResult.OrderBy( a => a.FirstVisits.Min() );
                    }
                }
                else
                {
                    qryResult = qryResult.Sort( sortProperty );
                }
            }
            else
            {
                qryResult = qryResult.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.NickName );
            }

            var attendancePercentField = gAttendeesAttendance.Columns.OfType<RockTemplateField>().First( a => a.HeaderText.EndsWith( "Attendance %" ) );
            attendancePercentField.HeaderText = string.Format( "{0}ly Attendance %", groupBy.ConvertToString() );

            // Calculate all the possible attendance summary dates
            UpdatePossibleAttendances( dateRange, groupBy );

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

            // pre-load the schedule names since FriendlyScheduleText requires building the ICal object, etc
            _scheduleNameLookup = new ScheduleService( rockContextAnalytics ).Queryable()
                .ToList()
                .ToDictionary( k => k.Id, v => v.FriendlyScheduleText );

            if ( includeParents )
            {
                gAttendeesAttendance.PersonIdField = "ParentId";
                gAttendeesAttendance.DataKeyNames = new string[] { "ParentId", "PersonId" };
            }
            else if ( includeChildren )
            {
                gAttendeesAttendance.PersonIdField = "ChildId";
                gAttendeesAttendance.DataKeyNames = new string[] { "ChildId", "PersonId" };
            }
            else
            {
                gAttendeesAttendance.PersonIdField = "PersonId";
                gAttendeesAttendance.DataKeyNames = new string[] { "PersonId" };
            }

            // Create the dynamic attendance grid columns as needed
            CreateDynamicAttendanceGridColumns();

            try
            {
                nbAttendeesError.Visible = false;

                gAttendeesAttendance.SetLinqDataSource( qryResult );
                var currentPageItems = gAttendeesAttendance.DataSource as List<AttendeeResult>;
                if ( currentPageItems != null )
                {
                    var currentPagePersonIds = new List<int>();
                    if ( includeParents )
                    {
                        currentPagePersonIds = currentPageItems.Select( i => i.ParentId ).ToList();
                        gAttendeesAttendance.PersonIdField = "ParentId";
                        gAttendeesAttendance.DataKeyNames = new string[] { "ParentId", "PersonId" };
                    }
                    else if ( includeChildren )
                    {
                        currentPagePersonIds = currentPageItems.Select( i => i.ChildId ).ToList();
                        gAttendeesAttendance.PersonIdField = "ChildId";
                        gAttendeesAttendance.DataKeyNames = new string[] { "ChildId", "PersonId" };
                    }
                    else
                    {
                        currentPagePersonIds = currentPageItems.Select( i => i.PersonId ).ToList();
                        gAttendeesAttendance.PersonIdField = "PersonId";
                        gAttendeesAttendance.DataKeyNames = new string[] { "PersonId" };
                    }

                    LoadCurrentPageObjects( currentPagePersonIds );
                }

                _currentlyExporting = isExporting;
                gAttendeesAttendance.DataBind();
                _currentlyExporting = false;
            }
            catch ( Exception exception )
            {
                LogAndShowException( exception );
            }
        }

        /// <summary>
        /// Loads the current page objects.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        private void LoadCurrentPageObjects( List<int> personIds )
        {
            // Load the addresses
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

            if ( familyGroupType != null && homeAddressGuid.HasValue && personIds != null )
            {
                var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                if ( homeAddressDv != null )
                {
                    _personLocations = new Dictionary<int, Location>();
                    foreach ( var item in new GroupMemberService( rockContextAnalytics )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            personIds.Contains( m.PersonId ) &&
                            m.Group.GroupTypeId == familyGroupType.Id )
                        .Select( m => new
                        {
                            m.PersonId,
                            Location = m.Group.GroupLocations
                                .Where( l =>
                                    l.GroupLocationTypeValueId == homeAddressDv.Id &&
                                    l.IsMappedLocation )
                                .Select( l => l.Location )
                                .FirstOrDefault()
                        } )
                        .Where( l => l.Location != null ) )
                    {
                        _personLocations.TryAdd( item.PersonId, item.Location );
                    }
                }
            }

            // Load the phone numbers
            _personPhoneNumbers = new PhoneNumberService( rockContextAnalytics )
                .Queryable().AsNoTracking()
                .Where( n => personIds.Contains( n.PersonId ) )
                .GroupBy( n => n.PersonId )
                .Select( n => new
                {
                    PersonId = n.Key,
                    PhoneNumbers = n.ToList()
                } )
                .ToDictionary( k => k.PersonId, v => v.PhoneNumbers );
        }

        /// <summary>
        /// Logs and shows the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void LogAndShowException( Exception exception )
        {
            LogException( exception );
            Exception sqlException = ReportingHelper.FindSqlTimeoutException( exception );
            if ( sqlException != null )
            {
                nbAttendeesError.Text = "The attendee report did not complete in a timely manner. Try again using a smaller date range and fewer campuses and groups.";
                nbAttendeesError.Visible = true;
                return;
            }

            nbAttendeesError.Text = "An error occurred";
            nbAttendeesError.Details = exception.Message;
            nbAttendeesError.Visible = true;
        }

        /// <summary>
        /// Creates the dynamic attendance grid columns.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        private void CreateDynamicAttendanceGridColumns()
        {
            ChartGroupBy groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;

            // Ensure the columns for the Attendance Checkmarks are there
            var attendanceSummaryFields = gAttendeesAttendance.Columns.OfType<BoolFromArrayField<DateTime>>().Where( a => a.DataField == "AttendanceSummary" ).ToList();
            var existingSummaryDates = attendanceSummaryFields.Select( a => a.ArrayKey ).ToList();

            if ( existingSummaryDates.Any( a => !_possibleAttendances.Contains( a ) ) || _possibleAttendances.Any( a => !existingSummaryDates.Contains( a ) ) )
            {
                foreach ( var oldField in attendanceSummaryFields.Reverse<BoolFromArrayField<DateTime>>() )
                {
                    // remove all these fields if they have changed
                    gAttendeesAttendance.Columns.Remove( oldField );
                }

                // limit to 520 checkmark columns so that we don't blow up the server (just in case they select every week for the last 100 years or something).
                var maxColumns = 520;
                foreach ( var summaryDate in _possibleAttendances.Take( maxColumns ) )
                {
                    var boolFromArrayField = new BoolFromArrayField<DateTime>();

                    boolFromArrayField.ArrayKey = summaryDate;
                    boolFromArrayField.DataField = "AttendanceSummary";
                    switch ( groupBy )
                    {
                        case ChartGroupBy.Year:
                            boolFromArrayField.HeaderText = summaryDate.ToString( "yyyy" );
                            break;

                        case ChartGroupBy.Month:
                            boolFromArrayField.HeaderText = summaryDate.ToString( "MMM yyyy" );
                            break;

                        case ChartGroupBy.Week:
                            boolFromArrayField.HeaderText = summaryDate.ToShortDateString();
                            break;

                        default:
                            // shouldn't happen
                            boolFromArrayField.HeaderText = summaryDate.ToString();
                            break;
                    }

                    gAttendeesAttendance.Columns.Add( boolFromArrayField );
                }
            }
        }

        /// <summary>
        /// Updates the possible attendance summary dates
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="attendanceGroupBy">The attendance group by.</param>
        public void UpdatePossibleAttendances( DateRange dateRange, ChartGroupBy attendanceGroupBy )
        {
            _possibleAttendances = GetPossibleAttendancesForDateRange( dateRange, attendanceGroupBy );
        }

        /// <summary>
        /// Gets the possible attendances for the date range.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="attendanceGroupBy">The attendance group by type.</param>
        /// <returns></returns>
        public List<DateTime> GetPossibleAttendancesForDateRange( DateRange dateRange, ChartGroupBy attendanceGroupBy )
        {
            var result = new List<DateTime>();

            // Attendance is grouped by Sunday dates between the start/end dates.
            // The possible dates (columns) should be calculated the same way.
            var startSunday = dateRange.Start.Value.SundayDate();
            var endDate = dateRange.End.Value;
            var endSunday = endDate.SundayDate();
            if ( endSunday > endDate )
            {
                endSunday = endSunday.AddDays( -7 );
            }

            if ( attendanceGroupBy == ChartGroupBy.Week )
            {
                var weekEndDate = startSunday;
                while ( weekEndDate <= endSunday )
                {
                    // Weeks are summarized as the last day of the "Rock" week (Sunday)
                    result.Add( weekEndDate );
                    weekEndDate = weekEndDate.AddDays( 7 );
                }
            }
            else if ( attendanceGroupBy == ChartGroupBy.Month )
            {
                var endOfFirstMonth = startSunday.AddDays( -( startSunday.Day - 1 ) ).AddMonths( 1 ).AddDays( -1 );
                var endOfLastMonth = endSunday.AddDays( -( endSunday.Day - 1 ) ).AddMonths( 1 ).AddDays( -1 );

                //// Months are summarized as the First Day of the month: For example, 5/1/2015 would include everything from 5/1/2015 - 5/31/2015 (inclusive)
                var monthStartDate = new DateTime( endOfFirstMonth.Year, endOfFirstMonth.Month, 1 );
                while ( monthStartDate <= endOfLastMonth )
                {
                    result.Add( monthStartDate );
                    monthStartDate = monthStartDate.AddMonths( 1 );
                }
            }
            else if ( attendanceGroupBy == ChartGroupBy.Year )
            {
                var endOfFirstYear = new DateTime( startSunday.Year, 1, 1 ).AddYears( 1 ).AddDays( -1 );
                var endOfLastYear = new DateTime( endSunday.Year, 1, 1 ).AddYears( 1 ).AddDays( -1 );

                //// Years are summarized as the First Day of the year: For example, 1/1/2015 would include everything from 1/1/2015 - 12/31/2015 (inclusive)
                var yearStartDate = new DateTime( endOfFirstYear.Year, 1, 1 );
                while ( yearStartDate <= endOfLastYear )
                {
                    result.Add( yearStartDate );
                    yearStartDate = yearStartDate.AddYears( 1 );
                }
            }

            // only include current and previous dates
            var currentDateTime = RockDateTime.Now;
            result = result.Where( a => a <= currentDateTime.Date ).ToList();

            return result;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendeesAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendeesAttendance_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var personDates = e.Row.DataItem as AttendeeResult;
            if ( personDates != null )
            {
                Literal lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                if ( lFirstVisitDate == null )
                {
                    // Since we have dynamic columns, the templatefields might not get created due some viewstate thingy
                    // so, if we lost the templatefield, force them to instantiate
                    var templateFields = gAttendeesAttendance.Columns.OfType<TemplateField>();
                    foreach ( var templateField in templateFields )
                    {
                        var cellIndex = gAttendeesAttendance.GetColumnIndex( templateField );
                        var cell = e.Row.Cells[cellIndex] as DataControlFieldCell;
                        templateField.InitializeCell( cell, DataControlCellType.DataCell, e.Row.RowState, e.Row.RowIndex );
                    }

                    lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                }

                var lSecondVisitDate = e.Row.FindControl( "lSecondVisitDate" ) as Literal;
                var lServiceTime = e.Row.FindControl( "lServiceTime" ) as Literal;
                var lHomeAddress = e.Row.FindControl( "lHomeAddress" ) as Literal;
                var lHomeAddressStreet = e.Row.FindControl( "lHomeAddressStreet" ) as Literal;
                var lHomeAddressCity = e.Row.FindControl( "lHomeAddressCity" ) as Literal;
                var lHomeAddressState = e.Row.FindControl( "lHomeAddressState" ) as Literal;
                var lHomeAddressPostalCode = e.Row.FindControl( "lHomeAddressPostalCode" ) as Literal;
                var lHomeAddressCountry = e.Row.FindControl( "lHomeAddressCountry" ) as Literal;

                var lPhoneNumbers = e.Row.FindControl( "lPhoneNumbers" ) as Literal;
                var lAttendanceCount = e.Row.FindControl( "lAttendanceCount" ) as Literal;
                var lAttendancePercent = e.Row.FindControl( "lAttendancePercent" ) as Literal;

                if ( personDates.FirstVisits != null )
                {
                    if ( personDates.FirstVisits.Count() >= 1 )
                    {
                        var firstVisit = personDates.FirstVisits.Min();

                        if ( firstVisit != null )
                        {
                            DateTime? firstVisitDateTime = firstVisit;
                            if ( firstVisitDateTime.HasValue )
                            {
                                lFirstVisitDate.Text = firstVisitDateTime.Value.ToShortDateString();
                            }
                        }

                        if ( personDates.FirstVisits.Count() >= 2 )
                        {
                            var secondVisit = personDates.FirstVisits.Skip( 1 ).FirstOrDefault();
                            if ( secondVisit != null )
                            {
                                DateTime? secondVisitDateTime = secondVisit;
                                if ( secondVisitDateTime.HasValue )
                                {
                                    lSecondVisitDate.Text = secondVisitDateTime.Value.ToShortDateString();
                                }
                            }
                        }
                    }
                }

                if ( personDates.LastVisit != null )
                {
                    if ( personDates.LastVisit.ScheduleId.HasValue )
                    {
                        if ( _scheduleNameLookup.ContainsKey( personDates.LastVisit.ScheduleId.Value ) )
                        {
                            lServiceTime.Text = _scheduleNameLookup[personDates.LastVisit.ScheduleId.Value];
                        }
                    }
                }

                int currentPersonId = personDates.PersonId;
                if ( gAttendeesAttendance.PersonIdField == "ParentId" )
                {
                    currentPersonId = personDates.ParentId;
                }
                else if ( gAttendeesAttendance.PersonIdField == "ChildId" )
                {
                    currentPersonId = personDates.ChildId;
                }

                try
                {
                    if ( _personLocations != null && _personLocations.ContainsKey( currentPersonId ) )
                    {
                        lHomeAddress.Text = _personLocations[currentPersonId].FormattedHtmlAddress;
                        lHomeAddressStreet.Text = _personLocations[currentPersonId].Street1.Trim();
                        if ( !string.IsNullOrEmpty( _personLocations[currentPersonId].Street2 ) )
                        {
                            lHomeAddressStreet.Text += " " + _personLocations[currentPersonId].Street2;
                        }
                        lHomeAddressCity.Text = _personLocations[currentPersonId].City;
                        lHomeAddressState.Text = _personLocations[currentPersonId].State;
                        lHomeAddressPostalCode.Text = _personLocations[currentPersonId].PostalCode;
                        lHomeAddressCountry.Text = _personLocations[currentPersonId].Country;
                    }
                }
                catch ( Exception ex )
                {
                    string msg = ex.Message;
                }

                bool isExporting = _currentlyExporting;
                if ( !isExporting && e is RockGridViewRowEventArgs )
                {
                    isExporting = ( ( RockGridViewRowEventArgs ) e ).IsExporting;
                }

                if ( _personPhoneNumbers != null && _personPhoneNumbers.ContainsKey( currentPersonId ) )
                {
                    var sb = new StringBuilder();
                    if ( !isExporting )
                    {
                        sb.Append( "<ul class='list-unstyled phonenumbers'>" );
                    }
                    foreach ( var phoneNumber in _personPhoneNumbers[currentPersonId] )
                    {
                        string formattedNumber = "Unlisted";
                        if ( !phoneNumber.IsUnlisted )
                        {
                            formattedNumber = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number, false );
                        }

                        if ( phoneNumber.NumberTypeValueId.HasValue )
                        {
                            var phoneType = DefinedValueCache.Get( phoneNumber.NumberTypeValueId.Value );
                            if ( phoneType != null )
                            {
                                formattedNumber = isExporting ?
                                    string.Format( "{1}: {0}", formattedNumber, phoneType.Value ) :
                                    string.Format( "{0} <small>{1}</small>", formattedNumber, phoneType.Value );
                            }
                        }
                        if ( isExporting )
                        {
                            sb.AppendFormat( "{0}{1}", sb.Length > 0 ? Environment.NewLine : string.Empty, formattedNumber );
                        }
                        else
                        {
                            sb.AppendFormat( "<li>{0}</li>", formattedNumber );
                        }
                    }
                    if ( !isExporting )
                    {
                        sb.Append( "</ul>" );
                    }
                    lPhoneNumbers.Text = sb.ToString();
                }

                int attendanceSummaryCount = personDates.AttendanceSummary.Count();
                lAttendanceCount.Text = attendanceSummaryCount.ToString();

                int? attendencePossibleCount = _possibleAttendances != null ? _possibleAttendances.Count() : ( int? ) null;

                if ( attendencePossibleCount.HasValue && attendencePossibleCount > 0 )
                {
                    var attendancePerPossibleCount = ( decimal ) attendanceSummaryCount / attendencePossibleCount.Value;
                    if ( attendancePerPossibleCount > 1 )
                    {
                        attendancePerPossibleCount = 1;
                    }

                    lAttendancePercent.Text = string.Format( "{0:P}", attendancePerPossibleCount );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupType = e.Item.DataItem as GroupType;

                var liGroupTypeItem = new HtmlGenericContainer( "li", "rocktree-item rocktree-folder" );
                liGroupTypeItem.ID = "liGroupTypeItem" + groupType.Id;
                e.Item.Controls.Add( liGroupTypeItem );

                AddGroupTypeControls( groupType, liGroupTypeItem );
            }
        }

        // list of grouptype ids that have already been rendered (in case a group type has multiple parents )
        private List<int> _addedGroupTypeIds;

        private List<int> _addedGroupIds;

        /// <summary>
        /// Adds the group type controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="pnlGroupTypes">The PNL group types.</param>
        private void AddGroupTypeControls( GroupType groupType, HtmlGenericContainer liGroupTypeItem )
        {
            if ( !_addedGroupTypeIds.Contains( groupType.Id ) )
            {
                _addedGroupTypeIds.Add( groupType.Id );

                bool showInactive = GetBlockPersonPreferences().GetValue( "show-inactive" ).AsBoolean();

                if ( ( groupType.Groups.Any() && showInactive ) || groupType.Groups.Where( g => g.IsActive ).Any() )
                {
                    bool showGroupAncestry = GetAttributeValue( AttributeKeys.ShowGroupAncestry ).AsBoolean( true );

                    var rockContextAnalytics = new RockContextAnalytics();
                    rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    var groupService = new GroupService( rockContextAnalytics );

                    var cblGroupTypeGroups = new RockCheckBoxList { ID = "cblGroupTypeGroups" + groupType.Id, FormGroupCssClass = "js-groups-container" };

                    string repeatDirection = GetAttributeValue( AttributeKeys.FilterColumnDirection );
                    int repeatColumns = GetAttributeValue( AttributeKeys.FilterColumnCount ).AsIntegerOrNull() ?? 0;

                    cblGroupTypeGroups.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
                    cblGroupTypeGroups.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;
                    cblGroupTypeGroups.Label = groupType.Name;
                    cblGroupTypeGroups.Items.Clear();

                    // Select only those Groups that don't have a Parent, or the ParentGroup is a different group type so we don't end up with an infinite recursion.
                    var eligibleGroups = groupType.Groups
                        .Where( g => ( g.ParentGroup == null ) || ( g.ParentGroup.GroupTypeId != groupType.Id ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();

                    foreach ( var group in eligibleGroups )
                    {
                        if ( group.IsActive || showInactive )
                        {
                            AddGroupControls( group, cblGroupTypeGroups, groupService, showGroupAncestry );
                        }
                    }

                    liGroupTypeItem.Controls.Add( cblGroupTypeGroups );
                }
                else
                {
                    if ( groupType.ChildGroupTypes.Any() )
                    {
                        liGroupTypeItem.Controls.Add( new Label { Text = groupType.Name, ID = "lbl" + groupType.Name } );
                    }
                }

                if ( groupType.ChildGroupTypes.Any() )
                {
                    var ulGroupTypeList = new HtmlGenericContainer( "ul", "list-unstyled" );

                    liGroupTypeItem.Controls.Add( ulGroupTypeList );
                    foreach ( var childGroupType in groupType.ChildGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var liChildGroupTypeItem = new HtmlGenericContainer( "li" );
                        liChildGroupTypeItem.ID = "liGroupTypeItem" + childGroupType.Id;
                        ulGroupTypeList.Controls.Add( liChildGroupTypeItem );
                        AddGroupTypeControls( childGroupType, liChildGroupTypeItem );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the group controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="service">The service.</param>
        /// <param name="showGroupAncestry">if set to <c>true</c> [show group ancestry].</param>
        private void AddGroupControls( Group group, RockCheckBoxList checkBoxList, GroupService service, bool showGroupAncestry )
        {
            // Only show groups that actually have a schedule, unless they choose IncludeGroupsWithoutSchedule
            if ( group != null )
            {
                if ( !_addedGroupIds.Contains( group.Id ) )
                {
                    _addedGroupIds.Add( group.Id );
                    if ( cbIncludeGroupsWithoutSchedule.Checked || group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
                    {
                        string displayName = showGroupAncestry ? service.GroupAncestorPathName( group.Id ) : group.Name;
                        checkBoxList.Items.Add( new ListItem( displayName, group.Id.ToString() ) );
                    }

                    bool showInactive = GetBlockPersonPreferences().GetValue( "show-inactive" ).AsBoolean();

                    if ( group.Groups != null )
                    {
                        foreach ( var childGroup in group.Groups
                            .Where( g => g.IsActive || showInactive )
                            .Where( g => !g.IsArchived )
                            .OrderBy( g => g.Order )
                            .ThenBy( g => g.Name )
                            .ToList() )
                        {
                            AddGroupControls( childGroup, checkBoxList, service, showGroupAncestry );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApply_Click( object sender, EventArgs e )
        {
            LoadChartAndGrids();
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        private enum ShowBy
        {
            /// <summary>
            /// The chart
            /// </summary>
            Chart = 0,

            /// <summary>
            /// The attendees
            /// </summary>
            Attendees = 1
        }

        /// <summary>
        ///
        /// </summary>
        private enum ViewBy
        {
            /// <summary>
            /// The attendee
            /// </summary>
            Attendees = 0,

            /// <summary>
            /// The parent of the attendee
            /// </summary>
            ParentsOfAttendees = 1,

            /// <summary>
            /// The children of the attendee
            /// </summary>
            ChildrenOfAttendees = 2
        }

        /// <summary>
        ///
        /// </summary>
        private enum AttendeesFilterBy
        {
            /// <summary>
            /// All Attendees
            /// </summary>
            All = 0,

            /// <summary>
            /// By nth visit
            /// </summary>
            ByVisit = 1,

            /// <summary>
            /// By pattern
            /// </summary>
            Pattern = 2
        }

        /// <summary>
        /// Displays the results by chart or attendees.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        private void DisplayShowBy( ShowBy showBy )
        {
            hfShowBy.Value = showBy.ConvertToInt().ToString();
            pnlShowByChart.Visible = showBy == ShowBy.Chart;
            pnlShowByAttendees.Visible = showBy == ShowBy.Attendees;
        }

        /// <summary>
        /// Handles the Click event of the btnShowByAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowByAttendees_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Attendees );
            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowByChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowByChart_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Chart );
            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BuildGroupTypesUI( true );
        }

        /// <summary>
        /// Handles the Click events of the GraphBy buttons.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGraphBy_Click( object sender, EventArgs e )
        {
            btnApply_Click( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the GroupBy buttons
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGroupBy_Click( object sender, EventArgs e )
        {
            btnApply_Click( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the btnCheckinDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCheckinDetails_Click( object sender, EventArgs e )
        {
            var groupTypes = GetSelectedGroupTypes();

            if ( groupTypes != null && groupTypes.Any() )
            {
                var groupTypeIds = groupTypes.Select( t => t.Id ).ToList().AsDelimited( "," );
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add( "GroupTypeIds", groupTypeIds );

                NavigateToLinkedPage( "Check-inDetailPage", queryParams );
            }
        }

        /// <summary>
        /// Attendee object that adds PersonInfo to the AttendeeVisit
        /// </summary>
        /// <seealso cref="RockWeb.Blocks.CheckIn.AttendanceAnalytics.AttendeeVisits" />
        public class AttendeeResult : AttendeeVisits
        {
            public PersonInfo Person { get; set; }

            public int ParentId { get; set; }

            public PersonInfo Parent { get; set; }

            public int ChildId { get; set; }

            public PersonInfo Child { get; set; }

            public AttendeeResult()
                : base()
            {
            }

            public AttendeeResult( AttendeeVisits attendeeDates )
            {
                this.PersonId = attendeeDates.PersonId;
                this.FirstVisits = attendeeDates.FirstVisits;
                this.LastVisit = attendeeDates.LastVisit;
                this.AttendanceSummary = attendeeDates.AttendanceSummary;
            }
        }

        /// <summary>
        /// List of person visits
        /// </summary>
        public class AttendeeVisits
        {
            public int PersonId { get; set; }

            public List<DateTime> FirstVisits { get; set; }

            public PersonLastAttendance LastVisit { get; set; }

            public List<DateTime> AttendanceSummary { get; set; }

            public AttendeeVisits()
            {
                FirstVisits = new List<DateTime>();
                AttendanceSummary = new List<DateTime>();
            }
        }

        /// <summary>
        /// Lightweight Rock Person object
        /// </summary>
        public class PersonInfo
        {
            public string NickName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public Gender Gender { get; set; }

            public int? Age { get; set; }

            public string GivingId { get; set; }

            public DateTime? Birthdate { get; set; }

            public DateTime? DeceasedDate { get; set; }

            public string Grade { get; set; }

            public int? ConnectionStatusValueId { get; set; }

            public override string ToString()
            {
                return NickName + " " + LastName;
            }
        }

        /// <summary>
        /// All visit information from the most recent attendance
        /// </summary>
        public class PersonLastAttendance
        {
            public int? CampusId { get; set; }

            public string CampusName { get; set; }

            public int? GroupId { get; set; }

            public string GroupName { get; set; }

            public bool InGroup { get; set; }

            public string RoleName { get; set; }

            public int? ScheduleId { get; set; }

            public DateTime StartDateTime { get; set; }

            public int? LocationId { get; set; }

            public string LocationName { get; set; }
        }

        public class TaskInfo
        {
            public string name { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public TimeSpan duration
            {
                get
                {
                    return end.Subtract( start );
                }
            }

            public override string ToString()
            {
                return string.Format( "{0}: {1:c}", name, duration );
            }

            public string TimeToRun()
            {
                double timeValue = duration.TotalMilliseconds;
                var timeUnit = "ms";
                if ( timeValue > 1000 )
                {
                    timeValue /= 1000;
                    timeUnit = "s";
                }

                if ( timeValue > 60 && timeUnit == "s" )
                {
                    timeValue /= 60;
                    timeUnit = "m";
                }

                var isValueAWholeNumber = Math.Abs( timeValue % 1 ) < 0.01;
                if ( isValueAWholeNumber )
                {
                    return string.Format( "{0:0}{1}", timeValue, timeUnit );
                }
                else
                {
                    return string.Format( "{0:0.0}{1}", timeValue, timeUnit );
                }
            }
        }


        protected void cbShowInactive_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "show-inactive", cbShowInactive.Checked.ToString() );
            preferences.Save();

            BuildGroupTypesUI( true );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbIncludeGroupsWithoutSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIncludeGroupsWithoutSchedule_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "IncludeGroupsWithoutSchedule", cbIncludeGroupsWithoutSchedule.Checked.ToTrueFalse() );
            preferences.Save();

            // NOTE: OnLoad already rebuilt the GroupTypes UI with the changed value of cbIncludeGroupsWithoutSchedule , so we just need to set it as a preference
        }

        protected void gpGroups_SelectItem( object sender, EventArgs e )
        {
            BindSelectedGroups();
        }
    }
}