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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// A person matched by the search sidebar, with the family context shown on the result card.
    /// </summary>
    public class RapidAttendanceEntrySearchResultBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the matched person.
        /// </summary>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person's primary family.
        /// </summary>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the person's nick name and last name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person's age in years, or null when unknown.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's primary family.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the names of the other family members, formatted for display.
        /// </summary>
        public string FamilyMemberNames { get; set; }

        /// <summary>
        /// Gets or sets the name of the family's campus. Null when the organization has a single campus, since the
        /// label would add nothing.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the person's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's mobile phone number, formatted for display.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the family's home address as formatted multi-line HTML.
        /// </summary>
        public string AddressHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person's record is active. Inactive people render
        /// de-emphasized.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is marked attended for the session's saved occurrence.
        /// </summary>
        public bool IsAttended { get; set; }
    }
}
