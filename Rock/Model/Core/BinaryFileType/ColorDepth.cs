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

namespace Rock.Model
{
    /// <summary>
    /// Represents the preferred color depth of the binary file type.
    /// </summary>
    public enum ColorDepth
    {
        /// <summary>
        /// An undefined color depth.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A preferred color depth of Black and White.
        /// </summary>
        BlackWhite = 0,

        /// <summary>
        /// A preferred color depth of 8-bit Grayscale.
        /// </summary>
        Grayscale8bit = 1,

        /// <summary>
        /// A preferred color depth of 24-bit Grayscale.
        /// </summary>
        Grayscale24bit = 2,

        /// <summary>
        /// A preferred color depth of 8-bit Color.
        /// </summary>
        Color8bit = 3,

        /// <summary>
        /// A preferred color depth of 24-bit Color.
        /// </summary>
        Color24bit = 4
    }
}
