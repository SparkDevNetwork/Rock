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
    public class KioskLocation 
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier that the location is associated with
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int? Order { get; set; }

        /// <summary>
        /// The schedules that are currently active
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        [DataMember]
        public List<KioskSchedule> KioskSchedules { get; set; }

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
                return KioskSchedules != null && KioskSchedules.Any( s => s.IsCheckInActive );
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
                return KioskSchedules.Min( s => (DateTime?)s.NextActiveDateTime );
            }
        }

        /// <summary>
        /// Gets a value indicating whether [active and not full].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [active and not full]; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveAndNotFull
        {
            get
            {
                if ( Location != null )
                {
                    if ( Location.IsActive )
                    {
                        if ( Location.FirmRoomThreshold.HasValue &&
                            Location.FirmRoomThreshold.Value <= KioskLocationAttendance.Get( Location.Id ).CurrentCount )
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        public KioskLocation()
            : base()
        {
            KioskSchedules = new List<KioskSchedule>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskLocation" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public KioskLocation( Location location )
            : base()
        {
            Location = location.Clone( false );
            KioskSchedules = new List<KioskSchedule>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Location.Name;
        }
    }
}