// <copyright>
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
    [AccountsField( "Accounts", "A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be uses.", false, order: 0 )]
    [BooleanField( "Display Pledges", "Determines if pledges should be shown.", true, order: 1 )]
    [CodeEditorField( "Lava Template", "The Lava template to use for the contribution statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"{% capture pageTitle %}{{ 'Global' | Attribute:'OrganizationName' }} | Contribution Statement{%endcapture%}
{{ pageTitle | SetPageTitle }}

<div class=""row margin-b-xl"">
    <div class=""col-md-6"">
        <div class=""pull-left"">
            <img src=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ 'Global' | Attribute:'EmailHeaderLogo' }}"" width=""100px"" />
        </div>
        
        <div class=""pull-left margin-l-md margin-t-sm"">
            <strong>{{ 'Global' | Attribute:'OrganizationName' }}</strong><br />
            {{ 'Global' | Attribute:'OrganizationAddress' }}<br />
            {{ 'Global' | Attribute:'OrganizationWebsite' }}
        </div>
    </div>
    <div class=""col-md-6 text-right"">
        <h4>Charitable Contributions for the Year {{ StatementStartDate | Date:'yyyy' }}</h4>
        <p>{{ StatementStartDate | Date:'M/d/yyyy' }} - {{ StatementEndDate | Date:'M/d/yyyy' }}<p>
    </div>
</div>

<h4>
{{ Salutation }} <br />
{{ StreetAddress1 }} <br />
{% if StreetAddress2 and StreetAddress2 != '' %}
    {{ StreetAddress2 }} <br />
{% endif %}
{{ City }}, {{ State }} {{ PostalCode }}
</h4>


<div class=""clearfix"">
    <div class=""pull-right"">
        <a href=""#"" class=""btn btn-primary hidden-print"" onClick=""window.print();""><i class=""fa fa-print""></i> Print Statement</a> 
    </div>
</div>

<hr style=""opacity: .5;"" />

<h4 class=""margin-t-md margin-b-md"">Gift List</h4>


    <table class=""table table-bordered table-striped table-condensed"">
        <thead>
            <tr>
                <th>Date</th>
                <th>Giving Area</th>
                <th>Check/Trans #</th>
                <th align=""right"">Amount</th>
            </tr>
        </thead>    

        {% for transaction in TransactionDetails %}
            <tr>
                <td>{{ transaction.Transaction.TransactionDateTime | Date:'M/d/yyyy' }}</td>
                <td>{{ transaction.Account.Name }}</td>
                <td>{{ transaction.Transaction.TransactionCode }}</td>
                <td align=""right"">{{ 'Global' | Attribute:'CurrencySymbol' }}{{ transaction.Amount }}</td>
            </tr>
        {% endfor %}
    
    </table>




<div class=""row"">
    <div class=""col-xs-6 col-xs-offset-6"">
        <h4 class=""margin-t-md margin-b-md"">Fund Summary</h4>
        <div class=""row"">
            <div class=""col-xs-6"">
                <strong>Fund Name</strong>
            </div>
            <div class=""col-xs-6 text-right"">
                <strong>Total Amount</strong>
            </div>
        </div>
        
        {% for accountsummary in AccountSummary %}
            <div class=""row"">
                <div class=""col-xs-6"">{{ accountsummary.AccountName }}</div>
                <div class=""col-xs-6 text-right"">{{ 'Global' | Attribute:'CurrencySymbol' }}{{ accountsummary.Total }}</div>
            </div>
         {% endfor %}
    </div>
</div>

{% assign pledgeCount = Pledges | Size %}

{% if pledgeCount > 0 %}
    <hr style=""opacity: .5;"" />
    <h4 class=""margin-t-md margin-b-md"">Pledges <small>(as of {{ StatementEndDate | Date:'M/dd/yyyy' }})</small></h4>
 
    {% for pledge in Pledges %}
        <div class=""row"">
            <div class=""col-xs-6"">
                <strong>{{ pledge.AccountName }}</strong>
                
                <p>
                    Amt Pledged: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountPledged }} <br />
                    Amt Given: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountGiven }} <br />
                    Amt Remaining: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountRemaining }}
                </p>
            </div>
            <div class=""col-xs-6 padding-t-md"">
                <div class=""hidden-print"">
                    Pledge Progress
                    <div class=""progress"">
                      <div class=""progress-bar"" role=""progressbar"" aria-valuenow=""{{ pledge.PercentComplete }}"" aria-valuemin=""0"" aria-valuemax=""100"" style=""width: {{ pledge.PercentComplete }}%;"">
                        {{ pledge.PercentComplete }}%
                      </div>
                    </div>
                </div>
                <div class=""visible-print-block"">
                    Percent Complete <br />
                    {{ pledge.PercentComplete }}%
                </div>
            </div>
        </div>
    {% endfor %}
{% endif %}

