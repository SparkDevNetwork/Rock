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

namespace Rock.ViewModels.Blocks.WorkFlow.WorkflowLaunch
{
    /// <summary>
    /// A single entity preview item shown in the Workflow Launch block.
    /// </summary>
    public class WorkflowLaunchItemBag
    {
        /// <summary>
        /// Gets or sets the primary line of text identifying the entity.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the optional secondary line of text (for example, the related
        /// group name or an "EntityType Id: N" description).
        /// </summary>
        public string SubText { get; set; }
    }
}
