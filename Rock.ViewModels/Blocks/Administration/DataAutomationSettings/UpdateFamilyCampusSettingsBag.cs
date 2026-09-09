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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Settings that control when a family's campus is automatically updated.
    /// </summary>
    public class UpdateFamilyCampusSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic family campus updating is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the most-family-attendance criteria is enabled.
        /// </summary>
        public bool IsMostFamilyAttendanceEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the most-family-attendance criteria.
        /// </summary>
        public int? MostFamilyAttendancePeriod { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of times a family must attend a campus before a campus change is triggered.
        /// </summary>
        public int? TimesToTriggerCampusChange { get; set; }

        /// <summary>
        /// Gets or sets the schedules excluded from the attendance criteria. Each value is a schedule unique identifier.
        /// </summary>
        public List<ListItemBag> ExcludeSchedules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the most-family-giving criteria is enabled.
        /// </summary>
        public bool IsMostFamilyGivingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the most-family-giving criteria.
        /// </summary>
        public int? MostFamilyGivingPeriod { get; set; }

        /// <summary>
        /// Gets or sets the tie-breaker used when the campuses calculated from attendance and giving differ.
        /// The value is the integer value of the campus criteria.
        /// </summary>
        public string MostAttendanceOrGiving { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ignore-if-manual-update criteria is enabled.
        /// </summary>
        public bool IsIgnoreIfManualUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the ignore-if-manual-update criteria.
        /// </summary>
        public int? IgnoreIfManualUpdatePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether specific campus changes should be ignored.
        /// </summary>
        public bool IsIgnoreCampusChangesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the collection of campus changes to ignore.
        /// </summary>
        public List<IgnoreCampusChangeBag> IgnoreCampusChanges { get; set; }
    }
}
