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
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the occurrence details for a given calendar event item.
    /// </summary>
    [DisplayName( "Calendar Event Item Occurrence List" )]
    [Category( "Event" )]
    [Description( "Displays the occurrence details for a given calendar event item." )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page to view linkage details",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Registration Instance Page",
        Key = AttributeKey.RegistrationInstancePage,
        Description = "The page to view registration details",
        IsRequired = true,
        Order = 1 )]

    [LinkedPage( "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "The page for viewing details about a group",
        IsRequired = true,
        Order = 2 )]

    [LinkedPage( "Content Item Detail Page",
        Key = AttributeKey.ContentItemDetailPage,
        Description = "The page for viewing details about a content item",
        IsRequired = true,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "94230E7A-8EB7-4407-9B8E-888B54C71E39" )]
    public partial class EventItemOccurrenceList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys

        private class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string ContentItemDetailPage = "ContentItemDetailPage";
        }

        private class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string CopyFromId = "CopyFromId";
        }

        #endregion Keys

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

                    var registrationField = gCalendarItemOccurrenceList.ColumnsOfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Registration" );
                    if ( registrationField != null )
                    {
                        registrationField.DataNavigateUrlFormatString = LinkedPageUrl( AttributeKey.RegistrationInstancePage ) + "?RegistrationInstanceId={0}";
                    }

                    var groupField = gCalendarItemOccurrenceList.ColumnsOfType<HyperLinkField>().FirstOrDefault( a => a.HeaderText == "Group" );
                    if ( groupField != null )
                    {
                        groupField.DataNavigateUrlFormatString = LinkedPageUrl( AttributeKey.GroupDetailPage ) + "?GroupId={0}";
                    }

                    AddAttributeColumns();

                    var copyField = new LinkButtonField();
                    copyField.HeaderText = "Copy";
                    copyField.CssClass = "btn btn-default btn-sm fa fa-clone";
                    copyField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    copyField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    gCalendarItemOccurrenceList.Columns.Add( copyField );
                    copyField.Click += gCalendarItemOccurrenceList_Copy;  

                    var deleteField = new DeleteField();
                    gCalendarItemOccurrenceList.Columns.Add( deleteField );
                    deleteField.Click += gCalendarItemOccurrenceList_Delete;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                SetFilter();
                BindCampusGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( "Campus", cblCampus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SetFilterPreference( "DateRange", drpDate.DelimitedValues );
            rFilter.SetFilterPreference( "Contact", tbContact.Text );

            BindCampusGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the rFilter control.
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
            var qryParams = new Dictionary<string, string>
            {
                { PageParameterKey.EventCalendarId, GetEventCalendarId( _eventItem ) },
                { PageParameterKey.EventItemId, _eventItem.Id.ToString() },
                { PageParameterKey.EventItemOccurrenceId, "0" }
            };

            NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Handles the Copy event of the gCalendarItemOccurrenceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCalendarItemOccurrenceList_Copy( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                EventItemOccurrenceService eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                EventItemOccurrence eventItemOccurrence = eventItemOccurrenceService.Get( e.RowKeyId );
                if ( eventItemOccurrence != null )
                {
                    var qryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.EventCalendarId, GetEventCalendarId( _eventItem ) },
                        { PageParameterKey.EventItemId, _eventItem.Id.ToString() },
                        { PageParameterKey.EventItemOccurrenceId, "0" },
                        { PageParameterKey.CopyFromId, eventItemOccurrence.Id.ToString() }
                    };

                    NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
                }
            }
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
                    var qryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.EventCalendarId, GetEventCalendarId( _eventItem ) },
                        { PageParameterKey.EventItemId, _eventItem.Id.ToString() },
                        { PageParameterKey.EventItemOccurrenceId, eventItemOccurrence.Id.ToString() }
                    };

                    NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCalendarItemOccurrenceList control.
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
        /// Handles the GridRebind event of the gCalendarItemOccurrenceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCalendarItemOccurrenceList_GridRebind( object sender, EventArgs e )
        {
            BindCampusGrid();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Sets the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDate.DelimitedValues = rFilter.GetFilterPreference( "DateRange" );

            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            string campusValue = rFilter.GetFilterPreference( "Campus" );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }

            tbContact.Text = rFilter.GetFilterPreference( "Contact" );
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

                var qry = new EventItemOccurrenceService( rockContext )
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

                // if a date range was specified, need to get all dates for items and filter based on any that have an occurrence within the date range
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
                    // If a date range was not specified display all of the event items but still calculate the next date if possible.
                    lowerDateRange = RockDateTime.Today;
                    upperDateRange = lowerDateRange.Value.AddYears( 1 ).AddDays( -1 );

                    foreach ( var eventItemOccurrenceWithDates in eventItemOccurrencesWithDates )
                    {
                        eventItemOccurrenceWithDates.StartDateTimes = eventItemOccurrenceWithDates.EventItemOccurrence.GetStartTimes( lowerDateRange.Value, upperDateRange.Value.AddDays( 1 ) );
                        if ( eventItemOccurrenceWithDates.StartDateTimes != null && eventItemOccurrenceWithDates.StartDateTimes.Any() )
                        {
                            eventItemOccurrenceWithDates.NextStartDateTime = eventItemOccurrenceWithDates.StartDateTimes.Min();
                        }
                    }

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

                gCalendarItemOccurrenceList.EntityTypeId = EntityTypeCache.Get<Rock.Model.EventItemOccurrence>().Id;
                gCalendarItemOccurrenceList.ObjectList = new Dictionary<string, object>();
                eventItemOccurrencesWithDates.ForEach( i => gCalendarItemOccurrenceList.ObjectList.Add( i.EventItemOccurrence.Id.ToString(), i.EventItemOccurrence ) );
                gCalendarItemOccurrenceList.DataSource = eventItemOccurrencesWithDates
                    .Select( c => new
                    {
                        c.EventItemOccurrence.Id,
                        c.EventItemOccurrence.Guid,
                        Campus = c.EventItemOccurrence.Campus != null ? c.EventItemOccurrence.Campus.Name : "All Campuses",
                        Date = c.NextStartDateTime.HasValue ? c.NextStartDateTime.Value.ToShortDateString() : "N/A",
                        Location = c.EventItemOccurrence.Location,
                        RegistrationInstanceId = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().RegistrationInstanceId : ( int? ) null,
                        RegistrationInstance = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().RegistrationInstance : null,
                        GroupId = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().GroupId : ( int? ) null,
                        Group = c.EventItemOccurrence.Linkages.Any() ? c.EventItemOccurrence.Linkages.FirstOrDefault().Group : null,
                        ContentItems = FormatContentItems( c.EventItemOccurrence.ContentChannelItems.Select( i => i.ContentChannelItem ).ToList() ),
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

        /// <summary>
        /// Formats the Content Items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string FormatContentItems( IEnumerable<ContentChannelItem> items )
        {
            var qryParams = new Dictionary<string, string> { { "ContentItemId", "" } };

            var itemLinks = new List<string>();
            foreach ( var item in items )
            {
                qryParams["ContentItemId"] = item.Id.ToString();
                itemLinks.Add( string.Format( "<a href='{0}'>{1}</a> ({2})", LinkedPageUrl( AttributeKey.ContentItemDetailPage, qryParams ), item.Title, item.ContentChannelType.Name ) );
            }
            return itemLinks.AsDelimited( "<br/>" );
        }

        /// <summary>s
        /// Adds columns for any Account attributes marked as Show In Grid
        /// </summary>
        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gCalendarItemOccurrenceList.Columns.OfType<AttributeField>().ToList() )
            {
                gCalendarItemOccurrenceList.Columns.Remove( column );
            }

            int entityTypeId = new EventItemOccurrence().TypeId;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    (  a.EntityTypeQualifierColumn == null || a.EntityTypeQualifierColumn == string.Empty  || a.EntityTypeQualifierColumn.Equals( "EventItemId", StringComparison.OrdinalIgnoreCase ) ) &&
                    ( a.EntityTypeQualifierValue == null || a.EntityTypeQualifierValue == string.Empty || a.EntityTypeQualifierValue == _eventItem.Id.ToString() ) &&
                    a.IsGridColumn
                   )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gCalendarItemOccurrenceList.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.AttributeId = attribute.Id;
                    boundField.HeaderText = attribute.Name;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gCalendarItemOccurrenceList.Columns.Add( boundField );
                }
            }
        }

        /// <summary>
        /// Gets an appropriate Event Calendar identifier for a given Event Item.  If the calendar id is specified in the page parameter
        /// collection, that value will be used, if not the calendar id from the first Event Calendar Item attached to the Event Item will
        /// be used.
        /// NOTE: This is necessary because this block has been used on pages/routes which do not supply the event calendar id (e.g., the link
        /// from a followed event).
        /// </summary>
        /// <param name="eventItem">The <see cref="EventItem"/>.</param>
        /// <returns></returns>
        private string GetEventCalendarId( EventItem eventItem )
        {
            int? calendarId = PageParameter( PageParameterKey.EventCalendarId ).AsIntegerOrNull();
            if ( calendarId.HasValue )
            {
                return calendarId.Value.ToString();
            }

            var calendarItem = eventItem.EventCalendarItems.FirstOrDefault();
            if ( calendarItem != null )
            {
                return calendarItem.EventCalendarId.ToString();
            }

            return string.Empty;
        }

        #endregion Methods

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion ISecondaryBlock

        #region Helper Class

        /// <summary>
        /// Event Item Occurence with Dates
        /// </summary>
        public class EventItemOccurrenceWithDates
        {
            /// <summary>
            /// The Event Item Occurrence.
            /// </summary>
            public EventItemOccurrence EventItemOccurrence { get; set; }

            /// <summary>
            /// The Next Start Date/Time.
            /// </summary>
            public DateTime? NextStartDateTime { get; set; }

            /// <summary>
            /// The list of Start Dates/Times.
            /// </summary>
            public List<DateTime> StartDateTimes { get; set; }
        }

        #endregion Helper Class
    }
}