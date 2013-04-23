//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Collections.Generic;

namespace RockWeb.Blocks.Finance.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    public partial class TransactionDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        # region Fields
        private string contextTypeName = string.Empty;

        #endregion

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
                BindDropdowns();
                BindForm();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="transactionId">The transactionId id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowTransactionEditValue( int transactionId, string batchId )
        {
            hfIdTransValue.Value = transactionId.ToString();
            hfBatchId.Value = batchId;

            var transactionValue = new Rock.Model.FinancialTransactionService().Get( transactionId );

            if ( transactionValue != null )
            {
                lValue.Text = "Edit";

                hfIdTransValue.Value = transactionValue.Id.ToString();
                tbAmount.Text = transactionValue.Amount.ToString();
                hfBatchId.Value = transactionValue.BatchId.ToString();
                ddlCreditCardType.SetValue( transactionValue.CreditCardTypeValueId );
                ddlCurrencyType.SetValue( transactionValue.CurrencyTypeValueId );
                ddlPaymentGateway.SetValue( transactionValue.GatewayId );
                ddlSourceType.SetValue( transactionValue.SourceTypeValueId );
                ddlTransactionType.SetValue( transactionValue.TransactionTypeValueId );
                tbSummary.Text = transactionValue.Summary;
                tbTransactionCode.Text = transactionValue.TransactionCode;
                dtTransactionDateTime.Text = transactionValue.TransactionDateTime.ToString();
            }
            else
            {
                lValue.Text = "Add";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveFinancialTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveTransaction_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialTransactionService = new Rock.Model.FinancialTransactionService();
                Rock.Model.FinancialTransaction financialTransaction = null;
                int financialTransactionId = !string.IsNullOrEmpty( hfIdTransValue.Value ) ? Int32.Parse( hfIdTransValue.Value ) : 0;

                // null if not associated with a batch
                int? batchId = hfBatchId.Value.AsInteger();

                if ( financialTransactionId == 0 )
                {
                    financialTransaction = new Rock.Model.FinancialTransaction();
                    financialTransactionService.Add( financialTransaction, CurrentPersonId );
                    financialTransaction.BatchId = batchId;
                }
                else
                {
                    financialTransaction = financialTransactionService.Get( financialTransactionId );
                }

                financialTransaction.AuthorizedPersonId = CurrentPerson.Id;
                decimal Amount = 0M;
                decimal.TryParse( tbAmount.Text.Replace( "$", string.Empty ), out Amount );
                financialTransaction.Amount = Amount;

                if ( ddlCreditCardType.SelectedValue != "-1" )
                {
                    financialTransaction.CreditCardTypeValueId = int.Parse( ddlCreditCardType.SelectedValue );
                }
                if ( ddlCurrencyType.SelectedValue != "-1" )
                {
                    financialTransaction.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
                }

                if ( !string.IsNullOrEmpty( ddlPaymentGateway.SelectedValue ) && ddlPaymentGateway.SelectedValue != "-1" )
                {
                    financialTransaction.GatewayId = int.Parse( ddlPaymentGateway.SelectedValue );
                }
                if ( ddlSourceType.SelectedValue != "-1" )
                {
                    financialTransaction.SourceTypeValueId = int.Parse( ddlSourceType.SelectedValue );
                }
                if ( ddlTransactionType.SelectedValue != "-1" )
                {
                    financialTransaction.TransactionTypeValueId = int.Parse( ddlTransactionType.SelectedValue );
                }
                financialTransaction.Summary = tbSummary.Text;
                financialTransaction.TransactionCode = tbTransactionCode.Text;
                financialTransaction.TransactionDateTime = dtTransactionDateTime.SelectedDate;

                financialTransactionService.Save( financialTransaction, CurrentPersonId );

                // is this 
                if ( batchId != null )
                {
                    NavigateToDetailPage( "financialBatchId", (int)financialTransaction.BatchId );
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
        protected void btnCancelTransaction_Click( object sender, EventArgs e )
        {
            NavigateToDetailPage( "financialBatchId", !string.IsNullOrEmpty( hfBatchId.Value ) ? Int32.Parse( hfBatchId.Value ) : 0 );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the form.
        /// </summary>
        protected void BindForm()
        {
            try
            {
                string batchId = PageParameter( "batchId" );
                string transactionId = PageParameter( "transactionId" );

                if ( !string.IsNullOrEmpty( transactionId ) )
                {
                    ShowTransactionEditValue( Convert.ToInt32( transactionId ), batchId );
                }
                else
                {
                    ShowTransactionEditValue( 0, batchId );
                }
                
            }
            catch ( Exception exp )
            {
                Response.Write( "The access request was unclear. Please fix the following: " + exp.Message );
                Response.End();
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
        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            //ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

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

        #endregion
    }
}
