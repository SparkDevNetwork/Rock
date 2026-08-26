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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The recalculated group requirement state returned to the client when
    /// the person, the role, or the Refresh Requirements button changes it.
    /// </summary>
    public class RefreshRequirementsResponseBag
    {
        /// <summary>
        /// Gets or sets the refreshed inline requirement alerts.
        /// </summary>
        public List<GroupMemberRequirementAlertBag> RequirementAlerts { get; set; }

        /// <summary>
        /// Gets or sets the requirement calculation error details, one
        /// "{type}: {error}" per line. Null when every calculation
        /// succeeded.
        /// </summary>
        public string CalculationErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requirement interaction
        /// (override, mark as met) is disabled for the recalculated state.
        /// </summary>
        public bool IsRequirementInteractionDisabled { get; set; }
    }
}
