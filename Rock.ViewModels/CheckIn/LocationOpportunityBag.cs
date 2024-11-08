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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines a single location that can be used during check-in.
    /// </summary>
    public class LocationOpportunityBag : CheckInItemBag
    {
        /// <summary>
        /// Gets or sets the maximum capacity of the location.
        /// </summary>
        /// <value>The maximum capacity; or <c>null</c> if not available.</value>
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets the number of spots currently filled in the location.
        /// </summary>
        /// <value>The number of spots filled.</value>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifiers that this location is valid
        /// for.
        /// </summary>
        /// <value>The schedule identifiers.</value>
        public List<string> ScheduleIds { get; set; }
    }
}
