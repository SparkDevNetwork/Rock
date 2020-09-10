﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given transaction for editing." )]

    [LinkedPage( "Batch Detail Page", "Page used to view batch.", true, "", "", 0 )]
    [LinkedPage( "Scheduled Transaction Detail Page", "Page used to view scheduled transaction detail.", true, "", "", 1 )]
    [LinkedPage( "Registration Detail Page", "Page used to view an event registration.", true, "", "", 2 )]
    [TextField( "Refund Batch Name Suffix", "The suffix to append to new batch name when refunding transactions. If left blank, the batch name will be the same as the original transaction's batch name.", false, " - Refund", "", 3 )]
    [BooleanField( "Carry Over Account", "Keep Last Used Account when adding multiple transactions in the same session.", true, "", 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE, "Location Types", "The type of location type to display for person (if none are selected all addresses will be included ).", false, true, order: 5 )]
    [BooleanField( "Transaction Source Required", "Determine if Transaction Source should be required.", false, "", 6 )]
    public partial class TransactionDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Constants

        /// <summary>
        /// This value is set as the accountId for "fake" financial transaction details
        /// added to the end of grid sources representing a total/footer row
        /// </summary>
        private const int TotalRowAccountId = int.MinValue;

        #endregion

        #region Properties

        private Control _focusControl = null;
        private List<FinancialTransactionDetail> TransactionDetailsState { get; set; }
        private List<int> TransactionImagesState { get; set; }

        private Dictionary<int, string> _accountNames = null;
        private Dictionary<int, string> AccountNames
        {
            get
            {
                if ( _accountNames == null )
                {
                    _accountNames = new Dictionary<int, string>();
                    new FinancialAccountService( new RockContext() ).Queryable()
                        .OrderBy( a => a.Order )
                        .Select( a => new { a.Id, a.Name } )
                        .ToList()
                        .ForEach( a => _accountNames.Add( a.Id, a.Name ) );
                    _accountNames.Add( TotalRowAccountId, "<strong>Total</strong>" );
                }
                return _accountNames;
            }
        }

        private bool UseSimpleAccountMode
        {
            get
            {
                return ViewState["UseSimpleAccountMode"] as bool? ?? false;
            }
            set
            {
                ViewState["UseSimpleAccountMode"] = value;
                pnlSingleAccount.Visible = value;
                pnlAccounts.Visible = !value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["TransactionDetailsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                TransactionDetailsState = new List<FinancialTransactionDetail>();
            }
            else
            {
                TransactionDetailsState = JsonConvert.DeserializeObject<List<FinancialTransactionDetail>>( json );
            }

            TransactionImagesState = ViewState["TransactionImagesState"] as List<int>;
            if ( TransactionImagesState == null )
            {
                TransactionImagesState = new List<int>();
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccountsView.DataKeyNames = new string[] { "Guid" };
            gAccountsView.ShowActionRow = false;

            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "TransactionId", "PLACEHOLDER" );

            gRefunds.DataKeyNames = new string[] { "Id" };
            gRefunds.ShowActionRow = false;

            var hlRefundCol = gRefunds.Columns[0] as HyperLinkField;
            if ( hlRefundCol != null )
            {
                hlRefundCol.DataNavigateUrlFormatString = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl().Replace( "PLACEHOLDER", "{0}" );
            }

            gRelated.DataKeyNames = new string[] { "Id" };
            gRelated.ShowActionRow = false;

            var hlRelatedCol = gRelated.Columns[0] as HyperLinkField;
            if ( hlRelatedCol != null )
            {
                hlRelatedCol.DataNavigateUrlFormatString = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl().Replace( "PLACEHOLDER", "{0}" );
            }

            gAccountsEdit.DataKeyNames = new string[] { "Guid" };
            gAccountsEdit.ShowActionRow = true;
            gAccountsEdit.Actions.ShowAdd = true;
            gAccountsEdit.Actions.AddClick += gAccountsEdit_AddClick;
            gAccountsEdit.GridRebind += gAccountsEdit_GridRebind;
            gAccountsEdit.RowDataBound += gAccountsEdit_RowDataBound;

            apAccount.DisplayActiveOnly = true;

            AddDynamicColumns();

            //function toggleCheckImages() {
            //    var image1src = $('#<%=imgCheck.ClientID%>').attr("src");
            //    var image2src = $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src");

            //    $('#<%=imgCheck.ClientID%>').attr("src", image2src);
            //    $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src", image1src);
            //}

            string script = @"
    $('.transaction-image-thumbnail').on('click', function() {
        var $primaryImg = $('.transaction-image');
        var primarySrc = $primaryImg.attr('src');
        $primaryImg.attr('src', $(this).attr('src'));
        $(this).attr('src', primarySrc);
    });
";
            ScriptManager.RegisterStartupScript( imgPrimary, imgPrimary.GetType(), "imgPrimarySwap", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TransactionId" ).AsInteger(), PageParameter( "BatchId" ).AsIntegerOrNull() );
            }
            else
            {
                nbErrorMessage.Visible = false;
                nbRefundError.Visible = false;
                ShowDialog();

                if ( pnlEditDetails.Visible )
                {
                    // Add Transaction and Payment Detail attribute controls

                    FinancialTransaction txn;
                    var txnId = hfTransactionId.Value.AsIntegerOrNull();
                    if ( txnId == 0 )
                        txnId = null;

                    // Get the current transaction if there is one
                    if ( txnId.HasValue )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            txn = GetTransaction( hfTransactionId.Value.AsInteger(), rockContext );
                        }
                    }
                    else
                    {
                        txn = new FinancialTransaction();
                        txn.FinancialPaymentDetail = new FinancialPaymentDetail();
                    }

                    // it is possible that an existing transaction doesn't have a FinancialPaymentDetail (usually because it was imported), so create it if is doesn't exist
                    if ( txn.FinancialPaymentDetail == null )
                    {
                        txn.FinancialPaymentDetail = new FinancialPaymentDetail();
                    }

                    // Update the transaction's properties to match what is currently selected on the screen
                    // This allows the shown attributes to change during AutoPostBack events, based on any Qualifiers specified in the attributes
                    txn.FinancialPaymentDetail.CurrencyTypeValueId = dvpCurrencyType.SelectedValueAsInt();

                    txn.LoadAttributes();
                    txn.FinancialPaymentDetail.LoadAttributes();

                    phAttributeEdits.Controls.Clear();
                    Helper.AddEditControls( txn, phAttributeEdits, false );
                    phPaymentAttributeEdits.Controls.Clear();
                    Helper.AddEditControls( txn.FinancialPaymentDetail, phPaymentAttributeEdits, false );
                }
            }

            var txnDetail = new FinancialTransactionDetail();
            txnDetail.LoadAttributes();
            phAccountAttributeEdits.Controls.Clear();
            Helper.AddEditControls( txnDetail, phAccountAttributeEdits, true, mdAccount.ValidationGroup );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _focusControl != null )
            {
                _focusControl.Focus();
            }

            base.OnPreRender( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["TransactionDetailsState"] = JsonConvert.SerializeObject( TransactionDetailsState, Formatting.None, jsonSetting );
            ViewState["TransactionImagesState"] = TransactionImagesState;

            return base.SaveViewState();
        }

        #endregion Control Methods

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var txn = GetTransaction( hfTransactionId.Value.AsInteger(), rockContext );
            if ( txn != null )
            {
                txn.LoadAttributes( rockContext );
                if ( txn.FinancialPaymentDetail != null )
                {
                    txn.FinancialPaymentDetail.LoadAttributes( rockContext );
                }

                ShowEditDetails( txn, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            bool isValid;
            int? savedTransactionId;
            SaveFinancialTransaction( out isValid, out savedTransactionId );

            if ( isValid && savedTransactionId.HasValue )
            {
                // Requery the batch to support EF navigation properties
                var savedTxn = GetTransaction( savedTransactionId.Value );
                if ( savedTxn != null )
                {
                    savedTxn.LoadAttributes();
                    if ( savedTxn.FinancialPaymentDetail != null )
                    {
                        savedTxn.FinancialPaymentDetail.LoadAttributes();
                    }

                    ShowReadOnlyDetails( savedTxn );
                }
            }
        }

        /// <summary>
        /// Saves the financial transaction.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is valid].</param>
        /// <param name="savedTransactionId">The saved transaction identifier.</param>
        private void SaveFinancialTransaction( out bool isValid, out int? savedTransactionId )
        {
            savedTransactionId = null;
            isValid = false;
            var rockContext = new RockContext();

            var txnService = new FinancialTransactionService( rockContext );
            var txnDetailService = new FinancialTransactionDetailService( rockContext );
            var txnImageService = new FinancialTransactionImageService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );

            FinancialTransaction txn = null;

            int? txnId = hfTransactionId.Value.AsIntegerOrNull();
            int? batchId = hfBatchId.Value.AsIntegerOrNull();

            if ( txnId.HasValue )
            {
                txn = txnService.Get( txnId.Value );
            }

            if ( txn == null )
            {
                if ( batchId.HasValue )
                {
                    txn = new FinancialTransaction();
                    txnService.Add( txn );
                    txn.BatchId = batchId;
                    txn.FinancialGatewayId = gpPaymentGateway.SelectedValueAsInt();
                }
                else
                {
                    nbErrorMessage.Title = "Missing Batch Information";
                    nbErrorMessage.Text = "<p>New transactions can only be added to an existing batch. Make sure you have navigated to this page by viewing the details of an existing batch.</p>";
                    nbErrorMessage.Visible = true;
                    return;
                }
            }

            if ( txn != null )
            {
                if ( txn.FinancialPaymentDetail == null )
                {
                    txn.FinancialPaymentDetail = new FinancialPaymentDetail();
                }

                string newPerson = ppAuthorizedPerson.PersonName;

                txn.AuthorizedPersonAliasId = ppAuthorizedPerson.PersonAliasId;
                txn.ShowAsAnonymous = cbShowAsAnonymous.Checked;
                txn.TransactionDateTime = dtTransactionDateTime.SelectedDateTime;
                txn.TransactionTypeValueId = dvpTransactionType.SelectedValue.AsInteger();
                txn.SourceTypeValueId = dvpSourceType.SelectedValueAsInt();
                // DO NOT ALLOW changing a payment gateway once it's already saved.
                //txn.FinancialGatewayId = gpPaymentGateway.SelectedValueAsInt();
                txn.TransactionCode = tbTransactionCode.Text;
                txn.FinancialPaymentDetail.CurrencyTypeValueId = dvpCurrencyType.SelectedValueAsInt();
                if ( IsNonCashTransaction( txn.FinancialPaymentDetail.CurrencyTypeValueId ) )
                {
                    txn.NonCashAssetTypeValueId = dvpNonCashAssetType.SelectedValue.AsIntegerOrNull();
                }
                else
                {
                    txn.NonCashAssetTypeValueId = null;
                }

                txn.FinancialPaymentDetail.CreditCardTypeValueId = dvpCreditCardType.SelectedValueAsInt();

                txn.Summary = tbSummary.Text;
                decimal totalAmount = tbSingleAccountAmount.Text == string.Empty ? TransactionDetailsState.Select( d => d.Amount ).ToList().Sum() : tbSingleAccountAmount.Text.AsDecimal();

                if ( cbIsRefund.Checked && totalAmount > 0 )
                {
                    nbErrorMessage.Title = "Incorrect Refund Amount";
                    nbErrorMessage.Text = "<p>A refund should have a negative amount. Please unselect the refund option, or change amounts to be negative values.</p>";
                    nbErrorMessage.Visible = true;
                    return;
                }

                if ( cbIsRefund.Checked )
                {
                    if ( txn.RefundDetails == null )
                    {
                        txn.RefundDetails = new FinancialTransactionRefund();
                    }
                    txn.RefundDetails.RefundReasonValueId = dvpRefundReasonEdit.SelectedValueAsId();
                    txn.RefundDetails.RefundReasonSummary = tbRefundSummaryEdit.Text;
                }
                else
                {
                    if ( txn.RefundDetails != null )
                    {
                        rockContext.FinancialTransactionRefunds.Remove( txn.RefundDetails );
                    }
                }

                if ( !Page.IsValid || !txn.IsValid )
                {
                    return;
                }

                foreach ( var txnDetail in TransactionDetailsState )
                {
                    if ( !txnDetail.IsValid )
                    {
                        return;
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    // Save the transaction
                    rockContext.SaveChanges();

                    // Delete any transaction details that were removed
                    var txnDetailsInDB = txnDetailService.Queryable().Where( a => a.TransactionId.Equals( txn.Id ) ).ToList();
                    var deletedDetails = from txnDetail in txnDetailsInDB
                                         where !TransactionDetailsState.Select( d => d.Guid ).Contains( txnDetail.Guid )
                                         select txnDetail;
                    deletedDetails.ToList().ForEach( txnDetail =>
                    {
                        txnDetailService.Delete( txnDetail );
                    } );

                    // Save Transaction Details
                    foreach ( var editorTxnDetail in TransactionDetailsState )
                    {
                        // Add or Update the activity type
                        var txnDetail = txn.TransactionDetails.FirstOrDefault( d => d.Guid.Equals( editorTxnDetail.Guid ) );
                        if ( txnDetail == null )
                        {
                            txnDetail = new FinancialTransactionDetail();
                            txnDetail.Guid = editorTxnDetail.Guid;
                            txn.TransactionDetails.Add( txnDetail );
                        }

                        txnDetail.AccountId = editorTxnDetail.AccountId;
                        txnDetail.Amount = UseSimpleAccountMode ? tbSingleAccountAmount.Text.AsDecimal() : editorTxnDetail.Amount;
                        txnDetail.FeeAmount = UseSimpleAccountMode ? tbSingleAccountFeeAmount.Text.AsDecimalOrNull() : editorTxnDetail.FeeAmount;
                        txnDetail.Summary = editorTxnDetail.Summary;

                        if ( editorTxnDetail.AttributeValues != null )
                        {
                            txnDetail.LoadAttributes();
                            txnDetail.AttributeValues = editorTxnDetail.AttributeValues;
                            rockContext.SaveChanges();
                            txnDetail.SaveAttributeValues( rockContext );
                        }
                    }

                    // Delete any transaction images that were removed
                    var orphanedBinaryFileIds = new List<int>();
                    var txnImagesInDB = txnImageService.Queryable().Where( a => a.TransactionId.Equals( txn.Id ) ).ToList();
                    foreach ( var txnImage in txnImagesInDB.Where( i => !TransactionImagesState.Contains( i.BinaryFileId ) ) )
                    {
                        orphanedBinaryFileIds.Add( txnImage.BinaryFileId );
                        txnImageService.Delete( txnImage );
                    }

                    // Save Transaction Images
                    int imageOrder = 0;
                    foreach ( var binaryFileId in TransactionImagesState )
                    {
                        // Add or Update the activity type
                        var txnImage = txnImagesInDB.FirstOrDefault( i => i.BinaryFileId == binaryFileId );
                        if ( txnImage == null )
                        {
                            txnImage = new FinancialTransactionImage();
                            txnImage.TransactionId = txn.Id;
                            txn.Images.Add( txnImage );
                        }

                        txnImage.BinaryFileId = binaryFileId;
                        txnImage.Order = imageOrder;
                        imageOrder++;
                    }

                    rockContext.SaveChanges();

                    // Make sure updated binary files are not temporary
                    foreach ( var binaryFile in binaryFileService.Queryable().Where( f => TransactionImagesState.Contains( f.Id ) ) )
                    {
                        binaryFile.IsTemporary = false;
                    }

                    // Delete any orphaned images
                    foreach ( var binaryFile in binaryFileService.Queryable().Where( f => orphanedBinaryFileIds.Contains( f.Id ) ) )
                    {
                        binaryFileService.Delete( binaryFile );
                    }

                    // Update any attributes
                    txn.LoadAttributes( rockContext );
                    txn.FinancialPaymentDetail.LoadAttributes( rockContext );
                    Helper.GetEditValues( phAttributeEdits, txn );
                    Helper.GetEditValues( phPaymentAttributeEdits, txn.FinancialPaymentDetail );

                    rockContext.SaveChanges();
                    txn.SaveAttributeValues( rockContext );
                    txn.FinancialPaymentDetail.SaveAttributeValues( rockContext );
                } );

                // Then refresh history block
                RockPage.UpdateBlocks( "~/Blocks/Core/HistoryLog.ascx" );

                // Save selected options to session state in order to prefill values for next added txn
                Session["NewTxnDefault_BatchId"] = txn.BatchId;
                Session["NewTxnDefault_TransactionDateTime"] = txn.TransactionDateTime;
                Session["NewTxnDefault_TransactionType"] = txn.TransactionTypeValueId;
                Session["NewTxnDefault_NonCashAssetType"] = txn.NonCashAssetTypeValueId;
                Session["NewTxnDefault_SourceType"] = txn.SourceTypeValueId;
                Session["NewTxnDefault_CurrencyType"] = txn.FinancialPaymentDetail.CurrencyTypeValueId;
                Session["NewTxnDefault_CreditCardType"] = txn.FinancialPaymentDetail.CreditCardTypeValueId;
                if ( TransactionDetailsState.Count() == 1 )
                {
                    Session["NewTxnDefault_Account"] = TransactionDetailsState.First().AccountId;
                }
                else
                {
                    Session.Remove( "NewTxnDefault_Account" );
                }

                isValid = true;
                savedTransactionId = txn.Id;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveThenAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveThenAdd_Click( object sender, EventArgs e )
        {
            bool isValid;
            int? savedTransactionId;
            SaveFinancialTransaction( out isValid, out savedTransactionId );

            if ( isValid )
            {
                ShowDetail( 0, hfBatchId.Value.AsIntegerOrNull() );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveThenViewBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveThenViewBatch_Click( object sender, EventArgs e )
        {
            bool isValid;
            int? savedTransactionId;
            SaveFinancialTransaction( out isValid, out savedTransactionId );

            if ( isValid )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "BatchId", hfBatchId.Value );
                NavigateToLinkedPage( "BatchDetailPage", qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int txnId = hfTransactionId.ValueAsInt();
            if ( txnId != 0 )
            {
                var txn = GetTransaction( txnId );
                if ( txn != null )
                {
                    txn.LoadAttributes();
                    txn.FinancialPaymentDetail.LoadAttributes();
                    ShowReadOnlyDetails( txn );
                }
            }
            else
            {
                Dictionary<string, string> pageParams = new Dictionary<string, string>();

                if ( !string.IsNullOrWhiteSpace( PageParameter( "BatchId" ) ) )
                {
                    pageParams.Add( "BatchId", PageParameter( "BatchId" ) );
                }

                NavigateToParentPage( pageParams );
            }
        }

        protected void lbRefundTransaction_Click( object sender, EventArgs e )
        {
            ShowRefundDialog();
        }

        protected void lbAddTransaction_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> pageParams = new Dictionary<string, string>();

            pageParams.Add( "BatchId", PageParameter( "BatchId" ) );
            pageParams.Add( "TransactionId", "0" );
            NavigateToPage( RockPage.Guid, pageParams );

        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int txnId = hfTransactionId.ValueAsInt();
            if ( txnId != 0 )
            {
                ShowReadOnlyDetails( GetTransaction( txnId ) );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAuthorizedPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAuthorizedPerson_SelectPerson( object sender, EventArgs e )
        {
            _focusControl = ppAuthorizedPerson;
        }

        /// <summary>
        /// Handles the Click event of the lbShowMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowMore_Click( object sender, EventArgs e )
        {
            if ( TransactionDetailsState.Count() == 1 )
            {
                TransactionDetailsState.First().Amount = tbSingleAccountAmount.Text.AsDecimal();
                tbSingleAccountAmount.Text = string.Empty;
                UseSimpleAccountMode = false;
                BindAccounts();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCurrencyType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCurrencyType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetCreditCardVisibility();
            SetNonCashAssetTypeVisibility();
            _focusControl = dvpCurrencyType;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gAccountsEdit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var account = ( FinancialTransactionDetail ) e.Row.DataItem;

            // If this is the total row
            if ( account.AccountId == TotalRowAccountId )
            {
                // disable the row select on each column
                foreach ( TableCell cell in e.Row.Cells )
                {
                    cell.RemoveCssClass( "grid-select-cell" );
                }
            }

            // If account is associated with an entity (i.e. registration), or this is the total row do not allow it to be deleted
            if ( account.EntityTypeId.HasValue || account.AccountId == TotalRowAccountId )
            {
                // Hide the edit button if this is the total row
                if ( account.AccountId == TotalRowAccountId )
                {
                    var editCell = GetEditCell( e.Row.Cells );
                    var editBtn = editCell.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();

                    if ( editBtn != null )
                    {
                        editBtn.Visible = false;
                    }
                }

                // Hide the delete button
                var deleteCell = GetDeleteCell( e.Row.Cells );
                var deleteBtn = deleteCell.ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();

                if ( deleteBtn != null )
                {
                    deleteBtn.Visible = false;
                }
            }
        }

        #endregion

        #region Account Transaction Details

        /// <summary>
        /// Handles the AddClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountsEdit_AddClick( object sender, EventArgs e )
        {
            ShowAccountDialog( Guid.NewGuid() );
        }

        /// <summary>
        /// Handles the EditClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_EditClick( object sender, RowEventArgs e )
        {
            Guid? guid = e.RowKeyValue.ToString().AsGuidOrNull();
            if ( guid.HasValue )
            {
                ShowAccountDialog( guid.Value );
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_DeleteClick( object sender, RowEventArgs e )
        {
            Guid? guid = e.RowKeyValue.ToString().AsGuidOrNull();
            if ( guid.HasValue )
            {
                var txnDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( txnDetail != null )
                {
                    TransactionDetailsState.Remove( txnDetail );
                }

                BindAccounts();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountsEdit_GridRebind( object sender, EventArgs e )
        {
            BindAccounts();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAccount_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfAccountGuid.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var txnDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( txnDetail == null )
                {
                    txnDetail = new FinancialTransactionDetail();
                    TransactionDetailsState.Add( txnDetail );
                }
                txnDetail.AccountId = apAccount.SelectedValue.AsInteger();
                txnDetail.Amount = tbAccountAmount.Text.AsDecimal();
                txnDetail.FeeAmount = tbAccountFeeAmount.Text.AsDecimalOrNull();
                txnDetail.Summary = tbAccountSummary.Text;

                txnDetail.LoadAttributes();
                Rock.Attribute.Helper.GetEditValues( phAccountAttributeEdits, txnDetail );
                foreach ( var attributeValue in txnDetail.AttributeValues )
                {
                    txnDetail.SetAttributeValue( attributeValue.Key, attributeValue.Value.Value );
                }

                BindAccounts();
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdRefund control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdRefund_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var txnService = new FinancialTransactionService( rockContext );
                var txn = txnService.Get( hfTransactionId.Value.AsInteger() );

                string errorMessage = string.Empty;
                bool process = cbProcess.Visible && cbProcess.Checked;

                var refundTxn = txnService.ProcessRefund( txn, tbRefundAmount.Text.AsDecimal(), dvpRefundReason.SelectedValueAsInt(), tbRefundSummary.Text, process, GetAttributeValue( "RefundBatchNameSuffix" ), out errorMessage );
                if ( refundTxn != null )
                {
                    rockContext.SaveChanges();

                    var updatedTxn = GetTransaction( txn.Id );
                    if ( updatedTxn != null )
                    {
                        updatedTxn.LoadAttributes( rockContext );
                        updatedTxn.FinancialPaymentDetail.LoadAttributes( rockContext );
                        ShowReadOnlyDetails( updatedTxn );
                    }

                    HideDialog();
                }
                else
                {
                    nbRefundError.Title = "Transaction Error";
                    nbRefundError.Text = string.Format( "<p>{0}</p>", errorMessage );
                    nbRefundError.Visible = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbIsRefund control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIsRefund_CheckedChanged( object sender, EventArgs e )
        {
            dvpRefundReasonEdit.Visible = cbIsRefund.Checked;
            tbRefundSummaryEdit.Visible = cbIsRefund.Checked;
        }

        #endregion

        #region Image Details

        /// <summary>
        /// Handles the ItemDataBound event of the dlImages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataListItemEventArgs"/> instance containing the event data.</param>
        protected void dlImages_ItemDataBound( object sender, DataListItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var imgupImage = e.Item.FindControl( "imgupImage" ) as Rock.Web.UI.Controls.ImageUploader;
                if ( imgupImage != null )
                {
                    imgupImage.BinaryFileId = ( int ) e.Item.DataItem;
                }
            }
        }


        /// <summary>
        /// Handles the ImageRemoved event of the imgupImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageUploaderEventArgs"/> instance containing the event data.</param>
        protected void imgupImage_ImageRemoved( object sender, ImageUploaderEventArgs e )
        {
            if ( e.BinaryFileId.HasValue )
            {
                TransactionImagesState.Remove( e.BinaryFileId.Value );
                BindImages();
            }
        }

        /// <summary>
        /// Handles the ImageUploaded event of the imgupImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageUploaderEventArgs"/> instance containing the event data.</param>
        protected void imgupImage_ImageUploaded( object sender, ImageUploaderEventArgs e )
        {
            if ( e.BinaryFileId.HasValue )
            {
                TransactionImagesState.Add( e.BinaryFileId.Value );
                BindImages();
            }
        }

        #endregion

        #endregion Events

        #region Methods

        private void AddDynamicColumns()
        {
            // Remove attribute columns
            foreach ( var attributeColumn in gAccountsView.Columns.OfType<AttributeField>().ToList() )
            {
                gAccountsView.Columns.Remove( attributeColumn );
            }
            foreach ( var attributeColumn in gAccountsEdit.Columns.OfType<AttributeField>().ToList() )
            {
                gAccountsEdit.Columns.Remove( attributeColumn );
            }

            var editColumn = gAccountsEdit.Columns.OfType<EditField>().FirstOrDefault();
            if ( editColumn != null )
            {
                gAccountsEdit.Columns.Remove( editColumn );
            }

            var deleteColumn = gAccountsEdit.Columns.OfType<DeleteField>().FirstOrDefault();
            if ( deleteColumn != null )
            {
                gAccountsEdit.Columns.Remove( deleteColumn );
            }


            // Add attribute columns
            int entityTypeId = new FinancialTransactionDetail().TypeId;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    a.EntityTypeQualifierColumn == "" &&
                    a.EntityTypeQualifierValue == "" )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AttributeField boundField = new AttributeField();
                boundField.DataField = attribute.Key;
                boundField.AttributeId = attribute.Id;
                boundField.HeaderText = attribute.Name;

                var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                if ( attributeCache != null )
                {
                    boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                }

                gAccountsView.Columns.Add( boundField );
                gAccountsEdit.Columns.Add( boundField );
            }

            var editField = new EditField();
            editField.Click += gAccountsEdit_EditClick;
            gAccountsEdit.Columns.Add( editField );

            var deleteField = new DeleteField();
            deleteField.Click += gAccountsEdit_DeleteClick;
            gAccountsEdit.Columns.Add( deleteField );
        }

        /// <summary>
        /// Navigates to the transaction in the list.
        /// </summary>
        private void ShowNavigationButton( int transactionId, int? batchId )
        {
            if ( batchId == null || !batchId.HasValue || batchId == 0 )
            {
                lbBack.Visible = false;
                lbNext.Visible = false;
                return;
            }

            lbBack.Visible = true;
            lbNext.Visible = true;
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var transactionsToMatch = financialTransactionService.Queryable()
                .Where( a => a.BatchId == batchId )
                .Select( a => a.Id )
                .ToList();

            var nextFinancialTransaction = transactionsToMatch.Where( a => a > transactionId ).Take( 1 ).FirstOrDefault();
            var backFinancialTransaction = transactionsToMatch.Where( a => a < transactionId ).LastOrDefault();

            if ( nextFinancialTransaction != default( int ) )
            {
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "BatchId", hfBatchId.Value );
                qryParam.Add( "TransactionId", nextFinancialTransaction.ToStringSafe() );
                lbNext.NavigateUrl = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl();
            }
            else
            {
                lbNext.AddCssClass( "disabled" );
            }

            if ( backFinancialTransaction != default( int ) )
            {
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "BatchId", hfBatchId.Value );
                qryParam.Add( "TransactionId", backFinancialTransaction.ToStringSafe() );
                lbBack.NavigateUrl = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl();
            }
            else
            {
                lbBack.AddCssClass( "disabled" );
            }
        }

        /// <summary>
        /// Gets the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private FinancialTransaction GetTransaction( int transactionId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var txn = new FinancialTransactionService( rockContext )
                .Queryable( "AuthorizedPersonAlias.Person,TransactionTypeValue,NonCashAssetTypeValue,SourceTypeValue,FinancialGateway,FinancialPaymentDetail.CurrencyTypeValue,TransactionDetails,ScheduledTransaction,ProcessedByPersonAlias.Person" )
                .Where( t => t.Id == transactionId )
                .FirstOrDefault();
            return txn;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public void ShowDetail( int transactionId )
        {
            ShowDetail( transactionId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public void ShowDetail( int transactionId, int? batchId )
        {
            FinancialTransaction txn = null;

            bool editAllowed = UserCanEdit;
            bool refundAllowed = false;

            var rockContext = new RockContext();

            FinancialBatch batch = null;
            if ( batchId.HasValue )
            {
                batch = new FinancialBatchService( rockContext ).Get( batchId.Value );
            }

            BindDropdowns( rockContext );

            if ( !transactionId.Equals( 0 ) )
            {
                txn = GetTransaction( transactionId, rockContext );
                if ( !editAllowed && txn != null )
                {
                    editAllowed = txn.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
                refundAllowed = txn != null && txn.IsAuthorized( "Refund", CurrentPerson );
            }

            bool batchEditAllowed = true;

            if ( txn == null )
            {
                txn = new FinancialTransaction { Id = 0 };
                txn.FinancialPaymentDetail = new FinancialPaymentDetail();
                txn.BatchId = batchId;

                // Hide processor fields when adding a new transaction
                gpPaymentGateway.Visible = false;

                // Set values based on previously saved txn values
                int prevBatchId = Session["NewTxnDefault_BatchId"] as int? ?? 0;
                if ( prevBatchId == batchId )
                {
                    txn.TransactionDateTime = Session["NewTxnDefault_TransactionDateTime"] as DateTime?;
                    txn.TransactionTypeValueId = Session["NewTxnDefault_TransactionType"] as int? ?? 0;
                    txn.SourceTypeValueId = Session["NewTxnDefault_SourceType"] as int?;
                    txn.FinancialPaymentDetail.CurrencyTypeValueId = Session["NewTxnDefault_CurrencyType"] as int?;
                    if ( IsNonCashTransaction( txn.FinancialPaymentDetail.CurrencyTypeValueId ) )
                    {
                        txn.NonCashAssetTypeValueId = Session["NewTxnDefault_NonCashAssetType"] as int?;
                    }
                    txn.FinancialPaymentDetail.CreditCardTypeValueId = Session["NewTxnDefault_CreditCardType"] as int?;
                    if ( this.GetAttributeValue( "CarryOverAccount" ).AsBoolean() )
                    {
                        int? accountId = Session["NewTxnDefault_Account"] as int?;
                        if ( accountId.HasValue )
                        {
                            var txnDetail = new FinancialTransactionDetail();
                            txnDetail.AccountId = accountId.Value;
                            txn.TransactionDetails.Add( txnDetail );
                        }
                    }
                }
            }
            else
            {
                gpPaymentGateway.Visible = true;

                if ( txn.Batch != null && txn.Batch.Status == BatchStatus.Closed )
                {
                    batchEditAllowed = false;
                }
            }

            hfTransactionId.Value = txn.Id.ToString();
            hfBatchId.Value = batchId.HasValue ? batchId.Value.ToString() : string.Empty;
            ShowNavigationButton( transactionId, batchId );

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            lbEdit.Visible = editAllowed && batchEditAllowed;
            lbRefund.Visible = refundAllowed && txn.RefundDetails == null;
            lbAddTransaction.Visible = editAllowed && batch != null && batch.Status != BatchStatus.Closed;

            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialTransaction.FriendlyTypeName );
            }
            else
            {
                if ( !batchEditAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = string.Format( "<strong>Note</strong> Because this {0} belongs to a batch that is closed, editing is not enabled.", FinancialTransaction.FriendlyTypeName );
                }
            }

            txn.LoadAttributes( rockContext );
            txn.FinancialPaymentDetail.LoadAttributes( rockContext );

            if ( readOnly )
            {
                ShowReadOnlyDetails( txn );
            }
            else
            {
                if ( txn.Id > 0 )
                {
                    ShowReadOnlyDetails( txn );
                }
                else
                {
                    ShowEditDetails( txn, rockContext );
                }
            }

            lbSave.Visible = !readOnly;
        }

        private bool IsNonCashTransaction( int? CurrencyTypeId )
        {
            if ( CurrencyTypeId == null )
            {
                return false;
            }

            var nonCashCurrencyType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH );
            if ( nonCashCurrencyType == null )
            {
                return false;
            }

            return ( CurrencyTypeId == nonCashCurrencyType.Id );
        }

        /// <summary>
        /// Shows the read only details.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowReadOnlyDetails( FinancialTransaction txn )
        {
            SetEditMode( false );

            if ( txn != null && txn.Id > 0 )
            {
                hfTransactionId.Value = txn.Id.ToString();

                SetHeadingInfo( txn );

                string rockUrlRoot = ResolveRockUrl( "/" );

                lAuthorizedPerson.Text = new DescriptionList().Add( "Person", ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null ) ? txn.AuthorizedPersonAlias.Person.GetAnchorTag( rockUrlRoot ) : string.Empty ).Html;

                var detailsLeft = new DescriptionList()
                    .Add( "Date/Time", txn.TransactionDateTime.HasValue ? txn.TransactionDateTime.Value.ToString( "g" ) : string.Empty );

                if ( txn.Batch != null )
                {
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "BatchId", txn.Batch.Id.ToString() );
                    string url = LinkedPageUrl( "BatchDetailPage", qryParam );
                    detailsLeft.Add( "Batch", !string.IsNullOrWhiteSpace( url ) ?
                        string.Format( "<a href='{0}'>{1}</a>", url, txn.Batch.Name ) :
                        txn.Batch.Name );
                }

                if ( txn.NonCashAssetTypeValue != null )
                {
                    detailsLeft.Add( "Non-Cash Asset Type", txn.NonCashAssetTypeValue != null ? txn.NonCashAssetTypeValue.Value : string.Empty );
                }

                detailsLeft.Add( "Source", txn.SourceTypeValue != null ? txn.SourceTypeValue.Value : string.Empty );
                detailsLeft.Add( "Transaction Code", txn.TransactionCode );

                if ( txn.FinancialGateway != null && txn.FinancialGateway.EntityType != null )
                {
                    string fgName = txn.FinancialGateway.Name.IsNotNullOrWhiteSpace()
                        ? txn.FinancialGateway.Name
                        : Rock.Financial.GatewayContainer.GetComponentName( txn.FinancialGateway.EntityType.Name );

                    detailsLeft.Add( "Payment Gateway", fgName );
                }

                detailsLeft.Add( "Foreign Key", txn.ForeignKey );

                if ( txn.ScheduledTransaction != null )
                {
                    var text = txn.ScheduledTransaction.GatewayScheduleId.IsNullOrWhiteSpace() ?
                        txn.ScheduledTransactionId.ToString() :
                        txn.ScheduledTransaction.GatewayScheduleId;

                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "ScheduledTransactionId", txn.ScheduledTransaction.Id.ToString() );
                    string url = LinkedPageUrl( "ScheduledTransactionDetailPage", qryParam );

                    detailsLeft.Add( "Scheduled Transaction Id", !string.IsNullOrWhiteSpace( url ) ?
                        string.Format( "<a href='{0}'>{1}</a>", url, text ) :
                        text );
                }

                if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValue != null )
                {
                    var paymentMethodDetails = new DescriptionList();

                    var currencyType = txn.FinancialPaymentDetail.CurrencyTypeValue;
                    if ( currencyType.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ) )
                    {
                        // Credit Card
                        paymentMethodDetails.Add( "Type", currencyType.Value + ( txn.FinancialPaymentDetail.CreditCardTypeValue != null ? ( " - " + txn.FinancialPaymentDetail.CreditCardTypeValue.Value ) : string.Empty ) );
                        paymentMethodDetails.Add( "Name on Card", txn.FinancialPaymentDetail.NameOnCard.ToStringSafe().Trim() );
                        paymentMethodDetails.Add( "Account Number", txn.FinancialPaymentDetail.AccountNumberMasked );
                        paymentMethodDetails.Add( "Expires", txn.FinancialPaymentDetail.ExpirationDate );
                    }
                    else
                    {
                        // ACH
                        paymentMethodDetails.Add( "Type", currencyType.Value );
                        paymentMethodDetails.Add( "Account Number", txn.FinancialPaymentDetail.AccountNumberMasked );
                    }

                    detailsLeft.Add( "Payment Method", paymentMethodDetails.GetFormattedList( "{0}: {1}" ).AsDelimited( "<br/>" ) );
                }
                else
                {
                    detailsLeft.Add( "Payment Method", "<div class='alert alert-warning'>No Payment Information found. This could be due to transaction that was imported from external system.</div>" );
                }

                var registrationEntityType = EntityTypeCache.Get( typeof( Rock.Model.Registration ) );
                if ( registrationEntityType != null )
                {
                    var registrationIds = txn.TransactionDetails
                        .Where( d => d.EntityTypeId == registrationEntityType.Id )
                        .Select( d => d.EntityId )
                        .Distinct()
                        .ToList();

                    if ( registrationIds.Any() )
                    {
                        var registrationLinks = new List<string>();
                        using ( var rockContext = new RockContext() )
                        {
                            foreach ( var registration in new RegistrationService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( r =>
                                    r.RegistrationInstance != null &&
                                    r.RegistrationInstance.RegistrationTemplate != null &&
                                    registrationIds.Contains( r.Id ) ) )
                            {
                                var qryParam = new Dictionary<string, string>();
                                qryParam.Add( "RegistrationId", registration.Id.ToString() );
                                registrationLinks.Add( string.Format( "<a href='{0}'>{1} - {2}</a>",
                                    LinkedPageUrl( "RegistrationDetailPage", qryParam ),
                                    registration.RegistrationInstance.RegistrationTemplate.Name,
                                    registration.RegistrationInstance.Name ) );
                            }
                        }
                        if ( registrationLinks.Any() )
                        {
                            detailsLeft.Add( "Registration", registrationLinks.AsDelimited( "<br/>" ) );
                        }
                    }
                }

                detailsLeft.Add( "Summary", txn.Summary.ConvertCrLfToHtmlBr() );

                if ( txn.RefundDetails != null )
                {
                    var refundTxt = "Yes";
                    if ( txn.RefundDetails.OriginalTransaction != null )
                    {
                        var qryParam = new Dictionary<string, string>();
                        qryParam.Add( "TransactionId", txn.RefundDetails.OriginalTransaction.Id.ToStringSafe() );
                        string url = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl();
                        refundTxt = string.Format( "Yes (<a href='{0}'>Original Transaction</a>)", url );
                    }
                    detailsLeft.Add( "Refund", refundTxt );

                    if ( txn.RefundDetails.RefundReasonValue != null )
                    {
                        detailsLeft.Add( "Refund Reason", txn.RefundDetails.RefundReasonValue.Value );
                    }
                    detailsLeft.Add( "Refund Summary", txn.RefundDetails.RefundReasonSummary );
                }
                if ( !string.IsNullOrWhiteSpace( txn.Status ) )
                {
                    string status = txn.Status;
                    if ( !string.IsNullOrWhiteSpace( txn.StatusMessage ) )
                    {
                        status += string.Format( "<br/><small>{0}</small>", txn.StatusMessage.ConvertCrLfToHtmlBr() );
                    }
                    detailsLeft.Add( "Status", status );
                }

                var modified = new StringBuilder();
                ;
                if ( txn.CreatedByPersonAlias != null && txn.CreatedByPersonAlias.Person != null && txn.CreatedDateTime.HasValue )
                {
                    modified.AppendFormat( "Created by {0} on {1} at {2}<br/>", txn.CreatedByPersonAlias.Person.GetAnchorTag( rockUrlRoot ),
                        txn.CreatedDateTime.Value.ToShortDateString(), txn.CreatedDateTime.Value.ToShortTimeString() );
                }
                if ( txn.ProcessedByPersonAlias != null && txn.ProcessedByPersonAlias.Person != null && txn.ProcessedDateTime.HasValue )
                {
                    modified.AppendFormat( "Processed by {0} on {1} at {2}<br/>", txn.ProcessedByPersonAlias.Person.GetAnchorTag( rockUrlRoot ),
                        txn.ProcessedDateTime.Value.ToShortDateString(), txn.ProcessedDateTime.Value.ToShortTimeString() );
                }
                if ( txn.ModifiedByPersonAlias != null && txn.ModifiedByPersonAlias.Person != null && txn.ModifiedDateTime.HasValue )
                {
                    modified.AppendFormat( "Last Modified by {0} on {1} at {2}<br/>", txn.ModifiedByPersonAlias.Person.GetAnchorTag( rockUrlRoot ),
                        txn.ModifiedDateTime.Value.ToShortDateString(), txn.ModifiedDateTime.Value.ToShortTimeString() );
                }
                detailsLeft.Add( "Updates", modified.ToString() );

                lDetailsLeft.Text = detailsLeft.Html;

                var authorizedPerson = txn.AuthorizedPersonAlias != null ? txn.AuthorizedPersonAlias.Person : null;
                if ( authorizedPerson != null )
                {
                    var campusDescription = new DescriptionList();
                    var associatedCampusIds = authorizedPerson.GetCampusIds();
                    if ( associatedCampusIds.Any() )
                    {
                        var campusNames = new List<string>();
                        using ( var rockContext = new RockContext() )
                        {
                            foreach ( var campus in new CampusService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( r =>
                                    associatedCampusIds.Contains( r.Id ) ) )
                            {
                                campusNames.Add( campus.Name );
                            }
                        }
                        if ( campusNames.Any() )
                        {
                            campusDescription.Add( "Campus".PluralizeIf( campusNames.Count > 1 ), campusNames.AsDelimited( "<br/>" ) );
                        }
                        lCampus.Text = campusDescription.Html;
                    }

                    var addressDescription = new DescriptionList();
                    var groupLocations = authorizedPerson.GetFamilies().SelectMany( a => a.GroupLocations );

                    var locationTypeValueIdList = GetAttributeValue( "LocationTypes" ).SplitDelimitedValues().AsGuidList()
                                            .Select( a => DefinedValueCache.Get( a ) )
                                            .Where( a => a != null )
                                            .Select( a => a.Id )
                                            .ToList();
                    if ( locationTypeValueIdList.Any() )
                    {
                        groupLocations = groupLocations.Where( a => a.GroupLocationTypeValueId.HasValue && locationTypeValueIdList.Contains( a.GroupLocationTypeValueId.Value ) );
                    }

                    if ( groupLocations.Any() )
                    {
                        rptAddresses.DataSource = groupLocations.OrderByDescending( l => l.CreatedDateTime ).ToList();
                        rptAddresses.DataBind();
                    }
                }

                var accounts = txn.TransactionDetails.ToList();
                var totalFeeAmount = txn.TotalFeeAmount;
                var hasFeeInfo = totalFeeAmount.HasValue;

                accounts.Add( new FinancialTransactionDetail
                {
                    AccountId = TotalRowAccountId,
                    FeeAmount = totalFeeAmount,
                    Amount = txn.TotalAmount
                } );

                var feeColumn = GetFeeColumn( gAccountsView );
                feeColumn.Visible = hasFeeInfo;

                gAccountsView.DataSource = accounts;
                gAccountsView.DataBind();

                if ( txn.Images.Any() )
                {
                    pnlImages.Visible = true;

                    var primaryImage = txn.Images
                        .OrderBy( i => i.Order )
                        .FirstOrDefault();
                    imgPrimary.ImageUrl = string.Format( "~/GetImage.ashx?id={0}", primaryImage.BinaryFileId );

                    rptrImages.DataSource = txn.Images
                        .Where( i => !i.Id.Equals( primaryImage.Id ) )
                        .OrderBy( i => i.Order )
                        .ToList();
                    rptrImages.DataBind();
                }
                else
                {
                    pnlImages.Visible = false;
                }

                var refunds = txn.Refunds
                    .Where( r =>
                        r.FinancialTransaction != null &&
                        r.FinancialTransaction.TransactionDateTime.HasValue )
                    .OrderBy( r => r.FinancialTransaction.TransactionDateTime.Value )
                    .ToList();
                if ( refunds.Any() )
                {
                    pnlRefunds.Visible = true;
                    gRefunds.DataSource = refunds
                        .Select( r => new
                        {
                            Id = r.FinancialTransaction.Id,
                            TransactionDateTime = r.FinancialTransaction.TransactionDateTime.Value.ToShortDateString() + " " +
                                r.FinancialTransaction.TransactionDateTime.Value.ToShortTimeString(),
                            TransactionCode = r.FinancialTransaction.TransactionCode,
                            r.RefundReasonValue,
                            r.RefundReasonSummary,
                            TotalAmount = r.FinancialTransaction.TotalAmount
                        } )
                        .ToList();
                    gRefunds.DataBind();
                }
                else
                {
                    pnlRefunds.Visible = false;
                }

                if ( !string.IsNullOrWhiteSpace( txn.TransactionCode ) )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        if ( txn.FinancialGatewayId.HasValue )
                        {
                            var relatedTxns = new FinancialTransactionService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( t =>
                                    t.FinancialGatewayId.HasValue &&
                                    t.FinancialGatewayId.Value == txn.FinancialGatewayId.Value &&
                                    t.TransactionCode == txn.TransactionCode &&
                                    t.AuthorizedPersonAliasId == txn.AuthorizedPersonAliasId &&
                                    t.Id != txn.Id &&
                                    t.TransactionDateTime.HasValue )
                                .OrderBy( t => t.TransactionDateTime.Value )
                                .ToList();
                            if ( relatedTxns.Any() )
                            {
                                pnlRelated.Visible = true;
                                gRelated.DataSource = relatedTxns
                                    .Select( t => new
                                    {
                                        Id = t.Id,
                                        TransactionDateTime = t.TransactionDateTime.Value.ToShortDateString() + " " +
                                            t.TransactionDateTime.Value.ToShortTimeString(),
                                        TransactionCode = t.TransactionCode,
                                        TotalAmount = t.TotalAmount
                                    } )
                                    .ToList();
                                gRelated.DataBind();
                            }
                            else
                            {
                                pnlRelated.Visible = false;
                            }
                        }
                        else
                        {
                            pnlRelated.Visible = false;
                        }
                    }
                }

                Helper.AddDisplayControls( txn, Helper.GetAttributeCategories( txn, false, false ), phAttributes, null, false );
                Helper.AddDisplayControls( txn.FinancialPaymentDetail, Helper.GetAttributeCategories( txn.FinancialPaymentDetail, false, false ), phAttributes, null, false );
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( FinancialTransaction.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Formats the type of the address.
        /// </summary>
        /// <param name="addressType">Type of the address.</param>
        /// <returns></returns>
        protected string FormatAddressType( object addressType )
        {
            string type = addressType != null ? addressType.ToString() : "Unknown";
            return type.EndsWith( "Address", StringComparison.CurrentCultureIgnoreCase ) ? type : type + " Address";
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowEditDetails( FinancialTransaction txn, RockContext rockContext )
        {
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript", @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });", true );

            if ( txn != null )
            {
                hfTransactionId.Value = txn.Id.ToString();

                SetHeadingInfo( txn );

                SetEditMode( true );

                if ( txn.AuthorizedPersonAlias != null )
                {
                    ppAuthorizedPerson.SetValue( txn.AuthorizedPersonAlias.Person );
                }
                else
                {
                    ppAuthorizedPerson.SetValue( null );
                }

                BindDropdowns( rockContext );

                cbShowAsAnonymous.Checked = txn.ShowAsAnonymous;
                dtTransactionDateTime.SelectedDateTime = txn.TransactionDateTime;
                dvpTransactionType.SetValue( txn.TransactionTypeValueId );
                dvpSourceType.Required = this.GetAttributeValue( "TransactionSourceRequired" ).AsBoolean();
                dvpSourceType.SetValue( txn.SourceTypeValueId );
                gpPaymentGateway.SetValue( txn.FinancialGatewayId );
                tbTransactionCode.Text = txn.TransactionCode;
                dvpCurrencyType.SetValue( txn.FinancialPaymentDetail != null ? txn.FinancialPaymentDetail.CurrencyTypeValueId : ( int? ) null );

                if ( ( txn.FinancialPaymentDetail != null ) && ( IsNonCashTransaction( txn.FinancialPaymentDetail.CurrencyTypeValueId ) ) )
                {
                    dvpNonCashAssetType.SetValue( txn.NonCashAssetTypeValueId );
                }
                SetNonCashAssetTypeVisibility();

                dvpCreditCardType.SetValue( txn.FinancialPaymentDetail != null ? txn.FinancialPaymentDetail.CreditCardTypeValueId : ( int? ) null );
                SetCreditCardVisibility();

                if ( txn.Id == 0 )
                {
                    gpPaymentGateway.Enabled = true;
                    btnSaveThenAdd.Visible = true;
                    btnSaveThenViewBatch.Visible = !string.IsNullOrEmpty( LinkedPageUrl( "BatchDetailPage" ) ) && txn.BatchId.HasValue;
                }
                else
                {
                    gpPaymentGateway.Enabled = false;
                    btnSaveThenAdd.Visible = false;
                    btnSaveThenViewBatch.Visible = false;
                }

                if ( txn.RefundDetails != null )
                {
                    cbIsRefund.Checked = true;
                    dvpRefundReasonEdit.Visible = true;
                    dvpRefundReasonEdit.SetValue( txn.RefundDetails.RefundReasonValueId );
                    tbRefundSummaryEdit.Visible = true;
                    tbRefundSummaryEdit.Text = txn.RefundDetails.RefundReasonSummary;
                }
                else
                {
                    cbIsRefund.Checked = false;
                    dvpRefundReasonEdit.Visible = false;
                    tbRefundSummaryEdit.Visible = false;
                }

                TransactionDetailsState = txn.TransactionDetails.ToList();
                TransactionImagesState = txn.Images.OrderBy( i => i.Order ).Select( i => i.BinaryFileId ).ToList();

                if ( TransactionDetailsState.Count() == 1 )
                {
                    UseSimpleAccountMode = true;
                }
                else
                {
                    UseSimpleAccountMode = false;
                }
                BindAccounts();

                tbSummary.Text = txn.Summary;

                BindImages();

                phAttributeEdits.Controls.Clear();
                Helper.AddEditControls( txn, phAttributeEdits, true );
                phPaymentAttributeEdits.Controls.Clear();
                Helper.AddEditControls( txn.FinancialPaymentDetail, phPaymentAttributeEdits, true );
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            valSummaryTop.Enabled = editable;
            fieldsetViewSummary.Visible = !editable;
            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void SetHeadingInfo( FinancialTransaction txn )
        {
            if ( txn.TransactionTypeValue != null )
            {
                hlType.Visible = true;
                hlType.Text = txn.TransactionTypeValue.Value;
            }
            else
            {
                hlType.Visible = false;
            }

            if ( txn.Batch != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "BatchId", txn.BatchId.ToString() );
                lBatchId.Text = string.Format( "<div class='label label-info'><a href='{1}'>Batch #{0}</a></div>", txn.BatchId, LinkedPageUrl( "BatchDetailPage", qryParams ) );
                lBatchId.Visible = true;
            }
            else
            {
                lBatchId.Visible = false;
            }
        }

        /// <summary>
        /// Binds the dropdowns.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindDropdowns( RockContext rockContext )
        {
            dvpTransactionType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid(), rockContext ).Id;
            dvpNonCashAssetType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE.AsGuid(), rockContext ).Id;
            dvpSourceType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid(), rockContext ).Id;
            dvpCurrencyType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid(), rockContext ).Id;
            dvpCreditCardType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid(), rockContext ).Id;
            dvpRefundReasonEdit.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_REFUND_REASON.AsGuid(), rockContext ).Id;
            dvpRefundReason.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_REFUND_REASON.AsGuid(), rockContext ).Id;
        }

        /// <summary>
        /// Sets the credit card visibility.
        /// </summary>
        private void SetCreditCardVisibility()
        {
            int? currencyType = dvpCurrencyType.SelectedValueAsInt();
            var creditCardCurrencyType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            dvpCreditCardType.Visible = currencyType.HasValue && currencyType.Value == creditCardCurrencyType.Id;
        }

        /// <summary>
        /// Sets the credit card visibility.
        /// </summary>
        private void SetNonCashAssetTypeVisibility()
        {
            int? currencyType = dvpCurrencyType.SelectedValueAsInt();
            dvpNonCashAssetType.Visible = IsNonCashTransaction( currencyType );
        }

        /// <summary>
        /// Binds the transaction details.
        /// </summary>
        private void BindAccounts()
        {
            var feeColumn = GetFeeColumn( gAccountsEdit );

            if ( UseSimpleAccountMode && TransactionDetailsState.Count() == 1 )
            {
                var txnDetail = TransactionDetailsState.First();
                tbSingleAccountAmount.Label = AccountName( txnDetail.AccountId );
                tbSingleAccountAmount.Text = txnDetail.Amount.ToString( "N2" );
                ApplyFeeValueToField( tbSingleAccountFeeAmount, txnDetail );
                feeColumn.Visible = txnDetail.FeeAmount.HasValue;
            }
            else
            {
                var accounts = TransactionDetailsState.ToList();

                var totalAmount = 0m;
                var totalFeeAmount = 0m;
                var hasFeeInfo = false;

                foreach ( var detail in accounts )
                {
                    totalAmount += detail.Amount;

                    if ( detail.FeeAmount.HasValue )
                    {
                        hasFeeInfo = true;
                        totalFeeAmount += detail.FeeAmount.Value;
                    }
                }

                accounts.Add( new FinancialTransactionDetail
                {
                    AccountId = TotalRowAccountId,
                    FeeAmount = hasFeeInfo ? totalFeeAmount : ( decimal? ) null,
                    Amount = totalAmount
                } );

                gAccountsEdit.DataSource = accounts;
                gAccountsEdit.DataBind();
                feeColumn.Visible = hasFeeInfo;
            }
        }

        private void BindImages()
        {
            var ds = TransactionImagesState.ToList();
            ds.Add( 0 );

            dlImages.DataSource = ds;
            dlImages.DataBind();
        }

        /// <summary>
        /// Shows the account dialog.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void ShowAccountDialog( Guid guid )
        {
            hfAccountGuid.Value = guid.ToString();

            var txnDetail = TransactionDetailsState.Where( d => d.Guid.Equals( guid ) ).FirstOrDefault();
            if ( txnDetail != null )
            {
                apAccount.SetValue( txnDetail.AccountId );
                tbAccountAmount.Text = txnDetail.Amount.ToString( "N2" );
                ApplyFeeValueToField( tbAccountFeeAmount, txnDetail );
                tbAccountSummary.Text = txnDetail.Summary;

                if ( txnDetail.Attributes == null )
                {
                    txnDetail.LoadAttributes();
                }
            }
            else
            {
                apAccount.SetValue( null );
                tbAccountAmount.Text = string.Empty;
                tbAccountFeeAmount.Text = string.Empty;
                tbAccountSummary.Text = string.Empty;

                txnDetail = new FinancialTransactionDetail();
                txnDetail.LoadAttributes();
            }

            phAccountAttributeEdits.Controls.Clear();
            Helper.AddEditControls( txnDetail, phAccountAttributeEdits, true, mdAccount.ValidationGroup );

            ShowDialog( "ACCOUNT" );

            _focusControl = tbAccountAmount;
        }

        /// <summary>
        /// Shows the refund dialog.
        /// </summary>
        private void ShowRefundDialog()
        {
            int? txnId = hfTransactionId.Value.AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var txnService = new FinancialTransactionService( rockContext );
                    var txn = txnService.Get( txnId.Value );
                    if ( txn != null && txn.IsAuthorized( "Refund", CurrentPerson ) )
                    {
                        var totalAmount = txn.TotalAmount;

                        var otherAmounts = new FinancialTransactionDetailService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.Transaction != null &&
                                (
                                    (
                                        txn.FinancialGatewayId.HasValue &&
                                        txn.TransactionCode != null &&
                                        txn.TransactionCode != "" &&
                                        d.Transaction.FinancialGatewayId.HasValue &&
                                        d.Transaction.FinancialGatewayId.Value == txn.FinancialGatewayId.Value &&
                                        d.Transaction.TransactionCode == txn.TransactionCode &&
                                        d.TransactionId != txn.Id ) ||
                                    (
                                        d.Transaction.RefundDetails != null &&
                                        d.Transaction.RefundDetails.OriginalTransactionId == txn.Id
                                    )
                                )
                            )
                            .Select( d => d.Amount )
                            .ToList()
                            .Sum();

                        totalAmount += otherAmounts;
                        if ( totalAmount > 0 )
                        {
                            tbRefundAmount.MaximumValue = totalAmount.ToString();
                        }

                        tbRefundAmount.Text = ( totalAmount > 0.0m ? totalAmount : 0.0m ).ToString( "N2" );
                        dvpRefundReason.SelectedIndex = -1;
                        tbRefundSummary.Text = string.Empty;

                        bool hasGateway = !string.IsNullOrWhiteSpace( txn.TransactionCode ) && txn.FinancialGateway != null;
                        cbProcess.Visible = hasGateway;
                        cbProcess.Checked = hasGateway;

                        ShowDialog( "REFUND" );
                        _focusControl = tbRefundAmount;
                    }
                }
            }
        }


        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ACCOUNT":
                    mdAccount.Show();
                    break;
                case "REFUND":
                    mdRefund.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ACCOUNT":
                    mdAccount.Hide();
                    break;
                case "REFUND":
                    mdRefund.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Accounts the name.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        protected string AccountName( int? accountId )
        {
            if ( accountId.HasValue )
            {
                return AccountNames.ContainsKey( accountId.Value ) ? AccountNames[accountId.Value] : string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Images the URL.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        protected string ImageUrl( int binaryFileId, int? maxWidth = null, int? maxHeight = null )
        {
            string width = maxWidth.HasValue ? string.Format( "&maxWidth={0}", maxWidth.Value ) : string.Empty;
            string height = maxHeight.HasValue ? string.Format( "&maxHeight={0}", maxHeight.Value ) : string.Empty;
            return ResolveRockUrl( string.Format( "~/GetImage.ashx?id={0}{1}{2}", binaryFileId, width, height ) );
        }

        /// <summary>
        /// Gets the defined value.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns></returns>
        protected string GetDefinedValue( int? definedValueId )
        {
            if ( definedValueId.HasValue )
            {
                var dv = DefinedValueCache.Get( definedValueId.Value );
                if ( dv != null )
                {
                    return dv.Value;
                }
            }

            return "None";
        }

        /// <summary>
        /// Gets the name of the financial gateway.
        /// </summary>
        /// <param name="gatewayId">The gateway identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected string GetFinancialGatewayName( int? gatewayId, RockContext rockContext )
        {
            if ( gatewayId.HasValue )
            {
                var gw = new FinancialGatewayService( rockContext ).Get( gatewayId.Value );
                if ( gw != null )
                {
                    return gw.Name;
                }
            }

            return "None";
        }

        /// <summary>
        /// Finds the first occuring cell within the collection that contains a EditField
        /// or null if no occurrences are found
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        private static DataControlFieldCell GetEditCell( TableCellCollection cells )
        {
            return GetFirstCellContainingFieldType<EditField>( cells );
        }

        /// <summary>
        /// Finds the first occuring cell within the collection that contains a DeleteField
        /// or null if no occurrences are found
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        private static DataControlFieldCell GetDeleteCell( TableCellCollection cells )
        {
            return GetFirstCellContainingFieldType<DeleteField>( cells );
        }

        /// <summary>
        /// Within the collection, returns the first occurrence of a cell containing a field of type T
        /// or null if no occurrences are found.
        /// </summary>
        /// <typeparam name="T">The IRockGridField type to search for</typeparam>
        /// <param name="cells">The collection of cells</param>
        /// <returns></returns>
        private static DataControlFieldCell GetFirstCellContainingFieldType<T>( TableCellCollection cells ) where T : IRockGridField
        {
            foreach ( var cell in cells.OfType<DataControlFieldCell>() )
            {
                if ( cell != null && cell.ContainingField is T )
                {
                    return cell;
                }
            }

            return null;
        }

        /// <summary>
        /// Take a fee and return empty string if null or the formated currency amount
        /// </summary>
        /// <param name="fee"></param>
        /// <returns></returns>
        private static string GetFeeAsText( decimal? fee )
        {
            if ( fee.HasValue )
            {
                return fee.Value.ToString( "N2" );
            }

            return string.Empty;
        }

        /// <summary>
        /// Applies the fee value to field.
        /// </summary>
        /// <param name="tbSingleAccountFeeAmount">The tb single account fee amount.</param>
        /// <param name="transactionDetail">The transaction detail.</param>
        private static void ApplyFeeValueToField( CurrencyBox tbSingleAccountFeeAmount, FinancialTransactionDetail transactionDetail )
        {
            tbSingleAccountFeeAmount.Text = GetFeeAsText( transactionDetail.FeeAmount );
            tbSingleAccountFeeAmount.Visible = transactionDetail.FeeAmount.HasValue;
        }

        /// <summary>
        /// Gets the fee column.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns></returns>
        private static CurrencyField GetFeeColumn( Grid grid )
        {
            return grid.ColumnsOfType<CurrencyField>().First( c => c.DataField == "FeeAmount" );
        }

        #endregion
    }
}