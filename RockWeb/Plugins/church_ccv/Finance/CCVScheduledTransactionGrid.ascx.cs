using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Finance
{
    #region Block Attributes

    /// <summary>
    /// Grid of Scheduled Transactions for current user 
    /// with option to delete
    /// </summary>
    [DisplayName( "CCV Grid of Scheduled Transactions" )]
    [Category( "CCV > Finance" )]
    [Description( "Grid of a person's scheduled transactions." )]
    [TextField( "Transaction Label", "The label to use to describe the transaction (e.g. 'Gift', 'Donation', etc.)", true, "Gift", "", 5 )]

    #endregion
    public partial class CCVScheduledTransactionGrid : RockBlock
    { 

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // setup the grid
            gScheduledTransactions.DataKeyNames = new[] { "id" };
            gScheduledTransactions.ShowActionRow = false;
            gScheduledTransactions.GridRebind += gScheduledTransactions_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );
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
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gScheduledTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledTransactions_Delete( object sender, RowEventArgs e )
        {
            // Get the row that triggered the event
            GridViewRow row = gScheduledTransactions.Rows[e.RowIndex];

            // Get schedule info
            string gatewayScheduleId = row.Cells[0].Text;
            string amount = row.Cells[1].Text;
            string paymentAccount = row.Cells[3].Text;

            // if schedule id and amount exist, try to delete the schedule
            if ( gatewayScheduleId.IsNotNullOrWhitespace() || amount.IsNotNullOrWhitespace())
            {
                using ( var rockContext = new RockContext() )
                {
                    FinancialScheduledTransactionService transactionService = new FinancialScheduledTransactionService( rockContext );

                    // get the transaction and load its attributes
                    var selectedTransaction = transactionService.GetByScheduleId( gatewayScheduleId );

                    if (selectedTransaction != null && selectedTransaction.FinancialGateway != null)
                    {
                        selectedTransaction.FinancialGateway.LoadAttributes( rockContext );
                    }

                    // try to cancel the scheduled transaction
                    string errorMessage = string.Empty;
                    if (transactionService.Cancel ( selectedTransaction, out errorMessage))
                    {
                        try
                        {
                            transactionService.GetStatus( selectedTransaction, out errorMessage );
                        }
                        catch { }

                        // success - save changes and update message
                        rockContext.SaveChanges();
                        nbMessage.Text = String.Format( "<div class='alert alert-success'>Your recurring {0} of {1} for payment account {2} has been deleted.</div>", GetAttributeValue( "TransactionLabel" ).ToLower(), amount, paymentAccount );
                        nbMessage.Visible = true;

                        // Rebind grid
                        BindGrid();
                    }
                    else
                    {
                        // cancel failed - update message
                        nbMessage.Text = String.Format( "<div class='alert alert-danger'>An error occured while deleting your scheduled transation. Message: {0}</div>", errorMessage );
                        nbMessage.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();

                //  get all scheduled transactions for current person
                gScheduledTransactions.DataSource = new FinancialScheduledTransactionService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.AuthorizedPersonAlias.Person.GivingId == CurrentPerson.GivingId &&
                        a.IsActive == true )
                    .OrderBy( a => a.NextPaymentDate )
                    .ToList()
                    .Select( a => new
                    {
                        a.Id,
                        a.TotalAmount,
                        a.FinancialPaymentDetail.AccountNumberMasked,
                        a.TransactionFrequencyValue,
                        a.NextPaymentDate,
                        a.FinancialPaymentDetail.CurrencyTypeValue,
                        a.StartDate,
                        a.GatewayScheduleId
                    } )
                    .ToList();

                // Bind to grid
                gScheduledTransactions.DataBind();

                // Hide the GatewayScheduleId - We dont want this showing client side
                // This is used by the deletion process
                gScheduledTransactions.Columns[0].Visible = false;
            }
        }
    }
}