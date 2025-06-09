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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Communication.CommunicationSaturationReport
{
    /// <summary>
    /// The additional configuration options for the Communication Recipient List block.
    /// </summary>
    public class CommunicationSaturationReportBlockDataBag
    {
        /// <summary>
        /// The data shown in the chart on the main tab
        /// </summary>
        public List<CommunicationSaturationReportChartDataBag> ChartData { get; set; }

        /// <summary>
        /// The data shown in the grid on the recipients tab
        /// </summary>
        public GridDataBag RecipientsGridData { get; set; }

        /// <summary>
        /// The data shown in the grid on the communications tab
        /// </summary>
        public GridDataBag CommunicationsGridData { get; set; }
    }
}
