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
    [Description( "Builds a list of all financial transactions which can be filtered by date, account, transaction type, etc." )]

    [ContextAware]
    [LinkedPage( "Detail Page" )]
    [TextField( "Title", "Title to display above the grid. Leave blank to hide.", false )]
    public partial class TransactionList : Rock.Web.UI.RockBlock, ISecondaryBlock, IPostBackEventHandler
    {
        #region Fields

        private bool _canEdit = false;
        private FinancialBatch _batch = null;
        private Person _person = null;
        private FinancialScheduledTransaction _scheduledTxn = null;

        private RockDropDownList _ddlMove = new RockDropDownList();

        // Dictionaries to cache values for databinding performance
        private Dictionary<int, string> _currencyTypes;
        private Dictionary<int, string> _creditCardTypes;

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

            string title = GetAttributeValue( "Title" );
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Transaction List";
            }
            lTitle.Text = title;

            _canEdit = UserCanEdit;

            gTransactions.DataKeyNames = new string[] { "Id" };
            gTransactions.Actions.ShowAdd = _canEdit;
            gTransactions.Actions.AddClick += gTransactions_Add;
            gTransactions.GridRebind += gTransactions_GridRebind;
            gTransactions.RowDataBound += gTransactions_RowDataBound;
            gTransactions.IsDeleteEnabled = _canEdit;

            // enable delete transaction
            gTransactions.Columns[gTransactions.Columns.Count - 1].Visible = true;

            int currentBatchId = PageParameter( "batchId" ).AsInteger();

            if ( _canEdit )
            {
                _ddlMove.ID = "ddlMove";
                _ddlMove.CssClass = "pull-left input-width-xl";
                _ddlMove.DataValueField = "Id";
                _ddlMove.DataTextField = "Name";
                _ddlMove.DataSource = new FinancialBatchService( new RockContext() )
                    .Queryable()
                    .Where( b =>
                        b.Status == BatchStatus.Open &&
                        b.BatchStartDateTime.HasValue &&
                        b.Id != currentBatchId )
                    .OrderBy( b => b.Name )
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        b.BatchStartDateTime
                    } )
                    .ToList()
                    .Select( b => new
                    {
                        b.Id,
                        Name = string.Format( "{0} ({1})", b.Name, b.BatchStartDateTime.Value.ToString( "d" ) )
                    } )
                    .ToList();
                _ddlMove.DataBind();
                _ddlMove.Items.Insert( 0, new ListItem( "-- Move Transactions To Batch --", "" ) );
                gTransactions.Actions.AddCustomActionControl( _ddlMove );
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upTransactions );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            bool promptWithFilter = true;
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                    promptWithFilter = false;
                }
                else if ( contextEntity is FinancialBatch )
                {
                    _batch = contextEntity as FinancialBatch;
                    gfTransactions.Visible = false;
                    promptWithFilter = false;
                }
                else if ( contextEntity is FinancialScheduledTransaction )
                {
                    _scheduledTxn = contextEntity as FinancialScheduledTransaction;
                    gfTransactions.Visible = false;
                    promptWithFilter = false;
                }
            }

            if ( !Page.IsPostBack )
            {
                BindFilter();

                if ( promptWithFilter && gfTransactions.Visible )
                {
                    //// NOTE: Special Case for this List Block since there could be a very large number of transactions:
                    //// If the filter is shown and we aren't filtering by anything else, don't automatically populate the grid. Wait for them to hit apply on the filter
                    gfTransactions.Show();
                }
                else
                {
                    BindGrid();
                }
            }

            if ( _canEdit && _batch != null )
            {
                string script = string.Format( @"
    $('#{0}').change(function( e ){{
        var count = $(""#{1} input[id$='_cbSelect_0']:checked"").length;
        if (count == 0) {{
            eval({2});
        }}
        else
        {{
            var $ddl = $(this);
            if ($ddl.val() != '') {{
                Rock.dialogs.confirm('Are you sure you want to move the selected transactions to a new batch (the control amounts on each batch will be updated to reflect the moved transaction\'s amounts)?', function (result) {{
                    if (result) {{
                        eval({2});
                    }}
                    $ddl.val('');
                }});
            }}
        }}
    }});
", _ddlMove.ClientID, gTransactions.ClientID, Page.ClientScript.GetPostBackEventReference( this, "MoveTransactions" ) );
                ScriptManager.RegisterStartupScript( _ddlMove, _ddlMove.GetType(), "moveTransaction", script, true );
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            // Set up the selection filter
            if ( _batch != null )
            {
                if ( _batch.Status == BatchStatus.Closed )
                {
                    nbClosedWarning.Visible = true;
                    gTransactions.Columns[0].Visible = false;
                    _ddlMove.Visible = false;
                }
                else
                {
                    nbClosedWarning.Visible = false;
                    gTransactions.Columns[0].Visible = true;
                    _ddlMove.Visible = true;
                }

                // If the batch is closed, do not allow any editing of the transactions
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.Actions.ShowAdd = true;
                    gTransactions.IsDeleteEnabled = true;
                }
                else
                {
                    gTransactions.Actions.ShowAdd = false;
                    gTransactions.IsDeleteEnabled = false;
                }
            }
            else if ( _scheduledTxn != null )
            {
                nbClosedWarning.Visible = false;
                gTransactions.Columns[0].Visible = false;
                _ddlMove.Visible = false;

                gTransactions.Actions.ShowAdd = false;
                gTransactions.IsDeleteEnabled = false;
            }
            else    // Person
            {
                nbClosedWarning.Visible = false;
                gTransactions.Columns[0].Visible = false;
                _ddlMove.Visible = false;
            }
            
            base.OnPreRender( e );
        }

        #endregion Control Methods

        #region Events

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

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
                            e.Value = definedValue.Value;
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
            gfTransactions.SaveUserPreference( "Row Limit", nbRowLimit.Text );
            gfTransactions.SaveUserPreference( "Amount Range", nreAmount.DelimitedValues );
            gfTransactions.SaveUserPreference( "Transaction Code", tbTransactionCode.Text );
            gfTransactions.SaveUserPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Transaction Type", ddlTransactionType.SelectedValue != All.Id.ToString() ? ddlTransactionType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Source Type", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

            BindGrid();
        }

        protected void gTransactions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var txn = e.Row.DataItem as FinancialTransaction;
                var lCurrencyType = e.Row.FindControl( "lCurrencyType" ) as Literal;
                if ( txn != null && lCurrencyType != null )
                {
                    string currencyType = string.Empty;
                    string creditCardType = string.Empty;

                    if ( txn.CurrencyTypeValueId.HasValue )
                    {
                        int currencyTypeId = txn.CurrencyTypeValueId.Value;
                        if ( _currencyTypes.ContainsKey( currencyTypeId ) )
                        {
                            currencyType = _currencyTypes[currencyTypeId];
                        }
                        else
                        {
                            var currencyTypeValue = DefinedValueCache.Read( currencyTypeId );
                            currencyType = currencyTypeValue != null ? currencyTypeValue.Value : string.Empty;
                            _currencyTypes.Add( currencyTypeId, currencyType );
                        }

                        if ( txn.CreditCardTypeValueId.HasValue )
                        {
                            int creditCardTypeId = txn.CreditCardTypeValueId.Value;
                            if ( _creditCardTypes.ContainsKey( creditCardTypeId ) )
                            {
                                creditCardType = _creditCardTypes[creditCardTypeId];
                            }
                            else
                            {
                                var creditCardTypeValue = DefinedValueCache.Read( creditCardTypeId );
                                creditCardType = creditCardTypeValue != null ? creditCardTypeValue.Value : string.Empty;
                                _creditCardTypes.Add( creditCardTypeId, creditCardType );
                            }

                            lCurrencyType.Text = string.Format( "{0} - {1}", currencyType, creditCardType );
                        }
                        else
                        {
                            lCurrencyType.Text = currencyType;
                        }
                    }
                }
            }

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
            ShowDetailForm( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gTransactions_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var transactionService = new FinancialTransactionService( rockContext );
            var transaction = transactionService.Get( e.RowKeyId );
            if ( transaction != null )
            {
                string errorMessage;
                if ( !transactionService.CanDelete( transaction, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                transactionService.Delete( transaction );
                rockContext.SaveChanges();

                RockPage.UpdateBlocks( "~/Blocks/Finance/BatchDetail.ascx" );
            }

            BindGrid();
        }

       public void RaisePostBackEvent( string eventArgument )
        {
            if ( _batch != null )
            {
                if ( eventArgument == "MoveTransactions" &&
                    _ddlMove != null &&
                    _ddlMove.SelectedValue != null &&
                    !String.IsNullOrWhiteSpace( _ddlMove.SelectedValue ) )
                {
                    var txnsSelected = new List<int>();

                    gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                    if ( txnsSelected.Any() )
                    {
                        var rockContext = new RockContext();
                        var batchService = new FinancialBatchService( rockContext );

                        var newBatch = batchService.Get( _ddlMove.SelectedValue.AsInteger() );
                        var oldBatch = batchService.Get( _batch.Id );

                        if ( newBatch != null && newBatch.Status == BatchStatus.Open )
                        {
                            var txnService = new FinancialTransactionService( rockContext );
                            var txnsToUpdate = txnService.Queryable()
                                .Where( t => txnsSelected.Contains( t.Id ) )
                                .ToList();

                            foreach ( var txn in txnsToUpdate )
                            {
                                txn.BatchId = newBatch.Id;
                                oldBatch.ControlAmount -= txn.TotalAmount;
                                newBatch.ControlAmount += txn.TotalAmount;
                            }

                            rockContext.SaveChanges();

                            var pageRef = new Rock.Web.PageReference( RockPage.PageId );
                            pageRef.Parameters = new Dictionary<string, string>();
                            pageRef.Parameters.Add( "batchid", newBatch.Id.ToString() );
                            string newBatchLink = string.Format( "<a href='{0}'>{1}</a>", 
                                pageRef.BuildUrl(), newBatch.Name );

                            RockPage.UpdateBlocks( "~/Blocks/Finance/BatchDetail.ascx" );

                            nbResult.Text = string.Format( "{0} transactions were moved to the '{1}' batch.",
                                txnsToUpdate.Count().ToString( "N0" ), newBatchLink );
                            nbResult.NotificationBoxType = NotificationBoxType.Success;
                            nbResult.Visible = true;
                        }
                        else
                        {
                            nbResult.Text = string.Format( "The selected batch does not exist, or is no longer open." );
                            nbResult.NotificationBoxType = NotificationBoxType.Danger;
                            nbResult.Visible = true;
                        }
                    }
                    else
                    {
                        nbResult.Text = string.Format( "There were not any transactions selected." );
                        nbResult.NotificationBoxType = NotificationBoxType.Warning;
                        nbResult.Visible = true;
                    }
                }

                _ddlMove.SelectedIndex = 0;
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
            nbRowLimit.Text = gfTransactions.GetUserPreference( "Row Limit" );
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
            _currencyTypes = new Dictionary<int,string>();
            _creditCardTypes = new Dictionary<int,string>();

            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            // If configured for a batch and batch is null, return
            int batchEntityTypeId = EntityTypeCache.Read( "Rock.Model.FinancialBatch" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == batchEntityTypeId ) && _batch == null )
            {
                return;
            }

            // If configured for a batch and batch is null, return
            int scheduledTxnEntityTypeId = EntityTypeCache.Read( "Rock.Model.FinancialScheduledTransaction" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == scheduledTxnEntityTypeId ) && _scheduledTxn == null )
            {
                return;
            }

            // Qry
            var qry = new FinancialTransactionService( new RockContext() )
                .Queryable( "AuthorizedPersonAlias.Person,ProcessedByPersonAlias.Person" );

            // Set up the selection filter
            if ( _batch != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                qry = qry.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );

                // If the batch is closed, do not allow any editing of the transactions
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.IsDeleteEnabled = true;
                }
                else
                {
                    gTransactions.IsDeleteEnabled = false;
                }
            }
            else if ( _scheduledTxn != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                qry = qry.Where( t => t.ScheduledTransactionId.HasValue && t.ScheduledTransactionId.Value == _scheduledTxn.Id );

                gTransactions.IsDeleteEnabled = false;
            }
            else    // Person
            {
                // otherwise set the selection based on filter settings
                if ( _person != null )
                {
                    qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == _person.Id );
                }

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.TransactionDateTime < upperDate );
                }

                

                // Amount Range
                var nre = new NumberRangeEditor();
                nre.DelimitedValues = gfTransactions.GetUserPreference( "Amount Range" );
                if ( nre.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDetails.Sum( d => d.Amount ) >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDetails.Sum( d => d.Amount ) <= nre.UpperValue.Value );
                }

                // Transaction Code
                string transactionCode = gfTransactions.GetUserPreference( "Transaction Code" );
                if ( !string.IsNullOrWhiteSpace( transactionCode ) )
                {
                    qry = qry.Where( t => t.TransactionCode == transactionCode.Trim() );
                }

                // Account Id
                int accountId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Account" ), out accountId ) )
                {
                    qry = qry.Where( t => t.TransactionDetails.Any( d => d.AccountId == accountId ) );
                }

                // Transaction Type
                int transactionTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Transaction Type" ), out transactionTypeId ) )
                {
                    qry = qry.Where( t => t.TransactionTypeValueId == transactionTypeId );
                }

                // Currency Type
                int currencyTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Currency Type" ), out currencyTypeId ) )
                {
                    qry = qry.Where( t => t.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    qry = qry.Where( t => t.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Source Type" ), out sourceTypeId ) )
                {
                    qry = qry.Where( t => t.SourceTypeValueId == sourceTypeId );
                }
            }

            SortProperty sortProperty = gTransactions.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "TotalAmount" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        qry = qry.OrderBy( t => t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.00M );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( t => t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M );
                    }
                }
                else
                {
                    qry = qry.Sort( sortProperty );
                }
            }
            else
            {
                // Default sort by Id if the transations are seen via the batch,
                // otherwise sort by descending date time.
                if ( ContextTypesRequired.Any( e => e.Id == batchEntityTypeId ) )
                {
                    qry = qry.OrderBy( t => t.Id );
                }
                else
                {
                    qry = qry.OrderByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
                }
            }

            // Row Limit
            int? rowLimit = gfTransactions.GetUserPreference( "Row Limit" ).AsIntegerOrNull();
            if ( rowLimit.HasValue )
            {
                qry = qry.Take( rowLimit.Value );
            }

            gTransactions.DataSource = qry.AsNoTracking().ToList();
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

        #endregion Internal Methods

    }
}