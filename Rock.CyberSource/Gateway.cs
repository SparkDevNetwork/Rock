using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CyberSource
{
    /// <summary>
    /// CyberSource Payment Gateway
    /// </summary>
    [Description( "CyberSource Payment Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "CyberSource" )]

    [TextField( "Merchant ID", "The CyberSource merchant ID (case-sensitive)", true, "", "", 0, "MerchantID" )]
    [MemoField( "Transaction Key", "The CyberSource transaction key", true, "", "", 0, "TransactionKey" )]
    [CustomRadioListField( "Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4 )]
    [TimeField( "Batch Process Time", "The Batch processing cut-off time.  When batches are created by Rock, they will use this for the start/stop when creating new batches", false, "00:00:00", "", 5 )]
    public class Gateway : GatewayComponent
    {
        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        /// <value>
        /// The gateway URL.
        /// </value>
        private string GatewayUrl
        {
            get
            {
                if ( GetAttributeValue( "Mode" ).Equals( "Live", StringComparison.CurrentCultureIgnoreCase ) )
                {
                    return "https://ics2ws.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_1.91.wsdl";
                }
                else
                {
                    return "https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_1.91.wsdl";
                }
            }
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
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY ) );
                return values;
            }
        }

        /// <summary>
        /// Gets the batch time offset.
        /// </summary>
        public override TimeSpan BatchTimeOffset
        {
            get
            {
                var timeValue = new TimeSpan( 0 );
                if ( TimeSpan.TryParse( GetAttributeValue( "BatchProcessTime" ), out timeValue ) )
                {
                    return timeValue;
                }
                return base.BatchTimeOffset;
            }
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            RequestMessage request = GetMerchantInfo();
            request.billTo = GetBillTo( paymentInfo );
            
            request.item = new Item[1];
            request.item[0] = GetItems( paymentInfo );
            request.purchaseTotals = GetTotals( paymentInfo );

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var cc = paymentInfo as CreditCardPaymentInfo;
                request.card = GetCard( cc );

                request.ccAuthService = new CCAuthService();
                request.ccAuthService.run = "true";
                request.ccAuthService.commerceIndicator = "internet";
                request.ccCaptureService = new CCCaptureService();
                request.ccCaptureService.run = "true";                         
            }            
            else if ( paymentInfo is ACHPaymentInfo )
            {
                var ach = paymentInfo as ACHPaymentInfo;
                request.check = GetCheck( ach );

                request.ecAuthenticateService = new ECAuthenticateService();
                request.ecAuthenticateService.run = "true";
                request.ecDebitService = new ECDebitService();
                request.ecDebitService.run = "true";
                request.ecDebitService.commerceIndicator = "internet";
            }
            
            else if ( paymentInfo is ReferencePaymentInfo )
            {
                var reference = paymentInfo as ReferencePaymentInfo;
                if ( reference.CurrencyTypeValue.Guid.Equals( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ) )
                {
                    //get request subscription ID ( financialscheduledtransaction.transactionCode )
                    


                }
                else if ( reference.CurrencyTypeValue.Guid.Equals( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) ) )
                {
                    var ach = paymentInfo as ACHPaymentInfo;
                    //request.check = GetCheck( ach );
                }                
            }
            else if ( paymentInfo is SwipePaymentInfo )
            {
                throw new NotImplementedException( "Point of Sale transactions not implemented." );
            }           

            ReplyMessage reply = SubmitTransaction( request );

            string template = GetTemplate( reply.decision.ToUpper() );
            string content = GetContent( reply );

            if ( "ACCEPT".Equals( reply.decision.ToUpper() ) )
            {
                var transaction = new FinancialTransaction();
                
                return transaction;
            }

            errorMessage = string.Empty;
            return null;
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="decision">The decision.</param>
        /// <returns></returns>
        private static string GetTemplate( string decision )
        {
            // Retrieves the text that corresponds to the decision.
            if ( "ACCEPT".Equals( decision ) )
            {
                return ( "The order succeeded.{0}" );
            }
            if ( "REJECT".Equals( decision ) )
            {
                return ( "Your order was not approved.{0}" );
            }

            // ERROR, or an unknown decision
            return ( "Your order could not be completed at this time.{0}" +
                    "\nPlease try again later." );
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns></returns>
        private static string GetContent( ReplyMessage reply )
        {
            /*
             * Uses the reason code to retrieve more details to add to the template.
             * The messages returned in this example are meant to demonstrate how to retrieve
             * the reply fields. Your application should display user-friendly messages.
             */
            int reasonCode = int.Parse( reply.reasonCode );
            switch ( reasonCode )
            {
                // Success
                case 100:
                    return ( "\nRequest ID: " + reply.requestID );
                // Missing field or fields
                case 101:
                    return ( "\nThe following required fields are missing: " +
                            EnumerateValues( reply.missingField ) );
                // Invalid field or fields
                case 102:
                    return ( "\nThe following fields are invalid: " +
                            EnumerateValues( reply.invalidField ) );
                // Insufficient funds
                case 204:
                    return ( "\nInsufficient funds in the account. Please use a " +
                            "different card or select another form of payment." );
                // Add additional reason codes here that you must handle more specifically.
                default:
                    // For all other reason codes, such as unrecognized reason codes or codes
                    // that do not require special handling, return an empty string.
                    return ( String.Empty );
            }
        }

        /// <summary>
        /// Enumerates the values.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        private static string EnumerateValues( string[] array )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach ( string val in array )
            {
                sb.Append( val + "\n" );
            }
            return ( sb.ToString() );
        }

        /// <summary>
        /// Submits the transaction.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private ReplyMessage SubmitTransaction( RequestMessage request )
        {
            string merchantID = GetAttributeValue( "MerchantID" );
            string transactionkey = GetAttributeValue( "TransactionKey" );
                        
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "ITransactionProcessor";
            binding.MaxBufferSize = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.ReaderQuotas.MaxDepth = 2147483647;
            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
            EndpointAddress address = new EndpointAddress( new Uri( GatewayUrl ) );

            var proxy = new TransactionProcessorClient( binding, address );
            proxy.ClientCredentials.UserName.UserName = merchantID;
            proxy.ClientCredentials.UserName.Password = transactionkey;
            proxy.Endpoint.Address = address;
            proxy.Endpoint.Binding = binding;
       
            try            
            {                
                ReplyMessage reply = proxy.runTransaction( request );
                return reply;
            }
            catch ( TimeoutException e )
            {
                //SaveOrderState();
                Console.WriteLine( "TimeoutException: " + e.Message + "\n" + e.StackTrace );
            }
            catch ( FaultException e )
            {
                //SaveOrderState();
                Console.WriteLine( "FaultException: " + e.Message + "\n" + e.StackTrace );
            }            
            catch ( WebException we )
            {
                //SaveOrderState();
                /*
                 * Some types of WebException indicate that the transaction may have been
                 * completed by CyberSource. The sample code shows how to identify these exceptions.
                 * If you receive such an exception, and your request included a payment service,
                 * you should use the CyberSource transaction search screens to determine whether
                 * the transaction was processed.
                 */
                Console.WriteLine( we.ToString() );
            }

            return null;
        }

        #region Scheduled Payments

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            RequestMessage request = GetMerchantInfo();
            request.billTo = GetBillTo( paymentInfo );
            request.purchaseTotals = GetTotals( paymentInfo );
            request.recurringSubscriptionInfo = GetRecurring( schedule, paymentInfo );

            request.paySubscriptionCreateService = new PaySubscriptionCreateService();
            request.paySubscriptionCreateService.run = "true";
            
            if ( paymentInfo is CreditCardPaymentInfo )
            { 
                // new credit card, make sure it's valid
                request.ccAuthService = new CCAuthService();
                request.ccAuthService.run = "true";
                request.ccAuthService.commerceIndicator = "internet";                
                request.subscription.paymentMethod = "credit card";
                var cc = paymentInfo as CreditCardPaymentInfo;
                request.card = GetCard( cc );
            }
            else if ( paymentInfo is ACHPaymentInfo )
            {
                var ach = paymentInfo as ACHPaymentInfo;
                request.check = GetCheck( ach );
                request.ecAuthenticateService = new ECAuthenticateService();
                request.ecAuthenticateService.run = "true";
                request.subscription.paymentMethod = "check";
            }
            else if ( paymentInfo is ReferencePaymentInfo )
            {
                var reference = paymentInfo as ReferencePaymentInfo;
                request.paySubscriptionCreateService.paymentRequestID = reference.ReferenceNumber;
                //request.paySubscriptionCreateService.paymentRequestToken = reference.ReferenceNumber;
                

            }

            errorMessage = string.Empty;
            return null;
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            return false;
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return false;
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
            return false;
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;
            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the billing details.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private BillTo GetBillTo( PaymentInfo paymentInfo )
        {            
            BillTo billingInfo = new BillTo();
            billingInfo.firstName = paymentInfo.FirstName.Left( 50 );       // up to 50 chars
            billingInfo.lastName = paymentInfo.LastName.Left( 60 );         // up to 60 chars
            billingInfo.email = paymentInfo.Email;                          // up to 255 chars
            billingInfo.phoneNumber = paymentInfo.Phone.Left( 15 );         // up to 15 chars
            billingInfo.street1 = paymentInfo.Street.Left( 50 );            // up to 50 chars
            billingInfo.city = paymentInfo.City.Left( 50 );                 // up to 50 chars
            billingInfo.state = paymentInfo.State.Left( 2 );                // only 2 chars
            billingInfo.postalCode = paymentInfo.Zip.Length > 5             
                ? Regex.Replace(paymentInfo.Zip, @"^(.{5})(.{4})$", "$1-$2")
                : paymentInfo.Zip;                                          // 9 chars with a separating -
            
            billingInfo.country = "US";                                     // only 2 chars
            billingInfo.ipAddress = Dns.GetHostEntry( Dns.GetHostName() )
                .AddressList.FirstOrDefault( ip => ip.AddressFamily == AddressFamily.InterNetwork ).ToString();

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var cc = paymentInfo as CreditCardPaymentInfo;
                billingInfo.street1 = cc.BillingStreet.Left( 50 );
                billingInfo.city = cc.BillingCity.Left( 50 );
                billingInfo.state = cc.BillingState.Left( 2 );
                billingInfo.postalCode = cc.BillingZip.Left( 10 );
            }

            return billingInfo;
        }

        /// <summary>
        /// Gets the detailed list of items.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private Item GetItems( PaymentInfo paymentInfo )
        {
            // #TODO this should really have a itemized list of charges
            // List<Item> itemList = new List<Item>();

            Item item = new Item();
            item.id = "0";
            item.unitPrice = paymentInfo.Amount.ToString();
            item.totalAmount = paymentInfo.Amount.ToString();
            return item;
        }

        /// <summary>
        /// Gets the purchase totals.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private PurchaseTotals GetTotals( PaymentInfo paymentInfo )
        {
            PurchaseTotals purchaseTotals = new PurchaseTotals();
            purchaseTotals.currency = "USD";
            return purchaseTotals;
        }

        /// <summary>
        /// Gets the card information.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private Card GetCard( CreditCardPaymentInfo cc )
        {
            var card = new Card();            
            card.accountNumber = cc.Number.AsNumeric();
            card.expirationMonth = cc.ExpirationDate.Month.ToString("D2");
            card.expirationYear = cc.ExpirationDate.Year.ToString("D4");
            card.cvNumber = cc.Code.AsNumeric();
            card.cvIndicator = "1";

            switch(cc.CreditCardTypeValue.Name)
            {
                case "Visa":
                    card.cardType = "001";
                    break;
                case "MasterCard":
                    card.cardType = "002";
                    break;
                case "American Express":
                    card.cardType = "003";
                    break;
                case "Discover":
                    card.cardType = "004";
                    break;
                case "Diners":
                    card.cardType = "005";
                    break;
                case "Carte Blanche":
                    card.cardType = "006";
                    break;
                case "JCB":
                    card.cardType = "007";
                    break;
                default:
                    card.cardType = string.Empty;
                    break;
            }

            return card;
        }

        /// <summary>
        /// Gets the check information.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private Check GetCheck( ACHPaymentInfo ach )
        {
            var check = new Check();
            check.accountNumber = ach.BankAccountNumber.AsNumeric();
            check.accountType = ach.AccountType == BankAccountType.Checking ? "C" : "S";
            check.bankTransitNumber = ach.BankRoutingNumber.AsNumeric();
            check.secCode = "WEB";
            
            return check;
        }

        /// <summary>
        /// Gets the merchant information.
        /// </summary>
        /// <returns></returns>
        private RequestMessage GetMerchantInfo()
        {
            RequestMessage request = new RequestMessage();
            
            request.merchantID = GetAttributeValue( "MerchantID" );
            request.clientLibraryVersion = Environment.Version.ToString();
            request.clientApplication = "Rock ChMS";
            request.clientApplicationVersion = Version.Current.ToString();
            request.clientApplicationUser = GetAttributeValue( "OrganizationName" );            
            request.clientEnvironment =
                Environment.OSVersion.Platform +
                Environment.OSVersion.Version.ToString() + "-CLR" +
                Environment.Version.ToString();

            request.merchantReferenceCode = "RockTransactionID";
            
            return request;
        }

        /// <summary>
        /// Gets the recurring.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        private RecurringSubscriptionInfo GetRecurring( PaymentSchedule schedule, PaymentInfo paymentInfo )
        {
            var recurringSubscriptionInfo = new RecurringSubscriptionInfo();
            recurringSubscriptionInfo.startDate = schedule.StartDate.ToString( "YYYYMMDD" );
            recurringSubscriptionInfo.amount = paymentInfo.Amount.ToString();
            recurringSubscriptionInfo.automaticRenew = "true";            
            
            SetPayPeriod( recurringSubscriptionInfo, schedule.TransactionFrequencyValue );

            return recurringSubscriptionInfo;
        }

        /// <summary>
        /// Gets the recurring.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        private RecurringSubscriptionInfo GetRecurring( FinancialScheduledTransaction schedule, PaymentInfo paymentInfo )
        {
            var recurringSubscriptionInfo = new RecurringSubscriptionInfo();
            recurringSubscriptionInfo.startDate = schedule.StartDate.ToString( "YYYYMMDD" );
            recurringSubscriptionInfo.amount = paymentInfo.Amount.ToString();
            recurringSubscriptionInfo.subscriptionID = schedule.GatewayScheduleId;
            recurringSubscriptionInfo.automaticRenew = "true";

            if ( schedule.TransactionFrequencyValueId > 0 )
            {
                SetPayPeriod( recurringSubscriptionInfo, DefinedValueCache.Read( schedule.TransactionFrequencyValueId ) );
            }

            return recurringSubscriptionInfo;
        }

        /// <summary>
        /// Sets the pay period.
        /// </summary>
        /// <param name="recurringSubscriptionInfo">The recurring subscription information.</param>
        /// <param name="transactionFrequencyValue">The transaction frequency value.</param>
        private void SetPayPeriod( RecurringSubscriptionInfo recurringSubscriptionInfo, DefinedValueCache transactionFrequencyValue )
        {
            var selectedFrequencyGuid = transactionFrequencyValue.Guid.ToString().ToUpper();
            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    recurringSubscriptionInfo.frequency = "ON-DEMAND";
                    recurringSubscriptionInfo.amount = "0";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    recurringSubscriptionInfo.frequency = "WEEKLY";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    recurringSubscriptionInfo.frequency = "BI-WEEKLY";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY:
                    recurringSubscriptionInfo.frequency = "SEMI-MONTHLY";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    recurringSubscriptionInfo.frequency = "MONTHLY";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY:
                    recurringSubscriptionInfo.frequency = "ANNUALLY";
                    break;
            }
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private DateTime? GetDate( string date )
        {
            DateTime dt = DateTime.MinValue;
            if ( DateTime.TryParseExact( date, "MMddyyyy", null, System.Globalization.DateTimeStyles.None, out dt ) )
            {
                return dt;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
