using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Extension;
using Rock.Model;
using Rock.Web.UI;
using Rock.Data;
using Rock.Web;
using System.IO;

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Export to GL" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Export the current batch to the GL system." )]

    public partial class ExportToGL : RockBlock, ISecondaryBlock
    {
        FinancialBatch _batch = null;
        protected int IsExported = 0;
        private string DownloadGuid;

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbDownload );

            if ( !string.IsNullOrWhiteSpace( PageParameter( "batchId" ) ) )
            {
                _batch = new FinancialBatchService( new RockContext() ).Get( PageParameter( "batchId" ).AsInteger() );
            }

            if ( _batch != null )
            {
                _batch.LoadAttributes();
                IsExported = ( _batch.GetAttributeValue( "GLExported" ).AsBoolean() == true ? 1 : 0 );
            }

            if ( !IsPostBack && !UserCanEdit )
            {
                lbShowExport.Visible = false;
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            DownloadGuid = ViewState["DownloadGuid"].ToString();

            if(!string.IsNullOrEmpty( DownloadGuid )){
                Page.ClientScript.RegisterHiddenField( "downloadGuid", DownloadGuid );
            }  
        }

        protected override object SaveViewState()
        {
            if( !string.IsNullOrEmpty( DownloadGuid ) )
            {
                ViewState["DownloadGuid"] = DownloadGuid.ToString();
            }

            return base.SaveViewState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the GLRecords for the given batch, with the appropriate information about
        /// the export data.
        /// </summary>
        /// <param name="batch">Batch to be exported.</param>
        /// <param name="date">The date of this deposit.</param>
        /// <param name="accountingPeriod">Accounting period as defined in the GL system.</param>
        /// <param name="journalType">The type of journal entry to create as defined in the GL system.</param>
        /// <returns>A collection of GLRecord objects to be imported into the GL system.</returns>
        List<GLRecord> GLRecordsForBatch( FinancialBatch batch, DateTime date, string accountingPeriod, string journalType )
        {
            /*
            List<GLRecord> records = new List<GLRecord>();

            // Variables for Attribute Keys
            string TransactionDetailKey = "DefaultProject";
            string TransactionKey = "DefaultProject";
            string AccountKey = "DefaultProject";

            //
            // Load all the transaction details, load their attributes and then group
            // by the account attributes, GLBankAccount+GLCompany+GLFund.
            //
            var transactions = batch.Transactions
                .SelectMany( t => t.TransactionDetails )
                .ToList();

            foreach ( var d in transactions )
            {
                d.LoadAttributes();
                d.Account.LoadAttributes();
                d.Transaction.LoadAttributes();
            }

            //var accounts = transactions.GroupBy(d => new { GLBankAccount = d.Account.GetAttributeValue("GLBankAccount"), GLCompany = d.Account.GetAttributeValue("GLCompany"), GLFund = d.Account.GetAttributeValue("GLFund") }, d => d).OrderBy(g => g.Key.GLBankAccount);

            var accounts = transactions.GroupBy( d => new
            {
                GLBankAccount = d.Account.GetAttributeValue( "GLBankAccount" ),
                GLCompany = d.Account.GetAttributeValue( "GLCompany" ),
                GLFund = d.Account.GetAttributeValue( "GLFund" ),
                DEFaultProjectCode = d.Account.GetAttributeValue( TransactionDetailKey )
                TransactionTypeValue = d.Transaction.TransactionTypeValue.EntityStringValue
            },
                d => d ).OrderBy( g => g.Key.GLBankAccount );

            //
            // Go through each group and build the line items.
            //
            foreach ( var grp in accounts )
            {
                GLRecord record = new GLRecord();

                //
                // Build the bank account deposit line item.
                //
                record.AccountingPeriod = accountingPeriod;
                record.AccountNumber = grp.Key.GLBankAccount;
                record.Amount = grp.Sum( d => d.Amount );
                record.Company = grp.Key.GLCompany;
                record.Date = date;
                record.Department = string.Empty;
                //record.Description1 = batch.Name + " (" + batch.Id.ToString() + ")";
                record.Description1 = grp.Key.TransactionTypeValue;
                record.Description2 = string.Empty;
                record.Fund = grp.Key.GLFund;
                record.Journal = "0";
                record.JournalType = journalType;
                record.Project = string.Empty;

                // Commenting out
                records.Add( record );

                //
                // Build each of the revenue fund withdrawls.
                //

                // 12/27/2017 DJR Flatirons wants this grouped by account, and then project code.  The issue then becomes,
                // not all transaction details have project codes because they are arbitrary.  This gets a bit messy here
                // when trying to essentially, group by accounts, and then by arbitrary sub accounts.
                //foreach (var grpTransactions in grp.GroupBy(t => t.AccountId, t => t))
                foreach ( var grpTransactions in grp.GroupBy( t => t ) )
                {
                    record = new GLRecord();

                    // Getting project information
                    var project = string.Empty;

                    // Checking there are items in Array
                    if ( grpTransactions.Any() )
                    {

                        // Checking to see if project is stored in Financial Transaction Detail
                        if ( ( grpTransactions.First().AttributeValues.Where( a => a.Key == TransactionDetailKey ).Any() ) &&
                            ( !String.IsNullOrEmpty( grpTransactions.First().AttributeValues.Where( a => a.Key == TransactionDetailKey ).First().Value.ValueFormatted ) ) )
                        {
                            // Project is in Transaction Detail
                            project = grpTransactions.First().AttributeValues.Where( a => a.Key == TransactionDetailKey ).First().Value.ValueFormatted;
                        }
                        // Checking if project is stored in Financial Transaction
                        else if ( ( grpTransactions.First().Transaction.AttributeValues.Where( val => val.Key == TransactionKey ).Any() ) &&
                            ( !String.IsNullOrEmpty( grpTransactions.First().Transaction.AttributeValues.Where( val => val.Key == TransactionKey ).First().Value.ValueFormatted ) ) )
                        {
                            // Project Code is in the Transaction
                            project = grpTransactions.First().Transaction.AttributeValues.Where( val => val.Key == TransactionKey ).First().Value.ValueFormatted;
                        }
                        // Checking if project is stored in the Financial Account
                        else if ( ( grpTransactions.First().Account.AttributeValues.Where( val => val.Key == AccountKey ).Any() ) &&
                            ( !String.IsNullOrEmpty( grpTransactions.First().Account.AttributeValues.Where( av => av.Key == AccountKey ).First().Value.ValueFormatted ) ) )
                        {
                            // Project Code is in the Account
                            project = grpTransactions.First().Account.AttributeValues.Where( av => av.Key == AccountKey ).First().Value.ValueFormatted;
                        }
                    }

                    // Set the amnt from the transaction 
                    var amnt = -( grpTransactions.Sum( t => t.Amount ) );
                    var accountName = grpTransactions.First().Account.Name;
                    bool addNewRecord = true;

                    // DJR 12/27/2017 Look up the record in the list, to see if it already exists for this account name and project code.
                    for ( var i = 0; i < records.Count(); i++ )
                    {
                        var r = records[i];

                        if ( r.Description1.Equals( accountName ) && project.Equals( r.Project ) )
                        {
                            // Just modify the amount on this record that's already in the list.
                            records[i].Amount = ( records[i].Amount + amnt );
                            addNewRecord = false;
                        }

                    }

                    // Assinging properties correct values

                    if ( addNewRecord )
                    {
                        record.AccountingPeriod = accountingPeriod;
                        record.AccountNumber = grpTransactions.First().Account.GetAttributeValue( "GLRevenueAccount" );
                        //record.AccountNumber = accountNumber;
                        // record.Amount = -(grpTransactions.Sum(t => t.Amount));
                        record.Amount = amnt;
                        record.Company = grp.Key.GLCompany;
                        record.Date = date;
                        record.Department = grpTransactions.First().Account.GetAttributeValue( "GLRevenueDepartment" );
                        record.Description1 = accountName;
                        record.Description2 = string.Empty;
                        record.Fund = grp.Key.GLFund;
                        record.Journal = "0";
                        record.JournalType = journalType;
                        record.Project = project;
                        records.Add( record );
                    }
                }
            }

            return records;
            */

            List<GLRecord> records = new List<GLRecord>();

            // Load all the transaction details, load their attributes and then group
            // by the account attributes, GLBankAccount+GLCompany+GLFund.
            var transactions = batch.Transactions
                .SelectMany( t => t.TransactionDetails )
                .ToList();
            foreach ( var d in transactions )
            {
                d.LoadAttributes();
                d.Account.LoadAttributes();
            }
            var accounts = transactions.GroupBy( d => new
            {
                GLBankAccount = d.Account.GetAttributeValue( "GLBankAccount" ),
                GLCompany = d.Account.GetAttributeValue( "GLCompany" ),
                GLFund = d.Account.GetAttributeValue( "GLFund" )
            }, d => d )
                .OrderBy( g => g.Key.GLBankAccount.AsIntegerOrNull() )
                           .ThenBy( g => g.Key.GLFund.AsIntegerOrNull() );

            // Go through each group and build the line items.
            foreach ( var grp in accounts.ToList() )
            {
                GLRecord record = new GLRecord();

                // Build the bank account deposit line item.
                record.AccountingPeriod = accountingPeriod;
                record.AccountNumber = grp.Key.GLBankAccount;
                record.Amount = grp.Sum( d => d.Amount );
                record.Company = grp.Key.GLCompany;
                record.Date = date;
                record.Department = string.Empty;
                record.Description1 = "Contributions";
                record.Description2 = string.Empty;
                record.Fund = grp.Key.GLFund;
                record.Journal = "0";
                record.JournalType = journalType;
                record.Project = string.Empty;

                records.Add( record );

                // Build each of the revenue fund withdrawls.
                foreach ( var grpTransactions in grp.GroupBy( t => t.AccountId, t => t )
                    .OrderBy( o => o.First().Account.GetAttributeValue( "GLRevenueDepartment" ).AsIntegerOrNull() )
                    .ThenBy( o => o.First().Account.GetAttributeValue( "GLRevenueAccount" ).AsIntegerOrNull() ) )
                {
                    record = new GLRecord();

                    record.AccountingPeriod = accountingPeriod;
                    record.AccountNumber = grpTransactions.First().Account.GetAttributeValue( "GLRevenueAccount" );
                    record.Amount = -( grpTransactions.Sum( t => t.Amount ) );
                    record.Company = grp.Key.GLCompany;
                    record.Date = date;
                    record.Department = grpTransactions.First().Account.GetAttributeValue( "GLRevenueDepartment" );
                    record.Description1 = "Contributions";
                    record.Description2 = string.Empty;
                    record.Fund = grp.Key.GLFund;
                    record.Journal = "0";
                    record.JournalType = journalType;
                    record.Project = string.Empty;

                    records.Add( record );
                }
            }

            return records;
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlMain.Visible = visible;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the lbShowExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowExport_Click( object sender, EventArgs e )
        {
            dpDate.SelectedDate = RockDateTime.Now;
            tbAccountingPeriod.Text = GetUserPreference( "com.bemaservices.exporttogl.accountingperiod" );
            tbJournalType.Text = GetUserPreference( "com.bemaservices.exporttogl.journaltype" );
            nbAlreadyExported.Visible = _batch.GetAttributeValue( "GLExported" ).AsBoolean();
            nbNotClosed.Visible = _batch.Status != BatchStatus.Closed;

            pnlExportModal.Visible = true;
            mdExport.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbExportSave_Click( object sender, EventArgs e )
        {
            SetUserPreference( "com.bemaservices.exporttogl.accountingperiod", tbAccountingPeriod.Text );
            SetUserPreference( "com.bemaservices.exporttogl.journaltype", tbJournalType.Text );

            mdExport.Hide();
            pnlExportModal.Visible = false;

            //
            // After the page updates, simulate a click on the download link, wait 1
            // second and then reload the page (non-postback) so that the UI will update
            // to reflect changes about the batch.
            //
            string script = string.Format( "document.getElementById('{0}').click(); setTimeout(function() {{ location.reload(false); }}, 5000);", lbDownload.ClientID );
			//string script = string.Format( "document.getElementById('{0}').click();", lbDownload.ClientID );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "PerformExport", script, true );
        }

        public static BinaryFile Save( string fileName, RockContext rockContext, string fileContents, BinaryFileType binaryFileType = null)
        {
            if ( binaryFileType == null )
            {
                binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );
            }

            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes( fileContents );
            MemoryStream stream = new MemoryStream( byteArray );

            var binaryFile = new BinaryFile()
            {
                Guid = Guid.NewGuid(),
                IsTemporary = true,
                BinaryFileTypeId = binaryFileType.Id,
                MimeType = "text/plain",
                FileName = fileName,
                ContentStream = stream
            };

            var binaryFileService = new BinaryFileService( rockContext );
            binaryFileService.Add( binaryFile );
            rockContext.SaveChanges();
            return binaryFile;
        }

        /// <summary>
        /// Handles the Click event of the lbDownload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDownload_Click( object sender, EventArgs e )
        {
            var parameters = RockPage.PageParameters();

            var records = GLRecordsForBatch( _batch, dpDate.SelectedDate.Value, tbAccountingPeriod.Text.Trim(), tbJournalType.Text.Trim() );

            if ( !UserCanEdit )
            {
                return;
            }

            //
            // Update the batch to reflect that it has been exported.
            //
            using ( var rockContext = new RockContext() )
            {
                FinancialBatch batch = new FinancialBatchService( rockContext ).Get( _batch.Id );

                batch.LoadAttributes();
                batch.SetAttributeValue( "GLExported", "true" );
                batch.SaveAttributeValues( rockContext );
                IsExported = 1;

                rockContext.SaveChanges();
            }
			
			//
            // Send the results as a CSV file for download.
            //
            Page.EnableViewState = false;
            Page.Response.Clear();
            Page.Response.ContentType = "text/plain";
            Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=GLTRN2000.txt" );
            Page.Response.Write( string.Join( "\r\n", records.Select( r => r.ToString() ).ToArray() ) );
            Page.Response.Flush();
            Page.Response.End();


        }

        #endregion
    }

    class GLRecord
    {
        public string Company { get; set; }
        public string Fund { get; set; }
        public string AccountingPeriod { get; set; }
        public string JournalType { get; set; }
        public string Journal { get; set; }

        public DateTime Date { get; set; }

        public string Description1 { get; set; }

        public string Description2 { get; set; }

        public string Department { get; set; }
        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }

        public string Project { get; set; }

        public override string ToString()
        {
            return string.Format( "\"00000\",\"0{0}00{1}{2}{3}{4}\",\"000\",\"{5}\",\"{6}\",\"{7}\",\"{8}{9}\",\"{10}\",\"{11}\"",
                ( Company ?? string.Empty ).PadLeft( 3, '0' ).TrimLength( 3 ),
                ( Fund ?? string.Empty ).PadLeft( 3, '0' ).TrimLength( 3 ),
                ( AccountingPeriod ?? string.Empty ).PadLeft( 2, '0' ).TrimLength( 2 ),
                ( JournalType ?? string.Empty ).PadLeft( 2, '0' ).TrimLength( 2 ),
                ( Journal ?? string.Empty ).PadLeft( 5, '0' ).TrimLength( 5 ),
                Date.ToString( "MMddyy" ),
                ( Description1 ?? string.Empty ).TrimLength( 30 ),
                ( Description2 ?? string.Empty ).TrimLength( 30 ),
                ( Department ?? string.Empty ).PadLeft( 3, '0' ).TrimLength( 3 ),
                ( AccountNumber ?? string.Empty ).PadLeft( 9, '0' ).TrimLength( 9 ),
                Math.Round( Amount * 100 ).ToString( "0" ),
                ( Project ?? string.Empty ).TrimLength( 30 ) );
        }
    }

    /* Commenting out because this static method already existing in BatchListWithGLExport
    static class StringExtensions
    {
        public static string TrimLength( this string value, int maxLength )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring( 0, maxLength );
        }
    }
    */
}