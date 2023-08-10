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

using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.StreakTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class StreakTypeDetailOptionsBag
    {
        /// <summary>
        /// Streak Occurrence Frequency list
        /// </summary>
        public List<ListItemBag> StreakOccurrenceFrequencies { get; set; }

        /// <summary>
        /// List of Represents of the attendance association of a StreakType.
        /// </summary>
        public List<ListItemBag> StreakStructureTypes { get; set; }

        /// <summary>
        /// The List of guids of the group type for check in config linked type
        /// </summary>
        public List<Guid> AttendanceCheckInConfigGroupTypesGuids { get; set; }
    }
}
