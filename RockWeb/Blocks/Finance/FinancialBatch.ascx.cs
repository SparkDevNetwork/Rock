//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Collections.Generic;

namespace RockWeb.Blocks.Administration
{
    [DetailPage]
    public partial class FinancialBatch : Rock.Web.UI.RockBlock
    {

        private bool _canConfigure = false;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFBFilter.ApplyFilterClick += rFBFilter_ApplyFilterClick;
            rFBFilter.DisplayFilterValue += rFBFilter_DisplayFilterValue;

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                grdFinancialBatch.DataKeyNames = new string[] { "id" };
                grdFinancialBatch.Actions.ShowAdd = true;

                grdFinancialBatch.Actions.AddClick += gridFinancialBatch_Add;
                grdFinancialBatch.GridRebind += grdFinancialBatch_GridRebind;
                grdFinancialBatch.GridReorder += grdFinancialBatch_GridReorder;

            }
            else
            {
                DisplayError( "You are not authorized to configure this page" );
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
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFBFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "From Date":
                case "Through Date":

                    DateTime fromdate = DateTime.Now;
                    e.Value = fromdate.ToString();
                    

                    break;

                case "IsClosed":
                case "Title":
                    break;

                case "Batch Type":

                    int definedValueId = 0;
                    if ( int.TryParse( e.Value, out definedValueId ) )
                    {
                        var definedValue = DefinedValueCache.Read( definedValueId );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Name;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFBFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFBFilter_ApplyFilterClick( object sender, EventArgs e )
        {

            rFBFilter.SaveUserPreference( "From Date", dtFromDate.Text );
            rFBFilter.SaveUserPreference( "Through Date", dtThroughDate.Text );
            rFBFilter.SaveUserPreference( "Title", txtTitle.Text );
            rFBFilter.SaveUserPreference( "Is Closed", cbIsClosedFilter.Checked.ToString() );
            rFBFilter.SaveUserPreference( "Batch Type", ddlBatchType.SelectedValue != All.Id.ToString() ? ddlBatchType.SelectedValue : string.Empty );

            BindGrid();
        }

        #endregion        

        private void BindFilter()
        {
            DateTime fromDate;
            if ( !DateTime.TryParse( rFBFilter.GetUserPreference( "From Date" ), out fromDate ) )
            {
                fromDate = DateTime.Today;
            }
            dtFromDate.Text = fromDate.ToShortDateString();
            dtThroughDate.Text = rFBFilter.GetUserPreference( "Through Date" );
            txtTitle.Text = rFBFilter.GetUserPreference( "Title" );
            cbIsClosedFilter.Checked = rFBFilter.GetUserPreference( "Is Closed" ) == "checked" ? true : false;

            BindDefinedTypeDropdown( ddlBatchType, Rock.SystemGuid.DefinedType.FINANCIAL_BATCH_TYPE, "Batch Type" );           

        }

        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

            if ( !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( userPreferenceKey ) ) )
            {
                ListControl.SelectedValue = rFBFilter.GetUserPreference( userPreferenceKey );
            }
        }

        void grdFinancialBatch_GridReorder( object sender, GridReorderEventArgs e )
        {
            var batchService = new Rock.Model.FinancialBatchService();
            var queryable = batchService.Queryable();

            List<Rock.Model.FinancialBatch> items = queryable.ToList();
            batchService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );
            BindGrid();
        }
        
        void grdFinancialBatch_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        private void BindGrid()
        {
            BatchSearchValue searchValue = GetSearchValue();

            var batchService = new FinancialBatchService();
            grdFinancialBatch.DataSource = batchService.Get( searchValue ).ToList();
            grdFinancialBatch.DataBind();
        }

        private BatchSearchValue GetSearchValue()
        {
            BatchSearchValue searchValue = new BatchSearchValue();

            DateTime? fromBatchDate = dtFromDate.SelectedDate;
            DateTime? toBatchDate = dtThroughDate.SelectedDate;
            searchValue.DateRange = new RangeValue<DateTime?>( fromBatchDate, toBatchDate );
            if ( ddlBatchType.SelectedValue != "-1" )
            {
                searchValue.BatchTypeValueId = int.Parse( ddlBatchType.SelectedValue );
            }
            searchValue.Title = txtTitle.Text;
            searchValue.IsClosed = cbIsClosedFilter.Checked ? true : false;
            return searchValue;
        }
     
        #region edit and delete blocks Financial Batch

        protected void grdFinancialBatch_Delete( object sender, RowEventArgs e )
        {
            var FinancialBatchService = new Rock.Model.FinancialBatchService();

            Rock.Model.FinancialBatch FinancialBatch = FinancialBatchService.Get( (int)grdFinancialBatch.DataKeys[e.RowIndex]["id"] );
            if ( FinancialBatch != null )
            {
                FinancialBatchService.Delete( FinancialBatch, CurrentPersonId );
                FinancialBatchService.Save( FinancialBatch, CurrentPersonId );
            }

            BindGrid();
        }
        
        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_EditValue( object sender, RowEventArgs e )
        {
            ShowDetailForm( (int)grdFinancialBatch.DataKeys[e.RowIndex]["id"] );
        }

        protected void gridFinancialBatch_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowDetailForm( 0 );
        }

        protected void ShowDetailForm( int id )
        {
            NavigateToDetailPage( "financialBatchId", id );
        }
        #endregion

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }
           
        protected void grdFinancialBatch_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            Rock.Model.FinancialBatch batch = e.Row.DataItem as Rock.Model.FinancialBatch;
            if ( batch != null )
            {
                Literal lDateRange = e.Row.FindControl( "lDateRange" ) as Literal;
                Literal TransactionTotal = e.Row.FindControl( "TransactionTotal" ) as Literal;
                if ( TransactionTotal != null )
                {
                    var data = batch.Transactions.Where(d => d.Amount > 0);
                    var totalSum = data.Sum(d => d.Amount);
                    TransactionTotal.Text = String.Format("{0:C}", totalSum); 

                    Literal Variance = e.Row.FindControl( "Variance" ) as Literal;
                    if ( Variance != null )
                    {
                        if ( batch.ControlAmount > 0 )
                        {
                            var variance = Convert.ToDecimal(batch.ControlAmount) - totalSum;
                            Variance.Text = String.Format( "{0:C}", variance ); 
                        }
                    }

                }
                Literal TransactionCount = e.Row.FindControl( "TransactionCount" ) as Literal;
                if ( TransactionCount != null )
                {
                    TransactionCount.Text = batch.Transactions.Count.ToString();
                }

               
            }
        }


}
}