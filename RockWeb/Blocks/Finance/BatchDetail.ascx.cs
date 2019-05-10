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
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Batch Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial batch." )]

    [LinkedPage( "Transaction Matching Page", "Page used to match transactions for a batch.", order: 1 )]
    [LinkedPage( "Audit Page", "Page used to display the history of changes to a batch.", order: 2 )]
    [DefinedTypeField( "Batch Names", "The Defined Type that contains a predefined list of batch names to choose from instead of entering it in manually when adding a new batch. Leave this blank to hide this option and let them edit the batch name manually.", false, "", "", 3 )]
    public partial class BatchDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccounts.DataKeyNames = new string[] { "Id" };
            gAccounts.ShowActionRow = false;

            gCurrencyTypes.DataKeyNames = new string[] { "Id" };
            gCurrencyTypes.ShowActionRow = false;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var batchId = PageParameter( "batchId" ).AsInteger();
            if ( !Page.IsPostBack )
            {
                ShowDetail( batchId );
            }

            // Add any attribute controls. 
            // This must be done here regardless of whether it is a postback so that the attribute values will get saved.
            var financialBatch = new FinancialBatchService( new RockContext() ).Get( batchId );
            if ( financialBatch == null )
            {
                financialBatch = new FinancialBatch();
            }

            financialBatch.LoadAttributes();
            phAttributes.Controls.Clear();
            Helper.AddEditControls( financialBatch, phAttributes, true, BlockValidationGroup );
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? batchId = PageParameter( pageReference, "batchId" ).AsIntegerOrNull();
            if ( batchId != null )
            {
                string batchName = new FinancialBatchService( new RockContext() )
                    .Queryable().Where( b => b.Id == batchId.Value )
                    .Select( b => b.Name )
                    .FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( batchName ) )
                {
                    breadCrumbs.Add( new BreadCrumb( batchName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Batch", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetBatch( hfBatchId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the lbMatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMatch_Click( object sender, EventArgs e )
        {
            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "BatchId", hfBatchId.Value );
            NavigateToLinkedPage( "TransactionMatchingPage", qryParam );
        }

        /// <summary>
        /// Handles the Click event of the lbHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHistory_Click( object sender, EventArgs e )
        {
            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "BatchId", hfBatchId.Value );
            NavigateToLinkedPage( "AuditPage", qryParam );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var batchService = new FinancialBatchService( rockContext );
            FinancialBatch batch = null;

            var changes = new History.HistoryChangeList();

            int batchId = hfBatchId.Value.AsInteger();
            if ( batchId == 0 )
            {
                batch = new FinancialBatch();
                batchService.Add( batch );
                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
            }
            else
            {
                batch = batchService.Get( batchId );
            }

            if ( batch != null )
            {
                if ( dvpBatchName.Visible )
                {
                    History.EvaluateChange( changes, "Batch Name", batch.Name, dvpBatchName.SelectedItem.Text );
                    batch.Name = dvpBatchName.SelectedItem.Text;
                }
                else
                {
                    History.EvaluateChange( changes, "Batch Name", batch.Name, tbName.Text );
                    batch.Name = tbName.Text;
                }

                BatchStatus batchStatus = (BatchStatus)ddlStatus.SelectedIndex;

                string errorMessage;
                if ( !batch.IsValidBatchStatusChange( batch.Status, batchStatus, this.CurrentPerson, out errorMessage ) )
                {
                    cvBatch.IsValid = false;
                    cvBatch.ErrorMessage = errorMessage;
                    return;
                }

                History.EvaluateChange( changes, "Status", batch.Status, batchStatus );
                batch.Status = batchStatus;

                CampusCache oldCampus = null;
                if ( batch.CampusId.HasValue )
                {
                    oldCampus = CampusCache.Get( batch.CampusId.Value );
                }

                CampusCache newCampus = null;
                if ( campCampus.SelectedCampusId.HasValue )
                {
                    newCampus = CampusCache.Get( campCampus.SelectedCampusId.Value );
                }

                History.EvaluateChange( changes, "Campus", oldCampus != null ? oldCampus.Name : "None", newCampus != null ? newCampus.Name : "None" );
                batch.CampusId = campCampus.SelectedCampusId;

                DateTime? startDateTime = dtpStart.SelectedDateTimeIsBlank ? null : dtpStart.SelectedDateTime;
                History.EvaluateChange( changes, "Start Date/Time", batch.BatchStartDateTime, startDateTime );
                batch.BatchStartDateTime = startDateTime;

                DateTime? endDateTime;
                if ( dtpEnd.SelectedDateTimeIsBlank && batch.BatchStartDateTime.HasValue )
                {
                    endDateTime = batch.BatchStartDateTime.Value.AddDays( 1 );
                }
                else
                {
                    endDateTime = dtpEnd.SelectedDateTimeIsBlank ? null : dtpEnd.SelectedDateTime;
                }

                History.EvaluateChange( changes, "End Date/Time", batch.BatchEndDateTime, endDateTime );
                batch.BatchEndDateTime = endDateTime;

                decimal controlAmount = tbControlAmount.Text.AsDecimal();
                History.EvaluateChange( changes, "Control Amount", batch.ControlAmount.FormatAsCurrency(), controlAmount.FormatAsCurrency() );
                batch.ControlAmount = controlAmount;

                int? controlItemCount = nbControlItemCount.Text.AsIntegerOrNull();
                History.EvaluateChange( changes, "Control Item Count", batch.ControlItemCount.FormatAsCurrency(), controlItemCount.FormatAsCurrency() );
                batch.ControlItemCount = controlItemCount;

                History.EvaluateChange( changes, "Accounting System Code", batch.AccountingSystemCode, tbAccountingCode.Text );
                batch.AccountingSystemCode = tbAccountingCode.Text;

                History.EvaluateChange( changes, "Notes", batch.Note, tbNote.Text );
                batch.Note = tbNote.Text;

                cvBatch.IsValid = batch.IsValid;
                if ( !Page.IsValid || !batch.IsValid )
                {
                    cvBatch.ErrorMessage = batch.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                batch.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, batch );

                rockContext.WrapTransaction( () =>
                {
                    if ( rockContext.SaveChanges() > 0 )
                    {
                        if ( changes.Any() )
                        {
                            pdAuditDetails.SetEntity( batch, ResolveRockUrl( "~" ) );
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                batch.Id,
                                changes );
                        }
                    }
                } );

                batch.SaveAttributeValues( rockContext );

                if ( batchId == 0 )
                {
                    // If created a new batch, navigate to same page so that transaction list displays correctly
                    var pageReference = CurrentPageReference;
                    pageReference.Parameters.AddOrReplace( "batchId", batch.Id.ToString() );
                    NavigateToPage( pageReference );
                }
                else
                {
                    hfBatchId.SetValue( batch.Id );

                    // Requery the batch to support EF navigation properties
                    var savedBatch = GetBatch( batch.Id );
                    ShowReadonlyDetails( savedBatch );

                    // If there is a batch context item, update the context's properties with new values
                    var contextObjects = new Dictionary<string, object>();
                    foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
                    {
                        var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                        if ( contextEntity is FinancialBatch )
                        {
                            var contextBatch = contextEntity as FinancialBatch;
                            contextBatch.CopyPropertiesFrom( batch );
                        }
                    }

                    // Then refresh transaction list
                    RockPage.UpdateBlocks( "~/Blocks/Finance/TransactionList.ascx" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int batchId = hfBatchId.ValueAsInt();
            if ( batchId != 0 )
            {
                ShowReadonlyDetails( GetBatch( batchId ) );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int batchId = hfBatchId.ValueAsInt();
            if ( batchId != 0 )
            {
                ShowReadonlyDetails( GetBatch( batchId ) );
            }
            else
            {
                ShowEditDetails( GetBatch( batchId ) );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the batch.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <returns></returns>
        private FinancialBatch GetBatch( int batchId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var batch = new FinancialBatchService( rockContext ).Get( batchId );
            return batch;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="batchId">The financial batch identifier.</param>
        public void ShowDetail( int batchId )
        {
            FinancialBatch batch = null;

            bool editAllowed = true;

            if ( !batchId.Equals( 0 ) )
            {
                batch = GetBatch( batchId );
                if ( batch != null )
                {
                    editAllowed = batch.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    pdAuditDetails.SetEntity( batch, ResolveRockUrl( "~" ) );
                }
            }

            if ( batch == null )
            {
                batch = new FinancialBatch { Id = 0, Status = BatchStatus.Open };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfBatchId.Value = batch.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
            }
            else if ( batch.Status == BatchStatus.Closed )
            {
                if ( !batch.IsAuthorized( "ReopenBatch", this.CurrentPerson ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = "Batch is closed and requires authorization to re-open before editing.";
                }
            }

            if ( readOnly )
            {
                lbEdit.Visible = false;
                ShowReadonlyDetails( batch );
            }
            else
            {
                lbEdit.Visible = true;
                if ( batch.Id > 0 )
                {
                    ShowReadonlyDetails( batch );
                }
                else
                {
                    ShowEditDetails( batch );
                }
            }

            lbSave.Visible = !readOnly;
        }

        /// <summary>
        /// Shows the financial batch summary.
        /// </summary>
        /// <param name="batch">The financial batch.</param>
        private void ShowReadonlyDetails( FinancialBatch batch )
        {
            SetEditMode( false );

            if ( batch != null )
            {
                hfBatchId.SetValue( batch.Id );

                SetHeadingInfo( batch, batch.Name );

                string campusName = string.Empty;
                if ( batch.CampusId.HasValue )
                {
                    var campus = CampusCache.Get( batch.CampusId.Value );
                    if ( campus != null )
                    {
                        campusName = campus.ToString();
                    }
                }

                var rockContext = new RockContext();
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var batchTransactionsQuery = financialTransactionService.Queryable().Where( a => a.BatchId.HasValue && a.BatchId.Value == batch.Id );

                var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
                var qryTransactionDetails = financialTransactionDetailService.Queryable().Where( a => a.Transaction.BatchId == batch.Id );
                decimal amountTotal = qryTransactionDetails.Select( a => (decimal?)a.Amount ).Sum() ?? 0;

                decimal amountVariance = amountTotal - batch.ControlAmount;
                string varianceAmountText = string.Format(
                    "{0} / {1} / " + ( amountVariance == 0.0M ? "{2}" : "<span class='label label-danger'>{2}</span>" ),
                    amountTotal.FormatAsCurrency(),
                    batch.ControlAmount.FormatAsCurrency(),
                    amountVariance.FormatAsCurrency() );

                string varianceCountText = null;

                if ( batch.ControlItemCount.HasValue )
                {
                    var itemCountTotal = batchTransactionsQuery.Count();
                    int itemCountVariance = itemCountTotal - batch.ControlItemCount.Value;
                    varianceCountText = string.Format(
                        "{0} / {1} / " + ( itemCountVariance == 0 ? "{2}" : "<span class='label label-danger'>{2}</span>" ),
                        itemCountTotal,
                        batch.ControlItemCount.Value,
                        itemCountVariance );
                }

                lDetails.Text = new DescriptionList()
                    .Add( "Date Range", new DateRange( batch.BatchStartDateTime, batch.BatchEndDateTime ).ToString( "g" ) )
                    .Add( "Transaction Amount / Control / Variance", varianceAmountText )
                    .Add( "Transaction Item Count / Control / Variance", varianceCountText, false )
                    .Add( "Accounting Code", batch.AccountingSystemCode )
                    .Add( "Notes", batch.Note )
                    .Html;

                batch.LoadAttributes();
                var attributes = batch.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

                var attributeCategories = Helper.GetAttributeCategories( attributes );

                Rock.Attribute.Helper.AddDisplayControls( batch, attributeCategories, phReadonlyAttributes, null, false );

                // Account Summary
                gAccounts.DataSource = qryTransactionDetails
                    .GroupBy( d => new
                    {
                        AccountId = d.AccountId,
                        AccountName = d.Account.Name
                    } )
                    .Select( s => new
                    {
                        Id = s.Key.AccountId,
                        Name = s.Key.AccountName,
                        Amount = s.Sum( a => (decimal?)a.Amount ) ?? 0.0M
                    } )
                    .OrderBy( s => s.Name )
                    .ToList();

                gAccounts.DataBind();

                // Currency Summary
                gCurrencyTypes.DataSource = batchTransactionsQuery
                    .GroupBy( c => new
                    {
                        CurrencyTypeValueId = c.FinancialPaymentDetailId.HasValue ? c.FinancialPaymentDetail.CurrencyTypeValueId : 0,
                    } )
                    .Select( s => new
                    {
                        CurrencyTypeValueId = s.Key.CurrencyTypeValueId,
                        Amount = s.Sum( a => (decimal?)a.TransactionDetails.Sum( t => t.Amount ) ) ?? 0.0M
                    } )
                    .ToList()
                    .Select( s => new
                    {
                        Id = s.CurrencyTypeValueId,
                        Name = DefinedValueCache.GetName( s.CurrencyTypeValueId ),
                        Amount = s.Amount
                    } ).OrderBy( a => a.Name ).ToList();

                gCurrencyTypes.DataBind();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="batch">The financial batch.</param>
        protected void ShowEditDetails( FinancialBatch batch )
        {
            if ( batch == null || batch.Id == 0 )
            {
                // if the "BatchNames" configuration setting is set, and this is a new batch present a DropDown of BatchNames instead of a text box
                var batchNamesDefinedTypeGuid = this.GetAttributeValue( "BatchNames" ).AsGuidOrNull();
                dvpBatchName.Visible = false;
                tbName.Visible = true;

                if ( batchNamesDefinedTypeGuid.HasValue )
                {
                    var batchNamesDefinedType = DefinedTypeCache.Get( batchNamesDefinedTypeGuid.Value );
                    if ( batchNamesDefinedType != null )
                    {
                        dvpBatchName.DefinedTypeId = batchNamesDefinedType.Id;
                        if ( batchNamesDefinedType.DefinedValues.Any( a => !string.IsNullOrWhiteSpace(a.Value) ) )
                        {
                            dvpBatchName.Visible = true;
                            tbName.Visible = false;
                        }
                    }
                }
            }

            if ( batch != null )
            {
                hfBatchId.Value = batch.Id.ToString();
                string title = batch.Id > 0 ?
                    ActionTitle.Edit( FinancialBatch.FriendlyTypeName ) :
                    ActionTitle.Add( FinancialBatch.FriendlyTypeName );

                SetHeadingInfo( batch, title );

                SetEditMode( true );

                tbName.Text = batch.Name;

                ddlStatus.BindToEnum<BatchStatus>();
                ddlStatus.SelectedIndex = (int)(BatchStatus)batch.Status;
                ddlStatus.Enabled = true;
                if ( batch.Status == BatchStatus.Closed )
                {
                    if ( !batch.IsAuthorized( "ReopenBatch", this.CurrentPerson ) )
                    {
                        ddlStatus.Enabled = false;
                    }
                }

                if ( batch.IsAutomated == true && batch.Status == BatchStatus.Pending )
                {
                    ddlStatus.Enabled = false;
                }

                campCampus.Campuses = CampusCache.All();
                if ( batch.CampusId.HasValue )
                {
                    campCampus.SetValue( batch.CampusId.Value );
                }

                tbControlAmount.Text = batch.ControlAmount.ToString( "N2" );
                nbControlItemCount.Text = batch.ControlItemCount.ToString();

                dtpStart.SelectedDateTime = batch.BatchStartDateTime;
                dtpEnd.SelectedDateTime = batch.BatchEndDateTime;

                tbAccountingCode.Text = batch.AccountingSystemCode;
                tbNote.Text = batch.Note;

                SetEditableForBatchStatus( ddlStatus.SelectedValueAsEnum<BatchStatus>() );
            }
        }

        /// <summary>
        /// Sets the editable for batch status.
        /// </summary>
        /// <param name="batch">The batch.</param>
        private void SetEditableForBatchStatus( BatchStatus batchStatus )
        {
            if ( batchStatus == BatchStatus.Closed )
            {
                tbName.ReadOnly = true;
                dtpStart.Enabled = false;
                dtpEnd.Enabled = false;
                tbControlAmount.ReadOnly = true;
                nbControlItemCount.ReadOnly = true;
                campCampus.Enabled = false;
                tbAccountingCode.ReadOnly = true;
                tbNote.ReadOnly = true;
            }
            else
            {
                tbName.ReadOnly = false;
                dtpStart.Enabled = true;
                dtpEnd.Enabled = true;
                tbControlAmount.ReadOnly = false;
                nbControlItemCount.ReadOnly = false;
                campCampus.Enabled = true;
                tbAccountingCode.ReadOnly = false;
                tbNote.ReadOnly = false;
            }
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <param name="title">The title.</param>
        private void SetHeadingInfo( FinancialBatch batch, string title )
        {
            lTitle.Text = title.FormatAsHtmlTitle();

            SetHeadingBatchStatus( batch.Status );

            if ( batch.Campus != null )
            {
                hlCampus.Visible = true;
                hlCampus.Text = batch.Campus.Name;
            }
            else
            {
                hlCampus.Visible = false;
            }

            hlBatchId.Text = string.Format( "Batch #{0}", batch.Id.ToString() );
            hlBatchId.Visible = batch.Id != 0;

            hlIsAutomated.Visible = batch.IsAutomated;
        }

        /// <summary>
        /// Sets the heading batch status.
        /// </summary>
        /// <param name="batchStatus">The batch status.</param>
        private void SetHeadingBatchStatus( BatchStatus batchStatus )
        {
            hlStatus.Text = batchStatus.ConvertToString();
            switch ( batchStatus )
            {
                case BatchStatus.Pending:
                    hlStatus.LabelType = LabelType.Danger;
                    break;
                case BatchStatus.Open:
                    hlStatus.LabelType = LabelType.Warning;
                    break;
                case BatchStatus.Closed:
                    hlStatus.LabelType = LabelType.Default;
                    break;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void ddlStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var batchStatus = ddlStatus.SelectedValueAsEnum<BatchStatus>();
            SetEditableForBatchStatus( batchStatus );
            SetHeadingBatchStatus( batchStatus );
        }

        #endregion
    }
}
