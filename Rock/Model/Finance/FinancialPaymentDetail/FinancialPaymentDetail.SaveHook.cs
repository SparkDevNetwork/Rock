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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPaymentDetail SaveHook
    /// </summary>
    public partial class FinancialPaymentDetail
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialPaymentDetail>
        {

            private History.HistoryChangeList HistoryChangeList { get; set; }

            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;
                HistoryChangeList = new History.HistoryChangeList();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            History.EvaluateChange( HistoryChangeList, "Account Number", string.Empty, Entity.AccountNumberMasked );
                            History.EvaluateChange( HistoryChangeList, "Currency Type", ( int? ) null, Entity.CurrencyTypeValue, Entity.CurrencyTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Credit Card Type", ( int? ) null, Entity.CreditCardTypeValue, Entity.CreditCardTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Name On Card", string.Empty, Entity.NameOnCard );
                            History.EvaluateChange( HistoryChangeList, "Expiration Month", string.Empty, Entity.ExpirationMonth.ToStringSafe(), true );
                            History.EvaluateChange( HistoryChangeList, "Expiration Year", string.Empty, Entity.ExpirationYear.ToStringSafe(), true );
                            History.EvaluateChange( HistoryChangeList, "Billing Location", string.Empty, History.GetValue<Location>( Entity.BillingLocation, Entity.BillingLocationId, rockContext ) );
                            break;
                        }
                    case EntityContextState.Modified:
                    case EntityContextState.Deleted:
                        {
                            History.EvaluateChange( HistoryChangeList, "Account Number", Entry.OriginalValues[nameof( Entity.AccountNumberMasked )].ToStringSafe(), Entity.AccountNumberMasked );
                            History.EvaluateChange( HistoryChangeList, "Currency Type", Entry.OriginalValues[nameof( Entity.CurrencyTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.CurrencyTypeValue, Entity.CurrencyTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Credit Card Type", Entry.OriginalValues[nameof( Entity.CreditCardTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.CreditCardTypeValue, Entity.CreditCardTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Name On Card", Entry.OriginalValues[nameof( Entity.NameOnCard )].ToStringSafe(), Entity.NameOnCard );
                            History.EvaluateChange( HistoryChangeList, "Expiration Month", Entry.OriginalValues[nameof( Entity.ExpirationMonth )].ToStringSafe(), Entity.ExpirationMonth.ToStringSafe(), true );
                            History.EvaluateChange( HistoryChangeList, "Expiration Year", Entry.OriginalValues[nameof( Entity.ExpirationYear )].ToStringSafe(), Entity.ExpirationYear.ToStringSafe(), true );
                            History.EvaluateChange( HistoryChangeList, "Billing Location", History.GetValue<Location>( null, Entry.OriginalValues[nameof( Entity.BillingLocationId )].ToStringSafe().AsIntegerOrNull(), rockContext ), History.GetValue<Location>( Entity.BillingLocation, Entity.BillingLocationId, rockContext ) );
                            break;
                        }
                }

                if ( Entry.State == EntityContextState.Added || Entry.State == EntityContextState.Modified )
                {
                    // Ensure that CurrencyTypeValueId is set. The UI tries to prevent it, but just in case, if it isn't, set it to Unknown
                    if ( !Entity.CurrencyTypeValueId.HasValue )
                    {
                            Entity.CurrencyTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid() )?.Id;
                    }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;
                    var associatedTransactions = new FinancialTransactionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t => t.FinancialPaymentDetailId == Entity.Id )
                        .Select( t => new { t.Id, t.BatchId } )
                        .ToList();

                    foreach ( var transaction in associatedTransactions )
                    {

                        // Save Transaction History changes.
                        HistoryService.SaveChanges( rockContext,
                            typeof( FinancialTransaction ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            transaction.Id,
                            HistoryChangeList,
                            true,
                            Entity.ModifiedByPersonAliasId,
                            DbContext.SourceOfChange );

                        // Save Batch History changes.
                        var batchHistory = new History.HistoryChangeList();
                        batchHistory.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Transaction" );

                        HistoryService.SaveChanges( rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            transaction.BatchId.Value,
                            batchHistory,
                            string.Empty,
                            typeof( FinancialTransaction ),
                            transaction.Id,
                            true,
                            Entity.ModifiedByPersonAliasId,
                            DbContext.SourceOfChange );
                    }
                }

                base.PostSave();
            }
        }
    }
}