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
using Rock.Model;

using System.Runtime.Serialization;

namespace Rock.CheckIn
{
    /// <summary>
    /// A POCO to represent a group, location and schedule combination for check-in.
    /// </summary>
    [DataContract]
    public class CheckInGroupLocationSchedule
    {
        /// <summary>
        /// The group location sort order.
        /// </summary>
        [DataMember]
        public int GroupLocationOrder { get; set; }

        /// <summary>
        /// The group entity.
        /// </summary>
        [DataMember]
        public Group Group { get; set; }

        /// <summary>
        /// The location entity.
        /// </summary>
        [DataMember]
        public Location Location { get; set; }

        /// <summary>
        /// The schedule entity.
        /// </summary>
        [DataMember]
        public Schedule Schedule { get; set; }
    }
}
