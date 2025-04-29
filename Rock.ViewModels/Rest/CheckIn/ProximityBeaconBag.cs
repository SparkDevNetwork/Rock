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
namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// Describes a single beacon that was detected when entering or leaving
    /// a proximity area.
    /// </summary>
    public class ProximityBeaconBag
    {
        /// <summary>
        /// The major identifier value (0 - 65535) of the beacon. This is used
        /// to identify the campus.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// The minor identifier value (0 - 65535) of the beacon. This is used
        /// to identify the specific location within the campus.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// The signal strength of the beacon. This is used to determine how
        /// close the individual is to the beacon.
        /// </summary>
        public int Rssi { get; set; }

        /// <summary>
        /// The accuracy estimate of the signal strength.
        /// </summary>
        public double Accuracy { get; set; }
    }
}
