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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// Everything the operator entered on the entry screen, submitted as one save.
    /// </summary>
    public class RapidAttendanceEntrySaveBag
    {
        /// <summary>
        /// Gets or sets the session attendance is recorded under. Null when attendance is not being taken.
        /// </summary>
        public RapidAttendanceEntrySessionBag Session { get; set; }

        /// <summary>
        /// Gets or sets the full attendance roster as shown when saving. Unchecked people have their attendance
        /// removed. Null when attendance is not being taken.
        /// </summary>
        public List<RapidAttendanceEntryAttendanceBag> Attendances { get; set; }

        /// <summary>
        /// Gets or sets the per-person entries: prayer requests, notes, workflows, and connection opportunities.
        /// </summary>
        public List<RapidAttendanceEntryPersonInputBag> PersonInputs { get; set; }
    }
}
