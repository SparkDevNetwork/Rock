//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Core;
using Rock.Financial;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    public partial class Financials : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Fund":

                    int fundId = 0;
                    if ( int.TryParse( e.Value, out fundId ) )
                    {
                        var service = new FundService();
                        var fund = service.Get( fundId );
                        if ( fund != null )
                        {
                            e.Value = fund.Name;
                        }
                    }

                    break;

                case "Currency Type":
                case "Credit Card Type":
                case "Source":

                    int definedValueId = 0;
                    if ( int.TryParse( e.Value, out definedValueId ) )
                    {
                        var definedValue = DefinedValueCache.Read( definedValueId );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Name;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserValue( "From Date", dtStartDate.Text );
            rFilter.SaveUserValue( "To Date", dtEndDate.Text );
            rFilter.SaveUserValue( "From Amount", txtFromAmount.Text );
            rFilter.SaveUserValue( "To Amount", txtToAmount.Text );
            rFilter.SaveUserValue( "Transaction Code", txtTransactionCode.Text );
            rFilter.SaveUserValue( "Fund", ddlFundType.SelectedValue != All.Id.ToString() ? ddlFundType.SelectedValue : string.Empty);
            rFilter.SaveUserValue( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            rFilter.SaveUserValue( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            rFilter.SaveUserValue( "Source", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            DateTime fromDate;
            if ( !DateTime.TryParse( rFilter.GetUserValue( "From Date" ), out fromDate ) )
            {
                fromDate = DateTime.Today;
            }
            dtStartDate.Text = fromDate.ToShortDateString();
            dtEndDate.Text = rFilter.GetUserValue( "To Date" );
            txtFromAmount.Text = rFilter.GetUserValue( "From Amount" );
            txtToAmount.Text = rFilter.GetUserValue( "To Amount" );
            txtTransactionCode.Text = rFilter.GetUserValue( "Transaction Code" );

            ddlFundType.Items.Add( new ListItem( All.Text, All.Id.ToString() ) );

            var fundService = new FundService();
            foreach ( Fund fund in fundService.Queryable() )
            {
                ListItem li = new ListItem( fund.Name, fund.Id.ToString() );
                li.Selected = fund.Id.ToString() == rFilter.GetUserValue( "Fund" );
                ddlFundType.Items.Add( li );
            }

            BindDefinedTypeDropdown( ddlCurrencyType, Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType, Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE, "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source" );
        }

        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userValueKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

            if ( !string.IsNullOrWhiteSpace( rFilter.GetUserValue( userValueKey ) ) )
            {
                ListControl.SelectedValue = rFilter.GetUserValue( userValueKey );
            }
        }

        private void BindGrid()
        {
            TransactionSearchValue searchValue = GetSearchValue();

            var transactionService = new TransactionService();
            grdTransactions.DataSource = transactionService.Get( searchValue ).ToList();
            grdTransactions.DataBind();
        }

        private TransactionSearchValue GetSearchValue()
        {
            TransactionSearchValue searchValue = new TransactionSearchValue();

            decimal? fromAmountRange = null;
            if ( !String.IsNullOrEmpty( txtFromAmount.Text ) )
            {
                fromAmountRange = Decimal.Parse( txtFromAmount.Text );
            }
            decimal? toAmountRange = null;
            if ( !String.IsNullOrEmpty( txtToAmount.Text ) )
            {
                toAmountRange = Decimal.Parse( txtToAmount.Text );
            }
            searchValue.AmountRange = new RangeValue<decimal?>( fromAmountRange, toAmountRange );
            if ( ddlCreditCardType.SelectedValue != All.Id.ToString() )
            {
                searchValue.CreditCardTypeId = int.Parse( ddlCreditCardType.SelectedValue );
            }
            if ( ddlCurrencyType.SelectedValue != All.Id.ToString() )
            {
                searchValue.CurrencyTypeId = int.Parse( ddlCurrencyType.SelectedValue );
            }
            DateTime? fromTransactionDate = dtStartDate.SelectedDate;
            DateTime? toTransactionDate = dtEndDate.SelectedDate;
            searchValue.DateRange = new RangeValue<DateTime?>( fromTransactionDate, toTransactionDate );
            if ( ddlFundType.SelectedValue != "-1" )
            {
                searchValue.FundId = int.Parse( ddlFundType.SelectedValue );
            }
            searchValue.TransactionCode = txtTransactionCode.Text;
            searchValue.SourceTypeId = int.Parse( ddlSourceType.SelectedValue );

            return searchValue;
        }

        #endregion
    }
}