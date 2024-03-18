using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Web.UI;
using InnerMyWellGateway = org.mywell.MyWellGateway.MyWellGateway;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using System.Web;
using Rock.Security;
using Newtonsoft.Json;
using org.mywell.MyWellGateway;
using RestSharp;

namespace Rock.MyWell
{
    /// <summary>
    /// Test Payment Gateway
    /// </summary>
    [Description( "The Test My Well Gateway is the primary gateway to use with giving & events." )]
    [DisplayName( "Test My Well Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "Test My Well Gateway" )]
    [TextField( "Private API Key", "", true, "", "", 0, null, false, null, Key = "PrivateApiKey", Description = "The private API Key used for internal operations", DefaultValue = "", IsPassword = true, Order = 1 )]
    [TextField( "Public API Key", "", true, "", "", 0, null, false, null, Key = "PublicApiKey", Description = "The public API Key used for tokenizing payment information", DefaultValue = "", IsPassword = true, Order = 2 )]
    [CustomRadioListField( "Allow ACH processing for Events", Key = "Ach", Description = "Enables option to pay with ACH for Events.", ListSource = "No,Yes", IsRequired = true, DefaultValue = "Yes" )]
    [CustomRadioListField( "Require Credit Card CVV", Key = "RequireCVV", Description = "Requires CVV for a credit card payment method for events and gifts.", ListSource = "No,Yes", IsRequired = true, DefaultValue = "Yes" )]
    [CustomRadioListField( "Verify Routing/Account Numbers", Key = "VerifyACH", Description = "This will prompt users to retype routing/account number. A warning will be displayed if the routing/account number from both fields do not match.", ListSource = "No,Yes", IsRequired = true, DefaultValue = "No" )]
    [CustomRadioListField( "Enable Apple Pay", Key = "ApplePay", Description = "Before enabling this, Apple Pay needs to be setup by following the directions under Installed Plugins -> My Well Gateway -> Apple Pay.", ListSource = "No,Yes", IsRequired = true, DefaultValue = "No" )]
    [CustomRadioListField( "Mode", Key = "Mode", Description = "Set to Sandbox mode to use the sandbox test gateway instead of the production gateway", ListSource = "Live,Sandbox", IsRequired = true, DefaultValue = "Live" )]
    [TextField( "Sandbox URL", "", true, "", "", 0, null, false, null, Key = "SandboxURL", Description = "This is the sandbox url", IsRequired = false )]
    [DecimalField( "Credit Card Fee Coverage Percentage", "", true, double.MinValue, "", 0, null, Key = "CreditCardFeeCoveragePercentage", Description = "The credit card fee percentage that will be used to determine what to add to the person's donation, if they want to cover the fee.", IsRequired = false, DefaultValue = null, Order = 9 )]
    [CurrencyField( "ACH Transaction Fee Coverage Amount", "", true, double.MinValue, "", 0, null, Key = "ACHTransactionFeeCoverageAmount", Description = "The  dollar amount to add to an ACH transaction, if they want to cover the fee.", IsRequired = false, DefaultValue = null, Order = 10 )]
    [TextField( "Batch Name Prefix", "", true, "", "", 0, null, false, null, Key = "BatchNamePrefix", Description = "The batch prefix name to use when creating new batches. The Currency Type abbreviation and the batch settled date is appended to the batch prefix automatically.", IsRequired = false, DefaultValue = "My Well", Order = 11 )]
    [CustomDropdownListField( "Open Unsettled Batches", Key = "BatchUnsettledTransactions", Description = "Transations that are not settled are moved from the My Well Pending Transaction batch to an unsettled batch. How often do you want these batches created? This batch will be in a pending/automated status until this value criteria is met which will set the batch status to open.", ListSource = "Weekly,Daily", IsRequired = true, Order = 12, DefaultValue = "Weekly" )]
    [TextField( "My Well Gateway Portal URL", "", true, "", "", 0, null, false, null, Key = "MyWellGatewayPortalURL", Description = "The My Well Gateway Portal URL that is given to the organization after onboarding. Do not include any path after 'gateway-admin' in the URL. Example: https://shortname.mywell.org/gateway-admin", IsRequired = true, DefaultValue = "https://shortname.mywell.org/gateway-admin", Order = 13 )]
    [Rock.SystemGuid.EntityTypeGuid( "98D4DA6E-9AB9-422F-8995-548E6392DB8A" )]
    public class TestMyWellGateway : GatewayComponent, IHostedGatewayComponent, IGatewayComponent, IAutomatedGatewayComponent, IObsidianHostedGatewayComponent, IPaymentTokenGateway, IFeeCoverageGatewayComponent, ISettlementGateway, IScheduledNumberOfPaymentsGateway
    {
        private readonly Lazy<InnerMyWellGateway> _myWellGateway = new Lazy<InnerMyWellGateway>(() =>
        {
            return ( InnerMyWellGateway ) GatewayContainer.GetComponent( "org.mywell.MyWellGateway.MyWellGateway" );
        } );

        private InnerMyWellGateway MyWellGateway => _myWellGateway.Value;
        
        #region Overrides

        /// <inheritdoc/>
        public override List<DefinedValueCache> SupportedPaymentSchedules => MyWellGateway.SupportedPaymentSchedules;

        /// <inheritdoc/>
        public override Dictionary<string, string> AttributeValueDefaults => MyWellGateway.AttributeValueDefaults;

        /// <inheritdoc/>
        public override FinancialTransaction Authorize( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            return MyWellGateway.Authorize( financialGateway, paymentInfo, out errorMessage );
        }

        /// <inheritdoc/>
        public override string Description => MyWellGateway.Description;

        /// <inheritdoc/>
        public override bool Equals( object obj )
        {
            return MyWellGateway.Equals( obj );
        }

        /// <inheritdoc/>
        public override string GetAttributeValue( string key )
        {
            return MyWellGateway.GetAttributeValue( key );
        }

        /// <inheritdoc/>
        public override List<string> GetAttributeValues( string key )
        {
            return MyWellGateway.GetAttributeValues( key );
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return MyWellGateway.GetHashCode();
        }

        /// <inheritdoc/>
        public override DateTime? GetNextPaymentDate( FinancialScheduledTransaction scheduledTransaction, DateTime? lastTransactionDate )
        {
            return MyWellGateway.GetNextPaymentDate( scheduledTransaction, lastTransactionDate );
        }

        /// <inheritdoc/>
        public override bool GetScheduledPaymentStatusSupported => MyWellGateway.GetScheduledPaymentStatusSupported;

        /// <inheritdoc/>
        public override void InitializeAttributeValues( HttpRequest request, string rootUrl )
        {
            MyWellGateway.InitializeAttributeValues( request, rootUrl );
        }

        /// <inheritdoc/>
        public override bool IsActive => MyWellGateway.IsActive;

        /// <inheritdoc/>
        public override bool IsCurrencyCodeSupported( DefinedValueCache currencyCode, DefinedValueCache creditCardType )
        {
            return MyWellGateway.IsCurrencyCodeSupported( currencyCode, creditCardType );
        }

        /// <inheritdoc/>
        public override bool IsUpdatingSchedulePaymentMethodSupported => MyWellGateway.IsUpdatingSchedulePaymentMethodSupported;

        /// <inheritdoc/>
        // This causes issues when trying to access the other component before it is resolved.
        //public override int Order => MyWellGateway.Order;

        /// <inheritdoc/>
        public override ISecured ParentAuthorityPre => MyWellGateway.ParentAuthorityPre;

        /// <inheritdoc/>
        public override bool PromptForBankAccountName( FinancialGateway financialGateway )
        {
            return MyWellGateway.PromptForBankAccountName( financialGateway );
        }

        /// <inheritdoc/>
        public override bool PromptForBillingAddress( FinancialGateway financialGateway )
        {
            return MyWellGateway.PromptForBillingAddress( financialGateway );
        }

        /// <inheritdoc/>
        public override bool PromptForNameOnCard( FinancialGateway financialGateway )
        {
            return MyWellGateway.PromptForNameOnCard( financialGateway );
        }

        /// <inheritdoc/>
        public override bool ReactivateScheduledPaymentSupported => MyWellGateway.ReactivateScheduledPaymentSupported;

        /// <inheritdoc/>
        public override bool SplitNameOnCard => MyWellGateway.SplitNameOnCard;

        /// <inheritdoc/>
        public override Dictionary<string, string> SupportedActions => MyWellGateway.SupportedActions;

        /// <inheritdoc/>
        public override bool SupportsRockInitiatedTransactions => MyWellGateway.SupportsRockInitiatedTransactions;

        /// <inheritdoc/>
        public override bool SupportsSavedAccount( bool isRepeating )
        {
            return MyWellGateway.SupportsSavedAccount( isRepeating );
        }

        /// <inheritdoc/>
        public override bool SupportsSavedAccount( DefinedValueCache currencyType )
        {
            return MyWellGateway.SupportsSavedAccount( currencyType );
        }

        /// <inheritdoc/>
        public override bool SupportsScheduleCurrencyChange => MyWellGateway.SupportsScheduleCurrencyChange;

        /// <inheritdoc/>
        public override bool SupportsStandardRockPaymentEntryForm => MyWellGateway.SupportsStandardRockPaymentEntryForm;

        /// <inheritdoc/>
        public override string ToString()
        {
            return MyWellGateway.ToString();
        }

        /// <inheritdoc/>
        public override bool UpdateScheduledPaymentSupported => MyWellGateway.UpdateScheduledPaymentSupported;

        /// <inheritdoc/>
        public override bool ValidateAttributeValues( out string errorMessage )
        {
            return MyWellGateway.ValidateAttributeValues( out errorMessage );
        }

        #endregion

        #region Implementation

        /// <inheritdoc/>
        public string ConfigureURL => MyWellGateway.ConfigureURL;

        /// <inheritdoc/>
        public string LearnMoreURL => MyWellGateway.LearnMoreURL;

        /// <inheritdoc/>
        public Exception MostRecentException => MyWellGateway.MostRecentException;
        
        /// <inheritdoc/>
        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            //return MyWellGateway.CreateCustomerAccount( financialGateway, paymentInfo, out errorMessage );
            
            org.mywell.MyWellGateway.FluidPayResponse fluidPayResponse = paymentInfo.ReferenceNumber.FromJsonOrNull<org.mywell.MyWellGateway.FluidPayResponse>();
            org.mywell.MyWellGateway.Transaction transaction = ( ( ( PaymentInfo ) paymentInfo ).TransactionTypeValueId.HasValue ? GetTransactionType( ( ( PaymentInfo ) paymentInfo ).TransactionTypeValueId ) : org.mywell.MyWellGateway.Transaction.DONATION );
            if ( transaction == org.mywell.MyWellGateway.Transaction.EVENT && fluidPayResponse.BillingAddress != null )
            {
                if ( string.IsNullOrEmpty( fluidPayResponse.BillingAddress.Address ) || string.IsNullOrEmpty( fluidPayResponse.BillingAddress.City ) || string.IsNullOrEmpty( fluidPayResponse.BillingAddress.Zip ) || string.IsNullOrEmpty( fluidPayResponse.BillingAddress.State ) || string.IsNullOrEmpty( fluidPayResponse.BillingAddress.Country ) )
                {
                    errorMessage = "Address is required.";
                    return null;
                }

                ( ( PaymentInfo ) paymentInfo ).Street1 = fluidPayResponse.BillingAddress.Address;
                ( ( PaymentInfo ) paymentInfo ).City = fluidPayResponse.BillingAddress.City;
                ( ( PaymentInfo ) paymentInfo ).State = fluidPayResponse.BillingAddress.State;
                ( ( PaymentInfo ) paymentInfo ).PostalCode = fluidPayResponse.BillingAddress.Zip;
                ( ( PaymentInfo ) paymentInfo ).Country = fluidPayResponse.BillingAddress.Country;
            }

            if ( !isAddressValid( paymentInfo ) )
            {
                errorMessage = "Address is required.";
                return null;
            }

            if ( fluidPayResponse.DisplayName.IsNotNullOrWhiteSpace() )
            {
                ( ( PaymentInfo ) paymentInfo ).AdditionalParameters.Add( "displayName", fluidPayResponse.DisplayName );
            }

            if ( fluidPayResponse.CurrencyType.HasValue )
            {
                ( ( PaymentInfo ) paymentInfo ).AdditionalParameters.Add( "currencyType", fluidPayResponse.CurrencyType.ToString() );
            }

            paymentInfo.ReferenceNumber = fluidPayResponse.Token;
            org.mywell.MyWellGateway.CustomerResponse customerResponse = CreateCustomer( GetGatewayUrl( financialGateway ), GetPrivateApiKey( financialGateway ), paymentInfo );
            if ( customerResponse?.Message != "" )
            {
                errorMessage = customerResponse?.Message ?? "null response from CreateCustomerAccount";
                return null;
            }

            if ( !string.IsNullOrWhiteSpace( customerResponse.Error ) )
            {
                errorMessage = "There was an error creating customer: " + customerResponse?.Error;
                return null;
            }

            if ( customerResponse.PaymentMethod != null )
            {
                errorMessage = string.Empty;
                var value = ( ( customerResponse.PaymentMethod.Card == null ) ? customerResponse.PaymentMethod.Ach.Id : customerResponse.PaymentMethod.Card.Id );
                ( ( PaymentInfo ) paymentInfo ).AdditionalParameters.Add( "paymentMethodId", value );
            }

            errorMessage = string.Empty;
            return customerResponse?.Id;
        }

        public Transaction GetTransactionType( int? transactionTypeValueId )
        {
            if ( transactionTypeValueId.HasValue )
            {
                var val = DefinedValueCache.Get( transactionTypeValueId.GetValueOrDefault() );
                var attributeValue = val.GetAttributeValue( "MyWellTransactionType" );
                if ( attributeValue.IsNotNullOrWhiteSpace() && attributeValue == org.mywell.MyWellGateway.Transaction.EVENT.ConvertToInt().ToString() )
                {
                    return org.mywell.MyWellGateway.Transaction.EVENT;
                }
            }

            return org.mywell.MyWellGateway.Transaction.DONATION;
        }

        public bool isAddressValid( ReferencePaymentInfo paymentInfo )
        {
            if ( ( ( PaymentInfo ) paymentInfo ).Country == "US" )
            {
                if ( string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).Street1 ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).City ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).PostalCode ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).State ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).Country ) )
                {
                    return false;
                }
            }
            else if ( string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).Street1 ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).City ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).State ) || string.IsNullOrEmpty( ( ( PaymentInfo ) paymentInfo ).Country ) )
            {
                return false;
            }

            return true;
        }

        private org.mywell.MyWellGateway.CustomerResponse CreateCustomer( string gatewayUrl, string apiKey, ReferencePaymentInfo paymentInfo )
        {
            var restClient = new RestClient( gatewayUrl );
            RestSharp.Newtonsoft.Json.RestRequest restRequest = new RestSharp.Newtonsoft.Json.RestRequest( "api/customer", Method.POST );
            restRequest.AddHeader( "apiToken", apiKey );
            var billingAddress = new org.mywell.MyWellGateway.CreateBillingAddressData
            {
                FirstName = ( ( PaymentInfo ) paymentInfo ).FirstName,
                LastName = ( ( PaymentInfo ) paymentInfo ).LastName,
                Street1 = ( ( PaymentInfo ) paymentInfo ).Street1,
                Street2 = ( ( PaymentInfo ) paymentInfo ).Street2,
                City = ( ( PaymentInfo ) paymentInfo ).City,
                State = ( ( PaymentInfo ) paymentInfo ).State,
                Country = ( ( PaymentInfo ) paymentInfo ).Country,
                PostalCode = ( ( ( ( PaymentInfo ) paymentInfo ).PostalCode != null ) ? ( ( PaymentInfo ) paymentInfo ).PostalCode : "" ),
                Phone = ( ( ( ( PaymentInfo ) paymentInfo ).Phone != null ) ? ( ( PaymentInfo ) paymentInfo ).Phone : "" ),
                Email = ( ( PaymentInfo ) paymentInfo ).Email
            };
            var shippingAddress = new org.mywell.MyWellGateway.CreateShippingAddressData
            {
                FirstName = ( ( PaymentInfo ) paymentInfo ).FirstName,
                LastName = ( ( PaymentInfo ) paymentInfo ).LastName,
                Street1 = ( ( PaymentInfo ) paymentInfo ).Street1,
                Street2 = ( ( PaymentInfo ) paymentInfo ).Street2,
                City = ( ( PaymentInfo ) paymentInfo ).City,
                State = ( ( PaymentInfo ) paymentInfo ).State,
                Country = ( ( PaymentInfo ) paymentInfo ).Country,
                PostalCode = ( ( ( ( PaymentInfo ) paymentInfo ).PostalCode != null ) ? ( ( PaymentInfo ) paymentInfo ).PostalCode : "" ),
                Phone = ( ( ( ( PaymentInfo ) paymentInfo ).Phone != null ) ? ( ( PaymentInfo ) paymentInfo ).Phone : "" ),
                Email = ( ( PaymentInfo ) paymentInfo ).Email
            };
            var paymentMethod = new org.mywell.MyWellGateway.CreatePaymentMethod
            {
                CurrencyType = ( ( ( PaymentInfo ) paymentInfo ).AdditionalParameters.ContainsKey( "currencyType" ) ? ( ( PaymentInfo ) paymentInfo ).AdditionalParameters["currencyType"] : null ),
                DisplayName = ( ( ( PaymentInfo ) paymentInfo ).AdditionalParameters.ContainsKey( "displayName" ) ? ( ( PaymentInfo ) paymentInfo ).AdditionalParameters["displayName"] : null ),
                Token = paymentInfo.ReferenceNumber
            };
            var obj = new org.mywell.MyWellGateway.CreateCustomerRequestData
            {
                Kind = GetTransactionType( ( ( PaymentInfo ) paymentInfo ).TransactionTypeValueId ),
                Description = ( ( PaymentInfo ) paymentInfo ).FullName,
                PaymentMethod = paymentMethod,
                BillingAddress = billingAddress,
                ShippingAddress = shippingAddress
            };
            restRequest.AddJsonBody( obj );
            var response = restClient.Execute( restRequest );
            return response.Content.FromJsonOrNull<org.mywell.MyWellGateway.CustomerResponse>();
        }

        public string GetGatewayUrl( FinancialGateway financialGateway )
        {
            if ( ( ( GatewayComponent ) this ).GetAttributeValue( financialGateway, "Mode" ).Equals( "Sandbox" ) )
            {
                return ( ( GatewayComponent ) this ).GetAttributeValue( financialGateway, "SandboxURL" );
            }

            return "https://api.mywell.org/";
        }
        private string GetPrivateApiKey( FinancialGateway financialGateway )
        {
            return ( ( GatewayComponent ) this ).GetAttributeValue( financialGateway, "PrivateApiKey" );
        }

        /// <inheritdoc/>
        public DateTime GetEarliestScheduledStartDate( FinancialGateway financialGateway ) => MyWellGateway.GetEarliestScheduledStartDate( financialGateway );

        /// <inheritdoc/>
        public Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId, HostedPaymentInfoControlOptions options ) => MyWellGateway.GetHostedPaymentInfoControl( financialGateway, controlId, options );

        /// <inheritdoc/>
        public string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl ) => MyWellGateway.GetHostPaymentInfoSubmitScript( financialGateway, hostedPaymentInfoControl );

        /// <inheritdoc/>
        public HostedGatewayMode[] GetSupportedHostedGatewayModes( FinancialGateway financialGateway ) => MyWellGateway.GetSupportedHostedGatewayModes( financialGateway );

        /// <inheritdoc/>
        public void UpdatePaymentInfoFromPaymentControl( FinancialGateway financialGateway, Control hostedPaymentInfoControl, ReferencePaymentInfo referencePaymentInfo, out string errorMessage ) => MyWellGateway.UpdatePaymentInfoFromPaymentControl( financialGateway, hostedPaymentInfoControl, referencePaymentInfo, out errorMessage );

        /// <inheritdoc/>
        public Payment AutomatedCharge( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage, Dictionary<string, string> metadata = null ) => MyWellGateway.AutomatedCharge( financialGateway, paymentInfo, out errorMessage, metadata );

        /// <inheritdoc/>
        public string GetObsidianControlFileUrl( FinancialGateway financialGateway ) => MyWellGateway.GetObsidianControlFileUrl( financialGateway );

        /// <inheritdoc/>
        public object GetObsidianControlSettings( FinancialGateway financialGateway, HostedPaymentInfoControlOptions options ) => MyWellGateway.GetObsidianControlSettings( financialGateway, options );

        /// <inheritdoc/>
        public bool TryGetPaymentTokenFromParameters( FinancialGateway financialGateway, IDictionary<string, string> parameters, out string paymentToken ) => MyWellGateway.TryGetPaymentTokenFromParameters( financialGateway, parameters, out paymentToken );

        /// <inheritdoc/>
        public bool IsPaymentTokenCharged( FinancialGateway financialGateway, string paymentToken ) => MyWellGateway.IsPaymentTokenCharged( financialGateway, paymentToken );

        /// <inheritdoc/>
        public FinancialTransaction FetchPaymentTokenTransaction( RockContext rockContext, FinancialGateway financialGateway, int? fundId, string paymentToken ) => MyWellGateway.FetchPaymentTokenTransaction( rockContext, financialGateway, fundId, paymentToken );

        /// <inheritdoc/>
        public decimal? GetCreditCardFeeCoveragePercentage( FinancialGateway financialGateway ) => MyWellGateway.GetCreditCardFeeCoveragePercentage( financialGateway );

        /// <inheritdoc/>
        public decimal? GetACHFeeCoverageAmount( FinancialGateway financialGateway ) => MyWellGateway.GetACHFeeCoverageAmount( financialGateway );

        /// <inheritdoc/>
        public int? GetSettlementBatchId( FinancialGateway financialGateway, FinancialTransaction financialTransaction ) => MyWellGateway.GetSettlementBatchId( financialGateway, financialTransaction );

        /// <inheritdoc/>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage ) => MyWellGateway.Charge( financialGateway, paymentInfo, out errorMessage );

        /// <inheritdoc/>
        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage ) => MyWellGateway.Credit( origTransaction, amount, comment, out errorMessage );

        /// <inheritdoc/>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage ) => MyWellGateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );

        /// <inheritdoc/>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage ) => MyWellGateway.UpdateScheduledPayment( transaction, paymentInfo, out errorMessage );

        /// <inheritdoc/>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage ) => MyWellGateway.CancelScheduledPayment( transaction, out errorMessage );

        /// <inheritdoc/>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage ) => MyWellGateway.ReactivateScheduledPayment( transaction, out errorMessage );

        /// <inheritdoc/>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage ) => MyWellGateway.GetScheduledPaymentStatus( transaction, out errorMessage );

        /// <inheritdoc/>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage ) => MyWellGateway.GetPayments( financialGateway, startDate, endDate, out errorMessage );

        /// <inheritdoc/>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage ) => MyWellGateway.GetReferenceNumber( transaction, out errorMessage );

        /// <inheritdoc/>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage ) => MyWellGateway.GetReferenceNumber( scheduledTransaction, out errorMessage );

        #endregion
    }
}