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
    }
}
