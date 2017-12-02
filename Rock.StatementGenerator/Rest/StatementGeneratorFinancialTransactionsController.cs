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
                DebugHelper.SQLLoggingStart( rockContext );
                IQueryable<FinancialTransaction> financialTransactionQry = GetFinancialTransactionQuery( options, rockContext, true );

                // Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
                // These are Persons that give as part of a Group.For example, Husband and Wife
                var qryGivingGroupIdsThatHaveTransactions = financialTransactionQry.Select( a => a.AuthorizedPersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
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

                var unionQry = qryGivingGroupIdsThatHaveTransactions.Union( qryIndividualGiversThatHaveTransactions );

                /*  Limit to Mailing Address and sort by ZipCode */
                IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );

                var unionJoinLocationQry = unionQry.Join( groupLocationsQry, pg => pg.GroupId, l => l.GroupId, ( pg, l ) => new
                {
                    pg.PersonId,
                    pg.GroupId,
                    LocationId = ( int? ) l.LocationId,
                    l.Location.PostalCode
                } );

                if ( !options.IncludeIndividualsWithNoAddress )
                {
                    unionJoinLocationQry = unionJoinLocationQry.Where( a => a.LocationId.HasValue );
                }

                if ( options.OrderByPostalCode )
                {
                    unionJoinLocationQry = unionJoinLocationQry.OrderBy( a => a.PostalCode );
                }

                var givingIdsQry = unionJoinLocationQry.Select( a => new { a.PersonId, a.GroupId } );

                var result = givingIdsQry.ToList().Select( a => new StatementGeneratorRecipient { GroupId = a.GroupId, PersonId = a.PersonId } ).ToList();

                DebugHelper.SQLLoggingStop();
                return result;
            }
        }

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

            var groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue && groupLocationTypeIds.Contains( a.GroupLocationTypeValueId.Value ) );
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

                var personAliasIds = personList.SelectMany( a => a.Aliases.Select( x => x.Id ) ).ToList();

                if ( personId.HasValue )
                {
                    // get transactions for a specific person
                    financialTransactionQry = financialTransactionQry.Where( a => personAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );
                }
                else
                {
                    // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                    financialTransactionQry = financialTransactionQry.Where( a => personAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );
                }

                var financialTransactionsList = financialTransactionQry
                    .Include( a => a.TransactionDetails ).Include( a => a.TransactionDetails.Select( x => x.Account ) )
                    .OrderBy( a => a.TransactionDateTime );

                foreach ( var financialTransaction in financialTransactionsList )
                {
                    financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.Name ).ToList();
                }

                var template = DefinedValueCache.Read( options.LayoutDefinedValueGuid.Value );
                var lavaTemplate = template.GetAttributeValue( "LavaTemplate" );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false, GetDeviceFamily = false, GetOSFamily = false, GetPageContext = false, GetPageParameters = false, GetCampuses = true, GetCurrentPerson = true } );

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

                var transactionDetailList = financialTransactionsList.SelectMany( a => a.TransactionDetails ).ToList();

                mergeFields.Add( "TransactionDetails", transactionDetailList );

                mergeFields.Add( "AccountSummary", transactionDetailList.GroupBy( t => t.Account.Name ).Select( s => new { AccountName = s.Key, Total = s.Sum( a => a.Amount ), Order = s.Max( a => a.Account.Order ) } ).OrderBy( s => s.Order ) );



                var statementPledgeYear = options.StartDate.Value.Year;

                // pledge information
                var pledges = new FinancialPledgeService( rockContext ).Queryable().AsNoTracking()
                                    .Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains( p.PersonAliasId.Value )
                                        && p.StartDate.Year <= statementPledgeYear && p.EndDate.Year >= statementPledgeYear )
                                    .GroupBy( p => p.Account )
                                    .Select( g => new PledgeSummary
                                    {
                                        AccountId = g.Key.Id,
                                        AccountName = g.Key.Name,
                                        AmountPledged = g.Sum( p => p.TotalAmount ),
                                        PledgeStartDate = g.Min( p => p.StartDate ),
                                        PledgeEndDate = g.Max( p => p.EndDate )
                                    } )
                                    .ToList();

                // add detailed pledge information
                foreach ( var pledge in pledges )
                {
                    var adjustedPedgeEndDate = pledge.PledgeEndDate.Value.Date.AddDays( 1 );
                    var statementYearEnd = new DateTime( statementPledgeYear + 1, 1, 1 );

                    if ( adjustedPedgeEndDate > statementYearEnd )
                    {
                        adjustedPedgeEndDate = statementYearEnd;
                    }

                    if ( adjustedPedgeEndDate > RockDateTime.Now )
                    {
                        adjustedPedgeEndDate = RockDateTime.Now;
                    }

                    pledge.AmountGiven = new FinancialTransactionDetailService( rockContext ).Queryable()
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

                mergeFields.Add( "Pledges", pledges );

                result.Html = lavaTemplate.ResolveMergeFields( mergeFields );
            }

            return result;
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

            if ( usePersonFilters )
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

            return financialTransactionQry;
        }
    }


}
