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

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// The capabilities of a printer defined in Rock.
    /// </summary>
    internal class PrinterCapabilities
    {
        /// <summary>
        /// The DPI (dots per inch) of the printer.
        /// </summary>
        public int? Dpi { get; set; }

        /// <summary>
        /// A value indicating whether the printer can cut labels.
        /// </summary>
        public bool IsCutterSupported { get; set; }
    }
}
