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
using Rock.Attribute;


public partial class Blocks_Finance_Transaction : Rock.Web.UI.RockBlock, IDetailBlock
{
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

    #region add, edit and delete blocks Financial Transaction
    protected void BindForm()
    {
        try
        {
            var vid = Request.QueryString["id"];
            var vbatch = Request.QueryString["batch"];
            if ( vid == null && vbatch == null )
                return;

            int id = 0;
            int.TryParse( vid.ToString() , out id);
            int batch = 0;
            int.TryParse( vbatch.ToString(), out batch );
            ShowTransactionEditValue( id, batch );

        }
        catch ( Exception exp )
        {
            Response.Write( "Access request was unclear." );
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
        hfIdTransValue.Value = transactionId.ToString();
        hfBatchId.Value = batchid.ToString();

        var transactionValue = new Rock.Model.FinancialTransactionService().Get( transactionId );

        if ( transactionValue != null )
        {
            lValue.Text = "Edit";

            hfIdTransValue.Value = transactionValue.Id.ToString();
            tbAmount.Text = transactionValue.Amount.ToString();
            hfBatchId.Value = transactionValue.BatchId.ToString();
            ddlCreditCartType.SetValue( transactionValue.CreditCardTypeValueId );
            ddlCurrencyType.SetValue( transactionValue.CurrencyTypeValueId );
            tbDescription.Text = transactionValue.Description;
            TranEntity.Text = transactionValue.EntityId.ToString();
            ddlEntityType.SetValue( transactionValue.EntityTypeId );
            ddlPaymentGateway.SetValue( transactionValue.PaymentGatewayId );
            tbRefundTransactionId.Text = transactionValue.RefundTransactionId.ToString();
            ddlSourceType.SetValue( transactionValue.SourceTypeValueId );
            tbSummary.Text = transactionValue.Summary;
            tbTransactionCode.Text = transactionValue.TransactionCode;
            dtTransactionDateTime.Text = transactionValue.TransactionDateTime.ToString();
            tbTransactionImageId.Text = transactionValue.TransactionImageId.ToString();


        }
        else
        {
            lValue.Text = "Add";
            emptyTransactionInputs();
        }


    }

    protected void btnSaveFinancialTransaction_Click( object sender, EventArgs e )
    {
        using ( new Rock.Data.UnitOfWorkScope() )
        {
            var financialTransactionService = new Rock.Model.FinancialTransactionService();
            Rock.Model.FinancialTransaction financialTransaction = null;
            int financialTransactionId = ( hfIdTransValue.Value ) != null ? Int32.Parse( hfIdTransValue.Value ) : 0;
            int batchid = ( hfBatchId.Value ) != null ? Int32.Parse( hfBatchId.Value ) : 0;

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
            decimal Amount = 0M;
            decimal.TryParse( tbAmount.Text.Replace( "$", string.Empty ), out Amount );
            financialTransaction.Amount = Amount;

            if ( ddlCreditCartType.SelectedValue != "-1" )
            {
                financialTransaction.CreditCardTypeValueId = int.Parse( ddlCreditCartType.SelectedValue );
            }
            if ( ddlCurrencyType.SelectedValue != "-1" )
            {
                financialTransaction.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
            }
            financialTransaction.Description = tbDescription.Text;
            if ( TranEntity.SelectedValue != "-1" )
            {
                financialTransaction.EntityId = int.Parse( TranEntity.SelectedValue );
            }
            if ( ddlEntityType.SelectedValue != "-1" )
            {
                financialTransaction.EntityTypeId = int.Parse( ddlEntityType.SelectedValue );
            }
            if ( ddlPaymentGateway.SelectedValue != "-1" )
            {
                financialTransaction.PaymentGatewayId = int.Parse( ddlPaymentGateway.SelectedValue );
            }
            int refundId = 0;
            int.TryParse( tbRefundTransactionId.Text, out refundId );
            financialTransaction.RefundTransactionId = refundId;
            if ( ddlSourceType.SelectedValue != "-1" )
            {
                financialTransaction.SourceTypeValueId = int.Parse( ddlSourceType.SelectedValue );
            }

            financialTransaction.Summary = tbSummary.Text;
            financialTransaction.TransactionCode = tbTransactionCode.Text;
            financialTransaction.TransactionDateTime = dtTransactionDateTime.SelectedDate;

            int imageId = -1;
            int.TryParse( tbTransactionImageId.Text, out imageId );
            financialTransaction.TransactionImageId = imageId;

            financialTransactionService.Save( financialTransaction, CurrentPersonId );
        }

    }


    protected void emptyTransactionInputs()
    {

        hfIdTransValue.Value = string.Empty;
        tbAmount.Text = string.Empty;
        hfBatchId.Value = string.Empty;
        ddlCreditCartType.SetValue( -1 );
        ddlCurrencyType.SetValue( -1 );
        tbDescription.Text = string.Empty;
        TranEntity.SetValue( -1 );
        ddlEntityType.SetValue( -1 );
        ddlPaymentGateway.SetValue( -1 );
        tbRefundTransactionId.Text = string.Empty;
        ddlSourceType.SetValue( -1 );
        tbSummary.Text = string.Empty;
        tbTransactionCode.Text = string.Empty;
        dtTransactionDateTime.SelectedDate = DateTime.MinValue;
        tbTransactionImageId.Text = string.Empty;
        TranCampus.SetValue( -1 );

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