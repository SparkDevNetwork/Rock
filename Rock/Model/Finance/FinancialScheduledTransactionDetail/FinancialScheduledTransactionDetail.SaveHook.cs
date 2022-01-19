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
    /// <summary>
    /// FinancialScheduledTransactionDetail SaveHook
    /// </summary>
    public partial class FinancialScheduledTransactionDetail
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialScheduledTransactionDetail>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;
                Entity.HistoryChangeList = new History.HistoryChangeList();

                var scheduledTransaction = Entity.ScheduledTransaction ?? new FinancialScheduledTransactionService( rockContext ).Get( Entity.ScheduledTransactionId );

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, acct ).SetNewValue( Entity.Amount.FormatAsCurrency( scheduledTransaction?.ForeignCurrencyCodeValueId ) );
                            break;
                        }
                    case EntityContextState.Modified:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );

                            int? accountId = Entity.Account != null ? Entity.Account.Id : Entity.AccountId;
                            int? origAccountId = Entry.OriginalValues[nameof( Entity.AccountId )].ToStringSafe().AsIntegerOrNull();
                            if ( !accountId.Equals( origAccountId ) )
                            {
                                History.EvaluateChange( Entity.HistoryChangeList, "Account", History.GetValue<FinancialAccount>( null, origAccountId, rockContext ), acct );
                            }

                            var originalCurrencyCodeValueId = scheduledTransaction?.ForeignCurrencyCodeValueId;
                            if ( scheduledTransaction != null )
                            {
                                var originalScheduledTransactionEntry = rockContext.ChangeTracker
                                    .Entries<FinancialScheduledTransaction>()
                                    .Where( s => ( int ) s.OriginalValues[nameof( Entity.Id )] == scheduledTransaction.Id )
                                    .FirstOrDefault();

                                if ( originalScheduledTransactionEntry != null )
                                {
                                    originalCurrencyCodeValueId = ( int? ) originalScheduledTransactionEntry.OriginalValues[nameof( FinancialScheduledTransaction.ForeignCurrencyCodeValueId )];
                                }
                            }

                            History.EvaluateChange( Entity.HistoryChangeList, acct + " Amount", Entry.OriginalValues[nameof( Entity.Amount )].ToStringSafe().AsDecimal().FormatAsCurrency( originalCurrencyCodeValueId ), Entity.Amount.FormatAsCurrency( scheduledTransaction?.ForeignCurrencyCodeValueId ) );
                            History.EvaluateChange( Entity.HistoryChangeList, acct + " Fee Coverage Amount", ( Entry.OriginalValues[nameof( Entity.FeeCoverageAmount )] as int? ).FormatAsCurrency(), Entity.FeeCoverageAmount.FormatAsCurrency() );

                            break;
                        }
                    case EntityContextState.Deleted:
                        {
                            string acct = History.GetValue<FinancialAccount>( Entity.Account, Entity.AccountId, rockContext );
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, acct ).SetOldValue( Entity.Amount.FormatAsCurrency( scheduledTransaction?.ForeignCurrencyCodeValueId ) );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( Entity.HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;

                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialScheduledTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.ScheduledTransactionId,
                        Entity.HistoryChangeList,
                        true, Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();
            }
       }
    }
}