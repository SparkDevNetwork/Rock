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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
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
    [DisplayName( "Attendance Analytics" )]
    [Category( "Check-in" )]
    [Description( "Shows a graph of attendance statistics which can be configured for specific groups, date range, etc." )]
    [GroupTypesField( "Group Types", "Optional List of specific group types that should be included. If none are selected, an option to select an attendance type will be displayed and all of that attendance type's areas will be available.", false, "", "", 0 )]
    [BooleanField( "Show Group Ancestry", "By default the group ancestry path is shown.  Unselect this to show only the group name.", true, "", 1 )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked", false, "", "", 2 )]
    [LinkedPage( "Check-in Detail Page", "Page that shows the user details for the check-in data.", false, "", "", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", "", true, false, Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK, "", 4 )]
    public partial class AttendanceAnalytics : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;
        private bool FilterIncludedInURL = false;

        private List<DateTime> _possibleAttendances = null;
        private Dictionary<int, string> _scheduleNameLookup = null;
        private Dictionary<int, Location> _personLocations = null;
        private Dictionary<int, List<PhoneNumber>> _personPhoneNumbers = null;

        private bool _currentlyExporting = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Setup for being able to copy text to clipboard
            RockPage.AddScriptLink( this.Page, "~/Scripts/ZeroClipboard/ZeroClipboard.js" );
            string script = string.Format( @"
    var client = new ZeroClipboard( $('#{0}'));
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );
            btnCopyToClipboard.Attributes["data-clipboard-target"] = hfFilterUrl.ClientID;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gChartAttendance.GridRebind += gChartAttendance_GridRebind;
            gAttendeesAttendance.GridRebind += gAttendeesAttendance_GridRebind;

            gAttendeesAttendance.EntityTypeId = EntityTypeCache.Read<Rock.Model.Person>().Id;

            dvpDataView.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

            _rockContext = new RockContext();

            // show / hide the checkin details page
            btnCheckinDetails.Visible = !string.IsNullOrWhiteSpace( GetAttributeValue( "Check-inDetailPage" ) );
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
            bcAttendance.Options.SetChartStyle( chartStyleDefinedValueGuid );
            bcAttendance.Options.xaxis = new AxisOptions { mode = AxisMode.categories, tickLength = 0 };
            bcAttendance.Options.series.bars.barWidth = 0.6;
            bcAttendance.Options.series.bars.align = "center";

            if ( !Page.IsPostBack )
            {
                lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );

                LoadDropDowns();
                try
                {
                    LoadSettings();
                    if ( FilterIncludedInURL )
                    {
                        LoadChartAndGrids();
                    }
                }
                catch ( Exception exception )
                {
                    LogAndShowException( exception );
                }
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

            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the GAttendeesAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gAttendeesAttendance_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAttendeesGrid( e.IsExporting );
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

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            clbCampuses.Items.Clear();
            var noCampusListItem = new ListItem();
            noCampusListItem.Text = "<span title='Include records that are not associated with a campus'>No Campus</span>";
            noCampusListItem.Value = "null";
            clbCampuses.Items.Add( noCampusListItem );
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Name ) )
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                clbCampuses.Items.Add( listItem );
            }

            var groupTypeGuids = this.GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();
            if ( !groupTypeGuids.Any() )
            {
                // show the CheckinType control if there isn't a block setting for specific group types
                ddlAttendanceType.Visible = true;
                var groupTypeService = new GroupTypeService( _rockContext );
                Guid groupTypePurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
                ddlAttendanceType.GroupTypes = groupTypeService.Queryable()
                        .Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeGuid )
                        .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            }
            else
            {
                // hide the CheckinType control if there is a block setting for group types
                ddlAttendanceType.Visible = false;
            }
        }

        /// <summary>
        /// Builds the group types UI
        /// </summary>
        private void BuildGroupTypesUI()
        {
            var groupTypes = this.GetSelectedGroupTypes();
            if ( groupTypes.Any() )
            {
                nbGroupTypeWarning.Visible = false;

                // only add each grouptype/group once in case they are a child of multiple parents
                _addedGroupTypeIds = new List<int>();
                _addedGroupIds = new List<int>();
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
        private List<GroupType> GetSelectedGroupTypes()
        {
            var groupTypeGuids = this.GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();
            if ( groupTypeGuids.Any() )
            {
                return new GroupTypeService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t => groupTypeGuids.Contains( t.Guid ) )
                    .OrderBy( t => t.Order )
                    .ThenBy( t => t.Name )
                    .ToList();
            }
            else
            {
                if ( ddlAttendanceType.SelectedGroupTypeId.HasValue )
                {
                    return new GroupTypeService( _rockContext )
                        .GetChildGroupTypes( ddlAttendanceType.SelectedGroupTypeId.Value )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();
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
            if ( this.DetailPageGuid.HasValue )
            {
                lcAttendance.ChartClick += lcAttendance_ChartClick;
            }

            bcAttendance.ShowTooltip = true;
            if ( this.DetailPageGuid.HasValue )
            {
                bcAttendance.ChartClick += lcAttendance_ChartClick;
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
                lcAttendance.TooltipFormatter = null;
                double? chartDataWeekCount = null;
                double? chartDataMonthCount = null;
                int maxXLabelCount = 20;
                if ( dateRange.End.HasValue && dateRange.Start.HasValue )
                {
                    chartDataWeekCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 7;
                    chartDataMonthCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 30;
                }

                switch ( groupBy )
                {
                    case ChartGroupBy.Week:
                        {
                            if ( chartDataWeekCount < maxXLabelCount )
                            {
                                lcAttendance.Options.xaxis.tickSize = new string[] { "7", "day" };
                            }
                            else
                            {
                                lcAttendance.Options.xaxis.tickSize = null;
                            }
                            
                            lcAttendance.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = 'Weekend of <br />' + itemDate.toLocaleDateString();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                        }

                        break;

                    case ChartGroupBy.Month:
                        {
                            if ( chartDataMonthCount < maxXLabelCount )
                            {
                                lcAttendance.Options.xaxis.tickSize = new string[] { "1", "month" };
                            }
                            else
                            {
                                lcAttendance.Options.xaxis.tickSize = null;
                            }

                            lcAttendance.TooltipFormatter = @"
function(item) {
    var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                        }

                        break;

                    case ChartGroupBy.Year:
                        {
                            lcAttendance.Options.xaxis.tickSize = new string[] { "1", "year" };
                            lcAttendance.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = itemDate.getFullYear();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
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

                bcAttendance.TooltipFormatter = lcAttendance.TooltipFormatter;

                var chartData = this.GetAttendanceChartData().ToList();
                var jsonSetting = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                };
                string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                var singleDateTime = chartData.GroupBy( a => a.DateTimeStamp ).Count() == 1;
                if ( singleDateTime )
                {
                    bcAttendance.ChartData = chartDataJson;
                }
                else
                {
                    lcAttendance.ChartData = chartDataJson;
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

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettings()
        {
            string keyPrefix = string.Format( "attendance-reporting-{0}-", this.BlockId );

            this.SetUserPreference( keyPrefix + "TemplateGroupTypeId", ddlAttendanceType.SelectedGroupTypeId.ToString(), false );

            this.SetUserPreference( keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues, false );
            this.SetUserPreference( keyPrefix + "GroupBy", hfGroupBy.Value, false );
            this.SetUserPreference( keyPrefix + "GraphBy", hfGraphBy.Value, false );
            this.SetUserPreference( keyPrefix + "CampusIds", clbCampuses.SelectedValues.AsDelimited( "," ), false );
            this.SetUserPreference( keyPrefix + "ScheduleIds", spSchedules.SelectedValues.ToList().AsDelimited( "," ), false );
            this.SetUserPreference( keyPrefix + "DataView", dvpDataView.SelectedValue, false );

            var selectedGroupIds = GetSelectedGroupIds();
            this.SetUserPreference( keyPrefix + "GroupIds", selectedGroupIds.AsDelimited( "," ), false );

            this.SetUserPreference( keyPrefix + "ShowBy", hfShowBy.Value, false );

            this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value, false );

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

            this.SetUserPreference( keyPrefix + "AttendeesFilterByType", attendeesFilterBy.ConvertToInt().ToString(), false );
            this.SetUserPreference( keyPrefix + "AttendeesFilterByVisit", ddlNthVisit.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "AttendeesFilterByPattern", string.Format( "{0}|{1}|{2}|{3}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, tbPatternMissedXTimes.Text, drpPatternDateRange.DelimitedValues ), false );

            this.SaveUserPreferences( keyPrefix );

            // Create URL for selected settings
            var pageReference = CurrentPageReference;
            foreach ( var setting in GetUserPreferences( keyPrefix ) )
            {
                string key = setting.Key.Substring( keyPrefix.Length );
                pageReference.Parameters.AddOrReplace( key, setting.Value );
            }

            Uri uri = new Uri( Request.Url.ToString() );
            hfFilterUrl.Value = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + pageReference.BuildUrl();
            btnCopyToClipboard.Disabled = false;
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
        private void LoadSettings()
        {
            FilterIncludedInURL = false;

            string keyPrefix = string.Format( "attendance-reporting-{0}-", this.BlockId );

            ddlAttendanceType.SelectedGroupTypeId = GetSetting( keyPrefix, "TemplateGroupTypeId" ).AsIntegerOrNull();
            BuildGroupTypesUI();

            string slidingDateRangeSettings = GetSetting( keyPrefix, "SlidingDateRange" );
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

            dvpDataView.SetValue( GetSetting( keyPrefix, "DataView" ) );

            hfGroupBy.Value = GetSetting( keyPrefix, "GroupBy" );
            hfGraphBy.Value = GetSetting( keyPrefix, "GraphBy" );

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
                string campusKey = keyPrefix + "CampusIds";

                var sessionPreferences = RockPage.SessionUserPreferences();
                if ( sessionPreferences.ContainsKey( campusKey ) )
                {
                    campusIdList = sessionPreferences[campusKey].Split( ',' ).ToList();
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

            var scheduleIdList = GetSetting( keyPrefix, "ScheduleIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList();
            if ( scheduleIdList.Any() )
            {
                var schedules = new ScheduleService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => scheduleIdList.Contains( s.Id ) )
                    .ToList();
                spSchedules.SetValues( schedules );
            }

            var groupIdList = GetSetting( keyPrefix, "GroupIds" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

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

            ShowBy showBy = GetSetting( keyPrefix, "ShowBy" ).ConvertToEnumOrNull<ShowBy>() ?? ShowBy.Chart;
            DisplayShowBy( showBy );

            ViewBy viewBy = GetSetting( keyPrefix, "ViewBy" ).ConvertToEnumOrNull<ViewBy>() ?? ViewBy.Attendees;
            hfViewBy.Value = viewBy.ConvertToInt().ToString();

            AttendeesFilterBy attendeesFilterBy = GetSetting( keyPrefix, "AttendeesFilterByType" ).ConvertToEnumOrNull<AttendeesFilterBy>() ?? AttendeesFilterBy.All;

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

            ddlNthVisit.SelectedValue = GetSetting( keyPrefix, "AttendeesFilterByVisit" );
            string attendeesFilterByPattern = GetSetting( keyPrefix, "AttendeesFilterByPattern" );
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
        private string GetSetting( string prefix, string key )
        {
            string setting = Request.QueryString[key];
            if ( setting != null )
            {
                FilterIncludedInURL = true;
                return setting;
            }

            return this.GetUserPreference( prefix + key );
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
                case AttendanceGraphBy.Group: gChartAttendance.Columns[1].HeaderText = "Group"; break;
                case AttendanceGraphBy.Campus: gChartAttendance.Columns[1].HeaderText = "Campus"; break;
                case AttendanceGraphBy.Location: gChartAttendance.Columns[1].HeaderText = "Location"; break;
                case AttendanceGraphBy.Schedule: gChartAttendance.Columns[1].HeaderText = "Schedule"; break;
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
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.End == null )
            {
                dateRange.End = RockDateTime.Now;
            }

            // Adjust the start/end times to reflect the attendance dates who's SundayDate value would fall between the date range selected
            DateTime start = dateRange.Start.HasValue ?
                dateRange.Start.Value.Date.AddDays( 0 - ( dateRange.Start.Value.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)dateRange.Start.Value.DayOfWeek - 1 ) ) :
                new DateTime( 1900, 1, 1 );

            DateTime end = dateRange.End.HasValue ?
                dateRange.End.Value.AddDays( 0 - (int)dateRange.End.Value.DayOfWeek ) :
                new DateTime( 2100, 1, 1, 23, 59, 59 );

            if ( end < start )
            {
                end = end.AddDays( start.Subtract( end ).Days + 6 );
            }

            string groupIds = GetSelectedGroupIds().AsDelimited( "," );

            var selectedCampusIds = clbCampuses.SelectedValues;
            string campusIds = selectedCampusIds.AsDelimited( "," );

            var scheduleIds = spSchedules.SelectedValues.ToList().AsDelimited( "," );

            var chartData = new AttendanceService( _rockContext ).GetChartData(
                hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week,
                hfGraphBy.Value.ConvertToEnumOrNull<AttendanceGraphBy>() ?? AttendanceGraphBy.Total,
                start,
                end,
                groupIds,
                campusIds,
                dvpDataView.SelectedValueAsInt(),
                scheduleIds );
            return chartData;
        }

        /// <summary>
        /// Binds the attendees grid.
        /// </summary>
        private void BindAttendeesGrid( bool isExporting = false )
        {
            // Get Group Type filter
            var groupTypes = this.GetSelectedGroupTypes();
            if ( groupTypes == null || !groupTypes.Any() )
            {
                return;
            }
            var groupTypeIdList = groupTypes.Select( t => t.Id ).ToList();

            // Get the daterange filter
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.End == null )
            {
                dateRange.End = RockDateTime.Now;
            }
            var start = dateRange.Start;
            var end = dateRange.End;

            // Get the group filter
            var groupIdList = new List<int>();
            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            if ( !string.IsNullOrWhiteSpace( groupIds ) )
            {
                groupIdList = groupIds.Split( ',' ).AsIntegerList();
            }

            // If campuses were included, filter attendances by those that have selected campuses
            // if 'null' is one of the campuses, treat that as a 'CampusId is Null'
            var includeNullCampus = clbCampuses.SelectedValues.Any( a => a.Equals( "null", StringComparison.OrdinalIgnoreCase ) );
            var campusIdList = clbCampuses.SelectedValues.AsIntegerList();
            campusIdList.Remove( 0 ); // remove 0 from the list, just in case it is there
            if ( !includeNullCampus && !campusIdList.Any() )
            {
                campusIdList = null;
            }

            // If schedules were included, filter attendance by those that have the selected schedules
            var scheduleIdList = spSchedules.SelectedValues.AsIntegerList();
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

            // Determine how dates shold be grouped
            ChartGroupBy groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;

            // Determine if parents or children are being included with results
            var includeParents = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ParentsOfAttendees;
            var includeChildren = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ChildrenOfAttendees;

            // Atttendance results
            var allAttendeeVisits = new Dictionary<int, AttendeeVisits>();
            var allResults = new List<AttendeeResult>();

            // Collection of async queries to run before assembling data
            var qryTasks = new List<Task>();

            DataTable dtAttendeeLastAttendance = null;
            DataTable dtAttendees = null;
            DataTable dtAttendeeFirstDates = null;
            List<int> personIdsWhoDidNotMiss = null;

            if ( !showNonAttenders )
            {
                // Call the stored procedure to get all the person ids and their attendance dates for anyone
                // whith attendance that matches the selected criteria.
                qryTasks.Add( Task.Run( () =>
                {
                    DataTable dtAttendeeDates = AttendanceService.GetAttendanceAnalyticsAttendeeDates(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                    foreach ( DataRow row in dtAttendeeDates.Rows )
                    {
                        int personId = (int)row["PersonId"];
                        allAttendeeVisits.AddOrIgnore( personId, new AttendeeVisits() );
                        var result = allAttendeeVisits[personId];
                        result.PersonId = personId;

                        DateTime summaryDate = DateTime.MinValue;
                        switch ( groupBy )
                        {
                            case ChartGroupBy.Week: summaryDate = (DateTime)row["SundayDate"]; break;
                            case ChartGroupBy.Month: summaryDate = (DateTime)row["MonthDate"]; break;
                            case ChartGroupBy.Year: summaryDate = (DateTime)row["YearDate"]; break;
                        }
                        if ( !result.AttendanceSummary.Contains( summaryDate ) )
                        {
                            result.AttendanceSummary.Add( summaryDate );
                        }
                    }
                } ) );

                // Call the stored procedure to get the last attendance
                qryTasks.Add( Task.Run( () =>
                {
                    dtAttendeeLastAttendance = AttendanceService.GetAttendanceAnalyticsAttendeeLastAttendance(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];
                } ) );

                // Call the stored procedure to get the names/demographic info for attendess
                qryTasks.Add( Task.Run( () =>
                {
                    dtAttendees = AttendanceService.GetAttendanceAnalyticsAttendees(
                        groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList, includeParents, includeChildren ).Tables[0];
                } ) );

                // If checking for missed attendance, get the people who missed that number of dates during the missed date range
                if ( attendedMissedCount.HasValue &&
                    attendedMissedDateRange.Start.HasValue &&
                    attendedMissedDateRange.End.HasValue )
                {
                    qryTasks.Add( Task.Run( () =>
                    {
                        personIdsWhoDidNotMiss = new List<int>();

                        DataTable dtAttendeeDatesMissed = AttendanceService.GetAttendanceAnalyticsAttendeeDates(
                            groupIdList, attendedMissedDateRange.Start.Value, attendedMissedDateRange.End.Value,
                            campusIdList, includeNullCampus, scheduleIdList ).Tables[0];

                        var missedResults = new Dictionary<int, AttendeeResult>();
                        foreach ( DataRow row in dtAttendeeDatesMissed.Rows )
                        {
                            int personId = (int)row["PersonId"];
                            missedResults.AddOrIgnore( personId, new AttendeeResult() );
                            var missedResult = missedResults[personId];
                            missedResult.PersonId = personId;

                            DateTime summaryDate = DateTime.MinValue;
                            switch ( groupBy )
                            {
                                case ChartGroupBy.Week: summaryDate = (DateTime)row["SundayDate"]; break;
                                case ChartGroupBy.Month: summaryDate = (DateTime)row["MonthDate"]; break;
                                case ChartGroupBy.Year: summaryDate = (DateTime)row["YearDate"]; break;
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
                    } ) );
                }

                // Call the stored procedure to get the first five dates that any person attended this group type
                qryTasks.Add( Task.Run( () =>
                {
                    dtAttendeeFirstDates = AttendanceService.GetAttendanceAnalyticsAttendeeFirstDates(
                        groupTypeIdList, groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList ).Tables[0];
                } ) );
            }
            else
            {
                qryTasks.Add( Task.Run( () =>
                {
                    DataSet ds = AttendanceService.GetAttendanceAnalyticsNonAttendees(
                        groupTypeIdList, groupIdList, start, end, campusIdList, includeNullCampus, scheduleIdList, includeParents, includeChildren );

                    DataTable dtNonAttenders = ds.Tables[0];
                    dtAttendeeFirstDates = ds.Tables[1];
                    dtAttendeeLastAttendance = ds.Tables[2];

                    foreach ( DataRow row in dtNonAttenders.Rows )
                    {
                        int personId = (int)row["Id"];

                        var result = new AttendeeResult();
                        result.PersonId = personId;

                        var person = new PersonInfo();
                        person.NickName = row["NickName"].ToString();
                        person.LastName = row["LastName"].ToString();
                        person.Email = row["Email"].ToString();
                        person.Birthdate = row["BirthDate"] as DateTime?;
                        person.Age = Person.GetAge( person.Birthdate );

                        person.ConnectionStatusValueId = row["ConnectionStatusValueId"] as int?;
                        result.Person = person;

                        if ( includeParents )
                        {
                            result.ParentId = (int)row["ParentId"];
                            var parent = new PersonInfo();
                            parent.NickName = row["ParentNickName"].ToString();
                            parent.LastName = row["ParentLastName"].ToString();
                            parent.Email = row["ParentEmail"].ToString();
                            parent.Birthdate = row["ParentBirthDate"] as DateTime?;
                            parent.Age = Person.GetAge( parent.Birthdate );
                            result.Parent = parent;
                        }

                        if ( includeChildren )
                        {
                            var child = new PersonInfo();
                            result.ChildId = (int)row["ChildId"];
                            child.NickName = row["ChildNickName"].ToString();
                            child.LastName = row["ChildLastName"].ToString();
                            child.Email = row["ChildEmail"].ToString();
                            child.Birthdate = row["ChildBirthDate"] as DateTime?;
                            child.Age = Person.GetAge( child.Birthdate );
                            result.Child = child;
                        }

                        allResults.Add( result );
                    }
                } ) );
            }

            // If a dataview filter was included, find the people who match that criteria
            List<int> dataViewPersonIds = null;
            qryTasks.Add( Task.Run( () =>
            {
                var dataViewId = dvpDataView.SelectedValueAsInt();
                if ( dataViewId.HasValue )
                {
                    dataViewPersonIds = new List<int>();
                    var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var errorMessages = new List<string>();
                        var dvPersonService = new PersonService( _rockContext );
                        ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression, out errorMessages );

                        SortProperty sort = null;
                        var dataViewPersonIdQry = dvPersonService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sort )
                            .Select( p => p.Id );
                        dataViewPersonIds = dataViewPersonIdQry.ToList();
                    }
                }
            } ) );

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

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
                    int personId = (int)row["PersonId"];
                    if ( allAttendeeVisits.ContainsKey( personId ) )
                    {
                        allAttendeeVisits[personId].FirstVisits.Add( (DateTime)row["StartDate"] );
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
                        int personId = (int)row["PersonId"];
                        if ( allAttendeeVisits.ContainsKey( personId ) )
                        {
                            var result = allAttendeeVisits[personId];
                            if ( result.LastVisit == null )
                            {
                                var lastAttendance = new PersonLastAttendance();
                                lastAttendance.CampusId = row["CampusId"] as int?;
                                lastAttendance.GroupId = row["GroupId"] as int?;
                                lastAttendance.GroupName = row["GroupName"].ToString();
                                lastAttendance.RoleName = row["RoleName"].ToString();
                                lastAttendance.InGroup = !string.IsNullOrWhiteSpace( lastAttendance.RoleName );
                                lastAttendance.ScheduleId = row["ScheduleId"] as int?;
                                lastAttendance.StartDateTime = (DateTime)row["StartDateTime"];
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
                        int personId = (int)row["Id"];
                        if ( allAttendeeVisits.ContainsKey( personId ) )
                        {
                            var result = new AttendeeResult( allAttendeeVisits[personId] );

                            var person = new PersonInfo();
                            person.NickName = row["NickName"].ToString();
                            person.LastName = row["LastName"].ToString();
                            person.Email = row["Email"].ToString();
                            person.Birthdate = row["BirthDate"] as DateTime?;
                            person.Age = Person.GetAge( person.Birthdate );
                            person.ConnectionStatusValueId = row["ConnectionStatusValueId"] as int?;
                            result.Person = person;

                            if ( includeParents )
                            {
                                result.ParentId = (int)row["ParentId"];
                                var parent = new PersonInfo();
                                parent.NickName = row["ParentNickName"].ToString();
                                parent.LastName = row["ParentLastName"].ToString();
                                parent.Email = row["ParentEmail"].ToString();
                                parent.Birthdate = row["ParentBirthDate"] as DateTime?;
                                parent.Age = Person.GetAge( parent.Birthdate );
                                result.Parent = parent;
                            }

                            if ( includeChildren )
                            {
                                var child = new PersonInfo();
                                result.ChildId = (int)row["ChildId"];
                                child.NickName = row["ChildNickName"].ToString();
                                child.LastName = row["ChildLastName"].ToString();
                                child.Email = row["ChildEmail"].ToString();
                                child.Birthdate = row["ChildBirthDate"] as DateTime?;
                                child.Age = Person.GetAge( child.Birthdate );
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
                    int personId = (int)row["PersonId"];
                    foreach ( var result in allResults.Where( r => r.PersonId == personId ) )
                    {
                        result.FirstVisits.Add( (DateTime)row["StartDate"] );
                    }
                }

                // Add the Last Attended information
                if ( dtAttendeeLastAttendance != null )
                {
                    foreach ( DataRow row in dtAttendeeLastAttendance.Rows )
                    {
                        int personId = (int)row["PersonId"];
                        foreach ( var result in allResults.Where( r => r.PersonId == personId ) )
                        {
                            if ( result.LastVisit == null )
                            {
                                var lastAttendance = new PersonLastAttendance();
                                lastAttendance.CampusId = row["CampusId"] as int?;
                                lastAttendance.GroupId = row["GroupId"] as int?;
                                lastAttendance.GroupName = row["GroupName"].ToString();
                                lastAttendance.RoleName = row["RoleName"].ToString();
                                lastAttendance.InGroup = !string.IsNullOrWhiteSpace( lastAttendance.RoleName );
                                lastAttendance.ScheduleId = row["ScheduleId"] as int?;
                                lastAttendance.StartDateTime = (DateTime)row["StartDateTime"];
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

            var personUrlFormatString = ( (RockPage)this.Page ).ResolveRockUrl( "~/Person/{0}" );

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

            var parentEmailField = gAttendeesAttendance.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Parent Email" );
            if ( parentEmailField != null )
            {
                parentEmailField.ExcelExportBehavior = includeParents ? ExcelExportBehavior.AlwaysInclude : ExcelExportBehavior.NeverInclude;
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

            // pre-load the schedule names since FriendlyScheduleText requires building the ICal object, etc
            _scheduleNameLookup = new ScheduleService( _rockContext ).Queryable()
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
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();

            if ( familyGroupType != null && homeAddressGuid.HasValue && personIds != null )
            {
                var homeAddressDv = DefinedValueCache.Read( homeAddressGuid.Value );
                if ( homeAddressDv != null )
                {
                    _personLocations = new Dictionary<int, Location>();
                    foreach ( var item in new GroupMemberService( _rockContext )
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
                        _personLocations.AddOrIgnore( item.PersonId, item.Location );
                    }
                }
            }

            // Load the phone numbers
            _personPhoneNumbers = new PhoneNumberService( _rockContext )
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
            TimeSpan dateRangeSpan = dateRange.End.Value - dateRange.Start.Value;

            var result = new List<DateTime>();

            if ( attendanceGroupBy == ChartGroupBy.Week )
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
            else if ( attendanceGroupBy == ChartGroupBy.Month )
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
            else if ( attendanceGroupBy == ChartGroupBy.Year )
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
                        var cellIndex = gAttendeesAttendance.Columns.IndexOf( templateField );
                        var cell = e.Row.Cells[cellIndex] as DataControlFieldCell;
                        templateField.InitializeCell( cell, DataControlCellType.DataCell, e.Row.RowState, e.Row.RowIndex );
                    }

                    lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                }

                Literal lSecondVisitDate = e.Row.FindControl( "lSecondVisitDate" ) as Literal;
                Literal lServiceTime = e.Row.FindControl( "lServiceTime" ) as Literal;
                Literal lHomeAddress = e.Row.FindControl( "lHomeAddress" ) as Literal;
                Literal lPhoneNumbers = e.Row.FindControl( "lPhoneNumbers" ) as Literal;
                Literal lAttendanceCount = e.Row.FindControl( "lAttendanceCount" ) as Literal;
                Literal lAttendancePercent = e.Row.FindControl( "lAttendancePercent" ) as Literal;

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
                    }
                }
                catch ( Exception ex )
                {
                    string msg = ex.Message;
                }

                bool isExporting = _currentlyExporting;
                if ( !isExporting && e is RockGridViewRowEventArgs )
                {
                    isExporting = ( (RockGridViewRowEventArgs)e ).IsExporting;
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
                            var phoneType = DefinedValueCache.Read( phoneNumber.NumberTypeValueId.Value );
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

                if ( groupType.Groups.Any() )
                {
                    bool showGroupAncestry = GetAttributeValue( "ShowGroupAncestry" ).AsBoolean( true );

                    var groupService = new GroupService( _rockContext );

                    var cblGroupTypeGroups = new RockCheckBoxList { ID = "cblGroupTypeGroups" + groupType.Id };

                    cblGroupTypeGroups.Label = groupType.Name;
                    cblGroupTypeGroups.Items.Clear();

                    // limit to Groups that don't have a Parent, or the ParentGroup is a different grouptype so we don't end up with infinite recursion
                    foreach ( var group in groupType.Groups
                        .Where( g => !g.ParentGroupId.HasValue || ( g.ParentGroup.GroupTypeId != groupType.Id ) )
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
                    bool includeGroupsWithoutSchedule = true;
                    if ( includeGroupsWithoutSchedule || group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
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
            BuildGroupTypesUI();
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

            public int? Age { get; set; }

            public DateTime? Birthdate { get; set; }

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

            public int? GroupId { get; set; }

            public string GroupName { get; set; }

            public bool InGroup { get; set; }

            public string RoleName { get; set; }

            public int? ScheduleId { get; set; }

            public DateTime StartDateTime { get; set; }

            public int? LocationId { get; set; }

            public string LocationName { get; set; }
        }
    }
}