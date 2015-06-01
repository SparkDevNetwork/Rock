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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskGroupType
    {
        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public GroupTypeCache GroupType { get; set; }

        /// <summary>
        /// Next time that a location/group/schedule will be active for
        /// this group type.  If the group type has locations, this time
        /// will be in the past, if there are no locations, this time would
        /// be in the future
        /// </summary>
        /// <value>
        /// The next active time.
        /// </value>
        [DataMember]
        public DateTime NextActiveTime { get; set; }

        /// <summary>
        /// All groups with active schedules
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroup> KioskGroups { get; set; }

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
            GroupType = GroupTypeCache.Read( groupTypeid );
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