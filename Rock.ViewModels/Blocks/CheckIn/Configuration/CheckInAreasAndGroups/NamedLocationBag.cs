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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// A single named location attached to a check-in group. Used by both the Main and Overflow grids on the
    /// Group editor.
    /// </summary>
    public class NamedLocationBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the underlying location.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name for the location, formatted as the full hierarchical name path (e.g.
        /// "Main Campus > Children's Wing > Nursery").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the campus this location belongs to, resolved through the location tree.
        /// <c>null</c> when the location sits under no campus root. The client uses this to filter the displayed
        /// grids by the active campus slicer without a server round-trip.
        /// </summary>
        public Guid? CampusGuid { get; set; }
    }
}
