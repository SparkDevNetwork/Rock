using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockBlocks.Plugins.org_lakepointe.Checkin
{
    [DisplayName( "View Open Schedules" )]
    [Category( "LPC > Check-in" )]
    [Description( "Displays a chart showing which schedules are open within the specified interval." )]
    [BooleanField( "Show Schedule Filter", "Should the Schedules Filter be displayed?", true, "Filters", 0 )]
    [BooleanField( "Show Campus Filter", "Should the Campus Filter be displayed?", true, "Filters", 1 )]
    [BooleanField( "Include Inactive Campuses", "Should campus filter include inactive campuses?", false, "Filters", 2 )]
    public partial class OpenScheduleViewer : RockBlock
    {
        #region Fields
        RockContext _rockContext = null;
        bool? _includeInactiveCampuses = null;
        bool? _showCampusFilter = null;

        #endregion

        #region Propeties
        private DataTable GroupLocationData
        {
            get
            {
                var dataKey = String.Concat( "GroupLocationData_", this.BlockId );
                return (DataTable)ViewState[dataKey];
            }
            set
            {
                var dataKey = String.Concat( "GroupLocationData_", this.BlockId );
                ViewState[dataKey] = value;
            }
        }

        private List<Session> ActiveSessionData
        {
            get
            {
                var dataKey = string.Concat( "ActiveSessionData_", this.BlockId );
                return (List<Session>)ViewState[dataKey];
            }
            set
            {
                var dataKey = String.Concat( "ActiveSessionData_", this.BlockId );
                ViewState[dataKey] = value;
            }
        }

        /// <summary>
        /// The current primary checkin-type id
        /// </summary>
        protected int? CurrentCheckinTypeId
        {
            get { return _currentCheckinTypeId; }
            set
            {
                _currentCheckinTypeId = value;
            }
        }
        private int? _currentCheckinTypeId;

        private bool ShowCampusFilter
        {
            get
            {
                if (!_showCampusFilter.HasValue)
                {
                    _showCampusFilter = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
                }

                return _showCampusFilter ?? false;
            }
        }

        private bool IncludeInactiveCampuses
        {
            get
            {
                if (!_includeInactiveCampuses.HasValue)
                {
                    _includeInactiveCampuses = GetAttributeValue( "IncludeInactiveCampuses" ).AsBoolean();
                }
                return _includeInactiveCampuses ?? false;
            }
        }
        #endregion

        #region BaseControlMethods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.time.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.resize.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.pie.js" );
            RockPage.AddScriptLink( this.Page, "~/Scripts/flot/jquery.flot.categories.js" );

            _rockContext = new RockContext();
            gGroupLocations.Actions.ShowBulkUpdate = false;
            gGroupLocations.Actions.ShowExcelExport = false;
            gGroupLocations.Actions.ShowMergeTemplate = false;
            gGroupLocations.Actions.ShowMergePerson = false;

            gGroupLocations.GridRebind += gGroupLocations_GridRebind;
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!IsPostBack)
            {
                LoadFilters();
                LoadSettings();
                BindDevices();
                BindCheckinTypes();
            }
            else
            {
                BindFlot();

                if ( Request.Form["__EVENTTARGET"] == "ChartClick" )
                {
                    LoadGroupLocations( Request.Form["__EVENTARGUMENT"] );
                }
            }
        }

        #endregion

        #region Events
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            UpdateUserPreferences();

            ActiveSessionData = FindActiveSessions();
            BindFlot();

            if (GroupLocationData != null)
            {
                BindGrid();
            }
        }

        protected void btnFilterSetDefault_Click( object sender, EventArgs e )
        {
            LoadSettings();

            if (GroupLocationData != null)
            {
                BindGrid();
            }
        }

        private void gGroupLocations_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindCheckinTypes();
        }

        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindDevices();
        }

        #endregion

        #region Methods

        #region Filters

        private void BindDevices()
        {
            var campusId = cpCampus.SelectedCampusId;

            Guid kioskDeviceType = Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid();
            ddlKiosk.Items.Clear();
            ddlKiosk.SelectedValue = null;
            using (var rockContext = new RockContext())
            {
                var kiosks = new DeviceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d => d.DeviceType.Guid.Equals( kioskDeviceType ) );

                var selected = new List<Device>();
                foreach (var kiosk in kiosks)
                {
                    if (kiosk.Locations.Where( l => l.CampusId == campusId ).Any())
                    {
                        selected.Add( kiosk );
                    }
                }

                ddlKiosk.DataSource = selected.OrderBy( d => d.Name ).Select( d => new
                {
                    d.Id,
                    d.Name
                } );
            }
            ddlKiosk.DataBind();
            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
        }

        private void BindCheckinTypes()
        {
            BindCheckinTypes( ddlCheckinType.SelectedValueAsInt() );
        }

        private void BindCheckinTypes( int? selectedValue )
        {
            ddlCheckinType.Items.Clear();

            if (ddlKiosk.SelectedValue != None.IdValue)
            {
                using (var rockContext = new RockContext())
                {
                    var kioskCheckinTypes = new List<GroupType>();

                    var groupTypeService = new GroupTypeService( rockContext );

                    Guid templateTypeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
                    ddlCheckinType.DataSource = groupTypeService
                        .Queryable().AsNoTracking()
                        .Where( t =>
                            t.GroupTypePurposeValue != null && 
                            t.GroupTypePurposeValue.Guid == templateTypeGuid )
                        .OrderBy( t => t.Name )
                        .Select( t => new
                        {
                            t.Name,
                            t.Id
                        } )
                        .ToList();
                    ddlCheckinType.DataBind();
                }

                if (selectedValue.HasValue)
                {
                    ddlCheckinType.SetValue( selectedValue );
                }
                else
                {
                    if (CurrentCheckinTypeId.HasValue)
                    {
                        ddlCheckinType.SetValue( CurrentCheckinTypeId );
                    }
                    else
                    {
                        var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_WEEKLY_SERVICE_CHECKIN_AREA.AsGuid() );
                        if (groupType != null)
                        {
                            ddlCheckinType.SetValue( groupType.Id );
                        }
                    }
                }
            }
        }

        private void LoadFilters()
        {
            LoadCampuses();
        }

        private void LoadCampuses()
        {
            cpCampus.Items.Clear();
            pnlCampus.Visible = ShowCampusFilter;
            if (ShowCampusFilter)
            {
                cpCampus.DataSource = CampusCache.All( IncludeInactiveCampuses )
                    .OrderBy( c => c.Name )
                    .ToList();
            }
        }

        #endregion

        #region Schedule Graph

        private void BindFlot()
        {
            if (ActiveSessionData != null)
            {
                string script = @"
                var inData = {0};
                var openData = {1};
                var dataSet = [ {{ label: ""Check-in Open"", data: inData, color: ""#00E800"", bars: {{ align: ""left"" }} }},
                                {{ label: ""Schedule Open"", data: openData, color: ""#e8e8e8"", bars: {{ align: ""right"" }} }}];
                var ticks = {2};

                var options =
                {{
                    series:
                    {{
                        bars:
                        {{
                            show: true
                        }}
                    }},
                    bars:
                    {{
                        align: ""center"",
                        barWidth: 0.5,
                        horizontal: true,
                        fillColor: {{ colors: [{{ opacity: 0.25 }}, {{ opacity: 1 }}] }},
                        lineWidth: 1
                    }},
                    xaxis:
                    {{
                        mode: ""time"",
                        axisLabel: ""Date/Time"",
                        axisLabelUseCanvas: true,
                        axisLabelFontSizePixels: 12,
                        axisLabelFontFamily: 'Verdana, Arial',
                        axisLabelPadding: 10,
                        tickColor: ""#5E5E5E"",
                        color: ""black""
                    }},
                    yaxis:
                    {{
                        axisLabel: ""Schedule"",
                        axisLabelUseCanvas: true,
                        axisLabelFontSizePixels: 12,
                        axisLabelFontFamily: 'Verdana, Arial',
                        axisLabelPadding: 3,
                        tickColor: ""#5E5E5E"",
                        ticks: ticks,
                        color: ""black""
                    }},
                    legend:
                    {{
                        show: true,
                        noColumns: 0,
                        labelBoxBorderColor: ""#858585"",
                        position: ""ne""
                    }},
                    grid:
                    {{
                        hoverable: true,
                        clickable: true,
                        borderWidth: 2,
                        backgroundColor: {{ colors: [""#171717"", ""#4F4F4F""] }}
                    }}
                }};

                $(document).ready( function () {{
                    $.plot($(""#flot-placeholder""), dataSet, options );
                    $(""#flot-placeholder"").UseTooltip();
                    $(""#flot-placeholder"").UsePlotClick();
                }});

                var previousPoint = null, previousLabel = null;
 
                $.fn.UseTooltip = function () {{
                    $(this).bind(""plothover"", function (event, pos, item)
                    {{
                        if (item)
                        {{
                            if ((previousLabel != item.series.label) ||
                                (previousPoint != item.dataIndex))
                            {{
                                previousPoint = item.dataIndex;
                                previousLabel = item.series.label;
                                $(""#tooltip"").remove();

                                var offset = new Date().getTimezoneOffset() * 60 * 1000;
                                var x = new Date(item.datapoint[0] + offset);
                                var y = item.datapoint[1];
                                var z = new Date(item.datapoint[2] + offset);

                                var color = item.series.color;
                                //alert(color)
                                //console.log(item.series.xaxis.ticks[x].label);                

                                showTooltip( item.pageX, item.pageY, color, 
                                    item.series.yaxis.ticks[y].label + "" : "" + (z.getMonth() + 1) + ""/"" + z.getDate() +
                                    "" : "" + z.getHours() + "":"" + ('00' + z.getMinutes()).slice(-2) + "" -> "" + x.getHours() + "":"" + ('00' + x.getMinutes()).slice(-2));
                            }}
                        }}
                        else
                        {{
                            $(""#tooltip"").remove();
                            previousPoint = null;
                        }}
                    }});
                }};

                $.fn.UsePlotClick = function () {{
                    $(this).bind(""plotclick"", function (event, pos, item)
                    {{
                        if (item)
                        {{
                            __doPostBack('ChartClick', item.datapoint[1]);
                        }}
                    }});
                }};

                function showTooltip( x, y, color, contents)
                {{
                    $('<div id=""tooltip"">' + contents + '</div>').css(
                    {{
                        position: 'absolute',
                        display: 'none',
                        top: y - 10,
                        left: x + 10,
                        border: '2px solid ' + color,
                        padding: '3px',
                        'font-size': '9px',
                        'border-radius': '5px',
                        'background-color': '#fff',
                        'font-family': 'Verdana, Arial, Helvetica, Tahoma, sans-serif',
                        opacity: 0.9
                    }}).appendTo( ""body"" ).fadeIn( 200 );
                }}
            ";

                script = string.Format( script, CheckinDataFromSessions(), ScheduleDataFromSessions(), TicksFromSessions() );
                ScriptManager.RegisterStartupScript( upReport, upReport.GetType(), "plotGraph" + RockDateTime.Now.Ticks.ToString(), script, true );
            }
        }

        private List<Session> FindActiveSessions()
        {
            var locationIds = new LocationService( _rockContext )
                .GetByDevice( ddlKiosk.SelectedValueAsInt() ?? 0, true )
                .Select( l => l.Id )
                .ToList();

            var checkinSchedules = new GroupLocationService( _rockContext )
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.LocationId ) && l.Group.IsActive )
                .SelectMany( l => l.Schedules )
                .GroupBy( s => s.Id )
                .Select( s => s.FirstOrDefault() )
                .Where( s => s.IsActive );

            var active = new List<Session>();
            foreach (var schedule in checkinSchedules)
            {
                // ::: GetOccurrences returns occurrences for schedules that reucr weekly but have no days selected. How to kill those?
                foreach (var occurrence in schedule.GetICalOccurrences( dpStart.SelectedDateTime.Value.Date, dpEnd.SelectedDateTime.Value.Date ) )
                {
                    // Note: this logic will break if the requested sequence spans more than 365 days. Not considered an issue.
                    if (occurrence.Period.StartTime.Value.DayOfYear == dpStart.SelectedDateTime.Value.DayOfYear) // first day of the series, consider start time
                    {
                        if (occurrence.Period.StartTime.Value.DayOfYear == dpEnd.SelectedDateTime.Value.DayOfYear) // also the last day of the series, so consider end time too
                        {
                            var calendarEvent = schedule.GetICalEvent();
                            if (calendarEvent != null && calendarEvent.Start != null)
                            {
                                var checkinStart = occurrence.Period.StartTime.Date.AddSeconds( calendarEvent.Start.AddMinutes( 0 - schedule.CheckInStartOffsetMinutes ?? 0 ).Value.TimeOfDay.TotalSeconds );
                                if (checkinStart < dpEnd.SelectedDateTime.Value) // starts before our window of interest closes
                                {
                                    var scheduleStart = occurrence.Period.StartTime.Value;
                                    var checkinEnd = scheduleStart.AddMinutes( schedule.CheckInEndOffsetMinutes.HasValue ? schedule.CheckInEndOffsetMinutes.Value : 0 );
                                    var scheduleEnd = occurrence.Period.StartTime.Value.AddMinutes( calendarEvent.End.Value.Subtract( calendarEvent.Start.Value ).TotalMinutes );
                                    if (scheduleEnd > dpStart.SelectedDateTime.Value) // ends after our window-of-interest opens
                                    {
                                        active.Add( new Session
                                        {
                                            Schedule = schedule,
                                            CheckinStart = checkinStart,
                                            CheckinEnd = checkinEnd,
                                            ScheduleStart = scheduleStart,
                                            ScheduleEnd = scheduleEnd
                                        } ) ;
                                    }
                                }
                            }
                        }
                        else // series spans multiple days, so consider up until midnight
                        {
                            var calendarEvent = schedule.GetICalEvent();
                            if (calendarEvent != null && calendarEvent.Start != null)
                            {
                                var scheduleEnd = occurrence.Period.StartTime.Value.AddMinutes( calendarEvent.End.Value.Subtract( calendarEvent.Start.Value ).TotalMinutes );
                                if (scheduleEnd > dpStart.SelectedDateTime.Value) // ends after our window-of-interest opens
                                {
                                    var scheduleStart = occurrence.Period.StartTime.Value;
                                    var checkinEnd = scheduleStart.AddMinutes( schedule.CheckInEndOffsetMinutes.HasValue ? schedule.CheckInEndOffsetMinutes.Value : 0 );
                                    var checkinStart = occurrence.Period.StartTime.Date.AddSeconds( calendarEvent.Start.AddMinutes( 0 - schedule.CheckInStartOffsetMinutes ?? 0 ).Value.TimeOfDay.TotalSeconds );
                                    active.Add( new Session
                                    {
                                        Schedule = schedule,
                                        CheckinStart = checkinStart,
                                        CheckinEnd = checkinEnd,
                                        ScheduleStart = scheduleStart,
                                        ScheduleEnd = scheduleEnd
                                    } );
                                }
                            }
                        }
                    }
                    else if (occurrence.Period.StartTime.Value.DayOfYear == dpEnd.SelectedDateTime.Value.DayOfYear) // last day of the series, consider end time
                    {
                        var calendarEvent = schedule.GetICalEvent();
                        if (calendarEvent != null && calendarEvent.Start != null)
                        {
                            var checkinStart = occurrence.Period.StartTime.Date.AddSeconds( calendarEvent.Start.AddMinutes( 0 - schedule.CheckInStartOffsetMinutes ?? 0 ).Value.TimeOfDay.TotalSeconds );
                            if (checkinStart < dpEnd.SelectedDateTime.Value) // starts before our window of interest closes
                            {
                                var scheduleStart = occurrence.Period.StartTime.Value;
                                var checkinEnd = scheduleStart.AddMinutes( schedule.CheckInEndOffsetMinutes.HasValue ? schedule.CheckInEndOffsetMinutes.Value : 0 );
                                var scheduleEnd = occurrence.Period.StartTime.Value.AddMinutes( calendarEvent.End.Value.Subtract( calendarEvent.Start.Value ).TotalMinutes );
                                active.Add( new Session
                                {
                                    Schedule = schedule,
                                    CheckinStart = checkinStart,
                                    CheckinEnd = checkinEnd,
                                    ScheduleStart = scheduleStart,
                                    ScheduleEnd = scheduleEnd
                                } );
                            }
                        }
                    }
                    else if ((occurrence.Period.StartTime.Value.DayOfYear > dpStart.SelectedDateTime.Value.DayOfYear) &&
                        (occurrence.Period.StartTime.Value.DayOfYear < dpEnd.SelectedDateTime.Value.DayOfYear)) // days in between, consider entire day
                    {
                        var calendarEvent = schedule.GetICalEvent();
                        if (calendarEvent != null && calendarEvent.Start != null)
                        {
                            var checkinStart = occurrence.Period.StartTime.Date.AddSeconds( calendarEvent.Start.AddMinutes( 0 - schedule.CheckInStartOffsetMinutes ?? 0 ).Value.TimeOfDay.TotalSeconds );
                            var scheduleStart = occurrence.Period.StartTime.Value;
                            var checkinEnd = scheduleStart.AddMinutes( schedule.CheckInEndOffsetMinutes.HasValue ? schedule.CheckInEndOffsetMinutes.Value : 0 );
                            var scheduleEnd = occurrence.Period.StartTime.Value.AddMinutes( calendarEvent.End.Value.Subtract(calendarEvent.Start.Value).TotalMinutes );
                            active.Add( new Session
                            {
                                Schedule = schedule,
                                CheckinStart = checkinStart,
                                CheckinEnd = checkinEnd,
                                ScheduleStart = scheduleStart,
                                ScheduleEnd = scheduleEnd
                            } );
                        }
                    }
                }
            }

            return active.OrderByDescending( s => s.Schedule.Category.Name ).ThenByDescending( s => s.Schedule.Name ).ToList();
        }

        // It would be more elegant to use a JSON serializer to do this, but quicker to code this way than to research how to force JSON to produce the right output
        private string TicksFromSessions()
        {
            var sb = new StringBuilder();
            var span = new TimeSpan( DateTime.Parse( "1/1/1970" ).Ticks );
            var schedules = new List<int>();
            sb.Append( "[" );
            string prefix = "";
            foreach (var session in ActiveSessionData.GroupBy( s => s.Schedule.Id ).Select( s => s.FirstOrDefault() ))
            {
                if (!schedules.Contains( session.Schedule.Id ))
                {
                    schedules.Add( session.Schedule.Id );
                }
                sb.Append( prefix );
                prefix = ",";
                sb.AppendFormat( @"[{0}, ""{1}->{2}""]", schedules.IndexOf( session.Schedule.Id ), session.Schedule.Category.Name, session.Schedule.Name );
            }
            sb.Append( "]" );

            return sb.ToString();
        }

        // It would be more elegant to use a JSON serializer to do this, but quicker to code this way than to research how to force JSON to produce the right output
        private string CheckinDataFromSessions()
        {
            var sb = new StringBuilder();
            var span = new TimeSpan( DateTime.Parse( "1/1/1970" ).Ticks ); // convert to javascript time
            var schedules = new List<int>();
            sb.Append( "[" );
            string prefix = "";
            foreach (var session in ActiveSessionData)
            {
                if (!schedules.Contains( session.Schedule.Id ))
                {
                    schedules.Add( session.Schedule.Id );
                }
                sb.Append( prefix );
                prefix = ",";
                sb.AppendFormat( "[{0}, {1}, {2}]",
                    session.CheckinEnd.Subtract( span ).Ticks / 10000,
                    schedules.IndexOf( session.Schedule.Id ),
                    session.CheckinStart.Subtract( span ).Ticks / 10000 );
            }
            sb.Append( "]" );

            return sb.ToString();
        }

        private string ScheduleDataFromSessions()
        {
            var sb = new StringBuilder();
            var span = new TimeSpan( DateTime.Parse( "1/1/1970" ).Ticks ); // convert to javascript time
            var schedules = new List<int>();
            sb.Append( "[" );
            string prefix = "";
            foreach (var session in ActiveSessionData)
            {
                if (!schedules.Contains( session.Schedule.Id ))
                {
                    schedules.Add( session.Schedule.Id );
                }
                sb.Append( prefix );
                prefix = ",";
                sb.AppendFormat( "[{0}, {1}, {2}]",
                    session.ScheduleEnd.Subtract( span ).Ticks / 10000,
                    schedules.IndexOf( session.Schedule.Id ),
                    session.ScheduleStart.Subtract( span ).Ticks / 10000 );
            }
            sb.Append( "]" );

            return sb.ToString();
        }

        #endregion

        #region Group/Location Grid

        private void LoadGroupLocations( string scheduleIndexString )
        {
            var activeSessionData = ActiveSessionData; // avoid having to parse this out of view state repeatedly

            if ((scheduleIndexString != null) && (activeSessionData != null))
            {
                var scheduleIndex = new int();
                if (int.TryParse( scheduleIndexString, out scheduleIndex ))
                {
                    DataTable dt = new System.Data.DataTable();
                    dt.Columns.Add( new DataColumn( "LocationName", typeof( string ) ) );
                    dt.Columns.Add( new DataColumn( "OpenClose", typeof( string ) ) );
                    dt.Columns.Add( new DataColumn( "GroupName", typeof( string ) ) );
                    dt.Columns.Add( new DataColumn( "GroupType", typeof( string ) ) );

                    // List of groups and locations corresponding to this schedule
                    // We have to rebuild this list to interpret the javascript index correctly
                    var schedules = new List<int>();
                    foreach (var session in activeSessionData.GroupBy( s => s.Schedule.Id ).Select( s => s.FirstOrDefault() ))
                    {
                        if (!schedules.Contains( session.Schedule.Id ))
                        {
                            schedules.Add( session.Schedule.Id );
                        }
                    }
                    var scheduleId = schedules[scheduleIndex];

                    var locationIds = new LocationService( _rockContext )
                        .GetByDevice( ddlKiosk.SelectedValueAsInt() ?? 0, true )
                        .Select( l => l.Id )
                        .ToList();

                    var groupLocations = new GroupLocationService( _rockContext )
                        .Queryable().AsNoTracking()
                        .Where( gl => gl.Schedules.Where( s => s.Id == scheduleId ).Any() && !gl.Group.IsArchived && gl.Group.IsActive && locationIds.Contains( gl.LocationId ) );
                    foreach (var groupLocation in groupLocations)
                    {
                        DataRow dr = dt.NewRow();
                        dr["LocationName"] = groupLocation.Location.Name;
                        dr["OpenClose"] = groupLocation.Location.IsActive ? "Open" : "Closed";
                        dr["GroupName"] = groupLocation.Group.Name;
                        dr["GroupType"] = groupLocation.Group.GroupType.Name;

                        dt.Rows.Add( dr );
                    }
                    GroupLocationData = dt;

                    var schedule = activeSessionData.Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault().Schedule;
                    rlScheduleName.Label = string.Format( @"{0}->{1}", schedule.Category.Name, schedule.Name );
                    BindGrid();
                }
            }
        }

        private void BindGrid()
        {
            nbGroupsError.Visible = false;
            nbGroupsError.Title = "";
            nbGroupsError.Text = "";

            if (GroupLocationData == null)
            {
                pnlResults.Visible = false;
                return;
            }

            var groupLocationList = GroupLocationData.AsEnumerable();

            var sortProperty = gGroupLocations.SortProperty;

            if (sortProperty != null)
            {
                if (sortProperty.Property == "GroupName")
                {
                    if (sortProperty.Direction == SortDirection.Ascending)
                    {
                        groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) )
                            .ThenBy( a => a.Field<string>( "OpenClose" ) );
                    }
                    else
                    {
                        groupLocationList = groupLocationList.OrderByDescending( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) )
                            .ThenBy( a => a.Field<string>( "OpenClose" ) );
                    }
                }
                else if (sortProperty.Property == "GroupType")
                {
                    if (sortProperty.Direction == SortDirection.Ascending)
                    {
                        groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "GroupType" ) )
                            .ThenBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) )
                            .ThenBy( a => a.Field<string>( "OpenClose" ) );
                    }
                    else
                    {
                        groupLocationList = groupLocationList.OrderByDescending( a => a.Field<string>( "GroupType" ) )
                            .ThenBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) )
                            .ThenBy( a => a.Field<string>( "OpenClose" ) );
                    }
                }
                else if (sortProperty.Property == "Location")
                {
                    if (sortProperty.Direction == SortDirection.Ascending)
                    {
                        groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "LocationName" ) );
                    }
                    else
                    {
                        groupLocationList = groupLocationList.OrderByDescending( a => a.Field<string>( "LocationName" ) );
                    }
                }
                else if (sortProperty.Property == "OpenClose")
                {
                    if (sortProperty.Direction == SortDirection.Ascending)
                    {
                        groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "OpenClose" ) )
                            .ThenBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) );
                    }
                    else
                    {
                        groupLocationList = groupLocationList.OrderByDescending( a => a.Field<string>( "OpenClose" ) )
                            .ThenBy( a => a.Field<string>( "GroupName" ) )
                            .ThenBy( a => a.Field<string>( "LocationName" ) );
                    }
                }
                else
                {
                    groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "GroupName" ) )
                        .ThenBy( a => a.Field<string>( "LocationName" ) )
                        .ThenBy( a => a.Field<string>( "OpenClose" ) );
                }

            }
            else
            {
                groupLocationList = groupLocationList.OrderBy( a => a.Field<string>( "GroupName" ) )
                    .ThenBy( a => a.Field<string>( "LocationName" ) )
                    .ThenBy( a => a.Field<string>( "OpenClose" ) );
            }

            if (groupLocationList.Count() > 0)
            {
                GroupLocationData = groupLocationList.CopyToDataTable();
            }

            gGroupLocations.DataSource = GroupLocationData;
            gGroupLocations.DataBind();

            pnlResults.Visible = true;
        }

        #endregion

        #region User Preference Management

        private string GetSetting( string prefix, string key )
        {
            string setting = Request.QueryString[key];
            if (setting != null)
            {
                return setting;
            }

            return this.GetUserPreference( prefix + key );
        }

        private void LoadSettings()
        {
            string keyPrefix = string.Format( "open-schedule-viewer-{0}", this.BlockId );

            if ( ShowCampusFilter )
            {
                int? campusId = GetSetting( keyPrefix, "CampusId" ).AsIntegerOrNull();

                cpCampus.SelectedCampusId = campusId;
            }

            DateTime? selectedStartDate = GetSetting( keyPrefix, "StartDate" ).AsDateTime();
            DateTime? selectedEndDate = GetSetting( keyPrefix, "EndDate" ).AsDateTime();

            dpStart.SelectedDateTime = selectedStartDate.HasValue ? selectedStartDate.Value : RockDateTime.Today;
            dpEnd.SelectedDateTime = selectedEndDate.HasValue ? selectedEndDate.Value : RockDateTime.Today.AddDays( 1 );

            var kioskSelection = GetSetting( keyPrefix, "Kiosk" );
            ddlKiosk.SelectedValue = kioskSelection.IsNullOrWhiteSpace() ? null : kioskSelection;

            var checkinType = GetSetting( keyPrefix, "CheckinType" );
            ddlCheckinType.SelectedValue = checkinType.IsNullOrWhiteSpace() ? null : checkinType;
        }

        private void UpdateUserPreferences()
        {
            string keyPrefix = string.Format( "open-schedule-viewer-{0}", this.BlockId );
            this.SetUserPreference( keyPrefix + "CampusId", cpCampus.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "StartDate", string.Format( "{0:g}", dpStart.SelectedDateTime ), false );
            this.SetUserPreference( keyPrefix + "EndDate", string.Format( "{0:g}", dpEnd.SelectedDateTime ), false );
            this.SetUserPreference( keyPrefix + "Kiosk", ddlKiosk.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "CheckinType", ddlCheckinType.SelectedValue, false );

            this.SaveUserPreferences( keyPrefix );
        }

        #endregion

        #endregion

        #region Supporting Classes

        [Serializable]
        new private class Session
        {
            [NonSerialized]
            private Schedule _schedule;       // can't serialize this
            private int _scheduleId;          // so serialize this instead and reconstruct

            public Schedule Schedule
            {
                get
                {
                    if ( _schedule == null )
                    {
                        _schedule = new ScheduleService( new RockContext() ).Queryable().AsNoTracking()
                            .Where( s => s.Id == _scheduleId ).FirstOrDefault();
                    }
                    return _schedule;
                }
                set
                {
                    _schedule = value;
                    _scheduleId = value.Id;
                }
            }

            public DateTime CheckinStart { get; set; }
            public DateTime CheckinEnd { get; set; }
            public DateTime ScheduleStart { get; set; }
            public DateTime ScheduleEnd { get; set; }
        }

        #endregion
    }
}