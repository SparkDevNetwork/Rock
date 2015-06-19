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
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Date Range" ), "Date Range", drpDate.DelimitedValues );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Audience" ), "Audience", cblAudience.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Status" ), "Status", cbActive.Checked.ToTrueFalse() );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues );
                            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues ).ToJson() );
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

            if ( e.Key == MakeKeyUniqueToEventCalendar( "Campuses" ) )
            {
                e.Value = ResolveValues( e.Value, cblCampus );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Date Range" ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Audiences" ) )
            {
                e.Value = ResolveValues( e.Value, cblAudience );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Status" ) )
            {
                e.Value = e.Value == "True" ? "Only Show Active Items" : string.Empty; 
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
                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                EventCalendarItem eventCalendarItem = eventCalendarItemService.Get( e.RowKeyId );
                if ( eventCalendarItem != null )
                {
                    if ( _canEdit )
                    {
                        string errorMessage;
                        if ( !eventCalendarItemService.CanDelete( eventCalendarItem, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        int eventCalendarId = eventCalendarItem.EventCalendarId;

                        eventCalendarItemService.Delete( eventCalendarItem );
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
                NavigateToLinkedPage( "DetailPage", "EventItemId", 0, "EventCalendarId", _eventCalendar.Id );
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
                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                EventCalendarItem eventCalendarItem = eventCalendarItemService.Get( e.RowKeyId );
                if ( eventCalendarItem != null )
                {
                    NavigateToLinkedPage( "DetailPage", "EventItemId", eventCalendarItem.EventItemId, "EventCalendarId", _eventCalendar.Id );
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
            string drFilter = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Date Range" ) );
            if ( string.IsNullOrWhiteSpace( drFilter ))
            {
                rFilter.SaveUserPreference(MakeKeyUniqueToEventCalendar( "Date Range" ), string.Empty );
            }
                
            if ( _eventCalendar != null )
            {
                cblAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
                cblCampus.DataSource = CampusCache.All();
                cblCampus.DataBind();
            }

            string campusValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Campuses" ) );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
            string audienceValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Audiences" ) );
            if ( !string.IsNullOrWhiteSpace( audienceValue ) )
            {
                cblAudience.SetValues( audienceValue.Split( ';' ).ToList() );
            }

            string statusValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Status" ) );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cbActive.Checked = statusValue.AsBoolean();
            }

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

            // Remove Active column
            foreach ( var column in gEventCalendarItems.Columns.OfType<BoundField>()
                .Where( c => c.DataField == "Active" )
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
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false );
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

            // Add IsActive column
            var activeField = new BoundField();
            activeField.DataField = "Active";
            activeField.SortExpression = "Active";
            activeField.HeaderText = "Active";
            activeField.HtmlEncode = false;
            gEventCalendarItems.Columns.Add( activeField );
            
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

                rFilter.Visible = true;
                gEventCalendarItems.Visible = true;

                var rockContext = new RockContext();

                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                var qry = eventCalendarItemService
                    .Queryable( "EventCalendar,EventItem.EventItemAudiences,EventItem.EventItemCampuses.EventItemSchedules" )
                    .Where( m => 
                        m.EventItem != null &&
                        m.EventCalendarId == _eventCalendar.Id );

                // Filter by Active Only
                if ( cbActive.Checked )
                {
                    qry = qry
                        .Where( m => 
                            m.EventItem.IsActive.HasValue && 
                            m.EventItem.IsActive.Value );
                }
                    
                // Filter by Campus
                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if ( campusIds.Count > 0 )
                {
                    qry = qry
                        .Where( i => 
                            i.EventItem.EventItemCampuses.Any( c => c.CampusId.HasValue && campusIds.Contains( c.CampusId.Value ) ) );
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
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues );
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
                    qry = qry.Where( i => i.EventItem.EventItemAudiences.Any( c => audiences.Contains( c.DefinedValueId ) ) );
                }
                // Filter by Status
                if ( cbActive.Checked )
                {
                    qry = qry.Where( i => i.EventItem.IsActive.HasValue && i.EventItem.IsActive.HasValue );
                }

                SortProperty sortProperty = gEventCalendarItems.SortProperty;

                // Sort and query db
                List<EventCalendarItem> eventCalendarItems = null;
                if ( sortProperty != null )
                {
                    eventCalendarItems = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    eventCalendarItems = qry.ToList().OrderByDescending( a => a.EventItem.GetEarliestStartDate() ).ToList();
                }

                // Now that items have been loaded and ordered from db, calculate the earliest date for each item, and then filter again on the date
                var drp = new DateRangePicker();
                drp.DelimitedValues = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Date Range" ) );
                DateTime? lowerDateRange = drp.LowerValue;
                DateTime? upperDateRange = drp.UpperValue;

                var itemsWithEarliestStartDate = eventCalendarItems
                    .Select( i => new
                    {
                        Item = i,
                        EarliestStartDate = i.EventItem.GetEarliestStartDate()
                    } )
                    .ToList();

                if ( lowerDateRange.HasValue || upperDateRange.HasValue )
                    itemsWithEarliestStartDate = itemsWithEarliestStartDate
                        .Where( i =>
                            !i.EarliestStartDate.HasValue ||
                            (
                                ( !lowerDateRange.HasValue || i.EarliestStartDate.Value >= lowerDateRange.Value ) &&
                                ( !upperDateRange.HasValue || i.EarliestStartDate.Value < upperDateRange.Value.AddDays( 1 ) )
                            ) )
                        .ToList();

                gEventCalendarItems.ObjectList = new Dictionary<string, object>();
                itemsWithEarliestStartDate.ForEach( m => gEventCalendarItems.ObjectList.Add( m.Item.Id.ToString(), m.Item ) );
                gEventCalendarItems.EntityTypeId = EntityTypeCache.Read( "Rock.Model.EventCalendarItem" ).Id;

                gEventCalendarItems.DataSource = itemsWithEarliestStartDate.Select( m => new
                {
                    m.Item.Id,
                    m.Item.Guid,
                    Date = m.EarliestStartDate.HasValue ? m.EarliestStartDate.Value.ToShortDateString() : "N/A",
                    Name = m.Item.EventItem.Name,
                    Campus = m.Item.EventItem.EventItemCampuses.ToList().Select( c => c.Campus != null ? c.Campus.Name : "All Campuses" ).ToList().AsDelimited( "<br>" ),
                    Calendar = m.Item.EventItem.EventCalendarItems.ToList().Select( i => i.EventCalendar.Name ).ToList().AsDelimited( "<br>" ),
                    Audience = m.Item.EventItem.EventItemAudiences.ToList().Select( i => i.DefinedValue.Value ).ToList().AsDelimited( "<br>" ),
                    Active = m.Item.EventItem.IsActive.Value ? "<span class='label label-success'>Active</span>" : "<span class='label label-default'>Inactive</span>"
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
    }
}