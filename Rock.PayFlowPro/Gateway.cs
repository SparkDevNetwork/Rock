// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Linq;
using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PayFlowPro
{
    /// <summary>
    /// PayFlowPro Payment Gateway
    /// </summary>
    [Description( "PayFlowPro Payment Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "PayFlowPro" )]

    [TextField( "PayPal Partner", "", true, "", "", 0, "Partner" )]
    [TextField( "PayPal Merchant Login", "", true, "", "", 1, "Vendor" )]
    [TextField( "PayPal User", "", false, "", "", 2, "User" )]
    [TextField( "PayPal Password", "", true, "", "", 3, "Password", true )]
    [CustomRadioListField( "Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4 )]

    public class Gateway : GatewayComponent
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
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY ) );
                values.Add( DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Returnes a boolean value indicating if 'Saved Account' functionality is supported for the given currency type.
        /// </summary>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns></returns>
        public override bool SupportsSavedAccount( DefinedValueCache currencyType )
        {
            // PayflowPro only supports saved account functionality for credit card transactions
            return currencyType.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
        }

        /// <summary>
        /// Authorizes the specified payment info.
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Authorize( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            Response ppResponse = null;

            var invoice = GetInvoice( paymentInfo );
            var tender = GetTender( paymentInfo );

            if ( tender != null )
            {
                if ( paymentInfo is ReferencePaymentInfo )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    var ppTransaction = new ReferenceTransaction( "Authorization", reference.TransactionCode, GetUserInfo( financialGateway ), GetConnection( financialGateway ), invoice, tender, PayflowUtility.RequestId );
                    ppResponse = ppTransaction.SubmitTransaction();
                }
                else
                {
                    var ppTransaction = new AuthorizationTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), invoice, tender, PayflowUtility.RequestId );
                    ppResponse = ppTransaction.SubmitTransaction();
                }
            }
            else
            {
                errorMessage = "Could not create tender from PaymentInfo";
            }

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        var transaction = new FinancialTransaction();
                        transaction.TransactionCode = txnResponse.Pnref;
                        return transaction;
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

            return null;
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            Response ppResponse = null;

            var invoice = GetInvoice( paymentInfo );
            var tender = GetTender( paymentInfo );

            if ( tender != null )
            {
                if ( paymentInfo is ReferencePaymentInfo )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    var ppTransaction = new ReferenceTransaction( "Sale", reference.TransactionCode, GetUserInfo( financialGateway ), GetConnection( financialGateway ), invoice, tender, PayflowUtility.RequestId );
                    ppResponse = ppTransaction.SubmitTransaction();
                }
                else
                {
                    var ppTransaction = new SaleTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), invoice, tender, PayflowUtility.RequestId );
                    ppResponse = ppTransaction.SubmitTransaction();
                }
            }
            else
            {
                errorMessage = "Could not create tender from PaymentInfo";
            }

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        var transaction = new FinancialTransaction();
                        transaction.TransactionCode = txnResponse.Pnref;
                        return transaction;
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

            return null;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            var recurring = GetRecurring( schedule );

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                recurring.OptionalTrx = "A";
            }

            var ppTransaction = new RecurringAddTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetInvoice( paymentInfo ), GetTender( paymentInfo ),
                recurring, PayflowUtility.RequestId );

            if ( paymentInfo is ReferencePaymentInfo )
            {
                var reference = paymentInfo as ReferencePaymentInfo;
                ppTransaction.OrigId = reference.TransactionCode;
            }
            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        RecurringResponse recurringResponse = ppResponse.RecurringResponse;

                        if ( recurringResponse != null )
                        {
                            var scheduledTransaction = new FinancialScheduledTransaction();
                            scheduledTransaction.TransactionCode = recurringResponse.TrxPNRef;
                            scheduledTransaction.GatewayScheduleId = recurringResponse.ProfileId;
                            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

                            GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
                            return scheduledTransaction;

                        }
                        else
                        {
                            errorMessage = "Invalid recurring response from the financial gateway";
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

            return null;
        }

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;

            var financialGateway = GetFinancialGateway( transaction );
            var ppTransaction = new RecurringReActivateTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetRecurring( transaction ), PayflowUtility.RequestId );

            var ppResponse = ppTransaction.SubmitTransaction();
            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        RecurringResponse recurringResponse = ppResponse.RecurringResponse;
                        if ( recurringResponse != null )
                        {
                            return true;
                        }
                        else
                        {
                            errorMessage = "Invalid recurring response from the financial gateway";
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

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
            errorMessage = string.Empty;

            RecurringModifyTransaction ppTransaction = null;
            var financialGateway = GetFinancialGateway( transaction );

            if ( paymentInfo != null )
            {
                ppTransaction = new RecurringModifyTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetRecurring( transaction ), GetInvoice( paymentInfo ), GetTender( paymentInfo ), PayflowUtility.RequestId );
            }
            else
            {
                ppTransaction = new RecurringModifyTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetRecurring( transaction ), PayflowUtility.RequestId );
            }

            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        RecurringResponse recurringResponse = ppResponse.RecurringResponse;
                        if ( recurringResponse != null )
                        {
                            return true;
                        }
                        else
                        {
                            errorMessage = "Invalid recurring response from the financial gateway";
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

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

            var financialGateway = GetFinancialGateway( transaction );
            
            var ppTransaction = new RecurringCancelTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetRecurring( transaction ), PayflowUtility.RequestId );
            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        RecurringResponse recurringResponse = ppResponse.RecurringResponse;
                        if ( recurringResponse != null )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

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

            var financialGateway = GetFinancialGateway( transaction );

            var ppTransaction = new RecurringInquiryTransaction( GetUserInfo( financialGateway ), GetConnection( financialGateway ), GetRecurring( transaction ), PayflowUtility.RequestId );
            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        RecurringResponse recurringResponse = ppResponse.RecurringResponse;
                        if ( recurringResponse != null )
                        {
                            transaction.IsActive = recurringResponse.Status.ToUpper() == "ACTIVE";
                            transaction.StartDate = GetDate( recurringResponse.Start ) ?? transaction.StartDate;
                            transaction.NextPaymentDate = GetDate( recurringResponse.NextPayment ) ?? transaction.NextPaymentDate;
                            transaction.NumberOfPayments = recurringResponse.Term.AsIntegerOrNull() ?? transaction.NumberOfPayments;
                            transaction.LastStatusUpdateDateTime = RockDateTime.Now;
                            return true;
                        }
                        return true;
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
                else
                {
                    errorMessage = "Invalid transaction response from the financial gateway";
                }
            }
            else
            {
                errorMessage = "Invalid response from the financial gateway.";
            }

            return false;
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway"></param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            var reportingApi = new Reporting.Api(
                GetAttributeValue( financialGateway, "User" ),
                GetAttributeValue( financialGateway, "Vendor" ),
                GetAttributeValue( financialGateway, "Partner" ),
                GetAttributeValue( financialGateway, "Password" ),
                GetAttributeValue( financialGateway, "Mode" ).Equals( "Test", StringComparison.CurrentCultureIgnoreCase ) );

            // Query the PayFlowPro Recurring Billing Report for transactions that were processed during data range
            var recurringBillingParams = new Dictionary<string, string>();
            recurringBillingParams.Add( "start_date", startDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );
            recurringBillingParams.Add( "end_date", endDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );
            recurringBillingParams.Add( "include_declines", "false" );
            DataTable recurringBillingTable = reportingApi.GetReport( "RecurringBillingReport", recurringBillingParams, out errorMessage );
            if ( recurringBillingTable != null )
            {
                // The Recurring Billing Report items does not include the amounts for each transaction, so need 
                // to run a custom report to try and get the amount/tender type for each transaction
                var transactionCodes = new Dictionary<string, int>();
                var customParams = new Dictionary<string, string>();
                customParams.Add( "start_date", startDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );
                customParams.Add( "end_date", endDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );
                customParams.Add( "maximum_amount", "1000000" );
                customParams.Add( "results", "Approvals Only" );
                customParams.Add( "recurring_only", "true" );
                customParams.Add( "show_order_id", "false" );
                customParams.Add( "show_transaction_id", "true" );
                customParams.Add( "show_time", "false" );
                customParams.Add( "show_type", "false" );
                customParams.Add( "show_tender_type", "true" );
                customParams.Add( "show_account_number", "false" );
                customParams.Add( "show_expires", "false" );
                customParams.Add( "show_aba_routing_number", "false" );
                customParams.Add( "show_amount", "true" );
                customParams.Add( "show_result_code", "true" );
                customParams.Add( "show_response_msg", "false" );
                customParams.Add( "show_comment1", "false" );
                customParams.Add( "show_comment2", "false" );
                customParams.Add( "show_tax_amount", "false" );
                customParams.Add( "show_purchase_order", "false" );
                customParams.Add( "show_original_transaction_id", "false" );
                customParams.Add( "show_avs_street_match", "false" );
                customParams.Add( "show_avs_zip_match", "false" );
                customParams.Add( "show_invoice_number", "false" );
                customParams.Add( "show_authcode", "false" );
                customParams.Add( "show_batch_id", "false" );
                customParams.Add( "show_csc_match", "false" );
                customParams.Add( "show_billing_first_name", "false" );
                customParams.Add( "show_billing_last_name", "false" );
                customParams.Add( "show_billing_company_name", "false" );
                customParams.Add( "show_billing_address", "false" );
                customParams.Add( "show_billing_city", "false" );
                customParams.Add( "show_billing_state", "false" );
                customParams.Add( "show_billing_zip", "false" );
                customParams.Add( "show_billing_email", "false" );
                customParams.Add( "show_billing_country", "false" );
                customParams.Add( "show_shipping_first_name", "false" );
                customParams.Add( "show_shipping_last_name", "false" );
                customParams.Add( "show_shipping_address", "false" );
                customParams.Add( "show_shipping_city", "false" );
                customParams.Add( "show_shipping_state", "false" );
                customParams.Add( "show_shipping_zip", "false" );
                customParams.Add( "show_shipping_country", "false" );
                customParams.Add( "show_customer_code", "false" );
                customParams.Add( "show_freight_amount", "false" );
                customParams.Add( "show_duty_amount", "false" );
                DataTable customTable = reportingApi.GetReport( "CustomReport", customParams, out errorMessage );
                if ( customTable != null )
                {
                    for ( int i = 0; i < customTable.Rows.Count; i++ )
                    {
                        transactionCodes.Add( customTable.Rows[i]["Transaction Id"].ToString(), i );
                    }
                }

                var txns = new List<Payment>();

                var transactionIdParams = new Dictionary<string, string>();
                transactionIdParams.Add( "transaction_id", string.Empty );

                var creditCardTypes = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid() ).DefinedValues;

                foreach ( DataRow recurringBillingRow in recurringBillingTable.Rows )
                {
                    bool foundTxn = false;
                    string transactionId = recurringBillingRow["Transaction ID"].ToString();
                    decimal amount = decimal.MinValue;
                    string tenderType = string.Empty;

                    if ( transactionCodes.ContainsKey(transactionId) )
                    {
                        int rowNumber = transactionCodes[transactionId];
                        amount = decimal.TryParse( customTable.Rows[rowNumber]["Amount"].ToString(), out amount ) ? ( amount / 100 ) : 0.0M;
                        tenderType = customTable.Rows[rowNumber]["Tender Type"].ToString();
                        foundTxn = true;
                    }
                    else
                    {
                        // If the custom report did not include the transaction, run a transactionIDSearch report to get the amount and tender type
                        transactionIdParams["transaction_id"] = transactionId;
                        DataTable transactionIdTable = reportingApi.GetSearch( "TransactionIDSearch", transactionIdParams, out errorMessage );
                        if ( transactionIdTable != null && transactionIdTable.Rows.Count == 1 )
                        {
                            amount = decimal.TryParse( transactionIdTable.Rows[0]["Amount"].ToString(), out amount ) ? ( amount / 100 ) : 0.0M;
                            tenderType = transactionIdTable.Rows[0]["Tender Type"].ToString();
                            foundTxn = true;
                        }
                    }

                    if (foundTxn)
                    { 
                        var payment = new Payment();
                        payment.Amount = amount;
                        payment.TransactionDateTime = recurringBillingRow["Time"].ToString().AsDateTime() ?? DateTime.MinValue;
                        payment.TransactionCode = recurringBillingRow["Transaction ID"].ToString();
                        payment.GatewayScheduleId = recurringBillingRow["Profile ID"].ToString();
                        payment.ScheduleActive = recurringBillingRow["Status"].ToString() == "Active";
                        payment.CreditCardTypeValue = creditCardTypes.Where( t => t.Value == tenderType ).FirstOrDefault();
                        txns.Add( payment );
                    }
                    else
                    {
                        errorMessage = "The TransactionIDSearch report did not return a value for transaction: " + recurringBillingRow["Transaction ID"].ToString();
                        return null;
                    }
                }

                return txns;
            }

            errorMessage = "The RecurringBillingReport report did not return any data";
            return null;
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

        #endregion

        #region private methods

        private FinancialGateway GetFinancialGateway( FinancialTransaction transaction )
        {
            return transaction != null ? GetFinancialGateway( transaction.FinancialGateway, transaction.FinancialGatewayId ) : null;
        }

        private FinancialGateway GetFinancialGateway( FinancialScheduledTransaction scheduledTransaction )
        {
            return scheduledTransaction != null ? GetFinancialGateway( scheduledTransaction.FinancialGateway, scheduledTransaction.FinancialGatewayId ) : null;
        }

        private FinancialGateway GetFinancialGateway( FinancialGateway financialGateway, int? financialGatewayId)
        {
            if ( financialGateway != null )
            {
                if ( financialGateway.Attributes == null )
                {
                    financialGateway.LoadAttributes();
                }
                return financialGateway;
            }

            if ( financialGatewayId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var gateway = new FinancialGatewayService( rockContext ).Get( financialGatewayId.Value );
                    gateway.LoadAttributes( rockContext );
                    return gateway;
                }
            }

            return null;
        }

        #endregion

        #region PayFlowPro Object Helper Methods

        private string GetGatewayUrl( FinancialGateway financialGateway )
        {
            if ( GetAttributeValue( financialGateway, "Mode" ).Equals( "Live", StringComparison.CurrentCultureIgnoreCase ) )
            {
                return "payflowpro.paypal.com";
            }
            else
            {
                return "pilot-payflowpro.paypal.com";
            }
        }

        private PayflowConnectionData GetConnection( FinancialGateway financialGateway )
        {
            return new PayflowConnectionData( GetGatewayUrl( financialGateway ) );
        }

        private UserInfo GetUserInfo( FinancialGateway financialGateway )
        {
            string user = GetAttributeValue( financialGateway, "User" );
            string vendor = GetAttributeValue( financialGateway, "Vendor" );
            string partner = GetAttributeValue( financialGateway, "Partner" );
            string password = GetAttributeValue( financialGateway, "Password" );

            if ( string.IsNullOrWhiteSpace( user ) )
            {
                user = vendor;
            }
            return new UserInfo( user, vendor, partner, password );
        }

        private Invoice GetInvoice( PaymentInfo paymentInfo )
        {
            var ppBillingInfo = new BillTo();

            ppBillingInfo.FirstName = paymentInfo.FirstName;
            ppBillingInfo.LastName = paymentInfo.LastName;
            ppBillingInfo.Email = paymentInfo.Email;
            ppBillingInfo.PhoneNum = paymentInfo.Phone;
            ppBillingInfo.Street = paymentInfo.Street1;
            ppBillingInfo.BillToStreet2 = paymentInfo.Street2;
            ppBillingInfo.City = paymentInfo.City;
            ppBillingInfo.State = paymentInfo.State;
            ppBillingInfo.Zip = paymentInfo.PostalCode;
            ppBillingInfo.BillToCountry = paymentInfo.Country;

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var cc = paymentInfo as CreditCardPaymentInfo;
                ppBillingInfo.Street = cc.BillingStreet1;
                ppBillingInfo.BillToStreet2 = cc.BillingStreet2;
                ppBillingInfo.City = cc.BillingCity;
                ppBillingInfo.State = cc.BillingState;
                ppBillingInfo.Zip = cc.BillingPostalCode;
                ppBillingInfo.BillToCountry = cc.BillingCountry;
            }

            var ppAmount = new Currency( paymentInfo.Amount );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;
            ppInvoice.BillTo = ppBillingInfo;
            ppInvoice.Comment1 = paymentInfo.Comment1;
            ppInvoice.Comment2 = paymentInfo.Comment2;
            ppInvoice.Desc = paymentInfo.Description;
            return ppInvoice;
        }

        private BaseTender GetTender( PaymentInfo paymentInfo )
        {
            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var cc = paymentInfo as CreditCardPaymentInfo;
                var ppCreditCard = new CreditCard( cc.Number, cc.ExpirationDate.ToString( "MMyy" ) );
                ppCreditCard.Cvv2 = cc.Code;
                return new CardTender( ppCreditCard );
            }

            if ( paymentInfo is ACHPaymentInfo )
            {
                var ach = paymentInfo as ACHPaymentInfo;
                var ppBankAccount = new BankAcct( ach.BankAccountNumber, ach.BankRoutingNumber );
                ppBankAccount.AcctType = ach.AccountType == BankAccountType.Checking ? "C" : "S";
                ppBankAccount.Name = ach.BankName;
                return new ACHTender( ppBankAccount );
            }

            if ( paymentInfo is SwipePaymentInfo )
            {
                var swipe = paymentInfo as SwipePaymentInfo;
                var ppSwipeCard = new SwipeCard( swipe.SwipeInfo );
                return new CardTender( ppSwipeCard );
            }

            if ( paymentInfo is ReferencePaymentInfo )
            {
                var reference = paymentInfo as ReferencePaymentInfo;
                if ( reference.CurrencyTypeValue.Guid.Equals( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) ) )
                {
                    return new ACHTender( (BankAcct)null );
                }
                else
                {
                    return new CardTender( (CreditCard)null );
                }
            }
            return null;
        }

        private RecurringInfo GetRecurring( PaymentSchedule schedule )
        {
            var ppRecurringInfo = new RecurringInfo();

            ppRecurringInfo.ProfileName = schedule.PersonId.ToString();
            ppRecurringInfo.Start = schedule.StartDate.ToString( "MMddyyyy" );
            SetPayPeriod( ppRecurringInfo, schedule.TransactionFrequencyValue );

            return ppRecurringInfo;
        }

        private RecurringInfo GetRecurring( FinancialScheduledTransaction schedule )
        {
            var ppRecurringInfo = new RecurringInfo();

            ppRecurringInfo.OrigProfileId = schedule.GatewayScheduleId;
            ppRecurringInfo.Start = schedule.StartDate.ToString( "MMddyyyy" );
            if ( schedule.TransactionFrequencyValueId > 0 )
            {
                SetPayPeriod( ppRecurringInfo, DefinedValueCache.Read( schedule.TransactionFrequencyValueId ) );
            }

            return ppRecurringInfo;
        }

        private void SetPayPeriod( RecurringInfo recurringInfo, DefinedValueCache transactionFrequencyValue )
        {
            recurringInfo.MaxFailPayments = 0;
            recurringInfo.Term = 0;
            var selectedFrequencyGuid = transactionFrequencyValue.Guid.ToString().ToUpper();
            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    recurringInfo.PayPeriod = "YEAR";
                    recurringInfo.Term = 1;
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    recurringInfo.PayPeriod = "WEEK";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    recurringInfo.PayPeriod = "BIWK";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY:
                    recurringInfo.PayPeriod = "SMMO";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    recurringInfo.PayPeriod = "MONT";
                    break;
            }
        }

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
