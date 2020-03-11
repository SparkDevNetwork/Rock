﻿// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Linq;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.ComponentModel;
using System.Data.Entity;
using System.Text;

namespace RockWeb.Plugins.com_bemadev.Finance
{
    [DisplayName( "Batch List with GL Export" )]
    [Category( "com_bemaservices > Finance" )]
    [Description( "Financial batch list that supports GL exports." )]
    [LinkedPage( "Detail Page", order: 0 )]
    [BooleanField( "Show Accounting Code", "Should the accounting code column be displayed.", false, "", 1 )]
    public partial class BatchListWithGLExport : Rock.Web.UI.RockBlock, IPostBackEventHandler
    {
        #region Fields

        private RockDropDownList ddlAction;
        private BootstrapButton btnGLExport;
        private List<FinancialBatch> _batches;

        #endregion

        #region Base Control Methods

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

            gfBatchFilter.ApplyFilterClick += gfBatchFilter_ApplyFilterClick;
            gfBatchFilter.ClearFilterClick += gfBatchFilter_ClearFilterClick;
            gfBatchFilter.DisplayFilterValue += gfBatchFilter_DisplayFilterValue;

            gBatchList.DataKeyNames = new string[] { "Id" };
            gBatchList.Actions.ShowAdd = UserCanEdit;
            gBatchList.Actions.AddClick += gBatchList_Add;
            gBatchList.GridRebind += gBatchList_GridRebind;
            gBatchList.RowDataBound += gBatchList_RowDataBound;
            gBatchList.IsDeleteEnabled = UserCanEdit;
            gBatchList.ShowConfirmDeleteDialog = false;

