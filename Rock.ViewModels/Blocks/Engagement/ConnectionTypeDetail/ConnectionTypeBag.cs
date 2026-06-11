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

using System.Collections.Generic;

using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Enums.Connection;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// The item details for the Connection Type Detail block.
    /// </summary>
    public class ConnectionTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the connection request detail page and route.
        /// </summary>
        public PageRouteValueBag ConnectionRequestDetailPage { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines how the due date for a request is calculated.
        /// </summary>
        public DueDateCalculationMode DueDateCalculationMode { get; set; }

        /// <summary>
        /// Flags that specify which optional features are enabled for this connection type.
        /// </summary>
        public EnabledFeatureFlags EnabledFeatures { get; set; }

        /// <summary>
        /// Flags that specify which request views are enabled for this connection type.
        /// </summary>
        public EnabledViewFlags EnabledViews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether full activity lists are enabled.
        /// </summary>
        public bool EnableFullActivityList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether future follow-ups are enabled.
        /// </summary>
        public bool EnableFutureFollowup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether request security is enabled.
        /// </summary>
        public bool EnableRequestSecurity { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Determines whether requests must move through statuses in a defined sequence.
        /// </summary>
        public bool IsSequentialStatusEnforced { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Number of days added to the calculated due date for a request.
        /// </summary>
        public int? RequestDueDateOffsetInDays { get; set; }

        /// <summary>
        /// Number of days before the due date when a request is considered "due soon."
        /// </summary>
        public int? RequestDueSoonOffsetInDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this connection type requires a placement group to connect.
        /// </summary>
        public bool RequiresPlacementGroupToConnect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all Connection Request due dates and due soon dates under this connection type should be recalculated during post save.
        /// </summary>
        public bool ShouldRecalculateRequestDueAndDueSoonDates { get; set; }

        /// <summary>
        /// Gets or sets the connection type attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> ConnectionTypeAttributes { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> ConnectionOpportunityAttributes { get; set; }

        /// <summary>
        /// Gets or sets the connection request attributes.
        /// </summary>
        public List<PublicEditableAttributeBag> ConnectionRequestAttributes { get; set; }

        /// <summary>
        /// Gets or sets the activity types.
        /// </summary>
        public List<ConnectionActivityTypeBag> ActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets the statuses.
        /// </summary>
        public List<ConnectionStatusBag> Statuses { get; set; }

        /// <summary>
        /// Gets or sets the connection type sources.
        /// </summary>
        public List<ConnectionTypeSourceBag> Sources { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        public List<ConnectionWorkflowBag> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the connection type additional settings.
        /// </summary>
        public ConnectionTypeAdditionalSettingsBag AdditionalSettings { get; set; }
    }
}
