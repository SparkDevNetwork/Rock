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

using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Represents a person and all the check-in related values that will be
    /// used during the check-in process.
    /// </summary>
    internal class Attendee : AttendeeBag
    {
        /// <summary>
        /// Gets or sets the recent attendances for this individual. This will
        /// always include todays attendance and may include attendance records
        /// as far back as the AutoSelect feature is configured for.
        /// </summary>
        /// <value>The recent attendances for this individual.</value>
        public List<RecentAttendance> RecentAttendances { get; set; }

        /// <summary>
        /// Gets or sets the opportunities that are available to be selected from.
        /// </summary>
        /// <value>The opportunities that are available to be selected from.</value>
        public OpportunityCollection Opportunities { get; set; }

        /// <summary>
        /// Gets or sets the last date and time the person checked in.
        /// </summary>
        /// <value>The last date and time the person checked in.</value>
        public DateTime? LastCheckIn { get; set; }
    }
}
