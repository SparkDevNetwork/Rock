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

namespace Rock.Address.Classes
{
    /// <summary>
    /// Represents the result of a driving distance calculation between an origin and a destination.
    /// </summary>
    public class DistanceResult
    {
        /// <summary>
        /// The destination point.
        /// </summary>
        public GeographyPoint DestinationPoint { get; set; }
        /// <summary>
        /// The driving distance in miles.
        /// </summary>
        public double DistanceMiles { get; set; }
        /// <summary>
        /// The driving duration in minutes.
        /// </summary>
        public int TimeMinutes { get; set; }
    }
}
