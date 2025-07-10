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

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// Describes the values that are expected to be sent to the ProximityCheckIn
    /// API endpoint.
    /// </summary>
    public class ProximityCheckInOptionsBag
    {
        /// <summary>
        /// The unique identifier of the proximity group. Multiple beacons
        /// will share the same unique identifier. There is usually only one
        /// proximity identifier per organization.
        /// </summary>
        public Guid ProximityGuid { get; set; }

        /// <summary>
        /// The set of beacons that were detected when entering or leaving
        /// an area.
        /// </summary>
        public List<ProximityBeaconBag> Beacons { get; set; }

        /// <summary>
        /// <c>true</c> if this message indicates that the individual has entered
        /// the area; otherwise, <c>false</c>.
        /// </summary>
        public bool IsPresent { get; set; }

        /// <summary>
        /// The unique identifier of the personal device that is being used
        /// when the area was entered or left.
        /// </summary>
        public Guid? PersonalDeviceGuid { get; set; }
    }
}
