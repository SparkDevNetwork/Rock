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

namespace Rock.Utility.Settings.DataAutomation
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateFamilyCampus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFamilyCampus"/> class.
        /// </summary>
        public UpdateFamilyCampus()
        {
            IsMostFamilyAttendanceEnabled = true;
            MostFamilyAttendancePeriod = 90;

            IsMostFamilyGivingEnabled = true;
            MostFamilyGivingPeriod = 90;

            IsIgnoreIfManualUpdateEnabled = true;
            IgnoreIfManualUpdatePeriod = 90;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is most family attendance enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is most family attendance enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMostFamilyAttendanceEnabled { get; set; }

        /// <summary>
        /// Gets or sets the most family attendance period.
        /// </summary>
        /// <value>
        /// The most family attendance period.
        /// </value>
        public int MostFamilyAttendancePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is most family giving enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is most family giving enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMostFamilyGivingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the most family giving period.
        /// </summary>
        /// <value>
        /// The most family giving period.
        /// </value>
        public int MostFamilyGivingPeriod { get; set; }

        /// <summary>
        /// Gets or sets the most attendance or giving.
        /// </summary>
        /// <value>
        /// The most attendance or giving.
        /// </value>
        public CampusCriteria MostAttendanceOrGiving { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ignore if manual update enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ignore if manual update enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIgnoreIfManualUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the ignore if manual update period.
        /// </summary>
        /// <value>
        /// The ignore if manual update period.
        /// </value>
        public int IgnoreIfManualUpdatePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ignore campus changes enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ignore campus changes enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIgnoreCampusChangesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the ignore campus changes.
        /// </summary>
        /// <value>
        /// The ignore campus changes.
        /// </value>
        public List<IgnoreCampusChangeItem> IgnoreCampusChanges { get; set; }

        /// <summary>
        /// Gets or sets the schedules to exclude from the attendance criteria
        /// </summary>
        public List<int> ExcludeSchedules { get; set; }
    }

    /// <summary>
    /// Enum for how campus should be updated if giving and campus criteria indicate different campuses
    /// </summary>
    public enum CampusCriteria
    {
        /// <summary>
        /// Ignore
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Use giving
        /// </summary>
        UseGiving = 1,

        /// <summary>
        /// Use attendance
        /// </summary>
        UseAttendance = 2,

        /// <summary>
        /// Use one with highest frequency
        /// </summary>
        UseHighestFrequency = 3,
    }

    /// <summary>
    /// Helper class for setting campus update exclusions
    /// </summary>
    public class IgnoreCampusChangeItem
    {
        /// <summary>
        /// Gets or sets from campus.
        /// </summary>
        /// <value>
        /// From campus.
        /// </value>
        public int FromCampus { get; set; }

        /// <summary>
        /// Gets or sets to campus.
        /// </summary>
        /// <value>
        /// To campus.
        /// </value>
        public int ToCampus { get; set; }

        /// <summary>
        /// Gets or sets the based on.
        /// </summary>
        /// <value>
        /// The based on.
        /// </value>
        public CampusCriteria? BasedOn { get; set; }
    }
    
}
