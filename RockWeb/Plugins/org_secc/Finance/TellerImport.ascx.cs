// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Xml.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Security.Cryptography;
using Rock.Security;
using System.Text;

namespace RockWeb.Plugins.org_secc.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Teller Import" )]
    [Category( "SECC > Finance" )]
    [Description( "Block for importing teller files to a batch." )]

    [LinkedPage( "Batch Detail Page", "The page used to display details of a batch.", false, "", "", 1 )]
    public partial class TellerImport : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _binaryFileId = null;
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();
        private static int _transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
        private static int _currencyTypeCheck = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid() ).Id;
        private static int _currencyTypeCash = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH.AsGuid() ).Id;
        private decimal _totalAmount = 0.0M;

        protected string signalREventName = "tellerImport";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _binaryFileId = ViewState["BinaryFileId"] as int?;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            signalREventName = string.Format( "tellerImport_{0}_{1}", this.BlockId, Session.SessionID );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.1.2.min.js", fingerprint: false );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Load dropdown list with all of the open batches
                    var batches = new FinancialBatchService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( b =>
                            b.Status != BatchStatus.Closed &&
                            b.BatchStartDateTime.HasValue )
                        .OrderByDescending( b => b.BatchStartDateTime )
                        .ThenBy( b => b.Name )
                        .Select( b => new
                        {
                            b.Id,
                            b.Name,
                            b.BatchStartDateTime
                        } )
                        .ToList()
                        .Select( b => new
                        {
                            b.Id,
                            Name = string.Format( "{0} ({1})", b.Name, b.BatchStartDateTime.Value.ToShortDateString() )
                        } )
                        .ToList();

                    ddlBatch.DataSource = batches;
                    ddlBatch.DataBind();
                    ddlBatch.Items.Insert( 0, new ListItem( "", "" ) );
                }

                ShowEntry();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["BinaryFileId"] = _binaryFileId;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the FileUploaded event of the fuTellerFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuTellerFile_FileUploaded( object sender, EventArgs e )
        {
            _binaryFileId = fuTellerFile.BinaryFileId;

            // Validate the XML (method will display error message if not valid
            IsXmlValid();
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            // If the XML is valid, ask for a confirmation
            if ( IsXmlValid() )
            {
                ShowConfirmation();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            // send signalR message to start progress indicator
            int progress = 0;
            _hubContext.Clients.All.receiveNotification( signalREventName, "0" );

            XDocument xml = null;
            int? total = null;
            int? batchId = null;
            string batchName = string.Empty;

            int? attributeId = null;
            int? binaryFileTypeId = null;

            using ( var rockContext = new RockContext() )
            {
                // Get the XML
                var binaryFile = new BinaryFileService( rockContext ).Get( _binaryFileId.Value );
                if ( binaryFile != null )
                {
                    using ( var stream = binaryFile.ContentStream )
                    {
                        xml = XDocument.Load( stream );
                    }
                }

                // Get the number of transactions
                if ( xml != null )
                {
                    total = xml.Root.Descendants().Where( n => n.Name == "Gift" ).Count();
                }

                if ( xml != null && total.HasValue && total.Value > 0 )
                {
                    var batchService = new FinancialBatchService( rockContext );
                    FinancialBatch batch = null;

                    // Load (or create) the batch
                    batchId = ddlBatch.SelectedValueAsInt();
                    if ( batchId.HasValue )
                    {
                        batch = batchService.Get( batchId.Value );
                    }

                    if ( batch == null )
                    {
                        batch = new FinancialBatch();
                        batch.Guid = Guid.NewGuid();
                        // batch.Name = Path.GetFileNameWithoutExtension( binaryFile.FileName.Replace("ShelbyTELLERContributions",""));
                        batch.Name = tbBatchName.Text;
                        batch.Status = BatchStatus.Open;
                        batch.BatchStartDateTime = RockDateTime.Today;
                        batch.BatchEndDateTime = batch.BatchStartDateTime.Value.AddDays( 1 );
                        batch.ControlAmount = 0;
                        batchService.Add( batch );

                        rockContext.SaveChanges();

                        batchId = batch.Id;
                    }

                    batchName = batch.Name;

                    // Get the attribute id for the envelop number attribute
                    int? personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
                    attributeId = new AttributeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.EntityTypeId == personEntityTypeId &&
                            a.Key == "GivingEnvelopeNumber" )
                        .Select( a => a.Id )
                        .FirstOrDefault();

                    // Get the binary file type for contribution images
                    var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid() );
                    if ( binaryFileType != null )
                    {
                        binaryFileTypeId = binaryFileType.Id;
                    }
                }
            }

            // Initialize the status variables
            int matchCount = 0;
            int unmatchCount = 0;
            int errorCount = 0;

            var allErrorMessages = new List<string>();

            // Process each transaction
            foreach ( var node in xml.Root.Descendants().Where( n => n.Name == "Gift" ) )
            {
                var errorMessages = new List<string>();

                var status = ProcessTransaction( node, batchId, attributeId, binaryFileTypeId, out errorMessages );

                switch ( status )
                {
                    case ProcessStatus.Matched: matchCount++; break;
                    case ProcessStatus.Unmatched: unmatchCount++; break;
                    case ProcessStatus.Error: errorCount++; break;
                }

                allErrorMessages.AddRange( errorMessages );

                // Update progress using signalR
                progress++;
                int percentage = ( progress * 100 ) / total.Value;
                _hubContext.Clients.All.receiveNotification( signalREventName, percentage.ToString( "N0" ) );
            }

            // update success message to indicate the txns that were updated
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<li>{0:N0} {1} processed.</li>", total.Value, "transaction".PluralizeIf( total.Value > 1 ) );
            sb.AppendFormat( "<li>{0:N0} {1} matched to an existing person.</li>", matchCount,
                ( matchCount == 1 ? "transaction was" : "transactions were" ) );
            sb.AppendFormat( "<li>{0:N0} {1} NOT matched to an existing person.</li>", unmatchCount,
                ( unmatchCount == 1 ? "transaction was" : "transactions were" ) );
            if ( errorCount > 0 )
            {
                sb.AppendFormat( "<li>{0:N0} {1} NOT imported due to error during import (see errors below).</li>", errorCount,
                    ( errorCount == 1 ? "transaction was" : "transactions were" ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var batch = new FinancialBatchService( rockContext ).Get( batchId.Value );
                if ( batch != null )
                {
                    // update batch control amount
                    batch.ControlAmount += _totalAmount;
                    rockContext.SaveChanges();

                    // Add link to batch
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "batchId", batchId.ToString() );
                    string batchUrl = LinkedPageUrl( "BatchDetailPage", qryParam );
                    string batchLink = string.Format( "<a href='{0}'>{1}</a>", batchUrl, batch.Name );

                    int totalTransactions = matchCount + unmatchCount;
                    string summaryformat = totalTransactions == 1 ?
                        "<li>{0} transaction of {1} was added to the {2} batch.</li>" :
                        "<li>{0} transactions totaling {1} were added to the {2} batch</li>";
                    sb.AppendFormat( summaryformat, totalTransactions.ToString( "N0" ), _totalAmount.FormatAsCurrency(), batchLink );
                }
            }

            nbSuccess.Text = string.Format( "<ul>{0}</ul>", sb.ToString() );

            // Display any errors that occurred
            if ( allErrorMessages.Any() )
            {
                StringBuilder sbErrors = new StringBuilder();
                foreach ( var errorMsg in allErrorMessages )
                {
                    sbErrors.AppendFormat( "<li>{0}</li>", errorMsg );
                }

                nbErrors.Text = string.Format( "<ul>{0}</ul>", sbErrors.ToString() );
                nbErrors.Visible = true;
            }
            else
            {
                nbErrors.Visible = false;
            }

            ShowResults();

        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirm_Click( object sender, EventArgs e )
        {
            ShowEntry();
        }

        /// <summary>
        /// Handles the Click event of the btnImportAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportAnother_Click( object sender, EventArgs e )
        {
            fuTellerFile.BinaryFileId = null;
            ddlBatch.SetValue( "" );

            ShowEntry();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the entry.
        /// </summary>
        private void ShowEntry()
        {
            pnlEntry.Visible = true;
            pnlConfirm.Visible = false;
            pnlResults.Visible = false;
        }

        /// <summary>
        /// Shows the confirmation.
        /// </summary>
        private void ShowConfirmation()
        {
            // Get the number of transactions in the XML file
            int? txnCount = null;
            var xml = GetXml();
            if ( xml != null )
            {
                txnCount = xml.Root.Descendants().Where( n => n.Name == "Gift" ).Count();
            }

            if ( txnCount.HasValue )
            {
                // Format a confirmation message based on number of txns and the batch (or lack of) selected
                string batchName = "a new batch";
                int? batchId = ddlBatch.SelectedValueAsInt();
                if ( batchId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var batch = new FinancialBatchService( rockContext ).Get( batchId.Value );
                        if ( batch != null )
                        {
                            batchName = string.Format( "the <strong><em>{0} ({1})</em></strong> batch", batch.Name, batch.BatchStartDateTime.Value.ToShortDateString() );
                        }
                    }
                }
                lConfirm.Text = string.Format( "This will import <strong>{0:N0}</strong> transactions to {1}. Click <em>Confirm</em> to continue.", txnCount.Value, batchName );

                // Show the confirm/status/result dialog 
                pnlEntry.Visible = false;
                pnlConfirm.Visible = true;
                pnlResults.Visible = false;
            }
        }

        /// <summary>
        /// Shows the results.
        /// </summary>
        private void ShowResults()
        {
            pnlEntry.Visible = false;
            pnlConfirm.Visible = false;
            pnlResults.Visible = true;
        }

        /// <summary>
        /// Processes a transaction.
        /// </summary>
        /// <param name="giftElement">The gift element.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="envelopeAttributeId">The envelope attribute identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private ProcessStatus ProcessTransaction( XElement giftElement, int? batchId, int? envelopeAttributeId, int? binaryFileTypeId, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get batch/sequence number first as they are used in any error messages
            string batchNo = GetChildElementValue( giftElement, "BatchNo" );
            string seqNo = GetChildElementValue( giftElement, "SequenceNo" );

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    // Check to see if accout/routing/check information is specified
                    string encryptedAccountNum = GetChildElementValue( giftElement, "DFIAcct" );
                    string encryptedRoutingNum = GetChildElementValue( giftElement, "DFID" );
                    string checkNum = GetChildElementValue( giftElement, "CheckNum" );

                    // Start to create the transaction
                    var txn = new FinancialTransaction();
                    txn.BatchId = batchId;
                    txn.TransactionTypeValueId = _transactionTypeContributionId;
                    txn.TransactionDateTime = GetChildElementValue( giftElement, "GiftDate" ).AsDateTime();
                    txn.Summary = string.Format( "{0}:{1}", batchNo, seqNo );
                    txn.FinancialPaymentDetail = new FinancialPaymentDetail();

                    // Try to find an person to associate with this account
                    int? personAliasId = null;
                    if ( !string.IsNullOrWhiteSpace( encryptedAccountNum ) && !string.IsNullOrWhiteSpace( encryptedRoutingNum ) )
                    {

                        // If account/routing information was included, use it to find matching person from Bank Account table
                        // A person will be selected if there is only ONE match found with same information.
                        var accountNum = DecryptAccountInformation( encryptedAccountNum );
                        var routingNum = DecryptAccountInformation( encryptedRoutingNum );

                        string checkMicrHashed = FinancialPersonBankAccount.EncodeAccountNumber( routingNum, accountNum );

                        if ( !string.IsNullOrWhiteSpace( checkMicrHashed ) )
                        {
                            var matchedPersonIds = new FinancialPersonBankAccountService( rockContext )
                                .Queryable()
                                .Where( a => a.AccountNumberSecured == checkMicrHashed )
                                .Select( a => a.PersonAlias.PersonId )
                                .Distinct()
                                .ToList();

                            if ( matchedPersonIds.Count() == 1 )
                            {
                                personAliasId = new PersonAliasService( rockContext )
                                    .GetPrimaryAliasId( matchedPersonIds.First() );
                            }
                        }

                        txn.MICRStatus = MICRStatus.Success;
                        txn.CheckMicrParts = Encryption.EncryptString( string.Format( "{0}_{1}_{2}", routingNum, accountNum, checkNum ) );
                        txn.FinancialPaymentDetail.CurrencyTypeValueId = _currencyTypeCheck;
                        txn.TransactionCode = checkNum;
                    }
                    else
                    {
                        // If account/routing number was NOT included, check for an envelope number, and if provided find first
                        // person with same envelope number (unlike account/routing number, this will automatically select first
                        // person when there are multiple people with same envelope number.
                        string envelopeNum = GetChildElementValue( giftElement, "EnvNum" );
                        if ( !string.IsNullOrWhiteSpace( envelopeNum ) )
                        {
                            int? personId = new AttributeValueService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( v =>
                                   v.AttributeId == envelopeAttributeId &&
                                   v.Value == envelopeNum )
                                .OrderBy( v => v.EntityId )
                                .Select( v => v.EntityId )
                                .FirstOrDefault();

                            if ( personId.HasValue )
                            {
                                personAliasId = new PersonAliasService( rockContext )
                                    .GetPrimaryAliasId( personId.Value );
                            }

                            txn.TransactionCode = envelopeNum;
                        }

                        txn.FinancialPaymentDetail.CurrencyTypeValueId = _currencyTypeCash;
                    }
                    txn.AuthorizedPersonAliasId = personAliasId;

                    // Save any images
                    if ( binaryFileTypeId.HasValue )
                    {
                        SaveImage( txn, GetChildElementValue( giftElement, "CheckImgFront" ), binaryFileTypeId.Value, string.Format( "CheckImageFront_{0}:{1}", batchNo, seqNo ) );
                        SaveImage( txn, GetChildElementValue( giftElement, "CheckImgBack" ), binaryFileTypeId.Value, string.Format( "CheckImageFront_{0}:{1}", batchNo, seqNo ) );
                        SaveImage( txn, GetChildElementValue( giftElement, "EnvImgFront" ), binaryFileTypeId.Value, string.Format( "CheckImageFront_{0}:{1}", batchNo, seqNo ) );
                        SaveImage( txn, GetChildElementValue( giftElement, "EnvImgBack" ), binaryFileTypeId.Value, string.Format( "CheckImageFront_{0}:{1}", batchNo, seqNo ) );
                    }

                    // Loop through the purposes and create the transaction detail (account) records
                    var purposes = giftElement.Element( "Purposes" );
                    if ( purposes != null )
                    {
                        foreach ( var purpose in purposes.Descendants().Where( p => p.Name == "Purpose" ) )
                        {
                            FinancialTransactionDetail txnDetail = null;

                            int? accountId = GetChildElementValue( purpose, "PurposeID" ).AsIntegerOrNull();
                            decimal? amount = GetChildElementValue( purpose, "Amount" ).AsDecimalOrNull();

                            if ( accountId.HasValue && amount.HasValue )
                            {
                                var account = new FinancialAccountService( rockContext ).Get( accountId.Value );
                                if ( account != null )
                                {
                                    txnDetail = new FinancialTransactionDetail();
                                    txnDetail.AccountId = accountId.Value;
                                    txnDetail.Amount = amount.Value;
                                    txn.TransactionDetails.Add( txnDetail );

                                    _totalAmount += amount.Value;
                                }
                            }

                            if ( txnDetail == null )
                            {
                                errorMessages.Add( string.Format( "Batch: {0}; Sequence: {1}; Error: Invalid Account (PurposeId:{2})", batchNo, seqNo, accountId ) );
                            }
                        }
                    }

                    if ( errorMessages.Any() )
                    {
                        return ProcessStatus.Error;
                    }

                    // Save the transaction and update the batch control amount
                    new FinancialTransactionService( rockContext ).Add( txn );
                    rockContext.SaveChanges();

                    return personAliasId.HasValue ? ProcessStatus.Matched : ProcessStatus.Unmatched;
                }

            }

            catch ( Exception ex )
            {
                errorMessages.Add( string.Format( "Batch: {0}; Sequence: {1}; Error: {2}", batchNo, seqNo, ex.Message ) );
                return ProcessStatus.Error;
            }
        }

        /// <summary>
        /// Gets the child element value.
        /// </summary>
        /// <param name="giftElement">The gift element.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private string GetChildElementValue( XElement giftElement, string propertyName)
        {
            var propElement = giftElement.Element( propertyName );
            if ( propElement != null )
            {
                return propElement.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="binaryfileTypeId">The binaryfile type identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="base64String">The base64 string.</param>
        /// <returns></returns>
        private void SaveImage( FinancialTransaction txn, string base64String, int binaryfileTypeId, string fileName )
        {
            if ( !string.IsNullOrWhiteSpace( base64String ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    BinaryFile binaryFile = new BinaryFile();
                    binaryFile.Guid = Guid.NewGuid();
                    binaryFile.IsTemporary = true;
                    binaryFile.BinaryFileTypeId = binaryfileTypeId;
                    binaryFile.MimeType = "image/tiff";
                    binaryFile.FileName = fileName;
                    binaryFile.ContentStream = new MemoryStream( Convert.FromBase64String( base64String ) );

                    var binaryFileService = new BinaryFileService( rockContext );
                    binaryFileService.Add( binaryFile );
                    rockContext.SaveChanges();

                    var transactionImage = new FinancialTransactionImage();
                    transactionImage.BinaryFileId = binaryFile.Id;
                    txn.Images.Add( transactionImage );
                }
            }
        }

        /// <summary>
        /// Decrypts the account information.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string DecryptAccountInformation( string value )
        {
            int i;
            string str = "2FD3C881-3324-4308-BAA0";
            string str1 = "9CD98A4E-5B73-4C4A-997A";
            byte[] num = new byte[32];
            byte[] numArray = new byte[32];
            for ( i = 0; i < str.Length; i++ )
            {
                num[i] = Convert.ToByte( str[i] );
            }
            for ( i = 0; i < str1.Length; i++ )
            {
                numArray[i] = Convert.ToByte( str1[i] );
            }
            RijndaelManaged rijndaelManaged = new RijndaelManaged()
            {
                BlockSize = 256,
                KeySize = 256
            };
            MemoryStream memoryStream = new MemoryStream( Convert.FromBase64String( value ) );
            ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor( num, numArray );
            memoryStream.Position = (long)0;
            return ( new StreamReader( new CryptoStream( memoryStream, cryptoTransform, CryptoStreamMode.Read ) ) ).ReadToEnd();
        }

        /// <summary>
        /// Determines whether the binary file has valid XML.
        /// </summary>
        /// <returns></returns>
        private bool IsXmlValid()
        {
            try
            {
                var xml = GetXml();
                if ( xml == null )
                {
                    ShowError( "Invalid Import File", "Could not read XML file." );
                    return false;
                }

                if ( xml.Root.Name != "Transactions" )
                {
                    ShowError( "Invalid Import File", "The import file does not appear to be a valid teller file." );
                    return false;
                }

                if ( !xml.Root.Descendants().Where( n => n.Name == "Gift" ).Any() )
                {
                    ShowError( "Warning", "The import file does not appear to contain any transactions." );
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ShowError( "Invalid Import File", ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <returns></returns>
        private XDocument GetXml()
        {
            XDocument xml = null;

            if ( _binaryFileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFile = new BinaryFileService( rockContext ).Get( _binaryFileId.Value );
                    if ( binaryFile != null )
                    {
                        using ( var stream = binaryFile.ContentStream )
                        {
                            xml = XDocument.Load( stream );
                        }
                    }
                }
            }

            return xml;
        }

        #region Show Notifications

        /// <summary>
        /// Shows a warning.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string title, string message )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Warning;
            ShowMessage( title, message );
        }

        /// <summary>
        /// Shows a error.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string title, string message )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            ShowMessage( title, message );
        }

        /// <summary>
        /// Shows a message.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowMessage( string title, string message )
        {
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        #endregion

        #endregion

        /// <summary>
        /// Enumeration for tracking transction status
        /// </summary>
        private enum ProcessStatus
        {
            Matched = 0,
            Unmatched = 1,
            Error = 2
        }
    }

}


