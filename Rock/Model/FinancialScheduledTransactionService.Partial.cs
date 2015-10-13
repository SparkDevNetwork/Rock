// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using DotLiquid;

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
                .Where( t => t.GatewayScheduleId == scheduleId )
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
        /// <param name="recieptEmail">The reciept email.</param>
        /// <returns></returns>
        public static string ProcessPayments( FinancialGateway gateway, string batchNamePrefix, List<Payment> payments, string batchUrlFormat = "", Guid? recieptEmail = null )
        {
            int totalPayments = 0;
            int totalAlreadyDownloaded = 0;
            int totalNoScheduledTransaction = 0;
            int totalAdded = 0;

            var batches = new List<FinancialBatch>();
            var batchSummary = new Dictionary<Guid, List<Payment>>();
            var initialControlAmounts = new Dictionary<Guid, decimal>();

            var txnPersonNames = new Dictionary<Guid, string>();

            var gatewayComponent = gateway.GetGatewayComponent();

            var newTransactions = new List<FinancialTransaction>();

            using ( var rockContext = new RockContext() )
            {
                var accountService = new FinancialAccountService( rockContext );
                var txnService = new FinancialTransactionService( rockContext );
                var batchService = new FinancialBatchService( rockContext );
                var scheduledTxnService = new FinancialScheduledTransactionService( rockContext );

                var contributionTxnType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

                var defaultAccount = accountService.Queryable()
                    .Where( a =>
                        a.IsActive &&
                        !a.ParentAccountId.HasValue &&
                        ( !a.StartDate.HasValue || a.StartDate.Value <= RockDateTime.Now ) &&
                        ( !a.EndDate.HasValue || a.EndDate.Value >= RockDateTime.Now )
                        )
                    .OrderBy( a => a.Order )
                    .FirstOrDefault();

                var batchTxnChanges = new Dictionary<Guid, List<string>>();
                var batchBatchChanges = new Dictionary<Guid, List<string>>();
                var scheduledTransactionIds = new List<int>();

                foreach ( var payment in payments.Where( p => p.Amount > 0.0M ) )
                {
                    totalPayments++;

                    // Only consider transactions that have not already been added
                    if ( !txnService.Queryable().AsNoTracking().Any( p => p.TransactionCode.Equals( payment.TransactionCode ) ) )
                    {
                        var scheduledTransaction = scheduledTxnService.GetByScheduleId( payment.GatewayScheduleId );
                        if ( scheduledTransaction != null )
                        {
                            scheduledTransactionIds.Add( scheduledTransaction.Id );

                            scheduledTransaction.IsActive = payment.ScheduleActive;

                            var transaction = new FinancialTransaction();
                            transaction.FinancialPaymentDetail = new FinancialPaymentDetail();

                            transaction.Guid = Guid.NewGuid();
                            transaction.TransactionCode = payment.TransactionCode;
                            transaction.TransactionDateTime = payment.TransactionDateTime;
                            transaction.ScheduledTransactionId = scheduledTransaction.Id;
                            transaction.AuthorizedPersonAliasId = scheduledTransaction.AuthorizedPersonAliasId;
                            txnPersonNames.Add( transaction.Guid, scheduledTransaction.AuthorizedPersonAlias.Person.FullName );
                            transaction.FinancialGatewayId = gateway.Id;
                            transaction.TransactionTypeValueId = contributionTxnType.Id;

                            var currencyTypeValue = payment.CurrencyTypeValue;
                            var creditCardTypevalue = payment.CreditCardTypeValue;

                            if ( scheduledTransaction.FinancialPaymentDetail != null )
                            {
                                if ( currencyTypeValue == null && scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
                                {
                                    currencyTypeValue = DefinedValueCache.Read( scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId.Value );
                                }

                                if ( creditCardTypevalue == null && scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                                {
                                    creditCardTypevalue = DefinedValueCache.Read( scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId.Value );
                                }

                                transaction.FinancialPaymentDetail.AccountNumberMasked = scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked;
                                transaction.FinancialPaymentDetail.NameOnCardEncrypted = scheduledTransaction.FinancialPaymentDetail.NameOnCardEncrypted;
                                transaction.FinancialPaymentDetail.ExpirationMonthEncrypted = scheduledTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted;
                                transaction.FinancialPaymentDetail.ExpirationYearEncrypted = scheduledTransaction.FinancialPaymentDetail.ExpirationYearEncrypted;
                                transaction.FinancialPaymentDetail.BillingLocationId = scheduledTransaction.FinancialPaymentDetail.BillingLocationId;
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
                            decimal remainingAmount = payment.Amount;
                            foreach ( var detail in scheduledTransaction.ScheduledTransactionDetails.Where( d => d.Amount != 0.0M ) )
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
                                    transaction.Summary = "Note: Downloaded transaction amount was less than the configured allocation amounts for the Scheduled Transaction.";
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
                                transaction.Summary = "Note: Downloaded transaction amount was greater than the configured allocation amounts for the Scheduled Transaction.";
                                var transactionDetail = transaction.TransactionDetails
                                    .OrderByDescending( d => d.Amount )
                                    .First();
                                if ( transactionDetail == null && defaultAccount != null )
                                {
                                    transactionDetail = new FinancialTransactionDetail();
                                    transactionDetail.AccountId = defaultAccount.Id;
                                }
                                if ( transactionDetail != null )
                                {
                                    transactionDetail.Amount += remainingAmount;
                                    transactionDetail.Summary = "Note: Extra amount was applied to this account.";
                                }
                            }

                            // Get the batch 
                            var batch = batchService.Get(
                                batchNamePrefix,
                                currencyTypeValue,
                                creditCardTypevalue,
                                transaction.TransactionDateTime.Value,
                                gateway.GetBatchTimeOffset(),
                                batches );

                            var batchChanges = new List<string>();
                            if ( batch.Id != 0 )
                            {
                                initialControlAmounts.AddOrIgnore( batch.Guid, batch.ControlAmount );
                            }
                            batch.ControlAmount += transaction.TotalAmount;

                            batch.Transactions.Add( transaction );

                            if ( recieptEmail.HasValue )
                            {
                                newTransactions.Add( transaction );
                            }

                            // Add summary
                            if ( !batchSummary.ContainsKey( batch.Guid ) )
                            {
                                batchSummary.Add( batch.Guid, new List<Payment>() );
                            }
                            batchSummary[batch.Guid].Add( payment );

                            totalAdded++;
                        }
                        else
                        {
                            totalNoScheduledTransaction++;
                        }
                    }
                    else
                    {
                        totalAlreadyDownloaded++;
                    }
                }

                rockContext.SaveChanges();

                // Queue a transaction to update the status of all affected scheduled transactions
                var updatePaymentStatusTxn = new Rock.Transactions.UpdatePaymentStatusTransaction( gateway.Id, scheduledTransactionIds );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( updatePaymentStatusTxn );

                if ( recieptEmail.HasValue )
                {
                    // Queue a transaction to send reciepts
                    var newTransactionIds = newTransactions.Select( t => t.Id ).ToList();
                    var sendPaymentRecieptsTxn = new Rock.Transactions.SendPaymentReciepts( recieptEmail.Value, newTransactionIds );
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendPaymentRecieptsTxn );
                }

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

            if ( totalNoScheduledTransaction > 0 )
            {
                sb.AppendFormat( "<li>{0} {1} could not be matched to an existing scheduled payment profile.</li>", totalNoScheduledTransaction.ToString( "N0" ),
                    ( totalNoScheduledTransaction == 1 ? "payment" : "payments" ) );
            }

            sb.AppendFormat( "<li>{0} {1} successfully added.</li>", totalAdded.ToString( "N0" ),
                ( totalAdded == 1 ? "payment was" : "payments were" ) );

            foreach ( var batchItem in batchSummary )
            {
                int items = batchItem.Value.Count;
                if (items > 0)
                {
                    var batch = batches
                        .Where( b => b.Guid.Equals( batchItem.Key ) )
                        .FirstOrDefault();

                    string batchName = string.Format("'{0} ({1})'", batch.Name, batch.BatchStartDateTime.Value.ToString("d"));
                    if ( !string.IsNullOrWhiteSpace( batchUrlFormat ) )
                    {
                        batchName = string.Format( "<a href='{0}'>{1}</a>", string.Format( batchUrlFormat, batch.Id ), batchName );
                    }

                    decimal sum = batchItem.Value.Select( p => p.Amount ).Sum();


                    string summaryformat = items == 1 ?
                        "<li>{0} transaction of {1} was added to the {2} batch.</li>" :
                        "<li>{0} transactions totaling {1} were added to the {2} batch</li>";

                    sb.AppendFormat( summaryformat, items.ToString( "N0" ), sum.FormatAsCurrency(), batchName );
                }
            }

            return sb.ToString();
        }

    }
}