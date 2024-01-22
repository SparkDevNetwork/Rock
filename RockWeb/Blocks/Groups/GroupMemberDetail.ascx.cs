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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Member Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group member for editing role, status, etc." )]

    [LinkedPage( "Registration Page",
        Description = "Page used for viewing the registration(s) associated with a particular group member",
        Key = AttributeKey.RegistrationPage,
        IsRequired = false,
        Order = 0 )]

    [BooleanField( "Show \"Move To Another Group\" Button",
        Description = "Set to false to hide the \"Move to another group\" button",
        Key = AttributeKey.ShowMoveToOtherGroup,
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField( "Hide Requirements",
        Description = "When set to 'Yes', the group member's requirements section will be hidden.",
        Key = AttributeKey.AreRequirementsPubliclyHidden,
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField( "Hide Requirement Type Summary",
        Description = "If requirements are being shown, setting this to 'Yes' will hide the requirement type's \"Summary\" value.",
        Key = AttributeKey.IsSummaryHidden,
        DefaultBooleanValue = false,
        Order = 3 )]

    [BooleanField( "Are Requirements Refreshed When Block Is Loaded",
        Description = "Set to true to refresh group member requirements when the block is loaded.",
        Key = AttributeKey.AreRequirementsRefreshedOnLoad,
        DefaultBooleanValue = false,
        Order = 4 )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        Order = 5 )]

    [BooleanField( "Enable Communications",
        Description = "Enables the capability to send quick communications from the block.",
        Key = AttributeKey.EnableCommunications,
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField( "Enable SMS",
        Description = "Allows SMS to be able to be sent from the communications if the individual has SMS enabled. Otherwise only email will be an option.",
        Key = AttributeKey.EnableSMS,
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField( "Append Organization Email Header/Footer",
        Description = "Will append the organization’s email header and footer to the email message.",
        Key = AttributeKey.AppendHeaderFooter,
        DefaultBooleanValue = true,
        Order = 8 )]

    [BooleanField( "Allow Selecting 'From'",
        Description = "Allows the 'from' of the communication to be changed to a different person.",
        Key = AttributeKey.AllowSelectingFrom,
        DefaultBooleanValue = true,
        Order = 9 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Order = 10 )]

    [CustomDropdownListField(
        "Schedule List Format",
        Key = AttributeKey.ScheduleListFormat,
        ListSource = "1^Schedule Time,2^Schedule Name,3^Schedule Time and Name",
        IsRequired = false,
        DefaultValue = "1",
        Order = 11 )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.GROUPS_GROUP_MEMBER_DETAIL )]
    public partial class GroupMemberDetail : RockBlock
    {
        #region Properties

        private List<GroupMemberAssignmentStateObj> GroupMemberAssignmentsState { get; set; }

        private int? LocationId
        {
            get
            {
                return hfLocationId.Value.AsIntegerOrNull();
            }
        }

        private int? ScheduleId
        {
            get
            {
                return hfScheduleId.Value.AsIntegerOrNull();
            }
        }

        private bool IsSignUpMode
        {
            get
            {
                return this.LocationId.ToIntSafe() > 0
                    && this.ScheduleId.ToIntSafe() > 0;
            }
        }

        #endregion

        private static class AttributeKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string ShowMoveToOtherGroup = "ShowMoveToOtherGroup";
            public const string AreRequirementsPubliclyHidden = "AreRequirementsPubliclyHidden";
            public const string IsSummaryHidden = "IsSummaryHidden";
            public const string AreRequirementsRefreshedOnLoad = "AreRequirementsRefreshedOnLoad";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string EnableCommunications = "EnableCommunications";
            public const string EnableSMS = "EnableSMS";
            public const string AppendHeaderFooter = "AppendHeaderFooter";
            public const string AllowSelectingFrom = "AllowSelectingFrom";
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ScheduleListFormat = "ScheduleListFormat";
        }

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string GroupMemberAssignmentsStateJson = "GroupMemberAssignmentsStateJson";
        }

        #endregion ViewStateKeys

        protected const string NO_LOCATION_PREFERENCE = "No Location Preference";

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string LocationId = "LocationId";
            public const string RegistrationId = "RegistrationId";
            public const string ScheduleId = "ScheduleId";
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gmrcRequirements.WorkflowEntryLinkedPageValue = this.GetAttributeValue( AttributeKey.WorkflowEntryPage );
            gmrcRequirements.IsSummaryHidden = this.GetAttributeValue( AttributeKey.IsSummaryHidden ).AsBoolean();

            gGroupPreferenceAssignments.DataKeyNames = new string[] { "Guid" };
            gGroupPreferenceAssignments.Actions.ShowAdd = true;
            gGroupPreferenceAssignments.Actions.AddClick += gGroupPreferenceAssignments_Add;
            gGroupPreferenceAssignments.GridRebind += gGroupPreferenceAssignments_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += GroupMemberDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upDetail );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[ViewStateKey.GroupMemberAssignmentsStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMemberAssignmentsState = new List<GroupMemberAssignmentStateObj>();
            }
            else
            {
                GroupMemberAssignmentsState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<GroupMemberAssignmentStateObj>>( json );
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

            ViewState[ViewStateKey.GroupMemberAssignmentsStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( GroupMemberAssignmentsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the GroupMemberDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void GroupMemberDetail_BlockUpdated( object sender, EventArgs e )
        {
            SetBlockOptions();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            ClearErrorMessage();

            bool areRequirementsPubliclyHidden = this.GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBooleanOrNull() ?? false;
            gmrcRequirements.Visible = !areRequirementsPubliclyHidden;

            if ( !Page.IsPostBack )
            {
                SetBlockOptions();
                ShowDetail
                (
                    PageParameter( PageParameterKey.GroupMemberId ).AsInteger(),
                    PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull(),
                    PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull(),
                    PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull(),
                    PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull()
                );
            }
            else
            {
                SetRequirementStatuses( new RockContext() );
            }
        }

        /// <summary>
        /// Sets the block options.
        /// </summary>
        public void SetBlockOptions()
        {
            bool showMoveToOtherGroup = this.GetAttributeValue( AttributeKey.ShowMoveToOtherGroup ).AsBooleanOrNull() ?? true;
            btnShowMoveDialog.Visible = showMoveToOtherGroup;

            bool enableCommunications = this.GetAttributeValue( AttributeKey.EnableCommunications ).AsBooleanOrNull() ?? true;
            btnShowCommunicationDialog.Visible = PageParameter( PageParameterKey.GroupMemberId ).AsInteger() != 0 && enableCommunications;

            bool areRequirementsPubliclyHidden = this.GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBooleanOrNull() ?? false;
            gmrcRequirements.Visible = !areRequirementsPubliclyHidden;
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupMemberId = PageParameter( pageReference, PageParameterKey.GroupMemberId ).AsIntegerOrNull();
            if ( groupMemberId != null )
            {
                GroupMember groupMember = new GroupMemberService( new RockContext() ).Get( groupMemberId.Value );
                if ( groupMember != null )
                {
                    // This should be replaced with a block setting when converted to Obsidian. -dsh
                    var pageReferenceHistory = ( Dictionary<int, List<BreadCrumb>> ) System.Web.HttpContext.Current.Session["RockPageReferenceHistory"];

                    var queryString = pageReferenceHistory.Values
                        .SelectMany( h => h )
                        .Where( bc => bc.Url.IsNotNullOrWhiteSpace() && bc.Url.StartsWith( "/" ) )
                        .Select( bc => Uri.TryCreate( "http://ignored" + bc.Url, UriKind.Absolute, out var uri ) ? uri : null )
                        .Where( u => u != null && u.Query.IsNotNullOrWhiteSpace() && u.Query != "?" )
                        .Select( u => u.ParseQueryString() )
                        .FirstOrDefault( q => q.AllKeys.Contains( PageParameterKey.GroupId ) );

                    var groupIdParam = queryString?[PageParameterKey.GroupId].AsIntegerOrNull();
                    if ( !groupIdParam.HasValue || groupIdParam.Value != groupMember.GroupId )
                    {
                        // if the GroupMember's Group isn't included in the breadcrumbs, make sure to add the Group to the breadcrumbs so we know which group the group member is in
                        breadCrumbs.Add( new BreadCrumb( groupMember.Group.Name, true ) );
                    }

                    breadCrumbs.Add( new BreadCrumb( groupMember.Person.FullName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group Member", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Populates the group schedule assignment locations.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        private void PopulateGroupScheduleAssignmentLocations( int groupId, int? scheduleId )
        {
            int? selectedLocationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();
            ddlGroupScheduleAssignmentLocation.Items.Clear();
            ddlGroupScheduleAssignmentLocation.Items.Add( new ListItem( NO_LOCATION_PREFERENCE, NO_LOCATION_PREFERENCE ) );
            if ( scheduleId.HasValue )
            {
                var locations = new LocationService( new RockContext() ).GetByGroupSchedule( scheduleId.Value, groupId )
                    .OrderBy( a => a.Name )
                    .Select( a => new
                    {
                        a.Id,
                        a.Name
                    } ).ToList();

                foreach ( var location in locations )
                {
                    var locationListItem = new ListItem( location.Name, location.Id.ToString() );
                    if ( selectedLocationId.HasValue && location.Id == selectedLocationId.Value )
                    {
                        locationListItem.Selected = true;
                    }

                    ddlGroupScheduleAssignmentLocation.Items.Add( locationListItem );
                }
            }
        }

        /// <summary>
        /// Binds the group preference assignments grid.
        /// </summary>
        private void BindGroupPreferenceAssignmentsGrid()
        {
            var assignments = GroupMemberAssignmentsState
                .OrderBy( a => a.ScheduleOrder )
                .ThenBy( a => a.ScheduleNextStartDateTime )
                .ThenBy( a => a.ScheduleName )
                .ThenBy( a => a.ScheduleId )
                .ThenBy( a => a.LocationId.HasValue ? a.LocationName : string.Empty )
                .ToList();
            gGroupPreferenceAssignments.DataSource = assignments;
            gGroupPreferenceAssignments.DataBind();
        }

        /// <summary>
        /// Loads the phone numbers.
        /// </summary>
        /// <returns></returns>
        private bool LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( spn => selectedNumberGuids.Contains( spn.Guid ) ).ToList();
            }

            if ( smsNumbers.Any() )
            {
                var smsDetails = smsNumbers.Select( spn => new
                {
                    spn.Id,
                    Description = spn.Name
                } );

                ddlSmsNumbers.DataSource = smsDetails;
                ddlSmsNumbers.Visible = smsNumbers.Count() > 1;
                ddlSmsNumbers.DataValueField = "Id";
                ddlSmsNumbers.DataTextField = "Description";
                ddlSmsNumbers.DataBind();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        public void ShowDetail( int groupMemberId )
        {
            ShowDetail( groupMemberId, null, null, null, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        public void ShowDetail( int groupMemberId, int? groupId, int? campusId, int? locationId, int? scheduleId )
        {
            // autoexpand the person picker if this is an add
            var personPickerStartupScript = @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });";

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "StartupScript", personPickerStartupScript, true );

            var rockContext = new RockContext();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                pdAuditDetails.SetEntity( groupMember, ResolveRockUrl( "~" ) );
            }
            else
            {
                // only create a new one if parent was specified
                if ( groupId.HasValue )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = groupId.Value;
                    groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.DateTimeAdded = RockDateTime.Now;

                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
                }
            }

            if ( groupMember == null )
            {
                if ( groupMemberId > 0 )
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                    nbErrorMessage.Title = "Warning";
                    nbErrorMessage.Text = "Group Member not found. Group Member may have been moved to another group or deleted.";
                }
                else
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbErrorMessage.Title = "Invalid Request";
                    nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid GroupMemberId or GroupId parameter is required.";
                }

                pnlEditDetails.Visible = false;
                return;
            }

            pnlEditDetails.Visible = true;

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            if ( campusId.HasValue )
            {
                hfCampusId.Value = campusId.Value.ToString();
            }

            if ( locationId.HasValue )
            {
                hfLocationId.Value = locationId.Value.ToString();
            }

            if ( scheduleId.HasValue )
            {
                hfScheduleId.Value = scheduleId.Value.ToString();
            }

            if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                cbIsNotified.Checked = groupMember.IsNotified;
                cbIsNotified.Visible = true;
            }
            else
            {
                cbIsNotified.Visible = false;
            }

            var group = groupMember.Group;
            var groupType = GroupTypeCache.Get( groupMember.Group.GroupTypeId );
            if ( !string.IsNullOrWhiteSpace( groupType.IconCssClass ) )
            {
                lGroupIconHtml.Text = string.Format( "<i class='{0}' ></i>", groupType.IconCssClass );
            }
            else
            {
                lGroupIconHtml.Text = "<i class='fa fa-user' ></i>";
            }

            if ( groupMember.Id.Equals( 0 ) )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( groupType.GroupTerm + " " + groupType.GroupMemberTerm ).FormatAsHtmlTitle();
                btnSaveThenAdd.Visible = true;
            }
            else
            {
                lReadOnlyTitle.Text = groupMember.Person.FullName.FormatAsHtmlTitle();
                btnSaveThenAdd.Visible = false;
            }

            if ( groupMember.DateTimeAdded.HasValue )
            {
                hlDateAdded.Text = string.Format( "Added: {0}", groupMember.DateTimeAdded.Value.ToShortDateString() );
                hlDateAdded.Visible = true;
            }
            else
            {
                hlDateAdded.Text = string.Empty;
                hlDateAdded.Visible = false;
            }

            hlArchived.Visible = groupMember.IsArchived;

            bool readOnly = true;
            nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );

            if ( IsUserAuthorized( Authorization.EDIT )
                || group.IsAuthorized( Authorization.EDIT, this.CurrentPerson )
                || group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson )
                || ( IsSignUpMode && group.IsAuthorized( Authorization.SCHEDULE, this.CurrentPerson ) ) )
            {
                readOnly = false;
                nbEditModeMessage.Text = string.Empty;
            }

            if ( groupMember.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            btnSave.Visible = !readOnly;
            btnSaveThenAdd.Visible = !readOnly;

            if ( readOnly || groupMember.Id == 0 )
            {
                // hide the ShowMoveDialog if this is readOnly or if this is a new group member (can't move a group member that doesn't exist yet)
                btnShowMoveDialog.Visible = false;
            }

            var currentSyncdRoles = new GroupSyncService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( s => s.GroupId == groupMember.GroupId )
                    .Select( s => s.GroupTypeRoleId )
                    .ToList();

            LoadDropDowns( currentSyncdRoles, groupMember.GroupRoleId );

            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            ddlGroupRole.Enabled = ddlGroupRole.Enabled == true ? !readOnly : false;

            ShowRequiredDocumentStatus( rockContext, groupMember, group );

            ppGroupMemberPerson.SetValue( groupMember.Person );
            ppGroupMemberPerson.Enabled = !readOnly;

            if ( groupMember.Id != 0 )
            {
                // once a group member record is saved, don't let them change the person
                ppGroupMemberPerson.Enabled = false;
            }

            tbNote.Text = groupMember.Note;
            tbNote.ReadOnly = readOnly;

            rblStatus.SetValue( ( int ) groupMember.GroupMemberStatus );
            rblStatus.Enabled = !readOnly;
            rblStatus.Label = string.Format( "{0} Status", group.GroupType.GroupMemberTerm );

            rblCommunicationPreference.SetValue( ( ( int ) groupMember.CommunicationPreference ).ToString() );
            rblCommunicationPreference.Enabled = !readOnly;

            var registrations = new RegistrationRegistrantService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    r.Registration != null &&
                    r.Registration.RegistrationInstance != null &&
                    r.GroupMemberId.HasValue &&
                    r.GroupMemberId.Value == groupMember.Id )
                .Select( r => new
                {
                    Id = r.Registration.Id,
                    Name = r.Registration.RegistrationInstance.Name
                } )
                .ToList();
            if ( registrations.Any() )
            {
                rcwLinkedRegistrations.Visible = true;
                rptLinkedRegistrations.DataSource = registrations;
                rptLinkedRegistrations.DataBind();
            }
            else
            {
                rcwLinkedRegistrations.Visible = false;
            }

            if ( groupMember.Group.RequiredSignatureDocumentTemplate != null )
            {
                fuSignedDocument.Label = groupMember.Group.RequiredSignatureDocumentTemplate.Name;
                if ( groupMember.Group.RequiredSignatureDocumentTemplate.BinaryFileType != null )
                {
                    fuSignedDocument.BinaryFileTypeGuid = groupMember.Group.RequiredSignatureDocumentTemplate.BinaryFileType.Guid;
                }

                var signatureDocument = new SignatureDocumentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == groupMember.Group.RequiredSignatureDocumentTemplateId.Value &&
                        d.AppliesToPersonAlias != null &&
                        d.AppliesToPersonAlias.PersonId == groupMember.PersonId &&
                        d.LastStatusDate.HasValue &&
                        d.Status == SignatureDocumentStatus.Signed &&
                        d.BinaryFile != null )
                    .OrderByDescending( d => d.LastStatusDate.Value )
                    .FirstOrDefault();

                if ( signatureDocument != null )
                {
                    hfSignedDocumentId.Value = signatureDocument.Id.ToString();
                    fuSignedDocument.BinaryFileId = signatureDocument.BinaryFileId;
                }

                fuSignedDocument.Visible = true;
            }
            else
            {
                fuSignedDocument.Visible = false;
            }

            /*
             * 2/21/2023 - JPH
             * If Location and Schedule IDs were provided in the query string, this block is being used in sign-up mode,
             * meaning scheduling is managed differently; don't display scheduling controls.
             * 
             * Reason: Sign-Up Feature
             */
            pnlScheduling.Visible = groupType.IsSchedulingEnabled && !this.IsSignUpMode;
            ddlGroupMemberScheduleTemplate.SetValue( groupMember.ScheduleTemplateId );
            ddlGroupMemberScheduleTemplate_SelectedIndexChanged( null, null );

            dpScheduleStartDate.SelectedDate = groupMember.ScheduleStartDate;
            nbScheduleReminderEmailOffsetDays.Text = groupMember.ScheduleReminderEmailOffsetDays.ToString();

            // Show the Group Member Attributes.
            groupMember.LoadAttributes();
            avcAttributes.Visible = false;
            avcAttributesReadOnly.Visible = false;

            List<string> editableAttributes;
            List<string> viewableAttributes;

            if ( group.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
            {
                // If the Current User has Administrate permissions for the Group, show all Attributes.
                editableAttributes = readOnly ? new List<string>()
                    : groupMember.Attributes.Select( a => a.Key ).ToList();
                viewableAttributes = groupMember.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
            }
            else
            {
                editableAttributes = readOnly ? new List<string>()
                    : groupMember.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
                viewableAttributes = groupMember.Attributes.Where( a => !editableAttributes.Contains( a.Key ) && a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            }

            if ( editableAttributes.Any() )
            {
                avcAttributes.Visible = true;
                avcAttributes.ExcludedAttributes = groupMember.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Value ).ToArray();
                avcAttributes.AddEditControls( groupMember );
            }

            if ( viewableAttributes.Any() )
            {
                avcAttributesReadOnly.Visible = true;
                avcAttributesReadOnly.ExcludedAttributes = groupMember.Attributes.Where( a => !viewableAttributes.Contains( a.Key ) ).Select( a => a.Value ).ToArray();
                avcAttributesReadOnly.AddDisplayControls( groupMember );
            }

            var groupHasRequirements = group.GetGroupRequirements( rockContext ).Any();
            pnlRequirements.Visible = groupHasRequirements;
            btnRefreshRequirements.Visible = groupHasRequirements;
            SetRequirementStatuses( rockContext );

            bool areRequirementsRefreshedOnLoad = this.GetAttributeValue( AttributeKey.AreRequirementsRefreshedOnLoad ).AsBooleanOrNull() ?? false;

            if ( groupType.IsSchedulingEnabled )
            {
                GroupMemberAssignmentsState = new List<GroupMemberAssignmentStateObj>();
                if ( groupMember.Id != default( int ) )
                {
                    // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order
                    var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );
                    var groupLocationService = new GroupLocationService( rockContext );
                    var qryGroupLocations = groupLocationService
                        .Queryable()
                        .Where( g => g.GroupId == group.Id );

                    var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                    var groupMemberAssignmentQuery = groupMemberAssignmentService
                        .Queryable()
                        .AsNoTracking()
                        .Where( x =>
                            x.GroupMemberId == groupMemberId
                            && (
                                !x.LocationId.HasValue
                                || qryGroupLocations.Any( gl => gl.LocationId == x.LocationId && gl.Schedules.Any( s => s.Id == x.ScheduleId ) )
                            ) );

                    GroupMemberAssignmentsState = groupMemberAssignmentQuery
                            .Include( a => a.Schedule )
                            .Include( a => a.Location )
                            .AsNoTracking()
                            .ToList()
                            .Select( a => new GroupMemberAssignmentStateObj()
                            {
                                Guid = a.Guid,
                                Id = a.Id,
                                LocationId = a.LocationId,
                                ScheduleId = a.ScheduleId.Value,
                                LocationName = a.LocationId.HasValue ? a.Location.ToString( true ) : NO_LOCATION_PREFERENCE,
                                ScheduleName = a.Schedule.Name,
                                FormattedScheduleName = GetFormattedScheduleForListing( a.Schedule.Name, a.Schedule.StartTimeOfDay ),
                                ScheduleOrder = a.Schedule.Order,
                                ScheduleNextStartDateTime = a.Schedule.GetNextStartDateTime( occurrenceDate )
                            } )
                            .ToList();
                }

                BindGroupPreferenceAssignmentsGrid();
            }

            if ( areRequirementsRefreshedOnLoad )
            {
                CalculateRequirements( true );
            }
            else
            {
                ShowGroupRequirementsStatuses( false );
            }
        }

        /// <summary>
        /// Shows the required document status.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        /// <param name="groupMember">The <see cref="GroupMember"/>.</param>
        /// <param name="group">The <see cref="Group"/>.</param>
        private void ShowRequiredDocumentStatus( RockContext rockContext, GroupMember groupMember, Group group )
        {
            if ( groupMember.Person != null && group.RequiredSignatureDocumentTemplate != null )
            {
                var documents = new SignatureDocumentService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == group.RequiredSignatureDocumentTemplate.Id &&
                        d.AppliesToPersonAlias.PersonId == groupMember.Person.Id )
                    .ToList();
                if ( !documents.Any( d => d.Status == SignatureDocumentStatus.Signed ) )
                {
                    var lastSent = documents.Any( d => d.Status == SignatureDocumentStatus.Sent ) ?
                        documents.Where( d => d.Status == SignatureDocumentStatus.Sent ).Max( d => d.LastInviteDate ) : ( DateTime? ) null;
                    pnlRequiredSignatureDocument.Visible = true;

                    if ( lastSent.HasValue )
                    {
                        lbResendDocumentRequest.Text = "Resend Signature Request";
                        lRequiredSignatureDocumentMessage.Text = string.Format( "A signed {0} document has not yet been received for {1}. The last request was sent {2}.", group.RequiredSignatureDocumentTemplate.Name, groupMember.Person.NickName, lastSent.Value.ToElapsedString() );
                    }
                    else
                    {
                        lbResendDocumentRequest.Text = "Send Signature Request";
                        lRequiredSignatureDocumentMessage.Text = string.Format( "The required {0} document has not yet been sent to {1} for signing.", group.RequiredSignatureDocumentTemplate.Name, groupMember.Person.NickName );
                    }
                }
                else
                {
                    pnlRequiredSignatureDocument.Visible = false;
                }
            }
            else
            {
                pnlRequiredSignatureDocument.Visible = false;
            }
        }

        /// <summary>
        /// Shows the group requirements statuses.
        /// </summary>
        private void ShowGroupRequirementsStatuses( bool forceRefreshRequirements )
        {
            if ( !pnlRequirements.Visible )
            {
                // group doesn't have any requirements
                return;
            }

            var rockContext = new RockContext();
            int groupMemberId = hfGroupMemberId.Value.AsInteger();
            var groupId = hfGroupId.Value.AsInteger();
            GroupMember groupMember = null;

            if ( !groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
            }
            else
            {
                // only create a new one if person is selected
                if ( ppGroupMemberPerson.PersonId.HasValue )
                {
                    groupMember = new GroupMember { Id = 0 };
                    groupMember.GroupId = groupId;
                    groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                    groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
                }
            }

            if ( groupMember == null )
            {
                // no person selected yet, so don't show anything
                gmrcRequirements.Visible = false;
                return;
            }

            var selectedGroupRoleId = ddlGroupRole.SelectedValue.AsInteger();
            if ( groupMember != null && selectedGroupRoleId != groupMember.GroupRoleId )
            {
                groupMember.GroupRoleId = selectedGroupRoleId;
            }

            bool areRequirementsPubliclyHidden = this.GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBooleanOrNull() ?? false;
            gmrcRequirements.Visible = !areRequirementsPubliclyHidden;

            // Force refreshing the requirements when loading the container.
            if ( forceRefreshRequirements || groupMember.IsNewOrChangedGroupMember( rockContext ) )
            {
                SetRequirementStatuses( rockContext );
            }

            //gmrcRequirements.DataBind();

            var requirementsWithErrors = gmrcRequirements.RequirementStatuses?.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error ).ToList();
            if ( requirementsWithErrors != null && requirementsWithErrors.Any() )
            {
                nbRequirementsErrors.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbRequirementsErrors.Visible = true;
                nbRequirementsErrors.Text = string.Format(
                    "An error occurred in one or more of the requirement calculations" );

                nbRequirementsErrors.Details = requirementsWithErrors.Select( a => string.Format( "{0}: {1}", a.GroupRequirement.GroupRequirementType.Name, a.CalculationException.Message ) ).ToList().AsDelimited( Environment.NewLine );
            }
            else
            {
                nbRequirementsErrors.Visible = false;
            }
        }

        /// <summary>
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        /// <param name="syncdRoles">The sync'd roles.</param>
        private void LoadDropDowns( List<int> syncdRoles, int groupMemberRole )
        {
            int groupId = hfGroupId.ValueAsInt();
            RockContext rockContext = new RockContext();
            int groupTypeId = new GroupService( rockContext ).GetSelect( groupId, a => a.GroupTypeId );

            IQueryable<GroupTypeRole> groupTypeRoles = new GroupTypeRoleService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r => r.GroupTypeId == groupTypeId );

            if ( syncdRoles.Any() )
            {
                // At least one role is sync'd so we need to handle them.
                if ( syncdRoles.Contains( groupMemberRole ) && hfGroupMemberId.ValueAsInt() != 0 )
                {
                    // This role is being sync'd so keep the full list of roles, disable the ddl, and show a tool tip explaining why it's disabled.
                    ddlGroupRole.ToolTip = "Role selection disabled because this member was added to this role automatically by Group Sync.";
                    ddlGroupRole.Enabled = false;
                }
                else
                {
                    // This role is not being sync'd but the group has sync'd roles. So remove the sync'd roles and display a tool tip explaining their absence.
                    groupTypeRoles = groupTypeRoles.Where( r => !syncdRoles.Contains( r.Id ) );

                    ddlGroupRole.ToolTip = "Roles used for Group Sync cannot be used for manual additions and so are not being displayed.";
                }
            }

            ddlGroupRole.DataSource = groupTypeRoles.OrderBy( a => a.Order ).ToList();
            ddlGroupRole.DataBind();

            rblStatus.BindToEnum<GroupMemberStatus>();

            ddlGroupMemberScheduleTemplate.Items.Clear();
            ddlGroupMemberScheduleTemplate.Items.Add( new ListItem() );

            var groupMemberScheduleTemplateList = new GroupMemberScheduleTemplateService( rockContext ).Queryable()
                .Where( a => !a.GroupTypeId.HasValue || a.GroupTypeId == groupTypeId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } );

            foreach ( var groupMemberScheduleTemplate in groupMemberScheduleTemplateList )
            {
                ddlGroupMemberScheduleTemplate.Items.Add( new ListItem( groupMemberScheduleTemplate.Name, groupMemberScheduleTemplate.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Registrations the URL.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        protected string RegistrationUrl( int registrationId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( PageParameterKey.RegistrationId, registrationId.ToString() );
            return LinkedPageUrl( AttributeKey.RegistrationPage, qryParams );
        }

        /// <summary>
        /// Calculates (or re-calculates) the requirements, then updates the results on the UI
        /// </summary>
        private void CalculateRequirements( bool forceRecheckRequirements )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );

            if ( groupMember != null && !groupMember.IsNewOrChangedGroupMember( rockContext ) )
            {
                groupMember.CalculateRequirements( rockContext, true );
            }

            ShowGroupRequirementsStatuses( forceRecheckRequirements );
        }

        private void SetRequirementStatuses( RockContext rockContext )
        {
            var groupService = new GroupService( rockContext );
            var group = groupService.GetInclude( hfGroupId.ValueAsInt(), g => g.Members );
            var groupMemberId = hfGroupMemberId.ValueAsInt();
            gmrcRequirements.SelectedGroupRoleId = ddlGroupRole.SelectedValue.AsIntegerOrNull();
            gmrcRequirements.RequirementStatuses = group.PersonMeetsGroupRequirements( rockContext, ppGroupMemberPerson.PersonId ?? 0, ddlGroupRole.SelectedValue.AsIntegerOrNull() );

            // Determine whether the current person is a leader of the chosen group.
            var groupMemberQuery = new GroupMemberService( rockContext ).GetByGroupId( hfGroupId.ValueAsInt() );
            var currentPersonIsLeaderOfCurrentGroup = this.CurrentPerson != null ?
                groupMemberQuery.Where( m => m.GroupRole.IsLeader ).Select( m => m.PersonId ).Contains( this.CurrentPerson.Id ) : false;

            gmrcRequirements.CreateRequirementStatusControls( groupMemberId, currentPersonIsLeaderOfCurrentGroup, IsCardInteractionDisabled( rockContext, groupMemberId, group.Id ) );
        }

        private bool IsCardInteractionDisabled( RockContext rockContext, int groupMemberId, int groupId )
        {
            if ( groupMemberId.Equals( 0 ) )
            {
                return true;
            }
            else
            {
                var groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                return ( groupMember.IsNewOrChangedGroupMember( rockContext ) );
            }
        }

        /// <summary>
        /// Navigates to parent page.
        /// </summary>
        private void NavigateToParentPage()
        {
            var qryString = new Dictionary<string, string>();

            /*
             * 1/15/2020 - JPH
             * Since we have established a relationship between Campuses and Groups (by way of the Campus.TeamGroup property),
             * it is now necessary to determine if we need to add the "CampusId" query string parameter in addition to the
             * "GroupId" parameter that we have always sent back to the parent Page here.
             *
             * Reason: Campus Team Feature
             */
            if ( hfCampusId.Value.AsIntegerOrNull().HasValue )
            {
                qryString[PageParameterKey.CampusId] = hfCampusId.Value;
            }

            /*
             * 2/21/2023 - JPH
             * If Location and Schedule IDs were provided in the query string, this block is being used in sign-up mode;
             * send the IDs back to the parent page.
             * 
             * Reason: Sign-Up Feature
             */
            if ( this.IsSignUpMode )
            {
                qryString[PageParameterKey.LocationId] = hfLocationId.Value;
                qryString[PageParameterKey.ScheduleId] = hfScheduleId.Value;
            }

            qryString[PageParameterKey.GroupId] = hfGroupId.Value;

            NavigateToParentPage( qryString );
        }

        #region Fundraising Transaction Transfers

        /// <summary>
        /// The constant value used for naming new fundraising transfer transactions.
        /// </summary>
        private const string _FundRaisingBatchName = "Fundraising Transaction Transfer";

        /// <summary>
        /// The constant "Note" field of batches used for fundraising transfer transactions.
        /// </summary>
        private const string _FundRaisingBatchNote = "Fundraising Transfer";

        /// <summary>
        /// Locates or creates an open Fundraising Transfer batch.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        /// <returns>An open <see cref="FinancialBatch"/> that can be used for fundraising transfer transactions.</returns>
        private FinancialBatch GetFundraisingTransferBatch( RockContext rockContext )
        {
            var batchService = new FinancialBatchService( rockContext );
            var availableBatch = batchService.Queryable()
                .Where( b => b.Status == BatchStatus.Open )
                .Where( b => b.Note.ToLower() == _FundRaisingBatchNote.ToLower() )
                .FirstOrDefault();

            // If an open batch already exists, use that.
            if ( availableBatch != null )
            {
                return availableBatch;
            }

            // No open batch, so make a new one.
            var newBatch = new FinancialBatch()
            {
                Name = _FundRaisingBatchName,
                Note = _FundRaisingBatchNote,
                Status = BatchStatus.Open,
                ControlAmount = 0,
                BatchStartDateTime = RockDateTime.Now,
                Guid = Guid.NewGuid()
            };

            batchService.Add( newBatch );
            rockContext.SaveChanges();

            return newBatch;
        }

        /// <summary>
        /// Validates that fundraising transactions can be moved from one group/groupmember to another.
        /// </summary>
        /// <param name="oldGroupMember">The original/existing <see cref="GroupMember"/>.</param>
        /// <param name="newGroupMember">The new <see cref="GroupMember"/>.</param>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        /// <returns>True if the transactions can be moved.</returns>
        private bool CanMoveFundraisingTransactions( Group newGroup, RockContext rockContext )
        {
            newGroup.LoadAttributes( rockContext );
            var accountGuid = newGroup.GetAttributeValue( "FinancialAccount" ).AsGuidOrNull();
            if ( accountGuid == null )
            {
                return false;
            }

            var financialAccount = new FinancialAccountService( rockContext ).Get( accountGuid.Value );
            if ( financialAccount == null )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Moves fundraising transactions from one group/groupmember to another.  This method should be wrapped in a transaction
        /// along with the creation/deletion of the new/old <see cref="GroupMember"/> records.
        /// </summary>
        /// <param name="oldGroupMember">The original/existing <see cref="GroupMember"/>.</param>
        /// <param name="newGroupMember">The new <see cref="GroupMember"/>.</param>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        private void MoveFundraisingTransactions( GroupMember oldGroupMember, GroupMember newGroupMember, RockContext rockContext )
        {
            int groupMemberTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP_MEMBER ).Id;
            var oldGroup = oldGroupMember.Group;
            var newGroup = newGroupMember.Group;

            newGroup.LoadAttributes( rockContext );
            var newAccountGuid = newGroup.GetAttributeValue( "FinancialAccount" ).AsGuid();
            var newFinancialAccount = new FinancialAccountService( rockContext ).Get( newAccountGuid );

            var transactionService = new FinancialTransactionService( rockContext );
            var oldTransactions = transactionService.Queryable()
                .Where( t => t.TransactionDetails
                    .Where( d => d.EntityId == oldGroupMember.Id )
                    .Where( d => d.EntityTypeId == groupMemberTypeId )
                    .Any() )
                .ToList();

            foreach ( var oldTransaction in oldTransactions )
            {
                bool transactionObjectMoved = false;
                FinancialTransaction creditTransaction = null;
                FinancialTransaction newTransaction = null;
                var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

                foreach ( var oldTransDetail in oldTransaction.TransactionDetails )
                {
                    if ( oldTransDetail.AccountId == newFinancialAccount.Id )
                    {
                        // Accounts are the same, so there is no need to adjust batches.  Just change the EntityId and move on.
                        oldTransDetail.EntityId = newGroupMember.Id;
                        rockContext.SaveChanges();
                        continue;
                    }

                    if ( oldTransaction.Batch.Status == BatchStatus.Open )
                    {
                        // Batch is open, so we can just change the account on the TransactionDetail (and the EntityId) and move on.
                        oldTransDetail.AccountId = newFinancialAccount.Id;
                        oldTransDetail.EntityId = newGroupMember.Id;
                        rockContext.SaveChanges();
                        continue;
                    }

                    // Batch is not open, so we need to make new transactions.
                    if ( !transactionObjectMoved )
                    {
                        // Start by finding or creating a batch.
                        var transferBatch = GetFundraisingTransferBatch( rockContext );

                        // Create new credit transaction (to cancel-out the original transaction).
                        creditTransaction = new FinancialTransaction();
                        creditTransaction.CopyPropertiesFrom( oldTransaction );
                        creditTransaction.Id = 0; // Reset Id to 0 as this is a new record.
                        creditTransaction.Guid = Guid.NewGuid();
                        creditTransaction.BatchId = transferBatch.Id;
                        creditTransaction.Summary = string.Format(
                            "Reversal created for transaction {0} to move Fundraising Donations from group {1} to {2}.{3}{4}",
                            oldTransaction.Id,
                            oldGroup.Id,
                            newGroup.Id,
                            Environment.NewLine,
                            creditTransaction.Summary );

                        creditTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                        creditTransaction.FinancialPaymentDetail.CopyPropertiesFrom( oldTransaction.FinancialPaymentDetail );
                        creditTransaction.FinancialPaymentDetail.Id = 0; // Reset Id to 0 as this is a new record.
                        creditTransaction.FinancialPaymentDetail.Guid = Guid.NewGuid();
                        transactionService.Add( creditTransaction );

                        // Create new transaction (to replace the original transaction).
                        newTransaction = new FinancialTransaction();
                        newTransaction.CopyPropertiesFrom( oldTransaction );
                        newTransaction.Id = 0; // Reset Id to 0 as this is a new record.
                        newTransaction.Guid = Guid.NewGuid();
                        newTransaction.BatchId = transferBatch.Id;
                        newTransaction.Summary = string.Format(
                            "New transaction to replace {0} (moved Fundraising Donations from group {1} to {2}).{3}{4}",
                            oldTransaction.Id,
                            oldGroup.Id,
                            newGroup.Id,
                            Environment.NewLine,
                            newTransaction.Summary );

                        newTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                        newTransaction.FinancialPaymentDetail.CopyPropertiesFrom( oldTransaction.FinancialPaymentDetail );
                        newTransaction.FinancialPaymentDetail.Id = 0; // Reset Id to 0 as this is a new record.
                        newTransaction.FinancialPaymentDetail.Guid = Guid.NewGuid();
                        transactionService.Add( newTransaction );

                        rockContext.SaveChanges();

                        // Only do this once per transaction.  If there is more than one record in the TransactionDetails
                        // collection, we'll use the same FinancialTransaction objects.
                        transactionObjectMoved = true;
                    }

                    if ( creditTransaction == null || newTransaction == null )
                    {
                        // This should not ever occur, as the transactions should have been created in the block above, but just in case
                        // something went wrong, we'll throw a more meaningful error here.
                        throw new Exception( "New distribution transactions were not created." );
                    }

                    // Make the new transaction details.
                    var creditTransDetail = new FinancialTransactionDetail();
                    creditTransDetail.CopyPropertiesFrom( oldTransDetail );
                    creditTransDetail.Id = 0; // Reset Id to 0 as this is a new record.
                    creditTransDetail.Guid = Guid.NewGuid();
                    creditTransDetail.Amount = oldTransDetail.Amount * -1; // Set amount to negative to cancel-out the original transaction.
                    creditTransDetail.TransactionId = creditTransaction.Id; // Assign this detail record to the credit transaction.
                    creditTransDetail.Summary = string.Format(
                        "Credit for FinancialTransactionDetail {0}.{1}{2}",
                        oldTransDetail.Id,
                        Environment.NewLine,
                        creditTransDetail.Summary );
                    financialTransactionDetailService.Add( creditTransDetail );

                    var newTransDetail = new FinancialTransactionDetail();
                    newTransDetail.CopyPropertiesFrom( oldTransDetail );
                    newTransDetail.Id = 0; // Reset Id to 0 as this is a new record.
                    newTransDetail.Guid = Guid.NewGuid();
                    newTransDetail.AccountId = newFinancialAccount.Id; // Set new AccountID.
                    newTransDetail.EntityId = newGroupMember.Id; // Set new EntityId.
                    newTransDetail.TransactionId = newTransaction.Id; // Assign this detail record to the new transaction.
                    newTransDetail.Summary = string.Format(
                        "Replacement for FinancialTransactionDetail {0}.{1}{2}",
                        oldTransDetail.Id,
                        Environment.NewLine,
                        newTransDetail.Summary );
                    financialTransactionDetailService.Add( newTransDetail );

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Sets the visibility and default value of the cbMoveGroupMemberFundraisingTransactions control.
        /// </summary>
        /// <param name="groupTypeId">The Id of the <see cref="GroupType"/> of the <see cref="Group"/> that the <see cref="GroupMember"/> belongs to.</param>
        /// <param name="rockContext">The <see cref="RockContext"/>.</param>
        private void SetFundraisingTransferOptionVisibility( int groupTypeId, RockContext rockContext )
        {
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext )
                .Queryable()
                .Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising )
                .Where( a => a.Id == groupTypeId )
                .Select( a => a.Id ).ToList();

            bool showFundraisingTransferOption = fundraisingGroupTypeIdList.Any();
            cbMoveGroupMemberFundraisingTransactions.Visible = showFundraisingTransferOption;
            cbMoveGroupMemberFundraisingTransactions.Checked = showFundraisingTransferOption;
        }

        #endregion Fundraising Transaction Transfers

        #endregion Internal Methods

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnRestoreArchivedGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRestoreArchivedGroupMember_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            int restoreGroupMemberId = hfRestoreGroupMemberId.Value.AsInteger();
            var groupMemberToRestore = groupMemberService.GetArchived().Where( a => a.Id == restoreGroupMemberId ).FirstOrDefault();
            if ( groupMemberToRestore != null )
            {
                groupMemberService.Restore( groupMemberToRestore );

                // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
                // So, make sure a message is displayed in the validation summary
                var isValid = groupMemberToRestore.IsValidGroupMember( rockContext );
                if ( !isValid )
                {
                    nbRestoreError.Text = groupMemberToRestore.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    nbRestoreError.Visible = true;
                    return;
                }

                rockContext.SaveChanges();
                NavigateToCurrentPageReference( new Dictionary<string, string> { { PageParameterKey.GroupMemberId, restoreGroupMemberId.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDontRestoreArchiveGroupmember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDontRestoreArchiveGroupmember_Click( object sender, EventArgs e )
        {
            // if they said Don't Restore, save the group member without prompting to restore
            if ( SaveGroupMember( false ) )
            {
                if ( cvGroupMember.IsValid )
                {
                    NavigateToParentPage();
                }
            }

            if ( !cvGroupMember.IsValid )
            {
                nbRestoreError.Text = cvGroupMember.ErrorMessage;
                nbRestoreError.Visible = true;
                return;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelRestore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelRestore_Click( object sender, EventArgs e )
        {
            mdRestoreArchivedPrompt.Hide();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( SaveGroupMember( true ) )
            {
                if ( cvGroupMember.IsValid )
                {
                    NavigateToParentPage();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAndAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveThenAdd_Click( object sender, EventArgs e )
        {
            if ( SaveGroupMember( true ) )
            {
                if ( cvGroupMember.IsValid )
                {
                    ShowDetail
                    (
                        0,
                        hfGroupId.Value.AsIntegerOrNull(),
                        hfCampusId.Value.AsIntegerOrNull(),
                        hfLocationId.Value.AsIntegerOrNull(),
                        hfScheduleId.Value.AsIntegerOrNull()
                    );
                }
            }
        }

        /// <summary>
        /// Saves the group member.
        /// </summary>
        /// <param name="checkForArchivedGroupMember">if set to <c>true</c> check to see if there already is a matching archived group member record</param>
        /// <returns></returns>
        private bool SaveGroupMember( bool checkForArchivedGroupMember )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                // Verify valid group
                var groupService = new GroupService( rockContext );
                GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var group = groupService.Get( hfGroupId.ValueAsInt() );
                if ( group == null )
                {
                    nbErrorMessage.Title = "Please select a Role";
                    return false;
                }

                // Check to see if a person was selected
                int? personId = ppGroupMemberPerson.PersonId;
                int? personAliasId = ppGroupMemberPerson.PersonAliasId;
                if ( !personId.HasValue || !personAliasId.HasValue )
                {
                    nbErrorMessage.Title = "Please select a Person";
                    return false;
                }

                // check to see if the user selected a role
                var role = new GroupTypeRoleService( rockContext ).Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );
                if ( role == null )
                {
                    nbErrorMessage.Title = "Please select a Role";
                    return false;
                }

                var groupMemberService = new GroupMemberService( rockContext );
                var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                GroupMember groupMember = null;

                int groupMemberId = int.Parse( hfGroupMemberId.Value );

                // if adding a new group member 
                if ( groupMemberId.Equals( 0 ) )
                {
                    if ( this.IsSignUpMode )
                    {
                        /*
                         * 2/21/2023 - JPH
                         * Only create a new GroupMember record if one doesn't already exist for this project (Group) & Person combination.
                         * It's possible they've already signed up for another occurrence (GroupLocationSchedule) within this same project.
                         * 
                         * Reason: Sign-Up Feature
                         */
                        groupMember = groupMemberService
                            .Queryable()
                            .Include( gm => gm.Person )
                            .Where( gm =>
                                gm.GroupId == group.Id
                                && gm.PersonId == personId.Value
                            )
                            .FirstOrDefault();
                    }

                    if ( groupMember == null )
                    {
                        groupMember = new GroupMember
                        {
                            Id = 0,
                            GroupId = group.Id
                        };
                    }
                }
                else
                {
                    // load existing group member
                    groupMember = groupMemberService.Get( groupMemberId );
                }

                if ( checkForArchivedGroupMember )
                {
                    // if the person or role hasn't change, then don't want to check for archived group member
                    if ( groupMember.PersonId == personId.Value && groupMember.GroupRoleId == role.Id )
                    {
                        checkForArchivedGroupMember = false;
                    }
                }

                groupMember.PersonId = personId.Value;
                groupMember.GroupRoleId = role.Id;
                groupMember.Note = tbNote.Text;
                groupMember.GroupMemberStatus = rblStatus.SelectedValueAsEnum<GroupMemberStatus>();
                groupMember.CommunicationPreference = rblCommunicationPreference.SelectedValueAsEnum<CommunicationType>();

                if ( cbIsNotified.Visible )
                {
                    groupMember.IsNotified = cbIsNotified.Checked;
                }

                // check for matching archived group member with same person and role if this is a new group member or if the person and/or role has changed
                if ( checkForArchivedGroupMember )
                {
                    // check if this is a duplicate member before checking for archived so that validation logic works a little smoother
                    if ( !GroupService.AllowsDuplicateMembers() )
                    {
                        GroupMember duplicateGroupMember;
                        if ( groupService.ExistsAsMember( group, personId.Value, role.Id, out duplicateGroupMember ) )
                        {
                            // duplicate exists, so let normal validation catch it instead of checking for archived group member
                            checkForArchivedGroupMember = false;
                        }
                    }
                }

                if ( checkForArchivedGroupMember )
                {
                    GroupMember archivedGroupMember;
                    if ( groupService.ExistsAsArchived( group, personId.Value, role.Id, out archivedGroupMember ) )
                    {
                        archivedGroupMember.GroupMemberStatus = groupMember.GroupMemberStatus;

                        // if the archived groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
                        // So, make sure a message is displayed in the validation summary

                        // set the IsArchived fields to false to see if the person would valid if they choose to restore/add this member
                        groupMemberService.Restore( archivedGroupMember );
                        cvGroupMember.IsValid = archivedGroupMember.IsValidGroupMember( rockContext );

                        if ( !cvGroupMember.IsValid )
                        {
                            cvGroupMember.ErrorMessage = archivedGroupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                            return false;
                        }

                        // matching archived person found, so prompt
                        mdRestoreArchivedPrompt.Show();
                        nbRestoreError.Visible = false;
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        nbRestoreArchivedGroupMember.Text = string.Format(
                            "There is an archived record for {0} as a {1} in this group. Do you want to restore the previous settings? Notes will be retained.",
                            person,
                            role );

                        hfRestoreGroupMemberId.Value = archivedGroupMember.Id.ToString();
                        return false;
                    }
                }

                if ( this.IsSignUpMode )
                {
                    /*
                     * 2/21/2023 - JPH
                     * If Location and Schedule IDs were provided in the query string, this block is being used in sign-up mode,
                     * meaning scheduling is managed differently; create a GroupMemberAssignment record for this project (Group),
                     * GroupMember, Location & Schedule combination, but only if one doesn't already exist (we might simply be
                     * updating an existing GroupMember record).
                     * 
                     * Reason: Sign-Up Feature
                     */
                    var groupMemberAssignment = groupMemberAssignmentService
                        .Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( gma =>
                            gma.GroupMember.GroupId == group.Id
                            && gma.GroupMember.PersonId == personId.Value
                            && gma.LocationId == LocationId.Value
                            && gma.ScheduleId == ScheduleId.Value
                        );

                    if ( groupMemberAssignment == null )
                    {
                        groupMemberAssignment = new GroupMemberAssignment
                        {
                            LocationId = LocationId.Value,
                            ScheduleId = ScheduleId.Value
                        };

                        if ( groupMember.Id == 0 )
                        {
                            groupMemberAssignment.GroupMember = groupMember;
                        }
                        else
                        {
                            groupMemberAssignment.GroupMemberId = groupMember.Id;
                        }

                        groupMemberAssignmentService.Add( groupMemberAssignment );
                    }
                }
                else if ( pnlScheduling.Visible )
                {
                    groupMember.ScheduleTemplateId = ddlGroupMemberScheduleTemplate.SelectedValue.AsIntegerOrNull();
                    groupMember.ScheduleStartDate = dpScheduleStartDate.SelectedDate;
                    groupMember.ScheduleReminderEmailOffsetDays = nbScheduleReminderEmailOffsetDays.Text.AsIntegerOrNull();
                    var groupLocationService = new GroupLocationService( rockContext );
                    var qryGroupLocations = groupLocationService
                        .Queryable()
                        .Where( g => g.GroupId == group.Id );

                    var uiGroupMemberAssignments = GroupMemberAssignmentsState.Select( r => r.Guid );
                    foreach ( var groupMemberAssignment in groupMember.GroupMemberAssignments.Where( r => !uiGroupMemberAssignments.Contains( r.Guid ) && (
                                !r.LocationId.HasValue
                                || qryGroupLocations.Any( gl => gl.LocationId == r.LocationId && gl.Schedules.Any( s => s.Id == r.ScheduleId ) )
                            ) ).ToList() )
                    {
                        groupMember.GroupMemberAssignments.Remove( groupMemberAssignment );
                        groupMemberAssignmentService.Delete( groupMemberAssignment );
                    }

                    foreach ( var groupMemberAssignmentStateObj in GroupMemberAssignmentsState )
                    {
                        GroupMemberAssignment groupMemberAssignment = groupMember.GroupMemberAssignments.Where( a => a.Guid == groupMemberAssignmentStateObj.Guid ).FirstOrDefault();
                        if ( groupMemberAssignment == null )
                        {
                            groupMemberAssignment = new GroupMemberAssignment();
                            groupMember.GroupMemberAssignments.Add( groupMemberAssignment );
                        }

                        groupMemberAssignment.ScheduleId = groupMemberAssignmentStateObj.ScheduleId;
                        groupMemberAssignment.LocationId = groupMemberAssignmentStateObj.LocationId;
                    }
                }

                if ( group.RequiredSignatureDocumentTemplate != null )
                {
                    var person = new PersonService( rockContext ).Get( personId.Value );

                    var documentService = new SignatureDocumentService( rockContext );
                    var binaryFileService = new BinaryFileService( rockContext );
                    SignatureDocument document = null;

                    int? signatureDocumentId = hfSignedDocumentId.Value.AsIntegerOrNull();
                    int? binaryFileId = fuSignedDocument.BinaryFileId;
                    if ( signatureDocumentId.HasValue )
                    {
                        document = documentService.Get( signatureDocumentId.Value );
                    }

                    if ( document == null && binaryFileId.HasValue )
                    {
                        document = new SignatureDocument();
                        document.SignatureDocumentTemplateId = group.RequiredSignatureDocumentTemplate.Id;
                        document.AppliesToPersonAliasId = personAliasId.Value;
                        document.AssignedToPersonAliasId = personAliasId.Value;
                        document.Name = string.Format(
                            "{0}_{1}",
                            group.Name.RemoveSpecialCharacters(),
                            person != null ? person.FullName.RemoveSpecialCharacters() : string.Empty );
                        document.Status = SignatureDocumentStatus.Signed;
                        document.LastStatusDate = RockDateTime.Now;
                        documentService.Add( document );
                    }

                    if ( document != null )
                    {
                        int? origBinaryFileId = document.BinaryFileId;
                        document.BinaryFileId = binaryFileId;

                        if ( origBinaryFileId.HasValue && origBinaryFileId.Value != document.BinaryFileId )
                        {
                            // if a new the binaryFile was uploaded, mark the old one as Temporary so that it gets cleaned up
                            var oldBinaryFile = binaryFileService.Get( origBinaryFileId.Value );
                            if ( oldBinaryFile != null && !oldBinaryFile.IsTemporary )
                            {
                                oldBinaryFile.IsTemporary = true;
                            }
                        }

                        // ensure the IsTemporary is set to false on binaryFile associated with this document
                        if ( document.BinaryFileId.HasValue )
                        {
                            var binaryFile = binaryFileService.Get( document.BinaryFileId.Value );
                            if ( binaryFile != null && binaryFile.IsTemporary )
                            {
                                binaryFile.IsTemporary = false;
                            }
                        }
                    }
                }

                groupMember.LoadAttributes();
                avcAttributes.GetEditValues( groupMember );

                if ( !Page.IsValid )
                {
                    return false;
                }

                // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
                // So, make sure a message is displayed in the validation summary
                cvGroupMember.IsValid = groupMember.IsValidGroupMember( rockContext );

                if ( !cvGroupMember.IsValid )
                {
                    cvGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return false;
                }

                // using WrapTransaction because there are three Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( groupMember.Id.Equals( 0 ) )
                    {
                        groupMemberService.Add( groupMember );
                    }

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                    groupMember.CalculateRequirements( rockContext, true );
                } );
            }

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbResendDocumentRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbResendDocumentRequest_Click( object sender, EventArgs e )
        {
            int groupMemberId = PageParameter( PageParameterKey.GroupMemberId ).AsInteger();
            if ( groupMemberId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );
                    if ( groupMember != null && groupMember.Group != null )
                    {
                        var sendErrorMessages = new List<string>();

                        string documentName = string.Format( "{0}_{1}", groupMember.Group.Name.RemoveSpecialCharacters(), groupMember.Person.FullName.RemoveSpecialCharacters() );
                        if ( new SignatureDocumentTemplateService( rockContext ).SendLegacyProviderDocument(
                            groupMember.Group.RequiredSignatureDocumentTemplate, groupMember.Person, groupMember.Person, documentName, groupMember.Person.Email, out sendErrorMessages ) )
                        {
                            rockContext.SaveChanges();
                            maSignatureRequestSent.Show( "A Signature Request Has Been Sent.", Rock.Web.UI.Controls.ModalAlertType.Information );
                            ShowRequiredDocumentStatus( rockContext, groupMember, groupMember.Group );
                        }
                        else
                        {
                            string errorMessage = string.Format( "Unable to send a signature request: <ul><li>{0}</li></ul>", sendErrorMessages.AsDelimited( "</li><li>" ) );
                            maSignatureRequestSent.Show( errorMessage, Rock.Web.UI.Controls.ModalAlertType.Alert );
                        }
                    }
                }
            }
        }

        #endregion Edit Events

        #region GroupPreferenceAssignment Events

        /// <summary>
        /// Handles the GridRebind event of the gGroupPreferenceAssignments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gGroupPreferenceAssignments_GridRebind( object sender, EventArgs e )
        {
            BindGroupPreferenceAssignmentsGrid();
        }

        /// <summary>
        /// Handles the Add event of the gGroupPreferenceAssignments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gGroupPreferenceAssignments_Add( object sender, EventArgs e )
        {
            gGroupPreferenceAssignments_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Click event of the btnEditGroupPreferenceAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnEditGroupPreferenceAssignment_Click( object sender, RowEventArgs e )
        {
            Guid groupPreferenceAssignmentGuid = ( Guid ) e.RowKeyValue;
            gGroupPreferenceAssignments_ShowEdit( groupPreferenceAssignmentGuid );
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteGroupPreferenceAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnDeleteGroupPreferenceAssignment_Click( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            var groupStateObj = GroupMemberAssignmentsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( groupStateObj != null )
            {
                GroupMemberAssignmentsState.Remove( groupStateObj );
            }

            BindGroupPreferenceAssignmentsGrid();
        }

        /// <summary>
        /// gs the statuses_ show edit.
        /// </summary>
        /// <param name="groupPreferenceAssignmentGuid">The group preference assignment status unique identifier.</param>
        protected void gGroupPreferenceAssignments_ShowEdit( Guid groupPreferenceAssignmentGuid )
        {
            int? selectedScheduleId = null;
            int? selectedLocationId = null;
            var groupMemberAssignmentState = GroupMemberAssignmentsState.FirstOrDefault( l => l.Guid.Equals( groupPreferenceAssignmentGuid ) );
            if ( groupMemberAssignmentState != null )
            {
                selectedScheduleId = groupMemberAssignmentState.ScheduleId;
                selectedLocationId = groupMemberAssignmentState.LocationId;
            }

            hfGroupScheduleAssignmentGuid.Value = groupPreferenceAssignmentGuid.ToString();
            var groupId = hfGroupId.Value.AsInteger();
            var rockContext = new RockContext();
            var groupLocationService = new GroupLocationService( rockContext );
            var scheduleList = groupLocationService
                .Queryable()
                .AsNoTracking()
                .Where( g => g.GroupId == groupId )
                .SelectMany( g => g.Schedules )
                .Distinct()
                .ToList();

            List<Schedule> sortedScheduleList = scheduleList.OrderByOrderAndNextScheduledDateTime();


            var configuredScheduleIds = GroupMemberAssignmentsState
                .Select( s => s.ScheduleId ).Distinct().ToList();

            // limit to schedules that haven't had a schedule preference set yet
            sortedScheduleList = sortedScheduleList.Where( a =>
                a.IsActive
                && a.IsPublic.HasValue
                && a.IsPublic.Value
                && ( !configuredScheduleIds.Contains( a.Id )
                || ( selectedScheduleId.HasValue
                    && a.Id == selectedScheduleId.Value ) ) )
             .ToList();

            ddlGroupScheduleAssignmentSchedule.Items.Clear();
            ddlGroupScheduleAssignmentSchedule.Items.Add( new ListItem() );
            foreach ( var schedule in sortedScheduleList )
            {
                var scheduleName = GetFormattedScheduleForListing( schedule.Name, schedule.StartTimeOfDay );
                var scheduleListItem = new ListItem( scheduleName, schedule.Id.ToString() );
                if ( selectedScheduleId.HasValue && selectedScheduleId.Value == schedule.Id )
                {
                    scheduleListItem.Selected = true;
                }

                ddlGroupScheduleAssignmentSchedule.Items.Add( scheduleListItem );
            }

            PopulateGroupScheduleAssignmentLocations( groupId, selectedScheduleId );
            ddlGroupScheduleAssignmentLocation.SetValue( selectedLocationId );

            mdGroupScheduleAssignment.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupScheduleAssignmentSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupId = hfGroupId.Value.AsInteger();
            PopulateGroupScheduleAssignmentLocations( groupId, ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupScheduleAssignment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupScheduleAssignment_SaveClick( object sender, EventArgs e )
        {
            var groupMemberAssignmentGuid = hfGroupScheduleAssignmentGuid.Value.AsGuid();
            var scheduleId = ddlGroupScheduleAssignmentSchedule.SelectedValue.AsIntegerOrNull();
            var locationId = ddlGroupScheduleAssignmentLocation.SelectedValue.AsIntegerOrNull();

            // schedule is required, but locationId can be null (which means no location specified )
            if ( !scheduleId.HasValue )
            {
                return;
            }

            var schedule = new ScheduleService( new RockContext() ).Get( scheduleId.Value );
            if ( schedule == null )
            {
                return;
            }

            var groupMemberAssignment = GroupMemberAssignmentsState.Where( w => w.Guid.Equals( groupMemberAssignmentGuid ) && !groupMemberAssignmentGuid.Equals( Guid.Empty ) ).FirstOrDefault();
            if ( groupMemberAssignment == null )
            {
                groupMemberAssignment = new GroupMemberAssignmentStateObj();
                groupMemberAssignment.Guid = Guid.NewGuid();
                GroupMemberAssignmentsState.Add( groupMemberAssignment );
            }

            // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );
            groupMemberAssignment.ScheduleId = scheduleId.Value;
            groupMemberAssignment.ScheduleName = schedule.Name;
            groupMemberAssignment.ScheduleOrder = schedule.Order;
            groupMemberAssignment.ScheduleNextStartDateTime = schedule.GetNextStartDateTime( occurrenceDate );
            groupMemberAssignment.FormattedScheduleName = GetFormattedScheduleForListing( schedule.Name, schedule.StartTimeOfDay );
            groupMemberAssignment.LocationId = locationId;
            groupMemberAssignment.LocationName = ddlGroupScheduleAssignmentLocation.SelectedItem.Text;

            BindGroupPreferenceAssignmentsGrid();
            mdGroupScheduleAssignment.Hide();
        }


        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            CalculateRequirements( true );
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppGroupMemberPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppGroupMemberPerson_SelectPerson( object sender, EventArgs e )
        {
            CalculateRequirements( true );
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefreshRequirements_Click( object sender, EventArgs e )
        {
            CalculateRequirements( true );
            nbRecheckedNotification.Text = "Successfully refreshed requirements.";
            nbRecheckedNotification.Visible = true;
            bool areRequirementsPubliclyHidden = this.GetAttributeValue( AttributeKey.AreRequirementsPubliclyHidden ).AsBooleanOrNull() ?? false;
            gmrcRequirements.Visible = !areRequirementsPubliclyHidden;
            gmrcRequirements.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnShowMoveDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowMoveDialog_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                lCurrentGroup.Text = groupMember.Group.Name;
                gpMoveGroupMember.SetValue( null );
                grpMoveGroupMember.Visible = false;
                nbMoveGroupMemberWarning.Visible = false;
                mdMoveGroupMember.Visible = true;
                mdMoveGroupMember.Show();
                SetFundraisingTransferOptionVisibility( groupMember.Group.GroupTypeId, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMoveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMoveGroupMember_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMember = groupMemberService.Get( hfGroupMemberId.Value.AsInteger() );
            groupMember.LoadAttributes();
            int destGroupId = gpMoveGroupMember.SelectedValue.AsInteger();
            var destGroup = new GroupService( rockContext ).Get( destGroupId );

            var destGroupMember = groupMemberService.Queryable().Where( a =>
                a.GroupId == destGroupId
                && a.PersonId == groupMember.PersonId
                && a.GroupRoleId == grpMoveGroupMember.GroupRoleId ).FirstOrDefault();

            if ( destGroupMember != null )
            {
                nbMoveGroupMemberWarning.Visible = true;
                nbMoveGroupMemberWarning.Text = string.Format( "{0} is already in {1}", groupMember.Person, destGroupMember.Group );
                return;
            }

            if ( !grpMoveGroupMember.GroupRoleId.HasValue )
            {
                nbMoveGroupMemberWarning.Visible = true;
                nbMoveGroupMemberWarning.Text = string.Format( "Please select a Group Role" );
                return;
            }

            var isArchive = false;

            // If we can't delete, then we'll have to archive the group member.
            // This is making this assumption since the only reason why CanDelete would return
            // false is because the group member is tied to an group that has history tracking enabled.
            string canDeleteWarning;
            if ( !groupMemberService.CanDelete( groupMember, out canDeleteWarning ) )
            {
                isArchive = true;
            }

            destGroupMember = new GroupMember();
            destGroupMember.GroupId = destGroupId;
            destGroupMember.GroupRoleId = grpMoveGroupMember.GroupRoleId.Value;
            destGroupMember.PersonId = groupMember.PersonId;
            destGroupMember.LoadAttributes();

            if ( cbMoveGroupMemberFundraisingTransactions.Checked )
            {
                if ( !CanMoveFundraisingTransactions( destGroup, rockContext ) )
                {
                    nbMoveGroupMemberWarning.Visible = true;
                    nbMoveGroupMemberWarning.Text = string.Format( "The destination group is not properly configured to accept the fundraising transactions." );
                    return;
                }
            }

            foreach ( var attribute in groupMember.Attributes )
            {
                if ( destGroupMember.Attributes.Any( a => a.Key == attribute.Key && a.Value.FieldTypeId == attribute.Value.FieldTypeId ) )
                {
                    destGroupMember.SetAttributeValue( attribute.Key, groupMember.GetAttributeValue( attribute.Key ) );
                }
            }

            // Un-link any registrant records that point to this group member.
            foreach ( var registrant in new RegistrationRegistrantService( rockContext ).Queryable()
                .Where( r => r.GroupMemberId == groupMember.Id ) )
            {
                registrant.GroupMemberId = null;
            }

            rockContext.WrapTransaction( () =>
            {
                groupMemberService.Add( destGroupMember );
                rockContext.SaveChanges();
                destGroupMember.SaveAttributeValues( rockContext );

                // move any Note records that were associated with the old groupMember to the new groupMember record
                if ( cbMoveGroupMemberMoveNotes.Checked )
                {
                    destGroupMember.Note = groupMember.Note;
                    int groupMemberEntityTypeId = EntityTypeCache.GetId<Rock.Model.GroupMember>().Value;
                    var noteService = new NoteService( rockContext );
                    var groupMemberNotes = noteService.Queryable().Where( a => a.NoteType.EntityTypeId == groupMemberEntityTypeId && a.EntityId == groupMember.Id );
                    foreach ( var note in groupMemberNotes )
                    {
                        note.EntityId = destGroupMember.Id;
                    }

                    rockContext.SaveChanges();
                }

                if ( cbMoveGroupMemberFundraisingTransactions.Checked )
                {
                    MoveFundraisingTransactions( groupMember, destGroupMember, rockContext );
                }

                if ( isArchive )
                {
                    groupMemberService.Archive( groupMember, this.CurrentPersonAliasId, true );
                }
                else
                {
                    groupMemberService.Delete( groupMember );
                }

                rockContext.SaveChanges();

                destGroupMember.CalculateRequirements( rockContext, true );
            } );

            var queryString = new Dictionary<string, string>();
            queryString.Add( PageParameterKey.GroupMemberId, destGroupMember.Id.ToString() );
            this.NavigateToPage( this.RockPage.Guid, queryString );
        }

        /// <summary>
        /// Handles the SelectItem event of the gpMoveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpMoveGroupMember_SelectItem( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var destGroup = new GroupService( rockContext ).Get( gpMoveGroupMember.SelectedValue.AsInteger() );
            if ( destGroup != null )
            {
                var destTempGroupMember = new GroupMember { Group = destGroup, GroupId = destGroup.Id };
                destTempGroupMember.LoadAttributes( rockContext );
                var destGroupMemberAttributes = destTempGroupMember.Attributes;
                var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
                groupMember.LoadAttributes();
                var currentGroupMemberAttributes = groupMember.Attributes;

                var lostAttributes = currentGroupMemberAttributes.Where( a => !destGroupMemberAttributes.Any( d => d.Key == a.Key && d.Value.FieldTypeId == a.Value.FieldTypeId ) );
                nbMoveGroupMemberWarning.Visible = lostAttributes.Any();
                nbMoveGroupMemberWarning.Text = "The destination group does not have the same group member attributes as the source. Some loss of data may occur";

                if ( destGroup.Id == groupMember.GroupId )
                {
                    grpMoveGroupMember.Visible = false;
                    nbMoveGroupMemberWarning.Visible = true;
                    nbMoveGroupMemberWarning.Text = "The destination group is the same as the current group";
                }
                else
                {
                    grpMoveGroupMember.Visible = true;
                    grpMoveGroupMember.GroupTypeId = destGroup.GroupTypeId;
                    grpMoveGroupMember.GroupRoleId = destGroup.GroupType.DefaultGroupRoleId;
                }
            }
            else
            {
                nbMoveGroupMemberWarning.Visible = false;
                grpMoveGroupMember.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupMemberScheduleTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupMemberScheduleTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            dpScheduleStartDate.Required = ddlGroupMemberScheduleTemplate.SelectedValue.AsIntegerOrNull().HasValue;
        }

        #endregion Events

        protected void btnShowCommunicationDialog_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                lCommunicationTo.Text = string.Format( "<strong>To: </strong>{0}", groupMember.Person.FullName );
                bool enableSMS = this.GetAttributeValue( AttributeKey.EnableSMS ).AsBooleanOrNull() ?? true;
                tglCommunicationPreference.Visible = enableSMS;
                mdQuickCommunication.Visible = true;
                mdQuickCommunication.Show();
            }
        }

        protected void tglCommunicationPreference_CheckedChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                if ( tglCommunicationPreference.Checked )
                {
                    // SMS was chosen.
                    LoadPhoneNumbers();
                    pnlEmailControls.Visible = false;
                    pnlSMSControls.Visible = true;
                    var smsAvailablePhoneNumbers = groupMember.Person.PhoneNumbers.Where( p => p.IsMessagingEnabled && p.IsValid );

                    // Make sure the person has an SMS-enabled phone number.
                    if ( !smsAvailablePhoneNumbers.Any() )
                    {
                        pnlSMSControls.Visible = false;
                        tbCommunicationMessage.Visible = false;
                        nbSendGroupMemberCommunication.Visible = true;
                        nbSendGroupMemberCommunication.Text = string.Format( "No SMS enabled phone number exists for {0}.", groupMember.Person.FullName );

                        return;
                    }

                    tbCommunicationMessage.Visible = true;
                    nbSendGroupMemberCommunication.Visible = false;
                    lCommunicationTo.Text = string.Format( "<strong>To: </strong>{0} | {1}", groupMember.Person.FullName, smsAvailablePhoneNumbers.First().ToString() );
                    hfToSMSNumber.Value = smsAvailablePhoneNumbers.First().ToSmsNumber();
                }
                else
                {
                    // Email was chosen.
                    pnlEmailControls.Visible = true;
                    pnlSMSControls.Visible = false;

                    // Make sure the person has an email address that's allowed.
                    if ( !groupMember.Person.IsEmailActive || !groupMember.Person.CanReceiveEmail() )
                    {
                        pnlEmailControls.Visible = false;
                        tbCommunicationMessage.Visible = false;
                        nbSendGroupMemberCommunication.Visible = true;
                        nbSendGroupMemberCommunication.Text = string.Format( "No email address is available for {0}.", groupMember.Person.FullName );
                        return;
                    }

                    tbCommunicationMessage.Visible = true;
                    nbSendGroupMemberCommunication.Visible = false;
                    lCommunicationTo.Text = string.Format( "<strong>To: </strong>{0}", groupMember.Person.FullName );
                }
            }
        }

        protected void mdQuickCommunication_SaveClick( object sender, EventArgs e )
        {
            var communicationType = tglCommunicationPreference.Checked ? CommunicationType.SMS : CommunicationType.Email;
            var communicationSuccessful = SendCommunication( communicationType );
            if ( communicationSuccessful )
            {
                mdQuickCommunication.Hide();
                nbCommunicationSuccess.Visible = true;
            }
        }

        /// <summary>
        /// Sends email to the intended recipient.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="fromEmail"></param>
        /// <param name="fromName"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="createCommunicationRecord"></param>
        private void SendEmail( RockEmailMessageRecipient recipient, string fromEmail, string fromName, string subject, string message, bool createCommunicationRecord )
        {
            var emailMessage = new RockEmailMessage();
            emailMessage.AddRecipient( recipient );
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = message;
            emailMessage.CreateCommunicationRecord = createCommunicationRecord;
            emailMessage.Send();
        }

        /// <summary>
        /// Sends SMS to the intended recipient.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="fromValue"></param>
        /// <param name="message"></param>
        /// <param name="createCommunicationRecord"></param>
        private void SendSMS( RockSMSMessageRecipient recipient, SystemPhoneNumberCache fromValue, string message, bool createCommunicationRecord )
        {
            var smsMessage = new RockSMSMessage();
            smsMessage.AddRecipient( recipient );
            smsMessage.FromSystemPhoneNumber = fromValue;
            smsMessage.Message = message;
            smsMessage.CreateCommunicationRecord = createCommunicationRecord;
            smsMessage.CommunicationName = "Group Member Quick Communication";
            smsMessage.Send();
        }

        private bool SendCommunication( CommunicationType communicationType )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember == null )
            {
                return false;
            }

            // Send to either SMS or Email.
            if ( communicationType == CommunicationType.Email )
            {
                string emailMessage = tbCommunicationMessage.Text;
                RockEmailMessageRecipient rockEmailMessageRecipient = new RockEmailMessageRecipient( groupMember.Person, new Dictionary<string, object>() );

                bool appendHeaderFooter = this.GetAttributeValue( AttributeKey.AppendHeaderFooter ).AsBooleanOrNull() ?? true;
                if ( appendHeaderFooter )
                {
                    var globalAttributes = GlobalAttributesCache.Get();
                    string emailHeader = globalAttributes.GetValue( "EmailHeader" );
                    string emailFooter = globalAttributes.GetValue( "EmailFooter" );
                    emailMessage = $"{emailHeader} {emailMessage} {emailFooter}";
                }

                SendEmail( rockEmailMessageRecipient, groupMember.Person.Email, groupMember.Person.FullName, tbEmailCommunicationSubject.Text, emailMessage, false );
                return true;
            }
            else if ( communicationType == CommunicationType.SMS && hfToSMSNumber.Value.IsNotNullOrWhiteSpace() )
            {
                hfFromSMSNumber.SetValue( ddlSmsNumbers.SelectedValue.AsInteger() );
                var smsPhoneNumbers = SystemPhoneNumberCache.All( false )
                    .Where( v => v.IsAuthorized( Authorization.VIEW, this.CurrentPerson )
                        && v.Id == hfFromSMSNumber.Value.AsInteger() )
                    .ToList();

                if ( !smsPhoneNumbers.Any() )
                {
                    // If there aren't any available SMS numbers to send from, set warning and return false.
                    nbSendGroupMemberCommunication.Text = string.Format( "Unable to send an SMS message, as you do not have an SMS-enabled phone number from which to send." );
                    return false;
                }

                var selectedSMSFrom = smsPhoneNumbers.First();
                RockSMSMessageRecipient rockSMSMessageRecipient = new RockSMSMessageRecipient( groupMember.Person, hfToSMSNumber.Value, new Dictionary<string, object>() );
                SendSMS( rockSMSMessageRecipient, selectedSMSFrom, tbCommunicationMessage.Text, false );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the formatted schedule name used for listing.
        /// </summary>
        /// <param name="scheduleName">The schedule name.</param>
        /// <param name="startTimeOfDay">The start time of day.</param>
        private string GetFormattedScheduleForListing( string scheduleName, TimeSpan startTimeOfDay )
        {
            var formattedScheduleName = string.Empty;
            var scheduleListFormat = GetAttributeValue( AttributeKey.ScheduleListFormat ).AsInteger();
            if ( scheduleListFormat == 1 )
            {
                formattedScheduleName = startTimeOfDay.ToTimeString();
            }
            else if ( scheduleListFormat == 2 )
            {
                formattedScheduleName = scheduleName;
            }
            else
            {
                formattedScheduleName = $"{startTimeOfDay.ToTimeString()} {scheduleName}";
            }

            return formattedScheduleName;
        }

        #region Helper Classes

        [Serializable]
        public class GroupMemberAssignmentStateObj
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public int ScheduleOrder { get; set; }

            public DateTime? ScheduleNextStartDateTime { get; set; }

            public int? LocationId { get; set; }

            public string LocationName { get; set; }

            public string ScheduleName { get; set; }

            public string FormattedScheduleName { get; set; }

            public int ScheduleId { get; set; }
        }

        #endregion

    }

    internal class GroupRequirementWithCategoryInfo
    {
        public int? CategoryId { get; set; }

        public string Name { get; set; }

        public IEnumerable<GroupRequirementStatus> RequirementResults { get; set; }
    }
}