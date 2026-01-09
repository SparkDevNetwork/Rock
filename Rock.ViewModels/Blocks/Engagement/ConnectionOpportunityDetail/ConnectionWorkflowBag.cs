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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityDetail
{
    /// <summary>
    /// Minimal bag describing a workflow configuration for a Connection Opportunity.
    /// </summary>
    public class ConnectionWorkflowBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public System.Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the workflow type as a ListItemBag.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the trigger type.
        /// </summary>
        public int TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value formatted as "|Primary|Secondary|".
        /// </summary>
        public string QualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the connection status ID used to filter workflow manual triggers.
        /// </summary>
        public int? ManualTriggerFilterConnectionStatusId { get; set; }

        /// <summary>
        /// Gets or sets the age classification that this workflow configuration applies to.
        /// </summary>
        public int AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the ID of the data view used to include records.
        /// </summary>
        public ListItemBag IncludeDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the data view used to exclude records.
        /// </summary>
        public ListItemBag ExcludeDataViewId { get; set; }
    }
}
