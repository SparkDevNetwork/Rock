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
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Constants;

namespace RockWeb.Blocks.Event
{
    public static class MyExtensions
    {
        public static List<int> AsIntegerList( this string str )
        {
            List<int> values = new List<int>();
            List<string> strValues = str.Split( ',' ).ToList();

            foreach (string strValue in strValues)
            {
                int value = 0;
                if (int.TryParse( strValue, out value ))
                {
                    values.Add( value );
                }
            }
            return values;

        }
        public static List<int> SelectedValuesAsInt( this CheckBoxList cbl)
        {
            List<int> values = new List<int>();
            foreach(  ListItem li in cbl.Items)
            {
                if ( li.Selected )
                {
                    int value = 0;
                    if ( int.TryParse( li.Value, out value ) )
                    {
                        values.Add( value );
                    }
                }
            }
            return values;
        }
        public static string ToDelimitedString( this List<int> values )
        {
            string delimitedString = "";
            foreach ( int value in values )
            {
                if ( delimitedString.Length > 0 )
                {
                    delimitedString += ",";
                }
                delimitedString += value.ToString();
            }
            return delimitedString;
        }
    }
    /// <summary>
    /// A wizard to simplify creation of Event Registrations.
    /// </summary>
    [DisplayName( "Event Registration Wizard" )]
    [Category( "Event" )]
    [Description( "A wizard to simplify creation of Event Registrations." )]

    #region "Block Attribute Settings"

