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
    public partial class FinancialBatchDetail : Rock.Web.UI.RockBlock, IDetailBlock
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
            
            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                transactionGrid.DataKeyNames = new string[] { "id" };
                transactionGrid.Actions.IsAddEnabled = true;

                transactionGrid.Actions.AddClick += gridFinancialTransaction_Add;
                transactionGrid.GridRebind += transactionGrid_GridRebind;
                transactionGrid.GridReorder += transactionGrid_GridReorder;

            }
            else
            {
                DisplayError( "You are not authorized to configure this page" );
            }
        }
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindDefinedTypeDropdown( ddlBatchType2, Rock.SystemGuid.DefinedType.FINANCIAL_BATCH_TYPE );  
                BindGrid();
            }
        }

        #endregion

        void transactionGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var batchService = new Rock.Model.FinancialTransactionService();
            var queryable = batchService.Queryable();

            List<Rock.Model.FinancialTransaction> items = queryable.ToList();
            batchService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );
            BindGrid();
        }

        void transactionGrid_GridRebind( object sender, EventArgs e )
        {
            BindDefinedTypeDropdown( ddlBatchType2, Rock.SystemGuid.DefinedType.FINANCIAL_BATCH_TYPE );            
            BindGrid();
        }

        private void BindGrid()
        {
            int itemId = 0;
            var parm = PageParameter( "financialBatchId" );
            if ( parm == null )
                return;

            int.TryParse( parm.ToString(), out itemId);
            if ( itemId == 0 )
                return;
            var batchService = new FinancialBatchService();
            var batch = batchService.Get( itemId );

            ShowEditValue( batch );

        }

        #region load forms

        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );

        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( Rock.Model.FinancialBatch batchValue )
            //int batchId )
        {
            
            hfIdValue.Value = batchValue.Id.ToString();
            
            lValue.Text = "Edit";
            tbName.Text = batchValue.Name;
            dtBatchDateStart.SelectedDate = batchValue.BatchDateStart;
            dtBatchDateEnd.SelectedDate = batchValue.BatchDateEnd;
            if (batchValue.CampusId.HasValue)
                CampusPicker1.SelectedCampusIds.Add( (int)batchValue.CampusId );
            //ddlEntity.SetValue( batchValue.EntityId );
            cbIsClosed.Checked = batchValue.IsClosed;
            ddlBatchType2.SetValue( batchValue.EntityId );
            tbControlAmount.Text = batchValue.ControlAmount.ToString();

            setTransactionDataSource( batchValue.Transactions.ToList() );


        }

        protected void btnSaveFinancialBatch_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialBatchService = new Rock.Model.FinancialBatchService();
                Rock.Model.FinancialBatch financialBatch = null;

                int financialBatchId = 0;
                if (! string.IsNullOrEmpty(hfIdValue.Value))
                    financialBatchId = Int32.Parse( hfIdValue.Value ) ;

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
                if (CampusPicker1.SelectedCampusIds.Count > 0)
                    financialBatch.CampusId =  CampusPicker1.SelectedCampusIds[0];

            //    financialBatch.EntityId = ddlEntity.SelectedValueAsInt();
                financialBatch.IsClosed = cbIsClosed.Checked;
                financialBatch.BatchTypeValueId = ddlBatchType2.SelectedValueAsInt().HasValue ? (int)ddlBatchType2.SelectedValueAsInt() : -1;
                float fcontrolamt = 0f;
                float.TryParse( tbControlAmount.Text, out fcontrolamt );
                financialBatch.ControlAmount = fcontrolamt;

                financialBatchService.Save( financialBatch, CurrentPersonId );
            }
            
            BindGrid();

        }

        protected void emptyInputs()
        {
            hfIdValue.Value = string.Empty;
            tbName.Text = string.Empty;
            dtBatchDateStart.SelectedDate = null;
            dtBatchDateEnd.SelectedDate = null;
            CampusPicker1.SelectedCampusIds.Clear();
            //ddlEntity.SetValue( -1 );
            cbIsClosed.Checked = false;
            ddlBatchType2.SetValue( -1 );
            tbControlAmount.Text = string.Empty;
        }

        protected void btnCancelFinancialBatch_Click( object sender, EventArgs e )
        {
            emptyInputs();
            NavigateToParentPage();
        }

        #endregion

        #region transactions grid

        public void setTransactionDataSource( List<FinancialTransaction> lst )
        {
            transactionGrid.DataSource = lst;
            transactionGrid.DataBind();
            transactionGrid.DataKeyNames = new string[] { "id" };

        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rTransactionsGrid_EditValue( object sender, RowEventArgs e )
        {

            Grid transactionGrid = sender as Grid;
            ShowTransactionEditValue( (int)transactionGrid.DataKeys[e.RowIndex]["Id"] );

        }
        protected void ShowTransactionEditValue( int id )
        {
            if ( id == 0 )
            {
                NavigateToDetailPage( "batchId", Int32.Parse( hfIdValue.Value ) );
            }
            else
                NavigateToDetailPage( "transactionId", id );
        }

        protected void gridFinancialTransaction_Add( object sender, EventArgs e )
        {
            ShowTransactionEditValue( 0 );
        }

        protected void grdFinancialTransactions_Delete( object sender, RowEventArgs e )
        {
            var financialTransactionService = new Rock.Model.FinancialTransactionService();
            Grid grdTransactions = sender as Grid;
            Rock.Model.FinancialTransaction financialTransaction = financialTransactionService.Get( (int)grdTransactions.DataKeys[e.RowIndex]["id"] );
            if ( financialTransaction != null )
            {
                financialTransactionService.Delete( financialTransaction, CurrentPersonId );
                financialTransactionService.Save( financialTransaction, CurrentPersonId );
            }

        }

        #endregion

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
           if ( !itemKey.Equals( "financialBatchId" ) )
            {
                return;
            }
        }
    }
}