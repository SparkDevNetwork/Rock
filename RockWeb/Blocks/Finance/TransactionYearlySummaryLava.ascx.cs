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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Transaction Yearly Summary Lava" )]
    [Category( "Finance" )]
    [Description( "Presents a summary financial transactions broke out by year and account using lava" )]

    [ContextAware( typeof( Person ) )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the transaction summary.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/TransactionYearlySummary.lava' %}", "", 1 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 2 )]
    public partial class TransactionYearlySummaryLava : RockBlock
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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var transactionIdsQry = new FinancialTransactionService( rockContext ).Queryable();
            var transactionDetailService = new FinancialTransactionDetailService( rockContext );
            var qry = transactionDetailService.Queryable();

            var targetPerson = this.ContextEntity<Person>();
            if ( targetPerson != null )
            {
                if ( targetPerson.GivingGroupId.HasValue )
                {
                    // gives as part of giving group
                    var groupMemberQry = new GroupMemberService( rockContext ).GetByGroupId( targetPerson.GivingGroupId.Value );
                    qry = qry.Where( a => groupMemberQry.Any( b => b.PersonId == a.Transaction.AuthorizedPersonAlias.PersonId ) );
                }
                else
                {
                    // gives individually
                    qry = qry.Where( a => a.Transaction.AuthorizedPersonAlias.PersonId == targetPerson.Id );
                }
            }

            var qryTransactionsDetailsByYear = qry
                .Where( a => a.Transaction.TransactionDateTime.HasValue )
                .GroupBy( a => SqlFunctions.DatePart( "year", a.Transaction.TransactionDateTime.Value ) );

            var qryAccountSummariesByYear = qryTransactionsDetailsByYear.Select( a => new
            {
                Year = a.Key,
                Accounts = a.GroupBy( b => b.Account ).Select( c => new
                {
                    Account = c.Key,
                    TotalAmount = c.Sum( d => d.Amount )
                } ).OrderBy( e => e.Account.Name )
            } ).OrderByDescending( a => a.Year );

            var summaryList = qryAccountSummariesByYear.ToList();

            var mergeObjects = GlobalAttributesCache.GetMergeFields( this.CurrentPerson );

            var yearsMergeObjects = new List<Dictionary<string, object>>();
            foreach ( var item in summaryList )
            {
                var accountsList = new List<object>();
                foreach ( var a in item.Accounts )
                {
                    var accountDictionary = new Dictionary<string, object>();
                    accountDictionary.Add( "Account", a.Account );
                    accountDictionary.Add( "TotalAmount", a.TotalAmount );
                    accountsList.Add( accountDictionary );
                }

                var yearDictionary = new Dictionary<string, object>();
                yearDictionary.Add( "Year", item.Year );
                yearDictionary.Add( "SummaryRows", accountsList );

                yearsMergeObjects.Add( yearDictionary );
            }

            mergeObjects.Add( "Rows", yearsMergeObjects );

            lLavaOutput.Text = string.Empty;
            if ( GetAttributeValue( "EnableDebug" ).AsBooleanOrNull().GetValueOrDefault( false ) )
            {
                lLavaOutput.Text = mergeObjects.lavaDebugInfo( rockContext );
            }

            string template = GetAttributeValue( "LavaTemplate" );

            lLavaOutput.Text += template.ResolveMergeFields( mergeObjects ).ResolveClientIds( upnlContent.ClientID );
        }

        #endregion
    }
}
