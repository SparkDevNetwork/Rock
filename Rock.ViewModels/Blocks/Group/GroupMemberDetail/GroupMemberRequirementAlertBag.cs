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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// One inline group requirement alert, rendered below the Role field in
    /// the Group Member Detail block.
    /// </summary>
    public class GroupMemberRequirementAlertBag
    {
        /// <summary>
        /// Gets or sets the requirement's title text.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the requirement type's summary text. Empty when the
        /// Hide Requirement Type Summary block setting is enabled.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets whether the member meets the requirement, with a
        /// stored manual completion or override presenting as met. Drives
        /// the alert style (met, not met, warning).
        /// </summary>
        public MeetsGroupRequirement MeetsGroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the status line shown under the title, from the
        /// requirement type's positive, negative, or warning label.
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the status line shown when the requirement's
        /// checkbox is checked client-side, from the requirement type's
        /// positive label.
        /// </summary>
        public string MetStatusText { get; set; }

        /// <summary>
        /// Gets or sets the requirement type's icon CSS class.
        /// </summary>
        public string TypeIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may
        /// override this requirement, which renders the override checkbox.
        /// </summary>
        public bool CanOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a manual check
        /// type requirement, which renders the mark-as-met checkbox.
        /// </summary>
        public bool IsManualRequirement { get; set; }

        /// <summary>
        /// Gets or sets the label for the manual mark-as-met checkbox link,
        /// from the requirement type's checkbox label or its name.
        /// </summary>
        public string ManualCheckboxLabel { get; set; }

        /// <summary>
        /// Gets or sets the link text for launching the does-not-meet
        /// workflow. Null when the link should not be shown.
        /// </summary>
        public string DoesNotMeetWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets the link text for launching the warning workflow.
        /// Null when the link should not be shown.
        /// </summary>
        public string WarningWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets the "Due: {date}" text. Null when the requirement
        /// is met or has no due date.
        /// </summary>
        public string DueDateText { get; set; }

        /// <summary>
        /// Gets or sets the tooltip describing who overrode the requirement
        /// and when. Null when the requirement was not overridden.
        /// </summary>
        public string OverriddenText { get; set; }

        /// <summary>
        /// Gets or sets the group requirement's unique identifier.
        /// </summary>
        public Guid GroupRequirementGuid { get; set; }

        /// <summary>
        /// Gets or sets the group member requirement's unique identifier.
        /// Null when no requirement record exists yet for the member.
        /// </summary>
        public Guid? GroupMemberRequirementGuid { get; set; }
    }
}
