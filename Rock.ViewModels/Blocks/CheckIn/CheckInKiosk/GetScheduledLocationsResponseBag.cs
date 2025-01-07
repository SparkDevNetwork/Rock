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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// The object sent in response to the GetScheduledLocations block action.
    /// </summary>
    public class GetScheduledLocationsResponseBag
    {
        /// <summary>
        /// A list of schedules to to be displayed for selection.
        /// </summary>
        public List<CheckInItemBag> Schedules { get; set; }

        /// <summary>
        /// The list of locations (and associated group) that can be scheduled.
        /// </summary>
        public List<ScheduledLocationBag> ScheduledLocations { get; set; }
    }
}
