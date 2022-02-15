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
                            HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" );

                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );

                            History.EvaluateChange( HistoryChangeList, "Authorized Person", string.Empty, person );
                            History.EvaluateChange( HistoryChangeList, "Gateway", string.Empty, History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            History.EvaluateChange( HistoryChangeList, "Gateway Schedule Id", string.Empty, Entity.GatewayScheduleId );
                            History.EvaluateChange( HistoryChangeList, "Transaction Code", string.Empty, Entity.TransactionCode );
                            History.EvaluateChange( HistoryChangeList, "Summary", string.Empty, Entity.Summary );
                            History.EvaluateChange( HistoryChangeList, "Type", ( null as int? ), Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Source", ( null as int? ), Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Frequency", ( null as int? ), Entity.TransactionFrequencyValue, Entity.TransactionFrequencyValueId );
                            History.EvaluateChange( HistoryChangeList, "Start Date", ( null as DateTime? ), Entity.StartDate );
                            History.EvaluateChange( HistoryChangeList, "End Date", ( null as DateTime? ), Entity.EndDate );
                            History.EvaluateChange( HistoryChangeList, "Number of Payments", ( null as int? ), Entity.NumberOfPayments );
                            History.EvaluateChange( HistoryChangeList, "Is Active", ( null as bool? ), Entity.IsActive );
                            History.EvaluateChange( HistoryChangeList, "Card Reminder Date", ( null as DateTime? ), Entity.CardReminderDate );
                            History.EvaluateChange( HistoryChangeList, "Last Reminded Date", ( null as DateTime? ), Entity.LastRemindedDate );
                            var isOrganizationCurrency = new RockCurrencyCodeInfo( Entity.ForeignCurrencyCodeValueId ).IsOrganizationCurrency;
                            if ( !isOrganizationCurrency )
                            {
                                History.EvaluateChange( HistoryChangeList, "Currency Code", ( null as int? ), Entity.ForeignCurrencyCodeValue, Entity.ForeignCurrencyCodeValueId );
                            }

                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            string origPerson = History.GetValue<PersonAlias>( null, Entry.OriginalValues[nameof( Entity.AuthorizedPersonAliasId )].ToStringSafe().AsIntegerOrNull(), rockContext );
                            string person = History.GetValue<PersonAlias>( Entity.AuthorizedPersonAlias, Entity.AuthorizedPersonAliasId, rockContext );
                            History.EvaluateChange( HistoryChangeList, "Authorized Person", origPerson, person );

                            int? origGatewayId = Entry.OriginalValues[nameof( Entity.FinancialGatewayId )].ToStringSafe().AsIntegerOrNull();
                            if ( !Entity.FinancialGatewayId.Equals( origGatewayId ) )
                            {
                                History.EvaluateChange( HistoryChangeList, "Gateway", History.GetValue<FinancialGateway>( null, origGatewayId, rockContext ), History.GetValue<FinancialGateway>( Entity.FinancialGateway, Entity.FinancialGatewayId, rockContext ) );
                            }

                            History.EvaluateChange( HistoryChangeList, "Gateway Schedule Id", Entry.OriginalValues[nameof( Entity.GatewayScheduleId )].ToStringSafe(), Entity.GatewayScheduleId );
                            History.EvaluateChange( HistoryChangeList, "Transaction Code", Entry.OriginalValues[nameof( Entity.TransactionCode) ].ToStringSafe(), Entity.TransactionCode );
                            History.EvaluateChange( HistoryChangeList, "Summary", Entry.OriginalValues[nameof( Entity.Summary )].ToStringSafe(), Entity.Summary );
                            History.EvaluateChange( HistoryChangeList, "Type", Entry.OriginalValues[nameof( Entity.TransactionTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TransactionTypeValue, Entity.TransactionTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Source", Entry.OriginalValues[nameof( Entity.SourceTypeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.SourceTypeValue, Entity.SourceTypeValueId );
                            History.EvaluateChange( HistoryChangeList, "Frequency", Entry.OriginalValues[nameof( Entity.TransactionFrequencyValueId )].ToStringSafe().AsIntegerOrNull(), Entity.TransactionFrequencyValue, Entity.TransactionFrequencyValueId );
                            History.EvaluateChange( HistoryChangeList, "Start Date", Entry.OriginalValues[nameof( Entity.StartDate )].ToStringSafe().AsDateTime(), Entity.StartDate );
                            History.EvaluateChange( HistoryChangeList, "End Date", Entry.OriginalValues[nameof( Entity.EndDate )].ToStringSafe().AsDateTime(), Entity.EndDate );
                            History.EvaluateChange( HistoryChangeList, "Number of Payments", Entry.OriginalValues[nameof( Entity.EndDate )].ToStringSafe().AsIntegerOrNull(), Entity.NumberOfPayments );
                            History.EvaluateChange( HistoryChangeList, "Is Active", Entry.OriginalValues[nameof( Entity.IsActive )].ToStringSafe().AsBooleanOrNull(), Entity.IsActive );
                            History.EvaluateChange( HistoryChangeList, "Card Reminder Date", Entry.OriginalValues[nameof( Entity.CardReminderDate )].ToStringSafe().AsDateTime(), Entity.CardReminderDate );
                            History.EvaluateChange( HistoryChangeList, "Last Reminded Date", Entry.OriginalValues[nameof( Entity.LastRemindedDate )].ToStringSafe().AsDateTime(), Entity.LastRemindedDate );
                            History.EvaluateChange( HistoryChangeList, "Currency Code", Entry.OriginalValues[nameof( Entity.ForeignCurrencyCodeValueId )].ToStringSafe().AsIntegerOrNull(), Entity.ForeignCurrencyCodeValue, Entity.ForeignCurrencyCodeValueId );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

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
                if ( HistoryChangeList?.Any() == true )
                {
                    var rockContext = ( RockContext ) DbContext;

                    HistoryService.SaveChanges( rockContext,
                        typeof( FinancialScheduledTransaction ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        Entity.Id,
                        HistoryChangeList,
                        true,
                        Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();
            }
        }
    }
}