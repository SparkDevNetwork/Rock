// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI.HtmlControls;
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
    [LinkedPage( "Detail Page", order: 0 )]
    [TextField( "Title", "Title to display above the grid. Leave blank to hide.", false, order: 1 )]
    [BooleanField( "Show Only Active Accounts on Filter", "If account filter is displayed, only list active accounts", false, "", 2, "ActiveAccountsOnlyFilter" )]
    [BooleanField( "Show Options", "Show an Options button in the title panel for showing images or summary.", false, order: 3 )]
    [IntegerField( "Image Height", "If the Show Images option is selected, the image height", false, 200, order: 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Types", "Optional list of transation types to limit the list to (if none are selected all types will be included).", false, true, "", "", 5 )]
    public partial class TransactionList : Rock.Web.UI.RockBlock, ISecondaryBlock, IPostBackEventHandler
    {
        private bool _isExporting = false;

        #region Fields

        private bool _canEdit = false;
        private int _imageHeight = 200;
        private FinancialBatch _batch = null;
        private Person _person = null;
        private FinancialScheduledTransaction _scheduledTxn = null;
        private Registration _registration = null;

        private RockDropDownList _ddlMove = new RockDropDownList();
        private LinkButton _lbReassign = new LinkButton();

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
            gfTransactions.ClearFilterClick += gfTransactions_ClearFilterClick;
            gfTransactions.DisplayFilterValue += gfTransactions_DisplayFilterValue;

            SetBlockOptions();

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

                _lbReassign.ID = "lbReassign";
                _lbReassign.CssClass = "btn btn-default btn-sm pull-left";
                _lbReassign.Click += _lbReassign_Click;
                _lbReassign.Text = "Reassign Transactions";
                gTransactions.Actions.AddCustomActionControl( _lbReassign );
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upTransactions );

        }

        /// <summary>
        /// Sets the block options.
        /// </summary>
        private void SetBlockOptions()
        {
            string title = GetAttributeValue( "Title" );
            if ( string.IsNullOrWhiteSpace( title ) )
            {
                title = "Transaction List";
            }

            bddlOptions.Visible = GetAttributeValue( "ShowOptions" ).AsBooleanOrNull() ?? false;
            _imageHeight = GetAttributeValue( "ImageHeight" ).AsIntegerOrNull() ?? 200;

            lTitle.Text = title;
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfTransactions_ClearFilterClick( object sender, EventArgs e )
        {
            gfTransactions.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbClosedWarning.Visible = false;
            nbResult.Visible = false;

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
                else if ( contextEntity is FinancialScheduledTransaction )
                {
                    _scheduledTxn = contextEntity as FinancialScheduledTransaction;
                    gfTransactions.Visible = false;
                }
                else if ( contextEntity is Registration )
                {
                    _registration = contextEntity as Registration;
                    gfTransactions.Visible = false;
                }

            }

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            else
            {
                ShowDialog();
            }

            if ( _canEdit && _batch != null )
            {
                string script = string.Format( @"
    $('#{0}').change(function( e ){{
        var count = $(""#{1} input[id$='_cbSelect_0']:checked"").length;
        if (count == 0) {{
            {2};
        }}
        else
        {{
            var $ddl = $(this);
            if ($ddl.val() != '') {{
                Rock.dialogs.confirm('Are you sure you want to move the selected transactions to a new batch (the control amounts on each batch will be updated to reflect the moved transaction\'s amounts)?', function (result) {{
                    if (result) {{
                        {2};
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
            bool showSelectColumn = false;

            // Set up the selection filter
            if ( _canEdit && _batch != null )
            {
                if ( _batch.Status == BatchStatus.Closed )
                {
                    nbClosedWarning.Visible = true;
                    _ddlMove.Visible = false;
                }
                else
                {
                    nbClosedWarning.Visible = false;
                    showSelectColumn = true;
                    _ddlMove.Visible = true;
                }

                // If the batch is closed, do not allow any editing of the transactions
                // NOTE that gTransactions_Delete click will also check if the transaction is part of a closed batch
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.Actions.ShowAdd = _canEdit;
                    gTransactions.IsDeleteEnabled = _canEdit;
                }
                else
                {
                    gTransactions.Actions.ShowAdd = false;
                    gTransactions.IsDeleteEnabled = false;
                }
            }
            else
            {
                nbClosedWarning.Visible = false;
                _ddlMove.Visible = false;

                // not in batch mode, so don't allow Add, and don't show the DeleteButton
                gTransactions.Actions.ShowAdd = false;
                var deleteField = gTransactions.ColumnsOfType<DeleteField>().FirstOrDefault();
                if ( deleteField != null )
                {
                    deleteField.Visible = false;
                }
            }

            if ( _canEdit && _person != null )
            {
                showSelectColumn = true;
                _lbReassign.Visible = true;
            }
            else
            {
                _lbReassign.Visible = false;
            }

            gTransactions.Columns[0].Visible = showSelectColumn;

            base.OnPreRender( e );
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetBlockOptions();
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
                case "Row Limit":
                    // row limit filter was removed, so hide it just in case
                    e.Value = null;
                    break;

                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Amount Range":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                    break;

                case "Account":

                    var accountIds = e.Value.SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                    if ( accountIds.Any() )
                    {
                        var service = new FinancialAccountService( new RockContext() );
                        var accountNames = service.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).Select( a => a.Name ).ToList().AsDelimited( ", ", " or " );
                        e.Value = accountNames;
                    }
                    else
                    {
                        e.Value = string.Empty;
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

                case "Campus":
                case "CampusAccount":
                    var campus = CampusCache.Read( e.Value.AsInteger() );
                    if ( campus != null )
                    {
                        e.Value = campus.Name;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    if ( e.Key == "Campus" )
                    {
                        e.Name = "Campus (of Batch)";
                    }
                    else if ( e.Key == "CampusAccount" )
                    {
                        e.Name = "Campus (of Account)";
                    }

                    break;

                case "Person":
                    if ( !( this.ContextEntity() is Person ) )
                    {
                        var person = new PersonService( new RockContext() ).Get( e.Value.AsInteger() );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
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
            gfTransactions.SaveUserPreference( "Account", apAccount.SelectedValue != All.Id.ToString() ? apAccount.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Transaction Type", ddlTransactionType.SelectedValue != All.Id.ToString() ? ddlTransactionType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Currency Type", ddlCurrencyType.SelectedValue != All.Id.ToString() ? ddlCurrencyType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Credit Card Type", ddlCreditCardType.SelectedValue != All.Id.ToString() ? ddlCreditCardType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Source Type", ddlSourceType.SelectedValue != All.Id.ToString() ? ddlSourceType.SelectedValue : string.Empty );

            // Campus of Batch
            gfTransactions.SaveUserPreference( "Campus", campCampusBatch.SelectedValue );

            // Campus of Account
            gfTransactions.SaveUserPreference( "CampusAccount", campCampusAccount.SelectedValue );

            gfTransactions.SaveUserPreference( "Person", ppPerson.SelectedValue.ToString() );

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

                    if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
                    {
                        int currencyTypeId = txn.FinancialPaymentDetail.CurrencyTypeValueId.Value;
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

                        if ( txn.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                        {
                            int creditCardTypeId = txn.FinancialPaymentDetail.CreditCardTypeValueId.Value;
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

                    var lTransactionImage = e.Row.FindControl( "lTransactionImage" ) as Literal;
                    if ( lTransactionImage != null && lTransactionImage.Visible )
                    {
                        var firstImage = txn.Images.FirstOrDefault();
                        if ( firstImage != null )
                        {
                            string imageSrc = string.Format( "~/GetImage.ashx?id={0}&height={1}", firstImage.BinaryFileId, _imageHeight );
                            lTransactionImage.Text = string.Format( "<image src='{0}' />", this.ResolveUrl( imageSrc ) );
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gTransactions_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
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

                // prevent deleting a Transaction that is in closed batch
                if ( transaction.Batch != null )
                {
                    if ( transaction.Batch.Status == BatchStatus.Closed )
                    {
                        mdGridWarning.Show( string.Format( "This {0} is assigned to a closed {1}", FinancialTransaction.FriendlyTypeName, FinancialBatch.FriendlyTypeName ), ModalAlertType.Information );
                        return;
                    }
                }

                if ( transaction.BatchId.HasValue )
                {
                    string caption = ( transaction.AuthorizedPersonAlias != null && transaction.AuthorizedPersonAlias.Person != null ) ?
                        transaction.AuthorizedPersonAlias.Person.FullName :
                        string.Format( "Transaction: {0}", transaction.Id );

                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        transaction.BatchId.Value,
                        new List<string> { "Deleted transaction" },
                        caption,
                        typeof( FinancialTransaction ),
                        transaction.Id,
                        false
                    );
                }

                transactionService.Delete( transaction );

                rockContext.SaveChanges();

                RockPage.UpdateBlocks( "~/Blocks/Finance/BatchDetail.ascx" );
            }

            BindGrid();
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( _canEdit && _batch != null )
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

                        if ( oldBatch != null && newBatch != null && newBatch.Status == BatchStatus.Open )
                        {
                            var txnService = new FinancialTransactionService( rockContext );
                            var txnsToUpdate = txnService.Queryable( "AuthorizedPersonAlias.Person" )
                                .Where( t => txnsSelected.Contains( t.Id ) )
                                .ToList();

                            decimal oldBatchControlAmount = oldBatch.ControlAmount;
                            decimal newBatchControlAmount = newBatch.ControlAmount;

                            foreach ( var txn in txnsToUpdate )
                            {
                                string caption = ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null ) ?
                                    txn.AuthorizedPersonAlias.Person.FullName :
                                    string.Format( "Transaction: {0}", txn.Id );

                                var changes = new List<string>();
                                History.EvaluateChange( changes, "Batch",
                                    string.Format( "{0} (Id:{1})", oldBatch.Name, oldBatch.Id ),
                                    string.Format( "{0} (Id:{1})", newBatch.Name, newBatch.Id ) );

                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( FinancialBatch ),
                                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                                    oldBatch.Id,
                                    changes,
                                    caption,
                                    typeof( FinancialTransaction ),
                                    txn.Id,
                                    false
                                );

                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( FinancialBatch ),
                                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                                    newBatch.Id,
                                    changes,
                                    caption,
                                    typeof( FinancialTransaction ),
                                    txn.Id, false
                                );

                                txn.BatchId = newBatch.Id;
                                oldBatchControlAmount -= txn.TotalAmount;
                                newBatchControlAmount += txn.TotalAmount;
                            }

                            var oldBatchChanges = new List<string>();
                            History.EvaluateChange( oldBatchChanges, "Control Amount", oldBatch.ControlAmount.FormatAsCurrency(), oldBatchControlAmount.FormatAsCurrency() );
                            oldBatch.ControlAmount = oldBatchControlAmount;

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                oldBatch.Id,
                                oldBatchChanges,
                                false
                            );

                            var newBatchChanges = new List<string>();
                            History.EvaluateChange( newBatchChanges, "Control Amount", newBatch.ControlAmount.FormatAsCurrency(), newBatchControlAmount.FormatAsCurrency() );
                            newBatch.ControlAmount = newBatchControlAmount;

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                newBatch.Id,
                                newBatchChanges,
                                false
                            );

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

        /// <summary>
        /// Handles the Click event of the _lbReassign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbReassign_Click( object sender, EventArgs e )
        {
            if ( _canEdit && _person != null )
            {
                var txnsSelected = new List<int>();
                gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                if ( txnsSelected.Any() )
                {
                    ShowDialog( "Reassign" );
                }
                else
                {
                    nbResult.Text = string.Format( "There were not any transactions selected." );
                    nbResult.NotificationBoxType = NotificationBoxType.Warning;
                    nbResult.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReassign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReassign_SaveClick( object sender, EventArgs e )
        {
            if ( _canEdit && _person != null )
            {
                int? personAliasId = ppReassign.PersonAliasId;
                var txnsSelected = new List<int>();
                gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                if ( txnsSelected.Any() && personAliasId.HasValue )
                {
                    var rockContext = new RockContext();
                    var txnService = new FinancialTransactionService( rockContext );
                    var txnsToUpdate = txnService.Queryable( "AuthorizedPersonAlias.Person" )
                        .Where( t => txnsSelected.Contains( t.Id ) )
                        .ToList();

                    foreach ( var txn in txnsToUpdate )
                    {
                        txn.AuthorizedPersonAliasId = personAliasId.Value;
                    }

                    rockContext.SaveChanges();
                }
            }

            HideDialog();
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

            apAccount.DisplayActiveOnly = GetAttributeValue( "ActiveAccountsOnlyFilter" ).AsBoolean();

            var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
            if ( accountIds.Any() )
            {
                var service = new FinancialAccountService( new RockContext() );
                var accounts = service.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).ToList();
                apAccount.SetValues( accounts );
            }
            else
            {
                apAccount.SetValue( 0 );
            }

            BindDefinedTypeDropdown( ddlTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
            BindDefinedTypeDropdown( ddlCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( ddlCreditCardType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( ddlSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source Type" );

            if ( this.ContextEntity() == null )
            {
                var campusi = CampusCache.All();
                campCampusBatch.Campuses = campusi;
                campCampusBatch.Visible = campusi.Any();
                campCampusBatch.SetValue( gfTransactions.GetUserPreference( "Campus" ) );

                campCampusAccount.Campuses = campusi;
                campCampusAccount.Visible = campusi.Any();
                campCampusAccount.SetValue( gfTransactions.GetUserPreference( "CampusAccount" ) );
            }
            else
            {
                campCampusBatch.Visible = false;
                campCampusAccount.Visible = false;
            }

            // don't show the person picker if the the current context is already a specific person
            if ( this.ContextEntity() is Person )
            {
                ppPerson.Visible = false;
            }
            else
            {
                ppPerson.Visible = true;
                var personId = gfTransactions.GetUserPreference( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    ppPerson.SetValue( person );
                }
                else
                {
                    ppPerson.SetValue( null );
                }
            }
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
        private void BindGrid( bool isExporting = false )
        {
            _currencyTypes = new Dictionary<int, string>();
            _creditCardTypes = new Dictionary<int, string>();

            // If configured for a registration and registration is null, return
            int registrationEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Registration ) ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == registrationEntityTypeId ) && _registration == null )
            {
                return;
            }

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
            var rockContext = new RockContext();
            var qry = new FinancialTransactionService( rockContext ).Queryable();

            // Transaction Types
            var transactionTypeValueIdList = GetAttributeValue( "TransactionTypes" ).SplitDelimitedValues().AsGuidList().Select( a => DefinedValueCache.Read( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList();

            if ( transactionTypeValueIdList.Any() )
            {
                qry = qry.Where( t => transactionTypeValueIdList.Contains( t.TransactionTypeValueId ) );
            }

            // Set up the selection filter
            if ( _batch != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                qry = qry.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );

                // If the batch is closed, do not allow any editing of the transactions
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.IsDeleteEnabled = _canEdit;
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
            else if ( _registration != null )
            {
                qry = qry
                    .Where( t => t.TransactionDetails
                        .Any( d =>
                            d.EntityTypeId.HasValue &&
                            d.EntityTypeId.Value == registrationEntityTypeId &&
                            d.EntityId.HasValue &&
                            d.EntityId.Value == _registration.Id ) );

                gTransactions.IsDeleteEnabled = false;
            }
            else    // Person
            {
                // otherwise set the selection based on filter settings
                if ( _person != null )
                {
                    // get the transactions for the person or all the members in the person's giving group (Family)
                    qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingId == _person.GivingId );
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
                var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                {
                    if ( accountIds.Any() )
                    {
                        qry = qry.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
                    }
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
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Source Type" ), out sourceTypeId ) )
                {
                    qry = qry.Where( t => t.SourceTypeValueId == sourceTypeId );
                }

                // Campus of Batch and/or Account
                if ( this.ContextEntity() == null )
                {
                    var campusOfBatch = CampusCache.Read( gfTransactions.GetUserPreference( "Campus" ).AsInteger() );
                    if ( campusOfBatch != null )
                    {
                        qry = qry.Where( b => b.Batch != null && b.Batch.CampusId == campusOfBatch.Id );
                    }

                    var campusOfAccount = CampusCache.Read( gfTransactions.GetUserPreference( "CampusAccount" ).AsInteger() );
                    if ( campusOfAccount != null )
                    {
                        qry = qry.Where( b => b.TransactionDetails.Any( a => a.Account.CampusId.HasValue && a.Account.CampusId == campusOfAccount.Id ) );
                    }
                }

                if ( !( this.ContextEntity() is Person ) )
                {
                    var filterPersonId = gfTransactions.GetUserPreference( "Person" ).AsIntegerOrNull();
                    if ( filterPersonId.HasValue )
                    {
                        // get the transactions for the person or all the members in the person's giving group (Family)
                        var filterPerson = new PersonService( rockContext ).Get( filterPersonId.Value );
                        if ( filterPerson != null )
                        {
                            qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingId == filterPerson.GivingId );
                        }
                    }
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

            var lTransactionImageField = gTransactions.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lTransactionImage" );
            var summaryField = gTransactions.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "Summary" );
            var showImages = bddlOptions.SelectedValue.AsIntegerOrNull() == 1;
            if ( lTransactionImageField != null )
            {
                lTransactionImageField.Visible = showImages;
            }

            if ( summaryField != null )
            {
                summaryField.Visible = !showImages;
            }

            if ( showImages )
            {
                qry = qry.Include( a => a.Images );
            }

            _isExporting = isExporting;

            gTransactions.SetLinqDataSource( qry.AsNoTracking() );
            gTransactions.DataBind();

            _isExporting = false;

            if ( _batch == null &&
                _scheduledTxn == null &&
                _registration == null &&
                _person == null )
            {
                pnlSummary.Visible = true;

                // No context - show account summary
                var qryTransactionDetails = qry.SelectMany( a => a.TransactionDetails );
                var qryFinancialAccount = new FinancialAccountService( rockContext ).Queryable();
                var accountSummaryQry = qryTransactionDetails.GroupBy( a => a.AccountId ).Select( a => new
                {
                    AccountId = a.Key,
                    TotalAmount = (decimal?)a.Sum( d => d.Amount )
                } ).Join( qryFinancialAccount, k1 => k1.AccountId, k2 => k2.Id, ( td, fa ) => new { td.TotalAmount, fa.Name, fa.Order, fa.Id } );

                // check for filtered accounts
                var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                if ( accountIds.Any() )
                {
                    accountSummaryQry = accountSummaryQry.Where( a => accountIds.Contains( a.Id ) ).OrderBy( a => a.Order );
                    lbFiltered.Text = "Filtered Account List";
                    lbFiltered.Visible = true;
                }
                else
                {
                    lbFiltered.Visible = false;
                }

                var summaryList = accountSummaryQry.ToList();
                var grandTotalAmount = ( summaryList.Count > 0 ) ? summaryList.Sum( a => a.TotalAmount ?? 0 ) : 0;
                lGrandTotal.Text = grandTotalAmount.FormatAsCurrency();
                rptAccountSummary.DataSource = summaryList.Select( a => new { a.Name, TotalAmount = a.TotalAmount.FormatAsCurrency() } ).ToList();
                rptAccountSummary.DataBind();
            }
            else
            {
                pnlSummary.Visible = false;
            }
        }

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        protected string GetAccounts( object dataItem )
        {
            var txn = dataItem as FinancialTransaction;
            if ( txn != null )
            {
                var summary = txn.TransactionDetails
                    .OrderBy( d => d.Account.Order )
                    .Select( d => string.Format( "{0}: {1}", d.Account.Name, d.Amount.FormatAsCurrency() ) )
                    .ToList();
                if ( summary.Any() )
                {
                    if ( _isExporting )
                    {
                        return summary.AsDelimited( Environment.NewLine );
                    }
                    else
                    {
                        return "<small>" + summary.AsDelimited( "<br/>" ) + "</small>";
                    }
                }
            }
            return string.Empty;
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
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "REASSIGN":
                    dlgReassign.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "REASSIGN":
                    dlgReassign.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion Internal Methods

        /// <summary>
        /// Handles the SelectionChanged event of the bddlOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlOptions_SelectionChanged( object sender, EventArgs e )
        {
            BindGrid();
        }
    }
}