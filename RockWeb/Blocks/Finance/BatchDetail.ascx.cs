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

    [LinkedPage( "Transaction Matching Page", "Page used to match transactions for a batch." )]
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

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "batchId" ).AsInteger() );
            }
        }

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
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var batchService = new FinancialBatchService( rockContext );
            FinancialBatch batch = null;

            int batchId = hfBatchId.Value.AsInteger();
            if ( batchId == 0 )
            {
                batch = new FinancialBatch();
                batchService.Add( batch );
            }
            else
            {
                batch = batchService.Get( batchId );
            }

            if ( batch != null )
            {
                batch.Name = tbName.Text;
                batch.Status = (BatchStatus)ddlStatus.SelectedIndex;
                batch.CampusId = campCampus.SelectedCampusId;
                batch.BatchStartDateTime = dtpStart.SelectedDateTimeIsBlank ? null : dtpStart.SelectedDateTime;
                if ( dtpEnd.SelectedDateTimeIsBlank && batch.BatchStartDateTime.HasValue )
                {
                    batch.BatchEndDateTime = batch.BatchStartDateTime.Value.AddDays( 1 );
                }
                else
                {
                    batch.BatchEndDateTime = dtpEnd.SelectedDateTimeIsBlank ? null : dtpEnd.SelectedDateTime;
                }
                batch.ControlAmount = tbControlAmount.Text.AsDecimal();
                batch.AccountingSystemCode = tbAccountingCode.Text;

                cvBatch.IsValid = batch.IsValid;
                if ( !Page.IsValid || !batch.IsValid )
                {
                    cvBatch.ErrorMessage = batch.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                rockContext.SaveChanges();

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
            var batch = new FinancialBatchService( rockContext )
                .Queryable( "Campus,Transactions.TransactionDetails.Account" )
                .Where( b => b.Id == batchId )
                .FirstOrDefault();
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
                }
            }

            if ( batch == null )
            {
                batch = new FinancialBatch { Id = 0 };
            }

            hfBatchId.Value = batch.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
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

                string campus = string.Empty;
                if ( batch.Campus != null )
                {
                    campus = batch.Campus.ToString();
                }

                decimal txnTotal = batch.Transactions.Sum( t => (decimal?)( t.TransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M ) ) ?? 0.0M;
                decimal variance = txnTotal - batch.ControlAmount;
                string amountFormat = string.Format( "{0} / {1} / " + ( variance == 0.0M ? "{2}" : "<span class='label label-danger'>{2}</span>" ),
                    txnTotal.ToString( "C2" ), batch.ControlAmount.ToString( "C2" ), variance.ToString( "C2" ) );

                lDetails.Text = new DescriptionList()
                    .Add( "Date Range", new DateRange( batch.BatchStartDateTime, batch.BatchEndDateTime ).ToString( "g" ) )
                    .Add( "Transaction / Control / Variance", amountFormat )
                    .Add( "Accounting Code", batch.AccountingSystemCode )
                    .Html;
                //Account Summary
                gAccounts.DataSource = batch.Transactions
                    .SelectMany( t => t.TransactionDetails )
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

                //Currency Summary
                gCurrencyTypes.DataSource = batch.Transactions
                    .Select(t=> new
                    {
                        CurrencyTypeValue=t.CurrencyTypeValue,
                        TotalAmount = t.TotalAmount
                    }) 
                    .GroupBy( c => new
                    {
                        CurrencyId = c.CurrencyTypeValue.Id,
                        CurrencyName = c.CurrencyTypeValue.Value
                    } )
                    .Select( s => new
                    {
                        Id = s.Key.CurrencyId,
                        Name = s.Key.CurrencyName,
                        Amount = s.Sum( a => (decimal?)a.TotalAmount ) ?? 0.0M
                    } )
                    .OrderBy( s => s.Name )
                    .ToList();
                gCurrencyTypes.DataBind();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="batch">The financial batch.</param>
        protected void ShowEditDetails( FinancialBatch batch )
        {
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

                campCampus.Campuses = CampusCache.All();
                if ( batch.CampusId.HasValue )
                {
                    campCampus.SetValue( batch.CampusId.Value );
                }

                tbControlAmount.Text = batch.ControlAmount.ToString( "N2" );

                dtpStart.SelectedDateTime = batch.BatchStartDateTime;
                dtpEnd.SelectedDateTime = batch.BatchEndDateTime;

                tbAccountingCode.Text = batch.AccountingSystemCode;
            }
        }

        private void SetHeadingInfo( FinancialBatch batch, string title )
        {
            lTitle.Text = title.FormatAsHtmlTitle();

            hlStatus.Text = batch.Status.ConvertToString();
            switch ( batch.Status )
            {
                case BatchStatus.Pending: hlStatus.LabelType = LabelType.Warning; break;
                case BatchStatus.Open: hlStatus.LabelType = LabelType.Info; break;
                case BatchStatus.Closed: hlStatus.LabelType = LabelType.Default; break;
            }

            if ( batch.Campus != null )
            {
                hlCampus.Visible = true;
                hlCampus.Text = batch.Campus.Name;
            }
            else
            {
                hlCampus.Visible = false;
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

        #endregion
    }
}
