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
using RestSharp;
using Rock.StatementGenerator;

namespace RockWeb.Plugins.com_visitgracechurch.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Contribution Statement from Statement Template" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a Lava based contribution statement." )]
    [AccountsField( "Accounts", "A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be uses.", false, order: 0 )]
    [BooleanField( "Display Pledges", "Determines if pledges should be shown.", true, order: 1 )]
    [DefinedValueField( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE , "Statement Template", required: true, order: 2)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Cash Currency Types", "Select the currency types you would like to mark as Non-Cash.", false, true, key:"CashTypes", order: 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Non-Cash Currency Types", "Select the currency types you would like to excluded.", false, true, key:"NonCashTypes", order: 4 )]
    [BooleanField( "Allow Person Querystring", "Determines if any person other than the currently logged in person is allowed to be passed through the querystring. For security reasons this is not allowed by default.", false, order: 5 )]
    public partial class ContributionStatementFromTemplate : Rock.Web.UI.RockBlock
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

            Person targetPerson = CurrentPerson;

            // get excluded currency types setting
            //List<Guid> excludedCurrencyTypes = new List<Guid>();
            //if ( GetAttributeValue( "ExcludedCurrencyTypes" ).IsNotNullOrWhiteSpace() )
            //{
            //    excludedCurrencyTypes = GetAttributeValue( "ExcludedCurrencyTypes" ).Split( ',' ).Select( Guid.Parse ).ToList();
            //}

            List<int> cashCurrencyTypes = new List<int>();
            if ( GetAttributeValue( "CashTypes").IsNotNullOrWhiteSpace() )
            {
                var cashCurrencyTypesGuids = GetAttributeValues( "CashTypes" ).Select( Guid.Parse ).ToList();
                cashCurrencyTypes = new DefinedValueService( rockContext ).GetByGuids( cashCurrencyTypesGuids ).Select( c => c.Id ).ToList();
            }

            List<int> nonCashCurrencyTypes = new List<int>();
            if ( GetAttributeValue( "CashTypes" ).IsNotNullOrWhiteSpace() )
            {
                var nonCashCurrencyTypesGuids = GetAttributeValues( "NonCashTypes" ).Select( Guid.Parse ).ToList();
                nonCashCurrencyTypes = new DefinedValueService( rockContext ).GetByGuids( nonCashCurrencyTypesGuids ).Select( c => c.Id ).ToList();
            }

            var personGuid = Request["PersonGuid"].AsGuidOrNull();
            
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

            // Get Data From Statement Generator

            var statementControler = new Rock.StatementGenerator.Rest.StatementGeneratorFinancialTransactionsController();

            //Set up options object

            Rock.StatementGenerator.StatementGeneratorOptions options = new Rock.StatementGenerator.StatementGeneratorOptions();

            options.StartDate = DateTime.Parse( "01/01/" + statementYear.ToString() );
            options.EndDate = DateTime.Parse( "01/01/" + ( statementYear + 1 ).ToString() );
            options.CurrencyTypeIdsCash = cashCurrencyTypes;
            options.CurrencyTypeIdsNonCash = nonCashCurrencyTypes;

            options.PledgesIncludeChildAccounts = true;
            options.PledgesIncludeNonCashGifts = true;
            options.PersonId = targetPerson.Id;
            options.IncludeIndividualsWithNoAddress = true;
            options.ExcludeInActiveIndividuals = false;

            options.PledgesAccountIds = new List<int>();

            options.LayoutDefinedValueGuid = GetAttributeValue( "StatementTemplate").AsGuidOrNull();

            // Set up person and group Ids

            var layout = DefinedValueCache.Get( GetAttributeValue( "StatementTemplate" ).AsGuid() );

            int groupId = targetPerson.GivingGroupId ?? targetPerson.PrimaryFamilyId ?? -1;
            int? personId = targetPerson.GivingGroupId.HasValue ? (int?)null: targetPerson.Id;  //Only give personId if giving individually

            //var results = statementControler.GetStatementGeneratorRecipientResult( groupId, personId, null, options );
            //var restClient = new RestClient("http://localhost/");
            //var givingRequest = new RestRequest();
            //if ( personId.HasValue )
            //{
            //    givingRequest = new RestRequest( "api/FinancialTransactions/GetStatementGeneratorRecipientResult?groupId=" + groupId.ToString() + "&personId=" + personId.ToString() );
            //}
            //else
            //{
            //    givingRequest = new RestRequest( "api/FinancialTransactions/GetStatementGeneratorRecipientResult?groupId=" + groupId.ToString() );
            //}
            //givingRequest.RequestFormat = RestSharp.DataFormat.Json;
            ////givingRequest.AddJsonBody( options );
            //givingRequest.AddParameter( "application/json", options.ToJson(), ParameterType.RequestBody );

            ////If not using PageParameter, you can use the REST Key. Otherwise use the logged in person's .ROCK auth cookie
            ////Using REST Key allows for everyone to view their own statement.
            //givingRequest.AddCookie( ".ROCK", Request.Cookies[".ROCK"].Value );
            //var response = restClient.Post( givingRequest );
            //var results = response.Content ?? response.ErrorMessage + response.ErrorException;

            //lResults.Text = results.FromJsonOrNull<dynamic>().Html ?? "";

            lResults.Text = GetStatementGeneratorRecipientResult( groupId, personId, null, options, targetPerson );

        }

        #endregion

        #region Helper Methods


        private string GetStatementGeneratorRecipientResult( int groupId, int? personId, Guid? locationGuid, StatementGeneratorOptions options, Person targetPerson )
        {
            if ( options == null )
            {
                throw new Exception( "StatementGenerationOption options must be specified" );
            }

            if ( options.LayoutDefinedValueGuid == null )
            {
                throw new Exception( "LayoutDefinedValueGuid option must be specified" );
            }

            var result = new StatementGeneratorRecipientResult();
            result.GroupId = groupId;
            result.PersonId = personId;

            using ( var rockContext = new RockContext() )
            {
                var financialTransactionQry = this.GetFinancialTransactionQuery( options, rockContext, false );
                var financialPledgeQry = GetFinancialPledgeQuery( options, rockContext, false );

                var personList = new List<Person>();
                Person person = null;
                if ( personId.HasValue )
                {
                    person = new PersonService( rockContext ).Queryable().Include( a => a.Aliases ).Where( a => a.Id == personId.Value ).FirstOrDefault();
                    personList.Add( person );
                }
                else
                {
                    // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    personList = groupMemberService.GetByGroupId( groupId ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.Person ).Include( a => a.Aliases ).ToList();
                    person = personList.FirstOrDefault();
                }

                if ( options.ExcludeOptedOutIndividuals == true && !options.DataViewId.HasValue )
                {
                    int? doNotSendGivingStatementAttributeId = AttributeCache.Get( Rock.StatementGenerator.SystemGuid.Attribute.PERSON_DO_NOT_SEND_GIVING_STATEMENT.AsGuid() ).Id;
                    if ( doNotSendGivingStatementAttributeId.HasValue )
                    {
                        var personIds = personList.Select( a => a.Id ).ToList();
                        var optedOutPersonQry = new AttributeValueService( rockContext ).Queryable().Where( a => a.AttributeId == doNotSendGivingStatementAttributeId );
                        if ( personIds.Count == 1 )
                        {
                            int entityPersonId = personIds[0];
                            optedOutPersonQry = optedOutPersonQry.Where( a => a.EntityId == entityPersonId );
                        }
                        else
                        {
                            optedOutPersonQry = optedOutPersonQry.Where( a => personIds.Contains( a.EntityId.Value ) );
                        }

                        var optedOutPersonIds = optedOutPersonQry
                            .Select( a => new
                            {
                                PersonId = a.EntityId.Value,
                                a.Value
                            } ).ToList().Where( a => a.Value.AsBoolean() == true ).Select( a => a.PersonId ).ToList();

                        if ( optedOutPersonIds.Any() )
                        {
                            bool givingLeaderOptedOut = personList.Any( a => optedOutPersonIds.Contains( a.Id ) && a.GivingLeaderId == a.Id );

                            var remaingPersonIds = personList.Where( a => !optedOutPersonIds.Contains( a.Id ) ).ToList();

                            if ( givingLeaderOptedOut || !remaingPersonIds.Any() )
                            {
                                // If the giving leader opted out, or if there aren't any people in the giving statement that haven't opted out, return NULL and OptedOut = true
                                result.OptedOut = true;
                                result.Html = null;
                                return result.Html;
                            }
                        }
                    }
                }

                var personAliasIds = personList.SelectMany( a => a.Aliases.Select( x => x.Id ) ).ToList();
                if ( personAliasIds.Count == 1 )
                {
                    var personAliasId = personAliasIds[0];
                    financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAliasId.Value == personAliasId );
                }
                else
                {
                    financialTransactionQry = financialTransactionQry.Where( a => personAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );
                }

                var financialTransactionsList = financialTransactionQry
                    .Include( a => a.FinancialPaymentDetail )
                    .Include( a => a.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( a => a.TransactionDetails )
                    .Include( a => a.TransactionDetails.Select( x => x.Account ) )
                    .OrderBy( a => a.TransactionDateTime ).ToList();

                foreach ( var financialTransaction in financialTransactionsList )
                {
                    if ( options.TransactionAccountIds != null )
                    {
                        // remove any Accounts that were not included (in case there was a mix of included and not included accounts in the transaction)
                        financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.Where( a => options.TransactionAccountIds.Contains( a.AccountId ) ).ToList();
                    }

                    financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.PublicName ).ToList();
                }

                var lavaTemplateValue = DefinedValueCache.Get( options.LayoutDefinedValueGuid.Value );
                var lavaTemplateLava = lavaTemplateValue.GetAttributeValue( "LavaTemplate" );
                var lavaTemplateFooterLava = lavaTemplateValue.GetAttributeValue( "FooterHtml" );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false, GetDeviceFamily = false, GetOSFamily = false, GetPageContext = false, GetPageParameters = false, GetCampuses = true, GetCurrentPerson = true } );
                mergeFields.Add( "LavaTemplate", lavaTemplateValue );

                mergeFields.Add( "PersonList", personList );
                mergeFields.Add( "StatementStartDate", options.StartDate );
                var humanFriendlyEndDate = options.EndDate.HasValue ? options.EndDate.Value.AddDays( -1 ) : RockDateTime.Now.Date;
                mergeFields.Add( "StatementEndDate", humanFriendlyEndDate );

                var familyTitle = Rock.Data.RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, personId, groupId, null, false, !options.ExcludeInActiveIndividuals );

                mergeFields.Add( "Salutation", familyTitle );

                Location mailingAddress;

                if ( locationGuid.HasValue )
                {
                    // get the location that was specified for the recipient
                    mailingAddress = new LocationService( rockContext ).Get( locationGuid.Value );
                }
                else
                {
                    // for backwards compatibility, get the first address
                    IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );
                    mailingAddress = groupLocationsQry.Where( a => a.GroupId == groupId ).Select( a => a.Location ).FirstOrDefault();
                }

                mergeFields.Add( "MailingAddress", mailingAddress );

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

                var transactionDetailListAll = financialTransactionsList.SelectMany( a => a.TransactionDetails ).ToList();

                if ( options.HideRefundedTransactions && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    var allRefunds = transactionDetailListAll.SelectMany( a => a.Transaction.Refunds ).ToList();
                    foreach ( var refund in allRefunds )
                    {
                        foreach ( var refundedOriginalTransactionDetail in refund.OriginalTransaction.TransactionDetails )
                        {
                            // remove the refund's original TransactionDetails from the results
                            if ( transactionDetailListAll.Contains( refundedOriginalTransactionDetail ) )
                            {
                                transactionDetailListAll.Remove( refundedOriginalTransactionDetail );
                                foreach ( var refundDetailId in refund.FinancialTransaction.TransactionDetails.Select( a => a.Id ) )
                                {
                                    // remove the refund's transaction from the results
                                    var refundDetail = transactionDetailListAll.FirstOrDefault( a => a.Id == refundDetailId );
                                    if ( refundDetail != null )
                                    {
                                        transactionDetailListAll.Remove( refundDetail );
                                    }
                                }
                            }
                        }
                    }
                }

                if ( options.HideCorrectedTransactions && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    // Hide transactions that are corrected on the same date. Transactions that have a matching negative dollar amount on the same date and same account will not be shown.

                    // get a list of dates that have at least one negative transaction
                    var transactionsByDateList = transactionDetailListAll.GroupBy( a => a.Transaction.TransactionDateTime.Value.Date ).Select( a => new
                    {
                        Date = a.Key,
                        TransactionDetails = a.ToList()
                    } )
                    .Where( a => a.TransactionDetails.Any( x => x.Amount < 0 ) )
                    .ToList();


                    foreach ( var transactionsByDate in transactionsByDateList )
                    {
                        foreach ( var negativeTransaction in transactionsByDate.TransactionDetails.Where( a => a.Amount < 0 ) )
                        {
                            // find the first transaction that has an amount that matches the negative amount (on the same day and same account)
                            // and make sure the matching transaction doesn't already have a refund associated with it
                            var correctedTransactionDetail = transactionsByDate.TransactionDetails
                                .Where( a => ( a.Amount == ( -negativeTransaction.Amount ) && a.AccountId == negativeTransaction.AccountId ) && !a.Transaction.Refunds.Any() )
                                .FirstOrDefault();
                            if ( correctedTransactionDetail != null )
                            {
                                // if the transaction was corrected, remove it, and also remove the associated correction (the negative one) transaction
                                transactionDetailListAll.Remove( correctedTransactionDetail );
                                transactionDetailListAll.Remove( negativeTransaction );
                            }
                        }
                    }
                }

                List<FinancialTransactionDetail> transactionDetailListCash = transactionDetailListAll;
                List<FinancialTransactionDetail> transactionDetailListNonCash = new List<FinancialTransactionDetail>();

                if ( options.CurrencyTypeIdsCash != null )
                {
                    // NOTE: if there isn't a FinancialPaymentDetail record, assume it is Cash
                    transactionDetailListCash = transactionDetailListCash.Where( a =>
                        ( a.Transaction.FinancialPaymentDetailId == null ) ||
                        ( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && options.CurrencyTypeIdsCash.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ) ).ToList();
                }

                if ( options.CurrencyTypeIdsNonCash != null )
                {
                    transactionDetailListNonCash = transactionDetailListAll.Where( a =>
                        a.Transaction.FinancialPaymentDetailId.HasValue &&
                        a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue
                        && options.CurrencyTypeIdsNonCash.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ).ToList();
                }

                // Add Merge Fields for Transactions for custom Statements that might want to organize the output by Transaction instead of TransactionDetail
                var transactionListCash = transactionDetailListCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                var transactionListNonCash = transactionDetailListNonCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                mergeFields.Add( "Transactions", transactionListCash );
                mergeFields.Add( "TransactionsNonCash", transactionListNonCash );

                // Add the standard TransactionDetails and TransactionDetailsNonCash that the default Rock templates use
                mergeFields.Add( "TransactionDetails", transactionDetailListCash );
                mergeFields.Add( "TransactionDetailsNonCash", transactionDetailListNonCash );

                mergeFields.Add( "TotalContributionAmount", transactionDetailListCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCount", transactionDetailListCash.Count() );

                mergeFields.Add( "TotalContributionAmountNonCash", transactionDetailListNonCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCountNonCash", transactionDetailListNonCash.Count() );

                mergeFields.Add(
                    "AccountSummary",
                    transactionDetailListCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new
                        {
                            AccountName = s.FirstOrDefault().Account.PublicName ?? s.Key,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                mergeFields.Add(
                    "AccountSummaryNonCash",
                    transactionDetailListNonCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new
                        {
                            AccountName = s.FirstOrDefault().Account.PublicName ?? s.Key,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                if ( options.PledgesAccountIds.Any() )
                {
                    var pledgeList = financialPledgeQry
                                        .Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains( p.PersonAliasId.Value ) )
                                        .Include( a => a.Account )
                                        .OrderBy( a => a.Account.Order )
                                        .ThenBy( a => a.Account.PublicName )
                                        .ToList();

                    var pledgeSummaryByPledgeList = pledgeList
                                        .Select( p => new
                                        {
                                            p.Account,
                                            Pledge = p
                                        } )
                                        .ToList();

                    //// Pledges but organized by Account (in case more than one pledge goes to the same account)
                    //// NOTE: In the case of multiple pledges to the same account (just in case they accidently or intentionally had multiple pledges to the same account)
                    ////  -- Date Range
                    ////    -- StartDate: Earliest StartDate of all the pledges for that account 
                    ////    -- EndDate: Lastest EndDate of all the pledges for that account
                    ////  -- Amount Pledged: Sum of all Pledges to that account
                    ////  -- Amount Given: 
                    ////    --  The sum of transaction amounts to that account between
                    ////      -- Start Date: Earliest Start Date of all the pledges to that account
                    ////      -- End Date: Whatever is earlier (Statement End Date or Pledges' End Date)
                    var pledgeSummaryList = pledgeSummaryByPledgeList.GroupBy( a => a.Account ).Select( a => new PledgeSummary
                    {
                        Account = a.Key,
                        PledgeList = a.Select( x => x.Pledge ).ToList()
                    } ).ToList();

                    // add detailed pledge information
                    if ( pledgeSummaryList.Any() )
                    {
                        int statementPledgeYear = options.StartDate.Value.Year;

                        List<int> pledgeCurrencyTypeIds = null;
                        if ( options.CurrencyTypeIdsCash != null )
                        {
                            pledgeCurrencyTypeIds = options.CurrencyTypeIdsCash;
                            if ( options.PledgesIncludeNonCashGifts && options.CurrencyTypeIdsNonCash != null )
                            {
                                pledgeCurrencyTypeIds = options.CurrencyTypeIdsCash.Union( options.CurrencyTypeIdsNonCash ).ToList();
                            }
                        }

                        foreach ( var pledgeSummary in pledgeSummaryList )
                        {
                            DateTime adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date;
                            if ( pledgeSummary.PledgeEndDate.Value.Date < DateTime.MaxValue.Date )
                            {
                                adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date.AddDays( 1 );
                            }

                            if ( options.EndDate.HasValue )
                            {
                                if ( adjustedPledgeEndDate > options.EndDate.Value )
                                {
                                    adjustedPledgeEndDate = options.EndDate.Value;
                                }
                            }

                            var pledgeFinancialTransactionDetailQry = new FinancialTransactionDetailService( rockContext ).Queryable().Where( t =>
                                                             t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                                                             && t.Transaction.TransactionDateTime >= pledgeSummary.PledgeStartDate
                                                             && t.Transaction.TransactionDateTime < adjustedPledgeEndDate );

                            if ( options.PledgesIncludeChildAccounts )
                            {
                                // If PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                                    t.AccountId == pledgeSummary.AccountId
                                    ||
                                    ( t.Account.ParentAccountId.HasValue && t.Account.ParentAccountId == pledgeSummary.AccountId )
                                );
                            }
                            else
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t => t.AccountId == pledgeSummary.AccountId );
                            }

                            if ( pledgeCurrencyTypeIds != null )
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                                    t.Transaction.FinancialPaymentDetailId.HasValue &&
                                    t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && pledgeCurrencyTypeIds.Contains( t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );
                            }

                            pledgeSummary.AmountGiven = pledgeFinancialTransactionDetailQry.Sum( t => ( decimal? ) t.Amount ) ?? 0;
                        }
                    }

                    // Pledges ( organized by each Account in case an account is used by more than one pledge )
                    mergeFields.Add( "Pledges", pledgeSummaryList );
                }

                mergeFields.Add( "Options", options );

                var currentPerson = targetPerson;
                result.Html = lavaTemplateLava.ResolveMergeFields( mergeFields, currentPerson );
                if ( !string.IsNullOrEmpty( lavaTemplateFooterLava ) )
                {
                    result.FooterHtml = lavaTemplateFooterLava.ResolveMergeFields( mergeFields, currentPerson );
                }

                result.Html = result.Html.Trim();
            }

            return result.Html;
        }

        private IQueryable<FinancialPledge> GetFinancialPledgeQuery( StatementGeneratorOptions options, RockContext rockContext, bool usePersonFilters )
        {
            // pledge information
            var pledgeQry = new FinancialPledgeService( rockContext ).Queryable();

            // only include pledges that started *before* the enddate of the statement ( we don't want pledges that start AFTER the statement end date )
            if ( options.EndDate.HasValue )
            {
                pledgeQry = pledgeQry.Where( p => p.StartDate < options.EndDate.Value );
            }

            // also only include pledges that ended *after* the statement start date ( we don't want pledges that ended BEFORE the statement start date )
            pledgeQry = pledgeQry.Where( p => p.EndDate >= options.StartDate.Value );

            // Filter to specified AccountIds (if specified)
            if ( options.PledgesAccountIds == null || !options.PledgesAccountIds.Any() )
            {
                // if no PledgeAccountIds where specified, don't include any pledges
                pledgeQry = pledgeQry.Where( a => false );
            }
            else
            {
                // NOTE: Only get the Pledges that were specifically pledged to the selected accounts
                // If the PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                var selectedAccountIds = options.PledgesAccountIds;
                pledgeQry = pledgeQry.Where( a => a.AccountId.HasValue && selectedAccountIds.Contains( a.AccountId.Value ) );
            }

            if ( usePersonFilters )
            {
                if ( options.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable().Where( a => a.Id == options.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        pledgeQry = pledgeQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !options.DataViewId.HasValue )
                    {
                        if ( !options.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( options.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                            pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive );
                        }

                        // Only include Non-Deceased People even if we are including inactive individuals
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.IsDeceased == false );
                    }
                }
            }

            return pledgeQry;
        }

        /// <summary>
        /// Gets the financial transaction query.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        private IQueryable<FinancialTransaction> GetFinancialTransactionQuery( StatementGeneratorOptions options, RockContext rockContext, bool usePersonFilters )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionQry = financialTransactionService.Queryable();

            // filter to specified date range
            financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime >= options.StartDate );

            if ( options.EndDate.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime < options.EndDate.Value );
            }

            // default to Contributions if nothing specified
            var transactionTypeContribution = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( options.TransactionTypeIds == null || !options.TransactionTypeIds.Any() )
            {
                options.TransactionTypeIds = new List<int>();
                if ( transactionTypeContribution != null )
                {
                    options.TransactionTypeIds.Add( transactionTypeContribution.Id );
                }
            }

            if ( options.TransactionTypeIds.Count() == 1 )
            {
                int selectedTransactionTypeId = options.TransactionTypeIds[0];
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionTypeValueId == selectedTransactionTypeId );
            }
            else
            {
                financialTransactionQry = financialTransactionQry.Where( a => options.TransactionTypeIds.Contains( a.TransactionTypeValueId ) );
            }

            // Filter to specified AccountIds (if specified)
            if ( options.TransactionAccountIds == null )
            {
                // if TransactionAccountIds wasn't supplied, don't filter on AccountId
            }
            else
            {
                // narrow it down to recipients that have transactions involving any of the AccountIds
                var selectedAccountIds = options.TransactionAccountIds;
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDetails.Any( x => selectedAccountIds.Contains( x.AccountId ) ) );
            }

            if ( usePersonFilters )
            {
                if ( options.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable().Where( a => a.Id == options.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        financialTransactionQry = financialTransactionQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !options.DataViewId.HasValue )
                    {
                        if ( !options.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( options.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                            financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive );
                        }
                    }

                    // Only include Non-Deceased People even if we are including inactive individuals
                    financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.IsDeceased == false );
                }
            }

            return financialTransactionQry;
        }

        private static IQueryable<GroupLocation> GetGroupLocationQuery( RockContext rockContext )
        {
            int? groupLocationTypeIdHome = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            int? groupLocationTypeIdWork = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() ).Id;
            var groupLocationTypeIds = new List<int>();
            if ( groupLocationTypeIdHome.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdHome.Value );
            }

            if ( groupLocationTypeIdWork.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdWork.Value );
            }

            IQueryable<GroupLocation> groupLocationsQry = null;
            if ( groupLocationTypeIds.Count == 2 )
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue )
                    .Where( a => a.GroupLocationTypeValueId == groupLocationTypeIdHome.Value || a.GroupLocationTypeValueId == groupLocationTypeIdWork.Value );
            }
            else
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue && groupLocationTypeIds.Contains( a.GroupLocationTypeValueId.Value ) );
            }
            return groupLocationsQry;
        }


        #endregion

    }
}
