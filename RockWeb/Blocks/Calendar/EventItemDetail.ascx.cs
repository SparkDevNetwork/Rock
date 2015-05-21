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

namespace RockWeb.Blocks.Calendar
{
    [DisplayName( "Event Item Detail" )]
    [Category( "Event Calendar" )]
    [Description( "Displays the details of the given eventItem." )]
    [BooleanField( "Show Edit", "", true, "", 2 )]
    public partial class EventItemDetail : RockBlock, IDetailBlock
    {

        #region Properties

        public List<EventItemAudience> EventItemAudiencesState { get; set; }

        public List<EventCalendarItem> EventCalendarItemsState { get; set; }

        public List<EventItemCampus> EventItemCampusesState { get; set; }

        public EventItemCampus _selectedEventItemCampusState { get; set; } //Used to manage Schedules

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["EventItemAudiencesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItemAudiencesState = new List<EventItemAudience>();
            }
            else
            {
                EventItemAudiencesState = JsonConvert.DeserializeObject<List<EventItemAudience>>( json );
            }

            json = ViewState["EventCalendarItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventCalendarItemsState = new List<EventCalendarItem>();
            }
            else
            {
                EventCalendarItemsState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }

            json = ViewState["SelectedEventItemCampusState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                _selectedEventItemCampusState = new EventItemCampus();
            }
            else
            {
                _selectedEventItemCampusState = JsonConvert.DeserializeObject<EventItemCampus>( json );
            }

            json = ViewState["EventItemCampusesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItemCampusesState = new List<EventItemCampus>();
            }
            else
            {
                EventItemCampusesState = JsonConvert.DeserializeObject<List<EventItemCampus>>( json );
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

            gEventItemCampuses.DataKeyNames = new string[] { "Guid" };
            gEventItemCampuses.Actions.ShowAdd = true;
            gEventItemCampuses.Actions.AddClick += gEventItemCampuses_Add;
            gEventItemCampuses.GridRebind += gEventItemCampuses_GridRebind;

            cblCalendars.SelectedIndexChanged += cblCalendars_SelectedIndexChanged;

            gSchedules.DataKeyNames = new string[] { "Guid" };
            gSchedules.Actions.ShowAdd = true;
            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlEventItemList );
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
                nbNotAllowedToEdit.Visible = false;

                ShowDialog();
            }

