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
using Rock.Constants;
using Rock.Model;
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
            rFilter.SaveUserPreference( "From Date", dtStartDate.Text );
            rFilter.SaveUserPreference( "To Date", dtEndDate.Text );
            rFilter.SaveUserPreference( "From Amount", txtFromAmount.Text );
            rFilter.SaveUserPreference( "To Amount", txtToAmount.Text );
            rFilter.SaveUserPreference( "Transaction Code", txtTransactionCode.Text );
            rFilter.SaveUserPreference( "Fund", ddlFundType.SelectedValue != All.Id.ToString() ? ddlFundType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            DateTime fromDate;
            if ( !DateTime.TryParse( rFilter.GetUserPreference( "From Date" ), out fromDate ) )
            {
                fromDate = DateTime.Today;
            }
            dtStartDate.Text = fromDate.ToShortDateString();
            dtEndDate.Text = rFilter.GetUserPreference( "To Date" );
            txtFromAmount.Text = rFilter.GetUserPreference( "From Amount" );
            txtToAmount.Text = rFilter.GetUserPreference( "To Amount" );
            txtTransactionCode.Text = rFilter.GetUserPreference( "Transaction Code" );

            ddlFundType.Items.Add( new ListItem( All.Text, All.Id.ToString() ) );

            var fundService = new FundService();
            foreach ( Fund fund in fundService.Queryable() )
            {
                ListItem li = new ListItem( fund.Name, fund.Id.ToString() );
                li.Selected = fund.Id.ToString() == rFilter.GetUserPreference( "Fund" );
                ddlFundType.Items.Add( li );
            }

            BindDefinedTypeDropdown( ddlCurrencyType, Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType, Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE, "Credit Card Type" );
        }

        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userValueKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

            if ( !string.IsNullOrWhiteSpace( rFilter.GetUserPreference( userValueKey ) ) )
            {
                ListControl.SelectedValue = rFilter.GetUserPreference( userValueKey );
            }
        }

        private void BindGrid()
        {
            TransactionSearchValue searchValue = GetSearchValue();

            var transactionService = new FinancialTransactionService();
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
                searchValue.CreditCardTypeValueId = int.Parse( ddlCreditCardType.SelectedValue );
            }
            if ( ddlCurrencyType.SelectedValue != All.Id.ToString() )
            {
                searchValue.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
            }
            DateTime? fromTransactionDate = dtStartDate.SelectedDate;
            DateTime? toTransactionDate = dtEndDate.SelectedDate;
            searchValue.DateRange = new RangeValue<DateTime?>( fromTransactionDate, toTransactionDate );
            if ( ddlFundType.SelectedValue != "-1" )
            {
                searchValue.FundId = int.Parse( ddlFundType.SelectedValue );
            }
            searchValue.TransactionCode = txtTransactionCode.Text;
            
            return searchValue;
        }

        #endregion
    }
}