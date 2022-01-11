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
using System.Linq;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// FinancialScheduledTransaction SaveHoook
    /// </summary>
    public partial class FinancialScheduledTransaction
    {
        /// <inheritdoc/>
        internal class SaveHook : EntitySaveHook<FinancialScheduledTransaction>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) DbContext;
                Entity.HistoryChangeList = new History.HistoryChangeList();

                switch ( Entry.State )
                {
                    case EntityContextState.Added:
                        {
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" );

                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );

                            History.EvaluateChange( Entity.HistoryChangeList, "Authorized Person", string.Empty, person );
                            History.EvaluateChange( Entity.HistoryChangeList, "Gateway", string.Empty, History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            History.EvaluateChange( Entity.HistoryChangeList, "Gateway Schedule Id", string.Empty, Entity.GatewayScheduleId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Code", string.Empty, Entity.TransactionCode );
                            History.EvaluateChange( Entity.HistoryChangeList, "Summary", string.Empty, Entity.Summary );
                            History.EvaluateChange( Entity.HistoryChangeList, "Type", ( null as int? ), Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Source", ( null as int? ), Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Frequency", ( null as int? ), Entity.TransactionFrequencyValue, Entity.TransactionFrequencyValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Start Date", ( null as DateTime? ), Entity.StartDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "End Date", ( null as DateTime? ), Entity.EndDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "Number of Payments", ( null as int? ), Entity.NumberOfPayments );
                            History.EvaluateChange( Entity.HistoryChangeList, "Is Active", ( null as bool? ), Entity.IsActive );
                            History.EvaluateChange( Entity.HistoryChangeList, "Card Reminder Date", ( null as DateTime? ), Entity.CardReminderDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "Last Reminded Date", ( null as DateTime? ), Entity.LastRemindedDate );
                            var isOrganizationCurrency = new RockCurrencyCodeInfo( Entity.ForeignCurrencyCodeValueId ).IsOrganizationCurrency;
                            if ( !isOrganizationCurrency )
                            {
                                History.EvaluateChange( Entity.HistoryChangeList, "Currency Code", ( null as int? ), Entity.ForeignCurrencyCodeValue, Entity.ForeignCurrencyCodeValueId );
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            string origPerson = History.GetValue<PersonAlias>( null, Entry.OriginalValues[nameof( Entity.AuthorizedPersonAliasId )].ToStringSafe().AsIntegerOrNull(), rockContext );
                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );
                            History.EvaluateChange( Entity.HistoryChangeList, "Authorized Person", origPerson, person );

                            int? origGatewayId = Entry.OriginalValues[nameof( Entity.FinancialGatewayId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.FinancialGatewayId.Equals( origGatewayId ) )
                            {
                                History.EvaluateChange( Entity.HistoryChangeList, "Gateway", History.GetValue<FinancialGateway>( null, origGatewayId, rockContext ), History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            }

                            History.EvaluateChange( Entity.HistoryChangeList, "Gateway Schedule Id", Entry.OriginalValues[nameof( Entity.GatewayScheduleId )].ToStringSafe(), Entity.GatewayScheduleId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Transaction Code", Entry.OriginalValues[nameof( Entity.TransactionCode) ].ToStringSafe(), Entity.TransactionCode );
                            History.EvaluateChange( Entity.HistoryChangeList, "Summary", Entry.OriginalValues[nameof( Entity.Summary )].ToStringSafe(), Entity.Summary );
                            History.EvaluateChange( Entity.HistoryChangeList, "Type", Entry.OriginalValues[nameof( Entity.TransactionTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Source", Entry.OriginalValues[nameof( Entity.SourceTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Frequency", Entry.OriginalValues[nameof( Entity.TransactionFrequencyValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TransactionFrequencyValue, Entity.TransactionFrequencyValueId );
                            History.EvaluateChange( Entity.HistoryChangeList, "Start Date", Entry.OriginalValues[nameof( Entity.StartDate )].ToStringSafe().AsDateTime(), Entity.StartDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "End Date", Entry.OriginalValues[nameof( Entity.EndDate )].ToStringSafe().AsDateTime(), Entity.EndDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "Number of Payments", Entry.OriginalValues[nameof( Entity.EndDate )].ToStringSafe().AsIntegerOrNull(), Entity.NumberOfPayments );
                            History.EvaluateChange( Entity.HistoryChangeList, "Is Active", Entry.OriginalValues[nameof( Entity.IsActive )].ToStringSafe().AsBooleanOrNull(), Entity.IsActive );
                            History.EvaluateChange( Entity.HistoryChangeList, "Card Reminder Date", Entry.OriginalValues[nameof( Entity.CardReminderDate )].ToStringSafe().AsDateTime(), Entity.CardReminderDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "Last Reminded Date", Entry.OriginalValues[nameof( Entity.LastRemindedDate )].ToStringSafe().AsDateTime(), Entity.LastRemindedDate );
                            History.EvaluateChange( Entity.HistoryChangeList, "Currency Code", Entry.OriginalValues[nameof( Entity.ForeignCurrencyCodeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.ForeignCurrencyCodeValue, Entity.ForeignCurrencyCodeValueId );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            Entity.HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

                            // If a FinancialPaymentDetail was linked to this FinancialScheduledTransaction and is now orphaned, delete it.
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
                if ( Entity.HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;

                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialScheduledTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.Id,
                        Entity.HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();
            }
        }
    }
}