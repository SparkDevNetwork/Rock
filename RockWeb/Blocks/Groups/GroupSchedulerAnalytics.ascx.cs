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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Scheduler Analytics" )]
    [Category( "Groups" )]
    [Description( "Provides some visibility into scheduling accountability. Shows check-ins, missed confirmations, declines, and decline reasons with ability to filter by group, date range, data view, and person." )]

    [TextField(
        "Decline Chart Colors",
        Description = "A comma-delimited list of colors that the decline reason chart will use. You will want as many colors as there are decline reasons.",
        IsRequired = false,
        DefaultValue = "#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766",
        Order = 0,
        Key = AttributeKeys.DeclineChartColors )]

    [ColorField(
        "Scheduled",
        Description = "Choose the color to show the number of scheduled persons.",
        IsRequired = true,
        DefaultValue = "#66B2FF",
        Category = "Bar Chart Colors",
        Order = 1,
        Key = AttributeKeys.BarChartScheduledColor)]

    [ColorField(
        "No Response",
        Description = "Choose the color to show the number of schedule requests where the person did not respond.",
        IsRequired = true,
        DefaultValue = "#FFFF66",
        Category = "Bar Chart Colors",
        Order = 2,
        Key = AttributeKeys.BarChartNoResponseColor)]

    [ColorField(
        "Declines",
        Description = "Choose the color to show the number of schedule requests where the person declined.",
        IsRequired = true,
        DefaultValue = "#FFB266",
        Category = "Bar Chart Colors",
        Order = 3,
        Key = AttributeKeys.BarChartDeclinesColor)]

    [ColorField(
        "Attended",
        Description = "Choose the color to show the number of schedule requests where the person attended.",
        IsRequired = true,
        DefaultValue = "#66FF66",
        Category = "Bar Chart Colors",
        Order = 4,
        Key = AttributeKeys.BarChartAttendedColor)]

    [ColorField(
        "Committed No Show",
        Description = "Choose the color to show the number of schedule requests where the person committed but did not attend.",
        IsRequired = true,
        DefaultValue = "#FF6666",
        Category = "Bar Chart Colors",
        Order = 5,
        Key = AttributeKeys.BarChartCommittedNoShowColor)]

    public partial class GroupSchedulerAnalytics : RockBlock
    {
        protected static class AttributeKeys
        {
            public const string DeclineChartColors = "DeclineChartColors";
            public const string BarChartScheduledColor = "BarChartScheduledColor";
            public const string BarChartNoResponseColor = "BarChartNoResponseColor";
            public const string BarChartDeclinesColor = "BarChartDeclinesColor";
            public const string BarChartAttendedColor = "BarChartAttendedColor";
            public const string BarChartCommittedNoShowColor = "BarChartCommittedNoShowColor";
        }

        #region Properties
        protected List<Attendance> attendances = new List<Attendance>();

        protected string SeriesColorsJSON { get; set; }
        protected string BarChartLabelsJSON { get; set; }
        protected string BarChartScheduledJSON { get; set; }
        protected string BarChartNoResponseJSON { get; set; }
        protected string BarChartDeclinesJSON { get; set; }
        protected string BarChartAttendedJSON { get; set; }
        protected string BarChartCommittedNoShowJSON { get; set; }
        protected int? BarChartMaxValue { get; set; }

        protected string DoughnutChartDeclineLabelsJSON { get; set; }
        protected string DoughnutChartDeclineValuesJSON { get; set; }

        private List<string> _errorMessages;

        #endregion Properties

        #region Overrides
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvDataViews.EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON ).Id;
            // NOTE: moment.js needs to be loaded before chartjs
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );

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
            if (!IsPostBack)
            {
                hfTabs.Value = "group";
                lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now ) +
                    "<h3>Doughnut Chart</h3><p>This chart represents the combined total of decline reasons that were selected.  In some cases, decline reasons are provided so this chart may be empty.</p>";
            }
        }

        #endregion Overrides

        /// <summary>
        /// Registers the chart scripts.
        /// </summary>
        protected void RegisterChartScripts()
        {
            RegisterBarChartScript();
            RegisterDoughnutChartScript();
        }

        /// <summary>
        /// Registers the doughnut chart Chart.js script. This should be called after loading the data into the class vars.
        /// </summary>
        protected void RegisterDoughnutChartScript()
        {
            if (DoughnutChartDeclineValuesJSON.IsNullOrWhiteSpace() )
            {
                return;
            }

            int valLength = DoughnutChartDeclineValuesJSON.Split( ',' ).Length;
            string colors = "['#F3F3F3']";

            if ( DoughnutChartDeclineLabelsJSON != "['No data found']" )
            {
                colors = "['" + string.Join( "','", this.GetAttributeValue( "DeclineChartColors" ).Split( ',' ).Take( valLength ) ) + "']";
            }

            string script = string.Format( @"
var dnutCtx = $('#{0}')[0].getContext('2d');

var dnutChart = new Chart(dnutCtx, {{
    type: 'doughnut',
    data: {{
        labels: {1},
        datasets: [{{
            type: 'doughnut',
            data: {2},
            backgroundColor: {3}
        }}]
    }},
    options: {{
        responsive: true,
        legend: {{
            position: 'right',
            fullWidth: true
        }},
        cutoutPercentage: 75,
        animation: {{
			animateScale: true,
			animateRotate: true
		}}
    }}
}});",
                doughnutChartCanvas.ClientID,
                DoughnutChartDeclineLabelsJSON,
                DoughnutChartDeclineValuesJSON,
                colors
                
            );

            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "groupSchedulerDoughnutChartScript", script, true );
        }

        /// <summary>
        /// Registers the bar chart Chart.js script. This should be called after loading the data into the class vars.
        /// </summary>
        protected void RegisterBarChartScript()
        {
            string steps = string.Empty;

            // This is to prevent chart.js displaying decimal values in the y-axis for low numbers. Forces the step size to 1.
            // for larger numbers we'll let chart.js scale as it does not display decimals for large numbers.
            if (BarChartMaxValue != null && BarChartMaxValue.Value < 9 )
            {
                steps = "ticks: { stepSize: 1 },";
            }

            string script = string.Format( @"
var barCtx = $('#{0}')[0].getContext('2d');

var barChart = new Chart(barCtx, {{
    type: 'bar',
    data: {{
        labels: {1},
        datasets: [{{
            label: 'Scheduled',
            backgroundColor: '{2}',
            borderColor: '#E0E0E0',
            data: {3},
        }},
        {{
            label: 'No Response',
            backgroundColor: '{4}',
            borderColor: '#E0E0E0',
            data: {5},
        }},
        {{
            label: 'Declines',
            backgroundColor: '{6}',
            borderColor: '#E0E0E0',
            data: {7}
        }},
        {{
            label: 'Attended',
            backgroundColor: '{8}',
            borderColor: '#E0E0E0',
            data: {9}
        }},
        {{
            label: 'Committed No Show',
            backgroundColor: '{10}',
            borderColor: '#E0E0E0',
            data: {11}
        }}]
    }},

    options: {{
        scales: {{
			xAxes: [{{
				stacked: true,
			}}],
			yAxes: [{{
                {12}
				stacked: true
			}}]
		}}
    }}
}});",
            barChartCanvas.ClientID,
            BarChartLabelsJSON,
            GetAttributeValue(AttributeKeys.BarChartScheduledColor),
            BarChartScheduledJSON,
            GetAttributeValue(AttributeKeys.BarChartNoResponseColor),
            BarChartNoResponseJSON,
            GetAttributeValue(AttributeKeys.BarChartDeclinesColor),
            BarChartDeclinesJSON,
            GetAttributeValue(AttributeKeys.BarChartAttendedColor),
            BarChartAttendedJSON,
            GetAttributeValue(AttributeKeys.BarChartCommittedNoShowColor),
            BarChartCommittedNoShowJSON,
            steps
            );

            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "groupSchedulerBarChartScript", script, true );
        }

        /// <summary>
        /// What to do if the block settings are changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Clears the existing data from class var attendances and repopulates it with data for the selected group and filter criteria.
        /// Data is organized by each person in the group.
        /// </summary>
        private void GetAttendanceData()
        {
            attendances.Clear();
            // Source data for all tables and graphs
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var groupAttendances = attendanceService
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.RequestedToAttend == true );

                switch ( hfTabs.Value )
                {
                    case "group":
                        groupAttendances = groupAttendances.Where( a => a.Occurrence.GroupId == gpGroups.GroupId );
                        break;
                    case "person":
                        groupAttendances = groupAttendances.Where( a => a.PersonAliasId == ppPerson.PersonAliasId );
                        break;
                    case "dataview":
                        var dataView = new DataViewService( rockContext ).Get( dvDataViews.SelectedValueAsInt().Value );
                        var personsFromDv = dataView.GetQuery( null, rockContext, null, out _errorMessages ) as IQueryable<Person>;
                        var personAliasIds = personsFromDv.Select( d => d.Aliases.Where( a => a.AliasPersonId == d.Id ).Select( a => a.Id ).FirstOrDefault() ).ToList();
                        
                        groupAttendances = groupAttendances.Where( a => personAliasIds.Contains( a.PersonAliasId.Value ) );
                        
                        break;
                    default:
                        break;
                }

                // parse the date range and add to query
                if ( sdrpDateRange.DelimitedValues.IsNotNullOrWhiteSpace())
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        groupAttendances = groupAttendances.Where( a => DbFunctions.TruncateTime( a.StartDateTime ) >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        groupAttendances = groupAttendances.Where( a => DbFunctions.TruncateTime( a.StartDateTime ) <= dateRange.End.Value );
                    }
                }

                // add selected locations to the query
                if ( cblLocations.SelectedValues.Any() )
                {
                    groupAttendances = groupAttendances.Where( a => cblLocations.SelectedValuesAsInt.Contains( a.Occurrence.LocationId ?? -1 ) );
                }

                // add selected schedules to the query
                if (cblSchedules.SelectedValues.Any() )
                {
                    groupAttendances = groupAttendances.Where( a => cblSchedules.SelectedValuesAsInt.Contains( a.Occurrence.ScheduleId ?? -1 ) );
                }

                attendances = groupAttendances.ToList();
            }
        }

        /// <summary>
        /// Populates the locations checkbox list for the selected group.
        /// </summary>
        protected void LoadLocations()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var locations = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gl => gl.Location.IsActive == true )
                    .Where( gl => gl.GroupId == gpGroups.GroupId )
                    .OrderBy( gl => gl.Order )
                    .ThenBy( gl => gl.Location.Name );

                var source = locations.Select( gl => gl.Location ).ToList();

                if ( source.Any() )
                {
                    cblLocations.Visible = true;
                    cblLocations.DataValueField = "Id";
                    cblLocations.DataTextField = "Name";
                    cblLocations.DataSource = source;
                    cblLocations.DataBind();
                }
            }
        }

        /// <summary>
        /// Populates the schedules checkbox list for the group
        /// </summary>
        protected void LoadSchedules()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupLocationService = new GroupLocationService( rockContext );
                var schedules = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gl => gl.GroupId == gpGroups.GroupId );

                if ( cblLocations.SelectedValues.Any() )
                {
                    schedules = schedules.Where( gl => cblLocations.SelectedValuesAsInt.Contains( gl.LocationId ) );
                }

                var source = schedules.SelectMany( gl => gl.Schedules ).DistinctBy( s => s.Guid ).ToList();

                if ( source.Any() )
                {
                    cblSchedules.Visible = true;
                    cblSchedules.DataValueField = "Id";
                    cblSchedules.DataTextField = "Name";
                    cblSchedules.DataSource = source;
                    cblSchedules.DataBind();
                }
            }
        }

        /// <summary>
        /// Shows the bar graph.
        /// </summary>
        protected void ShowBarGraph()
        {
            if ( !attendances.Any() )
            {
                nbNoData.Visible = true;
                return;
            }

            DateTime firstDateTime;
            DateTime lastDateTime;

            if ( sdrpDateRange.DelimitedValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );
                firstDateTime = dateRange.Start.Value.Date;
                lastDateTime = dateRange.End.Value.Date;
            }
            else
            {
                firstDateTime = attendances.Min( a => a.StartDateTime.Date );
                lastDateTime = attendances.Max( a => a.StartDateTime.Date );
            }

            int daysCount = ( int ) Math.Ceiling( ( lastDateTime - firstDateTime ).TotalDays );

            if ( daysCount / 7 > 26 )
            {
                // if more than 6 months summarize by month
                CreateBarChartGroupedByMonth( firstDateTime, lastDateTime );
            }
            else if ( daysCount > 31 )
            {
                //if more than 1 month summarize by week
                CreateBarChartGroupedByWeek( daysCount, firstDateTime );
            }
            else 
            {
                // Otherwise summarize by day
                CreateBarChartGroupedByDay( daysCount, firstDateTime );
            }
        }

        /// <summary>
        /// Creates the bar chart with data grouped by month.
        /// </summary>
        /// <param name="firstDateTime">The first date time.</param>
        /// <param name="lastDateTime">The last date time.</param>
        protected void CreateBarChartGroupedByMonth( DateTime firstDateTime, DateTime lastDateTime )
        {
            List<SchedulerGroupMember> barchartdata = attendances
                .GroupBy( a => new { StartYear = a.StartDateTime.Year, StartMonth = a.StartDateTime.Month  } )
                .Select( a => new SchedulerGroupMember
                {
                    StartDateTime = new DateTime(a.Key.StartYear, a.Key.StartMonth, 1 ),
                    Scheduled = a.Count(),
                    NoResponse = a.Count( aa => aa.RSVP == RSVP.Unknown ),
                    Declines = a.Count( aa => aa.RSVP == RSVP.No ),
                    Attended = a.Count( aa => aa.DidAttend == true ),
                    CommittedNoShow = a.Count( aa => aa.RSVP == RSVP.Yes && aa.DidAttend == false )
                } )
                .ToList();

                var monthsCount = ( ( lastDateTime.Year - firstDateTime.Year ) * 12 ) + ( lastDateTime.Month - firstDateTime.Month ) + 1;
                var months = Enumerable.Range( 0, monthsCount )
                    .Select(x => new
                    { 
                        year = firstDateTime.AddMonths(x).Year, 
                        month = firstDateTime.AddMonths(x).Month
                    } );

                var groupedByMonth = months
                    .GroupJoin( barchartdata, m => new { m.month, m.year },
                        a => new { month = a.StartDateTime.Month, year = a.StartDateTime.Year },
                        ( g, d ) => new
                        {
                            Month = g.month,
                            Year = g.year,
                            Scheduled = d.Sum( a => a.Scheduled ),
                            NoResponse = d.Sum( a => a.NoResponse ),
                            Declines = d.Sum( a => a.Declines ),
                            Attended = d.Sum( a => a.Attended ),
                            CommittedNoShow = d.Sum( a => a.CommittedNoShow ),
                            Total = d.Sum( a => a.Scheduled ) + d.Sum( a => a.NoResponse ) + d.Sum( a => a.Declines ) + d.Sum( a => a.Attended ) + d.Sum( a => a.CommittedNoShow )
                        }
                    );

                this.BarChartLabelsJSON = "['" + groupedByMonth
                    .OrderBy(a => a.Year)
                    .ThenBy( a => a.Month)
                    .Select( a => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName( a.Month ) + " " + a.Year )
                    .ToList()
                    .AsDelimited( "','" ) + "']";

                nbNoData.Visible = !barchartdata.Any();

                BarChartMaxValue = groupedByMonth.Max( x => x.Total );
                BarChartScheduledJSON = groupedByMonth.OrderBy(a => a.Year).ThenBy( a => a.Month).Select( d => d.Scheduled ).ToJson();
                BarChartNoResponseJSON = groupedByMonth.OrderBy(a => a.Year).ThenBy( a => a.Month).Select( d => d.NoResponse ).ToJson();
                BarChartDeclinesJSON = groupedByMonth.OrderBy(a => a.Year).ThenBy( a => a.Month).Select( d => d.Declines ).ToJson();
                BarChartAttendedJSON = groupedByMonth.OrderBy(a => a.Year).ThenBy( a => a.Month).Select( d => d.Attended ).ToJson();
                BarChartCommittedNoShowJSON = groupedByMonth.OrderBy(a => a.Year).ThenBy( a => a.Month).Select( d => d.CommittedNoShow ).ToJson();
        }

        /// <summary>
        /// Creates the bar chart with data grouped by week.
        /// </summary>
        /// <param name="daysCount">The days count.</param>
        /// <param name="firstDateTime">The first date time.</param>
        protected void CreateBarChartGroupedByWeek( int daysCount, DateTime firstDateTime )
        {
            List<SchedulerGroupMember> barchartdata = attendances
                .GroupBy( a => new { StartWeek = a.StartDateTime.StartOfWeek(DayOfWeek.Monday) } )
                .Select( a => new SchedulerGroupMember
                {
                    StartDateTime = a.Key.StartWeek,
                    Scheduled = a.Count(),
                    NoResponse = a.Count( aa => aa.RSVP == RSVP.Unknown ),
                    Declines = a.Count( aa => aa.RSVP == RSVP.No ),
                    Attended = a.Count( aa => aa.DidAttend == true ),
                    CommittedNoShow = a.Count( aa => aa.RSVP == RSVP.Yes && aa.DidAttend == false )
                } )
                .ToList();

            var weeks = Enumerable.Range( 0, ( int ) Math.Ceiling( ( daysCount / 7.0 ) + 1 ) )
                .Select(x => new
                { 
                    date = firstDateTime.StartOfWeek(DayOfWeek.Monday).AddDays(x * 7)
                } );

            var groupedByDate = weeks
                .GroupJoin( barchartdata, m => new { m.date },
                    a => new { date = a.StartDateTime.Date },
                    ( g, d ) => new
                    {
                        Date = g.date,
                        Scheduled = d.Sum( a => a.Scheduled ),
                        NoResponse = d.Sum( a => a.NoResponse ),
                        Declines = d.Sum( a => a.Declines ),
                        Attended = d.Sum( a => a.Attended ),
                        CommittedNoShow = d.Sum( a => a.CommittedNoShow ),
                        Total = d.Sum( a => a.Scheduled ) + d.Sum( a => a.NoResponse ) + d.Sum( a => a.Declines ) + d.Sum( a => a.Attended ) + d.Sum( a => a.CommittedNoShow )
                    }
                );

            this.BarChartLabelsJSON = "['" + groupedByDate
                .OrderBy(a => a.Date)
                .Select( a => a.Date.ToShortDateString() )
                .ToList()
                .AsDelimited( "','" ) + "']";

            nbNoData.Visible = !barchartdata.Any();

            BarChartMaxValue = groupedByDate.Max( x => x.Total );
            BarChartScheduledJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Scheduled ).ToJson();
            BarChartNoResponseJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.NoResponse ).ToJson();
            BarChartDeclinesJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Declines ).ToJson();
            BarChartAttendedJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Attended ).ToJson();
            BarChartCommittedNoShowJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.CommittedNoShow ).ToJson();
        }

        /// <summary>
        /// Creates the bar chart with data grouped by day.
        /// </summary>
        /// <param name="daysCount">The days count.</param>
        /// <param name="firstDateTime">The first date time.</param>
        protected void CreateBarChartGroupedByDay( int daysCount, DateTime firstDateTime )
        {
            List<SchedulerGroupMember> barchartdata = attendances
                .GroupBy( a => new { StartYear = a.StartDateTime.Year, StartMonth = a.StartDateTime.Month, StartDay = a.StartDateTime.Day  } )
                .Select( a => new SchedulerGroupMember
                {
                    StartDateTime = new DateTime( a.Key.StartYear, a.Key.StartMonth, a.Key.StartDay ),
                    Scheduled = a.Count(),
                    NoResponse = a.Count( aa => aa.RSVP == RSVP.Unknown ),
                    Declines = a.Count( aa => aa.RSVP == RSVP.No ),
                    Attended = a.Count( aa => aa.DidAttend == true ),
                    CommittedNoShow = a.Count( aa => aa.RSVP == RSVP.Yes && aa.DidAttend == false )
                } )
                .ToList();

            var days = Enumerable.Range( 0, daysCount + 1 )
                .Select(x => new
                { 
                    date = firstDateTime.AddDays(x)
                } );

            var groupedByDate = days
                .GroupJoin( barchartdata, m => new { m.date },
                    a => new { date = a.StartDateTime.Date },
                    ( g, d ) => new
                    {
                        Date = g.date,
                        Scheduled = d.Sum( a => a.Scheduled ),
                        NoResponse = d.Sum( a => a.NoResponse ),
                        Declines = d.Sum( a => a.Declines ),
                        Attended = d.Sum( a => a.Attended ),
                        CommittedNoShow = d.Sum( a => a.CommittedNoShow ),
                        Total = d.Sum( a => a.Scheduled ) + d.Sum( a => a.NoResponse ) + d.Sum( a => a.Declines ) + d.Sum( a => a.Attended ) + d.Sum( a => a.CommittedNoShow )
                    }
                );

            this.BarChartLabelsJSON = "['" + groupedByDate
                .OrderBy(a => a.Date)
                .Select( a => a.Date.ToShortDateString() )
                .ToList()
                .AsDelimited( "','" ) + "']";

            nbNoData.Visible = !barchartdata.Any();

            BarChartMaxValue = groupedByDate.Max( x => x.Total );
            BarChartScheduledJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Scheduled ).ToJson();
            BarChartNoResponseJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.NoResponse ).ToJson();
            BarChartDeclinesJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Declines ).ToJson();
            BarChartAttendedJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.Attended ).ToJson();
            BarChartCommittedNoShowJSON = groupedByDate.OrderBy(a => a.Date).Select( d => d.CommittedNoShow ).ToJson();
        }

        /// <summary>
        /// Validates the minimum data has been selected current tab in order to get data.
        /// </summary>
        /// <returns></returns>
        protected bool ValidateFilter()
        {
            nbGroupWarning.Visible = nbPersonWarning.Visible = nbDataviewWarning.Visible = false;
            switch ( hfTabs.Value )
            {
                case "group":
                    return gpGroups.GroupId != null ? true : !( nbGroupWarning.Visible = true );
                case "person":
                    return ppPerson.PersonAliasId != null ? true : !( nbPersonWarning.Visible = true );
                case "dataview":
                    return ( dvDataViews.SelectedValue != null && dvDataViews.SelectedValue != "0" ) ? true : !( nbDataviewWarning.Visible = true );
            }

            return false;
        }

        /// <summary>
        /// Shows the doughnut graph.
        /// </summary>
        protected void ShowDoughnutGraph()
        {
            if ( !attendances.Any() )
            {
                return;
            }

            var declines = attendances.Where( a => a.DeclineReasonValueId != null ).GroupBy( a => a.DeclineReasonValueId ).Select( a => new { Reason = a.Key, Count = a.Count() } );

            if ( declines.Any() )
            {
                DoughnutChartDeclineLabelsJSON = "['" + declines
                .OrderByDescending( d => d.Count )
                .Select( d => DefinedValueCache.Get( d.Reason.Value ).Value)
                .ToList()
                .AsDelimited("','") + "']";

                DoughnutChartDeclineValuesJSON = declines.OrderByDescending( d => d.Count ).Select( d => d.Count ).ToJson();
            }
            else
            {
                DoughnutChartDeclineLabelsJSON = "['No decline data found']";
                DoughnutChartDeclineValuesJSON = "[1]";
            }
        }

        /// <summary>
        /// Shows the grid.
        /// </summary>
        protected void ShowGrid()
        {
            gData.Visible = true;
            var schedulerGroupMembers = new List<SchedulerGroupMember>();

            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );

                foreach ( var personAliasId in attendances.Select( a => a.PersonAliasId ).Distinct() )
                {
                    var schedulerGroupMember = new SchedulerGroupMember();
                    schedulerGroupMember.Name = personAliasService.GetPerson( personAliasId.Value ).FullName;
                    schedulerGroupMember.Scheduled = attendances.Where( a => a.PersonAliasId == personAliasId.Value ).Count();
                    schedulerGroupMember.NoResponse = attendances.Where( a => a.PersonAliasId == personAliasId.Value && a.RSVP == RSVP.Unknown ).Count();
                    schedulerGroupMember.Declines = attendances.Where( a => a.PersonAliasId == personAliasId.Value && a.RSVP == RSVP.No).Count();
                    schedulerGroupMember.Attended = attendances.Where( a => a.PersonAliasId == personAliasId.Value && a.DidAttend == true ).Count();
                    schedulerGroupMember.CommittedNoShow = attendances.Where( a => a.PersonAliasId == personAliasId.Value && a.RSVP == RSVP.Yes && a.DidAttend == false ).Count();

                    schedulerGroupMembers.Add( schedulerGroupMember );
                }
            }

            gData.DataSource = schedulerGroupMembers;
            gData.DataBind();
        }

        /// <summary>
        /// Resets values for common controls. Should be called when selecting a group, person, or dataview.
        /// </summary>
        protected void ResetCommonControls()
        {
            // sdrpDateRange we'll leave the date alone since it is not derived from any other data.
            cblLocations.Items.Clear();
            cblLocations.Visible = false;

            cblSchedules.Items.Clear();
            cblSchedules.Visible = false;

            gData.Visible = false;
        }
        
        #region Control Events

        protected void gpGroups_SelectItem( object sender, EventArgs e )
        {
            ResetCommonControls();
            
            if ( !ValidateFilter() )                
            {
                return;
            }

            ppPerson.SetValue( null );
            dvDataViews.SetValue( null);
            LoadLocations();
            LoadSchedules();
        }

        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( !ValidateFilter() )                
            {
                return;
            }

            gpGroups.SetValue( null );
            dvDataViews.SetValue( null );
            ResetCommonControls();
        }

        protected void dvDataViews_SelectItem( object sender, EventArgs e )
        {
            if ( !ValidateFilter() )                
            {
                return;
            }

            gpGroups.SetValue( null );
            ppPerson.SetValue( null );
            ResetCommonControls();
        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            nbBarChartMessage.Visible = false;

            if ( !ValidateFilter() )
            {
                return;
            }

            GetAttendanceData();
            ShowGrid();
            ShowBarGraph();
            ShowDoughnutGraph();
            RegisterChartScripts();
        }

        protected void cblLocations_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadSchedules();
        }

        #endregion Control Events

        protected class SchedulerGroupMember
        {
            public string Name { get; set; }
            public DateTime StartDateTime { get; set; }
            public int Scheduled { get; set; }
            public int NoResponse { get; set; }
            public int Declines { get; set; }
            public int Attended { get; set; }
            public int CommittedNoShow { get; set; }
        }
    }
}