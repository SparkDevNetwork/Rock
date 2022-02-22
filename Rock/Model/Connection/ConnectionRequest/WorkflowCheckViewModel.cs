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

namespace Rock.Model
{
    /// <summary>
    /// Workflow Check View Model
    /// </summary>
    public sealed class WorkflowCheckViewModel
    {
        /// <summary>
        /// Converts to statusname.
        /// </summary>
        public string ToStatusName { get; set; }

        /// <summary>
        /// Gets or sets the name of from status.
        /// </summary>
        public string FromStatusName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [does cause workflows].
        /// </summary>
        public bool DoesCauseWorkflows { get; set; }
    }
}
