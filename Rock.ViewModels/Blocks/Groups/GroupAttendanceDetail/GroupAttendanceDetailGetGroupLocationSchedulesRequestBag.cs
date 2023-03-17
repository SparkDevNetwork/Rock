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

namespace Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail
{
    /// <summary>
    /// A bag that contains the get group location schedules request information.
    /// </summary>
    public class GroupAttendanceDetailGetGroupLocationSchedulesRequestBag
    {
        /// <summary>
        /// GUID of the group the schedules are part of.
        /// </summary>
        public Guid? GroupGuid { get; set; }

        /// <summary>
        /// GUID of the location the schedules are part of.
        /// </summary>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Will only include schedules that apply to a specific date (time ignored).
        /// </summary>
        public DateTimeOffset? Date { get; set; }
    }
}
