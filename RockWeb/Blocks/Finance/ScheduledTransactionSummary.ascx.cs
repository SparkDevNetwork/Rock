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
    [DisplayName( "Scheduled Transaction Summary" )]
    [Category( "Finance" )]
    [Description( "Block that shows a summary of the scheduled transactions for the currently logged in user." )]
    [CodeEditorField( "Template", "Liquid template for the content to be placed on the page.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/ScheduledTransactionSummary.lava'  %}", "", 1 )]
    [BooleanField("Enable Debug", "Displays a list of available merge fields using the current person's scheduled transactions.", false, "", 2)]
    [LinkedPage("Manage Scheduled Transactions Page", "Link to be used for managing an individual's scheduled transactions.", false, "", "", 3)]
    [LinkedPage( "Transaction History Page", "Link to use for viewing an individual's transaction history.", false, "", "", 4 )]
    [LinkedPage("Transaction Entry Page", "Link to use when adding new transactions.", false, "", "", 5)]
    public partial class ScheduledTransactionSummary : Rock.Web.UI.RockBlock
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

            ShowContent();
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

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)
        private void ShowContent()
        {
            List<Dictionary<string, object>> scheduleSummaries = new List<Dictionary<string, object>>();

            // get pledges for current user
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();
                FinancialScheduledTransactionService transactionService = new FinancialScheduledTransactionService( rockContext );

                var schedules = transactionService.Queryable( "ScheduledTransactionDetails.Account" )
                                .Where( s => s.AuthorizedPersonAlias.PersonId == CurrentPerson.Id && s.IsActive == true );

                foreach ( FinancialScheduledTransaction schedule in schedules )
                {
                    decimal totalAmount = 0;
                    
                    Dictionary<string, object> scheduleSummary = new Dictionary<string, object>();
                    scheduleSummary.Add("Id", schedule.Id);
                    scheduleSummary.Add("Guid", schedule.Guid);
                    scheduleSummary.Add("StartDate", schedule.StartDate);
                    scheduleSummary.Add("EndDate", schedule.EndDate);
                    scheduleSummary.Add("NextPaymentDate", schedule.NextPaymentDate);

                    if ( schedule.NextPaymentDate.HasValue )
                    {
                        scheduleSummary.Add( "DaysTillNextPayment", (schedule.NextPaymentDate.Value - DateTime.Now).Days );
                    }
                    else
                    {
                        scheduleSummary.Add( "DaysTillNextPayment", null );
                    }

                    DateTime? lastPaymentDate = schedule.Transactions.Max(t => t.TransactionDateTime);
                    scheduleSummary.Add("LastPaymentDate", lastPaymentDate);

                    if ( lastPaymentDate.HasValue )
                    {
                        scheduleSummary.Add("DaysSinceLastPayment",  (DateTime.Now - lastPaymentDate.Value).Days);
                    }
                    else
                    {
                        scheduleSummary.Add( "DaysSinceLastPayment", null );
                    }

                    scheduleSummary.Add("CurrencyType", schedule.CurrencyTypeValue.Value);
                    scheduleSummary.Add( "CreditCardType", (schedule.CreditCardTypeValue == null) ? "" : schedule.CreditCardTypeValue.Value );
                    scheduleSummary.Add("UrlEncryptedKey", schedule.UrlEncodedKey);
                    scheduleSummary.Add("Frequency",  schedule.TransactionFrequencyValue.Value);
                    scheduleSummary.Add("FrequencyDescription", schedule.TransactionFrequencyValue.Description);
                    
                    List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string,object>>();

                    foreach ( FinancialScheduledTransactionDetail detail in schedule.ScheduledTransactionDetails )
                    {
                        Dictionary<string, object> detailSummary = new Dictionary<string,object>();
                        detailSummary.Add("AccountId", detail.Id);
                        detailSummary.Add("AccountName", detail.Account.Name);
                        detailSummary.Add("Amount", detail.Amount);
                        detailSummary.Add("Summary", detail.Summary);

                        summaryDetails.Add( detailSummary );

                        totalAmount += detail.Amount;
                    }

                    scheduleSummary.Add("ScheduledAmount", totalAmount);
                    scheduleSummary.Add( "TransactionDetails", summaryDetails );

                    scheduleSummaries.Add( scheduleSummary );
                }

            }

            // added linked pages to mergefields
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "ManageScheduledTransactionsPage", LinkedPageUrl( "ManageScheduledTransactionsPage", null ) );
            linkedPages.Add( "TransactionHistoryPage", LinkedPageUrl( "TransactionHistoryPage", null ) );
            linkedPages.Add( "TransactionEntryPage", LinkedPageUrl( "TransactionEntryPage", null ) );



            var scheduleValues = new Dictionary<string, object>();
            scheduleValues.Add( "ScheduledTransactions", scheduleSummaries.ToList() );
            scheduleValues.Add( "LinkedPages", linkedPages );
            // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
            scheduleValues.Add( "Person", CurrentPerson );
            scheduleValues.Add( "CurrentPerson", CurrentPerson );

            string content = GetAttributeValue( "Template" ).ResolveMergeFields( scheduleValues );

            // show merge fields if needed
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                scheduleValues.Remove( "Person" );
                content += scheduleValues.lavaDebugInfo();
            }

            lContent.Text = content;
            
        }


        #endregion
    }    
}