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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The entries the operator typed for one individual before saving.
    /// </summary>
    public class RapidAttendanceEntryPersonInputBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the person the entries are for.
        /// </summary>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the prayer request entered for the person, or null when none was entered.
        /// </summary>
        public RapidAttendanceEntryPrayerRequestBag PrayerRequest { get; set; }

        /// <summary>
        /// Gets or sets the note entered for the person, or null when none was entered.
        /// </summary>
        public RapidAttendanceEntryNoteBag Note { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the checked workflow types, launched for the person on save.
        /// </summary>
        public List<Guid> WorkflowTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the checked connection opportunities. A connection request is
        /// created for each with the person as the requestor.
        /// </summary>
        public List<Guid> ConnectionOpportunityGuids { get; set; }
    }
}
