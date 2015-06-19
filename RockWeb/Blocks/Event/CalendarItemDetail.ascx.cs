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
    /// Displays the details of the given calendar item.
    /// </summary>
    [DisplayName( "Calendar Item Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given calendar item." )]

    [BooleanField( "Show Edit", "", true, "", 2 )]
    public partial class CalendarItemDetail : RockBlock, IDetailBlock
    {
        #region Properties

        public int _calendarId = 0;
        public bool _canEdit = false;

        public List<EventItemAudience> AudiencesState { get; set; }
        public List<EventCalendarItem> ItemsState { get; set; }
        public List<EventItemCampus> CampusesState { get; set; }
        public List<EventItemSchedule> SchedulesState { get; set; }

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["AudiencesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AudiencesState = new List<EventItemAudience>();
            }
            else
            {
                AudiencesState = JsonConvert.DeserializeObject<List<EventItemAudience>>( json );
            }

            json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ItemsState = new List<EventCalendarItem>();
            }
            else
            {
                ItemsState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }

            json = ViewState["CampusesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                CampusesState = new List<EventItemCampus>();
            }
            else
            {
                CampusesState = JsonConvert.DeserializeObject<List<EventItemCampus>>( json );
            }

            json = ViewState["SchedulesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SchedulesState = new List<EventItemSchedule>();
            }
            else
            {
                SchedulesState = JsonConvert.DeserializeObject<List<EventItemSchedule>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAudiences.DataKeyNames = new string[] { "Guid" };
            gAudiences.Actions.ShowAdd = true;
            gAudiences.Actions.AddClick += gAudiences_Add;
            gAudiences.GridRebind += gAudiences_GridRebind;

            gCampusDetails.DataKeyNames = new string[] { "Guid" };
            gCampusDetails.Actions.ShowAdd = true;
            gCampusDetails.Actions.AddClick += gCampusDetails_Add;
            gCampusDetails.GridRebind += gCampusDetails_GridRebind;

            gSchedules.DataKeyNames = new string[] { "Guid" };
            gSchedules.Actions.ShowAdd = true;
            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlEventItemList );

            // Get the calendar id of the calendar that user navigated from 
            _calendarId = PageParameter( "EventCalendarId" ).AsInteger();

            // Load the other calendars user is authorized to view 
            cblAdditionalCalendars.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                foreach ( var calendar in new EventCalendarService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( c => c.Id != _calendarId )
                    .OrderBy( c => c.Name ) )
                {
                    if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        cblAdditionalCalendars.Items.Add( new ListItem( calendar.Name, calendar.Id.ToString() ) );
                    }
                }
            }
            cblAdditionalCalendars.SelectedIndexChanged += cblAdditionalCalendars_SelectedIndexChanged;

            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( All.Text, string.Empty ) );
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
                string eventItemId = PageParameter( "EventItemId" );

                if ( !string.IsNullOrWhiteSpace( eventItemId ) )
                {
                    ShowDetail( eventItemId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                nbIncorrectCalendarItem.Visible = false;
                nbNotAllowedToEdit.Visible = false;
                
                ShowItemAttributes();

                ShowDialog();
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
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["AudiencesState"] = JsonConvert.SerializeObject( AudiencesState, Formatting.None, jsonSetting );
            ViewState["ItemsState"] = JsonConvert.SerializeObject( ItemsState, Formatting.None, jsonSetting );
            ViewState["CampusesState"] = JsonConvert.SerializeObject( CampusesState, Formatting.None, jsonSetting );
            ViewState["SchedulesState"] = JsonConvert.SerializeObject( SchedulesState, Formatting.None, jsonSetting );;

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? eventItemId = PageParameter( pageReference, "EventItemId" ).AsIntegerOrNull();
            if ( eventItemId != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    EventItem eventItem = new EventItemService( rockContext ).Get( eventItemId.Value );
                    if ( eventItem != null )
                    {
                        breadCrumbs.Add( new BreadCrumb( eventItem.Name, pageReference ) );
                    }
                    else
                    {
                        breadCrumbs.Add( new BreadCrumb( "New Event Item", pageReference ) );
                    }
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentEventItem = GetEventItem( hfEventItemId.Value.AsInteger() );
            if ( currentEventItem != null )
            {
                ShowDetail( currentEventItem.Id );
            }
            else
            {
                string eventItemId = PageParameter( "EventItemId" );
                if ( !string.IsNullOrWhiteSpace( eventItemId ) )
                {
                    ShowDetail( eventItemId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion Control Methods

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventItem eventItem = null;

            using ( var rockContext = new RockContext() )
            {
                var eventItemService = new EventItemService( rockContext );
                var eventCalendarItemService = new EventCalendarItemService( rockContext );
                var eventItemAudienceService = new EventItemAudienceService( rockContext );
                var eventItemCampusService = new EventItemCampusService( rockContext );
                var eventItemScheduleService = new EventItemScheduleService( rockContext );
                var scheduleService = new ScheduleService( rockContext );

                int eventItemId = hfEventItemId.ValueAsInt();
                if ( eventItemId != 0 )
                {
                    eventItem = eventItemService
                        .Queryable( "EventItemAudiences,EventItemCampuses.EventItemSchedules" )
                        .Where( i => i.Id == eventItemId )
                        .FirstOrDefault();
                }

                if ( eventItem == null )
                {
                    eventItem = new EventItem();
                    eventItemService.Add( eventItem );
                }

                eventItem.Name = tbName.Text;
                eventItem.IsActive = cbIsActive.Checked;
                eventItem.Description = tbDescription.Text;
                eventItem.DetailsUrl = tbDetailUrl.Text;

                if ( imgupPhoto.BinaryFileId != null )
                {
                    eventItem.PhotoId = imgupPhoto.BinaryFileId.Value;
                }

                // Remove any audiences that were removed in the UI
                var uiAudiences = AudiencesState.Select( r => r.Guid ).ToList();
                foreach ( var eventItemAudience in eventItem.EventItemAudiences.Where( r => !uiAudiences.Contains( r.Guid ) ).ToList() )
                {
                    eventItem.EventItemAudiences.Remove( eventItemAudience );
                    eventItemAudienceService.Delete( eventItemAudience );
                }

                // Add or Update audiences from the UI
                foreach ( var eventItemAudienceState in AudiencesState )
                {
                    EventItemAudience eventItemAudience = eventItem.EventItemAudiences.Where( a => a.Guid == eventItemAudienceState.Guid ).FirstOrDefault();
                    if ( eventItemAudience == null )
                    {
                        eventItemAudience = new EventItemAudience();
                        eventItem.EventItemAudiences.Add( eventItemAudience );
                    }
                    eventItemAudience.CopyPropertiesFrom( eventItemAudienceState );
                }

                // remove any calendar items that removed in the UI
                var calendarIds = new List<int> { _calendarId };
                calendarIds.AddRange( cblAdditionalCalendars.SelectedValuesAsInt );
                var uiCalendarGuids = ItemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ).Select( a => a.Guid );
                foreach ( var eventCalendarItem in eventItem.EventCalendarItems.Where( a => !uiCalendarGuids.Contains( a.Guid ) ).ToList() )
                {
                    eventItem.EventCalendarItems.Remove( eventCalendarItem );
                    eventCalendarItemService.Delete( eventCalendarItem );
                }

                // Add or Update calendar items from the UI
                foreach ( var calendar in ItemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ) )
                {
                    var eventCalendarItem = eventItem.EventCalendarItems.Where( a => a.Guid == calendar.Guid ).FirstOrDefault();
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventItem.EventCalendarItems.Add( eventCalendarItem );
                    }
                    eventCalendarItem.CopyPropertiesFrom( calendar );
                }

                // remove any campuses that removed in the UI
                var uiCampuses = CampusesState
                    .Where( c => !c.Guid.Equals( Guid.Empty ) )
                    .Select( l => l.Guid )
                    .ToList();

                foreach ( var itemCampus in eventItem.EventItemCampuses
                    .Where( l => !uiCampuses.Contains( l.Guid ) )
                    .ToList() )
                {
                    // TODO: schedules should cascade from campus
                    foreach ( var schedule in itemCampus.EventItemSchedules )
                    {
                        itemCampus.EventItemSchedules.Remove( schedule );
                        eventItemScheduleService.Delete( schedule );
                    }
                    eventItem.EventItemCampuses.Remove( itemCampus );
                    eventItemCampusService.Delete( itemCampus );
                }

                // Add or Update the campus/schedule information from the UI
                foreach ( var uiCampus in CampusesState )
                {
                    var eventItemCampus = eventItem.EventItemCampuses.Where( a => a.Guid == uiCampus.Guid ).FirstOrDefault();
                    if ( eventItemCampus == null )
                    {
                        eventItemCampus = new EventItemCampus();
                        eventItem.EventItemCampuses.Add( eventItemCampus );
                    }
                    eventItemCampus.CopyPropertiesFrom( uiCampus );

                    foreach ( var uiSchedule in uiCampus.EventItemSchedules )
                    {
                        var eventItemSchedule = eventItemCampus.EventItemSchedules.Where( s => s.Guid == uiSchedule.Guid ).FirstOrDefault();
                        if ( eventItemSchedule == null )
                        {
                            eventItemSchedule = new EventItemSchedule();
                            eventItemCampus.EventItemSchedules.Add( eventItemSchedule );
                        }

                        if ( eventItemSchedule.Schedule == null )
                        {
                            var schedule = new Schedule();
                            scheduleService.Add( schedule );
                            eventItemSchedule.Schedule = schedule;
                        }

                        eventItemSchedule.CopyPropertiesFrom( uiSchedule );
                        eventItemSchedule.Schedule.CopyPropertiesFrom( uiSchedule.Schedule );
                    }
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !eventItem.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    foreach ( EventCalendarItem eventCalendarItem in eventItem.EventCalendarItems )
                    {
                        eventCalendarItem.LoadAttributes();
                        Rock.Attribute.Helper.GetEditValues( phAttributes, eventCalendarItem );
                        eventCalendarItem.SaveAttributeValues();
                    }
                } );

                var qryParams = new Dictionary<string, string>();
                qryParams["EventCalendarId"] = PageParameter( "EventCalendarId" );
                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams["EventCalendarId"] = PageParameter( "EventCalendarId" );
            NavigateToParentPage( qryParams );
        }

        #endregion Edit Events

        #region Control Events

        #region Audience Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                var selectedIds = AudiencesState.Select( a => a.DefinedValueId ).ToList();
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !selectedIds.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ShowDialog( "EventItemAudiences", true );
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            var audience = AudiencesState.FirstOrDefault( a => a.DefinedValue.Guid.Equals( guid ) );
            if ( audience != null )
            {
                AudiencesState.Remove( audience );
            }
            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAudience_Click( object sender, EventArgs e )
        {
            int? definedValueId = ddlAudience.SelectedValueAsInt();
            if ( definedValueId.HasValue )
            {
                EventItemAudience eventItemAudience = new EventItemAudience { DefinedValueId = definedValueId.Value };
                AudiencesState.Add( eventItemAudience );
            }

            BindAudienceGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_GridRebind( object sender, EventArgs e )
        {
            BindAudienceGrid();
        }

        #endregion

        #region Campus Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCampusDetails_Add( object sender, EventArgs e )
        {
            CampusesState.RemoveEntity( Guid.Empty );

            var eventItemCampus = new EventItemCampus { Guid = Guid.Empty };
            CampusesState.Add( eventItemCampus );

            ShowCampusDialog( eventItemCampus );
        }

        /// <summary>
        /// Handles the Edit event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusDetails_Edit( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            var eventItemCampus = CampusesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            if ( eventItemCampus != null )
            {
                ShowCampusDialog( eventItemCampus );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgCampusDetails_SaveClick( object sender, EventArgs e )
        {
            // Client-side validation should have caught these required fields, but if not, just return
            if ( !ppContact.PersonAliasId.HasValue || !SchedulesState.Any() )
            {
                return;
            }

            Guid? guid = hfCampusGuid.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var itemCampus = CampusesState.FirstOrDefault( c => c.Guid.Equals( guid ) );
                if ( itemCampus != null )
                {  

                    var rockContext = new RockContext();

                    int? newCampusId = ddlCampus.SelectedValueAsInt();
                    if ( itemCampus.CampusId != newCampusId )
                    {
                        itemCampus.CampusId = newCampusId;
                        if ( newCampusId.HasValue )
                        {
                            var campus = new CampusService( rockContext ).Get( newCampusId.Value );
                            itemCampus.Campus = campus;
                        }
                        else
                        {
                            itemCampus.Campus = null;
                        }
                    }

                    itemCampus.Location = tbLocation.Text;
                    itemCampus.RegistrationUrl = tbRegistration.Text;

                    if ( itemCampus.ContactPersonAliasId != ppContact.PersonAliasId.Value )
                    {
                        itemCampus.ContactPersonAliasId = ppContact.PersonAliasId.Value;
                        var personAlias = new PersonAliasService( rockContext ).Get( ppContact.PersonAliasId.Value );
                        if ( personAlias != null )
                        {
                            itemCampus.ContactPersonAlias = personAlias;
                        }
                    }

                    itemCampus.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                    itemCampus.ContactEmail = tbEmail.Text;

                    itemCampus.CampusNote = tbCampusNote.Text;

                    // Remove any schedules from state that were removed in UI
                    var uiScheduleGuids = SchedulesState.Select( s => s.Guid ).ToList();
                    itemCampus.EventItemSchedules = itemCampus.EventItemSchedules
                        .Where( s => uiScheduleGuids.Contains( s.Guid ) )
                        .ToList();

                    // Add or update any schedules still in the UI
                    foreach ( var uiSchedule in SchedulesState )
                    {
                        var itemCampusSchedule = itemCampus.EventItemSchedules.FirstOrDefault( s => s.Guid.Equals( uiSchedule.Guid ) );
                        if ( itemCampusSchedule == null )
                        {
                            itemCampusSchedule = new EventItemSchedule { Guid = uiSchedule.Guid,  };
                            itemCampus.EventItemSchedules.Add( itemCampusSchedule );
                        }

                        if ( itemCampusSchedule.Schedule == null )
                        {
                            itemCampusSchedule.Schedule = new Schedule();
                        }

                        itemCampusSchedule.ScheduleId = uiSchedule.ScheduleId;
                        itemCampusSchedule.ScheduleName = uiSchedule.ScheduleName;
                        itemCampusSchedule.Schedule.iCalendarContent = uiSchedule.Schedule.iCalendarContent;
                    }

                    // Set the Guid now (otherwise it will not be valid )
                    bool isNew = itemCampus.Guid == Guid.Empty;
                    if ( isNew )
                    {
                        itemCampus.Guid = Guid.NewGuid();
                    }

                    if ( !itemCampus.IsValid )
                    {
                        // If validation failed and this is new, reset the guid back to empty
                        if ( isNew )
                        {
                            itemCampus.Guid = Guid.Empty;
                        }
                        return;
                    }

                    BindCampusGrid();

                    HideDialog();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampusDetails_Delete( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            CampusesState.RemoveEntity( guid );

            BindCampusGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCampusDetails_GridRebind( object sender, EventArgs e )
        {
            BindCampusGrid();
        }

        #endregion

        #region EventItemSchedule Events

        /// <summary>
        /// Handles the Add event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSchedules_Add( object sender, EventArgs e )
        {
            SchedulesState.RemoveEntity( Guid.Empty );

            var itemSchedule = new EventItemSchedule { ScheduleId = 0, Guid = Guid.Empty, Schedule = new Schedule() };
            SchedulesState.Add( itemSchedule );

            ShowScheduleDialog( itemSchedule );
        }

        /// <summary>
        /// Handles the Edit event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSchedules_Edit( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            var itemSchedule = SchedulesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            if ( itemSchedule != null )
            {
                ShowScheduleDialog( itemSchedule );
            } 
        }

        /// <summary>
        /// Handles the Click event of the btnSaveSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveSchedule_Click( object sender, EventArgs e )
        {
            Guid? guid = hfScheduleGuid.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var eventItemSchedule = SchedulesState.FirstOrDefault( s => s.Guid.Equals( guid ) );
                if ( eventItemSchedule != null && eventItemSchedule.Schedule != null )
                {
                    eventItemSchedule.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
                    eventItemSchedule.ScheduleName = tbScheduleName.Text;

                    // Set the Guid now (otherwise it will not be valid )
                    bool isNew = eventItemSchedule.Guid == Guid.Empty;
                    if ( isNew )
                    {
                        eventItemSchedule.Guid = Guid.NewGuid();
                    }

                    if ( !eventItemSchedule.IsValid )
                    {
                        // If validation failed and this is new, reset the guid back to empty
                        if ( isNew )
                        {
                            eventItemSchedule.Guid = Guid.Empty;
                        }
                        return;
                    }

                    BindSchedulesGrid();

                    HideDialog();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSchedules_Delete( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            SchedulesState.RemoveEntity( guid );

            BindSchedulesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSchedules_GridRebind( object sender, EventArgs e )
        {
            BindSchedulesGrid();
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblAdditionalCalendars control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblAdditionalCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            if ( ppContact.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppContact.PersonId.Value )
                        .Select( p => new {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where (n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ))
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        })
                        .FirstOrDefault();

                    if ( string.IsNullOrWhiteSpace( tbEmail.Text ) && contactInfo != null )
                    {
                        tbEmail.Text = contactInfo.Email;
                    }
                    
                    if ( string.IsNullOrWhiteSpace( pnPhone.Text ) && contactInfo != null )
                    {
                        pnPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnHideDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHideDialog_Click( object sender, EventArgs e )
        {
            HideDialog();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        public void ShowDetail( int eventItemId )
        {
            EventItem eventItem = null;

            bool editAllowed = UserCanEdit;

            var rockContext = new RockContext();

            if ( !eventItemId.Equals( 0 ) )
            {
                eventItem = GetEventItem( eventItemId, rockContext );
            }

            if ( eventItem == null )
            {
                eventItem = new EventItem { Id = 0, IsActive = true, Name = "" };
            }

            // Only users that have Edit rights to block, or edit rights to the calendar (from query string) should be able to edit
            if ( !editAllowed )
            {
                var eventCalendar = new EventCalendarService( rockContext ).Get( _calendarId );
                if ( eventCalendar != null )
                {
                    editAllowed = eventCalendar.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            bool readOnly = true;

            if ( !editAllowed )
            {
                // User is not authorized
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItem.FriendlyTypeName );
            }
            else
            {
                nbEditModeMessage.Text = string.Empty;

                if ( eventItem.Id != 0 && !eventItem.EventCalendarItems.Any( i => i.EventCalendarId == _calendarId ) )
                {
                    // Item does not belong to calendar
                    nbIncorrectCalendarItem.Visible = true;
                }
                else
                {
                    readOnly = false;
                }
            }


            pnlEditDetails.Visible = !readOnly;
            this.HideSecondaryBlocks( !readOnly );

            if ( !readOnly )
            {
                nbEditModeMessage.Text = string.Empty;
                hfEventItemId.Value = eventItem.Id.ToString();
                ShowEditDetails( eventItem );
            }

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="eventItem">The eventItem.</param>
        private void ShowEditDetails( EventItem eventItem )
        {
            if ( eventItem.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( EventItem.FriendlyTypeName ).FormatAsHtmlTitle();
                hlStatus.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = eventItem.Name.FormatAsHtmlTitle();
                if ( eventItem.IsActive.Value )
                {
                    hlStatus.Text = "Active";
                    hlStatus.LabelType = LabelType.Success;
                }
                else
                {
                    hlStatus.Text = "Inactive";
                    hlStatus.LabelType = LabelType.Campus;
                }
            }

            tbName.Text = eventItem.Name;
            cbIsActive.Checked = eventItem.IsActive.Value;
            tbDescription.Text = eventItem.Description;
            tbDetailUrl.Text = eventItem.DetailsUrl;

            if ( eventItem.EventCalendarItems != null )
            {
                cblAdditionalCalendars.SetValues( eventItem.EventCalendarItems.Select( c => c.EventCalendarId ).ToList() );
            }

            CampusesState = eventItem.EventItemCampuses.ToList();
            wpCampusDetails.Expanded = CampusesState.Any();

            AudiencesState = eventItem.EventItemAudiences.ToList();
            ItemsState = eventItem.EventCalendarItems.ToList();

            ShowItemAttributes();
            
            BindAudienceGrid();
            
            BindCampusGrid();
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            var eventCalendarList = new List<int> { _calendarId };
            eventCalendarList.AddRange( cblAdditionalCalendars.SelectedValuesAsInt );

            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList )
                {
                    EventCalendarItem eventCalendarItem = ItemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        ItemsState.Add( eventCalendarItem );
                    }

                    eventCalendarItem.LoadAttributes();

                    if ( eventCalendarItem.Attributes.Count > 0 )
                    {
                        wpAttributes.Visible = true;
                        phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        PlaceHolder phcalAttributes = new PlaceHolder();
                        Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phAttributes, true, BlockValidationGroup );
                    }
                }
            }
        }

        /// <summary>
        /// Binds the audience grid.
        /// </summary>
        private void BindAudienceGrid()
        {
            var values = new List<DefinedValueCache>();
            AudiencesState.ForEach( a => values.Add( DefinedValueCache.Read( a.DefinedValueId ) ) );

            gAudiences.DataSource = values
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();
            gAudiences.DataBind();
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindCampusGrid()
        {
            gCampusDetails.DataSource = CampusesState
                .Where( c => !c.Guid.Equals( Guid.Empty ) )
                .Select( c => new
                {
                    c.Id,
                    c.Guid,
                    Campus = ( c.Campus != null ) ? c.Campus.Name : "All Campuses",
                    Location = c.Location,
                    Contact = ( c.ContactPersonAlias != null ) ? c.ContactPersonAlias.Person.FullName : "None",
                    Phone = c.ContactPhone,
                    Email = c.ContactEmail,
                    Registration = !String.IsNullOrWhiteSpace( c.RegistrationUrl ) ? "<i class='fa fa-check'></i>" : ""
                } )
                .OrderBy( c => c.Campus )
                .ToList();
            gCampusDetails.DataBind();
        }

        /// <summary>
        /// Shows the campus dialog.
        /// </summary>
        /// <param name="eventItemCampus">The event item campus.</param>
        private void ShowCampusDialog( EventItemCampus eventItemCampus )
        {
            hfCampusGuid.Value = eventItemCampus.Guid.ToString();

            ddlCampus.SetValue( eventItemCampus.CampusId ?? -1 );
            tbLocation.Text = eventItemCampus.Location;
            tbRegistration.Text = eventItemCampus.RegistrationUrl;

            ppContact.SetValue( eventItemCampus.ContactPersonAlias != null ? eventItemCampus.ContactPersonAlias.Person : null );
            pnPhone.Text = eventItemCampus.ContactPhone;
            tbEmail.Text = eventItemCampus.ContactEmail;

            tbCampusNote.Text = eventItemCampus.CampusNote;

            SchedulesState = new List<EventItemSchedule>();
            foreach ( var itemSchedule in eventItemCampus.EventItemSchedules )
            {
                var scheduleState = itemSchedule.Clone( false );
                scheduleState.Schedule = itemSchedule.Schedule != null ? itemSchedule.Schedule.Clone( false ) : new Schedule();
                SchedulesState.Add( scheduleState );
            }
            BindSchedulesGrid();

            ShowDialog( "EventItemCampuses", true );
        }

        /// <summary>
        /// Binds the schedules grid.
        /// </summary>
        private void BindSchedulesGrid()
        {
            gSchedules.DataSource = SchedulesState
                .Where( s => !s.Guid.Equals( Guid.Empty ) )
                .OrderBy( s => s.ScheduleName )
                .Select( s => new
                {
                    s.Id,
                    s.Guid,
                    Schedule = s.ScheduleName,
                    Details = s.Schedule.FriendlyScheduleText
                } )
                .ToList();
            gSchedules.DataBind();

            // Set hidden field so that it can be used for client-side validation to ensure at least one schedule was created
            hfSchedules.Value = SchedulesState.Any() ? "WeGotSome" : "";
        }

        /// <summary>
        /// Shows the schedule dialog.
        /// </summary>
        /// <param name="itemSchedule">The item schedule.</param>
        private void ShowScheduleDialog( EventItemSchedule itemSchedule )
        {
            hfScheduleGuid.Value = itemSchedule.Guid.ToString();

            tbScheduleName.Text = itemSchedule.ScheduleName;
            sbSchedule.iCalendarContent = itemSchedule.Schedule.iCalendarContent;
            lScheduleText.Text = itemSchedule.Schedule.FriendlyScheduleText;

            ShowDialog( "EventItemSchedules", true );
        }

        /// <summary>
        /// Gets the eventItem.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        /// <returns></returns>
        private EventItem GetEventItem( int eventItemId, RockContext rockContext = null )
        {
            string key = string.Format( "EventItem:{0}", eventItemId );
            EventItem eventItem = RockPage.GetSharedItem( key ) as EventItem;
            if ( eventItem == null )
            {
                rockContext = rockContext ?? new RockContext();
                eventItem = new EventItemService( rockContext ).Queryable()
                    .Where( e => e.Id == eventItemId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, eventItem );
            }

            return eventItem;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCES":
                    dlgAudience.Show();
                    break;

                case "EVENTITEMSCHEDULES":
                    dlgCampusDetails.Show();
                    dlgSchedule.Show();
                    break;

                case "EVENTITEMCAMPUSES":
                    dlgCampusDetails.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCES":
                    dlgAudience.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;

                case "EVENTITEMSCHEDULES":
                    dlgSchedule.Hide();
                    hfActiveDialog.Value = "EVENTITEMCAMPUSES";
                    break;

                case "EVENTITEMCAMPUSES":
                    dlgCampusDetails.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;
            }
        }

        #endregion
    }
}