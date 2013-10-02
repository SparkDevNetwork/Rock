using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;

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

            int newspringTransactionNumber = 1;
            
            RequestMessage request = new RequestMessage();            
            request.merchantID = GetAttributeValue( "MerchantID" );            
            request.merchantReferenceCode = newspringTransactionNumber.ToString();
            request.clientLibraryVersion = Environment.Version.ToString();
            request.clientEnvironment =
                Environment.OSVersion.Platform +
                Environment.OSVersion.Version.ToString() + "-CLR" +
                Environment.Version.ToString();


            request.ccAuthService = new CCAuthService();
            request.ccAuthService.run = "true";
            request.ccCaptureService = new CCCaptureService();
            request.ccCaptureService.run = "true";

            BillTo billTo = new BillTo();
            billTo.firstName = "Jane";
            billTo.lastName = "Smith";
            billTo.email = "null@cybersource.com";
            billTo.street1 = "1 Linwa Blvd";
            billTo.city = "Anderson";
            billTo.state = "SC";
            billTo.postalCode = "29621";
            billTo.country = "US";
            //billTo.ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            request.billTo = billTo;

            Card card = new Card();
            card.accountNumber = "4111111111111111";
            card.cardType = "Visa";
            card.expirationMonth = "12";
            card.expirationYear = "2020";
            request.card = card;

            PurchaseTotals purchaseTotals = new PurchaseTotals();
            purchaseTotals.currency = "USD";
            request.purchaseTotals = purchaseTotals;

            // foreach item purchased
            // there is one item in this sample
            request.item = new Item[1];
            Item item = new Item();
            item.id = "0";
            item.unitPrice = "29.95";
            item.totalAmount = "29.95";
            request.item[0] = item;

            // set up WCF consumption
            string transactionkey = GetAttributeValue( "TransactionKey" );

            ChannelFactory<ITransactionProcessor> channelFactory = null;
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "ITransactionProcessor";
            binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
            binding.MaxBufferSize = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;

            channelFactory = new ChannelFactory<ITransactionProcessor>( binding );
            channelFactory.Credentials.UserName.UserName = request.merchantID;
            channelFactory.Credentials.UserName.Password = transactionkey;
            EndpointAddress address = new EndpointAddress( new Uri( GatewayUrl ) );

            ITransactionProcessor test = channelFactory.CreateChannel( address );
                        
            try            
            {
                TransactionProcessorClient proxy = new TransactionProcessorClient( binding, address );
                proxy.Endpoint.Address = address;
                proxy.Endpoint.Binding = binding;
                
                
                

                ReplyMessage reply = proxy.runTransaction( request );
                //SaveOrderState();
                
                ProcessReply( reply );

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

            errorMessage = string.Empty;
            return null;
        }

        private static bool ProcessReply( ReplyMessage reply )
        {
            string template = GetTemplate( reply.decision.ToUpper() );
            string content = GetContent( reply );
            // This example writes the message to the console. Choose an appropriate display
            // method for your own application.
            Console.WriteLine( template, content );
            return false;
        }
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
        private static string EnumerateValues( string[] array )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach ( string val in array )
            {
                sb.Append( val + "\n" );
            }
            return ( sb.ToString() );
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

    }


    public static class ServiceConfig
    {
        public static BasicHttpBinding DefaultBinding
        {
            get
            {
                    var binding = new BasicHttpBinding();
                    Configure(binding);
                    return binding;
                } 
            } 
     
            public static void Configure(HttpBindingBase binding)
            {
                if (binding == null)
                {
                    throw new ArgumentException("Argument 'binding' cannot be null. Cannot configure binding.");
                }
     
                binding.SendTimeout = new TimeSpan(0, 0, 30, 0); // 30 minute timeout
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.ReaderQuotas.MaxArrayLength = 2147483647;                
                binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
                binding.ReaderQuotas.MaxDepth = 2147483647;                
                binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            }
     
            public static ServiceMetadataBehavior ServiceMetadataBehavior
            {
                get
                {
                    return new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true, 
                        MetadataExporter = {PolicyVersion = PolicyVersion.Policy15}
                    };
                }
            }
     
            public static ServiceDebugBehavior ServiceDebugBehavior
            {
                get
                {
                    var smb = new ServiceDebugBehavior();
                    Configure(smb);
                    return smb;
                }
            }
     
     
            public static void Configure(ServiceDebugBehavior behavior)
            {
                if (behavior == null)
                {
                    throw new ArgumentException("Argument 'behavior' cannot be null. Cannot configure debug behavior.");
                }
                
                behavior.IncludeExceptionDetailInFaults = true;
            }
    }
}
