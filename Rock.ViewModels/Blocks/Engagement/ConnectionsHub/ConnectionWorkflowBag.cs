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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a connection workflow that can be launched from the Connections Hub.
    /// </summary>
    public class ConnectionWorkflowBag
    {
        /// <summary>
        /// Gets or sets the workflow item details including the workflow type name and connection workflow identifier.
        /// </summary>
        public ListItemBag ListItemBag { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the Connection Opportunity this workflow belongs to,
        /// or <c>null</c> if this is a Connection Type-level workflow.
        /// </summary>
        public Guid? ConnectionOpportunityGuid { get; set; }
    }
}
