// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class for storing the current attendance for a given kiosk group
    /// </summary>
    public class KioskGroupAttendance
    {
        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        public List<KioskScheduleAttendance> Schedules { get; set; }

        /// <summary>
        /// Gets the distinct person ids.
        /// </summary>
        /// <value>
        /// The distinct person ids.
        /// </value>
        public List<int> DistinctPersonIds 
        {
            get
            {
                if ( Schedules != null )
                {
                    return Schedules.Where( s => s.IsActive ).SelectMany( s => s.PersonIds ).Distinct().ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public int CurrentCount
        {
            get
            {
                var people = DistinctPersonIds;
                return people != null ? people.Count() : 0;
            }
        }
    }
}