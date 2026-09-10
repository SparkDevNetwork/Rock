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

using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// How far a group is from a person, how that distance is measured, and how the person shares where they are.
    /// </summary>
    /// <remarks>
    /// The modes are additive and ordered by what they cost to produce: each mode includes the location
    /// input and measurement of the one before it and adds to it. A higher mode therefore requires
    /// everything the lower modes require, which lets the block fall back to the next lower mode when a
    /// mode's external dependency is unavailable.
    /// </remarks>
    [Enums.EnumDomain( "Group" )]
    public enum DistanceCalculationMode
    {
        /// <summary>
        /// Distance is not measured, and no location is asked for.
        /// </summary>
        [Description( "None" )]
        None = 0,

        /// <summary>
        /// The direct line from the person's current location to the group, measured over stored coordinates. The person shares their location through the browser, so no geocoding is needed.
        /// </summary>
        [Description( "Straight-Line Distance (My Current Location)" )]
        StraightLineCurrentLocation = 1,

        /// <summary>
        /// Adds an address or ZIP entry to <see cref="StraightLineCurrentLocation"/>, still measured as a direct line. Resolving the entry to coordinates requires geocoding.
        /// </summary>
        [Description( "Straight-Line Distance (Address or Zip Code)" )]
        StraightLineAddress = 2,

        /// <summary>
        /// Adds the distance and time to drive from the person to the group, from a routing provider, to everything <see cref="StraightLineAddress"/> offers.
        /// </summary>
        [Description( "Driving Distance" )]
        Driving = 3
    }
}
