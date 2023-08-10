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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.StreakDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class StreakBag : EntityBagBase
    {
        /// <summary>
        /// The current number of non excluded occurrences attended in a row
        /// </summary>
        public int CurrentStreakCount { get; set; }

        /// <summary>
        /// The date that the current streak began
        /// </summary>
        public DateTime? CurrentStreakStartDate { get; set; }

        /// <summary>
        /// Gets or sets the System.DateTime when the person was enrolled in the streak type.
        /// This is not the Streak Type start date.
        /// </summary>
        public DateTime EnrollmentDate { get; set; }

        /// <summary>
        /// Gets or sets the IsActive.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The longest number of non excluded occurrences attended in a row
        /// </summary>
        public int LongestStreakCount { get; set; }

        /// <summary>
        /// The date the longest streak ended
        /// </summary>
        public DateTime? LongestStreakEndDate { get; set; }

        /// <summary>
        /// The date the longest streak began
        /// </summary>
        public DateTime? LongestStreakStartDate { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Location.
        /// </summary>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.PersonAlias.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.StreakType.
        /// </summary>
        public ListItemBag StreakType { get; set; }
    }
}
