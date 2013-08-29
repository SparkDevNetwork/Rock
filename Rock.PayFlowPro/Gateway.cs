//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;

using Rock.Attribute;
using Rock.Financial;

namespace Rock.PayFlowPro
{
    /// <summary>
    /// PayFlowPro Payment Gateway
    /// </summary>
    [Description( "PayFlowPro Payment Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "PayFlowPro" )]

    [TextField( "PayPal Partner", "", true, "", "", 0, "Partner" )]
    [TextField("PayPal Merchant Login", "", true, "", "", 1, "Vendor")]
    [TextField("PayPal User", "", false, "", "", 2, "User")]
    [TextField("PayPal Password", "", true, "", "", 3, "Password")]
    [CustomRadioListField("Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4)]

    public class Gateway : GatewayComponent
    {
        private string GatewayUrl
        {
            get 
            {
                if (GetAttributeValue("Mode").Equals("Live", StringComparison.CurrentCultureIgnoreCase))
                {
                    return "https://payflowpro.paypal.com";
                }
                else
                {
                    return "https://pilot-payflowpro.paypal.com";
                }
            }
        }

        private string Charge( Invoice invoice, BaseTender tender, out string errorMessage )
        {
            errorMessage = string.Empty;

            var ppConnection = new PayflowConnectionData( GatewayUrl );

            var ppUserInfo = new UserInfo(
                GetAttributeValue( "User" ),
                GetAttributeValue( "Vendor" ),
                GetAttributeValue( "Partner" ),
                GetAttributeValue( "Password" ) );

            var ppTransaction = new SaleTransaction( ppUserInfo, invoice, tender, PayflowUtility.RequestId );
            var ppResponse = ppTransaction.SubmitTransaction();

            if ( ppResponse != null )
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if ( txnResponse != null )
                {
                    if ( txnResponse.Result == 0 ) // Success
                    {
                        return txnResponse.Pnref;
                    }
                    else
                    {
                        errorMessage = string.Format( "[{0}] {1}", txnResponse.Result, txnResponse.RespMsg );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Initiates a credit-card sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string Charge( Rock.Financial.CreditCard creditCard, out string errorMessage )
        {
            var ppBillingInfo = new BillTo();
            ppBillingInfo.Street = creditCard.BillingStreet;
            ppBillingInfo.City = creditCard.BillingCity;
            ppBillingInfo.State = creditCard.BillingState;
            ppBillingInfo.Zip = creditCard.BillingZip;

            var ppAmount = new Currency( creditCard.Amount );

            var ppCreditCard = new PayPal.Payments.DataObjects.CreditCard( creditCard.Number, creditCard.Code );

            var ppCardTender = new CardTender( ppCreditCard );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;
            ppInvoice.BillTo = ppBillingInfo;

            return Charge( ppInvoice, ppCardTender, out errorMessage );
        }

        /// <summary>
        /// Initiates an ach sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public override string Charge( BankAccount bankAccount, out string errorMessage )
        {
            var ppAmount = new Currency( bankAccount.Amount );

            var ppBankAccount = new BankAcct( bankAccount.AccountNumber, bankAccount.RoutingNumber );
            ppBankAccount.AcctType = bankAccount.AccountType == BankAccountType.Checking ? "C" : "S";
            ppBankAccount.Name = bankAccount.BankName;

            var ppAchTender = new ACHTender( ppBankAccount );

            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;

            return Charge( ppInvoice, ppAchTender, out errorMessage );
        }

        public override Model.FinancialScheduledTransaction CreateScheduledTransaction( Model.FinancialScheduledTransaction transaction, Rock.Financial.CreditCard creditCard )
        {
            throw new NotImplementedException();
        }

        public override Model.FinancialScheduledTransaction CreateScheduledTransaction( Model.FinancialScheduledTransaction transaction, BankAccount bankAccount )
        {
            throw new NotImplementedException();
        }

        public override Model.FinancialScheduledTransaction UpdateScheduledTransaction( Model.FinancialScheduledTransaction transaction )
        {
            throw new NotImplementedException();
        }

        public override bool CancelScheduledTransaction( Model.FinancialScheduledTransaction transaction )
        {
            throw new NotImplementedException();
        }

        public override void ProcessPostBack( System.Web.HttpRequest request )
        {
            throw new NotImplementedException();
        }

        public override void DownloadNewTransactions()
        {
            throw new NotImplementedException();
        }

    }
}
