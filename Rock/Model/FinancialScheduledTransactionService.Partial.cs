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
        /// or any other perosn with same giving group id
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="givingGroupId">The giving group identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns>
        /// The <see cref="Rock.Model.FinancialTransaction" /> that matches the transaction code, this value will be null if a match is not found.
        /// </returns>
        public IQueryable<FinancialScheduledTransaction> Get( int? personId, int? givingGroupId, bool includeInactive )
        {
            var qry = Queryable( "ScheduledTransactionDetails" )
                .Where( t => t.IsActive || includeInactive );

            if ( givingGroupId.HasValue )
            {
                qry = qry.Where( t => t.AuthorizedPerson.GivingGroupId == givingGroupId.Value);
            }
            else if (personId.HasValue)
            {
                qry = qry.Where( t => t.AuthorizedPersonId == personId );
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
            return Queryable( "ScheduledTransactionDetails" )
                .Where( t => t.GatewayScheduleId == scheduleId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool GetStatus(FinancialScheduledTransaction scheduledTransaction, out string errorMessages)
        {
            if ( scheduledTransaction.GatewayEntityType != null )
            {
                var gateway = Rock.Financial.GatewayContainer.GetComponent( scheduledTransaction.GatewayEntityType.Guid.ToString() );
                if ( gateway != null && gateway.IsActive )
                {
                    return gateway.GetScheduledPaymentStatus( scheduledTransaction, out errorMessages );
                }
            }

            errorMessages = "Gateway is invalid or not active";
            return false;

        }

        public static string ProcessPayments( GatewayComponent gateway, string batchNamePrefix, List<Payment> payments )
        {
            var batches = new List<FinancialBatch>();
            var batchSummary = new Dictionary<Guid, List<Payment>>();

            var rockContext = new RockContext();
            var txnService = new FinancialTransactionService( rockContext );
            var batchService = new FinancialBatchService( rockContext );
            var scheduledTxnService = new FinancialScheduledTransactionService( rockContext );

            var contributionTxnTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            foreach ( var payment in payments.Where( p => p.Amount > 0.0M ) )
            {
                // Only consider transactions that have not already been added
                if ( txnService.GetByTransactionCode( payment.TransactionCode ) == null )
                {
                    var scheduledTransaction = scheduledTxnService.GetByScheduleId( payment.GatewayScheduleId );
                    if ( scheduledTransaction != null && scheduledTransaction.ScheduledTransactionDetails.Any() )
                    {
                        var transaction = new FinancialTransaction();
                        transaction.TransactionCode = payment.TransactionCode;
                        transaction.TransactionDateTime = payment.TransactionDateTime;
                        transaction.ScheduledTransactionId = scheduledTransaction.Id;
                        transaction.AuthorizedPersonId = scheduledTransaction.AuthorizedPersonId;
                        transaction.GatewayEntityTypeId = gateway.TypeId;
                        transaction.TransactionTypeValueId = contributionTxnTypeId;

                        var currencyTypeValue = payment.CurrencyTypeValue;
                        if ( currencyTypeValue != null )
                        if ( currencyTypeValue == null && scheduledTransaction.CurrencyTypeValueId.HasValue )
                        {
                            currencyTypeValue = DefinedValueCache.Read( scheduledTransaction.CurrencyTypeValueId.Value );
                        }
                        if (currencyTypeValue != null)
                        {
                            transaction.CurrencyTypeValueId = currencyTypeValue.Id;
                        }

                        var creditCardTypevalue = payment.CreditCardTypeValue;
                        if (creditCardTypevalue == null && scheduledTransaction.CreditCardTypeValueId.HasValue)
                        {
                            creditCardTypevalue = DefinedValueCache.Read( scheduledTransaction.CreditCardTypeValueId.Value );
                        }
                        if (creditCardTypevalue != null)
                        {
                            transaction.CreditCardTypeValueId = creditCardTypevalue.Id;
                        }

                        //transaction.SourceTypeValueId = DefinedValueCache.Read( sourceGuid ).Id;

                        // Try to allocate the amount of the transaction based on the current scheduled transaction accounts
                        decimal remainingAmount = payment.Amount;
                        foreach ( var detail in scheduledTransaction.ScheduledTransactionDetails )
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
                                detail.Amount = remainingAmount;
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
                            var transactionDetail = transaction.TransactionDetails
                                .OrderByDescending( d => d.Amount )
                                .First();
                            if ( transactionDetail != null )
                            {
                                transactionDetail.Amount += remainingAmount;
                            }
                        }

                        // Get the batch 
                        var batch = batchService.Get(
                            batchNamePrefix,
                            payment.CurrencyTypeValue,
                            payment.CreditCardTypeValue,
                            transaction.TransactionDateTime.Value,
                            gateway.BatchTimeOffset,
                            batches );

                        batch.ControlAmount += transaction.TotalAmount;
                        batch.Transactions.Add( transaction );

                        // Add summary
                        if ( !batchSummary.ContainsKey( batch.Guid ) )
                        {
                            batchSummary.Add( batch.Guid, new List<Payment>() );
                        }
                        batchSummary[batch.Guid].Add( payment );
                    }
                }
            }

            rockContext.SaveChanges();

            StringBuilder sb = new StringBuilder();
            foreach ( var batch in batchSummary )
            {
                string batchName = batches
                    .Where( b => b.Guid.Equals( batch.Key ) )
                    .Select( b => b.Name )
                    .FirstOrDefault();

                sb.AppendFormat( "<li>}{0}: {1} Transactions totaling: {2}",
                    batchName, batch.Value.Count.ToString( "N0" ),
                    batch.Value.Select( p => p.Amount ).Sum().ToString( "C2" ) );
            }

            return sb.ToString();
        }

    }
}