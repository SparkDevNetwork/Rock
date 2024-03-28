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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Humanizer;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block used to view the scheduled transactions, saved accounts and pledges of a person.
    /// </summary>
    [DisplayName( "Giving Configuration" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the scheduled transactions, saved accounts and pledges of a person." )]

    #region Block Attributes

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
        DefaultValue = Rock.SystemGuid.Page.CONTRIBUTION_STATEMENT_PAGE,
        Key = AttributeKey.ContributionStatementDetailPage )]

    [LinkedPage(
        "Scheduled Transaction Detail Page",
        Key = AttributeKey.ScheduledTransactionDetailPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.SCHEDULED_TRANSACTION,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "486E470A-DBD8-48D6-9A97-5B1B490A401E" )]
    public partial class GivingConfiguration : Rock.Web.UI.PersonBlock, ISecondaryBlock
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

        private static class PageParameterKey
        {
            public const string ScheduledTransactionGuid = "ScheduledTransactionGuid";
            public const string PersonActionIdentifier = "rckid";
            public const string PledgeId = "PledgeId";
            public const string StatementYear = "StatementYear";
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

            var isVisible = Person != null && Person.Id != 0;
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
            var addTransactionPage = this.GetAttributeValue( AttributeKey.AddTransactionPage );
            if ( addTransactionPage == null )
            {
                return;
            }

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.AddOrReplace( PageParameterKey.PersonActionIdentifier, Person.GetPersonActionIdentifier( "transaction" ) );
            NavigateToLinkedPage( AttributeKey.AddTransactionPage, queryParams );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPledges_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialPledge = e.Item.DataItem as FinancialPledge;
            if ( financialPledge == null )
            {
                return;
            }

            var lPledgeAccountName = e.Item.FindControl( "lPledgeAccountName" ) as Literal;
            var lPledgeTotalAmount = e.Item.FindControl( "lPledgeTotalAmount" ) as Literal;
            var lPledgeFrequency = e.Item.FindControl( "lPledgeFrequency" ) as Literal;
            var btnPledgeEdit = e.Item.FindControl( "btnPledgeEdit" ) as LinkButton;
            var btnPledgeDelete = e.Item.FindControl( "btnPledgeDelete" ) as LinkButton;

            lPledgeAccountName.Text = financialPledge.Account?.Name;
            lPledgeTotalAmount.Text = financialPledge.TotalAmount.FormatAsCurrency();
            lPledgeFrequency.Text = financialPledge.PledgeFrequencyValue != null ? ( "<span class='o-30'>|</span> " + financialPledge.PledgeFrequencyValue.ToString() ) : string.Empty;
            btnPledgeEdit.CommandArgument = financialPledge.Guid.ToString();
            btnPledgeDelete.CommandArgument = financialPledge.Guid.ToString();

            /* Show Text for Pledge StartDate, etc */
            var lPledgeDate = e.Item.FindControl( "lPledgeDate" ) as Literal;
            if ( financialPledge.StartDate != DateTime.MinValue.Date && financialPledge.EndDate != DateTime.MaxValue.Date )
            {
                var pledgeTimeSpan = financialPledge.StartDate - financialPledge.EndDate;
                lPledgeDate.Text = string.Format(
                    "{0} {1}",
                    financialPledge.StartDate.ToShortDateString(),
                    pledgeTimeSpan.Humanize() );
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

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransaction_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialScheduledTransaction = e.Item.DataItem as FinancialScheduledTransaction;
            if ( financialScheduledTransaction == null )
            {
                return;
            }

            var financialPaymentDetail = financialScheduledTransaction.FinancialPaymentDetail;

            var btnScheduledTransactionEdit = e.Item.FindControl( "btnScheduledTransactionEdit" ) as LinkButton;
            btnScheduledTransactionEdit.CommandArgument = financialScheduledTransaction.Guid.ToString();

            var btnScheduledTransactionInactivate = e.Item.FindControl( "btnScheduledTransactionInactivate" ) as LinkButton;
            btnScheduledTransactionInactivate.CommandArgument = financialScheduledTransaction.Guid.ToString();

            if ( financialScheduledTransaction.IsActive && financialScheduledTransaction.FinancialGateway.GetGatewayComponent().UpdateScheduledPaymentSupported )
            {
                btnScheduledTransactionInactivate.Visible = true;
            }
            else
            {
                btnScheduledTransactionInactivate.Visible = false;
            }

            var lScheduledTransactionAccountName = e.Item.FindControl( "lScheduledTransactionAccountName" ) as Literal;
            var lScheduledTransactionFrequencyAndNextPaymentDate = e.Item.FindControl( "lScheduledTransactionFrequencyAndNextPaymentDate" ) as Literal;
            var lScheduledTransactionTotalAmount = e.Item.FindControl( "lScheduledTransactionTotalAmount" ) as Literal;

            var lScheduledTransactionCardTypeLast4 = e.Item.FindControl( "lScheduledTransactionCardTypeLast4" ) as Literal;
            var lScheduledTransactionExpiration = e.Item.FindControl( "lScheduledTransactionExpiration" ) as Literal;
            var lScheduledTransactionSavedAccountName = e.Item.FindControl( "lScheduledTransactionSavedAccountName" ) as Literal;
            var lScheduledTransactionStatusHtml = e.Item.FindControl( "lScheduledTransactionStatusHtml" ) as Literal;
            var pnlCreditCardInfo = e.Item.FindControl( "pnlScheduledTransactionCreditCardInfo" ) as Panel;
            var lOtherCurrencyTypeInfo = e.Item.FindControl( "lScheduledTransactionOtherCurrencyTypeInfo" ) as Literal;

            string currencyType = financialPaymentDetail?.CurrencyTypeValue.Value;
            string creditCardType = null;
            string accountNumberMasked = financialPaymentDetail?.AccountNumberMasked;
            if ( financialPaymentDetail?.CreditCardTypeValueId != null )
            {
                creditCardType = DefinedValueCache.GetValue( financialPaymentDetail.CreditCardTypeValueId.Value );
            }

            var currencyTypeIdCreditCard = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var currencyTypeIdApplePay = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY.AsGuid() );
            var currencyTypeIdAndroidPay = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY.AsGuid() );

            if ( financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdCreditCard )
            {
                pnlCreditCardInfo.Visible = true;
                lOtherCurrencyTypeInfo.Visible = false;
                lScheduledTransactionCardTypeLast4.Text = FormatAccountTypeWithLast4( creditCardType, accountNumberMasked );
                lScheduledTransactionExpiration.Text = $"Exp: {financialPaymentDetail.ExpirationDate}";
            }
            else if ( financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdApplePay
                        || financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdAndroidPay )
            {
                // If the currency type is Apple Pay or Android Pay, use the currency type value.
                pnlCreditCardInfo.Visible = true;
                lOtherCurrencyTypeInfo.Visible = false;
                lScheduledTransactionCardTypeLast4.Text = FormatAccountTypeWithLast4( currencyType, accountNumberMasked );
                lScheduledTransactionExpiration.Text = $"Exp: {financialPaymentDetail.ExpirationDate}";
            }
            else
            {
                pnlCreditCardInfo.Visible = false;
                lOtherCurrencyTypeInfo.Visible = true;
                lOtherCurrencyTypeInfo.Text = currencyType;
            }

            if ( financialPaymentDetail?.FinancialPersonSavedAccount != null )
            {
                var savedAccount = financialPaymentDetail.FinancialPersonSavedAccount;
                lScheduledTransactionSavedAccountName.Text = savedAccount.Name;

                if ( savedAccount.LastErrorCode.IsNotNullOrWhiteSpace() )
                {
                    var errorStatus = $"The associated account has {savedAccount.LastErrorCode} on {savedAccount.LastErrorCodeDateTime.ToShortDateString()}.";
                    lScheduledTransactionStatusHtml.Text = $"<span class='text-xs text-danger text-nowrap' data-toggle='tooltip' data-placement='auto' data-container='body' title data-original-title='{errorStatus}'>Error</span>";
                }

            }

            var frequencyText = DefinedValueCache.GetValue( financialScheduledTransaction.TransactionFrequencyValueId );
            var nextPaymentDate = financialScheduledTransaction.NextPaymentDate;

            if ( financialScheduledTransaction.IsActive )
            {
                if ( nextPaymentDate.HasValue )
                {
                    lScheduledTransactionFrequencyAndNextPaymentDate.Text = $"{frequencyText} <span class='o-30'>|</span> Next Gift {nextPaymentDate.ToShortDateString()}";
                }
                else
                {
                    lScheduledTransactionFrequencyAndNextPaymentDate.Text = $"{frequencyText}";
                }
            }
            else
            {
                lScheduledTransactionFrequencyAndNextPaymentDate.Text = $"{frequencyText}";
                lScheduledTransactionStatusHtml.Text = "<span class='text-xs text-warning text-nowrap'>Inactive</span>";
            }

            if ( lScheduledTransactionAccountName != null )
            {
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
                    .Select( d => string.Format( "{0}", !d.IsOther ? d.Name : "Other" ) )
                    .ToList();

                if ( summary.Any() )
                {
                    lScheduledTransactionAccountName.Text = summary.AsDelimited( ", " );
                }
            }

            if ( lScheduledTransactionTotalAmount != null )
            {
                lScheduledTransactionTotalAmount.Text = financialScheduledTransaction.TotalAmount.FormatAsCurrency( financialScheduledTransaction.ForeignCurrencyCodeValueId );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptContributionStatementsYYYY control.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void rptContributionStatementsYYYY_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var year = e.Item.DataItem as int?;
            var btnContributionStatementYYYY = e.Item.FindControl( "btnContributionStatementYYYY" ) as LinkButton;

            if ( year.HasValue && btnContributionStatementYYYY != null )
            {
                string yearHtml;
                if ( year == RockDateTime.Now.Year )
                {
                    yearHtml = $"{year} <small>YTD</small>";
                }
                else
                {
                    yearHtml = year.ToString();
                }

                btnContributionStatementYYYY.Text = yearHtml;
                btnContributionStatementYYYY.CommandArgument = year.ToString();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptContributionStatementsYYYY control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptContributionStatementsYYYY_ItemCommand( object sender, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Select" )
            {
                var statementYear = e.CommandArgument.ToString();
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                queryParams.AddOrReplace( PageParameterKey.PersonActionIdentifier, Person.GetPersonActionIdentifier( "contribution-statement" ) );
                queryParams.AddOrReplace( PageParameterKey.StatementYear, statementYear );
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
                    mdWarningAlert.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                financialPersonSavedAccountService.Delete( financialPersonSavedAccount );
                rockContext.SaveChanges();
            }

            ShowDetail();

            // Clean up stale accounts from the Text-To-Give settings area.
            BindSavedAccounts();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSavedAccounts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialPersonSavedAccount = e.Item.DataItem as FinancialPersonSavedAccount;
            if ( financialPersonSavedAccount == null )
            {
                return;
            }

            var lSavedAccountName = e.Item.FindControl( "lSavedAccountName" ) as Literal;

            var pnlCreditCardInfo = e.Item.FindControl( "pnlSavedAccountCreditCardInfo" ) as Panel;
            var lOtherCurrencyTypeInfo = e.Item.FindControl( "lSavedAccountOtherCurrencyTypeInfo" ) as Literal;

            var lSavedAccountCardTypeLast4 = e.Item.FindControl( "lSavedAccountCardTypeLast4" ) as Literal;
            var lSavedAccountExpiration = e.Item.FindControl( "lSavedAccountExpiration" ) as Literal;
            var lSavedAccountStatusHtml = e.Item.FindControl( "lSavedAccountStatusHtml" ) as Literal;
            var btnSavedAccountDelete = e.Item.FindControl( "btnSavedAccountDelete" ) as LinkButton;

            lSavedAccountName.Text = financialPersonSavedAccount.Name;
            var financialPaymentDetail = financialPersonSavedAccount.FinancialPaymentDetail;

            string currencyType = financialPaymentDetail?.CurrencyTypeValue.Value;
            string creditCardType = null;
            string accountNumberMasked = financialPaymentDetail?.AccountNumberMasked;
            if ( financialPaymentDetail?.CreditCardTypeValueId != null )
            {
                creditCardType = DefinedValueCache.GetValue( financialPaymentDetail.CreditCardTypeValueId.Value );
            }

            // Collect the currency types that are allowed to be listed with saved account name and expiration date.
            var currencyTypeIdCreditCard = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var currencyTypeIdApplePay = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY.AsGuid() );
            var currencyTypeIdAndroidPay = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY.AsGuid() );

            if ( financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdCreditCard )
            {
                pnlCreditCardInfo.Visible = true;
                lOtherCurrencyTypeInfo.Visible = false;
                lSavedAccountCardTypeLast4.Text = FormatAccountTypeWithLast4( creditCardType, accountNumberMasked );
                lSavedAccountExpiration.Text = $"Exp: {financialPaymentDetail.ExpirationDate}";
            }
            else if ( financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdApplePay
                        || financialPaymentDetail?.CurrencyTypeValueId == currencyTypeIdAndroidPay )
            {
                // If the currency type is Apple Pay or Android Pay, use the currency type value.
                pnlCreditCardInfo.Visible = true;
                lOtherCurrencyTypeInfo.Visible = false;
                lSavedAccountCardTypeLast4.Text = FormatAccountTypeWithLast4( currencyType, accountNumberMasked );
                lSavedAccountExpiration.Text = $"Exp: {financialPaymentDetail.ExpirationDate}";
            }
            else
            {
                pnlCreditCardInfo.Visible = false;
                lOtherCurrencyTypeInfo.Visible = true;
                lOtherCurrencyTypeInfo.Text = currencyType;
            }

            var cardIsExpired = financialPaymentDetail.CardExpirationDate.HasValue && financialPaymentDetail.CardExpirationDate.Value < RockDateTime.Now;

            var cardInUse = new FinancialScheduledTransactionService( new RockContext() ).Queryable().Where( a => a.FinancialPaymentDetailId.HasValue
                && a.IsActive
                && a.FinancialPaymentDetail.FinancialPersonSavedAccountId.HasValue
                && a.FinancialPaymentDetail.FinancialPersonSavedAccountId.Value == financialPersonSavedAccount.Id ).Any();

            btnSavedAccountDelete.Visible = !cardInUse && !financialPersonSavedAccount.IsDefault;
            btnSavedAccountDelete.CommandArgument = financialPersonSavedAccount.Guid.ToString();

            if ( cardIsExpired )
            {
                lSavedAccountStatusHtml.Text = "<span class='text-xs text-danger text-nowrap'>Expired</span>";
            }
            else
            {
                if ( financialPersonSavedAccount.LastErrorCode.IsNotNullOrWhiteSpace() )
                {
                    var errorStatus = $" {financialPersonSavedAccount.LastErrorCode} on {financialPersonSavedAccount.LastErrorCodeDateTime.ToShortDateString()}.";
                    lSavedAccountStatusHtml.Text = $"<span class='text-xs text-danger text-nowrap' data-toggle='tooltip' data-placement='auto' data-container='body' title data-original-title='{errorStatus}'>Error</span>";
                }
                else if ( cardInUse )
                {
                    lSavedAccountStatusHtml.Text = "<span class='text-xs text-success text-nowrap'>In Use</span>";
                }
                else if ( financialPersonSavedAccount.IsDefault )
                {
                    lSavedAccountStatusHtml.Text = "<span class='text-xs text-muted text-nowrap'>Default</span>";
                }
                else
                {
                    lSavedAccountStatusHtml.Text = "<span class='text-xs text-muted text-nowrap'>Not In Use</span>";
                }
            }
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
            queryParams.AddOrReplace( PageParameterKey.PledgeId, "0" );
            queryParams.AddOrReplace( PageParameterKey.PersonActionIdentifier, Person.GetPersonActionIdentifier( "pledge" ) );
            NavigateToLinkedPage( AttributeKey.PledgeDetailPage, queryParams );
        }

        /// <summary>
        /// Event when the user clicks to delete (but really inactivates) the scheduled transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptScheduledTransaction_Inactivate( object sender, CommandEventArgs e )
        {
            var scheduledTransactionGuid = e.CommandArgument.ToStringSafe().AsGuid();
            var rockContext = new RockContext();
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialScheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionGuid );
            if ( financialScheduledTransaction?.FinancialGateway == null )
            {
                return;
            }

            string errorMessage;

            /* 2021-08-27 MDP

            We really don't want to actually delete a FinancialScheduledTransaction.
            Just inactivate it, even if there aren't FinancialTransactions associated with it.
            It is possible the the Gateway has processed a transaction on it that Rock doesn't know about yet.
            If that happens, Rock won't be able to match a record for that downloaded transaction!
            We also might want to match inactive or "deleted" schedules on the Gateway to a person in Rock,
            so we'll need the ScheduledTransaction to do that.

            So, don't delete ScheduledTransactions.

            However, if ScheduledTransaction does not currently have any FinancialTransactions associated with it,
            we can *say* we are deleting it in the messages. Also, when doing a 'Show Inactive Scheduled Transactions'
            we won't list Scheduled Transactions that are Inactive AND don't currently have financial transactions
            associated with it. If a transactions come in later, then we'll end up showing it as an inactive scheduled
            transaction again.

            */

            if ( financialScheduledTransactionService.Cancel( financialScheduledTransaction, out errorMessage ) )
            {
                try
                {
                    financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out errorMessage );
                }
                catch
                {
                    // Ignore
                }

                rockContext.SaveChanges();
            }
            else
            {
                mdWarningAlert.Show( errorMessage, ModalAlertType.Information );
            }

            ShowDetail();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnShowInactiveScheduledTransactions_Click( object sender, EventArgs e )
        {
            // toggle showing Inactive scheduled transactions
            hfShowInactiveScheduledTransactions.Value = ( !hfShowInactiveScheduledTransactions.Value.AsBoolean() ).ToTrueFalse();
            BindScheduledTransactions();
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
                queryParams.AddOrReplace( PageParameterKey.ScheduledTransactionGuid, financialScheduledTransaction.Guid.ToString() );
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
                queryParams.AddOrReplace( PageParameterKey.PledgeId, pledge.Id.ToString() );
                queryParams.AddOrReplace( PageParameterKey.PersonActionIdentifier, Person.GetPersonActionIdentifier( "pledge" ) );
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
                mdWarningAlert.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            pledgeService.Delete( pledge );
            rockContext.SaveChanges();

            ShowDetail();
        }

        protected void btnEditTextToGive_Click( object sender, EventArgs e )
        {
            pnlTextToGive.Visible = true;
            btnEditTextToGive.Visible = false;
            pnlTextToGiveView.Visible = false;
            pnlTextToGiveEdit.Visible = true;

            BindSavedAccounts();

            var financialAccount = GetDefaultFinancialAccount();
            apAccountPicker.SetValue( financialAccount );

            var defaultSavedAccount = GetDefaultSavedAccount();
            ddlSavedAccount.AddCssClass( "text-sm" );
            ddlSavedAccount.SelectedValue = defaultSavedAccount == null ? string.Empty : defaultSavedAccount.Id.ToString();
        }

        protected void btnSaveTextToGive_Click( object sender, EventArgs e )
        {
            var selectedSavedAccountId = ddlSavedAccount.SelectedValueAsInt();
            var selectedFinancialAccountId = apAccountPicker.SelectedValueAsInt();

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.ConfigureTextToGive( Person.Id, selectedFinancialAccountId, selectedSavedAccountId, out _ );
                rockContext.SaveChanges();
                Person = personService.Get( Person.Id );
            }

            btnEditTextToGive.Visible = true;
            pnlTextToGiveView.Visible = true;
            pnlTextToGiveEdit.Visible = false;
            SetTextToGiveDetails();

            // Editting Text-To-Give settings may affect the display of saved accounts, so rebind this.
            BindSavedAccountList();
        }

        protected void btnCanceTextToGive_Click( object sender, EventArgs e )
        {
            btnEditTextToGive.Visible = true;
            pnlTextToGiveView.Visible = true;
            pnlTextToGiveEdit.Visible = false;
            SetTextToGiveDetails();
        }

        #endregion Base Control Methods

        #region Internal Methods

        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            if ( !Page.IsPostBack )
            {
                SetTextToGiveDetails();
            }

            BindSavedAccountList();
            BindContributionStatements();
            BindScheduledTransactions();
            BindPledgeList();
        }

        /// <summary>
        /// Set the Text-To-Give Details
        /// </summary>
        private void SetTextToGiveDetails()
        {
            if ( !Person.ContributionFinancialAccountId.HasValue )
            {
                // Don't show anything.
                pnlTextToGive.Visible = false;
                btnEditTextToGive.Visible = false;
                pnlTextToGiveAddSettings.Visible = true;
                return;
            }

            pnlTextToGive.Visible = true;

            var financialAccount = GetDefaultFinancialAccount();
            lTTGDefaultAccount.Text = financialAccount == null ? "None" : financialAccount.PublicName;

            var defaultSavedAccount = GetDefaultSavedAccount();
            lTTGSavedAccount.Text = defaultSavedAccount == null ? "None" : GetSavedAccountName( defaultSavedAccount );
        }

        /// <summary>
        /// Populate the appropriate saved accounts for the person and gateway in the drop down list
        /// </summary>
        private void BindSavedAccounts()
        {
            var selectedId = ddlSavedAccount.SelectedValue.AsIntegerOrNull();
            ddlSavedAccount.Items.Clear();

            // Get the saved accounts for the person.
            var savedAccounts = GetSavedAccounts();

            // Bind the accounts.
            if ( savedAccounts != null && savedAccounts.Any() )
            {
                var savedAccountDataItems = savedAccounts.Select( sa => new
                {
                    Id = ( int? ) sa.Id,
                    Name = GetSavedAccountName( sa )
                } ).ToList();

                // Add a blank option to unset the default account altogether.
                savedAccountDataItems.Insert( 0, new
                {
                    Id = ( int? ) null,
                    Name = string.Empty,
                } );

                ddlSavedAccount.DataSource = savedAccountDataItems;
                ddlSavedAccount.Enabled = true;
            }
            else
            {
                ddlSavedAccount.Enabled = false;
                ddlSavedAccount.DataSource = new List<object>
                {
                    new {
                        Name = "No Saved Accounts",
                        Id = (int?) null
                    }
                };
            }

            ddlSavedAccount.DataBind();

            // Try to select the previously selected account
            if ( selectedId.HasValue && savedAccounts.Any( sa => sa.Id == selectedId ) )
            {
                ddlSavedAccount.SelectedValue = selectedId.Value.ToString();
            }
        }

        /// <summary>
        /// Get the name of the saved account
        /// </summary>
        /// <param name="savedAccount"></param>
        /// <returns></returns>
        private string GetSavedAccountName( FinancialPersonSavedAccount savedAccount )
        {
            const string unnamed = "<Unnamed>";

            if ( savedAccount == null )
            {
                return unnamed;
            }

            var name = savedAccount.Name.IsNullOrWhiteSpace() ? unnamed : savedAccount.Name.Trim();

            if ( savedAccount.FinancialPaymentDetail != null )
            {
                var expirationMonth = savedAccount.FinancialPaymentDetail.ExpirationMonth;
                var expirationYear = savedAccount.FinancialPaymentDetail.ExpirationYear;

                if ( expirationMonth.HasValue || expirationYear.HasValue )
                {
                    var monthString = expirationMonth.HasValue ?
                        ( expirationMonth.Value < 10 ? ( "0" + expirationMonth.Value.ToString() ) : expirationMonth.Value.ToString() ) :
                        "??";
                    var yearString = expirationYear.HasValue ?
                        ( expirationYear.Value % 100 ).ToString() :
                        "??";

                    name += string.Format( " ({0}/{1})", monthString, yearString );
                }
            }

            return name;
        }

        /// <summary>
        /// Gets the saved accounts.
        /// </summary>
        /// <returns></returns>
        private List<FinancialPersonSavedAccount> GetSavedAccounts()
        {
            if ( _savedAccounts != null )
            {
                return _savedAccounts;
            }

            if ( Person == null )
            {
                return new List<FinancialPersonSavedAccount>();
            }

            var supportedGatewayIds = GetSupportedGatewayIds();
            if ( supportedGatewayIds == null || !supportedGatewayIds.Any() )
            {
                return new List<FinancialPersonSavedAccount>();
            }

            var rockContext = new RockContext();
            var service = new FinancialPersonSavedAccountService( rockContext );

            _savedAccounts = service
                .GetByPersonId( Person.Id )
                .Include( sa => sa.FinancialPaymentDetail )
                .AsNoTracking()
                .Where( sa =>
                    sa.FinancialGatewayId.HasValue &&
                    supportedGatewayIds.Contains( sa.FinancialGatewayId.Value ) )
                .OrderBy( sa => sa.IsDefault )
                .ThenByDescending( sa => sa.CreatedDateTime )
                .ToList();

            return _savedAccounts;
        }
        private List<FinancialPersonSavedAccount> _savedAccounts = null;

        /// <summary>
        /// Gets the default saved account.
        /// </summary>
        /// <returns></returns>
        private FinancialPersonSavedAccount GetDefaultSavedAccount()
        {
            var savedAccounts = GetSavedAccounts();
            return savedAccounts == null ? null : savedAccounts.FirstOrDefault( sa => sa.IsDefault );
        }

        /// <summary>
        /// Gets the default financial account.
        /// </summary>
        /// <returns></returns>
        private FinancialAccount GetDefaultFinancialAccount()
        {
            return Person == null ? null : Person.ContributionFinancialAccount;
        }

        /// <summary>
        /// Gets the supported gateway ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSupportedGatewayIds()
        {
            if ( _supportedGatewayIds == null )
            {
                var rockContext = new RockContext();
                var gatewayService = new FinancialGatewayService( rockContext );
                var activeGatewayEntityTypes = gatewayService.Queryable( "EntityType" ).AsNoTracking()
                    .Where( fg => fg.IsActive )
                    .GroupBy( fg => fg.EntityType )
                    .ToList();

                var supportedTypes = Rock.Reflection.FindTypes( typeof( IAutomatedGatewayComponent ) );
                _supportedGatewayIds = new List<int>();

                foreach ( var entityType in activeGatewayEntityTypes )
                {
                    if ( supportedTypes.Any( t => t.Value.FullName == entityType.Key.Name ) )
                    {
                        _supportedGatewayIds.AddRange( entityType.Select( fg => fg.Id ) );
                    }
                }
            }

            return _supportedGatewayIds;
        }
        private List<int> _supportedGatewayIds = null;

        /// <summary>
        /// Binds the scheduled transactions.
        /// </summary>
        private void BindScheduledTransactions()
        {
            var rockContext = new RockContext();
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var qry = financialScheduledTransactionService
                .Queryable( "ScheduledTransactionDetails,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                .AsNoTracking();

            // Valid Accounts
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            if ( Person.GivingGroupId.HasValue )
            {
                // Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == Person.GivingGroupId );
            }
            else
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == Person.Id );
            }

            // Only show the button if there some inactive scheduled transactions.
            // 12-JAN-22 DMV: This adds a small performance hit here as this hydrates the query.
            btnShowInactiveScheduledTransactions.Visible = qry.Any( a => !a.IsActive );

            var includeInactive = hfShowInactiveScheduledTransactions.Value.AsBoolean();
            if ( !includeInactive )
            {
                btnShowInactiveScheduledTransactions.Text = "Show Inactive";
                qry = qry.Where( t => t.IsActive );
            }
            else
            {
                // If including Inactive, show both Active and Inactive.
                btnShowInactiveScheduledTransactions.Text = "Hide Inactive";
            }

            qry = qry
                .OrderBy( t => t.AuthorizedPersonAlias.Person.LastName )
                .ThenBy( t => t.AuthorizedPersonAlias.Person.NickName )
                .ThenByDescending( t => t.IsActive )
                .ThenByDescending( t => t.StartDate );

            var scheduledTransactionList = qry.ToList();

            // Refresh the active transactions.
            financialScheduledTransactionService.GetStatus( scheduledTransactionList, true );

            rptScheduledTransaction.DataSource = scheduledTransactionList;
            rptScheduledTransaction.DataBind();
        }

        /// <summary>
        /// Binds the saved account list.
        /// </summary>
        private void BindSavedAccountList()
        {
            var rockContext = new RockContext();
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var savedAccountList = financialPersonSavedAccountService
                .GetByPersonId( this.Person.Id )
                .Where( a => a.FinancialPaymentDetail != null )
                .ToList();

            rptSavedAccounts.DataSource = savedAccountList;
            rptSavedAccounts.DataBind();

            pnlSavedAccounts.Visible = savedAccountList.Any();
        }

        /// <summary>
        /// Binds the pledge list.
        /// </summary>
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

        /// <summary>
        /// Binds the contribution statements.
        /// </summary>
        private void BindContributionStatements()
        {
            var numberOfYears = GetAttributeValue( AttributeKey.MaxYearsToDisplay ).AsInteger();

            var rockContext = new RockContext();
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == Person.GivingId ).Select( a => a.Id ).ToList();

            // Get the transactions for the person or all the members in the person's giving group (Family).
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

            rptContributionStatementsYYYY.DataSource = statementYears;
            rptContributionStatementsYYYY.DataBind();

            pnlStatement.Visible = statementYears.Any();
        }

        private string FormatAccountTypeWithLast4( string type, string accountNumberMasked )
        {
            if ( accountNumberMasked.IsNotNullOrWhiteSpace() && accountNumberMasked.Length >= 4 )
            {
                var last4 = accountNumberMasked.Substring( accountNumberMasked.Length - 4 );
                return $"{type} - {last4}";
            }
            else
            {
                return type;
            }
        }

        #endregion Methods
    }
}