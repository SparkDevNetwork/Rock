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
using System.Linq;
using System.Threading.Tasks;

using Rock.Data;

namespace Rock.Model
{
    public partial class FinancialTransaction
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialTransaction>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;

                Entity.HistoryChangeList = new History.HistoryChangeList();
                Entity.BatchHistoryChangeList = new Dictionary<int, History.HistoryChangeList>();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" );

                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );

                            History.EvaluateChange( Entity.HistoryChangeList, "Authorized Person", string.Empty, person );
                            History.EvaluateChange( Entity.HistoryChangeList, "Batch", string.Empty, History.GetValue<FinancialBatch>( Entity.Batch, Entity.BatchId, rockContext ) );
                            History.EvaluateChange( Entity.HistoryChangeList, "Gateway", string.Empty, History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Date/Time", ( DateTime? ) null, Entity.TransactionDateTime );
                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Code", string.Empty, Entity.TransactionCode );
                            History.EvaluateChange( Entity.HistoryChangeList, "Summary", string.Empty, Entity.Summary );
                            History.EvaluateChange( Entity.HistoryChangeList, "Type", ( int? ) null, Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Source", ( int? ) null, Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Scheduled Transaction Id", ( int? ) null, Entity.ScheduledTransactionId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Processed By", string.Empty, History.GetValue<PersonAlias>( Entity.ProcessedByPersonAlias, Entity.ProcessedByPersonAliasId, rockContext ) );
                            History.EvaluateChange( Entity.HistoryChangeList, "Processed Date/Time", ( DateTime? ) null, Entity.ProcessedDateTime );
                            History.EvaluateChange( Entity.HistoryChangeList, "Status", string.Empty, Entity.Status );
                            History.EvaluateChange( Entity.HistoryChangeList, "Status Message", string.Empty, Entity.StatusMessage );

                            int? batchId = Entity.Batch != null ? Entity.Batch.Id : Entity.BatchId;
                            if ( batchId.HasValue )
                            {
                                var batchChanges = new History.HistoryChangeList();
                                batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" ).SetNewValue( $"{Entity.TotalAmount.FormatAsCurrency()} for {person}" );
                                Entity.BatchHistoryChangeList.Add( batchId.Value, batchChanges );
                            }

                            break;
                        }
                    case EntityContextState.Modified:
                        {
                            string origPerson = History.GetValue<PersonAlias>( null, Entry.OriginalValues[nameof( Entity.AuthorizedPersonAliasId )].ToStringSafe().AsIntegerOrNull(), rockContext );
                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );
                            History.EvaluateChange( Entity.HistoryChangeList, "Authorized Person", origPerson, person );

                            int? origBatchId = OriginalValues[nameof( Entity.BatchId )].ToStringSafe().AsIntegerOrNull();
                            int? batchId = Entity.Batch != null ? Entity.Batch.Id : Entity.BatchId;
                            if ( !batchId.Equals( origBatchId ) )
                            {
                                string origBatch = History.GetValue<FinancialBatch>( null, origBatchId, rockContext );
                                string batch = History.GetValue<FinancialBatch>( Entity.Batch, Entity.BatchId, rockContext );
                                History.EvaluateChange( Entity.HistoryChangeList, "Batch", origBatch, batch );
                            }

                            int? origGatewayId = OriginalValues[nameof( Entity.FinancialGatewayId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.FinancialGatewayId.Equals( origGatewayId ) )
                            {
                                History.EvaluateChange( Entity.HistoryChangeList, "Gateway", History.GetValue<FinancialGateway>( null, origGatewayId, rockContext ), History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            }

                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Date/Time", OriginalValues[nameof( Entity.TransactionDateTime )].ToStringSafe().AsDateTime(), Entity.TransactionDateTime );
                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Code", OriginalValues[nameof( Entity.TransactionCode )].ToStringSafe(), Entity.TransactionCode );
                            History.EvaluateChange( Entity.HistoryChangeList, "Summary", OriginalValues[nameof( Entity.Summary )].ToStringSafe(), Entity.Summary );
                            History.EvaluateChange( Entity.HistoryChangeList, "Type", OriginalValues[nameof( Entity.TransactionTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Source", OriginalValues[nameof( Entity.SourceTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Scheduled Transaction Id", OriginalValues[nameof( Entity.ScheduledTransactionId )].ToStringSafe().AsIntegerOrNull(), Entity.ScheduledTransactionId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Processed By", OriginalValues[nameof( Entity.ProcessedByPersonAliasId )].ToStringSafe().AsIntegerOrNull(), Entity.ProcessedByPersonAlias, Entity.ProcessedByPersonAliasId, rockContext );
                            History.EvaluateChange( Entity.HistoryChangeList, "Processed Date/Time", OriginalValues[nameof( Entity.ProcessedDateTime )].ToStringSafe().AsDateTime(), Entity.ProcessedDateTime );
                            History.EvaluateChange( Entity.HistoryChangeList, "Status", OriginalValues[nameof( Entity.Status )].ToStringSafe(), Entity.Status );
                            History.EvaluateChange( Entity.HistoryChangeList, "Status Message", OriginalValues[nameof( Entity.StatusMessage )].ToStringSafe(), Entity.StatusMessage );

                            if ( !batchId.Equals( origBatchId ) )
                            {
                                var batchChanges = new History.HistoryChangeList();

                                if ( origBatchId.HasValue )
                                {
                                    batchChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" ).SetOldValue( $"{Entity.TotalAmount.FormatAsCurrency()} for {person}" );
                                }
                                if ( batchId.HasValue )
                                {
                                    batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" ).SetNewValue( $"{Entity.TotalAmount.FormatAsCurrency()} for {person}" );
                                }

                                Entity.BatchHistoryChangeList.Add( batchId.Value, batchChanges );
                            }
                            else
                            {
                                if ( batchId.HasValue )
                                {
                                    var batchChanges = new History.HistoryChangeList();
                                    batchChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, $"Transaction Id:{Entity.Id}" );
                                    Entity.BatchHistoryChangeList.Add( batchId.Value, batchChanges );
                                }
                            }
                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

                            int? batchId = Entity.Batch != null ? Entity.Batch.Id : Entity.BatchId;
                            if ( batchId.HasValue )
                            {
                                string batch = History.GetValue<FinancialBatch>( Entity.Batch, Entity.BatchId, rockContext );
                                string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );
                                var batchChanges = new History.HistoryChangeList();
                                batchChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" ).SetOldValue( $"{Entity.TotalAmount.FormatAsCurrency()} for {person}" );

                                Entity.BatchHistoryChangeList.Add( batchId.Value, batchChanges );
                            }

                            /* 01/06/2022 - Shaun Cummings
                             * 
                             * FinancialTransactionImage records have a cascade delete relationship to FinancialTransaction records, 
                             * but deleting the parent does not automatically trigger the SaveHook logic of the child, so we need to
                             * do that explicitly when the parent record is being deleted.
                             * 
                             * Reason:  To ensure that BinaryFile records associated with this transaction are cleaned up and proper
                             * history records are maintained.
                             * 
                             * */

                            var childImages = new FinancialTransactionImageService( rockContext ).Queryable().Where( a => a.TransactionId == Entity.Id );
                            foreach ( var image in childImages )
                            {
                                FinancialTransactionImage.SaveHook.ProcessImageDeletion( image, rockContext );
                            }

                            // If a FinancialPaymentDetail was linked to this FinancialTransaction and is now orphaned, delete it.
                            var financialPaymentDetailService = new FinancialPaymentDetailService( rockContext );
                            financialPaymentDetailService.DeleteOrphanedFinancialPaymentDetail( Entry );

                            break;
                        }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) DbContext;

                if ( Entity.HistoryChangeList?.Any() == true )
                {
                    // Save transaction history.
                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.Id,
                        Entity.HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );
                }

                if ( Entity.BatchHistoryChangeList != null )
                {
                    // Save batch history.
                    foreach ( var keyVal in Entity.BatchHistoryChangeList )
                    {
                        if ( keyVal.Value.Any() )
                        {
                            HistoryService.SaveChanges( rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                                keyVal.Key,
                                keyVal.Value,
                                string.Empty,
                                typeof( FinancialTransaction ),
                                Entity.Id,
                                true,
                                Entity.ModifiedByPersonAliasId,
                                rockContext.SourceOfChange );
                        }
                    }
                }

                if ( null != Entity.RefundDetails || Entity.TotalAmount <= 0 )
                {
                    // The data context operation doesn't need to wait for this to compelete
                    Task.Run( () => StreakTypeService.HandleFinancialTransactionRecord( Entity.Id ) );
                }

                base.PostSave();
            }
        }
    }
}
