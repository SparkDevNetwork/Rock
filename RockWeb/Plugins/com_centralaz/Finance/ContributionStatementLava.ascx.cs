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
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Lava;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    [DisplayName( "Contribution Statement Lava" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Provides a contribution statement for the user to print off." )]

    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~/Plugins/com_centralaz/Finance/Lava/ContributionStatementLava.lava' %}", "", 0 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 1 )]
    public partial class ContributionStatementLava : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                GenerateStatement();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GenerateStatement();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Generates the statement.
        /// </summary>
        private void GenerateStatement()
        {
            RockContext rockContext = new RockContext();
            var year = PageParameter( "Year" ).AsIntegerOrNull();
            if ( year != null )
            {
                var transactionDetailService = new FinancialTransactionDetailService( rockContext );
                var qry = transactionDetailService.Queryable("FinancialTransaction,FinancialPaymentDetail").AsNoTracking()
                    .Where( a => a.Transaction.TransactionDateTime.HasValue );

                var targetPerson = CurrentPerson;
                if ( targetPerson != null )
                {
                    qry = qry.Where( t => 
                        t.Transaction.AuthorizedPersonAlias.Person.GivingId == targetPerson.GivingId &&
                        t.Transaction.TransactionDateTime.HasValue &&
                        t.Transaction.TransactionDateTime.Value.Year == year
                        );

                    var transactionDetails = qry.Select( t => new
                    {
                        AccountId = t.Account.Id,
                        AccountName = t.Account.Name,
                        Amount = t.Amount,
                        Transaction = t.Transaction
                    } )
                    .ToList();

                    var summaryList = transactionDetails
                        .GroupBy( a => a.Transaction )
                        .Select( t => new
                        {
                            Transaction = t.Key,
                            Date = t.Key.TransactionDateTime.Value,
                            Amount = t.Sum(d=> d.Amount),
                            SourceType = t.Key.FinancialPaymentDetail != null ?  t.Key.FinancialPaymentDetail.CurrencyTypeValue.Value : "N/A",
                            Accounts = t.GroupBy( b => b.AccountId ).Select( c => new
                            {
                                AccountName = c.Max( d => d.AccountName ),
                                TotalAmount = c.Sum( d => d.Amount )
                            } ).OrderBy( e => e.AccountName )
                        } ).OrderByDescending( a => a.Date )
                        .ToList();

                    var totalList = transactionDetails
                       .GroupBy( b => b.AccountId )
                       .Select( c => new
                           {
                               AccountName = c.Max( d => d.AccountName ),
                               TotalAmount = c.Sum( d => d.Amount )
                           } )
                       .OrderBy( e => e.AccountName )
                       .ToList();

                    var givingPeopleList = new PersonService(rockContext).Queryable()
                        .Where( p=> p.GivingId == targetPerson.GivingId)
                        .ToList()
                        ;
                    var finalTotal = totalList.Sum( t => t.TotalAmount );

                    var mergeObjects = GlobalAttributesCache.GetMergeFields( this.CurrentPerson );

                    var yearsMergeObjects = new List<Dictionary<string, object>>();
                    foreach ( var item in summaryList )
                    {
                        var accountsList = new List<object>();
                        foreach ( var a in item.Accounts )
                        {
                            var accountDictionary = new Dictionary<string, object>();
                            accountDictionary.Add( "Fund", a.AccountName );
                            accountDictionary.Add( "Amount", a.TotalAmount );
                            accountsList.Add( accountDictionary );
                        }

                        var transactionDictionary = new Dictionary<string, object>();
                        transactionDictionary.Add( "Date", item.Date );
                        transactionDictionary.Add( "Type", item.SourceType );
                        transactionDictionary.Add( "Amount", item.Amount );
                        transactionDictionary.Add( "Funds", accountsList );

                        yearsMergeObjects.Add( transactionDictionary );
                    }

                    mergeObjects.Add( "GivingPeople", givingPeopleList );
                    mergeObjects.Add( "Transactions", yearsMergeObjects );
                    mergeObjects.Add( "Totals", totalList );
                    mergeObjects.Add( "FinalTotal", finalTotal );
                    mergeObjects.Add( "CurrentPerson", CurrentPerson );

                    lContent.Text = string.Empty;
                    if ( GetAttributeValue( "EnableDebug" ).AsBooleanOrNull().GetValueOrDefault( false ) )
                    {
                        lContent.Text = mergeObjects.lavaDebugInfo( rockContext );
                    }

                    string template = GetAttributeValue( "LavaTemplate" );

                    lContent.Text += template.ResolveMergeFields( mergeObjects ).ResolveClientIds( upnlContent.ClientID );
                }
            }
        }
        #endregion
    }
}