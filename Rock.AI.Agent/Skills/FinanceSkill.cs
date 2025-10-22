using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    [Description( "This skill provides an overview of connection features." )]
    [AgentSkillGuid( "4FC57368-8362-49F0-A1A2-EBC9EFDD947C" )]
    [EntityTypeGuid( "92C9469F-C158-4476-8854-EF4805EA0970" )]
    [AgentUsage( "For analytical requests, prefer the SummarizeFinancialTransactions tool. Use ListFinancialTransactions for raw transaction information when explicitly requested." )]
    internal sealed partial class FinanceSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<FinanceSkill> _logger;
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinanceSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        /// <param name="rockContextFactory">Factory used to create Rock data contexts.</param>
        public FinanceSkill( ILogger<FinanceSkill> logger, IRockContextFactory rockContextFactory )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
        }

        #endregion

        #region Shared Helpers

        /// <summary>
        /// Gets the financial accounts to be used for filtering based on the supplied account keys and campus key.
        /// </summary>
        /// <param name="originalAccountIds">The account ids requested to be filtered on.</param>
        /// <param name="campusId">The campus id requested to be filtered on.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<FinancialAccountCache> GetFinancialAccountsForQuery( List<string> originalAccountIds, string campusId, RockContext rockContext )
        {
            // The filtering for accounts will be handled as such:
            // A. If no accounts are specified, but a campus is specified, find all accounts for that campus. 
            // B. If accounts are specified, and no campus is specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include all child accounts.
            // C. If both accounts and campus are specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include only child accounts for the specified campus.
            if ( campusId.IsNullOrWhiteSpace() && !originalAccountIds.Any() )
            {
                return new List<FinancialAccountCache>();
            }

            var results = new List<FinancialAccountCache>();

            // Case A: No accounts specified, campus specified.
            if ( !originalAccountIds.Any() && campusId.IsNotNullOrWhiteSpace() )
            {
                var campusIntId = IdHasher.Instance.GetId( campusId );

                results = FinancialAccountCache.All()
                    .Where( a => a.IsActive && a.CampusId.HasValue && a.CampusId.Value == campusIntId )
                    .ToList();
            }
            // Case B: Accounts specified, no campus specified.
            else if ( originalAccountIds.Any() && campusId.IsNullOrWhiteSpace() )
            {
                // Decode multiple account ids (ignore invalid keys).
                var accountIds = originalAccountIds.Where( k => k.IsNotNullOrWhiteSpace() )
                    .Select( k => IdHasher.Instance.GetId( k ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .Distinct()
                    .ToList();

                var accounts = new List<FinancialAccountCache>();

                foreach ( var acctId in accountIds )
                {
                    var acct = FinancialAccountCache.Get( acctId, rockContext );
                    if ( acct == null )
                    {
                        continue;
                    }
                    accounts.Add( acct );

                    // Only include child accounts if Uses Campus Child Accounts is enabled.
                    if ( acct.UsesCampusChildAccounts )
                    {
                        var children = acct.ChildAccounts;

                        foreach ( var child in children )
                        {
                            var childAcct = FinancialAccountCache.Get( child.Id, rockContext );
                            if ( childAcct != null && !accounts.Any( a => a.Id == childAcct.Id ) )
                            {
                                accounts.Add( childAcct );
                            }
                        }
                    }
                }

                results = accounts;
            }
            // Case C: Both accounts and campus specified.
            else
            {
                var campusIntId = IdHasher.Instance.GetId( campusId );

                // Decode multiple account ids (ignore invalid keys).
                var accountIds = originalAccountIds.Where( k => k.IsNotNullOrWhiteSpace() )
                    .Select( k => IdHasher.Instance.GetId( k ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .Distinct()
                    .ToList();

                var accounts = new List<FinancialAccountCache>();

                foreach ( var acctId in accountIds )
                {
                    var acct = FinancialAccountCache.Get( acctId, rockContext );
                    if ( acct == null )
                    {
                        continue;
                    }
                    accounts.Add( acct );
                    // Only include child accounts for the specified campus if Uses Campus Child Accounts is enabled.
                    if ( acct.UsesCampusChildAccounts )
                    {
                        var children = acct.ChildAccounts
                            .Where( ca => ca.CampusId.HasValue && ca.CampusId.Value == campusIntId );

                        foreach ( var child in children )
                        {
                            var childAcct = FinancialAccountCache.Get( child.Id, rockContext );

                            if ( childAcct != null && !accounts.Any( a => a.Id == childAcct.Id ) )
                            {
                                accounts.Add( childAcct );
                            }
                        }
                    }
                }

                results = accounts;
            }

            return results;
        }

        /// <summary>
        /// Builds the base <see cref="IQueryable{FinancialTransaction}"/> applying only transaction-scope filters.
        /// Account (fund) filtering is intentionally deferred to detail-level projections to avoid excluding
        /// multi-fund transactions from analytic calculations.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="options">User-supplied query options.</param>
        /// <returns>A filtered queryable of transactions.</returns>
        private IQueryable<FinancialTransaction> GetFinancialTransactionsQueryable( RockContext rockContext, FinancialTransactionQueryOptions options )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );

            // Pull what we need and leave AccountId out here on purpose.
            var qry = financialTransactionService
                .Queryable()
                .Include( t => t.TransactionDetails )
                .Include( t => t.FinancialPaymentDetail )
                .Include( t => t.Batch ); // for CampusId

            if ( options.PersonId.HasValue )
            {
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == options.PersonId.Value );
            }

            // Prefer the canonical link: Transaction -> Batch -> CampusId
            if ( options.BatchCampusId.HasValue )
            {
                var campusId = options.BatchCampusId.Value;
                qry = qry.Where( t => t.Batch != null && t.Batch.CampusId == campusId );
            }

            // DO NOT filter by AccountId at the transaction level.
            // That would exclude valid transactions that include other funds.
            // We'll respect AccountId only when aggregating details.

            if ( options.PaymentMethodTypeId.HasValue )
            {
                qry = qry.Where( t => t.FinancialPaymentDetail.CurrencyTypeValueId == options.PaymentMethodTypeId.Value );
            }

            if ( options.StartDate.HasValue )
            {
                qry = qry.Where( t => t.TransactionDateTime >= options.StartDate.Value );
            }

            if ( options.EndDate.HasValue )
            {
                qry = qry.Where( t => t.TransactionDateTime <= options.EndDate.Value );
            }

            return qry;
        }

        #endregion
    }
}