    [AccountField(
        "Default Account",
        Key = AttributeKey.DefaultAccount,
        Description = "Select the default financial account which will be pre-filled if a cost is set on the new registration instance.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 0 )]

    [EventCalendarField(
        "Default Calendar",
        Key = AttributeKey.DefaultCalendar,
        Description = "The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 1 )]

    [RegistrationTemplatesField(
        "Available Registration Templates",
        Key = AttributeKey.AvailableRegistrationTemplates,
        Description = "The list of templates the staff person can pick from – not all templates need to be available to all blocks.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 2 )]

    [GroupField(
        "Root Group",
        Key = AttributeKey.RootGroup,
        Description = "This is the \"root\" of the group tree which will be offered for the staff person to pick the parent group from – limiting where the new group can be created.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [LinkedPage(
        "Group Viewer Page",
        Key = AttributeKey.GroupViewerPage,
        Description = "Determines which page the link in the final confirmation screen will take you to.",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 4 )]

    [BooleanField(
        "Require Group",
        Key = AttributeKey.RequireGroup,
        Description = "If set to \"Yes\", you will be required to create a new group.",
        Category = "",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Set Registration Instance Active",
        Key = AttributeKey.SetRegistrationInstanceActive,
        Description = "If set to \"No\", the new registration instance will be created, but marked as \"inactive\".",
        Category = "",
        DefaultBooleanValue = true,
        Order = 5 )]

    [BooleanField(
        "Enable Calendar Events",
        Key = AttributeKey.EnableCalendarEvents,
        Description = "If calendar events are not enabled, registrations and groups will be created and linked, but not linked to any calendar event.",
        Category = "",
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Allow Creating New Calendar Events",
        Key = AttributeKey.AllowCreatingNewCalendarEvents,
        Description = "If set to \"Yes\", the staff person will be offered the \"New Event\" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.",
        Category = "",
        DefaultBooleanValue = false,
        Order = 7 )]

    [MemoField(
        "Instructions Lava Template",
        Key = AttributeKey.InstructionsLavaTemplate,
        Description = "Instructions added here will appear at the top of each page.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 8 )]


    [WorkflowTypeField( "Completion Workflow", "One or more workflow(s) that will be launched when a new registration is created.", false, false, "", "", 9, AttributeKey.CompletionWorkflow )]

    //// The attribute below should replace the attribute above in V9 code.
    //[WorkflowTypeField(
    //    "Completion Workflow",
    //    Key = AttributeKey.CompletionWorkflow,
    //    Description = "One or more workflow(s) that will be launched when a new registration is created.",
    //    Category = "",
    //    IsRequired = false,
    //    DefaultValue = "",
    //    AllowMultiple = false,
    //    Order = 9 )]

    #endregion

    public partial class EventRegistrationWizard : RockBlock
    {
        protected static class AttributeKey
        {
            public const string DefaultAccount = "DefaultAccount";
            public const string DefaultCalendar = "DefaultCalendar";
            public const string AvailableRegistrationTemplates = "AvailableRegistrationTemplates";
            public const string RootGroup = "RootGroup";
            public const string GroupViewerPage = "GroupViewerPage";
            public const string RequireGroup = "RequireGroup";
            public const string SetRegistrationInstanceActive = "SetRegistrationInstanceActive";
            public const string EnableCalendarEvents = "EnableCalendarEvents";
            public const string AllowCreatingNewCalendarEvents = "AllowCreatingNewCalendarEvents";
            public const string InstructionsLavaTemplate = "InstructionsLavaTemplate";
            public const string CompletionWorkflow = "CompletionWorkflow";
        }

        #region Control Methods

        private void SaveCalendarItemState(List<EventCalendarItem> itemState)
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ItemsState"] = JsonConvert.SerializeObject( itemState, Formatting.None, jsonSetting );
        }
        private List<EventCalendarItem> GetCalendarItemState()
        {
            List<EventCalendarItem> itemState;
            string json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                itemState = new List<EventCalendarItem>();
            }
            else
            {
                itemState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }
            return itemState;
        }

        #region Control Initialization

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Tell the browsers to not cache. This will help prevent browser using stale wizard stuff after navigating away from this page
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            Init_SetupAudienceControls();

            if ( !Page.IsPostBack )
            {
                SetActiveWizardStep( ActiveWizardStep.Initiate );

                using (var rockContext = new RockContext())
                {
                    Init_SetContact();
                    Init_SetRegistrationTemplateValues( rockContext );
                    Init_SetCampusAndEventSelectionOption();
                    Init_SetDefaultAccount( rockContext );
                    Init_SetDefaultCalendar( rockContext );
                    Init_SetRootGroup( rockContext );
                    Init_SetGroupRequired();
                }
            }

            ShowItemAttributes();
        }
        private void Init_SetupAudienceControls()
        {
            gAudiences.DataKeyNames = new string[] { "Guid" };
            gAudiences.Actions.ShowAdd = true;
            gAudiences.Actions.AddClick += gAudiences_Add;
            gAudiences.GridRebind += gAudiences_GridRebind;

            if ( !Page.IsPostBack )
            {
                BindAudienceGrid();
            }
        }
        private void Init_SetContact()
        {
            ppContact.SetValue( CurrentPerson );
            tbContactEmail.Text = CurrentPerson.Email;
            var pn = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
            if ( pn != null )
            {
                tbContactPhone.Text = pn.NumberFormatted;
            }
        }
        private void Init_SetRegistrationTemplateValues( RockContext rockContext )
        {
            List<Guid> registrationTemplateGuids = new List<Guid>();
            foreach (string selectedRegistrationTemplate in GetAttributeValues( AttributeKey.AvailableRegistrationTemplates ))
            {
                Guid? registrationTemplateGuid = selectedRegistrationTemplate.AsGuidOrNull();
                if ( registrationTemplateGuid != null )
                {
                    registrationTemplateGuids.Add( registrationTemplateGuid.Value );
                }
            }

            var registrationTemplates = new RegistrationTemplateService( rockContext ).GetByGuids( registrationTemplateGuids ).ToList();

            ddlTemplate.DataSource = registrationTemplates;
            ddlTemplate.DataBind();
        }
        private void Init_SetCampusAndEventSelectionOption()
        {
            if ( !GetAttributeValue ( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                ddlCampus.Enabled = false;
                ddlCampus.Visible = false;
            }
            else
            {
                ddlCampus.DataSource = CampusCache.All();
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, new ListItem( All.Text, string.Empty ) );
            }
            //else if ( !GetAttributeValue ( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            //{
            //    cpCampus.Enabled = false;
            //    cpCampus.Visible = false;
            //}

            if ( !GetAttributeValue ( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            {
                pnlNewEventSelection.Visible = false;
            }
        }
        private void Init_SetDefaultAccount( RockContext rockContext )
        {
            Guid? acctGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
            if (acctGuid != null)
            {
                var acctService = new FinancialAccountService( rockContext );
                var acct = acctService.Get( acctGuid.Value );
                apAccount.SetValue( acct );
            }

        }
        private void Init_SetDefaultCalendar( RockContext rockContext )
        {
            int defaultCalendarId = -1;
            Guid? calendarGuid = GetAttributeValue( AttributeKey.DefaultCalendar ).AsGuidOrNull();
            if (calendarGuid != null)
            {
                var calendarService = new EventCalendarService( rockContext );
                var calendar = calendarService.Get( calendarGuid.Value );
                defaultCalendarId = calendar.Id;
            }

            foreach ( var calendar in
                new EventCalendarService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( c => c.Name ) )
            {
                if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ))
                {
                    ListItem liCalendar = new ListItem( calendar.Name, calendar.Id.ToString() );
                    if (calendar.Id == defaultCalendarId)
                    {
                        liCalendar.Selected = true;
                    }
                    cblCalendars.Items.Add( liCalendar );
                }
            }

            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt();
        }
        private void Init_SetRootGroup( RockContext rockContext )
        {
            Guid? groupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            if (groupGuid != null)
            {
                var groupService = new GroupService( rockContext );
                var rootGroup = groupService.Get( groupGuid.Value );
                gpParentGroup.RootGroupId = rootGroup.Id;
            }
        }
        private void Init_SetGroupRequired()
        {
            bool groupRequired = GetAttributeValue( AttributeKey.RequireGroup ).AsBoolean();
            this.tbGroupName.Required = groupRequired;
            if ( groupRequired )
            {
                tbGroupName.Help = "If you do not enter a group name, no group will be created.";
            }
        }

        #endregion Control Initialization

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        private void SetResultLinks()
        {
            var qryGroup = new Dictionary<string, string>();
            qryGroup.Add( "GroupId", "value" );
            hlGroup.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.GroupViewerPage ), qryGroup );

            var qryRegistrationInstance = new Dictionary<string, string>();
            qryRegistrationInstance.Add( "EventId", "value" );
            //ToDo:  should this be in the system guid collection?
            hlRegistrationInstance.NavigateUrl = GetPageUrl( "844dc54b-daec-47b3-a63a-712dd6d57793", qryRegistrationInstance );

            var qryEventOccurrence = new Dictionary<string, string>();
            qryEventOccurrence.Add( "EventId", "value" );
            hlEventOccurrence.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_OCCURRENCE, qryEventOccurrence );

            var qryEventDetail = new Dictionary<string, string>();
            qryEventDetail.Add( "EventId", "value" );
            hlEventDetail.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_DETAIL, qryEventDetail );
        }
        private string GetPageUrl( string pageGuid, Dictionary<string, string> queryParams = null )
        {
            return new PageReference( pageGuid, queryParams ).BuildUrl();
        }

        #endregion


        #region "Wizard Navigation Control"

        private enum ActiveWizardStep { Initiate, Registration, Group, Event, EventOccurrence, Summary, Finished }

        private void SetActiveWizardStep( ActiveWizardStep step )
        {
            if ( step == ActiveWizardStep.Summary )
            {
                using ( var rockContext = new RockContext() )
                {
                    DisplaySummary( rockContext );
                }
            }
            else if ( step == ActiveWizardStep.Finished )
            {
                if ( !ValidateInput() )
                {
                    SetActiveWizardStep( ActiveWizardStep.Summary );
                    return;
                }

                using (var rockContext = new RockContext())
                {
                    CommitChanges( rockContext );
                }

            }

            SetupWizardCSSClasses( step );
            ShowInputPanel( step );
            SetupWizardButtons( step );
        }

        private bool ValidateInput()
        {
            return false;
        }

        private void CommitChanges( RockContext rockContext )
        {
            // Create RegistrationInstance object.
            var registrationInstance = new RegistrationInstance();
            registrationInstance.AdditionalConfirmationDetails = htmlConfirmationDetails.Text;
            registrationInstance.AdditionalReminderDetails = htmlReminderDetails.Text;
            registrationInstance.ContactPersonAliasId = ppContact.PersonAliasId;
            registrationInstance.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbContactPhone.Number );
            registrationInstance.ContactEmail = tbContactEmail.Text;

            //TODO Create Linkages.
            //registration.Linkages
            registrationInstance.Name = tbRegistrationName.Text;
            registrationInstance.RegistrationInstructions = htmlRegistrationInstructions.Text;
            registrationInstance.RegistrationTemplateId = ddlTemplate.SelectedValueAsInt().Value;
            registrationInstance.SendReminderDateTime = dtpReminderDate.SelectedDateTime;
            registrationInstance.StartDateTime = dtpRegistrationStarts.SelectedDateTime;
            registrationInstance.EndDateTime = dtpRegistrationEnds.SelectedDateTime;
            //registration.Details - should this get set?
            registrationInstance.IsActive = GetAttributeValue( AttributeKey.SetRegistrationInstanceActive ).AsBoolean();

            //TODO missing Slug
            

            // Set Maximum Attendees
            int maximumAttendees = 0;
            if (int.TryParse( numbMaximumAttendees.Text, out maximumAttendees ))
            {
                registrationInstance.MaxAttendees = maximumAttendees;
            }

            //Set Completyion Workflow
            var workFlowGuid = GetAttributeValue( AttributeKey.CompletionWorkflow ).AsGuidOrNull();
            if ( workFlowGuid != null )
            {
                var workflowType = new WorkflowTypeService( rockContext ).Get( workFlowGuid.Value );
                registrationInstance.RegistrationWorkflowTypeId = workflowType.Id;
            }

            // Set Cost variables if Cost is to be determined on the instance.
            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationTemplate = registrationTemplateService.Get( registrationInstance.RegistrationTemplateId );
            if ( registrationTemplate.SetCostOnInstance == true )
            {
                // Set Account
                registrationInstance.AccountId = apAccount.SelectedValueAsId();

                // Set Cost
                decimal cost = 0.0M;
                if (decimal.TryParse( cbCost.Text, out cost ))
                {
                    registrationInstance.Cost = cost;
                }

                // Set Minimum Payment
                decimal minimumPayment = 0.0M;
                if (decimal.TryParse( cbMinimumInitialPayment.Text, out minimumPayment ))
                {
                    registrationInstance.MinimumInitialPayment = minimumPayment;
                }

                //TODO Where does Batch Prefix go?
            }

            // Save changes to database.
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            registrationInstanceService.Add( registrationInstance );
            rockContext.SaveChanges();

            // Create Linkage.
            var linkage = new EventItemOccurrenceGroupMap();
            linkage.RegistrationInstanceId = registrationInstance.Id;
            linkage.UrlSlug = tbSlug.Text;

            // Create Group.
            if (!string.IsNullOrWhiteSpace( tbGroupName.Text ))
            {
                var groupService = new GroupService( rockContext );
                var group = new Group();
                group.Name = tbGroupName.Text;

                Group parentGroup = null;

                var parentGroupId = gpParentGroup.SelectedValueAsId();
                if ( parentGroupId != null )
                {
                    parentGroup = groupService.Get( parentGroupId.Value );
                }
                else
                {
                    Guid? rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
                    if ( rootGroupGuid != null )
                    {
                        parentGroup = groupService.Get( rootGroupGuid.Value );
                    }
                }
                if ( parentGroup != null )
                {
                    group.ParentGroupId = parentGroup.Id;
                }

                groupService.Add( group );
                rockContext.SaveChanges();

                linkage.GroupId = group.Id;
            }

            if (GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean())
            {
                if (tglEventSelection.Checked)
                {
                    var eventItemService = new EventItemService( rockContext );
                    var eventCalendarItemService = new EventCalendarItemService( rockContext );
                    var eventItemAudienceService = new EventItemAudienceService( rockContext );

                    var eventItem = new EventItem();
                    eventItem.Name = tbCalendarEventName.Text;
                    eventItem.Summary = tbEventSummary.Text;
                    eventItem.Description = htmlEventDescription.Text;
                    eventItem.IsActive = GetAttributeValue( AttributeKey.SetRegistrationInstanceActive ).AsBoolean();
                    if (eventItem.PhotoId != null)
                    {
                        eventItem.PhotoId = imgupPhoto.BinaryFileId;
                    }

                    // Add or Update audiences
                    List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
                    foreach ( int audienceId in audiencesState )
                    {
                        EventItemAudience eventItemAudience = eventItem.EventItemAudiences.Where( a => a.DefinedValueId == audienceId ).FirstOrDefault();
                        if (eventItemAudience == null)
                        {
                            eventItemAudience = new EventItemAudience();
                            eventItemAudience.DefinedValueId = audienceId;
                            eventItem.EventItemAudiences.Add( eventItemAudience );
                        }
                    }

                    // Add or Update calendar items from the UI
                    foreach ( var calendarId in cblCalendars.SelectedValuesAsInt() )
                    {
                        var calendar = eventCalendarItemService.Get( calendarId );
                        var eventCalendarItem = new EventCalendarItem();
                        eventItem.EventCalendarItems.Add( eventCalendarItem );
                        eventCalendarItem.CopyPropertiesFrom( calendar );
                    }

                    // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        foreach (EventCalendarItem eventCalendarItem in eventItem.EventCalendarItems)
                        {
                            eventCalendarItem.LoadAttributes();
                            Rock.Attribute.Helper.GetEditValues( phAttributes, eventCalendarItem );
                            eventCalendarItem.SaveAttributeValues();
                        }
                    } );

                    //TODO Save event occurrence
                    var eventItemOccurrence = new EventItemOccurrence { EventItemId = eventItem.Id };
                    eventItemOccurrence.CampusId = ddlCampus.SelectedValueAsInt();
                    eventItemOccurrence.Location = tbLocationDescription.Text;

                    string iCalendarContent = sbSchedule.iCalendarContent;
                    var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
                    if (calEvent != null && calEvent.DTStart != null)
                    {
                        if (eventItemOccurrence.Schedule == null)
                        {
                            eventItemOccurrence.Schedule = new Schedule();
                        }

                        eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                    }

                    eventItemOccurrence.ContactPersonAliasId = ppContact.PersonAliasId;
                    eventItemOccurrence.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbContactPhone.Number );
                    eventItemOccurrence.ContactEmail = tbContactEmail.Text;
                    eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                    var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                    eventItemOccurrenceService.Add( eventItemOccurrence );
                    rockContext.SaveChanges();

                    linkage.EventItemOccurrenceId = eventItemOccurrence.Id;
                }
                else
                {
                    if (eipSelectedEvent.SelectedValueAsId() != null)
                    {
                        var eventItemService = new EventItemService( rockContext );
                        var eventItem = eventItemService.Get( eipSelectedEvent.SelectedValueAsId().Value );
                        //TODO assign linkage to eventItem.EventItemOccurrences?
                        //linkage.EventItemOccurrenceId = hfLinkageEventItemOccurrenceId.Value.AsIntegerOrNull();
                    }
                }

            }

            //linkage.EventItemOccurrenceId = hfLinkageEventItemOccurrenceId.Value.AsIntegerOrNull();
            //linkage.PublicName = tbLinkagePublicName.Text;

            var linkageService = new EventItemOccurrenceGroupMapService( rockContext );
            linkageService.Add( linkage );
            rockContext.SaveChanges();


        }

        /// <summary>
        /// Sets the appropriate CSS classes (active, complete or none) on wizard div elements.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardCSSClasses( ActiveWizardStep step )
        {
            string baseClass = "wizard-item";
            string initiateClass = baseClass;
            string registrationClass = baseClass;
            string groupClass = baseClass;
            string eventClass = baseClass;
            string eventoccurrenceClass = baseClass;
            string summaryClass = baseClass;

            // Hide wizard items if the block settings don't indicate that they should be used.
            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                divEvent.Visible = true;
                //if ( GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
                //{
                    divEventOccurrence.Visible = true;
                //}
                //else
                //{
                //    divEventOccurrence.Visible = false;
                //}
            }
            else
            {
                divEvent.Visible = false;
                divEventOccurrence.Visible = false;
            }

            pnlWizard.Visible = true;
            switch (step)
            {
                case ActiveWizardStep.Initiate:
                    //pnlWizard.Visible = false;
                    initiateClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Registration:
                    initiateClass = baseClass + "  complete";
                    registrationClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Group:
                    initiateClass = baseClass + "  complete";
                    registrationClass = baseClass + "  complete";
                    groupClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Event:
                    initiateClass = baseClass + "  complete";
                    registrationClass = baseClass + " complete";
                    groupClass = baseClass + " complete";
                    eventClass = baseClass + " active";
                    break;
                case ActiveWizardStep.EventOccurrence:
                    initiateClass = baseClass + "  complete";
                    registrationClass = baseClass + " complete";
                    groupClass = baseClass + " complete";
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Summary:
                    initiateClass = baseClass + "  complete";
                    registrationClass = baseClass + " complete";
                    groupClass = baseClass + " complete";
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " complete";
                    summaryClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Finished:
                    pnlWizard.Visible = false;
                    break;
                default:
                    break;
            }

            divTemplate.Attributes.Remove( "class" );
            divTemplate.Attributes.Add( "class", initiateClass );
            divRegistration.Attributes.Remove( "class" );
            divRegistration.Attributes.Add( "class", registrationClass );
            divGroup.Attributes.Remove( "class" );
            divGroup.Attributes.Add( "class", groupClass );
            divEvent.Attributes.Remove( "class" );
            divEvent.Attributes.Add( "class", eventClass );
            divEventOccurrence.Attributes.Remove( "class" );
            divEventOccurrence.Attributes.Add( "class", eventoccurrenceClass );
            divSummary.Attributes.Remove( "class" );
            divSummary.Attributes.Add( "class", summaryClass );
        }

        /// <summary>
        /// Displays the appropriate input panel.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void ShowInputPanel( ActiveWizardStep step )
        {
            pnlInitiate.Visible = false;
            pnlRegistration.Visible = false;
            pnlGroup.Visible = false;
            pnlEvent.Visible = false;
            pnlEventOccurrence.Visible = false;
            pnlSummary.Visible = false;
            pnlFinished.Visible = false;

            switch (step)
            {
                case ActiveWizardStep.Initiate:
                    pnlInitiate.Visible = true;
                    break;
                case ActiveWizardStep.Registration:
                    pnlRegistration.Visible = true;
                    break;
                case ActiveWizardStep.Group:
                    pnlGroup.Visible = true;
                    break;
                case ActiveWizardStep.Event:
                    pnlEvent.Visible = true;
                    break;
                case ActiveWizardStep.EventOccurrence:
                    pnlEventOccurrence.Visible = true;
                    break;
                case ActiveWizardStep.Summary:
                    pnlSummary.Visible = true;
                    break;
                case ActiveWizardStep.Finished:
                    pnlFinished.Visible = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Enables or disables wizard buttons, allowing the user to go backward but not skip forward.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardButtons( ActiveWizardStep step )
        {
            lbRegistration.Enabled = false;
            lbGroup.Enabled = false;
            lbEvent.Enabled = false;
            lbEventOccurrence.Enabled = false;

            switch (step)
            {
                case ActiveWizardStep.Initiate: break;
                case ActiveWizardStep.Registration: break;
                case ActiveWizardStep.Group:
                    lbRegistration.Enabled = true;
                    break;
                case ActiveWizardStep.Event:
                    lbRegistration.Enabled = true;
                    lbGroup.Enabled = true;
                    break;
                case ActiveWizardStep.EventOccurrence:
                    lbRegistration.Enabled = true;
                    lbGroup.Enabled = true;
                    if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
                    {
                        lbEvent.Enabled = true;
                    }
                    break;
                case ActiveWizardStep.Summary:
                    lbRegistration.Enabled = true;
                    lbGroup.Enabled = true;
                    if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
                    {
                        lbEvent.Enabled = true;
                        lbEventOccurrence.Enabled = true;
                    }
                    break;
                case ActiveWizardStep.Finished: break;

                default: break;

            }
        }

        #region "Wizard LinkButton Event Handlers"

        protected void lbTemplate_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Initiate );
        }

        protected void lbRegistration_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Registration );
        }

        protected void lbGroup_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Group );
        }

        protected void lbEvent_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        protected void lbEventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
        }

        protected void lbNext_Initiate_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Registration );
        }

        protected void lbPrev_Registration_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Initiate );
        }

        protected void lbNext_Registration_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Group );
        }

        protected void lbPrev_Group_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Registration );
        }

        protected void lbNext_Group_Click( object sender, EventArgs e )
        {
            if (GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean())
            {
                SetActiveWizardStep( ActiveWizardStep.Event );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Summary );
            }
        }

        protected void lbPrev_Event_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Group );
        }

        protected void lbNext_Event_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
        }

        protected void lbPrev_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        protected void lbNext_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Summary );
        }

        protected void lbPrev_Summary_Click( object sender, EventArgs e )
        {
            if (GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean())
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Group );
            }
        }

        protected void lbNext_Summary_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Finished );
        }

        #endregion

        #endregion

        #region "Control Event Handlers"

        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            int? selectedPerson = ppContact.SelectedValue;
            if (selectedPerson == null)
            {
                tbContactEmail.Text = string.Empty;
                tbContactPhone.Text = string.Empty;
            }
            else
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var contact = personService.Get( selectedPerson.Value );

                tbContactEmail.Text = contact.Email;
                var pn = contact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                if (pn != null)
                {
                    tbContactPhone.Text = pn.NumberFormatted;
                }
            }
        }

        protected void ddlTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedTemplateId = ddlTemplate.SelectedValue.AsIntegerOrNull();
            if (selectedTemplateId == null)
            {
                pnlCosts.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationTemplateService = new RegistrationTemplateService( rockContext );
                var registrationTemplate = registrationTemplateService.Get( selectedTemplateId.Value );
                pnlCosts.Visible = registrationTemplate.SetCostOnInstance ?? true ;
            }

            // V9 code should show the description when this is selected.
        }

        protected void tglEventSelection_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglEventSelection.Checked )
            {
                pnlExistingEvent.Visible = false;
                pnlNewEvent.Visible = true;
            }
            else
            {
                pnlExistingEvent.Visible = true;
                pnlNewEvent.Visible = false;
            }
        }

        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;
        }

        #region Audience Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if (definedType != null)
            {
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !audiencesState.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ViewState["AudiencesState"] = audiencesState;

            ShowDialog( "EventItemAudience", true );
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            Guid guid = (Guid)e.RowKeyValue;
            var audience = DefinedValueCache.Get( guid );
            if (audience != null)
            {
                audiencesState.Remove( audience.Id );
            }
            ViewState["AudiencesState"] = audiencesState;

            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAudience_Click( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            int? definedValueId = ddlAudience.SelectedValueAsInt();
            if (definedValueId.HasValue)
            {
                audiencesState.Add( definedValueId.Value );
            }

            ViewState["AudiencesState"] = audiencesState;

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
        /// Binds the audience grid.
        /// </summary>
        private void BindAudienceGrid()
        {
            var values = new List<DefinedValueCache>();
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            audiencesState.ForEach( a => values.Add( DefinedValueCache.Get( a ) ) );

            gAudiences.DataSource = values
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();
            gAudiences.DataBind();
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
            switch (hfActiveDialog.Value)
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch (hfActiveDialog.Value)
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;
            }
        }

        #endregion

        protected void tbRegistrationName_TextChanged( object sender, EventArgs e )
        {
            if ((tbGroupName.Text == string.Empty) || (tbGroupName.Text == hfPreviousName.Value))
            {
                tbGroupName.Text = tbRegistrationName.Text;
            }
            hfPreviousName.Value = tbRegistrationName.Text;
        }

        protected void cblCalendars_SelectionChanged( object sender, EventArgs e )
        {
            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt();
            ShowItemAttributes();
        }

        #endregion

        #region "Event Attributes"
        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            var eventCalendarList = ( List<int> ) Session["CurrentCalendars"];
            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            using (var rockContext = new RockContext())
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach (int eventCalendarId in eventCalendarList.Distinct())
                {
                    var itemsState = GetCalendarItemState();
                    EventCalendarItem eventCalendarItem = itemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if (eventCalendarItem == null)
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        itemsState.Add( eventCalendarItem );
                    }
                    SaveCalendarItemState( itemsState );

                    eventCalendarItem.LoadAttributes();

                    if (eventCalendarItem.Attributes.Count > 0)
                    {
                        wpAttributes.Visible = true;
                        phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        PlaceHolder phcalAttributes = new PlaceHolder();
                        Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phAttributes, true, BlockValidationGroup );
                    }
                }
            }
        }

        private void DisplaySummary( RockContext rockContext )
        {
            string itemTemplate = "<p><ul><li><strong>{0}</strong> {1}</li></ul></p>";

            // Registration Summary
            string registrationDescription = "\"" + tbRegistrationName.Text + "\"" +
                " will be created from the \"" + ddlTemplate.SelectedItem.Text + "\" " +
                "template.";
            if ( !dtpRegistrationStarts.SelectedDateTimeIsBlank )
            {
                registrationDescription += " Registration opens on " +
                    dtpRegistrationStarts.SelectedDateTime.Value.ToLongDateString();

                if ( !dtpRegistrationEnds.SelectedDateTimeIsBlank )
                {
                    registrationDescription += " and closes on " +
                        dtpRegistrationEnds.SelectedDateTime.Value.ToLongDateString();
                }
                registrationDescription += ".";
            }
            var registrationLiteral = new Literal() { Text = string.Format( itemTemplate, "Registration", registrationDescription ) };
            phChanges.Controls.Add( registrationLiteral );

            // Group Summary
            if ( !string.IsNullOrWhiteSpace( tbGroupName.Text ) )
            {

                string groupDescription = "\"" + tbGroupName.Text + "\" will be created ";
                Group parentGroup = null;

                var parentGroupId = gpParentGroup.SelectedValueAsId();
                if ( parentGroupId != null )
                {
                    var groupService = new GroupService( rockContext );
                    parentGroup = groupService.Get( parentGroupId.Value );
                }
                else
                {
                    Guid? rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
                    if (rootGroupGuid != null)
                    {
                        var groupService = new GroupService( rockContext );
                        parentGroup = groupService.Get( rootGroupGuid.Value );
                    }
                }
                if ( parentGroup == null)
                {
                    groupDescription += " as a new root group.";
                }
                else
                {
                    string parentGroupTitle = parentGroup.Name;
                    while ( parentGroup.ParentGroup != null )
                    {
                        parentGroupTitle = parentGroup.ParentGroup.Name + " > " + parentGroupTitle;
                        parentGroup = parentGroup.ParentGroup;
                    }
                    groupDescription += " under the parent group the \"" + parentGroupTitle + "\".";
                }

                var groupLiteral = new Literal() { Text = string.Format( itemTemplate, "Group", groupDescription ) };
                phChanges.Controls.Add( groupLiteral );
            }

            // EventOccurrence Summary

            //EventDetail Summary
        }
        #endregion



    }
}