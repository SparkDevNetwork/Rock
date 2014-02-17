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
using Rock.Model;
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

        #endregion Fields

        #region Control Methods

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
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialTransactionService = new Rock.Model.FinancialTransactionService();
                Rock.Model.FinancialTransaction financialTransaction = null;
                int financialTransactionId = !string.IsNullOrEmpty( hfIdTransValue.Value ) ? int.Parse( hfIdTransValue.Value ) : 0;

                // null if not associated with a batch
                int? batchId = hfBatchId.Value.AsInteger();

                if ( financialTransactionId == 0 )
                {
                    financialTransaction = new Rock.Model.FinancialTransaction();
                    financialTransactionService.Add( financialTransaction, CurrentPersonAlias );
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

                decimal amount = 0M;
                decimal.TryParse( tbAmount.Text.Replace( "$", string.Empty ), out amount );
                financialTransaction.Amount = amount;

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

                financialTransactionService.Save( financialTransaction, CurrentPersonAlias );

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

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            BindDropdowns();
            var transaction = new FinancialTransactionService().Get( hfIdTransValue.ValueAsInt() );
            ShowEdit( transaction );
        }

        protected void gTransactionDetails_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
        }

        protected void gTransactionDetails_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
        }

        #endregion Events

        #region Internal Methods

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
                tbAmount.Text = transaction.Amount.ToString();
                hfBatchId.Value = PageParameter( "financialBatchId" );
                ddlCreditCardType.SetValue( transaction.CreditCardTypeValueId );
                ddlCurrencyType.SetValue( transaction.CurrencyTypeValueId );
                if ( transaction.GatewayEntityTypeId.HasValue )
                {
                    var gatewayEntity = Rock.Web.Cache.EntityTypeCache.Read( transaction.GatewayEntityTypeId.Value );
                    if ( gatewayEntity != null )
                    {
                        ddlPaymentGateway.SetValue( gatewayEntity.Guid.ToString() );
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
            }

            SetEditMode( true );

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
                .Add( "Amount", transaction.Amount )
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
                transaction = new FinancialTransactionService().Get( itemKeyValue );
            }
            else
            {
                transaction = new FinancialTransaction { Id = 0 };
            }

            hfIdTransValue.Value = transaction.Id.ToString();
            hfBatchId.Value = PageParameter( "financialBatchId" );

            bool readOnly = false;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
            }

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
            }
            else
            {
                lbEdit.Visible = false;
                ShowSummary( transaction );
            }

            lbSave.Visible = !readOnly;

            // Load the TransactionDetails grid here if this transaction already exists.
            if ( transaction.Id != 0 )
            {
                var financialTransactionDetails = new FinancialTransactionDetailService().Queryable().ToList();
                gTransactionDetails.DataSource = financialTransactionDetails;
                gTransactionDetails.DataBind();
                gTransactionDetails.Actions.ShowAdd = true;
                pnlTransactionDetails.Visible = true;
            }
            else
            {
                pnlTransactionDetails.Visible = false;
            }
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

        #endregion Internal Methods
    }
}