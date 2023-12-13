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
    /// The results of the RunNotMetWorkflow and RunWarningWorkflow API actions of
    /// the GroupMemberRequirementCard control. This tells the front end what it
    /// should do now that the workflow has been handled.
    /// </summary>
    public class GroupMemberRequirementCardRunWorkflowResultsBag
    {
        /// <summary>
        /// If set, show this string as an alert
        /// </summary>
        public string Alert { get; set; }

        /// <summary>
        /// If set, will open this URL in a new window/tab
        /// </summary>
        public string Open { get; set; }

        /// <summary>
        /// If set, will navigate the window/tab to this URL
        /// </summary>
        public string GoTo { get; set; }
    }
}
