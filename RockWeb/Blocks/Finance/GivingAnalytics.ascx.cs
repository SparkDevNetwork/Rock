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

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Shows a graph of attendance statistics which can be configured for specific groups, date range, etc.
    /// </summary>
    [DisplayName( "Giving Analysis" )]
    [Category( "Finance" )]
    [Description( "Shows a graph of giving statistics which can be configured for specific date range, amounts, currency types, campus, etc." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked" )]
    public partial class GivingAnalytics : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;

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

            gChartAmount.GridRebind += gChartAmount_GridRebind;
            gGiversGifts.GridRebind += gGiversGifts_GridRebind;

            _rockContext = new RockContext();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGiversGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGiversGifts_GridRebind( object sender, EventArgs e )
        {
            BindGiversGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gChartAmount_GridRebind( object sender, EventArgs e )
        {
            BindChartAmountGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var chartStyleDefinedValueGuid = this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();

            lcAmount.Options.SetChartStyle( chartStyleDefinedValueGuid );

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
            LoadChartAndGrids();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            cblCurrencyTypes.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ) );
            cblTransactionSource.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ) );
            cpCampuses.Campuses = CampusCache.All();
        }

        /// <summary>
        /// Loads the chart and any visible grids
        /// </summary>
        public void LoadChartAndGrids()
        {
            lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );
            
            lcAmount.ShowTooltip = true;
            if ( this.DetailPageGuid.HasValue )
            {
                lcAmount.ChartClick += lcAmount_ChartClick;
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

            if ( nreAmount.LowerValue.HasValue )
            {
                dataSourceParams.AddOrReplace( "minAmount", nreAmount.LowerValue.Value.ToString() );
            }

            if ( nreAmount.UpperValue.HasValue )
            {
                dataSourceParams.AddOrReplace( "maxAmount", nreAmount.UpperValue.Value.ToString() );
            }

            var selectedCurrencyTypeIds = cblCurrencyTypes.SelectedValuesAsInt;
            if ( selectedCurrencyTypeIds.Any() )
            {
                dataSourceParams.AddOrReplace( "currencyTypeIds", selectedCurrencyTypeIds.AsDelimited( "," ) );
            }

            var selectedTxnSourceIds = cblTransactionSource.SelectedValuesAsInt;
            if ( selectedCurrencyTypeIds.Any() )
            {
                dataSourceParams.AddOrReplace( "cblTransactionSource", selectedTxnSourceIds.AsDelimited( "," ) );
            }

            if ( cpCampuses.SelectedCampusIds.Any() )
            {
                dataSourceParams.AddOrReplace( "campusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            }

            var selectedDataViewId = dvpDataView.SelectedValue.AsIntegerOrNull();
            if ( selectedDataViewId.HasValue )
            {
                dataSourceParams.AddOrReplace( "dataViewId", selectedDataViewId.Value.ToString() );
            }

            dataSourceParams.AddOrReplace( "dataViewInclude", rblDataViewAction.SelectedValue );

            dataSourceParams.AddOrReplace( "graphBy", hfGraphBy.Value.AsInteger() );

            SaveSettingsToUserPreferences();

            dataSourceUrl += "?" + dataSourceParams.Select( s => string.Format( "{0}={1}", s.Key, s.Value ) ).ToList().AsDelimited( "&" );

            lcAmount.DataSourceUrl = this.ResolveUrl( dataSourceUrl );

            if ( pnlChartAmountGrid.Visible )
            {
                BindChartAmountGrid();
            }

            if ( pnlDetails.Visible )
            {
                BindGiversGrid();
            }
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettingsToUserPreferences()
        {
            string keyPrefix = string.Format( "giving-analytics-{0}-", this.BlockId );

            this.SetUserPreference( keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues );
            this.SetUserPreference( keyPrefix + "AmountRange", nreAmount.DelimitedValues );
            this.SetUserPreference( keyPrefix + "CurrencyTypesIds", cblCurrencyTypes.SelectedValues.AsDelimited(",") );
            this.SetUserPreference( keyPrefix + "SourcesIds", cblTransactionSource.SelectedValues.AsDelimited( "," ) );
            this.SetUserPreference( keyPrefix + "CampusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            this.SetUserPreference( keyPrefix + "DataView", dvpDataView.SelectedValue );
            this.SetUserPreference( keyPrefix + "DataViewAction", rblDataViewAction.SelectedValue );

            this.SetUserPreference( keyPrefix + "GraphBy", hfGraphBy.Value );

            this.SetUserPreference( keyPrefix + "GroupIds", selectedGroupIds.AsDelimited( "," ) );

            this.SetUserPreference( keyPrefix + "ShowBy", hfShowBy.Value );

            this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value );

            GiversFilterBy giversFilterBy;
            if ( radFirstTime.Checked )
            {
                giversFilterBy = GiversFilterBy.FirstTime;
            }
            else if ( radByPattern.Checked )
            {
                giversFilterBy = GiversFilterBy.Pattern;
            }
            else
            {
                giversFilterBy = GiversFilterBy.All;
            }

            this.SetUserPreference( keyPrefix + "GiversFilterByType", giversFilterBy.ConvertToInt().ToString() );
            this.SetUserPreference( keyPrefix + "GiversFilterByPattern", string.Format( "{0}|{1}|{2}|{3}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, tbPatternMissedXTimes.Text, drpPatternDateRange.DelimitedValues ) );
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettingsFromUserPreferences()
        {
            string keyPrefix = string.Format( "giving-analytics-{0}-", this.BlockId );

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

            nreAmount.DelimitedValues = this.GetUserPreference( keyPrefix + "AmountRange" );

            cblCurrencyTypes.SetValues()
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

            GiversFilterBy attendeesFilterBy = this.GetUserPreference( keyPrefix + "AttendeesFilterByType" ).ConvertToEnumOrNull<GiversFilterBy>() ?? GiversFilterBy.All;

            switch ( attendeesFilterBy )
            {
                case GiversFilterBy.All:
                    radAllGivers.Checked = true;
                    break;
                case GiversFilterBy.ByVisit:
                    radByVisit.Checked = true;
                    break;
                case GiversFilterBy.Pattern:
                    radByPattern.Checked = true;
                    break;
                default:
                    radAllGivers.Checked = true;
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
        protected void lcAmount_ChartClick( object sender, ChartClickArgs e )
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
        protected void lShowChartAmountGrid_Click( object sender, EventArgs e )
        {
            if ( pnlChartAmountGrid.Visible )
            {
                pnlChartAmountGrid.Visible = false;
                lShowChartAmountGrid.Text = "Show Data <i class='fa fa-chevron-down'></i>";
                lShowChartAmountGrid.ToolTip = "Show Data";
            }
            else
            {
                pnlChartAmountGrid.Visible = true;
                lShowChartAmountGrid.Text = "Hide Data <i class='fa fa-chevron-up'></i>";
                lShowChartAmountGrid.ToolTip = "Hide Data";
                BindChartAmountGrid();
            }
        }

        /// <summary>
        /// Binds the chart attendance grid.
        /// </summary>
        private void BindChartAmountGrid()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            string groupIds = GetSelectedGroupIds().AsDelimited( "," );
            string campusIds = cpCampuses.SelectedCampusIds.AsDelimited( "," );

            SortProperty sortProperty = gChartAmount.SortProperty;

            var chartData = new AttendanceService( _rockContext ).GetChartData(
                hfGroupBy.Value.ConvertToEnumOrNull<AttendanceGroupBy>() ?? AttendanceGroupBy.Week,
                hfGraphBy.Value.ConvertToEnumOrNull<AttendanceGraphBy>() ?? AttendanceGraphBy.Total,
                dateRange.Start,
                dateRange.End,
                groupIds,
                campusIds );

            if ( sortProperty != null )
            {
                gChartAmount.DataSource = chartData.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gChartAmount.DataSource = chartData.OrderBy( a => a.DateTimeStamp ).ToList();
            }

            gChartAmount.DataBind();
        }

        private List<DateTime> _possibleAttendances = null;
        private Dictionary<int, string> _scheduleNameLookup = null;

        /// <summary>
        /// Binds the attendees grid.
        /// </summary>
        private void BindGiversGrid()
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
                // only return attendees where their lastvisit was their nth Visit
                int skipCount = byNthVisit.Value - 1;
                qryResult = qryResult.Where( a => a.LastVisit.Id == a.FirstVisits.OrderBy( x => x.StartDateTime ).Skip( skipCount ).Select( b => b.Id ).FirstOrDefault() );
            }

            if ( attendedMinCount.HasValue )
            {
                qryResult = qryResult.Where( a => a.AttendanceSummary.Count() >= attendedMinCount );
            }

            double? attendedMissedPossible = null;
            if ( attendedMissedCount.HasValue )
            {
                if ( attendedMissedDateRange.Start.HasValue && attendedMissedDateRange.End.HasValue )
                {
                    attendedMissedPossible = Math.Ceiling( ( attendedMissedDateRange.End.Value - attendedMissedDateRange.Start.Value ).TotalDays / 7 );
                    qryMissed = qryMissed.Where( a => a.StartDateTime >= attendedMissedDateRange.Start.Value && a.StartDateTime < attendedMissedDateRange.End.Value );
                    var qryMissedByPerson = qryMissed.GroupBy( a => a.PersonAlias.PersonId ).Select( a => new
                    {
                        PersonId = a.Key,
                        AttendanceCount = a.Count()
                    } ).Where( x => ( attendedMissedPossible - x.AttendanceCount ) >= attendedMissedCount );

                    // filter to only people that missed at least X weeks between specified missed date range
                    qryResult = qryResult.Where( a => qryMissedByPerson.Any( b => b.PersonId == a.PersonId ) );
                }
            }

            SortProperty sortProperty = gGiversGifts.SortProperty;

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

            var attendancePercentField = gGiversGifts.Columns.OfType<RockTemplateField>().First( a => a.HeaderText.EndsWith( "Attendance %" ) );
            attendancePercentField.HeaderText = string.Format( "{0}ly Attendance %", groupBy.ConvertToString() );

            var includeParents = hfViewBy.Value.ConvertToEnumOrNull<ViewBy>().GetValueOrDefault( ViewBy.Attendees ) == ViewBy.ParentsOfAttendees;
            var parentField = gGiversGifts.Columns.OfType<PersonField>().FirstOrDefault( a => a.HeaderText == "Parent" );
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

                gGiversGifts.PersonIdField = "ParentId";
                gGiversGifts.DataKeyNames = new string[] { "ParentId", "PersonId" };
                qryFinalResult = qryResultWithParent;
            }
            else
            {
                gGiversGifts.PersonIdField = "PersonId";
                gGiversGifts.DataKeyNames = new string[] { "PersonId" };
                qryFinalResult = qryResult;
            }

            // Create the dynamic attendance grid columns as needed
            CreateDynamicAttendanceGridColumns();

            try
            {
                nbGiversError.Visible = false;

                // increase the timeout from 30 to 90. The Query can be slow if SQL hasn't calculated the Query Plan for the query yet. 
                // Most of the time consumption is figuring out the Query Plan, but after it figures it out, it caches it so that the next time it'll be much faster
                rockContext.Database.CommandTimeout = 90;
                gGiversGifts.DataSource = qryFinalResult.AsNoTracking().ToList();

                gGiversGifts.DataBind();
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

                nbGiversError.Text = errorMessage;
                nbGiversError.Details = stackTrace;
                nbGiversError.Visible = true;
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
            var attendanceSummaryFields = gGiversGifts.Columns.OfType<BoolFromArrayField<DateTime>>().Where( a => a.DataField == "AttendanceSummary" ).ToList();
            var existingSummaryDates = attendanceSummaryFields.Select( a => a.ArrayKey ).ToList();

            if ( existingSummaryDates.Any( a => !_possibleAttendances.Contains( a ) ) || _possibleAttendances.Any( a => !existingSummaryDates.Contains( a ) ) )
            {
                foreach ( var oldField in attendanceSummaryFields.Reverse<BoolFromArrayField<DateTime>>() )
                {
                    // remove all these fields if they have changed
                    gGiversGifts.Columns.Remove( oldField );
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

                    gGiversGifts.Columns.Add( boolFromArrayField );
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
            foreach ( var checkmarkedAttendanceField in gGiversGifts.Columns.OfType<CallbackField>() )
            {
                gGiversGifts.Columns.Remove( checkmarkedAttendanceField );
            }

            TimeSpan dateRangeSpan = dateRange.End.Value - dateRange.Start.Value;

            _possibleAttendances = new List<DateTime>();

            if ( attendanceGroupBy == AttendanceGroupBy.Week )
            {
                var endOfFirstWeek = dateRange.Start.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
                var endOfLastWeek = dateRange.End.Value.EndOfWeek( RockDateTime.FirstDayOfWeek );
                var weekEndDate = endOfFirstWeek;
                while ( weekEndDate <= endOfLastWeek )
                {
                    // Weeks are summarized as the last day of the "Rock" week (Sunday)
                    _possibleAttendances.Add( weekEndDate );
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
                    _possibleAttendances.Add( monthStartDate );
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
                    _possibleAttendances.Add( yearStartDate );
                    yearStartDate = yearStartDate.AddYears( 1 );
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGiversGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGiversGifts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                Literal lFirstVisitDate = e.Row.FindControl( "lFirstVisitDate" ) as Literal;
                if ( lFirstVisitDate == null )
                {
                    // Since we have dynamic columns, the templatefields might not get created due some viewstate thingy
                    // so, if we lost the templatefield, force them to instantiate
                    var templateFields = gGiversGifts.Columns.OfType<TemplateField>();
                    foreach ( var templateField in templateFields )
                    {
                        var cellIndex = gGiversGifts.Columns.IndexOf( templateField );
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
                        AddGroupControls( group, cblGroupTypeGroups, groupService );
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
                        liChildGroupTypeItem.ID = "liGroupTypeItem" + groupType.Id;
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
        private void AddGroupControls( Group group, RockCheckBoxList checkBoxList, GroupService service )
        {
            // Only show groups that actually have a schedule
            if ( group != null )
            {
                if ( group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
                {
                    checkBoxList.Items.Add( new ListItem( service.GroupAncestorPathName( group.Id ), group.Id.ToString() ) );
                }

                if ( group.Groups != null )
                {
                    foreach ( var childGroup in group.Groups
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        AddGroupControls( childGroup, checkBoxList, service );
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
        private enum GiversFilterBy
        {
            /// <summary>
            /// All Attendees
            /// </summary>
            All = 0,

            /// <summary>
            /// By First Time
            /// </summary>
            FirstTime = 1,

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
            pnlChart.Visible = showBy == ShowBy.Chart;
            pnlDetails.Visible = showBy == ShowBy.Attendees;
        }

        /// <summary>
        /// Handles the Click event of the btnShowDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowDetails_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Attendees );
            BindGiversGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnShowChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowChart_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Chart );
            BindChartAmountGrid();
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
        /// Handles the Click event of the btnApplyGiversFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyGiversFilter_Click( object sender, EventArgs e )
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