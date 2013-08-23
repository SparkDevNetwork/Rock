using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PayPal.Payments.Common;
using PayPal.Payments.Common.Utility;
using PayPal.Payments.DataObjects;
using PayPal.Payments.Transactions;

using Rock.Attribute;

namespace Rock.Financial.Gateway
{
    [TextField("PayPal Partner", "", true, "", "", 1, "Partner")]
    [TextField("PayPal Merchant Login", "", true, "", "", 1, "Vendor")]
    [TextField("PayPal User", "", true, "", "", 2, "User")]
    [TextField("PayPal Password", "", true, "", "", 3, "User")]
    [CustomRadioListField("Mode", "Mode to use for transactions", "Live,Test", true, "Live", "", 4)]
    public class PayFlow : GatewayComponent
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

        public override Model.FinancialTransaction Charge( Model.FinancialTransaction transaction, CreditCard creditCard, bool testTransaction = false )
        {
            var ppConnection = new PayflowConnectionData();
            
            var ppBillingInfo = new BillTo();
            ppBillingInfo.Street = creditCard.BillingStreet;
            ppBillingInfo.City = creditCard.BillingCity;
            ppBillingInfo.State = creditCard.BillingState;
            ppBillingInfo.Zip = creditCard.BillingZip;

            var ppAmount = new Currency(transaction.Amount);

            var ppCreditCard = new PayPal.Payments.DataObjects.CreditCard(creditCard.Number, creditCard.Code);
            
            var ppCardTender = new CardTender(ppCreditCard);
            
            var ppInvoice = new Invoice();
            ppInvoice.Amt = ppAmount;
            ppInvoice.BillTo = ppBillingInfo;

            var ppUersInfo = new UserInfo(
                GetAttributeValue("User"),
                GetAttributeValue("Vendor"),
                GetAttributeValue("Partner"),
                GetAttributeValue("Password"));

            var ppTransaction = new CreditTransaction(ppUersInfo, ppInvoice, ppCardTender, PayflowUtility.RequestId);
            var ppResponse = ppTransaction.SubmitTransaction();

            if (ppResponse != null)
            {
                TransactionResponse txnResponse = ppResponse.TransactionResponse;
                if (txnResponse != null)
                {
                    // Check response
                    Rock.Model.FinancialTransaction rockTransaction = new Model.FinancialTransaction();
                    return rockTransaction;
                }
            }

            return null;
        }

        public override Model.FinancialTransaction Charge( Model.FinancialTransaction transaction, BankAccount bankAccount, bool testTransaction = false )
        {
            throw new NotImplementedException();
        }

        public override Model.FinancialScheduledTransaction CreateScheduledTransaction( Model.FinancialScheduledTransaction transaction, CreditCard creditCard )
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
