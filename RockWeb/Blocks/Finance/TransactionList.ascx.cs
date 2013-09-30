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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [ContextAware]
    [LinkedPage("Detail Page")]
    public partial class TransactionList : Rock.Web.UI.RockBlock
    {
        #region Fields
        
        private bool _canConfigure = false;
        private FinancialBatch _batch = null;
        private Person _person = null;

        #endregion

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

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                rGridTransactions.DataKeyNames = new string[] { "id" };
                rGridTransactions.Actions.ShowAdd = true;
                rGridTransactions.Actions.AddClick += rGridTransactions_Add;
                rGridTransactions.GridRebind += rGridTransactions_GridRebind;                

                // enable delete transaction
                rGridTransactions.Columns[rGridTransactions.Columns.Count-1].Visible = true;
            }
            else
            {
                DisplayError( "You are not authorized to configure this page" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad(e);

            var contextEntity = this.ContextEntity();
            if (contextEntity != null)
            {
                dtStartDate.Visible = false;
                dtEndDate.Visible = false;
                txtFromAmount.Visible = false;
                txtToAmount.Visible = false;
                txtTransactionCode.Visible = false;

                if (contextEntity is Person)
                {
                    _person = contextEntity as Person;
                }
                else if (contextEntity is FinancialBatch)
                {
                    _batch = contextEntity as FinancialBatch;
                }
            }

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
                        var service = new FinancialAccountService();
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
            rFilter.SaveUserPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty);
            rFilter.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Source", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rGridTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Add( object sender, EventArgs e )
        {
            ShowDetailForm( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Edit( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ShowDetailForm( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var financialTransactionService = new Rock.Model.FinancialTransactionService();

            FinancialTransaction financialTransaction = financialTransactionService.Get( (int)e.RowKeyValue );
            if ( financialTransaction != null )
            {
                financialTransactionService.Delete( financialTransaction, CurrentPersonId );
                financialTransactionService.Save( financialTransaction, CurrentPersonId );
            }

            BindGrid();
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            //DateTime fromDate;
            //if ( !DateTime.TryParse( rFilter.GetUserPreference( "From Date" ), out fromDate ) )
            //{
            //    fromDate = DateTime.Today;
            //}
            //dtStartDate.Text = fromDate.ToShortDateString();
            dtStartDate.Text = rFilter.GetUserPreference( "From Date" );
            dtEndDate.Text = rFilter.GetUserPreference( "To Date" );
            txtFromAmount.Text = rFilter.GetUserPreference( "From Amount" );
            txtToAmount.Text = rFilter.GetUserPreference( "To Amount" );
            txtTransactionCode.Text = rFilter.GetUserPreference( "Transaction Code" );

            var accountService = new FinancialAccountService();
            foreach ( FinancialAccount account in accountService.Queryable() )
            {
                ListItem li = new ListItem( account.Name, account.Id.ToString() );
                li.Selected = account.Id.ToString() == rFilter.GetUserPreference( "Account" );
                ddlAccount.Items.Add( li );
            }
            ddlAccount.Items.Insert( 0, Rock.Constants.All.ListItem );

            BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
            BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType,new Guid(  Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source" );
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
            ListControl.Items.Insert( 0, Rock.Constants.All.ListItem );

            if ( !string.IsNullOrWhiteSpace( rFilter.GetUserPreference( userPreferenceKey ) ) )
            {
                ListControl.SelectedValue = rFilter.GetUserPreference( userPreferenceKey );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            TransactionSearchValue searchValue = GetSearchValue();
            SortProperty sortProperty = rGridTransactions.SortProperty;
            
            var transactionService = new FinancialTransactionService();
            var queryable = transactionService.Get( searchValue );

            if ( _batch != null )
            {
                queryable = queryable.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );
            }

            if ( _person != null )
            {
                queryable = queryable.Where( t => t.AuthorizedPersonId == _person.Id );
            }

            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( t => t.TransactionDateTime );
            }

            rGridTransactions.DataSource = queryable.ToList();
            rGridTransactions.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "transactionId", id );
        }

        /// <summary>
        /// Gets the search value.
        /// </summary>
        /// <returns></returns>
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

            DateTime? fromTransactionDate = dtStartDate.SelectedDate;
            DateTime? toTransactionDate = dtEndDate.SelectedDate;
            searchValue.DateRange = new RangeValue<DateTime?>( dtStartDate.SelectedDate, dtEndDate.SelectedDate );
            
            if ( !string.IsNullOrEmpty( txtTransactionCode.Text )  )
            {
                searchValue.TransactionCode = txtTransactionCode.Text;
            }
            
            if ( ddlAccount.SelectedValue != Rock.Constants.All.IdValue )
            {
                searchValue.AccountId = ddlAccount.SelectedValueAsInt();
            }

            if ( ddlTransactionType.SelectedValue != Rock.Constants.All.IdValue )
            {
                searchValue.TransactionTypeValueId = ddlTransactionType.SelectedValueAsInt();
            }

            if ( ddlCurrencyType.SelectedValue != Rock.Constants.All.IdValue )
            {
                searchValue.CurrencyTypeValueId = ddlCurrencyType.SelectedValueAsInt();
            }

            if ( ddlCreditCardType.SelectedValue != Rock.Constants.All.IdValue )
            {
                searchValue.CreditCardTypeValueId = ddlCreditCardType.SelectedValueAsInt();
            }

            if ( ddlSourceType.SelectedValue != Rock.Constants.All.IdValue )
            {
                searchValue.SourceTypeValueId = ddlSourceType.SelectedValueAsInt();
            }

            return searchValue;
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
