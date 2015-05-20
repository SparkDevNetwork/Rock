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

namespace RockWeb.Blocks.Calendar
{
    [DisplayName( "Event Calendar Item List" )]
    [Category( "Calendar" )]
    [Description( "Lists all the items in the given calendar." )]

    [LinkedPage( "Detail Page" )]
    public partial class EventCalendarItemList : RockBlock, ISecondaryBlock
    {
        #region Private Variables

        private EventCalendar _eventCalendar = null;
        private bool _canView = false;

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
                eventCalendarId = PageParameter( "CalendarTypeId" ).AsInteger();
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

                if ( _eventCalendar != null && _eventCalendar.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    _canView = true;

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                    gEventCalendarItems.DataKeyNames = new string[] { "Id" };
                    gEventCalendarItems.Actions.AddClick += gEventCalendarItems_AddClick;
                    gEventCalendarItems.GridRebind += gEventCalendarItems_GridRebind;
                    gEventCalendarItems.ExportFilename = _eventCalendar.Name;

                    // make sure they have Auth to edit the block OR edit to the EventCalendar
                    bool canEditBlock = IsUserAuthorized( Authorization.EDIT ) || _eventCalendar.IsAuthorized( Authorization.EDIT, this.CurrentPerson );
                    gEventCalendarItems.Actions.ShowAdd = canEditBlock;
                    gEventCalendarItems.IsDeleteEnabled = canEditBlock;
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
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Campus" ), "Campus", cpsCampus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Date Range" ), "Date Range", drpDate.DelimitedValues );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Audience" ), "Audience", cblAudience.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToEventCalendar( "Status" ), "Status", cbActive.Checked.ToTrueFalse() );

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
                return;
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Date Range" ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Audience" ) )
            {
                e.Value = ResolveValues( e.Value, cblAudience );
            }
            else if ( e.Key == MakeKeyUniqueToEventCalendar( "Status" ) )
            {
                return;
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
            RockContext rockContext = new RockContext();
            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
            EventCalendarItem eventCalendarItem = eventCalendarItemService.Get( e.RowKeyId );
            if ( eventCalendarItem != null )
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
            NavigateToLinkedPage( "DetailPage", "EventItemId", 0, "EventCalendarId", _eventCalendar.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gEventCalendarItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEventCalendarItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "EventItemId", e.RowKeyId );
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
            drpDate.DelimitedValues = rFilter.GetUserPreference( "Date Range" );

            if ( _eventCalendar != null )
            {
                cblAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            }

            string campusValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Campus" ) );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cpsCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
            string audienceValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Audience" ) );
            if ( !string.IsNullOrWhiteSpace( audienceValue ) )
            {
                cblAudience.SetValues( audienceValue.Split( ';' ).ToList() );
            }

            string statusValue = rFilter.GetUserPreference( MakeKeyUniqueToEventCalendar( "Status" ) );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cbActive.Checked = statusValue.AsBoolean();
            }

        }

        /// <summary>
        /// Binds the group members grid.
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
                var qry = eventCalendarItemService.Queryable( "EventItem, EventCalendar, EventItem.EventItemCampuses, EventItem.EventItemAudiences" )
                    .Where( m => m.EventCalendarId == _eventCalendar.Id );

                // Filter by Campus
                List<int> campusIds = cpsCampus.SelectedValuesAsInt;
                if ( campusIds.Count > 0 )
                {
                    qry = qry.Where( i => i.EventItem.EventItemCampuses.Any( c => c.CampusId.HasValue && campusIds.Contains( c.CampusId.Value ) ) );
                }

                // Filter by Date Range
                drpDate.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
                DateTime minusSixMonths = DateTime.Now.AddMonths( -6 );
                DateTime minusOneMonth = DateTime.Now.AddDays( -30 );
                DateTime plusOneMonth = DateTime.Now.AddDays( 30 );
                if ( drpDate.LowerValue.HasValue && drpDate.UpperValue.HasValue )
                {
                    //qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.GetScheduledStartTimes( drpDate.LowerValue.Value, drpDate.UpperValue.Value ).Any() ) );
                   // qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.EffectiveStartDate.Value > drpDate.LowerValue.Value && s.Schedule.EffectiveStartDate.Value < drpDate.UpperValue.Value ) );
                }
                else
                {
                    if ( !drpDate.LowerValue.HasValue && !drpDate.UpperValue.HasValue )
                    {
                        //qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.GetScheduledStartTimes( DateTime.Now.AddMonths( -6 ), DateTime.Now.AddDays( 30 ) ).Any() ) );
                      //  qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.EffectiveStartDate.Value > minusSixMonths && s.Schedule.EffectiveStartDate.Value < plusOneMonth ) );

                    }
                    if ( drpDate.LowerValue.HasValue )
                    {
                        // qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.GetScheduledStartTimes( drpDate.LowerValue.Value, DateTime.Now.AddDays( 30 ) ).Any() ) );
                       // qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.EffectiveStartDate.Value > drpDate.LowerValue.Value && s.Schedule.EffectiveStartDate.Value < plusOneMonth ) );
                    }
                    if ( drpDate.UpperValue.HasValue )
                    {
                        //qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.GetScheduledStartTimes( DateTime.Now.AddDays( -30 ), drpDate.UpperValue.Value ).Any() ) );
                        //qry = qry.Where( i => i.EventItem.EventItemSchedules.Any( s => s.Schedule.EffectiveStartDate.Value > minusOneMonth && s.Schedule.EffectiveStartDate.Value < drpDate.UpperValue.Value ) );

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

                List<EventCalendarItem> eventCalendarItems = null;

                if ( sortProperty != null )
                {
                    eventCalendarItems = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    eventCalendarItems = qry.ToList();  // qry.OrderBy( a => a.EventItem.EventItemSchedules.OrderByDescending( s => s.Schedule.EffectiveStartDate.Value ).FirstOrDefault().Schedule.EffectiveStartDate.Value ).ToList();
                }

                // Since we're not binding to actual group member list, but are using AttributeField columns,
                // we need to save the workflows into the grid's object list
                gEventCalendarItems.ObjectList = new Dictionary<string, object>();
                eventCalendarItems.ForEach( m => gEventCalendarItems.ObjectList.Add( m.Id.ToString(), m ) );
                gEventCalendarItems.EntityTypeId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.EVENT_CALENDAR_ITEM.AsGuid() ).Id;

                gEventCalendarItems.DataSource = eventCalendarItems.Select( m => new
                {
                    m.Id,
                    m.Guid,
                    Name = m.EventItem.Name,
                    Campus = m.EventItem.EventItemCampuses.ToList().Select( c => c.Campus.Name ).ToList().AsDelimited( "/n" ),
                    Calendar = m.EventItem.EventCalendarItems.ToList().Select( i => i.EventCalendar.Name ).ToList().AsDelimited( "/n" ),
                    Audience = m.EventItem.EventItemAudiences.ToList().Select( i => i.DefinedValue.Value ).ToList().AsDelimited( "/n" ),
                    Active = m.EventItem.IsActive.Value ? "<Rock:HighlightLabel ID='hlInactive' runat='server' LabelType='Success' Text='Active' />" : "<Rock:HighlightLabel ID='hlInactive' runat='server' LabelType='Danger' Text='Inactive' />"
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
        /// Makes the key unique to group.
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