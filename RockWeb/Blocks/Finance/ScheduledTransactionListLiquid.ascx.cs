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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using DotLiquid;
using Rock.Security;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Scheduled Transaction List Liquid" )]
    [Category( "Finance" )]
    [Description( "Block that shows a list of scheduled transactions for the currently logged in user with the ability to modify the formatting using liquid." )]
    [CodeEditorField( "Template", "Liquid template for the display of the scheduled transactions.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/ScheduledTransactionListLiquid.lava'  %}", "", 1 )]
    [BooleanField("Enable Debug", "Displays a list of available merge fields using the current person's scheduled transactions.", false, "", 2)]
    [LinkedPage("Scheduled Transaction Edit Page", "Link to be used for managing an individual's scheduled transactions.", false, "", "", 3)]
    [LinkedPage( "Scheduled Transaction Entry Page", "Link to use when adding new transactions.", false, "", "", 4 )]
    [TextField( "Transaction Label", "The label to use to describe the transaction (e.g. 'Gift', 'Donation', etc.)", true, "Gift", "", 5 )]
    public partial class ScheduledTransactionListLiquid : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            lbAddScheduledTransaction.Text = string.Format( "Create New {0}", GetAttributeValue("TransactionLabel") );

            // set initial debug info
            if ( !IsPostBack )
            {
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Text = "<pre>At least one scheduled transaction needs to exist for the current user in order to display debug information.</pre>";
                }

                ShowContent();
            }

        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowContent();
        }

        protected void rptScheduledTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var transactionSchedule = e.Item.DataItem as FinancialScheduledTransaction;

                HiddenField hfScheduledTransactionId = (HiddenField)e.Item.FindControl( "hfScheduledTransactionId" );
                hfScheduledTransactionId.Value = transactionSchedule.Id.ToString();

                // create dictionary for liquid
                Dictionary<string, object> scheduleSummary = new Dictionary<string, object>();
                scheduleSummary.Add( "Id", transactionSchedule.Id );
                scheduleSummary.Add( "Guid", transactionSchedule.Guid );
                scheduleSummary.Add( "StartDate", transactionSchedule.StartDate );
                scheduleSummary.Add( "EndDate", transactionSchedule.EndDate );
                scheduleSummary.Add( "NextPaymentDate", transactionSchedule.NextPaymentDate );

                if ( transactionSchedule.NextPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysTillNextPayment", (transactionSchedule.NextPaymentDate.Value - DateTime.Now).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysTillNextPayment", null );
                }

                DateTime? lastPaymentDate = transactionSchedule.Transactions.Max( t => t.TransactionDateTime );
                scheduleSummary.Add( "LastPaymentDate", lastPaymentDate );

                if ( lastPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", (DateTime.Now - lastPaymentDate.Value).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", null );
                }

                scheduleSummary.Add( "CurrencyType", transactionSchedule.CurrencyTypeValue.Value );
                scheduleSummary.Add( "CreditCardType", (transactionSchedule.CreditCardTypeValue == null) ? "" : transactionSchedule.CreditCardTypeValue.Value );
                scheduleSummary.Add( "UrlEncryptedKey", transactionSchedule.UrlEncodedKey );
                scheduleSummary.Add( "Frequency", transactionSchedule.TransactionFrequencyValue.Value );
                scheduleSummary.Add( "FrequencyDescription", transactionSchedule.TransactionFrequencyValue.Description );

                List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string, object>>();
                decimal totalAmount = 0;

                foreach ( FinancialScheduledTransactionDetail detail in transactionSchedule.ScheduledTransactionDetails )
                {
                    Dictionary<string, object> detailSummary = new Dictionary<string, object>();
                    detailSummary.Add( "AccountId", detail.Id );
                    detailSummary.Add( "AccountName", detail.Account.Name );
                    detailSummary.Add( "Amount", detail.Amount );
                    detailSummary.Add( "Summary", detail.Summary );

                    summaryDetails.Add( detailSummary );

                    totalAmount += detail.Amount;
                }

                scheduleSummary.Add( "ScheduledAmount", totalAmount );
                scheduleSummary.Add( "TransactionDetails", summaryDetails );

                Dictionary<string, object> schedule = new Dictionary<string, object>();
                schedule.Add( "ScheduledTransaction", scheduleSummary );

                // merge into content
                Literal lLiquidContent = (Literal)e.Item.FindControl( "lLiquidContent" );
                lLiquidContent.Text = GetAttributeValue( "Template" ).ResolveMergeFields( schedule );

                // set debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Text = schedule.lavaDebugInfo();
                }
            }
        }

        protected void bbtnDelete_Click( object sender, EventArgs e )
        {
            BootstrapButton bbtnDelete = (BootstrapButton)sender;
            RepeaterItem riItem = (RepeaterItem)bbtnDelete.NamingContainer;

            HiddenField hfScheduledTransactionId = (HiddenField)riItem.FindControl( "hfScheduledTransactionId" );
            Literal content = (Literal)riItem.FindControl( "lLiquidContent" );
            Button btnEdit = (Button)riItem.FindControl( "btnEdit" );

            var rockContext = new Rock.Data.RockContext();
            FinancialScheduledTransactionService fstService = new FinancialScheduledTransactionService( rockContext );
            var currentTransaction = fstService.Get( Int32.Parse(hfScheduledTransactionId.Value) );

            string errorMessage = string.Empty;
            if ( fstService.Cancel( currentTransaction, out errorMessage ) )
            {
                rockContext.SaveChanges();
                content.Text = String.Format( "<div class='alert alert-success'>Your recurring {0} has been deleted.</div>", GetAttributeValue( "TransactionLabel" ).ToLower() );
            }
            else
            {
                content.Text = String.Format( "<div class='alert alert-danger'>An error occured while deleting your scheduled transation. Message: {0}</div>", errorMessage );
            }
            
            bbtnDelete.Visible = false;
            btnEdit.Visible = false;

        }

        protected void btnEdit_Click( object sender, EventArgs e )
        {
            Button btnEdit = (Button)sender;
            RepeaterItem riItem = (RepeaterItem)btnEdit.NamingContainer;

            HiddenField hfScheduledTransactionId = (HiddenField)riItem.FindControl( "hfScheduledTransactionId" );

            var transactionSchedule = riItem.DataItem as FinancialScheduledTransaction;

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "ScheduledTransactionId", hfScheduledTransactionId.Value );
            this.NavigateToLinkedPage( "ScheduledTransactionEditPage", qryParams );
        }

        protected void lbAddScheduledTransaction_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue("ScheduledTransactionEntryPage") ) ) {
                this.NavigateToLinkedPage( "ScheduledTransactionEntryPage", null );
            }
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)
        private void ShowContent()
        {

            // get scheduled contributions for current user
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();
                FinancialScheduledTransactionService transactionService = new FinancialScheduledTransactionService( rockContext );

                var schedules = transactionService.Queryable( "ScheduledTransactionDetails.Account" )
                                .Where( s => s.AuthorizedPersonAlias.PersonId == CurrentPerson.Id && s.IsActive == true );

                rptScheduledTransactions.DataSource = schedules.ToList();
                rptScheduledTransactions.DataBind();

                if ( schedules.Count() == 0 )
                {
                    pnlNoScheduledTransactions.Visible = true;
                    lNoScheduledTransactionsMessage.Text = string.Format("No {0} currently exist.", GetAttributeValue("TransactionLabel").Pluralize().ToLower());
                }
            }
            
        }


        #endregion
}
    
}