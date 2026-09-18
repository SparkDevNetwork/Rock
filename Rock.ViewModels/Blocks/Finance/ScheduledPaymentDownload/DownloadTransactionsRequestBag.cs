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
using System;

namespace Rock.ViewModels.Blocks.Finance.ScheduledPaymentDownload
{
    /// <summary>
    /// The gateway and date range selected for a scheduled payment download.
    /// </summary>
    public class DownloadTransactionsRequestBag
    {
        /// <summary>
        /// The unique identifier of the financial gateway to download payments from.
        /// </summary>
        public Guid? FinancialGatewayGuid { get; set; }

        /// <summary>
        /// The first date of the range to download, in ISO 8601 format.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// The last date of the range to download, in ISO 8601 format.
        /// </summary>
        public string EndDate { get; set; }
    }
}
