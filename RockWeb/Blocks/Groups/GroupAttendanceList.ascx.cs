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


using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Attendance List" )]
    [Category( "Groups" )]
    [Description( "Lists all the scheduled occurrences for a given group." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [BooleanField( "Allow Add", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", true, "", 1 )]
    public partial class GroupAttendanceList : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canView = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _rockContext = new RockContext();

            int groupId = PageParameter( "GroupId" ).AsInteger();
            _group = new GroupService( _rockContext )
                .Queryable( "GroupLocations" ).AsNoTracking()
                .FirstOrDefault( g => g.Id == groupId );

            if ( _group != null && _group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                _group.LoadAttributes( _rockContext );
                _canView = true;

                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                gOccurrences.DataKeyNames = new string[] { "Date", "ScheduleId", "LocationId" };
                gOccurrences.Actions.AddClick += gOccurrences_Add;
                gOccurrences.GridRebind += gOccurrences_GridRebind;

                // make sure they have Auth to edit the block OR edit to the Group
                bool canEditBlock = IsUserAuthorized( Authorization.EDIT ) || _group.IsAuthorized( Authorization.EDIT, this.CurrentPerson );
                gOccurrences.Actions.ShowAdd = canEditBlock && GetAttributeValue( "AllowAdd" ).AsBoolean();
                gOccurrences.IsDeleteEnabled = canEditBlock;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlContent.Visible = _canView;

            if ( !Page.IsPostBack && _canView )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( "Date Range" ), "Date Range", drpDates.DelimitedValues );
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( "Schedule" ), "Schedule", ddlSchedule.SelectedValue );
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( "Location" ), "Location", ddlLocation.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == MakeKeyUniqueToGroup( "Date Range" ) )
            {
                // show the date range pickers current value, instead of the user preference since we set it to a default value if blank
                e.Value = DateRangePicker.FormatDelimitedValues( drpDates.DelimitedValues );
            }
            else if ( e.Key == MakeKeyUniqueToGroup( "Schedule" ) )
            {
                int scheduleId = e.Value.AsInteger();
                if ( scheduleId != 0 )
                {
                    var schedule = new ScheduleService( _rockContext ).Get( scheduleId );
                    e.Value = schedule != null ? schedule.Name : string.Empty;
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else if ( e.Key == MakeKeyUniqueToGroup( "Location" ) )
            {
                string locId = e.Value;
                if ( locId.StartsWith( "P" ) )
                {
                    locId = locId.Substring( 1 );
                }

                int locationId = locId.AsInteger();
                e.Value = new LocationService( _rockContext ).GetPath( locationId );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Edit event of the gOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gOccurrences_Edit( object sender, RowEventArgs e )
        {
            // The iCalendar date format is returned as UTC kind date, so we need to manually format it instead of using 'o'
            string occurrenceDate = ( (DateTime)e.RowKeyValues["Date"] ).ToString( "yyyy-MM-ddTHH:mm:ss" );
            var qryParams = new Dictionary<string, string> { 
                { "GroupId", _group.Id.ToString() },
                { "Date", occurrenceDate },
            };

            var locationId = e.RowKeyValues["LocationId"] as int?;
            if ( locationId.HasValue )
            {
                qryParams.Add( "LocationId", locationId.Value.ToString() );
            }

            var scheduleId = e.RowKeyValues["ScheduleId"] as int?;
            if ( scheduleId.HasValue )
            {
                qryParams.Add( "ScheduleId", scheduleId.Value.ToString() );
            }

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Add event of the gOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gOccurrences_Add( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string> { 
                { "GroupId", _group.Id.ToString() } 
            };

            if ( ddlSchedule.Visible && ddlSchedule.SelectedValue != "0" )
            {
                qryParams.Add( "ScheduleId", ddlSchedule.SelectedValue );
            }

            if ( ddlLocation.Visible )
            {
                int? locId = ddlLocation.SelectedValueAsInt();
                if ( locId.HasValue && locId.Value != 0 )
                {
                    qryParams.Add( "LocationId", locId.Value.ToString() );
                }
            }

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the GridRebind event of the gOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gOccurrences_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            string dateRangePreference = rFilter.GetUserPreference( MakeKeyUniqueToGroup( "Date Range" ) );
            if ( string.IsNullOrWhiteSpace( dateRangePreference ) )
            {
                // set the dateRangePreference to force rFilter_DisplayFilterValue to show our default one year limit
                dateRangePreference = ",";
                rFilter.SaveUserPreference( MakeKeyUniqueToGroup( "Date Range" ), "Date Range", dateRangePreference );
            }

            var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( dateRangePreference );

            // if there is no start date, default to a year ago to minimize the chance of loading too much data
            drpDates.LowerValue = dateRange.Start ?? RockDateTime.Today.AddYears( -1 );
            drpDates.UpperValue = dateRange.End;

            if ( _group != null )
            {
                var grouplocations = _group.GroupLocations
                    .Where( l =>
                        l.Location.Name != null &&
                        l.Location.Name != string.Empty )
                    .ToList();

                var locations = new Dictionary<string, string> { { string.Empty, string.Empty } };

                var locationService = new LocationService( _rockContext );
                foreach ( var location in grouplocations.Select( l => l.Location ) )
                {
                    if ( !locations.ContainsKey( location.Id.ToString() ) )
                    {
                        locations.Add( location.Id.ToString(), locationService.GetPath( location.Id ) );
                    }

                    var parentLocation = location.ParentLocation;
                    while ( parentLocation != null )
                    {
                        string key = string.Format( "P{0}", parentLocation.Id );
                        if ( !locations.ContainsKey( key ) )
                        {
                            locations.Add( key, locationService.GetPath( parentLocation.Id ) );
                        }

                        parentLocation = parentLocation.ParentLocation;
                    }
                }

                if ( locations.Any() )
                {
                    ddlLocation.Visible = true;
                    gOccurrences.Columns[2].Visible = true;
                    ddlLocation.DataSource = locations.OrderBy( l => l.Value );
                    ddlLocation.DataBind();
                    ddlLocation.SetValue( rFilter.GetUserPreference( MakeKeyUniqueToGroup( "Location" ) ) );
                }
                else
                {
                    ddlLocation.Visible = false;
                    gOccurrences.Columns[2].Visible = false;
                }

                var schedules = new Dictionary<int, string> { { 0, string.Empty } };
                grouplocations.SelectMany( l => l.Schedules ).OrderBy( s => s.Name ).ToList()
                    .ForEach( s => schedules.AddOrIgnore( s.Id, s.Name ) );

                if ( schedules.Any() )
                {
                    ddlSchedule.Visible = true;
                    gOccurrences.Columns[1].Visible = true;
                    ddlSchedule.DataSource = schedules;
                    ddlSchedule.DataBind();
                    ddlSchedule.SetValue( rFilter.GetUserPreference( MakeKeyUniqueToGroup( "Schedule" ) ) );
                }
                else
                {
                    ddlSchedule.Visible = false;
                    gOccurrences.Columns[1].Visible = false;
                }
            }
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGrid()
        {
            if ( _group != null )
            {
                lHeading.Text = _group.Name;

                DateTime? fromDateTime = drpDates.LowerValue;
                DateTime? toDateTime = drpDates.UpperValue;
                List<int> locationIds = new List<int>();
                List<int> scheduleIds = new List<int>();

                // Location Filter
                if ( ddlLocation.Visible )
                {
                    string locValue = ddlLocation.SelectedValue;
                    if ( locValue.StartsWith( "P" ) )
                    {
                        int? parentLocationId = locValue.Substring( 1 ).AsIntegerOrNull();
                        if ( parentLocationId.HasValue )
                        {
                            locationIds = new LocationService( _rockContext )
                                .GetAllDescendents( parentLocationId.Value )
                                .Select( l => l.Id )
                                .ToList();
                        }
                    }
                    else
                    {
                        int? locationId = locValue.AsIntegerOrNull();
                        if ( locationId.HasValue )
                        {
                            locationIds.Add( locationId.Value );
                        }
                    }
                }

                // Schedule Filter
                if ( ddlSchedule.Visible && ddlSchedule.SelectedValue != "0" )
                {
                    scheduleIds.Add( ddlSchedule.SelectedValueAsInt() ?? 0 );
                }

                var qry = new ScheduleService( _rockContext ).GetGroupOccurrences( _group, fromDateTime, toDateTime, locationIds, scheduleIds, true ).AsQueryable();

                SortProperty sortProperty = gOccurrences.SortProperty;
                List<ScheduleOccurrence> occurrences = null;
                if ( sortProperty != null )
                {
                    occurrences = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    occurrences = qry.OrderByDescending( a => a.Date ).ThenByDescending( a => a.StartTime ).ToList();
                }

                gOccurrences.DataSource = occurrences;
                gOccurrences.DataBind();
            }
        }

        /// <summary>
        /// Makes the key unique to group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToGroup( string key )
        {
            if ( _group != null )
            {
                return string.Format( "{0}-{1}", _group.Id, key );
            }

            return key;
        }

        #endregion
        
}
}