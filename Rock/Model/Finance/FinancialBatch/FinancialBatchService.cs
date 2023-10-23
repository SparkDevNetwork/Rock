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
using System.Data.SqlClient;
using System.Linq;

using Rock.Data;
using Rock.Financial;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialBatch"/> entity objects.
    /// </summary>
    public partial class FinancialBatchService
    {
        /// <summary>
        /// Creates the standard history changes for a new batch. If the batch
        /// has already been saved (Id not equal to 0) then no history changes
        /// will be created.
        /// </summary>
        /// <param name="batch">The batch to be evaluated</param>
        /// <param name="changes">The list of history changes to be saved with the batch.</param>
        public static void EvaluateNewBatchHistory( FinancialBatch batch, History.HistoryChangeList changes )
        {
            if ( batch == null )
            {
                throw new ArgumentNullException( nameof( batch ) );
            }

            if ( changes == null )
            {
                throw new ArgumentNullException( nameof( changes ) );
            }

            if ( batch.Id == 0 )
            {
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                History.EvaluateChange( changes, "Batch Name", string.Empty, batch.Name );
                History.EvaluateChange( changes, "Status", null, batch.Status );
                History.EvaluateChange( changes, "Start Date/Time", null, batch.BatchStartDateTime );
                History.EvaluateChange( changes, "End Date/Time", null, batch.BatchEndDateTime );
            }
        }

        /// <summary>
        /// <para>
        /// Gets the first FinancialBatch matching the specified filter parameters,
        /// or creates a new FinancialBatch if one isn't found.
        /// </para>
        /// <para>
        /// If a new batch is created it will not have been saved to the database
        /// yet and will have an Id of <c>0</c>.</para>
        /// </summary>
        /// <param name="transaction">The transaction that will be saved to the database. It should be fully populated with everything except BatchId.</param>
        /// <param name="namePrefix">The prefix that should be applied to the batch name. This will only be used with supported gateways.</param>
        /// <param name="nameSuffix">The suffix that should be applied to the batch name. This will only be used with supported gateways.</param>
        /// <returns>The financial batch that should be used for the transaction.</returns>
        public FinancialBatch GetForNewTransaction( FinancialTransaction transaction, string namePrefix = null, string nameSuffix = null )
        {
            // Validate that the transaction provides the required information.
            if ( transaction == null )
            {
                throw new ArgumentNullException( nameof( transaction ) );
            }

            if ( !transaction.TransactionDateTime.HasValue && !transaction.FutureProcessingDateTime.HasValue )
            {
                throw new ArgumentException( "Transaction must have either TransactionDateTime or FutureProcessingDateTime specified.", nameof( transaction ) );
            }

            // Get all the additional bits we need.
            FinancialGateway gateway;

            if ( transaction.FinancialGateway != null )
            {
                gateway = transaction.FinancialGateway;
            }
            else if ( transaction.FinancialGatewayId.HasValue )
            {
                gateway = new FinancialGatewayService( ( RockContext ) Context ).Get( transaction.FinancialGatewayId.Value );
            }
            else
            {
                gateway = null;
            }

            var currencyType = transaction.FinancialPaymentDetail?.CurrencyTypeValueId.HasValue == true
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value )
                : null;

            var creditCardType = transaction.FinancialPaymentDetail?.CreditCardTypeValueId.HasValue == true
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CreditCardTypeValueId.Value )
                : null;

            var transactionDateTime = transaction.TransactionDateTime ?? transaction.FutureProcessingDateTime.Value;

            // Check if the gateway supports automatic settlement.
            if ( gateway?.GetGatewayComponent() is ISettlementGateway settlementGateway )
            {
                var batchId = settlementGateway.GetSettlementBatchId( gateway, transaction );

                if ( batchId.HasValue )
                {
                    return Get( batchId.Value );
                }
            }

            return GetInternal( namePrefix ?? string.Empty,
                nameSuffix ?? string.Empty,
                currencyType,
                creditCardType,
                transactionDateTime,
                gateway?.GetBatchTimeOffset() ?? TimeSpan.Zero,
                gateway?.BatchDayOfWeek,
                null );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="namePrefix">The name prefix.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <param name="creditCardType">Type of the credit card.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetForNewTransaction method." )]
        public FinancialBatch Get( string namePrefix, DefinedValueCache currencyType, DefinedValueCache creditCardType,
            DateTime transactionDate, TimeSpan batchTimeOffset, List<FinancialBatch> batches = null )
        {
            return Get( namePrefix ?? string.Empty, string.Empty, currencyType, creditCardType, transactionDate, batchTimeOffset, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="namePrefix">The name prefix.</param>
        /// <param name="nameSuffix">The name suffix.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <param name="creditCardType">Type of the credit card.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetForNewTransaction method." )]
        public FinancialBatch Get( string namePrefix, string nameSuffix, DefinedValueCache currencyType, DefinedValueCache creditCardType,
            DateTime transactionDate, TimeSpan batchTimeOffset, List<FinancialBatch> batches = null )
        {
            return Get( namePrefix ?? string.Empty, nameSuffix ?? string.Empty, currencyType, creditCardType, transactionDate, batchTimeOffset, null, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="namePrefix">The name prefix.</param>
        /// <param name="nameSuffix">The name suffix.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <param name="creditCardType">Type of the credit card.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <param name="batchWeeklyDayOfWeek">If batching weekly, the day of the week the batch should begin</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetForNewTransaction method." )]
        public FinancialBatch Get( string namePrefix, string nameSuffix, DefinedValueCache currencyType, DefinedValueCache creditCardType,
        DateTime transactionDate, TimeSpan batchTimeOffset, DayOfWeek? batchWeeklyDayOfWeek, List<FinancialBatch> batches = null )
        {
            return GetInternal( namePrefix, nameSuffix, currencyType, creditCardType, transactionDate, batchTimeOffset, batchWeeklyDayOfWeek, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="namePrefix">The name prefix.</param>
        /// <param name="nameSuffix">The name suffix.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <param name="creditCardType">Type of the credit card.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <param name="batchWeeklyDayOfWeek">If batching weekly, the day of the week the batch should begin</param>
        /// <returns></returns>
        internal FinancialBatch GetInternal( string namePrefix, string nameSuffix, DefinedValueCache currencyType, DefinedValueCache creditCardType,
        DateTime transactionDate, TimeSpan batchTimeOffset, DayOfWeek? batchWeeklyDayOfWeek, List<FinancialBatch> batches = null )
        {
            // Use the credit card type's batch name suffix, or if that doesn't exist, use the currency type value
            string ccSuffix = string.Empty;

            if ( creditCardType != null )
            {
                ccSuffix = creditCardType.GetAttributeValue( "BatchNameSuffix" );
                if ( string.IsNullOrWhiteSpace( ccSuffix ) )
                {
                    ccSuffix = creditCardType.Value;
                }
            }

            if ( string.IsNullOrWhiteSpace( ccSuffix ) && currencyType != null )
            {
                ccSuffix = currencyType.Value;
            }

            string batchName = namePrefix.Trim() + ( string.IsNullOrWhiteSpace( ccSuffix ) ? "" : " " + ccSuffix ) + nameSuffix;

            return GetByNameAndDateInternal( batchName.Truncate(50), transactionDate, batchTimeOffset, batchWeeklyDayOfWeek, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="batchName">Name of the batch.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetForNewTransaction method." )]
        public FinancialBatch GetByNameAndDate( string batchName, DateTime transactionDate, TimeSpan batchTimeOffset, List<FinancialBatch> batches = null )
        {
            return GetByNameAndDate( batchName, transactionDate, batchTimeOffset, null, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="batchName">Name of the batch.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <param name="batchWeeklyDayOfWeek">If batching weekly, the day of the week the batch should begin</param>
        /// <returns></returns>
        [RockObsolete( "1.16" )]
        [Obsolete( "Use the GetForNewTransaction method." )]
        public FinancialBatch GetByNameAndDate( string batchName, DateTime transactionDate, TimeSpan batchTimeOffset, DayOfWeek? batchWeeklyDayOfWeek, List<FinancialBatch> batches = null )
        {
            return GetByNameAndDateInternal( batchName, transactionDate, batchTimeOffset, batchWeeklyDayOfWeek, batches );
        }

        /// <summary>
        /// Gets the first FinancialBatch matching the specified filter parameters, or creates a new FinancialBatch if one isn't found.
        /// </summary>
        /// <param name="batchName">Name of the batch.</param>
        /// <param name="transactionDate">The transaction date.</param>
        /// <param name="batchTimeOffset">The batch time offset.</param>
        /// <param name="batches">The batches.</param>
        /// <param name="batchWeeklyDayOfWeek">If batching weekly, the day of the week the batch should begin</param>
        /// <returns></returns>
        private FinancialBatch GetByNameAndDateInternal( string batchName, DateTime transactionDate, TimeSpan batchTimeOffset, DayOfWeek? batchWeeklyDayOfWeek, List<FinancialBatch> batches = null )
        {
            FinancialBatch batch = null;

            // If a list of batches was passed, search those first
            if ( batches != null )
            {
                batch = batches
                    .Where( b =>
                        b.Status == BatchStatus.Open &&
                        b.BatchStartDateTime <= transactionDate &&
                        b.BatchEndDateTime > transactionDate &&
                        b.Name == batchName )
                    .OrderByDescending( b => b.BatchStartDateTime )
                    .FirstOrDefault();

                if ( batch != null )
                {
                    return batch;
                }
            }

            // If batch was not found in existing list, search database
            batch = Queryable()
                .Where( b =>
                    b.Status == BatchStatus.Open &&
                    b.BatchStartDateTime <= transactionDate &&
                    b.BatchEndDateTime > transactionDate &&
                    b.Name == batchName )
                .OrderByDescending( b => b.BatchStartDateTime )
                .FirstOrDefault();

            // If still no batch, create a new one
            if ( batch == null )
            {
                batch = new FinancialBatch();
                batch.Guid = Guid.NewGuid();
                batch.Name = batchName;
                batch.Status = BatchStatus.Open;

                var isWeekly = batchWeeklyDayOfWeek.HasValue;
                var batchStartDateTime = transactionDate.Date.Add( batchTimeOffset );

                if ( isWeekly )
                {
                    var dayOfWeekDifference = batchWeeklyDayOfWeek.Value - batchStartDateTime.DayOfWeek;
                    batchStartDateTime = batchStartDateTime.AddDays( dayOfWeekDifference );

                    if ( batchStartDateTime > transactionDate )
                    {
                        batchStartDateTime = batchStartDateTime.AddDays( -7 );
                    }

                    batch.BatchEndDateTime = batchStartDateTime.AddDays( 7 );
                }
                else
                {
                    if ( batchStartDateTime > transactionDate )
                    {
                        batchStartDateTime = batchStartDateTime.AddDays( -1 );
                    }

                    batch.BatchEndDateTime = batchStartDateTime.AddDays( 1 );
                }

                batch.BatchStartDateTime = batchStartDateTime;
                batch.ControlAmount = 0;
                Add( batch );
            }

            // Add the batch to the list
            if ( batches != null )
            {
                batches.Add( batch );
            }

            return batch;
        }

        /// <summary>
        /// Use this to increment <see cref="FinancialBatch.ControlAmount"/> in cases where a transaction amount should increase the control
        /// amount. In the event that multiple transactions charge simultaneously, it is possible that the control amount additions will
        /// be overwritten. This method ensures that the control amount is safely updated and no data is lost.
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="newTransactionAmount">Use a negative amount to decrease the control amount</param>
        /// <param name="history"></param>
        public void IncrementControlAmount( int batchId, decimal newTransactionAmount, History.HistoryChangeList history )
        {
            var result = Context.Database.SqlQuery<ControlAmountUpdateResult>(
@"UPDATE [FinancialBatch]
SET [ControlAmount] = [ControlAmount] + @newTransactionAmount
OUTPUT 
    deleted.[ControlAmount] [OldAmount],
    inserted.[ControlAmount] [NewAmount]
WHERE [Id] = @batchId;",
                new SqlParameter( "@newTransactionAmount", newTransactionAmount ),
                new SqlParameter( "@batchId", batchId ) ).FirstOrDefault();

            if ( history != null && result != null )
            {
                History.EvaluateChange( history, "Control Amount", result.OldAmount.FormatAsCurrency(), result.NewAmount.FormatAsCurrency() );
            }
        }

        /// <summary>
        /// Type used to get the control amount changes from SQL Server within <see cref="IncrementControlAmount"/>
        /// </summary>
        private class ControlAmountUpdateResult {
            public decimal OldAmount { get; set; }
            public decimal NewAmount { get; set; }
        }
    }
}