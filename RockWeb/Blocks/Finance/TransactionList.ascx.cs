//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
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

            _canConfigure = RockPage.IsAuthorized( "Edit", CurrentPerson );

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
                DisplayError( "You are not authorized to edit these transactions" );
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
            if (contextEntity  != null)
            {
                if (contextEntity is Person)
                {
                    _person = contextEntity as Person;
                }
                else if (contextEntity is FinancialBatch)
                {
                    _batch = contextEntity as FinancialBatch;
                    rFilter.Visible = false;
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
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues(e.Value);
                    break;

                case "Amount Range":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                    break;

                case "Account":

                    int accountId = 0;
                    if ( int.TryParse( e.Value, out accountId ) )
                    {
                        var service = new FinancialAccountService();
                        var account = service.Get( accountId );
                        if ( account != null )
                        {
                            e.Value = account.Name;
                        }
                    }

                    break;

                case "Transaction Type":
                case "Currency Type":
                case "Credit Card Type":
                case "Source Type":

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
            rFilter.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            rFilter.SaveUserPreference( "Amount Range", nreAmount.DelimitedValues );
            rFilter.SaveUserPreference( "Transaction Code", txtTransactionCode.Text );
            rFilter.SaveUserPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty);
            rFilter.SaveUserPreference( "Transaction Type", ddlTransactionType.SelectedValue != All.Id.ToString() ? ddlTransactionType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Source Type", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

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
            drpDates.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
            nreAmount.DelimitedValues = rFilter.GetUserPreference( "Amount Range" );
            txtTransactionCode.Text = rFilter.GetUserPreference( "Transaction Code" );

            var accountService = new FinancialAccountService();
            ddlAccount.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( FinancialAccount account in accountService.Queryable() )
            {
                ListItem li = new ListItem( account.Name, account.Id.ToString() );
                li.Selected = account.Id.ToString() == rFilter.GetUserPreference( "Account" );
                ddlAccount.Items.Add( li );
            }

            BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
            BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType,new Guid(  Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source Type" );
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
            ListControl.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

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
            var queryable = new FinancialTransactionService().Queryable();

            // Set up the selection filter
            if ( _batch != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                queryable = queryable.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );
            }
            else
            {
                // otherwise set the selection based on filter settings
                if ( _person != null )
                {
                    queryable = queryable.Where( t => t.AuthorizedPersonId == _person.Id );
                }

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    queryable = queryable.Where( t => t.TransactionDateTime >= drp.LowerValue.Value );
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    queryable = queryable.Where( t => t.TransactionDateTime < upperDate );
                }

                // Amount Range
                var nre = new NumberRangeEditor();
                nre.DelimitedValues = rFilter.GetUserPreference( "Amount Range" );
                if ( nre.LowerValue.HasValue )
                {
                    queryable = queryable.Where( t => t.Amount >= nre.LowerValue.Value );
                }
                if ( nre.UpperValue.HasValue )
                {
                    queryable = queryable.Where( t => t.Amount <= nre.UpperValue.Value );
                }

                // Transaction Code
                string transactionCode = rFilter.GetUserPreference( "Transaction Code" );
                if ( !string.IsNullOrWhiteSpace( transactionCode ) )
                {
                    queryable = queryable.Where( t => t.TransactionCode == transactionCode.Trim() );
                }

                // Account Id
                int accountId = int.MinValue; 
                if ( int.TryParse( rFilter.GetUserPreference( "Account" ), out accountId ) )
                {
                    queryable = queryable.Where( t => t.TransactionDetails.Any( d => d.AccountId == accountId ) );
                }

                // Transaction Type
                int transactionTypeId = int.MinValue;
                if ( int.TryParse( rFilter.GetUserPreference( "Transaction Type" ), out transactionTypeId ) )
                {
                    queryable = queryable.Where( t => t.TransactionTypeValueId == transactionTypeId );
                }

                // Currency Type
                int currencyTypeId = int.MinValue;
                if ( int.TryParse( rFilter.GetUserPreference( "Currency Type" ), out currencyTypeId ) )
                {
                    queryable = queryable.Where( t => t.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( rFilter.GetUserPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    queryable = queryable.Where( t => t.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( rFilter.GetUserPreference( "Source Type" ), out sourceTypeId ) )
                {
                    queryable = queryable.Where( t => t.SourceTypeValueId == sourceTypeId );
                }
            }

            SortProperty sortProperty = rGridTransactions.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( t => t.TransactionDateTime );
            }

            rGridTransactions.DataSource = queryable.AsNoTracking().ToList();
            rGridTransactions.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            if ( _batch != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "financialBatchId", _batch.Id.ToString() );
                qryParams.Add( "transactionid", id.ToString() );
                NavigateToLinkedPage( "DetailPage", qryParams );
            }
            else if ( _person != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "personId", _person.Id.ToString() );
                qryParams.Add( "transactionid", id.ToString() );
                NavigateToLinkedPage( "DetailPage", qryParams );
            }
            else
            {
                NavigateToLinkedPage( "DetailPage", "transactionId", id );
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
