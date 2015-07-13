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
    /// Displays the details of a given calendar item at a campus.
    /// </summary>
    [DisplayName( "Calendar Item Campus Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given calendar item at a campus." )]

    public partial class CalendarItemCampusDetail : RockBlock, IDetailBlock
    {
        #region Properties

        public EventItemCampusGroupMap LinkageState { get; set; }
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

            string json = ViewState["LinkageState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                LinkageState = new EventItemCampusGroupMap();
            }
            else
            {
                LinkageState = JsonConvert.DeserializeObject<EventItemCampusGroupMap>( json );
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

            gSchedules.DataKeyNames = new string[] { "Guid" };
            gSchedules.Actions.ShowAdd = true;
            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;

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
                ShowDetail( PageParameter( "EventItemCampusId" ).AsInteger() );
            }
            else
            {
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

            ViewState["LinkageState"] = JsonConvert.SerializeObject( LinkageState, Formatting.None, jsonSetting );
            ViewState["SchedulesState"] = JsonConvert.SerializeObject( SchedulesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
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
            EventItemCampus eventItemCampus = null;

            using ( var rockContext = new RockContext() )
            {
                var eventItemCampusService = new EventItemCampusService( rockContext );
                var eventItemScheduleService = new EventItemScheduleService( rockContext );
                var eventItemCampusGroupMapService = new EventItemCampusGroupMapService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                var scheduleService = new ScheduleService( rockContext );

                int eventItemCampusId = hfEventItemCampusId.ValueAsInt();
                if ( eventItemCampusId != 0 )
                {
                    eventItemCampus = eventItemCampusService
                        .Queryable( "Linkages,EventItemSchedules" )
                        .Where( i => i.Id == eventItemCampusId )
                        .FirstOrDefault();
                }

                if ( eventItemCampus == null )
                {
                    eventItemCampus = new EventItemCampus{ EventItemId = PageParameter("EventItemId").AsInteger() };
                    eventItemCampusService.Add( eventItemCampus );
                }

                int? newCampusId = ddlCampus.SelectedValueAsInt();
                if ( eventItemCampus.CampusId != newCampusId )
                {
                    eventItemCampus.CampusId = newCampusId;
                    if ( newCampusId.HasValue )
                    {
                        var campus = new CampusService( rockContext ).Get( newCampusId.Value );
                        eventItemCampus.Campus = campus;
                    }
                    else
                    {
                        eventItemCampus.Campus = null;
                    }
                }

                eventItemCampus.Location = tbLocation.Text;

                if ( !eventItemCampus.ContactPersonAliasId.Equals( ppContact.PersonAliasId ))
                {
                    PersonAlias personAlias = null;
                    eventItemCampus.ContactPersonAliasId = ppContact.PersonAliasId;
                    if ( eventItemCampus.ContactPersonAliasId.HasValue )
                    {
                        personAlias = new PersonAliasService( rockContext ).Get( eventItemCampus.ContactPersonAliasId.Value );
                    }

                    if ( personAlias != null )
                    {
                        eventItemCampus.ContactPersonAlias = personAlias;
                    }
                }

                eventItemCampus.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                eventItemCampus.ContactEmail = tbEmail.Text;
                eventItemCampus.CampusNote = tbCampusNote.Text;

                // Remove any linkage no longer in UI
                Guid uiLinkageGuid = LinkageState != null ? LinkageState.Guid : Guid.Empty;
                foreach( var linkage in eventItemCampus.Linkages.Where( l => !l.Guid.Equals(uiLinkageGuid)).ToList())
                {
                    eventItemCampus.Linkages.Remove( linkage );
                    eventItemCampusGroupMapService.Delete( linkage );
                }

                // Add/Update linkage in UI
                if ( !uiLinkageGuid.Equals( Guid.Empty ))
                {
                    var linkage = eventItemCampus.Linkages.Where( l => l.Guid.Equals( uiLinkageGuid)).FirstOrDefault();
                    if ( linkage == null )
                    {
                        linkage = new EventItemCampusGroupMap();
                        eventItemCampus.Linkages.Add( linkage );
                    }

                    linkage.CopyPropertiesFrom( LinkageState );

                    // If a new registration instance was created in UI
                    if ( !linkage.RegistrationInstanceId.HasValue && LinkageState.RegistrationInstance != null )
                    {
                        var registrationInstance = new RegistrationInstance();
                        registrationInstanceService.Add( registrationInstance );
                        registrationInstance.CopyPropertiesFrom( LinkageState.RegistrationInstance );

                        linkage.RegistrationInstance = registrationInstance;
                    }

                }

                // Remove any schedules not in the uI
                List<Guid> uiScheduleGuids = SchedulesState.Select( s => s.Guid ).ToList();
                foreach( var schedule in eventItemCampus.EventItemSchedules.Where( s => !uiScheduleGuids.Contains( s.Guid )).ToList())
                {
                    eventItemCampus.EventItemSchedules.Remove( schedule );
                    eventItemScheduleService.Delete( schedule );
                }

                // Add or Update the schedule information from the UI
                foreach ( var uiSchedule in SchedulesState )
                {
                    var eventItemSchedule = eventItemCampus.EventItemSchedules.Where( s => s.Guid == uiSchedule.Guid ).FirstOrDefault();
                    if ( eventItemSchedule == null )
                    {
                        eventItemSchedule = new EventItemSchedule();
                        eventItemScheduleService.Add( eventItemSchedule );
                        eventItemCampus.EventItemSchedules.Add( eventItemSchedule );
                    }

                    eventItemSchedule.CopyPropertiesFrom( uiSchedule );

                    if ( eventItemSchedule.Schedule == null )
                    {
                        var schedule = new Schedule();
                        scheduleService.Add( schedule );
                        eventItemSchedule.Schedule = schedule;
                    }

                    eventItemSchedule.Schedule.CopyPropertiesFrom( uiSchedule.Schedule );
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !eventItemCampus.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                rockContext.SaveChanges();

                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
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
            qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
            qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
            NavigateToParentPage( qryParams );
        }

        #endregion Edit Events

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbCalendarDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarDetail_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var eventItem = new EventCalendarItemService( rockContext )
                    .Get( PageParameter( "EventItemId" ).AsInteger() );

                if ( eventItem != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "EventCalendarId", eventItem.EventCalendarId.ToString() );

                    var pageCache = PageCache.Read( RockPage.PageId );
                    if ( pageCache != null && pageCache.ParentPage != null && pageCache.ParentPage.ParentPage != null )
                    {
                        NavigateToPage( pageCache.ParentPage.ParentPage.Guid, qryParams );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCalendarItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarItem_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var eventItem = new EventCalendarItemService( rockContext )
                    .Get( PageParameter( "EventItemId" ).AsInteger() );

                if ( eventItem != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "EventItemId", eventItem.Id.ToString() );
                    qryParams.Add( "EventCalendarId", eventItem.EventCalendarId.ToString() );
                    NavigateToParentPage( qryParams );
                }
            }
        }

        #endregion

        #region Linkage Events

        /// <summary>
        /// Handles the Click event of the lbCreateNewRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateNewRegistration_Click( object sender, EventArgs e )
        {
            LinkageState = new EventItemCampusGroupMap { Guid = Guid.Empty };
            ShowNewLinkageDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbLinkToExistingRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkToExistingRegistration_Click( object sender, EventArgs e )
        {
            LinkageState = new EventItemCampusGroupMap { Guid = Guid.Empty };
            ShowExistingLinkageDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbEditRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditRegistration_Click( object sender, EventArgs e )
        {
            if ( LinkageState.RegistrationInstanceId == 0 )
            {
                ShowNewLinkageDialog();
            }
            else
            {
                ShowExistingLinkageDialog();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteRegistration_Click( object sender, EventArgs e )
        {
            LinkageState = new EventItemCampusGroupMap { Guid = Guid.Empty };
            DisplayRegistration();
        }
        
        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlExistingLinkageTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlExistingLinkageTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindExistingLinkages();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgExistingLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgNewLinkage_SaveClick( object sender, EventArgs e )
        {
            int? registrationTemplateId = ddlNewLinkageTemplate.SelectedValueAsInt();
            if ( registrationTemplateId.HasValue )
            {
                var rockContext = new RockContext();

                if ( LinkageState.RegistrationInstance == null )
                {
                    LinkageState.RegistrationInstance = new RegistrationInstance();
                }

                LinkageState.RegistrationInstance.RegistrationTemplateId = registrationTemplateId.Value;
                if ( LinkageState.RegistrationInstance.RegistrationTemplate == null )
                {
                    LinkageState.RegistrationInstance.RegistrationTemplate = new RegistrationTemplate();
                }

                var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( registrationTemplateId.Value );
                if ( registrationTemplate != null )
                {
                    LinkageState.RegistrationInstance.RegistrationTemplate.CopyPropertiesFrom( registrationTemplate );
                }

                rieNewLinkage.GetValue( LinkageState.RegistrationInstance );

                int? groupId = gpNewLinkageGroup.SelectedValueAsInt();
                if ( groupId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        LinkageState.GroupId = group.Id;
                        LinkageState.Group = group;
                    }
                }

                LinkageState.PublicName = rieNewLinkage.Name;
                LinkageState.UrlSlug = tbExistingLinkageUrlSlug.Text;

                // Set the Guid now (otherwise it will not be valid )
                bool isNew = LinkageState.Guid == Guid.Empty;
                if ( isNew )
                {
                    LinkageState.Guid = Guid.NewGuid();
                }

                if ( !LinkageState.IsValid )
                {
                    // If validation failed and this is new, reset the guid back to empty
                    if ( isNew )
                    {
                        LinkageState.Guid = Guid.Empty;
                    }
                    return;
                }

                DisplayRegistration();

                HideDialog();
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgExistingLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgExistingLinkage_SaveClick( object sender, EventArgs e )
        {
            int? registrationInstanceId = ddlExistingLinkageInstance.SelectedValueAsInt();
            if ( registrationInstanceId.HasValue )
            {
                var rockContext = new RockContext();

                var registrationInstance = new RegistrationInstanceService( rockContext ).Get( registrationInstanceId.Value );
                if ( registrationInstance != null )
                {
                    LinkageState.RegistrationInstanceId = registrationInstance.Id;
                    LinkageState.RegistrationInstance = registrationInstance;
                }

                int? groupId = gpExistingLinkageGroup.SelectedValueAsInt();
                if ( groupId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        LinkageState.GroupId = group.Id;
                        LinkageState.Group = group;
                    }
                }

                LinkageState.PublicName = tbExistingLinkagePublicName.Text;
                LinkageState.UrlSlug = tbExistingLinkageUrlSlug.Text;

                // Set the Guid now (otherwise it will not be valid )
                bool isNew = LinkageState.Guid == Guid.Empty;
                if ( isNew )
                {
                    LinkageState.Guid = Guid.NewGuid();
                }

                if ( !LinkageState.IsValid )
                {
                    // If validation failed and this is new, reset the guid back to empty
                    if ( isNew )
                    {
                        LinkageState.Guid = Guid.Empty;
                    }
                    return;
                }

                DisplayRegistration();

                HideDialog();
            }
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
        protected void dlgSchedule_SaveClick( object sender, EventArgs e )
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
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
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

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        public void ShowDetail( int eventItemCampusId )
        {
            pnlDetails.Visible = true;

            EventItemCampus eventItemCampus = null;

            var rockContext = new RockContext();

            if ( !eventItemCampusId.Equals( 0 ) )
            {
                eventItemCampus = new EventItemCampusService( rockContext ).Get( eventItemCampusId );
                lActionTitle.Text = ActionTitle.Edit( "Calendar Item Campus Detail" ).FormatAsHtmlTitle();
            }

            if ( eventItemCampus == null )
            {
                eventItemCampus = new EventItemCampus { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( "Calendar Item Campus Detail" ).FormatAsHtmlTitle();
            }

            hfEventItemCampusId.Value = eventItemCampus.Id.ToString();

            ddlCampus.SetValue( eventItemCampus.CampusId ?? -1 );
            tbLocation.Text = eventItemCampus.Location;

            ppContact.SetValue( eventItemCampus.ContactPersonAlias != null ? eventItemCampus.ContactPersonAlias.Person : null );
            pnPhone.Text = eventItemCampus.ContactPhone;
            tbEmail.Text = eventItemCampus.ContactEmail;

            tbCampusNote.Text = eventItemCampus.CampusNote;

            LinkageState = new EventItemCampusGroupMap { Guid = Guid.Empty };
            var registration = eventItemCampus.Linkages.FirstOrDefault();
            if ( registration != null )
            {
                LinkageState = registration.Clone( false );
                LinkageState.RegistrationInstance = registration.RegistrationInstance != null ? registration.RegistrationInstance.Clone( false ) : new RegistrationInstance();
                LinkageState.RegistrationInstance.RegistrationTemplate =
                    registration.RegistrationInstance != null && registration.RegistrationInstance.RegistrationTemplate != null ?
                    registration.RegistrationInstance.RegistrationTemplate.Clone( false ) : new RegistrationTemplate();
                LinkageState.Group = registration.Group != null ? registration.Group.Clone( false ) : new Group();
            }

            DisplayRegistration();

            SchedulesState = new List<EventItemSchedule>();
            foreach ( var itemSchedule in eventItemCampus.EventItemSchedules )
            {
                var scheduleState = itemSchedule.Clone( false );
                scheduleState.Schedule = itemSchedule.Schedule != null ? itemSchedule.Schedule.Clone( false ) : new Schedule();
                SchedulesState.Add( scheduleState );
            }

            BindSchedulesGrid();
        
        }

        /// <summary>
        /// Binds the registrations grid.
        /// </summary>
        private void DisplayRegistration()
        {
            if ( LinkageState != null && LinkageState.Guid != Guid.Empty )
            {
                lRegistration.Text =
                    ( LinkageState.RegistrationInstance != null ? LinkageState.RegistrationInstance.Name : "" ) +
                    ( LinkageState.Group != null ? " - " + LinkageState.Group.Name : "" );
                lbCreateNewRegistration.Visible = false;
                lbLinkToExistingRegistration.Visible = false;
                lbEditRegistration.Visible = true;
                lbDeleteRegistration.Visible = true;
            }
            else
            {
                lRegistration.Text = string.Empty;
                lbCreateNewRegistration.Visible = true;
                lbLinkToExistingRegistration.Visible = true;
                lbEditRegistration.Visible = false;
                lbDeleteRegistration.Visible = false;
            }
        }

        private void ShowNewLinkageDialog()
        {
            rieNewLinkage.ShowActive = false;
            rieNewLinkage.ShowUrlSlug = true;

            ddlNewLinkageTemplate.Items.Clear();

            using ( var rockContext = new RockContext() )
            {
                foreach ( var template in new RegistrationTemplateService( rockContext )
                    .Queryable().AsNoTracking() )
                {
                    if ( template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ListItem li = new ListItem( template.Name, template.Id.ToString() );
                        ddlNewLinkageTemplate.Items.Add( li );
                        li.Selected = LinkageState.RegistrationInstance != null &&
                            LinkageState.RegistrationInstance.RegistrationTemplateId == template.Id;
                    }
                }

                gpNewLinkageGroup.SetValue( LinkageState.Group );

                rieNewLinkage.SetValue( LinkageState.RegistrationInstance );
                rieNewLinkage.UrlSlug = LinkageState.UrlSlug;

                if ( LinkageState.RegistrationInstance == null )
                {
                    var contactPersonId = ppContact.PersonId;
                    if ( contactPersonId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( contactPersonId.Value );
                        if ( person != null )
                        {
                            rieNewLinkage.ContactName = person.FullName;
                        }
                    }
                    if ( !string.IsNullOrWhiteSpace( tbEmail.Text ) )
                    {
                        rieNewLinkage.ContactEmail = tbEmail.Text;
                    }
                }

                tbExistingLinkageUrlSlug.Text = LinkageState.UrlSlug;
            }

            ShowDialog( "EventItemNewLinkage", true );
        }

        /// <summary>
        /// Shows the linkage dialog.
        /// </summary>
        /// <param name="itemLinkage">The item linkage.</param>
        private void ShowExistingLinkageDialog()
        {
            ddlExistingLinkageTemplate.Items.Clear();
            
            using ( var rockContext = new RockContext() )
            {
                foreach( var template in new RegistrationTemplateService( rockContext )
                    .Queryable().AsNoTracking() )
                {
                    if ( template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ListItem li = new ListItem( template.Name, template.Id.ToString() );
                        ddlExistingLinkageTemplate.Items.Add( li );
                        li.Selected = LinkageState.RegistrationInstanceId != 0 &&
                                template.Instances.Any( i => i.Id == LinkageState.RegistrationInstanceId );
                    }
                }
            }

            BindExistingLinkages( LinkageState.RegistrationInstanceId );

            gpExistingLinkageGroup.SetValue( LinkageState.Group );
            tbExistingLinkagePublicName.Text = LinkageState.PublicName;
            tbExistingLinkageUrlSlug.Text = LinkageState.UrlSlug;

            ShowDialog( "EventItemExistingLinkage", true );
        }

        private void BindExistingLinkages( int? registrationInstanceId = null )
        {
            ddlExistingLinkageInstance.Items.Clear();
            int? templateId = ddlExistingLinkageTemplate.SelectedValueAsInt();
            if ( templateId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var instance in new RegistrationInstanceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i => i.RegistrationTemplateId == templateId.Value ) )
                    {
                        ListItem li = new ListItem( instance.Name, instance.Id.ToString() );
                        ddlExistingLinkageInstance.Items.Add( li );
                        li.Selected = registrationInstanceId.HasValue && instance.Id == registrationInstanceId.Value;
                    }
                }
            }
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

            ShowDialog( "EventItemSchedule", true );
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
                case "EVENTITEMNEWLINKAGE":
                    dlgNewLinkage.Show();
                    break;

                case "EVENTITEMEXISTINGLINKAGE":
                    dlgExistingLinkage.Show();
                    break;

                case "EVENTITEMSCHEDULE":
                    dlgSchedule.Show();
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
                case "EVENTITEMNEWLINKAGE":
                    dlgNewLinkage.Hide();
                    break;

                case "EVENTITEMEXISTINGLINKAGE":
                    dlgExistingLinkage.Hide();
                    break;

                case "EVENTITEMSCHEDULE":
                    dlgSchedule.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty; ;

        }

        #endregion

}
}