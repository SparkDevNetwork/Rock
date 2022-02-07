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

using System;

namespace Rock.Model
{
    /// <summary>
    /// Helper class for grouping attendance records associated into logical occurrences based on
    /// a given schedule
    /// </summary>
    public class ScheduleOccurrence
    {
        /// <summary>
        /// Gets or sets the logical occurrence date of the occurrence
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the logical start date/time ( only used for ordering )
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        /// <value>
        /// The name of the schedule.
        /// </value>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the location path.
        /// </summary>
        /// <value>
        /// The location path.
        /// </value>
        public string LocationPath { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the did attend count.
        /// </summary>
        /// <value>
        /// The did attend count.
        /// </value>
        public int DidAttendCount { get; set; }

        /// <summary>
        /// Gets or sets the did not occur count.
        /// </summary>
        /// <value>
        /// The did not occur count.
        /// </value>
        public int DidNotOccurCount { get; set; }

        /// <summary>
        /// Gets a value indicating whether attendance has been entered for this occurrence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance entered]; otherwise, <c>false</c>.
        /// </value>
        public bool AttendanceEntered
        {
            get
            {
                return DidAttendCount > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether occurrence did not occur for the selected
        /// start time. This is determined by not having any attendance records with 
        /// a 'DidAttend' value, and at least one attendance record with 'DidNotOccur'
        /// value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [did not occur]; otherwise, <c>false</c>.
        /// </value>
        public bool DidNotOccur
        {
            get
            {
                return DidAttendCount <= 0 && DidNotOccurCount > 0;
            }
        }

        /// <summary>
        /// Gets the attendance percentage.
        /// </summary>
        /// <value>
        /// The percentage.
        /// </value>
        public double Percentage
        {
            get
            {
                if ( TotalCount > 0 )
                {
                    return DidAttendCount / ( double ) TotalCount;
                }
                else
                {
                    return 0.0d;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleOccurrence" /> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="scheduleName">Name of the schedule.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="locationName">Name of the location.</param>
        /// <param name="locationPath">The location path.</param>
        /// ,
        public ScheduleOccurrence( DateTime date, TimeSpan startTime, int? scheduleId = null, string scheduleName = "", int? locationId = null, string locationName = "", string locationPath = "" )
        {
            Date = date;
            StartTime = startTime;
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            LocationId = locationId;
            LocationName = locationName;
            LocationPath = locationPath;
        }
    }
}
