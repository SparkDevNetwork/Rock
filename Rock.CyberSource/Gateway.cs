//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;

using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;

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

    [TextField( "PayPal Partner", "", true, "", "", 0, "Partner" )]
    [TextField( "PayPal Merchant Login", "", true, "", "", 1, "Vendor" )]
    [TextField( "PayPal User", "", false, "", "", 2, "User" )]
    [TextField( "PayPal Password", "", true, "", "", 3, "Password" )]
    [CustomRadioListField( "Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4 )]
    [TimeField( "Batch Process Time", "The Paypal Batch processing cut-off time.  When batches are created by Rock, they will use this for the start/stop when creating new batches", false, "00:00:00", "", 5)]

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
        /// Gets the batch time offset.
        /// </summary>
        public override TimeSpan BatchTimeOffset
        {
            get
            {
                var timeValue = new TimeSpan( 0 );
                if ( TimeSpan.TryParse( GetAttributeValue("BatchProcessTime"), out timeValue ) )
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
            Response ppResponse = null;

            var invoice = GetInvoice( paymentInfo );
            var tender = GetTender( paymentInfo );

            if ( tender != null )
            {
                if ( paymentInfo is ReferencePaymentInfo )
                {
                    var reference = paymentInfo as ReferencePaymentInfo;
                    var ppTransaction = new ReferenceTransaction( "Sale", reference.ReferenceNumber, GetUserInfo(), GetConnection(), invoice, tender, PayflowUtility.RequestId );
                    ppResponse = ppTransaction.SubmitTransaction();
                }
                else
                {
                    var ppTransaction = new SaleTransaction( GetUserInfo(), GetConnection(), invoice, tender, PayflowUtility.RequestId );
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
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            var recurring = GetReccurring( schedule );

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                recurring.OptionalTrx = "A";
            }

            var ppTransaction = new RecurringAddTransaction( GetUserInfo(), GetConnection(), GetInvoice( paymentInfo ), GetTender( paymentInfo ), 
                recurring, PayflowUtility.RequestId );

            if ( paymentInfo is ReferencePaymentInfo )
            {
                var reference = paymentInfo as ReferencePaymentInfo;
                ppTransaction.OrigId = reference.ReferenceNumber;
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
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            RecurringModifyTransaction ppTransaction = null;

            if ( paymentInfo != null )
            {
                ppTransaction = new RecurringModifyTransaction( GetUserInfo(), GetConnection(), GetReccurring( transaction ), GetInvoice( paymentInfo ), GetTender( paymentInfo ), PayflowUtility.RequestId );
            }
            else
            {
                ppTransaction = new RecurringModifyTransaction( GetUserInfo(), GetConnection(), GetReccurring( transaction ), PayflowUtility.RequestId );
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

            var ppTransaction = new RecurringCancelTransaction( GetUserInfo(), GetConnection(), GetReccurring(transaction), PayflowUtility.RequestId );
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

            var ppTransaction = new RecurringInquiryTransaction( GetUserInfo(), GetConnection(), GetReccurring( transaction ), PayflowUtility.RequestId );
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
                            transaction.NumberOfPayments = recurringResponse.Term.AsInteger( false ) ?? transaction.NumberOfPayments;
                            transaction.LastStatusUpdateDateTime = DateTime.Now;
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
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( DateTime startDate, DateTime endDate, out string errorMessage )
        {
            var reportingApi = new Reporting.Api(
                GetAttributeValue( "User" ),
                GetAttributeValue( "Vendor" ),
                GetAttributeValue( "Partner" ),
                GetAttributeValue( "Password" ) );

            var reportParams = new Dictionary<string, string>();
            reportParams.Add( "start_date", startDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );
            reportParams.Add( "end_date", endDate.ToString( "yyyy-MM-dd HH:mm:ss" ) );

            DataTable dt = reportingApi.GetReport( "RecurringBillingReport", reportParams, out errorMessage );
            if ( dt != null )
            {
                var txns = new List<Payment>();

                // The Recurring Billing Report items does not include the amounts for each transaction, so need 
                // to do a transactionIDSearch to get the amount for each transaction

                reportParams = new Dictionary<string, string>();
                reportParams.Add( "transaction_id", string.Empty );

                foreach ( DataRow row in dt.Rows )
                {
                    reportParams["transaction_id"] = row["Transaction ID"].ToString();
                    DataTable dtTxn = reportingApi.GetSearch( "TransactionIDSearch", reportParams, out errorMessage );
                    if ( dtTxn != null && dtTxn.Rows.Count == 1 )
                    {
                        var payment = new Payment();

                        decimal amount = decimal.MinValue;
                        payment.Amount = decimal.TryParse( dtTxn.Rows[0]["Amount"].ToString(), out amount ) ? (amount / 100) : 0.0M;

                        var time = DateTime.MinValue;
                        payment.TransactionDateTime = DateTime.TryParse( row["Time"].ToString(), out time ) ? time : DateTime.MinValue;

                        payment.TransactionCode = row["Transaction ID"].ToString();
                        payment.GatewayScheduleId = row["Profile ID"].ToString();
                        payment.ScheduleActive = row["Status"].ToString() == "Active";
                        txns.Add( payment );
                    }
                    else
                    {
                        errorMessage = "The TransactionIDSearch report did not return a value for transaction: " + row["Transaction ID"].ToString();
                        return null;
                    }
                }

                return txns;
            }

            errorMessage = "The RecurringBillingReport report did not return any data";
            return null;
        }

        #endregion

        #region CyberSource Object Helper Methods

        private string GatewayUrl
        {
            get
            {
                if ( GetAttributeValue( "Mode" ).Equals( "Live", StringComparison.CurrentCultureIgnoreCase ) )
                {
                    return "payflowpro.paypal.com";
                }
                else
                {
                    return "pilot-payflowpro.paypal.com";
                }
            }
        }

        private PayflowConnectionData GetConnection()
        {
            return new PayflowConnectionData( GatewayUrl );
        }

        private UserInfo GetUserInfo()
        {
            return new UserInfo(
                GetAttributeValue( "User" ),
                GetAttributeValue( "Vendor" ),
                GetAttributeValue( "Partner" ),
                GetAttributeValue( "Password" ) );
        }

        private Invoice GetInvoice( PaymentInfo paymentInfo )
        {
            var ppBillingInfo = new BillTo();
            
            ppBillingInfo.FirstName = paymentInfo.FirstName;
            ppBillingInfo.LastName = paymentInfo.LastName;
            ppBillingInfo.Email = paymentInfo.Email;
            ppBillingInfo.PhoneNum = paymentInfo.Phone;
            ppBillingInfo.Street = paymentInfo.Street;
            ppBillingInfo.State = paymentInfo.State;
            ppBillingInfo.Zip = paymentInfo.Zip;

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var cc = paymentInfo as CreditCardPaymentInfo;
                ppBillingInfo.Street = cc.BillingStreet;
                ppBillingInfo.City = cc.BillingCity;
                ppBillingInfo.State = cc.BillingState;
                ppBillingInfo.Zip = cc.BillingZip;
            }

            var ppAmount = new Currency( paymentInfo.Amount );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;
            ppInvoice.BillTo = ppBillingInfo;

            return ppInvoice;
        }

        private BaseTender GetTender( PaymentInfo paymentInfo )
        {
            if (paymentInfo is CreditCardPaymentInfo)
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

        private RecurringInfo GetReccurring( PaymentSchedule schedule )
        {
            var ppRecurringInfo = new RecurringInfo();

            ppRecurringInfo.ProfileName = schedule.PersonId.ToString();
            ppRecurringInfo.Start = schedule.StartDate.ToString( "MMddyyyy" );
            SetPayPeriod( ppRecurringInfo, schedule.TransactionFrequencyValue );

            return ppRecurringInfo;
        }

        private RecurringInfo GetReccurring( FinancialScheduledTransaction schedule )
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
            if (DateTime.TryParseExact( date, "MMddyyyy", null, System.Globalization.DateTimeStyles.None, out dt ))
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
