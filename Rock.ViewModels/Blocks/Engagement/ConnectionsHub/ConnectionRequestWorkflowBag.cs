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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a persisted workflow that was launched from a connection request,
    /// as displayed in the Workflows list of the request detail panel.
    /// </summary>
    public class ConnectionRequestWorkflowBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key of the ConnectionRequestWorkflow row. Used as the client row key.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow type that was launched.
        /// </summary>
        public string WorkflowTypeName { get; set; }

        /// <summary>
        /// Gets or sets the trigger that launched the workflow (Manual, Status Changed, etc.).
        /// </summary>
        public ConnectionWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the names of the workflow's currently active activities. Empty when the workflow has completed.
        /// </summary>
        public List<string> ActiveActivityNames { get; set; }

        /// <summary>
        /// Gets or sets the date and time the workflow was activated.
        /// </summary>
        public DateTimeOffset? ActivatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow has a completed date.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the raw workflow status value, shown when a completed workflow has a custom terminal status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the URL to open when the row is clicked: the Workflow Entry page when the current person
        /// has an active entry form, otherwise the Workflow Detail page. Null when the relevant page setting is blank.
        /// </summary>
        public string WorkflowUrl { get; set; }
    }
}
