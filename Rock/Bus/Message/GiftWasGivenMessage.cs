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
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Gift Event Types
    /// </summary>
    public static class GiftEventTypes
    {
        /// <summary>
        /// Success
        /// </summary>
        public const string GiftSuccess = "Gift.Success";

        /// <summary>
        /// Failure
        /// </summary>
        public const string GiftFailure = "Gift.Failure";

        /// <summary>
        /// Pending
        /// </summary>
        public const string GiftPending = "Gift.Pending";
    }

    /// <summary>
    /// Gift Was Given Message
    /// </summary>
    public class GiftWasGivenMessage : GiftWasGivenMessageData, IEventMessage<GivingEventQueue>
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
        /// Publishes the gift event.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="eventType">Type of the event. May be null if using an <see cref="IStatusProvidingGateway"/> gateway component.</param>
        /// <param name="gatewaySupportedCardTypesDefinedValueGuid">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the credit card types supported by the <see cref="FinancialGateway"/> for a specified currency.</param>
        /// <param name="gatewayCurrencyUnitMultiple">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the "unit multiple" (e.g., 100 for dollars) of the currency specified by the gatway.</param>
        public static void PublishTransactionEvent( int transactionId, string eventType = null, Guid? gatewaySupportedCardTypesDefinedValueGuid = null, Guid? gatewayCurrencyUnitMultiple = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var transactionService = new FinancialTransactionService( rockContext );
                var gateway = transactionService.Queryable()
                    .AsNoTracking()
                    .Include( t => t.FinancialGateway )
                    .Where( t => t.Id == transactionId )
                    .Select( t => t.FinancialGateway )
                    .FirstOrDefault();

                var gatewayComponent = gateway?.GetGatewayComponent();
                var searchKeyTiedGateway = gatewayComponent as ISearchKeyTiedGateway;
                var searchKeyTypeGuid = searchKeyTiedGateway?.GetPersonSearchKeyTypeGuid( gateway );
                var data = GetGiftWasGivenMessageData( rockContext, transactionId, searchKeyTypeGuid, gatewaySupportedCardTypesDefinedValueGuid, gatewayCurrencyUnitMultiple );

                if ( data != null )
                {
                    if ( eventType.IsNullOrWhiteSpace() && gatewayComponent is IStatusProvidingGateway statusGateway )
                    {
                        eventType =
                            statusGateway.IsPendingStatus( data.FinancialTransaction.Status ) ? GiftEventTypes.GiftPending :
                            statusGateway.IsSuccessStatus( data.FinancialTransaction.Status ) ? GiftEventTypes.GiftSuccess :
                            statusGateway.IsFailureStatus( data.FinancialTransaction.Status ) ? GiftEventTypes.GiftFailure :
                            null;
                    }

                    if ( !eventType.IsNullOrWhiteSpace() )
                    {
                        var message = new GiftWasGivenMessage
                        {
                            EventType = eventType,
                            Address = data.Address,
                            FinancialTransaction = data.FinancialTransaction,
                            Person = data.Person,
                            Time = RockDateTime.Now
                        };

                        _ = RockMessageBus.PublishAsync<GivingEventQueue, GiftWasGivenMessage>( message );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the transaction view.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="personSearchKeyTypeGuid">The person search key type unique identifier.</param>
        /// <param name="gatewaySupportedCardTypesDefinedValueGuid">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the credit card types supported by the <see cref="FinancialGateway"/> for a specified currency.</param>
        /// <param name="gatewayCurrencyUnitMultiple">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the "unit multiple" (e.g., 100 for dollars) of the currency specified by the gatway.</param>
        /// <returns></returns>
        private static GiftWasGivenMessageData GetGiftWasGivenMessageData( RockContext rockContext, int transactionId, Guid? personSearchKeyTypeGuid, Guid? gatewaySupportedCardTypesDefinedValueGuid, Guid? gatewayCurrencyUnitMultiple )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var data = financialTransactionService.Queryable()
                .AsNoTracking()
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( "TransactionDetails.Account" )
                .Where( t => t.Id == transactionId )
                .Select( t => new GiftWasGivenMessageData
                {
                    Person = t.AuthorizedPersonAliasId.HasValue ? new TransactionPersonView
                    {
                        PrimaryAliasId = t.AuthorizedPersonAliasId.Value,
                        Id = t.AuthorizedPersonAlias.Person.Id,
                        Guid = t.Guid,
                        FirstName = t.AuthorizedPersonAlias.Person.FirstName,
                        NickName = t.AuthorizedPersonAlias.Person.NickName,
                        LastName = t.AuthorizedPersonAlias.Person.LastName,
                        Email = t.AuthorizedPersonAlias.Person.Email,
                        ForeignId = t.AuthorizedPersonAlias.Person.ForeignId,
                    } : null,
                    FinancialTransaction = new TransactionView
                    {
                        Id = t.Id,
                        Guid = t.Guid,
                        CreditCardTypeValueId = t.FinancialPaymentDetail.CreditCardTypeValueId,
                        CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                        ForeignCurrencyCode = new TransactionCurrencyCodeView
                        {
                            ValueId = t.ForeignCurrencyCodeValueId,
                        },
                        ScheduledTransactionId = t.ScheduledTransactionId,
                        TransactionFrequencyValue = new TransactionFrequencyView
                        {
                            ValueId = t.ScheduledTransaction.TransactionFrequencyValueId,
                        },
                        SourceTypeValueId = t.SourceTypeValueId,
                        Status = t.Status,
                        StatusMessage = t.StatusMessage,
                        TransactionCode = t.TransactionCode,
                        TransactionDateTime = t.TransactionDateTime,
                        TransactionTypeValueId = t.TransactionTypeValueId,
                        ForeignKey = t.ForeignKey,
                        Details = t.TransactionDetails.Select( td => new TransactionDetailView
                        {
                            Id = td.Id,
                            Guid = td.Guid,
                            AccountId = td.AccountId,
                            AccountName = td.Account.Name,
                            PublicAccountName = td.Account.PublicName,
                            Amount = td.Amount,
                            ForeignCurrencyAmount = td.ForeignCurrencyAmount
                        } ).ToList()
                    }
                } )
                .FirstOrDefault();

            if ( data != null )
            {
                data.Person.HydratePersonData( rockContext, data.Address, personSearchKeyTypeGuid );
                data.FinancialTransaction.HydrateDefinedValues( gatewaySupportedCardTypesDefinedValueGuid, gatewayCurrencyUnitMultiple );
                data.FinancialTransaction.TableName = nameof( FinancialTransaction );
            }

            return data;
        }
    }

    #region Data Classes

    /// <summary>
    /// Gift Was Given Message Data
    /// </summary>
    public class GiftWasGivenMessageData
    {
        /*
             5/16/2022 - SMC

             Do not refactor this without approval from the DSD.

             These events are used by some existing plugins and modifying the data contract will break those.

             Reason:  Existing implementations expect this data contract.
        */

        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        public TransactionView FinancialTransaction { get; set; }

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

    /// <summary>
    /// Helper class/methods for gift message data.
    /// </summary>
    internal static class GiftMessageDataHelper
    {
        /// <summary>
        /// Hydrates the person data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personData">The person data.</param>
        /// <param name="addressData">The address data.</param>
        /// <param name="personSearchKeyTypeGuid">The person search key type unique identifier.</param>
        internal static void HydratePersonData( this TransactionPersonView personData, RockContext rockContext, TransactionAddressView addressData, Guid? personSearchKeyTypeGuid )
        {
            if ( personData == null )
            {
                return;
            }

            var personService = new PersonService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );
            var person = personService.Get( personData.Id );
            var primaryFamily = person?.GetFamilies( rockContext ).FirstOrDefault();

            if ( primaryFamily != null )
            {
                addressData = groupLocationService.Queryable()
                    .AsNoTracking()
                    .Include( gl => gl.Location )
                    .Include( gl => gl.GroupLocationTypeValue )
                    .Where( gl => gl.GroupId == primaryFamily.Id )
                    .OrderBy( gl => gl.GroupLocationTypeValue.Order )
                    .Select( gl => new TransactionAddressView
                    {
                        Id = gl.Location.Id,
                        Guid = gl.Location.Guid,
                        Street1 = gl.Location.Street1,
                        Street2 = gl.Location.Street2,
                        City = gl.Location.City,
                        State = gl.Location.State,
                        Country = gl.Location.Country,
                        PostalCode = gl.Location.PostalCode
                    } )
                    .FirstOrDefault();
            }

            if ( person != null && personSearchKeyTypeGuid.HasValue )
            {
                personData.PersonSearchKeys = person.GetPersonSearchKeys( rockContext )
                    .AsNoTracking()
                    .Where( psk => psk.SearchTypeValue.Guid == personSearchKeyTypeGuid.Value )
                    .Select( psk => psk.SearchValue )
                    .Distinct()
                    .ToList();
            }
        }

        /// <summary>
        /// Hydrates the defined values.
        /// </summary>
        /// <param name="transactionView">The transaction view.</param>
        /// <param name="gatewaySupportedCardTypesDefinedValueGuid">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the credit card types supported by the <see cref="FinancialGateway"/> for a specified currency.</param>
        /// <param name="gatewayCurrencyUnitMultiple">[Optional] The <see cref="Guid"/> of the <see cref="DefinedValue"/> that indicates the "unit multiple" (e.g., 100 for dollars) of the currency specified by the gatway.</param>
        internal static void HydrateDefinedValues( this TransactionViewBase transactionView, Guid? gatewaySupportedCardTypesDefinedValueGuid, Guid? gatewayCurrencyUnitMultiple )
        {
            transactionView.CreditCardTypeValue = GetDefinedValue( transactionView.CreditCardTypeValueId );
            transactionView.CurrencyTypeValue = GetDefinedValue( transactionView.CurrencyTypeValueId );
            transactionView.SourceTypeValue = GetDefinedValue( transactionView.SourceTypeValueId );
            transactionView.TransactionTypeValue = GetDefinedValue( transactionView.TransactionTypeValueId );

            if ( transactionView.TransactionFrequencyValue?.ValueId != null )
            {
                var transactionFrequencyValue = DefinedValueCache.Get( transactionView.TransactionFrequencyValue.ValueId.Value );
                if ( transactionFrequencyValue != null )
                {
                    transactionView.TransactionFrequencyValue.Value = transactionFrequencyValue.Value;
                    transactionView.TransactionFrequencyValue.Description = transactionFrequencyValue.Description;
                }
            }

            if ( transactionView.ForeignCurrencyCode?.ValueId != null )
            {
                var currencyCodeValue = DefinedValueCache.Get( transactionView.ForeignCurrencyCode.ValueId.Value );
                if ( currencyCodeValue != null )
                {
                    string attrKey_Symbol = AttributeCache.Get( Rock.SystemGuid.Attribute.CURRENCY_CODE_SYMBOL ).Key;
                    string attrKey_Position = AttributeCache.Get( Rock.SystemGuid.Attribute.CURRENCY_CODE_POSITION ).Key;
                    string attrKey_DecimalPlaces = AttributeCache.Get( Rock.SystemGuid.Attribute.CURRENCY_CODE_DECIMAL_PLACES ).Key;

                    transactionView.ForeignCurrencyCode.Value = currencyCodeValue.Value;
                    transactionView.ForeignCurrencyCode.Symbol = currencyCodeValue.GetAttributeValue( attrKey_Symbol );
                    transactionView.ForeignCurrencyCode.Position = currencyCodeValue.GetAttributeValue( attrKey_Position );
                    transactionView.ForeignCurrencyCode.DecimalPlaces = currencyCodeValue.GetAttributeValue( attrKey_DecimalPlaces ).AsIntegerOrNull();

                    if ( gatewaySupportedCardTypesDefinedValueGuid.HasValue )
                    {
                        string attrKey_SupportedCardTypes = AttributeCache.Get( gatewaySupportedCardTypesDefinedValueGuid.Value ).Key;
                        transactionView.ForeignCurrencyCode.GatewaySupportedCreditCardTypeValues = currencyCodeValue.GetAttributeValues( attrKey_SupportedCardTypes ).AsGuidList();
                        transactionView.ForeignCurrencyCode.GatewaySupportedCreditCardTypes = new List<string>();
                        foreach ( var supportedCardType in transactionView.ForeignCurrencyCode.GatewaySupportedCreditCardTypeValues )
                        {
                            var dvCardType = DefinedValueCache.Get( supportedCardType );
                            if ( dvCardType == null )
                            {
                                continue;
                            }

                            transactionView.ForeignCurrencyCode.GatewaySupportedCreditCardTypes.Add( dvCardType.Value );
                        }
                    }

                    if ( gatewayCurrencyUnitMultiple.HasValue )
                    {
                        string attrKey_CurrencyUnitMultiple = AttributeCache.Get( gatewayCurrencyUnitMultiple.Value ).Key;
                        transactionView.ForeignCurrencyCode.GatewayCurrencyUnitMultiple = currencyCodeValue.GetAttributeValue( attrKey_CurrencyUnitMultiple ).AsIntegerOrNull();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the defined value.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private static string GetDefinedValue( int? id )
        {
            if ( id is null )
            {
                return string.Empty;
            }

            var value = DefinedValueCache.Get( id.Value )?.Value;
            return value.IsNullOrWhiteSpace() ? string.Empty : value;
        }
    }

    #region View Components

    // Comment here about the scheduled transaction data classes

    /// <summary>
    /// Abstract Base Class for Transaction Views
    /// </summary>
    public abstract class TransactionViewBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the foreign currency code.
        /// </summary>
        /// <value>
        /// The foreign currency code.
        /// </value>
        public TransactionCurrencyCodeView ForeignCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        public string CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value identifier.
        /// </summary>
        /// <value>
        /// The credit card type value identifier.
        /// </value>
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value.
        /// </summary>
        /// <value>
        /// The credit card type value.
        /// </value>
        public string CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value identifier.
        /// </summary>
        /// <value>
        /// The transaction type value identifier.
        /// </value>
        public int? TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value.
        /// </summary>
        /// <value>
        /// The transaction type value.
        /// </value>
        public string TransactionTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the source type value.
        /// </summary>
        /// <value>
        /// The source type value.
        /// </value>
        public string SourceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the scheduled transaction identifier.
        /// </summary>
        /// <value>
        /// The scheduled transaction identifier.
        /// </value>
        public int? ScheduledTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the scheduled transaction frequency value.
        /// </summary>
        /// <value>
        /// The scheduled transaction frequency value.
        /// </value>
        public TransactionFrequencyView TransactionFrequencyValue { get; set; }
    }

    /// <summary>
    /// Financial Transaction View
    /// </summary>
    public class TransactionView : TransactionViewBase
    {
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public List<TransactionDetailView> Details { get; set; }
    }

    /// <summary>
    /// Financial Scheduled Transaction View
    /// </summary>
    public class ScheduledTransactionView : TransactionViewBase
    {
        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the next gift date.
        /// </summary>
        /// <value>
        /// The next gift date.
        /// </value>
        public DateTime? NextGiftDate { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public List<ScheduledTransactionDetailView> Details { get; set; }
    }

    /// <summary>
    /// Financial Transaction Detail View Base
    /// </summary>
    public class TransactionDetailViewBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the name of the public account.
        /// </summary>
        /// <value>
        /// The name of the public account.
        /// </value>
        public string PublicAccountName { get; set; }
    }

    /// <summary>
    /// Financial Transaction Detail View
    /// </summary>
    public class TransactionDetailView : TransactionDetailViewBase
    {
        /// <summary>
        /// Gets or sets the foreign currency amount.
        /// </summary>
        /// <value>
        /// The foreign currency amount.
        /// </value>
        public decimal? ForeignCurrencyAmount { get; set; }
    }

    /// <summary>
    /// Scheduled Financial Transaction Detail View
    /// </summary>
    public class ScheduledTransactionDetailView : TransactionDetailViewBase
    {
        // See TransactionDetailViewBase for properties.
    }

    /// <summary>
    /// Transaction Frequency View
    /// </summary>
    public class TransactionFrequencyView
    {
        /// <summary>
        /// Gets or sets the value identifier.
        /// </summary>
        /// <value>
        /// The value identifier.
        /// </value>
        public int? ValueId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
    }

    /// <summary>
    /// Person View
    /// </summary>
    public class TransactionPersonView
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the primary alias identifier.
        /// </summary>
        /// <value>
        /// The primary alias identifier.
        /// </value>
        public int PrimaryAliasId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        public int? ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the person search keys.
        /// </summary>
        /// <value>
        /// The person search keys.
        /// </value>
        public List<string> PersonSearchKeys { get; set; }
    }

    /// <summary>
    /// Address View
    /// </summary>
    public class TransactionAddressView
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the street1.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the street2.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get; set; }
    }

    /// <summary>
    /// Currency Code View
    /// </summary>
    public class TransactionCurrencyCodeView
    {
        /// <summary>
        /// Gets or sets the value identifier.
        /// </summary>
        /// <value>
        /// The value identifier.
        /// </value>
        public int? ValueId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        /// <value>
        /// The symbol.
        /// </value>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public string Position { get; set; }

        /// <summary>
        /// Gets or sets the decimal places.
        /// </summary>
        /// <value>
        /// The decimal places.
        /// </value>
        public int? DecimalPlaces { get; set; }

        /// <summary>
        /// Gets or sets the gateway supported credit card type values.
        /// </summary>
        /// <value>
        /// The gateway supported credit card type values.
        /// </value>
        public List<Guid> GatewaySupportedCreditCardTypeValues { get; set; }

        /// <summary>
        /// Gets or sets the gateway supported credit card types.
        /// </summary>
        /// <value>
        /// The gateway supported credit card types.
        /// </value>
        public List<string> GatewaySupportedCreditCardTypes { get; set; }

        /// <summary>
        /// Gets or sets the gateway currency unit multiple.
        /// </summary>
        /// <value>
        /// The gateway currency unit multiple.
        /// </value>
        public int? GatewayCurrencyUnitMultiple { get; set; }
    }

    #endregion View Components

    #endregion Data Classes
}
