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
    public partial class FinancialBatch : Rock.Web.UI.RockBlock
    {
        #region workingnotes
        /*
         * filters
         * 
         * From Date
    Through Date
    IsClosed
    Title
    Batch Type
         * 
         grid columns
                    ID
    Title 
    Date Range (Start Date – End Date) (sortable)
    Is Closed (sortable)
    Control Amount
    Batch Total
    Variance (Control Amt – Batch Total) if not zero would be nice if the table cell (td) was marked with class="warning" so we could style it red.
    Batch Count
    Batch Type
    Funds listed w/ Batch totals
    Edit Button
    Delete Button (should warn though)
         * 
         * 
         */
        #endregion

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
                grdFinancialBatch.Actions.IsAddEnabled = true;

                grdFinancialBatch.Actions.AddClick += gridFinancialBatch_Add;
                grdFinancialBatch.GridRebind += grdFinancialBatch_GridRebind;
                grdFinancialBatch.GridReorder += grdFinancialBatch_GridReorder;

                modalValue.SaveClick += btnSaveFinancialBatch_Click;
                modalValue.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
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
            cbIsClosed.Checked = rFBFilter.GetUserPreference( "Is Closed" ) == "checked" ? true : false;

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
     
        #region edit and delete blocks

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

            ShowEditValue( (int)grdFinancialBatch.DataKeys[e.RowIndex]["id"], true );

        }
        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int batchId, bool setValues )
        {

            hfIdValue.Value = batchId.ToString();           
           
            var batchValue = new Rock.Model.FinancialBatchService().Get( batchId );

            if ( batchValue != null )
            {
                lValue.Text = "Edit";
                tbName.Text = batchValue.Name;
                dtBatchDateStart.SelectedDate = batchValue.BatchDateStart;
                dtBatchDateEnd.SelectedDate = batchValue.BatchDateEnd;
                ddlCampus.SetValue( batchValue.CampusId);
                ddlEntity.SetValue(batchValue.EntityId);
                cbIsClosed.Checked = batchValue.IsClosed;
            }
            else
            {
                lValue.Text = "Add";
                emptyInputs();
            }

            modalValue.Show();
            
        }

        protected void btnSaveFinancialBatch_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialBatchService = new Rock.Model.FinancialBatchService();
                Rock.Model.FinancialBatch financialBatch = null;
                int financialBatchId = ( hfIdValue.Value ) != null ? Int32.Parse( hfIdValue.Value ) : 0;

                if ( financialBatchId == 0 )
                {
                    financialBatch = new Rock.Model.FinancialBatch();
                    financialBatchService.Add( financialBatch, CurrentPersonId );
                }
                else
                {
                    financialBatch = financialBatchService.Get( financialBatchId );
                }

                financialBatch.Name = tbName.Text;
                financialBatch.BatchDateStart = dtBatchDateStart.SelectedDate;
                financialBatch.BatchDateEnd = dtBatchDateEnd.SelectedDate;
                financialBatch.CampusId = ddlCampus.SelectedValueAsInt();
                financialBatch.EntityId = ddlEntity.SelectedValueAsInt();
                financialBatch.IsClosed = cbIsClosed.Checked;

                financialBatchService.Save( financialBatch, CurrentPersonId );
            }

            BindFilter();
            BindGrid();

        }

        protected void emptyInputs()
        {
            tbName.Text = string.Empty;
            dtBatchDateStart.SelectedDate = null;
            dtBatchDateEnd.SelectedDate = null;
            ddlCampus.SetValue( -1 );
            ddlEntity.SetValue( -1 );
            cbIsClosed.Checked = false;
        }

        protected void btnCancelFinancialBatch_Click( object sender, EventArgs e )
        {
            emptyInputs();
            modalValue.Hide();
        }

        protected void gridFinancialBatch_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowEditValue( 0, false );
        }
        #endregion

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

        }
    }
}