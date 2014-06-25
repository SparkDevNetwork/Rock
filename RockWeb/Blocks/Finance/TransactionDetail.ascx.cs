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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given transaction for editing." )]
    public partial class TransactionDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Fields

        private string contextTypeName = string.Empty;
        private bool readOnly = false;

        #endregion Fields

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );

            gTransactionDetails.DataKeyNames = new string[] { "id" };
            gTransactionDetails.Actions.AddClick += gTransactionDetails_Add;
            gTransactionDetails.GridRebind += gTransactionDetails_GridRebind;
            mdDetails.SaveClick += mdDetails_SaveClick;
            mdDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
            }

            ScriptManager.RegisterStartupScript( pnlDetails, pnlDetails.GetType(), "images-fluidbox", "$('.photo a.transaction-image').fluidbox();", true );

            string deleteButtonScriptFormat = @"
                $('.image-delete-button').on( 'click', function (event) {{
                return Rock.dialogs.confirmDelete(event, '{0}');
            }});";
            string deleteButtonScript = string.Format( deleteButtonScriptFormat, "image" );
            ScriptManager.RegisterStartupScript( pnlTransactionImages, pnlTransactionImages.GetType(), "image-delete-confirm-script", deleteButtonScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "transactionId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "transactionId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSaveFinancialTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveTransaction_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var financialTransactionService = new Rock.Model.FinancialTransactionService( rockContext );
            Rock.Model.FinancialTransaction financialTransaction = null;
            int financialTransactionId = !string.IsNullOrEmpty( hfIdTransValue.Value ) ? int.Parse( hfIdTransValue.Value ) : 0;

            // null if not associated with a batch
            int? batchId = hfBatchId.Value.AsIntegerOrNull();

            if ( financialTransactionId == 0 )
            {
                financialTransaction = new Rock.Model.FinancialTransaction();
                financialTransactionService.Add( financialTransaction );
                financialTransaction.BatchId = batchId;
            }
            else
            {
                financialTransaction = financialTransactionService.Get( financialTransactionId );
            }

            if ( ppAuthorizedPerson.PersonId != null )
            {
                financialTransaction.AuthorizedPersonId = ppAuthorizedPerson.PersonId;
            }
            else
            {
                financialTransaction.AuthorizedPersonId = null;
            }

            if ( ddlCurrencyType.SelectedItem.ToString() == "Credit Card" )
            {
                financialTransaction.CreditCardTypeValueId = int.Parse( ddlCreditCardType.SelectedValue );
            }
            else
            {
                financialTransaction.CreditCardTypeValueId = null;
            }

            financialTransaction.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
            if ( !string.IsNullOrEmpty( ddlPaymentGateway.SelectedValue ) )
            {
                var gatewayEntity = Rock.Web.Cache.EntityTypeCache.Read( new Guid( ddlPaymentGateway.SelectedValue ) );
                if ( gatewayEntity != null )
                {
                    financialTransaction.GatewayEntityTypeId = gatewayEntity.Id;
                }
            }

            financialTransaction.SourceTypeValueId = int.Parse( ddlSourceType.SelectedValue );
            financialTransaction.TransactionTypeValueId = int.Parse( ddlTransactionType.SelectedValue );

            financialTransaction.Summary = tbSummary.Text;
            financialTransaction.TransactionCode = tbTransactionCode.Text;
            financialTransaction.TransactionDateTime = dtTransactionDateTime.SelectedDateTime;

            rockContext.SaveChanges();

            if ( batchId != null )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["financialBatchid"] = hfBatchId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelFinancialTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelTransaction_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrEmpty( hfBatchId.Value ) )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["financialBatchid"] = hfBatchId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCurrencyType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCurrencyType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // We don't want to show the Credit Card Type drop down if the type of currency isn't Credit Card.
            if ( ddlCurrencyType.SelectedItem.ToString() == "Credit Card" )
            {
                ddlCreditCardType.Visible = true;
            }
            else
            {
                ddlCreditCardType.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            BindDropdowns();
            var transaction = new FinancialTransactionService( new RockContext() ).Get( hfIdTransValue.ValueAsInt() );
            ShowEdit( transaction );
        }

        /// <summary>
        /// Handles the RowSelected event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gTransactionDetails_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            if ( !readOnly )
            {
                var rockContext = new RockContext();
                LoadRelatedImages( new FinancialTransactionService( rockContext ).Get( hfIdTransValue.ValueAsInt() ).Id );
                var transactionDetailsId = (int)e.RowKeyValue;
                hfIdValue.Value = transactionDetailsId.ToString();
                var ftd = new FinancialTransactionDetailService( rockContext ).Get( transactionDetailsId );
                ddlTransactionAccount.SelectedValue = ftd.AccountId.ToString();
                tbTransactionAmount.Text = ftd.Amount.ToString();
                tbTransactionSummary.Text = ftd.Summary;
                mdDetails.Show();
            }
        }

        /// <summary>
        /// Handles the Delete event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gTransactionDetails_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var ftdService = new FinancialTransactionDetailService( rockContext );
            var ftd = ftdService.Get( e.RowKeyId );
            if ( ftd != null )
            {
                string errorMessage;
                if ( !ftdService.CanDelete( ftd, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, Rock.Web.UI.Controls.ModalAlertType.Information );
                    return;
                }

                ftdService.Delete( ftd );
                rockContext.SaveChanges();
            }

            FinancialTransaction transaction = new FinancialTransaction();
            transaction = new FinancialTransactionService( rockContext ).Get( hfIdTransValue.ValueAsInt() );
            BindTransactionDetailGrid( transaction );
        }

        /// <summary>
        /// Handles the Add event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gTransactionDetails_Add( object sender, EventArgs e )
        {
            var transactionDetailsId = 0;
            hfIdValue.Value = transactionDetailsId.ToString();
            ddlTransactionAccount.SelectedIndex = 0;
            tbTransactionAmount.Text = string.Empty;
            tbTransactionSummary.Text = string.Empty;
            mdDetails.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gTransactionDetails_GridRebind( object sender, EventArgs e )
        {
            FinancialTransaction transaction = new FinancialTransaction();
            transaction = new FinancialTransactionService( new RockContext() ).Get( hfIdTransValue.ValueAsInt() );
            BindTransactionDetailGrid( transaction );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void mdDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            if ( !string.IsNullOrWhiteSpace( tbTransactionAmount.Text ) )
            {
                var ftdService = new FinancialTransactionDetailService( rockContext );
                FinancialTransactionDetail ftd = null;
                var transactionDetailId = int.Parse( hfIdValue.Value );
                if ( transactionDetailId > 0 )
                {
                    ftd = ftdService.Get( transactionDetailId );
                }
                else
                {
                    ftd = new FinancialTransactionDetail { Id = 0 };
                }

                ftd.TransactionId = hfIdTransValue.ValueAsInt();
                ftd.AccountId = int.Parse( ddlTransactionAccount.SelectedValue );
                ftd.Amount = decimal.Parse( tbTransactionAmount.Text );
                ftd.Summary = tbTransactionSummary.Text;

                if ( transactionDetailId == 0 )
                {
                    ftdService.Add( ftd );
                }

                rockContext.SaveChanges();
            }

            mdDetails.Hide();
            FinancialTransaction transaction = new FinancialTransaction();
            transaction = new FinancialTransactionService( rockContext ).Get( hfIdTransValue.ValueAsInt() );
            BindTransactionDetailGrid( transaction );
            LoadRelatedImages( transaction.Id );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the dlImages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataListItemEventArgs"/> instance containing the event data.</param>
        protected void dlImages_ItemDataBound( object sender, DataListItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                PlaceHolder ph = e.Item.FindControl( "phImage" ) as PlaceHolder;
                var imageId = ( (BinaryFile)e.Item.DataItem ).Id;
                var imageUrl = "/GetImage.ashx?id=" + imageId;
                var imageTag = new LiteralControl( "<img src='" + imageUrl + "' style='max-width:100%;max-height:100%;' />" );
                var imageLink = new HyperLink();
                imageLink.Attributes.Add( "href", imageUrl );
                imageLink.AddCssClass( "transaction-image" );
                ph.Controls.Add( imageLink );
                imageLink.Controls.Add( imageTag );

                LinkButton lbDelete = e.Item.FindControl( "lbDelete" ) as LinkButton;
                lbDelete.Attributes.Add( "imageId", imageId.ToString() );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            LinkButton lbDelete = sender as LinkButton;
            var imageId = int.Parse( lbDelete.Attributes["imageId"] );
            var rockContext = new RockContext();

            // Delete the reference to the binary file (image) from the FinancialTransactionImage table
            var imageService = new FinancialTransactionImageService( rockContext );
            var financialTransactionImage = imageService.Queryable().Where( image => image.BinaryFileId == imageId ).FirstOrDefault();
            imageService.Delete( financialTransactionImage );

            // Delete the actual binary file (image)
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( imageId );
            binaryFileService.Delete( binaryFile );

            rockContext.SaveChanges();

            LoadRelatedImages( hfIdTransValue.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Click event of the lbSaveImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveImage_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new FinancialTransactionImageService( rockContext );
            var financialTransactionImage = new FinancialTransactionImage();
            var transactionId = hfIdTransValue.ValueAsInt();
            financialTransactionImage.BinaryFileId = imgupTransactionImages.BinaryFileId.Value;
            financialTransactionImage.TransactionId = transactionId;
            financialTransactionImage.TransactionImageTypeValueId = ddlTransactionImageType.SelectedValueAsInt();
            service.Add( financialTransactionImage );
            rockContext.SaveChanges();
            imgupTransactionImages.BinaryFileId = 0;
            LoadRelatedImages( transactionId );
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Loads the account drop down.
        /// </summary>
        private void LoadAccountDropDown()
        {
            var accountList = new FinancialAccountService( new RockContext() ).Queryable().ToList();
            foreach ( var account in accountList )
            {
                ListItem acc = new ListItem();
                acc.Text = account.Name;
                acc.Value = account.Id.ToString();
                ddlTransactionAccount.Items.Add( acc );
            }
        }

        /// <summary>
        /// Binds the dropdowns.
        /// </summary>
        protected void BindDropdowns()
        {
            BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source" );
            BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
        }

        /// <summary>
        /// Binds the defined type dropdown.
        /// </summary>
        /// <param name="ListControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        private void BindDefinedTypeDropdown( ListControl listControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            listControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
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
        /// Shows the edit values.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        protected void ShowEdit( FinancialTransaction transaction )
        {
            if ( transaction != null && transaction.Id != 0 )
            {
                lTitle.Text = "Edit Transaction".FormatAsHtmlTitle();
                hfIdTransValue.Value = transaction.Id.ToString();
                hfBatchId.Value = PageParameter( "financialBatchId" );
                ddlCreditCardType.SetValue( transaction.CreditCardTypeValueId );
                ddlCurrencyType.SetValue( transaction.CurrencyTypeValueId );
                if ( transaction.GatewayEntityTypeId.HasValue )
                {
                    var gatewayEntity = Rock.Web.Cache.EntityTypeCache.Read( transaction.GatewayEntityTypeId.Value );
                    if ( gatewayEntity != null )
                    {
                        ddlPaymentGateway.SetValue( gatewayEntity.Guid.ToString().ToUpper() );
                    }
                }

                ddlSourceType.SetValue( transaction.SourceTypeValueId );
                ddlTransactionType.SetValue( transaction.TransactionTypeValueId );
                tbSummary.Text = transaction.Summary;
                tbTransactionCode.Text = transaction.TransactionCode;
                dtTransactionDateTime.SelectedDateTime = transaction.TransactionDateTime;
                if ( transaction.AuthorizedPersonId != null )
                {
                    ppAuthorizedPerson.PersonId = transaction.AuthorizedPersonId;
                    ppAuthorizedPerson.PersonName = transaction.AuthorizedPerson.FullName;
                }
            }
            else
            {
                lTitle.Text = "Add Transaction".FormatAsHtmlTitle();
                int? contextPersonId = PageParameter( "PersonId" ).AsIntegerOrNull();
                if ( contextPersonId.HasValue )
                {
                    var contextPerson = new PersonService( new RockContext() ).Get( (int)contextPersonId );
                    ppAuthorizedPerson.PersonId = contextPerson.Id;
                    ppAuthorizedPerson.PersonName = contextPerson.FullName;
                }                
            }

            SetEditMode( true );
            LoadRelatedImages( transaction.Id );

            if ( ddlCurrencyType != null && ddlCurrencyType.SelectedItem != null && ddlCurrencyType.SelectedItem.ToString() != "Credit Card" )
            {
                ddlCreditCardType.Visible = false;
            }
        }

        /// <summary>
        /// Shows the summary.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void ShowSummary( FinancialTransaction transaction )
        {
            lTitle.Text = "View Transaction".FormatAsHtmlTitle();
            SetEditMode( false );

            string authorizedPerson = string.Empty;
            if ( transaction.AuthorizedPerson != null )
            {
                authorizedPerson = transaction.AuthorizedPerson.FullName;
            }

            lDetailsLeft.Text = new DescriptionList()
                .Add( "Transaction Date/Time", transaction.TransactionDateTime )
                .Add( "Transaction Type", transaction.TransactionTypeValue )
                .Add( "Credit Card Type", transaction.CreditCardTypeValue )
                .Add( "Authorized Person", authorizedPerson )
                .Html;

            lDetailsRight.Text = new DescriptionList()
                .Add( "Summary", transaction.Summary )
                .Add( "Source Type", transaction.SourceTypeValue )
                .Add( "Transaction Code", transaction.TransactionCode )
                .Add( "Currency Type", transaction.CurrencyTypeValue )
                .Add( "Payment Gateway", transaction.GatewayEntityType )
                .Html;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "transactionId" ) && !!itemKey.Equals( "batchfk" ) )
            {
                return;
            }

            FinancialTransaction transaction = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                transaction = new FinancialTransactionService( new RockContext() ).Get( itemKeyValue );
            }
            else
            {
                transaction = new FinancialTransaction { Id = 0 };
            }

            hfIdTransValue.Value = transaction.Id.ToString();
            hfBatchId.Value = PageParameter( "financialBatchId" );

            if ( !readOnly )
            {
                lbEdit.Visible = true;
                if ( transaction.Id > 0 )
                {
                    ShowSummary( transaction );
                }
                else
                {
                    BindDropdowns();
                    ShowEdit( transaction );
                }

                gTransactionDetails.Actions.ShowAdd = true;
                gTransactionDetails.IsDeleteEnabled = true;
                pnlImageUpload.Visible = true;
            }
            else
            {
                lbEdit.Visible = false;
                ShowSummary( transaction );
                gTransactionDetails.Actions.ShowAdd = false;
                gTransactionDetails.IsDeleteEnabled = false;
                pnlImageUpload.Visible = false;
            }

            lbSave.Visible = !readOnly;
            LoadAccountDropDown();
            BindTransactionDetailGrid( transaction );
            BindDefinedTypeDropdown( ddlTransactionImageType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_IMAGE_TYPE ), "Transaction Image Type" );
            LoadRelatedImages( transaction.Id );
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            valSummaryTop.Controls.Clear();
            valSummaryTop.Controls.Add( new LiteralControl( message ) );
            valSummaryTop.Visible = true;
        }

        /// <summary>
        /// Binds the transaction detail grid.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private void BindTransactionDetailGrid( FinancialTransaction transaction )
        {
            // Load the TransactionDetails grid here if this transaction already exists.
            if ( transaction.Id != 0 )
            {
                var financialTransactionDetails = new FinancialTransactionDetailService( new RockContext() ).Queryable().Where( trans => trans.TransactionId == transaction.Id ).ToList();
                gTransactionDetails.DataSource = financialTransactionDetails;
                gTransactionDetails.DataBind();
                pnlTransactionDetails.Visible = true;
            }
            else
            {
                pnlTransactionDetails.Visible = false;
            }
        }

        /// <summary>
        /// Loads the related images.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void LoadRelatedImages( int transactionId )
        {
            var rockContext = new RockContext();
            
            // Let's get a list of related images from the FinancialTransactionImage table
            var relatedImageList = new FinancialTransactionImageService( rockContext ).Queryable().Where( image => image.TransactionId == transactionId ).ToList();

            var binaryFileService = new BinaryFileService( rockContext );

            // Now let's get the binary files from the BinaryFile table
            var binaryFileList = new List<BinaryFile>();
            foreach ( var relatedImage in relatedImageList )
            {
                var binaryFileId = relatedImage.BinaryFileId;
                var binaryFile = binaryFileService.Get( binaryFileId );
                binaryFileList.Add( binaryFile );
            }

            dlImages.DataSource = binaryFileList;
            dlImages.DataBind();
        }

        #endregion Internal Methods
    }
}