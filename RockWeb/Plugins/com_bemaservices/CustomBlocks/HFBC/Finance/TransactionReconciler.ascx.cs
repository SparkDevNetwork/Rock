using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using CsvHelper;
using System.IO;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Web;
using RestSharp;
using System.Xml.Serialization;
using RestSharp.Deserializers;
using System.Collections;

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Transaction Reconciler" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Allows you to select a batch an reconcile the information in Rock against the information in the payment processor" )]
    [AttributeField( Rock.SystemGuid.EntityType.FINANCIAL_BATCH, "Reconciled Flag", "Attribute to designate Batch as being reconciled", true )]
    public partial class TransactionReconciler : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            scriptManager.RegisterPostBackControl( this.btnExportToExcel );

            if ( !IsPostBack )
            {
                if ( !string.IsNullOrEmpty( Request.QueryString["BatchIds"] ) )
                {
                    var batchIds = Request.QueryString["BatchIds"].Split( ',' ).AsIntegerList();

                    var batches = CombineRockAndNMI( batchIds );

                    // Binding
                    rItems.DataSource = batches;
                    rItems.DataBind();
                }
            }
        }

        /// <summary>
        /// Combines the Financial info from Rock with Financial info from NMI.
        /// </summary>
        /// <param name="batchIds">The batch ids.</param>
        /// <returns></returns>
        private List<Batch> CombineRockAndNMI( List<int> batchIds )
        {
            RockContext rockContext = new RockContext();
            FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );

            List<Batch> batches = new List<Batch>();

            foreach ( var batchId in batchIds )
            {
                var batch = financialBatchService.GetNoTracking( batchId );
                if ( batch != null )
                {
                    List<string> transactionIds = batch.Transactions.Where( x => !string.IsNullOrEmpty( x.TransactionCode ) ).Select( x => x.TransactionCode ).ToList();
                    List<NMITransaction> nmiTransactions = GetTransactionsFromNMI( transactionIds );

                    var newBatch = new Batch
                    {
                        RockBatch = batch,
                        Amount = batch.Transactions.Sum( x => x.TotalAmount ),
                        TransactionCount = batch.Transactions.Count,
                        BatchDateTime = batch.CreatedDateTime.Value,
                    };

                    // Looping through transactions
                    foreach ( var transaction in batch.Transactions )
                    {
                        if( transaction.TransactionDetails.Any() )
                        {
                            var newTransaction = new Transaction
                            {
                                Account = transaction.TransactionDetails.FirstOrDefault().Account,
                                RockTransaction = transaction
                            };

                            // looping through transaction details
                            foreach ( var transactionDetail in transaction.TransactionDetails )
                            {
                                var matchingFund = newBatch.Funds.Where( x => x.FundName == transactionDetail.Account.Name );
                                if ( matchingFund.Any() )
                                {
                                    if ( transactionDetail.Amount < 0 )
                                    {
                                        matchingFund.FirstOrDefault().DepositAmount += transactionDetail.Amount;
                                    }
                                    else
                                    {
                                        matchingFund.FirstOrDefault().CreditAmount += transactionDetail.Amount;
                                    }
                                    matchingFund.FirstOrDefault().GLCode = transactionDetail.Account.GlCode;
                                }
                                else
                                {
                                    // Loading Attributes
                                    transactionDetail.Account.LoadAttributes();

                                    var newFund = new Fund
                                    {
                                        CreditAmount = transactionDetail.Amount < 0 ? 0 : transactionDetail.Amount,
                                        DepositAmount = transactionDetail.Amount > 0 ? transactionDetail.Amount : 0,
                                        FundName = transactionDetail.Account.Name,
                                        GLFund = transactionDetail.Account.GetAttributeValue("GLFund"),
                                        GLBankAccount = transactionDetail.Account.GetAttributeValue( "GLBankAccount"),
                                        GLRevenueAccount = transactionDetail.Account.GetAttributeValue( "GLRevenueAccount" ),
                                        GLRevenueDepartment = transactionDetail.Account.GetAttributeValue( "GLRevenueDepartment" )
                                    };

                                    newBatch.Funds.Add( newFund );
                                }
                            }

                            transaction.LoadAttributes();
                            var nmiTransaction = nmiTransactions.Where( x => x.TransactionId == transaction.TransactionCode );
                            if ( nmiTransaction.Any() )
                            {
                                newTransaction.NMITransaction = nmiTransaction.FirstOrDefault();
                            }

                            newBatch.Transactions.Add( newTransaction );
                        }
                    }

                    batches.Add( newBatch );
                }

            }

            return batches;
        }

        /// <summary>
        /// Gets the transactions from NMI API
        /// </summary>
        /// <param name="transactionIds">The transaction ids.</param>
        /// <returns></returns>
        private List<NMITransaction> GetTransactionsFromNMI( List<string> transactionIds )
        {
            RockContext rockContext = new RockContext();
            FinancialGatewayService financialGatewayService = new FinancialGatewayService( rockContext );
            var gateway = financialGatewayService.Get( 3 );
            gateway.LoadAttributes();

            var username = gateway.GetAttributeValue( "AdminUsername" );
            var password = gateway.GetAttributeValue( "AdminPassword" );

            var client = new RestClient( "https://secure.networkmerchants.com" );
            var request = new RestRequest( "api/query.php", Method.GET );
            request.AddQueryParameter( "username", username );
            request.AddQueryParameter( "password", password );
            request.AddQueryParameter( "transaction_id", string.Join( ",", transactionIds ) );  

            IRestResponse<List<NMITransaction>> response = client.Execute<List<NMITransaction>>( request );

            return response.Data;
        }

        /// <summary>
        /// Called when repeater generates table items
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void OnItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Repeater rFunds = e.Item.FindControl( "rItemsFunds" ) as Repeater;
                Repeater rTransactions = e.Item.FindControl( "rItemsTransactions" ) as Repeater;
                Button btnMarkReconciled = e.Item.FindControl( "btnMarkReconciled" ) as Button;

                Batch batch = e.Item.DataItem as Batch;

                // Loading batch Attributes
                batch.RockBatch.LoadAttributes();
                var batchStatus = batch.RockBatch.GetAttributeValue( "Reconciled" ).AsBoolean();

                rFunds.DataSource = batch.Funds;
                rTransactions.DataSource = batch.Transactions;
                rFunds.DataBind();
                rTransactions.DataBind();

                btnMarkReconciled.Text = batchStatus ? "Batch Reconciled" : "Mark Reconciled";
                btnMarkReconciled.Enabled = !batchStatus;
                btnMarkReconciled.CommandArgument = batch.RockBatch.Id.ToString();

            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rItemsTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rItemsTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Repeater rItemsTransactionBreakdown = e.Item.FindControl( "rItemsTransactionBreakdown" ) as Repeater;
                Transaction transaction = e.Item.DataItem as Transaction;

                List<TransactionDetail> transactionDetails = new List<TransactionDetail>();
                RockContext rockContext = new RockContext();

                foreach ( var transactionDetail in transaction.RockTransaction.TransactionDetails )
                {
                    var registration = "No link found";
                    if ( transactionDetail.EntityId > 0 && transactionDetail.EntityType.FriendlyName == "Registration" )
                    {
                        RegistrationService registrationService = new RegistrationService( rockContext );
                        registration = registrationService.GetNoTracking( transactionDetail.EntityId.Value ).RegistrationInstance.Name;
                    }

                    var newTransaction = new TransactionDetail
                    {
                        EntityType = transactionDetail.EntityType != null ? transactionDetail.EntityType.FriendlyName : "",
                        EntityName = registration,
                        Amount = transactionDetail.Amount.FormatAsCurrency()
                    };

                    transactionDetails.Add( newTransaction );
                }

                rItemsTransactionBreakdown.DataSource = transactionDetails;
                rItemsTransactionBreakdown.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMarkReconciled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMarkReconciled_Click( object sender, EventArgs e )
        {
            Button button = sender as Button;
            var batchId = button.CommandArgument;

            RockContext rockContext = new RockContext();
            FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );
            var batch = financialBatchService.Get( batchId.AsInteger() );
            batch.LoadAttributes();

            var attributeGuid = GetAttributeValue( "ReconciledFlag" ).AsGuidOrNull();
            if( attributeGuid != null )
            {
                var attribute = AttributeCache.Get( attributeGuid.Value );

                batch.SetAttributeValue( attribute.Key, "True" );
                batch.SaveAttributeValue( attribute.Key );
                rockContext.SaveChanges();

                Button btnMarkReconciled = FindControlRecursive( this.FindControl( "upMain" ), "btnMarkReconciled" ) as Button;
                btnMarkReconciled.Text = "Batch Reconciled";
                btnMarkReconciled.Enabled = false;
            }
        }

        /// <summary>
        /// Finds the control recursive.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Control FindControlRecursive( Control container, string name )
        {
            if ( ( container.ID != null ) && ( container.ID.Equals( name ) ) )
                return container;

            foreach ( Control ctrl in container.Controls )
            {
                Control foundCtrl = FindControlRecursive( ctrl, name );
                if ( foundCtrl != null )
                    return foundCtrl;
            }
            return null;
        }

        /// <summary>
        /// Handles the Click event of the btnExportToExcel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportToExcel_Click( object sender, EventArgs e)
        {
            var batchIds = Request.QueryString["BatchIds"].Split( ',' ).AsIntegerList();

            var batches = CombineRockAndNMI( batchIds );
            var csvContents = CreateCSV( batches );

            Response.ClearContent();
            Response.Clear();
            Response.ContentType = "text/plain";
            //Response.AppendHeader( "Content-Disposition", string.Format( "attachment; filename=BatchExport_{0}.csv", string.Join( "-", batches.Select( x => x.RockBatch.Id ).ToList() ) ) );
            Response.AppendHeader( "Content-Disposition", string.Format( "attachment; filename=BatchExport_{0}.csv", RockDateTime.Now.ToString( "MM-dd-yyyy-Hmm" ) ) );
            Response.Output.Write( csvContents );
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Creates the CSV.
        /// </summary>
        /// <param name="batches">The batches.</param>
        /// <returns></returns>
        protected string CreateCSV( List<Batch> batches)
        {
            var output = @"RockBatchId,RockBatchName,RockBatchStatus,RockBatchStartDateTime,RockBatchEndDateTime,RockBatchCampus,TransactionCount,RockBatchControlAmount,Amount,NMITransactionActionType,NMITransactionAmount,NMITransactionDate,NMITransactionResponseText,NMITransactionSource,NMITransactionSuccess,NMITransactionAuthorizationCode,NMITransactionCustomerId,NMITransactionFirstName,NMITransactionLastName,NMITransactionEmail,NMITransactionTransactionId,NMITransactionTransactionType,GLCompany,GLFund,GLBankAccount,GLRevenueDepartment,GLRevenueAccount" + System.Environment.NewLine;

            foreach ( var batch in batches )
            {
                foreach( var transaction in batch.Transactions )
                {
                    // Loading Attributes
                    transaction.Account.LoadAttributes();

                    // Creating line
                    var line = string.Format( "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}{27}",
                        batch.RockBatch.Id,
                        batch.RockBatch.Name,
                        batch.RockBatch.Status,
                        batch.RockBatch.BatchStartDateTime,
                        batch.RockBatch.BatchEndDateTime,
                        batch.RockBatch.Campus != null ? batch.RockBatch.Campus.Name : string.Empty,
                        batch.TransactionCount,
                        batch.RockBatch.ControlAmount,
                        batch.Amount,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.ActionType : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.Amount.FormatAsCurrency() : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.Date : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.ResponseText : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.Source : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Action.Success == 1 ?"True" : "False" : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.AuthorizationCode : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.CustomerId : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.FirstName : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.LastName : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.Email : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.TransactionId : string.Empty,
                        transaction.NMITransaction != null ? transaction.NMITransaction.TransactionType == "cc" ? "Credit Card" : transaction.NMITransaction.TransactionType : string.Empty,
                        transaction.Account.GetAttributeValue("GLCompany"),
                        transaction.Account.GetAttributeValue( "GLFund" ),
                        transaction.Account.GetAttributeValue( "GLBankAccount" ),
                        transaction.Account.GetAttributeValue( "GLRevenueDepartment" ),
                        transaction.Account.GetAttributeValue( "GLRevenueAccount" ),
                        System.Environment.NewLine
                    );

                    output = output + line;
                }
            }

            return output;
        }
    }


    [XmlRoot( "nm_response" )]
    [DeserializeAs( Name = "transaction" )]
    public class NMITransaction
    {
        [XmlElement( "transaction_id" )]
        public string TransactionId { get; set; }

        [XmlElement( "transaction_type" )]
        public string TransactionType { get; set; }

        [XmlElement( "authorization_code" )]
        public string AuthorizationCode { get; set; }

        [XmlElement( "order_description" )]
        public string OrderDescription { get; set; }

        [XmlElement( "first_name" )]
        public string FirstName { get; set; }

        [XmlElement( "last_name" )]
        public string LastName { get; set; }

        [XmlElement( "email" )]
        public string Email { get; set; }

        [XmlElement( "customer_id" )]
        public string CustomerId { get; set; }

        [XmlElement( "action" )]
        public Action Action { get; set; }
    }

    public class Action
    {
        [XmlElement( "amount" )]
        public decimal Amount { get; set; }

        [XmlElement( "action_type" )]
        public string ActionType { get; set; }

        [XmlElement( "date" )]
        public string Date { get; set; }

        [XmlElement( "succes" )]
        public int Success { get; set; }

        [XmlElement( "source" )]
        public string Source { get; set; }

        [XmlElement( "response_text" )]
        public string ResponseText { get; set; }
    }

    public class Batch
    {
        public Batch()
        {
            Transactions = new List<Transaction>();
            Funds = new List<Fund>();
        }

        public FinancialBatch RockBatch { get; set; }
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime BatchDateTime { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
    public class Fund
    {
        public string GLCode { get; set; }
        public string FundName { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string GLFund { get; set; }
        public string GLBankAccount { get; set; }
        public string GLRevenueDepartment { get; set; }
        public string GLRevenueAccount { get; set; }

    }
    public class Transaction
    {
        public FinancialAccount Account { get; set; }
        public FinancialTransaction RockTransaction { get; set; }
        public NMITransaction NMITransaction { get; set; }
    }

    public class TransactionDetail
    {
        public string EntityType { get; set; }
        public string EntityName { get; set; }
        public string Amount { get; set; }
    }
}
