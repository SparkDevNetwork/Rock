using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Web.UI;
using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.TransNational.Pi.Controls;
using Rock.Web.Cache;

// Use Newtonsoft RestRequest which is the same as RestSharp.RestRequest but uses the JSON.NET serializer
using RestRequest = RestSharp.Newtonsoft.Json.RestRequest;

namespace Rock.TransNational.Pi
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Financial.GatewayComponent" />
    [Description( "The TransNational Pi Gateway is the primary gateway to use with My Well giving." )]
    [DisplayName( "TransNational Pi Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "TransNational Pi Gateway" )]

    #region Component Attributes

    [TextField(
        "Private API Key",
        Key = AttributeKey.PrivateApiKey,
        Description = "The private API Key used for internal operations",
        Order = 1 )]

    [TextField(
        "Public API Key",
        Key = AttributeKey.PublicApiKey,
        Description = "The public API Key used for web client operations",
        Order = 2
        )]

    [CustomRadioListField( "Mode",
        Key = AttributeKey.Mode,
        Description = "Set to Sandbox mode to use the sandbox test gateway instead of the production app gateway",
        ListSource = "Live,Sandbox",
        IsRequired = true,

        DefaultValue = "Live" )]

    #endregion Component Attributes
    public class PiGateway : GatewayComponent, IHostedGatewayComponent, IAutomatedGatewayComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string PrivateApiKey = "PrivateApiKey";
            public const string PublicApiKey = "PublicApiKey";
            public const string Mode = "Mode";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        /// <value>
        /// The gateway URL.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        public string GetGatewayUrl( FinancialGateway financialGateway )
        {
            bool testMode = this.GetAttributeValue( financialGateway, AttributeKey.Mode ).Equals( "Sandbox" );
            if ( testMode )
            {
                return "https://sandbox.gotnpgateway.com";
            }
            else
            {
                return "https://app.gotnpgateway.com";
            }
        }

        /// <summary>
        /// Gets the public API key.
        /// </summary>
        /// <value>
        /// The public API key.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        public string GetPublicApiKey( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.PublicApiKey );
        }

        /// <summary>
        /// Gets the private API key.
        /// </summary>
        /// <value>
        /// The private API key.
        /// </value>
        [System.Diagnostics.DebuggerStepThrough]
        private string GetPrivateApiKey( FinancialGateway financialGateway )
        {
            return this.GetAttributeValue( financialGateway, AttributeKey.PrivateApiKey );
        }

        #region IAutomatedGatewayComponent

        /// <summary>
        /// The most recent exception thrown by the gateway's remote API
        /// </summary>
        public Exception MostRecentException { get; private set; }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public Payment AutomatedCharge( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage, Dictionary<string, string> metadata = null )
        {
            // TODO - Fees? If Pi provides fee info, we won't be able to use the Charge method as the transaction it returns
            // doesn't have the capacity to relay fee info (also the reason this method returns a Payment rather than a
            // transaction.

            // payment.FeeAmount and payment.NetAmount

            MostRecentException = null;

            try
            {
                var transaction = Charge( financialGateway, paymentInfo, out errorMessage );

                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    MostRecentException = new Exception( errorMessage );
                    return null;
                }

                var paymentDetail = transaction.FinancialPaymentDetail;

                var payment = new Payment
                {
                    AccountNumberMasked = paymentDetail.AccountNumberMasked,
                    Amount = paymentInfo.Amount,
                    ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted,
                    ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted,
                    IsSettled = transaction.IsSettled,
                    SettledDate = transaction.SettledDate,
                    NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted,
                    Status = transaction.Status,
                    StatusMessage = transaction.StatusMessage,
                    TransactionCode = transaction.TransactionCode,
                    TransactionDateTime = transaction.TransactionDateTime ?? RockDateTime.Now
                };

                if ( paymentDetail.CreditCardTypeValueId.HasValue )
                {
                    payment.CreditCardTypeValue = DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value );
                }

                if ( paymentDetail.CurrencyTypeValueId.HasValue )
                {
                    payment.CurrencyTypeValue = DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value );
                }

                return payment;
            }
            catch ( Exception e )
            {
                MostRecentException = e;
                throw;
            }
        }

        #endregion IAutomatedGatewayComponent

        #region IHostedGatewayComponent

        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="enableACH">if set to <c>true</c> [enable ach]. (Credit Card is always enabled)</param>
        /// <param name="controlId">The control identifier.</param>
        /// <returns></returns>
        public Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId, HostedPaymentInfoControlOptions options )
        {
            PiHostedPaymentControl piHostedPaymentControl = new PiHostedPaymentControl { ID = controlId };
            piHostedPaymentControl.PiGateway = this;
            piHostedPaymentControl.GatewayBaseUrl = this.GetGatewayUrl( financialGateway );
            List<PiPaymentType> enabledPaymentTypes = new List<PiPaymentType>();
            if (options?.EnableACH ?? true)
            {
                enabledPaymentTypes.Add( PiPaymentType.ach );
            }

            if ( options?.EnableCreditCard ?? true )
            {
                enabledPaymentTypes.Add( PiPaymentType.card );
            }

            piHostedPaymentControl.EnabledPaymentTypes = enabledPaymentTypes.ToArray();

            piHostedPaymentControl.PublicApiKey = this.GetPublicApiKey( financialGateway );

            return piHostedPaymentControl;
        }

        /// <summary>
        /// Gets the paymentInfoToken that the hostedPaymentInfoControl returned (see also <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" />)
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostedPaymentInfoToken( FinancialGateway financialGateway, Control hostedPaymentInfoControl, out string errorMessage )
        {
            errorMessage = null;
            var tokenResponse = ( hostedPaymentInfoControl as PiHostedPaymentControl ).PaymentInfoTokenRaw.FromJsonOrNull<Pi.TokenizerResponse>();
            if ( tokenResponse?.IsSuccessStatus() != true )
            {
                if ( tokenResponse.HasValidationError() )
                {
                    if ( tokenResponse.Invalid.Any() )
                    {
                        errorMessage = $"Invalid {tokenResponse.Invalid.ToList().AsDelimited( "," ) }";
                        return null;
                    }
                }

                errorMessage = $"Failure: {tokenResponse?.Message ?? "null response from GetHostedPaymentInfoToken"}";
                return null;
            }
            else
            {
                return ( hostedPaymentInfoControl as PiHostedPaymentControl ).PaymentInfoToken;
            }
        }

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token
        /// Put this on your 'Next' or 'Submit' button so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl )
        {
            return $"submitTokenizer('{hostedPaymentInfoControl.ClientID}');";
        }

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Learn More' link
        /// </summary>
        /// <value>
        /// The learn more URL.
        /// </value>
        public string LearnMoreURL => "https://www.mywell.org";

        /// <summary>
        /// Gets the URL that the Gateway Information UI will navigate to when they click the 'Configure' link
        /// </summary>
        /// <value>
        /// The configure URL.
        /// </value>
        public string ConfigureURL => "https://www.mywell.org/get-started/";

        /// <summary>
        /// Creates the customer account using a token received from the HostedPaymentInfoControl <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.Boolean,System.String)" />
        /// and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentToken">The payment token.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public string CreateCustomerAccount( FinancialGateway financialGateway, string paymentToken, PaymentInfo paymentInfo, out string errorMessage )
        {
            var createCustomerResponse = this.CreateCustomer( GetGatewayUrl( financialGateway ), GetPrivateApiKey( financialGateway ), paymentToken, paymentInfo );

            if ( createCustomerResponse?.IsSuccessStatus() != true )
            {
                errorMessage = $"Failure: {createCustomerResponse?.Message ?? "null response from CreateCustomerAccount"}";
                return null;
            }
            else
            {
                errorMessage = string.Empty;
                return createCustomerResponse?.Data?.Id;
            }
        }

        /// <summary>
        /// Gets the earliest scheduled start date that the gateway will accept for the start date, based on the current local time.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public DateTime GetEarliestScheduledStartDate( FinancialGateway financialGateway )
        {
            // Pi Gateway requires that a subscription has to have a start date at least 24 after the current UTC Date
            // This sometimes will make the minimum date 2 days from now if it is already currently tomorrow in UTC (for example after 5PM Arizona Time which is offset by -7 hours from UTC)
            return DateTime.SpecifyKind( RockDateTime.Now, DateTimeKind.Local ).AddDays( 1 ).ToUniversalTime().Date;
        }

        #endregion IHostedGatewayComponent

        #region PiGateway Rock Wrappers

        #region Customers

        /// <summary>
        /// Creates the billing address.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private T CreateBillingAddress<T>( PaymentInfo paymentInfo ) where T : BillingAddress, new()
        {
            var result = new T
            {
                FirstName = paymentInfo.FirstName,
                LastName = paymentInfo.LastName,
                Company = paymentInfo.BusinessName,
                AddressLine1 = paymentInfo.Street1,
                AddressLine2 = paymentInfo.Street2,
                City = paymentInfo.City,
                State = paymentInfo.State,
                PostalCode = paymentInfo.PostalCode,
                Country = paymentInfo.Country,
                Email = paymentInfo.Email,
                Phone = paymentInfo.Phone,
            };

            // if the Gateway requires FirstName, just put '-' if no FirstName was provided
            if ( result.FirstName.IsNullOrWhiteSpace() )
            {
                result.FirstName = "-";
            }

            return result;
        }

        /// <summary>
        /// Creates the customer.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
        /// NOTE: Pi Gateway supports multiple payment tokens per customer, but Rock will implement it as one Payment Method per Customer, and 0 or more Pi Customers (one for each payment entry) per Rock Person.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenizerToken">The tokenizer token.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private CustomerResponse CreateCustomer( string gatewayUrl, string apiKey, string tokenizerToken, PaymentInfo paymentInfo )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/customer", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var createCustomer = new CreateCustomerRequest
            {
                Description = paymentInfo.FullName,
                PaymentMethod = new PaymentMethodRequest( tokenizerToken ),
                BillingAddress = CreateBillingAddress<BillingAddress>( paymentInfo )
            };

            restRequest.AddJsonBody( createCustomer );

            var response = restClient.Execute( restRequest );

            var createCustomerResponse = ParseResponse<CustomerResponse>( response );
            return createCustomerResponse;
        }

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerResponse GetCustomer( string gatewayUrl, string apiKey, string customerId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/customer/{customerId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<CustomerResponse>( response );
        }

        /// <summary>
        /// Updates the customer address.
        /// https://sandbox.gotnpgateway.com/docs/api/#update-a-specific-customer-address
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private UpdateCustomerAddressResponse UpdateCustomerAddress( string gatewayUrl, string apiKey, string customerId, PaymentInfo paymentInfo )
        {
            var customer = GetCustomer( gatewayUrl, apiKey, customerId );
            var billingAddressId = customer?.Data?.BillingAddress?.Id;

            UpdateCustomerAddressRequest updateCustomerAddressRequest = CreateBillingAddress<UpdateCustomerAddressRequest>( paymentInfo );

            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/customer/{customerId}/address/{billingAddressId}", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( updateCustomerAddressRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<UpdateCustomerAddressResponse>( response );
        }

        #endregion Customers

        #region Transactions

        /// <summary>
        /// Posts a transaction.
        /// https://sandbox.gotnpgateway.com/docs/api/#processing-a-transaction
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="type">The type (sale, authorize, credit)</param>
        /// <param name="referencedPaymentInfo">The referenced payment information.</param>
        /// <returns></returns>
        private CreateTransactionResponse PostTransaction( string gatewayUrl, string apiKey, TransactionType type, ReferencePaymentInfo referencedPaymentInfo )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            var tokenizerToken = referencedPaymentInfo.ReferenceNumber;
            var amount = referencedPaymentInfo.Amount;

            var transaction = new Rock.TransNational.Pi.CreateTransaction
            {
                Type = type,
                Amount = amount
            };

            if ( customerId.IsNotNullOrWhiteSpace() )
            {
                transaction.PaymentMethodRequest = new Rock.TransNational.Pi.PaymentMethodRequest( new Rock.TransNational.Pi.PaymentMethodCustomer( customerId ) );
            }
            else
            {
                transaction.PaymentMethodRequest = new Rock.TransNational.Pi.PaymentMethodRequest( tokenizerToken );
            }

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

            transaction.Description = stringBuilderDescription.ToString().Truncate( 255 );

            transaction.BillingAddress = CreateBillingAddress<BillingAddress>( referencedPaymentInfo );
            transaction.BillingAddress.CustomerId = customerId;

            restRequest.AddJsonBody( transaction );

            var response = restClient.Execute( restRequest );

            return ParseResponse<CreateTransactionResponse>( response );
        }

        /// <summary>
        /// Parses the response or throws an exception if the response could not be parsed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Unable to parse response: {response.Content}
        /// </exception>
        private static T ParseResponse<T>( IRestResponse response )
        {
            var result = JsonConvert.DeserializeObject<T>( response.Content );

            if ( result is BaseResponseData )
            {
                ( result as BaseResponseData ).StatusCode = response.StatusCode;
            }

            if ( result == null )
            {
                if ( response.ErrorException != null )
                {
                    throw response.ErrorException;
                }
                else if ( response.ErrorMessage.IsNotNullOrWhiteSpace() )
                {
                    throw new Exception( response.ErrorMessage );
                }
                else
                {
                    throw new Exception( $"Unable to parse response: {response.Content} " );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the transaction status.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionStatusResponse GetTransactionStatus( string gatewayUrl, string apiKey, string transactionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionStatusResponse>( response );
        }

        /// <summary>
        /// Posts the void.
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostVoid( string gatewayUrl, string apiKey, string transactionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/void", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionVoidRefundResponse>( response );
        }

        /// <summary>
        /// Posts the refund.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostRefund( string gatewayUrl, string apiKey, string transactionId, decimal amount )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/refund", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var refundRequest = new TransactionRefundRequest { Amount = amount };
            restRequest.AddJsonBody( refundRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionVoidRefundResponse>( response );
        }

        #endregion Transactions

        #region Plans

        /// <summary>
        /// Updates the billing plan BillingFrequency, BillingCycleInterval, BillingDays and Duration
        /// </summary>
        /// <param name="subscriptionRequestParameters">The subscription request parameters.</param>
        /// <param name="scheduleTransactionFrequencyValueGuid">The schedule transaction frequency value unique identifier.</param>
        /// <param name="startDate">The start date.</param>
        private static void SetSubscriptionBillingPlanParameters( SubscriptionRequestParameters subscriptionRequestParameters, Guid scheduleTransactionFrequencyValueGuid, DateTime startDate )
        {
            BillingPlanParameters billingPlanParameters = subscriptionRequestParameters as BillingPlanParameters;

            // NOTE: Don't convert startDate to UTC, let the gateway worry about that
            billingPlanParameters.NextBillDateUTC = startDate;
            BillingFrequency? billingFrequency = null;
            int billingDuration = 0;
            int billingCycleInterval = 1;
            string billingDays = null;
            int startDayOfMonth = subscriptionRequestParameters.NextBillDateUTC.Value.Day;
            int twiceMonthlySecondDayOfMonth = subscriptionRequestParameters.NextBillDateUTC.Value.AddDays( 15 ).Day;

            // PiGateway doesn't allow Day of Month over 28, but will automatically schedule for the last day of the month if you pass in 31
            if ( startDayOfMonth > 28 )
            {
                startDayOfMonth = 31;
            }

            if ( twiceMonthlySecondDayOfMonth > 28 )
            {
                twiceMonthlySecondDayOfMonth = 31;
            }

            if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY.AsGuid() )
            {
                billingFrequency = BillingFrequency.monthly;
                billingDays = $"{startDayOfMonth}";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY.AsGuid() )
            {
                // see https://sandbox.gotnpgateway.com/docs/api/#bill-once-month-on-the-1st-and-the-15th-until-canceled
                var twiceMonthlyDays = new int[2] { startDayOfMonth, twiceMonthlySecondDayOfMonth };
                billingFrequency = BillingFrequency.twice_monthly;
                // twiceMonthly Days have to be in numeric order
                billingDays = $"{twiceMonthlyDays.OrderBy( a => a ).ToList().AsDelimited( "," )}";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY.AsGuid() )
            {
                // see https://sandbox.gotnpgateway.com/docs/api/#bill-once-every-7-days-until-canceled
                billingCycleInterval = 1;
                billingFrequency = BillingFrequency.daily;
                billingDays = "7";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY.AsGuid() )
            {
                // see https://sandbox.gotnpgateway.com/docs/api/#bill-once-other-week-until-canceled
                billingCycleInterval = 2;
                billingFrequency = BillingFrequency.daily;
                billingDays = "7";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() )
            {
                // if ONE-TIME create a monthly subscription, but with a duration of 1 so that it only does it once
                billingCycleInterval = 1;
                billingFrequency = BillingFrequency.monthly;
                billingDays = $"{startDayOfMonth}";
            }

            billingPlanParameters.BillingFrequency = billingFrequency;
            billingPlanParameters.BillingCycleInterval = billingCycleInterval;
            billingPlanParameters.BillingDays = billingDays;
            billingPlanParameters.Duration = billingDuration;
        }

        /// <summary>
        /// Creates the plan.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-plan
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planParameters">The plan parameters.</param>
        /// <returns></returns>
        private CreatePlanResponse CreatePlan( string gatewayUrl, string apiKey, CreatePlanParameters planParameters )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/plan", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( planParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<CreatePlanResponse>( response );
        }

        /// <summary>
        /// Deletes the plan.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        private string DeletePlan( string gatewayUrl, string apiKey, string planId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/plan/{planId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );
            var response = restClient.Execute( restRequest );

            return response.Content;
        }

        /// <summary>
        /// Gets the plans.
        /// https://sandbox.gotnpgateway.com/docs/api/#get-all-plans
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        private GetPlansResult GetPlans( string gatewayUrl, string apiKey )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/plans", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<GetPlansResult>( response );
        }

        #endregion Plans

        #region Transaction Query

        /// <summary>
        /// Returns a list of Transactions that meet the queryTransactionStatusRequest parameters
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="queryTransactionStatusRequest">The query transaction status request.</param>
        /// <returns></returns>
        private TransactionSearchResult SearchTransactions( string gatewayUrl, string apiKey, QueryTransactionStatusRequest queryTransactionStatusRequest )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction/search", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( queryTransactionStatusRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<TransactionSearchResult>( response );
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Creates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse CreateSubscription( string gatewayUrl, string apiKey, SubscriptionRequestParameters subscriptionParameters )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/subscription", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Updates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#update-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse UpdateSubscription( string gatewayUrl, string apiKey, string subscriptionId, SubscriptionRequestParameters subscriptionParameters )
        {
            // ToDo: This currently does not work due to an issue on the PiGateway side. Waiting to see if it can be fixed.


            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Deletes the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#delete-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse DeleteSubscription( string gatewayUrl, string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.DELETE );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Gets the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#get-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        private SubscriptionResponse GetSubscription( string gatewayUrl, string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionResponse>( response );
        }

        /// <summary>
        /// Searches the subscriptions.
        /// (undocumented as of 4/15/2019) /recurring/subscription/search
        /// </summary>
        /// <param name="gatewayUrl">The gateway URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="querySubscriptionsRequest">The query subscriptions request.</param>
        /// <returns></returns>
        private SubscriptionsSearchResult SearchSubscriptions( string gatewayUrl, string apiKey, QuerySubscriptionsRequest querySubscriptionsRequest )
        {
            var restClient = new RestClient( gatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/search", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( querySubscriptionsRequest );

            var response = restClient.Execute( restRequest );

            return ParseResponse<SubscriptionsSearchResult>( response );
        }

        #endregion Subscriptions

        #endregion PiGateway Rock wrappers

        #region Exceptions

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Exception" />
        public class ReferencePaymentInfoRequired : Exception
        {
            public ReferencePaymentInfoRequired()
                : base( "PiGateway requires a token or customer reference" )
            {
            }
        }

        #endregion 

        #region GatewayComponent implementation

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
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var response = this.PostTransaction( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), TransactionType.sale, referencedPaymentInfo );
            if ( !response.IsSuccessStatus() )
            {
                errorMessage = response.Message;
                return null;
            }

            var financialTransaction = new FinancialTransaction();
            financialTransaction.TransactionCode = response.Data.Id;
            financialTransaction.FinancialPaymentDetail = PopulatePaymentInfo( paymentInfo, response.Data?.PaymentMethodResponse, response.Data?.BillingAddress );

            return financialTransaction;
        }

        /// <summary>
        /// Populates the FinancialPaymentDetail record for a FinancialTransaction or FinancialScheduledTransaction
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentMethodResponse">The payment method response.</param>
        /// <param name="billingAddressResponse">The billing address response.</param>
        /// <returns></returns>
        private static FinancialPaymentDetail PopulatePaymentInfo( PaymentInfo paymentInfo, PaymentMethodResponse paymentMethodResponse, BillingAddress billingAddressResponse )
        {
            FinancialPaymentDetail financialPaymentDetail = new FinancialPaymentDetail();
            if ( billingAddressResponse != null )
            {
                // since we are using a token for payment, it is possible that the Gateway has a different address associated with the payment method
                financialPaymentDetail.NameOnCardEncrypted = Encryption.EncryptString( $"{billingAddressResponse.FirstName} {billingAddressResponse.LastName}" );

                // if address wasn't collected when entering the transaction, set the address to the billing info returned from the gateway (if any)
                if ( paymentInfo.Street1.IsNullOrWhiteSpace() )
                {
                    if ( billingAddressResponse.AddressLine1.IsNotNullOrWhiteSpace() )
                    {
                        paymentInfo.Street1 = billingAddressResponse.AddressLine1;
                        paymentInfo.Street2 = billingAddressResponse.AddressLine2;
                        paymentInfo.City = billingAddressResponse.City;
                        paymentInfo.State = billingAddressResponse.State;
                        paymentInfo.PostalCode = billingAddressResponse.PostalCode;
                        paymentInfo.Country = billingAddressResponse.Country;
                    }
                }
            }

            var creditCardResponse = paymentMethodResponse?.Card;
            var achResponse = paymentMethodResponse?.ACH;

            if ( creditCardResponse != null )
            {
                financialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
                financialPaymentDetail.AccountNumberMasked = creditCardResponse.MaskedCard;

                if ( creditCardResponse.ExpirationDate?.Length == 5 )
                {
                    financialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( creditCardResponse.ExpirationDate.Substring( 0, 2 ) );
                    financialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( creditCardResponse.ExpirationDate.Substring( 3, 2 ) );
                }

                //// The gateway tells us what the CreditCardType is since it was selected using their hosted payment entry frame.
                //// So, first see if we can determine CreditCardTypeValueId using the CardType response from the gateway
                var creditCardTypeValue = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ) )?.GetDefinedValueFromValue( creditCardResponse.CardType );
                if ( creditCardTypeValue == null )
                {
                    // otherwise, see if we can figure it out from the MaskedCard using RegEx
                    creditCardTypeValue = CreditCardPaymentInfo.GetCreditCardType( creditCardResponse.MaskedCard );
                }

                financialPaymentDetail.CreditCardTypeValueId = creditCardTypeValue?.Id;
            }
            else if ( achResponse != null )
            {
                financialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
                financialPaymentDetail.AccountNumberMasked = achResponse.MaskedAccountNumber;
            }

            return financialPaymentDetail;
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
            if ( origTransaction == null || origTransaction.TransactionCode.IsNullOrWhiteSpace() || origTransaction.FinancialGateway == null )
            {
                errorMessage = "Invalid original transaction, transaction code, or gateway.";
                return null;
            }

            var transactionId = origTransaction.TransactionCode;
            FinancialGateway financialGateway = origTransaction.FinancialGateway;

            var transactionStatus = this.GetTransactionStatus( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), transactionId );
            var transactionStatusTransaction = transactionStatus.Data.FirstOrDefault( a => a.Id == transactionId );
            TransactionVoidRefundResponse response;
            if ( transactionStatusTransaction.IsPendingSettlement() )
            {
                // https://sandbox.gotnpgateway.com/docs/api/#void
                response = this.PostVoid( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), transactionId );
            }
            else
            {
                // https://sandbox.gotnpgateway.com/docs/api/#refund
                response = this.PostRefund( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), transactionId, origTransaction.TotalAmount );
            }

            if ( response.IsSuccessStatus() )
            {
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = "#TODO#";
                errorMessage = string.Empty;
                return transaction;
            }

            errorMessage = response.Message;
            return null;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            // create a guid to include in the Pi Subscription Description so that we can refer back to it to ensure an orphaned subscription doesn't exist when an exception occurs
            var descriptionGuid = Guid.NewGuid();

            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var customerId = referencedPaymentInfo.GatewayPersonIdentifier;
            string subscriptionDescription = $"Subscription Ref: {descriptionGuid}";

            try
            {
                errorMessage = string.Empty;

                SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
                {
                    Customer = new SubscriptionCustomer { Id = customerId },
                    PlanId = null,
                    Description = subscriptionDescription,
                    Duration = 0,
                    Amount = paymentInfo.Amount
                };

                SetSubscriptionBillingPlanParameters( subscriptionParameters, schedule.TransactionFrequencyValue.Guid, schedule.StartDate );

                var subscriptionResult = this.CreateSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionParameters );
                var subscriptionId = subscriptionResult.Data?.Id;

                if ( subscriptionId.IsNullOrWhiteSpace() )
                {
                    errorMessage = subscriptionResult.Message;
                    return null;
                }

                // set the paymentInfo.TransactionCode to the subscriptionId so that we know what CreateSubsciption created.
                // this might be handy in case we have an exception and need to know what the subscriptionId is
                referencedPaymentInfo.TransactionCode = subscriptionId;

                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.TransactionCode = customerId;
                scheduledTransaction.GatewayScheduleId = subscriptionId;
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;

                CustomerResponse customerInfo;
                try
                {
                    customerInfo = this.GetCustomer( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), customerId );
                }
                catch ( Exception ex )
                {
                    throw new Exception( $"Exception getting Customer Information for Scheduled Payment. {errorMessage}", ex );
                }

                scheduledTransaction.FinancialPaymentDetail = PopulatePaymentInfo( paymentInfo, customerInfo?.Data?.PaymentMethod, customerInfo?.Data?.BillingAddress );
                try
                {
                    GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
                }
                catch ( Exception ex )
                {
                    throw new Exception( $"Exception getting Scheduled Payment Status. {errorMessage}", ex );
                }

                return scheduledTransaction;
            }
            catch ( Exception )
            {
                // if there is an exception, Rock won't save this as a scheduled transaction, so make sure the subscription didn't get created so mystery scheduled transactions don't happen
                var subscriptionRequest = new QuerySubscriptionsRequest();
                subscriptionRequest.CustomerIdSearch = new QuerySearchString { SearchValue = customerId, ComparisonOperator = "=" };

                var subscriptionSearchResult = this.SearchSubscriptions( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionRequest );
                var orphanedSubscription = subscriptionSearchResult?.Data?.FirstOrDefault( a => a.Description == subscriptionDescription );

                if ( orphanedSubscription?.Id != null )
                {
                    var subscriptionId = orphanedSubscription.Id;
                    var subscriptionResult = this.DeleteSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
                }

                throw;
            }
        }


        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
            {
                Customer = new SubscriptionCustomer { Id = referencedPaymentInfo.GatewayPersonIdentifier },
                Duration = 0,
                Amount = referencedPaymentInfo.Amount
            };

            var transactionFrequencyGuid = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ).Guid;

            SetSubscriptionBillingPlanParameters( subscriptionParameters, transactionFrequencyGuid, scheduledTransaction.StartDate );

            FinancialGateway financialGateway = scheduledTransaction.FinancialGateway;

            var gatewayUrl = this.GetGatewayUrl( financialGateway );
            var apiKey = this.GetPrivateApiKey( financialGateway );

            var subscriptionResult = this.UpdateSubscription( gatewayUrl, apiKey, subscriptionId, subscriptionParameters );
            if ( !subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = subscriptionResult.Message;
                return false;
            }

            var updateCustomerAddressResponse = this.UpdateCustomerAddress( gatewayUrl, apiKey, scheduledTransaction.TransactionCode, referencedPaymentInfo );

            if ( !updateCustomerAddressResponse.IsSuccessStatus() )
            {
                errorMessage = updateCustomerAddressResponse.Message;
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            FinancialGateway financialGateway = scheduledTransaction.FinancialGateway;

            var subscriptionResult = this.DeleteSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                if ( subscriptionResult.StatusCode == System.Net.HttpStatusCode.NotFound || subscriptionResult.StatusCode == System.Net.HttpStatusCode.Forbidden )
                {
                    // assume as status code of Forbidden or NonFound indicates that the schedule doesn't exist, or was deleted
                    errorMessage = string.Empty;
                    return true;
                }

                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        public override bool ReactivateScheduledPaymentSupported => false;

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = "The payment gateway associated with this scheduled transaction (Pi) does not support reactivating scheduled transactions. A new scheduled transaction should be created instead.";
            return false;
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            FinancialGateway financialGateway = scheduledTransaction.FinancialGateway;
            if ( financialGateway == null && scheduledTransaction.FinancialGatewayId.HasValue )
            {
                financialGateway = new FinancialGatewayService( new Rock.Data.RockContext() ).GetNoTracking( scheduledTransaction.FinancialGatewayId.Value );
            }

            var subscriptionResult = this.GetSubscription( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                var subscriptionInfo = subscriptionResult.Data;
                if ( subscriptionInfo != null )
                {
                    scheduledTransaction.NextPaymentDate = subscriptionInfo.NextBillDateUTC.Value.ToLocalTime().Date;
                }

                scheduledTransaction.LastStatusUpdateDateTime = RockDateTime.Now;

                errorMessage = string.Empty;
                return true;
            }
            else
            {
                if ( subscriptionResult.StatusCode == System.Net.HttpStatusCode.NotFound || subscriptionResult.StatusCode == System.Net.HttpStatusCode.Forbidden )
                {
                    // assume as status code of Forbidden or NonFound indicates that the schedule doesn't exist, or was deleted
                    scheduledTransaction.IsActive = false;
                    errorMessage = string.Empty;
                    return true;
                }

                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDateTime, DateTime endDateTime, out string errorMessage )
        {
            QueryTransactionStatusRequest queryTransactionStatusRequest = new QueryTransactionStatusRequest
            {
                DateTimeRangeUTC = new QueryDateTimeRange( startDateTime, endDateTime )
            };

            var searchResult = this.SearchTransactions( this.GetGatewayUrl( financialGateway ), this.GetPrivateApiKey( financialGateway ), queryTransactionStatusRequest );

            if ( !searchResult.IsSuccessStatus() )
            {
                errorMessage = searchResult.Message;
                return null;
            }

            errorMessage = string.Empty;

            var paymentList = new List<Payment>();

            var rockContext = new RockContext();

            foreach ( var transaction in searchResult.Data )
            {
                var customerId = transaction.CustomerId;

                var gatewayScheduleId = transaction.SubscriptionId;
                var payment = new Payment
                {
                    TransactionCode = transaction.Id,
                    Amount = transaction.Amount,
                    TransactionDateTime = transaction.CreatedDateTime.Value,
                    GatewayScheduleId = gatewayScheduleId,
                };

                if (transaction.PaymentType == "ach" )
                {
                    payment.AccountNumberMasked = transaction?.PaymentMethodResponse?.ACH?.MaskedAccountNumber;
                }
                else
                {
                    payment.AccountNumberMasked = transaction?.PaymentMethodResponse?.Card?.MaskedCard;
                }

                paymentList.Add( payment );
            }

            return paymentList;
        }

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            // PI Gateway uses either a PiGateway CustomerId for this, which is stored in FinancialPersonSavedAccount.GatewayPersonIdentifier, not a previously processed transaction
            // Note: GatewayPersonIdentifer comes from CreateCustomerAccount
            errorMessage = string.Empty;
            return string.Empty;
        }

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            // we can figure out the customerId from scheduledTransaction.TransactionCode since
            // that is what our implementation of PiGateway stores customerId (see AddScheduledPayment)
            errorMessage = null;
            return scheduledTransaction.TransactionCode;
        }



        #endregion GatewayComponent implementation
    }
}
