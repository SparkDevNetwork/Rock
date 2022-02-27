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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Passion | Transaction Report" )]
    [Category( "Finance" )]
    [Description( "Block that reports transactions for the currently logged in user with filters." )]
    [TextField( "Transaction Label", "The label to use to describe the transactions (e.g. 'Gifts', 'Donations', etc.)", true, "Gifts", "", 1 )]
    [TextField( "Account Label", "The label to use to describe accounts.", true, "Accounts", "", 2 )]
    [AccountsField( "Accounts", "List of accounts to allow the person to view", false, "", "", 3 )]
    [BooleanField( "Show Transaction Code", "Show the transaction code column in the table.", true, "", 4, "ShowTransactionCode" )]
    [BooleanField( "Show Foreign Key", "Show the transaction foreign key column in the table.", false, "", 4, "ShowForeignKey" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Types", "Optional list of transaction types to limit the list to (if none are selected all types will be included).", false, true, "", "", 5 )]
    [BooleanField( "Use Person Context", "Determines if the person context should be used instead of the CurrentPerson.", false, order: 5 )]
    [CodeEditorField("Lava Template", "The Lava template to use for the contribution statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"{% capture pageTitle %}{{ 'Global' | Attribute:'OrganizationName' }} | Contribution Statement{%endcapture%}

{% assign pledgeCount = Pledges | Size %}

{% if pledgeCount > 0 %}

<div class=""col-xs-12"">
<hr style=""opacity: .5;"" />
</div>

<div class=""col-xs-12 text-center form-group"">
    <img src=""../Content/ExternalSite/Graphics/a-b-logo.png"" />
</div>

    {% for pledge in Pledges %}
            <div class=""col-xs-12 col-sm-8 col-sm-offset-2"">
                <div class=""row form-group"">
                    <div class=""col-xs-6 statcard-desc"">Commitment:</div>
                    <div class=""col-xs-6 statcard-desc text-right"">{{ pledge.AmountPledged | FormatAsCurrency }}</div>
                    <div class=""col-xs-6 statcard-desc"">Given:</div>
                    <div class=""col-xs-6 statcard-desc text-right"">{{ pledge.AmountGiven | FormatAsCurrency }}</div>
                    <div class=""col-xs-6 statcard-desc"">Outstanding:</div>
                    <div class=""col-xs-6 statcard-desc text-right"">{{ pledge.AmountRemaining | FormatAsCurrency }}</div>
                </div>
                <div class=""hidden-print"">
                    <div class=""progress"">
                      <div class=""progress-bar"" role=""progressbar"" aria-valuenow=""{{ pledge.PercentComplete }}"" aria-valuemin=""0"" aria-valuemax=""100"" style=""width: {{ pledge.PercentComplete }}%;"">
                        {{ pledge.PercentComplete }}%
                      </div>
                    </div>
                </div>
            </div>
    {% endfor %}
{% endif %}


", order: 2)]
    [ContextAware]
    public partial class TransactionReport : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion


        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( GetAttributeValue( "UsePersonContext" ).AsBoolean() )
            {
                TargetPerson = ContextEntity<Person>();
            }
            else
            {
                TargetPerson = CurrentPerson;
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            cblAccounts.Label = GetAttributeValue( "AccountLabel" );
            gTransactions.DataKeyNames = new string[] { "Id" };
            gTransactions.RowItemText = GetAttributeValue( "TransactionLabel" );
            gTransactions.EmptyDataText = string.Format( "No {0} found with the provided criteria.", GetAttributeValue( "TransactionLabel" ).ToLower() );
            gTransactions.GridRebind += gTransactions_GridRebind;

            gTransactions.Actions.ShowMergeTemplate = false;
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
                // set default date range
                drpFilterDates.LowerValue = new DateTime( RockDateTime.Now.Year, 1, 1 );
                drpFilterDates.UpperValue = RockDateTime.Now;

                // load account list
                LoadAccounts();

                BindGrid();
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
            //
        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the bbtnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnApply_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the accounts.
        /// </summary>
        private void LoadAccounts()
        {
            var rockContext = new RockContext();
            FinancialAccountService accountService = new FinancialAccountService( rockContext );

            List<Guid> selectedAccounts = new List<Guid>();

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) ) )
            {
                selectedAccounts = GetAttributeValue( "Accounts" ).Split( ',' ).AsGuidList();
            }

            var accountList = accountService.Queryable()
                                .Where( a => selectedAccounts.Contains( a.Guid ) )
                                .OrderBy( a => a.Order )
                                .Select( a => new
                                {
                                    a.Id,
                                    a.PublicName
                                } ).ToList();

            if ( accountList.Any() )
            {
                foreach ( var account in accountList )
                {
                    ListItem checkbox = new ListItem( account.PublicName, account.Id.ToString(), true );
                    checkbox.Selected = true;

                    cblAccounts.Items.Add( checkbox );
                }
            }
            else
            {
                cblAccounts.Items.Clear();
            }

            // only show Account Checkbox list if there are accounts are configured for the block
            cblAccounts.Visible = accountList.Any();

            /* Also Load Commitments */

            Person targetPerson = CurrentPerson;

            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
            var personAliasIds = new PersonAliasService(rockContext).Queryable().Where(a => a.Person.GivingId == targetPerson.GivingId).Select(a => a.Id).ToList();

            var statementYear = RockDateTime.Now.Year;

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add("StatementStartDate", "1/1/" + statementYear.ToString());
            if (statementYear == RockDateTime.Now.Year)
            {
                mergeFields.Add("StatementEndDate", RockDateTime.Now);
            }
            else
            {
                mergeFields.Add("StatementEndDate", "12/31/" + statementYear.ToString());
            }


            List<PledgeSummary> pledges = GetPledgeDataForPersonYear(rockContext, statementYear, personAliasIds);
            mergeFields.Add("Pledges", pledges);

            var template = GetAttributeValue("LavaTemplate");

            lCommitmentResults.Text = template.ResolveMergeFields(mergeFields);

            
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            FinancialTransactionService transService = new FinancialTransactionService( rockContext );
            var qry = transService.Queryable( "TransactionDetails.Account,FinancialPaymentDetail" );

            List<int> personAliasIds;

            if ( TargetPerson != null )
            {
                personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == TargetPerson.GivingId ).Select( a => a.Id ).ToList();
            }
            else
            {
                personAliasIds = new List<int>();
            }

            qry = qry.Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );


            // if the Account Checkboxlist is visible, filter to what was selected.  Otherwise, show all the accounts that the person contributed to
            if ( cblAccounts.Visible )
            {
                // get list of selected accounts
                List<int> selectedAccountIds = cblAccounts.Items.Cast<ListItem>()
                                                .Where( i => i.Selected == true )
                                                .Select( i => int.Parse( i.Value ) ).ToList();
                qry = qry.Where( t => t.TransactionDetails.Any( d => selectedAccountIds.Contains( d.AccountId ) ) );
            }

            var ytdQry = qry;

            if ( drpFilterDates.LowerValue.HasValue )
            {
                var ytdLower = new DateTime(RockDateTime.Now.Year, 1, 1);
                qry = qry.Where( t => t.TransactionDateTime.Value >= drpFilterDates.LowerValue.Value );
                ytdQry = ytdQry.Where(t => t.TransactionDateTime.Value >= ytdLower);
            }

            if ( drpFilterDates.UpperValue.HasValue )
            {
                var lastDate = drpFilterDates.UpperValue.Value.AddDays( 1 ); // add one day to ensure we get all transactions till midnight
                var ytdUpper = RockDateTime.Now.AddDays(1);
                qry = qry.Where( t => t.TransactionDateTime.Value < lastDate );
                ytdQry = ytdQry.Where(t => t.TransactionDateTime.Value < ytdUpper);
            }

            // Transaction Types
            var transactionTypeValueIdList = GetAttributeValue( "TransactionTypes" ).SplitDelimitedValues().AsGuidList().Select( a => DefinedValueCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList();

            if ( transactionTypeValueIdList.Any() )
            {
                qry = qry.Where( t => transactionTypeValueIdList.Contains( t.TransactionTypeValueId ) );
                ytdQry = ytdQry.Where(t => transactionTypeValueIdList.Contains(t.TransactionTypeValueId));
            }

            qry = qry.OrderByDescending( a => a.TransactionDateTime );

            var txns = qry.ToList();
            var txnsTotal = ytdQry.ToList();

            // get account totals
            Dictionary<string, decimal> accountTotals = new Dictionary<string, decimal>();
            Dictionary<string, decimal> totalAccounts = new Dictionary<string, decimal>();

            foreach ( var transaction in txns )
            {
                foreach ( var transactionDetail in transaction.TransactionDetails )
                {
                    if ( accountTotals.Keys.Contains( transactionDetail.Account.PublicName ) )
                    {
                        accountTotals[transactionDetail.Account.PublicName] += transactionDetail.Amount;
                    }
                    else
                    {
                        accountTotals.Add( transactionDetail.Account.PublicName, transactionDetail.Amount );
                    }
                }
            }

            foreach (var transaction in txnsTotal)
            {
                foreach (var transactionDetail in transaction.TransactionDetails)
                {
                    if (totalAccounts.Keys.Contains(transactionDetail.Account.PublicName))
                    {
                        totalAccounts[transactionDetail.Account.PublicName] += transactionDetail.Amount;
                    }
                    else
                    {
                        totalAccounts.Add(transactionDetail.Account.PublicName, transactionDetail.Amount);
                    }
                }
            }

            gyTD.InnerHtml = totalAccounts.Sum(x => x.Value).ToString("C2");

            lAccountSummary.Text = string.Empty;
            if ( accountTotals.Count > 0 )
            {
                pnlSummary.Visible = true;
                foreach ( var key in accountTotals.Keys )
                {
                    lAccountSummary.Text += string.Format( "<li><b>{0}:</b> <span class='pull-right'>{1}</li>", key, accountTotals[key].ToString("C2") );
                }
            }
            else
            {
                pnlSummary.Visible = false;
            }

            gTransactions.EntityTypeId = EntityTypeCache.Get<FinancialTransaction>().Id;
            gTransactions.DataSource = txns.Select( t => new
            {
                t.Id,
                t.TransactionDateTime,
                CurrencyType = FormatCurrencyType( t ),
                t.TransactionCode,
                t.ForeignKey,
                Summary = FormatSummary( t ),
                t.TotalAmount
            } ).ToList();

            gTransactions.ColumnsOfType<Rock.Web.UI.Controls.RockBoundField>().First( c => c.HeaderText == "Transaction Code" ).Visible =
                GetAttributeValue( "ShowTransactionCode" ).AsBoolean();

            gTransactions.ColumnsOfType<Rock.Web.UI.Controls.RockBoundField>().First( c => c.HeaderText == "Foreign Key" ).Visible =
                GetAttributeValue( "ShowForeignKey" ).AsBoolean();

            gTransactions.DataBind();
        }

        /// <summary>
        /// Formats the type of the currency.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        /// <returns></returns>
        protected string FormatCurrencyType( FinancialTransaction txn )
        {
            string currencyType = string.Empty;
            string creditCardType = string.Empty;

            if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
            {
                int currencyTypeId = txn.FinancialPaymentDetail.CurrencyTypeValueId.Value;

                var currencyTypeValue = DefinedValueCache.Get( currencyTypeId );
                currencyType = currencyTypeValue != null ? currencyTypeValue.Value : string.Empty;

                if ( txn.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                {
                    int creditCardTypeId = txn.FinancialPaymentDetail.CreditCardTypeValueId.Value;
                    var creditCardTypeValue = DefinedValueCache.Get( creditCardTypeId );
                    creditCardType = creditCardTypeValue != null ? creditCardTypeValue.Value : string.Empty;

                    return string.Format( "{0} - {1}", currencyType, creditCardType );
                }
            }

            return currencyType;
        }

        /// <summary>
        /// Formats the summary.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        /// <returns></returns>
        private string FormatSummary( FinancialTransaction txn )
        {
            var sb = new StringBuilder();
            foreach ( var transactionDetail in txn.TransactionDetails )
            {
                sb.AppendFormat( "{0}<br>", transactionDetail.Account );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the pledge data for the given person and year.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="statementYear">The statement year.</param>
        /// <param name="personAliasIds">The person alias ids.</param>
        /// <returns></returns>
        private static List<PledgeSummary> GetPledgeDataForPersonYear(RockContext rockContext, int statementYear, List<int> personAliasIds)
        {
            var pledges = new FinancialPledgeService(rockContext).Queryable().AsNoTracking()
                                            .Where(p => p.PersonAliasId.HasValue && personAliasIds.Contains(p.PersonAliasId.Value)
                                               && p.StartDate.Year <= statementYear && p.EndDate.Year >= statementYear)
                                            .GroupBy(p => p.Account)
                                            .Select(g => new PledgeSummary
                                            {
                                                AccountId = g.Key.Id,
                                                AccountName = g.Key.Name,
                                                PublicName = g.Key.PublicName,
                                                AmountPledged = g.Sum(p => p.TotalAmount),
                                                PledgeStartDate = g.Min(p => p.StartDate),
                                                PledgeEndDate = g.Max(p => p.EndDate)
                                            })
                                            .ToList();

            // add detailed pledge information
            foreach (var pledge in pledges)
            {
                var adjustedPledgeEndDate = pledge.PledgeEndDate.Value.Date;
                var adjustedPledgeStartDate = DateTime.Parse("7/1/2018");
                var statementYearEnd = new DateTime(statementYear + 1, 1, 1);

                if (adjustedPledgeEndDate != DateTime.MaxValue.Date)
                {
                    adjustedPledgeEndDate = adjustedPledgeEndDate.AddDays(1);
                }

                if (adjustedPledgeEndDate > statementYearEnd)
                {
                    adjustedPledgeEndDate = statementYearEnd;
                }

                if (adjustedPledgeEndDate > RockDateTime.Now)
                {
                    adjustedPledgeEndDate = RockDateTime.Now;
                }

                pledge.AmountGiven = new FinancialTransactionDetailService(rockContext).Queryable()
                                            .Where(t =>
                                                t.AccountId == pledge.AccountId
                                                && t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains(t.Transaction.AuthorizedPersonAliasId.Value)
                                                && t.Transaction.TransactionDateTime >= adjustedPledgeStartDate
                                                && t.Transaction.TransactionDateTime < adjustedPledgeEndDate)
                                            .Sum(t => (decimal?)t.Amount) ?? 0;

                pledge.AmountRemaining = (pledge.AmountGiven > pledge.AmountPledged) ? 0 : (pledge.AmountPledged - pledge.AmountGiven);

                if (pledge.AmountPledged > 0)
                {
                    var test = (double)pledge.AmountGiven / (double)pledge.AmountPledged;
                    pledge.PercentComplete = (int)((pledge.AmountGiven * 100) / pledge.AmountPledged);
                }
            }

            return pledges;
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
        #endregion
    }
}