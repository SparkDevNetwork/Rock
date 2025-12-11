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

using System;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Represents a workflow option for a benevolence request.
    /// </summary>
    public class BenevolenceRequestWorkflowBag
    {
        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Gets or sets the workflow type unique identifier.
        /// </summary>
        public Guid TypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the workflow type name.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the trigger type for the workflow.
        /// </summary>
        public BenevolenceWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the URL to launch the workflow.
        /// </summary>
        public string LaunchUrl { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the workflow icon.
        /// </summary>
        public string iconCssClass { get; set; }
    }
}
