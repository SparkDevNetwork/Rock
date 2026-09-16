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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrantList
{
    /// <summary>
    /// The placement state of a single registrant for one registration
    /// template placement. Rendered as a button in the Placements column of
    /// the Registration Instance - Registrant List block.
    /// </summary>
    public class RegistrantPlacementBag
    {
        /// <summary>
        /// Gets or sets the identifier of the registration template placement
        /// this entry belongs to.
        /// </summary>
        public int PlacementId { get; set; }

        /// <summary>
        /// Gets or sets the number of placement groups the registrant has
        /// been placed in for this placement.
        /// </summary>
        public int GroupCount { get; set; }

        /// <summary>
        /// Gets or sets the names of the placement groups the registrant has
        /// been placed in for this placement.
        /// </summary>
        public List<string> GroupNames { get; set; }
    }
}
