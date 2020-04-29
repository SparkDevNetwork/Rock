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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Utility;

namespace Rock.NMI
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( StringEnumConverter ) )]
    public enum NMIPaymentType
    {
        /// <summary>
        /// The card
        /// </summary>
        card,

        /// <summary>
        /// The ach
        /// </summary>
        ach
    }

    /// <summary>
    /// Base Response for most classes. All this really does is give <see cref="_additionalData"/>
    /// </summary>
    internal class BaseResponse
    {
        /// <summary>
        /// Newtonsoft.Json.JsonExtensionData instructs the Newtonsoft.Json.JsonSerializer to deserialize properties with no
        /// matching class member into the specified collection
        /// </summary>
        /// <value>
        /// The other data.
        /// </value>
        [Newtonsoft.Json.JsonExtensionData( ReadData = true, WriteData = false )]
        public IDictionary<string, Newtonsoft.Json.Linq.JToken> _additionalData { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class TokenizerResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [JsonProperty( "message" )]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the card.
        /// </summary>
        /// <value>
        /// The card.
        /// </value>
        [JsonProperty( "card" )]
        public CardTokenResponse Card { get; set; }

        /// <summary>
        /// Gets or sets the check.
        /// </summary>
        /// <value>
        /// The check.
        /// </value>
        [JsonProperty( "check" )]
        public CheckTokenResponse Check { get; set; }

        /// <summary>
        /// Determines whether [is success status].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is success status]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSuccessStatus() => ErrorMessage.IsNullOrWhiteSpace() && ValidationMessage.IsNullOrWhiteSpace();

        /// <summary>
        /// Determines whether [has validation error].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has validation error]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasValidationError() => ValidationMessage.IsNotNullOrWhiteSpace();

        /// <summary>
        /// Gets or sets the validation message.
        /// We added these things since NMI doesn't really do it and we had to do custom stuff in gatewayCollect.js to populate these
        /// </summary>
        /// <value>
        /// The validation message.
        /// </value>
        [JsonProperty( "validationMessage" )]
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        [JsonProperty( "errorMessage" )]
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class CardTokenResponse
    {
        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        [JsonProperty( "number" )]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the bin.
        /// </summary>
        /// <value>
        /// The bin.
        /// </value>
        [JsonProperty( "bin" )]
        public string Bin { get; set; }

        /// <summary>
        /// Gets or sets the exp.
        /// </summary>
        /// <value>
        /// The exp.
        /// </value>
        [JsonProperty( "exp" )]
        public string Exp { get; set; }

        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>
        /// The hash.
        /// </value>
        [JsonProperty( "hash" )]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class CheckTokenResponse
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        [JsonProperty( "account" )]
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>
        /// The hash.
        /// </value>
        [JsonProperty( "hash" )]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the aba.
        /// </summary>
        /// <value>
        /// The aba.
        /// </value>
        [JsonProperty( "aba" )]
        public string Aba { get; set; }
    }

    /* Customer Vault Response Classes */

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class CustomerVaultQueryResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the customer vault.
        /// </summary>
        /// <value>
        /// The customer vault.
        /// </value>
        [JsonProperty( "customer_vault" )]
        public CustomerVault CustomerVault { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class CustomerVault : BaseResponse
    {
        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        [JsonProperty( "customer" )]
        public Customer Customer { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class Customer : BaseResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the address1.
        /// </summary>
        /// <value>
        /// The address1.
        /// </value>
        [JsonProperty( "address_1" )]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address2.
        /// </summary>
        /// <value>
        /// The address2.
        /// </value>
        [JsonProperty( "address_2" )]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [JsonProperty( "company" )]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [JsonProperty( "city" )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [JsonProperty( "state" )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty( "postal_code" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty( "country" )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty( "email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [JsonProperty( "phone" )]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the cc number (Masked)
        /// </summary>
        /// <value>
        /// The cc number.
        /// </value>
        [JsonProperty( "cc_number" )]
        public string CcNumber { get; set; }

        /// <summary>
        /// Gets or sets the cc hash.
        /// </summary>
        /// <value>
        /// The cc hash.
        /// </value>
        [JsonProperty( "cc_hash" )]
        public string CcHash { get; set; }

        /// <summary>
        /// Gets or sets the cc exp.
        /// </summary>
        /// <value>
        /// The cc exp.
        /// </value>
        [JsonProperty( "cc_exp" )]
        public string CcExp { get; set; }

        /// <summary>
        /// Gets or sets the cc start date.
        /// </summary>
        /// <value>
        /// The cc start date.
        /// </value>
        [JsonProperty( "cc_start_date" )]
        public string CcStartDate { get; set; }

        /// <summary>
        /// Gets or sets the cc issue number.
        /// </summary>
        /// <value>
        /// The cc issue number.
        /// </value>
        [JsonProperty( "cc_issue_number" )]
        public string CCIssueNumber { get; set; }

        /// <summary>
        /// Gets or sets the check account.
        /// </summary>
        /// <value>
        /// The check account.
        /// </value>
        [JsonProperty( "check_account" )]
        public string CheckAccount { get; set; }

        /// <summary>
        /// Gets or sets the check hash.
        /// </summary>
        /// <value>
        /// The check hash.
        /// </value>
        [JsonProperty( "check_hash" )]
        public string CheckHash { get; set; }

        /// <summary>
        /// Gets or sets the check aba.
        /// </summary>
        /// <value>
        /// The check aba.
        /// </value>
        [JsonProperty( "check_aba" )]
        public string CheckAba { get; set; }

        /// <summary>
        /// Gets or sets the name of the check.
        /// </summary>
        /// <value>
        /// The name of the check.
        /// </value>
        [JsonProperty( "check_name" )]
        public string CheckName { get; set; }

        /// <summary>
        /// Gets or sets the type of the account holder.
        /// </summary>
        /// <value>
        /// The type of the account holder.
        /// </value>
        [JsonProperty( "account_holder_type" )]
        public string AccountHolderType { get; set; }

        /// <summary>
        /// Gets or sets the type of the account.
        /// </summary>
        /// <value>
        /// The type of the account.
        /// </value>
        [JsonProperty( "account_type" )]
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the sec code.
        /// </summary>
        /// <value>
        /// The sec code.
        /// </value>
        [JsonProperty( "sec_code" )]
        public string SecCode { get; set; }

        /// <summary>
        /// Gets or sets the processor identifier.
        /// </summary>
        /// <value>
        /// The processor identifier.
        /// </value>
        [JsonProperty( "processor_id" )]
        public string ProcessorId { get; set; }

        /// <summary>
        /// Gets or sets the cc bin.
        /// </summary>
        /// <value>
        /// The cc bin.
        /// </value>
        [JsonProperty( "cc_bin" )]
        public string CcBin { get; set; }

        /// <summary>
        /// Gets or sets the type of the cc.
        /// </summary>
        /// <value>
        /// The type of the cc.
        /// </value>
        [JsonProperty( "cc_type" )]
        public string CcType { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        [JsonProperty( "created" )]
        public string Created { get; set; }

        /// <summary>
        /// Gets or sets the updated.
        /// </summary>
        /// <value>
        /// The updated.
        /// </value>
        [JsonProperty( "updated" )]
        public string Updated { get; set; }

        /// <summary>
        /// Gets or sets the account updated.
        /// </summary>
        /// <value>
        /// The account updated.
        /// </value>
        [JsonProperty( "account_updated" )]
        public string AccountUpdated { get; set; }

        /// <summary>
        /// Gets or sets the customer vault identifier.
        /// </summary>
        /// <value>
        /// The customer vault identifier.
        /// </value>
        [JsonProperty( "customer_vault_id" )]
        public string CustomerVaultId { get; set; }
    }

    /// <summary>
    /// Charge Response 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class ChargeResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        [JsonProperty( "response" )]
        public string Response { get; set; }

        /// <summary>
        /// Determines whether response indicates an error
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </returns>
        public bool IsError() => Response != "1";

        /// <summary>
        /// Gets or sets the response text.
        /// </summary>
        /// <value>
        /// The response text.
        /// </value>
        [JsonProperty( "responsetext" )]
        public string ResponseText { get; set; }

        /// <summary>
        /// Gets or sets the authentication code.
        /// </summary>
        /// <value>
        /// The authentication code.
        /// </value>
        [JsonProperty( "authcode" )]
        public string AuthCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty( "transactionid" )]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the avs response.
        /// </summary>
        /// <value>
        /// The avs response.
        /// </value>
        [JsonProperty( "avsresponse" )]
        public string AVSResponse { get; set; }

        /// <summary>
        /// Gets or sets the CVV response.
        /// </summary>
        /// <value>
        /// The CVV response.
        /// </value>
        [JsonProperty( "cvvresponse" )]
        public string CVVResponse { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "orderid" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty( "response_code" )]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the customer vault identifier.
        /// </summary>
        /// <value>
        /// The customer vault identifier.
        /// </value>
        [JsonProperty( "customer_vault_id" )]
        public string CustomerVaultId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.ChargeResponse" />
    internal class RefundResponse : ChargeResponse
    {
        // RefundResponse fields map to the fields in ChargeResponse
    }

    /// <summary>
    /// 
    /// </summary>
    internal class CreateCustomerResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        [JsonProperty( "response" )]
        public string Response { get; set; }

        /// <summary>
        /// Determines whether response indicates an error
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </returns>
        public bool IsError() => Response != "1";

        /// <summary>
        /// Gets or sets the response text.
        /// </summary>
        /// <value>
        /// The response text.
        /// </value>
        [JsonProperty( "responsetext" )]
        public string ResponseText { get; set; }

        /// <summary>
        /// Gets or sets the authentication code.
        /// </summary>
        /// <value>
        /// The authentication code.
        /// </value>
        [JsonProperty( "authcode" )]
        public string AuthCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty( "transactionid" )]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the avs response.
        /// </summary>
        /// <value>
        /// The avs response.
        /// </value>
        [JsonProperty( "avsresponse" )]
        public string AVSResponse { get; set; }

        /// <summary>
        /// Gets or sets the CVV response.
        /// </summary>
        /// <value>
        /// The CVV response.
        /// </value>
        [JsonProperty( "cvvresponse" )]
        public string CVVResponse { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "orderid" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty( "response_code" )]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the customer vault identifier.
        /// </summary>
        /// <value>
        /// The customer vault identifier.
        /// </value>
        [JsonProperty( "customer_vault_id" )]
        public string CustomerVaultId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.CreateCustomerResponse" />
    internal class UpdateCustomerResponse : CreateCustomerResponse
    {
        // UpdateCustomerResponse is the same as CreateCustomerResponse
    }

    /* Subscription Response */

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.ChargeResponse" />
    internal class SubscriptionResponse : ChargeResponse
    {
        // SubscriptionResponse is exactly the same as ChargeResponse
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class QuerySubscriptionsResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the subscriptions result.
        /// </summary>
        /// <value>
        /// The subscriptions result.
        /// </value>
        [JsonProperty( "nm_response" )]
        public SubscriptionListResult SubscriptionsResult { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class SubscriptionListResult : BaseResponse
    {
        /// <summary>
        /// Gets or sets the subscription list.
        /// </summary>
        /// <value>
        /// The subscription list.
        /// </value>
        [JsonProperty( "subscription" )]
        public Subscription[] SubscriptionList { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class SubscriptionResult : BaseResponse
    {
        /// <summary>
        /// Gets or sets the subscription.
        /// </summary>
        /// <value>
        /// The subscription.
        /// </value>
        [JsonProperty( "subscription" )]
        public Subscription Subscription { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class Subscription : BaseResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the subscription identifier.
        /// </summary>
        /// <value>
        /// The subscription identifier.
        /// </value>
        [JsonProperty( "subscription_id" )]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the next charge date.
        /// </summary>
        /// <value>
        /// The next charge date.
        /// </value>
        [JsonProperty( "next_charge_date" )]
        private string _nextChargeDate { get; set; }

        /// <summary>
        /// Gets the next charge date.
        /// </summary>
        /// <value>
        /// The next charge date.
        /// </value>
        public DateTime? NextChargeDate
        {
            get
            {
                return DateTimeHelper.ParseDateValue( _nextChargeDate );
            }
        }

        /// <summary>
        /// Gets or sets the order description.
        /// </summary>
        /// <value>
        /// The order description.
        /// </value>
        [JsonProperty( "order_description" )]
        public string OrderDescription { get; set; }

        /// <summary>
        /// Gets or sets the plan.
        /// </summary>
        /// <value>
        /// The plan.
        /// </value>
        [JsonProperty( "plan" )]
        public SubscriptionPlan Plan { get; set; }

        /// <summary>
        /// Gets or sets the completed payments.
        /// </summary>
        /// <value>
        /// The completed payments.
        /// </value>
        [JsonProperty( "completed_payments" )]
        public int? CompletedPayments { get; set; }

        /// <summary>
        /// Gets or sets the attempted payments.
        /// </summary>
        /// <value>
        /// The attempted payments.
        /// </value>
        [JsonProperty( "attempted_payments" )]
        public int? AttemptedPayments { get; set; }

        /// <summary>
        /// Gets or sets the remaining payments.
        /// </summary>
        /// <value>
        /// The remaining payments.
        /// </value>
        [JsonProperty( "remaining_payments" )]
        public string RemainingPayments { get; set; }

        /// <summary>
        /// Gets or sets the ponumber.
        /// </summary>
        /// <value>
        /// The ponumber.
        /// </value>
        [JsonProperty( "ponumber" )]
        public string Ponumber { get; set; }

        /// <summary>
        /// Gets or sets the orderid.
        /// </summary>
        /// <value>
        /// The orderid.
        /// </value>
        [JsonProperty( "orderid" )]
        public string Orderid { get; set; }

        /// <summary>
        /// Gets or sets the shipping.
        /// </summary>
        /// <value>
        /// The shipping.
        /// </value>
        [JsonProperty( "shipping" )]
        public string Shipping { get; set; }

        /// <summary>
        /// Gets or sets the tax.
        /// </summary>
        /// <value>
        /// The tax.
        /// </value>
        [JsonProperty( "tax" )]
        public string Tax { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the address1.
        /// </summary>
        /// <value>
        /// The address1.
        /// </value>
        [JsonProperty( "address_1" )]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address2.
        /// </summary>
        /// <value>
        /// The address2.
        /// </value>
        [JsonProperty( "address_2" )]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [JsonProperty( "company" )]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [JsonProperty( "city" )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [JsonProperty( "state" )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty( "postal_code" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty( "country" )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty( "email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [JsonProperty( "phone" )]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>
        /// The fax.
        /// </value>
        [JsonProperty( "fax" )]
        public string Fax { get; set; }

        /// <summary>
        /// Gets or sets the cell phone.
        /// </summary>
        /// <value>
        /// The cell phone.
        /// </value>
        [JsonProperty( "cell_phone" )]
        public string CellPhone { get; set; }

        /// <summary>
        /// Gets or sets the customertaxid.
        /// </summary>
        /// <value>
        /// The customertaxid.
        /// </value>
        [JsonProperty( "customertaxid" )]
        public string CustomerTaxId { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        /// <value>
        /// The website.
        /// </value>
        [JsonProperty( "website" )]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the cc number.
        /// </summary>
        /// <value>
        /// The cc number.
        /// </value>
        [JsonProperty( "cc_number" )]
        public string CcNumber { get; set; }

        /// <summary>
        /// Gets or sets the cc hash.
        /// </summary>
        /// <value>
        /// The cc hash.
        /// </value>
        [JsonProperty( "cc_hash" )]
        public string CcHash { get; set; }

        /// <summary>
        /// Gets or sets the cc exp.
        /// </summary>
        /// <value>
        /// The cc exp.
        /// </value>
        [JsonProperty( "cc_exp" )]
        public string CcExp { get; set; }

        /// <summary>
        /// Gets or sets the cc start date.
        /// </summary>
        /// <value>
        /// The cc start date.
        /// </value>
        [JsonProperty( "cc_start_date" )]
        public string CcStartDate { get; set; }

        /// <summary>
        /// Gets or sets the cc issue number.
        /// </summary>
        /// <value>
        /// The cc issue number.
        /// </value>
        [JsonProperty( "cc_issue_number" )]
        public string CcIssueNumber { get; set; }

        /// <summary>
        /// Gets or sets the cc bin.
        /// </summary>
        /// <value>
        /// The cc bin.
        /// </value>
        [JsonProperty( "cc_bin" )]
        public string CcBin { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class SubscriptionPlan
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        [JsonProperty( "plan_id" )]
        public string PlanId { get; set; }

        /// <summary>
        /// Gets or sets the name of the plan.
        /// </summary>
        /// <value>
        /// The name of the plan.
        /// </value>
        [JsonProperty( "plan_name" )]
        public string PlanName { get; set; }

        /// <summary>
        /// Gets or sets the plan amount.
        /// </summary>
        /// <value>
        /// The plan amount.
        /// </value>
        [JsonProperty( "plan_amount" )]
        public string PlanAmount { get; set; }

        /// <summary>
        /// Gets or sets the plan payments.
        /// </summary>
        /// <value>
        /// The plan payments.
        /// </value>
        [JsonProperty( "plan_payments" )]
        public string PlanPayments { get; set; }

        /// <summary>
        /// Gets or sets the day frequency.
        /// </summary>
        /// <value>
        /// The day frequency.
        /// </value>
        [JsonProperty( "day_frequency" )]
        public string DayFrequency { get; set; }

        /// <summary>
        /// Gets or sets the month frequency.
        /// </summary>
        /// <value>
        /// The month frequency.
        /// </value>
        [JsonProperty( "month_frequency" )]
        public string MonthFrequency { get; set; }

        /// <summary>
        /// Gets or sets the day of month.
        /// </summary>
        /// <value>
        /// The day of month.
        /// </value>
        [JsonProperty( "day_of_month" )]
        public string DayOfMonth { get; set; }
    }

    /* Get Payments Response */

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class QueryTransactionsResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the transaction list result.
        /// </summary>
        /// <value>
        /// The transaction list result.
        /// </value>
        [JsonProperty( "nm_response" )]
        public TransactionListResult TransactionListResult { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class TransactionListResult : BaseResponse
    {
        /// <summary>
        /// Gets or sets the transaction list.
        /// </summary>
        /// <value>
        /// The transaction list.
        /// </value>
        [JsonProperty( "transaction" )]
        public Transaction[] TransactionList { get; set; }

        /// <summary>
        /// Gets or sets the error response.
        /// </summary>
        /// <value>
        /// The error response.
        /// </value>
        [JsonProperty( "error_response" )]
        public string ErrorResponse { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class Transaction : BaseResponse
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty( "transaction_id" )]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the original transaction identifier.
        /// This is the GatewayScheduleId that was used for this transaction
        /// </summary>
        /// <value>
        /// The original transaction identifier.
        /// </value>
        [JsonProperty( "original_transaction_id" )]
        public string OriginalTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        [JsonProperty( "condition" )]
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        [JsonProperty( "customerid" )]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the transaction action.
        /// </summary>
        /// <value>
        /// The transaction action.
        /// </value>
        [JsonProperty( "action" )]
        [JsonConverter( typeof( SingleOrArrayJsonConverter<TransactionAction> ) )]
        public TransactionAction[] TransactionActions { get; set; }

        /// <summary>
        /// Gets or sets the partial payment identifier.
        /// </summary>
        /// <value>
        /// The partial payment identifier.
        /// </value>
        [JsonProperty( "partial_payment_id" )]
        public string PartialPaymentId { get; set; }

        /// <summary>
        /// Gets or sets the partial payment balance.
        /// </summary>
        /// <value>
        /// The partial payment balance.
        /// </value>
        [JsonProperty( "partial_payment_balance" )]
        public string PartialPaymentBalance { get; set; }

        /// <summary>
        /// Gets or sets the platform identifier.
        /// </summary>
        /// <value>
        /// The platform identifier.
        /// </value>
        [JsonProperty( "platform_id" )]
        public string PlatformId { get; set; }

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        [JsonProperty( "transaction_type" )]
        public string TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "order_id" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the authorization code.
        /// </summary>
        /// <value>
        /// The authorization code.
        /// </value>
        [JsonProperty( "authorization_code" )]
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Gets or sets the ponumber.
        /// </summary>
        /// <value>
        /// The ponumber.
        /// </value>
        [JsonProperty( "ponumber" )]
        public string Ponumber { get; set; }

        /// <summary>
        /// Gets or sets the order description.
        /// </summary>
        /// <value>
        /// The order description.
        /// </value>
        [JsonProperty( "order_description" )]
        public string OrderDescription { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the address1.
        /// </summary>
        /// <value>
        /// The address1.
        /// </value>
        [JsonProperty( "address_1" )]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address2.
        /// </summary>
        /// <value>
        /// The address2.
        /// </value>
        [JsonProperty( "address_2" )]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [JsonProperty( "company" )]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [JsonProperty( "city" )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [JsonProperty( "state" )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty( "postal_code" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty( "country" )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty( "email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [JsonProperty( "phone" )]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>
        /// The fax.
        /// </value>
        [JsonProperty( "fax" )]
        public string Fax { get; set; }

        /// <summary>
        /// Gets or sets the cell phone.
        /// </summary>
        /// <value>
        /// The cell phone.
        /// </value>
        [JsonProperty( "cell_phone" )]
        public string CellPhone { get; set; }

        /// <summary>
        /// Gets or sets the customertaxid.
        /// </summary>
        /// <value>
        /// The customertaxid.
        /// </value>
        [JsonProperty( "customertaxid" )]
        public string Customertaxid { get; set; }

        /// <summary>
        /// Gets or sets the website.
        /// </summary>
        /// <value>
        /// The website.
        /// </value>
        [JsonProperty( "website" )]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the first name of the shipping.
        /// </summary>
        /// <value>
        /// The first name of the shipping.
        /// </value>
        [JsonProperty( "shipping_first_name" )]
        public string ShippingFirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the shipping.
        /// </summary>
        /// <value>
        /// The last name of the shipping.
        /// </value>
        [JsonProperty( "shipping_last_name" )]
        public string ShippingLastName { get; set; }

        /// <summary>
        /// Gets or sets the shipping address1.
        /// </summary>
        /// <value>
        /// The shipping address1.
        /// </value>
        [JsonProperty( "shipping_address_1" )]
        public string ShippingAddress1 { get; set; }

        /// <summary>
        /// Gets or sets the shipping address2.
        /// </summary>
        /// <value>
        /// The shipping address2.
        /// </value>
        [JsonProperty( "shipping_address_2" )]
        public string ShippingAddress2 { get; set; }

        /// <summary>
        /// Gets or sets the shipping company.
        /// </summary>
        /// <value>
        /// The shipping company.
        /// </value>
        [JsonProperty( "shipping_company" )]
        public string ShippingCompany { get; set; }

        /// <summary>
        /// Gets or sets the shipping city.
        /// </summary>
        /// <value>
        /// The shipping city.
        /// </value>
        [JsonProperty( "shipping_city" )]
        public string ShippingCity { get; set; }

        /// <summary>
        /// Gets or sets the state of the shipping.
        /// </summary>
        /// <value>
        /// The state of the shipping.
        /// </value>
        [JsonProperty( "shipping_state" )]
        public string ShippingState { get; set; }

        /// <summary>
        /// Gets or sets the shipping postal code.
        /// </summary>
        /// <value>
        /// The shipping postal code.
        /// </value>
        [JsonProperty( "shipping_postal_code" )]
        public string ShippingPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the shipping country.
        /// </summary>
        /// <value>
        /// The shipping country.
        /// </value>
        [JsonProperty( "shipping_country" )]
        public string ShippingCountry { get; set; }

        /// <summary>
        /// Gets or sets the shipping email.
        /// </summary>
        /// <value>
        /// The shipping email.
        /// </value>
        [JsonProperty( "shipping_email" )]
        public string ShippingEmail { get; set; }

        /// <summary>
        /// Gets or sets the shipping carrier.
        /// </summary>
        /// <value>
        /// The shipping carrier.
        /// </value>
        [JsonProperty( "shipping_carrier" )]
        public string ShippingCarrier { get; set; }

        /// <summary>
        /// Gets or sets the tracking number.
        /// </summary>
        /// <value>
        /// The tracking number.
        /// </value>
        [JsonProperty( "tracking_number" )]
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the shipping date.
        /// </summary>
        /// <value>
        /// The shipping date.
        /// </value>
        [JsonProperty( "shipping_date" )]
        public string ShippingDate { get; set; }

        /// <summary>
        /// Gets or sets the shipping.
        /// </summary>
        /// <value>
        /// The shipping.
        /// </value>
        [JsonProperty( "shipping" )]
        public string Shipping { get; set; }

        /// <summary>
        /// Gets or sets the shipping phone.
        /// </summary>
        /// <value>
        /// The shipping phone.
        /// </value>
        [JsonProperty( "shipping_phone" )]
        public string ShippingPhone { get; set; }

        /// <summary>
        /// Gets or sets the cc number.
        /// </summary>
        /// <value>
        /// The cc number.
        /// </value>
        [JsonProperty( "cc_number" )]
        public string CcNumber { get; set; }

        /// <summary>
        /// Gets or sets the cc hash.
        /// </summary>
        /// <value>
        /// The cc hash.
        /// </value>
        [JsonProperty( "cc_hash" )]
        public string CcHash { get; set; }

        /// <summary>
        /// Gets or sets the cc exp.
        /// </summary>
        /// <value>
        /// The cc exp.
        /// </value>
        [JsonProperty( "cc_exp" )]
        public string CcExp { get; set; }

        /// <summary>
        /// Gets or sets the cavv.
        /// </summary>
        /// <value>
        /// The cavv.
        /// </value>
        [JsonProperty( "cavv" )]
        public string Cavv { get; set; }

        /// <summary>
        /// Gets or sets the cavv result.
        /// </summary>
        /// <value>
        /// The cavv result.
        /// </value>
        [JsonProperty( "cavv_result" )]
        public string CavvResult { get; set; }

        /// <summary>
        /// Gets or sets the xid.
        /// </summary>
        /// <value>
        /// The xid.
        /// </value>
        [JsonProperty( "xid" )]
        public string Xid { get; set; }

        /// <summary>
        /// Gets or sets the eci.
        /// </summary>
        /// <value>
        /// The eci.
        /// </value>
        [JsonProperty( "eci" )]
        public string Eci { get; set; }

        /// <summary>
        /// Gets or sets the directory server identifier.
        /// </summary>
        /// <value>
        /// The directory server identifier.
        /// </value>
        [JsonProperty( "directory_server_id" )]
        public string DirectoryServerId { get; set; }

        /// <summary>
        /// Gets or sets the three ds version.
        /// </summary>
        /// <value>
        /// The three ds version.
        /// </value>
        [JsonProperty( "three_ds_version" )]
        public string ThreeDsVersion { get; set; }

        /// <summary>
        /// Gets or sets the avs response.
        /// </summary>
        /// <value>
        /// The avs response.
        /// </value>
        [JsonProperty( "avs_response" )]
        public string AvsResponse { get; set; }

        /// <summary>
        /// Gets or sets the CSC response.
        /// </summary>
        /// <value>
        /// The CSC response.
        /// </value>
        [JsonProperty( "csc_response" )]
        public string CscResponse { get; set; }

        /// <summary>
        /// Gets or sets the cardholder authentication.
        /// </summary>
        /// <value>
        /// The cardholder authentication.
        /// </value>
        [JsonProperty( "cardholder_auth" )]
        public string CardholderAuth { get; set; }

        /// <summary>
        /// Gets or sets the cc start date.
        /// </summary>
        /// <value>
        /// The cc start date.
        /// </value>
        [JsonProperty( "cc_start_date" )]
        public string CcStartDate { get; set; }

        /// <summary>
        /// Gets or sets the cc issue number.
        /// </summary>
        /// <value>
        /// The cc issue number.
        /// </value>
        [JsonProperty( "cc_issue_number" )]
        public string CcIssueNumber { get; set; }

        /// <summary>
        /// Gets or sets the check account.
        /// </summary>
        /// <value>
        /// The check account.
        /// </value>
        [JsonProperty( "check_account" )]
        public string CheckAccount { get; set; }

        /// <summary>
        /// Gets or sets the check hash.
        /// </summary>
        /// <value>
        /// The check hash.
        /// </value>
        [JsonProperty( "check_hash" )]
        public string CheckHash { get; set; }

        /// <summary>
        /// Gets or sets the check aba.
        /// </summary>
        /// <value>
        /// The check aba.
        /// </value>
        [JsonProperty( "check_aba" )]
        public string CheckAba { get; set; }

        /// <summary>
        /// Gets or sets the name of the check.
        /// </summary>
        /// <value>
        /// The name of the check.
        /// </value>
        [JsonProperty( "check_name" )]
        public string CheckName { get; set; }

        /// <summary>
        /// Gets or sets the type of the account holder.
        /// </summary>
        /// <value>
        /// The type of the account holder.
        /// </value>
        [JsonProperty( "account_holder_type" )]
        public string AccountHolderType { get; set; }

        /// <summary>
        /// Gets or sets the type of the account.
        /// </summary>
        /// <value>
        /// The type of the account.
        /// </value>
        [JsonProperty( "account_type" )]
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the sec code.
        /// </summary>
        /// <value>
        /// The sec code.
        /// </value>
        [JsonProperty( "sec_code" )]
        public string SecCode { get; set; }

        /// <summary>
        /// Gets or sets the drivers license number.
        /// </summary>
        /// <value>
        /// The drivers license number.
        /// </value>
        [JsonProperty( "drivers_license_number" )]
        public string DriversLicenseNumber { get; set; }

        /// <summary>
        /// Gets or sets the state of the drivers license.
        /// </summary>
        /// <value>
        /// The state of the drivers license.
        /// </value>
        [JsonProperty( "drivers_license_state" )]
        public string DriversLicenseState { get; set; }

        /// <summary>
        /// Gets or sets the drivers license dob.
        /// </summary>
        /// <value>
        /// The drivers license dob.
        /// </value>
        [JsonProperty( "drivers_license_dob" )]
        public string DriversLicenseDob { get; set; }

        /// <summary>
        /// Gets or sets the social security number.
        /// </summary>
        /// <value>
        /// The social security number.
        /// </value>
        [JsonProperty( "social_security_number" )]
        public string SocialSecurityNumber { get; set; }

        /// <summary>
        /// Gets or sets the processor identifier.
        /// </summary>
        /// <value>
        /// The processor identifier.
        /// </value>
        [JsonProperty( "processor_id" )]
        public string ProcessorId { get; set; }

        /// <summary>
        /// Gets or sets the tax.
        /// </summary>
        /// <value>
        /// The tax.
        /// </value>
        [JsonProperty( "tax" )]
        public string Tax { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        [JsonProperty( "currency" )]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the surcharge.
        /// </summary>
        /// <value>
        /// The surcharge.
        /// </value>
        [JsonProperty( "surcharge" )]
        public string Surcharge { get; set; }

        /// <summary>
        /// Gets or sets the tip.
        /// </summary>
        /// <value>
        /// The tip.
        /// </value>
        [JsonProperty( "tip" )]
        public string Tip { get; set; }

        /// <summary>
        /// Gets or sets the card balance.
        /// </summary>
        /// <value>
        /// The card balance.
        /// </value>
        [JsonProperty( "card_balance" )]
        public string CardBalance { get; set; }

        /// <summary>
        /// Gets or sets the card available balance.
        /// </summary>
        /// <value>
        /// The card available balance.
        /// </value>
        [JsonProperty( "card_available_balance" )]
        public string CardAvailableBalance { get; set; }

        /// <summary>
        /// Gets or sets the entry mode.
        /// </summary>
        /// <value>
        /// The entry mode.
        /// </value>
        [JsonProperty( "entry_mode" )]
        public string EntryMode { get; set; }

        /// <summary>
        /// Gets or sets the cc bin.
        /// </summary>
        /// <value>
        /// The cc bin.
        /// </value>
        [JsonProperty( "cc_bin" )]
        public string CcBin { get; set; }

        /// <summary>
        /// Gets or sets the type of the cc.
        /// </summary>
        /// <value>
        /// The type of the cc.
        /// </value>
        [JsonProperty( "cc_type" )]
        public string CcType { get; set; }

        /// <summary>
        /// Gets or sets the signature image.
        /// </summary>
        /// <value>
        /// The signature image.
        /// </value>
        [JsonProperty( "signature_image" )]
        public string SignatureImage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.NMI.BaseResponse" />
    internal class TransactionAction : BaseResponse
    {
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [JsonProperty( "date" )]
        private string _date { get; set; }

        /// <summary>
        /// Gets the action date.
        /// </summary>
        /// <value>
        /// The action date.
        /// </value>
        public DateTime? ActionDate
        {
            get
            {
                return DateTimeHelper.ParseDateValue( _date );
            }
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [JsonProperty( "amount" )]
        private string _amount { get; set; }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Amount => _amount.AsDecimalOrNull();

        /// <summary>
        /// Gets or sets the type of the action.
        /// </summary>
        /// <value>
        /// The type of the action.
        /// </value>
        [JsonProperty( "action_type" )]
        public string ActionType { get; set; }

        /// <summary>
        /// Gets or sets the response text.
        /// </summary>
        /// <value>
        /// The response text.
        /// </value>
        [JsonProperty( "response_text" )]
        public string ResponseText { get; set; }

        /// <summary>
        /// Gets or sets the processor batch identifier.
        /// </summary>
        /// <value>
        /// The processor batch identifier.
        /// </value>
        [JsonProperty( "processor_batch_id" )]
        public string ProcessorBatchId { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty( "response_code" )]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the success.
        /// </summary>
        /// <value>
        /// The success.
        /// </value>
        [JsonProperty( "success" )]
        public string Success { get; set; }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        [JsonProperty( "ip_address" )]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the aource.
        /// </summary>
        /// <value>
        /// The aource.
        /// </value>
        [JsonProperty( "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the API method.
        /// </summary>
        /// <value>
        /// The API method.
        /// </value>
        [JsonProperty( "api_method" )]
        public string ApiMethod { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [JsonProperty( "username" )]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        /// <value>
        /// The batch identifier.
        /// </value>
        [JsonProperty( "batch_id" )]
        public string BatchId { get; set; }

        /// <summary>
        /// Gets or sets the processor response text.
        /// </summary>
        /// <value>
        /// The processor response text.
        /// </value>
        [JsonProperty( "processor_response_text" )]
        public string ProcessorResponseText { get; set; }

        /// <summary>
        /// Gets or sets the processor response code.
        /// </summary>
        /// <value>
        /// The processor response code.
        /// </value>
        [JsonProperty( "processor_response_code" )]
        public string ProcessorResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the device license number.
        /// </summary>
        /// <value>
        /// The device license number.
        /// </value>
        [JsonProperty( "device_license_number" )]
        public string DeviceLicenseNumber { get; set; }

        /// <summary>
        /// Gets or sets the device nickname.
        /// </summary>
        /// <value>
        /// The device nickname.
        /// </value>
        [JsonProperty( "device_nickname" )]
        public string DeviceNickname { get; set; }
    }

    #region ThreeStepClasses

    internal class ThreeStepResponse : BaseResponse
    {
        [JsonProperty( "result" )]
        public string Result { get; set; }

        /// <summary>
        /// Determines whether response indicates an error
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </returns>
        public bool IsError() => Result != "1";

        [JsonProperty( "result-text" )]
        public string ResultText { get; set; }

        [JsonProperty( "transaction-id" )]
        public string TransactionId { get; set; }

        [JsonProperty( "result-code" )]
        public string ResultCode { get; set; }

        [JsonProperty( "form-url" )]
        public string FormUrl { get; set; }
    }

    internal class ThreeStepChargeStep3Response : ThreeStepResponse
    {
        [JsonProperty( "action-type" )]
        [JsonConverter( typeof( SingleOrArrayJsonConverter<string> ) )]
        public string[] ActionType { get; set; }

        [JsonProperty( "authorization-code" )]
        public string AuthorizationCode { get; set; }

        [JsonProperty( "avs-result" )]
        public string AvsResult { get; set; }

        [JsonProperty( "cvv-result" )]
        public string CvvResult { get; set; }

        [JsonProperty( "amount" )]
        public string Amount { get; set; }

        [JsonProperty( "amount-authorized" )]
        public string AmountAuthorized { get; set; }

        [JsonProperty( "tip-amount" )]
        public string TipAmount { get; set; }

        [JsonProperty( "surcharge-amount" )]
        public string SurchargeAmount { get; set; }

        [JsonProperty( "ip-address" )]
        public string IpAddress { get; set; }

        [JsonProperty( "industry" )]
        public string Industry { get; set; }

        [JsonProperty( "processor-id" )]
        public string ProcessorId { get; set; }

        [JsonProperty( "currency" )]
        public string Currency { get; set; }

        [JsonProperty( "customer-id" )]
        public string CustomerId { get; set; }

        [JsonProperty( "customer-vault-id" )]
        public string CustomerVaultId { get; set; }

        [JsonProperty( "tax-amount" )]
        public string TaxAmount { get; set; }

        [JsonProperty( "shipping-amount" )]
        public string ShippingAmount { get; set; }

        [JsonProperty( "billing" )]
        public ThreeStepBilling Billing { get; set; }

        [JsonProperty( "shipping" )]
        public ThreeStepShipping Shipping { get; set; }

        [JsonProperty( "sec-code" )]
        public string SecCode { get; set; }
    }

    internal class ThreeStepSubscriptionStep3Response : ThreeStepChargeStep3Response
    {
        [JsonProperty( "subscription-id" )]
        public string SubscriptionId { get; set; }
    }

    internal class ThreeStepBilling : BaseResponse
    {
        [JsonProperty( "billing-id" )]
        public string BillingId { get; set; }

        [JsonProperty( "first-name" )]
        public string FirstName { get; set; }

        [JsonProperty( "last-name" )]
        public string LastName { get; set; }

        [JsonProperty( "address1" )]
        public string Address1 { get; set; }

        [JsonProperty( "city" )]
        public string City { get; set; }

        [JsonProperty( "state" )]
        public string State { get; set; }

        [JsonProperty( "postal" )]
        public string Postal { get; set; }

        [JsonProperty( "country" )]
        public string Country { get; set; }

        [JsonProperty( "email" )]
        public string Email { get; set; }

        [JsonProperty( "cc-number" )]
        public string CcNumber { get; set; }

        [JsonProperty( "cc-exp" )]
        public string CcExp { get; set; }

        [JsonProperty( "account-number" )]
        public string AccountNumber { get; set; }

        [JsonProperty( "account-name" )]
        public string AccountName { get; set; }

        [JsonProperty( "routing-number" )]
        public string RoutingNumber { get; set; }

        [JsonProperty( "account-type" )]
        public string AccountType { get; set; }

        [JsonProperty( "entity-type" )]
        public string EntityType { get; set; }

        [JsonProperty( "priority" )]
        public string Priority { get; set; }
    }

    internal class ThreeStepShipping : BaseResponse
    {
        [JsonProperty( "shipping-id" )]
        public string ShippingId { get; set; }
    }

    internal class ThreeStepPlan
    {
        [JsonProperty( "payments" )]
        public string Payments { get; set; }

        [JsonProperty( "amount" )]
        public string Amount { get; set; }

        [JsonProperty( "day-frequency" )]
        public string DayFrequency { get; set; }

        [JsonProperty( "month-frequency" )]
        public string MonthFrequency { get; set; }

        [JsonProperty( "day-of-month" )]
        public string DayOfMonth { get; set; }
    }

    #endregion ThreeStepClasses

    /// <summary>
    /// 
    /// </summary>
    internal static class DateTimeHelper
    {
        /// <summary>
        /// Parses an NMI Formatted date to a DateTime
        /// </summary>
        /// <param name="dateString">The date string.</param>
        /// <returns></returns>
        public static DateTime? ParseDateValue( string dateString )
        {
            if ( !string.IsNullOrWhiteSpace( dateString ) && dateString.Length >= 14 )
            {
                int year = dateString.Substring( 0, 4 ).AsInteger();
                int month = dateString.Substring( 4, 2 ).AsInteger();
                int day = dateString.Substring( 6, 2 ).AsInteger();
                int hour = dateString.Substring( 8, 2 ).AsInteger();
                int min = dateString.Substring( 10, 2 ).AsInteger();
                int sec = dateString.Substring( 12, 2 ).AsInteger();

                return new DateTime( year, month, day, hour, min, sec );
            }

            return dateString.AsDateTime();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class FriendlyMessageHelper
    {
        /// <summary>
        /// Gets the friendly message.
        /// </summary>
        /// <param name="apiMessage">The API message.</param>
        /// <returns></returns>
        internal static string GetFriendlyMessage( string apiMessage )
        {
            if ( apiMessage.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var friendlyMessage = FriendlyMessageMap.GetValueOrNull( apiMessage );

            if ( friendlyMessage.IsNullOrWhiteSpace() )
            {
                if ( apiMessage.StartsWith( "Duplicate transaction REFID:" ) )
                {
                    // This can happen if using the same card number and amount within a short window ( maybe around 20 minutes? )
                    return "Duplicate transaction detected";
                }

                var friendlyMessagePartial = FriendlyMessageMap.Where( k => apiMessage.StartsWith( k.Key ) ).Select( a => a.Value ).FirstOrDefault();
                if ( friendlyMessagePartial != null )
                {
                    return friendlyMessagePartial;
                }
            }

            return friendlyMessage ?? apiMessage;
        }

        /// <summary>
        /// Gets the result code message.
        /// </summary>
        /// <param name="resultCode">The result code.</param>
        /// <param name="resultText">The result text.</param>
        /// <returns></returns>
        internal static string GetResultCodeMessage( int resultCode, string resultText )
        {
            switch ( resultCode )
            {
                case 100:
                    {
                        return "Transaction was approved.";
                    }

                case 200:
                    {
                        return "Transaction was declined by processor.";
                    }

                case 201:
                    {
                        return "Do not honor.";
                    }

                case 202:
                    {
                        return "Insufficient funds.";
                    }

                case 203:
                    {
                        return "Over limit.";
                    }

                case 204:
                    {
                        return "Transaction not allowed.";
                    }

                case 220:
                    {
                        return "Incorrect payment information.";
                    }

                case 221:
                    {
                        return "No such card issuer.";
                    }

                case 222:
                    {
                        return "No card number on file with issuer.";
                    }

                case 223:
                    {
                        return "Expired card.";
                    }

                case 224:
                    {
                        return "Invalid expiration date.";
                    }

                case 225:
                    {
                        return "Invalid card security code.";
                    }

                case 240:
                    {
                        return "Call issuer for further information.";
                    }

                case 250: // pickup card
                case 251: // lost card
                case 252: // stolen card
                case 253: // fradulent card
                    {
                        // these are more sensitive declines so sanitize them a bit but provide a code for later lookup
                        return string.Format( "This card was declined (code: {0}).", resultCode );
                    }

                case 260:
                    {
                        return string.Format( "Declined with further instructions available. ({0})", resultText );
                    }

                case 261:
                    {
                        return "Declined-Stop all recurring payments.";
                    }

                case 262:
                    {
                        return "Declined-Stop this recurring program.";
                    }

                case 263:
                    {
                        return "Declined-Update cardholder data available.";
                    }

                case 264:
                    {
                        return "Declined-Retry in a few days.";
                    }

                case 300:
                    {
                        return "Transaction was rejected by gateway.";
                    }

                case 400:
                    {
                        return "Transaction error returned by processor.";
                    }

                case 410:
                    {
                        return "Invalid merchant configuration.";
                    }

                case 411:
                    {
                        return "Merchant account is inactive.";
                    }

                case 420:
                    {
                        return "Communication error.";
                    }

                case 421:
                    {
                        return "Communication error with issuer.";
                    }

                case 430:
                    {
                        return "Duplicate transaction at processor.";
                    }

                case 440:
                    {
                        return "Processor format error.";
                    }

                case 441:
                    {
                        return "Invalid transaction information.";
                    }

                case 460:
                    {
                        return "Processor feature not available.";
                    }

                case 461:
                    {
                        return "Unsupported card type.";
                    }
            }

            return string.Empty;
        }

        /// <summary>
        /// The friendly message map
        /// </summary>
        private static readonly Dictionary<string, string> FriendlyMessageMap = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            // Credit Card related
            { "Card number must be 13-19 digits and a recognizable card format", "Invalid Credit Card Number" },
            { "ccnumber is empty", "Invalid Credit Card Number" },
            { "Expiration date must be a present or future month and year", "Invalid Expiration Date" },
            { "ccexp is empty", "Invalid Expiration Date" },
            { "cvv is empty", "Invalid CVV" },

            // Partial matches on CreditCard error
            { "Required Field cc_number is Missing or Empty", "Invalid Credit Card Number" },
            { "CVV must be 3 or 4 digits", "Invalid CVV" },

            // ACH Related
            { "Routing number must be 6 or 9 digits and a recognizable format", "Invalid Routing Number" },
            { "Account owner's name should be at least 3 characters", "Account owner's name should be at least 3 characters" },
            { "checkaba is empty", "Invalid Routing Number" },
            { "checkaccount is empty", "Invalid Account Number" },
            { "checkname is empty", "Invalid Name on Account" },

            // This seems to happen if entering an invalid ACH Account number ??
            { "Connection to tokenization service failed", "Invalid Account Number" },

            // Partial Matches on ACH errors
            { "Check Account number must contain only digits", "Invalid Check Account Number" },
            { "ABA number must contain only digits", "Invalid Routing Number" },

            // Declined
            { "FAILED", "Declined" },
            { "DECLINE", "Declined" }
        };
    }

    #region NMI.Gateway Exceptions

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class NMIGatewayException : Exception
    {
        string _stackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="NMIGatewayException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public NMIGatewayException( string errorMessage ) : this( errorMessage, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NMIGatewayException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NMIGatewayException( string errorMessage, Exception innerException ) : base( errorMessage, innerException )
        {
            // lets set the stacktrace manually to ensure we have one. Otherwise, it would usually be blank.
            _stackTrace = new System.Diagnostics.StackTrace( true ).ToString();
        }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public override string StackTrace => this._stackTrace ?? base.StackTrace;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ArgumentNullException" />
    public class NullFinancialGatewayException : ArgumentNullException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullFinancialGatewayException"/> class.
        /// </summary>
        public NullFinancialGatewayException()
            : base( "Unable to determine financial gateway" )
        {
        }
    }

    #endregion NMI.Gateway Exceptions
}