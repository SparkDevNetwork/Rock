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
    /// The result of geocoding an input string: the resolved point and, when the provider supplies one, a recommended viewport sized to the matched place.
    /// </summary>
    /// <remarks>
    /// Internal by design: this shape backs an in-flight feature and may change or be removed. It is not
    /// part of the public API for plugins. The viewport a provider returns is sized to the match, so a
    /// ZIP, a city, and a street address each come back with a differently sized box.
    /// </remarks>
    internal class GeocodeResult
    {
        /// <summary>
        /// The resolved location point.
        /// </summary>
        public GeographyPoint Location { get; }

        /// <summary>
        /// The recommended viewport around the location, or <c>null</c> when the provider returned none.
        /// </summary>
        public GeographyBounds Viewport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeocodeResult"/> class.
        /// </summary>
        /// <param name="location">The resolved location point.</param>
        /// <param name="viewport">The recommended viewport around the location, or <c>null</c> when none was returned.</param>
        public GeocodeResult( GeographyPoint location, GeographyBounds viewport )
        {
            Location = location;
            Viewport = viewport;
        }
    }
}
