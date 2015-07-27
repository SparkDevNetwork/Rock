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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the occurrence details for a given calendar item.
    /// </summary>
    [DisplayName( "Calendar Item Occurrence List" )]
    [Category( "Event" )]
    [Description( "Displays the occurrence details for a given calendar item." )]

    [LinkedPage( "Detail Page", "The page to view linkage details", true, "", "", 0 )]
    [LinkedPage( "Registration Instance Page", "The page to view registration details", true, "", "", 1 )]
    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 2 )]
    public partial class CalendarItemOccurrenceList : RockBlock, ISecondaryBlock
    {
        #region Properties

        private EventItem _eventItem = null;

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int? eventItemId = PageParameter( "EventItemId" ).AsIntegerOrNull();
            if ( eventItemId.HasValue )
            {
                string key = string.Format( "EventItem:{0}", eventItemId );
                _eventItem = RockPage.GetSharedItem( key ) as EventItem;
                if ( _eventItem == null )
                {
                    _eventItem = new EventItemService( new RockContext() ).Queryable()
                        .Where( i => i.Id == eventItemId )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _eventItem );
                }

                if ( _eventItem != null )
                {
                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                    gCalendarItemOccurrenceList.DataKeyNames = new string[] { "Id" };
                    gCalendarItemOccurrenceList.Actions.ShowAdd = true;
                    gCalendarItemOccurrenceList.Actions.AddClick += gCalendarItemOccurrenceList_Add;
                    gCalendarItemOccurrenceList.GridRebind += gCalendarItemOccurrenceList_GridRebind;

                    var registrationCol = gCalendarItemOccurrenceList.Columns[3] as HyperLinkField;
                    registrationCol.DataNavigateUrlFormatString = LinkedPageUrl("RegistrationInstancePage") + "?RegistrationInstanceId={0}";

                    var groupCol = gCalendarItemOccurrenceList.Columns[4] as HyperLinkField;
                    groupCol.DataNavigateUrlFormatString = LinkedPageUrl("GroupDetailPage") + "?GroupId={0}";

                }
            }
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
                SetFilter();
                BindCampusGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Campus", cblCampus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( "DateRange", drpDate.DelimitedValues );
            rFilter.SaveUserPreference( "Contact", tbContact.Text );

            BindCampusGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":
                    {
                        var values = new List<string>();
                        foreach ( string value in e.Value.Split( ';' ) )
                        {
                            var item = cblCampus.Items.FindByValue( value );
                            if ( item != null )
                            {
                                values.Add( item.Text );
                            }
                        }
                        e.Value = values.AsDelimited( ", " );
                        break;
                    }
                case "DateRange":
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
                        break;
                    }
                case "Contact":
                    {
                        e.Value = e.Value;
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the Add event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCalendarItemOccurrenceList_Add( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
            qryParams.Add( "EventItemId", _eventItem.Id.ToString() );
            qryParams.Add( "EventItemOccurrenceId", "0" );
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the RowSelected event of the gCalendarItemOccurrenceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCalendarItemOccurrenceList_RowSelected( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                EventItemOccurrenceService eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                EventItemOccurrence eventItemOccurrence = eventItemOccurrenceService.Get( e.RowKeyId );
                if ( eventItemOccurrence != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                    qryParams.Add( "EventItemId", _eventItem.Id.ToString() );
                    qryParams.Add( "EventItemOccurrenceId", eventItemOccurrence.Id.ToString() );
                    NavigateToLinkedPage( "DetailPage", qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCalendarItemOccurrenceList_Delete( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                EventItemOccurrenceService eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                EventItemOccurrence eventItemOccurrence = eventItemOccurrenceService.Get( e.RowKeyId );
                if ( eventItemOccurrence != null )
                {
                    string errorMessage;
                    if ( !eventItemOccurrenceService.CanDelete( eventItemOccurrence, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    eventItemOccurrenceService.Delete( eventItemOccurrence );
                    rockContext.SaveChanges();
                }
            }

            BindCampusGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCalendarItemOccurrenceList_GridRebind( object sender, EventArgs e )
        {
            BindCampusGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDate.DelimitedValues = rFilter.GetUserPreference( "DateRange" );

            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            string campusValue = rFilter.GetUserPreference( "Campus" );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }

            tbContact.Text = rFilter.GetUserPreference( "Contact" );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindCampusGrid()
        {
            if ( _eventItem != null )
            {
                pnlEventCalendarCampusItems.Visible = true;

                var rockContext = new RockContext();

                var qry = new EventItemOccurrenceService(  rockContext )
                    .Queryable().AsNoTracking()
                    .Where( c => c.EventItemId == _eventItem.Id );

                // Filter by Campus
                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if ( campusIds.Any() )
                {
                    qry = qry
                        .Where( i =>
                            !i.CampusId.HasValue ||
                            campusIds.Contains( i.CampusId.Value ) );
                }

                SortProperty sortProperty = gCalendarItemOccurrenceList.SortProperty;

                // Sort and query db
                List<EventItemOccurrence> eventItemOccurrences = null;
                if ( sortProperty != null )
                {
                    // If sorting on date, wait until after checking to see if date range was specified
                    if ( sortProperty.Property == "Date" )
                    {
                        eventItemOccurrences = qry.ToList();
                    }
                    else
                    {
                        eventItemOccurrences = qry.Sort( sortProperty ).ToList();
                    }
                }
                else
                {
                    eventItemOccurrences = qry.ToList().OrderBy( a => a.NextStartDateTime ).ToList();
                }

                // Contact filter
                if ( !string.IsNullOrWhiteSpace( tbContact.Text ) )
                {
                    eventItemOccurrences = eventItemOccurrences
                        .Where( i => 
                            i.ContactPersonAlias != null &&
                            i.ContactPersonAlias.Person != null &&
                            i.ContactPersonAlias.Person.FullName.Contains( tbContact.Text ) )
                        .ToList();
                }

                // Now that items have been loaded and ordered from db, calculate the next start date for each item
                var eventItemOccurrencesWithDates = eventItemOccurrences
                    .Select( i => new EventItemOccurrenceWithDates
                    {
                        EventItemOccurrence = i,
                        NextStartDateTime = i.NextStartDateTime,
                    } )
                    .ToList();

                var dateCol = gCalendarItemOccurrenceList.Columns.OfType<BoundField>().Where( c => c.DataField == "Date" ).FirstOrDefault();

                // if a date range was specified, need to get all dates for items and filter based on any that have an occurrence withing the date range
                DateTime? lowerDateRange = drpDate.LowerValue;
                DateTime? upperDateRange = drpDate.UpperValue;
                if ( lowerDateRange.HasValue || upperDateRange.HasValue )
                {
                    // If only one value was included, default the other to be a years difference
                    lowerDateRange = lowerDateRange ?? upperDateRange.Value.AddYears( -1 ).AddDays( 1 );
                    upperDateRange = upperDateRange ?? lowerDateRange.Value.AddYears( 1 ).AddDays( -1 );

                    // Get the start datetimes within the selected date range
                    eventItemOccurrencesWithDates.ForEach( i => i.StartDateTimes = i.EventItemOccurrence.GetStartTimes( lowerDateRange.Value, upperDateRange.Value.AddDays( 1 ) ) );

                    // Filter out calendar items with no dates within range
                    eventItemOccurrencesWithDates = eventItemOccurrencesWithDates.Where( i => i.StartDateTimes.Any() ).ToList();

                    // Update the Next Start Date to be the next date in range instead
                    dateCol.HeaderText = "Next Date In Range";
                    eventItemOccurrencesWithDates.ForEach( i => i.NextStartDateTime = i.StartDateTimes.Min() );
                }
                else
                {
                    dateCol.HeaderText = "Next Start Date";
                }

                // Now sort on date if that is what was selected
                if ( sortProperty != null && sortProperty.Property == "Date" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        eventItemOccurrencesWithDates = eventItemOccurrencesWithDates.OrderBy( a => a.NextStartDateTime ).ToList();
                    }
                    else
                    {
                        eventItemOccurrencesWithDates = eventItemOccurrencesWithDates.OrderByDescending( a => a.NextStartDateTime ).ToList();
                    }
                }

                gCalendarItemOccurrenceList.DataSource = eventItemOccurrencesWithDates
                    .Select( c => new
                    {
                        c.EventItemOccurrence.Id,
                        c.EventItemOccurrence.Guid,
                        Campus = c.EventItemOccurrence.Campus != null ? c.EventItemOccurrence.Campus.Name : "All Campuses",
                        Date = c.NextStartDateTime.HasValue ? c.NextStartDateTime.Value.ToShortDateString() : "N/A",
                        Location = c.EventItemOccurrence.Location,
                        RegistrationInstanceId = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().RegistrationInstanceId : (int?)null,
                        RegistrationInstance = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().RegistrationInstance : null,
                        GroupId = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().GroupId : (int?)null,
                        Group = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().Group : null,
                        Contact = c.EventItemOccurrence.ContactPersonAlias != null ? c.EventItemOccurrence.ContactPersonAlias.Person.FullName : "",
                        Phone = c.EventItemOccurrence.ContactPhone,
                        Email = c.EventItemOccurrence.ContactEmail,
                    } )
                    .ToList();
                gCalendarItemOccurrenceList.DataBind();
            }
            else
            {
                pnlEventCalendarCampusItems.Visible = false;
            }
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

        public class EventItemOccurrenceWithDates
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }
            public DateTime? NextStartDateTime { get; set; }
            public List<DateTime> StartDateTimes { get; set; }
        }

}
}