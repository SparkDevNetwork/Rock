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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The family shown in the main entry pane: its rendered header, its members, and the attendance roster state.
    /// </summary>
    public class RapidAttendanceEntryFamilyBag
    {
        /// <summary>
        /// Gets or sets the family's unique identifier.
        /// </summary>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the family's name.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the family header rendered from the Family Header Template block setting.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the family members, ordered adults first. Members drive both the attendance roster and the
        /// person navigation pills.
        /// </summary>
        public List<RapidAttendanceEntryPersonBag> Members { get; set; }

        /// <summary>
        /// Gets or sets the people with a "Can check-in" relationship to a family member. They are listed for
        /// attendance only. Null when attendance is not being taken or the Show Can Check-In Relationships setting
        /// is disabled.
        /// </summary>
        public List<RapidAttendanceEntryPersonBag> CanCheckInGuests { get; set; }

        /// <summary>
        /// Gets or sets the number of people attended for the session's occurrence, across all families. Null when
        /// no valid session was supplied.
        /// </summary>
        public int? AttendanceCount { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunities offered for this family: the configured Connection Type's
        /// active opportunities the operator may view, filtered to those available for the session's campus
        /// (falling back to the family's). Null when no type is configured or nothing qualifies, hiding the section.
        /// </summary>
        public List<ListItemBag> ConnectionOpportunityItems { get; set; }
    }
}
