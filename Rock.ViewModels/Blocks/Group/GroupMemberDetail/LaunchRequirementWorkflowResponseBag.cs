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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The result of launching a requirement's does-not-meet or warning
    /// workflow from the Group Member Detail block.
    /// </summary>
    public class LaunchRequirementWorkflowResponseBag
    {
        /// <summary>
        /// Gets or sets the message shown to the user. Null when the client
        /// should navigate straight to the workflow entry page.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the workflow entry page URL. With a message the
        /// client opens it in a new tab after the message; without one it
        /// navigates directly. Null when there is nothing to open.
        /// </summary>
        public string WorkflowEntryUrl { get; set; }
    }
}
