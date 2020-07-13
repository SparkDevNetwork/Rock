// <copyright>
// Copyright by BEMA Information Technologies
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
//
// </copyright>

///
/// Created By: BEMA Services, Inc.
/// Author: Bob Rufenacht
/// Description:
///     This class exposes the DistinctPersonIds class on KioskLocationAttendance so that workflow
///     actions can determine the number of people checked in at a particular room and schedule.
///     It enables thresholds to be evaluated for each schedule and not just overall for all services.
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Web.Cache;
using Rock.Model;
using Rock.CheckIn;

namespace com_bemaservices.CheckIn.ExtensionMethods
{

    public static class KioskLocationAttendanceExtensions
    {
        /// <summary>
        /// Gets the distinct person ids.
        /// </summary>
        /// <value>
        /// The distinct person ids.
        /// </value>
        public static List<int> DistinctPersonIds (this KioskLocationAttendance k, int scheduleId)
        {
                if ( k.Groups != null )
                {
                    return k.Groups.SelectMany( g => g.Schedules.Where( s => s.ScheduleId == scheduleId ).SelectMany( s => s.PersonIds ) ).Distinct().ToList();
                }
                else
                {
                    return null;
                }
        }

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        public static int CurrentCount ( this KioskLocationAttendance k, int scheduleId )
        {
                var people = DistinctPersonIds ( k, scheduleId );
                return people != null ? people.Count() : 0;
        }

    }
}