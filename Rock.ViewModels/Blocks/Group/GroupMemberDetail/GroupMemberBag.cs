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

using Rock.Enums.Communication;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The entity bag for the Group Member Detail block.
    /// </summary>
    public class GroupMemberBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the person this membership belongs to. Only
        /// changeable while adding a new member.
        /// </summary>
        public ListItemBag Person { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the group role this member holds.
        /// </summary>
        public int? GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the member's status in the group.
        /// </summary>
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the internal note about this member's involvement.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets how this member prefers to receive group
        /// communications.
        /// </summary>
        public CommunicationType CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group's leaders have
        /// already been notified about this member. Only visible with
        /// ADMINISTRATE authorization.
        /// </summary>
        public bool IsNotified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether chat notifications are
        /// muted for this person in this group.
        /// </summary>
        public bool IsChatMuted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is banned
        /// from the group's chat channel.
        /// </summary>
        public bool IsChatBanned { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the schedule template this member
        /// follows for serving.
        /// </summary>
        public int? ScheduleTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the date the member's schedule template takes
        /// effect. Required when a schedule template is selected.
        /// </summary>
        public DateTimeOffset? ScheduleStartDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days before a scheduled occurrence to
        /// send this member's reminder. Null uses the group's default.
        /// </summary>
        public int? ScheduleReminderEmailOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the manually uploaded signed document binary file,
        /// when the group requires a signature document template.
        /// </summary>
        public ListItemBag SignedDocument { get; set; }

        /// <summary>
        /// Gets or sets the member's schedule and location assignment
        /// preferences. Edited client-side and reconciled on save.
        /// </summary>
        public List<GroupScheduleAssignmentBag> ScheduleAssignments { get; set; }

        /// <summary>
        /// Gets or sets the group requirement guids the user has manually
        /// marked as met. Held client-side and persisted as
        /// GroupMemberRequirement rows in the same transaction as the save.
        /// </summary>
        public List<Guid> ManuallyMetRequirementGuids { get; set; }

        /// <summary>
        /// Gets or sets the group requirement guids a leader has overridden as
        /// met while adding. Held client-side and persisted as overridden
        /// GroupMemberRequirement rows in the same transaction as the save.
        /// </summary>
        public List<Guid> OverriddenRequirementGuids { get; set; }

        /// <summary>
        /// Gets or sets the group member assignment attribute values. Only
        /// used in sign-up mode.
        /// </summary>
        public Dictionary<string, string> AssignmentAttributeValues { get; set; }
    }
}
