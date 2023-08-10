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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Workflow.WorkflowTriggerDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    /// <seealso cref="Rock.ViewModels.Utility.IValidPropertiesBox" />
    public class WorkflowTriggerDetailBox : BlockBox, IValidPropertiesBox
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public WorkflowTriggerBag Entity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value><c>true</c> if this instance is editable; otherwise, <c>false</c>.</value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets the valid properties.
        /// </summary>
        /// <value>The valid properties.</value>
        public List<string> ValidProperties { get; set; }

        /// <summary>
        /// Gets or sets the readonly notification message.
        /// </summary>
        /// <value>
        /// The readonly notification message.
        /// </value>
        public string ReadonlyNotificationMessage { get; set; }
    }
}
