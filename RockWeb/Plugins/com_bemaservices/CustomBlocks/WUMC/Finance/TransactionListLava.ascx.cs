﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemadev.Finance
{
    [DisplayName( "Transaction List Lava" )]
    [Category( "com_bemaservices > Finance" )]
    [Description( "Presents a summary of financial transactions broke out by year and account using lava" )]

    [ContextAware( typeof( Person ) )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the transaction summary.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/TransactionYearlySummary.lava' %}", "", 1 )]
    [AccountsField( "Accounts", "Limit the results to transactions that match the selected accounts.", false, "", "", 2 )]
    public partial class TransactionListLava : RockBlock, ISecondaryBlock
    {
        #region Base Control Methods

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
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

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
        /// 
        /// </summary>
        private class SummaryRecord
        {
            public int Year { get; set; }
            public int AccountId { get; set; }
            public decimal TotalAmount { get; set; }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var contributionType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( contributionType != null )
            {
                string timeframe = ddlTimeFrame.SelectedValue.ToString();
                DateTime today = DateTime.Today;
                DateTime endDate = DateTime.MinValue;
                DateTime startDate = DateTime.MinValue;

                if (timeframe == "1")
                {
                    //Last 3 months
                    var month = new DateTime(today.Year, today.Month, 1);
                    startDate = month.AddMonths(-3);
                    endDate = today;
                }
                else if(timeframe == "2")
                {
                    //Last month
                    var month = new DateTime(today.Year, today.Month, 1);
                    startDate = month.AddMonths(-1);
                    endDate = today;
                }
                else if (timeframe == "3")
                {
                    // Current Year
                  
                    startDate = new DateTime(today.Year, 1, 1);
                    endDate = new DateTime(today.Year, 12, 31);
                }
                else if (timeframe == "4")
                {
                    // last Year

                    startDate = new DateTime(today.AddYears(-1).Year, 1, 1);
                    endDate = new DateTime(today.AddYears(-1).Year, 12, 31);
                }

                var rockContext = new RockContext();
                var transactionDetailService = new FinancialTransactionDetailService( rockContext );
                var qry = transactionDetailService.Queryable().AsNoTracking()
                    .Where( a =>
                        a.Transaction.TransactionTypeValueId == contributionType.Id &&
                        a.Transaction.TransactionDateTime.HasValue &&
                        a.Transaction.TransactionDateTime >= startDate &&
                        a.Transaction.TransactionDateTime <= endDate && 
                        a.Transaction.FinancialPaymentDetail.CurrencyTypeValue.Id != 667 );
				
				qry = qry.OrderByDescending(x => x.Transaction.TransactionDateTime);

               
                var targetPerson = CurrentPerson;
                var personBuisness = targetPerson.GetBusinesses();
                Person selectedBuisness = new Person();

                if (Request["businessId"] != null )
                {
                    var personService = new PersonService(rockContext);
                    var buisness = personService.Get(Request["businessId"].AsInteger());

                    foreach (var bPerson in personBuisness)
                    {
                        if( bPerson.Id == buisness.Id)
                        {
                            selectedBuisness = bPerson;
                        }

                    }
                }

                if (selectedBuisness != null && Request["businessId"] != null)
                {
                    qry = qry.Where(t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == selectedBuisness.GivingId);
                }
                else if (targetPerson != null)
                {
                    qry = qry.Where(t => t.Transaction.AuthorizedPersonAlias.Person.GivingId == targetPerson.GivingId);
                }

                // Filter to configured Accounts.
                var accountGuids = GetAttributeValue( "Accounts" ).SplitDelimitedValues().AsGuidList();
                if ( accountGuids.Any() )
                {
                    qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
                }

                //List<SummaryRecord> summaryList;

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                mergeFields.Add("TransactionDetails", qry.ToList());

                lLavaOutput.Text = string.Empty;

                string template = GetAttributeValue( "LavaTemplate" );

                lLavaOutput.Text += template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );

                //if (IsUserAuthorized(Authorization.EDIT))
                //{
                //    lDebug.Visible = true;
                //    lDebug.Text = mergeFields.lavaDebugInfo();
                //}
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        protected void ddlTimeFrame_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }
    }
}
