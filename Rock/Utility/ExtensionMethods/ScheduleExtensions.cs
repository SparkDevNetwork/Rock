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
using System.Linq;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Rock.Model.Schedule Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Sorts the list of Schedules by the day/time they are scheduled (then by Name, Id). For example: Saturday 4pm, Saturday 6pm, Sunday 9am, Sunday 11am, Sunday 1pm
        /// </summary>
        /// <param name="scheduleList">The schedule list.</param>
        /// <returns></returns>
        public static List<Schedule> OrderByNextScheduledDateTime( this List<Schedule> scheduleList )
        {
            // Calculate the Next Start Date Time based on the start of the week so that schedule columns are in the correct order
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );
            List<Schedule> sortedScheduleList = scheduleList
                .OrderBy( a => a.GetNextStartDateTime( occurrenceDate ) )
                .ThenBy( a => a.Name )
                .ThenBy( a => a.Id )
                .ToList();

            return sortedScheduleList;
        }
    }
}
