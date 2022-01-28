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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    public partial class FinancialTransactionDetail
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialTransactionDetail>
        {
            public History.HistoryChangeList HistoryChangeList { get; set; }

            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;
                HistoryChangeList = new History.HistoryChangeList();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );
                            HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, acct ).SetNewValue( Entity.Amount.FormatAsCurrency() );
                            break;
                        }
                    case EntityContextState.Modified:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );

                            int? accountId = Entity.Account != null ? Entity.Account.Id : Entity.AccountId;
                            int? origAccountId = OriginalValues[nameof( Entity.AccountId )].ToStringSafe().AsIntegerOrNull();
                            if ( !accountId.Equals( origAccountId ) )
                            {
                                History.EvaluateChange( HistoryChangeList, "Account", History.GetValue<FinancialAccount>( null, origAccountId, rockContext ), acct );
                            }

                            History.EvaluateChange( HistoryChangeList, acct, OriginalValues[nameof( Entity.Amount )].ToStringSafe().AsDecimal().FormatAsCurrency(), Entity.Amount.FormatAsCurrency() );
                            History.EvaluateChange( HistoryChangeList, acct, OriginalValues[nameof( Entity.FeeAmount )].ToStringSafe().AsDecimal().FormatAsCurrency(), Entity.FeeAmount.FormatAsCurrency() );
                            History.EvaluateChange( HistoryChangeList, acct, OriginalValues[nameof( Entity.FeeCoverageAmount )].ToStringSafe().AsDecimal().FormatAsCurrency(), Entity.FeeCoverageAmount.FormatAsCurrency() );

                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );
                            HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, acct ).SetOldValue( Entity.Amount.FormatAsCurrency() );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) DbContext;

                if ( HistoryChangeList?.Any() == true )
                {
                    // Save transaction history.
                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.TransactionId,
                        HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );

                    var txn = new FinancialTransactionService( rockContext ).GetSelect( Entity.TransactionId, s => new { s.Id, s.BatchId } );
                    if ( txn != null && txn.BatchId != null )
                    {
                        // Save batch history.
                        var batchHistory = new History.HistoryChangeList();
                        batchHistory.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, $"Transaction ID:{txn.Id}" );

                        HistoryService.SaveChanges( rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            txn.BatchId.Value,
                            batchHistory,
                            string.Empty,
                            typeof( FinancialTransaction ),
                            Entity.TransactionId,
                            true,
                            Entity.ModifiedByPersonAliasId,
                            rockContext.SourceOfChange );
                    }
                }

                base.PostSave();
            }
        }
    }
}