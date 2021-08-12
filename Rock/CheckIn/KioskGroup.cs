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

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskGroup
    {
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public Group Group { get; set; }

        /// <summary>
        /// All locations with active schedules.  Note: the location itself 
        /// may not be active (i.e. room is closed)
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        [DataMember]
        public List<KioskLocation> KioskLocations { get; set; }

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
                return KioskLocations != null && KioskLocations.Any( s => s.IsCheckInActive );
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-out is active
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckOutActive
        {
            get
            {
                return KioskLocations != null && KioskLocations.Any( s => s.IsCheckOutActive );
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
                // get list of distinct schedules used by this Kiosk's locations
                var kioskSchedules = KioskLocations.SelectMany( a => a.KioskSchedules );
                kioskSchedules = kioskSchedules.DistinctBy( a => a.Schedule.Id );

                return kioskSchedules.Min( s => ( DateTime? ) s.NextActiveDateTime );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroup" /> class.
        /// </summary>
        public KioskGroup()
            : base()
        {
            KioskLocations = new List<KioskLocation>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroup" /> class.
        /// </summary>
        /// <param name="group">The group.</param>
        public KioskGroup( Group group )
            : base()
        {
            Group = group.Clone( false );
            KioskLocations = new List<KioskLocation>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Group.ToString();
        }
    }
}