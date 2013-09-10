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
    [TextField( "PayPal Password", "", true, "", "", 3, "Password" )]
    [CustomRadioListField( "Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4 )]

    public class Gateway : GatewayComponent
    {
        #region Gateway Configuration Properties

        /// <summary>
        /// Gets the supported frequency values.
        /// </summary>
        /// <value>
        /// The supported frequency values.
        /// </value>
        public override List<DefinedValueCache> SupportedFrequencyValues
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

        #endregion

        /// <summary>
        /// Initiates a credit-card sale transaction.  If succesful, the TransactionCode is updated
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool Charge( FinancialTransaction transaction, Rock.Financial.CreditCard creditCard, out string errorMessage )
        {
            return MakeSale( transaction, GetInvoice( creditCard ), GetTender( creditCard ), out errorMessage );
        }

        /// <summary>
        /// Initiates an ach sale transaction. If succesful, the TransactionCode is updated
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool Charge( FinancialTransaction transaction, BankAccount bankAccount, out string errorMessage )
        {
            return MakeSale( transaction, GetInvoice( bankAccount ), GetTender( bankAccount ), out errorMessage );
        }

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CreateScheduledTransaction( FinancialScheduledTransaction scheduledTransaction, Rock.Financial.CreditCard creditCard, out string errorMessage )
        {
            var recurringInfo = GetReccurring( scheduledTransaction );
            return CreateRecurring( scheduledTransaction, recurringInfo, GetInvoice( creditCard ), GetTender( creditCard ), out errorMessage );
        }

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CreateScheduledTransaction( FinancialScheduledTransaction scheduledTransaction, BankAccount bankAccount, out string errorMessage )
        {
            var recurringInfo = GetReccurring( scheduledTransaction );
            return CreateRecurring( scheduledTransaction, recurringInfo, GetInvoice( bankAccount ), GetTender( bankAccount ), out errorMessage );
        }

        public override Model.FinancialScheduledTransaction UpdateScheduledTransaction( Model.FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool CancelScheduledTransaction( Model.FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override void ProcessPostBack( System.Web.HttpRequest request )
        {
            throw new NotImplementedException();
        }

        public override DataTable DownloadNewTransactions( DateTime startDate, DateTime endDate, out string errorMessage )
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
                var txns = new List<FinancialTransaction>();

                // The Recurring Billing Report items does not include the amounts for each transaction, so need 
                // to do a transactionIDSearch to get the amount for each transaction

                reportParams = new Dictionary<string, string>();
                reportParams.Add( "transaction_id", string.Empty );

                dt.Columns.Add( "Amount" );
                foreach ( DataRow row in dt.Rows )
                {
                    reportParams["transaction_id"] = row["Transaction ID"].ToString();
                    DataTable dtTxn = reportingApi.GetSearch( "TransactionIDSearch", reportParams, out errorMessage );
                    if ( dtTxn != null && dtTxn.Rows.Count == 1 )
                    {
                        decimal amount = decimal.MinValue;
                        if ( decimal.TryParse( dtTxn.Rows[0]["Amount"].ToString(), out amount ) )
                        {
                            var txn = new FinancialTransaction();
                            txn.Amount = amount;
                            txn.TransactionCode = row["Transaction ID"].ToString();
                            txns.Add( txn );
                        }

                        row["Amount"] = dtTxn.Rows[0]["Amount"];
                    }
                    else
                    {
                        return null;
                    }
                }

                return dt;
            }

            return null;
        }

        #region PayFlowPro Helper Methods

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

        private Invoice GetInvoice( Rock.Financial.CreditCard creditCard )
        {
            var ppAmount = new Currency( creditCard.Amount );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;

            return ppInvoice;
        }

        private Invoice GetInvoice( BankAccount bankAccount )
        {
            var ppAmount = new Currency( bankAccount.Amount );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;

            return ppInvoice;
        }

        private BaseTender GetTender( Rock.Financial.CreditCard creditCard )
        {
            var ppBillingInfo = new BillTo();
            ppBillingInfo.Street = creditCard.BillingStreet;
            ppBillingInfo.City = creditCard.BillingCity;
            ppBillingInfo.State = creditCard.BillingState;
            ppBillingInfo.Zip = creditCard.BillingZip;

            var ppAmount = new Currency( creditCard.Amount );

            var ppCreditCard = new PayPal.Payments.DataObjects.CreditCard( creditCard.Number, creditCard.ExpirationDate.ToString( "MMyy" ) );
            ppCreditCard.Cvv2 = creditCard.Code;

            return new CardTender( ppCreditCard );
        }

        private BaseTender GetTender( BankAccount bankAccount )
        {
            var ppBankAccount = new BankAcct( bankAccount.AccountNumber, bankAccount.RoutingNumber );
            ppBankAccount.AcctType = bankAccount.AccountType == BankAccountType.Checking ? "C" : "S";
            ppBankAccount.Name = bankAccount.BankName;

            return new ACHTender( ppBankAccount );
        }

        private RecurringInfo GetReccurring( FinancialScheduledTransaction scheduledTransaction )
        {
            var ppRecurringInfo = new RecurringInfo();
            ppRecurringInfo.ProfileName = scheduledTransaction.AuthorizedPersonId.ToString();
            ppRecurringInfo.Start = scheduledTransaction.StartDate.ToString( "MMddyyyy" );

            var selectedFrequencyGuid = DefinedValueCache.Read( scheduledTransaction.TransactionFrequencyValueId ).Guid.ToString().ToUpper();
            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    ppRecurringInfo.PayPeriod = "YEAR";
                    ppRecurringInfo.Term = 1;
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    ppRecurringInfo.PayPeriod = "WEEK";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    ppRecurringInfo.PayPeriod = "BIWK";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY:
                    ppRecurringInfo.PayPeriod = "SMMO";
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    ppRecurringInfo.PayPeriod = "MONT";
                    break;
            }

            return ppRecurringInfo;
        }

        #endregion

        private bool MakeSale( FinancialTransaction transaction, Invoice invoice, BaseTender tender, out string errorMessage )
        {
            errorMessage = string.Empty;

            var ppTransaction = new SaleTransaction( GetUserInfo(), GetConnection(), invoice, tender, PayflowUtility.RequestId );
            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        transaction.TransactionCode = txnResponse.Pnref;
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


        private bool CreateRecurring( FinancialScheduledTransaction scheduledTransaction, RecurringInfo recurringInfo, Invoice invoice, BaseTender tender, out string errorMessage )
        {
            errorMessage = string.Empty;

            var ppTransaction = new RecurringAddTransaction( GetUserInfo(), GetConnection(), invoice, tender, recurringInfo, PayflowUtility.RequestId );
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
                            scheduledTransaction.TransactionCode = recurringResponse.ProfileId;
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

    }
}
