// <copyright>
// Copyright by Central Christian Church
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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Batch Copier" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Block that allows users to copy a batch." )]
    [LinkedPage( "Detail Page", order: 0 )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, order: 1 )]
    public partial class BatchCopier : Rock.Web.UI.RockBlock, ISecondaryBlock
    {
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
                    _accountNames.Add( int.MinValue, "&nbsp;&nbsp;&nbsp;&nbsp;<strong>Total</strong>" );
                }
                return _accountNames;
            }
        }

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void lbDisplay_Click( object sender, EventArgs e )
        {
            pnlView.Visible = true;
            lbDisplay.Visible = false;
        }

        protected void lbCopyBatch_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var batchService = new FinancialBatchService( rockContext );

            FinancialBatch oldBatch = null;
            var changes = new List<string>();

            int? oldBatchId = PageParameter( "batchId" ).AsIntegerOrNull();
            if ( oldBatchId != null )
            {
                oldBatch = batchService.Get( oldBatchId.Value );
                if ( oldBatch != null )
                {
                    var batch = new FinancialBatch();
                    changes.Add( "Created the batch" );

                    BatchStatus batchStatus = BatchStatus.Pending;
                    CampusCache newCampus = oldBatch.CampusId.HasValue ? CampusCache.Read( oldBatch.CampusId.Value ) : null;
                    DateTime? startDateTime = dtpBatchDate.SelectedDateTimeIsBlank ? null : dtpBatchDate.SelectedDateTime;

                    History.EvaluateChange( changes, "Batch Name", batch.Name, oldBatch.Name );
                    History.EvaluateChange( changes, "Status", batch.Status, batchStatus );
                    History.EvaluateChange( changes, "Campus", "None", newCampus != null ? newCampus.Name : "None" );
                    History.EvaluateChange( changes, "Start Date/Time", oldBatch.BatchStartDateTime, startDateTime );
                    History.EvaluateChange( changes, "Control Amount", batch.ControlAmount.FormatAsCurrency(), oldBatch.ControlAmount.FormatAsCurrency() );
                    History.EvaluateChange( changes, "Accounting System Code", batch.AccountingSystemCode, oldBatch.AccountingSystemCode );
                    History.EvaluateChange( changes, "Notes", batch.Note, oldBatch.Note );

                    batch.Name = oldBatch.Name;
                    batch.Status = batchStatus;
                    batch.CampusId = oldBatch.CampusId;
                    batch.BatchStartDateTime = startDateTime;
                    batch.ControlAmount = oldBatch.ControlAmount;
                    batch.AccountingSystemCode = oldBatch.AccountingSystemCode;
                    batch.Note = oldBatch.Note;

                    //Fill in the FinancialTransactions
                    foreach ( FinancialTransaction oldTxn in oldBatch.Transactions )
                    {
                        var txn = new FinancialTransaction();

                        if ( txn != null )
                        {
                            var financialPaymentDetail = oldTxn.FinancialPaymentDetail.Clone(false);
                            txn.FinancialPaymentDetail = financialPaymentDetail;
                            txn.FinancialPaymentDetail.Id = 0;
                            txn.FinancialPaymentDetail.Guid = Guid.NewGuid();

                            string newPerson = ( oldTxn.AuthorizedPersonAlias != null && oldTxn.AuthorizedPersonAlias.Person != null ) ?
                                        oldTxn.AuthorizedPersonAlias.Person.FullName : string.Empty;

                            txn.AuthorizedPersonAliasId = oldTxn.AuthorizedPersonAliasId;
                            txn.TransactionDateTime = startDateTime;
                            txn.TransactionTypeValueId = oldTxn.TransactionTypeValueId;
                            txn.SourceTypeValueId = oldTxn.SourceTypeValueId;
                            txn.FinancialGatewayId = oldTxn.FinancialGatewayId;
                            txn.TransactionCode = oldTxn.TransactionCode;
                            txn.Summary = oldTxn.Summary;

                            if ( oldTxn.RefundDetails != null )
                            {
                                txn.RefundDetails = oldTxn.RefundDetails;
                            }

                            // Add Transaction Details
                            foreach ( var oldTxnDetail in oldTxn.TransactionDetails )
                            {
                                string oldAccountName = string.Empty;
                                string newAccountName = string.Empty;
                                decimal newAmount = 0.0M;

                                // Add or Update the activity type
                                var txnDetail = new FinancialTransactionDetail();
                                txn.TransactionDetails.Add( txnDetail );

                                newAccountName = AccountName( oldTxnDetail.AccountId );
                                newAmount = oldTxnDetail.Amount;

                                txnDetail.AccountId = oldTxnDetail.AccountId;
                                txnDetail.Amount = newAmount;
                                txnDetail.Summary = oldTxnDetail.Summary;
                            }

                            batch.Transactions.Add( txn );
                        }
                    }

                    //Check Validation
                    cvBatch.IsValid = oldBatch.IsValid;
                    if ( !Page.IsValid || !oldBatch.IsValid )
                    {
                        cvBatch.ErrorMessage = oldBatch.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                        return;
                    }

                    batchService.Add( batch );

                    // Save Batch
                    rockContext.WrapTransaction( () =>
                    {
                        if ( rockContext.SaveChanges() > 0 )
                        {
                            HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( FinancialBatch ),
                                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                    batch.Id,
                                    changes );
                        }
                    } );

                    NavigateToLinkedPage( "DetailPage", "batchId", batch.Id );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            lbDisplay.Visible = visible;
        }

        /// <summary>
        /// Gets the defined value.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns></returns>
        protected string GetDefinedValue( int? definedValueId )
        {
            if ( definedValueId.HasValue )
            {
                var dv = DefinedValueCache.Read( definedValueId.Value );
                if ( dv != null )
                {
                    return dv.Value;
                }
            }

            return "None";
        }

        /// <summary>
        /// Gets the name of the financial gateway.
        /// </summary>
        /// <param name="gatewayId">The gateway identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected string GetFinancialGatewayName( int? gatewayId, RockContext rockContext )
        {
            if ( gatewayId.HasValue )
            {
                var gw = new FinancialGatewayService( rockContext ).Get( gatewayId.Value );
                if ( gw != null )
                {
                    return gw.Name;
                }
            }

            return "None";
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

        #endregion

    }
}