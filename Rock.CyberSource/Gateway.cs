using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Web.Services.Protocols;

using CyberSource.Clients;
using CyberSource.Clients.SoapWebReference;

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

    [TextField( "CyberSource Merchant ID", "The CyberSource Merchant ID", true, "", "", 0, "MerchantID" )]
    [CustomRadioListField( "Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4 )]
    [TimeField( "Batch Process Time", "The Batch processing cut-off time.  When batches are created by Rock, they will use this for the start/stop when creating new batches", false, "00:00:00", "", 5 )]
    public class Gateway : GatewayComponent
    {
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
            RequestMessage request = new RequestMessage();

            int newspringTransactionNumber = 1;
            request.merchantID = "newspringcc";
            request.merchantReferenceCode = newspringTransactionNumber.ToString();
            request.ccAuthService = new CCAuthService();
            request.ccAuthService.run = "true";
            request.ccCaptureService = new CCCaptureService();
            request.ccCaptureService.run = "true";

            BillTo billTo = new BillTo();
            billTo.firstName = "Jane";
            billTo.lastName = "Smith";
            billTo.email = "david.stevens@newspring.cc";
            billTo.street1 = "1 Linwa Blvd";
            billTo.city = "Anderson";
            billTo.state = "SC";
            billTo.postalCode = "29621";
            billTo.country = "USA";
            request.billTo = billTo;

            Card card = new Card();
            card.accountNumber = "4111111111111111";
            card.cardType = "Visa";
            card.expirationMonth = "8";
            card.expirationYear = "2015";
            request.card = card;
            
            // there is one item in this sample
            request.item = new Item[1];
            Item item = new Item();
            item.id = "0";
            item.unitPrice = "29.95";
            item.totalAmount = "29.95";
            request.item[0] = item;
            request.purchaseTotals.currency = "USD";

            try
            {
                ReplyMessage reply = SoapClient.RunTransaction( request );
                //SaveOrderState();
                // Using the Decision and Reason Code describes the ProcessReply
                // method.
                ProcessReply( reply );
            }
            catch ( SignException se )
            {
                //SaveOrderState();
                Console.WriteLine( se.ToString() );
            }
            catch ( SoapHeaderException she )
            {
                //SaveOrderState();
                Console.WriteLine( she.ToString() );
            }
            catch ( SoapBodyException sbe )
            {
                //SaveOrderState();
                /*
                 * Some types of SoapBodyException indicate that the transaction may have been
                 * completed by CyberSource. The sample code shows how to identify these exceptions.
                 * If you receive such an exception, and your request included a payment service,
                 * you should use the CyberSource transaction search screens to determine whether
                 * the transaction was processed.
                 */
                Console.WriteLine( sbe.ToString() );
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



    }
}


        #endregion
