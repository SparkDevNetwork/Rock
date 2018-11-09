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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// view an existing scheduled transaction.
    /// </summary>
    [DisplayName( "Scheduled Transaction View" )]
    [Category( "Finance" )]
    [Description( "View an existing scheduled transaction." )]

    [LinkedPage("Update Page", "The page used to update in existing scheduled transaction.")]
    public partial class ScheduledTransactionView : RockBlock
    {

        #region Properties

        private List<FinancialScheduledTransactionDetail> TransactionDetailsState { get; set; }

        private Dictionary<int, string> _accountNames = null;
        private Dictionary<int, string> AccountNames
        {
            get
            {
                if ( _accountNames == null )
                {
                    _accountNames = new Dictionary<int, string>();
                    new FinancialAccountService( new RockContext() ).Queryable()
                        .OrderBy( a => a.Order )
                        .Select( a => new { a.Id, a.Name } )
                        .ToList()
                        .ForEach( a => _accountNames.Add( a.Id, a.Name ) );
                }
                return _accountNames;
            }
        }

        #endregion

        #region base control methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["TransactionDetailsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                TransactionDetailsState = new List<FinancialScheduledTransactionDetail>();
            }
            else
            {
                TransactionDetailsState = JsonConvert.DeserializeObject<List<FinancialScheduledTransactionDetail>>( json );
            }

        }

        protected override void OnInit( EventArgs e )
        {
            gAccountsView.DataKeyNames = new string[] { "Guid" };
            gAccountsView.ShowActionRow = false;

            gAccountsEdit.DataKeyNames = new string[] { "Guid" };
            gAccountsEdit.ShowActionRow = true;
            gAccountsEdit.Actions.ShowAdd = true;
            gAccountsEdit.Actions.AddClick += gAccountsEdit_AddClick;
            gAccountsEdit.GridRebind += gAccountsEdit_GridRebind;
            gAccountsEdit.RowDataBound += gAccountsEdit_RowDataBound;

            base.OnInit( e );
            string script = @"
    $('a.js-cancel-txn').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to cancel this scheduled transaction?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });

    $('a.js-reactivate-txn').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to reactivate this scheduled transaction?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbCancelSchedule, lbCancelSchedule.GetType(), "update-txn-status", script, true );
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if (!Page.IsPostBack)
            {
                ShowView( GetScheduledTransaction() );
            }
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["TransactionDetailsState"] = JsonConvert.SerializeObject( TransactionDetailsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbUpdate_Click( object sender, EventArgs e )
        {
            var txn = GetScheduledTransaction();
            if ( txn != null && txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( "ScheduledTransactionId", txn.Id.ToString() );
                parms.Add( "Person", txn.AuthorizedPersonAlias.Person.UrlEncodedKey );
                NavigateToLinkedPage( "UpdatePage", parms );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var txnService = new FinancialScheduledTransactionService( rockContext );
                    var txn = txnService
                        .Queryable( "AuthorizedPersonAlias.Person,FinancialGateway" )
                        .FirstOrDefault( t => t.Id == txnId.Value );

                    if ( txn != null )
                    {
                        string errorMessage = string.Empty;
                        if ( txnService.GetStatus( txn, out errorMessage ) )
                        {
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }
                        ShowView( txn );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelSchedule_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var txnService = new FinancialScheduledTransactionService( rockContext );
                    var txn = txnService
                        .Queryable( "AuthorizedPersonAlias.Person,FinancialGateway" )
                        .FirstOrDefault( t => t.Id == txnId.Value );
                    if ( txn != null )
                    {
                        if ( txn.FinancialGateway != null )
                        {
                            txn.FinancialGateway.LoadAttributes( rockContext );
                        }

                        string errorMessage = string.Empty;
                        if ( txnService.Cancel( txn, out errorMessage ) )
                        {
                            txnService.GetStatus( txn, out errorMessage );
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }

                        ShowView( txn );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbReactivateSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReactivateSchedule_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var txnService = new FinancialScheduledTransactionService( rockContext );
                    var txn = txnService
                        .Queryable( "AuthorizedPersonAlias.Person,FinancialGateway" )
                        .FirstOrDefault( t => t.Id == txnId.Value );

                    if ( txn != null )
                    {
                        if ( txn.FinancialGateway != null )
                        {
                            txn.FinancialGateway.LoadAttributes( rockContext );
                        }

                        string errorMessage = string.Empty;
                        if ( txnService.Reactivate( txn, out errorMessage ) )
                        {
                            txnService.GetStatus( txn, out errorMessage );
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }

                        ShowView( txn );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gAccountsEdit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var account = (FinancialScheduledTransactionDetail)e.Row.DataItem;

                // If this is the total row
                if ( account.AccountId == int.MinValue )
                {
                    // disable the row select on each column
                    foreach ( TableCell cell in e.Row.Cells )
                    {
                        cell.RemoveCssClass( "grid-select-cell" );
                    }
                }

                // If account is associated with an entity (i.e. registration), or this is the total row do not allow it to be deleted
                if ( account.EntityTypeId.HasValue || account.AccountId == int.MinValue )
                {
                    // Hide the edit button if this is the total row
                    if ( account.AccountId == int.MinValue )
                    {
                        var editBtn = e.Row.Cells[3].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                        if ( editBtn != null )
                        {
                            editBtn.Visible = false;
                        }
                    }

                    // Hide the delete button
                    var deleteBtn = e.Row.Cells[4].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                    if ( deleteBtn != null )
                    {
                        deleteBtn.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountsEdit_AddClick( object sender, EventArgs e )
        {
            ShowAccountDialog( Guid.NewGuid() );
        }

        /// <summary>
        /// Handles the EditClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_EditClick( object sender, RowEventArgs e )
        {
            Guid? guid = e.RowKeyValue.ToString().AsGuidOrNull();
            if ( guid.HasValue )
            {
                ShowAccountDialog( guid.Value );
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_DeleteClick( object sender, RowEventArgs e )
        {
            Guid? guid = e.RowKeyValue.ToString().AsGuidOrNull();
            if ( guid.HasValue )
            {
                var txnDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( txnDetail != null )
                {
                    TransactionDetailsState.Remove( txnDetail );
                }

                BindAccounts();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAccountsEdit_GridRebind( object sender, EventArgs e )
        {
            BindAccounts();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAccount_SaveClick( object sender, EventArgs e )
        {
            Guid? guid = hfAccountGuid.Value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var txnDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( txnDetail == null )
                {
                    txnDetail = new FinancialScheduledTransactionDetail();
                    TransactionDetailsState.Add( txnDetail );
                }
                txnDetail.AccountId = apAccount.SelectedValue.AsInteger();
                txnDetail.Amount = tbAccountAmount.Text.AsDecimal();
                txnDetail.Summary = tbAccountSummary.Text;

                txnDetail.LoadAttributes();
                Rock.Attribute.Helper.GetEditValues( phAccountAttributeEdits, txnDetail );
                foreach ( var attributeValue in txnDetail.AttributeValues )
                {
                    txnDetail.SetAttributeValue( attributeValue.Key, attributeValue.Value.Value );
                }

                BindAccounts();
            }

            HideDialog();
        }

        protected void lbChangeAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var txn = GetTransaction( rockContext );
                {
                    ShowAccountEdit( txn );
                }
            }
        }

        protected void lbSaveAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var txn = GetTransaction( rockContext );
                {
                    decimal totalAmount = TransactionDetailsState.Select( d => d.Amount ).ToList().Sum();
                    if ( txn.TotalAmount != totalAmount )
                    {
                        nbError.Title = "Incorrect Amount";
                        nbError.Text = string.Format( "<p>When updating account allocations, the total amount needs to remain the same as the original amount ({0}).</p>", txn.TotalAmount.FormatAsCurrency() );
                        nbError.Visible = true;
                        return;
                    }

                    var txnDetailService = new FinancialScheduledTransactionDetailService( rockContext );
                    var accountService = new FinancialAccountService( rockContext );

                    // Delete any transaction details that were removed
                    var txnDetailsInDB = txnDetailService.Queryable().Where( a => a.ScheduledTransactionId.Equals( txn.Id ) ).ToList();
                    var deletedDetails = from txnDetail in txnDetailsInDB
                                         where !TransactionDetailsState.Select( d => d.Guid ).Contains( txnDetail.Guid )
                                         select txnDetail;

                    bool accountChanges = deletedDetails.Any();

                    deletedDetails.ToList().ForEach( txnDetail =>
                    {
                        txnDetailService.Delete( txnDetail );
                    } );

                    var changeSummary = new StringBuilder();

                    // Save Transaction Details
                    foreach ( var editorTxnDetail in TransactionDetailsState )
                    {
                        editorTxnDetail.Account = accountService.Get( editorTxnDetail.AccountId );

                        // Add or Update the activity type
                        var txnDetail = txn.ScheduledTransactionDetails.FirstOrDefault( d => d.Guid.Equals( editorTxnDetail.Guid ) );
                        if ( txnDetail == null )
                        {
                            accountChanges = true;
                            txnDetail = new FinancialScheduledTransactionDetail();
                            txnDetail.Guid = editorTxnDetail.Guid;
                            txn.ScheduledTransactionDetails.Add( txnDetail );

                        }
                        else
                        {
                            if ( txnDetail.AccountId != editorTxnDetail.AccountId ||
                                txnDetail.Amount != editorTxnDetail.Amount ||
                                txnDetail.Summary != editorTxnDetail.Summary )
                            {
                                accountChanges = true;
                            }
                        }

                        changeSummary.AppendFormat( "{0}: {1}", editorTxnDetail.Account != null ? editorTxnDetail.Account.Name : "?", editorTxnDetail.Amount.FormatAsCurrency() );
                        changeSummary.AppendLine();

                        txnDetail.AccountId = editorTxnDetail.AccountId;
                        txnDetail.Amount = editorTxnDetail.Amount;
                        txnDetail.Summary = editorTxnDetail.Summary;
                    }

                    if ( accountChanges )
                    {
                        // save changes
                        rockContext.SaveChanges();

                        // Add a note about the change
                        var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
                        if ( noteType != null )
                        {
                            var noteService = new NoteService( rockContext );
                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.EntityId = txn.Id;
                            note.Caption = "Updated Transaction";
                            note.Text = changeSummary.ToString();
                            noteService.Add( note );
                        }
                        rockContext.SaveChanges();
                    }

                    ShowView( txn );
                }
            }
        }

        protected void lbCancelAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var txn = GetTransaction( rockContext );
                if ( txn != null )
                {
                    ShowAccountView( txn );
                }
            }
        }

        #endregion

        #region  Methods

        private FinancialScheduledTransaction GetTransaction( RockContext rockContext )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                var txnService = new FinancialScheduledTransactionService( rockContext );
                return txnService
                    .Queryable( "AuthorizedPersonAlias.Person,FinancialGateway" )
                    .FirstOrDefault( t => t.Id == txnId.Value );
            }

            return null;
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowView( FinancialScheduledTransaction txn )
        {
            if ( txn != null )
            {
                hlStatus.Text = txn.IsActive ? "Active" : "Inactive";
                hlStatus.LabelType = txn.IsActive ? LabelType.Success : LabelType.Danger;

                string rockUrlRoot = ResolveRockUrl( "/" );

                var detailsLeft = new DescriptionList()
                    .Add( "Person", ( txn.AuthorizedPersonAlias != null && txn.AuthorizedPersonAlias.Person != null ) ?
                        txn.AuthorizedPersonAlias.Person.GetAnchorTag( rockUrlRoot ) : string.Empty );

                var detailsRight = new DescriptionList()
                    .Add( "Amount", ( txn.ScheduledTransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M ).FormatAsCurrency() )
                    .Add( "Frequency", txn.TransactionFrequencyValue != null ? txn.TransactionFrequencyValue.Value : string.Empty )
                    .Add( "Start Date", txn.StartDate.ToShortDateString() )
                    .Add( "End Date", txn.EndDate.HasValue ? txn.EndDate.Value.ToShortDateString() : string.Empty )
                    .Add( "Next Payment Date", txn.NextPaymentDate.HasValue ? txn.NextPaymentDate.Value.ToShortDateString() : string.Empty )
                    .Add( "Last Status Refresh", txn.LastStatusUpdateDateTime.HasValue ? txn.LastStatusUpdateDateTime.Value.ToString( "g" ) : string.Empty );

                detailsLeft.Add( "Source", txn.SourceTypeValue != null ? txn.SourceTypeValue.Value : string.Empty );

                if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValue != null )
                {
                    var paymentMethodDetails = new DescriptionList();

                    var currencyType = txn.FinancialPaymentDetail.CurrencyTypeValue;
                    if ( currencyType.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ) )
                    {
                        // Credit Card
                        paymentMethodDetails.Add( "Type", currencyType.Value + ( txn.FinancialPaymentDetail.CreditCardTypeValue != null ? ( " - " + txn.FinancialPaymentDetail.CreditCardTypeValue.Value ) : string.Empty ) );
                        paymentMethodDetails.Add( "Name on Card", txn.FinancialPaymentDetail.NameOnCard.Trim() );
                        paymentMethodDetails.Add( "Account Number", txn.FinancialPaymentDetail.AccountNumberMasked );
                        paymentMethodDetails.Add( "Expires", txn.FinancialPaymentDetail.ExpirationDate );
                    }
                    else
                    {
                        // ACH
                        paymentMethodDetails.Add( "Type", currencyType.Value );
                        paymentMethodDetails.Add( "Account Number", txn.FinancialPaymentDetail.AccountNumberMasked );
                    }

                    detailsLeft.Add( "Payment Method", paymentMethodDetails.GetFormattedList( "{0}: {1}" ).AsDelimited( "<br/>" ) );
                }

                GatewayComponent gateway = null;
                if ( txn.FinancialGateway != null )
                {
                    gateway = txn.FinancialGateway.GetGatewayComponent();
                    if ( gateway != null )
                    {
                        detailsLeft.Add( "Payment Gateway", GatewayContainer.GetComponentName( gateway.TypeName ) );
                    }
                }

                detailsLeft
                    .Add( "Transaction Code", txn.TransactionCode )
                    .Add( "Schedule Id", txn.GatewayScheduleId );

                lDetailsLeft.Text = detailsLeft.Html;
                lDetailsRight.Text = detailsRight.Html;

                gAccountsView.DataSource = txn.ScheduledTransactionDetails.ToList();
                gAccountsView.DataBind();

                var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
                if ( noteType != null )
                {
                    var rockContext = new RockContext();
                    rptrNotes.DataSource = new NoteService( rockContext ).Get( noteType.Id, txn.Id )
                        .Where( n => n.CreatedDateTime.HasValue )
                        .OrderBy( n => n.CreatedDateTime )
                        .ToList()
                        .Select( n => new
                        {
                            n.Caption,
                            Text = n.Text.ConvertCrLfToHtmlBr(),
                            Person = ( n.CreatedByPersonAlias != null && n.CreatedByPersonAlias.Person != null ) ? n.CreatedByPersonAlias.Person.FullName : "",
                            Date = n.CreatedDateTime.HasValue ? n.CreatedDateTime.Value.ToShortDateString() : "",
                            Time = n.CreatedDateTime.HasValue ? n.CreatedDateTime.Value.ToShortTimeString() : ""
                        } )
                        .ToList();
                    rptrNotes.DataBind();
                }

                lbRefresh.Visible = gateway != null && gateway.GetScheduledPaymentStatusSupported;
                lbUpdate.Visible = gateway != null && gateway.UpdateScheduledPaymentSupported;
                lbCancelSchedule.Visible = txn.IsActive;
                lbReactivateSchedule.Visible = !txn.IsActive && gateway != null && gateway.ReactivateScheduledPaymentSupported;
            }
        }

        private void ShowAccountView( FinancialScheduledTransaction txn )
        {
            gAccountsView.DataSource = txn.ScheduledTransactionDetails.ToList();
            gAccountsView.DataBind();

            SetAccountEditMode( false );
        }

        private void ShowAccountEdit( FinancialScheduledTransaction txn )
        {
            if ( txn != null )
            {
                TransactionDetailsState = txn.ScheduledTransactionDetails.ToList();
                BindAccounts();
            }

            SetAccountEditMode( true );
        }

        private void SetAccountEditMode( bool editable )
        {
            pnlViewAccounts.Visible = !editable;
            pnlEditAccounts.Visible = editable;
        }

        /// <summary>
        /// Binds the transaction details.
        /// </summary>
        private void BindAccounts()
        {
            var accounts = TransactionDetailsState.ToList();
            gAccountsEdit.DataSource = accounts;
            gAccountsEdit.DataBind();
        }

        /// <summary>
        /// Shows the account dialog.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        private void ShowAccountDialog( Guid guid )
        {
            hfAccountGuid.Value = guid.ToString();

            var txnDetail = TransactionDetailsState.Where( d => d.Guid.Equals( guid ) ).FirstOrDefault();
            if ( txnDetail != null )
            {
                apAccount.SetValue( txnDetail.AccountId );
                tbAccountAmount.Text = txnDetail.Amount.ToString( "N2" );
                tbAccountSummary.Text = txnDetail.Summary;

                if ( txnDetail.Attributes == null )
                {
                    txnDetail.LoadAttributes();
                }
            }
            else
            {
                apAccount.SetValue( null );
                tbAccountAmount.Text = string.Empty;
                tbAccountSummary.Text = string.Empty;

                txnDetail = new FinancialScheduledTransactionDetail();
                txnDetail.LoadAttributes();
            }

            phAccountAttributeEdits.Controls.Clear();
            Helper.AddEditControls( txnDetail, phAccountAttributeEdits, true, mdAccount.ValidationGroup );

            ShowDialog( "ACCOUNT" );
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
                case "ACCOUNT":
                    mdAccount.Show();
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
                case "ACCOUNT":
                    mdAccount.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Accounts the name.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        protected string AccountName( int? accountId )
        {
            if ( accountId.HasValue )
            {
                return AccountNames.ContainsKey( accountId.Value ) ? AccountNames[accountId.Value] : string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the scheduled transaction.
        /// </summary>
        /// <returns></returns>
        private FinancialScheduledTransaction GetScheduledTransaction()
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if (txnId.HasValue)
            {
                var rockContext = new RockContext();
                var service = new FinancialScheduledTransactionService( rockContext );
                return service
                    .Queryable( "ScheduledTransactionDetails,FinancialGateway,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                    .Where( t => t.Id == txnId.Value )
                    .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Accounts the name.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        protected string AccountName( int accountId )
        {
            return AccountNames.ContainsKey( accountId ) ? AccountNames[accountId] : "";
        }

        private void ShowErrorMessage(string message)
        {
            nbError.Text = message;
            nbError.Visible = true;
        }

        #endregion

}
}