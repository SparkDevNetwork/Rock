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

namespace Rock.ViewModels.Blocks.Finance.TransactionFeeReport
{
    /// <summary>
    /// The fee-coverage totals and transaction counts shown by the Transaction Fee Report block.
    /// </summary>
    public class TransactionFeeReportBag
    {
        /// <summary>
        /// Gets or sets the combined credit card and ACH fee coverage amount.
        /// </summary>
        public decimal TotalFeeCoverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the total fee coverage amount for credit card transactions.
        /// </summary>
        public decimal CreditCardFeeCoverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the total fee coverage amount for ACH transactions.
        /// </summary>
        public decimal AchFeeCoverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the number of credit card and ACH transactions that have a fee coverage amount.
        /// </summary>
        public int TotalTransactionCount { get; set; }

        /// <summary>
        /// Gets or sets the number of credit card transactions that have a fee coverage amount.
        /// </summary>
        public int CreditCardTransactionCount { get; set; }

        /// <summary>
        /// Gets or sets the number of ACH transactions that have a fee coverage amount.
        /// </summary>
        public int AchTransactionCount { get; set; }
    }
}
