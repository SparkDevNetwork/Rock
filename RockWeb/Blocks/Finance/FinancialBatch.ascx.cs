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

                modalTransactions.SaveClick += btnSaveFinancialTransaction_Click;
                modalTransactions.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdTransValue.ClientID );
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
            BindDefinedTypeDropdown( ddlBatchType2, Rock.SystemGuid.DefinedType.FINANCIAL_BATCH_TYPE, "Batch Type" );
            
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
                ddlBatchType2.SetValue( batchValue.EntityId );
                tbControlAmount.Text = batchValue.ControlAmount.ToString();
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
                financialBatch.BatchTypeValueId = ddlBatchType2.SelectedValueAsInt().HasValue?(int)ddlBatchType2.SelectedValueAsInt(): -1;
                float fcontrolamt = 0f;
                float.TryParse( tbControlAmount.Text, out fcontrolamt);
                financialBatch.ControlAmount = fcontrolamt ;

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
            ddlBatchType2.SetValue( -1 );
            tbControlAmount.Text = string.Empty;
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
                    TransactionTotal.Text = totalSum.ToString("{0:C}");


                    Literal Variance = e.Row.FindControl( "Variance" ) as Literal;
                    if ( Variance != null )
                    {
                        if ( batch.ControlAmount > 0 )
                        {
                            var variance = Convert.ToDecimal(batch.ControlAmount) - totalSum;
                            Variance.Text = variance.ToString( "{0:C}" );
                        }
                    }

                }
                Literal TransactionCount = e.Row.FindControl( "TransactionCount" ) as Literal;
                if ( TransactionCount != null )
                {
                    TransactionCount.Text = batch.Transactions.Count.ToString();
                }

                Grid transactions = e.Row.FindControl( "transactionGrid" ) as Grid;
                if ( transactions != null )
                {
                    transactions.DataSource = batch.Transactions.ToList();
                    transactions.DataBind();
                    transactions.DataKeyNames = new string[] { "id" };
                    transactions.Actions.IsAddEnabled = true;
                    transactions.Actions.AddClick += gridFinancialTransaction_Add;
                }
               
            }
        }

        #region add, edit and delete blocks Financial Transaction

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

            BindGrid();
        }
        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rTransactionsGrid_EditValue( object sender, RowEventArgs e )
        {
            Grid transactionGrid = sender as Grid;
            ShowTransactionEditValue( (int)transactionGrid.DataKeys[e.RowIndex]["Id"], (int)transactionGrid.DataKeys[e.RowIndex]["BatchId"], true );

        }
        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="transactionId">The transactionId id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowTransactionEditValue( int transactionId, int batchid, bool setValues )
        {

            hfIdTransValue.Value = transactionId.ToString();
            hfBatchId.Value = batchid.ToString();

            var transactionValue = new Rock.Model.FinancialTransactionService().Get( transactionId );

            if ( transactionValue != null )
            {
                lValue.Text = "Edit";
                
                hfIdTransValue.Value = transactionValue.Id.ToString();
                tbAmount.Text = transactionValue.Amount.ToString();
                hfBatchId.Value = transactionValue.BatchId.ToString();
                ddlCreditCartType.SetValue( transactionValue.CreditCardTypeValueId);
                ddlCurrencyType.SetValue(  transactionValue.CurrencyTypeValueId);
                tbDescription.Text = transactionValue.Description;
                TranEntity.Text = transactionValue.EntityId.ToString();
                ddlEntityType.SetValue(  transactionValue.EntityTypeId);
                ddlPaymentGateway.SetValue(  transactionValue.PaymentGatewayId);
                tbRefundTransactionId.Text = transactionValue.RefundTransactionId.ToString();
                ddlSourceType.SetValue(  transactionValue.SourceTypeValueId);
                tbSummary.Text = transactionValue.Summary;
                tbTransactionCode.Text = transactionValue.TransactionCode;
                dtTransactionDateTime.Text = transactionValue.TransactionDateTime.ToString();
                tbTransactionImageId.Text = transactionValue.TransactionImageId.ToString();


            }
            else
            {
                lValue.Text = "Add";
                emptyInputs();
            }

            modalTransactions.Show();

        }

        protected void btnSaveFinancialTransaction_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialTransactionService = new Rock.Model.FinancialTransactionService();
                Rock.Model.FinancialTransaction financialTransaction = null;
                int financialTransactionId = ( hfIdTransValue.Value ) != null ? Int32.Parse( hfIdTransValue.Value ) : 0;
                int batchid = ( hfBatchId.Value ) != null ? Int32.Parse( hfBatchId.Value ) : 0;

                if ( financialTransactionId == 0 )
                {
                    financialTransaction = new Rock.Model.FinancialTransaction();
                    financialTransactionService.Add( financialTransaction, CurrentPersonId );
                    financialTransaction.BatchId = batchid;
                }
                else
                {
                    financialTransaction = financialTransactionService.Get( financialTransactionId );
                }

