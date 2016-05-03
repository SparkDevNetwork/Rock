// <copyright>
// Copyright by the Spark Development Network
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
using System.Data.Entity;
using Rock.Security;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Contribution Statement Lava" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a Lava based contribution statement." )]
    [AccountsField("Accounts", "A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be uses.", false, order: 0 )]
    [BooleanField("Display Pledges", "Determines if pledges should be shown.", true, order:1)]
    [CodeEditorField("Lava Template", "The Lava template to use for the contribution statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"{% assign currentYear = 'Now' | Date:'yyyy' %}

<h4>Available Contribution Statements</h4>

<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>", order: 2)]
    [BooleanField("Enable Debug", "Shows the merge fields available for the Lava", order:3)]
    public partial class ContributionStatementLava : Rock.Web.UI.RockBlock
    {
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

            if ( !Page.IsPostBack )
            {
                DisplayResults();
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
            DisplayResults();
        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            RockContext rockContext = new RockContext();

            var statementYear = RockDateTime.Now.Year;

            if ( Request["StatementYear"] != null )
            {
                Int32.TryParse( Request["StatementYear"].ToString(), out statementYear );
            }

            FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            
            var qry = financialTransactionDetailService.Queryable().AsNoTracking()
                        .Where( t=> t.Transaction.AuthorizedPersonAlias.Person.GivingId == CurrentPerson.GivingId);

            qry = qry.Where( t => t.Transaction.TransactionDateTime.Value.Year == statementYear );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) ) )
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            } else
            {
                var accountGuids = GetAttributeValue( "Accounts" ).Split( ',' ).Select( Guid.Parse ).ToList();
                qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
            }

            qry = qry.OrderByDescending( t => t.Transaction.TransactionDateTime );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "StatementStartDate", "1/1/" + statementYear.ToString() );
            if ( statementYear == RockDateTime.Now.Year )
            {
                mergeFields.Add( "StatementEndDate", RockDateTime.Now );
            }
            else
            {
                mergeFields.Add( "StatementEndDate", "12/31/" + statementYear.ToString() );
            }

            var familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
            var groupMemberQry = new GroupMemberService( rockContext ).Queryable().Where( m => m.Group.GroupTypeId == familyGroupTypeId );

            // get giving group members in order by family role (adult -> child) and then gender (male -> female)
            var givingGroup = new PersonService( rockContext ).Queryable().AsNoTracking()
                                    .Where( p => p.GivingId == CurrentPerson.GivingId )
                                    .GroupJoin(
                                        groupMemberQry,
                                        p => p.Id,
                                        m => m.PersonId,
                                        (p, m) => new {p, m})
                                    .SelectMany( x => x.m.DefaultIfEmpty(), (y,z) => new { Person = y.p, GroupMember = z} )
                                    .Select( p => new { FirstName = p.Person.NickName, LastName = p.Person.LastName, FamilyRoleOrder = p.GroupMember.GroupRole.Order, Gender = p.Person.Gender, PersonId = p.Person.Id } )
                                    .DistinctBy(p => p.PersonId)
                                    .OrderBy(p => p.FamilyRoleOrder).ThenBy(p => p.Gender)
                                    .ToList();

            string salutation = string.Empty;

            if (givingGroup.GroupBy(g => g.LastName).Count() == 1 )
            {
                salutation = string.Join( ", ", givingGroup.Select( g => g.FirstName ) ) + " " + givingGroup.FirstOrDefault().LastName;
                if ( salutation.Contains( "," ) )
                {
                    salutation = salutation.ReplaceLastOccurrence( ",", " &" );
                }
            }
            else
            {
                salutation = string.Join( ", ", givingGroup.Select( g => g.FirstName + " " + g.LastName ) );
                if ( salutation.Contains( "," ) )
                {
                    salutation = salutation.ReplaceLastOccurrence( ",", " &" );
                }
            }
            mergeFields.Add( "Salutation", salutation );

            var homeAddress = CurrentPerson.GetHomeLocation();
            mergeFields.Add( "StreetAddress1", homeAddress.Street1 );
            mergeFields.Add( "StreetAddress2", homeAddress.Street2 );
            mergeFields.Add( "City", homeAddress.City );
            mergeFields.Add( "State", homeAddress.State );
            mergeFields.Add( "PostalCode", homeAddress.PostalCode );
            mergeFields.Add( "Country", homeAddress.Country );

            mergeFields.Add( "TransactionDetails", qry.ToList() );
                        
            mergeFields.Add( "AccountSummary", qry.GroupBy( t => t.Account.Name ).Select( s => new AccountSummary { AccountName = s.Key, Total = s.Sum( a => a.Amount ), Order = s.Max(a => a.Account.Order) } ).OrderBy(s => s.Order ));

            // pledge information
            var pledges = new FinancialPledgeService( rockContext ).Queryable().AsNoTracking()
                                .Where(p => 
                                    p.PersonAlias.Person.GivingId == CurrentPerson.GivingId
                                    && (p.StartDate.Year == statementYear || p.EndDate.Year == statementYear))
                                .GroupBy(p => p.Account)
                                .Select(g => new PledgeSummary {
                                                    AccountId = g.Key.Id,
                                                    AccountName = g.Key.Name,
                                                    AmountPledged = g.Sum( p => p.TotalAmount ),
                                                    PledgeStartDate = g.Min(p => p.StartDate),
                                                    PledgeEndDate = g.Max( p => p.EndDate)
                                } )
                                .ToList();

            // add detailed pledge information
            foreach(var pledge in pledges )
            {
                var adjustedPedgeEndDate = pledge.PledgeEndDate.Value.Date.AddHours( 23 ).AddMinutes( 59 ).AddSeconds( 59 );
                pledge.AmountGiven = new FinancialTransactionDetailService( rockContext ).Queryable()
                                            .Where( t =>
                                                 t.AccountId == pledge.AccountId
                                                 && t.Transaction.TransactionDateTime >= pledge.PledgeStartDate
                                                 && t.Transaction.TransactionDateTime <= adjustedPedgeEndDate )
                                            .Sum( t => t.Amount );

                pledge.AmountRemaining = (pledge.AmountGiven > pledge.AmountPledged) ? 0 : (pledge.AmountPledged - pledge.AmountGiven);

                if ( pledge.AmountPledged > 0 )
                {
                    var test = (double)pledge.AmountGiven / (double)pledge.AmountPledged;
                    pledge.PercentComplete = (int)((pledge.AmountGiven * 100) / pledge.AmountPledged);
                }
            }

            mergeFields.Add( "Pledges", pledges );

            var template = GetAttributeValue( "LavaTemplate" );

            lResults.Text = template.ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// Pledge Summary Class
        /// </summary>
        public class PledgeSummary : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the pledge account identifier.
            /// </summary>
            /// <value>
            /// The pledge account identifier.
            /// </value>
            public int AccountId { get; set; }

            /// <summary>
            /// Gets or sets the pledge account.
            /// </summary>
            /// <value>
            /// The pledge account.
            /// </value>
            public string AccountName { get; set; }

            /// <summary>
            /// Gets or sets the pledge start date.
            /// </summary>
            /// <value>
            /// The pledge start date.
            /// </value>
            public DateTime? PledgeStartDate { get; set; }

            /// <summary>
            /// Gets or sets the pledge end date.
            /// </summary>
            /// <value>
            /// The pledge end date.
            /// </value>
            public DateTime? PledgeEndDate { get; set; }

            /// <summary>
            /// Gets or sets the amount pledged.
            /// </summary>
            /// <value>
            /// The amount pledged.
            /// </value>
            public decimal AmountPledged { get; set; }

            /// <summary>
            /// Gets or sets the amount given.
            /// </summary>
            /// <value>
            /// The amount given.
            /// </value>
            public decimal AmountGiven { get; set; }

            /// <summary>
            /// Gets or sets the amount remaining.
            /// </summary>
            /// <value>
            /// The amount remaining.
            /// </value>
            public decimal AmountRemaining { get; set; }

            /// <summary>
            /// Gets or sets the percent complete.
            /// </summary>
            /// <value>
            /// The percent complete.
            /// </value>
            public int PercentComplete { get; set; }
        }

        /// <summary>
        /// Account Summary Class
        /// </summary>
        public class AccountSummary : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the name of the account.
            /// </summary>
            /// <value>
            /// The name of the account.
            /// </value>
            public string AccountName { get; set; }

            /// <summary>
            /// Gets or sets the total.
            /// </summary>
            /// <value>
            /// The total.
            /// </value>
            public decimal Total { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            public int Order { get; set; }
        }
        #endregion  
    }
}