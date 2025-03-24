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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results of the GetConfig API action of the GroupMemberRequirementCard control.
    /// </summary>
    public class GroupMemberRequirementCardGetConfigResultsBag
    {
        /// <summary>
        /// Control for manually marking a requirement as met
        /// </summary>
        public GroupMemberRequirementCardSubControlConfigBag ManualRequirementControl { get; set; } = new GroupMemberRequirementCardSubControlConfigBag();

        /// <summary>
        /// Control for overriding the need for a requirement to be met
        /// </summary>
        public GroupMemberRequirementCardSubControlConfigBag OverrideRequirementControl { get; set; } = new GroupMemberRequirementCardSubControlConfigBag();

        /// <summary>
        /// Control for running a workflow when requirement is not met
        /// </summary>
        public GroupMemberRequirementCardSubControlConfigBag NotMetWorkflowControl { get; set; } = new GroupMemberRequirementCardSubControlConfigBag();

        /// <summary>
        /// Control for running a workflow when requirement has a warning
        /// </summary>
        public GroupMemberRequirementCardSubControlConfigBag WarningWorkflowControl { get; set; } = new GroupMemberRequirementCardSubControlConfigBag();

        /// <summary>
        /// Whether this requirement has been overridden as complete or not
        /// </summary>
        public bool IsOverridden { get; set; } = false;

        /// <summary>
        /// If overridden, this contains the display name of the person who overrode the requirement
        /// </summary>
        public string OverriddenBy { get; set; } = string.Empty;

        /// <summary>
        /// If overridden, this contains the display date of when the override took place
        /// </summary>
        public string OverriddenAt { get; set; } = string.Empty;

        /// <summary>
        /// The message declaring the status of this requirement
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The summary of what the requirement is
        /// </summary>
        public string Summary { get; set; } = string.Empty;


    }
}