<hr style=""opacity: .5;"" />
<p class=""text-center"">
    Thank you for your continued support of the {{ 'Global' | Attribute:'OrganizationName' }}. If you have any questions about your statement,
    email {{ 'Global' | Attribute:'OrganizationEmail' }} or call {{ 'Global' | Attribute:'OrganizationPhone' }}.
</p>

<p class=""text-center"">
    <em>Unless otherwise noted, the only goods and services provided are intangible religious benefits.</em>
</p>", order: 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Excluded Currency Types", "Select the currency types you would like to excluded.", false, true, order: 4 )]
    [BooleanField( "Allow Person Querystring", "Determines if any person other than the currently logged in person is allowed to be passed through the querystring. For security reasons this is not allowed by default.", false, order: 5 )]
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

            if ( PageParameter( "StatementYear" ).IsNotNullOrWhiteSpace() )
            {
                Int32.TryParse( PageParameter( "StatementYear" ), out statementYear );
            }

            FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

            Person targetPerson = CurrentPerson;

            // get excluded currency types setting
            List<Guid> excludedCurrencyTypes = new List<Guid>();
            if ( GetAttributeValue( "ExcludedCurrencyTypes" ).IsNotNullOrWhiteSpace() )
            {
                excludedCurrencyTypes = GetAttributeValue( "ExcludedCurrencyTypes" ).Split( ',' ).Select( Guid.Parse ).ToList();
            }

            var personGuid = PageParameter( "PersonGuid" ).AsGuidOrNull();

            if ( personGuid.HasValue )
            {
                // if "AllowPersonQueryString is False", only use the PersonGuid if it is a Guid of one of the current person's businesses
                var isCurrentPersonsBusiness = targetPerson != null && targetPerson.GetBusinesses().Any( b => b.Guid == personGuid.Value );
                if ( GetAttributeValue( "AllowPersonQuerystring" ).AsBoolean() || isCurrentPersonsBusiness )
                {
                    var person = new PersonService( rockContext ).Get( personGuid.Value );
                    if ( person != null )
                    {
                        targetPerson = person;
                    }
                }
            }

            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
            var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == targetPerson.GivingId ).Select( a => a.Id ).ToList();

            // get the transactions for the person or all the members in the person's giving group (Family)
            var qry = financialTransactionDetailService.Queryable().AsNoTracking()
                        .Where( t => t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value ) );

            qry = qry.Where( t => t.Transaction.TransactionDateTime.Value.Year == statementYear );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) ) )
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            }
            else
            {
                var accountGuids = GetAttributeValue( "Accounts" ).Split( ',' ).Select( Guid.Parse ).ToList();
                qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
            }

            if ( excludedCurrencyTypes.Count > 0 )
            {
                qry = qry.Where( t => !excludedCurrencyTypes.Contains( t.Transaction.FinancialPaymentDetail.CurrencyTypeValue.Guid ) );
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

            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;
            var groupMemberQry = new GroupMemberService( rockContext ).Queryable().Where( m => m.Group.GroupTypeId == familyGroupTypeId );

            // get giving group members in order by family role (adult -> child) and then gender (male -> female)
            var givingGroup = new PersonService( rockContext ).Queryable().AsNoTracking()
                                    .Where( p => p.GivingId == targetPerson.GivingId )
                                    .GroupJoin(
                                        groupMemberQry,
                                        p => p.Id,
                                        m => m.PersonId,
                                        ( p, m ) => new { p, m } )
                                    .SelectMany( x => x.m.DefaultIfEmpty(), ( y, z ) => new { Person = y.p, GroupMember = z } )
                                    .Select( p => new { FirstName = p.Person.NickName, LastName = p.Person.LastName, FamilyRoleOrder = p.GroupMember.GroupRole.Order, Gender = p.Person.Gender, PersonId = p.Person.Id } )
                                    .DistinctBy( p => p.PersonId )
                                    .OrderBy( p => p.FamilyRoleOrder ).ThenBy( p => p.Gender )
                                    .ToList();

            string salutation = string.Empty;

            if ( givingGroup.GroupBy( g => g.LastName ).Count() == 1 )
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

            var mailingAddress = targetPerson.GetMailingLocation();
            if ( mailingAddress != null )
            {
                mergeFields.Add( "StreetAddress1", mailingAddress.Street1 );
                mergeFields.Add( "StreetAddress2", mailingAddress.Street2 );
                mergeFields.Add( "City", mailingAddress.City );
                mergeFields.Add( "State", mailingAddress.State );
                mergeFields.Add( "PostalCode", mailingAddress.PostalCode );
                mergeFields.Add( "Country", mailingAddress.Country );
            }
            else
            {
                mergeFields.Add( "StreetAddress1", string.Empty );
                mergeFields.Add( "StreetAddress2", string.Empty );
                mergeFields.Add( "City", string.Empty );
                mergeFields.Add( "State", string.Empty );
                mergeFields.Add( "PostalCode", string.Empty );
                mergeFields.Add( "Country", string.Empty );
            }

            mergeFields.Add( "TransactionDetails", qry.ToList() );

            mergeFields.Add( "AccountSummary", qry.GroupBy( t => new { t.Account.Name, t.Account.PublicName, t.Account.Description } )
                                                .Select( s => new AccountSummary
                                                {
                                                    AccountName = s.Key.Name,
                                                    PublicName = s.Key.PublicName,
                                                    Description = s.Key.Description,
                                                    Total = s.Sum( a => a.Amount ),
                                                    Order = s.Max( a => a.Account.Order )
                                                } )
                                                .OrderBy( s => s.Order ) );
            // pledge information
            var pledges = new FinancialPledgeService( rockContext ).Queryable().AsNoTracking()
                                .Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains(p.PersonAliasId.Value)
                                    && p.StartDate.Year <= statementYear && p.EndDate.Year >= statementYear )
                                .GroupBy( p => p.Account )
                                .Select( g => new PledgeSummary
                                {
                                    AccountId = g.Key.Id,
                                    AccountName = g.Key.Name,
                                    PublicName = g.Key.PublicName,
                                    AmountPledged = g.Sum( p => p.TotalAmount ),
                                    PledgeStartDate = g.Min( p => p.StartDate ),
                                    PledgeEndDate = g.Max( p => p.EndDate )
                                } )
                                .ToList();

            // add detailed pledge information
            foreach ( var pledge in pledges )
            {
                var adjustedPledgeEndDate = pledge.PledgeEndDate.Value.Date;
                var statementYearEnd = new DateTime( statementYear + 1, 1, 1 );
                
                if ( adjustedPledgeEndDate != DateTime.MaxValue.Date )
                {
                    adjustedPledgeEndDate = adjustedPledgeEndDate.AddDays( 1 );
                }

                if ( adjustedPledgeEndDate > statementYearEnd )
                {
                    adjustedPledgeEndDate = statementYearEnd;
                }

                if ( adjustedPledgeEndDate > RockDateTime.Now )
                {
                    adjustedPledgeEndDate = RockDateTime.Now;
                }

                pledge.AmountGiven = new FinancialTransactionDetailService( rockContext ).Queryable()
                                            .Where( t =>
                                                 t.AccountId == pledge.AccountId
                                                 && t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                                                 && t.Transaction.TransactionDateTime >= pledge.PledgeStartDate
                                                 && t.Transaction.TransactionDateTime < adjustedPledgeEndDate )
                                            .Sum( t => ( decimal? ) t.Amount ) ?? 0;

                pledge.AmountRemaining = ( pledge.AmountGiven > pledge.AmountPledged ) ? 0 : ( pledge.AmountPledged - pledge.AmountGiven );

                if ( pledge.AmountPledged > 0 )
                {
                    var test = ( double ) pledge.AmountGiven / ( double ) pledge.AmountPledged;
                    pledge.PercentComplete = ( int ) ( ( pledge.AmountGiven * 100 ) / pledge.AmountPledged );
                }
            }

            mergeFields.Add( "Pledges", pledges );

            var template = GetAttributeValue( "LavaTemplate" );

            lResults.Text = template.ResolveMergeFields( mergeFields );

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
            /// Gets or sets the Public Name of the pledge account.
            /// </summary>
            /// <value>
            /// The Public Name of the pledge account.
            /// </value>
            public string PublicName { get; set; }

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
            /// Gets or sets the public name of the account.
            /// </summary>
            /// <value>
            /// The public name of the account.
            /// </value>
            public string PublicName { get; set; }

            /// <summary>
            /// Gets or sets the description of the account.
            /// </summary>
            /// <value>
            /// The description of the account.
            /// </value>
            public string Description { get; set; }

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
