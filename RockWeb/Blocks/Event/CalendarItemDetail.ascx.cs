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

        public List<EventItemAudience> AudiencesState { get; set; }
        public List<EventCalendarItem> ItemsState { get; set; }
        public List<EventItemCampus> CampusesState { get; set; }
        public EventItemCampus SelectedCampusState { get; set; } //Used to manage Schedules

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

            json = ViewState["SelectedCampusState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SelectedCampusState = new EventItemCampus();
            }
            else
            {
                SelectedCampusState = JsonConvert.DeserializeObject<EventItemCampus>( json );
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gEventItemAudiences.DataKeyNames = new string[] { "Guid" };
            gEventItemAudiences.Actions.ShowAdd = true;
            gEventItemAudiences.Actions.AddClick += gEventItemAudiences_Add;
            gEventItemAudiences.GridRebind += gEventItemAudiences_GridRebind;

            gEventItemCampuses.DataKeyNames = new string[] { "Guid" };
            gEventItemCampuses.Actions.ShowAdd = true;
            gEventItemCampuses.Actions.AddClick += gEventItemCampuses_Add;
            gEventItemCampuses.GridRebind += gEventItemCampuses_GridRebind;

            cblEventCalendars.SelectedIndexChanged += cblEventCalendars_SelectedIndexChanged;

            gEventItemSchedules.DataKeyNames = new string[] { "Guid" };
            gEventItemSchedules.Actions.ShowAdd = true;
            gEventItemSchedules.Actions.AddClick += gEventItemSchedules_Add;
            gEventItemSchedules.GridRebind += gEventItemSchedules_GridRebind;

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
                ShowItemAttributes();
                //ShowDialog();
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
            ViewState["SelectedCampusState"] = JsonConvert.SerializeObject( SelectedCampusState, Formatting.None, jsonSetting );
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

        #endregion Control Methods

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
                eventItem = eventItemService.Queryable( "EventItemAudiences,EventItemCampuses.EventItemSchedules" ).Where( ei => ei.Id == eventItemId ).FirstOrDefault();
                // remove any campuses that removed in the UI
                var selectedEventItemCampuses = CampusesState.Select( l => l.Guid );
                foreach ( var eventItemCampus in eventItem.EventItemCampuses.Where( l => !selectedEventItemCampuses.Contains( l.Guid ) ).ToList() )
                {
                    foreach ( var eventItemSchedule in eventItemCampus.EventItemSchedules )
                    {
                        eventItemCampus.EventItemSchedules.Remove( eventItemSchedule );
                        eventItemScheduleService.Delete( eventItemSchedule );
                    }
                    eventItem.EventItemCampuses.Remove( eventItemCampus );
                    eventItemCampusService.Delete( eventItemCampus );
                }

                // remove any calendar items that removed in the UI
                List<int> eventCalendarList = cblEventCalendars.SelectedValuesAsInt;
                var selectedEventCalendarItems = ItemsState.Where( i => eventCalendarList.Contains( i.EventCalendarId ) ).Select( a => a.Guid );
                foreach ( var eventCalendarItem in eventItem.EventCalendarItems.Where( a => !selectedEventCalendarItems.Contains( a.Guid ) ).ToList() )
                {
                    eventItem.EventCalendarItems.Remove( eventCalendarItem );
                    eventCalendarItemService.Delete( eventCalendarItem );
                }

                // Remove any audiences that were removed in the UI
                var selectedEventItemAudiences = AudiencesState.Select( r => r.Guid );
                foreach ( var eventItemAudience in eventItem.EventItemAudiences.Where( r => !selectedEventItemAudiences.Contains( r.Guid ) ).ToList() )
                {
                    eventItem.EventItemAudiences.Remove( eventItemAudience );
                    eventItemAudienceService.Delete( eventItemAudience );
                }
            }
            try
            {
                if ( tbDetailUrl.Text.Substring( 0, 4 ) != "http" )
                {
                    eventItem.DetailsUrl = String.Format( "http://{0}", tbDetailUrl.Text );
                }
                else
                {
                    eventItem.DetailsUrl = tbDetailUrl.Text;
                }
            }
            catch
            {
                if ( !String.IsNullOrWhiteSpace( tbDetailUrl.Text ) )
                {
                    eventItem.DetailsUrl = String.Format( "http://{0}", tbDetailUrl.Text );
                }
            }


            eventItem.Name = tbName.Text;
            eventItem.Description = tbDescription.Text;
            eventItem.IsActive = cbIsActive.Checked;

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

            if ( imgupPhoto.BinaryFileId != null )
            {
                eventItem.PhotoId = imgupPhoto.BinaryFileId.Value;
            }

            foreach ( EventItemCampus eventItemCampusState in CampusesState )
            {
                EventItemCampus eventItemCampus = eventItem.EventItemCampuses.Where( a => a.Guid == eventItemCampusState.Guid ).FirstOrDefault();
                if ( eventItemCampus == null )
                {
                    eventItemCampus = new EventItemCampus();
                    eventItem.EventItemCampuses.Add( eventItemCampus );
                }
                else
                {
                    eventItemCampusState.Id = eventItemCampus.Id;
                    eventItemCampusState.Guid = eventItemCampus.Guid;

                    var selectedEventItemSchedules = eventItemCampusState.EventItemSchedules.Select( s => s.Guid ).ToList();
                    foreach ( var eventItemSchedule in eventItemCampus.EventItemSchedules.Where( s => !selectedEventItemSchedules.Contains( s.Guid ) ).ToList() )
                    {
                        eventItemCampus.EventItemSchedules.Remove( eventItemSchedule );
                    }
                }

                eventItemCampus.CopyPropertiesFrom( eventItemCampusState );
                try
                {
                    eventItemCampus.CampusId = eventItemCampusState.Campus.Id;
                }
                catch
                {
                }
                var existingEventItemSchedules = eventItemCampus.EventItemSchedules.Select( s => s.Guid ).ToList();
                foreach ( var eventItemScheduleState in eventItemCampusState.EventItemSchedules.Where( s => !existingEventItemSchedules.Contains( s.Guid ) ).ToList() )
                {
                    eventItemCampus.EventItemSchedules.Add( eventItemScheduleState );
                }
            }

            List<int> eventCalendarIdList = cblEventCalendars.SelectedValuesAsInt;
            foreach ( int eventCalendarId in eventCalendarIdList )
            {
                EventCalendarItem eventCalendarItemState = ItemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                EventCalendarItem eventCalendarItem = eventItem.EventCalendarItems.Where( a => a.Guid == eventCalendarItemState.Guid ).FirstOrDefault();
                if ( eventCalendarItem == null )
                {
                    eventCalendarItem = new EventCalendarItem();
                    eventItem.EventCalendarItems.Add( eventCalendarItem );
                }
                eventCalendarItem.CopyPropertiesFrom( eventCalendarItemState );
            }

            // Check to see if user is still allowed to edit with selected eventItem type and parent eventItem
            if ( !IsUserAuthorized( Authorization.EDIT ) ) //!eventItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
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
                    eventCalendarItem.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, eventCalendarItem );
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
            var qryParams = new Dictionary<string, string>();
            qryParams["EventCalendarId"] = PageParameter( "EventCalendarId" );
            NavigateToParentPage( qryParams );
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
                    var qryParams = new Dictionary<string, string>();
                    qryParams["EventCalendarId"] = PageParameter( "EventCalendarId" );
                    NavigateToParentPage( qryParams );
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                var qryParams = new Dictionary<string, string>();
                qryParams["EventCalendarId"] = PageParameter( "EventCalendarId" );
                NavigateToParentPage( qryParams );
            }
        }

        #endregion Edit Events

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

        #region EventItemAudience Events

        protected void gEventItemAudiences_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            AudiencesState.RemoveEntity( rowGuid );
            BindEventItemAudiencesGrid();
        }

        protected void btnAddEventItemAudience_Click( object sender, EventArgs e )
        {
            DefinedValue eventItemAudienceValue = new DefinedValueService( new RockContext() ).Get( ddlAudience.SelectedValueAsInt().Value );
            EventItemAudience eventItemAudience = new EventItemAudience();
            eventItemAudience.DefinedValue = eventItemAudienceValue;
            eventItemAudience.DefinedValueId = eventItemAudienceValue.Id;
            // Controls will show warnings
            if ( !eventItemAudience.IsValid )
            {
                return;
            }

            if ( AudiencesState.Any( a => a.Guid.Equals( eventItemAudience.Guid ) ) )
            {
                AudiencesState.RemoveEntity( eventItemAudience.Guid );
            }
            AudiencesState.Add( eventItemAudience );

            BindEventItemAudiencesGrid();

            HideDialog();
        }

        private void gEventItemAudiences_GridRebind( object sender, EventArgs e )
        {
            BindEventItemAudiencesGrid();
        }

        private void gEventItemAudiences_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ddlAudience.Items.Clear();

            ddlAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            foreach ( EventItemAudience eventItemAudience in AudiencesState )
            {
                ddlAudience.Items.Remove( eventItemAudience.DefinedValue.Value );
            }

            ShowDialog( "EventItemAudiences", true );
        }

        #endregion

        #region EventItemSchedule Events

        protected void gEventItemSchedules_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            SelectedCampusState.EventItemSchedules.Remove( SelectedCampusState.EventItemSchedules.Where( s => s.Guid == rowGuid ).FirstOrDefault() );
            BindEventItemSchedulesGrid();
        }

        protected void btnAddEventItemSchedule_Click( object sender, EventArgs e )
        {
            EventItemSchedule eventItemSchedule = new EventItemSchedule();
            eventItemSchedule.Schedule = new Schedule();
            eventItemSchedule.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
            if ( !String.IsNullOrWhiteSpace( tbEventItemSchedule.Text ) )
            {
                eventItemSchedule.ScheduleName = tbEventItemSchedule.Text;
            }
            else
            {
                eventItemSchedule.ScheduleName = eventItemSchedule.Schedule.FriendlyScheduleText;
            }
            eventItemSchedule.ScheduleId = 0;

            SelectedCampusState.EventItemSchedules.Add( eventItemSchedule );

            BindEventItemSchedulesGrid();

            HideDialog();
            ShowDialog( "EventItemCampuses", true );
        }

        private void gEventItemSchedules_GridRebind( object sender, EventArgs e )
        {
            BindEventItemSchedulesGrid();
        }

        private void gEventItemSchedules_Add( object sender, EventArgs e )
        {
            tbEventItemSchedule.Text = String.Empty;
            sbSchedule.iCalendarContent = String.Empty;
            ShowDialog( "EventItemSchedules", true );
        }

        #endregion

        #region Campus Events

        protected void dlgEventItemCampus_SaveClick( object sender, EventArgs e )
        {
            if ( ppContact.PersonAliasId != null )
            {
                EventItemCampus eventItemCampus = null;
                Guid guid = hfAddEventItemCampusGuid.Value.AsGuid();
                if ( !guid.IsEmpty() )
                {
                    eventItemCampus = CampusesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
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
                eventItemCampus.ContactPersonAliasId = ppContact.PersonAliasId.Value;
                eventItemCampus.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                eventItemCampus.ContactEmail = tbEmail.Text;
                eventItemCampus.RegistrationUrl = tbRegistration.Text;
                eventItemCampus.CampusNote = tbCampusNote.Text;
                foreach ( EventItemSchedule schedule in SelectedCampusState.EventItemSchedules )
                {
                    eventItemCampus.EventItemSchedules.Add( schedule );
                }
                if ( !eventItemCampus.IsValid )
                {
                    return;
                }
                SelectedCampusState = null;
                if ( CampusesState.Any( a => a.Guid.Equals( eventItemCampus.Guid ) ) )
                {
                    CampusesState.RemoveEntity( eventItemCampus.Guid );
                }

                CampusesState.Add( eventItemCampus );

                BindEventItemCampusesGrid();

                HideDialog();
            }
        }

        protected void gEventItemCampuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            CampusesState.RemoveEntity( rowGuid );

            BindEventItemCampusesGrid();
        }

        private void gEventItemCampuses_GridRebind( object sender, EventArgs e )
        {
            BindEventItemCampusesGrid();
        }

        protected void gEventItemCampuses_Edit( object sender, RowEventArgs e )
        {
            Guid eventItemCampusGuid = (Guid)e.RowKeyValue;
            gEventItemCampuses_ShowEdit( eventItemCampusGuid );
        }

        protected void gEventItemCampuses_ShowEdit( Guid eventItemCampusGuid )
        {
            EventItemCampus eventItemCampus = CampusesState.FirstOrDefault( l => l.Guid.Equals( eventItemCampusGuid ) );
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
                SelectedCampusState = eventItemCampus;
                BindEventItemSchedulesGrid();
                ShowDialog( "EventItemCampuses", true );
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
            SelectedCampusState = new EventItemCampus();
            BindEventItemSchedulesGrid();
            ShowDialog( "EventItemCampuses", true );
        }

        #endregion

        protected void cblEventCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            if ( ppContact.PersonId.HasValue )
            {
                Person contact = new PersonService( new RockContext() ).Get( ppContact.PersonId.Value );
                tbEmail.Text = contact.Email;
                PhoneNumber phoneNumber = contact.PhoneNumbers.FirstOrDefault();
                if ( phoneNumber != null )
                {
                    pnPhone.Text = phoneNumber.NumberFormatted;
                }
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

            SetEditMode( true );

            tbName.Text = eventItem.Name;
            tbDescription.Text = eventItem.Description;
            cbIsActive.Checked = eventItem.IsActive.Value;
            tbDetailUrl.Text = eventItem.DetailsUrl;
            if ( CampusesState == null )
            {
                CampusesState = eventItem.EventItemCampuses.ToList();
            }
            if ( AudiencesState == null )
            {
                AudiencesState = eventItem.EventItemAudiences.ToList();
            }
            if ( ItemsState == null )
            {
                ItemsState = eventItem.EventCalendarItems.ToList();
            }

            var rockContext = new RockContext();

            var eventItemService = new EventItemService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns( eventItem.EventCalendarItems.ToList() );
            ShowItemAttributes();
            BindEventItemAudiencesGrid();
            BindEventItemCampusesGrid();
        }

        private void ShowItemAttributes()
        {
            List<int> eventCalendarList = cblEventCalendars.SelectedValuesAsInt;
            wpEventItemAttributes.Visible = false;
            phAttributes.Controls.Clear();

            foreach ( int eventCalendarId in eventCalendarList )
            {
                EventCalendarItem eventCalendarItem = ItemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                if ( eventCalendarItem == null )
                {
                    eventCalendarItem = new EventCalendarItem();
                    eventCalendarItem.EventCalendarId = eventCalendarId;
                }
                eventCalendarItem.LoadAttributes();
                if ( eventCalendarItem.Attributes.Count > 0 )
                {
                    wpEventItemAttributes.Visible = true;
                    phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", new EventCalendarService( new RockContext() ).Get( eventCalendarId ).Name ) ) );
                    PlaceHolder phcalAttributes = new PlaceHolder();
                    Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phAttributes, true, BlockValidationGroup );
                }
                ItemsState.Add( eventCalendarItem );
            }
        }

        private void BindEventItemAudiencesGrid()
        {
            SetEventItemAudienceListOrder( AudiencesState );
            gEventItemAudiences.DataSource = AudiencesState.Select( a => new
            {
                a.Id,
                a.Guid,
                Audience = a.DefinedValue.Value
            } ).ToList();
            gEventItemAudiences.DataBind();
        }

        private void BindEventItemSchedulesGrid()
        {
            gEventItemSchedules.DataSource = SelectedCampusState.EventItemSchedules.Select( s => new
            {
                s.Id,
                s.Guid,
                Schedule = s.ScheduleName
            } ).ToList();
            gEventItemSchedules.DataBind();
        }

        private void BindEventItemCampusesGrid()
        {
            SetEventItemCampusListOrder( CampusesState );
            gEventItemCampuses.DataSource = CampusesState.Select( c => new
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
        private void LoadDropDowns( List<EventCalendarItem> itemList )
        {
            ddlAudience.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            
            cblEventCalendars.Items.Clear();
            foreach ( var calendar in new EventCalendarService( new RockContext() )
                .Queryable().AsNoTracking()
                .OrderBy( c => c.Name ) )
            {
                if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    cblEventCalendars.Items.Add( new ListItem( calendar.Name, calendar.Id.ToString() ) );
                }
            }
                
            if ( !Page.IsPostBack )
            {
                string eventItemId = PageParameter( "EventItemId" );
                if ( !string.IsNullOrWhiteSpace( eventItemId ) )
                {
                    if ( eventItemId.AsInteger() == 0 )
                    {
                        string eventCalendarId = PageParameter( "EventCalendarId" );
                        if ( !string.IsNullOrWhiteSpace( eventItemId ) )
                        {
                            cblEventCalendars.SetValue( eventCalendarId );
                        }
                    }
                    else
                    {
                        cblEventCalendars.SetValues( itemList.Select( i => i.EventCalendarId.ToString() ).ToList() );
                    }
                }
            }
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
                    dlgEventItemAudiences.Show();
                    break;

                case "EVENTITEMSCHEDULES":
                    dlgEventItemSchedules.Show();
                    break;

                case "EVENTITEMCAMPUSES":
                    dlgEventItemCampus.Show();
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
                    dlgEventItemAudiences.Hide();
                    break;

                case "EVENTITEMSCHEDULES":
                    dlgEventItemSchedules.Hide();
                    break;

                case "EVENTITEMCAMPUSES":
                    dlgEventItemCampus.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetEventItemAudienceListOrder( List<EventItemAudience> eventItemAudienceList )
        {
            if ( eventItemAudienceList != null )
            {
                if ( eventItemAudienceList.Any() )
                {
                    int order = 0;
                    eventItemAudienceList.OrderBy( a => a.DefinedValue.Order ).ThenBy( a => a.DefinedValue.Value ).ToList().ForEach( a => a.DefinedValue.Order = order++ );
                }
            }
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetEventItemCampusListOrder( List<EventItemCampus> eventItemCampusList )
        {
            if ( eventItemCampusList != null )
            {
                if ( eventItemCampusList.Any() )
                {
                    eventItemCampusList.OrderBy( c => ( c.Campus != null ) ? c.Campus.Name : "All Campuses" ).ThenBy( c => ( c.ContactPersonAlias != null ) ? c.ContactPersonAlias.Name : "Unknown" ).ToList();
                }
            }
        }

        #endregion
    }
}