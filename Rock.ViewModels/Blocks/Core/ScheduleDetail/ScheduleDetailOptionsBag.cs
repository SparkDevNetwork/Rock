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

namespace Rock.ViewModels.Blocks.Core.ScheduleDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class ScheduleDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the exclusions.
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public List<ScheduleExclusionBag> Exclusions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has schedule warning.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has schedule warning; otherwise, <c>false</c>.
        /// </value>
        public bool HasScheduleWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can delete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can delete; otherwise, <c>false</c>.
        /// </value>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has attendance.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has attendance; otherwise, <c>false</c>.
        /// </value>
        public bool HasAttendance { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string HelpText { get; set; }
    }
}
