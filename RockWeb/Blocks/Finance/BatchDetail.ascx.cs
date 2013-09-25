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

    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Detail Page")]
    public partial class BatchDetail : Rock.Web.UI.RockBlock, IDetailBlock
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
            
            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                rGridTransactions.DataKeyNames = new string[] { "id" };
                rGridTransactions.Actions.ShowAdd = true;

                rGridTransactions.Actions.AddClick += rGridTransactions_Add;
                rGridTransactions.GridRebind += rGridTransactions_GridRebind;
                rGridTransactions.GridReorder += rGridTransactions_GridReorder;

                // enable delete transaction
                rGridTransactions.Columns[rGridTransactions.Columns.Count - 1].Visible = true;

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
                LoadDropDowns();
                BindGrid();
            }
        }

        /// <summary>
        /// Sets the transaction data source.
        /// </summary>
        /// <param name="lst">The LST.</param>
        public void setTransactionDataSource( List<FinancialTransaction> list )
        {
            rGridTransactions.DataSource = list;
            rGridTransactions.DataBind();
            rGridTransactions.DataKeyNames = new string[] { "id" };
        }      
        
        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {           
            ddlStatus.BindToEnum( typeof( BatchStatus ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var itemId = PageParameter( "financialBatchId" );
            if ( !string.IsNullOrWhiteSpace( itemId ) )
            {
                int batchId;
                Int32.TryParse( itemId, out batchId );
                if ( batchId > 0 )
                {
                    ShowEditValue( batchId );
                }                
            }
        }
        
        /// <summary>
        /// Binds the defined type dropdown.
        /// </summary>
        /// <param name="ListControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int batchId )
        {
            var batchService = new FinancialBatchService();
            var batch = batchService.Get( batchId );
            hfIdValue.Value = batch.Id.ToString();
            
            lValue.Text = "Edit";
            tbName.Text = batch.Name;
            dtBatchDate.LowerValue = batch.BatchStartDateTime;
            dtBatchDate.UpperValue = batch.BatchEndDateTime;
            if ( batch.CampusId.HasValue )
            {
                cpCampus.SelectedCampusId = batch.CampusId;  
            }
            ddlStatus.BindToEnum( typeof(BatchStatus) );
            tbControlAmount.Text = batch.ControlAmount.ToString();
            setTransactionDataSource( batch.Transactions.ToList() );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                    financialBatch.CreatedByPersonId = CurrentPersonId.Value;
                    financialBatchService.Add( financialBatch, CurrentPersonId );
                }
                else
                {
                    financialBatch = financialBatchService.Get( financialBatchId );
                }

                financialBatch.Name = tbName.Text;
                financialBatch.BatchStartDateTime = dtBatchDate.LowerValue;
                financialBatch.BatchEndDateTime = dtBatchDate.UpperValue;
                financialBatch.CampusId = cpCampus.SelectedCampusId;                
                financialBatch.Status = (BatchStatus) ddlStatus.SelectedIndex;
                decimal fcontrolamt = 0;
                decimal.TryParse( tbControlAmount.Text, out fcontrolamt );
                financialBatch.ControlAmount = fcontrolamt;

                financialBatchService.Save( financialBatch, CurrentPersonId );
            }
            
            BindGrid();
            NavigateToParentPage();

        }

        /// <summary>
        /// Handles the Click event of the btnCancelFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelFinancialBatch_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Add event of the gridFinancialTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Add( object sender, EventArgs e )
        {
            ShowTransactionEditValue( 0 );
        }

        /// <summary>
        /// Handles the Delete event of the grdFinancialTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Delete( object sender, RowEventArgs e )
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

        /// <summary>
        /// Handles the RowSelected event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridTransactions_Edit( object sender, RowEventArgs e )
        {
            ShowTransactionEditValue( (int)e.RowKeyValue );
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Handles the GridReorder event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void rGridTransactions_GridReorder( object sender, GridReorderEventArgs e )
        {
            var batchService = new Rock.Model.FinancialTransactionService();
            var queryable = batchService.Queryable();

            List<Rock.Model.FinancialTransaction> items = queryable.ToList();
            batchService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rGridTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }
        
        /// <summary>
        /// Shows the transaction edit value.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowTransactionEditValue( int id )
        {
            if ( id == 0 )
            {
                NavigateToLinkedPage( "DetailPage", "financialBatchId", Int32.Parse( hfIdValue.Value ) );
            }
            else
            {
                NavigateToLinkedPage( "DetailPage", "transactionId", id );
            }
        }
                
        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "financialBatchId" ) )
            {
                return;
            }
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

        #endregion
    }       
}
