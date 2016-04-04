﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// PayFlowPro Payment Gateway
    /// </summary>
    [Description( "NMI Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "NMI Gateway" )]

    [TextField( "Security Key", "The API key", true, "", "", 0 )]
    [TextField( "Three Step API URL", "The URL of the NMI Three Step API", true, "https://secure.networkmerchants.com/api/v2/three-step", "", 1, "APIUrl" )]
    [TextField( "Query API URL", "The URL of the NMI Query API", true, "https://secure.networkmerchants.com/api/query.php", "", 3, "QueryUrl" )]
    [TextField( "Admin Username", "The username of an NMI user", true, "", "", 4 )]
    [TextField( "Admin Password", "The password of an NMI user", true, "", "", 5, "AdminPassword", true )]
    public class NMIGateway : GatewayComponent
    {

        #region Gateway Component Implementation

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
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
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
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "The NMI Gateway only supports a three-step charge.";
            return null;
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
        /// <exception cref="System.ArgumentNullException">paymentInfo</exception>
        public override string ChargeStep1( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                var rootElement = GetRoot( financialGateway, "sale" );

                rootElement.Add(
                    new XElement( "ip-address", paymentInfo.IPAddress ),
                    new XElement( "currency", "USD" ),
                    new XElement( "amount", paymentInfo.Amount.ToString() ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "tax-amount", "0.00" ),
                    new XElement( "shipping-amount", "0.00" ) );

                if ( paymentInfo is ReferencePaymentInfo )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    rootElement.Add( new XElement( "customer-vault-id", reference.TransactionCode ) );
                }

                if ( paymentInfo.AdditionalParameters != null )
                {
                    foreach ( var keyValue in paymentInfo.AdditionalParameters )
                    {
                        XElement xElement = new XElement( keyValue.Key, keyValue.Value );
                        rootElement.Add( xElement );
                    }
                }

                rootElement.Add( GetBilling( paymentInfo ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc );

                if ( result == null )
                {
                    errorMessage = "Invalid Response from NMI!";
                    return null;
                }

                if ( result.GetValueOrNull( "result" ) != "1" )
                {
                    errorMessage = result.GetValueOrNull( "result-text" );
                    return null;
                }

                return result.GetValueOrNull( "form-url" );
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
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">tokenId</exception>
        public override FinancialTransaction ChargeStep3( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null || paymentInfo.AdditionalParameters == null ||
                    !paymentInfo.AdditionalParameters.ContainsKey( "token-id" ) )
                {
                    throw new ArgumentNullException( "tokenId" );
                }

                var rootElement = GetRoot( financialGateway, "complete-action" );
                rootElement.Add( new XElement( "token-id", paymentInfo.AdditionalParameters["token-id"] ) );
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc );

                if ( result == null )
                {
                    errorMessage = "Invalid Response from NMI!";
                    return null;
                }

                if ( result.GetValueOrNull( "result" ) != "1" )
                {
                    errorMessage = result.GetValueOrNull( "result-text" );
                    return null;
                }

                var transaction = new FinancialTransaction();
                transaction.TransactionCode = result.GetValueOrNull( "transaction-id" );
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();

                string ccNumber = result.GetValueOrNull( "billing_cc-number" );
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber;

                    string mmyy = result.GetValueOrNull( "billing_cc-exp" );
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        transaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        transaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = result.GetValueOrNull( "billing_account_number" );
                }

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
        /// Credits the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction transaction, decimal amount, string comment, out string errorMessage )
        {
            errorMessage = "The NMI Gateway does not yet support Credit transactions.";
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
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "The NMI Gateway only supports adding scheduled payment using three-step process.";
            return null;
        }

        /// <summary>
        /// Performs the first step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">paymentInfo</exception>
        public override string AddScheduledPaymentStep1( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                var rootElement = GetRoot( financialGateway, "add-subscription" );

                rootElement.Add(
                    new XElement( "start-date", schedule.StartDate.ToString( "yyyyMMdd" ) ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "currency", "USD" ),
                    new XElement( "tax-amount", "0.00" ) );

                if ( paymentInfo is ReferencePaymentInfo )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    rootElement.Add( new XElement( "customer-vault-id", reference.TransactionCode ) );
                }

                if ( paymentInfo.AdditionalParameters != null )
                {
                    foreach ( var keyValue in paymentInfo.AdditionalParameters )
                    {
                        XElement xElement = new XElement( keyValue.Key, keyValue.Value );
                        rootElement.Add( xElement );
                    }
                }

                rootElement.Add( GetPlan( schedule, paymentInfo ) );

                rootElement.Add( GetBilling( paymentInfo ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc );

                if ( result == null )
                {
                    errorMessage = "Invalid Response from NMI!";
                    return null;
                }

                if ( result.GetValueOrNull( "result" ) != "1" )
                {
                    errorMessage = result.GetValueOrNull( "result-text" );
                    return null;
                }

                return result.GetValueOrNull( "form-url" );
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
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">tokenId</exception>
        public override FinancialScheduledTransaction AddScheduledPaymentStep3( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                if ( paymentInfo == null || paymentInfo.AdditionalParameters == null ||
                    !paymentInfo.AdditionalParameters.ContainsKey( "token-id" ) )
                {
                    throw new ArgumentNullException( "tokenId" );
                }

                var rootElement = GetRoot( financialGateway, "complete-action" );
                rootElement.Add( new XElement( "token-id", paymentInfo.AdditionalParameters["token-id"] ) );
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc );

                if ( result == null )
                {
                    errorMessage = "Invalid Response from NMI!";
                    return null;
                }

                if ( result.GetValueOrNull( "result" ) != "1" )
                {
                    errorMessage = result.GetValueOrNull( "result-text" );
                    return null;
                }

                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.GatewayScheduleId = result.GetValueOrNull( "subscription-id" );
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;

                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                string ccNumber = result.GetValueOrNull( "billing_cc-number" );
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber;

                    string mmyy = result.GetValueOrNull( "billing_cc-exp" );
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        scheduledTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        scheduledTransaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = result.GetValueOrNull( "billing_account_number" );
                }

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
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = "The NMI Gateway does not yet support reactivating scheduled payments.";
            return false;
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "The NMI Gateway does not yet support updating scheduled payments.";
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
            errorMessage = "The NMI Gateway does not yet support cancelling scheduled payments.";
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
            errorMessage = "The NMI Gateway does not yet support querying for the status of scheduled payments.";
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

            var txns = new List<Payment>();

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "username", GetAttributeValue( financialGateway, "AdminUsername" ) );
            queryParams.Add( "password", GetAttributeValue( financialGateway, "AdminPassword" ) );
            queryParams.Add( "start_date", startDate.ToString( "yyyyMMddHHmmss" ) );
            queryParams.Add( "end_date", endDate.ToString( "yyyyMMddHHmmss" ) );

            string url = GetAttributeValue( financialGateway, "QueryUrl" );
            string queryString = queryParams.ToList().Select( p => string.Format( "{0}={1}", p.Key, p.Value ) ).ToList().AsDelimited( "&" );

            var restClient = new RestClient( url + "?" + queryString );
            var restRequest = new RestRequest( Method.GET );

            try
            {
                var response = restClient.Execute( restRequest );
                var xdocResult = GetXmlResponse( response );
                if ( xdocResult != null )
                {
                    foreach ( var xTxn in xdocResult.Root.Elements( "transaction" ) )
                    {
                        string subscriptionId = GetXElementValue( xTxn, "original_transaction_id" ).Trim();
                        if ( !string.IsNullOrWhiteSpace( subscriptionId ) )
                        {
                            Payment payment = null;
                            var statusMessage = new StringBuilder();

                            foreach ( var xAction in xTxn.Elements( "action" ) )
                            {
                                DateTime? actionDate = ParseDateValue( GetXElementValue( xAction, "date" ) );
                                string actionType = GetXElementValue( xAction, "action_type" );
                                string responseText = GetXElementValue( xAction, "response_text" );
                                if ( actionDate.HasValue )
                                {
                                    statusMessage.AppendFormat( "{0} {1}: {2}; Status: {3}",
                                        actionDate.Value.ToShortDateString(), actionDate.Value.ToShortTimeString(),
                                        actionType.FixCase(), responseText );
                                    statusMessage.AppendLine();
                                }

                                // Don't create the payment record unless there is a sale from a subscription
                                if ( payment == null && actionType == "sale" && GetXElementValue( xAction, "source" ) == "recurring" )
                                {
                                    decimal? txnAmount = GetXElementValue( xAction, "amount" ).AsDecimalOrNull();
                                    if ( txnAmount.HasValue && actionDate.HasValue )
                                    {
                                        payment = new Payment();
                                        payment.Status = GetXElementValue( xTxn, "condition" ).FixCase();
                                        payment.IsFailure = payment.Status == "Failed";
                                        payment.StatusMessage = GetXElementValue( xTxn, "response_text" );
                                        payment.Amount = txnAmount.Value;
                                        payment.TransactionDateTime = actionDate.Value;
                                        payment.TransactionCode = GetXElementValue( xTxn, "transaction_id" );
                                        payment.GatewayScheduleId = subscriptionId;
                                        payment.ScheduleActive = true;
                                    }
                                }
                            }

                            if ( payment != null )
                            {
                                payment.StatusMessage = statusMessage.ToString();
                                txns.Add( payment );
                            }
                        }
                    }
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }

            return txns;
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
            return string.Empty;
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
            return string.Empty;
        }

        private XElement GetRoot( FinancialGateway financialGateway, string elementName )
        {
            XElement rootElement = new XElement( elementName,
                new XElement( "api-key", GetAttributeValue( financialGateway, "SecurityKey" ) )
            );

            return rootElement;
        }

        /// <summary>
        /// Creates a billing XML element
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetBilling( PaymentInfo paymentInfo )
        {
            XElement billingElement = new XElement( "billing",
                new XElement( "first-name", paymentInfo.FirstName ),
                new XElement( "last-name", paymentInfo.LastName ),
                new XElement( "address1", paymentInfo.Street1 ),
                new XElement( "address2", paymentInfo.Street2 ),
                new XElement( "city", paymentInfo.City ),
                new XElement( "state", paymentInfo.State ),
                new XElement( "postal", paymentInfo.PostalCode ),
                new XElement( "country", paymentInfo.Country ),
                new XElement( "phone", paymentInfo.Phone ),
                new XElement( "email", paymentInfo.Email )
            );

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
        private XElement GetPlan( PaymentSchedule schedule, PaymentInfo paymentInfo )
        {
            XElement planElement = new XElement( "plan",
                new XElement( "payments", schedule.NumberOfPayments.HasValue ? schedule.NumberOfPayments.Value.ToString() : "0" ),
                new XElement( "amount", paymentInfo.Amount.ToString() ) );

            var selectedFrequencyGuid = schedule.TransactionFrequencyValue.Guid.ToString().ToUpper();
            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    planElement.Add( new XElement( "months-frequency", "12" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    planElement.Add( new XElement( "day-frequency", "7" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    planElement.Add( new XElement( "day-frequency", "14" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    planElement.Add( new XElement( "months-frequency", "1" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
            }

            return planElement;
        }

        /// <summary>
        /// Posts to gateway.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private Dictionary<string, string> PostToGateway( FinancialGateway financialGateway, XDocument data )
        {
            var restClient = new RestClient( GetAttributeValue( financialGateway, "APIUrl" ) );
            var restRequest = new RestRequest( Method.POST );
            restRequest.RequestFormat = DataFormat.Xml;
            restRequest.AddBody( data );
            try
            {
                var response = restClient.Execute( restRequest );
                var xdocResult = GetXmlResponse( response );
                if ( xdocResult != null )
                {
                    // Convert XML result to a dictionary
                    var result = new Dictionary<string, string>();
                    foreach ( XElement element in xdocResult.Root.Elements() )
                    {
                        if ( element.HasElements )
                        {
                            string prefix = element.Name.LocalName;
                            foreach ( XElement childElement in element.Elements() )
                            {
                                result.Add( prefix + "_" + childElement.Name.LocalName, childElement.Value.Trim() );
                            }
                        }
                        else
                        {
                            result.Add( element.Name.LocalName, element.Value.Trim() );
                        }
                    }
                    return result;
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }

            return null;
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
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }

        private string GetXElementValue( XElement parentElement, string elementName )
        {
            var x = parentElement.Element( elementName );
            if ( x != null )
            {
                return x.Value;
            }
            return string.Empty;
        }

        private DateTime? ParseDateValue( string dateString )
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

            return DateTime.MinValue;

        }
        #endregion

    }
}
