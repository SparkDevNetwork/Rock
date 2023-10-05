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

using Rock.ViewModels.Utility;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupRequirementTypeDetail
{
    public class GroupRequirementTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this requirement can expire.
        /// </summary>
        public bool CanExpire { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the checkbox label. This is the text that is used for the checkbox if this is a manually set requirement
        /// </summary>
        public string CheckboxLabel { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.DataView.
        /// </summary>
        public ListItemBag DataView { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the text for the "Does Not Meet" workflow link.
        /// </summary>
        public string DoesNotMeetWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets "Does Not Meet" workflow type.
        /// </summary>
        public ListItemBag DoesNotMeetWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the number of days before the requirement is due.
        /// </summary>
        public int? DueDateOffsetInDays { get; set; }

        /// <summary>
        /// Gets or sets the type of due date.
        /// </summary>
        public string DueDateType { get; set; }

        /// <summary>
        /// Gets or sets the number of days after the requirement is met before it expires (If CanExpire is true). NULL means never expires
        /// </summary>
        public int? ExpireInDays { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the negative label. This is the text that is displayed when the requirement is not met.
        /// </summary>
        public string NegativeLabel { get; set; }

        /// <summary>
        /// Gets or sets the positive label. This is the text that is displayed when the requirement is met.
        /// </summary>
        public string PositiveLabel { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement check.
        /// </summary>
        public string RequirementCheckType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement type's "Does Not Meet" workflow should auto-initiate.
        /// </summary>
        public bool ShouldAutoInitiateDoesNotMeetWorkflow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement type's "Warning" workflow should auto-initiate.
        /// </summary>
        public bool ShouldAutoInitiateWarningWorkflow { get; set; }

        /// <summary>
        /// Gets or sets the SQL expression.
        /// </summary>
        public string SqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the warning Rock.Model.DataView.
        /// </summary>
        public ListItemBag WarningDataView { get; set; }

        /// <summary>
        /// Gets or sets the warning label.
        /// </summary>
        public string WarningLabel { get; set; }

        /// <summary>
        /// Gets or sets the warning SQL expression.
        /// </summary>
        public string WarningSqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the text for the "Warning" workflow link.
        /// </summary>
        public string WarningWorkflowLinkText { get; set; }

        /// <summary>
        /// Gets or sets "Warning" workflow type.
        /// </summary>
        public ListItemBag WarningWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the SQL help text.
        /// </summary>
        /// <value>
        /// The SQL help text.
        /// </value>
        public string SqlHelpHTML { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can administrate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
