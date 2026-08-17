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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingParticipant
{
    /// <summary>
    /// The configuration passed to the group member requirements container control. The
    /// control loads the requirement statuses itself; these values tell it which group
    /// member to load and where to launch requirement workflows.
    /// </summary>
    public class FundraisingParticipantRequirementsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the opportunity group.
        /// </summary>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the participant's group role.
        /// </summary>
        public Guid? GroupRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the participant group member.
        /// </summary>
        public Guid? GroupMemberGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the participant person.
        /// </summary>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the linked page value used to launch a requirement workflow.
        /// </summary>
        public string WorkflowEntryLinkedPageValue { get; set; }
    }
}
