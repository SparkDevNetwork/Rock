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
using System.Text;

using Rock.Data;
using Rock.Financial;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialScheduledTransaction"/> entity objects.
    /// </summary>
    public partial class FinancialScheduledTransactionService
    {
        /// <summary>
        /// Gets schedule transactions associated to a person.  Includes any transactions associated to person
        /// or any other person with same giving group id
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="givingGroupId">The giving group identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns>
        /// The <see cref="Rock.Model.FinancialTransaction" /> that matches the transaction code, this value will be null if a match is not found.
        /// </returns>
        public IQueryable<FinancialScheduledTransaction> Get( int? personId, int? givingGroupId, bool includeInactive )
        {
            var qry = Queryable()
                .Include( a => a.ScheduledTransactionDetails )
                .Include( a => a.FinancialPaymentDetail.CurrencyTypeValue )
                .Include( a => a.FinancialPaymentDetail.CreditCardTypeValue );

            if ( !includeInactive )
            {
                qry = qry.Where( t => t.IsActive );
            }

            if ( givingGroupId.HasValue )
            {
                //  Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == givingGroupId );
            }
            else if ( personId.HasValue )
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == personId );
            }

            return qry
                .OrderByDescending( t => t.IsActive )
                .ThenByDescending( t => t.StartDate );
        }

        /// <summary>
        /// Gets the by schedule identifier.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns></returns>
        public FinancialScheduledTransaction GetByScheduleId( string scheduleId )
        {
            return Queryable( "ScheduledTransactionDetails,AuthorizedPersonAlias.Person" )
                .Where( t => t.GatewayScheduleId == scheduleId.Trim() )
                .FirstOrDefault();
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( FinancialScheduledTransaction item )
        {
            if ( item.FinancialPaymentDetailId.HasValue )
            {
                var paymentDetailsService = new FinancialPaymentDetailService( (Rock.Data.RockContext)this.Context );
                var paymentDetail = paymentDetailsService.Get( item.FinancialPaymentDetailId.Value );
                if ( paymentDetail != null )
                {
                    paymentDetailsService.Delete( paymentDetail );
                }
            }

            return base.Delete( item );
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool GetStatus( FinancialScheduledTransaction scheduledTransaction, out string errorMessages )
        {
            if ( scheduledTransaction != null &&
                scheduledTransaction.FinancialGateway != null &&
                scheduledTransaction.FinancialGateway.IsActive )
            {
                if ( scheduledTransaction.FinancialGateway.Attributes == null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes( (RockContext)this.Context );
                }

                var gateway = scheduledTransaction.FinancialGateway.GetGatewayComponent();
                if ( gateway != null )
                {
                    return gateway.GetScheduledPaymentStatus( scheduledTransaction, out errorMessages );
                }
            }

            errorMessages = "Gateway is invalid or not active";
            return false;
        }

        /// <summary>
        /// Reactivates the specified scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool Reactivate( FinancialScheduledTransaction scheduledTransaction, out string errorMessages )
        {
            if ( scheduledTransaction != null &&
                scheduledTransaction.FinancialGateway != null &&
                scheduledTransaction.FinancialGateway.IsActive )
            {
                if ( scheduledTransaction.FinancialGateway.Attributes == null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes( (RockContext)this.Context );
                }

                var gateway = scheduledTransaction.FinancialGateway.GetGatewayComponent();
                if ( gateway != null )
                {
                    if ( gateway.ReactivateScheduledPayment( scheduledTransaction, out errorMessages ) )
                    {
                        var noteTypeService = new NoteTypeService( (RockContext)this.Context );
                        var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
                        if ( noteType != null )
                        {
                            var noteService = new NoteService( (RockContext)this.Context );
                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.EntityId = scheduledTransaction.Id;
                            note.Caption = "Reactivated Transaction";
                            noteService.Add( note );
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            errorMessages = "Gateway is invalid or not active";
            return false;
        }

        /// <summary>
        /// Cancels the specified scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool Cancel( FinancialScheduledTransaction scheduledTransaction, out string errorMessages )
        {
            if ( scheduledTransaction != null &&
                scheduledTransaction.FinancialGateway != null &&
                scheduledTransaction.FinancialGateway.IsActive )
            {
                if ( scheduledTransaction.FinancialGateway.Attributes == null )
                {
                    scheduledTransaction.FinancialGateway.LoadAttributes( (RockContext)this.Context );
                }

                var gateway = scheduledTransaction.FinancialGateway.GetGatewayComponent();
                if ( gateway != null )
                {
                    if ( gateway.CancelScheduledPayment( scheduledTransaction, out errorMessages ) )
                    {
                        var noteTypeService = new NoteTypeService( (RockContext)this.Context );
                        var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
                        if ( noteType != null )
                        {
                            var noteService = new NoteService( (RockContext)this.Context );
                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.EntityId = scheduledTransaction.Id;
                            note.Caption = "Cancelled Transaction";
                            noteService.Add( note );
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            errorMessages = "Gateway is invalid or not active";
            return false;
        }

        /// <summary>
        /// Processes the payments.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="batchNamePrefix">The batch name prefix.</param>
        /// <param name="payments">The payments.</param>
        /// <param name="batchUrlFormat">The batch URL format.</param>
        /// <param name="receiptEmail">The receipt email.</param>
        /// <returns></returns>
        public static string ProcessPayments( FinancialGateway gateway, string batchNamePrefix, List<Payment> payments, string batchUrlFormat = "", Guid? receiptEmail = null )
        {
            int totalPayments = 0;
            int totalAlreadyDownloaded = 0;
            int totalNoMatchingTransaction = 0;
            int totalAdded = 0;
            int totalReversals = 0;
            int totalStatusChanges = 0;

            var batchSummary = new Dictionary<Guid, List<Decimal>>();
            var initialControlAmounts = new Dictionary<Guid, decimal>();

            var gatewayComponent = gateway.GetGatewayComponent();

            var newTransactions = new List<FinancialTransaction>();

            var contributionTxnType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

            int? defaultAccountId = null;
            using ( var rockContext2 = new RockContext() )
            {
                defaultAccountId = new FinancialAccountService( rockContext2 ).Queryable()
                    .Where( a =>
                        a.IsActive &&
                        !a.ParentAccountId.HasValue &&
                        ( !a.StartDate.HasValue || a.StartDate.Value <= RockDateTime.Now ) &&
                        ( !a.EndDate.HasValue || a.EndDate.Value >= RockDateTime.Now )
                        )
                    .OrderBy( a => a.Order )
                    .Select( a => a.Id )
                    .FirstOrDefault();
            }

            var batchTxnChanges = new Dictionary<Guid, List<string>>();
            var batchBatchChanges = new Dictionary<Guid, List<string>>();
            var scheduledTransactionIds = new List<int>();
            List<FinancialTransaction> transactionsWithAttributes = new List<FinancialTransaction>();

            foreach ( var payment in payments.Where( p => p.Amount > 0.0M ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    totalPayments++;

                    var financialTransactionService = new FinancialTransactionService( rockContext );

                    // Find existing payments with same transaction code
                    FinancialTransaction originalTxn = null;
                    var txns = financialTransactionService
                        .Queryable( "TransactionDetails" )
                        .Where( t => t.TransactionCode == payment.TransactionCode )
                        .ToList();
                    if ( txns.Any() )
                    {
                        originalTxn = txns.OrderBy( t => t.Id ).First();
                    }

                    // Calculate whether a transaction needs to be added
                    var txnAmount = CalculateTransactionAmount( payment, txns );
                    if ( txnAmount != 0.0M )
                    {
                        var scheduledTransaction = new FinancialScheduledTransactionService( rockContext ).GetByScheduleId( payment.GatewayScheduleId );

                        // Verify that the payment is for an existing scheduled transaction, or has the same transaction code as an existing payment
                        if ( scheduledTransaction != null || originalTxn != null )
                        {
                            var transaction = new FinancialTransaction();
                            transaction.Guid = Guid.NewGuid();
                            transaction.TransactionCode = payment.TransactionCode;
                            transaction.TransactionDateTime = payment.TransactionDateTime;
                            transaction.Status = payment.Status;
                            transaction.IsSettled = payment.IsSettled;
                            transaction.SettledGroupId = payment.SettledGroupId;
                            transaction.SettledDate = payment.SettledDate;
                            transaction.StatusMessage = payment.StatusMessage;
                            transaction.FinancialPaymentDetail = new FinancialPaymentDetail();

                            FinancialPaymentDetail financialPaymentDetail = null;
                            List<ITransactionDetail> originalTxnDetails = new List<ITransactionDetail>();

                            if ( scheduledTransaction != null )
                            {
                                scheduledTransactionIds.Add( scheduledTransaction.Id );
                                if ( payment.ScheduleActive.HasValue )
                                {
                                    scheduledTransaction.IsActive = payment.ScheduleActive.Value;
                                }

                                transaction.ScheduledTransactionId = scheduledTransaction.Id;
                                transaction.AuthorizedPersonAliasId = scheduledTransaction.AuthorizedPersonAliasId;
                                transaction.SourceTypeValueId = scheduledTransaction.SourceTypeValueId;
                                financialPaymentDetail = scheduledTransaction.FinancialPaymentDetail;
                                scheduledTransaction.ScheduledTransactionDetails.ToList().ForEach( d => originalTxnDetails.Add( d ) );
                            }
                            else
                            {
                                transaction.AuthorizedPersonAliasId = originalTxn.AuthorizedPersonAliasId;
                                transaction.SourceTypeValueId = originalTxn.SourceTypeValueId;
                                financialPaymentDetail = originalTxn.FinancialPaymentDetail;
                                originalTxn.TransactionDetails.ToList().ForEach( d => originalTxnDetails.Add( d ) );
                            }

                            transaction.FinancialGatewayId = gateway.Id;
                            transaction.TransactionTypeValueId = contributionTxnType.Id;

                            if ( txnAmount < 0.0M )
                            {
                                transaction.Summary = "Reversal created for previous transaction(s) to correct the total transaction amount." + Environment.NewLine;
                            }

                            // Set the attributes of the transaction
                            if ( payment.Attributes != null && payment.Attributes.Count > 0 )
                            {
                                transaction.LoadAttributes();
                                foreach ( var attribute in payment.Attributes )
                                {
                                    transaction.SetAttributeValue( attribute.Key, attribute.Value );
                                }
                                transactionsWithAttributes.Add( transaction );
                            }

                            var currencyTypeValue = payment.CurrencyTypeValue;
                            var creditCardTypevalue = payment.CreditCardTypeValue;

                            if ( financialPaymentDetail != null )
                            {
                                if ( currencyTypeValue == null && financialPaymentDetail.CurrencyTypeValueId.HasValue )
                                {
                                    currencyTypeValue = DefinedValueCache.Read( financialPaymentDetail.CurrencyTypeValueId.Value );
                                }

                                if ( creditCardTypevalue == null && financialPaymentDetail.CreditCardTypeValueId.HasValue )
                                {
                                    creditCardTypevalue = DefinedValueCache.Read( financialPaymentDetail.CreditCardTypeValueId.Value );
                                }

                                transaction.FinancialPaymentDetail.AccountNumberMasked = financialPaymentDetail.AccountNumberMasked;
                                transaction.FinancialPaymentDetail.NameOnCardEncrypted = financialPaymentDetail.NameOnCardEncrypted;
                                transaction.FinancialPaymentDetail.ExpirationMonthEncrypted = financialPaymentDetail.ExpirationMonthEncrypted;
                                transaction.FinancialPaymentDetail.ExpirationYearEncrypted = financialPaymentDetail.ExpirationYearEncrypted;
                                transaction.FinancialPaymentDetail.BillingLocationId = financialPaymentDetail.BillingLocationId;
                            }

                            if ( currencyTypeValue != null )
                            {
                                transaction.FinancialPaymentDetail.CurrencyTypeValueId = currencyTypeValue.Id;
                            }
                            if ( creditCardTypevalue != null )
                            {
                                transaction.FinancialPaymentDetail.CreditCardTypeValueId = creditCardTypevalue.Id;
                            }

                            // Try to allocate the amount of the transaction based on the current scheduled transaction accounts
                            decimal remainingAmount = Math.Abs( txnAmount );
                            foreach ( var detail in originalTxnDetails.Where( d => d.Amount != 0.0M ) )
                            {
                                var transactionDetail = new FinancialTransactionDetail();
                                transactionDetail.AccountId = detail.AccountId;

                                if ( detail.Amount <= remainingAmount )
                                {
                                    // If the configured amount for this account is less than or equal to the remaining
                                    // amount, allocate the configured amount
                                    transactionDetail.Amount = detail.Amount;
                                    remainingAmount -= detail.Amount;
                                }
                                else
                                {
                                    // If the configured amount is greater than the remaining amount, only allocate
                                    // the remaining amount
                                    transaction.Summary += "Note: Downloaded transaction amount was less than the configured allocation amounts for the Scheduled Transaction.";
                                    transactionDetail.Amount = remainingAmount;
                                    transactionDetail.Summary = "Note: The downloaded amount was not enough to apply the configured amount to this account.";
                                    remainingAmount = 0.0M;
                                }

                                transaction.TransactionDetails.Add( transactionDetail );

                                if ( remainingAmount <= 0.0M )
                                {
                                    // If there's no amount left, break out of details
                                    break;
                                }
                            }

                            // If there's still amount left after allocating based on current config, add the remainder
                            // to the account that was configured for the most amount
                            if ( remainingAmount > 0.0M )
                            {
                                transaction.Summary += "Note: Downloaded transaction amount was greater than the configured allocation amounts for the Scheduled Transaction.";
                                var transactionDetail = transaction.TransactionDetails
                                    .OrderByDescending( d => d.Amount )
                                    .FirstOrDefault();
                                if ( transactionDetail == null && defaultAccountId.HasValue )
                                {
                                    transactionDetail = new FinancialTransactionDetail();
                                    transactionDetail.AccountId = defaultAccountId.Value;
                                    transaction.TransactionDetails.Add( transactionDetail );
                                }
                                if ( transactionDetail != null )
                                {
                                    transactionDetail.Amount += remainingAmount;
                                    transactionDetail.Summary = "Note: Extra amount was applied to this account.";
                                }
                            }

                            // If the amount to apply was negative, update all details to be negative (absolute value was used when allocating to accounts)
                            if ( txnAmount < 0.0M )
                            {
                                foreach ( var txnDetail in transaction.TransactionDetails )
                                {
                                    txnDetail.Amount = 0 - txnDetail.Amount;
                                }
                            }

                            // Get the batch
                            var batch = new FinancialBatchService( rockContext ).Get(
                                batchNamePrefix,
                                currencyTypeValue,
                                creditCardTypevalue,
                                transaction.TransactionDateTime.Value,
                                gateway.GetBatchTimeOffset() );

                            var batchChanges = new List<string>();
                            if ( batch.Id != 0 )
                            {
                                initialControlAmounts.AddOrIgnore( batch.Guid, batch.ControlAmount );

                                transaction.BatchId = batch.Id;
                                financialTransactionService.Add( transaction );
                            }
                            else
                            {
                                batch.Transactions.Add( transaction );
                            }

                            batch.ControlAmount += transaction.TotalAmount;

                            batch.Transactions.Add( transaction );

                            if ( txnAmount > 0.0M && receiptEmail.HasValue )
                            {
                                newTransactions.Add( transaction );
                            }

                            // Add summary
                            if ( !batchSummary.ContainsKey( batch.Guid ) )
                            {
                                batchSummary.Add( batch.Guid, new List<Decimal>() );
                            }
                            batchSummary[batch.Guid].Add( txnAmount );

                            if ( txnAmount > 0.0M )
                            {
                                totalAdded++;
                            }
                            else
                            {
                                totalReversals++;
                            }
                        }
                        else
                        {
                            totalNoMatchingTransaction++;
                        }
                    }
                    else
                    {
                        totalAlreadyDownloaded++;
                    }

                    foreach ( var txn in txns
                        .Where( t =>
                            t.Status != payment.Status ||
                            t.StatusMessage != payment.StatusMessage ||
                            t.IsSettled != payment.IsSettled ||
                            t.SettledGroupId != payment.SettledGroupId ||
                            t.SettledDate != payment.SettledDate ) )
                    {
                        txn.IsSettled = payment.IsSettled;
                        txn.SettledGroupId = payment.SettledGroupId;
                        txn.SettledDate = payment.SettledDate;
                        txn.Status = payment.Status;
                        txn.StatusMessage = payment.StatusMessage;
                        totalStatusChanges++;
                    }

                    rockContext.SaveChanges();
                }
            }
            if ( transactionsWithAttributes.Count > 0 )
            {
                foreach ( var transaction in transactionsWithAttributes )
                {
                    using ( var rockContext3 = new RockContext() )
                    {
                        transaction.SaveAttributeValues( rockContext3 );
                        rockContext3.SaveChanges();
                    }
                }
            }

            // Queue a transaction to update the status of all affected scheduled transactions
            var updatePaymentStatusTxn = new Rock.Transactions.UpdatePaymentStatusTransaction( gateway.Id, scheduledTransactionIds );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( updatePaymentStatusTxn );

            if ( receiptEmail.HasValue )
            {
                // Queue a transaction to send receipts
                var newTransactionIds = newTransactions.Select( t => t.Id ).ToList();
                var sendPaymentReceiptsTxn = new Rock.Transactions.SendPaymentReceipts( receiptEmail.Value, newTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentReceiptsTxn );
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<li>{0} {1} downloaded.</li>", totalPayments.ToString( "N0" ),
                ( totalPayments == 1 ? "payment" : "payments" ) );

            if ( totalAlreadyDownloaded > 0 )
            {
                sb.AppendFormat( "<li>{0} {1} previously downloaded and {2} already been added.</li>", totalAlreadyDownloaded.ToString( "N0" ),
                    ( totalAlreadyDownloaded == 1 ? "payment was" : "payments were" ),
                    ( totalAlreadyDownloaded == 1 ? "has" : "have" ) );
            }

            if ( totalStatusChanges > 0 )
            {
                sb.AppendFormat( "<li>{0} {1} previously downloaded but had a change of status.</li>", totalStatusChanges.ToString( "N0" ),
                ( totalStatusChanges == 1 ? "payment was" : "payments were" ) );
            }

            if ( totalNoMatchingTransaction > 0 )
            {
                sb.AppendFormat( "<li>{0} {1} could not be matched to an existing scheduled payment profile or a previous transaction.</li>", totalNoMatchingTransaction.ToString( "N0" ),
                    ( totalNoMatchingTransaction == 1 ? "payment" : "payments" ) );
            }

            sb.AppendFormat( "<li>{0} {1} successfully added.</li>", totalAdded.ToString( "N0" ),
                ( totalAdded == 1 ? "payment was" : "payments were" ) );

            if ( totalReversals > 0 )
            {
                sb.AppendFormat( "<li>{0} {1} added as a reversal to a previous transaction.</li>", totalReversals.ToString( "N0" ),
                    ( totalReversals == 1 ? "payment was" : "payments were" ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var batches = new FinancialBatchService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( b => batchSummary.Keys.Contains( b.Guid ) )
                    .ToList();

                foreach ( var batchItem in batchSummary )
                {
                    int items = batchItem.Value.Count;
                    if ( items > 0 )
                    {
                        var batch = batches
                            .Where( b => b.Guid.Equals( batchItem.Key ) )
                            .FirstOrDefault();

                        string batchName = string.Format( "'{0} ({1})'", batch.Name, batch.BatchStartDateTime.Value.ToString( "d" ) );
                        if ( !string.IsNullOrWhiteSpace( batchUrlFormat ) )
                        {
                            batchName = string.Format( "<a href='{0}'>{1}</a>", string.Format( batchUrlFormat, batch.Id ), batchName );
                        }

                        decimal sum = batchItem.Value.Sum();

                        string summaryformat = items == 1 ?
                            "<li>{0} transaction of {1} was added to the {2} batch.</li>" :
                            "<li>{0} transactions totaling {1} were added to the {2} batch</li>";

                        sb.AppendFormat( summaryformat, items.ToString( "N0" ), sum.FormatAsCurrency(), batchName );
                    }
                }
            }

            return sb.ToString();
        }

        private static decimal CalculateTransactionAmount( Payment payment, List<FinancialTransaction> transactions )
        {
            decimal rockAmount = 0.0M;
            decimal processedAmount = payment.IsFailure ? 0.0M : payment.Amount;

            if ( transactions != null && transactions.Any() )
            {
                rockAmount = transactions.Sum( t => t.TotalAmount );
            }

            return processedAmount - rockAmount;
        }

        private void LaunchWorkflow( WorkflowTypeCache workflowType, FinancialTransaction transaction )
        {
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    string workflowName = transaction.TransactionCode;
                    if ( transaction.AuthorizedPersonAliasId != null )
                    {
                        var person = new PersonAliasService( rockContext ).GetPerson( transaction.AuthorizedPersonAliasId.Value );
                        if ( person != null )
                        {
                            workflowName = person.FullName;
                        }
                    }

                    var workflowService = new WorkflowService( rockContext );
                    var workflow = Rock.Model.Workflow.Activate( workflowType, workflowName, rockContext );
                    if ( workflow != null )
                    {
                        List<string> workflowErrors;
                        workflowService.Process( workflow, transaction, out workflowErrors );
                    }
                }
            }
        }
    }
}