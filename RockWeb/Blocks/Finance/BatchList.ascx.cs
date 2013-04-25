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

namespace RockWeb.Blocks.Finance.Administration
{
    [DetailPage]
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

            rFBFilter.ApplyFilterClick += rFBFilter_ApplyFilterClick;
            rFBFilter.DisplayFilterValue += rFBFilter_DisplayFilterValue;

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                rGridBatch.DataKeyNames = new string[] { "id" };
                rGridBatch.Actions.ShowAdd = true;

                rGridBatch.Actions.AddClick += rGridBatch_Add;
                rGridBatch.GridRebind += rGridBatch_GridRebind;
                rGridBatch.GridReorder += rGridBatch_GridReorder;

                var campusService = new CampusService();
                ddlCampus.DataSource = campusService.Queryable()
                    .OrderBy( a => a.Name )
                    .ToDictionary( a => a.Id, a => a.Name );                
                ddlCampus.DataValueField = "Key";
                ddlCampus.DataTextField = "Value";
                ddlCampus.DataBind();
                ddlCampus.Items.Insert( 0, Rock.Constants.All.Text );
                ddlStatus.BindToEnum( typeof( BatchStatus ) );
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
                case "Date":
                    DateTime batchDate = DateTime.Now;
                    e.Value = batchDate.ToString();
                    break;

                case "Status":
                case "Title":
                    break;

                case "Campus":
                    e.Value = ddlCampus.SelectedItem.Text;
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
            rFBFilter.SaveUserPreference( "From Date", dtBatchDate.Text );
            rFBFilter.SaveUserPreference( "Title", txtTitle.Text );
            rFBFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            rFBFilter.SaveUserPreference( "Campus", ddlCampus.SelectedValue );
            
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the grdFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridBatch_Delete( object sender, RowEventArgs e )
        {
            var FinancialBatchService = new Rock.Model.FinancialBatchService();

            Rock.Model.FinancialBatch FinancialBatch = FinancialBatchService.Get( (int)rGridBatch.DataKeys[e.RowIndex]["id"] );
            if ( FinancialBatch != null )
            {
                FinancialBatchService.Delete( FinancialBatch, CurrentPersonId );
                FinancialBatchService.Save( FinancialBatch, CurrentPersonId );
            }

            BindGrid();
        }
        
        /// <summary>
        /// Handles the RowSelected event of the rGridBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridBatch_Edit( object sender, RowEventArgs e )
        {
            ShowDetailForm( (int)rGridBatch.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Add event of the gridFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridBatch_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowDetailForm( 0 );
        }
                
        #endregion

        #region Internal Methods
        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            DateTime fromDate;
            if ( !DateTime.TryParse( rFBFilter.GetUserPreference( "From Date" ), out fromDate ) )
            {
                fromDate = DateTime.Today;
            }
            dtBatchDate.Text = fromDate.ToShortDateString();
            txtTitle.Text = !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( "Title" ) ) ?
                rFBFilter.GetUserPreference( "Title" ) : null;
            ddlStatus.SelectedValue = !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( "Status" ) ) ?
                rFBFilter.GetUserPreference( "Status" ) : null;
            ddlCampus.SelectedValue = !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( "Campus" ) ) ?
                rFBFilter.GetUserPreference( "Campus" ) : null;
        }

        /// <summary>
        /// Binds the defined type dropdown.
        /// </summary>
        /// <param name="ListControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            //ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

            ListControl.SelectedValue = !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( userPreferenceKey ) ) ?
                ListControl.SelectedValue = rFBFilter.GetUserPreference( userPreferenceKey ) : null;
        }

        /// <summary>
        /// Handles the GridReorder event of the grdFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void rGridBatch_GridReorder( object sender, GridReorderEventArgs e )
        {
            var batchService = new Rock.Model.FinancialBatchService();
            var queryable = batchService.Queryable();

            List<Rock.Model.FinancialBatch> items = queryable.ToList();
            batchService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the grdFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rGridBatch_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var batchService = new FinancialBatchService();
            var batches = batchService.Queryable();
            SortProperty sortProperty = rGridBatch.SortProperty;

            if ( dtBatchDate.SelectedDate.HasValue )
            {
                batches = batches.Where( batch => batch.BatchDate >= dtBatchDate.SelectedDate );
            }

            if ( ddlStatus.SelectedIndex > -1 )
            {
                var batchStatus = ddlStatus.SelectedValueAsEnum<BatchStatus>();
                batches = batches.Where( Batch => Batch.Status == batchStatus );
            }

            if ( !string.IsNullOrEmpty( txtTitle.Text ) )
            {
                batches = batches.Where( Batch => Batch.Name == txtTitle.Text );
            }

            if ( ddlCampus.SelectedIndex > -1 && ddlCampus.SelectedValue != Rock.Constants.All.Text )
            {
                int campusId = Convert.ToInt32( ddlCampus.SelectedValue );
                batches = batches.Where( Batch => Batch.CampusId == campusId );
            }
            
            if ( sortProperty != null )
            {
                rGridBatch.DataSource = batches.Sort( sortProperty ).ToList();
            }
            else
            {
                rGridBatch.DataSource = batches.OrderBy( b => b.Name ).ToList();
            }

            rGridBatch.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToDetailPage( "financialBatchId", id );
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            valSummaryTop.Controls.Clear();
            valSummaryTop.Controls.Add( new LiteralControl( message ) );
            valSummaryTop.Visible = true;
        }

        /// <summary>
        /// Handles the RowDataBound event of the grdFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void rGridBatch_RowDataBound( object sender, GridViewRowEventArgs e )
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

        #endregion
        
    }        
}
