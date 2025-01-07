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
    /// The response data returned by the GetCurrentAttendance block action.
    /// </summary>
    public class GetCurrentAttendanceResponseBag
    {
        /// <summary>
        /// All the attendance records related to the kiosk.
        /// </summary>
        public List<ActiveAttendanceBag> Attendance { get; set; }

        /// <summary>
        /// The group definitions for all groups related to the kiosk.
        /// </summary>
        public List<GroupOpportunityBag> Groups { get; set; }

        /// <summary>
        /// The location definitions for all locations related to the kiosk.
        /// </summary>
        public List<LocationStatusItemBag> Locations { get; set; }
    }
}
