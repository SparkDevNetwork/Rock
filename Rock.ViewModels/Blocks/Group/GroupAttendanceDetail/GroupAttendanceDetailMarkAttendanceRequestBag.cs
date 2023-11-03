﻿// <copyright>
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
    /// A bag that contains the mark attendance request information.
    /// </summary>
    public class GroupAttendanceDetailMarkAttendanceRequestBag
    {
        /// <summary>
        /// Gets or sets the attendance occurrence unique identifier.
        /// </summary>
        public Guid AttendanceOccurrenceGuid { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Person attended.
        /// </summary>
        public bool? DidAttend { get; set; }
    }
}
