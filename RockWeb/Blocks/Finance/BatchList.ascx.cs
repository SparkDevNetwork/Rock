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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Batch List" )]
    [Category( "Finance" )]
    [Description( "Lists all financial batches and provides filtering by campus, status, etc." )]

    [LinkedPage( "Detail Page" )]
    public partial class BatchList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfBatchFilter.ApplyFilterClick += gfBatchFilter_ApplyFilterClick;
            gfBatchFilter.DisplayFilterValue += gfBatchFilter_DisplayFilterValue;

            _canConfigure = IsUserAuthorized( Authorization.EDIT );

            if ( _canConfigure )
            {
                gBatchList.DataKeyNames = new string[] { "id" };
                gBatchList.Actions.ShowAdd = true;
                gBatchList.Actions.AddClick += gBatchList_Add;
                gBatchList.GridRebind += gBatchList_GridRebind;
                gBatchList.GridReorder += gBatchList_GridReorder;
            }
            else
            {
                nbWarningMessage.Text = "You are not authorized to edit these batches";
                nbWarningMessage.Visible = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfBatchFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfBatchFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Status":
                    e.Value = e.Value.ConvertToEnum<BatchStatus>().ConvertToString();
                    break;

                case "Campus":
                    e.Value = CampusCache.Read( int.Parse( e.Value ) ).Name;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfBatchFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfBatchFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfBatchFilter.SaveUserPreference( "From Date", dpBatchDate.Text );
            gfBatchFilter.SaveUserPreference( "Title", tbTitle.Text );

            int? statusFilter = ddlStatus.SelectedValueAsInt( false );
            gfBatchFilter.SaveUserPreference( "Status", statusFilter.HasValue && statusFilter.Value >= 0 ? statusFilter.ToString() : string.Empty );

            int? campusFilter = campCampus.SelectedCampusId;
            gfBatchFilter.SaveUserPreference( "Campus", campusFilter.HasValue && campusFilter.Value > 0 ? campusFilter.ToString() : string.Empty );

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
            FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );
            FinancialBatch financialBatch = financialBatchService.Get( e.RowKeyId );
            if ( financialBatch != null )
            {
                string errorMessage;
                if ( !financialBatchService.CanDelete( financialBatch, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                financialBatchService.Delete( financialBatch );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_Edit( object sender, RowEventArgs e )
        {
            ShowDetailForm( (int)gBatchList.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Add event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gBatchList_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowDetailForm( 0 );
        }

        /// <summary>
        /// Handles the GridReorder event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gBatchList_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var batchService = new Rock.Model.FinancialBatchService( rockContext );
            var queryable = batchService.Queryable();

            List<Rock.Model.FinancialBatch> items = queryable.ToList();
            batchService.Reorder( items, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBatchList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            Rock.Model.FinancialBatch batch = e.Row.DataItem as Rock.Model.FinancialBatch;
            if ( batch != null )
            {
                var startDate = Convert.ToDateTime( e.Row.DataItem.GetPropertyValue( "BatchStartDateTime" ) ).ToShortDateString();
                e.Row.Cells[1].Text = startDate;

                Literal transactionTotal = e.Row.FindControl( "TransactionTotal" ) as Literal;
                if ( transactionTotal != null )
                {
                    var data = batch.Transactions.Where( d => d.TotalAmount > 0 );
                    var totalSum = data.Sum( d => d.TotalAmount );
                    transactionTotal.Text = string.Format( "{0:C}", totalSum );

                    Label variance = e.Row.FindControl( "lblVariance" ) as Label;
                    if ( variance != null )
                    {
                        if ( batch.ControlAmount > 0 )
                        {
                            var varianceAmount = Convert.ToDecimal( batch.ControlAmount ) - totalSum;
                            variance.Text = string.Format( "{0:C}", varianceAmount );
                            if ( varianceAmount != 0 )
                            {
                                variance.AddCssClass( "text-danger" );
                            }
                        }
                    }
                }

                Literal transactionCount = e.Row.FindControl( "TransactionCount" ) as Literal;
                if ( transactionCount != null )
                {
                    transactionCount.Text = batch.Transactions.Count.ToString();
                }

                var status = e.Row.DataItem.GetPropertyValue( "Status" ).ToString();
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    switch ( status.ToUpper() )
                    {
                        case "CLOSED":
                            e.Row.Cells[7].Text = "<span class='label label-success'>Closed</span>";
                            break;
                        case "OPEN":
                            e.Row.Cells[7].Text = "<span class='label label-warning'>Open</span>";
                            break;
                        case "PENDING":
                            e.Row.Cells[7].Text = "<span class='label label-default'>Pending</span>";
                            break;
                    }
                }

                // Get any warnings for this row and display them in the Warnings column
                Label warnings = e.Row.FindControl( "lblWarnings" ) as Label;
                var warningList = GetWarnings( batch );
                if ( warningList != null )
                {
                    foreach ( var warning in warningList )
                    {
                        switch ( warning.ToUpper() )
                        {
                            case "UNTIED":
                                warnings.Text += "<span class='label label-danger'>Untied Transactions</span>";
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the warnings.
        /// </summary>
        /// <returns></returns>
        private List<string> GetWarnings( FinancialBatch batch )
        {
            var warningList = new List<string>();
            if ( batch.Status == BatchStatus.Open )
            {
                var transactionService = new FinancialTransactionService( new RockContext() );
                var transactionList = transactionService.Queryable().Where( trans => trans.BatchId == batch.Id && trans.AuthorizedPersonId == null ).ToList();
                if ( transactionList.Count > 0 )
                {
                    warningList.Add( "UNTIED" );
                }
            }

            return warningList;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            string titleFilter = gfBatchFilter.GetUserPreference( "Title" );
            tbTitle.Text = !string.IsNullOrWhiteSpace( titleFilter ) ? titleFilter : string.Empty;

            ddlStatus.BindToEnum( typeof( BatchStatus ) );
            ddlStatus.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlStatus.SetValue( gfBatchFilter.GetUserPreference( "Status" ) );

            var campusi = new CampusService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            campCampus.Campuses = campusi;
            campCampus.Visible = campusi.Any();
            campCampus.SetValue( gfBatchFilter.GetUserPreference( "Campus" ) );
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
            listControl.SelectedValue = !string.IsNullOrWhiteSpace( gfBatchFilter.GetUserPreference( userPreferenceKey ) ) ?
                listControl.SelectedValue = gfBatchFilter.GetUserPreference( userPreferenceKey ) : null;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var batchService = new FinancialBatchService( new RockContext() );
            var batches = batchService.Queryable();

            if ( dpBatchDate.SelectedDate.HasValue )
            {
                batches = batches.Where( batch => batch.BatchStartDateTime >= dpBatchDate.SelectedDate );
            }

            string status = gfBatchFilter.GetUserPreference( "Status" );
            if ( !string.IsNullOrWhiteSpace( status ) )
            {
                var batchStatus = (BatchStatus)Enum.Parse( typeof( BatchStatus ), status );
                batches = batches.Where( batch => batch.Status == batchStatus );
            }

            if ( !string.IsNullOrEmpty( tbTitle.Text ) )
            {
                batches = batches.Where( batch => batch.Name == tbTitle.Text );
            }

            if ( campCampus.SelectedCampusId.HasValue )
            {
                batches = batches.Where( batch => batch.CampusId == campCampus.SelectedCampusId.Value );
            }

            SortProperty sortProperty = gBatchList.SortProperty;
            if ( sortProperty != null )
            {
                gBatchList.DataSource = batches.Sort( sortProperty ).ToList();
            }
            else
            {
                gBatchList.DataSource = batches.OrderBy( batch => batch.Name ).ToList();
            }

            gBatchList.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "financialBatchId", id );
        }

        #endregion
    }
}
