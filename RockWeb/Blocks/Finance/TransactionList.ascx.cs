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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction List" )]
    [Category( "Finance" )]
    [Description( "Builds a list of all financial transactions which can be filtered by date, account/fund, transaction type, etc." )]
    [ContextAware]
    [LinkedPage( "Detail Page" )]
    [TextField( "Title", "Title to display above the grid. Leave blank to hide.", false )]
    public partial class TransactionList : Rock.Web.UI.RockBlock, ISecondaryBlock
    {
        #region Fields

        private bool _canConfigure = false;
        private FinancialBatch _batch = null;
        private Person _person = null;

        #endregion Fields

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfTransactions.ApplyFilterClick += gfTransactions_ApplyFilterClick;
            gfTransactions.DisplayFilterValue += gfTransactions_DisplayFilterValue;

            _canConfigure = IsUserAuthorized( Authorization.EDIT );

            if ( _canConfigure )
            {
                gTransactions.DataKeyNames = new string[] { "id" };
                gTransactions.Actions.ShowAdd = true;
                gTransactions.Actions.AddClick += gTransactions_Add;
                gTransactions.GridRebind += gTransactions_GridRebind;

                // enable delete transaction
                gTransactions.Columns[gTransactions.Columns.Count - 1].Visible = true;
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
            base.OnLoad( e );

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
                else if ( contextEntity is FinancialBatch )
                {
                    _batch = contextEntity as FinancialBatch;
                    gfTransactions.Visible = false;
                }
            }

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gfTransactions_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Amount Range":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                    break;

                case "Account":

                    int accountId = 0;
                    if ( int.TryParse( e.Value, out accountId ) )
                    {
                        var service = new FinancialAccountService( new RockContext() );
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
        /// Handles the ApplyFilterClick event of the gfTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfTransactions_ApplyFilterClick( object sender, EventArgs e )
        {
            gfTransactions.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            gfTransactions.SaveUserPreference( "Amount Range", nreAmount.DelimitedValues );
            gfTransactions.SaveUserPreference( "Transaction Code", tbTransactionCode.Text );
            gfTransactions.SaveUserPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Transaction Type", ddlTransactionType.SelectedValue != All.Id.ToString() ? ddlTransactionType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Source Type", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gTransactions_Add( object sender, EventArgs e )
        {
            ShowDetailForm( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gTransactions_Edit( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            if ( _batch != null && _batch.Status != BatchStatus.Open )
            {
                BindGrid();
                return;
            }

            ShowDetailForm( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gTransactions_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            FinancialTransactionService service = new FinancialTransactionService( rockContext );
            FinancialTransaction item = service.Get( e.RowKeyId );
            if ( item != null )
            {
                string errorMessage;
                if ( !service.CanDelete( item, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( item );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }
        
        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
            nreAmount.DelimitedValues = gfTransactions.GetUserPreference( "Amount Range" );
            tbTransactionCode.Text = gfTransactions.GetUserPreference( "Transaction Code" );

            var accountService = new FinancialAccountService( new RockContext() );
            ddlAccount.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( FinancialAccount account in accountService.Queryable() )
            {
                ListItem li = new ListItem( account.Name, account.Id.ToString() );
                li.Selected = account.Id.ToString() == gfTransactions.GetUserPreference( "Account" );
                ddlAccount.Items.Add( li );
            }

            BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
            BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source Type" );
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
            listControl.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            if ( !string.IsNullOrWhiteSpace( gfTransactions.GetUserPreference( userPreferenceKey ) ) )
            {
                listControl.SelectedValue = gfTransactions.GetUserPreference( userPreferenceKey );
            }
        }

        /// <summary>
        /// Refreshes the list. Public method...can be called from other blocks.
        /// </summary>
        public void RefreshList()
        {
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is FinancialBatch )
                {
                    var batchId = PageParameter( "batchId" );
                    var batch = new FinancialBatchService( new RockContext() ).Get( int.Parse( batchId ) );
                    _batch = batch;
                    BindGrid();
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            int batchEntityTypeId = EntityTypeCache.Read( "Rock.Model.FinancialBatch" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == batchEntityTypeId ) && _batch == null )
            {
                return;
            }

            var queryable = new FinancialTransactionService( new RockContext() ).Queryable();

            // Set up the selection filter
            if ( _batch != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                queryable = queryable.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );

                // If the batch is closed, do not allow any editing of the transactions
                if ( _batch.Status != BatchStatus.Open && _canConfigure )
                {
                    gTransactions.Actions.ShowAdd = false;
                    gTransactions.IsDeleteEnabled = false;
                }
                else
                {
                    gTransactions.Actions.ShowAdd = true;
                    gTransactions.IsDeleteEnabled = true;
                }
            }
            else if ( !string.IsNullOrWhiteSpace( PageParameter( "batchId" ) ) && _batch == null )
            {
                // this makes sure the grid will show no transactions when you're adding a new financial batch.
                queryable = queryable.Where( t => t.BatchId.HasValue && t.BatchId.Value == 0 );
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
                drp.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
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
                nre.DelimitedValues = gfTransactions.GetUserPreference( "Amount Range" );
                if ( nre.LowerValue.HasValue )
                {
                    queryable = queryable.Where( t => t.TransactionDetails.Sum( d => d.Amount ) >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    queryable = queryable.Where( t => t.TransactionDetails.Sum( d => d.Amount ) <= nre.UpperValue.Value );
                }

                // Transaction Code
                string transactionCode = gfTransactions.GetUserPreference( "Transaction Code" );
                if ( !string.IsNullOrWhiteSpace( transactionCode ) )
                {
                    queryable = queryable.Where( t => t.TransactionCode == transactionCode.Trim() );
                }

                // Account Id
                int accountId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Account" ), out accountId ) )
                {
                    queryable = queryable.Where( t => t.TransactionDetails.Any( d => d.AccountId == accountId ) );
                }

                // Transaction Type
                int transactionTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Transaction Type" ), out transactionTypeId ) )
                {
                    queryable = queryable.Where( t => t.TransactionTypeValueId == transactionTypeId );
                }

                // Currency Type
                int currencyTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Currency Type" ), out currencyTypeId ) )
                {
                    queryable = queryable.Where( t => t.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    queryable = queryable.Where( t => t.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Source Type" ), out sourceTypeId ) )
                {
                    queryable = queryable.Where( t => t.SourceTypeValueId == sourceTypeId );
                }
            }

            SortProperty sortProperty = gTransactions.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "TotalAmount" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        queryable = queryable.OrderBy( t => t.TransactionDetails.Sum( d => d.Amount ) );
                    }
                    else
                    {
                        queryable = queryable.OrderByDescending( t => t.TransactionDetails.Sum( d => d.Amount ) );
                    }
                }
                else
                {
                    queryable = queryable.Sort( sortProperty );
                }
            }
            else
            {
                queryable = queryable.OrderBy( t => t.TransactionDateTime );
            }

            gTransactions.DataSource = queryable.AsNoTracking().ToList();
            gTransactions.DataBind();
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
                qryParams.Add( "batchId", _batch.Id.ToString() );
                qryParams.Add( "transactionId", id.ToString() );
                NavigateToLinkedPage( "DetailPage", qryParams );
            }
            else if ( _person != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "personId", _person.Id.ToString() );
                qryParams.Add( "transactionId", id.ToString() );
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

        #endregion Internal Methods

    }
}