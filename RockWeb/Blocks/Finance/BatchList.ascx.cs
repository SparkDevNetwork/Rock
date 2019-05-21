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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Batch List" )]
    [Category( "Finance" )]
    [Description( "Lists all financial batches and provides filtering by campus, status, etc." )]
    [LinkedPage( "Detail Page", order: 0 )]
    [BooleanField( "Show Accounting Code", "Should the accounting code column be displayed.", false, "", 1 )]
    [BooleanField( "Show Accounts Column", "Should the accounts column be displayed.", true, "", 2 )]
    [CodeEditorField( "Summary Lava Template", "The lava template for display the content for summary", CodeEditorMode.Lava, CodeEditorTheme.Rock, order: 3, defaultValue: @"
         <div class='panel panel-block'>
            <div class='panel-heading'>
                <h1 class='panel-title'>Total Results</h1>
            </div>
            <div class='panel-body'>
                {% assign totalAmount = 0 %}
                {% for batchSummary in BatchSummary %}
                <div class='row'>
                    <div class='col-xs-8'>{{ batchSummary.FinancialAccount.Name }}</div>
                    <div class='col-xs-4 text-right'>{{ batchSummary.TotalAmount | FormatAsCurrency }}</div>
                </div>
                {% assign totalAmount = totalAmount | Plus: batchSummary.TotalAmount %}
                {% endfor %}
                <div class='row'>
                    <div class='col-xs-8'><b>Total: </div>
                    <div class='col-xs-4 text-right'>
                        {{ totalAmount | FormatAsCurrency }}
                    </div>
                </div>
            </div>
        </div>
" )]

    public partial class BatchList : RockBlock, IPostBackEventHandler, ICustomGridColumns
    {
        #region Constants

        private const string SUMMARY_LAVA_TEMPLATE = "SummaryLavaTemplate";

        #endregion

        #region Fields

        private RockDropDownList ddlAction;
        public List<AttributeCache> AvailableAttributes { get; set; }

        // Dictionaries to cache values for performance
        private static Dictionary<int, FinancialAccount> _financialAccountLookup;

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
                                Rock.dialogs.confirm('This batch has transactions. Are you sure that you want to delete this batch and all of its transactions?', function (result) {
                                    if (result) {
                                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                                    }
                                });
                            } else {
                                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        }
                    });
                });";

            ScriptManager.RegisterStartupScript( gBatchList, gBatchList.GetType(), "deleteBatchScript", deleteScript, true );

            gBatchList.Actions.AddCustomActionControl( ddlAction );
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
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
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
                        $('#{3}').val($ddl.val());
                        window.location = ""javascript:{2}"";
                    }}
                    else
                    {{
                        var $ddl = $(this);
                        if ($ddl.val() != '') {{
                            Rock.dialogs.confirm('Are you sure you want to ' + ($ddl.val() == 'OPEN' ? 'open' : 'close') + ' the selected batches?', function (result) {{
                                if (result) {{
                                    $('#{3}').val($ddl.val());
                                    window.location = ""javascript:{2}"";
                                }}
                                $ddl.val('');
                            }});
                        }}
                    }}
                }});";

            string script = string.Format( 
                scriptFormat, 
                ddlAction.ClientID, // {0}
                gBatchList.ClientID,  // {1}
                Page.ClientScript.GetPostBackEventReference( this, "StatusUpdate" ),  // {2}
                hfAction.ClientID // {3}
                );

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
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => "Attribute_" + a.Key == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

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
                case "Contains Source Type":
                    {
                        var sourceTypeValueId = e.Value.AsIntegerOrNull();
                        if ( sourceTypeValueId.HasValue )
                        {
                            e.Value = DefinedValueCache.GetValue( sourceTypeValueId.Value );
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
            gfBatchFilter.SaveUserPreference( "Contains Transaction Type", dvpTransactionType.SelectedValue );
            gfBatchFilter.SaveUserPreference( "Contains Source Type", dvpSourceType.SelectedValue );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfBatchFilter.SaveUserPreference( "Attribute_" + attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }

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
                if ( batch.IsAuthorized( Rock.Security.Authorization.DELETE, CurrentPerson ) )
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

                        var changes = new History.HistoryChangeList();
                        changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Batch" );
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            batch.Id,
                            changes );

                        batchService.Delete( batch );

                        rockContext.SaveChanges();
                    } );
                }
                else
                {
                    mdGridWarning.Show( "You are not authorized to delete the selected batch.", ModalAlertType.Warning);
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
                var cell = ( e.Row.Cells[gBatchList.GetColumnIndex( deleteField )] as DataControlFieldCell ).Controls[0];

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
            if ( eventArgument == "StatusUpdate" && hfAction.Value.IsNotNullOrWhiteSpace() )
            {
                var batchesSelected = new List<int>();

                gBatchList.SelectedKeys.ToList().ForEach( b => batchesSelected.Add( b.ToString().AsInteger() ) );

                if ( batchesSelected.Any() )
                {
                    var newStatus = hfAction.Value == "OPEN" ? BatchStatus.Open : BatchStatus.Closed;
                    var rockContext = new RockContext();
                    var batchService = new FinancialBatchService( rockContext );
                    var batchesToUpdate = batchService.Queryable()
                        .Where( b =>
                            batchesSelected.Contains( b.Id ) &&
                            b.Status != newStatus )
                        .ToList();

                    foreach ( var batch in batchesToUpdate )
                    {
                        var changes = new History.HistoryChangeList();
                        History.EvaluateChange( changes, "Status", batch.Status, newStatus );

                        string errorMessage;
                        if ( !batch.IsValidBatchStatusChange( batch.Status, newStatus, this.CurrentPerson, out errorMessage ) )
                        {
                            maWarningDialog.Show( errorMessage, ModalAlertType.Warning );
                            return;
                        }

                        if ( batch.IsAutomated && batch.Status == BatchStatus.Pending && newStatus != BatchStatus.Pending )
                        {
                            errorMessage = string.Format( "{0} is an automated batch and the status can not be modified when the status is pending. The system will automatically set this batch to OPEN when all transactions have been downloaded.", batch.Name );
                            maWarningDialog.Show( errorMessage, ModalAlertType.Warning );
                            return;
                        } 

                        batch.Status = newStatus;

                        if ( !batch.IsValid )
                        {
                            string message = string.Format( "Unable to update status for the selected batches.<br/><br/>{0}", batch.ValidationResults.AsDelimited( "<br/>" ) );
                            maWarningDialog.Show( message, ModalAlertType.Warning );
                            return;
                        }

                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            batch.Id,
                            changes,
                            false );
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
                hfAction.Value = string.Empty;
                BindGrid();
            }
        }

        #endregion

        #region Methods

        private void SetVisibilityOption()
        {
            bool showAccountingCode = GetAttributeValue( "ShowAccountingCode" ).AsBoolean();
            tbAccountingCode.Visible = showAccountingCode;
            var accountingCodeColumn = gBatchList.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "AccountingSystemCode" );
            if ( accountingCodeColumn != null )
            {
                accountingCodeColumn.Visible = showAccountingCode;
            }

            if ( showAccountingCode )
            {
                string accountingCode = gfBatchFilter.GetUserPreference( "Accounting Code" );
                tbAccountingCode.Text = !string.IsNullOrWhiteSpace( accountingCode ) ? accountingCode : string.Empty;
            }

            bool showAccountsColumn = GetAttributeValue( "ShowAccountsColumn" ).AsBoolean();
            var accountsColumn = gBatchList.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.HeaderText == "Accounts" );
            if ( accountsColumn != null )
            {
                accountsColumn.Visible = showAccountsColumn;
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            string titleFilter = gfBatchFilter.GetUserPreference( "Title" );
            tbTitle.Text = !string.IsNullOrWhiteSpace( titleFilter ) ? titleFilter : string.Empty;

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
            dvpTransactionType.DefinedTypeId = definedTypeTransactionTypes.Id;
            dvpTransactionType.SetValue( gfBatchFilter.GetUserPreference( "Contains Transaction Type" ) );

            var campusi = CampusCache.All();
            campCampus.Campuses = campusi;
            campCampus.Visible = campusi.Any();
            campCampus.SetValue( gfBatchFilter.GetUserPreference( "Campus" ) );

            drpBatchDate.DelimitedValues = gfBatchFilter.GetUserPreference( "Date Range" );

            var definedTypeSourceTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() );
            dvpSourceType.DefinedTypeId = definedTypeSourceTypes.Id;
            dvpSourceType.SetValue( gfBatchFilter.GetUserPreference( "Contains Source Type" ) );

            BindAttributes();
            AddDynamicControls();
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
        /// Handles the DataBound event of the lVarianceAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lVarianceAmount_DataBound( object sender, RowEventArgs e )
        {
            Literal lVarianceAmount = sender as Literal;
            BatchRow batchRow = e.Row.DataItem as BatchRow;
            if (batchRow.AmountVariance != 0.00M)
            {
                lVarianceAmount.Text = string.Format( "<span class='label label-danger'>{0}</span>", batchRow.AmountVariance.FormatAsCurrency() );
            }
            else
            {
                lVarianceAmount.Text = string.Format( "<span class=''>{0}</span>", batchRow.AmountVariance.FormatAsCurrency() );
            }
            
        }

        /// <summary>
        /// Handles the DataBound event of the lVarianceItemCount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lVarianceItemCount_DataBound( object sender, RowEventArgs e )
        {
            Literal lVarianceItemCount = sender as Literal;
            BatchRow batchRow = e.Row.DataItem as BatchRow;
            if ( batchRow.ItemCountVariance.HasValue )
            {
                if ( batchRow.ItemCountVariance != 0 )
                {
                    lVarianceItemCount.Text = string.Format( "<span class='label label-danger'>{0}</span>", batchRow.ItemCountVariance );
                }
                else
                {
                    lVarianceItemCount.Text = string.Format( "<span class=''>{0}</span>", batchRow.ItemCountVariance );
                }
            }
            else
            {
                // doesn't apply
                lVarianceItemCount.Text = "-";
            }
        }

        /// <summary>
        /// Handles the DataBound event of the lBatchStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lBatchStatus_DataBound( object sender, RowEventArgs e )
        {
            Literal lBatchStatus = sender as Literal;
            BatchRow batchRow = e.Row.DataItem as BatchRow;
            lBatchStatus.Text = string.Format( "<span class='{0}'>{1}</span>", batchRow.StatusLabelClass, batchRow.StatusText );
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
                var rockContext = new RockContext();
                _financialAccountLookup = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().ToList().ToDictionary( k => k.Id, v => v );

                var financialBatchQry = GetQuery( rockContext ).AsNoTracking();

                var batchRowQry = financialBatchQry.Select( b => new BatchRow
                {
                    Id = b.Id,
                    BatchStartDateTime = b.BatchStartDateTime.Value,
                    Name = b.Name,
                    AccountingSystemCode = b.AccountingSystemCode,
                    TransactionCount = b.Transactions.Count(),
                    ControlAmount = b.ControlAmount,
                    ControlItemCount = b.ControlItemCount,
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
                            Amount = s.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M
                        } )
                        .ToList()
                } );

                gBatchList.ObjectList = financialBatchQry.ToList().ToDictionary( k => k.Id.ToString(), v => v as object );

                gBatchList.SetLinqDataSource( batchRowQry.AsNoTracking() );
                gBatchList.EntityTypeId = EntityTypeCache.Get<FinancialBatch>().Id;
                gBatchList.DataBind();

                RegisterJavaScriptForGridActions();

                var qryTransactionDetails = financialBatchQry.SelectMany( a => a.Transactions ).SelectMany( a => a.TransactionDetails );
                var accountSummaries = qryTransactionDetails.GroupBy( a => a.Account ).Select( a => new
                {
                    FinancialAccount = a.Key,
                    TotalAmount = (decimal?)a.Sum( d => d.Amount )
                } ).OrderBy( a => a.FinancialAccount.Order )
                .ToList();

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
                mergeFields.Add( "BatchSummary", accountSummaries );

                string lavaTemplate = this.GetAttributeValue( SUMMARY_LAVA_TEMPLATE );
                lSummary.Text = lavaTemplate.ResolveMergeFields( mergeFields );
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IOrderedQueryable<FinancialBatch> GetQuery( RockContext rockContext )
        {
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
                qry = qry.Where( batch => batch.Name.Contains( title ) );
            }

            // filter by accounting code
            if ( tbAccountingCode.Visible )
            {
                string accountingCode = gfBatchFilter.GetUserPreference( "Accounting Code" );
                if ( !string.IsNullOrEmpty( accountingCode ) )
                {
                    qry = qry.Where( batch => batch.AccountingSystemCode.Contains( accountingCode ) );
                }
            }

            // filter by campus
            var campus = CampusCache.Get( gfBatchFilter.GetUserPreference( "Campus" ).AsInteger() );
            if ( campus != null )
            {
                qry = qry.Where( b => b.CampusId == campus.Id );
            }

            // filter by batches that contain transactions of the specified source type
            var sourceTypeValueId = gfBatchFilter.GetUserPreference( "Contains Source Type" ).AsIntegerOrNull();
            if ( sourceTypeValueId.HasValue )
            {
                qry = qry.Where( a => a.Transactions.Any( t => t.SourceTypeValueId == sourceTypeValueId.Value ) );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, batchService, Rock.Reporting.FilterMode.SimpleFilter );
                }
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
                                sortedQry = qry.OrderBy( b => b.Transactions.Sum( t => (decimal?)( t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M ) ) ?? 0.0M );
                            }
                            else
                            {
                                sortedQry = qry.OrderByDescending( b => b.Transactions.Sum( t => (decimal?)( t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M ) ) ?? 0.0M );
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

        #endregion

        #region Helper Class

        public class BatchAccountSummary
        {
            public int AccountId { get; set; }
            public int AccountOrder
            {
                get
                {
                    return _financialAccountLookup[this.AccountId].Order;
                }
            }

            public string AccountName
            {
                get
                {
                    return _financialAccountLookup[this.AccountId].Name;
                }
            }

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

            public decimal TransactionAmount
            {
                get
                {
                    return AccountSummaryList.Select( a => a.Amount ).Sum();
                }
            }

            public decimal ControlAmount { get; set; }

            public int? ControlItemCount { get; set; }

            public List<BatchAccountSummary> AccountSummaryList
            {
                get
                {
                    return _accountSummaryList.OrderBy( a => a.AccountOrder ).ToList();
                }
                set
                {
                    _accountSummaryList = value;
                }
            }

            private List<BatchAccountSummary> _accountSummaryList;
            public string CampusName { get; set; }
            public BatchStatus Status { get; set; }
            public bool UnMatchedTxns { get; set; }
            public string BatchNote { get; set; }

            /// <summary>
            /// Gets the amount variance.
            /// </summary>
            /// <value>
            /// The amount variance.
            /// </value>
            public decimal AmountVariance
            {
                get
                {
                    return TransactionAmount - ControlAmount;
                }
            }

            /// <summary>
            /// Gets the item count variance.
            /// </summary>
            /// <value>
            /// The item count variance.
            /// </value>
            public int? ItemCountVariance
            {
                get
                {
                    if ( ControlItemCount.HasValue )
                    {
                        return TransactionCount - ControlItemCount;
                    }
                    else
                    {
                        return ( int? ) null;
                    }
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
                        case BatchStatus.Closed: return "label label-default";
                        case BatchStatus.Open: return "label label-info";
                        case BatchStatus.Pending: return "label label-warning";
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

        #endregion

        #region Attributes

        /// <summary>
        /// Binds the attributes
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new FinancialBatch().TypeId;
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }

        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gBatchList.Columns.OfType<AttributeField>().ToList() )
            {
                gBatchList.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = (IRockControl)control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = gfBatchFilter.GetUserPreference( "Attribute_" + attribute.Key );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }

                    bool columnExists = gBatchList.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gBatchList.Columns.Add( boundField );
                    }
                }
            }

            // Add delete column
            var deleteField = new DeleteField();
            gBatchList.Columns.Add( deleteField );
            deleteField.Click += gBatchList_Delete;
        }

        #endregion Attributes
    }
}