//financialTransaction.Id	=	hfIdTransValue.Value;
//financialTransaction.Amount	=	tbAmount.Text;
//financialTransaction.BatchId	=	hfBatchId.Value;
if ( ddlCreditCartType.SelectedValue != "-1" )
{
    financialTransaction.CreditCardTypeValueId = int.Parse( ddlCreditCartType.SelectedValue );
}
if ( ddlCurrencyType.SelectedValue != "-1" )
{
    financialTransaction.CurrencyTypeValueId = int.Parse( ddlCurrencyType.SelectedValue );
}
financialTransaction.Description	=	tbDescription.Text;
if ( TranEntity.SelectedValue != "-1" )
{
    financialTransaction.EntityId = int.Parse( TranEntity.SelectedValue );
}
if ( ddlEntityType.SelectedValue != "-1" )
{
    financialTransaction.EntityTypeId = int.Parse( ddlEntityType.SelectedValue );
}
if ( ddlPaymentGateway.SelectedValue != "-1" )
{
    financialTransaction.PaymentGatewayId = int.Parse( ddlPaymentGateway.SelectedValue );
}
//financialTransaction.RefundTransactionId	=	tbRefundTransactionId.Text
//financialTransaction.SourceTypeValueId	=	ddlSourceType
//financialTransaction.Summary	=	tbSummary.Text;
//financialTransaction.TransactionCode	=	tbTransactionCode.Text;
//financialTransaction.TransactionDateTime	=	dtTransactionDateTime
//financialTransaction.TransactionImageId	=	tbTransactionImageId.Text;


                financialTransactionService.Save( financialTransaction, CurrentPersonId );
            }

            BindFilter();
            BindGrid();

        }

        protected void emptyTransactionInputs()
        {
            //qtbName.Text = string.Empty;
            //qdtTransactionDateStart.SelectedDate = null;
            //qdtTransactionDateEnd.SelectedDate = null;
            //qddlCampus.SetValue( -1 );
            //qddlEntity.SetValue( -1 );
            //qcbIsClosed.Checked = false;

//            hfIdTransValue
//tbAmount.Text = string.Empty;
//hfBatchId
//ddlCreditCartType.SetValue( -1 );
//ddlCurrencyType.SetValue( -1 );
//tbDescription.Text = string.Empty;
//TranEntity
//ddlEntityType.SetValue( -1 );
//ddlPaymentGateway.SetValue( -1 );
//tbRefundTransactionId
//ddlSourceType.SetValue( -1 );
//tbSummary.Text = string.Empty;
//tbTransactionCode.Text = string.Empty;
//dtTransactionDateTime
//tbTransactionImageId.Text = string.Empty;
//TranCampus


        }

        protected void btnCancelFinancialTransaction_Click( object sender, EventArgs e )
        {
            emptyTransactionInputs();
            modalValue.Hide();
        }

        protected void gridFinancialTransaction_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowTransactionEditValue( 0,0, false );
        }
        #endregion

       

}
}