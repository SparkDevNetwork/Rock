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

namespace Rock.Core.Geography.Classes
{
    /// <summary>
    /// A latitude/longitude bounding box (a rectangular map area).
    /// </summary>
    /// <remarks>
    /// Internal by design: this shape backs the geocoding viewport for an in-flight feature and may
    /// change or be removed. It is not part of the public API for plugins.
    /// </remarks>
    internal class GeographyBounds
    {
        /// <summary>
        /// The northern edge latitude.
        /// </summary>
        public double North { get; }

        /// <summary>
        /// The southern edge latitude.
        /// </summary>
        public double South { get; }

        /// <summary>
        /// The eastern edge longitude.
        /// </summary>
        public double East { get; }

        /// <summary>
        /// The western edge longitude.
        /// </summary>
        public double West { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeographyBounds"/> class.
        /// </summary>
        /// <param name="north">The northern edge latitude.</param>
        /// <param name="south">The southern edge latitude.</param>
        /// <param name="east">The eastern edge longitude.</param>
        /// <param name="west">The western edge longitude.</param>
        public GeographyBounds( double north, double south, double east, double west )
        {
            North = north;
            South = south;
            East = east;
            West = west;
        }
    }
}