            // Rebuild the attribute controls on postback based on eventItem type
            if ( pnlDetails.Visible )
            {
                //TODO: Build attributes based off of calendars cbl

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

            ViewState["EventItemAudiencesState"] = JsonConvert.SerializeObject( EventItemAudiencesState, Formatting.None, jsonSetting );
            ViewState["EventCalendarItemsState"] = JsonConvert.SerializeObject( EventCalendarItemsState, Formatting.None, jsonSetting );
            ViewState["EventItemCampusesState"] = JsonConvert.SerializeObject( EventItemCampusesState, Formatting.None, jsonSetting );
            ViewState["SelectedEventItemCampusState"] = JsonConvert.SerializeObject( _selectedEventItemCampusState, Formatting.None, jsonSetting );
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
                EventItem eventItem = new EventItemService( new RockContext() ).Get( eventItemId.Value );
                if ( eventItem != null )
                {
                    breadCrumbs.Add( new BreadCrumb( eventItem.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Event Item", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventItem eventItem;

            RockContext rockContext = new RockContext();

            EventItemService eventItemService = new EventItemService( rockContext );
            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
            EventItemCampusService eventItemCampusService = new EventItemCampusService( rockContext );
            EventItemAudienceService eventItemAudienceService = new EventItemAudienceService( rockContext );
            EventItemScheduleService eventItemScheduleService = new EventItemScheduleService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            int eventItemId = int.Parse( hfEventItemId.Value );

            if ( eventItemId == 0 )
            {
                eventItem = new EventItem();
                eventItem.Name = string.Empty;
            }
            else
            {
                eventItem = eventItemService.Queryable( "EventItemSchedules, EventItemAudiences, EventItemCampuses" ).Where( ei => ei.Id == eventItemId ).FirstOrDefault();
                // remove any campuses that removed in the UI
                var selectedCampuses = EventItemCampusesState.Select( l => l.Guid );
                foreach ( var campus in eventItem.EventItemCampuses.Where( l => !selectedCampuses.Contains( l.Guid ) ).ToList() )
                {
                    foreach ( var schedule in campus.EventItemSchedules )
                    {
                        campus.EventItemSchedules.Remove( schedule );
                        eventItemScheduleService.Delete( schedule );
                    }
                    eventItem.EventItemCampuses.Remove( campus );
                    eventItemCampusService.Delete( campus );
                }

                // remove any calendar items that removed in the UI
                List<int> calendarList = cblCalendars.SelectedValuesAsInt;
                var selectedCalendarItems = EventCalendarItemsState.Where( i => calendarList.Contains( i.EventCalendarId ) ).Select( a => a.Guid );
                foreach ( var calendarItem in eventItem.EventCalendarItems.Where( a => !selectedCalendarItems.Contains( a.Guid ) ).ToList() )
                {
                    eventItem.EventCalendarItems.Remove( calendarItem );
                    eventCalendarItemService.Delete( calendarItem );
                }

                // Remove any audiences that were removed in the UI
                var selectedAudiences = EventItemAudiencesState.Select( r => r.Guid );
                foreach ( var audience in eventItem.EventItemAudiences.Where( r => !selectedAudiences.Contains( r.Guid ) ).ToList() )
                {
                    eventItem.EventItemAudiences.Remove( audience );
                    eventItemAudienceService.Delete( audience );
                }
            }

            eventItem.Name = tbName.Text;
            eventItem.Description = tbDescription.Text;
            eventItem.IsActive = cbIsActive.Checked;

            eventItem.DetailsUrl = tbDetailUrl.Text;

            foreach ( var audienceState in EventItemAudiencesState )
            {
                EventItemAudience audience = eventItem.EventItemAudiences.Where( a => a.Guid == audienceState.Guid ).FirstOrDefault();
                if ( audience == null )
                {
                    audience = new EventItemAudience();
                    eventItem.EventItemAudiences.Add( audience );
                }

                audience.CopyPropertiesFrom( audienceState );
            }

            if ( imgupPhoto.BinaryFileId != null )
            {
                eventItem.PhotoId = imgupPhoto.BinaryFileId.Value;
            }

            foreach ( EventItemCampus campusState in EventItemCampusesState )
            {
                EventItemCampus campus = eventItem.EventItemCampuses.Where( a => a.Guid == campusState.Guid ).FirstOrDefault();
                if ( campus == null )
                {
                    campus = new EventItemCampus();
                    eventItem.EventItemCampuses.Add( campus );
                }

                campus.CopyPropertiesFrom( campusState );
            }

            List<int> calendarIdList = cblCalendars.SelectedValuesAsInt;
            foreach ( int calendarId in calendarIdList )
            {
                EventCalendarItem itemState = EventCalendarItemsState.FirstOrDefault( i => i.EventCalendarId == calendarId );
                itemState.LoadAttributes();
                Rock.Attribute.Helper.GetEditValues( phAttributes, itemState );

                EventCalendarItem item = eventItem.EventCalendarItems.Where( a => a.Guid == itemState.Guid ).FirstOrDefault();
                if ( item == null )
                {
                    item = new EventCalendarItem();
                    eventItem.EventCalendarItems.Add( item );
                }

                item.CopyPropertiesFrom( itemState );
            }

            // Check to see if user is still allowed to edit with selected eventItem type and parent eventItem
            if ( !eventItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbNotAllowedToEdit.Visible = true;
                return;
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
                var adding = eventItem.Id.Equals( 0 );
                if ( adding )
                {
                    eventItemService.Add( eventItem );
                }

                rockContext.SaveChanges();
                foreach ( EventCalendarItem eventCalendarItem in eventItem.EventCalendarItems )
                {
                    eventCalendarItem.SaveAttributeValues();
                }
                if ( adding )
                {
                    // add ADMINISTRATE to the person who added the eventItem 
                    Rock.Security.Authorization.AllowPerson( eventItem, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
                }

                eventItem.SaveAttributeValues( rockContext );

                rockContext.SaveChanges();
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfEventItemId.Value.Equals( "0" ) )
            {
                int? parentEventItemId = PageParameter( "ParentEventItemId" ).AsIntegerOrNull();
                if ( parentEventItemId.HasValue )
                {
                    // Cancelling on Add, and we know the parentEventItemID, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( parentEventItemId != 0 )
                    {
                        qryParams["EventItemId"] = parentEventItemId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                NavigateToParentPage();
            }
        }

        #endregion

        #region Control Events

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
        #region Audience Events

        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            EventItemAudiencesState.RemoveEntity( rowGuid );

            BindAudiencesGrid();
        }
        protected void btnAddAudience_Click( object sender, EventArgs e )
        {
            DefinedValue audienceValue = new DefinedValueService( new RockContext() ).Get( ddlAudience.SelectedValueAsInt().Value );
            EventItemAudience audience = new EventItemAudience();
            audience.DefinedValue = audienceValue;
            // Controls will show warnings
            if ( !audience.IsValid )
            {
                return;
            }

            if ( EventItemAudiencesState.Any( a => a.Guid.Equals( audience.Guid ) ) )
            {
                EventItemAudiencesState.RemoveEntity( audience.Guid );
            }
            EventItemAudiencesState.Add( audience );

            BindAudiencesGrid();

            HideDialog();
        }
        private void gAudiences_GridRebind( object sender, EventArgs e )
        {
            BindAudiencesGrid();
        }
        private void gAudiences_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ddlAudience.Items.Clear();

            ddlAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            foreach ( EventItemAudience audience in EventItemAudiencesState )
            {
                ddlAudience.Items.Remove( audience.DefinedValue.Value );
            }

            ShowDialog( "Audiences", true );

        }
        #endregion

        #region Schedule Events
        protected void gSchedules_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            _selectedEventItemCampusState.EventItemSchedules.Remove( _selectedEventItemCampusState.EventItemSchedules.Where( s => s.Guid == rowGuid ).FirstOrDefault() );
            BindSchedulesGrid();
        }
        protected void btnAddSchedule_Click( object sender, EventArgs e )
        {
            EventItemSchedule eventItemSchedule = new EventItemSchedule();
            eventItemSchedule.Schedule = new Schedule();
            eventItemSchedule.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
            eventItemSchedule.ScheduleName = tbSchedule.Text;

            _selectedEventItemCampusState.EventItemSchedules.Add( eventItemSchedule );

            BindSchedulesGrid();

            HideDialog();
        }
        private void gSchedules_GridRebind( object sender, EventArgs e )
        {
            BindSchedulesGrid();
        }
        private void gSchedules_Add( object sender, EventArgs e )
        {
            tbSchedule.Text = String.Empty;
            sbSchedule.iCalendarContent = null;
            ShowDialog( "Schedules", true );
        }
        #endregion

        #region Campus Events
        protected void dlgCampus_SaveClick( object sender, EventArgs e )
        {
            if ( ppContact.PersonAliasId != null )
            {
                EventItemCampus eventItemCampus = null;
                Guid guid = hfAddEventItemCampusGuid.Value.AsGuid();
                if ( !guid.IsEmpty() )
                {
                    eventItemCampus = EventItemCampusesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
                }

                if ( eventItemCampus == null )
                {
                    eventItemCampus = new EventItemCampus();
                }
                eventItemCampus.Campus = null;
                try
                {
                    eventItemCampus.Campus = new CampusService( new RockContext() ).Get( ddlCampus.SelectedValueAsId().Value );
                }
                catch
                {
                    eventItemCampus.CampusId = null;
                }
                eventItemCampus.Location = tbLocation.Text;
                eventItemCampus.ContactPersonAlias = new PersonAliasService( new RockContext() ).Get( ppContact.PersonAliasId.Value );
                eventItemCampus.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                eventItemCampus.ContactEmail = tbEmail.Text;
                eventItemCampus.RegistrationUrl = tbRegistration.Text;
                eventItemCampus.CampusNote = tbCampusNote.Text;
                foreach ( EventItemSchedule schedule in _selectedEventItemCampusState.EventItemSchedules )
                {
                    eventItemCampus.EventItemSchedules.Add( schedule );
                }
                _selectedEventItemCampusState = null;
                if ( EventItemCampusesState.Any( a => a.Guid.Equals( eventItemCampus.Guid ) ) )
                {
                    EventItemCampusesState.RemoveEntity( eventItemCampus.Guid );
                }

                EventItemCampusesState.Add( eventItemCampus );

                BindCampusesGrid();

                HideDialog();
            }

        }

        protected void gEventItemCampuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            EventItemCampusesState.RemoveEntity( rowGuid );

            BindCampusesGrid();
        }

        private void gEventItemCampuses_GridRebind( object sender, EventArgs e )
        {
            BindCampusesGrid();
        }

        protected void gEventItemCampuses_Edit( object sender, RowEventArgs e )
        {

            Guid eventItemCampusGuid = (Guid)e.RowKeyValue;
            gEventItemCampuses_ShowEdit( eventItemCampusGuid );
        }

        protected void gEventItemCampuses_ShowEdit( Guid eventItemCampusGuid )
        {
            EventItemCampus eventItemCampus = EventItemCampusesState.FirstOrDefault( l => l.Guid.Equals( eventItemCampusGuid ) );
            if ( eventItemCampus != null )
            {
                ddlCampus.DataSource = CampusCache.All();
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
                if ( eventItemCampus.Campus == null )
                {
                    ddlCampus.SelectedValue = "-1";
                }
                else
                {
                    ddlCampus.SelectedValue = eventItemCampus.Campus.Id.ToString();
                }
                ppContact.PersonId = eventItemCampus.ContactPersonAlias.PersonId;
                pnPhone.Text = eventItemCampus.ContactPhone;
                tbLocation.Text = eventItemCampus.Location;
                tbEmail.Text = eventItemCampus.ContactEmail;
                tbRegistration.Text = eventItemCampus.RegistrationUrl;
                tbCampusNote.Text = eventItemCampus.CampusNote;
                hfAddEventItemCampusGuid.Value = eventItemCampusGuid.ToString();
                _selectedEventItemCampusState = eventItemCampus;
                BindSchedulesGrid();
                ShowDialog( "Campuses", true );
            }

        }

        private void gEventItemCampuses_Add( object sender, EventArgs e )
        {
            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
            ppContact.SelectedValue = null;
            pnPhone.Text = String.Empty;
            tbLocation.Text = string.Empty;
            tbEmail.Text = String.Empty;
            tbRegistration.Text = String.Empty;
            tbCampusNote.Text = String.Empty;
            hfAddEventItemCampusGuid.Value = Guid.Empty.ToString();
            _selectedEventItemCampusState = new EventItemCampus();
            BindSchedulesGrid();
            ShowDialog( "Campuses", true );
        }
        #endregion

        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        private void ShowItemAttributes()
        {
            List<int> calendarIdList = cblCalendars.SelectedValuesAsInt;
            foreach ( int calendarId in calendarIdList )
            {
                EventCalendarItem item = EventCalendarItemsState.FirstOrDefault( i => i.EventCalendarId == calendarId );
                if ( item == null )
                {
                    item = new EventCalendarItem();
                    item.EventCalendarId = calendarId;
                }
                item.LoadAttributes();
                phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", new EventCalendarService( new RockContext() ).Get( calendarId ).Name ) ) );
                PlaceHolder phcalAttributes = new PlaceHolder();
                Rock.Attribute.Helper.AddEditControls( item, phAttributes, true, BlockValidationGroup );
                EventCalendarItemsState.Add( item );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        /// <param name="parentEventItemId">The parent eventItem identifier.</param>
        public void ShowDetail( int eventItemId )
        {
            EventItem eventItem = null;

            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            RockContext rockContext = null;

            if ( !eventItemId.Equals( 0 ) )
            {
                eventItem = GetEventItem( eventItemId, rockContext );
            }

            if ( eventItem == null )
            {
                eventItem = new EventItem { Id = 0, IsActive = true, Name = "" };

                rockContext = rockContext ?? new RockContext();
            }

            viewAllowed = editAllowed || eventItem.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = IsUserAuthorized( Authorization.EDIT ) || eventItem.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            hfEventItemId.Value = eventItem.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItem.FriendlyTypeName );
            }

            if ( readOnly )
            {
                SetEditMode( false );
            }
            else
            {
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
                hlInactive.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = eventItem.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = eventItem.Name;
            tbDescription.Text = eventItem.Description;
            cbIsActive.Checked = eventItem.IsActive.Value;
            if ( EventItemCampusesState == null )
            {
                EventItemCampusesState = eventItem.EventItemCampuses.ToList();
            }
            if ( EventItemAudiencesState == null )
            {
                EventItemAudiencesState = eventItem.EventItemAudiences.ToList();
            }
            if ( EventCalendarItemsState == null )
            {
                EventCalendarItemsState = eventItem.EventCalendarItems.ToList();
            }

            var rockContext = new RockContext();

            var eventItemService = new EventItemService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns();
            ShowItemAttributes();
            // BindSchedulesGrid();
            BindAudiencesGrid();
            BindCampusesGrid();
            //ddlCampus.SetValue( eventItem.CampusId );

        }

        private void BindAudiencesGrid()
        {
            SetAudienceListOrder( EventItemAudiencesState );
            gAudiences.DataSource = EventItemAudiencesState.Select( a => new
            {
                a.Id,
                a.Guid,
                Audience = a.DefinedValue.Value
            } ).ToList();
            gAudiences.DataBind();
        }

        private void BindSchedulesGrid()
        {
            gSchedules.DataSource = _selectedEventItemCampusState.EventItemSchedules.Select( s => new
            {
                s.Id,
                s.Guid,
                Schedule = s.ScheduleName
            } ).ToList();
            gSchedules.DataBind();
        }

        private void BindCampusesGrid()
        {
            SetCampusListOrder( EventItemCampusesState );
            gEventItemCampuses.DataSource = EventItemCampusesState.Select( c => new
            {
                c.Id,
                c.Guid,
                Campus = ( c.Campus != null ) ? c.Campus.Name : "All Campuses",
                Location = c.Location,
                Contact = ( c.ContactPersonAlias != null ) ? c.ContactPersonAlias.Person.FullName : "Unknown",
                Phone = c.ContactPhone,
                Email = c.ContactEmail,
                Registration = !String.IsNullOrWhiteSpace( c.RegistrationUrl ) ? "<i class='fa fa-check'></i>" : ""
            } ).ToList();
            gEventItemCampuses.DataBind();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            this.HideSecondaryBlocks( editable );
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
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            cblCalendars.DataSource = new EventCalendarService( new RockContext() ).Queryable().Select( e => e.Name ).ToList();
            cblCalendars.DataBind();
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
                case "AUDIENCES":
                    dlgAudiences.Show();
                    break;
                case "SCHEDULES":
                    dlgSchedules.Show();
                    break;
                case "CAMPUSES":
                    dlgCampus.Show();
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
                case "AUDIENCES":
                    dlgAudiences.Hide();
                    break;
                case "SCHEDULES":
                    dlgSchedules.Hide();
                    break;
                case "CAMPUSES":
                    dlgCampus.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }


        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetAudienceListOrder( List<EventItemAudience> audienceList )
        {
            if ( audienceList != null )
            {
                if ( audienceList.Any() )
                {
                    int order = 0;
                    audienceList.OrderBy( a => a.DefinedValue.Order ).ThenBy( a => a.DefinedValue.Value ).ToList().ForEach( a => a.DefinedValue.Order = order++ );
                }
            }

        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetCampusListOrder( List<EventItemCampus> campusList )
        {
            if ( campusList != null )
            {
                if ( campusList.Any() )
                {
                    campusList.OrderBy( c => ( c.Campus != null ) ? c.Campus.Name : "All Campuses" ).ThenBy( c => ( c.ContactPersonAlias != null ) ? c.ContactPersonAlias.Name : "Unknown" ).ToList();
                }
            }
        }

        #endregion
    }
}