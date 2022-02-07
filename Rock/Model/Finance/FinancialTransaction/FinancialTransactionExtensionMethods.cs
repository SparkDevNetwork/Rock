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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    ///FinancialTransaction Extension Methods
    /// </summary>
    public static partial class FinancialTransactionExtensionMethods
    {
        /// <summary>
        /// Process a refund for a transaction.
        /// </summary>
        /// <param name="transaction">The refund transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="reasonValueId">The reason value identifier.</param>
        /// <param name="summary">The summary.</param>
        /// <param name="process">if set to <c>true</c> [process].</param>
        /// <param name="batchNameSuffix">The batch name suffix.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public static FinancialTransaction ProcessRefund( this FinancialTransaction transaction, decimal? amount, int? reasonValueId, string summary, bool process, string batchNameSuffix, out string errorMessage )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new FinancialTransactionService( rockContext );
                var refundTransaction = service.ProcessRefund( transaction, amount, reasonValueId, summary, process, batchNameSuffix, out errorMessage );

                if ( refundTransaction != null )
                {
                    rockContext.SaveChanges();
                }

                return refundTransaction;
            }
        }

        /// <summary>
        /// Distributes a total fee amount among the details of a transaction according to each detail's
        /// percent of the total transaction amount.
        /// For example, consider a $10 transaction has two details, one for $1 and another for $9.
        /// If this method were called with a $1 fee, that fee would be distributed as 10 cents and
        /// 90 cents respectively.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="totalFee">The total fee for the transaction</param>
        public static void SetApportionedFeesOnDetails( this FinancialTransaction transaction, decimal? totalFee )
        {
            if ( transaction.TransactionDetails == null || !transaction.TransactionDetails.Any() )
            {
                return;
            }

            if ( !totalFee.HasValue )
            {
                foreach ( var detail in transaction.TransactionDetails )
                {
                    detail.FeeAmount = null;
                }

                return;
            }

            var totalAmount = transaction.TotalAmount;
            var totalFeeRemaining = totalFee.Value;
            var numberOfDetailsRemaining = transaction.TransactionDetails.Count;

            foreach ( var detail in transaction.TransactionDetails )
            {
                numberOfDetailsRemaining--;
                var isLastDetail = numberOfDetailsRemaining == 0;

                if ( isLastDetail )
                {
                    // Ensure that the full fee value is retained and some part of it
                    // is not lost because of rounding
                    detail.FeeAmount = totalFeeRemaining;
                }
                else
                {
                    var percentOfTotal = detail.Amount / totalAmount;
                    var apportionedFee = Math.Round( percentOfTotal * totalFee.Value, 2 );

                    detail.FeeAmount = apportionedFee;
                    totalFeeRemaining -= apportionedFee;
                }
            }
        }

        /// <summary>
        /// Distributes the Organization's currency total amount among the details of a transaction according to each detail's
        /// percent of the total transaction amount.
        /// For example, consider a $10 transaction has two details, one for $1 and another for $9.
        /// If this method were called with a $11 amount, the detail's amount would be set to $9.9 and $1.1 respectively.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="totalOrganizationCurrencyAmount">The total amount for the transaction in the organization's currency</param>
        public static void SetApportionedDetailAmounts( this FinancialTransaction transaction, decimal totalOrganizationCurrencyAmount )
        {
            if ( transaction.TransactionDetails == null || !transaction.TransactionDetails.Any() )
            {
                return;
            }

            var totalAmount = transaction.TotalAmount;
            var totalOrganizationCurrencyAmountRemaining = totalOrganizationCurrencyAmount;
            var numberOfDetailsRemaining = transaction.TransactionDetails.Count;

            foreach ( var detail in transaction.TransactionDetails )
            {
                numberOfDetailsRemaining--;
                var isLastDetail = numberOfDetailsRemaining == 0;

                if ( isLastDetail )
                {
                    // Ensure that the full amount is retained and some part of it
                    // is not lost because of rounding
                    detail.Amount = totalOrganizationCurrencyAmountRemaining;
                }
                else
                {
                    var percentOfTotal = detail.Amount / totalAmount;
                    var organizationCurrencyAmount = Math.Round( percentOfTotal * totalOrganizationCurrencyAmount, 2 );

                    detail.Amount = organizationCurrencyAmount;
                    totalOrganizationCurrencyAmountRemaining -= organizationCurrencyAmount;
                }
            }
        }
    }
}
