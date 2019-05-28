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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.GroupScheduling
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Group Scheduler Analytics" )]
    [Category( "Group Scheduling" )]
    [Description( "Provides some visibility into scheduling accountability. Shows check-ins, missed confirmations, declines, and decline reasons with ability to filter by group, date range, data view, and person." )]

    [TextField(
        "Decline Chart Colors",
        Description = "A comma-delimited list of colors that the decline reason chart will use. You will want as many colors as there are decline reasons.",
        IsRequired = false,
        DefaultValue = "#01B8AA,#374649,#FD625E,#F2C80F,#5F6B6D,#8AD4EB,#FE9666,#A66999",
        Order = 0,
        Key = AttributeKey.DeclineChartColors )]

    [ColorField(
        "No Response",
        Description = "Choose the color to show the number of schedule requests where the person did not respond.",
        IsRequired = true,
        DefaultValue = "#FFC870",
        Category = "Bar Chart Colors",
        Order = 2,
        Key = AttributeKey.BarChartNoResponseColor )]

    [ColorField(
        "Declines",
        Description = "Choose the color to show the number of schedule requests where the person declined.",
        IsRequired = true,
        DefaultValue = "#D4442E",
        Category = "Bar Chart Colors",
        Order = 3,
        Key = AttributeKey.BarChartDeclinesColor )]

    [ColorField(
        "Attended",
        Description = "Choose the color to show the number of schedule requests where the person attended.",
        IsRequired = true,
        DefaultValue = "#16C98D",
        Category = "Bar Chart Colors",
        Order = 4,
        Key = AttributeKey.BarChartAttendedColor )]

    [ColorField(
        "Committed No Show",
        Description = "Choose the color to show the number of schedule requests where the person committed but did not attend.",
        IsRequired = true,
        DefaultValue = "#484848",
        Category = "Bar Chart Colors",
        Order = 5,
        Key = AttributeKey.BarChartCommittedNoShowColor )]

    public partial class GroupSchedulerAnalytics : RockBlock
    {
        protected static class AttributeKey
        {
            public const string DeclineChartColors = "DeclineChartColors";
            public const string BarChartNoResponseColor = "BarChartNoResponseColor";
            public const string BarChartDeclinesColor = "BarChartDeclinesColor";
            public const string BarChartAttendedColor = "BarChartAttendedColor";
            public const string BarChartCommittedNoShowColor = "BarChartCommittedNoShowColor";
        }

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The selected Date Range
            /// </summary>
            public const string SelectedDateRange = "SelectedDateRange";

            /// <summary>
            /// The selected View By
            /// </summary>
            public const string SelectedViewBy = "ViewBy";

            /// <summary>
            /// The selected group identifier
            /// </summary>
            public const string SelectedGroupId = "SelectedGroupId";

            /// <summary>
            /// The selected group location ids
            /// </summary>
            public const string SelectedLocationIds = "SelectedLocationIds";

            /// <summary>
            /// The selected schedule ids
            /// </summary>
            public const string SelectedScheduleIds = "SelectedScheduleIds";

            /// <summary>
            /// The selected data view identifier
            /// </summary>
            public const string SelectedDataViewId = "SelectedDataViewId";

            /// <summary>
            /// The selected person identifier
            /// </summary>
            public const string SelectedPersonId = "SelectedPersonId";
        }

        #endregion UserPreferanceKeys

        #region Properties

        protected string SeriesColorsJSON { get; set; }

        protected string BarChartLabelsJSON { get; set; }

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

            btnCopyToClipboard.Visible = true;
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format(
@"new ClipboardJS('#{0}');
    $('#{0}').tooltip();
",
btnCopyToClipboard.ClientID );

            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

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
            if ( !IsPostBack )
            {
                LoadFilterFromUserPreferencesOrURL();

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
            if ( DoughnutChartDeclineValuesJSON.IsNullOrWhiteSpace() )
            {
                return;
            }

            int valLength = DoughnutChartDeclineValuesJSON.Split( ',' ).Length;
            string colors = "['#F3F3F3']";

            if ( DoughnutChartDeclineLabelsJSON != "['No data found']" )
            {
                colors = "['" + string.Join( "','", this.GetAttributeValue( "DeclineChartColors" ).Split( ',' ).Take( valLength ) ) + "']";
            }

            string script = string.Format(
@"
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
                colors );

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
            if ( BarChartMaxValue != null && BarChartMaxValue.Value < 9 )
            {
                steps = "ticks: { stepSize: 1 },";
            }

            string script = string.Format(
@"
var barCtx = $('#{0}')[0].getContext('2d');

var barChart = new Chart(barCtx, {{
    type: 'bar',
    data: {{
        labels: {1},
        datasets: [
        {{
            label: 'No Response',
            backgroundColor: '{2}',
            borderColor: '#E0E0E0',
            data: {3},
        }},
        {{
            label: 'Declines',
            backgroundColor: '{4}',
            borderColor: '#E0E0E0',
            data: {5}
        }},
        {{
            label: 'Attended',
            backgroundColor: '{6}',
            borderColor: '#E0E0E0',
            data: {7}
        }},
        {{
            label: 'Committed No Show',
            backgroundColor: '{8}',
            borderColor: '#E0E0E0',
            data: {9}
        }}]
    }},

    options: {{
        scales: {{
			xAxes: [{{
				stacked: true,
			}}],
			yAxes: [{{
                {10}
				stacked: true
			}}]
		}}
    }}
}});",
            barChartCanvas.ClientID,
            BarChartLabelsJSON,
            GetAttributeValue( AttributeKey.BarChartNoResponseColor ),
            BarChartNoResponseJSON,
            GetAttributeValue( AttributeKey.BarChartDeclinesColor ),
            BarChartDeclinesJSON,
            GetAttributeValue( AttributeKey.BarChartAttendedColor ),
            BarChartAttendedJSON,
            GetAttributeValue( AttributeKey.BarChartCommittedNoShowColor ),
            BarChartCommittedNoShowJSON,
            steps );

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
        /// Loads the filter from user preferences or the URL
        /// </summary>
        private void LoadFilterFromUserPreferencesOrURL()
        {

            sdrpDateRange.DelimitedValues = this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedDateRange );
            hfTabs.Value = this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedViewBy );
            gpGroups.GroupId = this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedGroupId ).AsIntegerOrNull();

            LoadLocations();
            LoadSchedules();

            cblLocations.SetValues( this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedLocationIds ).SplitDelimitedValues() );
            cblSchedules.SetValues( this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedScheduleIds ).SplitDelimitedValues() );
            dvDataViews.SetValue( this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedDataViewId ).AsIntegerOrNull() );
            Person selectedPerson = null;
            int? selectedPersonId = this.GetUrlSettingOrBlockUserPreference( UserPreferenceKey.SelectedPersonId ).AsIntegerOrNull();
            if ( selectedPersonId.HasValue )
            {
                selectedPerson = new PersonService( new RockContext() ).GetNoTracking( selectedPersonId.Value );
            }

            ppPerson.SetValue( selectedPerson );

            
        }

        /// <summary>
        /// Gets the URL setting (if there is one) or block user preference.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetUrlSettingOrBlockUserPreference( string key )
        {
            string setting = Request.QueryString[key];
            if ( setting != null )
            {
                return setting;
            }

            return this.GetBlockUserPreference( key );
        }

        /// <summary>
        /// Clears the existing data from class var attendances and repopulates it with data for the selected group and filter criteria.
        /// Data is organized by each person in the group.
        /// </summary>
        private List<Attendance> GetAttendanceData()
        {
            this.SetBlockUserPreference( UserPreferenceKey.SelectedDateRange, sdrpDateRange.DelimitedValues );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedViewBy, hfTabs.Value );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedGroupId, gpGroups.GroupId.ToString() );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedLocationIds, cblLocations.SelectedValues.AsDelimited( "," ) );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedScheduleIds, cblSchedules.SelectedValues.AsDelimited( "," ) );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedDataViewId, dvDataViews.SelectedValue );
            this.SetBlockUserPreference( UserPreferenceKey.SelectedPersonId, ppPerson.SelectedValue.ToString() );

            // Create URL for selected settings
            var pageReference = CurrentPageReference;
            foreach ( var setting in GetBlockUserPreferences() )
            {
                pageReference.Parameters.AddOrReplace( setting.Key, setting.Value );
            }

            Uri uri = new Uri( Request.Url.ToString() );
            btnCopyToClipboard.Attributes["data-clipboard-text"] = uri.GetLeftPart( UriPartial.Authority ) + pageReference.BuildUrl();
            btnCopyToClipboard.Disabled = false;

            // Source data for all tables and graphs
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var groupAttendances = attendanceService
                    .Queryable()
                    .Include( a => a.PersonAlias )
                    .AsNoTracking()
                    .Where( a => a.RequestedToAttend == true );

                switch ( hfTabs.Value )
                {
                    case "group":
                        groupAttendances = groupAttendances.Where( a => a.Occurrence.GroupId == gpGroups.GroupId );

                        // add selected locations to the query
                        if ( cblLocations.SelectedValues.Any() )
                        {
                            groupAttendances = groupAttendances.Where( a => cblLocations.SelectedValuesAsInt.Contains( a.Occurrence.LocationId ?? -1 ) );
                        }

                        // add selected schedules to the query
                        if ( cblSchedules.SelectedValues.Any() )
                        {
                            groupAttendances = groupAttendances.Where( a => cblSchedules.SelectedValuesAsInt.Contains( a.Occurrence.ScheduleId ?? -1 ) );
                        }

                        break;
                    case "person":
                        groupAttendances = groupAttendances.Where( a => a.PersonAlias.PersonId == ppPerson.PersonId );
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
                if ( sdrpDateRange.DelimitedValues.IsNotNullOrWhiteSpace() )
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

                return groupAttendances.ToList();
            }
        }

        /// <summary>
        /// Populates the locations checkbox list for the selected group.
        /// </summary>
        protected void LoadLocations()
        {
            using ( var rockContext = new RockContext() )
            {
                // keep any selected locations that exist for the currently selected group
                var selectedLocationIds = cblLocations.SelectedValuesAsInt;

                var groupLocationService = new GroupLocationService( rockContext );
                var groupLocationQuery = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gl => gl.Location.IsActive == true )
                    .Where( gl => gl.GroupId == gpGroups.GroupId )
                    .OrderBy( gl => gl.Order )
                    .ThenBy( gl => gl.Location.Name );

                var source = groupLocationQuery.Select( gl => new { Id = gl.LocationId, gl.Location.Name } ).ToList();

                cblLocations.Visible = true;
                cblLocations.DataValueField = "Id";
                cblLocations.DataTextField = "Name";
                cblLocations.DataSource = source;
                cblLocations.DataBind();

                cblLocations.SetValues( selectedLocationIds );
            }
        }

        /// <summary>
        /// Populates the schedules checkbox list for the group
        /// </summary>
        protected void LoadSchedules()
        {
            using ( var rockContext = new RockContext() )
            {
                // keep any selected schedules that exist for the currently selected locations
                var selectedScheduleIds = cblSchedules.SelectedValuesAsInt;

                var groupLocationService = new GroupLocationService( rockContext );
                var groupLocations = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gl => gl.GroupId == gpGroups.GroupId );

                if ( cblLocations.SelectedValues.Any() )
                {
                    groupLocations = groupLocations.Where( gl => cblLocations.SelectedValuesAsInt.Contains( gl.LocationId ) );
                }

                var groupSchedules = groupLocations.SelectMany( a => a.Schedules ).DistinctBy( a => a.Guid ).ToList();

                List<Schedule> sortedScheduleList = groupSchedules.OrderByNextScheduledDateTime();

                cblSchedules.Visible = sortedScheduleList.Any();

                if ( sortedScheduleList.Any() )
                {
                    cblSchedules.Visible = true;
                    cblSchedules.DataValueField = "Id";
                    cblSchedules.DataTextField = "Name";
                    cblSchedules.DataSource = sortedScheduleList;
                    cblSchedules.DataBind();
                }

                cblSchedules.SetValues( selectedScheduleIds );
            }
        }

        /// <summary>
        /// Shows the bar graph.
        /// </summary>
        /// <param name="attendances">The attendances.</param>
        protected void ShowBarGraph( List<Attendance> attendances )
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
                CreateBarChartGroupedByMonth( firstDateTime, lastDateTime, attendances );
            }
            else if ( daysCount > 31 )
            {
                // if more than 1 month summarize by week
                CreateBarChartGroupedByWeek( daysCount, firstDateTime, attendances );
            }
            else
            {
                // Otherwise summarize by day
                CreateBarChartGroupedByDay( daysCount, firstDateTime, attendances );
            }
        }

        /// <summary>
        /// Creates the bar chart with data grouped by month.
        /// </summary>
        /// <param name="firstDateTime">The first date time.</param>
        /// <param name="lastDateTime">The last date time.</param>
        /// <param name="attendances">The attendances.</param>
        protected void CreateBarChartGroupedByMonth( DateTime firstDateTime, DateTime lastDateTime, List<Attendance> attendances )
        {
            List<SchedulerSummaryData> barchartdata = attendances
                .GroupBy( a => new { StartYear = a.StartDateTime.Year, StartMonth = a.StartDateTime.Month } )
                .Select( a => new SchedulerSummaryData( new DateTime( a.Key.StartYear, a.Key.StartMonth, 1 ), a.ToList() ) )
                .ToList();

            var monthsCount = ( ( lastDateTime.Year - firstDateTime.Year ) * 12 ) + ( lastDateTime.Month - firstDateTime.Month ) + 1;
            var months = Enumerable.Range( 0, monthsCount )
                .Select( x => new
                {
                    year = firstDateTime.AddMonths( x ).Year,
                    month = firstDateTime.AddMonths( x ).Month
                } );

            var groupedByMonth = months
                .GroupJoin(
                    barchartdata,
                    m => new { m.month, m.year },
                    a => new { month = a.StartDateTime.Month, year = a.StartDateTime.Year },
                    ( g, d ) => new
                    {
                        Month = g.month,
                        Year = g.year,
                        Scheduled = d.Sum( a => a.ScheduledCount ),
                        NoResponse = d.Sum( a => a.NoResponseCount ),
                        Declines = d.Sum( a => a.DeclineCount ),
                        Attended = d.Sum( a => a.AttendedCount ),
                        CommittedNoShow = d.Sum( a => a.CommittedNoShowCount ),
                        Total = d.Sum( a => a.ScheduledCount ) + d.Sum( a => a.NoResponseCount ) + d.Sum( a => a.DeclineCount ) + d.Sum( a => a.AttendedCount ) + d.Sum( a => a.CommittedNoShowCount )
                    } );

            this.BarChartLabelsJSON = "['" + groupedByMonth
                .OrderBy( a => a.Year )
                .ThenBy( a => a.Month )
                .Select( a => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName( a.Month ) + " " + a.Year )
                .ToList()
                .AsDelimited( "','" ) + "']";

            nbNoData.Visible = !barchartdata.Any();

            BarChartMaxValue = groupedByMonth.Max( x => x.Total );
            BarChartNoResponseJSON = groupedByMonth.OrderBy( a => a.Year ).ThenBy( a => a.Month ).Select( d => d.NoResponse ).ToJson();
            BarChartDeclinesJSON = groupedByMonth.OrderBy( a => a.Year ).ThenBy( a => a.Month ).Select( d => d.Declines ).ToJson();
            BarChartAttendedJSON = groupedByMonth.OrderBy( a => a.Year ).ThenBy( a => a.Month ).Select( d => d.Attended ).ToJson();
            BarChartCommittedNoShowJSON = groupedByMonth.OrderBy( a => a.Year ).ThenBy( a => a.Month ).Select( d => d.CommittedNoShow ).ToJson();
        }

        /// <summary>
        /// Creates the bar chart with data grouped by week.
        /// </summary>
        /// <param name="daysCount">The days count.</param>
        /// <param name="firstDateTime">The first date time.</param>
        /// <param name="attendances">The attendances.</param>
        protected void CreateBarChartGroupedByWeek( int daysCount, DateTime firstDateTime, List<Attendance> attendances )
        {
            List<SchedulerSummaryData> barchartdata = attendances
                .GroupBy( a => new { StartWeek = a.StartDateTime.StartOfWeek( DayOfWeek.Monday ) } )
                .Select( a => new SchedulerSummaryData( a.Key.StartWeek, a.ToList() ) )
                .ToList();

            var weeks = Enumerable.Range( 0, ( int ) Math.Ceiling( ( daysCount / 7.0 ) + 1 ) )
                .Select( x => new
                {
                    date = firstDateTime.StartOfWeek( DayOfWeek.Monday ).AddDays( x * 7 )
                } );

            var groupedByDate = weeks
                .GroupJoin(
                    barchartdata,
                    m => new { m.date },
                    a => new { date = a.StartDateTime.Date },
                    ( g, d ) => new
                    {
                        Date = g.date,
                        Scheduled = d.Sum( a => a.ScheduledCount ),
                        NoResponse = d.Sum( a => a.NoResponseCount ),
                        Declines = d.Sum( a => a.DeclineCount ),
                        Attended = d.Sum( a => a.AttendedCount ),
                        CommittedNoShow = d.Sum( a => a.CommittedNoShowCount ),
                        Total = d.Sum( a => a.ScheduledCount ) + d.Sum( a => a.NoResponseCount ) + d.Sum( a => a.DeclineCount ) + d.Sum( a => a.AttendedCount ) + d.Sum( a => a.CommittedNoShowCount )
                    } );

            this.BarChartLabelsJSON = "['" + groupedByDate
                .OrderBy( a => a.Date )
                .Select( a => a.Date.ToShortDateString() )
                .ToList()
                .AsDelimited( "','" ) + "']";

            nbNoData.Visible = !barchartdata.Any();

            BarChartMaxValue = groupedByDate.Max( x => x.Total );
            BarChartNoResponseJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.NoResponse ).ToJson();
            BarChartDeclinesJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.Declines ).ToJson();
            BarChartAttendedJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.Attended ).ToJson();
            BarChartCommittedNoShowJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.CommittedNoShow ).ToJson();
        }

        /// <summary>
        /// Creates the bar chart with data grouped by day.
        /// </summary>
        /// <param name="daysCount">The days count.</param>
        /// <param name="firstDateTime">The first date time.</param>
        /// <param name="attendances">The attendances.</param>
        protected void CreateBarChartGroupedByDay( int daysCount, DateTime firstDateTime, List<Attendance> attendances )
        {
            List<SchedulerSummaryData> barchartdata = attendances
                .GroupBy( a => new { StartYear = a.StartDateTime.Year, StartMonth = a.StartDateTime.Month, StartDay = a.StartDateTime.Day } )
                .Select( a => new SchedulerSummaryData( new DateTime( a.Key.StartYear, a.Key.StartMonth, a.Key.StartDay ), a.ToList() ) )
                .ToList();

            var days = Enumerable.Range( 0, daysCount + 1 )
                .Select( x => new
                {
                    date = firstDateTime.AddDays( x )
                } );

            var groupedByDate = days
                .GroupJoin(
                    barchartdata,
                    m => new { m.date },
                    a => new { date = a.StartDateTime.Date },
                    ( g, d ) => new
                    {
                        Date = g.date,
                        Scheduled = d.Sum( a => a.ScheduledCount ),
                        NoResponse = d.Sum( a => a.NoResponseCount ),
                        Declines = d.Sum( a => a.DeclineCount ),
                        Attended = d.Sum( a => a.AttendedCount ),
                        CommittedNoShow = d.Sum( a => a.CommittedNoShowCount ),
                        Total = d.Sum( a => a.ScheduledCount ) + d.Sum( a => a.NoResponseCount ) + d.Sum( a => a.DeclineCount ) + d.Sum( a => a.AttendedCount ) + d.Sum( a => a.CommittedNoShowCount )
                    } );

            this.BarChartLabelsJSON = "['" + groupedByDate
                .OrderBy( a => a.Date )
                .Select( a => a.Date.ToShortDateString() )
                .ToList()
                .AsDelimited( "','" ) + "']";

            nbNoData.Visible = !barchartdata.Any();

            BarChartMaxValue = groupedByDate.Max( x => x.Total );
            BarChartNoResponseJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.NoResponse ).ToJson();
            BarChartDeclinesJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.Declines ).ToJson();
            BarChartAttendedJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.Attended ).ToJson();
            BarChartCommittedNoShowJSON = groupedByDate.OrderBy( a => a.Date ).Select( d => d.CommittedNoShow ).ToJson();
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
        /// <param name="attendances">The attendances.</param>
        protected void ShowDoughnutGraph( List<Attendance> attendances )
        {
            if ( !attendances.Any() )
            {
                return;
            }

            var declines = attendances.Where( a => a.IsScheduledPersonDeclined() ).GroupBy( a => a.DeclineReasonValueId ).Select( a => new { Reason = a.Key, Count = a.Count() } );

            if ( declines.Any() )
            {
                DoughnutChartDeclineLabelsJSON = "['" + declines
                .OrderByDescending( d => d.Count )
                .Select( d => d.Reason.HasValue ? DefinedValueCache.Get( d.Reason.Value ).Value : "Declined without reason" )
                .ToList()
                .AsDelimited( "','" ) + "']";

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
        /// <param name="attendances">The attendances.</param>
        protected void ShowGrid( List<Attendance> attendances )
        {
            gData.Visible = true;
            var schedulerSummaryDataList = new List<SchedulerSummaryData>();

            using ( var rockContext = new RockContext() )
            {
                var attendancesPersonIds = attendances.Where( a => a.PersonAlias != null ).Select( a => a.PersonAlias.PersonId ).Distinct().ToList();
                var personList = new PersonService( rockContext ).Queryable().Where( a => attendancesPersonIds.Contains( a.Id ) ).AsNoTracking()
                    .ToList()
                    .OrderBy( a => a.FullName ).ToList();

                var attendancesByPersonLookup = attendances.Where( a => a.PersonAlias != null ).GroupBy( a => a.PersonAlias.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );

                foreach ( var person in personList )
                {
                    var personAttendances = attendancesByPersonLookup.GetValueOrNull( person.Id ) ?? new List<Attendance>();
                    var schedulerGroupMember = new SchedulerSummaryData( person, personAttendances );
                    schedulerSummaryDataList.Add( schedulerGroupMember );
                }
            }

            if (gData.SortProperty != null)
            {
                schedulerSummaryDataList = schedulerSummaryDataList.AsQueryable().Sort( gData.SortProperty ).ToList();
            }

            gData.DataSource = schedulerSummaryDataList;
            gData.DataBind();
        }

        #region Control Events

        /// <summary>
        /// Handles the SelectItem event of the gpGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroups_SelectItem( object sender, EventArgs e )
        {
            if ( !ValidateFilter() )
            {
                return;
            }

            LoadLocations();
            LoadSchedules();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( !ValidateFilter() )
            {
                return;
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the dvDataViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvDataViews_SelectItem( object sender, EventArgs e )
        {
            if ( !ValidateFilter() )
            {
                return;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            nbBarChartMessage.Visible = false;

            if ( !ValidateFilter() )
            {
                return;
            }

            ShowData();
        }

        /// <summary>
        /// Shows the data.
        /// </summary>
        private void ShowData()
        {
            var attendances = GetAttendanceData();
            ShowGrid( attendances );
            ShowBarGraph( attendances );
            ShowDoughnutGraph( attendances );
            RegisterChartScripts();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblLocations_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadSchedules();
        }

        #endregion Control Events

        protected class SchedulerSummaryData
        {
            public SchedulerSummaryData( DateTime startDateTime, List<Attendance> attendances )
            {
                this.StartDateTime = startDateTime;
                SetCountFields( attendances );
            }

            public SchedulerSummaryData( Person person, List<Attendance> attendances )
            {
                Name = person.FullName;
                SetCountFields( attendances );
            }

            private void SetCountFields( List<Attendance> attendances )
            {
                ScheduledCount = attendances.Count();
                NoResponseCount = attendances.Count( aa => aa.RSVP == RSVP.Unknown & aa.DidAttend == false );
                DeclineCount = attendances.Count( aa => aa.RSVP == RSVP.No & aa.DidAttend == false );
                AttendedCount = attendances.Count( aa => aa.DidAttend == true );
                CommittedNoShowCount = attendances.Count( aa => aa.RSVP == RSVP.Yes && aa.DidAttend == false );
            }

            public string Name { get; private set; }

            public DateTime StartDateTime { get; private set; }

            public int ScheduledCount { get; private set; }
            public string ScheduledCountText
            {
                get
                {
                    return ScheduledCount == 0 ? "-" : ScheduledCount.ToString();
                }
            }

            public int NoResponseCount { get; private set; }
            public string NoResponseCountText
            {
                get
                {
                    return NoResponseCount == 0 ? "-" : NoResponseCount.ToString();
                }
            }

            public int DeclineCount { get; private set; }
            public string DeclineCountText
            {
                get
                {
                    return DeclineCount == 0 ? "-" : DeclineCount.ToString();
                }
            }

            public int AttendedCount { get; private set; }
            public string AttendedCountText
            {
                get
                {
                    return AttendedCount == 0 ? "-" : AttendedCount.ToString();
                }
            }

            public int CommittedNoShowCount { get; private set; }
            public string CommittedNoShowCountText
            {
                get
                {
                    return CommittedNoShowCount == 0 ? "-" : CommittedNoShowCount.ToString();
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gData_GridRebind( object sender, GridRebindEventArgs e )
        {
            ShowData();
        }
    }
}