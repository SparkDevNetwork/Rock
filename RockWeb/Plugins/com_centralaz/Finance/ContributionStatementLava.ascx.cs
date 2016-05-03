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
            var adultRoleId = new GroupTypeRoleService( rockContext ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            var year = PageParameter( "Year" ).AsIntegerOrNull();
            if ( year != null )
            {
                var targetPerson = CurrentPerson;
                if ( targetPerson != null )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                    var contributionTypeGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid();
                    var transactionDetailService = new FinancialTransactionService( rockContext );
                    var qry = transactionDetailService.Queryable( "FinancialTransaction,FinancialPaymentDetail" ).AsNoTracking()
                        .Where( a => a.TransactionDateTime.HasValue &&
                            a.TransactionDateTime.Value.Year == year &&
                            a.AuthorizedPersonAlias.Person.GivingId == targetPerson.GivingId &&
                            a.TransactionTypeValue.Guid == contributionTypeGuid
                            );

                    //Add Transaction Details
                    var transactionList = qry
                        .Select( t => new
                        {
                            Date = t.TransactionDateTime.Value,
                            Amount = t.TransactionDetails.Sum( d => d.Amount ),
                            SourceType = t.FinancialPaymentDetail != null ? t.FinancialPaymentDetail.CurrencyTypeValue.Value : "N/A",
                            Accounts = t.TransactionDetails.GroupBy( b => b.Account.Id ).Select( c => new
                            {
                                AccountName = c.Max( d => d.Account.Name ),
                                TotalAmount = c.Sum( d => d.Amount )
                            } ).OrderBy( e => e.AccountName )
                        } ).OrderBy( a => a.Date )
                        .ToList();

                    var yearsMergeObjects = new List<Dictionary<string, object>>();
                    foreach ( var item in transactionList )
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

                    mergeFields.Add( "Transactions", yearsMergeObjects );

                    // Add Yearly Totals
                    var qryTransactionDetails = qry.SelectMany( a => a.TransactionDetails );
                    var qryFinancialAccount = new FinancialAccountService( rockContext ).Queryable();
                    var accountSummaryQry = qryTransactionDetails.GroupBy( a => a.AccountId ).Select( a => new
                    {
                        AccountId = a.Key,
                        TotalAmount = (decimal?)a.Sum( d => d.Amount )
                    } ).Join( qryFinancialAccount, k1 => k1.AccountId, k2 => k2.Id, ( td, fa ) => new { td.TotalAmount, fa.Name, fa.Order } )
                    .OrderBy( a => a.Order );

                    var summaryList = accountSummaryQry.ToList();
                    var grandTotalAmount = ( summaryList.Count > 0 ) ? summaryList.Sum( a => a.TotalAmount ?? 0 ) : 0;

                    mergeFields.Add( "Totals", summaryList );
                    mergeFields.Add( "FinalTotal", grandTotalAmount );

                    //Add Giving People
                    var givingPeopleList = new PersonService( rockContext ).Queryable()
                        .Where( p => p.GivingId == targetPerson.GivingId )
                        .ToList();

                    var givingAdultList = givingPeopleList.Where( p =>
                            p.GetFamilyMembers( true ).Any( m =>
                                m.PersonId == p.Id &&
                                m.GroupRoleId == adultRoleId ) )
                                .ToList();

                    mergeFields.Add( "GivingPeople", givingPeopleList );
                    mergeFields.Add( "GivingAdults", givingAdultList );

                    lContent.Text = string.Empty;
                    if ( GetAttributeValue( "EnableDebug" ).AsBooleanOrNull().GetValueOrDefault( false ) )
                    {
                        lContent.Text = mergeFields.lavaDebugInfo( rockContext );
                    }

                    string template = GetAttributeValue( "LavaTemplate" );

                    lContent.Text += template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID );
                }
            }
        }

        #endregion
    }
}