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

namespace Rock.ViewModels.Blocks.Group.GroupAttendanceDetail
{
    /// <summary>
    /// A bag that contains the attendance information.
    /// </summary>
    public class GroupAttendanceDetailAttendanceBag
    {
        /// <summary>
        /// Gets or sets the Person's unique identifier.
        /// </summary>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Person's nick name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the Person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Indicates whether the Person has attended.
        /// </summary>
        public bool DidAttend { get; set; }

        /// <summary>
        /// Gets or sets the Campus that a Person's family belongs to.
        /// <para>Enables filtering attendees by Campus.</para>
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the item template used for rendering the Attendance in the Group Attendance Detail block.
        /// </summary>
        public string ItemTemplate { get; set; }
    }
}
