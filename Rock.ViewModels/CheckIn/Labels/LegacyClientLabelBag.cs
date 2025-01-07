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

using System.Collections.Generic;

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// Defines the structure of a legacy label that should be printed by the
    /// client instead of the server.
    /// </summary>
    public class LegacyClientLabelBag
    {
        /// <summary>
        /// The address of the printer. This should be either an IP address or
        /// an IP address and port number.
        /// </summary>
        public string PrinterAddress { get; set; }

        /// <summary>
        /// A unique key that identifies the source of the label for cache
        /// purposes.
        /// </summary>
        public string LabelKey { get; set; }

        /// <summary>
        /// The URL that the label contents can be retrieved from.
        /// </summary>
        public string LabelFile { get; set; }

        /// <summary>
        /// The merge fields that should be used when creating the final ZPL
        /// content to be printed.
        /// </summary>
        public Dictionary<string, string> MergeFields { get; set; }
    }
}
