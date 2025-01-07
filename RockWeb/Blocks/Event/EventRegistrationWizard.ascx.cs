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
using Rock.Constants;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A wizard to simplify creation of Event Registrations.
    /// </summary>
    [DisplayName( "Event Registration Wizard" )]
    [Category( "Event" )]
    [Description( "A wizard to simplify creation of Event Registrations." )]

    #region Block Attribute Settings

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
        "Registration Instance Page",
        Key = AttributeKey.RegistrationInstancePage,
        Description = "Determines which page the link in the final confirmation screen will take you to.",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE,
        Order = 4 )]

    [LinkedPage(
        "Group Viewer Page",
        Key = AttributeKey.GroupViewerPage,
        Description = "Determines which page the link in the final confirmation screen will take you to.",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 5 )]

    [BooleanField(
        "Require Group",
        Key = AttributeKey.RequireGroup,
        Description = "If set to \"Yes\", you will be required to create a new group.",
        Category = "",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField(
        "Set Registration Instance Active",
        Key = AttributeKey.SetRegistrationInstanceActive,
        Description = "If set to \"No\", the new registration instance will be created, but marked as \"inactive\".",
        Category = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField(
        "Enable Calendar Events",
        Key = AttributeKey.EnableCalendarEvents,
        Description = "If calendar events are not enabled, registrations and groups will be created and linked, but not linked to any calendar event.",
        Category = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 8 )]

    [BooleanField(
        "Allow Creating New Calendar Events",
        Key = AttributeKey.AllowCreatingNewCalendarEvents,
        Description = "If set to \"Yes\", the staff person will be offered the \"New Event\" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.",
        Category = "",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 9 )]

    [BooleanField(
        "Require Calendar Events",
        Key = AttributeKey.RequireCalendarEvents,
        Description = "If calendar events are enabled and required, you must either select an existing calendar event or create a new one, and you must create an event occurrence.",
        Category = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 10 )]

    [BooleanField(
        "Include Inactive Calendar Items",
        Key = AttributeKey.IncludeInactiveCalendarItems,
        Description = "Check this box to hide inactive calendar items.",
        Category = "",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 11 )]

    [WorkflowTypeField(
        "Completion Workflow",
        Key = AttributeKey.CompletionWorkflow,
        Description = "A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:\r\n + Group\r\n + RegistrationInstance\r\n + EventItemOccurrenceGuid",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        AllowMultiple = false,
        Order = 12 )]

    [GroupTypesField(
        "Check-In Group Types",
        Key = AttributeKey.CheckInGroupTypes,
        Description = "Select group types which should enable check-in.  If the selected registration template is one of these types, check-in options will be enabled for the group.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 13 )]

    [BooleanField(
        "Display Link to Event Details Page on Confirmation Screen",
        Key = AttributeKey.DisplayEventDetailsLink,
        Description = "Check this box to show the link to the event details page in the wizard confirmation screen.",
        Category = "",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 14 )]

    [LinkedPage(
        "External Event Details Page",
        Key = AttributeKey.EventDetailsPage,
        Description = "Determines which page the link in the final confirmation screen will take you to (if \"Display Link to Event Details ... \" is selected).",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAILS,
        Order = 15 )]

    [BooleanField(
        "Enable Existing Group Selection ",
        Key = AttributeKey.EnableExistingGroupSelection,
        Description = "When enabled, an optional toggle switch will allow choosing an existing group or creating a new group.",
        Category = "",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 16 )]

    [BooleanField(
        "Require URL Slug",
        Key = AttributeKey.RequireURLSlug,
        Description = "If set to \"Yes\", you will be required to input a URL Slug.",
        Category = "",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 17 )]

    #region Advanced Block Attribute Settings 

    [MemoField(
        "Registration Template Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_InitiateWizard,
        Description = "Instructions here will show up on the first panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 1 )]

    [MemoField(
        "Registration Instance Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Registration,
        Description = "Instructions here will show up on the second panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 2 )]

    [MemoField(
        "Group Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Group,
        Description = "Instructions here will show up on the third panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [MemoField(
        "Event Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Event,
        Description = "Instructions here will show up on the fourth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 4 )]

    [MemoField(
        "Event Occurrence Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_EventOccurrence,
        Description = "Instructions here will show up on the fifth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 5 )]

    [MemoField(
        "Summary Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Summary,
        Description = "Instructions here will show up on the sixth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 6 )]

    [MemoField(
        "Wizard Finished Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Finished,
        Description = "Instructions here will show up on the final panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 7 )]

    #endregion Advanced Block Attribute Settings

    #endregion Block Attribute Settings

    [Rock.SystemGuid.BlockTypeGuid( "B1C7E983-5000-4CBE-84DD-6B7D428635AC" )]
    public partial class EventRegistrationWizard : RockBlock
    {
        private static class AttributeKey
        {
            public const string DefaultAccount = "DefaultAccount";
            public const string DefaultCalendar = "DefaultCalendar";
            public const string AvailableRegistrationTemplates = "AvailableRegistrationTemplates";
            public const string RootGroup = "RootGroup";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string GroupViewerPage = "GroupViewerPage";
            public const string RequireGroup = "RequireGroup";
            public const string SetRegistrationInstanceActive = "SetRegistrationInstanceActive";
            public const string EnableCalendarEvents = "EnableCalendarEvents";
            public const string AllowCreatingNewCalendarEvents = "AllowCreatingNewCalendarEvents";
            public const string RequireCalendarEvents = "RequireCalendarEvents";
            public const string IncludeInactiveCalendarItems = "IncludeInactiveCalendarItems";
            public const string CompletionWorkflow = "CompletionWorkflow";
            public const string CheckInGroupTypes = "CheckInGroupTypes";
            public const string DisplayEventDetailsLink = "DisplayEventDetailsLink";
            public const string EventDetailsPage = "EventDetailsPage";
            public const string EnableExistingGroupSelection = "EnableExistingGroupSelection ";
            public const string RequireURLSlug = "RequireURLSlug ";

            public const string LavaInstruction_InitiateWizard = "LavaInstruction_InitiateWizard";
            public const string LavaInstruction_Registration = "LavaInstruction_Registration";
            public const string LavaInstruction_Group = "LavaInstruction_Group";
            public const string LavaInstruction_Event = "LavaInstruction_Event";
            public const string LavaInstruction_EventOccurrence = "LavaInstruction_EventOccurrence";
            public const string LavaInstruction_Summary = "LavaInstruction_Summary";
            public const string LavaInstruction_Finished = "LavaInstruction_Finished";
        }

        #region Control Methods

        /// <summary>
        /// Save event calendar items state to ViewState.
        /// </summary>
        /// <param name="itemState">The list of EventCalendarItems to save.</param>
        private void SaveCalendarItemState( List<EventCalendarItem> itemState )
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ItemsState"] = JsonConvert.SerializeObject( itemState, Formatting.None, jsonSetting );
        }

        /// <summary>
        /// Retrieve event calendar items state from ViewState.
        /// </summary>
        /// <returns>Returns a List of EventCalendarItem objects.</returns>
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

        private void SaveSelectedTemplate( RegistrationTemplate template )
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["SelectedTemplate"] = JsonConvert.SerializeObject( template, Formatting.None, jsonSetting );
        }

        private RegistrationTemplate GetSelectedTemplate()
        {
            RegistrationTemplate template;
            string json = ViewState["SelectedTemplate"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                template = null;
            }
            else
            {
                template = JsonConvert.DeserializeObject<RegistrationTemplate>( json );
            }
            return template;
        }

        /// <summary>
        /// Handles the selection process for a given Registration Template.
        /// </summary>
        /// <param name="selectedTemplateId">The Id of the selected Registration Template</param>
        private void HandleRegistrationTemplateSelection( int? selectedTemplateId )
        {
            if ( selectedTemplateId == null )
            {
                pnlCosts.Visible = true;
                pnlDefaultPayment.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationTemplateService = new RegistrationTemplateService( rockContext );
                var registrationTemplate = registrationTemplateService.Get( selectedTemplateId.Value );
                SaveSelectedTemplate( registrationTemplate );
                pnlCosts.Visible = registrationTemplate.SetCostOnInstance ?? false;
                pnlDefaultPayment.Visible = registrationTemplate.SetCostOnInstance ?? false;
                if ( !registrationTemplate.GroupTypeId.HasValue )
                {
                    tbGroupName.Text = string.Empty;
                    pnlCheckinOptions.Visible = false;
                }
                else
                {
                    pnlCheckinOptions.Visible = GroupTypeIsCheckinGroup( registrationTemplate.GroupTypeId.Value, rockContext );
                }

                lTemplateDescription.Text = registrationTemplate.Description;
                dpPaymentDeadline.Required = registrationTemplate.IsPaymentPlanAllowed == true;
                dpPaymentDeadline.Visible = registrationTemplate.IsPaymentPlanAllowed == true;
            }
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            if ( Session["CurrentCalendars"] == null )
            {
                // If the user's session has expired, create a new list in session and reset them
                // to the first step of the wizard.
                if ( cblCalendars.SelectedValuesAsInt.Count() > 0 )
                {
                    Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
                }
                else
                {
                    Session["CurrentCalendars"] = new List<int>();
                }
                SetActiveWizardStep( ActiveWizardStep.Registration );
            }

            var eventCalendarList = ( List<int> ) Session["CurrentCalendars"];
            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList.Distinct() )
                {
                    var itemsState = GetCalendarItemState();
                    var eventCalendarItem = itemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        itemsState.Add( eventCalendarItem );
                    }
                    SaveCalendarItemState( itemsState );

                    eventCalendarItem.LoadAttributes();
                    if ( eventCalendarItem.Attributes.Count > 0 )
                    {
                        wpAttributes.Visible = true;
                        phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        Helper.AddEditControls( eventCalendarItem, phAttributes, true, BlockValidationGroup );
                    }
                }
            }
        }

        /// <summary>
        /// Display the summary of items that will be created.
        /// </summary>
        /// <param name="rockContext"></param>
        private void DisplaySummary( RockContext rockContext )
        {
            string itemTemplate = "<p><ul><li><strong>{0}</strong> {1}</li></ul></p>";

            // Registration Summary
            string registrationDescription = "\"" + tbRegistrationName.Text + "\"" + " will be created from the \"" + GetSelectedTemplate().Name + "\" " + "template.";

            if ( !dtpRegistrationStarts.SelectedDateTimeIsBlank )
            {
                registrationDescription += " Registration opens on " + dtpRegistrationStarts.SelectedDateTime.Value.ToLongDateString();
                if ( !dtpRegistrationEnds.SelectedDateTimeIsBlank )
                {
                    registrationDescription += " and closes on " + dtpRegistrationEnds.SelectedDateTime.Value.ToLongDateString();
                }
                registrationDescription += ".";
            }
            var registrationLiteral = new Literal() { Text = string.Format( itemTemplate, "Registration", registrationDescription ) };
            phChanges.Controls.Add( registrationLiteral );

            // Group Summary
            if ( !string.IsNullOrWhiteSpace( tbGroupName.Text ) || gpExistingGroup.SelectedIds.Length > 0 )
            {
                string groupDescription = string.Empty;
                var groupService = new GroupService( rockContext );

                if ( !string.IsNullOrWhiteSpace( tbGroupName.Text ) )
                {
                    groupDescription = "\"" + tbGroupName.Text + "\" will be created ";

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
                    if ( parentGroup == null )
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
                }
                else
                {
                    var selectedGroup = groupService.Get( gpExistingGroup.SelectedValueAsId() ?? 0 );
                    if ( selectedGroup != null )
                    {
                        groupDescription = "\"" + selectedGroup.Name + "\" will be used.";
                    }
                }

                List<int> selectedSchedules = spGroupLocationSchedule.SelectedValuesAsInt().ToList();
                selectedSchedules.Remove( 0 );
                if ( lpGroupLocation.Location != null )
                {
                    groupDescription += "  Location: " + lpGroupLocation.Location.Name;
                    if ( selectedSchedules.Any() )
                    {
                        groupDescription += ",";
                    }
                    else
                    {
                        groupDescription += ".";
                    }
                }

                string groupSchedules = "";
                if ( selectedSchedules.Any() )
                {
                    var scheduleService = new ScheduleService( rockContext );

                    foreach ( int selectedScheduleId in selectedSchedules )
                    {
                        if ( !string.IsNullOrWhiteSpace( groupSchedules ) )
                        {
                            groupSchedules += ", ";
                        }
                        groupSchedules += scheduleService.Get( selectedScheduleId ).Name;
                    }
                    groupDescription += "  Schedule(s): " + groupSchedules + ".";
                }

                var groupLiteral = new Literal() { Text = string.Format( itemTemplate, "Group", groupDescription ) };
                phChanges.Controls.Add( groupLiteral );
            }

            // Calendar Event Summary
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                string eventDescription;
                var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
                if ( tglEventSelection.Checked )
                {
                    eventDescription = "An event occurrence will be created for the new \"" + tbCalendarEventName.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }
                else
                {
                    eventDescription = "An event occurrence will be created for the \"" + eipSelectedEvent.SelectedItem.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }

                var eventLiteral = new Literal() { Text = string.Format( itemTemplate, "Calendar Event", eventDescription ) };
                phChanges.Controls.Add( eventLiteral );
            }
        }

        public class CommitResult
        {
            public string RegistrationInstanceId, GroupId, EventItemId, FirstEventCalendarId, EventOccurrenceId, RegistrationTitle;
            public CommitResult()
            {
                RegistrationInstanceId = "";
                GroupId = "";
                EventItemId = "";
                FirstEventCalendarId = "";
                EventOccurrenceId = "";
                RegistrationTitle = "";
            }
        }

        /// <summary>
        /// This method commits all changes to the database at once.
        /// </summary>
        /// <param name="rockContext"></param>
        private CommitResult CommitChanges( RockContext rockContext )
        {
            var result = new CommitResult();

            // Create RegistrationInstance object.
            var registrationInstance = new RegistrationInstance
            {
                AdditionalConfirmationDetails = htmlConfirmationDetails.Text,
                AdditionalReminderDetails = htmlReminderDetails.Text,
                ContactPersonAliasId = ppContact.PersonAliasId,
                ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbContactPhone.Number ),
                ContactEmail = tbContactEmail.Text,
                Name = tbRegistrationName.Text,
                RegistrationInstructions = htmlRegistrationInstructions.Text,
                RegistrationTemplateId = GetSelectedTemplate().Id,
                SendReminderDateTime = dtpReminderDate.SelectedDateTime,
                StartDateTime = dtpRegistrationStarts.SelectedDateTime,
                EndDateTime = dtpRegistrationEnds.SelectedDateTime,
                IsActive = GetAttributeValue( AttributeKey.SetRegistrationInstanceActive ).AsBoolean(),
                PaymentDeadlineDate = dpPaymentDeadline.SelectedDate
            };

            // Set Maximum Attendees
            var maximumAttendees = 0;
            if ( int.TryParse( numbMaximumAttendees.Text, out maximumAttendees ) )
            {
                registrationInstance.MaxAttendees = maximumAttendees;
            }

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationTemplate = registrationTemplateService.Get( registrationInstance.RegistrationTemplateId );
            registrationInstance.AccountId = apAccount.SelectedValueAsId();

            // Set Cost variables if Cost is to be determined on the instance.
            if ( registrationTemplate.SetCostOnInstance == true )
            {
                registrationInstance.Cost = cbCost.Value;
                registrationInstance.MinimumInitialPayment = cbMinimumInitialPayment.Value;
                registrationInstance.DefaultPayment = cbDefaultPaymentAmount.Value;
            }

            // Save changes to database.
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            registrationInstanceService.Add( registrationInstance );
            rockContext.SaveChanges();
            result.RegistrationInstanceId = registrationInstance.Id.ToString();
            result.RegistrationTitle = registrationInstance.Name;

            // Create Linkage.
            var linkage = new EventItemOccurrenceGroupMap();
            linkage.PublicName = tbPublicName.Text;
            linkage.RegistrationInstanceId = registrationInstance.Id;
            linkage.UrlSlug = tbSlug.Text;

            // Create Group.
            if ( !string.IsNullOrWhiteSpace( tbGroupName.Text ) || gpExistingGroup.SelectedIds.Length > 0 )
            {
                var groupService = new GroupService( rockContext );
                var group = new Group();

                if ( gpExistingGroup.SelectedIds.Length > 0 )
                {
                    var groupId = gpExistingGroup.SelectedValueAsId() ?? 0;
                    group = groupService.Get( groupId );
                }
                else
                {
                    group.Name = tbGroupName.Text;
                    group.GroupTypeId = registrationTemplate.GroupTypeId.Value; // RegistrationTemplate MUST have a group type value if the user is creating a group.

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

                    // Set group CampusId if campus is selected.
                    if ( ( ddlCampus.Enabled ) && ( !string.IsNullOrWhiteSpace( ddlCampus.SelectedValue ) ) )
                    {
                        group.CampusId = ddlCampus.SelectedValueAsInt();
                    }

                    groupService.Add( group );
                    rockContext.SaveChanges();
                }

                result.GroupId = group.Id.ToString();

                // Add GrouId to linkage.
                linkage.GroupId = group.Id;

                // Add GroupLocation and GroupLocationSchedule
                if ( GroupTypeIsCheckinGroup( group.GroupTypeId, rockContext ) )
                {
                    if ( lpGroupLocation.Location != null )
                    {
                        var groupLocation = new GroupLocation();
                        groupLocation.LocationId = lpGroupLocation.Location.Id;

                        List<int> selectedSchedules = spGroupLocationSchedule.SelectedValuesAsInt().ToList();
                        selectedSchedules.Remove( 0 );
                        if ( selectedSchedules.Any() )
                        {
                            var scheduleService = new ScheduleService( rockContext );
                            foreach ( int selectedScheduleId in selectedSchedules )
                            {
                                var groupLocationSchedule = scheduleService.Get( selectedScheduleId );
                                groupLocation.Schedules.Add( groupLocationSchedule );
                            }
                        }

                        group.GroupLocations.Add( groupLocation );
                        rockContext.SaveChanges();
                    }
                }
            }

            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                EventItem eventItem = null;
                if ( tglEventSelection.Checked )  // "New Event" option selected.
                {
                    var eventItemService = new EventItemService( rockContext );
                    var eventCalendarItemService = new EventCalendarItemService( rockContext );
                    var eventItemAudienceService = new EventItemAudienceService( rockContext );

                    // Create new EventItem
                    eventItem = new EventItem();
                    eventItem.Name = tbCalendarEventName.Text;
                    eventItem.Summary = tbEventSummary.Text;
                    eventItem.Description = htmlEventDescription.Text;
                    eventItem.IsActive = GetAttributeValue( AttributeKey.SetRegistrationInstanceActive ).AsBoolean();
                    if ( imgupPhoto.BinaryFileId != null )
                    {
                        eventItem.PhotoId = imgupPhoto.BinaryFileId;
                    }

                    // Add audiences
                    List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
                    foreach ( int audienceId in audiencesState )
                    {
                        var eventItemAudience = new EventItemAudience();
                        eventItemAudience.DefinedValueId = audienceId;
                        eventItem.EventItemAudiences.Add( eventItemAudience );
                    }

                    // Add calendar items from the UI
                    var calendarIds = new List<int>();
                    calendarIds.AddRange( cblCalendars.SelectedValuesAsInt );
                    var itemsState = GetCalendarItemState();
                    foreach ( var calendar in itemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ) )
                    {
                        var eventCalendarItem = new EventCalendarItem();
                        eventItem.EventCalendarItems.Add( eventCalendarItem );
                        eventCalendarItem.CopyPropertiesFrom( calendar );
                    }

                    eventItemService.Add( eventItem );
                    rockContext.SaveChanges();

                    foreach ( var eventCalendarItem in eventItem.EventCalendarItems.OrderBy( i => i.EventCalendarId ) )
                    {
                        eventCalendarItem.LoadAttributes();
                        Helper.GetEditValues( phAttributes, eventCalendarItem );
                        eventCalendarItem.SaveAttributeValues();
                        if ( string.IsNullOrWhiteSpace( result.FirstEventCalendarId ) )
                        {
                            result.FirstEventCalendarId = eventCalendarItem.EventCalendarId.ToString();
                        }
                    }
                    result.EventItemId = eventItem.Id.ToString();
                }
                else // "Existing Event" option selected.
                {
                    if ( eipSelectedEvent.SelectedValueAsId() != null )
                    {
                        var eventItemService = new EventItemService( rockContext );
                        eventItem = eventItemService.Get( eipSelectedEvent.SelectedValueAsId().Value );
                    }
                }

                // if eventItem is null, no EventItem was selected or created and we will not create an occurrence, either.
                if ( eventItem != null )
                {
                    // Create new EventItemOccurrence.
                    var eventItemOccurrence = new EventItemOccurrence { EventItemId = eventItem.Id };
                    eventItemOccurrence.CampusId = ddlCampus.SelectedValueAsInt();
                    eventItemOccurrence.Location = tbLocationDescription.Text;
                    eventItemOccurrence.ContactPersonAliasId = ppContact.PersonAliasId;
                    eventItemOccurrence.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbContactPhone.Number );
                    eventItemOccurrence.ContactEmail = tbContactEmail.Text;
                    eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                    // Set Calendar.
                    string iCalendarContent = sbSchedule.iCalendarContent ?? string.Empty;
                    var calEvent = InetCalendarHelper.CreateCalendarEvent( iCalendarContent );
                    if ( calEvent != null && calEvent.DtStart != null )
                    {
                        if ( eventItemOccurrence.Schedule == null )
                        {
                            eventItemOccurrence.Schedule = new Schedule();
                        }

                        eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                    }

                    var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                    eventItemOccurrenceService.Add( eventItemOccurrence );
                    rockContext.SaveChanges();
                    result.EventOccurrenceId = eventItemOccurrence.Id.ToString();

                    // Add EventItemOccurrenceId to linkage.
                    linkage.EventItemOccurrenceId = eventItemOccurrence.Id;
                }
            }

            var linkageService = new EventItemOccurrenceGroupMapService( rockContext );
            linkageService.Add( linkage );
            rockContext.SaveChanges();

            LaunchPostWizardWorkflow( rockContext, linkage );

            return result;
        }

        /// <summary>
        /// Builds the link URLs for each object on the final page of the wizard.
        /// </summary>
        private void SetResultLinks( CommitResult result )
        {
            lblEventRegistrationTitle.Text = result.RegistrationTitle;
            if ( string.IsNullOrWhiteSpace( result.RegistrationInstanceId ) )
            {
                liRegistrationLink.Visible = false;
            }
            else
            {
                var qryRegistrationInstance = new Dictionary<string, string>();
                qryRegistrationInstance.Add( "RegistrationInstanceId", result.RegistrationInstanceId );
                hlRegistrationInstance.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.RegistrationInstancePage ), qryRegistrationInstance );
            }

            if ( string.IsNullOrWhiteSpace( result.GroupId ) )
            {
                liGroupLink.Visible = false;
            }
            else
            {
                var qryGroup = new Dictionary<string, string>();
                qryGroup.Add( "GroupId", result.GroupId );
                hlGroup.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.GroupViewerPage ), qryGroup );
            }

            if ( string.IsNullOrWhiteSpace( result.EventItemId ) )
            {
                liEventLink.Visible = false;
            }
            else
            {
                var qryEventDetail = new Dictionary<string, string>();
                qryEventDetail.Add( "EventItemId", result.EventItemId );
                qryEventDetail.Add( "EventCalendarId", result.FirstEventCalendarId );
                hlEventDetail.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_DETAIL, qryEventDetail );
            }

            if ( string.IsNullOrWhiteSpace( result.EventOccurrenceId ) )
            {
                liEventOccurrenceLink.Visible = false;
            }
            else
            {
                var qryEventOccurrence = new Dictionary<string, string>();
                qryEventOccurrence.Add( "EventItemOccurrenceId", result.EventOccurrenceId );
                hlEventOccurrence.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_OCCURRENCE, qryEventOccurrence );

                bool showEventDetailsLink = GetAttributeValue( AttributeKey.DisplayEventDetailsLink ).AsBoolean();
                if ( !showEventDetailsLink )
                {
                    liExternalEventLink.Visible = false;
                }
                else
                {
                    var qryExternalEventOccurrence = new Dictionary<string, string>();
                    qryExternalEventOccurrence.Add( "EventOccurrenceId", result.EventOccurrenceId );
                    hlExternalEventDetails.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.EventDetailsPage ), qryExternalEventOccurrence );
                }
            }
        }

        /// <summary>
        /// Gets a specific page URL from a PageReference.
        /// </summary>
        /// <param name="pageGuid">The Guid of the page.</param>
        /// <param name="queryParams">Optional query parameters to be included in the URL.</param>
        /// <returns>Returns a string representing a specific page URL from a PageReference.</returns>
        private string GetPageUrl( string pageGuid, Dictionary<string, string> queryParams = null )
        {
            return new PageReference( pageGuid, queryParams ).BuildUrl();
        }

        /// <summary>
        /// Checks to see if the selected RegistrationTemplate has a GroupTypeId.
        /// </summary>
        /// <returns>Returns true if the RegistrationTemplate has a GroupTypeId.</returns>
        private bool SelectedTemplateHasGroupType()
        {
            var registrationTemplate = GetSelectedTemplate();
            if ( registrationTemplate == null )
            {
                return false;
            }
            return registrationTemplate.GroupTypeId.HasValue;
        }

        private bool ValidateParentGroupSelection( RockContext rockContext )
        {
            nbNotAuthorized.Visible = false;
            nbNotPermitted.Visible = false;

            if ( tbGroupName.Text == string.Empty )
            {
                return true;
            }

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
                if ( rootGroupGuid != null )
                {
                    var groupService = new GroupService( rockContext );
                    parentGroup = groupService.Get( rootGroupGuid.Value );
                }
            }

            if ( parentGroup != null )
            {
                bool isAuthorized = parentGroup.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) ||
                    parentGroup.IsAuthorized( Authorization.MANAGE_MEMBERS, CurrentPerson );
                if ( !isAuthorized )
                {
                    nbNotAuthorized.Visible = true;
                    return false;
                }
                else
                {
                    var registrationTemplate = GetSelectedTemplate();
                    var groupTypeService = new GroupTypeService( rockContext );
                    var templateGroupType = groupTypeService.Get( registrationTemplate.GroupTypeId.Value );
                    var parentGroupType = groupTypeService.Get( parentGroup.GroupTypeId );

                    bool isChildPermitted = parentGroupType.AllowAnyChildGroupType || parentGroupType.ChildGroupTypes.Contains( templateGroupType );
                    if ( !isChildPermitted )
                    {
                        nbNotPermitted.Text = string.Format( "Groups of type \"{0}\" are not permitted under the parent \"{1}\".", templateGroupType.Name, parentGroup.Name );
                        nbNotPermitted.Visible = true;
                        return false;
                    }
                }
            }

            return true;
        }

        private bool GroupTypeIsCheckinGroup( int groupTypeId, RockContext rockContext )
        {
            var checkInGroupGuids = GetAttributeValues( AttributeKey.CheckInGroupTypes ).AsGuidList();
            if ( !checkInGroupGuids.Any() )
            {
                return false;
            }

            var groupTypeService = new GroupTypeService( rockContext );
            var checkinGroups = groupTypeService.Queryable().AsNoTracking()
                .Where( gt => checkInGroupGuids.Contains( gt.Guid ) )
                .Where( gt => gt.Id == groupTypeId );
            return checkinGroups.Any();
        }

        #region Control Initialization

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;

            // Tell the browsers to not cache. This will help prevent browser using stale wizard stuff after navigating away from this page
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            // Hide inactive events if the option has been selected.
            eipSelectedEvent.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCalendarItems ).AsBoolean();

            Init_SetupAudienceControls();
            Init_GroupControls();

            if ( !Page.IsPostBack )
            {
                SetActiveWizardStep( ActiveWizardStep.InitiateWizard );

                using ( var rockContext = new RockContext() )
                {
                    Init_SetContact();
                    Init_SetRegistrationTemplateSelector( rockContext );
                    Init_SetCampusAndEventSelectionOption();
                    Init_SetDefaultAccount( rockContext );
                    Init_SetDefaultCalendar( rockContext );
                    Init_SetRootGroup( rockContext );
                    Init_SetGroupRequired();
                    Init_SetCalendarEventRequired();
                    Init_SetURLSlugRequired();
                }
            }

            // Build calendar item attributes on every Init event to ensure they are populated by ViewState.
            ShowItemAttributes();
        }

        private void Init_GroupControls()
        {
            var enableExistingGroupSelection = GetAttributeValue( AttributeKey.EnableExistingGroupSelection ).AsBoolean();
            tgExistingroup.Visible = enableExistingGroupSelection;
            pnlExistingGroup.Visible = enableExistingGroupSelection && tgExistingroup.Checked;
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
            foreach ( string selectedRegistrationTemplate in GetAttributeValues( AttributeKey.AvailableRegistrationTemplates ) )
            {
                Guid? registrationTemplateGuid = selectedRegistrationTemplate.AsGuidOrNull();
                if ( registrationTemplateGuid != null )
                {
                    registrationTemplateGuids.Add( registrationTemplateGuid.Value );
                }
            }

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationTemplateQuery = registrationTemplateService.GetByGuids( registrationTemplateGuids ).AsNoTracking();

            // If no registration templates were selected in the block settings, all active registration templates will be available.
            if ( registrationTemplateGuids.Count == 0 )
            {
                registrationTemplateQuery = registrationTemplateService.Queryable().AsNoTracking().Where( rt => rt.IsActive == true );
            }

            // If Group is required, only registration templates with a GroupType set will be available.
            if ( GetAttributeValue( AttributeKey.RequireGroup ).AsBoolean() )
            {
                registrationTemplateQuery = registrationTemplateQuery.Where( rt => rt.GroupTypeId.HasValue );
            }

            var registrationTemplates = registrationTemplateQuery.ToList().OrderBy( r => r.Name );
            ddlTemplate.DataSource = registrationTemplates;
            ddlTemplate.DataBind();
        }

        private void Init_SetCampusAndEventSelectionOption()
        {
            // Hide wizard items if the block settings don't indicate that they should be used.
            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                divEvent.Visible = true;
                divEventOccurrence.Visible = true;

                ddlCampus.DataSource = CampusCache.All( includeInactive: false );
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, new ListItem( All.Text, string.Empty ) );
            }
            else
            {
                divEvent.Visible = false;
                divEventOccurrence.Visible = false;

                ddlCampus.Enabled = false;
                ddlCampus.Visible = false;
            }

            if ( !GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            {
                pnlNewEventSelection.Visible = false;
            }
        }

        private void Init_SetDefaultAccount( RockContext rockContext )
        {
            Guid? acctGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
            if ( acctGuid != null )
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
            if ( calendarGuid != null )
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
                if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    ListItem liCalendar = new ListItem( calendar.Name, calendar.Id.ToString() );
                    if ( calendar.Id == defaultCalendarId )
                    {
                        liCalendar.Selected = true;
                    }
                    cblCalendars.Items.Add( liCalendar );
                }
            }

            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
        }

        private void Init_SetRootGroup( RockContext rockContext )
        {
            Guid? groupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            if ( groupGuid != null )
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
            if ( !groupRequired )
            {
                tbGroupName.Help = "If you do not enter a group name, no group will be created.";
            }
        }

        private void Init_SetCalendarEventRequired()
        {
            bool requireEvent = GetAttributeValue( AttributeKey.RequireCalendarEvents ).AsBoolean();
            eipSelectedEvent.Required = requireEvent;
            if ( !requireEvent )
            {
                eipSelectedEvent.Help = "If you do not select an event item, no event occurrence will be created.";
            }
        }

        private void Init_SetURLSlugRequired()
        {
            bool slugRequired = GetAttributeValue( AttributeKey.RequireURLSlug ).AsBoolean();
            this.tbSlug.Required = slugRequired;
            if ( !slugRequired )
            {
                tbSlug.Help = "If you do not enter a URL Slug, no registration will be created.";
            }
        }

        private void Init_SetRegistrationTemplateSelector( RockContext rockContext )
        {
            bool groupRequired = GetAttributeValue( AttributeKey.RequireGroup ).AsBoolean();
            List<string> selectedRegistrationTemplates = GetAttributeValues( AttributeKey.AvailableRegistrationTemplates );

            if ( groupRequired || selectedRegistrationTemplates.Count > 0 )
            {
                rtpRegistrationTemplate.Visible = false;
                ddlTemplate.Visible = true;
                Init_SetRegistrationTemplateValues( rockContext );
            }
            else
            {
                rtpRegistrationTemplate.Visible = true;
                ddlTemplate.Visible = false;
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

        #endregion Control Methods

        #region Wizard Navigation Control

        private enum ActiveWizardStep { InitiateWizard, Registration, Group, Event, EventOccurrence, Summary, Finished }

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
                CommitResult result = null;
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        result = CommitChanges( rockContext );
                    } );
                }
                SetResultLinks( result );
            }

            SetLavaInstructions( step );
            SetupWizardCSSClasses( step );
            ShowInputPanel( step );
            SetupWizardButtons( step );
        }
        private void SetLavaInstructions( ActiveWizardStep step )
        {
            pnlLavaInstructions.Visible = false;
            string lavaTemplate = string.Empty;
            switch ( step )
            {
                case ActiveWizardStep.InitiateWizard:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_InitiateWizard );
                    break;
                case ActiveWizardStep.Registration:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Registration );
                    break;
                case ActiveWizardStep.Group:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Group );
                    break;
                case ActiveWizardStep.Event:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Event );
                    break;
                case ActiveWizardStep.EventOccurrence:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_EventOccurrence );
                    break;
                case ActiveWizardStep.Summary:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Summary );
                    break;
                case ActiveWizardStep.Finished:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Finished );
                    break;
            }

            if ( lavaTemplate != string.Empty )
            {
                pnlLavaInstructions.Visible = true;
                var mergeObjects = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "ActiveWizardStep", step.ToString() );
                mergeObjects.Add( "Page", ( int ) step + 1 );
                lLavaInstructions.Text = lavaTemplate.ResolveMergeFields( mergeObjects );
            }
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

            pnlWizard.Visible = true;
            switch ( step )
            {
                case ActiveWizardStep.InitiateWizard:
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
            pnlInitiate_Header.Visible = false;
            pnlRegistration.Visible = false;
            pnlRegistration_Header.Visible = false;
            pnlGroup.Visible = false;
            pnlGroup_Header.Visible = false;
            pnlEvent.Visible = false;
            pnlEvent_Header.Visible = false;
            pnlEventOccurrence.Visible = false;
            pnlEventOccurrence_Header.Visible = false;
            pnlSummary.Visible = false;
            pnlSummary_Header.Visible = false;
            pnlFinished.Visible = false;
            pnlFinished_Header.Visible = false;

            switch ( step )
            {
                case ActiveWizardStep.InitiateWizard:
                    pnlInitiate.Visible = true;
                    pnlInitiate_Header.Visible = true;
                    break;
                case ActiveWizardStep.Registration:
                    pnlRegistration.Visible = true;
                    pnlRegistration_Header.Visible = true;
                    break;
                case ActiveWizardStep.Group:
                    pnlGroup.Visible = true;
                    pnlGroup_Header.Visible = true;
                    break;
                case ActiveWizardStep.Event:
                    pnlEvent.Visible = true;
                    pnlEvent_Header.Visible = true;
                    break;
                case ActiveWizardStep.EventOccurrence:
                    pnlEventOccurrence.Visible = true;
                    pnlEventOccurrence_Header.Visible = true;
                    break;
                case ActiveWizardStep.Summary:
                    pnlSummary.Visible = true;
                    pnlSummary_Header.Visible = true;
                    break;
                case ActiveWizardStep.Finished:
                    pnlFinished.Visible = true;
                    pnlFinished_Header.Visible = true;
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

            switch ( step )
            {
                case ActiveWizardStep.InitiateWizard:
                    break;
                case ActiveWizardStep.Registration:
                    break;
                case ActiveWizardStep.Group:
                    lbRegistration.Enabled = true;
                    break;
                case ActiveWizardStep.Event:
                    lbRegistration.Enabled = true;
                    if ( SelectedTemplateHasGroupType() )
                    {
                        lbGroup.Enabled = true;
                    }
                    break;
                case ActiveWizardStep.EventOccurrence:
                    lbRegistration.Enabled = true;
                    if ( SelectedTemplateHasGroupType() )
                    {
                        lbGroup.Enabled = true;
                    }
                    if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
                    {
                        lbEvent.Enabled = true;
                    }
                    break;
                case ActiveWizardStep.Summary:
                    lbRegistration.Enabled = true;
                    if ( SelectedTemplateHasGroupType() )
                    {
                        lbGroup.Enabled = true;
                    }
                    if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
                    {
                        lbEvent.Enabled = true;
                        if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
                        {
                            lbEventOccurrence.Enabled = true;
                        }
                        else
                        {
                            lbEventOccurrence.Enabled = false;
                        }
                    }
                    break;
                case ActiveWizardStep.Finished:
                    break;
                default:
                    break;
            }
        }

        #region Wizard LinkButton Event Handlers

        protected void lbTemplate_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.InitiateWizard );
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
            SetActiveWizardStep( ActiveWizardStep.InitiateWizard );
        }

        protected void lbNext_Registration_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            if ( SelectedTemplateHasGroupType() )
            {
                SetActiveWizardStep( ActiveWizardStep.Group );
            }
            else if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                SetActiveWizardStep( ActiveWizardStep.Event );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Summary );
            }
        }

        protected void lbPrev_Group_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Registration );
        }

        protected void lbNext_Group_Click( object sender, EventArgs e )
        {
            bool groupSelectionIsValid = false;

            using ( var rockContext = new RockContext() )
            {
                groupSelectionIsValid = ValidateParentGroupSelection( rockContext );
            }

            if ( !groupSelectionIsValid )
            {
                SetActiveWizardStep( ActiveWizardStep.Group );
            }
            else if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
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
            if ( SelectedTemplateHasGroupType() )
            {
                SetActiveWizardStep( ActiveWizardStep.Group );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Registration );
            }
        }

        protected void lbNext_Event_Click( object sender, EventArgs e )
        {
            bool requireEvent = GetAttributeValue( AttributeKey.RequireCalendarEvents ).AsBoolean();
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Summary );
            }
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
            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
                {
                    SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
                }
                else
                {
                    SetActiveWizardStep( ActiveWizardStep.Event );
                }
            }
            else
            {
                if ( SelectedTemplateHasGroupType() )
                {
                    SetActiveWizardStep( ActiveWizardStep.Group );
                }
                else
                {
                    SetActiveWizardStep( ActiveWizardStep.Registration );
                }
            }
        }

        protected void lbNext_Summary_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Finished );
        }

        #endregion Wizard LinkButton Event Handlers

        #endregion Wizard Navigation Control

        #region Control Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload page if block settings where changed
            this.NavigateToCurrentPageReference();
        }

        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            int? selectedPerson = ppContact.SelectedValue;
            if ( selectedPerson == null )
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
                if ( pn != null )
                {
                    tbContactPhone.Text = pn.NumberFormatted;
                }
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the rtpRegistrationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rtpRegistrationTemplate_SelectItem( object sender, EventArgs e )
        {
            HandleRegistrationTemplateSelection( rtpRegistrationTemplate.SelectedValueAsId() );
        }

        protected void ddlTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            HandleRegistrationTemplateSelection( ddlTemplate.SelectedValue.AsIntegerOrNull() );
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

        protected void tbRegistrationName_TextChanged( object sender, EventArgs e )
        {
            if ( SelectedTemplateHasGroupType() )
            {
                if ( ( tbGroupName.Text == string.Empty ) || ( tbGroupName.Text == hfPreviousName.Value ) )
                {
                    tbGroupName.Text = tbRegistrationName.Text;
                }
            }
            else
            {
                tbGroupName.Text = string.Empty;
            }
            hfPreviousName.Value = tbRegistrationName.Text;
        }

        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
            ShowItemAttributes();
        }

        protected void gpParentGroup_SelectItem( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ValidateParentGroupSelection( rockContext );
            }
        }

        protected void cvUrlSlug_ServerValidate( object source, ServerValidateEventArgs args )
        {
            var urlSlug = args.Value;
            if ( urlSlug.IsNullOrWhiteSpace() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var eventMapingService = new EventItemOccurrenceGroupMapService( rockContext );

                args.IsValid = !eventMapingService.Queryable().AsNoTracking().Any( m => m.UrlSlug == urlSlug );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgExistingroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgExistingroup_CheckedChanged( object sender, EventArgs e )
        {
            pnlNewGroup.Visible = !tgExistingroup.Checked;
            pnlExistingGroup.Visible = tgExistingroup.Checked;
            if ( tgExistingroup.Checked )
            {
                tbGroupName.Text = string.Empty;
            }
            else
            {
                gpExistingGroup.SetValue( null );
            }
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
            if ( definedType != null )
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
            Guid guid = ( Guid ) e.RowKeyValue;
            var audience = DefinedValueCache.Get( guid );
            if ( audience != null )
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
            if ( definedValueId.HasValue )
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
            switch ( hfActiveDialog.Value )
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
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;
            }
        }

        #endregion

        #endregion Control Event Handlers


        /// <summary>
        /// Starts the workflow.
        /// </summary>
        /// <param name="rockContext">The workflow service.</param>
        /// <param name="linkage">Type <see cref="T:EventItemOccurrenceGroupMap" /> created by the wizard.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="workflowNameSuffix">The workflow instance name suffix (the part that is tacked onto the end fo the name to distinguish one instance from another).</param>
        protected void LaunchPostWizardWorkflow( RockContext rockContext, EventItemOccurrenceGroupMap linkage )
        {
            //Set Completion Workflow
            var workFlowGuid = GetAttributeValue( AttributeKey.CompletionWorkflow ).AsGuidOrNull();
            if ( workFlowGuid != null )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflowType = WorkflowTypeCache.Get( workFlowGuid.Value );

                //launch workflow if configured
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    // set workflow name
                    string workflowName = "New " + workflowType.WorkTerm;
                    var workflow = Workflow.Activate( workflowType, workflowName );

                    // set attributes
                    if ( linkage.Group != null )
                    {
                        workflow.SetAttributeValue( "Group", linkage.Group.Guid );
                    }

                    if ( linkage.RegistrationInstance != null )
                    {
                        workflow.SetAttributeValue( "RegistrationInstance", linkage.RegistrationInstance.Guid );
                    }

                    if ( linkage.EventItemOccurrence != null )
                    {
                        workflow.SetAttributeValue( "EventItemOccurrenceGuid", linkage.EventItemOccurrence.Guid );
                    }

                    // launch workflow
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );
                }
            }

        }
    }
}