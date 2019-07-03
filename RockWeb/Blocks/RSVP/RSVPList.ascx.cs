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
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data.Entity;

namespace RockWeb.Blocks.RSVP
{
    /// <summary>
    /// Lists all the event items in the given calendar.
    /// </summary>
    [DisplayName( "RSVP Occurrences List" )]
    [Category( "RSVP" )]
    [Description( "Lists RSVP Occurrences." )]

    [LinkedPage(
        "RSVP Detail Page",
        Description = "The Page to displays RSVP Details",
        Key = AttributeKey.RSVPDetailPage )]
    public partial class RSVPList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys
        protected static class AttributeKey
        {
            public const string RSVPDetailPage = "RSVPDetailPage";
        }

        protected static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string OccurrenceId = "OccurrenceId";
        }

        protected static class UserPreferenceKey
        {
            public const string DateRange = "DateRange";
            public const string Location = "Location";
            public const string Schedule = "Schedule";
        }
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                if ( groupId != null )
                {
                    var rsvpOccurrences = GetGroupRSVP( groupId.Value );
                    if ( rsvpOccurrences.Where( r => r.Id != 0 ).Any() )
                    {
                        SetFilter();
                        BindRSVPItemsGrid( rsvpOccurrences );
                    }
                    else
                    {
                        NavigateToRSVPDetail();
                    }
                }
            }
        }

        #endregion

        #region RSVPItems Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.DateRange ), "Date Range", drpDates.DelimitedValues );
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.Location ), "Location", ddlSchedule.SelectedValue );
            rFilter.SaveUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.Schedule ), "Schedule", ddlLocation.SelectedValue );

            BindRSVPItemsGrid();
        }

        /// <summary>
        /// Makes the key unique to the selected group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToGroup(string key)
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();

            if ( groupId != null )
            {
                key = string.Format( "{0}-{1}", groupId.Value.ToString(), key );
            }
            
            return "RSVPList-" + key;
        }

        /// <summary>
        /// Formats the filter display values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == MakeKeyUniqueToGroup( UserPreferenceKey.Location ) )
            {
                e.Value = ddlLocation.SelectedItem.Text;
            }
            else if ( e.Key == MakeKeyUniqueToGroup( UserPreferenceKey.DateRange ) )
            {
                var drp = new DateRangePicker();
                drp.DelimitedValues = e.Value;
                if ( drp.LowerValue.HasValue && !drp.UpperValue.HasValue )
                {
                    drp.UpperValue = drp.LowerValue.Value.AddYears( 1 ).AddDays( -1 );
                }
                else if ( drp.UpperValue.HasValue && !drp.LowerValue.HasValue )
                {
                    drp.LowerValue = drp.UpperValue.Value.AddYears( -1 ).AddDays( 1 );
                }
                e.Value = DateRangePicker.FormatDelimitedValues( drp.DelimitedValues );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gEventCalendarItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRSVPItems_GridRebind( object sender, EventArgs e )
        {
            BindRSVPItemsGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDates.DelimitedValues = rFilter.GetUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.DateRange ) );

            var locations = GetLocations();
            if ( locations.Any() )
            {
                ddlLocation.DataSource = locations;
                ddlLocation.DataBind();

                string locationValue = rFilter.GetUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.Location ) );
                if ( !string.IsNullOrWhiteSpace( locationValue ) )
                {
                    int? locationId = locationValue.AsIntegerOrNull();
                    if ( locations.Select( l => l.Key == locationId ).Any() )
                    {
                        ddlLocation.SelectedValue = locationValue;
                    }
                }
            }
            else
            {
                ddlLocation.Visible = false;
            }

            var schedules = Getschedules( locations );
            if ( schedules.Any() )
            {
                ddlSchedule.DataSource = schedules;
                ddlSchedule.DataBind();

                string scheduleValue = rFilter.GetUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.Schedule ) );
                if ( !string.IsNullOrWhiteSpace(scheduleValue) )
                {
                    int? scheduleId = scheduleValue.AsIntegerOrNull();
                    if ( locations.Select( l => l.Key == scheduleId ).Any() )
                    {
                        ddlSchedule.SelectedValue = scheduleValue;
                    }
                }
            }
            else
            {
                ddlSchedule.Visible = false;
            }
            
        }

        /// <summary>
        /// Gets locations associated with the group to be displayed in the filter selections.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetLocations()
        {
            var locations = new Dictionary<int, string>();

            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupLocationService = new GroupLocationService( rockContext );
                    var groupsLocationList = groupLocationService.Queryable()
                        .Where( a => a.GroupId == groupId )
                        .AsNoTracking().Select( a => a.LocationId ).ToList();

                    var locationService = new LocationService( rockContext );
                    var locationList = locationService.Queryable()
                        .Where( l => groupsLocationList.Contains( l.Id ) )
                        .AsNoTracking().ToList();

                    foreach ( var location in locationList )
                    {
                        if ( !locations.ContainsKey( location.Id ) )
                        {
                            if ( location.IsNamedLocation )
                            {
                                locations.Add( location.Id, location.Name );
                            }
                            else
                            {
                                locations.Add( location.Id, location.EntityStringValue );
                            }
                        }
                    }
                }
            }

            return locations;
        }

        /// <summary>
        /// Gets locations associated with the group to be displayed in the filter selections.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> Getschedules( Dictionary<int, string> locations )
        {
            var schedules = new Dictionary<int, string>();

            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupLocations = new GroupLocationService( rockContext ).Queryable()
                        .Where( l => l.GroupId == groupId )
                        .AsNoTracking();

                    groupLocations.SelectMany( l => l.Schedules ).OrderBy( s => s.Name ).ToList()
                    .ForEach( s => schedules.AddOrIgnore( s.Id, s.Name ) );
                    if ( schedules.Any() )
                    {
                        ddlSchedule.Visible = true;
                        ddlSchedule.DataSource = schedules;
                        ddlSchedule.DataBind();
                        ddlSchedule.SetValue( rFilter.GetUserPreference( MakeKeyUniqueToGroup( UserPreferenceKey.Schedule ) ) );
                    }
                    else
                    {
                        ddlSchedule.Visible = false;
                    }

                }
            }

            return schedules;
        }

        /// <summary>
        /// Binds the RSVP items grid.
        /// </summary>
        protected void BindRSVPItemsGrid( List<RSVPListOccurrence> items = null )
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                gRSVPItems.DataSource = GetGroupRSVP( groupId.Value );
                gRSVPItems.DataBind();
            }
        }

        /// <summary>
        /// Gets the data items to be included in the grid.
        /// </summary>
        /// <param name="groupId"></param>
        private List<RSVPListOccurrence> GetGroupRSVP( int groupId )
        {
            DateTime? fromDateTime = drpDates.LowerValue;
            DateTime? toDateTime = drpDates.UpperValue;
            List<int> locationIds = new List<int>();
            List<int> scheduleIds = new List<int>();

            // Location Filter
            if ( ddlLocation.Visible )
            {
                int? locationId = ddlLocation.SelectedValue.AsIntegerOrNull();
                if ( locationId.HasValue )
                {
                    locationIds.Add( locationId.Value );
                }
            }

            //// Schedule Filter
            //if ( ddlSchedule.Visible )
            //{
            //    int? scheduleId = ddlSchedule.SelectedValue.AsIntegerOrNull();
            //    if ( scheduleId.HasValue )
            //    {
            //        scheduleIds.Add( scheduleId.Value );
            //    }
            //}

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var group = groupService.Get( groupId );

            // Get all the occurrences for this group for the selected dates, location and schedule
            var occurrences = new AttendanceOccurrenceService( rockContext )
                .GetGroupOccurrences( group, fromDateTime, toDateTime, locationIds, scheduleIds )
                .Select( o => new RSVPListOccurrence( o ) )
                .ToList();


            // Sort the occurrences
            List<RSVPListOccurrence> sortedOccurrences = null;
            if ( gRSVPItems.SortProperty != null )
            {
                sortedOccurrences = occurrences.AsQueryable().Sort( gRSVPItems.SortProperty ).ToList();
            }
            else
            {
                sortedOccurrences = occurrences.OrderByDescending( a => a.OccurrenceDate ).ThenByDescending( a => a.StartTime ).ToList();
            }

            return sortedOccurrences;
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the RowSelected event of gRSVPItems.
        /// </summary>
        protected void gRSVPItems_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToRSVPDetail( e.RowKeyId.ToString() );
        }

        /// <summary>
        /// Handles the Click event of btnDetails.
        /// </summary>
        protected void btnDetails_Click( object sender, RowEventArgs e )
        {
            NavigateToRSVPDetail( e.RowKeyId.ToString() );
        }

        /// <summary>
        /// Navigates to the row details page for a selected RSVP occurrence.
        /// </summary>
        /// <param name="occurrenceId">The Id of the occurence to display.</param>
        private void NavigateToRSVPDetail( string occurrenceId = "" )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId == null )
            {
                // Can't use this page or the detail page without selecting a group.
                NavigateToParentPage();
            }
            else
            {
                queryParams.Add( PageParameterKey.GroupId, groupId.Value.ToString() );
                if ( !string.IsNullOrWhiteSpace( occurrenceId ) )
                {
                    queryParams.Add( PageParameterKey.OccurrenceId, occurrenceId);
                }

                NavigateToLinkedPage( AttributeKey.RSVPDetailPage, queryParams );
            }
        }

        #endregion

    }


    public class RSVPListOccurrence
    {
        public int Id { get; set; }
        public DateTime OccurrenceDate { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int? ParentLocationId { get; set; }
        public string ParentLocationPath { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? CampusId { get; set; }
        public int InvitedCount { get; set; }
        public int AcceptedCount { get; set; }
        public int DeclinedCount { get; set; }
        public int NoResponseCount { get; set; }

        public RSVPListOccurrence(AttendanceOccurrence occurrence)
        {
            Id = occurrence.Id;
            OccurrenceDate = occurrence.OccurrenceDate;
            LocationId = occurrence.LocationId;

            if (occurrence.Location != null)
            {
                if (occurrence.Location.Name.IsNotNullOrWhiteSpace())
                {
                    LocationName = occurrence.Location.Name;
                }
                else
                {
                    LocationName = occurrence.Location.ToString();
                }
            }

            LocationName = occurrence.Location != null ? occurrence.Location.Name : string.Empty;
            ParentLocationId = occurrence.Location != null ? occurrence.Location.ParentLocationId : (int?)null;
            ScheduleId = occurrence.ScheduleId;

            if (occurrence.Schedule != null)
            {
                if (occurrence.Schedule.Name.IsNotNullOrWhiteSpace())
                {
                    ScheduleName = occurrence.Schedule.Name;
                }
                else
                {
                    ScheduleName = occurrence.Schedule.ToString();
                }
            }

            StartTime = occurrence.Schedule != null ? occurrence.Schedule.StartTimeOfDay : new TimeSpan();


            foreach ( var attendee in occurrence.Attendees )
            {
                if ( attendee.RequestedToAttend == true )
                {
                    this.InvitedCount++;
                    if ( attendee.RSVPDateTime.HasValue )
                    {
                        if ( attendee.DeclineReasonValueId.HasValue )
                        {
                            this.DeclinedCount++;
                        }
                        else
                        {
                            this.AcceptedCount++;
                        }
                    }
                    else
                    {
                        this.NoResponseCount++;
                    }
                }
            }

        }
    }

}