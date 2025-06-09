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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationSaturationReport
{
    /// <summary>
    /// The additional configuration options for the Communication Recipient List block.
    /// </summary>
    public class CommunicationSaturationReportFiltersBag
    {
        #region Filter Options

        /// <summary>
        /// String representing a range of time to filter results of the report by.
        /// </summary>
        public string DateRangeDelimitedString { get; set; }

        /// <summary>
        /// A data view of people to filter results of the report by.
        /// </summary>
        public ListItemBag DataView { get; set; }

        /// <summary>
        /// The connection status of people to filter results of the report by.
        /// </summary>
        public ListItemBag ConnectionStatus { get; set; }

        /// <summary>
        /// The communication medium(s) to filter results of the report by.
        /// </summary>
        public List<string> Medium { get; set; } = new List<string> { "1" };

        /// <summary>
        /// Whether or not to include only bulk communications or all communications in the report.
        /// </summary>
        public bool BulkOnly { get; set; } = true;

        #endregion
    }
}
