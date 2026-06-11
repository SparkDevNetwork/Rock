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

using Rock.Enums.Connection;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOpportunityDetail
{
    /// <summary>
    /// The additional configuration options for the Connection Opportunity Detail block.
    /// </summary>
    public class ConnectionOpportunityDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reorder column should be visible.
        /// </summary>
        /// <value>
        /// Whether the reorder column is visible.
        /// </value>
        public bool? IsReOrderColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Future Follow-Up is enabled on the associated Connection Type.
        /// </summary>
        /// <remarks>
        /// This is used to determine whether or not to show the Future Follow-Up Connection State option when
        /// configuring workflow triggers for the Connection Opportunity.
        /// </remarks>
        public bool IsFutureFollowupEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Group Placement is enabled on the associated Connection Type.
        /// </summary>
        public bool IsGroupPlacementEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Due Date Calculation Mode that is set on the associated Connection Type.
        /// </summary>
        /// <remarks>
        /// This is used to determine whether to render controls that set "RequestDueDateOffsetInDays" and "RequestDueSoonOffsetInDays"
        /// on the ConnectionOpportunity
        /// </remarks>
        public DueDateCalculationMode? DueDateCalculationMode { get; set; }
    }
}
