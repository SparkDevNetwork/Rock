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

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Lists all the items in the given calendar.
    /// </summary>
    [DisplayName( "Calendar Item List" )]
    [Category( "Event" )]
    [Description( "Lists all the items in the given calendar." )]

    [LinkedPage( "Detail Page" )]
    public partial class CalendarItemList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private EventCalendar _eventCalendar = null;
        private bool _canView = false;
        private bool _canEdit = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if this block has a specific EventCalendarId set, use that, otherwise, determine it from the PageParameters
            Guid eventCalendarGuid = GetAttributeValue( "EventCalendar" ).AsGuid();
            int eventCalendarId = 0;

            if ( eventCalendarGuid == Guid.Empty )
            {
                eventCalendarId = PageParameter( "EventCalendarId" ).AsInteger();
            }

            if ( !( eventCalendarId == 0 && eventCalendarGuid == Guid.Empty ) )
            {
                string key = string.Format( "EventCalendar:{0}", eventCalendarId );
                _eventCalendar = RockPage.GetSharedItem( key ) as EventCalendar;
                if ( _eventCalendar == null )
                {
                    _eventCalendar = new EventCalendarService( new RockContext() ).Queryable()
                        .Where( g => g.Id == eventCalendarId || g.Guid == eventCalendarGuid )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _eventCalendar );
                }

                if ( _eventCalendar != null )
                {
                    _canEdit = UserCanEdit || _eventCalendar.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    _canView = _canEdit || _eventCalendar.IsAuthorized( Authorization.VIEW, CurrentPerson );

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                    gEventCalendarItems.DataKeyNames = new string[] { "Id" };
                    gEventCalendarItems.Actions.ShowAdd = _canEdit;
                    gEventCalendarItems.Actions.AddClick += gEventCalendarItems_AddClick;
                    gEventCalendarItems.GridRebind += gEventCalendarItems_GridRebind;
                    gEventCalendarItems.ExportFilename = _eventCalendar.Name;
                    gEventCalendarItems.IsDeleteEnabled = _canEdit;
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
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    SetFilter();
                    BindEventCalendarItemsGrid();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region EventCalendarItems Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Campus" ), "Campus", cblCampus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "DateRange" ), "Date Range", drpDate.DelimitedValues );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Audience" ), "Audience", cblAudience.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Status" ), "Status", ddlStatus.SelectedValue );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "ApprovalStatus" ), "Approval Status", ddlApprovalStatus.SelectedValue );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch { }
                    }
                }
            }

            BindEventCalendarItemsGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToEventCalendar( a.Key ) == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch { }
                }
            }

            if ( e.Key == MakeKeyUniqueToEventCalendar( "Campus" ) )
            {
                e.Value = ResolveValues( e.Value, cblCampus );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "DateRange" ) )
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
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Audiences" ) )
            {
                e.Value = ResolveValues( e.Value, cblAudience );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Status" ) ||
                e.Key == MakeKeyUniqueToEventCalendar( "ApprovalStatus" ) )
            {
                e.Value = e.Value;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteEventCalendarItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteEventCalendarItem_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                EventItemService eventItemService = new EventItemService( rockContext );
                EventItem eventItem = eventItemService.Get( e.RowKeyId );
                if ( eventItem != null )
                {
                    if ( _canEdit )
                    {
                        string errorMessage;
                        if ( !eventItemService.CanDelete( eventItem, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        eventItemService.Delete( eventItem );
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        mdGridWarning.Show( "You are not authorized to delete this calendar item", ModalAlertType.Warning );
                    }
                }
            }

            BindEventCalendarItemsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gEventCalendarItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gEventCalendarItems_AddClick( object sender, EventArgs e )
        {
            if ( _canEdit )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", _eventCalendar.Id.ToString() );
                qryParams.Add( "EventItemId", "0");
                NavigateToLinkedPage( "DetailPage", qryParams );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gEventCalendarItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEventCalendarItems_Edit( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                EventItemService eventItemService = new EventItemService( rockContext );
                EventItem eventItem = eventItemService.Get( e.RowKeyId );
                if ( eventItem != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "EventCalendarId", _eventCalendar.Id.ToString() );
                    qryParams.Add( "EventItemId", eventItem.Id.ToString() );
                    NavigateToLinkedPage( "DetailPage", qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gEventCalendarItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gEventCalendarItems_GridRebind( object sender, EventArgs e )
        {
            BindEventCalendarItemsGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDate.DelimitedValues = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "DateRange" ) );

            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            string campusValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Campus" ) );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }

            cblAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            string audienceValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Audiences" ) );
            if ( !string.IsNullOrWhiteSpace( audienceValue ) )
            {
                cblAudience.SetValues( audienceValue.Split( ';' ).ToList() );
            }

            ddlStatus.SetValue( rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Status" ) ) );

            ddlApprovalStatus.SetValue( rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "ApprovalStatus" ) ) );

            BindAttributes();
            AddDynamicControls();
        }

        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            if ( _eventCalendar != null )
            {
                int entityTypeId = new EventCalendarItem().TypeId;
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "EventCalendarId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( _eventCalendar.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gEventCalendarItems.Columns.OfType<AttributeField>().ToList() )
            {
                gEventCalendarItems.Columns.Remove( column );
            }

            // Remove status columns
            foreach ( var column in gEventCalendarItems.Columns.OfType<BoundField>()
                .Where( c =>
                    c.DataField == "Status" ||
                    c.DataField == "ApprovalStatus" )
                .ToList() )
            {
                gEventCalendarItems.Columns.Remove( column );
            }

            // Remove Delete column
            foreach ( var column in gEventCalendarItems.Columns.OfType<DeleteField>().ToList() )
            {
                gEventCalendarItems.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = (IRockControl)control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( attribute.Key ) );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch { }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gEventCalendarItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gEventCalendarItems.Columns.Add( boundField );
                    }
                }
            }

            // Add Status column
            var statusField = new BoundField();
            statusField.DataField = "Status";
            statusField.SortExpression = "EventItem.IsActive";
            statusField.HeaderText = "Status";
            statusField.HtmlEncode = false;
            gEventCalendarItems.Columns.Add( statusField );

            // Add Approval Status column
            var approvalStatusField = new BoundField();
            approvalStatusField.DataField = "ApprovalStatus";
            approvalStatusField.SortExpression = "EventItem.IsApproved";
            approvalStatusField.HeaderText = "Approval Status";
            approvalStatusField.HtmlEncode = false;
            gEventCalendarItems.Columns.Add( approvalStatusField );

            // Add delete column
            if ( _canEdit )
            {
                var deleteField = new DeleteField();
                gEventCalendarItems.Columns.Add( deleteField );
                deleteField.Click += DeleteEventCalendarItem_Click;
            }

        }


        /// <summary>
        /// Binds the event calendar items grid.
        /// </summary>
        protected void BindEventCalendarItemsGrid()
        {
            if ( _eventCalendar != null )
            {
                pnlEventCalendarItems.Visible = true;

                var rockContext = new RockContext();

                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                var qry = eventCalendarItemService
                    .Queryable( "EventCalendar,EventItem.EventItemAudiences,EventItem.EventItemOccurrences.Schedule" )
                    .Where( m =>
                        m.EventItem != null &&
                        m.EventCalendarId == _eventCalendar.Id );

                // Filter by Status
                string statusFilter = ddlStatus.SelectedValue;
                if ( statusFilter == "Active" )
                {
                    qry = qry
                        .Where( m => m.EventItem.IsActive );
                }
                else if ( statusFilter == "Inactive" )
                {
                    qry = qry
                        .Where( m => !m.EventItem.IsActive );
                }

                // Filter by Approval Status
                string approvalStatusFilter = ddlApprovalStatus.SelectedValue;
                if ( approvalStatusFilter == "Approved" )
                {
                    qry = qry
                        .Where( m => m.EventItem.IsApproved );
                }
                else if ( approvalStatusFilter == "Not Approved" )
                {
                    qry = qry
                        .Where( m => !m.EventItem.IsApproved );
                }

                // Filter by Campus
                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if ( campusIds.Any() )
                {
                    qry = qry
                        .Where( i =>
                            i.EventItem.EventItemOccurrences
                                .Any( c =>
                                    !c.CampusId.HasValue ||
                                    campusIds.Contains( c.CampusId.Value ) ) );
                }

                // Filter query by any configured attribute filters
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in AvailableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                            if ( expression != null )
                            {
                                var attributeValues = attributeValueService
                                    .Queryable()
                                    .Where( v => v.Attribute.Id == attribute.Id );

                                attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                qry = qry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                        }
                    }
                }

                // Filter by Audience
                List<int> audiences = cblAudience.SelectedValuesAsInt;
                if ( audiences.Any() )
                {
                    qry = qry.Where( i => i.EventItem.EventItemAudiences
                        .Any( c => audiences.Contains( c.DefinedValueId ) ) );
                }

                SortProperty sortProperty = gEventCalendarItems.SortProperty;

                // Sort and query db
                List<EventCalendarItem> eventCalendarItems = null;
                if ( sortProperty != null )
                {
                    // If sorting on date, wait until after checking to see if date range was specified
                    if ( sortProperty.Property == "Date" )
                    {
                        eventCalendarItems = qry.ToList();
                    }
                    else
                    {
                        eventCalendarItems = qry.Sort( sortProperty ).ToList();
                    }
                }
                else
                {
                    eventCalendarItems = qry.OrderBy( a => a.EventItem.Name ).ToList();
                }

                // Now that items have been loaded and ordered from db, calculate the next start date for each item
                var calendarItemsWithDates = eventCalendarItems
                    .Select( i => new EventCalendarItemWithDates
                    {
                        EventCalendarItem = i,
                        NextStartDateTime = i.EventItem.NextStartDateTime,
                    } )
                    .ToList();

                var dateCol = gEventCalendarItems.Columns.OfType<BoundField>().Where( c => c.DataField == "Date" ).FirstOrDefault();

                // if a date range was specified, need to get all dates for items and filter based on any that have an occurrence withing the date range
                DateTime? lowerDateRange = drpDate.LowerValue;
                DateTime? upperDateRange = drpDate.UpperValue;
                if ( lowerDateRange.HasValue || upperDateRange.HasValue )
                {
                    // If only one value was included, default the other to be a years difference
                    lowerDateRange = lowerDateRange ?? upperDateRange.Value.AddYears( -1 ).AddDays( 1 );
                    upperDateRange = upperDateRange ?? lowerDateRange.Value.AddYears( 1 ).AddDays( -1 );

                    // Get the start datetimes within the selected date range
                    calendarItemsWithDates.ForEach( i => i.StartDateTimes = i.EventCalendarItem.EventItem.GetStartTimes( lowerDateRange.Value, upperDateRange.Value.AddDays( 1 ) ) );

                    // Filter out calendar items with no dates within range
                    calendarItemsWithDates = calendarItemsWithDates.Where( i => i.StartDateTimes.Any() ).ToList();

                    // Update the Next Start Date to be the next date in range instead
                    dateCol.HeaderText = "Next Date In Range";
                    calendarItemsWithDates.ForEach( i => i.NextStartDateTime = i.StartDateTimes.Min() );
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
                        calendarItemsWithDates = calendarItemsWithDates.OrderBy( a => a.NextStartDateTime ).ToList();
                    }
                    else
                    {
                        calendarItemsWithDates = calendarItemsWithDates.OrderByDescending( a => a.NextStartDateTime ).ToList();
                    }
                }

                // Save the calendar items to the grid's objectlist
                gEventCalendarItems.ObjectList = new Dictionary<string, object>();
                calendarItemsWithDates.ForEach( i => gEventCalendarItems.ObjectList.Add( i.EventCalendarItem.Id.ToString(), i.EventCalendarItem ) );
                gEventCalendarItems.EntityTypeId = EntityTypeCache.Read( "Rock.Model.EventCalendarItem" ).Id;

                gEventCalendarItems.DataSource = calendarItemsWithDates.Select( i => new
                {
                    i.EventCalendarItem.EventItem.Id,
                    i.EventCalendarItem.EventItem.Guid,
                    Date = i.NextStartDateTime.HasValue ? i.NextStartDateTime.Value.ToShortDateString() : "N/A",
                    Name = i.EventCalendarItem.EventItem.Name,
                    Campus = i.EventCalendarItem.EventItem.EventItemOccurrences.ToList().Select( c => c.Campus != null ? c.Campus.Name : "All Campuses" ).ToList().AsDelimited( "<br>" ),
                    Calendar = i.EventCalendarItem.EventItem.EventCalendarItems.ToList().Select( c => c.EventCalendar.Name ).ToList().AsDelimited( "<br>" ),
                    Audience = i.EventCalendarItem.EventItem.EventItemAudiences.ToList().Select( a => a.DefinedValue.Value ).ToList().AsDelimited( "<br>" ),
                    Status = i.EventCalendarItem.EventItem.IsActive ? "<span class='label label-success'>Active</span>" : "<span class='label label-default'>Inactive</span>",
                    ApprovalStatus = i.EventCalendarItem.EventItem.IsApproved ? "<span class='label label-info'>Approved</span>" : "<span class='label label-warning'>Not Approved</span>"
                } ).ToList();

                gEventCalendarItems.DataBind();
            }
            else
            {
                pnlEventCalendarItems.Visible = false;
            }
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        /// <summary>
        /// Makes the key unique to event calendar.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToEventCalendar( string key )
        {
            if ( _eventCalendar != null )
            {
                return string.Format( "{0}-{1}", _eventCalendar.Id, key );
            }
            return key;
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

        public class EventCalendarItemWithDates
        {
            public EventCalendarItem EventCalendarItem { get; set; }
            public DateTime? NextStartDateTime { get; set; }
            public List<DateTime> StartDateTimes { get; set; }
        }
    }
}