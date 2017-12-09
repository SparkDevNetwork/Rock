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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.StatementGenerator.Rest
{
    /// <summary>
    /// NOTE: WebApi doesn't support Controllers with the Same Name, even if they have different namespaces, so can't call this FinancialTransactionsController
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class StatementGeneratorFinancialTransactionsController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Gets the statement generator templates.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorTemplates" )]
        public List<DefinedValue> GetStatementGeneratorTemplates()
        {
            List<DefinedValue> result = null;
            using ( var rockContext = new RockContext() )
            {
                result = new Model.DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE.AsGuid() ).AsNoTracking()
                    .ToList();

                result.ForEach( a => a.LoadAttributes( rockContext ) );
            }

            return result;
        }

        /// <summary>
        /// Gets the statement generator recipients. This will be sorted based on the StatementGeneratorOptions
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipients" )]
        public List<StatementGeneratorRecipient> GetStatementGeneratorRecipients( [FromBody]StatementGeneratorOptions options )
        {
            if ( options == null )
            {
                throw new Exception( "StatementGenerationOption options must be specified" );
            }

            using ( var rockContext = new RockContext() )
            {
                var financialTransactionQry = GetFinancialTransactionQuery( options, rockContext, true );
                var financialPledgeQry = GetFinancialPledgeQuery( options, rockContext, true );

                // Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
                // These are Persons that give as part of a Group.For example, Husband and Wife
                var qryGivingGroupIdsThatHaveTransactions = financialTransactionQry.Select( a => a.AuthorizedPersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                var qryGivingGroupIdsThatHavePledges = financialPledgeQry.Select( a => a.PersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                // Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
                // These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
                // to determine which address(es) the statements need to be mailed to 
                var groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
                var groupMembersQry = new GroupMemberService( rockContext ).Queryable().Where( m => m.Group.GroupTypeId == groupTypeIdFamily );

                var qryIndividualGiversThatHaveTransactions = financialTransactionQry
                    .Where( a => !a.AuthorizedPersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.AuthorizedPersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var qryIndividualGiversThatHavePledges = financialPledgeQry
                    .Where( a => !a.PersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.PersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var unionQry = qryGivingGroupIdsThatHaveTransactions.Union( qryIndividualGiversThatHaveTransactions );

                if ( options.PledgesAccountIds.Any() )
                {
                    unionQry = unionQry.Union( qryGivingGroupIdsThatHavePledges ).Union( qryIndividualGiversThatHavePledges );
                }

                /*  Limit to Mailing Address and sort by ZipCode */
                IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );

                // Do an outer join on location so we can include people that don't have an address (if options.IncludeIndividualsWithNoAddress) //
                var unionJoinLocationQry = from pg in unionQry
                                           join l in groupLocationsQry on pg.GroupId equals l.GroupId into u
                                           from l in u.DefaultIfEmpty()
                                           select new
                                           {
                                               pg.PersonId,
                                               pg.GroupId,
                                               LocationId = ( int? ) l.LocationId,
                                               l.Location.PostalCode
                                           };

                if ( options.PersonId == null && !options.IncludeIndividualsWithNoAddress )
                {
                    unionJoinLocationQry = unionJoinLocationQry.Where( a => a.LocationId.HasValue );
                }

                if ( options.OrderByPostalCode )
                {
                    unionJoinLocationQry = unionJoinLocationQry.OrderBy( a => a.PostalCode );
                }

                var givingIdsQry = unionJoinLocationQry.Select( a => new { a.PersonId, a.GroupId } );

                var result = givingIdsQry.ToList().Select( a => new StatementGeneratorRecipient { GroupId = a.GroupId, PersonId = a.PersonId } ).ToList();

                return result;
            }
        }

        /// <summary>
        /// Gets the group location query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static IQueryable<GroupLocation> GetGroupLocationQuery( RockContext rockContext )
        {
            var groupLocationTypeIdHome = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var groupLocationTypeIdWork = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() )?.Id;
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

        /// <summary>
        /// Gets the statement generator recipient result for a GivingGroup
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, [FromBody]StatementGeneratorOptions options )
        {
            return GetStatementGeneratorRecipientResult( groupId, ( int? ) null, options );
        }

        /// <summary>
        /// Gets the statement generator recipient result for a specific person and associated group (family)
        /// NOTE: If a person is in multiple families, call this for each of the families so that the statement will go to the address of each family
        /// </summary>
        /// <param name="statementGeneratorRecipient">The statement generator recipient.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, int? personId, [FromBody]StatementGeneratorOptions options )
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

                if ( options.ExcludeOptedOutIndividuals == true )
                {
                    int? doNotSendGivingStatementAttributeId = AttributeCache.Read( Rock.StatementGenerator.SystemGuid.Attribute.PERSON_DO_NOT_SEND_GIVING_STATEMENT.AsGuid() )?.Id;
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
                                return result;
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
                    financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.Name ).ToList();
                }

                var lavaTemplateValue = DefinedValueCache.Read( options.LayoutDefinedValueGuid.Value );
                var lavaTemplateLava = lavaTemplateValue.GetAttributeValue( "LavaTemplate" );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false, GetDeviceFamily = false, GetOSFamily = false, GetPageContext = false, GetPageParameters = false, GetCampuses = true, GetCurrentPerson = true } );
                mergeFields.Add( "LavaTemplate", lavaTemplateValue );

                mergeFields.Add( "PersonList", personList );
                mergeFields.Add( "StatementStartDate", options.StartDate );
                mergeFields.Add( "StatementEndDate", options.EndDate );

                var familyTitle = Rock.Data.RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, personId, groupId, null, false );

                mergeFields.Add( "Salutation", familyTitle );

                IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );

                var mailingAddress = groupLocationsQry.Where( a => a.GroupId == groupId ).Select( a => a.Location ).FirstOrDefault();
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
                    // Hide transactions that are corrected on the same date. Transactions that have a matching negative dollar amount on the same date will not be shown.

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
                            // find the first transaction that has an amount that matches the negative amount (on the same day)
                            // and make sure the matching transaction doesn't already have a refund associated with it
                            var correctedTransactionDetail = transactionsByDate.TransactionDetails.FirstOrDefault( a => a.Amount == ( -negativeTransaction.Amount ) && !a.Transaction.Refunds.Any() );
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
                /* TODO
                if ( options.CashAccountIds != null )
                {
                    transactionDetailListCash = transactionDetailListCash.Where( a => options.CashAccountIds.Contains( a.AccountId ) ).ToList();
                }

                if ( options.NonCashAccountIds != null )
                {
                    transactionDetailListNonCash = transactionDetailListAll.Where( a => options.NonCashAccountIds.Contains( a.AccountId ) ).ToList();
                }*/

                mergeFields.Add( "TransactionDetails", transactionDetailListCash );
                mergeFields.Add( "TransactionDetailsNonCash", transactionDetailListNonCash );

                mergeFields.Add( "TotalContributionAmount", transactionDetailListCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCount", transactionDetailListCash.Count() );

                mergeFields.Add( "TotalContributionAmountNonCash", transactionDetailListNonCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCountNonCash", transactionDetailListNonCash.Count() );

                mergeFields.Add( "AccountSummary", transactionDetailListCash.GroupBy( t => t.Account.Name ).Select( s => new { AccountName = s.Key, Total = s.Sum( a => a.Amount ), Order = s.Max( a => a.Account.Order ) } ).OrderBy( s => s.Order ) );
                mergeFields.Add( "AccountSummaryNonCash", transactionDetailListNonCash.GroupBy( t => t.Account.Name ).Select( s => new { AccountName = s.Key, Total = s.Sum( a => a.Amount ), Order = s.Max( a => a.Account.Order ) } ).OrderBy( s => s.Order ) );

                if ( options.PledgesAccountIds.Any() )
                {
                    var pledgeList = financialPledgeQry
                                        .Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains( p.PersonAliasId.Value ) )
                                        .Include( a => a.Account )
                                        .OrderBy( a => a.Account.Order )
                                        .ThenBy( a => a.Account.PublicName )
                                        .ToList();

                    var pledgeSummaryList = pledgeList
                                        .Select( p => new PledgeSummaryByPledge
                                        {
                                            Account = p.Account,
                                            Pledge = p
                                        } )
                                        .ToList();

                    // add detailed pledge information
                    if ( pledgeSummaryList.Any() )
                    {
                        int statementPledgeYear = options.StartDate.Value.Year;
                        foreach ( var pledge in pledgeSummaryList )
                        {
                            DateTime adjustedPedgeEndDate = pledge.PledgeEndDate.Value.Date;
                            if ( pledge.PledgeEndDate.Value.Date < DateTime.MaxValue.Date )
                            {
                                adjustedPedgeEndDate = pledge.PledgeEndDate.Value.Date.AddDays( 1 );
                            }

                            // if the pledge is to a child account of one of the selected PledgeAccountIs, set the Account as the ParentAccount
                            if ( !options.PledgesAccountIds.Contains( pledge.AccountId ) && pledge.Account.ParentAccountId.HasValue )
                            {
                                pledge.Account = pledge.Account.ParentAccount;
                            }

                            var statementYearEnd = new DateTime( statementPledgeYear + 1, 1, 1 );

                            if ( adjustedPedgeEndDate > statementYearEnd )
                            {
                                adjustedPedgeEndDate = statementYearEnd;
                            }

                            if ( adjustedPedgeEndDate > RockDateTime.Now )
                            {
                                adjustedPedgeEndDate = RockDateTime.Now;
                            }

                            var pledgeFinancialTransactionDetailQry = new FinancialTransactionDetailService( rockContext ).Queryable();
                            if ( options.PledgesIncludeChildAccounts )
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                                    t.AccountId == pledge.AccountId
                                    ||
                                    ( t.Account.ParentAccountId.HasValue && t.Account.ParentAccountId == pledge.AccountId )
                                );
                            }
                            else
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t => t.AccountId == pledge.AccountId );
                            }

                            pledge.AmountGiven = pledgeFinancialTransactionDetailQry
                                                        .Where( t =>
                                                             t.AccountId == pledge.AccountId
                                                             && t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                                                             && t.Transaction.TransactionDateTime >= pledge.PledgeStartDate
                                                             && t.Transaction.TransactionDateTime < adjustedPedgeEndDate )
                                                        .Sum( t => ( decimal? ) t.Amount ) ?? 0;

                            pledge.AmountRemaining = ( pledge.AmountGiven > pledge.AmountPledged ) ? 0 : ( pledge.AmountPledged - pledge.AmountGiven );

                            if ( pledge.AmountPledged > 0 )
                            {
                                pledge.PercentComplete = ( int ) ( ( pledge.AmountGiven * 100 ) / pledge.AmountPledged );
                            }
                        }
                    }

                    // Pledges but organized by Account (in case more than one pledge goes to the same account)
                    var pledgeAccounts = pledgeSummaryList.GroupBy( a => a.Account ).Select( a => new PledgeSummaryByAccount
                    {
                        Account = a.Key,
                        PledgeList = a.Select( x => x.Pledge ).ToList()
                    } );


                    // Pledges ( organized by each Account in case an account is used by more than one pledge )
                    mergeFields.Add( "Pledges", pledgeAccounts );
                }

                mergeFields.Add( "Options", options );

                result.Html = lavaTemplateLava.ResolveMergeFields( mergeFields );
                result.Html = result.Html.Trim();
            }

            return result;
        }

        /// <summary>
        /// Gets the financial pledge query.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        private IQueryable<FinancialPledge> GetFinancialPledgeQuery( StatementGeneratorOptions options, RockContext rockContext, bool usePersonFilters )
        {
            int statementPledgeYear = options.StartDate.Value.Year;

            // pledge information
            var pledgeQry = new FinancialPledgeService( rockContext ).Queryable()
                                .Where( p => p.StartDate.Year <= statementPledgeYear && p.EndDate.Year >= statementPledgeYear );

            // Filter to specified AccountIds (if specified)
            if ( options.PledgesAccountIds == null || !options.PledgesAccountIds.Any() )
            {
                // if no PledgeAccountIds where specified, don't include any pledges
                pledgeQry = pledgeQry.Where( a => false );
            }
            else
            {
                // narrow it down to recipients that have pledges involving any of the AccountIds
                var selectedAccountIds = options.PledgesAccountIds;
                if ( options.PledgesIncludeChildAccounts )
                {
                    // If Pledges Include Child Accounts, also check for child accounts (but only one level deep)
                    pledgeQry = pledgeQry.Where( a => (
                        a.AccountId.HasValue && selectedAccountIds.Contains( a.AccountId.Value ) )
                        ||
                        a.Account.ParentAccountId.HasValue && selectedAccountIds.Contains( a.Account.ParentAccountId.Value ) );
                }
                else
                {
                    pledgeQry = pledgeQry.Where( a => a.AccountId.HasValue && selectedAccountIds.Contains( a.AccountId.Value ) );
                }
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
                    if ( !options.IncludeBusinesses )
                    {
                        int recordTypeValueIdPerson = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                    }

                    if ( options.ExcludeInActiveIndividuals )
                    {
                        int recordStatusValueIdActive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive );
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

            // only include Contributions
            var transactionTypeContribution = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( transactionTypeContribution != null )
            {
                int transactionTypeContributionId = transactionTypeContribution.Id;
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionTypeValueId == transactionTypeContributionId );
            }

            // Filter to specified AccountIds (if specified)
            /* TODO
            if ( options.CashAccountIds == null && options.NonCashAccountIds == null )
            {
                // if neither CashAccountIds or NonCashAccountIds was supplied, don't filter on AccountId
            }
            else
            {
                // narrow it down to recipients that have transactions involving any of the AccountIds
                var selectedAccountIds = options.CashAccountIds.Union( options.NonCashAccountIds ).Distinct().ToList();
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDetails.Any( x => selectedAccountIds.Contains( x.AccountId ) ) );
            }
            */

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
                    if ( !options.IncludeBusinesses )
                    {
                        int recordTypeValueIdPerson = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                    }

                    if ( options.ExcludeInActiveIndividuals )
                    {
                        int recordStatusValueIdActive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                        financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive );
                    }
                }
            }

            return financialTransactionQry;
        }
    }
}
