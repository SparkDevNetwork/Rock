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
    /// Represents the preferred resolution of the binary file type.
    /// </summary>
    public enum Resolution
    {
        /// <summary>
        /// The undefined.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A preferred resolution of 72 DPI.
        /// </summary>
        DPI72 = 0,

        /// <summary>
        /// A preferred resolution of 150 DPI.
        /// </summary>
        DPI150 = 1,

        /// <summary>
        /// A preferred resolution of 300 DPI.
        /// </summary>
        DPI300 = 2,

        /// <summary>
        /// A preferred resolution of 600 DPI.
        /// </summary>
        DPI600 = 3
    }
}
