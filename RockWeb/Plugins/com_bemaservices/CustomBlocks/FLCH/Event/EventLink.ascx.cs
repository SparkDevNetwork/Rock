// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;
using DDay.iCal;
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
using com.centralaz.RoomManagement.Attribute;
using Attribute = Rock.Model.Attribute;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_bemaservices.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Event Link" )]
    [Category( "BEMA Services > Event" )]
    [Description( "A one off form for serving entry into a workflow" )]

    [LinkedPage( "Result Page", "A page to redirect user to after they have created an Event", true, "", "", 1 )]
    [WorkflowTypeField( "Workflow", "The workflow to save the data into.", true, false, "", "", 2 )]
    [CategoryField( "Category Selection", "A top category to use for selecting the defaults.", false, "Rock.Model.RegistrationTemplate", "", "", false, "", "", 3 )]

    [TextField( "Room Reservation Instruction Text", "Inctructions for the Room Reservation tab", false, "", "", 5 )]
    [TextField( "Event Registration Instruction Text", "Inctructions for the Event Registration tab", false, "", "", 6 )]
    [TextField( "Calendar Instruction Text", "Inctructions for the Calendar tab", false, "", "", 6 )]
    [ContentChannelField( "Content Channel", "The Channel to save Content Items to" )]

    public partial class EventLink : RockBlock
    {
        #region Fields
        private const string REQUIRE_EMAIL_KEY = "IsRequireEmail";
        private const string REQUIRE_MOBILE_KEY = "IsRequiredMobile";

        RockContext _rockContext = null;
        string _mode = "Simple";
        Group _group = null;
        GroupTypeRole _defaultGroupRole = null;
        Person _primaryContact = null;
        Person _currentPerson = null;
        DefinedValueCache _dvcConnectionStatus = null;
        DefinedValueCache _dvcRecordStatus = null;
        DefinedValueCache _married = null;
        DefinedValueCache _homeAddressType = null;
        GroupTypeCache _familyType = null;
        GroupTypeRoleCache _adultRole = null;
        RegistrationTemplate _registrationTemplate = null;
        RegistrationInstance _registrationInstance = null;
        Reservation _reservation = null;
        EventCalendar _eventCalendar = null;
        EventItem _eventItem = null;
        EventItemOccurrence _eventItemOccurrence = null;
        ContentChannelItem _contentChannelItem = null;
        Workflow _workflow = null;
        bool _autoFill = true;
        bool _isValidSettings = true;

        #endregion

        #region Properties

        private List<RegistrationTemplateForm> FormState { get; set; }

        private Dictionary<Guid, List<RegistrationTemplateFormField>> FormFieldsState { get; set; }

        private List<Guid> ExpandedForms = new List<Guid>();

        private List<RegistrationTemplateDiscount> DiscountState { get; set; }

        private List<RegistrationTemplateFee> FeeState { get; set; }

        private List<Attribute> RegistrationAttributesState { get; set; }

        public List<int> AudiencesState { get; set; }

        public List<EventCalendarItem> ItemsState { get; set; }

        private int? GridFieldsDeleteIndex { get; set; }

        /// <summary>
        /// Gets or sets the state of the locations.
        /// </summary>
        /// <value>
        /// The state of the locations.
        /// </value>
        private List<ReservationLocationSummary> LocationsState { get; set; }

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>
        /// The type of the reservation.
        /// </value>
        private ReservationType ReservationType { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            //AudiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            string itemsStateJson = ViewState["ItemsState"] as string;
            ItemsState = itemsStateJson.IsNotNullOrWhiteSpace()
                ? JsonConvert.DeserializeObject<List<EventCalendarItem>>( itemsStateJson )
                : new List<EventCalendarItem>();
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

            //ViewState["AudiencesState"] = AudiencesState;
            ViewState["ItemsState"] = JsonConvert.SerializeObject( ItemsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //gAudiences.DataKeyNames = new string[] { "Guid" };
            //gAudiences.Actions.ShowAdd = true;
            //gAudiences.Actions.AddClick += gAudiences_Add;
            //gAudiences.GridRebind += gAudiences_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            BuildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            RockPage.AddScriptLink( "~/Scripts/Rock/slug.js" );

            if ( !CheckSettings() )
            {
                _isValidSettings = false;
                nbNotice.Visible = true;
                pnlView.Visible = false;
            }
            else
            {
                nbNotice.Visible = false;
                pnlView.Visible = true;

                if ( !Page.IsPostBack )
                {
                    ShowDetails();
                }
                else
                {
                    pnlContentItem.Visible = ( tEventCalendar.Checked || tEventReg.Checked );
                    ddlEventOrRegistration.Visible = ( tEventCalendar.Checked && tEventReg.Checked );
                    ShowItemAttributes();
                    ShowDialog();
                }
            }
        }

        #endregion

        #region  General Events / Methods

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            // Check _isValidSettings in case the form was showing and they clicked the visible register button.
            if ( Page.IsValid && _isValidSettings )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                var workflowService = new WorkflowService( rockContext );

                var workflowType = WorkflowTypeCache.Get( GetAttributeValue( "Workflow" ) );
                if ( workflowType != null )
                {
                    var workflow = Workflow.Activate( workflowType, tbEventName.Text );
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );


                    _primaryContact = new PersonService( rockContext ).Get( ppPrimaryContract.PersonId ?? 0 );
                    _currentPerson = CurrentPerson;
                    workflow.SetAttributeValue( "Owner", _primaryContact.PrimaryAlias.Guid );
                    workflow.SetAttributeValue( "EndDate", dpEventEndDate.SelectedDate );
                    workflow.Name = tbInternalEventName.Text;


                    _workflow = workflow;

                    if ( tEventRoom.Checked == false && tEventReg.Checked == false && tEventCalendar.Checked == false && tAnnoucement.Checked == false )
                    {
                        nbWarning.Text = "Choose at least once event option!";
                        nbWarning.Visible = true;
                        return;
                    }

                    if ( tEventRoom.Checked == true )
                    {
                        SaveRoomReservation();
                        workflow.SetAttributeValue( "RoomReservation", _reservation.Guid );
                    }

                    if ( tEventReg.Checked == true )
                    {
                        SaveRegistration();
                        workflow.SetAttributeValue( "RegistrationTemplate", _registrationTemplate.Guid );
                    }

                    if ( tEventCalendar.Checked == true )
                    {
                        SaveCalendarItem();
                        workflow.SetAttributeValue( "EventItem", _eventItem.Guid );
                    }

                    if ( tAnnoucement.Checked == true )
                    {
                        SaveContentChannelItem();
                        workflow.SetAttributeValue( "ContentChannelItem", _contentChannelItem.Guid );
                    }

                    workflow.SaveAttributeValues( rockContext );
                }


                // Show the results
                pnlView.Visible = false;
                pnlResult.Visible = true;

                //// Show lava content
                //var mergeFields = new Dictionary<string, object>();
                //mergeFields.Add( "Group", _group );

                //string template = GetAttributeValue( "ResultLavaTemplate" );
                //lResult.Text = template.ResolveMergeFields( mergeFields );

                // Will only redirect if a value is specifed
                NavigateToLinkedPage( "ResultPage", "EventLinkId", _workflow.Id );
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {

        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        public void BuildControls()
        {

            _rockContext = _rockContext ?? new RockContext();

            if ( GetAttributeValue( "CategorySelection" ) != null )
            {
                var registrationTemplates = new List<RegistrationTemplate>();
                var category = new CategoryService( _rockContext ).Get( GetAttributeValue( "CategorySelection" ).AsGuid() );
                if ( category != null )
                {
                    registrationTemplates = new RegistrationTemplateService( new RockContext() ).Queryable().AsNoTracking().Where( a => a.CategoryId == category.Id ).ToList();
                }

                ddlTemplate.DataSource = registrationTemplates;
                ddlTemplate.DataTextField = "Name";
                ddlTemplate.DataValueField = "Id";

                ddlTemplate.DataBind();

                ppPrimaryContract.SetValue( CurrentPerson );

                if ( GetAttributeValue( "RoomReservationInstructionText" ) != null )
                {
                    nbEventRoom.Visible = true;
                    nbEventRoom.Text = GetAttributeValue( "RoomReservationInstructionText" );
                }

                if ( GetAttributeValue( "EventRegistrationInstructionText" ) != null )
                {
                    nbEventReg.Visible = true;
                    nbEventReg.Text = GetAttributeValue( "EventRegistrationInstructionText" );
                }

                if ( GetAttributeValue( "CalendarInstructionText" ) != null )
                {
                    nbCalendar.Visible = true;
                    nbCalendar.Text = GetAttributeValue( "CalendarInstructionText" );
                }
            }

            cblCalendars.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                foreach ( var calendar in new EventCalendarService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name ) )
                {
                    cblCalendars.Items.Add( new ListItem( calendar.Name, calendar.Id.ToString() ) );
                }
            }

            ddlReservationType.Items.Clear();
                foreach ( var reservationType in new ReservationTypeService(  _rockContext ).Queryable().AsNoTracking().OrderBy( m => m.Name ).ToList() )
                {
                    if ( reservationType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlReservationType.Items.Add( new ListItem( reservationType.Name, reservationType.Id.ToString().ToUpper() ) );
                    }
                }
               

        }

        /// <summary>
        /// Checks the settings.  If false is returned, it's expected that the caller will make
        /// the nbNotice visible to inform the user of the "settings" error.
        /// </summary>
        /// <returns>true if settings are valid; false otherwise</returns>
        private bool CheckSettings()
        {
            _rockContext = _rockContext ?? new RockContext();

            return true;
        }

        /// <summary>
        /// Shows the dialog specified in hfActiveDialog.
        /// </summary>
        private void ShowDialog()
        {
            Dialogs dialogs;
            if ( Enum.TryParse( hfActiveDialog.Value, out dialogs ) )
            {
                ShowDialog( dialogs );
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( Dialogs dialog )
        {
            hfActiveDialog.Value = dialog.ToString();

            switch ( dialog )
            {
                case Dialogs.EventItemAudience:
                    dlgAudience.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            Dialogs dialogs;
            Enum.TryParse( hfActiveDialog.Value, out dialogs );
            switch ( dialogs )
            {
                case Dialogs.EventItemAudience:
                    dlgAudience.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #region Calendar Events / Methods

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCalendars control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            var eventCalendarList = new List<int>();
            eventCalendarList.AddRange( cblCalendars.SelectedValuesAsInt );

            if ( ItemsState == null )
            {
                ItemsState = new List<EventCalendarItem>();
            }
            phAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList.Distinct() )
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
                        phAttributes.Controls.Add( new LiteralControl( string.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
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
            //var values = new List<DefinedValueCache>();
            //AudiencesState.ForEach( a => values.Add( DefinedValueCache.Get( a ) ) );

            //gAudiences.DataSource = values
            //    .OrderBy( v => v.Order )
            //    .ThenBy( v => v.Value )
            //    .ToList();
            //gAudiences.DataBind();
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            Guid guid = ( Guid ) e.RowKeyValue;
            var audience = DefinedValueCache.Get( guid );
            if ( audience != null )
            {
                AudiencesState.Remove( audience.Id );
            }

            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !AudiencesState.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ShowDialog( Dialogs.EventItemAudience );
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
                AudiencesState.Add( definedValueId.Value );
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

        /// <summary>
        /// Saves the calendar item.
        /// </summary>
        public void SaveCalendarItem()
        {
            var rockContext = new RockContext();
            EventItem eventItem = null;

            var eventItemService = new EventItemService( rockContext );
            var eventCalendarItemService = new EventCalendarItemService( rockContext );
            var eventItemAudienceService = new EventItemAudienceService( rockContext );

            eventItem = new EventItem();
            eventItemService.Add( eventItem );

            eventItem.Name = tbEventName.Text;
            eventItem.IsActive = true;
            eventItem.IsApproved = false;
            eventItem.Summary = tbEventDescription.Text;
            eventItem.Description = tbEventDescription.Text;
            eventItem.DetailsUrl = null;

            /*  Audience No Longer Being used.  They're using the attribute on the EventCalendarItem Now.
            // Add or Update audiences from the UI
            foreach ( int audienceId in AudiencesState )
            {
                EventItemAudience eventItemAudience = eventItem.EventItemAudiences.Where( a => a.DefinedValueId == audienceId ).FirstOrDefault();
                if ( eventItemAudience == null )
                {
                    eventItemAudience = new EventItemAudience();
                    eventItemAudience.DefinedValueId = audienceId;
                    eventItem.EventItemAudiences.Add( eventItemAudience );
                }
            }
             */

            var calendarIds = new List<int>();
            calendarIds.AddRange( cblCalendars.SelectedValuesAsInt );

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

            if ( iuImage.BinaryFileId != null )
            {
                eventItem.PhotoId = iuImage.BinaryFileId;
            }

            eventItem.ForeignGuid = _workflow.Guid;
            eventItem.ForeignId = _workflow.Id;

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
            _eventItem = eventItem;

            EventItemOccurrence eventItemOccurrence = null;

            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
            var eventItemOccurrenceGroupMapService = new EventItemOccurrenceGroupMapService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var scheduleService = new ScheduleService( rockContext );


            eventItemOccurrence = new EventItemOccurrence { EventItemId = _eventItem.Id };

            string iCalendarContent = sbEventSchedule2.iCalendarContent;
            var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
            if ( calEvent != null && calEvent.DTStart != null )
            {
                if ( eventItemOccurrence.Schedule == null )
                {
                    eventItemOccurrence.Schedule = new Schedule();
                }

                eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
            }

            if ( _primaryContact != null )
            {
                eventItemOccurrence.ContactPersonAliasId = _primaryContact.PrimaryAliasId;

                var phoneNumber = _primaryContact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                string email = _primaryContact.Email;

                if ( email != "" )
                {
                    eventItemOccurrence.ContactEmail = email;
                }

                if ( phoneNumber != null )
                {
                    eventItemOccurrence.ContactPhone = phoneNumber.NumberFormatted;
                }
            }


            var campus = new CampusService( rockContext ).Get( cpCampus.SelectedValueAsId() ?? 0 );
            eventItemOccurrence.Campus = campus;


            if ( _registrationInstance != null )
            {
                var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap();
                // eventItemOccurrenceGroupMap.RegistrationInstanceId = _registrationInstance.Id;
                eventItemOccurrenceGroupMap.RegistrationInstanceId = _registrationInstance.Id;
                eventItemOccurrenceGroupMap.Guid = Guid.NewGuid();
                eventItemOccurrenceGroupMap.CreatedDateTime = RockDateTime.Now;
                eventItemOccurrenceGroupMap.PublicName = tbEventName.Text;

                _registrationInstance.Linkages.Add( eventItemOccurrenceGroupMap );
                eventItemOccurrence.Linkages.Add( eventItemOccurrenceGroupMap );

            }

            eventItemOccurrenceService.Add( eventItemOccurrence );

            rockContext.SaveChanges();
            _eventItemOccurrence = eventItemOccurrence;

        }

        /// <summary>
        /// Handles the CheckedChanged event of the tEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tEventCalendar_CheckedChanged( object sender, EventArgs e )
        {
            if ( tEventCalendar.Checked )
            {
                pnlEventCalendar.Visible = true;
                ShowItemAttributes();
                BindAudienceGrid();
            }
            else
            {
                pnlEventCalendar.Visible = false;
            }
        }

        #endregion

        #region Registration Events / Methods

        /// <summary>
        /// Saves the registration.
        /// </summary>
        public void SaveRegistration()
        {
            using ( var rockContext = new RockContext() )
            {

                var registrationTemplateService = new RegistrationTemplateService( rockContext );
                var registrationTemplate = registrationTemplateService.Get( ddlTemplate.SelectedValueAsId() ?? 0 );

                LoadStateDetails( registrationTemplate, rockContext );

                if ( registrationTemplate != null )
                {
                    RegistrationTemplate newRegistrationTemplate = registrationTemplate.Clone( false );

                    // newRegistrationTemplate.CreatedByPersonAliasId = CurrentPerson.PrimaryAliasId;
                    newRegistrationTemplate.CreatedDateTime = RockDateTime.Now;
                    newRegistrationTemplate.ModifiedByPersonAlias = null;
                    newRegistrationTemplate.ModifiedByPersonAliasId = null;
                    newRegistrationTemplate.ModifiedDateTime = RockDateTime.Now;
                    newRegistrationTemplate.Id = 0;
                    newRegistrationTemplate.Guid = Guid.NewGuid();
                    newRegistrationTemplate.Name = tbEventName.Text;
                    newRegistrationTemplate.CategoryId = cpCategory.SelectedValueAsId();
                    newRegistrationTemplate.ForeignGuid = _workflow.Guid;
                    newRegistrationTemplate.ForeignId = _workflow.Id;

                    _registrationTemplate = newRegistrationTemplate;

                    rockContext.SaveChanges();
                }


                registrationTemplateService.Add( _registrationTemplate );
                rockContext.SaveChanges();


                // Create temporary state objects for the new registration template
                var newFormState = new List<RegistrationTemplateForm>();
                var newFormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                var newDiscountState = new List<RegistrationTemplateDiscount>();
                var newFeeState = new List<RegistrationTemplateFee>();
                var newAttributeState = new List<Attribute>();

                foreach ( var form in FormState )
                {
                    var newForm = form.Clone( false );
                    newForm.RegistrationTemplateId = 0;
                    newForm.Id = 0;
                    newForm.Guid = Guid.NewGuid();
                    newFormState.Add( newForm );

                    if ( FormFieldsState.ContainsKey( form.Guid ) )
                    {
                        newFormFieldsState.Add( newForm.Guid, new List<RegistrationTemplateFormField>() );
                        foreach ( var formField in FormFieldsState[form.Guid] )
                        {
                            var newFormField = formField.Clone( false );
                            newFormField.RegistrationTemplateFormId = 0;
                            newFormField.Id = 0;
                            newFormField.Guid = Guid.NewGuid();
                            newFormFieldsState[newForm.Guid].Add( newFormField );

                            if ( formField.FieldSource != RegistrationFieldSource.PersonField )
                            {
                                newFormField.Attribute = formField.Attribute;
                            }

                            if ( formField.FieldSource == RegistrationFieldSource.RegistrantAttribute && formField.Attribute != null )
                            {
                                var newAttribute = formField.Attribute.Clone( false );
                                newAttribute.Id = 0;
                                newAttribute.Guid = Guid.NewGuid();
                                newAttribute.IsSystem = false;

                                newFormField.AttributeId = null;
                                newFormField.Attribute = newAttribute;

                                foreach ( var qualifier in formField.Attribute.AttributeQualifiers )
                                {
                                    var newQualifier = qualifier.Clone( false );
                                    newQualifier.Id = 0;
                                    newQualifier.Guid = Guid.NewGuid();
                                    newQualifier.IsSystem = false;
                                    newAttribute.AttributeQualifiers.Add( newQualifier );
                                }
                            }
                        }
                    }
                }

                foreach ( var discount in DiscountState )
                {
                    var newDiscount = discount.Clone( false );
                    newDiscount.RegistrationTemplateId = 0;
                    newDiscount.Id = 0;
                    newDiscount.Guid = Guid.NewGuid();
                    newDiscountState.Add( newDiscount );
                }

                foreach ( var fee in FeeState )
                {
                    var newFee = fee.Clone( false );
                    newFee.RegistrationTemplateId = 0;
                    newFee.Id = 0;
                    newFee.Guid = Guid.NewGuid();
                    newFeeState.Add( newFee );
                    foreach ( var item in fee.FeeItems )
                    {
                        var feeItem = item.Clone( false );
                        feeItem.Id = 0;
                        feeItem.Guid = Guid.NewGuid();
                        newFee.FeeItems.Add( feeItem );
                    }
                }

                foreach ( var attribute in RegistrationAttributesState )
                {
                    var newAttribute = attribute.Clone( false );
                    newAttribute.EntityTypeQualifierValue = null;
                    newAttribute.Id = 0;
                    newAttribute.Guid = Guid.NewGuid();
                    newAttributeState.Add( newAttribute );
                }

                FormState = newFormState;
                FormFieldsState = newFormFieldsState;
                DiscountState = newDiscountState;
                FeeState = newFeeState;
                RegistrationAttributesState = newAttributeState;


                // add/updated forms/fields
                foreach ( var formUI in FormState )
                {
                    var form = _registrationTemplate.Forms.FirstOrDefault( f => f.Guid.Equals( formUI.Guid ) );
                    if ( form == null )
                    {
                        form = new RegistrationTemplateForm();
                        form.Guid = formUI.Guid;
                        _registrationTemplate.Forms.Add( form );
                    }

                    form.Name = formUI.Name;
                    form.Order = formUI.Order;

                    if ( FormFieldsState.ContainsKey( form.Guid ) )
                    {
                        foreach ( var formFieldUI in FormFieldsState[form.Guid] )
                        {
                            var formField = form.Fields.FirstOrDefault( a => a.Guid.Equals( formFieldUI.Guid ) );
                            if ( formField == null )
                            {
                                formField = new RegistrationTemplateFormField();
                                formField.Guid = formFieldUI.Guid;
                                form.Fields.Add( formField );
                            }

                            formField.AttributeId = formFieldUI.AttributeId;
                            if ( !formField.AttributeId.HasValue &&
                                formFieldUI.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                                formFieldUI.Attribute != null )
                            {
                                var attr = AttributeCache.Get( formFieldUI.Attribute.Guid, rockContext );
                                if ( attr != null )
                                {
                                    formField.AttributeId = attr.Id;
                                }
                                else
                                {
                                    formField.Attribute = formFieldUI.Attribute;
                                }
                            }

                            formField.FieldSource = formFieldUI.FieldSource;
                            formField.PersonFieldType = formFieldUI.PersonFieldType;
                            formField.IsInternal = formFieldUI.IsInternal;
                            formField.IsSharedValue = formFieldUI.IsSharedValue;
                            formField.ShowCurrentValue = formFieldUI.ShowCurrentValue;
                            formField.PreText = formFieldUI.PreText;
                            formField.PostText = formFieldUI.PostText;
                            formField.IsGridField = formFieldUI.IsGridField;
                            formField.IsRequired = formFieldUI.IsRequired;
                            formField.Order = formFieldUI.Order;
                            formField.ShowOnWaitlist = formFieldUI.ShowOnWaitlist;
                            formField.FieldVisibilityRules = formFieldUI.FieldVisibilityRules;
                        }
                    }
                }

                // add/updated discounts
                foreach ( var discountUI in DiscountState )
                {
                    var discount = registrationTemplate.Discounts.FirstOrDefault( a => a.Guid.Equals( discountUI.Guid ) );
                    if ( discount == null )
                    {
                        discount = new RegistrationTemplateDiscount();
                        discount.Guid = discountUI.Guid;
                        registrationTemplate.Discounts.Add( discount );
                    }

                    discount.Code = discountUI.Code;
                    discount.DiscountPercentage = discountUI.DiscountPercentage;
                    discount.DiscountAmount = discountUI.DiscountAmount;
                    discount.Order = discountUI.Order;
                    discount.MaxUsage = discountUI.MaxUsage;
                    discount.MaxRegistrants = discountUI.MaxRegistrants;
                    discount.MinRegistrants = discountUI.MinRegistrants;
                    discount.StartDate = discountUI.StartDate;
                    discount.EndDate = discountUI.EndDate;
                    discount.AutoApplyDiscount = discountUI.AutoApplyDiscount;
                }

                // add/updated fees
                foreach ( var feeUI in FeeState )
                {
                    var registrationTemplateFeeService = new RegistrationTemplateFeeService( rockContext );
                    var registrationTemplateFeeItemService = new RegistrationTemplateFeeItemService( rockContext );

                    var fee = registrationTemplate.Fees.FirstOrDefault( a => a.Guid.Equals( feeUI.Guid ) );
                    if ( fee == null )
                    {
                        fee = new RegistrationTemplateFee();
                        fee.Guid = feeUI.Guid;
                        registrationTemplate.Fees.Add( fee );
                    }

                    fee.Name = feeUI.Name;
                    fee.FeeType = feeUI.FeeType;

                    // delete any feeItems no longer defined
                    foreach ( var deletedFeeItem in fee.FeeItems.ToList().Where( a => !feeUI.FeeItems.Any( x => x.Guid == a.Guid ) ) )
                    {
                        registrationTemplateFeeItemService.Delete( deletedFeeItem );
                    }

                    // add any new feeItems
                    foreach ( var newFeeItem in feeUI.FeeItems.ToList().Where( a => !fee.FeeItems.Any( x => x.Guid == a.Guid ) ) )
                    {
                        newFeeItem.RegistrationTemplateFee = fee;
                        newFeeItem.RegistrationTemplateFeeId = fee.Id;
                        registrationTemplateFeeItemService.Add( newFeeItem );
                    }

                    // update feeItems to match
                    foreach ( var feeItem in fee.FeeItems )
                    {
                        var feeItemUI = feeUI.FeeItems.FirstOrDefault( x => x.Guid == feeItem.Guid );
                        if ( feeItemUI != null )
                        {
                            feeItem.Order = feeItemUI.Order;
                            feeItem.Name = feeItemUI.Name;
                            feeItem.Cost = feeItemUI.Cost;
                            feeItem.MaximumUsageCount = feeItemUI.MaximumUsageCount;
                        }
                    }

                    fee.DiscountApplies = feeUI.DiscountApplies;
                    fee.AllowMultiple = feeUI.AllowMultiple;
                    fee.Order = feeUI.Order;
                    fee.IsActive = feeUI.IsActive;
                    fee.IsRequired = feeUI.IsRequired;
                }

                registrationTemplate.ModifiedByPersonAliasId = CurrentPersonAliasId;
                registrationTemplate.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges();

                //SaveAttributes( new Registration().TypeId, "RegistrationTemplateId", registrationTemplate.Id.ToString(), RegistrationAttributesState, rockContext );

            }

            RegistrationInstance instance = null;

            using ( var newrockContext = new RockContext() )
            {
                var newService = new RegistrationInstanceService( newrockContext );

                if ( instance == null )
                {
                    instance = new RegistrationInstance();
                    instance.RegistrationTemplateId = _registrationTemplate.Id;
                    instance.Name = tbEventName.Text;
                    instance.ForeignGuid = _workflow.Guid;
                    instance.ForeignId = _workflow.Id;
                    instance.StartDateTime = dpEventRegStartDate.SelectedDateTime;
                    instance.EndDateTime = dpEventRegEndDate.SelectedDateTime;
                    instance.Details = tbEventDescription.Text;


                    if ( _primaryContact != null )
                    {
                        instance.ContactPersonAliasId = _primaryContact.PrimaryAliasId;

                        var phoneNumber = _primaryContact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                        string email = _primaryContact.Email;

                        if ( email != "" )
                        {
                            instance.ContactEmail = email;
                        }

                        if ( phoneNumber != null )
                        {
                            instance.ContactPhone = phoneNumber.NumberFormatted;
                        }
                    }
                    newService.Add( instance );
                }

                newrockContext.SaveChanges();

                _registrationInstance = instance;
            }


        }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( RegistrationTemplate registrationTemplate, RockContext rockContext )
        {
            if ( registrationTemplate != null )
            {
                // If no forms, add at one
                if ( !registrationTemplate.Forms.Any() )
                {
                    var form = new RegistrationTemplateForm();
                    form.Guid = Guid.NewGuid();
                    form.Order = 0;
                    form.Name = "Default Form";
                    registrationTemplate.Forms.Add( form );
                }

                var defaultForm = registrationTemplate.Forms.First();

                // Add first name field if it doesn't exist
                if ( !defaultForm.Fields
                    .Any( f =>
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == RegistrationPersonFieldType.FirstName ) )
                {
                    var formField = new RegistrationTemplateFormField();
                    formField.FieldSource = RegistrationFieldSource.PersonField;
                    formField.PersonFieldType = RegistrationPersonFieldType.FirstName;
                    formField.IsGridField = true;
                    formField.IsRequired = true;
                    formField.ShowOnWaitlist = true;
                    formField.PreText = @"<div class='row'><div class='col-md-6'>";
                    formField.PostText = "    </div>";
                    formField.Order = defaultForm.Fields.Any() ? defaultForm.Fields.Max( f => f.Order ) + 1 : 0;
                    defaultForm.Fields.Add( formField );
                }

                // Add last name field if it doesn't exist
                if ( !defaultForm.Fields
                    .Any( f =>
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == RegistrationPersonFieldType.LastName ) )
                {
                    var formField = new RegistrationTemplateFormField();
                    formField.FieldSource = RegistrationFieldSource.PersonField;
                    formField.PersonFieldType = RegistrationPersonFieldType.LastName;
                    formField.IsGridField = true;
                    formField.IsRequired = true;
                    formField.ShowOnWaitlist = true;
                    formField.PreText = "    <div class='col-md-6'>";
                    formField.PostText = @"    </div></div>";
                    formField.Order = defaultForm.Fields.Any() ? defaultForm.Fields.Max( f => f.Order ) + 1 : 0;
                    defaultForm.Fields.Add( formField );
                }

                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                foreach ( var form in registrationTemplate.Forms.OrderBy( f => f.Order ) )
                {
                    FormState.Add( form.Clone( false ) );
                    FormFieldsState.Add( form.Guid, form.Fields.ToList() );
                }

                DiscountState = registrationTemplate.Discounts.OrderBy( a => a.Order ).ToList();
                FeeState = registrationTemplate.Fees.OrderBy( a => a.Order ).ToList();
                var attributeService = new AttributeService( rockContext );
                RegistrationAttributesState = attributeService.GetByEntityTypeId( new Registration().TypeId, true ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( registrationTemplate.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

            }
            else
            {
                FormState = new List<RegistrationTemplateForm>();
                FormFieldsState = new Dictionary<Guid, List<RegistrationTemplateFormField>>();
                DiscountState = new List<RegistrationTemplateDiscount>();
                FeeState = new List<RegistrationTemplateFee>();
                RegistrationAttributesState = new List<Attribute>();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tEventReg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tEventReg_CheckedChanged( object sender, EventArgs e )
        {
            if ( tEventReg.Checked )
            {
                pnlEventReg.Visible = true;
            }
            else
            {
                pnlEventReg.Visible = false;
            }
        }

        #endregion

        #region Reservation Events / Methods

        /// <summary>
        /// Saves the room reservation.
        /// </summary>
        public void SaveRoomReservation()
        {
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                bool saveSuccess = false;

                ResourceService resourceService = new ResourceService( rockContext );
                LocationService locationService = new LocationService( rockContext );
                LocationLayoutService locationLayoutService = new LocationLayoutService( rockContext );
                ReservationService reservationService = new ReservationService( rockContext );
                ReservationMinistryService reservationMinistryService = new ReservationMinistryService( rockContext );
                ReservationResourceService reservationResourceService = new ReservationResourceService( rockContext );
                ReservationLocationService reservationLocationService = new ReservationLocationService( rockContext );

                Reservation reservation = null;
                var changes = new History.HistoryChangeList();

                reservation = new Reservation { Id = 0 };
                reservation.ApprovalState = ReservationApprovalState.Unapproved;
                reservation.RequesterAliasId = ppPrimaryContract.PersonAliasId;
                changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Reservation" ) );

                ReservationType = new ReservationType();
                var reservationType = new ReservationTypeService( rockContext ).Get( ddlReservationType.SelectedValueAsId().Value );
                reservation.ReservationType = reservationType;
                reservation.ReservationTypeId = reservationType.Id;
                reservation.Note = tbEventDescription.Text;

                //ReservationLocation reservationLocation = reservation.ReservationLocations.Where(a => a.Id == slpLocation.SelectedValueAsId().Value).FirstOrDefault();

                ReservationLocation reservationLocation = new ReservationLocation();
                reservation.ReservationLocations.Add( reservationLocation );

                reservationLocation.Reservation = reservationService.Get( reservation.Id );
                reservationLocation.LocationId = slpLocation.SelectedValueAsId().Value;
                reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                reservationLocation.ReservationId = reservation.Id;


                reservation.Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent );

                reservation.CleanupTime = 15;
                reservation.SetupTime = 15;
                reservation.NumberAttending = 0;

                reservation.Name = tbEventName.Text;

                var phoneNumber = _primaryContact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                string email = _primaryContact.Email;

                reservation.EventContactPersonAliasId = _primaryContact.PrimaryAliasId;

                if ( email != "" )
                {
                    reservation.EventContactEmail = email;
                }

                if ( phoneNumber != null )
                {
                    reservation.EventContactPhone = phoneNumber.NumberFormatted;
                }

                reservation.AdministrativeContactPersonAliasId = _currentPerson.PrimaryAliasId;

                var phoneNumberCP = _currentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                string emailCP = _currentPerson.Email;

                if ( emailCP != "" )
                {
                    reservation.AdministrativeContactEmail = emailCP;
                }

                if ( phoneNumberCP != null )
                {
                    reservation.AdministrativeContactPhone = phoneNumberCP.NumberFormatted;
                }

                reservation.ForeignGuid = _workflow.Guid;
                reservation.ForeignId = _workflow.Id;

                var campus = new CampusService( rockContext ).Get( cpCampus.SelectedValueAsId() ?? 0 );
                reservation.Campus = campus;


                // Check to make sure there's a schedule
                if ( String.IsNullOrWhiteSpace( lScheduleText.Text ) )
                {
                    nbNotice.Text = "<b>Please add a schedule.</b>";
                    nbNotice.Visible = true;
                    return;
                }

                // Check to make sure there's no conflicts
                var conflictInfo = reservationService.GenerateConflictInfo( reservation, this.CurrentPageReference.Route );

                if ( !string.IsNullOrWhiteSpace( conflictInfo ) )
                {
                    nbNotice.Text = conflictInfo;
                    nbNotice.Visible = true;
                    return;
                }

                reservation = reservationService.UpdateApproval( reservation,ReservationApprovalState.Unapproved );
                reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );

                if ( reservation.Id.Equals( 0 ) )
                {
                    reservationService.Add( reservation );
                }

                rockContext.WrapTransaction( () =>
                 {
                     rockContext.SaveChanges();

                     reservationLocation.SaveAttributeValues( rockContext );

                     reservation.SaveAttributeValues( rockContext );

                     saveSuccess = true;

                 } );

                _reservation = reservation;

            }

        }

        /// <summary>
        /// Handles the SelectItem event of the srpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void slpLocation_SelectItem( object sender, EventArgs e )
        {
            // LoadLocationImage();
            LoadLocationConflictMessage();
            // BindLocationLayoutGrid();
            // SelectDefaultLayout();
        }

        /// <summary>
        /// Loads the location conflict message when using the location editor modal.
        /// </summary>
        private void LoadLocationConflictMessage()
        {

            if ( slpLocation.SelectedValueAsId().HasValue )
            {
                var rockContext = new RockContext();

                var locationId = slpLocation.SelectedValueAsId().Value;
                var location = new LocationService( rockContext ).Get( locationId );

                var reservationLocationGuid = hfAddReservationLocationGuid.Value.AsGuid();

                int reservationId = hfReservationId.ValueAsInt();
                var newReservation = new Reservation() { Id = reservationId, Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent ), SetupTime = 15, CleanupTime = 15 };
                var message = new ReservationService( rockContext ).BuildLocationConflictHtmlList( newReservation, locationId, this.CurrentPageReference.Route );

                if ( message != null )
                {
                    nbLocationConflicts.Text = string.Format( "{0} is already reserved for the scheduled times by the following reservations:<ul>{1}</ul>", location.Name, message );
                    nbLocationConflicts.Visible = true;
                }
                else
                {
                    nbLocationConflicts.Visible = false;
                }

            }
            else
            {
                nbLocationConflicts.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule2( object sender, EventArgs e )
        {
            //nbErrorWarning.Visible = false;

            var schedule = new Schedule { iCalendarContent = sbEventSchedule2.iCalendarContent };
            lScheduleText2.Text = schedule.FriendlyScheduleText;

            if ( sbSchedule.iCalendarContent != null )
            {
                slpLocation.Enabled = true;
            }
            else
            {
                slpLocation.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            //nbErrorWarning.Visible = false;

            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;

            if ( sbSchedule.iCalendarContent != null )
            {
                slpLocation.Enabled = true;
            }
            else
            {
                slpLocation.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tEventRoom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tEventRoom_CheckedChanged( object sender, EventArgs e )
        {
            if ( tEventRoom.Checked )
            {
                pnlEventRoom.Visible = true;
            }
            else
            {
                pnlEventRoom.Visible = false;
            }
        }

        #endregion

        #region Content Channel Item Events / Methods

        /// <summary>
        /// Handles the CheckedChanged event of the tAnnoucement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tAnnoucement_CheckedChanged( object sender, EventArgs e )
        {
            if ( tAnnoucement.Checked )
            {
                pnlAnnoucement.Visible = true;
            }
            else
            {
                pnlAnnoucement.Visible = false;
            }
        }

        /// <summary>
        /// Saves the content channel item.
        /// </summary>
        public void SaveContentChannelItem()
        {
            if ( pnlContentItem.Visible )
            {
                var rockContext = new RockContext();
                ContentChannelItem contentItem = GetContentItem( rockContext );

                if ( contentItem != null )
                {
                    contentItem.Title = tbEventName.Text;
                    contentItem.ItemGlobalKey = CreateItemGlobalKey();

                    contentItem.LoadAttributes( rockContext );
                    contentItem.SetAttributeValue( "Description", tbEventDescription.Text );

                     if ( _primaryContact != null )
                    {
                        contentItem.SetAttributeValue( "PrimaryContact", _primaryContact.PrimaryAlias.Guid );

                        var phoneNumber = _primaryContact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                        string email = _primaryContact.Email;

                        if ( email != "" )
                        {
                            contentItem.SetAttributeValue( "ContactEmail", email );
                        }

                        if ( phoneNumber != null )
                        {
                            contentItem.SetAttributeValue( "ContactPhone", phoneNumber.NumberFormatted );
                        }
                    }

                    var campus = new CampusService( rockContext ).Get( cpCampus.SelectedValueAsId() ?? 0 );
                    if ( campus != null )
                    {
                        contentItem.SetAttributeValue( "Campus", campus.Guid.ToString() );
                    }

                    if ( iuImage.BinaryFileId != null )
                    {
                        var binaryFile = new BinaryFileService( rockContext ).Get( iuImage.BinaryFileId.Value );
                        if ( binaryFile != null )
                        {
                            contentItem.SetAttributeValue( "Image", binaryFile.Guid.ToString() );
                        }
                    }

                    if ( tEventReg.Checked == true )
                    {
                        contentItem.SetAttributeValue( "RegistrationLinkUrl", String.Format( "/Registration?RegistrationInstanceId={0}", _registrationInstance.Id ) );
                    }
                    if ( tEventCalendar.Checked == true )
                    {
                        contentItem.SetAttributeValue( "CalendarLinkUrl", String.Format( "/event/{0}", _eventItemOccurrence.Id ) );
                    }
                    
                    // If this is a new item and the channel is manually sorted then we need to set the order to the next number
                    if ( contentItem.Id == 0 && new ContentChannelService( rockContext ).IsManuallySorted( contentItem.ContentChannelId ) )
                    {
                        contentItem.Order = new ContentChannelItemService( rockContext ).GetNextItemOrderValueForContentChannel( contentItem.ContentChannelId );
                    }

                    if ( contentItem.ContentChannelType.IncludeTime )
                    {
                        contentItem.StartDateTime = dtpStart.SelectedDateTime ?? RockDateTime.Now;
                        contentItem.ExpireDateTime = ( contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ) ?
                            dtpExpire.SelectedDateTime : null;
                    }
                    else
                    {
                        contentItem.StartDateTime = dpStart.SelectedDate ?? RockDateTime.Today;
                        contentItem.ExpireDateTime = ( contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ) ?
                            dpExpire.SelectedDate : null;
                    }

                    if ( contentItem.ContentChannelType.DisableStatus )
                    {
                        // if DisableStatus == True, just set the status to Approved
                        contentItem.Status = ContentChannelItemStatus.Approved;
                    }
                    else
                    {
                        contentItem.ApprovedDateTime = null;
                        contentItem.ApprovedByPersonAliasId = null;
                        contentItem.Status = ContentChannelItemStatus.PendingApproval;

                    }

                    contentItem.ForeignGuid = _workflow.Guid;
                    contentItem.ForeignId = _workflow.Id;

                    if ( !Page.IsValid || !contentItem.IsValid )
                    {
                        // Controls will render the error messages                    
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        if ( !string.IsNullOrEmpty( hfSlug.Value ) )
                        {
                            var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );
                            contentChannelItemSlugService.SaveSlug( contentItem.Id, hfSlug.Value, null );
                        }

                        rockContext.SaveChanges();
                        contentItem.SaveAttributeValues( rockContext );

                        if ( _eventItemOccurrence != null )
                        {
                            var occurrenceChannelItemService = new EventItemOccurrenceChannelItemService( rockContext );
                            var occurrenceChannelItem = occurrenceChannelItemService
                                .Queryable()
                                .Where( c =>
                                    c.ContentChannelItemId == contentItem.Id &&
                                    c.EventItemOccurrenceId == _eventItemOccurrence.Id )
                                .FirstOrDefault();

                            if ( occurrenceChannelItem == null )
                            {
                                occurrenceChannelItem = new EventItemOccurrenceChannelItem();
                                occurrenceChannelItem.ContentChannelItemId = contentItem.Id;
                                occurrenceChannelItem.EventItemOccurrenceId = _eventItemOccurrence.Id;
                                occurrenceChannelItemService.Add( occurrenceChannelItem );
                                rockContext.SaveChanges();
                            }
                        }
                    } );

                    _contentChannelItem = contentItem;

                }
            }

        }

        /// <summary>
        /// Creates the item global key.
        /// </summary>
        /// <returns></returns>
        private string CreateItemGlobalKey()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );
                return contentChannelItemSlugService.GetUniqueContentSlug( tbInternalEventName.Text, null );
            }
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentItemId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentChannelItem GetContentItem( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            ContentChannelItem contentItem = null;

            if ( contentItem == null )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( GetAttributeValue( "ContentChannel" ).AsGuid() );
                if ( contentChannel != null )
                {
                    contentItem = new ContentChannelItem
                    {
                        ContentChannel = contentChannel,
                        ContentChannelId = contentChannel.Id,
                        ContentChannelType = contentChannel.ContentChannelType,
                        ContentChannelTypeId = contentChannel.ContentChannelType.Id,
                        StartDateTime = RockDateTime.Now
                    };

                    var hierarchy = GetNavHierarchy();
                    if ( hierarchy.Any() )
                    {
                        var parentItem = contentItemService.Get( hierarchy.Last().AsInteger() );
                        if ( parentItem != null &&
                            parentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) &&
                            parentItem.ContentChannel.ChildContentChannels.Any( c => c.Id == contentChannel.Id ) )
                        {
                            var order = parentItem.ChildItems
                                .Select( a => ( int? ) a.Order )
                                .DefaultIfEmpty()
                                .Max();

                            var assoc = new ContentChannelItemAssociation();
                            assoc.ContentChannelItemId = parentItem.Id;
                            assoc.Order = order.HasValue ? order.Value + 1 : 0;
                            contentItem.ParentItems.Add( assoc );
                        }
                    }

                    if ( contentChannel.RequiresApproval )
                    {
                        contentItem.Status = ContentChannelItemStatus.PendingApproval;
                    }
                    else
                    {
                        contentItem.Status = ContentChannelItemStatus.Approved;
                        contentItem.ApprovedDateTime = RockDateTime.Now;
                        contentItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    }

                    contentItemService.Add( contentItem );
                }
            }

            return contentItem;
        }

        /// <summary>
        /// Gets the nav hierarchy.
        /// </summary>
        /// <returns></returns>
        private List<string> GetNavHierarchy()
        {
            var qryParam = PageParameter( "Hierarchy" );
            if ( !string.IsNullOrWhiteSpace( qryParam ) )
            {
                return qryParam.SplitDelimitedValues( false ).ToList();
            }

            return new List<string>();
        }

        #endregion       

        private class ReservationLocationSummary : ReservationLocation
        {
            public bool IsNew { get; set; }
        }

        /// <summary>
        /// A list of Rock:ModalDialog IDs on the page. Use to indicate which to show/hide.
        /// </summary>
        private enum Dialogs
        {
            EventItemAudience,
            EventOccurrenceAttributes
        }
    }
}
