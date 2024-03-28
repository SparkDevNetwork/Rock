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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
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
    /// This is the *Internal* block for editing scheduled transactions
    /// </summary>
    [DisplayName( "Scheduled Transaction View" )]
    [Category( "Finance" )]
    [Description( "View/Edit an existing scheduled transaction." )]

    #region Block Attributes

    [LinkedPage(
        "Update Page for Gateways",
        Key = AttributeKey.UpdatePageUnhosted,
        Description = "The page used to update an existing scheduled transaction for Gateways that don't support a hosted payment interface.",
        Order = 0 )]

    [LinkedPage(
        "Update Page for Hosted Gateways",
        Key = AttributeKey.UpdatePageHosted,
        Description = "The page used to update an existing scheduled transaction for Gateways that support a hosted payment interface.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "85753750-7465-4241-97A6-E5F27EA38C8B" )]
    public partial class ScheduledTransactionView : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string UpdatePageUnhosted = "UpdatePage";
            public const string UpdatePageHosted = "UpdatePageHosted";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            [RockObsolete( "1.13.1" )]
            [Obsolete( "Pass the GUID instead using the key ScheduledTransactionGuid." )]
            public const string ScheduledTransactionId = "ScheduledTransactionId";

            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
            public const string PersonId = "PersonId";
        }

        #endregion PageParameterKeys

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string TransactionDetailsState = "TransactionDetailsState";
            public const string ForeignCurrencyDefinedValueId = "ForeignCurrencyDefinedValueId";
            public const string PersonIdState = "PersonIdState";
        }

        #endregion ViewStateKeys

        #region Constants

        /// <summary>
        /// This value is set as the accountId for "fake" financial transaction details
        /// added to the end of grid sources representing a total/footer row
        /// </summary>
        private const int TotalRowAccountId = int.MinValue;

        #endregion

        #region Properties

        private List<FinancialScheduledTransactionDetail> TransactionDetailsState { get; set; }

        private int? ForeignCurrencyDefinedValueId { get; set; }

        private int? PersonId { get; set; }

        #endregion

        #region base control methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[ViewStateKey.TransactionDetailsState] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                TransactionDetailsState = new List<FinancialScheduledTransactionDetail>();
            }
            else
            {
                TransactionDetailsState = JsonConvert.DeserializeObject<List<FinancialScheduledTransactionDetail>>( json );
            }

            ForeignCurrencyDefinedValueId = ( int? ) ViewState[ViewStateKey.ForeignCurrencyDefinedValueId];

            PersonId = ( int? ) ViewState[ViewStateKey.PersonIdState];
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            ViewState[ViewStateKey.TransactionDetailsState] = JsonConvert.SerializeObject( TransactionDetailsState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.ForeignCurrencyDefinedValueId] = ForeignCurrencyDefinedValueId;
            ViewState[ViewStateKey.PersonIdState] = PersonId;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            gAccountsView.DataKeyNames = new string[] { "Guid" };
            gAccountsView.ShowActionRow = false;

            gAccountsEdit.DataKeyNames = new string[] { "Guid" };
            gAccountsEdit.ShowActionRow = true;
            gAccountsEdit.Actions.ShowAdd = true;
            gAccountsEdit.Actions.AddClick += gAccountsEdit_AddClick;
            gAccountsEdit.GridRebind += gAccountsEdit_GridRebind;

            base.OnInit( e );
            string script = @"
    $('a.js-cancel-txn').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to cancel this scheduled transaction?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });

    $('a.js-reactivate-txn').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to reactivate this scheduled transaction?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( btnCancelSchedule, btnCancelSchedule.GetType(), "update-txn-status", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowView( GetScheduledTransaction() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            var financialScheduledTransaction = GetScheduledTransaction();
            if ( financialScheduledTransaction != null )
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( PageParameterKey.ScheduledTransactionGuid, financialScheduledTransaction.Guid.ToString() );

                var hostedGatewayComponent = financialScheduledTransaction.FinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;
                if ( hostedGatewayComponent != null && hostedGatewayComponent.GetSupportedHostedGatewayModes( financialScheduledTransaction.FinancialGateway ).Contains( HostedGatewayMode.Hosted ) )
                {
                    NavigateToLinkedPage( AttributeKey.UpdatePageHosted, queryParams );
                }
                else
                {
                    NavigateToLinkedPage( AttributeKey.UpdatePageUnhosted, queryParams );
                }
            }
        }

        /// <summary>
        /// Gets the scheduled transaction Guid based on what is specified in the URL
        /// </summary>
        /// <param name="refresh">if set to <c>true</c> [refresh].</param>
        /// <returns></returns>
        private Guid? GetScheduledTransactionGuidFromUrl()
        {
            var financialScheduledTransactionGuid = PageParameter( PageParameterKey.ScheduledTransactionGuid ).AsGuidOrNull();

#pragma warning disable CS0618
            var financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
#pragma warning restore CS0618

            if ( financialScheduledTransactionGuid.HasValue  )
            {
                return financialScheduledTransactionGuid.Value;
            }

            if ( financialScheduledTransactionId.HasValue )
            {
                return new FinancialScheduledTransactionService( new RockContext() ).GetGuid( financialScheduledTransactionId.Value );
            }

            return null;
        }

        /// <summary>
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            var financialScheduledTranactionGuid = GetScheduledTransactionGuidFromUrl();
            if ( !financialScheduledTranactionGuid.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.FinancialGateway )
                    .FirstOrDefault( t => t.Guid == financialScheduledTranactionGuid.Value );

                if ( financialScheduledTransaction == null )
                {
                    return;
                }

                string errorMessage = string.Empty;
                if ( financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage ) )
                {
                    rockContext.SaveChanges();
                }
                else
                {
                    if ( financialScheduledTransaction.IsActive == false )
                    {
                        // if GetStatus failed, but the scheduled transaction is inactive, just show Schedule is Inactive
                        // This takes care of dealing with gateways that delete the scheduled payment vs inactivating them on the gateway side
                        ShowErrorMessage( "Schedule is inactive" );
                    }
                    else
                    {
                        ShowErrorMessage( errorMessage );
                    }
                }

                ShowView( financialScheduledTransaction );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelSchedule_Click( object sender, EventArgs e )
        {
            var financialScheduledTranactionGuid = GetScheduledTransactionGuidFromUrl();
            if ( !financialScheduledTranactionGuid.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.FinancialGateway )
                    .FirstOrDefault( t => t.Guid == financialScheduledTranactionGuid.Value );

                if ( financialScheduledTransaction == null )
                {
                    return;
                }

                if ( financialScheduledTransaction.FinancialGateway != null )
                {
                    financialScheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                string errorMessage = string.Empty;
                if ( financialScheduledTransactionService.Cancel( financialScheduledTransaction, out errorMessage ) )
                {
                    financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage );
                    rockContext.SaveChanges();
                }
                else
                {
                    ShowErrorMessage( errorMessage );
                }

                ShowView( financialScheduledTransaction );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnReactivateSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReactivateSchedule_Click( object sender, EventArgs e )
        {
            var financialScheduledTranactionGuid = GetScheduledTransactionGuidFromUrl();
            if ( !financialScheduledTranactionGuid.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.FinancialGateway )
                    .FirstOrDefault( t => t.Guid == financialScheduledTranactionGuid.Value );

                if ( financialScheduledTransaction == null )
                {
                    return;
                }

                if ( financialScheduledTransaction.FinancialGateway != null )
                {
                    financialScheduledTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                string errorMessage = string.Empty;
                if ( financialScheduledTransactionService.Reactivate( financialScheduledTransaction, out errorMessage ) )
                {
                    financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage );
                    rockContext.SaveChanges();
                }
                else
                {
                    ShowErrorMessage( errorMessage );
                }

                ShowView( financialScheduledTransaction );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Passing the person ID to the parent in case the parent page uses the person model context.
            var queryParams = new Dictionary<string, string>();

            if ( PersonId != null )
            {
                queryParams.Add( PageParameterKey.PersonId, PersonId?.ToString() );
            }

            NavigateToParentPage( queryParams );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsView_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var financialTransactionDetail = ( FinancialScheduledTransactionDetail ) e.Row.DataItem;
            if ( financialTransactionDetail == null )
            {
                return;
            }

            var lAccountsViewAccountName = e.Row.FindControl( "lAccountsViewAccountName" ) as Literal;
            lAccountsViewAccountName.Text = FinancialAccountCache.Get( financialTransactionDetail.AccountId )?.Name;

            var lAccountsViewAmountMinusFeeCoverageAmount = e.Row.FindControl( "lAccountsViewAmountMinusFeeCoverageAmount" ) as Literal;
            decimal amountMinusFeeCoverageAmount;
            if ( financialTransactionDetail.FeeCoverageAmount.HasValue )
            {
                amountMinusFeeCoverageAmount = financialTransactionDetail.Amount - financialTransactionDetail.FeeCoverageAmount.Value;
            }
            else
            {
                amountMinusFeeCoverageAmount = financialTransactionDetail.Amount;
            }

            lAccountsViewAmountMinusFeeCoverageAmount.Text = amountMinusFeeCoverageAmount.FormatAsCurrency( ForeignCurrencyDefinedValueId );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var financialTransactionDetail = ( FinancialScheduledTransactionDetail ) e.Row.DataItem;

            // If this is the total row
            if ( financialTransactionDetail.AccountId == TotalRowAccountId )
            {
                // disable the row select on each column
                foreach ( TableCell cell in e.Row.Cells )
                {
                    cell.RemoveCssClass( "grid-select-cell" );
                }
            }

            var lAccountsEditAccountName = e.Row.FindControl( "lAccountsEditAccountName" ) as Literal;
            lAccountsEditAccountName.Text = FinancialAccountCache.Get( financialTransactionDetail.AccountId )?.Name;

            var lAccountsEditAmountMinusFeeCoverageAmount = e.Row.FindControl( "lAccountsEditAmountMinusFeeCoverageAmount" ) as Literal;
            decimal amountMinusFeeCoverageAmount;
            if ( financialTransactionDetail.FeeCoverageAmount.HasValue )
            {
                amountMinusFeeCoverageAmount = financialTransactionDetail.Amount - financialTransactionDetail.FeeCoverageAmount.Value;
            }
            else
            {
                amountMinusFeeCoverageAmount = financialTransactionDetail.Amount;
            }

            lAccountsEditAmountMinusFeeCoverageAmount.Text = amountMinusFeeCoverageAmount.FormatAsCurrency( ForeignCurrencyDefinedValueId );

            // If account is associated with an entity (i.e. registration), or this is the total row do not allow it to be deleted
            if ( financialTransactionDetail.EntityTypeId.HasValue || financialTransactionDetail.AccountId == TotalRowAccountId )
            {
                // Hide the edit button if this is the total row
                if ( financialTransactionDetail.AccountId == TotalRowAccountId )
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
                var financialTransactionDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( financialTransactionDetail != null )
                {
                    TransactionDetailsState.Remove( financialTransactionDetail );
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
                var financialTransactionDetail = TransactionDetailsState.Where( t => t.Guid.Equals( guid.Value ) ).FirstOrDefault();
                if ( financialTransactionDetail == null )
                {
                    financialTransactionDetail = new FinancialScheduledTransactionDetail();
                    TransactionDetailsState.Add( financialTransactionDetail );
                }

                financialTransactionDetail.AccountId = apAccount.SelectedValue.AsInteger();
                var feeCoverageAmount = tbAccountFeeCoverageAmount.Value;
                financialTransactionDetail.Amount = ( tbAccountAmountMinusFeeCoverageAmount.Value ?? 0.0M ) + ( feeCoverageAmount ?? 0.00M );
                financialTransactionDetail.FeeCoverageAmount = feeCoverageAmount;
                financialTransactionDetail.Summary = tbAccountSummary.Text;

                financialTransactionDetail.LoadAttributes();
                Rock.Attribute.Helper.GetEditValues( phAccountAttributeEdits, financialTransactionDetail );
                foreach ( var attributeValue in financialTransactionDetail.AttributeValues )
                {
                    financialTransactionDetail.SetAttributeValue( attributeValue.Key, attributeValue.Value.Value );
                }

                BindAccounts();
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnChangeAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnChangeAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransaction = GetTransaction( rockContext );
                {
                    ShowAccountEdit( financialScheduledTransaction );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransaction = GetTransaction( rockContext );

                decimal totalAmount = TransactionDetailsState.Select( d => d.Amount ).ToList().Sum();
                if ( financialScheduledTransaction.TotalAmount != totalAmount )
                {
                    nbError.Title = "Incorrect Amount";
                    nbError.Text = string.Format( "<p>When updating account allocations, the total amount needs to remain the same as the original amount ({0}).</p>", financialScheduledTransaction.TotalAmount.FormatAsCurrency( financialScheduledTransaction.ForeignCurrencyCodeValueId ) );
                    nbError.Visible = true;
                    return;
                }

                var txnDetailService = new FinancialScheduledTransactionDetailService( rockContext );
                var accountService = new FinancialAccountService( rockContext );

                // Delete any transaction details that were removed
                var txnDetailsInDB = txnDetailService.Queryable().Where( a => a.ScheduledTransactionId.Equals( financialScheduledTransaction.Id ) ).ToList();
                var deletedDetails = from txnDetail in txnDetailsInDB
                                     where !TransactionDetailsState.Select( d => d.Guid ).Contains( txnDetail.Guid )
                                     select txnDetail;

                bool accountChanges = deletedDetails.Any();

                deletedDetails.ToList().ForEach( txnDetail =>
                {
                    txnDetailService.Delete( txnDetail );
                } );

                // Save Transaction Details
                foreach ( var editorTxnDetail in TransactionDetailsState )
                {
                    editorTxnDetail.Account = accountService.Get( editorTxnDetail.AccountId );

                    // Add or Update the activity type
                    var financialTransactionDetail = financialScheduledTransaction.ScheduledTransactionDetails.FirstOrDefault( d => d.Guid.Equals( editorTxnDetail.Guid ) );
                    if ( financialTransactionDetail == null )
                    {
                        accountChanges = true;
                        financialTransactionDetail = new FinancialScheduledTransactionDetail();
                        financialTransactionDetail.Guid = editorTxnDetail.Guid;
                        financialScheduledTransaction.ScheduledTransactionDetails.Add( financialTransactionDetail );
                    }
                    else
                    {
                        if ( financialTransactionDetail.AccountId != editorTxnDetail.AccountId ||
                            financialTransactionDetail.Amount != editorTxnDetail.Amount ||
                            financialTransactionDetail.FeeCoverageAmount != editorTxnDetail.FeeCoverageAmount ||
                            financialTransactionDetail.Summary != editorTxnDetail.Summary )
                        {
                            accountChanges = true;
                        }
                    }

                    financialTransactionDetail.AccountId = editorTxnDetail.AccountId;
                    financialTransactionDetail.Amount = editorTxnDetail.Amount;
                    financialTransactionDetail.FeeCoverageAmount = editorTxnDetail.FeeCoverageAmount;
                    financialTransactionDetail.Summary = editorTxnDetail.Summary;
                }

                if ( accountChanges )
                {
                    // save changes
                    rockContext.SaveChanges();
                }

                ShowView( financialScheduledTransaction );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelAccounts_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var financialScheduledTransaction = GetTransaction( rockContext );
                if ( financialScheduledTransaction != null )
                {
                    ShowAccountView( financialScheduledTransaction );
                }
            }
        }

        #endregion

        #region  Methods

        /// <summary>
        /// Gets the transaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private FinancialScheduledTransaction GetTransaction( RockContext rockContext )
        {
            var scheduledTransactionGuid = GetScheduledTransactionGuidFromUrl();
            if ( scheduledTransactionGuid.HasValue )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                return financialScheduledTransactionService
                    .Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.FinancialGateway )
                    .FirstOrDefault( t => t.Guid == scheduledTransactionGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="financialScheduledTransaction">The TXN.</param>
        private void ShowView( FinancialScheduledTransaction financialScheduledTransaction )
        {
            if ( financialScheduledTransaction == null )
            {
                return;
            }

            ForeignCurrencyDefinedValueId = financialScheduledTransaction.ForeignCurrencyCodeValueId;

            if ( financialScheduledTransaction.FinancialPaymentDetail.CardExpirationDate < RockDateTime.Now )
            {
                // Show that card is expired
                hlStatus.Text = "Card Expired";
                hlStatus.LabelType = LabelType.Warning;
            }
            else if ( financialScheduledTransaction.Status.HasValue && financialScheduledTransaction.Status != FinancialScheduledTransactionStatus.Active )
            {
                // show that that the gateway reported another problem 
                hlStatus.Text = financialScheduledTransaction.Status.ConvertToString();
                hlStatus.LabelType = LabelType.Warning;
            }
            else
            {

                hlStatus.Text = financialScheduledTransaction.IsActive ? "Active" : "Inactive";
                hlStatus.LabelType = financialScheduledTransaction.IsActive ? LabelType.Success : LabelType.Danger;
            }

            string rockUrlRoot = ResolveRockUrl( "/" );
            Person person = null;
            if ( financialScheduledTransaction.AuthorizedPersonAlias != null )
            {
                person = financialScheduledTransaction.AuthorizedPersonAlias.Person;
            }

            PersonId = person.Id;

            var detailsLeft = new DescriptionList().Add( "Person", person );

            var detailsRight = new DescriptionList()
                .Add( "Amount", ( financialScheduledTransaction.ScheduledTransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ).FormatAsCurrency( ForeignCurrencyDefinedValueId ) )
                .Add( "Frequency", financialScheduledTransaction.TransactionFrequencyValue != null ? financialScheduledTransaction.TransactionFrequencyValue.Value : string.Empty )
                .Add( "Start Date", financialScheduledTransaction.StartDate.ToShortDateString() )
                .Add( "End Date", financialScheduledTransaction.EndDate.HasValue ? financialScheduledTransaction.EndDate.Value.ToShortDateString() : string.Empty )
                .Add( "Next Payment Date", financialScheduledTransaction.NextPaymentDate.HasValue ? financialScheduledTransaction.NextPaymentDate.Value.ToShortDateString() : string.Empty )
                .Add( "Last Status Refresh", financialScheduledTransaction.LastStatusUpdateDateTime.HasValue ? financialScheduledTransaction.LastStatusUpdateDateTime.Value.ToString( "g" ) : string.Empty );

            detailsLeft.Add( "Source", financialScheduledTransaction.SourceTypeValue != null ? financialScheduledTransaction.SourceTypeValue.Value : string.Empty );

            if ( financialScheduledTransaction.FinancialPaymentDetail != null && financialScheduledTransaction.FinancialPaymentDetail.CurrencyTypeValue != null )
            {
                var paymentMethodDetails = new DescriptionList();

                var currencyType = financialScheduledTransaction.FinancialPaymentDetail.CurrencyTypeValue;
                if ( currencyType.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ) )
                {
                    // Credit Card
                    paymentMethodDetails.Add( "Type", currencyType.Value + ( financialScheduledTransaction.FinancialPaymentDetail.CreditCardTypeValue != null ? ( " - " + financialScheduledTransaction.FinancialPaymentDetail.CreditCardTypeValue.Value ) : string.Empty ) );
                    paymentMethodDetails.Add( "Name on Card", financialScheduledTransaction.FinancialPaymentDetail.NameOnCard?.Trim() );
                    paymentMethodDetails.Add( "Account Number", financialScheduledTransaction.FinancialPaymentDetail.AccountNumberMasked );
                    paymentMethodDetails.Add( "Expires", financialScheduledTransaction.FinancialPaymentDetail.ExpirationDate );
                }
                else
                {
                    // ACH
                    paymentMethodDetails.Add( "Type", currencyType.Value );
                    paymentMethodDetails.Add( "Account Number", financialScheduledTransaction.FinancialPaymentDetail.AccountNumberMasked );
                }

                detailsLeft.Add( "Payment Method", paymentMethodDetails.GetFormattedList( "{0}: {1}" ).AsDelimited( "<br/>" ) );
            }

            GatewayComponent gateway = null;
            if ( financialScheduledTransaction.FinancialGateway != null )
            {
                gateway = financialScheduledTransaction.FinancialGateway.GetGatewayComponent();
                if ( gateway != null )
                {
                    detailsLeft.Add( "Payment Gateway", GatewayContainer.GetComponentName( gateway.TypeName ) );
                }
            }

            detailsLeft
                .Add( "Transaction Code", financialScheduledTransaction.TransactionCode )
                .Add( "Schedule Id", financialScheduledTransaction.GatewayScheduleId );

            lComments.Visible = financialScheduledTransaction.Summary.IsNotNullOrWhiteSpace();
            lComments.Text = financialScheduledTransaction.Summary.ConvertCrLfToHtmlBr();

            lDetailsLeft.Text = detailsLeft.Html;
            lDetailsRight.Text = detailsRight.Html;

            gAccountsView.DataSource = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
            gAccountsView.DataBind();

            btnRefresh.Visible = gateway != null && gateway.GetScheduledPaymentStatusSupported;
            btnUpdate.Visible = gateway != null && gateway.UpdateScheduledPaymentSupported;
            btnCancelSchedule.Visible = financialScheduledTransaction.IsActive && gateway.UpdateScheduledPaymentSupported;
            btnReactivateSchedule.Visible = !financialScheduledTransaction.IsActive && gateway != null && gateway.ReactivateScheduledPaymentSupported;
        }

        /// <summary>
        /// Shows the account view.
        /// </summary>
        /// <param name="financialScheduledTransaction">The financial scheduled transaction.</param>
        private void ShowAccountView( FinancialScheduledTransaction financialScheduledTransaction )
        {
            gAccountsView.DataSource = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
            gAccountsView.DataBind();

            SetAccountEditMode( false );
        }

        /// <summary>
        /// Shows the account edit.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowAccountEdit( FinancialScheduledTransaction financialScheduledTransaction )
        {
            if ( financialScheduledTransaction != null )
            {
                TransactionDetailsState = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
                BindAccounts();
            }

            SetAccountEditMode( true );
        }

        /// <summary>
        /// Sets the account edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
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
                SetAccountAmountMinusFeeCoverageTextboxText( tbAccountAmountMinusFeeCoverageAmount, txnDetail );
                SetAccountFeeCoverageAmountTextboxText( tbAccountFeeCoverageAmount, txnDetail );
                tbAccountSummary.Text = txnDetail.Summary;

                if ( txnDetail.Attributes == null )
                {
                    txnDetail.LoadAttributes();
                }
            }
            else
            {
                apAccount.SetValue( null );
                tbAccountAmountMinusFeeCoverageAmount.Value = null;
                tbAccountFeeCoverageAmount.Value = null;
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
        /// Gets the scheduled transaction.
        /// </summary>
        /// <returns></returns>
        private FinancialScheduledTransaction GetScheduledTransaction()
        {
            var financialScheduledTransactionGuid = GetScheduledTransactionGuidFromUrl();
            if ( financialScheduledTransactionGuid.HasValue )
            {
                var rockContext = new RockContext();
                var service = new FinancialScheduledTransactionService( rockContext );

                return service
                    .Queryable()
                    .Include( s => s.ScheduledTransactionDetails )
                    .Include( s => s.FinancialGateway )
                    .Include( s => s.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( s => s.FinancialPaymentDetail.CreditCardTypeValue )
                    .Where( t => t.Guid == financialScheduledTransactionGuid.Value )
                    .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowErrorMessage( string message )
        {
            nbError.Text = message;
            nbError.Visible = true;
        }

        #endregion

        /// <summary>
        /// Sets the account amount minus fee coverage textbox text.
        /// </summary>
        /// <param name="tbAccountAmount">The tb account amount.</param>
        /// <param name="transactionDetail">The transaction detail.</param>
        private void SetAccountAmountMinusFeeCoverageTextboxText( CurrencyBox tbAccountAmountMinusFeeCoverageAmount, FinancialScheduledTransactionDetail transactionDetail )
        {
            /* 2021-01-28 MDP

              FinancialScheduledTransactionDetail.Amount includes the FeeCoverageAmount.
              For example, if a person scheduled to gave $100.00 but elected to pay $1.80 to cover the fee.
              FinancialScheduledTransactionDetail.Amount would be stored as $101.80 and
              FinancialScheduledTransactionDetail.FeeCoverageAmount would be stored as $1.80.

              However, when the FinancialScheduledTransactionDetail.Amount is used in this EditBox,
              don't include the FinanciaFinancialScheduledTransactionDetaillTransactionDetail.FeeCoverageAmount.
              So in the above example, the Textbox would say $100.00
             
             */

            var feeCoverageAmount = transactionDetail.FeeCoverageAmount;
            var accountAmount = transactionDetail.Amount;
            tbAccountAmountMinusFeeCoverageAmount.CurrencyCodeDefinedValueId = ForeignCurrencyDefinedValueId ?? 0;

            if ( feeCoverageAmount.HasValue )
            {
                tbAccountAmountMinusFeeCoverageAmount.Value = ( accountAmount - feeCoverageAmount.Value );
            }
            else
            {
                tbAccountAmountMinusFeeCoverageAmount.Value = accountAmount;
            }
        }

        /// <summary>
        /// Sets the account fee coverage amount textbox text.
        /// </summary>
        /// <param name="tbAccountFeeCoverageAmount">The tb account fee coverage amount.</param>
        /// <param name="transactionDetail">The transaction detail.</param>
        private void SetAccountFeeCoverageAmountTextboxText( CurrencyBox tbAccountFeeCoverageAmount, FinancialScheduledTransactionDetail transactionDetail )
        {
            tbAccountFeeCoverageAmount.CurrencyCodeDefinedValueId = ForeignCurrencyDefinedValueId ?? 0;
            tbAccountFeeCoverageAmount.Value = transactionDetail.FeeCoverageAmount;
            tbAccountFeeCoverageAmount.Visible = transactionDetail.FeeCoverageAmount.HasValue;
        }
    }
}