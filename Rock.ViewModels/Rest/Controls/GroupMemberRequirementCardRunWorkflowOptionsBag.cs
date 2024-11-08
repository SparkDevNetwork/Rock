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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the RunNotMetWorkflow and RunWarningWorkflow
    /// API actions of the GroupMemberRequirementCard control.
    /// </summary>
    public class GroupMemberRequirementCardRunWorkflowOptionsBag
    {
        /// <summary>
        /// Identifier for the GroupMember
        /// </summary>
        public Guid GroupMemberGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Identifier for the GroupMemberRequirement
        /// </summary>
        public Guid GroupMemberRequirementGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Identifier for the GroupRequirement
        /// </summary>
        public Guid GroupRequirementGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// A URL or page identifier for the page where the workflow should be run
        /// </summary>
        public string WorkflowEntryLinkedPageValue { get; set; } = String.Empty;
    }
}
