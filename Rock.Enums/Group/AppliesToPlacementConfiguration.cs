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

namespace Rock.Enums.Group
{
    /// <summary>
    /// Represents the different modes Person Filters can apply to when configuring Group Placements.
    /// </summary>
    public enum AppliesToPlacementConfiguration
    {
        /// <summary>
        /// Person Filters should only be applied in the "People To Place" Panel.
        /// </summary>
        PeopleToPlace = 0,

        /// <summary>
        /// Person Filters should only be applied in the "Destination Groups" Panel.
        /// </summary>
        DestinationGroupMembers = 1,

        /// <summary>
        /// Person Filters should be applied to all people.
        /// </summary>
        AllPeople = 2,
    }
}
