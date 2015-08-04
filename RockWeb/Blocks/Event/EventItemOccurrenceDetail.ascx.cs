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
using Humanizer;
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
    /// Displays the details of a given calendar item occurrence.
    /// </summary>
    [DisplayName( "Calendar Item Occurrence Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given calendar item occurrence." )]

    [AccountField( "Default Account", "The default account to use for new registration instances", false, "2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5", "", 0 )]
    [LinkedPage( "Registration Instance Page", "The page to view registration details", true, "", "", 1 )]
    [LinkedPage( "Group Detail Page", "The page for viewing details about a group", true, "", "", 2 )]
    public partial class CalendarItemOccurrenceDetail : RockBlock, IDetailBlock
    {
        #region Properties

        public EventItemOccurrenceGroupMap LinkageState { get; set; }

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
                LinkageState = new EventItemOccurrenceGroupMap();
            }
            else
            {
                LinkageState = JsonConvert.DeserializeObject<EventItemOccurrenceGroupMap>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                ShowDetail( PageParameter( "EventItemOccurrenceId" ).AsInteger() );
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

            return base.SaveViewState();
        }

        #endregion Control Methods

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( hfEventItemOccurrenceId.Value.AsInteger() );

            ShowEditDetails( eventItemOccurrence );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                EventItemOccurrenceService eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                EventItemOccurrence eventItemOccurrence = eventItemOccurrenceService.Get( hfEventItemOccurrenceId.Value.AsInteger() );

                if ( eventItemOccurrence != null )
                {
                    string errorMessage;
                    if ( !eventItemOccurrenceService.CanDelete( eventItemOccurrence, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    eventItemOccurrenceService.Delete( eventItemOccurrence );

                    rockContext.SaveChanges();
                }
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventItemOccurrence eventItemOccurrence = null;

            using ( var rockContext = new RockContext() )
            {
                bool newItem = false;
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                var eventItemOccurrenceGroupMapService = new EventItemOccurrenceGroupMapService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                var scheduleService = new ScheduleService( rockContext );

                int eventItemOccurrenceId = hfEventItemOccurrenceId.ValueAsInt();
                if ( eventItemOccurrenceId != 0 )
                {
                    eventItemOccurrence = eventItemOccurrenceService
                        .Queryable( "Linkages" )
                        .Where( i => i.Id == eventItemOccurrenceId )
                        .FirstOrDefault();
                }

                if ( eventItemOccurrence == null )
                {
                    newItem = true;
                    eventItemOccurrence = new EventItemOccurrence{ EventItemId = PageParameter("EventItemId").AsInteger() };
                    eventItemOccurrenceService.Add( eventItemOccurrence );
                }

                int? newCampusId = ddlCampus.SelectedValueAsInt();
                if ( eventItemOccurrence.CampusId != newCampusId )
                {
                    eventItemOccurrence.CampusId = newCampusId;
                    if ( newCampusId.HasValue )
                    {
                        var campus = new CampusService( rockContext ).Get( newCampusId.Value );
                        eventItemOccurrence.Campus = campus;
                    }
                    else
                    {
                        eventItemOccurrence.Campus = null;
                    }
                }

                eventItemOccurrence.Location = tbLocation.Text;

                string iCalendarContent = sbSchedule.iCalendarContent;
                var calEvent = ScheduleICalHelper.GetCalenderEvent( iCalendarContent );
                if ( calEvent != null && calEvent.DTStart != null )
                {
                    if ( eventItemOccurrence.Schedule == null )
                    {
                        eventItemOccurrence.Schedule = new Schedule();
                    }
                    eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                }
                else
                {
                    if ( eventItemOccurrence.ScheduleId.HasValue )
                    {
                        var oldSchedule = scheduleService.Get( eventItemOccurrence.ScheduleId.Value );
                        if ( oldSchedule != null )
                        {
                            scheduleService.Delete( oldSchedule );
                        }
                    }
                }

                if ( !eventItemOccurrence.ContactPersonAliasId.Equals( ppContact.PersonAliasId ))
                {
                    PersonAlias personAlias = null;
                    eventItemOccurrence.ContactPersonAliasId = ppContact.PersonAliasId;
                    if ( eventItemOccurrence.ContactPersonAliasId.HasValue )
                    {
                        personAlias = new PersonAliasService( rockContext ).Get( eventItemOccurrence.ContactPersonAliasId.Value );
                    }

                    if ( personAlias != null )
                    {
                        eventItemOccurrence.ContactPersonAlias = personAlias;
                    }
                }

                eventItemOccurrence.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                eventItemOccurrence.ContactEmail = tbEmail.Text;
                eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                // Remove any linkage no longer in UI
                Guid uiLinkageGuid = LinkageState != null ? LinkageState.Guid : Guid.Empty;
                foreach( var linkage in eventItemOccurrence.Linkages.Where( l => !l.Guid.Equals(uiLinkageGuid)).ToList())
                {
                    eventItemOccurrence.Linkages.Remove( linkage );
                    eventItemOccurrenceGroupMapService.Delete( linkage );
                }

                // Add/Update linkage in UI
                if ( !uiLinkageGuid.Equals( Guid.Empty ))
                {
                    var linkage = eventItemOccurrence.Linkages.Where( l => l.Guid.Equals( uiLinkageGuid)).FirstOrDefault();
                    if ( linkage == null )
                    {
                        linkage = new EventItemOccurrenceGroupMap();
                        eventItemOccurrence.Linkages.Add( linkage );
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

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !eventItemOccurrence.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                rockContext.SaveChanges();

                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );

                if ( newItem )
                {
                    NavigateToParentPage( qryParams );
                }
                else
                {
                    qryParams.Add( "EventItemOccurrenceId", eventItemOccurrence.Id.ToString() );
                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int eventItemId = hfEventItemOccurrenceId.ValueAsInt();
            if ( eventItemId == 0 )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
                NavigateToParentPage( qryParams );
            }
            else
            {
                var eventItemOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( eventItemId );
                ShowReadonlyDetails( eventItemOccurrence );
            }
        }

        #endregion 

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbCalendarsDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarsDetail_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            if ( pageCache != null && pageCache.ParentPage != null && pageCache.ParentPage.ParentPage != null && pageCache.ParentPage.ParentPage.ParentPage != null )
            {
                NavigateToPage( pageCache.ParentPage.ParentPage.ParentPage.Guid, qryParams );
            }
        }

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
            LinkageState = new EventItemOccurrenceGroupMap { Guid = Guid.Empty };
            ShowNewLinkageDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbLinkToExistingRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLinkToExistingRegistration_Click( object sender, EventArgs e )
        {
            LinkageState = new EventItemOccurrenceGroupMap { Guid = Guid.Empty };
            ShowExistingLinkageDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbEditRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditRegistration_Click( object sender, EventArgs e )
        {
            ShowEditLinkageDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteRegistration_Click( object sender, EventArgs e )
        {
            LinkageState = new EventItemOccurrenceGroupMap { Guid = Guid.Empty };
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
                    LinkageState.RegistrationInstance.IsActive = true;
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
                LinkageState.UrlSlug = rieNewLinkage.UrlSlug;

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
        /// Handles the SaveClick event of the dlgEditLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgEditLinkage_SaveClick( object sender, EventArgs e )
        {
            if ( LinkageState.RegistrationInstance != null )
            {
                var rockContext = new RockContext();

                rieEditLinkage.GetValue( LinkageState.RegistrationInstance );

                int? groupId = gpEditLinkageGroup.SelectedValueAsInt();
                if ( groupId.HasValue && groupId.Value != ( LinkageState.GroupId ?? 0 ) )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        LinkageState.GroupId = group.Id;
                        LinkageState.Group = group;
                    }
                }

                LinkageState.PublicName = tbEditLinkagePublicName.Text;
                LinkageState.UrlSlug = tbEditLinkageUrlSlug.Text;

                if ( !LinkageState.IsValid )
                {
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

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        public void ShowDetail( int eventItemOccurrenceId )
        {
            pnlDetails.Visible = true;

            EventItemOccurrence eventItemOccurrence = null;

            var rockContext = new RockContext();

            if ( !eventItemOccurrenceId.Equals( 0 ) )
            {
                eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( eventItemOccurrenceId );
            }

            if ( eventItemOccurrence == null )
            {
                eventItemOccurrence = new EventItemOccurrence { Id = 0 };
            }

            bool canEdit  = UserCanEdit || eventItemOccurrence.IsAuthorized( Authorization.EDIT, CurrentPerson );
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( !canEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItemOccurrence.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( eventItemOccurrence );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                if ( !eventItemOccurrenceId.Equals( 0))
                {
                    ShowReadonlyDetails( eventItemOccurrence );
                }
                else
                {
                    ShowEditDetails( eventItemOccurrence );
                }
            }
        }

        private void ShowEditDetails( EventItemOccurrence eventItemOccurrence )
        {
            if ( eventItemOccurrence == null )
            {
                eventItemOccurrence = new EventItemOccurrence();
            }

            if ( eventItemOccurrence.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( "Event Occurrence" ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( "Event Occurrence" ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            hfEventItemOccurrenceId.Value = eventItemOccurrence.Id.ToString();

            ddlCampus.SetValue( eventItemOccurrence.CampusId ?? -1 );
            tbLocation.Text = eventItemOccurrence.Location;

            if ( eventItemOccurrence.Schedule != null )
            {
                sbSchedule.iCalendarContent = eventItemOccurrence.Schedule.iCalendarContent;
                lScheduleText.Text = eventItemOccurrence.Schedule.FriendlyScheduleText;
            }
            else
            {
                sbSchedule.iCalendarContent = string.Empty;
                lScheduleText.Text = string.Empty;
            }

            ppContact.SetValue( eventItemOccurrence.ContactPersonAlias != null ? eventItemOccurrence.ContactPersonAlias.Person : null );
            pnPhone.Text = eventItemOccurrence.ContactPhone;
            tbEmail.Text = eventItemOccurrence.ContactEmail;

            htmlOccurrenceNote.Text = eventItemOccurrence.Note;

            LinkageState = new EventItemOccurrenceGroupMap { Guid = Guid.Empty };
            var registration = eventItemOccurrence.Linkages.FirstOrDefault();
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
        }

        private void ShowReadonlyDetails( EventItemOccurrence eventItemOccurrence )
        {
            SetEditMode( false );

            hfEventItemOccurrenceId.Value = eventItemOccurrence.Id.ToString();

            lActionTitle.Text = "Event Occurrence".FormatAsHtmlTitle();

            var leftDesc = new DescriptionList();
            leftDesc.Add( "Campus", eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All" );
            leftDesc.Add( "Location Description", eventItemOccurrence.Location );
            leftDesc.Add( "Schedule", eventItemOccurrence.Schedule != null ? eventItemOccurrence.Schedule.FriendlyScheduleText : string.Empty );

            if ( eventItemOccurrence.Linkages.Any() )
            {
                var linkage = eventItemOccurrence.Linkages.First();
                if ( linkage.RegistrationInstance != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "RegistrationInstanceId", linkage.RegistrationInstance.Id.ToString() );
                    leftDesc.Add( "Registration", string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( "RegistrationInstancePage", qryParams ), linkage.RegistrationInstance.Name ) );
                }

                if ( linkage.Group != null )
                {
                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "GroupId", linkage.Group.Id.ToString() );
                    leftDesc.Add( "Group", string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( "GroupDetailPage", qryParams ), linkage.Group.Name ) );
                }
            }
            lLeftDetails.Text = leftDesc.Html;

            var rightDesc = new DescriptionList();
            rightDesc.Add( "Contact", eventItemOccurrence.ContactPersonAlias != null && eventItemOccurrence.ContactPersonAlias.Person != null ?
                eventItemOccurrence.ContactPersonAlias.Person.FullName : "" );
            rightDesc.Add( "Phone", eventItemOccurrence.ContactPhone );
            rightDesc.Add( "Email", eventItemOccurrence.ContactEmail );
            lRightDetails.Text = rightDesc.Html;

            lOccurrenceNotes.Visible = !string.IsNullOrWhiteSpace( eventItemOccurrence.Note );
            lOccurrenceNotes.Text = eventItemOccurrence.Note;
        }

        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
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

            bool newLinkage = !LinkageState.RegistrationInstanceId.HasValue || LinkageState.RegistrationInstanceId.Value == 0;
            
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
                    var contactPersonAliasId = ppContact.PersonAliasId;
                    if ( contactPersonAliasId.HasValue )
                    {
                        var personAlias = new PersonAliasService( rockContext ).Get( contactPersonAliasId.Value );
                        if ( personAlias != null )
                        {
                            rieNewLinkage.ContactPersonAlias = personAlias;
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( pnPhone.Text))
                    {
                        rieNewLinkage.ContactPhone = pnPhone.Text;
                    }

                    if ( !string.IsNullOrWhiteSpace( tbEmail.Text ) )
                    {
                        rieNewLinkage.ContactEmail = tbEmail.Text;
                    }

                    Guid? accountGuid = GetAttributeValue( "DefaultAccount" ).AsGuidOrNull();
                    if ( accountGuid.HasValue )
                    {
                        var account = new FinancialAccountService( rockContext ).Get( accountGuid.Value );
                        rieNewLinkage.AccountId = account != null ? account.Id : 0;
                    }

                }
            }

            ShowDialog( "EventItemNewLinkage", true );
        }

        /// <summary>
        /// Shows the edit linkage dialog.
        /// </summary>
        private void ShowEditLinkageDialog()
        {
            rieEditLinkage.ShowActive = false;
            rieEditLinkage.ShowUrlSlug = false;

            lEditLinkageTemplate.Text = LinkageState.RegistrationInstance.RegistrationTemplate.Name;
            gpEditLinkageGroup.SetValue( LinkageState.Group );
            tbEditLinkagePublicName.Text = LinkageState.PublicName;
            tbEditLinkageUrlSlug.Text = LinkageState.UrlSlug;
            rieEditLinkage.SetValue( LinkageState.RegistrationInstance );

            ShowDialog( "EventItemEditLinkage", true );
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

                case "EVENTITEMEDITLINKAGE":
                    dlgEditLinkage.Show();
                    break;

                case "EVENTITEMEXISTINGLINKAGE":
                    dlgExistingLinkage.Show();
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

                case "EVENTITEMEDITLINKAGE":
                    dlgEditLinkage.Hide();
                    break;

                case "EVENTITEMEXISTINGLINKAGE":
                    dlgExistingLinkage.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty; ;

        }

        #endregion

}
}