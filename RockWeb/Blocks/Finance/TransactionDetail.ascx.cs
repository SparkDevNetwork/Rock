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

            var transaction = new Rock.Model.FinancialTransactionService().Get( transactionId );

            if ( transaction != null )
            {
                lValue.Text = "Edit";

                hfIdTransValue.Value = transaction.Id.ToString();
                tbAmount.Text = transaction.Amount.ToString();
                hfBatchId.Value = transaction.BatchId.ToString();
                ddlCreditCardType.SetValue( transaction.CreditCardTypeValueId );
                ddlCurrencyType.SetValue( transaction.CurrencyTypeValueId );
                ddlPaymentGateway.SetValue( transaction.GatewayId );
                ddlSourceType.SetValue( transaction.SourceTypeValueId );
                ddlTransactionType.SetValue( transaction.TransactionTypeValueId );
                tbSummary.Text = transaction.Summary;
                tbTransactionCode.Text = transaction.TransactionCode;
                dtTransactionDateTime.Text = transaction.TransactionDateTime.ToString();
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

                if ( ddlCreditCardType.SelectedValue != Rock.Constants.All.IdValue )
                {
                    financialTransaction.CreditCardTypeValueId = int.Parse( ddlCreditCardType.SelectedValue );
                }
                if ( ddlCurrencyType.SelectedValue != Rock.Constants.All.IdValue )
                {
                    financialTransaction.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
                }

                if ( !string.IsNullOrEmpty( ddlPaymentGateway.SelectedValue ) && ddlPaymentGateway.SelectedValue != Rock.Constants.All.IdValue )
                {
                    financialTransaction.GatewayId = int.Parse( ddlPaymentGateway.SelectedValue );
                }
                if ( ddlSourceType.SelectedValue != Rock.Constants.All.IdValue )
                {
                    financialTransaction.SourceTypeValueId = int.Parse( ddlSourceType.SelectedValue );
                }
                if ( ddlTransactionType.SelectedValue != Rock.Constants.All.IdValue )
                {
                    financialTransaction.TransactionTypeValueId = int.Parse( ddlTransactionType.SelectedValue );
                }

                financialTransaction.Summary = tbSummary.Text;
                financialTransaction.TransactionCode = tbTransactionCode.Text;
                financialTransaction.TransactionDateTime = dtTransactionDateTime.SelectedDateTime;

                financialTransactionService.Save( financialTransaction, CurrentPersonId );

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
        protected void btnCancelTransaction_Click( object sender, EventArgs e )
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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the form.
        /// </summary>
        protected void BindForm()
        {
            try
            {
                string batchId = PageParameter( "financialBatchId" );
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
