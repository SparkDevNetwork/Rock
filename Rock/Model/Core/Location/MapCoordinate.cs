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

namespace Rock.Model
{
    #region Map Helper Classes

    /// <summary>
    /// Helper class to store map coordinates
    /// </summary>
    public class MapCoordinate
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        public MapCoordinate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public MapCoordinate( double? latitude, double? longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    #endregion Map Helper Classes
}
