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

namespace Rock.ViewModels.Blocks.Finance.TransactionReport
{
    /// <summary>
    /// An account name paired with an amount. Used both for a transaction's per-account breakdown
    /// in the grid and for the aggregated per-account totals shown in the summary.
    /// </summary>
    public class TransactionReportAccountSummaryBag
    {
        /// <summary>
        /// Gets or sets the public name of the account.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the amount associated with the account.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
