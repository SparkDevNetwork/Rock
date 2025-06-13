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

namespace Rock.Lava.Filters.Internal
{
    /// <summary>
    /// Describes the result of a modify entity operation in Lava.
    /// </summary>
    internal class GroupProximityResult : LavaDataObject
    {
        /// <summary>
        /// The group that the location is associated with.
        /// </summary>
        public Rock.Model.Group Group { get; set; }

        /// <summary>
        /// The group location that was used for the proximity search.
        /// </summary>
        public Rock.Model.Location Location { get; set; }

        /// <summary>
        /// Distance in meters (as the crow flies)
        /// </summary>
        public double? StraightLineDistance { get; set; }

        /// <summary>
        /// Distance in miles via roads
        /// </summary>
        public double? DrivingDistance { get; set; }

        /// <summary>
        /// Drive time in minutes
        /// </summary>
        public int? DriveTime { get; set; }
    }
}
