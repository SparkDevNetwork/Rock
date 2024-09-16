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

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Contains details about a single label that was rendered and is now ready
    /// to be printed. May also indicate an error during the rendering phase.
    /// </summary>
    internal class RenderedLabel
    {
        /// <summary>
        /// The data that should be sent to the printer. This may be <c>null</c>
        /// if an error occurred during rendering.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Where this label should be printed from.
        /// </summary>
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// The device that this label should be sent to.
        /// </summary>
        public DeviceCache PrintTo { get; set; }

        /// <summary>
        /// Any error that occurred during the rendering phase.
        /// </summary>
        public string Error { get; set; }
    }
}
