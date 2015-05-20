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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// Shows a graph of attendance statistics which can be configured for specific groups, date range, etc.
    /// </summary>
    [DisplayName( "Attendance Analysis" )]
    [Category( "Check-in" )]
    [Description( "Shows a graph of attendance statistics which can be configured for specific groups, date range, etc." )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked" )]
    [BooleanField( "Show Group Ancestry", "By default the group ancestry path is shown.  Unselect this to show only the group name.", true )]
    [GroupTypeField( "Check-in Type", required: false, key: "GroupTypeTemplate", groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    public partial class AttendanceReporting : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;

        private List<DateTime> _possibleAttendances = null;
        private Dictionary<int, string> _scheduleNameLookup = null;

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

            gChartAttendance.GridRebind += gChartAttendance_GridRebind;
            gAttendeesAttendance.GridRebind += gAttendeesAttendance_GridRebind;

            gAttendeesAttendance.EntityTypeId = EntityTypeCache.Read<Rock.Model.Person>().Id;

            _rockContext = new RockContext();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendeesAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttendeesAttendance_GridRebind( object sender, EventArgs e )
        {
            BindAttendeesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gChartAttendance_GridRebind( object sender, EventArgs e )
        {
            BindChartAttendanceGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // GroupTypesUI dynamically creates controls, so we need to rebuild it on every OnLoad()
            BuildGroupTypesUI();

            var chartStyleDefinedValueGuid = this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();

            lcAttendance.Options.SetChartStyle( chartStyleDefinedValueGuid );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                LoadSettingsFromUserPreferences();
                LoadChartAndGrids();
            }
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
                return ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
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
            BuildGroupTypesUI();
            LoadChartAndGrids();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            cpCampuses.Campuses = CampusCache.All();

            var groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
            if ( !groupTypeTemplateGuid.HasValue )
            {
                // show the CheckinType(GroupTypeTemplate) control if there isn't a block setting for it
                ddlCheckinType.Visible = true;
                var groupTypeService = new GroupTypeService( _rockContext );
                Guid groupTypePurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
                ddlCheckinType.GroupTypes = groupTypeService.Queryable()
                        .Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeGuid )
                        .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            }
            else
            {
                // hide the CheckinType(GroupTypeTemplate) control if there is a block setting for it
                ddlCheckinType.Visible = false;
            }
        }

        /// <summary>
        /// Builds the group types UI
        /// </summary>
        private void BuildGroupTypesUI()
        {
            var groupType = this.GetSelectedTemplateGroupType();

            if ( groupType != null )
            {
                nbGroupTypeWarning.Visible = false;
                var groupTypes = new GroupTypeService( _rockContext ).GetChildGroupTypes( groupType.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Name );
                rptGroupTypes.DataSource = groupTypes.ToList();
                rptGroupTypes.DataBind();
            }
            else
            {
                nbGroupTypeWarning.Text = "Please select a check-in type.";
                nbGroupTypeWarning.Visible = true;
            }
        }

        /// <summary>
        /// Gets the type of the selected template group (Check-In Type)
        /// </summary>
        /// <returns></returns>
        private GroupTypeCache GetSelectedTemplateGroupType()
        {
            var groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
            if ( !groupTypeTemplateGuid.HasValue )
            {
                if ( ddlCheckinType.SelectedGroupTypeId.HasValue )
                {
                    var groupType = GroupTypeCache.Read( ddlCheckinType.SelectedGroupTypeId.Value );
                    if ( groupType != null )
                    {
                        groupTypeTemplateGuid = groupType.Guid;
                    }
                }
            }

            return groupTypeTemplateGuid.HasValue ? GroupTypeCache.Read( groupTypeTemplateGuid.Value ) : null;
        }

        /// <summary>
        /// Loads the chart and any visible grids
        /// </summary>
        public void LoadChartAndGrids()
        {
            lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );

            lcAttendance.ShowTooltip = true;
            if ( this.DetailPageGuid.HasValue )
            {
                lcAttendance.ChartClick += lcAttendance_ChartClick;
            }

            var dataSourceUrl = "~/api/Attendances/GetChartData";
            var dataSourceParams = new Dictionary<string, object>();
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            if ( dateRange.Start.HasValue )
            {
                dataSourceParams.AddOrReplace( "startDate", dateRange.Start.Value.ToString( "o" ) );
            }

            if ( dateRange.End.HasValue )
            {
                dataSourceParams.AddOrReplace( "endDate", dateRange.End.Value.ToString( "o" ) );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<AttendanceGroupBy>() ?? AttendanceGroupBy.Week;
            lcAttendance.TooltipFormatter = null;
            switch ( groupBy )
            {
                case AttendanceGroupBy.Week:
                    {
                        lcAttendance.Options.xaxis.tickSize = new string[] { "7", "day" };
                        lcAttendance.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = 'Weekend of <br />' + itemDate.toLocaleDateString();
    var seriesLabel = item.series.label;
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                    }

                    break;

                case AttendanceGroupBy.Month:
                    {
                        lcAttendance.Options.xaxis.tickSize = new string[] { "1", "month" };
                        lcAttendance.TooltipFormatter = @"
function(item) {
    var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
    var seriesLabel = item.series.label;
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                    }

                    break;

                case AttendanceGroupBy.Year:
                    {
                        lcAttendance.Options.xaxis.tickSize = new string[] { "1", "year" };
                        lcAttendance.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = itemDate.getFullYear();
    var seriesLabel = item.series.label;
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                    }

                    break;
            }

            string groupByTextPlural = groupBy.ConvertToString().ToLower().Pluralize();
            lPatternXFor.Text = string.Format( " {0} for the selected date range", groupByTextPlural );
            lPatternAndMissedXBetween.Text = string.Format( " {0} between", groupByTextPlural );

            dataSourceParams.AddOrReplace( "groupBy", hfGroupBy.Value.AsInteger() );
            dataSourceParams.AddOrReplace( "graphBy", hfGraphBy.Value.AsInteger() );

            if ( cpCampuses.SelectedCampusIds.Any() )
            {
                dataSourceParams.AddOrReplace( "campusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            }

            var selectedGroupIds = GetSelectedGroupIds();

            if ( selectedGroupIds.Any() )
            {
                dataSourceParams.AddOrReplace( "groupIds", selectedGroupIds.AsDelimited( "," ) );
            }
            else
            {
                // set the value to 0 to indicate that no groups where selected (and so that Rest Endpoint doesn't 404)
                dataSourceParams.AddOrReplace( "groupIds", 0 );
            }

            SaveSettingsToUserPreferences();

            dataSourceUrl += "?" + dataSourceParams.Select( s => string.Format( "{0}={1}", s.Key, s.Value ) ).ToList().AsDelimited( "&" );

            // if no Campuses or Groups are selected show a warning since no data will show up
            nbCampusesWarning.Visible = false;
            nbGroupsWarning.Visible = false;

            if ( !selectedGroupIds.Any() )
            {
                nbGroupsWarning.Visible = true;
                return;
            }

            if ( !cpCampuses.SelectedCampusIds.Any() )
            {
                nbCampusesWarning.Visible = true;
                return;
            }

            lcAttendance.DataSourceUrl = this.ResolveUrl( dataSourceUrl );

            if ( pnlChartAttendanceGrid.Visible )
            {
                BindChartAttendanceGrid();
            }

            if ( pnlShowByAttendees.Visible )
            {
                BindAttendeesGrid();
            }
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettingsToUserPreferences()
        {
            string keyPrefix = string.Format( "attendance-reporting-{0}-", this.BlockId );

            this.SetUserPreference( keyPrefix + "TemplateGroupTypeId", ddlCheckinType.SelectedGroupTypeId.ToString() );

            this.SetUserPreference( keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues );
            this.SetUserPreference( keyPrefix + "GroupBy", hfGroupBy.Value );
            this.SetUserPreference( keyPrefix + "GraphBy", hfGraphBy.Value );
            this.SetUserPreference( keyPrefix + "CampusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );

            var selectedGroupIds = GetSelectedGroupIds();

            this.SetUserPreference( keyPrefix + "GroupIds", selectedGroupIds.AsDelimited( "," ) );

            this.SetUserPreference( keyPrefix + "ShowBy", hfShowBy.Value );

            this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value );

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

            this.SetUserPreference( keyPrefix + "AttendeesFilterByType", attendeesFilterBy.ConvertToInt().ToString() );
            this.SetUserPreference( keyPrefix + "AttendeesFilterByVisit", ddlNthVisit.SelectedValue );
            this.SetUserPreference( keyPrefix + "AttendeesFilterByPattern", string.Format( "{0}|{1}|{2}|{3}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, tbPatternMissedXTimes.Text, drpPatternDateRange.DelimitedValues ) );
        }

        /// <summary>
        /// Gets the selected group ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedGroupIds()
        {
            var selectedGroupIds = new List<int>();
            var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
            foreach ( var cblGroup in checkboxListControls )
            {
                selectedGroupIds.AddRange( cblGroup.SelectedValuesAsInt );
            }

            return selectedGroupIds;
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettingsFromUserPreferences()
        {
            string keyPrefix = string.Format( "attendance-reporting-{0}-", this.BlockId );

            ddlCheckinType.SelectedGroupTypeId = this.GetUserPreference( keyPrefix + "TemplateGroupTypeId" ).AsIntegerOrNull();
            BuildGroupTypesUI();

            string slidingDateRangeSettings = this.GetUserPreference( keyPrefix + "SlidingDateRange" );
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

            hfGroupBy.Value = this.GetUserPreference( keyPrefix + "GroupBy" );
            hfGraphBy.Value = this.GetUserPreference( keyPrefix + "GraphBy" );

            var campusIdList = this.GetUserPreference( keyPrefix + "CampusIds" ).Split( ',' ).ToList();
            cpCampuses.SetValues( campusIdList );

            // if no campuses are selected, default to showing all of them
            if ( cpCampuses.SelectedCampusIds.Count == 0 )
            {
                foreach ( ListItem item in cpCampuses.Items )
                {
                    item.Selected = true;
                }
            }

            var groupIdList = this.GetUserPreference( keyPrefix + "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            // if no groups are selected, default to showing all of them
            var selectAll = groupIdList.Count == 0;

            var checkboxListControls = rptGroupTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
            foreach ( var cblGroup in checkboxListControls )
            {
                foreach ( ListItem item in cblGroup.Items )
                {
                    item.Selected = selectAll || groupIdList.Contains( item.Value );
                }
            }

            ShowBy showBy = this.GetUserPreference( keyPrefix + "ShowBy" ).ConvertToEnumOrNull<ShowBy>() ?? ShowBy.Chart;
            DisplayShowBy( showBy );

            ViewBy viewBy = this.GetUserPreference( keyPrefix + "ViewBy" ).ConvertToEnumOrNull<ViewBy>() ?? ViewBy.Attendees;
            hfViewBy.Value = viewBy.ConvertToInt().ToString();

            AttendeesFilterBy attendeesFilterBy = this.GetUserPreference( keyPrefix + "AttendeesFilterByType" ).ConvertToEnumOrNull<AttendeesFilterBy>() ?? AttendeesFilterBy.All;

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

            ddlNthVisit.SelectedValue = this.GetUserPreference( keyPrefix + "AttendeesFilterByVisit" );
            string attendeesFilterByPattern = this.GetUserPreference( keyPrefix + "AttendeesFilterByPattern" );
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
        /// Lcs the attendance_ chart click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void lcAttendance_ChartClick( object sender, ChartClickArgs e )
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
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            string campusIds = cpCampuses.SelectedCampusIds.AsDelimited( "," );

            SortProperty sortProperty = gChartAttendance.SortProperty;

            var chartData = new AttendanceService( _rockContext ).GetChartData(
                hfGroupBy.Value.ConvertToEnumOrNull<AttendanceGroupBy>() ?? AttendanceGroupBy.Week,
                hfGraphBy.Value.ConvertToEnumOrNull<AttendanceGraphBy>() ?? AttendanceGraphBy.Total,
                dateRange.Start,
                dateRange.End,
                groupIds,
                campusIds );

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
        /// Binds the attendees grid.
        /// </summary>
        private void BindAttendeesGrid()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.End == null || dateRange.End > RockDateTime.Now )
            {
                dateRange.End = RockDateTime.Now;
            }

            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            string campusIds = cpCampuses.SelectedCampusIds.AsDelimited( "," );

            var rockContext = new RockContext();
            var qry = new AttendanceService( rockContext ).Queryable();

            qry = qry.Where( a => a.DidAttend.HasValue && a.DidAttend.Value );
            var groupType = this.GetSelectedTemplateGroupType();
            var qryAllVisits = qry;
            if ( groupType != null )
            {
                var childGroupTypeIds = new GroupTypeService( rockContext ).GetChildGroupTypes( groupType.Id ).Select( a => a.Id );
                qryAllVisits = qry.Where( a => childGroupTypeIds.Any( b => b == a.Group.GroupTypeId ) );
            }
            else
            {
                return;
            }

            if ( !string.IsNullOrWhiteSpace( groupIds ) )
            {
                var groupIdList = groupIds.Split( ',' ).AsIntegerList();
                qry = qry.Where( a => a.GroupId.HasValue && groupIdList.Contains( a.GroupId.Value ) );
            }

            if ( !string.IsNullOrWhiteSpace( campusIds ) )
            {
                var campusIdList = campusIds.Split( ',' ).AsIntegerList();
                qry = qry.Where( a => a.CampusId.HasValue && campusIdList.Contains( a.CampusId.Value ) );
            }

            // have the "Missed" query be the same as the qry before the Main date range is applied since it'll have a different date range
            var qryMissed = qry;

            if ( dateRange.Start.HasValue )
            {
                qry = qry.Where( a => a.StartDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                qry = qry.Where( a => a.StartDateTime < dateRange.End.Value );
            }

            AttendanceGroupBy groupBy = hfGroupBy.Value.ConvertToEnumOrNull<AttendanceGroupBy>() ?? AttendanceGroupBy.Week;

            var qryAttendanceWithSummaryDateTime = qry.GetAttendanceWithSummaryDateTime( groupBy );

            var qryByPerson = qry.GroupBy( a => a.PersonAlias.PersonId ).Select( a => new
            {
                PersonId = a.Key,
                Attendances = a
            } );

            // we want to get the first 2 visits at a minimum so we can show the date in the grid
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

            // get either the first 2 visits or the first 5 visits (using a const take of 2 or 5 vs a variable to help the SQL optimizer)
            var qryByPersonWithSummary = qryByPerson.Select( a => new
            {
                PersonId = a.PersonId,
                FirstVisits = qryAllVisits.Where( b => b.PersonAlias.PersonId == a.PersonId ).Select( s => new { s.Id, s.StartDateTime } ).OrderBy( x => x.StartDateTime ).Take( 2 ),
                LastVisit = a.Attendances.OrderByDescending( x => x.StartDateTime ).FirstOrDefault(),
                AttendanceSummary = qryAttendanceWithSummaryDateTime.Where( x => x.Attendance.PersonAlias.PersonId == a.PersonId ).GroupBy( g => g.SummaryDateTime ).Select( s => s.Key )
            } );

            if ( nthVisitsTake > 2 )
            {
                qryByPersonWithSummary = qryByPerson.Select( a => new
                {
                    PersonId = a.PersonId,
                    FirstVisits = qryAllVisits.Where( b => b.PersonAlias.PersonId == a.PersonId ).Select( s => new { s.Id, s.StartDateTime } ).OrderBy( x => x.StartDateTime ).Take( 5 ),
                    LastVisit = a.Attendances.OrderByDescending( x => x.StartDateTime ).FirstOrDefault(),
                    AttendanceSummary = qryAttendanceWithSummaryDateTime.Where( x => x.Attendance.PersonAlias.PersonId == a.PersonId ).GroupBy( g => g.SummaryDateTime ).Select( s => s.Key )
                } );
            }

            var qryPerson = new PersonService( rockContext ).Queryable();

            var qryResult = qryByPersonWithSummary.Join(
                qryPerson,
                a => a.PersonId,
                p => p.Id,
                ( a, p ) => new
                    {
                        a.PersonId,
                        Person = p,
                        a.FirstVisits,
                        a.LastVisit,
                        p.PhoneNumbers,
                        a.AttendanceSummary
                    } );

            if ( byNthVisit.HasValue )
            {
                // only return attendees where their nth visit is within the selected daterange
                int skipCount = byNthVisit.Value - 1;
                qryResult = qryResult.Where( a => a.FirstVisits.OrderBy( x => x.StartDateTime ).Skip( skipCount ).Take( 1 ).Any( d => d.StartDateTime >= dateRange.Start && d.StartDateTime < dateRange.End ) );
            }

            if ( attendedMinCount.HasValue )
            {
                qryResult = qryResult.Where( a => a.AttendanceSummary.Count() >= attendedMinCount );
            }

            if ( attendedMissedCount.HasValue )
            {
                if ( attendedMissedDateRange.Start.HasValue && attendedMissedDateRange.End.HasValue )
                {
                    var attendedMissedPossible = GetPossibleAttendancesForDateRange( attendedMissedDateRange, groupBy );
                    int attendedMissedPossibleCount = attendedMissedPossible.Count();

                    qryMissed = qryMissed.Where( a => a.StartDateTime >= attendedMissedDateRange.Start.Value && a.StartDateTime < attendedMissedDateRange.End.Value );
                    var qryMissedAttendanceByPersonAndSummary = qryMissed.GetAttendanceWithSummaryDateTime( groupBy )
                        .GroupBy( g1 => new { g1.SummaryDateTime, g1.Attendance.PersonAlias.PersonId } )
                        .GroupBy( a => a.Key.PersonId )
                        .Select( a => new
                        {
                            PersonId = a.Key,
                            AttendanceCount = a.Count()
                        } );

                    var qryMissedByPerson = qryMissedAttendanceByPersonAndSummary.
                        Where( x => ( attendedMissedPossibleCount - x.AttendanceCount ) >= attendedMissedCount );

                    // filter to only people that missed at least X weeks/months/years between specified missed date range
                    qryResult = qryResult.Where( a => qryMissedByPerson.Any( b => b.PersonId == a.PersonId ) );
                }
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
                        qryResult = qryResult.OrderByDescending( a => a.FirstVisits.FirstOrDefault().StartDateTime );
                    }
                    else
                    {
                        qryResult = qryResult.OrderBy( a => a.FirstVisits.FirstOrDefault().StartDateTime );
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

            var includeParents = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ParentsOfAttendees;
            var parentField = gAttendeesAttendance.Columns.OfType<PersonField>().FirstOrDefault( a => a.HeaderText == "Parent" );
            if ( parentField != null )
            {
                parentField.Visible = includeParents;
            }

            // Calculate all the possible attendance summary dates
            UpdatePossibleAttendances( dateRange, groupBy );

            // pre-load the schedule names since FriendlyScheduleText requires building the ICal object, etc
            _scheduleNameLookup = new ScheduleService( rockContext ).Queryable()
                .ToList()
                .ToDictionary( k => k.Id, v => v.FriendlyScheduleText );

            IQueryable<object> qryFinalResult;

            if ( includeParents )
            {
                var groupTypeFamily = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                int adultRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                int childRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
                int groupTypeFamilyId = groupTypeFamily.Id;
                var qryFamilyGroups = new GroupService( rockContext ).Queryable().Where( m => m.GroupTypeId == groupTypeFamilyId );

                var qryResultWithParent = qryResult.Select( a =>
                    qryFamilyGroups.Where( g => g.Members.Any( m => m.PersonId == a.PersonId && m.GroupRoleId == childRoleId ) )
                      .SelectMany( aa => aa.Members ).Where( bb => bb.GroupRoleId == adultRoleId )
                      .Select( s =>
                          new
                          {
                              Parent = s.Person,
                              Attendance = a
                          } )
                 )
                .SelectMany( x => x )
                .Select( s => new
                {
                    ParentId = s.Parent.Id,
                    PersonId = s.Attendance.PersonId,
                    s.Parent,
                    s.Attendance.Person,
                    s.Attendance.FirstVisits,
                    s.Attendance.LastVisit,
                    s.Attendance.PhoneNumbers,
                    s.Attendance.AttendanceSummary
                } );

                gAttendeesAttendance.PersonIdField = "ParentId";
                gAttendeesAttendance.DataKeyNames = new string[] { "ParentId", "PersonId" };
                qryFinalResult = qryResultWithParent;
            }
            else
            {
                gAttendeesAttendance.PersonIdField = "PersonId";
                gAttendeesAttendance.DataKeyNames = new string[] { "PersonId" };
                qryFinalResult = qryResult;
            }

            // Create the dynamic attendance grid columns as needed
            CreateDynamicAttendanceGridColumns();

            try
            {
                nbAttendeesError.Visible = false;

                // increase the timeout from 30 to 90. The Query can be slow if SQL hasn't calculated the Query Plan for the query yet.
                // Most of the time consumption is figuring out the Query Plan, but after it figures it out, it caches it so that the next time it'll be much faster
                rockContext.Database.CommandTimeout = 90;
                gAttendeesAttendance.DataSource = qryFinalResult.AsNoTracking().ToList();

                gAttendeesAttendance.DataBind();
            }
            catch ( Exception exception )
            {
                string errorMessage = null;
                string stackTrace = string.Empty;
                while ( exception != null )
                {
                    errorMessage = exception.Message;
                    stackTrace += exception.StackTrace;
                    if ( exception is System.Data.SqlClient.SqlException )
                    {
                        // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                        if ( ( exception as System.Data.SqlClient.SqlException ).Number == -2 )
                        {
                            errorMessage = "The attendee report did not complete in a timely manner. Try again using a smaller date range and fewer campuses and groups.";
                            break;
                        }
                        else
                        {
                            exception = exception.InnerException;
                        }
                    }
                    else
                    {
                        exception = exception.InnerException;
                    }
                }

                nbAttendeesError.Text = errorMessage;
                nbAttendeesError.Details = stackTrace;
                nbAttendeesError.Visible = true;
            }
        }

        /// <summary>
        /// Creates the dynamic attendance grid columns.
        /// </summary>
        /// <param name="groupBy">The group by.</param>
        private void CreateDynamicAttendanceGridColumns()
        {
            AttendanceGroupBy groupBy = hfGroupBy.Value.ConvertToEnumOrNull<AttendanceGroupBy>() ?? AttendanceGroupBy.Week;

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
                        case AttendanceGroupBy.Year:
                            boolFromArrayField.HeaderText = summaryDate.ToString( "yyyy" );
                            break;

                        case AttendanceGroupBy.Month:
                            boolFromArrayField.HeaderText = summaryDate.ToString( "MMM yyyy" );
                            break;

                        case AttendanceGroupBy.Week:
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
        public void UpdatePossibleAttendances( DateRange dateRange, AttendanceGroupBy attendanceGroupBy )
        {
            _possibleAttendances = GetPossibleAttendancesForDateRange( dateRange, attendanceGroupBy );
        }

        /// <summary>
        /// Gets the possible attendances for the date range.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="attendanceGroupBy">The attendance group by type.</param>
        /// <returns></returns>
        public List<DateTime> GetPossibleAttendancesForDateRange( DateRange dateRange, AttendanceGroupBy attendanceGroupBy )
        {
            TimeSpan dateRangeSpan = dateRange.End.Value - dateRange.Start.Value;

            var result = new List<DateTime>();

            if ( attendanceGroupBy == AttendanceGroupBy.Week )
            {
                var endOfFirstWeek = dateRange.Start.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
                var endOfLastWeek = dateRange.End.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
                var weekEndDate = endOfFirstWeek;
                while ( weekEndDate <= endOfLastWeek )
                {
                    // Weeks are summarized as the last day of the "Rock" week (Sunday)
                    result.Add( weekEndDate );
                    weekEndDate = weekEndDate.AddDays( 7 );
                }
            }
            else if ( attendanceGroupBy == AttendanceGroupBy.Month )
            {
                var endOfFirstMonth = dateRange.Start.Value.AddDays( -( dateRange.Start.Value.Day - 1 ) ).AddMonths( 1 ).AddDays( -1 );
                var endOfLastMonth = dateRange.End.Value.AddDays( -( dateRange.End.Value.Day - 1 ) ).AddMonths( 1 ).AddDays( -1 );

                //// Months are summarized as the First Day of the month: For example, 5/1/2015 would include everything from 5/1/2015 - 5/31/2015 (inclusive)
                var monthStartDate = new DateTime( endOfFirstMonth.Year, endOfFirstMonth.Month, 1 );
                while ( monthStartDate <= endOfLastMonth )
                {
                    result.Add( monthStartDate );
                    monthStartDate = monthStartDate.AddMonths( 1 );
                }
            }
            else if ( attendanceGroupBy == AttendanceGroupBy.Year )
            {
                var endOfFirstYear = new DateTime( dateRange.Start.Value.Year, 1, 1 ).AddYears( 1 ).AddDays( -1 );
                var endOfLastYear = new DateTime( dateRange.End.Value.Year, 1, 1 ).AddYears( 1 ).AddDays( -1 );

                //// Years are summarized as the First Day of the year: For example, 1/1/2015 would include everything from 1/1/2015 - 12/31/2015 (inclusive)
                var yearStartDate = new DateTime( endOfFirstYear.Year, 1, 1 );
                while ( yearStartDate <= endOfLastYear )
                {
                    result.Add( yearStartDate );
                    yearStartDate = yearStartDate.AddYears( 1 );
                }
            }

            return result;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendeesAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendeesAttendance_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                Literal lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                if ( lFirstVisitDate == null )
                {
                    // Since we have dynamic columns, the templatefields might not get created due some viewstate thingy
                    // so, if we lost the templatefield, force them to instantiate
                    var templateFields = gAttendeesAttendance.Columns.OfType<TemplateField>();
                    foreach ( var templateField in templateFields )
                    {
                        var cellIndex = gAttendeesAttendance.Columns.IndexOf( templateField );
                        var cell = e.Row.Cells[cellIndex] as DataControlFieldCell;
                        templateField.InitializeCell( cell, DataControlCellType.DataCell, e.Row.RowState, e.Row.RowIndex );
                    }

                    lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                }

                Literal lSecondVisitDate = e.Row.FindControl( "lSecondVisitDate" ) as Literal;
                Literal lServiceTime = e.Row.FindControl( "lServiceTime" ) as Literal;
                Literal lHomeAddress = e.Row.FindControl( "lHomeAddress" ) as Literal;
                Literal lAttendanceCount = e.Row.FindControl( "lAttendanceCount" ) as Literal;
                Literal lAttendancePercent = e.Row.FindControl( "lAttendancePercent" ) as Literal;
                var person = dataItem.GetPropertyValue( "Person" ) as Person;

                var firstVisits = dataItem.GetPropertyValue( "FirstVisits" ) as IEnumerable<object>;

                if ( firstVisits != null )
                {
                    var firstVisit = firstVisits.FirstOrDefault();
                    var secondVisit = firstVisits.Skip( 1 ).FirstOrDefault();
                    if ( firstVisit != null )
                    {
                        DateTime? firstVisitDateTime = firstVisit.GetPropertyValue( "StartDateTime" ) as DateTime?;
                        if ( firstVisitDateTime.HasValue )
                        {
                            lFirstVisitDate.Text = firstVisitDateTime.Value.ToShortDateString();
                        }
                    }

                    if ( secondVisit != null )
                    {
                        DateTime? secondVisitDateTime = secondVisit.GetPropertyValue( "StartDateTime" ) as DateTime?;
                        if ( secondVisitDateTime.HasValue )
                        {
                            lSecondVisitDate.Text = secondVisitDateTime.Value.ToShortDateString();
                        }
                    }
                }

                var lastVisit = dataItem.GetPropertyValue( "LastVisit" ) as Attendance;
                if ( lastVisit != null && lastVisit.ScheduleId.HasValue )
                {
                    if ( _scheduleNameLookup.ContainsKey( lastVisit.ScheduleId.Value ) )
                    {
                        lServiceTime.Text = _scheduleNameLookup[lastVisit.ScheduleId.Value];
                    }
                }

                if ( person != null )
                {
                    // Yep, get the address one-row-at-a-time. It usually ends up being faster than joining (especially when there could be 1000s of records, and we only show 50 at a time)
                    var address = person.GetHomeLocation( _rockContext );
                    if ( address != null )
                    {
                        lHomeAddress.Text = address.FormattedHtmlAddress;
                    }
                }

                var attendanceSummary = dataItem.GetPropertyValue( "AttendanceSummary" ) as IEnumerable<DateTime>;
                int attendanceSummaryCount = attendanceSummary.Count();
                lAttendanceCount.Text = attendanceSummaryCount.ToString();

                int? attendencePossibleCount = _possibleAttendances != null ? _possibleAttendances.Count() : (int?)null;

                if ( attendencePossibleCount.HasValue && attendencePossibleCount > 0 )
                {
                    var attendancePerPossibleCount = (decimal)attendanceSummaryCount / attendencePossibleCount.Value;
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

        /// <summary>
        /// Adds the group type controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="pnlGroupTypes">The PNL group types.</param>
        private void AddGroupTypeControls( GroupType groupType, HtmlGenericContainer liGroupTypeItem, List<int> addedGroupTypes = null )
        {
            if ( addedGroupTypes == null )
            {
                addedGroupTypes = new List<int>();
            }

            if ( !addedGroupTypes.Contains( groupType.Id ) )
            {
                addedGroupTypes.Add( groupType.Id );

                if ( groupType.Groups.Any() )
                {
                    bool showGroupAncestry = GetAttributeValue( "ShowGroupAncestry" ).AsBoolean( true );

                    var groupService = new GroupService( _rockContext );

                    var cblGroupTypeGroups = new RockCheckBoxList { ID = "cblGroupTypeGroups" + groupType.Id };

                    cblGroupTypeGroups.Label = groupType.Name;
                    cblGroupTypeGroups.Items.Clear();

                    foreach ( var group in groupType.Groups
                        .Where( g => !g.ParentGroupId.HasValue )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        AddGroupControls( group, cblGroupTypeGroups, groupService, showGroupAncestry );
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
                    var ulGroupTypeList = new HtmlGenericContainer( "ul", "rocktree-children" );

                    liGroupTypeItem.Controls.Add( ulGroupTypeList );
                    foreach ( var childGroupType in groupType.ChildGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var liChildGroupTypeItem = new HtmlGenericContainer( "li", "rocktree-item rocktree-folder" );
                        liChildGroupTypeItem.ID = "liGroupTypeItem" + childGroupType.Id;
                        ulGroupTypeList.Controls.Add( liChildGroupTypeItem );
                        AddGroupTypeControls( childGroupType, liChildGroupTypeItem, addedGroupTypes );
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
            // Only show groups that actually have a schedule
            if ( group != null )
            {
                if ( group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
                {
                    string displayName = showGroupAncestry ? service.GroupAncestorPathName( group.Id ) : group.Name;
                    checkBoxList.Items.Add( new ListItem( displayName, group.Id.ToString() ) );
                }

                if ( group.Groups != null )
                {
                    foreach ( var childGroup in group.Groups
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        AddGroupControls( childGroup, checkBoxList, service, showGroupAncestry );
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
            ParentsOfAttendees = 1
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
        /// Displays the show by.
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
            BindAttendeesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnShowByChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowByChart_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Chart );
            BindChartAttendanceGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BuildGroupTypesUI();
        }

        /// <summary>
        /// Handles the Click event of the btnApplyAttendeesFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyAttendeesFilter_Click( object sender, EventArgs e )
        {
            // both Attendess Filter Apply button just do the same thing as the main apply button
            btnApply_Click( sender, e );
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
    }
}