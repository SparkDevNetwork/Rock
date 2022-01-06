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
using Rock;
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using System;
using System.Data.Entity;
using System.Linq;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Scheduled Gift Event Types
    /// </summary>
    public static class ScheduledGiftEventTypes
    {
        /// <summary>
        /// Created
        /// </summary>
        public const string ScheduledGiftCreated = "ScheduledGift.Created";

        /// <summary>
        /// Updated
        /// </summary>
        public const string ScheduledGiftUpdated = "ScheduledGift.Updated";

        /// <summary>
        /// Inactivated
        /// </summary>
        public const string ScheduledGiftInactivated = "ScheduledGift.Inactivated";
    }

    /// <summary>
    /// Scheduled Gift Event Message
    /// </summary>
    public class ScheduledGiftWasModifiedMessage : ScheduledGiftWasModifiedMessageData, IEventMessage<GivingEventQueue>
    {
        /// <summary>
        /// Gets or sets the name of the sender node.
        /// </summary>
        /// <value>
        /// The name of the sender node.
        /// </value>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public DateTime Time { get; set; }

        /// <summary>
        /// Publishes the scheduled transaction event.
        /// </summary>
        /// <param name="scheduledTransactionId">The scheduled transaction identifier.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="gatewaySupportedCardTypesDefinedValueGuid">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the credit card types supported by the <see cref="FinancialGateway"/> for a specified currency.</param>
        /// <param name="gatewayCurrencyUnitMultiple">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the "unit multiple" (e.g., 100 for dollars) of the currency specified by the gatway.</param>
        public static void PublishScheduledTransactionEvent( int scheduledTransactionId, string eventType, Guid? gatewaySupportedCardTypesDefinedValueGuid = null, Guid? gatewayCurrencyUnitMultiple = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var scheduleService = new FinancialScheduledTransactionService( rockContext );
                var gateway = scheduleService.Queryable()
                    .AsNoTracking()
                    .Include( s => s.FinancialGateway )
                    .Where( s => s.Id == scheduledTransactionId )
                    .Select( s => s.FinancialGateway )
                    .FirstOrDefault();

                var gatewayComponent = gateway?.GetGatewayComponent();
                var searchKeyTiedGateway = gatewayComponent as ISearchKeyTiedGateway;
                var searchKeyTypeGuid = searchKeyTiedGateway?.GetPersonSearchKeyTypeGuid( gateway );
                var data = GetScheduledGiftWasModifiedMessageData( rockContext, scheduledTransactionId, searchKeyTypeGuid, gatewaySupportedCardTypesDefinedValueGuid, gatewayCurrencyUnitMultiple );

                if ( data != null )
                {
                    var statusGateway = gatewayComponent as IStatusProvidingGateway;

                    var message = new ScheduledGiftWasModifiedMessage
                    {
                        EventType = eventType,
                        Address = data.Address,
                        FinancialScheduledTransaction = data.FinancialScheduledTransaction,
                        Person = data.Person,
                        Time = RockDateTime.Now
                    };

                    _ = RockMessageBus.PublishAsync<GivingEventQueue, ScheduledGiftWasModifiedMessage>( message );
                }
            }
        }

        /// <summary>
        /// Gets the message data for a <see cref="ScheduledGiftWasModifiedMessage"/>.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="scheduledTransactionId">The scheduled transaction identifier.</param>
        /// <param name="personSearchKeyTypeGuid">The person search key type unique identifier.</param>
        /// <param name="gatewaySupportedCardTypesDefinedValueGuid">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the credit card types supported by the <see cref="FinancialGateway"/> for a specified currency.</param>
        /// <param name="gatewayCurrencyUnitMultiple">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the "unit multiple" (e.g., 100 for dollars) of the currency specified by the gatway.</param>
        /// <returns></returns>
        private static ScheduledGiftWasModifiedMessageData GetScheduledGiftWasModifiedMessageData( RockContext rockContext, int scheduledTransactionId, Guid? personSearchKeyTypeGuid, Guid? gatewaySupportedCardTypesDefinedValueGuid, Guid? gatewayCurrencyUnitMultiple )
        {
            var status = "success";
            var statusMessage = string.Empty;

            var scheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var scheduledTransaction = scheduledTransactionService.Queryable()
                .AsNoTracking()
                .Where( s => s.Id == scheduledTransactionId )
                .FirstOrDefault();

            var data = new ScheduledGiftWasModifiedMessageData
            {
                Person = new TransactionPersonView
                {
                    PrimaryAliasId = scheduledTransaction.AuthorizedPersonAliasId,
                    Id = scheduledTransaction.AuthorizedPersonAlias.Person.Id,
                    Guid = scheduledTransaction.Guid,
                    FirstName = scheduledTransaction.AuthorizedPersonAlias.Person.FirstName,
                    NickName = scheduledTransaction.AuthorizedPersonAlias.Person.NickName,
                    LastName = scheduledTransaction.AuthorizedPersonAlias.Person.LastName,
                    Email = scheduledTransaction.AuthorizedPersonAlias.Person.Email,
                    ForeignId = scheduledTransaction.AuthorizedPersonAlias.Person.ForeignId,
                },
                FinancialScheduledTransaction = new ScheduledTransactionView
                {
                    Id = scheduledTransaction.Id,
                    Guid = scheduledTransaction.Guid,
                    CreditCardTypeValueId = scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId,
                    CurrencyTypeValueId = scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId,
                    ForeignCurrencyCode = new TransactionCurrencyCodeView
                    {
                        ValueId = scheduledTransaction.ForeignCurrencyCodeValueId,
                    },
                    ScheduledTransactionId = scheduledTransaction.Id,
                    TransactionFrequencyValue = new TransactionFrequencyView {
                        ValueId = scheduledTransaction.TransactionFrequencyValueId,
                    },
                    SourceTypeValueId = scheduledTransaction.SourceTypeValueId,
                    Status = status,
                    StatusMessage = statusMessage,
                    TransactionCode = scheduledTransaction.TransactionCode,
                    TransactionTypeValueId = scheduledTransaction.TransactionTypeValueId,
                    ForeignKey = scheduledTransaction.ForeignKey,
                    Details = scheduledTransaction.ScheduledTransactionDetails.Select( d => new ScheduledTransactionDetailView
                    {
                        Id = d.Id,
                        Guid = d.Guid,
                        AccountId = d.AccountId,
                        AccountName = d.Account.Name,
                        PublicAccountName = d.Account.PublicName,
                        /* Shaun Cummings - September 10, 2021
                         * 
                         * For scheduled transactions, if the transaction has a ForeignCurrencyCode, the Amount property
                         * reflects the amount in that foreign currency.
                         * */
                        Amount = d.Amount
                    }).ToList()
                }
            };

            if ( data != null )
            {
                data.Person.HydratePersonData( rockContext, data.Address, personSearchKeyTypeGuid );
                data.FinancialScheduledTransaction.HydrateDefinedValues( gatewaySupportedCardTypesDefinedValueGuid, gatewayCurrencyUnitMultiple );
            }

            return data;
        }
    }

    /// <summary>
    /// Scheduled Gift Event Data
    /// </summary>
    public class ScheduledGiftWasModifiedMessageData
    {
        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        public ScheduledTransactionView FinancialScheduledTransaction { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public TransactionPersonView Person { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public TransactionAddressView Address { get; set; }
    }
}
