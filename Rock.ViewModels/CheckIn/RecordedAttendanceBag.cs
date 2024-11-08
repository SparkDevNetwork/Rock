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
    /// Details about a newly recorded attendance record.
    /// </summary>
    public class RecordedAttendanceBag
    {
        /// <summary>
        /// Gets or sets the attendance details.
        /// </summary>
        /// <value>The attendance details.</value>
        public AttendanceBag Attendance { get; set; }

        /// <summary>
        /// Gets or sets the in progress achievements.
        /// </summary>
        /// <value>The in progress achievements.</value>
        public List<AchievementBag> InProgressAchievements { get; set; }

        /// <summary>
        /// Gets or sets the achievements that were completed before the new
        /// attendance record was created.
        /// </summary>
        /// <value>The previously completed achievements.</value>
        public List<AchievementBag> PreviouslyCompletedAchievements { get; set; }

        /// <summary>
        /// Gets or sets the achievements that were just completed by this
        /// new attendance record.
        /// </summary>
        /// <value>The just completed achievements.</value>
        public List<AchievementBag> JustCompletedAchievements { get; set; }
    }
}
