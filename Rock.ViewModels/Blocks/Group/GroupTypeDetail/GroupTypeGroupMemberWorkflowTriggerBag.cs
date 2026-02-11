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

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// Represents a group member workflow trigger for a group type.
    /// </summary>
    public class GroupTypeGroupMemberWorkflowTriggerBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the workflow trigger.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the order of the workflow trigger.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow trigger is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the workflow type to execute.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the trigger type.
        /// </summary>
        public GroupMemberWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the "from" status qualifier. Note that for some trigger types (e.g. member added/removed),
        /// the UI labels the "to" status as "With Status of" but it is still stored in the "to" slot.
        /// </summary>
        public GroupMemberStatus? FromStatus { get; set; }

        /// <summary>
        /// Gets or sets the "to" status qualifier.
        /// </summary>
        public GroupMemberStatus? ToStatus { get; set; }

        /// <summary>
        /// Gets or sets the "from" role qualifier. Note that for some trigger types (e.g. member added/removed),
        /// the UI labels the "to" role as "With Role of" but it is still stored in the "to" slot.
        /// </summary>
        public Guid? FromRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the "to" role qualifier.
        /// </summary>
        public Guid? ToRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow should trigger only on first attendance.
        /// </summary>
        public bool TriggerOnFirstAttendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a note should be shown/collected on placement.
        /// </summary>
        public bool ShowNoteOnPlacement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a note is required on placement.
        /// </summary>
        public bool RequireNoteOnPlacement { get; set; }
    }
}


