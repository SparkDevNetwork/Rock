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


public partial class Blocks_Finance_Transaction : Rock.Web.UI.RockBlock, IDetailBlock
{
    #region Control Methods
    private string contextTypeName = string.Empty;

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
            BindDrops();
            BindForm();
        }
    }

    #endregion

    private void DisplayError( string message )
    {
        pnlMessage.Controls.Clear();
        pnlMessage.Controls.Add( new LiteralControl( message ) );
        pnlMessage.Visible = true;
    }

    protected void BindDrops()
    {
        BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
        BindDefinedTypeDropdown( ddlCreditCardType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
        BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source" );
        BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );        
    }

    private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userPreferenceKey )
    {
        ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
        ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

    }

    #region add, edit and delete blocks Financial Transaction
    protected void BindForm()
    {
        try
        {
            var vid = PageParameter( "transactionId" );
            var vbatch = PageParameter( "batchId" );
            if (  vbatch == null )
                return;

            int id = 0;
            int.TryParse( vid.ToString() , out id);
            int batch = 0;
            int.TryParse( vbatch.ToString(), out batch );
            ShowTransactionEditValue( id, batch );

        }
        catch ( Exception exp )
        {
            Response.Write( "The access request was unclear. Please fix the following: " + exp.Message );            
            Response.End();
        }
    }
   
    /// <summary>
    /// Shows the edit value.
    /// </summary>
    /// <param name="transactionId">The transactionId id.</param>
    /// <param name="setValues">if set to <c>true</c> [set values].</param>
    protected void ShowTransactionEditValue( int transactionId, int batchid )
    {

        emptyTransactionInputs();
        hfIdTransValue.Value = transactionId.ToString();
        hfBatchId.Value = batchid.ToString();

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

    protected void btnSaveFinancialTransaction_Click( object sender, EventArgs e )
    {
        using ( new Rock.Data.UnitOfWorkScope() )
        {
            var financialTransactionService = new Rock.Model.FinancialTransactionService();
            Rock.Model.FinancialTransaction financialTransaction = null;
            int financialTransactionId = string.IsNullOrEmpty( hfIdTransValue.Value ) ? 0: Int32.Parse( hfIdTransValue.Value ) ;
            
            int batchid = string.IsNullOrEmpty( hfBatchId.Value ) ? 0 : Int32.Parse( hfBatchId.Value ) ;

            if ( financialTransactionId == 0 )
            {
                financialTransaction = new Rock.Model.FinancialTransaction();
                financialTransactionService.Add( financialTransaction, CurrentPersonId );
                financialTransaction.BatchId = batchid;
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
 
            if ( !string.IsNullOrEmpty( ddlPaymentGateway.SelectedValue) && ddlPaymentGateway.SelectedValue != "-1" )
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

            NavigateToParentPage();
        }
    }


    protected void emptyTransactionInputs()
    {

        hfIdTransValue.Value = string.Empty;
        tbAmount.Text = string.Empty;
        hfBatchId.Value = string.Empty;
        ddlCreditCardType.SetValue( -1 );
        ddlCurrencyType.SetValue( -1 );
        ddlPaymentGateway.SetValue( -1 );
        ddlSourceType.SetValue( -1 );
        ddlTransactionType.SetValue( -1 );
        tbSummary.Text = string.Empty;
        tbTransactionCode.Text = string.Empty;
        dtTransactionDateTime.SelectedDate = DateTime.Now;
    }

    protected void btnCancelFinancialTransaction_Click( object sender, EventArgs e )
    {
        emptyTransactionInputs();
        NavigateToParentPage();
    }

    #endregion


    public void ShowDetail( string itemKey, int itemKeyValue )
    {

        if ( !itemKey.Equals( "transactionId" ) && !!itemKey.Equals( "batchfk" ) )
            {
                return;
            }
    }
}