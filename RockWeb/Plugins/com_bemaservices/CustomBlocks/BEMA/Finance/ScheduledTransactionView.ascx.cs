﻿// <copyright>
// Copyright by BEMA Software Services
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


/*
 * BEMA Modified Core Block ( v10.3.1)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 * 
 * Additional Features:
 * - FE1) Added Ability to enter Cancelation Reason to Scheduled Transaction
 * - FE2) Added Ability to see linkages with other entities
 */


namespace RockWeb.Plugins.com_bemaservices.Finance
{
    /// <summary>
    /// view an existing scheduled transaction.
    /// </summary>
    [DisplayName( "Scheduled Transaction View" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "View an existing scheduled transaction." )]

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


    /* BEMA.FE1.Start */
    [BooleanField(
        "Display Cancelation Reason Entry",
        Key = BemaAttributeKey.DisplayCancelationReasonEnabled,
        Description = "If Enabled, a reason for Cancelation must be entered.",
        DefaultBooleanValue = false,
        Category = "BEMA Additional Features",
        Order = 0 )]
		
    [AttributeField(
        "Reason For Cancelation Attribute",
        Key = BemaAttributeKey.CancelationReasonAttribute,
        Description = "The Attribute that stores the Reason for Cancelation",
        EntityTypeGuid = "76824E8A-CCC4-4085-84D9-8AF8C0807E20", // Financial Schedule Transaction
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "",
        Category = "BEMA Additional Features",
        Order = 10 )]
    /* BEMA.FE1.End */

    /* BEMA.FE2.Start */
    [BooleanField(
        "Display Linkages",
        Key = BemaAttributeKey.DisplayLinkagesEnabled,
        Description = "If Enabled, linkages to other entities will be displayed.",
        DefaultBooleanValue = false,
        Category = "BEMA Additional Features",
        Order = 1 )]
    /* BEMA.FE2.End */

    public partial class ScheduledTransactionView : RockBlock
    {
        #region BEMA Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class BemaAttributeKey
        {
            public const string DisplayCancelationReasonEnabled = "DisplayCancelationReasonEnabled";
            public const string CancelationReasonAttribute = "CancelationReasonAttribute";
            public const string DisplayLinkagesEnabled = "DisplayLinkagesEnabled";
        }

        #endregion BEMA Attribute Keys
		
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

        public static class PageParameterKey
        {
            public const string Person = "Person";
            public const string ScheduledTransactionId = "ScheduledTransactionId";
        }

        #endregion PageParameterKeys

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string TransactionDetailsState = "TransactionDetailsState";
        }

        #endregion ViewStateKeys

        #region Properties

        private List<FinancialScheduledTransactionDetail> TransactionDetailsState { get; set; }

        private Dictionary<int, string> _financialAccountNameLookup = null;

        private Dictionary<int, string> FinancialAccountNameLookup
        {
            get
            {
                if ( _financialAccountNameLookup == null )
                {
                    _financialAccountNameLookup = new Dictionary<int, string>();
                    new FinancialAccountService( new RockContext() ).Queryable()
                        .OrderBy( a => a.Order )
                        .Select( a => new { a.Id, a.Name } )
                        .ToList()
                        .ForEach( a => _financialAccountNameLookup.Add( a.Id, a.Name ) );
                }

                return _financialAccountNameLookup;
            }
        }

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
            gAccountsEdit.RowDataBound += gAccountsEdit_RowDataBound;

            base.OnInit( e );

            /* BEMA.FE1.Start */
            string script = string.Empty;
            if ( GetAttributeValue( BemaAttributeKey.DisplayCancelationReasonEnabled ).AsBoolean() )
            {
                script = string.Format( @"
                    function clearActiveDialog() {{
                        $('#{0}').val('');
                    }}",
                    tbCancelReason.ClientID
                );
            }
            else
            {
                script = @"
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
            }
            /* BEMA.FE1.End */


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
            if ( financialScheduledTransaction != null && financialScheduledTransaction.AuthorizedPersonAlias != null && financialScheduledTransaction.AuthorizedPersonAlias.Person != null )
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( PageParameterKey.ScheduledTransactionId, financialScheduledTransaction.Id.ToString() );
                queryParams.Add( PageParameterKey.Person, financialScheduledTransaction.AuthorizedPersonAlias.Person.UrlEncodedKey );

