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

using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group member.
    /// </summary>
    [DisplayName( "Group Member Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group member for editing role, status, etc." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Registration Page",
        Description = "The page used to view the registration(s) linked to this group member.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.RegistrationPage )]

    [BooleanField(
        "Show \"Move To Another Group\" Button",
        Description = "Whether the button for moving this member to another group is shown.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.ShowMoveToOtherGroup )]

    [BooleanField(
        "Hide Requirements",
        Description = "Whether this member's requirement status alerts, and the option to refresh them, are hidden.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.AreRequirementsPubliclyHidden )]

    [BooleanField(
        "Hide Requirement Type Summary",
        Description = "Whether each requirement type's summary text is hidden, when requirements are shown.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.IsSummaryHidden )]

    [BooleanField(
        "Are Requirements Refreshed When Block Is Loaded",
        Description = "Whether group requirements are recalculated every time the block loads, instead of using cached results.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.AreRequirementsRefreshedOnLoad )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "The page used to launch a new workflow of the selected type.",
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY,
        Order = 5,
        Key = AttributeKey.WorkflowEntryPage )]

    [BooleanField(
        "Enable Communications",
        Description = "Whether quick communications can be sent from this block.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.EnableCommunications )]

    [BooleanField(
        "Enable SMS",
        Description = "Whether SMS is offered as a communication option, when the recipient has a messaging-enabled number. Email is the only option otherwise.",
        DefaultBooleanValue = true,
        Order = 7,
        Key = AttributeKey.EnableSMS )]

    [BooleanField(
        "Append Organization Email Header/Footer",
        Description = "Whether the organization's email header and footer are appended to the message.",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.AppendHeaderFooter )]

    [BooleanField(
        "Allow Sending From Other Email Addresses",
        Description = "Whether the email 'From' address can be changed. When disabled, messages are sent from the logged-in person's email address.",
        DefaultBooleanValue = true,
        Order = 9,
        Key = AttributeKey.AllowSelectingFrom )]

    [SystemPhoneNumberField(
        "Allowed SMS Numbers",
        Description = "The system phone numbers offered as the SMS sender. All authorized numbers are offered if none are selected.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 10,
        Key = AttributeKey.AllowedSMSNumbers )]

    [CustomDropdownListField(
        "Schedule List Format",
        Description = "How each schedule is displayed in the preference list and picker. By time, by name, or both.",
        ListSource = "1^Schedule Time,2^Schedule Name,3^Schedule Time and Name",
        IsRequired = false,
        DefaultValue = "1",
        Order = 11,
        Key = AttributeKey.ScheduleListFormat )]

    [BooleanField(
        "Include Group Name in Breadcrumb",
        Description = "Whether the group's name is included in the breadcrumb trail ahead of the member's name.",
        DefaultBooleanValue = true,
        Order = 12,
        Key = AttributeKey.IncludeGroupNameInBreadcrumb )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "443841E5-6D0E-4CF4-83D0-CE8083FF10EA" )]
    [Rock.SystemGuid.BlockTypeGuid( "BB6FB9A3-4177-4702-BC8B-1B254137732F" )]
    public class GroupMemberDetail : RockEntityDetailBlockType<GroupMember, GroupMemberBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string LocationId = "LocationId";
            public const string RegistrationId = "RegistrationId";
            public const string ScheduleId = "ScheduleId";
            public const string ReturnUrl = "returnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

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
            public const string IncludeGroupNameInBreadcrumb = "IncludeGroupNameInBreadcrumb";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag>();
            var entity = GetInitialEntity();

            // TODO: Guard states, authorization, and full bag population per conversion plan §7.
            box.Entity = GetEntityBagForEdit( entity );
            box.Options = GetBoxOptions( entity );
            box.NavigationUrls = GetBoxNavigationUrls();
            box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<GroupMember>();

            return box;
        }

        /// <inheritdoc/>
        protected override GroupMember GetInitialEntity()
        {
            // TODO: Apply new-member defaults (group, default role, active status, date added) per conversion plan §7.2.
            return GetInitialEntity<GroupMember, GroupMemberService>( RockContext, PageParameterKey.GroupMemberId );
        }

        /// <summary>
        /// Gets the options bag that describes everything the client needs
        /// to render the block.
        /// </summary>
        /// <param name="entity">The group member being viewed or edited.</param>
        /// <returns>The options bag.</returns>
        private GroupMemberDetailOptionsBag GetBoxOptions( GroupMember entity )
        {
            // TODO: Populate roles, statuses, scheduling, requirements, and visibility flags per conversion plan §7.
            return new GroupMemberDetailOptionsBag();
        }

        /// <summary>
        /// Gets the navigation URLs required by the client.
        /// </summary>
        /// <returns>A dictionary of key and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            // TODO: Parent page URL carrying GroupId, CampusId, and sign-up parameters per conversion plan §7.14.
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="GroupMemberBag"/> that represents the entity.</returns>
        private GroupMemberBag GetCommonEntityBag( GroupMember entity )
        {
            if ( entity == null )
            {
                return null;
            }

            // TODO: Populate the bag from the entity per conversion plan §4.
            return new GroupMemberBag
            {
                IdKey = entity.IdKey
            };
        }

        /// <inheritdoc/>
        protected override GroupMemberBag GetEntityBagForView( GroupMember entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override GroupMemberBag GetEntityBagForEdit( GroupMember entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( GroupMember entity, ValidPropertiesBox<GroupMemberBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // TODO: Apply valid properties to the entity per conversion plan §8.
            return true;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out GroupMember entity, out BlockActionResult error )
        {
            var entityService = new GroupMemberService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new GroupMember();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{GroupMember.FriendlyTypeName} not found." );
                return false;
            }

            // TODO: Enforce the full edit authorization matrix (block EDIT, group EDIT, MANAGE_MEMBERS, sign-up SCHEDULE) per conversion plan §3.
            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            // TODO: Member name crumb, optionally prefixed with the group name per the IncludeGroupNameInBreadcrumb setting.
            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the group member represented by the box.
        /// </summary>
        /// <param name="box">The box containing the valid properties to save.</param>
        /// <param name="isSaveThenAdd">Whether the client will reload in add mode after saving.</param>
        /// <param name="isRestoreDeclined">Whether the user declined restoring a matching archived member.</param>
        /// <returns>A <see cref="SaveGroupMemberResponseBag"/> describing the result.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<GroupMemberBag> box, bool isSaveThenAdd, bool isRestoreDeclined )
        {
            // TODO: Implement per conversion plan §8, including must-meet requirement enforcement and the archived-member prompt.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Restores a matching archived group member instead of creating a
        /// new record.
        /// </summary>
        /// <param name="archivedGroupMemberIdKey">The IdKey of the archived group member to restore.</param>
        /// <returns>The restored member's IdKey for reload.</returns>
        [BlockAction]
        public BlockActionResult RestoreArchivedGroupMember( string archivedGroupMemberIdKey )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Moves the group member to another group, optionally moving notes
        /// and fundraising transactions.
        /// </summary>
        /// <param name="bag">The move request.</param>
        /// <returns>The new member's IdKey.</returns>
        [BlockAction]
        public BlockActionResult MoveGroupMember( MoveGroupMemberRequestBag bag )
        {
            // TODO: Implement per conversion plan §8, including the fundraising transfer machinery.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the role options and warnings for a selected destination
        /// group in the move modal.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member being moved.</param>
        /// <param name="destinationGroupIdKey">The IdKey of the selected destination group.</param>
        /// <returns>A <see cref="MoveGroupMemberOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetMoveGroupMemberOptions( string groupMemberIdKey, string destinationGroupIdKey )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the state required to open the quick communication modal.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to communicate with.</param>
        /// <returns>A <see cref="CommunicationOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetCommunicationOptions( string groupMemberIdKey )
        {
            // TODO: Implement per conversion plan §8, including the four email and four SMS state cases.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Sends a quick email or SMS communication to the group member.
        /// </summary>
        /// <param name="bag">The communication request.</param>
        /// <returns>A success or validation error result.</returns>
        [BlockAction]
        public BlockActionResult SendCommunication( SendCommunicationRequestBag bag )
        {
            // TODO: Implement per conversion plan §8, with server-side required-field validation.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Sends the required signature document request to the group
        /// member. Pending Open Decision A in the conversion plan.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member to send the request to.</param>
        /// <returns>A success or error result.</returns>
        [BlockAction]
        public BlockActionResult SendSignatureRequest( string groupMemberIdKey )
        {
            // TODO: Blocked on Open Decision A. No working send API exists for legacy providers.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the existing sign-up group member for the selected person so
        /// the form can re-hydrate. Sign-up mode only.
        /// </summary>
        /// <param name="personIdKey">The IdKey of the selected person.</param>
        /// <returns>The existing member's bag, or an empty result when none exists.</returns>
        [BlockAction]
        public BlockActionResult GetExistingSignUpGroupMember( string personIdKey )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Recalculates the group requirements for the member and returns
        /// refreshed inline alert bags.
        /// </summary>
        /// <param name="groupMemberIdKey">The IdKey of the member.</param>
        /// <param name="selectedRoleId">The currently selected role identifier.</param>
        /// <returns>The refreshed requirement alerts.</returns>
        [BlockAction]
        public BlockActionResult RefreshRequirements( string groupMemberIdKey, int selectedRoleId )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        /// <summary>
        /// Gets the schedule and location options for the Assignment
        /// Preference modal.
        /// </summary>
        /// <param name="groupIdKey">The IdKey of the group.</param>
        /// <param name="selectedScheduleId">The selected schedule identifier, when loading locations.</param>
        /// <returns>A <see cref="ScheduleAssignmentOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetScheduleAssignmentOptions( string groupIdKey, int? selectedScheduleId )
        {
            // TODO: Implement per conversion plan §8.
            return ActionBadRequest( "Not implemented." );
        }

        #endregion Block Actions
    }
}
