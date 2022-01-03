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
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Wrapper for a GroupType (Checkin Area) for the current Kiosk
    /// </summary>
    [DataContract]
    public class KioskGroupType
    {
        /// <summary>
        /// Gets or sets the group type (Checkin Area)
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaVisible]
        public GroupTypeCache GroupType => GroupTypeCache.Get( GroupTypeId );

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// All groups with active schedules
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroup> KioskGroups { get; set; }

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
                return KioskGroups != null && KioskGroups.Any( s => s.IsCheckInActive );
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-out is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckOutActive
        {
            get
            {
                return KioskGroups != null && KioskGroups.Any( s => s.IsCheckOutActive );
            }
        }

        /// <summary>
        /// Next time that a location/group/schedule will be active for
        /// this group type.  If the group type has locations, this time
        /// will be in the past, if there are no locations, this time would
        /// be in the future
        /// </summary>
        /// <value>
        /// The next active time.
        /// </value>
        public DateTime NextActiveTime
        {
            get
            {
                return KioskGroups.Min( s => (DateTime?)s.NextActiveDateTime ) ?? DateTime.MaxValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroupType" /> class.
        /// </summary>
        public KioskGroupType()
            : base()
        {
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroupType"/> class.
        /// </summary>
        /// <param name="groupTypeid">The group typeid.</param>
        public KioskGroupType( int groupTypeid )
            : base()
        {
            GroupTypeId = groupTypeid;
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroupType" /> class.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        public KioskGroupType( GroupType groupType )
            : this( groupType != null ? groupType.Id : 0 )
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GroupType.ToString();
        }
    }
}