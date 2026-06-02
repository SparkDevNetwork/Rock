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

using Rock.Enums.CheckIn;
using Rock.Model;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The editable detail of a check-in area, exchanged with the Area editor in the right pane. Also used as the
    /// payload shape for both saving an existing area and creating a new one (the server treats <see cref="IdKey"/>
    /// as the discriminator).
    /// </summary>
    public class AreaDetailBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the underlying area group type, or null/empty for a new
        /// (unsaved) area.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the area name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether checking into a group of this area should be blocked when the
        /// person already has an attendance record for the same scheduled service.
        /// </summary>
        public bool IsConcurrentCheckInPrevented { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the check-in setup type whose configuration this area inherits,
        /// or null when the area inherits no setup type.
        /// </summary>
        public Guid? InheritedGroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the rule that decides what happens when someone tries to check in to a group of this area
        /// (None, Add on Check-in, Already Enrolled in Group).
        /// </summary>
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the matching logic applied when <see cref="AttendanceRule"/> is
        /// <see cref="AttendanceRule.AlreadyEnrolledInGroup"/>. Ignored otherwise.
        /// </summary>
        public AlreadyEnrolledMatchingLogic AlreadyEnrolledMatchingLogic { get; set; }

        /// <summary>
        /// Gets or sets where check-in labels should be printed for this area.
        /// </summary>
        public PrintTo AttendancePrintTo { get; set; }

        /// <summary>
        /// Gets or sets the area's current attribute values, in the public string form expected by
        /// the AttributeValuesContainer. Keyed by attribute key. The matching schema is resolved
        /// client-side from <c>InitializationBox.InheritedAttributesByGuid</c> based on the
        /// currently-selected inherit-from setup type.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the check-in labels attached to this area, in display order.
        /// </summary>
        public List<CheckInLabelBag> CheckInLabels { get; set; }

        /// <summary>
        /// Gets or sets the classic check-in labels attached to this area, in display order.
        /// </summary>
        public List<ClassicCheckInLabelBag> ClassicCheckInLabels { get; set; }
    }
}
