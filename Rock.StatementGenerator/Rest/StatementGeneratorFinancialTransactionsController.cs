using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.StatementGenerator.Rest
{
    public class StatementGeneratorFinancialTransactionsController : Rock.Rest.ApiControllerBase
    {
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
            using ( var rockContext = new RockContext() )
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

                /* TODO Limit to Mailing Address and sort by ZipCode*/

                // var unionJoinLocationQry = unionQry.Join
                /*
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

                var groupIdsWithMailingAddressQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue && groupLocationTypeIds.Contains( a.GroupLocationTypeValueId.Value ) )
                    .Select( a => a.GroupId ).Distinct();

                var groupMembersWithMailingAddress = new GroupMemberService( rockContext ).Queryable().Where( m => groupIdsWithMailingAddressQry.Any( g => g == m.GroupId ) );

                financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.PersonId )

                */

                var givingIdsQry = unionQry.Distinct();

                var result = givingIdsQry.ToList().Select( a => new StatementGeneratorRecipient { GroupId = a.GroupId, PersonId = a.PersonId } ).ToList();

                // TODO
                return result;
            }
        }

        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( StatementGeneratorRecipient statementGeneratorRecipient, [FromBody]StatementGeneratorOptions options )
        {
            // TODO
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.StatementGenerator.Rest.StatementGeneratorRecipient" />
    public class StatementGeneratorRecipientResult : StatementGeneratorRecipient
    {
        public string Html { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StatementGeneratorRecipient
    {
        /// <summary>
        /// Gets or sets the GroupId of the Family to use as the Address.
        /// if PersonId is null, this is also the GivingGroupId to use when fetching transactions
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier for people that give as Individuals. If this is null, get the Transactions based on the GivingGroupId
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StatementGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the cash account ids.
        /// </summary>
        /// <value>
        /// The cash account ids.
        /// </value>
        public List<int> CashAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the non cash account ids.
        /// </summary>
        /// <value>
        /// The non cash account ids.
        /// </value>
        public List<int> NonCashAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// NULL means to get all individuals
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Person DataViewId to filter the statements to
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include individuals with no address].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include individuals with no address]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeIndividualsWithNoAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exclude in active individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude in active individuals]; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludeInActiveIndividuals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exclude opted out individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude opted out individuals]; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludeOptedOutIndividuals { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [include businesses].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeBusinesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide refunded transactions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide refunded transactions]; otherwise, <c>false</c>.
        /// </value>
        public bool HideRefundedTransactions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [hide corrected transactions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide corrected transactions]; otherwise, <c>false</c>.
        /// </value>
        public bool HideCorrectedTransactions { get; set; } = true;
    }
}