            ddlAction = new RockDropDownList();
            ddlAction.ID = "ddlAction";
            ddlAction.CssClass = "pull-left input-width-lg";
            ddlAction.Items.Add( new ListItem( "-- Select Action --", string.Empty ) );
            ddlAction.Items.Add( new ListItem( "Open Selected Batches", "OPEN" ) );
            ddlAction.Items.Add( new ListItem( "Close Selected Batches", "CLOSE" ) );
            string deleteScript = @"
    $('table.js-grid-batch-list a.grid-delete-button').click(function( e ){
        var $btn = $(this);
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this batch?', function (result) {
            if (result) {
                if ( $btn.closest('tr').hasClass('js-has-transactions') ) {
                    Rock.dialogs.confirm('This batch has transactions. Are you sure that you want to delete this batch and all of it\'s transactions?', function (result) {
                        if (result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                } else {
                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gBatchList, gBatchList.GetType(), "deleteBatchScript", deleteScript, true );

            btnGLExport = new BootstrapButton();
            btnGLExport.ID = "btnGLExport";
            btnGLExport.CssClass = "pull-right btn btn-default btn-sm";
            btnGLExport.Text = "<i class='fa fa-download'></i>";
            btnGLExport.ToolTip = "GL Export";
            btnGLExport.Click += btnGLExport_Click;

            gBatchList.Actions.AddCustomActionControl( ddlAction );
            gBatchList.Actions.AddCustomActionControl( btnGLExport );
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfBatchFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfBatchFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfBatchFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbResult.Visible = false;

            if ( !Page.IsPostBack )
            {
                SetVisibilityOption();
                BindFilter();
                BindGrid();
            }

            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbDownload );
        }

        /// <summary>
        /// Registers the java script for grid actions.
        /// NOTE: This needs to be done after the BindGrid
        /// </summary>
        private void RegisterJavaScriptForGridActions()
        {
            string scriptFormat = @"
    $('#{0}').change(function( e ){{
        var count = $(""#{1} input[id$='_cbSelect_0']:checked"").length;
        if (count == 0) {{
            eval({2});
        }}
        else
        {{
            var $ddl = $(this);
            if ($ddl.val() != '') {{
                Rock.dialogs.confirm('Are you sure you want to ' + ($ddl.val() == 'OPEN' ? 'open' : 'close') + ' the selected batches?', function (result) {{
                    if (result) {{
                        eval({2});
                    }}
                    $ddl.val('');
                }});
            }}
        }}
    }});";
            string script = string.Format( scriptFormat, ddlAction.ClientID, gBatchList.ClientID, Page.ClientScript.GetPostBackEventReference( this, "StatusUpdate" ) );
            ScriptManager.RegisterStartupScript( ddlAction, ddlAction.GetType(), "ConfirmStatusChange", script, true );
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
            SetVisibilityOption();
            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfBatchFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfBatchFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Row Limit":
                    {
                        // row limit filter was removed, so hide it just in case
                        e.Value = null;
                        break;
                    }

                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<BatchStatus>();
                        if ( status.HasValue )
                        {
                            e.Value = status.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Contains Transaction Type":
                    {
                        var transactionTypeValueId = e.Value.AsIntegerOrNull();
                        if ( transactionTypeValueId.HasValue )
                        {
                            var transactionTypeValue = DefinedValueCache.Get( transactionTypeValueId.Value );
                            e.Value = transactionTypeValue != null ? transactionTypeValue.ToString() : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Campus":
                    {
                        var campus = CampusCache.Get( e.Value.AsInteger() );
                        if ( campus != null )
                        {
                            e.Value = campus.Name;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfBatchFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfBatchFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfBatchFilter.SaveUserPreference( "Date Range", drpBatchDate.DelimitedValues );
            gfBatchFilter.SaveUserPreference( "Title", tbTitle.Text );
            if ( tbAccountingCode.Visible )
            {
                gfBatchFilter.SaveUserPreference( "Accounting Code", tbAccountingCode.Text );
            }

            gfBatchFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfBatchFilter.SaveUserPreference( "Campus", campCampus.SelectedValue );
            gfBatchFilter.SaveUserPreference("Batch Id", tbBatchId.Text);
            gfBatchFilter.SaveUserPreference( "Contains Transaction Type", ddlTransactionType.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var batchService = new FinancialBatchService( rockContext );
            var transactionService = new FinancialTransactionService( rockContext );
            var batch = batchService.Get( e.RowKeyId );
            if ( batch != null )
            {
                if ( UserCanEdit || batch.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson ) )
                {
                    string errorMessage;
                    if ( !batchService.CanDelete( batch, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        foreach ( var txn in transactionService.Queryable()
                            .Where( t => t.BatchId == batch.Id ) )
                        {
                            transactionService.Delete( txn );
                        }

                        batchService.Delete( batch );

                        rockContext.SaveChanges();
                    } );
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var batchRow = e.Row.DataItem as BatchRow;
                var deleteField = gBatchList.Columns.OfType<DeleteField>().First();
                var cell = ( e.Row.Cells[gBatchList.Columns.IndexOf( deleteField )] as DataControlFieldCell ).Controls[0];

                if ( batchRow != null )
                {
                    if ( batchRow.TransactionCount > 0 )
                    {
                        e.Row.AddCssClass( "js-has-transactions" );
                    }

                    // Hide delete button if the batch is closed.
                    if ( batchRow.Status == BatchStatus.Closed && cell != null )
                    {
                        cell.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "batchId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Add event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBatchList_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "batchId", 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBatchList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "StatusUpdate" &&
                ddlAction != null &&
                ddlAction.SelectedValue != null &&
                !string.IsNullOrWhiteSpace( ddlAction.SelectedValue ) )
            {
                var batchesSelected = new List<int>();

                gBatchList.SelectedKeys.ToList().ForEach( b => batchesSelected.Add( b.ToString().AsInteger() ) );

                if ( batchesSelected.Any() )
                {
                    var newStatus = ddlAction.SelectedValue == "OPEN" ? BatchStatus.Open : BatchStatus.Closed;

                    var rockContext = new RockContext();
                    var batchService = new FinancialBatchService( rockContext );
                    var batchesToUpdate = batchService.Queryable()
                        .Where( b =>
                            batchesSelected.Contains( b.Id ) &&
                            b.Status != newStatus )
                        .ToList();

                    foreach ( var batch in batchesToUpdate )
                    {
                        var changes = new List<string>();
                        batch.Status = newStatus;

                        if ( !batch.IsValid )
                        {
                            string message = string.Format( "Unable to update status for the selected batches.<br/><br/>{0}", batch.ValidationResults.AsDelimited( "<br/>" ) );
                            maWarningDialog.Show( message, ModalAlertType.Warning );
                            return;
                        }
                    }

                    rockContext.SaveChanges();

                    nbResult.Text = string.Format(
                        "{0} batches were {1}.",
                        batchesToUpdate.Count().ToString( "N0" ),
                        newStatus == BatchStatus.Open ? "opened" : "closed" );

                    nbResult.NotificationBoxType = NotificationBoxType.Success;
                    nbResult.Visible = true;
                }
                else
                {
                    nbResult.Text = string.Format( "There were not any batches selected." );
                    nbResult.NotificationBoxType = NotificationBoxType.Warning;
                    nbResult.Visible = true;
                }

                ddlAction.SelectedIndex = 0;
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnGLExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnGLExport_Click( object sender, EventArgs e )
        {
            if ( gBatchList.SelectedKeys.Any() )
            {
                dpDate.SelectedDate = RockDateTime.Now;
                tbAccountingPeriod.Text = GetUserPreference( "com.bemadev.exporttogl.accountingperiod" );
                tbJournalType.Text = GetUserPreference( "com.bemadev.exporttogl.journaltype" );

                pnlExportModal.Visible = true;
                mdExport.Show();
            }
            else
            {
                nbResult.Text = string.Format( "There were not any batches selected." );
                nbResult.NotificationBoxType = NotificationBoxType.Warning;
                nbResult.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbExportSave_Click( object sender, EventArgs e )
        {
            SetUserPreference( "com.bemadev.exporttogl.accountingperiod", tbAccountingPeriod.Text );
            SetUserPreference( "com.bemadev.exporttogl.journaltype", tbJournalType.Text );

            pnlExportModal.Visible = false;
            mdExport.Hide();

            string script = string.Format( "document.getElementById('{0}').click();", lbDownload.ClientID );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "PerformExport", script, true );
        }

        /// <summary>
        /// Handles the Click event of the lbDownload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDownload_Click( object sender, EventArgs e )
        { 
            var rockContext = new RockContext();
            var batches = new List<FinancialBatch>();

            var parameters = RockPage.PageParameters();
            List<GLRecord> records = new List<GLRecord>();
            var batchesSelected = new List<int>();

            gBatchList.SelectedKeys.ToList().ForEach( b => batchesSelected.Add( b.ToString().AsInteger() ) );

            if ( batchesSelected.Any() )
            {

                GLRecord record = new GLRecord();

                // Build the header row
                //DONOTIMPORT	JOURNAL	DATE	DESCRIPTION	REFERENCE_NO	LINE_NO	ACCT_NO	LOCATION_ID	DEPT_ID	GLDIMFUND	GLENTRY_CLASSID	GLENTRY_VENDORID	DEBIT	MEMO	
                record.DoNotImport = "DONOTIMPORT";
                record.JournalType = "JOURNAL";
                record.Date = "DATE";
                record.Description1 = "DESCRIPTION";
                record.ReferenceNumber = "REFERENCE_NO";
                record.RowNumber = "LINE_NO";
                record.AccountNumber = "ACCT_NO";
                record.Location = "LOCATION_ID";
                record.Department = "DEPT_ID";
                record.GLDIMFUND = "GLDIMFUND";
                record.GLENTRY_CLASSID = "GLENTRY_CLASSID";
                record.GLENTRY_VENDORID = "GLENTRY_VENDORID";
                record.AmountDebit = "DEBIT";
                record.AmountCredit = "CREDIT";
                record.Memo = "MEMO";


                records.Add(record);

                batches = new FinancialBatchService( rockContext ).Queryable().Where( b => batchesSelected.Contains( b.Id ) ).ToList();

                foreach ( var batch in batches )
                {
                    records.AddRange( GLRecordsForBatch( batch, dpDate.SelectedDate.Value, tbAccountingPeriod.Text.Trim(), tbJournalType.Text.Trim() ) );
                }
            }

            if ( !UserCanEdit )
            {
                return;
            }

            // Update the batch to reflect that it has been exported.
            foreach ( var batch in batches )
            {
                batch.LoadAttributes();
                batch.SetAttributeValue( "GLExported", "true" );
                batch.SaveAttributeValues( rockContext );
            }
            rockContext.SaveChanges();


            // Send the results as a CSV file for download.
            Page.EnableViewState = false;
            Page.Response.Clear();
            Page.Response.ContentType = "text/plain";
            Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=RockFinancialExport.csv" );
            Page.Response.Write( string.Join( "\r\n", records.Select( r => r.ToString() ).ToArray() ) );
            Page.Response.Flush();
            Page.Response.End();
        }

        #endregion

        #region Methods

        private void SetVisibilityOption()
        {
            bool showAccountingCode = GetAttributeValue( "ShowAccountingCode" ).AsBoolean();
            tbAccountingCode.Visible = showAccountingCode;
            gBatchList.Columns[4].Visible = showAccountingCode;

            if ( showAccountingCode )
            {
                string accountingCode = gfBatchFilter.GetUserPreference( "Accounting Code" );
                tbAccountingCode.Text = !string.IsNullOrWhiteSpace( accountingCode ) ? accountingCode : string.Empty;
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            string titleFilter = gfBatchFilter.GetUserPreference( "Title" );
            tbTitle.Text = !string.IsNullOrWhiteSpace( titleFilter ) ? titleFilter : string.Empty;

            string batchId = gfBatchFilter.GetUserPreference("Batch Id");
            tbBatchId.Text = !string.IsNullOrWhiteSpace(batchId) ? batchId : string.Empty;


            if ( tbAccountingCode.Visible )
            {
                string accountingCode = gfBatchFilter.GetUserPreference( "Accounting Code" );
                tbAccountingCode.Text = !string.IsNullOrWhiteSpace( accountingCode ) ? accountingCode : string.Empty;
            }

            ddlStatus.BindToEnum<BatchStatus>();
            ddlStatus.Items.Insert( 0, Rock.Constants.All.ListItem );
            string statusFilter = gfBatchFilter.GetUserPreference( "Status" );
            if ( string.IsNullOrWhiteSpace( statusFilter ) )
            {
                statusFilter = BatchStatus.Open.ConvertToInt().ToString();
            }

            ddlStatus.SetValue( statusFilter );

            var definedTypeTransactionTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() );
            ddlTransactionType.BindToDefinedType( definedTypeTransactionTypes, true );
            ddlTransactionType.SetValue( gfBatchFilter.GetUserPreference( "Contains Transaction Type" ) );

            var campusi = CampusCache.All();
            campCampus.Campuses = campusi;
            campCampus.Visible = campusi.Any();
            campCampus.SetValue( gfBatchFilter.GetUserPreference( "Campus" ) );

            drpBatchDate.DelimitedValues = gfBatchFilter.GetUserPreference( "Date Range" );
        }

        /// <summary>
        /// Formats the value as currency (called from markup)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string FormatValueAsCurrency( decimal value )
        {
            return value.FormatAsCurrency();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {

            var txnCountCol = gBatchList.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.DataField == "TransactionCount" );
            if ( txnCountCol != null )
            {
                txnCountCol.HeaderText = isExporting ? "Transaction Count" :
                    "<span class='hidden-print'>Transaction Count</span><span class='visible-print-inline'>Txns</span>";
            }

            var txnAmountCol = gBatchList.ColumnsOfType<CurrencyField>().FirstOrDefault( c => c.DataField == "TransactionAmount" );
            if ( txnAmountCol != null )
            {
                txnAmountCol.HeaderText = isExporting ? "Transaction Amount" :
                    "<span class='hidden-print'>Transaction Total</span><span class='visible-print-inline'>Txn Total</span>";
            }

            var accountsCol = gBatchList.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.HeaderText == "Accounts" );
            if ( accountsCol != null )
            {
                accountsCol.DataField = isExporting ? "AccountSummaryText" : "AccountSummaryHtml";
            }

            try
            {
                var qry = GetQuery().AsNoTracking();
                var batchRowQry = qry.Select( b => new BatchRow
                {
                    Id = b.Id,
                    BatchStartDateTime = b.BatchStartDateTime.Value,
                    Name = b.Name,
                    AccountingSystemCode = b.AccountingSystemCode,
                    TransactionCount = b.Transactions.Count(),
                    TransactionAmount = b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M,
                    ControlAmount = b.ControlAmount,
                    CampusName = b.Campus != null ? b.Campus.Name : "",
                    Status = b.Status,
                    UnMatchedTxns = b.Transactions.Any( t => !t.AuthorizedPersonAliasId.HasValue ),
                    BatchNote = b.Note,
                    AccountSummaryList = b.Transactions
                        .SelectMany( t => t.TransactionDetails )
                        .GroupBy( d => d.AccountId )
                        .Select( s => new BatchAccountSummary
                        {
                            AccountId = s.Key,
                            AccountOrder = s.Max( d => d.Account.Order ),
                            AccountName = s.Max( d => d.Account.Name ),
                            Amount = s.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M
                        } )
                        .OrderBy( s => s.AccountOrder )
                        .ToList()
                } );

                gBatchList.SetLinqDataSource( batchRowQry.AsNoTracking() );
                gBatchList.EntityTypeId = EntityTypeCache.Get<Rock.Model.FinancialBatch>().Id;
                gBatchList.DataBind();

                RegisterJavaScriptForGridActions();

                var qryTransactionDetails = qry.SelectMany( a => a.Transactions ).SelectMany( a => a.TransactionDetails );
                var accountSummaryQry = qryTransactionDetails.GroupBy( a => a.Account ).Select( a => new
                {
                    a.Key.Name,
                    a.Key.Order,
                    TotalAmount = ( decimal? ) a.Sum( d => d.Amount )
                } ).OrderBy( a => a.Order );

                var summaryList = accountSummaryQry.ToList();
                var grandTotalAmount = ( summaryList.Count > 0 ) ? summaryList.Sum( a => a.TotalAmount ?? 0 ) : 0;
                string currencyFormat = GlobalAttributesCache.Value( "CurrencySymbol" ) + "{0:n}";
                lGrandTotal.Text = string.Format( currencyFormat, grandTotalAmount );
                rptAccountSummary.DataSource = summaryList.Select( a => new { a.Name, TotalAmount = string.Format( currencyFormat, a.TotalAmount ) } ).ToList();
                rptAccountSummary.DataBind();
            }
            catch ( Exception ex )
            {
                nbWarningMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Gets the query.  Set the timeout to 90 seconds in case the user
        /// has not set any filters and they've imported N years worth of
        /// batch data into Rock.
        /// </summary>
        /// <returns></returns>
        private IOrderedQueryable<FinancialBatch> GetQuery()
        {
            var rockContext = new RockContext();
            var batchService = new FinancialBatchService( rockContext );
            rockContext.Database.CommandTimeout = 90;
            var qry = batchService.Queryable()
                .Where( b => b.BatchStartDateTime.HasValue );

            // filter by date
            string dateRangeValue = gfBatchFilter.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValue ) )
            {
                var drp = new DateRangePicker();
                drp.DelimitedValues = dateRangeValue;
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( b => b.BatchStartDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    var endOfDay = drp.UpperValue.Value.AddDays( 1 );
                    qry = qry.Where( b => b.BatchStartDateTime < endOfDay );
                }
            }

            // filter by status
            var status = gfBatchFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<BatchStatus>();
            if ( status.HasValue )
            {
                qry = qry.Where( b => b.Status == status );
            }

            // filter by batches that contain transactions of the specified transaction type
            var transactionTypeValueId = gfBatchFilter.GetUserPreference( "Contains Transaction Type" ).AsIntegerOrNull();
            if ( transactionTypeValueId.HasValue )
            {
                qry = qry.Where( a => a.Transactions.Any( t => t.TransactionTypeValueId == transactionTypeValueId.Value ) );
            }

            // filter by title
            string title = gfBatchFilter.GetUserPreference( "Title" );
            if ( !string.IsNullOrEmpty( title ) )
            {
                qry = qry.Where( batch => batch.Name.StartsWith( title ) );
            }

            // filter by accounting code
            if ( tbAccountingCode.Visible )
            {
                string accountingCode = gfBatchFilter.GetUserPreference( "Accounting Code" );
                if ( !string.IsNullOrEmpty( accountingCode ) )
                {
                    qry = qry.Where( batch => batch.AccountingSystemCode.StartsWith( accountingCode ) );
                }
            }

            // filter by campus
            var campus = CampusCache.Get( gfBatchFilter.GetUserPreference( "Campus" ).AsInteger() );
            if ( campus != null )
            {
                qry = qry.Where( b => b.CampusId == campus.Id );
            }
			
			// filter by Id
			string bactchId = gfBatchFilter.GetUserPreference( "Batch Id" );
            if ( !string.IsNullOrEmpty( bactchId ) )
            {
			  int id = bactchId.AsInteger();
              qry = qry.Where( batch => batch.Id == id );
			}

            IOrderedQueryable<FinancialBatch> sortedQry = null;

            SortProperty sortProperty = gBatchList.SortProperty;
            if ( sortProperty != null )
            {
                switch ( sortProperty.Property )
                {
                    case "TransactionCount":
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                sortedQry = qry.OrderBy( b => b.Transactions.Count() );
                            }
                            else
                            {
                                sortedQry = qry.OrderByDescending( b => b.Transactions.Count() );
                            }

                            break;
                        }

                    case "TransactionAmount":
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                sortedQry = qry.OrderBy( b => b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M );
                            }
                            else
                            {
                                sortedQry = qry.OrderByDescending( b => b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M );
                            }

                            break;
                        }

                    default:
                        {
                            sortedQry = qry.Sort( sortProperty );
                            break;
                        }
                }
            }
            else
            {
                sortedQry = qry
                    .OrderByDescending( b => b.BatchStartDateTime )
                    .ThenBy( b => b.Name );
            }

            return sortedQry;
        }

        /// <summary>
        /// Get the GLRecords for the given batch, with the appropriate information about
        /// the export data.
        /// </summary>
        /// <param name="batch">Batch to be exported.</param>
        /// <param name="date">The date of this deposit.</param>
        /// <param name="accountingPeriod">Accounting period as defined in the GL system.</param>
        /// <param name="journalType">The type of journal entry to create as defined in the GL system.</param>
        /// <returns>A collection of GLRecord objects to be imported into the GL system.</returns>
        List<GLRecord> GLRecordsForBatch( FinancialBatch batch, DateTime date, string accountingPeriod, string journalType )
        {
            List<GLRecord> records = new List<GLRecord>();

            // Load all the transaction details, load their attributes and then group
            // by the account attributes, GLBankAccount+GLCompany+GLFund.
            var transactions = batch.Transactions
                .SelectMany( t => t.TransactionDetails )
                .ToList();
            foreach ( var d in transactions )
            {
                d.LoadAttributes();
                d.Account.LoadAttributes();
            }
            var accounts = transactions.GroupBy( d => new { GLBankAccount = d.Account.GetAttributeValue( "GLBankAccount" ), GLCompany = d.Account.GetAttributeValue( "GLCompany" ), GLFund = d.Account.GetAttributeValue( "GLFund" ) }, d => d ).OrderBy( g => g.Key.GLBankAccount );

            GLRecord record = new GLRecord();

            // Go through each group and build the line items.
            foreach ( var grp in accounts )
            {
                record = new GLRecord();
                int rowNumber = 0;

                // Build the bank account deposit line item.

                record.JournalType = journalType;
                record.AccountingPeriod = accountingPeriod;
                record.RowNumber = rowNumber.ToString();
                record.AccountNumber = grp.Key.GLBankAccount;
                record.AmountDebit = Math.Round(grp.Sum( d => d.Amount ) * 100).ToString("0");
                record.AmountCredit = string.Empty;
                record.Company = grp.Key.GLCompany;
                record.EmployeeId = string.Empty;
                record.ProjectId = string.Empty;
                record.ReferenceNumber = "Batch #" + batch.Id.ToString();
                record.Memo = "Contribution Deposit";

                record.Description1 = "Rock Import";
                record.Date = batch.BatchStartDateTime.Value.ToString("M/d/yyyy");
                record.Department = string.Empty;
 
                record.Description2 = string.Empty;
                record.Fund = grp.Key.GLFund;
 
              

                //records.Add( record );
				
				var locations2 = grp.GroupBy(d => new { GLLocation = d.Account.GetAttributeValue("FinancialGlLocation"), GLAccount = d.Account.GetAttributeValue( "FinancialGlBankAccount") }, d => d).OrderBy(g => g.Key.GLLocation);

                foreach (var grpTransactions in locations2)
                {
                    record = new GLRecord();
					record.RowNumber = (rowNumber += 1).ToString();
					
					if(rowNumber == 1)
					{
						record.Description1 = batch.Name;
						record.JournalType = journalType;
						record.Date = batch.BatchStartDateTime.Value.ToString("M/d/yyyy");
					}
					else
					{
						record.Description1 = string.Empty;
						record.JournalType = string.Empty;
						record.Date = string.Empty;;
					}
					
                  
                    record.AccountingPeriod = accountingPeriod;
                   
                    record.AmountDebit = grpTransactions.Sum(d => d.Amount).ToString("0.##");
                    record.Location = grpTransactions.Key.GLLocation;
					record.AccountNumber = grpTransactions.Key.GLAccount;
      
                    record.AmountCredit = string.Empty;
                    record.Company = grp.Key.GLCompany;
                    record.EmployeeId = string.Empty;
                    record.ProjectId = string.Empty;
                    record.ReferenceNumber = "Batch #" + batch.Id.ToString();
                    record.Memo = "Contribution Deposit #" + batch.Id.ToString();

                   // record.Description1 = "Rock Import";
                    
                    record.Department = string.Empty;

                    record.Description2 = string.Empty;
                   // record.AccountNumber = grp.Key.GLAccount;

                    records.Add(record);
                }

                // Build each of the revenue fund withdrawls.
				var fundAccounts = grp.GroupBy(d => new { GLLocation = d.Account.GetAttributeValue("FinancialGlLocation"), GlAccount = d.Account.GetAttributeValue("FinancialGlAccount"), GlDepartment = d.Account.GetAttributeValue("FinancialGlDepartment"), GlFund = d.Account.GetAttributeValue("FinancialGlFund"), RockAccount = d.Account.Name }, d => d).OrderBy(g => g.Key.GLLocation);

                foreach (var grpTransactions in fundAccounts)
                {
                    record = new GLRecord();

                    record.JournalType = string.Empty;
                    record.AccountingPeriod = string.Empty;
                    record.RowNumber = (rowNumber += 1).ToString();
                    record.AmountDebit = string.Empty;
                    record.Location = grpTransactions.Key.GLLocation;
                    record.AccountNumber = grpTransactions.Key.GlAccount;
                    record.Department = grpTransactions.Key.GlDepartment;
                    record.GLDIMFUND = grpTransactions.Key.GlFund;
                   // record.GLENTRY_CLASSID = grpTransactions.Account.GetAttributeValue("GlProgram");
                    record.AmountCredit = grpTransactions.Sum(d => d.Amount).ToString("0.##");
                    record.Company = string.Empty;
                    record.EmployeeId = string.Empty;
                    record.ProjectId = string.Empty;
                    record.ReferenceNumber = "Batch #" + batch.Id.ToString();

                    record.Description1 = string.Empty;
                    record.Date = string.Empty;
                  
                    record.Description2 = string.Empty;
                    record.Fund = grp.Key.GLFund;

                    record.Memo = grpTransactions.Key.RockAccount + " #" + batch.Id.ToString();

                    records.Add(record);
                }
            }

            return records;
        }

        #endregion

        #region Helper Class

        public class BatchAccountSummary
        {
            public int AccountId { get; set; }
            public int AccountOrder { get; set; }
            public string AccountName { get; set; }
            public decimal Amount { get; set; }
            public override string ToString()
            {
                return string.Format( "{0}: {1}", AccountName, Amount.FormatAsCurrency() );
            }
        }

        public class BatchRow
        {
            public int Id { get; set; }
            public DateTime BatchStartDateTime { get; set; }
            public string Name { get; set; }
            public string AccountingSystemCode { get; set; }
            public int TransactionCount { get; set; }
            public decimal TransactionAmount { get; set; }
            public decimal ControlAmount { get; set; }
            public List<BatchAccountSummary> AccountSummaryList { get; set; }
            public string CampusName { get; set; }
            public BatchStatus Status { get; set; }
            public bool UnMatchedTxns { get; set; }
            public string BatchNote { get; set; }

            public decimal Variance
            {
                get
                {
                    return TransactionAmount - ControlAmount;
                }
            }

            public string AccountSummaryText
            {
                get
                {
                    var summary = new List<string>();
                    AccountSummaryList.ForEach( a => summary.Add( a.ToString() ) );
                    return summary.AsDelimited( Environment.NewLine );
                }
            }

            public string AccountSummaryHtml
            {
                get
                {
                    var summary = new List<string>();
                    AccountSummaryList.ForEach( a => summary.Add( a.ToString() ) );
                    return "<small>" + summary.AsDelimited( "<br/>" ) + "</small>";
                }
            }

            public string StatusText
            {
                get
                {
                    return Status.ConvertToString();
                }
            }


            public string StatusLabelClass
            {
                get
                {
                    switch ( Status )
                    {
                        case BatchStatus.Closed:
                            return "label label-default";
                        case BatchStatus.Open:
                            return "label label-info";
                        case BatchStatus.Pending:
                            return "label label-warning";
                    }

                    return string.Empty;
                }
            }

            public string Notes
            {
                get
                {
                    var notes = new StringBuilder();

                    switch ( Status )
                    {
                        case BatchStatus.Open:
                            {
                                if ( UnMatchedTxns )
                                {
                                    notes.Append( "<span class='label label-warning'>Unmatched Transactions</span><br/>" );
                                }

                                break;
                            }
                    }

                    notes.Append( BatchNote );

                    return notes.ToString();
                }
            }
        }

        class GLRecord
        {
            public string DoNotImport { get; set; }
            public string JournalType { get; set; }
            public string AccountingPeriod { get; set; }
            public string Description1 { get; set; }
            public string ReferenceNumber { get; set; }
            public string RowNumber { get; set; }
            public string AccountNumber { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string GLDIMFUND { get; set; }
            public string GLENTRY_CLASSID { get; set; }
            public string GLENTRY_VENDORID { get; set; }
            public string Memo { get; set; }
            public string AmountDebit { get; set; }
            public string AmountCredit { get; set; }
            public string EmployeeId { get; set; }
            public string ProjectId { get; set; }


  


            public string Company { get; set; }
            public string Fund { get; set; }
            
            
            public string Journal { get; set; }

            public string Date { get; set; }

            

            public string Description2 { get; set; }

         

            public override string ToString()
            {
                return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                    (DoNotImport),
                    (JournalType),
                    (Date),
                    (Description1),
                    (ReferenceNumber),
                    (RowNumber),
                    (AccountNumber),
                    (Location),
                    (Department),
                    (GLDIMFUND),
                    (GLENTRY_CLASSID),
                    (GLENTRY_VENDORID),
                    (AmountDebit),
                    (AmountCredit),
                    (Memo)
                    );

            }
        }

        #endregion 
    }

    static class StringExtensions
    {
        public static string TrimLength( this string value, int maxLength )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring( 0, maxLength );
        }
    }
}