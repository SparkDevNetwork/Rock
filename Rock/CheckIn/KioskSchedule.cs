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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskSchedule
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the check in times.
        /// </summary>
        /// <value>
        /// The check in times.
        /// </value>
        [DataMember]
        public List<CheckInTimes> CheckInTimes { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets the campus current date time.
        /// </summary>
        /// <value>
        /// The campus current date time.
        /// </value>
        public DateTime CampusCurrentDateTime
        {
            get
            {
                if ( CampusId.HasValue )
                {
                    var campus = CampusCache.Get( CampusId.Value );
                    if ( campus != null )
                    {
                        return campus.CurrentDateTime;
                    }
                }
                return RockDateTime.Now;
            }
        }

        /// <summary>
        /// Gets a value indicating whether check in is active
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckInActive
        {
            get
            {
                var now = CampusCurrentDateTime;
                return CheckInTimes.Any( t =>
                    t.CheckInStart <= now &&
                    t.CheckInEnd > now );
            }
        }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [LavaInclude]
        public DateTime? StartTime
        {
            get
            {
                var now = CampusCurrentDateTime;
                var times = CheckInTimes
                    .Where( t =>
                        t.CheckInStart <= now &&
                        t.CheckInEnd > now )
                    .OrderBy( t => t.Start )
                    .FirstOrDefault();
                return times != null ? times.Start : (DateTime?)null;
            }
        }

        /// <summary>
        /// Gets the next active date time.
        /// </summary>
        /// <value>
        /// The next active date time.
        /// </value>
        public DateTime? NextActiveDateTime
        {
            get
            {
                var now = CampusCurrentDateTime;
                var times = CheckInTimes
                    .Where( t => t.CheckInStart > now )
                    .OrderBy( t => t.CheckInStart )
                    .FirstOrDefault();
                return times != null ? times.CheckInStart : (DateTime?)null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskSchedule" /> class.
        /// </summary>
        public KioskSchedule()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskSchedule" /> class.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        public KioskSchedule( Schedule schedule )
            : base()
        {
            Schedule = schedule.Clone( false );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Schedule.ToString();
        }
    }
}