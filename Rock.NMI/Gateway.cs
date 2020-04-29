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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Xml.Linq;

using Newtonsoft.Json;

using RestSharp;

using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.NMI.Controls;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.NMI
{
    /// <summary>
    /// NMI Payment Gateway
    /// </summary>
    [DisplayName( "NMI Gateway" )]
    [Description( "" )]

    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "NMI Gateway" )]

    [TextField(
        "Security Key",
        Key = AttributeKey.SecurityKey,
        Description = "The API key",
        IsRequired = true,
        Order = 0
        )]

    [TextField(
        "Admin Username",
        Key = AttributeKey.AdminUsername,
        Description = "The username of the NMI user",
        IsRequired = true,
        Order = 1 )]

    [TextField( "Admin Password",
        Key = AttributeKey.AdminPassword,
        Description = "The password of the NMI user",
        IsRequired = true,
        IsPassword = true,
        Order = 2 )]

    [TextField(
        "Three Step API URL",
        Key = AttributeKey.ThreeStepAPIURL,
        Description = "The URL of the NMI Three Step API",
        IsRequired = true,
        DefaultValue = "https://secure.networkmerchants.com/api/v2/three-step",
        Order = 3 )]

    [TextField(
        "Query API URL",
        Key = AttributeKey.QueryApiUrl,
        Description = "The URL of the NMI Query API",
        IsRequired = true,
        DefaultValue = "https://secure.networkmerchants.com/api/query.php",
        Order = 4 )]

    [BooleanField(
        "Prompt for Name On Card",
        Key = AttributeKey.PromptForName,
        Description = "Should users be prompted to enter name on the card. This only applies when using the Three Step API.",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Prompt for Billing Address",
        Key = AttributeKey.PromptForAddress,
        Description = "Should users be prompted to enter billing address. This only applies when using the Three Step API.",
        DefaultBooleanValue = false,
        Order = 6 )]

    [TextField( "Direct Post API URL",
        Key = AttributeKey.DirectPostAPIUrl,
        Description = "The URL of the NMI Direct Post Query API",
        IsRequired = true,
        DefaultValue = "https://secure.nmi.com/api/transact.php",
        Order = 7 )]

    [TextField( "Tokenization Key",
        Key = AttributeKey.TokenizationKey,
        Description = "The Public Security Key to use for Tokenization. This is required for when using the NMI Hosted Gateway.",
        IsRequired = false,
        DefaultValue = "",
        Order = 8 )]
    public class Gateway : GatewayComponent, IThreeStepGatewayComponent, IHostedGatewayComponent
    {
        #region Attribute Keys

        /// <summary>;
        /// Keys to use for Component Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string SecurityKey = "SecurityKey";
            public const string AdminUsername = "AdminUsername";
            public const string AdminPassword = "AdminPassword";

            // NOTE: Lets call this ThreeStepAPIUrl but keep "APIUrl" for backwards compatibility 
            public const string ThreeStepAPIURL = "APIUrl";

            public const string QueryApiUrl = "QueryUrl";
            public const string DirectPostAPIUrl = "DirectPostAPIUrl";
            public const string TokenizationKey = "TokenizationKey";
            public const string PromptForName = "PromptForName";
            public const string PromptForAddress = "PromptForAddress";
        }

        #endregion Attribute Keys

        #region Gateway Component Implementation

        /// <summary>
        /// Gets the step2 form URL.
        /// </summary>
        /// <value>
        /// The step2 form URL.
        /// </value>
        public string Step2FormUrl
        {
            get
            {
                return string.Format( "~/NMIGatewayStep2.html?timestamp={0}", RockDateTime.Now.Ticks );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the gateway requires the name on card for CC processing
        /// </summary>
        /// <value>
        /// <c>true</c> if [name on card required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForNameOnCard( FinancialGateway financialGateway )
        {
            return GetAttributeValue( financialGateway, AttributeKey.PromptForName ).AsBoolean();
        }

        /// <summary>
        /// Gets a value indicating whether gateway provider needs first and last name on credit card as two distinct fields.
        /// </summary>
        /// <value>
        /// <c>true</c> if [split name on card]; otherwise, <c>false</c>.
        /// </value>
        public override bool SplitNameOnCard
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Prompts for the person name associated with a bank account.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public override bool PromptForBankAccountName( FinancialGateway financialGateway )
        {
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether [address required].
        /// </summary>
        /// <value>
        /// <c>true</c> if [address required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForBillingAddress( FinancialGateway financialGateway )
        {
            return GetAttributeValue( financialGateway, AttributeKey.PromptForAddress ).AsBoolean();
        }

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public override List<DefinedValueCache> SupportedPaymentSchedules
        {
            get
            {
                var values = new List<DefinedValueCache>();
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating if 'Saved Account' functionality is supported for frequency (i.e. one-time vs repeating )
        /// </summary>
        /// <param name="isRepeating">if set to <c>true</c> [is repeating].</param>
        /// <returns></returns>
        public override bool SupportsSavedAccount( bool isRepeating )
        {
            // NOTE: The NMI ThreeStep gateway doesn't seem to work with Saved account and scheduled transactions, but we'll deal with it by using the direct post api when in saved account mode???
            return true;
        }

        /// <summary>
        /// Flag indicating if the gateway supports modifying an existing schedule's payment method.
        /// </summary>
        public override bool IsUpdatingSchedulePaymentMethodSupported
        {
            get
            {
                /* 2020-03-02 MDP
                 * the NMI Gateway supports editing the ScheduledPaymentMethod, but as of 2020-03-02, ScheduledTransactionEdit (unhosted) doesn't support using a ThreeGateway process.
                 * However the new ScheduledTransactionEditV2 (the hosted gateways) does support editing a scheduled payment method
                 * So, we'll return true, but we'll have ScheduledTransactionEdit prevent changing method for ThreeStepGateway (NMI)
                 */
                return true;
            }
        }

        /// <summary>
        /// Gets the financial transaction parameters that are passed to step 1
        /// </summary>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetStep1Parameters( string redirectUrl )
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add( "redirect-url", redirectUrl );
            return parameters;
        }

        /// <summary>
        /// Performs the first step of a three-step charge
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        /// Url to post the Step2 request to
        /// </returns>
        /// <exception cref="ArgumentNullException">paymentInfo</exception>
        public string ChargeStep1( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                var rootElement = CreateThreeStepRootDoc( financialGateway, "sale" );

                rootElement.Add(
                    new XElement( "ip-address", paymentInfo.IPAddress ),
                    new XElement( "currency", "USD" ),
                    new XElement( "amount", paymentInfo.Amount.ToString() ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "tax-amount", "0.00" ),
                    new XElement( "shipping-amount", "0.00" ) );

                bool isReferencePayment = paymentInfo is ReferencePaymentInfo;
                if ( isReferencePayment )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    string customerVaultId = reference.GatewayPersonIdentifier;
                    if ( customerVaultId.IsNullOrWhiteSpace() )
                    {
                        // for backwards compatiblity for accounts created prior to v11
                        customerVaultId = reference.ReferenceNumber;
                    }

                    rootElement.Add( new XElement( "customer-vault-id", customerVaultId ) );
                }

                if ( paymentInfo.AdditionalParameters != null )
                {
                    if ( !isReferencePayment )
                    {
                        rootElement.Add( new XElement( "add-customer" ) );
                    }

                    foreach ( var keyValue in paymentInfo.AdditionalParameters )
                    {
                        XElement xElement = new XElement( keyValue.Key, keyValue.Value );
                        rootElement.Add( xElement );
                    }
                }

                rootElement.Add( GetThreeStepBilling( paymentInfo ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                ThreeStepResponse threeStepResponse = PostToGatewayThreeStepAPI<ThreeStepResponse>( financialGateway, xdoc );

                if ( threeStepResponse == null )
                {
                    errorMessage = "Invalid Response from NMI";
                    return null;
                }

                if ( threeStepResponse.IsError() )
                {
                    errorMessage = FriendlyMessageHelper.GetFriendlyMessage( threeStepResponse.ResultText );
                    return null;
                }

                return threeStepResponse.FormUrl;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Performs the final step of a three-step charge.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public FinancialTransaction ChargeStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( resultQueryString.IsNullOrWhiteSpace() )
                {
                    errorMessage = "invalid resultQueryString";
                    return null;
                }

                var rootElement = CreateThreeStepRootDoc( financialGateway, "complete-action" );
                rootElement.Add( new XElement( "token-id", resultQueryString.Substring( 10 ) ) );
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var threeStepChangeStep3Response = PostToGatewayThreeStepAPI<ThreeStepChargeStep3Response>( financialGateway, xdoc );

                if ( threeStepChangeStep3Response == null )
                {
                    errorMessage = "Invalid Response from NMI";
                    return null;
                }

                if ( threeStepChangeStep3Response.IsError() )
                {
                    errorMessage = FriendlyMessageHelper.GetFriendlyMessage( threeStepChangeStep3Response.ResultText );

                    string resultCodeMessage = FriendlyMessageHelper.GetResultCodeMessage( threeStepChangeStep3Response.ResultCode?.AsInteger() ?? 0, errorMessage );
                    if ( resultCodeMessage.IsNotNullOrWhiteSpace() )
                    {
                        errorMessage += $" ({resultCodeMessage})";
                    }

                    // write result error as an exception
                    ExceptionLogService.LogException( new NMIGatewayException( $@"Error processing NMI transaction.
Result Code:  {threeStepChangeStep3Response.ResultCode} ({resultCodeMessage}).
Result text: {threeStepChangeStep3Response.ResultText}.
Amount: {threeStepChangeStep3Response.Amount}.
Transaction id: {threeStepChangeStep3Response.TransactionId}.
                    Card Holder Name: {threeStepChangeStep3Response.Billing?.FirstName} {threeStepChangeStep3Response.Billing?.LastName}." ) );

                    return null;
                }

                var transaction = new FinancialTransaction();
                transaction.TransactionCode = threeStepChangeStep3Response.TransactionId;
                transaction.ForeignKey = threeStepChangeStep3Response.CustomerVaultId;
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                transaction.FinancialPaymentDetail.GatewayPersonIdentifier = threeStepChangeStep3Response.CustomerVaultId;

                string ccNumber = threeStepChangeStep3Response.Billing?.CcNumber;
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    transaction.FinancialPaymentDetail.NameOnCardEncrypted = Encryption.EncryptString( $"{threeStepChangeStep3Response.Billing?.FirstName} {threeStepChangeStep3Response.Billing?.LastName}" );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;
                    transaction.FinancialPaymentDetail.CreditCardTypeValueId = CreditCardPaymentInfo.GetCreditCardTypeFromCreditCardNumber( ccNumber.Replace( '*', '1' ).AsNumeric() )?.Id;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                    string mmyy = threeStepChangeStep3Response.Billing?.CcExp;
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        transaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        transaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = threeStepChangeStep3Response.Billing?.AccountNumber.Masked( true );
                }

                transaction.AdditionalLavaFields = GetAdditionalLavaFields( threeStepChangeStep3Response );

                return transaction;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Credits (Refunds) the specified transaction.
        /// </summary>
        /// <param name="origTransaction">The original transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            errorMessage = string.Empty;

            var financialGateway = origTransaction?.FinancialGateway ?? new FinancialGatewayService( new RockContext() ).Get( origTransaction.FinancialGatewayId ?? 0 );

            if ( financialGateway == null )
            {
                throw new NullFinancialGatewayException();
            }

            if ( origTransaction.TransactionCode.IsNullOrWhiteSpace() )
            {
                errorMessage = "Invalid transaction code";
                return null;
            }

            // https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#transaction_variables at 'Refund'
            var queryParameters = new Dictionary<string, string>();
            queryParameters.Add( "type", "refund" );
            queryParameters.Add( "transactionid", origTransaction.TransactionCode );
            queryParameters.Add( "amount", amount.ToString( "0.00" ) );

            var refundResponse = PostToGatewayDirectPostAPI<RefundResponse>( financialGateway, queryParameters );

            if ( refundResponse == null )
            {
                errorMessage = "Invalid Response from NMI";
                return null;
            }

            if ( refundResponse.IsError() )
            {
                errorMessage = FriendlyMessageHelper.GetFriendlyMessage( refundResponse.ResponseText );
                return null;
            }

            // return a refund transaction
            var transaction = new FinancialTransaction();
            transaction.TransactionCode = refundResponse.TransactionId;
            return transaction;
        }

        /// <summary>
        /// Performs the first step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">paymentInfo</exception>
        public string AddScheduledPaymentStep1( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                var rootElement = CreateThreeStepRootDoc( financialGateway, "add-subscription" );

                rootElement.Add(
                    new XElement( "start-date", schedule.StartDate.ToString( "yyyyMMdd" ) ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "currency", "USD" ),
                    new XElement( "tax-amount", "0.00" ) );

                bool isReferencePayment = paymentInfo is ReferencePaymentInfo;

                if ( isReferencePayment )
                {
                    /* MDP 2020-02-28
                     This doesn't work, and never has! When AddScheduledPaymentStep3 is called, a 'ccnumber ..' error occurs.
                     We got around it by not allowing saved accounts for Scheduled Transactions.
                     However, as of V11, we now use the DirectPost API (AddScheduledPayment) instead of AddScheduledPaymentStep3 to add a scheduled transaction 
                     */

                    var reference = paymentInfo as ReferencePaymentInfo;
                    rootElement.Add( new XElement( "customer-vault-id", reference.ReferenceNumber ) );
                }

                if ( paymentInfo.AdditionalParameters != null )
                {
                    foreach ( var keyValue in paymentInfo.AdditionalParameters )
                    {
                        XElement xElement = new XElement( keyValue.Key, keyValue.Value );
                        rootElement.Add( xElement );
                    }
                }

                rootElement.Add( GetThreeStepPlan( schedule, paymentInfo ) );

                rootElement.Add( GetThreeStepBilling( paymentInfo ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var threeStepResponse = PostToGatewayThreeStepAPI<ThreeStepResponse>( financialGateway, xdoc );

                if ( threeStepResponse == null )
                {
                    errorMessage = "Invalid Response from NMI";
                    return null;
                }

                if ( threeStepResponse.IsError() )
                {
                    errorMessage = FriendlyMessageHelper.GetFriendlyMessage( threeStepResponse.ResultText );
                    return null;
                }

                return threeStepResponse.FormUrl;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Performs the third step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">tokenId</exception>
        public FinancialScheduledTransaction AddScheduledPaymentStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                var rootElement = CreateThreeStepRootDoc( financialGateway, "complete-action" );
                rootElement.Add( new XElement( "token-id", resultQueryString.Substring( 10 ) ) );
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var threeStepSubscriptionStep3Response = PostToGatewayThreeStepAPI<ThreeStepSubscriptionStep3Response>( financialGateway, xdoc );

                if ( threeStepSubscriptionStep3Response == null )
                {
                    errorMessage = "Invalid Response from NMI";
                    return null;
                }

                if ( threeStepSubscriptionStep3Response.IsError() )
                {
                    errorMessage = FriendlyMessageHelper.GetFriendlyMessage( threeStepSubscriptionStep3Response.ResultText );
                    return null;
                }

                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.GatewayScheduleId = threeStepSubscriptionStep3Response.SubscriptionId;
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;
                scheduledTransaction.TransactionCode = threeStepSubscriptionStep3Response.TransactionId;

                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                scheduledTransaction.FinancialPaymentDetail.GatewayPersonIdentifier = threeStepSubscriptionStep3Response.CustomerVaultId;

                var customerId = threeStepSubscriptionStep3Response.CustomerVaultId;
                if ( customerId.IsNullOrWhiteSpace() )
                {
                    var paymentInfo = new ReferencePaymentInfo();
                    string createVaultErrorMessage;
                    customerId = CreateCustomerVaultFromExistingScheduledTransaction( financialGateway, paymentInfo, scheduledTransaction, out createVaultErrorMessage );
                }

                string ccNumber = threeStepSubscriptionStep3Response.Billing?.CcNumber;
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;
                    scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId = CreditCardPaymentInfo.GetCreditCardTypeFromCreditCardNumber( ccNumber.Replace( '*', '1' ).AsNumeric() )?.Id;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                    string mmyy = threeStepSubscriptionStep3Response.Billing?.CcExp;
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        scheduledTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        scheduledTransaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = threeStepSubscriptionStep3Response.Billing?.AccountNumber.Masked( true );
                }

                scheduledTransaction.FinancialPaymentDetail.GatewayPersonIdentifier = customerId;
                scheduledTransaction.TransactionCode = customerId;

                GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );

                return scheduledTransaction;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Cancels the scheduled payment using the ThreeStep API
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            var financialGateway = transaction.FinancialGateway ?? new FinancialGatewayService( new RockContext() ).Get( transaction.FinancialGatewayId.Value );
            DeleteSubscription( financialGateway, transaction.GatewayScheduleId );
            transaction.IsActive = false;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the scheduled payment status supported.
        /// </summary>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatusSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            var financialGateway = new FinancialGatewayService( new RockContext() ).Get( transaction.FinancialGatewayId.Value );

            var restClient = new RestClient( GetAttributeValue( financialGateway, AttributeKey.QueryApiUrl ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, AttributeKey.AdminUsername ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, AttributeKey.AdminPassword ) );
            restRequest.AddParameter( "report_type", "recurring" );
            restRequest.AddParameter( "subscription_id", transaction.GatewayScheduleId );

            var response = restClient.Execute( restRequest );
            if ( response != null )
            {
                if ( response.StatusCode == HttpStatusCode.OK )
                {
                    var xdocResult = GetXmlResponse( response );
                    var subscriptionNode = xdocResult.Root.Element( "subscription" );
                    if ( subscriptionNode == null )
                    {
                        errorMessage = "subscription not found";
                        return false;
                    }

                    string subscriptionJson = JsonConvert.SerializeXNode( subscriptionNode );

                    Subscription subscription;
                    try
                    {
                        subscription = subscriptionJson.FromJsonOrThrow<SubscriptionResult>()?.Subscription;
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new NMIGatewayException( "Unexpected PaymentStatus response from NMI Gateway", ex ) );
                        subscription = null;
                    }

                    var subscription_id = subscription?.SubscriptionId;

                    if ( subscription_id == null )
                    {
                        errorMessage = "unable to determine subscription_id from response";
                        return false;
                    }
                    else if ( subscription_id != transaction.GatewayScheduleId )
                    {
                        errorMessage = "subscription_id mismatch in response";
                        return false;
                    }

                    transaction.NextPaymentDate = subscription.NextChargeDate;
                    transaction.LastStatusUpdateDateTime = RockDateTime.Now;

                    return true;
                }
            }

            errorMessage = "Unexpected response";
            return false;
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;

            var paymentList = new List<Payment>();

            var restClient = new RestClient( GetAttributeValue( financialGateway, AttributeKey.QueryApiUrl ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, AttributeKey.AdminUsername ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, AttributeKey.AdminPassword ) );
            restRequest.AddParameter( "start_date", startDate.ToString( "yyyyMMddHHmmss" ) );
            restRequest.AddParameter( "end_date", endDate.ToString( "yyyyMMddHHmmss" ) );

            try
            {
                var response = restClient.Execute( restRequest );
                if ( response == null )
                {
                    errorMessage = "Empty response returned From gateway.";
                    return paymentList;
                }

                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    errorMessage = $"Status code of {response.StatusCode} returned From gateway.";
                    return paymentList;
                }

                XDocument doc = XDocument.Parse( response.Content );

                var responseNode = doc.Descendants( "nm_response" ).FirstOrDefault();
                var jsonResponse = JsonConvert.SerializeXNode( responseNode );

                QueryTransactionsResponse queryTransactionsResponse;
                try
                {
                    queryTransactionsResponse = jsonResponse.FromJsonOrThrow<QueryTransactionsResponse>();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new NMIGatewayException( "Unexpected GetPayments Response from NMI Gateway", ex ) );
                    queryTransactionsResponse = null;
                }

                if ( queryTransactionsResponse == null )
                {
                    errorMessage = "Unexpected response returned From gateway.";
                    return paymentList;
                }

                var errorResponse = queryTransactionsResponse?.TransactionListResult?.ErrorResponse;
                if ( errorResponse != null )
                {
                    errorMessage = errorResponse;
                    return paymentList;
                }

                var transactionList = queryTransactionsResponse?.TransactionListResult?.TransactionList;
                if ( transactionList == null )
                {
                    errorMessage = "Unexpected transaction list response returned From gateway.";
                    return paymentList;
                }

                foreach ( Transaction transaction in transactionList )
                {
                    Payment payment = new Payment();
                    payment.TransactionCode = transaction.TransactionId;

                    payment.Status = transaction.Condition?.FixCase();
                    payment.IsFailure =
                        payment.Status == "Failed" ||
                        payment.Status == "Abandoned" ||
                        payment.Status == "Canceled";
                    payment.TransactionCode = transaction.TransactionId;

                    payment.GatewayScheduleId = transaction.OriginalTransactionId;

                    // NMI does include CustomerId sometimes
                    payment.GatewayPersonIdentifier = transaction.CustomerId;

                    var statusMessage = new StringBuilder();
                    DateTime? transactionDateTime = null;
                    foreach ( var transactionAction in transaction.TransactionActions )
                    {
                        DateTime? actionDate = transactionAction.ActionDate;
                        string actionType = transactionAction.ActionType;

                        string responseText = transactionAction.ResponseText;

                        if ( actionDate.HasValue )
                        {
                            statusMessage.AppendFormat(
                                "{0} {1}: {2}; Status: {3}",
                                actionDate.Value.ToShortDateString(),
                                actionDate.Value.ToShortTimeString(),
                                actionType.FixCase(),
                                responseText );

                            statusMessage.AppendLine();
                        }

                        decimal? transactionAmount = transactionAction.Amount;
                        if ( transactionAmount.HasValue && actionDate.HasValue )
                        {
                            payment.Amount = transactionAmount.Value;
                        }

                        if ( actionType == "sale" )
                        {
                            transactionDateTime = actionDate.Value;
                        }

                        if ( actionType == "settle" )
                        {
                            payment.IsSettled = true;
                            payment.SettledGroupId = transactionAction.ProcessorBatchId.Trim();
                            payment.SettledDate = actionDate;
                            transactionDateTime = transactionDateTime.HasValue ? transactionDateTime.Value : actionDate.Value;
                        }

                        if ( transactionDateTime.HasValue )
                        {
                            payment.TransactionDateTime = transactionDateTime.Value;
                            payment.StatusMessage = statusMessage.ToString();
                            paymentList.Add( payment );
                        }
                    }
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                var nmiGatewayException = new NMIGatewayException( webException.Message + " - " + message, webException );
                ExceptionLogService.LogException( nmiGatewayException );
                throw nmiGatewayException;
            }

            return paymentList;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            var customerVaultId = transaction?.FinancialPaymentDetail?.GatewayPersonIdentifier;

            if ( customerVaultId.IsNullOrWhiteSpace() )
            {
                // older implementations of the NMI gateway only stored the customer vault id in transaction.ForeignKey
                customerVaultId = transaction.ForeignKey;
            }

            return customerVaultId;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            var customerVaultId = scheduledTransaction?.FinancialPaymentDetail?.GatewayPersonIdentifier;

            if ( customerVaultId.IsNullOrWhiteSpace() )
            {
                // older implementations of the NMI gateway might have stored the customer vault id in transaction.ForeignKey
                customerVaultId = scheduledTransaction.ForeignKey;
            }

            return customerVaultId;
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns></returns>
        private XElement CreateThreeStepRootDoc( FinancialGateway financialGateway, string elementName )
        {
            XElement apiKeyElement = new XElement( "api-key", GetAttributeValue( financialGateway, AttributeKey.SecurityKey ) );
            XElement rootElement = new XElement( elementName, apiKeyElement );

            return rootElement;
        }

        /// <summary>
        /// Creates a billing XML element
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetThreeStepBilling( PaymentInfo paymentInfo )
        {
            // If giving from a Business, FirstName will be blank
            // The Gateway might require a FirstName, so just put '-' if no FirstName was provided
            if ( paymentInfo.FirstName.IsNullOrWhiteSpace() )
            {
                paymentInfo.FirstName = "-";
            }

            XElement billingElement = new XElement(
                "billing",
                new XElement( "first-name", paymentInfo.FirstName ),
                new XElement( "last-name", paymentInfo.LastName ),
                new XElement( "address1", paymentInfo.Street1 ),
                new XElement( "address2", paymentInfo.Street2 ),
                new XElement( "city", paymentInfo.City ),
                new XElement( "state", paymentInfo.State ),
                new XElement( "postal", paymentInfo.PostalCode ),
                new XElement( "country", paymentInfo.Country ),
                new XElement( "phone", paymentInfo.Phone ),
                new XElement( "email", paymentInfo.Email ) );

            if ( paymentInfo is ACHPaymentInfo )
            {
                var ach = paymentInfo as ACHPaymentInfo;
                billingElement.Add( new XElement( "account-type", ach.AccountType == BankAccountType.Savings ? "savings" : "checking" ) );
                billingElement.Add( new XElement( "entity-type", "personal" ) );
            }

            return billingElement;
        }

        /// <summary>
        /// Creates a scheduled transaction plan XML element
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetThreeStepPlan( PaymentSchedule schedule, PaymentInfo paymentInfo )
        {
            var selectedFrequencyGuid = schedule.TransactionFrequencyValue.Guid.ToString().ToUpper();

            if ( selectedFrequencyGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME )
            {
                // Make sure number of payments is set to 1 for one-time future payments
                schedule.NumberOfPayments = 1;
            }

            XElement planElement = new XElement(
                "plan",
                new XElement( "payments", schedule.NumberOfPayments.HasValue ? schedule.NumberOfPayments.Value.ToString() : "0" ),
                new XElement( "amount", paymentInfo.Amount.ToString() ) );

            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    planElement.Add( new XElement( "month-frequency", "12" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    planElement.Add( new XElement( "day-frequency", "7" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    planElement.Add( new XElement( "day-frequency", "14" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    planElement.Add( new XElement( "month-frequency", "1" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
            }

            return planElement;
        }

        /// <summary>
        /// Posts to gateway using the 3-Step API.
        /// https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#3step_methodology
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private T PostToGatewayThreeStepAPI<T>( FinancialGateway financialGateway, XDocument data ) where T : class
        {
            var restClient = new RestClient( GetAttributeValue( financialGateway, AttributeKey.ThreeStepAPIURL ) );
            var restRequest = new RestRequest( Method.POST );
            restRequest.RequestFormat = DataFormat.Xml;
            restRequest.AddParameter( "text/xml", data.ToString(), ParameterType.RequestBody );

            try
            {
                var response = restClient.Execute( restRequest );
                var xdocResult = GetXmlResponse( response );
                if ( xdocResult == null )
                {
                    return null;
                }

                var settings = new JsonSerializerSettings();

                settings.Error += new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>( ( s, e ) =>
                {
                    System.Diagnostics.Debug.WriteLine( $"FromJSON Error: {e.ErrorContext?.Error?.Message}" );
                    e.ErrorContext.Handled = true;
                } );

                string jsonResponse = JsonConvert.SerializeXNode( xdocResult.Root, Formatting.None, true );

                T postResult;
                if ( typeof( T ).Equals( typeof( string ) ) )
                {
                    // if caller just wants the response as a string, return the json
                    postResult = jsonResponse as T;
                }
                else
                {
                    try
                    {
                        postResult = jsonResponse.FromJsonOrThrow<T>();
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new NMIGatewayException( "Unexpected ThreeStep Response from NMI Gateway", ex ) );
                        postResult = null;
                    }
                }

                return postResult;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                var nmiGatewayException = new NMIGatewayException( webException.Message + " - " + message, webException );
                ExceptionLogService.LogException( nmiGatewayException );
                throw nmiGatewayException;
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the additional lava fields in the form of a flatted out dictionary where child properties are delimited with '_' (Billing.Street1 becomes Billing_Street1)
        /// </summary>
        /// <param name="xdocResult">The xdoc result.</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetAdditionalLavaFields<T>( T obj )
        {
            var xdocResult = JsonConvert.DeserializeXNode( obj.ToJson(), "root" );
            var additionalLavaFields = new Dictionary<string, object>();
            foreach ( XElement element in xdocResult.Root.Elements() )
            {
                if ( element.HasElements )
                {
                    string prefix = element.Name.LocalName;
                    foreach ( XElement childElement in element.Elements() )
                    {
                        additionalLavaFields.AddOrIgnore( prefix + "_" + childElement.Name.LocalName, childElement.Value.Trim() );
                    }
                }
                else
                {
                    additionalLavaFields.AddOrIgnore( element.Name.LocalName, element.Value.Trim() );
                }
            }

            return additionalLavaFields;
        }

        /// <summary>
        /// Gets the response as an XDocument
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private XDocument GetXmlResponse( IRestResponse response )
        {
            if ( response.StatusCode == HttpStatusCode.OK &&
                response.Content.Trim().Length > 0 &&
                response.Content.Contains( "<?xml" ) )
            {
                return XDocument.Parse( response.Content );
            }

            return null;
        }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns></returns>
        private string GetResponseMessage( Stream responseStream )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            char[] read = new char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                string str = new string( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }

        #endregion

        #region DirectPost API related

        /// <summary>
        /// Posts to gateway using the Direct Post Api https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#methodology
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private T PostToGatewayDirectPostAPI<T>( FinancialGateway financialGateway, Dictionary<string, string> queryParameters ) where T : class
        {
            var directPostApiURL = GetAttributeValue( financialGateway, AttributeKey.DirectPostAPIUrl );
            var restClient = new RestClient( directPostApiURL );

            var restRequest = new RestRequest( Method.POST );
            restRequest.RequestFormat = DataFormat.Xml;
            foreach ( var queryParameter in queryParameters )
            {
                restRequest.AddQueryParameter( queryParameter.Key, queryParameter.Value );
            }

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, AttributeKey.AdminUsername ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, AttributeKey.AdminPassword ) );

            try
            {
                var response = restClient.Execute( restRequest );
                string jsonResponse;

                // deal with either an XML response or QueryString style response
                var xdocResult = GetXmlResponse( response );
                if ( xdocResult != null )
                {
                    jsonResponse = JsonConvert.SerializeXNode( xdocResult.Root );
                }
                else
                {
                    // response in the form of response=3&responsetext=Plan Payments is required REFID:123456789&authcode=&transactionid=&avsresponse=&cvvresponse=&orderid=&type=&response_code=300&customer_vault_id=
                    // so convert this to a dictionary
                    var resultAsDictionary = response?.Content?.Split( '&' ).ToList().Select( s => s.Split( '=' ) ).Where( a => a.Length == 2 ).ToDictionary( k => k[0], v => v[1] );
                    jsonResponse = resultAsDictionary.ToJson();
                }

                T postResult;
                if ( typeof( T ).Equals( typeof( string ) ) )
                {
                    // if caller just wants the response as a string, return the json
                    postResult = jsonResponse as T;
                }
                else
                {
                    try
                    {
                        postResult = jsonResponse.FromJsonOrThrow<T>();
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new NMIGatewayException( "Unexpected DirectPost Response from NMI Gateway", ex ) );
                        postResult = null;
                    }
                }

                return postResult;
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );

                var nmiGatewayException = new NMIGatewayException( webException.Message + " - " + message, webException );
                ExceptionLogService.LogException( nmiGatewayException );
                throw nmiGatewayException;
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Charges the specified payment info using the DirectPost API
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            if ( financialGateway == null )
            {
                throw new NullFinancialGatewayException();
            }

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            var tokenizerToken = referencedPaymentInfo.ReferenceNumber;
            var amount = referencedPaymentInfo.Amount;

            // https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#transaction_variables
            var queryParameters = new Dictionary<string, string>();
            queryParameters.Add( "type", "sale" );

            // if both customerId and tokenizerToken are available, use customerId
            if ( customerId.IsNotNullOrWhiteSpace() )
            {
                queryParameters.Add( "customer_vault_id", customerId );
            }
            else
            {
                queryParameters.Add( "payment_token", tokenizerToken );
            }

            queryParameters.Add( "amount", amount.ToString( "0.00" ) );

            PopulateAddressParameters( referencedPaymentInfo, queryParameters );

            StringBuilder stringBuilderDescription = new StringBuilder();
            if ( referencedPaymentInfo.Description.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Description );
            }

            if ( referencedPaymentInfo.Comment1.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Comment1 );
            }

            if ( referencedPaymentInfo.Comment2.IsNotNullOrWhiteSpace() )
            {
                stringBuilderDescription.AppendLine( referencedPaymentInfo.Comment2 );
            }

            var description = stringBuilderDescription.ToString().Truncate( 255 );

            queryParameters.Add( "order_description", description );
            queryParameters.Add( "ipaddress", referencedPaymentInfo.IPAddress );

            var chargeResponse = PostToGatewayDirectPostAPI<ChargeResponse>( financialGateway, queryParameters );

            // NOTE: When debugging, you might get a Duplicate Transaction error if using the same CustomerVaultId and Amount within a short window ( maybe around 20 minutes? ) 
            if ( chargeResponse.IsError() )
            {
                errorMessage = FriendlyMessageHelper.GetFriendlyMessage( chargeResponse.ResponseText );

                string resultCodeMessage = FriendlyMessageHelper.GetResultCodeMessage( chargeResponse.ResponseCode.AsInteger(), chargeResponse.ResponseText );
                if ( resultCodeMessage.IsNotNullOrWhiteSpace() )
                {
                    errorMessage += string.Format( " ({0})", resultCodeMessage );
                }

                // write result error as an exception
                var exception = new NMIGatewayException( $"Error processing NMI transaction. Result Code:  {chargeResponse.ResponseCode} ({resultCodeMessage}). Result text: {chargeResponse.ResponseText} " );
                ExceptionLogService.LogException( exception );

                return null;
            }

            var transaction = new FinancialTransaction();
            transaction.TransactionCode = chargeResponse.TransactionId;
            transaction.ForeignKey = chargeResponse.CustomerVaultId;

            Customer customerInfo = this.GetCustomerVaultQueryResponse( financialGateway, customerId )?.CustomerVault.Customer;
            transaction.FinancialPaymentDetail = CreatePaymentPaymentDetail( customerInfo );

            transaction.AdditionalLavaFields = GetAdditionalLavaFields( chargeResponse );

            return transaction;
        }

        /// <summary>
        /// Populates the payment information.
        /// </summary>
        /// <param name="customerInfo">The customer information.</param>
        /// <returns></returns>
        private FinancialPaymentDetail CreatePaymentPaymentDetail( Customer customerInfo )
        {
            var financialPaymentDetail = new FinancialPaymentDetail();
            UpdateFinancialPaymentDetail( customerInfo, financialPaymentDetail );
            return financialPaymentDetail;
        }

        /// <summary>
        /// Updates the financial payment detail fields from the information in customerInfo
        /// </summary>
        /// <param name="customerInfo">The customer information.</param>
        /// <param name="financialPaymentDetail">The financial payment detail.</param>
        private void UpdateFinancialPaymentDetail( Customer customerInfo, FinancialPaymentDetail financialPaymentDetail )
        {
            financialPaymentDetail.GatewayPersonIdentifier = customerInfo.CustomerVaultId;

            string ccNumber = customerInfo.CcNumber;
            if ( !string.IsNullOrWhiteSpace( ccNumber ) )
            {
                // cc payment
                var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                financialPaymentDetail.NameOnCardEncrypted = Encryption.EncryptString( $"{customerInfo.FirstName} {customerInfo.LastName}" );
                financialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;

                //// The gateway tells us what the CreditCardType is since it was selected using their hosted payment entry frame.
                //// So, first see if we can determine CreditCardTypeValueId using the CardType response from the gateway

                // See if we can figure it out from the CC Type (Amex, Visa, etc)
                var creditCardTypeValue = CreditCardPaymentInfo.GetCreditCardTypeFromName( customerInfo.CcType );
                if ( creditCardTypeValue == null )
                {
                    // GetCreditCardTypeFromName should have worked, but just in case, see if we can figure it out from the MaskedCard using RegEx
                    creditCardTypeValue = CreditCardPaymentInfo.GetCreditCardTypeFromCreditCardNumber( customerInfo.CcNumber );
                }

                financialPaymentDetail.CreditCardTypeValueId = creditCardTypeValue?.Id;
                financialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                string mmyy = customerInfo.CcExp;
                if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                {
                    financialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                    financialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                }
            }
            else
            {
                // ach payment
                var curType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                financialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : ( int? ) null;
                financialPaymentDetail.AccountNumberMasked = customerInfo.CheckAccount;
            }
        }

        /// <summary>
        /// Adds the scheduled payment using the DirectPost API
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            if ( financialGateway == null )
            {
                throw new NullFinancialGatewayException();
            }

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            var tokenizerToken = referencedPaymentInfo.ReferenceNumber;
            var amount = referencedPaymentInfo.Amount;

            // https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#recurring_variables @ Adding a Custom Subscription
            var queryParameters = new Dictionary<string, string>();
            queryParameters.Add( "recurring", "add_subscription" );

            var selectedFrequencyGuid = schedule.TransactionFrequencyValue.Guid.ToString().ToUpper();

            if ( selectedFrequencyGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME )
            {
                // Make sure number of payments is set to 1 for one-time future payments
                schedule.NumberOfPayments = 1;
            }

            queryParameters.Add( "plan_payments", schedule.NumberOfPayments.HasValue ? schedule.NumberOfPayments.Value.ToString() : "0" );
            queryParameters.Add( "plan_amount", amount.ToString( "0.00" ) );

            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    queryParameters.Add( "month_frequency", "12" );
                    queryParameters.Add( "day_of_month", schedule.StartDate.Day.ToString() );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    queryParameters.Add( "day_frequency", "7" );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    queryParameters.Add( "day_frequency", "14" );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    queryParameters.Add( "month_frequency", "1" );
                    queryParameters.Add( "day_of_month", schedule.StartDate.Day.ToString() );
                    break;
            }

            queryParameters.Add( "start_date", schedule.StartDate.ToString( "yyyyMMdd" ) );

            if ( customerId.IsNotNullOrWhiteSpace() )
            {
                // this isn't documented in the recurring documention, but it is on the charge documentation. It does work.
                queryParameters.Add( "customer_vault_id", customerId );
            }
            else
            {
                queryParameters.Add( "payment_token", tokenizerToken );
            }

            PopulateAddressParameters( referencedPaymentInfo, queryParameters );

            // create a guid to include in the NMI Subscription Description so that we can refer back to it to ensure an orphaned subscription doesn't exist when an exception occurs
            var descriptionGuid = Guid.NewGuid();

            var subscriptionDescription = $"{referencedPaymentInfo.Description}|Subscription Ref: {descriptionGuid}";

            queryParameters.Add( "order_description", subscriptionDescription );

            try
            {
                errorMessage = string.Empty;

                var addSubscriptionResponse = PostToGatewayDirectPostAPI<SubscriptionResponse>( financialGateway, queryParameters );

                if ( addSubscriptionResponse.IsError() )
                {
                    errorMessage = FriendlyMessageHelper.GetFriendlyMessage( addSubscriptionResponse.ResponseText );

                    string resultCodeMessage = FriendlyMessageHelper.GetResultCodeMessage( addSubscriptionResponse.ResponseCode.AsInteger(), addSubscriptionResponse.ResponseText );
                    if ( resultCodeMessage.IsNotNullOrWhiteSpace() )
                    {
                        errorMessage += string.Format( " ({0})", resultCodeMessage );
                    }

                    // write result error as an exception
                    var exception = new NMIGatewayException( $"Error processing NMI subscription. Result Code:  {addSubscriptionResponse.ResponseCode} ({resultCodeMessage}). Result text: {addSubscriptionResponse.ResponseText} " );
                    ExceptionLogService.LogException( exception );

                    return null;
                }

                // set the paymentInfo.TransactionCode to the subscriptionId so that we know what CreateSubsciption created.
                // this might be handy in case we have an exception and need to know what the subscriptionId is
                referencedPaymentInfo.TransactionCode = addSubscriptionResponse.TransactionId;

                Customer customerInfo;
                try
                {
                    customerInfo = this.GetCustomerVaultQueryResponse( financialGateway, customerId )?.CustomerVault.Customer;

                    // NOTE: NMI updates the customer address when doing the AddSubscription request
                }
                catch ( Exception ex )
                {
                    throw new NMIGatewayException( $"Exception getting Customer Information for Scheduled Payment.", ex );
                }

                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.GatewayScheduleId = addSubscriptionResponse.TransactionId;
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;
                scheduledTransaction.FinancialPaymentDetail = CreatePaymentPaymentDetail( customerInfo );
                scheduledTransaction.TransactionCode = addSubscriptionResponse.TransactionId;

                errorMessage = string.Empty;

                try
                {
                    GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
                }
                catch ( Exception ex )
                {
                    throw new NMIGatewayException( $"Exception getting Scheduled Payment Status. {errorMessage}", ex );
                }

                return scheduledTransaction;
            }
            catch ( Exception )
            {
                // if there is an exception, Rock won't save this as a scheduled transaction, so make sure the subscription didn't get created so mystery scheduled transactions don't happen

                // NOTE: NMI doesn't have a way to search subscriptions for a specific customer, so just get the last few of them and find the one we created and delete it
                var subscriptionSearchResult = this.SearchSubscriptions( financialGateway, 10 );

                var orphanedSubscription = subscriptionSearchResult?.SubscriptionsResult?.SubscriptionList?.FirstOrDefault( a => a.OrderDescription == subscriptionDescription );

                if ( orphanedSubscription?.SubscriptionId != null )
                {
                    var subscriptionId = orphanedSubscription.SubscriptionId;
                    var deleteSubscriptionResponse = this.DeleteSubscription( financialGateway, subscriptionId );
                }

                throw;
            }
        }

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        /// <returns></returns>
        public override bool ReactivateScheduledPaymentSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            var referencePaymentInfo = new ReferencePaymentInfo();

            // NOTE: If transaction.FinancialPaymentDetail.GatewayPersonIdentifier is blank, a new customervault will be created from the GatewayScheduleId. So it is ok if both GatewayPersonIdentifier and FinancialPersonSavedAccountId are unknown
            referencePaymentInfo.GatewayPersonIdentifier = transaction.FinancialPaymentDetail.GatewayPersonIdentifier;
            referencePaymentInfo.FinancialPersonSavedAccountId = transaction.FinancialPaymentDetail.FinancialPersonSavedAccountId;
            referencePaymentInfo.Amount = transaction.TotalAmount;

            return UpdateScheduledPayment( transaction, referencePaymentInfo, out errorMessage );
        }

        /// <summary>
        /// Updates the scheduled payment supported.
        /// </summary>
        /// <returns></returns>
        public override bool UpdateScheduledPaymentSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Updates the scheduled payment. In the case of NMI, the scheduled payment will get deleted and a new one created to replace it.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            PaymentSchedule paymentSchedule = new PaymentSchedule
            {
                TransactionFrequencyValue = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ),
                StartDate = scheduledTransaction.StartDate,
                EndDate = scheduledTransaction.EndDate,
                NumberOfPayments = scheduledTransaction.NumberOfPayments,
                PersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId
            };

            var financialGateway = scheduledTransaction.FinancialGateway ?? new FinancialGatewayService( new RockContext() ).Get( scheduledTransaction.FinancialGatewayId.Value );

            if ( referencedPaymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                // if this scheduled transaction was created without saving the customerId to Rock data (from previous version) or unknown to Rock for some other reason, create a new customer vault record.
                // Note: NMI doesn't support getting the CustomerVaultId from an existing subscription, but we get around that by create a new customer vault id from the existing transaction
                referencedPaymentInfo.GatewayPersonIdentifier = CreateCustomerVaultFromExistingScheduledTransaction( financialGateway, referencedPaymentInfo, scheduledTransaction, out errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return false;
                }
            }

            // since we can't update a subscription in NMI, we'll have to Delete and Create a new one
            DeleteSubscription( scheduledTransaction.FinancialGateway, scheduledTransaction.GatewayScheduleId );

            // add the scheduled payment, but don't use the financialScheduledTransaction that was returned since we already have one
            var dummyFinancialScheduledTransaction = AddScheduledPayment( scheduledTransaction.FinancialGateway, paymentSchedule, paymentInfo, out errorMessage );
            if ( dummyFinancialScheduledTransaction != null )
            {
                scheduledTransaction.GatewayScheduleId = dummyFinancialScheduledTransaction.GatewayScheduleId;

                scheduledTransaction.IsActive = true;
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;

                try
                {
                    // update FinancialPaymentDetail with any changes in payment information
                    Customer customerInfo = this.GetCustomerVaultQueryResponse( financialGateway, referencedPaymentInfo.GatewayPersonIdentifier )?.CustomerVault.Customer;
                    UpdateFinancialPaymentDetail( customerInfo, scheduledTransaction.FinancialPaymentDetail );
                }
                catch ( Exception ex )
                {
                    throw new NMIGatewayException( $"Exception getting Customer Information for Scheduled Payment.", ex );
                }

                errorMessage = string.Empty;

                try
                {
                    GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
                }
                catch ( Exception ex )
                {
                    throw new NMIGatewayException( $"Exception getting Scheduled Payment Status. {errorMessage}", ex );
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the subscription using the DirectPost API
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        private SubscriptionResponse DeleteSubscription( FinancialGateway financialGateway, string subscriptionId )
        {
            if ( financialGateway == null )
            {
                throw new NullFinancialGatewayException();
            }

            var queryParameters = new Dictionary<string, string>();
            queryParameters.Add( "recurring", "delete_subscription" );
            queryParameters.Add( "subscription_id", subscriptionId );

            var deleteSubscriptionResponse = PostToGatewayDirectPostAPI<SubscriptionResponse>( financialGateway, queryParameters );

            return deleteSubscriptionResponse;
        }

        /// <summary>
        /// Searches the subscriptions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultLimit">The result limit.</param>
        /// <returns></returns>
        private QuerySubscriptionsResponse SearchSubscriptions( FinancialGateway financialGateway, int resultLimit )
        {
            var restClient = new RestClient( GetAttributeValue( financialGateway, AttributeKey.QueryApiUrl ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, AttributeKey.AdminUsername ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, AttributeKey.AdminPassword ) );
            restRequest.AddParameter( "source", "recurring" );
            restRequest.AddParameter( "report_type", "recurring" );

            /* MDP 2020-02-24 The 'recurring' query is very limited. For example, none of these options work
             * customer_vault_id
             * date_search
             * start_date
             * end_date
             * order_description
             *
             * Also, the results are missing some fields that would be helpful to have. For example, these fields are not returrned
             * customer_vault_id
            */

            // order from newest to oldest
            restRequest.AddParameter( "result_order", "reverse" );
            restRequest.AddParameter( "result_limit", resultLimit.ToString() );

            var response = restClient.Execute( restRequest );
            XDocument doc = XDocument.Parse( response.Content );

            var responseNode = doc.Descendants( "nm_response" ).FirstOrDefault();
            var jsonResponse = JsonConvert.SerializeXNode( responseNode );

            QuerySubscriptionsResponse querySubscriptionsResponse;

            try
            {
                querySubscriptionsResponse = jsonResponse.FromJsonOrThrow<QuerySubscriptionsResponse>();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new NMIGatewayException( "Unexpected QuerySubscriptions Response from NMI Gateway", ex ) );
                querySubscriptionsResponse = null;
            }

            return querySubscriptionsResponse;
        }

        /// <summary>
        /// Gets the customer vault query response.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="customerVaultId">The customer vault identifier.</param>
        /// <returns></returns>
        private CustomerVaultQueryResponse GetCustomerVaultQueryResponse( FinancialGateway financialGateway, string customerVaultId )
        {
            var restClient = new RestClient( GetAttributeValue( financialGateway, AttributeKey.QueryApiUrl ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, AttributeKey.AdminUsername ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, AttributeKey.AdminPassword ) );
            restRequest.AddParameter( "report_type", "customer_vault" );
            restRequest.AddParameter( "customer_vault_id", customerVaultId );

            var response = restClient.Execute( restRequest );
            XDocument doc = XDocument.Parse( response.Content );

            var customerVaultNode = doc.Descendants( "customer_vault" ).FirstOrDefault();
            var jsonResponse = JsonConvert.SerializeXNode( customerVaultNode );

            CustomerVaultQueryResponse customerVaultQueryResponse;
            try
            {
                customerVaultQueryResponse = jsonResponse.FromJsonOrThrow<CustomerVaultQueryResponse>();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new NMIGatewayException( "Unexpected CustomerVault response from NMI Gateway", ex ) );
                customerVaultQueryResponse = null;
            }

            return customerVaultQueryResponse;
        }

        #endregion DirectPost API related

        #region Exceptions

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Exception" />
        public class ReferencePaymentInfoRequired : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReferencePaymentInfoRequired"/> class.
            /// </summary>
            public ReferencePaymentInfoRequired()
                : base( "NMI gateway requires a token or customer reference" )
            {
            }
        }

        #endregion 

        #region IHostedGatewayComponent

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Configure' link
        /// </summary>
        /// <value>
        /// The configure URL.
        /// </value>
        public string ConfigureURL => "https://www.nmi.com/";

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Learn More' link
        /// </summary>
        /// <value>
        /// The learn more URL.
        /// </value>
        public string LearnMoreURL => "https://www.nmi.com/";

        /// <summary>
        /// Gets the hosted gateway modes that this gateway has configured/supports. Use this to determine which mode to use (in cases where both are supported, like Scheduled Payments lists ).
        /// If the Gateway supports both hosted and unhosted (and has Hosted mode configured), hosted mode should be preferred.
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <returns></returns>
        /// <value>
        /// The hosted gateway modes that this gateway supports
        /// </value>
        public HostedGatewayMode[] GetSupportedHostedGatewayModes( FinancialGateway financialGateway )
        {
            // NMI Gateway supports Hosted mode if a TokenizationKey is configured. If so, Hosted gateway mode should be preferred
            var hostedGatewayConfigured = this.GetAttributeValue( financialGateway, AttributeKey.TokenizationKey ).IsNotNullOrWhiteSpace();
            if ( !hostedGatewayConfigured )
            {
                return new HostedGatewayMode[1] { HostedGatewayMode.Unhosted };
            }
            else
            {
                return new HostedGatewayMode[2] { HostedGatewayMode.Hosted, HostedGatewayMode.Unhosted };
            }
        }

        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// Note: A HostedPaymentInfoControl can optionally implement <seealso cref="T:Rock.Financial.IHostedGatewayPaymentControlTokenEvent" />
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId, HostedPaymentInfoControlOptions options )
        {
            NMIHostedPaymentControl nmiHostedPaymentControl = new NMIHostedPaymentControl { ID = controlId };
            nmiHostedPaymentControl.NMIGateway = this;
            List<NMIPaymentType> enabledPaymentTypes = new List<NMIPaymentType>();
            if ( options?.EnableACH ?? true )
            {
                enabledPaymentTypes.Add( NMIPaymentType.ach );
            }

            if ( options?.EnableCreditCard ?? true )
            {
                enabledPaymentTypes.Add( NMIPaymentType.card );
            }

            nmiHostedPaymentControl.EnabledPaymentTypes = enabledPaymentTypes.ToArray();

            nmiHostedPaymentControl.TokenizationKey = this.GetAttributeValue( financialGateway, AttributeKey.TokenizationKey );

            return nmiHostedPaymentControl;
        }

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token.
        /// Have your 'Next' or 'Submit' call this so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl )
        {
            return $"Rock.NMI.controls.gatewayCollectJS.submitPaymentInfo('{hostedPaymentInfoControl.ClientID}');";
        }

        /// <summary>
        /// Populates the properties of the referencePaymentInfo from this gateway's <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" >hostedPaymentInfoControl</seealso>
        /// This includes the ReferenceNumber, plus any other fields that the gateway wants to set
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <param name="referencePaymentInfo">The reference payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        public void UpdatePaymentInfoFromPaymentControl( FinancialGateway financialGateway, Control hostedPaymentInfoControl, ReferencePaymentInfo referencePaymentInfo, out string errorMessage )
        {
            var nmiHostedPaymentControl = hostedPaymentInfoControl as NMIHostedPaymentControl;
            errorMessage = null;
            TokenizerResponse tokenResponse;
            try
            {
                tokenResponse = nmiHostedPaymentControl.PaymentInfoTokenRaw.FromJsonOrThrow<TokenizerResponse>();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new NMIGatewayException( "Unexpected Token Response from NMI Gateway", ex ) );
                tokenResponse = null;
            }

            if ( tokenResponse?.IsSuccessStatus() != true )
            {
                if ( tokenResponse.HasValidationError() )
                {
                    errorMessage = tokenResponse.ValidationMessage;
                }

                errorMessage = tokenResponse?.ErrorMessage ?? "null response from GetHostedPaymentInfoToken";
                referencePaymentInfo.ReferenceNumber = nmiHostedPaymentControl.PaymentInfoToken;
                errorMessage = FriendlyMessageHelper.GetFriendlyMessage( errorMessage );
            }
            else
            {
                referencePaymentInfo.ReferenceNumber = nmiHostedPaymentControl.PaymentInfoToken;
            }
        }

        /// <summary>
        /// Creates the customer account using a token received from the HostedPaymentInfoControl <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String,Rock.Financial.HostedPaymentInfoControlOptions)" />
        /// and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            return CreateCustomerVault( financialGateway, paymentInfo, out errorMessage );
        }

        /// <summary>
        /// Creates the customer vault from existing scheduled transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="financialScheduledTransaction">The financial scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private string CreateCustomerVaultFromExistingScheduledTransaction( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, FinancialScheduledTransaction financialScheduledTransaction, out string errorMessage )
        {
            // NOTE that *does* work even if the GatewayScheduleId is from a deleted subscription. NMI just soft-deletes subscriptions, and can get the customer vault from those too.
            paymentInfo.TransactionCode = financialScheduledTransaction.GatewayScheduleId;
            return CreateCustomerVault( financialGateway, paymentInfo, out errorMessage );
        }

        /// <summary>
        /// Creates the customer vault.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private string CreateCustomerVault( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( financialGateway == null )
            {
                throw new NullFinancialGatewayException();
            }

            // see https://secure.tnbcigateway.com/merchants/resources/integration/integration_portal.php?#cv_variables
            var queryParameters = new Dictionary<string, string>();
            queryParameters.Add( "customer_vault", "add_customer" );

            if ( paymentInfo.ReferenceNumber.IsNotNullOrWhiteSpace() )
            {
                queryParameters.Add( "payment_token", paymentInfo.ReferenceNumber );
            }
            else
            {
                queryParameters.Add( "source_transaction_id", paymentInfo.TransactionCode );
            }

            PopulateAddressParameters( paymentInfo, queryParameters );

            var createCustomerResponse = PostToGatewayDirectPostAPI<CreateCustomerResponse>( financialGateway, queryParameters );

            if ( createCustomerResponse.IsError() )
            {
                errorMessage = FriendlyMessageHelper.GetFriendlyMessage( createCustomerResponse.ResponseText );

                string resultCodeMessage = FriendlyMessageHelper.GetResultCodeMessage( createCustomerResponse.ResponseCode.AsInteger(), errorMessage );
                if ( resultCodeMessage.IsNotNullOrWhiteSpace() )
                {
                    errorMessage += $" ({resultCodeMessage})";
                }

                // write result error as an exception
                var exception = new NMIGatewayException( $"Error creating NMI customer. Result Code:  {createCustomerResponse.ResponseCode} ({resultCodeMessage}). Result text: {createCustomerResponse.ResponseText} " );
                ExceptionLogService.LogException( exception );

                return null;
            }

            return createCustomerResponse.CustomerVaultId;
        }

        /// <summary>
        /// Populates the address parameters.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="queryParameters">The query parameters.</param>
        private static void PopulateAddressParameters( ReferencePaymentInfo referencedPaymentInfo, Dictionary<string, string> queryParameters )
        {
            if ( !referencedPaymentInfo.IncludesAddressData() )
            {
                return;
            }

            queryParameters.Add( "first_name", referencedPaymentInfo.FirstName );
            queryParameters.Add( "last_name", referencedPaymentInfo.LastName );
            queryParameters.Add( "address1", referencedPaymentInfo.Street1 );
            queryParameters.Add( "address2", referencedPaymentInfo.Street2 );
            queryParameters.Add( "city", referencedPaymentInfo.City );
            queryParameters.Add( "state", referencedPaymentInfo.State );
            queryParameters.Add( "zip", referencedPaymentInfo.PostalCode );
            queryParameters.Add( "country", referencedPaymentInfo.Country );
            queryParameters.Add( "phone", referencedPaymentInfo.Phone );
            queryParameters.Add( "email", referencedPaymentInfo.Email );
            queryParameters.Add( "company", referencedPaymentInfo.BusinessName );
        }

        /// <summary>
        /// Gets the earliest scheduled start date that the gateway will accept for the start date, based on the current local time.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public DateTime GetEarliestScheduledStartDate( FinancialGateway financialGateway )
        {
            return RockDateTime.Today.AddDays( 1 ).Date;
        }

        #endregion IHostedGatewayComponent

        #region NMI Specific

        /// <summary>
        /// Gets the three step javascript which will validate the inputs and submit payment details to NMI gateway
        /// </summary>
        /// <param name="validationGroup">The validation group.</param>
        /// <param name="postbackControlReference">The postback control reference.</param>
        /// <returns></returns>
        public static string GetThreeStepJavascript( string validationGroup, string postbackControlReference )
        {
            var script = Scripts.threeStepScript.Replace( "{{validationGroup}}", validationGroup ).Replace( "{{postbackControlReference}}", postbackControlReference );
            return script;
        }

        #endregion
    }
}