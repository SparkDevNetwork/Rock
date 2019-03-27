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
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialTransactionDetail"/> entity objects.
    /// </summary>
    public partial class FinancialTransactionDetailService
    {

        /// <summary>
        /// Gets the gifts.
        /// </summary>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetGifts()
        {
            Guid contributionTxnGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid();

            return Queryable().AsNoTracking()
                .Where( t =>
                    t.Account != null &&
                    t.Account.IsTaxDeductible &&
                    t.Transaction != null &&
                    t.Transaction.TransactionTypeValue != null &&
                    t.Transaction.TransactionTypeValue.Guid.Equals( contributionTxnGuid ) &&
                    t.Transaction.TransactionDateTime.HasValue &&
                    t.Transaction.AuthorizedPersonAlias != null &&
                    t.Transaction.AuthorizedPersonAlias.Person != null );
        }

        #region Stored Procedure Queries

        /// <summary>
        /// Gets the giving analytics account totals.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="transactionTypeIds">The transaction type ids.</param>
        /// <returns></returns>
        public static DataSet GetGivingAnalyticsAccountTotals( DateTime? start, DateTime? end, List<int> accountIds, List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> transactionTypeIds )
        {
            var parameters = GetGivingAnalyticsParameters( start, end, null, null, accountIds, currencyTypeIds, sourceTypeIds, transactionTypeIds );
            return DbService.GetDataSet( "spFinance_GivingAnalyticsQuery_AccountTotals", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the giving analytics transaction data.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="transactionTypeIds">The transaction type ids.</param>
        /// <returns></returns>
        public static DataSet GetGivingAnalyticsTransactionData( DateTime? start, DateTime? end, List<int> accountIds, List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> transactionTypeIds )
        {
            var parameters = GetGivingAnalyticsParameters( start, end, null, null, accountIds, currencyTypeIds, sourceTypeIds, transactionTypeIds );
            return DbService.GetDataSet( "[dbo].[spFinance_GivingAnalyticsQuery_TransactionData]", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the giving analytics first last ever dates.
        /// </summary>
        /// <returns></returns>
        public static DataSet GetGivingAnalyticsFirstLastEverDates()
        {
            var parameters = new Dictionary<string, object>();
            return DbService.GetDataSet( "spFinance_GivingAnalyticsQuery_FirstLastEverDates", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        /// <summary>
        /// Gets the giving analytics person summary.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="minAmount">The minimum amount.</param>
        /// <param name="maxAmount">The maximum amount.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="transactionTypeIds">The transaction type ids.</param>
        /// <returns></returns>
        public static DataSet GetGivingAnalyticsPersonSummary( DateTime? start, DateTime? end, decimal? minAmount, decimal? maxAmount,
            List<int> accountIds, List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> transactionTypeIds )
        {
            var parameters = GetGivingAnalyticsParameters( start, end, minAmount, maxAmount, accountIds, currencyTypeIds, sourceTypeIds, transactionTypeIds );
            return DbService.GetDataSet( "spFinance_GivingAnalyticsQuery_PersonSummary", System.Data.CommandType.StoredProcedure, parameters, 300 );
        }

        private static Dictionary<string, object> GetGivingAnalyticsParameters( DateTime? start, DateTime? end, decimal? minAmount, decimal? maxAmount,
            List<int> accountIds, List<int> currencyTypeIds, List<int> sourceTypeIds, List<int> transactionTypeIds )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if ( start.HasValue )
            {
                parameters.Add( "StartDate", start.Value );
            }

            if ( end.HasValue )
            {
                parameters.Add( "EndDate", end.Value );
            }

            if ( minAmount.HasValue )
            {
                parameters.Add( "MinAmount", minAmount.Value );
            }

            if ( maxAmount.HasValue )
            {
                parameters.Add( "MaxAmount", maxAmount.Value );
            }

            if ( accountIds != null && accountIds.Any() )
            {
                parameters.Add( "AccountIds", accountIds.AsDelimited( "," ) );
            }

            if ( currencyTypeIds != null && currencyTypeIds.Any() )
            {
                parameters.Add( "CurrencyTypeIds", currencyTypeIds.AsDelimited( "," ) );
            }

            if ( sourceTypeIds != null && sourceTypeIds.Any() )
            {
                parameters.Add( "SourceTypeIds", sourceTypeIds.AsDelimited( "," ) );
            }

            if ( transactionTypeIds != null && transactionTypeIds.Any() )
            {
                parameters.Add( "TransactionTypeIds", transactionTypeIds.AsDelimited( "," ) );
            }

            return parameters;
        }

        #endregion

    }

}