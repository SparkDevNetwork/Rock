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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// A single Group Member Workflow Trigger row. The pipe-delimited 7-tuple
    /// <c>TypeQualifier</c> persisted on the entity is decomposed into individual typed fields
    /// here so the Vue layer never touches the raw qualifier string.
    /// </summary>
    public class GroupMemberWorkflowTriggerBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the workflow trigger.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the user-facing name of the workflow trigger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the trigger is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the workflow type to start when the trigger fires.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the trigger type (Member Added / Removed / Status Changed / Role
        /// Changed / Attended / Placed Elsewhere).
        /// </summary>
        public GroupMemberWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the "from" status qualifier used by
        /// <see cref="GroupMemberWorkflowTriggerType.MemberStatusChanged"/>. Null = Any.
        /// </summary>
        public GroupMemberStatus? FromStatus { get; set; }

        /// <summary>
        /// Gets or sets the "to" status qualifier used by
        /// <see cref="GroupMemberWorkflowTriggerType.MemberAddedToGroup"/>,
        /// <see cref="GroupMemberWorkflowTriggerType.MemberRemovedFromGroup"/>, and
        /// <see cref="GroupMemberWorkflowTriggerType.MemberStatusChanged"/>. Null = Any.
        /// </summary>
        public GroupMemberStatus? ToStatus { get; set; }

        /// <summary>
        /// Gets or sets the "from" role qualifier Guid used by
        /// <see cref="GroupMemberWorkflowTriggerType.MemberRoleChanged"/>. Null = Any.
        /// </summary>
        public Guid? FromRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the "to" role qualifier Guid used by
        /// <see cref="GroupMemberWorkflowTriggerType.MemberAddedToGroup"/>,
        /// <see cref="GroupMemberWorkflowTriggerType.MemberRemovedFromGroup"/>, and
        /// <see cref="GroupMemberWorkflowTriggerType.MemberRoleChanged"/>. Null = Any.
        /// </summary>
        public Guid? ToRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow fires only on the member's
        /// first attendance. Used by <see cref="GroupMemberWorkflowTriggerType.MemberAttendedGroup"/>.
        /// </summary>
        public bool TriggerOnFirstAttendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the placement workflow shows a note entry
        /// field. Used by <see cref="GroupMemberWorkflowTriggerType.MemberPlacedElsewhere"/>.
        /// </summary>
        public bool ShowNoteOnPlacement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the placement workflow requires a note.
        /// Used by <see cref="GroupMemberWorkflowTriggerType.MemberPlacedElsewhere"/>.
        /// </summary>
        public bool RequireNoteOnPlacement { get; set; }
    }
}
