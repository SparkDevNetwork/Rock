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
using System.Data.Entity;
using System.Linq;

using Rock.BulkExport;
using Rock.Data;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialTransaction"/> entity objects.
    /// </summary>
    public partial class FinancialTransactionService
    {

        /// <summary>
        /// Gets a transaction by its transaction code.
        /// </summary>
        /// <param name="financialGatewayId">The financial gateway identifier.</param>
        /// <param name="transactionCode">A <see cref="System.String" /> representing the transaction code for the transaction</param>
        /// <returns>
        /// The <see cref="Rock.Model.FinancialTransaction" /> that matches the transaction code, this value will be null if a match is not found.
        /// </returns>
        public FinancialTransaction GetByTransactionCode( int? financialGatewayId, string transactionCode )
        {
            if ( !string.IsNullOrWhiteSpace( transactionCode ) )
            {
                var qry = Queryable()
                    .Where( t => t.TransactionCode.Equals( transactionCode.Trim(), StringComparison.OrdinalIgnoreCase ) );

                if ( financialGatewayId.HasValue )
                {
                    qry = qry.Where( t => t.FinancialGatewayId.HasValue && t.FinancialGatewayId.Value == financialGatewayId.Value );
                }

                return qry.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Get transactions that have a FutureProcessingDateTime.
        /// </summary>
        /// <returns></returns>
        public IQueryable<FinancialTransaction> GetFutureTransactions()
        {
            return Queryable().Where( t => t.FutureProcessingDateTime.HasValue );
        }

        /// <summary>
        /// Process a refund for a transaction.
        /// </summary>
        /// <param name="transaction">The refund transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public FinancialTransaction ProcessRefund( FinancialTransaction transaction, out string errorMessage )
        {
            return ProcessRefund( transaction, null, null, string.Empty, true, " - Refund", out errorMessage );
        }

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
        public FinancialTransaction ProcessRefund( FinancialTransaction transaction, decimal? amount, int? reasonValueId, string summary, bool process, string batchNameSuffix, out string errorMessage )
        {
            errorMessage = string.Empty;

            // Validate parameters
            if ( transaction == null )
            {
                errorMessage = "A valid transaction is required";
                return null;
            }

            if ( transaction.Batch == null )
            {
                errorMessage = "Transaction must belong to a batch";
                return null;
            }

            if ( !amount.HasValue || amount.Value <= 0.0m )
            {
                amount = transaction.TotalAmount;
            }

            if ( !amount.HasValue || amount.Value <= 0.0m )
            {
                errorMessage = string.Format( "Amount must be greater than {0}", 0.0m.FormatAsCurrency() );
                return null;
            }


            FinancialTransaction refundTransaction = null;

            // If processing the refund through gateway, get the gateway component and process a "Credit" transaction.
            if ( process )
            {
                if ( transaction.FinancialGateway == null || transaction.TransactionCode.IsNullOrWhiteSpace() )
                {
                    errorMessage = "When processing the refund through the Gateway, the transaction must have a valid Gateway and Transaction Code";
                    return null;
                }

                var gatewayComponent = transaction.FinancialGateway?.GetGatewayComponent();
                if ( gatewayComponent == null )
                {
                    errorMessage = "Could not get the Gateway component in order to process the refund";
                    return null;
                }

                refundTransaction = gatewayComponent.Credit( transaction, amount.Value, summary, out errorMessage );
                if ( refundTransaction == null )
                {
                    return null;
                }
            }
            else
            {
                refundTransaction = new FinancialTransaction();
            }

            refundTransaction.AuthorizedPersonAliasId = transaction.AuthorizedPersonAliasId;
            refundTransaction.TransactionDateTime = RockDateTime.Now;
            refundTransaction.FinancialGatewayId = transaction.FinancialGatewayId;
            refundTransaction.TransactionTypeValueId = transaction.TransactionTypeValueId;
            refundTransaction.SourceTypeValueId = transaction.SourceTypeValueId;
            refundTransaction.ForeignCurrencyCodeValueId = transaction.ForeignCurrencyCodeValueId;

            if ( transaction.FinancialPaymentDetail != null )
            {
                refundTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                refundTransaction.FinancialPaymentDetail.AccountNumberMasked = transaction.FinancialPaymentDetail.AccountNumberMasked;
                refundTransaction.FinancialPaymentDetail.BillingLocationId = transaction.FinancialPaymentDetail.BillingLocationId;
                refundTransaction.FinancialPaymentDetail.CreditCardTypeValueId = transaction.FinancialPaymentDetail.CreditCardTypeValueId;
                refundTransaction.FinancialPaymentDetail.CurrencyTypeValueId = transaction.FinancialPaymentDetail.CurrencyTypeValueId;
                refundTransaction.FinancialPaymentDetail.ExpirationMonth = transaction.FinancialPaymentDetail.ExpirationMonth;
                refundTransaction.FinancialPaymentDetail.ExpirationYear = transaction.FinancialPaymentDetail.ExpirationYear;
                refundTransaction.FinancialPaymentDetail.NameOnCard = transaction.FinancialPaymentDetail.NameOnCard;
            }

            decimal remainingBalance = amount.Value;
            decimal? foreignCurrencyAmount = transaction.TransactionDetails.Select( d => d.ForeignCurrencyAmount ).Sum();
            decimal remainingForeignBalance = foreignCurrencyAmount ?? 0.0m;

            /*
             * If the refund is for a currency other then the Organization's currency it is up to the
             * gateway to return the correct transaction details.
             */

            if ( refundTransaction.TransactionDetails?.Any() != true )
            {
                foreach ( var account in transaction.TransactionDetails.Where( a => a.Amount > 0 ) )
                {
                    var transactionDetail = new FinancialTransactionDetail();
                    transactionDetail.AccountId = account.AccountId;
                    transactionDetail.EntityId = account.EntityId;
                    transactionDetail.EntityTypeId = account.EntityTypeId;
                    refundTransaction.TransactionDetails.Add( transactionDetail );

                    if ( remainingBalance >= account.Amount )
                    {
                        transactionDetail.Amount = 0 - account.Amount;
                        remainingBalance -= account.Amount;
                    }
                    else
                    {
                        transactionDetail.Amount = 0 - remainingBalance;
                        remainingBalance = 0.0m;
                    }

                    if ( account.ForeignCurrencyAmount.HasValue )
                    {
                        if ( remainingForeignBalance >= account.ForeignCurrencyAmount.Value )
                        {
                            transactionDetail.ForeignCurrencyAmount = 0 - account.ForeignCurrencyAmount.Value;
                            remainingForeignBalance -= account.ForeignCurrencyAmount.Value;
                        }
                        else
                        {
                            transactionDetail.ForeignCurrencyAmount = 0 - remainingForeignBalance;
                            remainingForeignBalance = 0.0m;
                        }
                    }

                    if ( remainingBalance <= 0.0m && remainingForeignBalance <= 0.0m )
                    {
                        break;
                    }
                }
            }

            if ( remainingBalance > 0 && refundTransaction.TransactionDetails.Any() )
            {
                refundTransaction.TransactionDetails.Last().Amount += remainingBalance;
            }

            var rockContext = this.Context as Rock.Data.RockContext;

            var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );
            if ( registrationEntityType != null )
            {
                foreach ( var transactionDetail in refundTransaction.TransactionDetails
                    .Where( d =>
                        d.EntityTypeId.HasValue &&
                        d.EntityTypeId.Value == registrationEntityType.Id &&
                        d.EntityId.HasValue ) )
                {
                    var registrationChanges = new History.HistoryChangeList();
                    registrationChanges.AddChange( History.HistoryVerb.Process, History.HistoryChangeType.Record, $"{transactionDetail.Amount.FormatAsCurrency()} Refund" );
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        transactionDetail.EntityId.Value,
                        registrationChanges
                    );
                }
            }

            refundTransaction.RefundDetails = new FinancialTransactionRefund();
            refundTransaction.RefundDetails.RefundReasonValueId = reasonValueId;
            refundTransaction.RefundDetails.RefundReasonSummary = summary;
            refundTransaction.RefundDetails.OriginalTransactionId = transaction.Id;

            string batchName = transaction.Batch.Name;
            if ( batchNameSuffix.IsNotNullOrWhiteSpace() && !batchName.EndsWith( batchNameSuffix ) )
            {
                batchName += batchNameSuffix;
            }

            // Get the batch
            var batchService = new FinancialBatchService( rockContext );
            TimeSpan timespan = new TimeSpan();
            if ( transaction.FinancialGateway != null )
            {
                timespan = transaction.FinancialGateway.GetBatchTimeOffset();
            }
            var batch = batchService.GetByNameAndDate( batchName, refundTransaction.TransactionDateTime.Value, timespan );

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0 )
            {
                rockContext.SaveChanges();
            }

            refundTransaction.BatchId = batch.Id;
            Add( refundTransaction );
            rockContext.SaveChanges();

            batchService.IncrementControlAmount( batch.Id, refundTransaction.TotalAmount, null );
            rockContext.SaveChanges();

            return refundTransaction;
        }

        /// <summary>
        /// Gets an export of FinancialTransaction Records
        /// </summary>
        /// <param name="page">The page being requested (where first page is 1).</param>
        /// <param name="pageSize">The number of records to provide per page. NOTE: This is limited to the 'API Max Items Per Page' global attribute.</param>
        /// <param name="exportOptions">The export options.</param>
        /// <returns></returns>
        public FinancialTransactionsExport GetFinancialTransactionExport( int page, int pageSize, FinancialTransactionExportOptions exportOptions )
        {
            IQueryable<FinancialTransaction> financialTransactionQry;
            SortProperty sortProperty = exportOptions.SortProperty;

            RockContext rockContext = this.Context as RockContext;

            if ( exportOptions.DataViewId.HasValue )
            {
                financialTransactionQry = ModelExport.QueryFromDataView<FinancialTransaction>( rockContext, exportOptions.DataViewId.Value );
            }
            else
            {
                financialTransactionQry = this.Queryable();
            }

            if ( sortProperty != null )
            {
                financialTransactionQry = financialTransactionQry.Sort( sortProperty );
            }

            if ( exportOptions.ModifiedSince.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.ModifiedDateTime.HasValue && a.ModifiedDateTime >= exportOptions.ModifiedSince.Value );
            }

            if ( exportOptions.StartDateTime.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime.HasValue && a.TransactionDateTime >= exportOptions.StartDateTime.Value );
            }

            if ( exportOptions.EndDateTime.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime.HasValue && a.TransactionDateTime < exportOptions.EndDateTime.Value );
            }

            var skip = ( page - 1 ) * pageSize;

            FinancialTransactionsExport financialTransactionsExport = new FinancialTransactionsExport();
            financialTransactionsExport.Page = page;
            financialTransactionsExport.PageSize = pageSize;
            financialTransactionsExport.TotalCount = financialTransactionQry.Count();

            var pagedFinancialTransactionQry = financialTransactionQry
                .Include( a => a.AuthorizedPersonAlias )
                .Include( a => a.TransactionDetails )
                .Include( a => a.FinancialPaymentDetail )
                .Include( a => a.TransactionDetails.Select( d => d.Account ) )
                .AsNoTracking()
                .Skip( skip )
                .Take( pageSize );

            var financialTransactionList = pagedFinancialTransactionQry.ToList();
            financialTransactionsExport.FinancialTransactions = financialTransactionList.Select( f => new FinancialTransactionExport( f ) ).ToList();

            AttributesExport.LoadAttributeValues( exportOptions, rockContext, financialTransactionsExport.FinancialTransactions, pagedFinancialTransactionQry );
            return financialTransactionsExport;
        }

        /// <summary>
        /// Gets the giving automation source transaction query by giving identifier.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <returns>IQueryable&lt;FinancialTransaction&gt;.</returns>
        public IQueryable<FinancialTransaction> GetGivingAutomationSourceTransactionQueryByGivingId( string givingId )
        {
            var givingIdPersonAliasIdQuery = new PersonAliasService( this.Context as RockContext ).Queryable().Where( a => a.Person.GivingId == givingId ).Select( a => a.Id );

            return GetGivingAutomationSourceTransactionQuery().Where( a => a.AuthorizedPersonAliasId.HasValue && givingIdPersonAliasIdQuery.Contains( a.AuthorizedPersonAliasId.Value ) );
        }

        /// <summary>
        /// Gets the giving automation source transaction query.
        /// This is used by <see cref="Rock.Jobs.GivingAutomation"/>.
        /// </summary>
        /// <returns></returns>
        public IQueryable<FinancialTransaction> GetGivingAutomationSourceTransactionQuery()
        {
            var query = Queryable().AsNoTracking();

            /*  10/10/2022 MDP
               
             Exclude Giver Anonymous from any Giving Automation/Giving Analytics logic. So exclude the following

             - Giving Alerts
             - Giving Journey
             - Giving Overview (The Giving Overview block should not show info for Giver Anonymous)

            */

            var giverAnonymousPersonGuid = SystemGuid.Person.GIVER_ANONYMOUS.AsGuid();
            var giverAnonymousPersonAliasIds = new PersonAliasService( this.Context as RockContext ).Queryable().Where( a => a.Person.Guid == giverAnonymousPersonGuid ).Select( a => a.Id );
            query = query.Where( a => a.AuthorizedPersonAliasId.HasValue && !giverAnonymousPersonAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );

            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();

            // Filter by transaction type (defaults to contributions only)
            var transactionTypeIds = settings.TransactionTypeGuids.Select( DefinedValueCache.Get ).Select( dv => dv.Id ).ToList();

            if ( transactionTypeIds.Count() == 1 )
            {
                var transactionTypeId = transactionTypeIds[0];
                query = query.Where( t => t.TransactionTypeValueId == transactionTypeId );
            }
            else
            {
                query = query.Where( t => transactionTypeIds.Contains( t.TransactionTypeValueId ) );
            }

            List<int> accountIds;
            if ( settings.FinancialAccountGuids?.Any() == true )
            {
                accountIds = FinancialAccountCache.GetByGuids( settings.FinancialAccountGuids ).Select( a => a.Id ).ToList();
            }
            else
            {
                accountIds = new List<int>();
            }

            // Filter accounts, defaults to tax deductible only
            if ( !accountIds.Any() )
            {
                query = query.Where( t => t.TransactionDetails.Any( td => td.Account.IsTaxDeductible ) );
            }
            else if ( settings.AreChildAccountsIncluded == true )
            {
                var selectedAccountIds = accountIds.ToList();
                var childAccountsIds = FinancialAccountCache.GetByIds( accountIds ).SelectMany( a => a.GetDescendentFinancialAccountIds() ).ToList();
                selectedAccountIds.AddRange( childAccountsIds );
                selectedAccountIds = selectedAccountIds.Distinct().ToList();

                if ( selectedAccountIds.Count() == 1 )
                {
                    var accountId = selectedAccountIds[0];
                    query = query.Where( t => t.TransactionDetails.Any( td => td.AccountId == accountId ) );

                }
                else
                {
                    query = query.Where( t => t.TransactionDetails.Any( td => selectedAccountIds.Contains( td.AccountId ) ) );
                }
            }
            else
            {
                if ( accountIds.Count() == 1 )
                {
                    var accountId = accountIds[0];
                    query = query.Where( t => t.TransactionDetails.Any( td => accountId == td.AccountId ) );
                }
                else
                {
                    query = query.Where( t => t.TransactionDetails.Any( td => accountIds.Contains( td.AccountId ) ) );
                }
            }

            // We'll need to factor in partial amount refunds...
            // Exclude transactions that have full refunds.
            // If it does have a refund, include the transaction if it is just a partial refund
            query = query.Where( t =>
                    // Limit to ones that don't have refunds, or has a partial refund
                    !t.Refunds.Any()
                    ||
                    // If it does have refunds, we can exclude the transaction if the refund amount is the same amount (full refund)
                    // Otherwise, we'll have to include the transaction and figure out the partial amount left after the refund.
                    (
                        (
                        // total original amount
                        t.TransactionDetails.Sum( xx => xx.Amount )

                           // total amount of any refund(s) for this transaction
                           + t.Refunds.Sum( r => r.FinancialTransaction.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.00M )
                        )

                        != 0.00M
                    )
                );

            // Remove transactions with $0 or negative amounts. If those are refunds, those will factored in above
            query = query.Where( t => t.TransactionDetails.Any( d => d.Amount > 0M ) );

            return query;
        }

        /// <summary>
        /// Gets the giving automation monthly account giving history. This is used for the Giving Overview block's monthly
        /// bar chart and also yearly summary.
        /// </summary>
        /// <returns></returns>
        public List<MonthlyAccountGivingHistory> GetGivingAutomationMonthlyAccountGivingHistory( string givingId )
        {
            return GetGivingAutomationMonthlyAccountGivingHistory( givingId, null );
        }

        /// <summary>
        /// Gets the giving automation monthly account giving history. This is used for the Giving Overview block's monthly
        /// bar chart and also yearly summary.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns>List&lt;MonthlyAccountGivingHistory&gt;.</returns>
        public List<MonthlyAccountGivingHistory> GetGivingAutomationMonthlyAccountGivingHistory( string givingId, DateTime? startDateTime )
        {
            var givingIdPersonAliasIdQuery = new PersonAliasService( this.Context as RockContext )
                .Queryable()
                .Where( a => a.Person.GivingId == givingId )
                .Select( a => a.Id );

            var qry = GetGivingAutomationSourceTransactionQuery()
                .AsNoTracking()
                .Where( t =>
                    t.TransactionDateTime.HasValue &&
                    t.AuthorizedPersonAliasId.HasValue &&
                    givingIdPersonAliasIdQuery.Contains( t.AuthorizedPersonAliasId.Value ) );

            if ( startDateTime.HasValue )
            {
                qry = qry.Where( t => t.TransactionDateTime >= startDateTime );
            }

            var views = qry
                .SelectMany( t => t.TransactionDetails.Where(td => td.Account.IsTaxDeductible == true).Select( td => new
                {
                    TransactionDateTime = t.TransactionDateTime.Value,
                    td.AccountId,
                    td.Amount,

                    // For each Refund (there could be more than one) get the refund amount for each if the refunds's Detail records for the Account.
                    // Then sum that up for the total refund amount for the account
                    AccountRefundAmount = td.Transaction
                        .Refunds.Select( a => a.FinancialTransaction.TransactionDetails.Where( rrr => rrr.AccountId == td.AccountId )
                        .Sum( rrrr => ( decimal? ) rrrr.Amount ) ).Sum() ?? 0.0M
                } ) )
                .ToList();

            var monthlyAccountGivingHistoryList = views
                .GroupBy( a => new { a.TransactionDateTime.Year, a.TransactionDateTime.Month, a.AccountId } )
                .Select( t => new MonthlyAccountGivingHistory
                {
                    Year = t.Key.Year,
                    Month = t.Key.Month,
                    AccountId = t.Key.AccountId,
                    Amount = t.Sum( d => d.Amount + d.AccountRefundAmount )
                } )
                .OrderByDescending( a => a.Year )
                .ThenByDescending( a => a.Month )
                .ToList();

            return monthlyAccountGivingHistoryList;
        }

        /// <summary>
        /// Gets the giving automation monthly account giving history that was stored as JSON in an attribute. This is used for the
        /// Giving Overview block's monthly bar chart and also yearly summary.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use GetGivingAutomationMonthlyAccountGivingHistory instead" )]
        public static List<MonthlyAccountGivingHistory> GetGivingAutomationMonthlyAccountGivingHistoryFromJson( string json )
        {
            var monthlyAccountGivingHistoryList = json.FromJsonOrNull<List<MonthlyAccountGivingHistory>>();
            return monthlyAccountGivingHistoryList ?? new List<MonthlyAccountGivingHistory>();
        }
    }
}