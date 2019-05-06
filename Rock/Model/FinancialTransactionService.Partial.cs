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
using System.Data.Entity;
using System.Linq;

using Rock.BulkExport;
using Rock.Data;
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
        /// <param name="transactionCode">The transaction code.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use GetByTransactionCode(financialGatewayId, transaction). This one could return incorrect results if transactions from different financial gateways happen to use the same transaction code" )]
        public FinancialTransaction GetByTransactionCode( string transactionCode )
        {
            return this.GetByTransactionCode( null, transactionCode );
        }

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
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( FinancialTransaction item )
        {
            if ( item.FinancialPaymentDetailId.HasValue )
            {
                var paymentDetailsService = new FinancialPaymentDetailService( ( Rock.Data.RockContext ) this.Context );
                var paymentDetail = paymentDetailsService.Get( item.FinancialPaymentDetailId.Value );
                if ( paymentDetail != null )
                {
                    paymentDetailsService.Delete( paymentDetail );
                }
            }

            return base.Delete( item );
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

                var gatewayComponent = transaction.FinancialGateway.GetGatewayComponent();
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
            if ( transaction.FinancialPaymentDetail != null )
            {
                refundTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                refundTransaction.FinancialPaymentDetail.AccountNumberMasked = transaction.FinancialPaymentDetail.AccountNumberMasked;
                refundTransaction.FinancialPaymentDetail.BillingLocationId = transaction.FinancialPaymentDetail.BillingLocationId;
                refundTransaction.FinancialPaymentDetail.CreditCardTypeValueId = transaction.FinancialPaymentDetail.CreditCardTypeValueId;
                refundTransaction.FinancialPaymentDetail.CurrencyTypeValueId = transaction.FinancialPaymentDetail.CurrencyTypeValueId;
                refundTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted = transaction.FinancialPaymentDetail.ExpirationMonthEncrypted;
                refundTransaction.FinancialPaymentDetail.ExpirationYearEncrypted = transaction.FinancialPaymentDetail.ExpirationYearEncrypted;
                refundTransaction.FinancialPaymentDetail.NameOnCardEncrypted = transaction.FinancialPaymentDetail.NameOnCardEncrypted;
            }

            decimal remainingBalance = amount.Value;
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

                if ( remainingBalance <= 0.0m )
                {
                    break;
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
            decimal controlAmount = batch.ControlAmount + refundTransaction.TotalAmount;
            batch.ControlAmount = controlAmount;

            // If this is a new Batch, SaveChanges so that we can get the Batch.Id
            if ( batch.Id == 0)
            {
                rockContext.SaveChanges();
            }

            refundTransaction.BatchId = batch.Id;
            Add( refundTransaction );

            errorMessage = string.Empty;
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
    }
}