                if ( financialScheduledTransaction.FinancialGateway.GetGatewayComponent() is IHostedGatewayComponent )
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
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            int? financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
            if ( financialScheduledTransactionId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                    var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                        .Include( a => a.AuthorizedPersonAlias.Person )
                        .Include( a => a.FinancialGateway )
                        .FirstOrDefault( t => t.Id == financialScheduledTransactionId.Value );

                    if ( financialScheduledTransaction != null )
                    {
                        string errorMessage = string.Empty;
                        if ( financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage ) )
                        {
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }

                        ShowView( financialScheduledTransaction );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelSchedule_Click( object sender, EventArgs e )
        {
            /* BEMA.FE1.Start */
            if ( GetAttributeValue( BemaAttributeKey.DisplayCancelationReasonEnabled ).AsBoolean() )
            {
                if ( GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuidOrNull() != null )
                {
                    mdCancelSchedule.Show();
                }
                else
                {
                    nbError.Text = "Please configure the BEMA Scheduled Transaction View Block Settings to enable Cancelation Reason.";
                    nbError.Visible = true;
                    ExceptionLogService.LogException( new Exception( "Please configure the BEMA Scheduled Transaction View Block Settings to enable Cancelation Reason." ) );
                }
            }
            else
            {
                /* BEMA.FE1.End */

                int? financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
                if ( financialScheduledTransactionId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                        var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                            .Include( a => a.AuthorizedPersonAlias.Person )
                            .Include( a => a.FinancialGateway )
                            .FirstOrDefault( t => t.Id == financialScheduledTransactionId.Value );

                        if ( financialScheduledTransaction != null )
                        {
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
                }
			/* BEMA.FE1.Start */	
            }
			/* BEMA.FE1.End */
        }

        /// <summary>
        /// Handles the Click event of the btnReactivateSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReactivateSchedule_Click( object sender, EventArgs e )
        {
            int? financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
            if ( financialScheduledTransactionId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                    var financialScheduledTransaction = financialScheduledTransactionService.Queryable()
                        .Include( a => a.AuthorizedPersonAlias.Person )
                        .Include( a => a.FinancialGateway )
                        .FirstOrDefault( t => t.Id == financialScheduledTransactionId.Value );

                    if ( financialScheduledTransaction != null )
                    {
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

                    /* BEMA.FE1.Start */
                    if ( GetAttributeValue( BemaAttributeKey.DisplayCancelationReasonEnabled ).AsBoolean() )
                    {
                        if ( GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuidOrNull() != null )
                        {
                            financialScheduledTransaction.LoadAttributes();
                            var attributeGuid = GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuid();
                            var attribute = AttributeCache.Get( attributeGuid );
                            financialScheduledTransaction.SetAttributeValue( attribute.Key, "" );
                            financialScheduledTransaction.SaveAttributeValue( attribute.Key, rockContext );
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            ExceptionLogService.LogException( new Exception( "Please configure the BEMA Scheduled Transaction View Block Settings to enable Cancelation Reason." ) );
                        }
                        lCancelationInformation.Text = string.Empty;
                        ShowAccountView( financialScheduledTransaction );
                    }
                    /* BEMA.FE1.End */
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAccountsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAccountsEdit_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var account = ( FinancialScheduledTransactionDetail ) e.Row.DataItem;

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
                financialTransactionDetail.Amount = tbAccountAmount.Text.AsDecimal();
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
                    nbError.Text = string.Format( "<p>When updating account allocations, the total amount needs to remain the same as the original amount ({0}).</p>", financialScheduledTransaction.TotalAmount.FormatAsCurrency() );
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
                            financialTransactionDetail.Summary != editorTxnDetail.Summary )
                        {
                            accountChanges = true;
                        }
                    }

                    financialTransactionDetail.AccountId = editorTxnDetail.AccountId;
                    financialTransactionDetail.Amount = editorTxnDetail.Amount;
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
            int? scheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
            if ( scheduledTransactionId.HasValue )
            {
                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                return financialScheduledTransactionService
                    .Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.FinancialGateway )
                    .FirstOrDefault( t => t.Id == scheduledTransactionId.Value );
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

            hlStatus.Text = financialScheduledTransaction.IsActive ? "Active" : "Inactive";
            hlStatus.LabelType = financialScheduledTransaction.IsActive ? LabelType.Success : LabelType.Danger;

            string rockUrlRoot = ResolveRockUrl( "/" );
            Person person = null;
            if ( financialScheduledTransaction.AuthorizedPersonAlias != null )
            {
                person = financialScheduledTransaction.AuthorizedPersonAlias.Person;
            }

            var detailsLeft = new DescriptionList().Add( "Person", person );

            var detailsRight = new DescriptionList()
                .Add( "Amount", ( financialScheduledTransaction.ScheduledTransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ).FormatAsCurrency() )
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
                    paymentMethodDetails.Add( "Name on Card", financialScheduledTransaction.FinancialPaymentDetail.NameOnCard.Trim() );
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

            lSummary.Visible = financialScheduledTransaction.Summary.IsNotNullOrWhiteSpace();
            lSummary.Text = financialScheduledTransaction.Summary.ConvertCrLfToHtmlBr();

            lDetailsLeft.Text = detailsLeft.Html;
            lDetailsRight.Text = detailsRight.Html;

            gAccountsView.DataSource = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
            gAccountsView.DataBind();

            btnRefresh.Visible = gateway != null && gateway.GetScheduledPaymentStatusSupported;
            btnUpdate.Visible = gateway != null && gateway.UpdateScheduledPaymentSupported;
            btnCancelSchedule.Visible = financialScheduledTransaction.IsActive;
            btnReactivateSchedule.Visible = !financialScheduledTransaction.IsActive && gateway != null && gateway.ReactivateScheduledPaymentSupported;

            /* BEMA.FE2.Start */
            if ( GetAttributeValue( BemaAttributeKey.DisplayLinkagesEnabled ).AsBoolean() )
            {
                if ( financialScheduledTransaction.ScheduledTransactionDetails.Where( x => x.EntityId != null && x.EntityId > 0 ).Any() )
                {
                    lEntityInfo.Text = "<strong>Linkages</strong><br/>";
                    GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
                    var groupMemberList = financialScheduledTransaction.ScheduledTransactionDetails.Where( x => x.EntityId > 0 ).ToList();
                    foreach ( var groupMember in groupMemberList )
                    {
                        if ( groupMember.EntityTypeId == 90 ) //Group Member
                        {
                            var groupMemberObject = groupMemberService.Get( groupMember.EntityId.Value );
                            lEntityInfo.Text += string.Format( "{0} - {1}<br/>", groupMemberObject.Group.Name, groupMemberObject.Person.FullName );
                        }
                    }

                }
            }
            /* BEMA.FE2.End */

            /* BEMA.FE1.Start */
            if ( GetAttributeValue( BemaAttributeKey.DisplayCancelationReasonEnabled ).AsBoolean() )
            {
                if ( GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuidOrNull() != null )
                {
                    financialScheduledTransaction.LoadAttributes();
                    var attributeGuid = GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuid();
                    var attribute = AttributeCache.Get( attributeGuid );
                    var attributeValue = financialScheduledTransaction.GetAttributeValue( attribute.Key );
                    if ( !string.IsNullOrEmpty( attributeValue ) )
                    {
                        lCancelationInformation.Text = string.Format( @"<dt>Reason For Cancelation</dt><dd>{0}</dd>", attributeValue );
                    }
                }
            }
            /* BEMA.FE2.End */
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
        /// Gets the scheduled transaction.
        /// </summary>
        /// <returns></returns>
        private FinancialScheduledTransaction GetScheduledTransaction()
        {
            int? financialScheduledTransactionId = PageParameter( PageParameterKey.ScheduledTransactionId ).AsIntegerOrNull();
            if ( financialScheduledTransactionId.HasValue )
            {
                var rockContext = new RockContext();
                var service = new FinancialScheduledTransactionService( rockContext );

                return service
                    .Queryable()
                    .Include( s => s.ScheduledTransactionDetails )
                    .Include( s => s.FinancialGateway )
                    .Include( s => s.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( s => s.FinancialPaymentDetail.CreditCardTypeValue )
                    .Where( t => t.Id == financialScheduledTransactionId.Value )
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
        /// Handles the DataBound event of the lAccountName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lAccountName_DataBound( object sender, RowEventArgs e )
        {
            FinancialScheduledTransactionDetail financialScheduledTransactionDetail = e.Row.DataItem as FinancialScheduledTransactionDetail;
            Literal lAccountName = sender as Literal;
            lAccountName.Text = FinancialAccountNameLookup.GetValueOrNull( financialScheduledTransactionDetail.AccountId );
        }

        /* BEMA.FE1.Start */
        #region Custom Bema Code
        /// <summary>
        /// Handles the SaveClick event of the mdCancelSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCancelSchedule_SaveClick( object sender, EventArgs e )
        {
            mdCancelSchedule.Hide();
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

                    // Updating Cancelation Reason Attribute
                    if ( GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuidOrNull() != null )
                    {
                        txn.LoadAttributes();
                        var attributeGuid = GetAttributeValue( BemaAttributeKey.CancelationReasonAttribute ).AsGuid();
                        var attribute = AttributeCache.Get( attributeGuid );
                        txn.SetAttributeValue( attribute.Key, tbCancelReason.Text );
                        txn.SaveAttributeValue( attribute.Key, rockContext );
                        rockContext.SaveChanges();
                    }

                    ShowView( txn );
                }
            }
        }
        #endregion
        /* BEMA.FE1.End */
    }
}