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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Administration.Security
{
    /// <summary>
    /// The permission data for a single security action. Returned both during
    /// initialization and after any change so the grids can refresh.
    /// </summary>
    public class SecurityActionDataBag
    {
        /// <summary>
        /// Gets or sets the rules defined directly on this entity for the action.
        /// </summary>
        public GridDataBag ItemRules { get; set; }

        /// <summary>
        /// Gets or sets the rules inherited from parent authorities for the action.
        /// </summary>
        public GridDataBag ParentRules { get; set; }

        /// <summary>
        /// Gets or sets the warning shown when no "All Users" rule applies,
        /// describing whether non-matching people are allowed or denied by default.
        /// A <c>null</c> value indicates the warning should not be shown.
        /// </summary>
        public string NoMatchMessage { get; set; }

        /// <summary>
        /// Gets or sets the warning shown when a circular reference was detected
        /// in the entity's parent authority chain, meaning the inherited rules
        /// shown may be incomplete. A <c>null</c> value indicates the warning
        /// should not be shown.
        /// </summary>
        public string CircularReferenceMessage { get; set; }
    }
}
