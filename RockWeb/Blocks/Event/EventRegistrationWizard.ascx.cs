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

namespace RockWeb.Blocks.Event
{
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
        "Set Registration Instance Active",
        Key = AttributeKey.SetRegistrationInstanceActive,
        Description = "If unchecked, the new registration instance will be created, but marked as \"inactive\".",
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


    [WorkflowTypeField( "Completion Workflow", "A workflow that will be launched when a new registration is created.", true, false, "", "", 9, AttributeKey.CompletionWorkflows )]

    //// The attribute below should replace the attribute above in V9 code.
    //[WorkflowTypeField(
    //    "Completion Workflow",
    //    Key = AttributeKey.CompletionWorkflows,
    //    Description = "One or more workflow(s) that will be launched when a new registration is created.",
    //    Category = "",
    //    IsRequired = false,
    //    DefaultValue = "",
    //    AllowMultiple = true,
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
            public const string SetRegistrationInstanceActive = "SetRegistrationInstanceActive";
            public const string EnableCalendarEvents = "EnableCalendarEvents";
            public const string AllowCreatingNewCalendarEvents = "AllowCreatingNewCalendarEvents";
            public const string InstructionsLavaTemplate = "InstructionsLavaTemplate";
            public const string CompletionWorkflows = "CompletionWorkflows";
        }

        #region Private Variables

        #endregion

        #region Properties

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

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

            if ( !Page.IsPostBack )
            {
                ppContact.SetValue( CurrentPerson );
                SetActiveWizardStep( ActiveWizardStep.Initiate );

                using (var rockContext = new RockContext())
                {
                    Init_SetRegistrationTemplateValues( rockContext );
                    Init_SetCampusSelectionOption();
                    Init_SetDefaultAccount( rockContext );
                    Init_SetDefaultCalendar( rockContext );
                    Init_SetRootGroup( rockContext );
                }

                //sbSchedule.iCalendarContent = string.Empty;
                //lScheduleText.Text = string.Empty;

                DisplayDebug_AttributeValues();
            }
        }
        private void Init_SetRegistrationTemplateValues( RockContext rockContext )
        {
            List<Guid> registrationTemplateGuids = new List<Guid>();
            foreach (string selectedRegistrationTemplate in GetAttributeValues( AttributeKey.AvailableRegistrationTemplates ))
            {
                Guid? registrationTemplateGuid = selectedRegistrationTemplate.AsGuidOrNull();
                if (registrationTemplateGuid != null)
                {
                    registrationTemplateGuids.Add( registrationTemplateGuid.Value );
                }
            }

            var registrationTemplates = new RegistrationTemplateService( rockContext ).GetByGuids( registrationTemplateGuids ).ToList();

            ddlTemplate.DataSource = registrationTemplates;
            ddlTemplate.DataBind();
        }
        private void Init_SetCampusSelectionOption()
        {
            if ( !GetAttributeValue ( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                cpCampus.Enabled = false;
                cpCampus.Visible = false;
            }
            else if ( !GetAttributeValue ( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            {
                cpCampus.Enabled = false;
                cpCampus.Visible = false;
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

        private void DisplayDebug_AttributeValues()
        {
            var rockContext = new RockContext();
            lblDebug.Text = "Attribute Values:<br />";

            Guid? acctGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
            if ( acctGuid != null )
            {
                var acctService = new FinancialAccountService( rockContext );
                var acct = acctService.Get( acctGuid.Value );
                lblDebug.Text += "DefaultAccount: " + acct.Name + "<br />";
            }

            Guid? calendarGuid = GetAttributeValue( AttributeKey.DefaultCalendar ).AsGuidOrNull();
            if (calendarGuid != null)
            {
                var calendarService = new EventCalendarService( rockContext );
                var calendar = calendarService.Get( calendarGuid.Value );
                lblDebug.Text += "DefaultCalendar: " + calendar.Name + "<br />";
            }

            foreach (string selectedRegistrationTemplate in GetAttributeValues( AttributeKey.AvailableRegistrationTemplates ) )
            {
                Guid? registrationTemplateGuid = selectedRegistrationTemplate.AsGuidOrNull();
                if ( registrationTemplateGuid != null )
                {
                    var registrationTemplateService = new RegistrationTemplateService( rockContext );
                    var registrationTemplate = registrationTemplateService.Get( registrationTemplateGuid.Value );
                    lblDebug.Text += "AvailableRegistrationTemplate: " + registrationTemplate.Name + "<br />";
                }
            }

            Guid? groupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
            if (groupGuid != null)
            {
                var groupService = new GroupService( rockContext );
                var rootGroup = groupService.Get( groupGuid.Value );
                lblDebug.Text += "RootGroup: " + rootGroup.Name + "<br />";
            }

            lblDebug.Text += "GroupViewerPage: " + LinkedPageUrl( AttributeKey.GroupViewerPage, new Dictionary<string, string>() { { "GroupId", "XYZ" } } ) + "<br />";
            lblDebug.Text += "SetRegistrationInstanceActive: " + GetAttributeValue( AttributeKey.SetRegistrationInstanceActive ) + "<br />";
            lblDebug.Text += "EnableCalendarEvents: " + GetAttributeValue( AttributeKey.EnableCalendarEvents ) + "<br />";
            lblDebug.Text += "AllowCreatingNewCalendarEvents: " + GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ) + "<br />";
            lblDebug.Text += "InstructionsLavaTemplate: " + GetAttributeValue( AttributeKey.InstructionsLavaTemplate ) + "<br />";

            foreach (string selectedCompletionWorkflow in GetAttributeValues( AttributeKey.CompletionWorkflows ))
            {
                Guid? workflowTypeGuid = selectedCompletionWorkflow.AsGuidOrNull();
                if (workflowTypeGuid != null)
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                    lblDebug.Text += "CompletionWorkflow: " + workflowType.Name + "<br />";
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

        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            return base.SaveViewState();
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
            SetupWizardCSSClasses( step );
            ShowInputPanel( step );
            SetupWizardButtons( step );
        }

        /// <summary>
        /// Sets the appropriate CSS classes (active, complete or none) on wizard div elements.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardCSSClasses( ActiveWizardStep step )
        {
            string baseClass = "wizard-item";
            string registrationClass = baseClass;
            string groupClass = baseClass;
            string eventClass = baseClass;
            string eventoccurrenceClass = baseClass;
            string summaryClass = baseClass;

            // Hide wizard items if the block settings don't indicate that they should be used.
            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
            {
                divEvent.Visible = true;
                if ( GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
                {
                    divEventOccurrence.Visible = true;
                }
                else
                {
                    divEventOccurrence.Visible = false;
                }
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
                    pnlWizard.Visible = false;
                    break;
                case ActiveWizardStep.Registration:
                    registrationClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Group:
                    registrationClass = baseClass + "  complete";
                    groupClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Event:
                    registrationClass = baseClass + " complete";
                    groupClass = baseClass + " complete";
                    eventClass = baseClass + " active";
                    break;
                case ActiveWizardStep.EventOccurrence:
                    registrationClass = baseClass + " complete";
                    groupClass = baseClass + " complete";
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Summary:
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
                        lbEvent.Enabled = true;
                    break;
                case ActiveWizardStep.Summary:
                    lbRegistration.Enabled = true;
                    lbGroup.Enabled = true;
                    if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
                        lbEvent.Enabled = true;
                    if ( GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
                        lbEventOccurrence.Enabled = true;
                    break;
                case ActiveWizardStep.Finished: break;

                default: break;

            }
        }

        #endregion

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
            if ( GetAttributeValue( AttributeKey.EnableCalendarEvents ).AsBoolean() )
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
            if ( GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
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
                if ( GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
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
                SetActiveWizardStep( ActiveWizardStep.Group );
            }
        }

        protected void lbNext_Summary_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Finished );
        }

        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var contact = personService.Get( ppContact.SelectedValue.Value );

            tbContactEmail.Text = contact.Email;
            var pn = contact.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
            if ( pn != null )
            {
                tbContactPhone.Text = pn.NumberFormatted;
            }
        }

        protected void ddlTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
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

        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {

        }

        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;
        }
    }
}