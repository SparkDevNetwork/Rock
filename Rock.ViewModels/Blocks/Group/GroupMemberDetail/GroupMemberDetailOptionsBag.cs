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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The additional configuration options for the Group Member Detail block.
    /// </summary>
    public class GroupMemberDetailOptionsBag
    {
        #region Role

        /// <summary>
        /// Gets or sets the group roles available for selection. Roles used
        /// by Group Sync are excluded unless the member already holds one.
        /// </summary>
        public List<ListItemBag> RoleItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role selection is
        /// locked because the member is in a role controlled by Group Sync.
        /// </summary>
        public bool IsRoleLockedBySync { get; set; }

        /// <summary>
        /// Gets or sets the tooltip explaining why the role selection is
        /// locked.
        /// </summary>
        public string RoleLockedMessage { get; set; }

        #endregion Role

        #region Form Options

        /// <summary>
        /// Gets or sets the member status options.
        /// </summary>
        public List<ListItemBag> StatusItems { get; set; }

        /// <summary>
        /// Gets or sets the communication preference options.
        /// </summary>
        public List<ListItemBag> CommunicationPreferenceItems { get; set; }

        /// <summary>
        /// Gets or sets the schedule templates available for the group's
        /// group type.
        /// </summary>
        public List<ListItemBag> ScheduleTemplateItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scheduling section
        /// is shown. True when the group type has scheduling enabled and
        /// the block is not in sign-up mode.
        /// </summary>
        public bool IsSchedulingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in sign-up
        /// mode (LocationId and ScheduleId page parameters present).
        /// </summary>
        public bool IsSignUpMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the chat preference
        /// checkboxes are shown. True when chat is enabled globally and for
        /// this group.
        /// </summary>
        public bool AreChatPreferencesShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Group Leaders
        /// Notified checkbox is shown. Requires ADMINISTRATE authorization.
        /// </summary>
        public bool IsNotifiedShown { get; set; }

        #endregion Form Options

        #region Header

        /// <summary>
        /// Gets or sets the group type's term for the group (e.g. "Group").
        /// </summary>
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the group type's term for a member (e.g. "Member").
        /// </summary>
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the formatted "Added: {date}" header label text.
        /// </summary>
        public string AddedDateText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group member being
        /// viewed is itself archived. Drives the red Archived header label.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the message explaining why the form is read-only.
        /// </summary>
        public string EditModeMessage { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class from the group's group type.
        /// </summary>
        public string GroupIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Save Then Add footer
        /// button is shown. Only shown while adding a new member.
        /// </summary>
        public bool IsSaveThenAddShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Move header button
        /// is shown. Hidden until the record exists.
        /// </summary>
        public bool IsMoveButtonShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Communicate header
        /// button is shown. Hidden until the record exists.
        /// </summary>
        public bool IsCommunicationButtonShown { get; set; }

        #endregion Header

        #region Person Display

        /// <summary>
        /// Gets or sets the member's person IdKey, used by the avatar's
        /// hover card in edit mode.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the member's photo URL, shown beside their name
        /// once the person can no longer be changed.
        /// </summary>
        public string PersonPhotoUrl { get; set; }

        #endregion Person Display

        #region Registrations and Documents

        /// <summary>
        /// Gets or sets the registrations linked to this member. Text is
        /// the registration name, value is the registration page URL.
        /// </summary>
        public List<ListItemBag> LinkedRegistrations { get; set; }

        /// <summary>
        /// Gets or sets the required signature document state, or null when
        /// the group has no required signature document template.
        /// </summary>
        public SignatureDocumentStatusBag SignatureDocument { get; set; }

        #endregion Registrations and Documents

        #region Requirements

        /// <summary>
        /// Gets or sets a value indicating whether the group has any
        /// requirements configured.
        /// </summary>
        public bool HasGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the inline requirement alerts rendered below the
        /// Role field.
        /// </summary>
        public List<GroupMemberRequirementAlertBag> RequirementAlerts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requirement alerts are
        /// hidden per the Hide Requirements block setting.
        /// </summary>
        public bool AreRequirementsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each requirement type's
        /// summary text is hidden.
        /// </summary>
        public bool IsRequirementSummaryHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requirement interaction
        /// (override, mark as met) is disabled.
        /// </summary>
        public bool IsRequirementInteractionDisabled { get; set; }

        /// <summary>
        /// Gets or sets the Workflow Entry linked page attribute value used
        /// to launch requirement workflows.
        /// </summary>
        public string WorkflowEntryPageValue { get; set; }

        /// <summary>
        /// Gets or sets the requirement calculation error details, one
        /// "{type}: {error}" per line. Null when every calculation
        /// succeeded.
        /// </summary>
        public string RequirementCalculationErrors { get; set; }

        /// <summary>
        /// Gets or sets the group's unique identifier, used by requirement
        /// REST endpoints.
        /// </summary>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the group role's unique identifier, used by
        /// requirement REST endpoints.
        /// </summary>
        public Guid? GroupRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the group member's unique identifier, used by
        /// requirement REST endpoints. Null while adding.
        /// </summary>
        public Guid? GroupMemberGuid { get; set; }

        /// <summary>
        /// Gets or sets the person's unique identifier, used by requirement
        /// REST endpoints.
        /// </summary>
        public Guid? PersonGuid { get; set; }

        #endregion Requirements

        #region Attributes

        /// <summary>
        /// Gets or sets the group member attributes the current person may
        /// view but not edit.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ViewableAttributes { get; set; }

        /// <summary>
        /// Gets or sets the formatted values for the viewable attributes.
        /// </summary>
        public Dictionary<string, string> ViewableAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the group member assignment attributes. Only
        /// populated in sign-up mode.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> AssignmentAttributes { get; set; }

        /// <summary>
        /// Gets or sets the assignment attributes the current person may
        /// view but not edit. Only populated in sign-up mode.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ViewableAssignmentAttributes { get; set; }

        /// <summary>
        /// Gets or sets the formatted values for the viewable assignment
        /// attributes.
        /// </summary>
        public Dictionary<string, string> ViewableAssignmentAttributeValues { get; set; }

        #endregion Attributes
    }
}
