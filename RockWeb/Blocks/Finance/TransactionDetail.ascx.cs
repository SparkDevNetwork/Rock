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

    [LinkedPage( "Batch Detail Page", "Page used to view batch." )]
    [LinkedPage( "Scheduled Transaction Detail Page", "Page used to view scheduled transaction detail." )]
    public partial class TransactionDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
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
                    _accountNames.Add( int.MinValue, "&nbsp;&nbsp;&nbsp;&nbsp;<strong>Total</strong>" );
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

            gAccountsEdit.DataKeyNames = new string[] { "Guid" };
            gAccountsEdit.ShowActionRow = true;
            gAccountsEdit.Actions.ShowAdd = true;
            gAccountsEdit.Actions.AddClick += gAccountsEdit_AddClick;
            gAccountsEdit.GridRebind += gAccountsEdit_GridRebind;
            gAccountsEdit.RowDataBound += gAccountsEdit_RowDataBound;


            //function toggleCheckImages() {
            //    var image1src = $('#<%=imgCheck.ClientID%>').attr("src");
            //    var image2src = $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src");

            //    $('#<%=imgCheck.ClientID%>').attr("src", image2src);
            //    $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src", image1src);
            //}

            string script = @"
    $('.transaction-image-thumbnail').click( function() {
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
                ShowDetail( PageParameter( "transactionId" ).AsInteger(), PageParameter( "batchId" ).AsIntegerOrNull() );
            }
            else
            {
                ShowDialog();
            }
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
            ShowEditDetails( GetTransaction( hfTransactionId.Value.AsInteger() ), new RockContext() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var txnService = new FinancialTransactionService( rockContext );
            var txnDetailService = new FinancialTransactionDetailService( rockContext );
            var txnImageService = new FinancialTransactionImageService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );

            FinancialTransaction txn = null;

            int? txnId = hfTransactionId.Value.AsIntegerOrNull();
            int? batchId = hfBatchId.Value.AsIntegerOrNull();

            var changes = new List<string>();

            if ( txnId.HasValue )
            {
                txn = txnService.Get( txnId.Value );
            }

            if ( txn == null )
            {
                txn = new FinancialTransaction();
                txnService.Add( txn );
                txn.BatchId = batchId;
                changes.Add( "Created transaction" );
            }

            if ( txn != null )
            {
                string newPerson = ppAuthorizedPerson.PersonName;

                if ( batchId.HasValue )
                {
                    if ( !txn.AuthorizedPersonAliasId.Equals( ppAuthorizedPerson.PersonAliasId ) )
                    {
                        string prevPerson = ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null ) ?
                            txn.AuthorizedPersonAlias.Person.FullName : string.Empty;
                        History.EvaluateChange( changes, "Person", prevPerson, newPerson );
                    }

                    History.EvaluateChange( changes, "Date/Time", txn.TransactionDateTime, dtTransactionDateTime.SelectedDateTime );
                    History.EvaluateChange( changes, "Type", GetDefinedValue( txn.TransactionTypeValueId ), GetDefinedValue( ddlTransactionType.SelectedValue.AsInteger() ) );
                    History.EvaluateChange( changes, "Source", GetDefinedValue( txn.SourceTypeValueId ), GetDefinedValue( ddlSourceType.SelectedValueAsInt() ) );

                    if ( !txn.FinancialGatewayId.Equals( gpPaymentGateway.SelectedValueAsInt() ) )
                    {
                        History.EvaluateChange( changes, "Gateway", GetFinancialGatewayName( txn.FinancialGatewayId, rockContext ), GetFinancialGatewayName( gpPaymentGateway.SelectedValueAsInt(), rockContext ) );
                    }

                    History.EvaluateChange( changes, "Transaction Code", txn.TransactionCode, tbTransactionCode.Text );
                    History.EvaluateChange( changes, "Currency Type", GetDefinedValue( txn.CurrencyTypeValueId ), GetDefinedValue( ddlCurrencyType.SelectedValueAsInt() ) );
                    History.EvaluateChange( changes, "Credit Card Type", GetDefinedValue( txn.CreditCardTypeValueId ), GetDefinedValue( ddlCreditCardType.SelectedValueAsInt() ) );
                    History.EvaluateChange( changes, "Summary", txn.Summary, tbSummary.Text );
                }

                txn.AuthorizedPersonAliasId = ppAuthorizedPerson.PersonAliasId;
                txn.TransactionDateTime = dtTransactionDateTime.SelectedDateTime;
                txn.TransactionTypeValueId = ddlTransactionType.SelectedValue.AsInteger();
                txn.SourceTypeValueId = ddlSourceType.SelectedValueAsInt();
                txn.FinancialGatewayId = gpPaymentGateway.SelectedValueAsInt();
                txn.TransactionCode = tbTransactionCode.Text;
                txn.CurrencyTypeValueId = ddlCurrencyType.SelectedValueAsInt();
                txn.CreditCardTypeValueId = ddlCreditCardType.SelectedValueAsInt();
                txn.Summary = tbSummary.Text;

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
                        if ( batchId.HasValue )
                        {
                            History.EvaluateChange( changes, txnDetail.Account != null ? txnDetail.Account.Name : "Unknown", txnDetail.Amount.ToString( "C2" ), string.Empty );
                        }
                        txnDetailService.Delete( txnDetail );
                    } );

                    // Save Transaction Details
                    foreach ( var editorTxnDetail in TransactionDetailsState )
                    {
                        string oldAccountName = string.Empty;
                        string newAccountName = string.Empty;
                        decimal oldAmount = 0.0M;
                        decimal newAmount = 0.0M;

                        // Add or Update the activity type
                        var txnDetail = txn.TransactionDetails.FirstOrDefault( d => d.Guid.Equals( editorTxnDetail.Guid ) );
                        if ( txnDetail != null )
                        {
                            oldAccountName = AccountName( txnDetail.AccountId );
                            oldAmount = txnDetail.Amount;
                        }
                        else 
                        {
                            txnDetail = new FinancialTransactionDetail();
                            txnDetail.Guid = editorTxnDetail.Guid;
                            txn.TransactionDetails.Add( txnDetail );
                        }

                        newAccountName = AccountName( editorTxnDetail.AccountId );
                        newAmount = UseSimpleAccountMode ? tbSingleAccountAmount.Text.AsDecimal() : editorTxnDetail.Amount;

                        if ( batchId.HasValue )
                        {
                            if ( string.IsNullOrWhiteSpace(oldAccountName) )
                            {
                                History.EvaluateChange( changes, newAccountName, string.Empty, newAmount.ToString( "C2" ) );
                            }
                            else
                            {
                                if ( oldAccountName == newAccountName )
                                {
                                    if ( oldAmount != newAmount )
                                    {
                                        History.EvaluateChange( changes, oldAccountName, oldAmount.ToString("C2"), newAmount.ToString("C2") );
                                    }
                                }
                                else
                                {
                                    History.EvaluateChange( changes, oldAccountName, oldAmount.ToString( "C2" ), string.Empty );
                                    History.EvaluateChange( changes, newAccountName, string.Empty, newAmount.ToString( "C2" ) );
                                }
                            }
                        }

                        txnDetail.AccountId = editorTxnDetail.AccountId;
                        txnDetail.Amount = newAmount;
                        txnDetail.Summary = editorTxnDetail.Summary;
                    }

                    // Delete any transaction images that were removed
                    var orphanedBinaryFileIds = new List<int>();
                    var txnImagesInDB = txnImageService.Queryable().Where( a => a.TransactionId.Equals( txn.Id ) ).ToList();
                    foreach ( var txnImage in txnImagesInDB.Where( i => !TransactionImagesState.Contains( i.BinaryFileId ) ) )
                    {
                        changes.Add( "Removed Image" );
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
                            changes.Add( "Added Image" );
                            txnImage = new FinancialTransactionImage();
                            txnImage.TransactionId = txn.Id;
                            txn.Images.Add( txnImage );
                        }
                        else
                        {
                            if ( txnImage.BinaryFileId != binaryFileId )
                            {
                                changes.Add( "Updated Image" );
                            }
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

                    // If the transaction is associated with a batch, update that batch's history
                    if ( batchId.HasValue )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            batchId.Value,
                            changes,
                            !string.IsNullOrWhiteSpace( newPerson ) ? newPerson : string.Format( "Transaction Id:{0}", txn.Id ),
                            typeof( FinancialTransaction ),
                            txn.Id
                        );
                    }

                    rockContext.SaveChanges();

                } );

                // Save selected options to session state in order to prefill values for next added txn
                Session["NewTxnDefault_BatchId"] = txn.BatchId;
                Session["NewTxnDefault_TransactionDateTime"] = txn.TransactionDateTime;
                Session["NewTxnDefault_TransactionType"] = txn.TransactionTypeValueId;
                Session["NewTxnDefault_SourceType"] = txn.SourceTypeValueId;
                Session["NewTxnDefault_CurrencyType"] = txn.CurrencyTypeValueId;
                Session["NewTxnDefault_CreditCardType"] = txn.CreditCardTypeValueId;
                if ( TransactionDetailsState.Count() == 1 )
                {
                    Session["NewTxnDefault_Account"] = TransactionDetailsState.First().AccountId;
                }
                else
                {
                    Session.Remove("NewTxnDefault_Account");
                }

                // Requery the batch to support EF navigation properties
                var savedTxn = GetTransaction( txn.Id );
                ShowReadOnlyDetails( savedTxn );
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
                ShowReadOnlyDetails( GetTransaction( txnId ) );
            }
            else
            {
                Dictionary<string, string> pageParams = new Dictionary<string, string>();

                if ( !string.IsNullOrWhiteSpace( PageParameter( "batchId" ) ) )
                {
                    pageParams.Add( "BatchId", PageParameter( "batchId" ) );
                }

                NavigateToParentPage( pageParams );
            }
        }

        protected void lbAddTransaction_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> pageParams = new Dictionary<string, string>();

            pageParams.Add( "BatchId", PageParameter( "batchId" ) );
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
            _focusControl = ddlCurrencyType;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gAccountsEdit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var account = (FinancialTransactionDetail)e.Row.DataItem;

                // If this is the total row
                if ( account.AccountId == int.MinValue )
                {
                    // disable the row select on each column
                    foreach( TableCell cell in e.Row.Cells )
                    {
                        cell.RemoveCssClass( "grid-select-cell" );
                    }

                    // Hide the delete button
                    var ctrls = e.Row.Cells[3].ControlsOfTypeRecursive<LinkButton>();
                    if ( ctrls.Any() && ctrls.Count() == 1 )
                    {
                        ctrls.First().Visible = false;
                    }
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
        /// Handles the RowSelected event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_RowSelected( object sender, RowEventArgs e )
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
                txnDetail.AccountId = ddlAccount.SelectedValue.AsInteger();
                txnDetail.Amount = tbAccountAmount.Text.AsDecimal();
                txnDetail.Summary = tbAccountSummary.Text;

                BindAccounts();
            }

            HideDialog();
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
                    imgupImage.BinaryFileId = (int)e.Item.DataItem;
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

        /// <summary>
        /// Navigates to the next transaction in the list.
        /// </summary>
        private void ShowNextButton( int transactionId, int? batchId )
        {
            if ( batchId == null || ! batchId.HasValue || batchId == 0 )
            {
                lbNext.Visible = false;
                return;
            }

            lbNext.Visible = true;
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var qryTransactionsToMatch = financialTransactionService.Queryable()
                .Where( a => a.BatchId == batchId );

            var nextFinancialTransaction = qryTransactionsToMatch.Where( a => a.Id > transactionId ).Take( 1 ).FirstOrDefault();

            if ( nextFinancialTransaction != null )
            {
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "batchId", hfBatchId.Value );
                qryParam.Add( "transactionId", nextFinancialTransaction.Id.ToStringSafe() );
                lbNext.NavigateUrl = new PageReference( CurrentPageReference.PageId, 0, qryParam ).BuildUrl();
            }
            else
            {
                lbNext.AddCssClass( "disabled" );
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
                .Queryable( "AuthorizedPersonAlias.Person,TransactionTypeValue,SourceTypeValue,FinancialGateway,CurrencyTypeValue,TransactionDetails,ScheduledTransaction,ProcessedByPersonAlias.Person" )
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
            // show or hide the add new transaction button depending if there is a batch id in the querystring
            bool haveBatch = !string.IsNullOrWhiteSpace( PageParameter( "batchId" ) );
            
            FinancialTransaction txn = null;

            bool editAllowed = UserCanEdit;

            var rockContext = new RockContext();

            if ( !transactionId.Equals( 0 ) )
            {
                txn = GetTransaction( transactionId, rockContext );
                if ( !editAllowed && txn != null )
                {
                    editAllowed = txn.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            if ( txn == null )
            {
                txn = new FinancialTransaction { Id = 0 };
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
                    txn.CurrencyTypeValueId = Session["NewTxnDefault_CurrencyType"] as int?;
                    txn.CreditCardTypeValueId = Session["NewTxnDefault_CreditCardType"] as int?;
                    int? accountId = Session["NewTxnDefault_Account"] as int?;
                    if ( accountId.HasValue )
                    {
                        var txnDetail = new FinancialTransactionDetail();
                        txnDetail.AccountId = accountId.Value;
                        txn.TransactionDetails.Add( txnDetail );
                    }
                }
            }
            else
            {
                gpPaymentGateway.Visible = true;
            }

            hfTransactionId.Value = txn.Id.ToString();
            hfBatchId.Value = batchId.HasValue ? batchId.Value.ToString() : string.Empty;
            ShowNextButton( transactionId, batchId );

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            lbEdit.Visible = editAllowed;
            lbAddTransaction.Visible = editAllowed && haveBatch;

            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialTransaction.FriendlyTypeName );
            }

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

        /// <summary>
        /// Shows the read only details.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowReadOnlyDetails( FinancialTransaction txn )
        {
            SetEditMode( false );

            if ( txn != null )
            {
                hfTransactionId.Value = txn.Id.ToString();

                SetHeadingInfo( txn );

                string rockUrlRoot = ResolveRockUrl( "/" );

                var detailsLeft = new DescriptionList()
                    .Add( "Person", ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null ) ? txn.AuthorizedPersonAlias.Person.GetAnchorTag( rockUrlRoot ) : string.Empty )
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

                detailsLeft.Add( "Source", txn.SourceTypeValue != null ? txn.SourceTypeValue.Value : string.Empty );

                if ( txn.FinancialGateway != null )
                {
                    detailsLeft.Add( "Payment Gateway", Rock.Financial.GatewayContainer.GetComponentName( txn.FinancialGateway.Name ) );
                }

                detailsLeft.Add( "Transaction Code", txn.TransactionCode );
                
                if ( txn.ScheduledTransaction != null )
                {
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "ScheduledTransactionId", txn.ScheduledTransaction.Id.ToString() );
                    string url = LinkedPageUrl( "ScheduledTransactionDetailPage", qryParam );
                    detailsLeft.Add( "Scheduled Transaction Id", !string.IsNullOrWhiteSpace( url ) ?
                        string.Format( "<a href='{0}'>{1}</a>", url, txn.ScheduledTransaction.GatewayScheduleId ) :
                        txn.ScheduledTransaction.GatewayScheduleId );
                }

                if ( txn.CurrencyTypeValue != null )
                {
                    string currencyType = txn.CurrencyTypeValue.Value;
                    if ( txn.CurrencyTypeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ) )
                    {
                        currencyType += txn.CreditCardTypeValue != null ? ( " - " + txn.CreditCardTypeValue.Value ) : string.Empty;
                    }
                    detailsLeft.Add( "Currency Type", currencyType );
                }

                detailsLeft.Add( "Summary", txn.Summary.ConvertCrLfToHtmlBr() );

                var modified = new StringBuilder(); ;
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

                var accounts = txn.TransactionDetails.ToList();
                accounts.Add( new FinancialTransactionDetail
                { 
                    AccountId = int.MinValue, 
                    Amount = txn.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M
                });
                gAccountsView.DataSource = accounts;
                gAccountsView.DataBind();

                if ( txn.Images.Any() )
                {
                    var primaryImage = txn.Images
                        .OrderBy( i => i.Order )
                        .FirstOrDefault();
                    imgPrimary.ImageUrl = string.Format( "~/GetImage.ashx?id={0}", primaryImage.BinaryFileId );
                    imgPrimary.Visible = true;

                    rptrImages.DataSource = txn.Images
                        .Where( i => !i.Id.Equals( primaryImage.Id ) )
                        .OrderBy( i => i.Order )
                        .ToList();
                    rptrImages.DataBind();
                }
                else
                {
                    imgPrimary.Visible = false;
                }
            }
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
                BindDropdowns( rockContext );

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
                dtTransactionDateTime.SelectedDateTime = txn.TransactionDateTime;
                ddlTransactionType.SetValue( txn.TransactionTypeValueId );
                ddlSourceType.SetValue( txn.SourceTypeValueId );
                gpPaymentGateway.SetValue( txn.FinancialGatewayId );
                tbTransactionCode.Text = txn.TransactionCode;
                ddlCurrencyType.SetValue( txn.CurrencyTypeValueId );
                ddlCreditCardType.SetValue( txn.CreditCardTypeValueId );
                SetCreditCardVisibility();

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
        }

        /// <summary>
        /// Binds the dropdowns.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void BindDropdowns( RockContext rockContext )
        {
            ddlTransactionType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid(), rockContext ) );
            ddlSourceType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid(), rockContext ), true );
            ddlCurrencyType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid(), rockContext ), true );
            ddlCreditCardType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid(), rockContext ), true );

            ddlAccount.DataSource = AccountNames;
            ddlAccount.DataBind();
        }

        /// <summary>
        /// Sets the credit card visibility.
        /// </summary>
        private void SetCreditCardVisibility()
        {
            int? currencyType = ddlCurrencyType.SelectedValueAsInt();
            var creditCardCurrencyType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
            ddlCreditCardType.Visible = currencyType.HasValue && currencyType.Value == creditCardCurrencyType.Id;
        }

        /// <summary>
        /// Binds the transaction details.
        /// </summary>
        private void BindAccounts()
        {
            if ( UseSimpleAccountMode && TransactionDetailsState.Count() == 1 )
            {
                var txnDetail = TransactionDetailsState.First();
                tbSingleAccountAmount.Label = AccountName( txnDetail.AccountId );
                tbSingleAccountAmount.Text = txnDetail.Amount.ToString("N2");
            }
            else
            {
                var accounts = TransactionDetailsState.ToList();
                accounts.Add( new FinancialTransactionDetail
                {
                    AccountId = int.MinValue,
                    Amount = TransactionDetailsState.Sum( d => (decimal?)d.Amount ) ?? 0.0M
                } );

                gAccountsEdit.DataSource = accounts;
                gAccountsEdit.DataBind();
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
                ddlAccount.SetValue( txnDetail.AccountId );
                tbAccountAmount.Text = txnDetail.Amount.ToString( "N2" );
                tbAccountSummary.Text = txnDetail.Summary;
            }
            else
            {
                ddlAccount.SelectedIndex = -1;
                tbAccountSummary.Text = string.Empty;
                tbAccountSummary.Text = string.Empty;
            }

            ShowDialog( "ACCOUNT" );

            _focusControl = tbAccountAmount;
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
                var dv = DefinedValueCache.Read( definedValueId.Value );
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

        #endregion

    }
}