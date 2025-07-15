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
using Rock.ViewModels.Workflow;

namespace Rock.ViewModels.Blocks.WorkFlow.WorkflowEntry
{
    /// <summary>
    /// The initial configuration options for the WorkflowEntry block when used
    /// on a Web site.
    /// </summary>
    public class WorkflowEntryOptionsBag
    {
        /// <summary>
        /// If <c>true</c> then a captcha is required in order to submit any
        /// component data back to the server.
        /// </summary>
        public bool IsCaptchaEnabled { get; set; }

        /// <summary>
        /// The initial action details to display.
        /// </summary>
        public InteractiveActionBag InitialAction { get; set; }
    }
}
