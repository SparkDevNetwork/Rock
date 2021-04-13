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
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Giving Configuration" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the giving." )]

    [LinkedPage(
        "Add Transaction Page",
        Key = AttributeKey.AddTransactionPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.ADD_TRANSACTION,
        Order = 0 )]
    [IntegerField(
        "Person Token Expire Minutes",
        Key = AttributeKey.PersonTokenExpireMinutes,
        Description = "The number of minutes the person token for the transaction is valid after it is issued.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Order = 1 )]
    [IntegerField(
        "Person Token Usage Limit",
        Key = AttributeKey.PersonTokenUsageLimit,
        Description = "The maximum number of times the person token for the transaction can be used.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 2 )]
    [AccountsField(
        "Accounts",
        Key = AttributeKey.Accounts,
        Description = "A selection of accounts to use for checking if transactions for the current user exist.",
        IsRequired = false,
        Order = 3 )]
    [LinkedPage(
        "Pledge Detail Page",
        Key = AttributeKey.PledgeDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.PLEDGE_DETAIL,
        Order = 4 )]
    [IntegerField(
        "Max Years To Display",
        Description = "The maximum number of years to display (including the current year).",
        IsRequired = true,
        DefaultIntegerValue = 3,
        Order = 5,
        Key = AttributeKey.MaxYearsToDisplay )]
    [LinkedPage(
        "Contribution Statement Detail Page",
        Description = "The contribution statement detail page.",
        Order = 6,
        Key = AttributeKey.ContributionStatementDetailPage )]
    [LinkedPage(
        "Scheduled Transaction Detail Page",
        Key = AttributeKey.ScheduledTransactionDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.SCHEDULED_TRANSACTION,
        Order = 7 )]
    public partial class GivingConfiguration : Rock.Web.UI.PersonBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddTransactionPage = "AddTransactionPage";
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            public const string Accounts = "Accounts";
            public const string ContributionStatementDetailPage = "ContributionStatementDetailPage";
            public const string MaxYearsToDisplay = "MaxYearsToDisplay";
            public const string PledgeDetailPage = "PledgeDetailPage";
            public const string ScheduledTransactionDetailPage = "ScheduledTransactionDetailPage";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var isVisible = ( Person != null && Person.Id != 0 );
            pnlContent.Visible = isVisible;
            if ( isVisible )
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddTransaction_Click( object sender, EventArgs e )
        {
            var addTransactionPage = new Rock.Web.PageReference( this.GetAttributeValue( AttributeKey.AddTransactionPage ) );
            if ( addTransactionPage != null )
            {
                // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
                var personKey = this.Person.GetImpersonationToken( RockDateTime.Now.AddMinutes( this.GetAttributeValue( AttributeKey.PersonTokenExpireMinutes ).AsIntegerOrNull() ?? 60 ), this.GetAttributeValue( AttributeKey.PersonTokenUsageLimit ).AsIntegerOrNull(), addTransactionPage.PageId );
                addTransactionPage.QueryString["Person"] = personKey;
                Response.Redirect( addTransactionPage.BuildUrl() );

            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddScheduledTransaction_Click( object sender, EventArgs e )
        {
            // just send them the same page as Add Transaction page since you can add a scheduled transaction there either way
            btnAddTransaction_Click( sender, e );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPledges_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialPledge = e.Item.DataItem as FinancialPledge;
            if ( financialPledge != null )
            {
                var lPledgeDate = e.Item.FindControl( "lPledgeDate" ) as Literal;
                if ( financialPledge.StartDate != DateTime.MinValue.Date && financialPledge.EndDate != DateTime.MaxValue.Date )
                {
                    lPledgeDate.Text = string.Format(
                        "{0} {1}",
                        financialPledge.StartDate.ToShortDateString(),
                        financialPledge.EndDate.Humanize( true, financialPledge.StartDate, null ) );
                }
                else if ( financialPledge.StartDate == DateTime.MinValue.Date && financialPledge.EndDate != DateTime.MaxValue.Date )
                {
                    lPledgeDate.Text = string.Format( "Till {0}", financialPledge.EndDate.ToShortDateString() );
                }
                else if ( financialPledge.StartDate != DateTime.MinValue.Date && financialPledge.EndDate == DateTime.MaxValue.Date )
                {
                    lPledgeDate.Text = string.Format( "{0} On-Ward", financialPledge.StartDate.ToShortDateString() );
                }
                else
                {
                    lPledgeDate.Text = "No Dates Provided";
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransaction_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialScheduledTransaction = e.Item.DataItem as FinancialScheduledTransaction;
            if ( financialScheduledTransaction != null )
            {
                var lAccounts = e.Item.FindControl( "lAccounts" ) as Literal;
                var lNextPaymentDate = e.Item.FindControl( "lNextPaymentDate" ) as Literal;

                if ( lAccounts != null && lNextPaymentDate != null && financialScheduledTransaction != null )
                {
                    lNextPaymentDate.Text = financialScheduledTransaction.NextPaymentDate.ToShortDateString();

                    var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                    var summary = financialScheduledTransaction.ScheduledTransactionDetails
                        .Select( d => new
                        {
                            IsOther = accountGuids.Any() && !accountGuids.Contains( d.Account.Guid ),
                            Order = d.Account.Order,
                            Name = d.Account.Name,
                        } )
                        .OrderBy( d => d.IsOther )
                        .ThenBy( d => d.Order )
                        .Select( d => string.Format( "{0}",
                            !d.IsOther ? d.Name : "Other" ) )
                        .ToList();

                    if ( summary.Any() )
                    {
                        lAccounts.Text = summary.AsDelimited( ", " );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rStatements control.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void rStatements_ItemDataBound( Object Sender, RepeaterItemEventArgs e )
        {
            var year = e.Item.DataItem as int?;
            var lbYear = e.Item.FindControl( "lbYear" ) as LinkButton;

            if ( year.HasValue && lbYear != null )
            {
                string yearStr = year.ToStringSafe();
                if ( year == RockDateTime.Now.Year )
                {
                    yearStr = yearStr + " <small>YTD</small>";
                }
                lbYear.Text = yearStr;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rStatements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rStatements_ItemCommand( object sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Select" )
            {
                var statementYear = e.CommandArgument.ToString();
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.Add( "StatementYear", statementYear );
                NavigateToLinkedPage( AttributeKey.ContributionStatementDetailPage, queryParams );
            }
        }

        /// <summary>
        /// Event when the user clicks to delete the saved accounts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptSavedAccounts_Delete( object sender, CommandEventArgs e )
        {
            var bankAccountGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var financialPersonSavedAccount = financialPersonSavedAccountService.Get( bankAccountGuid );

            if ( financialPersonSavedAccount != null )
            {
                string errorMessage;
                if ( !financialPersonSavedAccountService.CanDelete( financialPersonSavedAccount, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                financialPersonSavedAccountService.Delete( financialPersonSavedAccount );
                rockContext.SaveChanges();
            }

            ShowDetail();
        }


        /// <summary>
        /// Handles the Click event of the lbAddScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddScheduledTransaction_Click( object sender, EventArgs e )
        {
            // just send them the same page as Add Transaction page since you can add a scheduled transaction there either way
            btnAddTransaction_Click( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the lbAddPledge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPledge_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.AddOrReplace( "PledgeId", "0" );
            queryParams.AddOrReplace( "PersonGuid", Person.Guid.ToString() );
            NavigateToLinkedPage( AttributeKey.PledgeDetailPage, queryParams );
        }

        /// <summary>
        /// Event when the user clicks to delete the scheduled transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptScheduledTransaction_Delete( object sender, CommandEventArgs e )
        {
            var scheduledTransactionGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionGuid );

            if ( financialScheduledTransaction != null )
            {
                string errorMessage;
                if ( !financialScheduledTransactionService.CanDelete( financialScheduledTransaction, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                financialScheduledTransactionService.Delete( financialScheduledTransaction );
                rockContext.SaveChanges();
            }

            ShowDetail();
        }

        /// <summary>
        /// Event when the user clicks to edit the scheduled transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptScheduledTransaction_Edit( object sender, CommandEventArgs e )
        {
            var scheduledTransactionGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionGuid );

            if ( financialScheduledTransaction != null )
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.AddOrReplace( "ScheduledTransactionId", financialScheduledTransaction.Id.ToString() );
                queryParams.AddOrReplace( "PersonGuid", Person.Guid.ToString() );
                NavigateToLinkedPage( AttributeKey.ScheduledTransactionDetailPage, queryParams );
            }
        }

        /// <summary>
        /// Event when the user clicks to edit the pledge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptPledges_Edit( object sender, CommandEventArgs e )
        {
            var pledgeGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledge = pledgeService.Get( pledgeGuid );
            if ( pledge != null )
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.AddOrReplace( "PledgeId", pledge.Id.ToString() );
                queryParams.AddOrReplace( "PersonGuid", Person.Guid.ToString() );
                NavigateToLinkedPage( AttributeKey.PledgeDetailPage, queryParams );
            }
        }

        /// <summary>
        /// Event when the user clicks to delete the pledge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptPledges_Delete( object sender, CommandEventArgs e )
        {
            var pledgeGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledge = pledgeService.Get( pledgeGuid );

            string errorMessage;

            if ( pledge == null )
            {
                return;
            }

            if ( !pledgeService.CanDelete( pledge, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            pledgeService.Delete( pledge );
            rockContext.SaveChanges();

            ShowDetail();
        }

        #endregion Base Control Methods

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            BindSavedAccountList();
            BindContributionStatement();
            BindScheduledTransactions();
            BindPledgeList();
        }

        private void BindScheduledTransactions()
        {
            var rockContext = new RockContext();
            var qry = new FinancialScheduledTransactionService( rockContext )
                .Queryable( "ScheduledTransactionDetails,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                .AsNoTracking();

            // Valid Accounts
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            qry = qry.Where( t => t.IsActive );

            if ( Person.GivingGroupId.HasValue )
            {
                //  Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == Person.GivingGroupId );
            }
            else
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == Person.Id );
            }

            qry = qry
                .OrderBy( t => t.AuthorizedPersonAlias.Person.LastName )
                .ThenBy( t => t.AuthorizedPersonAlias.Person.NickName )
                .ThenByDescending( t => t.IsActive )
                .ThenByDescending( t => t.StartDate );

            var scheduledTransactionList = qry.ToList();

            rptScheduledTransaction.DataSource = scheduledTransactionList;
            rptScheduledTransaction.DataBind();
        }

        private void BindSavedAccountList()
        {
            var rockContext = new RockContext();
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var savedAccountList = financialPersonSavedAccountService
                .Queryable()
                .Where( a => a.PersonAliasId == this.Person.PrimaryAliasId.Value && a.FinancialPaymentDetail != null )
                .ToList();

            rptSavedAccounts.DataSource = savedAccountList;
            rptSavedAccounts.DataBind();

            pnlSavedAccounts.Visible = savedAccountList.Any();
        }

        private void BindPledgeList()
        {
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledgesQry = pledgeService.Queryable();
            pledgesQry = pledgesQry.Where( p => p.PersonAlias.Person.GivingId == Person.GivingId );

            // Filter by configured limit accounts if specified.
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                pledgesQry = pledgesQry.Where( p => accountGuids.Contains( p.Account.Guid ) );
            }

            var pledges = pledgesQry.ToList();
            rptPledges.DataSource = pledges;
            rptPledges.DataBind();
        }

        private void BindContributionStatement()
        {
            var numberOfYears = GetAttributeValue( AttributeKey.MaxYearsToDisplay ).AsInteger();

            var rockContext = new RockContext();
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == Person.GivingId ).Select( a => a.Id ).ToList();

            // get the transactions for the person or all the members in the person's giving group (Family)
            var qry = financialTransactionDetailService.Queryable().AsNoTracking().Where( t =>
                t.Transaction.AuthorizedPersonAliasId.HasValue
                && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                && t.Transaction.TransactionDateTime.HasValue );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Accounts ) ) )
            {
                qry = qry.Where( t => t.Account.IsTaxDeductible );
            }
            else
            {
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).Split( ',' ).Select( Guid.Parse ).ToList();
                qry = qry.Where( t => accountGuids.Contains( t.Account.Guid ) );
            }

            var yearQry = qry.GroupBy( t => t.Transaction.TransactionDateTime.Value.Year )
                                .Select( g => g.Key )
                                .OrderByDescending( y => y );

            var statementYears = yearQry.Take( numberOfYears ).ToList();

            rStatements.DataSource = statementYears;
            rStatements.DataBind();

            pnlStatement.Visible = statementYears.Any();
        }

        #endregion Methods
    }

}