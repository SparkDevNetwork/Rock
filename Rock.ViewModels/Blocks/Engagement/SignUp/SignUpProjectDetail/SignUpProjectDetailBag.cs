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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// The item details for the Sign-Up Project Detail block. A sign-up project is a group whose
    /// type is (or inherits from) the sign-up group type.
    /// </summary>
    public class SignUpProjectDetailBag : EntityBagBase
    {
        #region Common Properties

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the project.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the project is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the project is a system group, in which case
        /// it cannot be edited or deleted.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is authorized to edit and
        /// delete the project.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is authorized to
        /// administrate the project. This controls the security button and the group
        /// requirements section.
        /// </summary>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is authorized to add, edit
        /// and delete the project's opportunities.
        /// </summary>
        public bool CanSchedule { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the project's group type. The name of this property
        /// must match the group attribute qualifier column so the block can refresh the group
        /// attributes when the group type changes while adding a project.
        /// </summary>
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the project's group type.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the project's campus.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the name of the project's campus, displayed as a panel label.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the project type defined value selected for
        /// the project's "Project Type" attribute.
        /// </summary>
        public string ProjectTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the display name of the selected project type, displayed as a panel label.
        /// </summary>
        public string ProjectTypeName { get; set; }

        #endregion Common Properties

        #region View Properties

        /// <summary>
        /// Gets or sets the summaries of the group requirements that apply to the project, from
        /// both the group type and the project itself.
        /// </summary>
        public List<SignUpProjectRequirementSummaryBag> RequirementSummaries { get; set; }

        /// <summary>
        /// Gets or sets the project's opportunities (location and schedule combinations).
        /// </summary>
        public List<SignUpOpportunityBag> Opportunities { get; set; }

        #endregion View Properties

        #region Edit Properties

        /// <summary>
        /// Gets or sets a value indicating whether the project type radio button list should be
        /// displayed, which is the case when the group type defines the "Project Type" attribute.
        /// </summary>
        public bool IsProjectTypeVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a project type must be selected.
        /// </summary>
        public bool IsProjectTypeRequired { get; set; }

        /// <summary>
        /// Gets or sets the help text displayed for the project type radio button list.
        /// </summary>
        public string ProjectTypeHelpText { get; set; }

        /// <summary>
        /// Gets or sets the record source defined value that overrides the group type's record
        /// source for members added to the project.
        /// </summary>
        public ListItemBag GroupMemberRecordSource { get; set; }

        /// <summary>
        /// Gets or sets the system communication used to send reminders for in-person projects.
        /// </summary>
        public ListItemBag ReminderSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the number of days before an opportunity that reminders are sent.
        /// </summary>
        public int? ReminderOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the additional details appended to the reminder communication.
        /// </summary>
        public string ReminderAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the additional details appended to the communication sent when
        /// registering.
        /// </summary>
        public string ConfirmationAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the attributes that apply to each member of the project.
        /// </summary>
        public List<PublicEditableAttributeBag> MemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attributes that apply to each member of each opportunity of the
        /// project.
        /// </summary>
        public List<PublicEditableAttributeBag> MemberOpportunityAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group requirements defined specifically for the project. Only
        /// populated when the current person can administrate the project.
        /// </summary>
        public List<SignUpProjectRequirementBag> GroupRequirements { get; set; }

        #endregion Edit Properties
    }
}
