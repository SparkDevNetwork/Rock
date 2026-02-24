using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
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

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinanceSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public FinanceSkill( ILogger<FinanceSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Shared Helpers

        /// <summary>
        /// Gets the financial accounts to be used for filtering based on the supplied account keys and campus key.
        /// </summary>
        /// <param name="originalAccountIds">The account ids requested to be filtered on.</param>
        /// <param name="campusId">The campus id requested to be filtered on.</param>
        /// <returns></returns>
        private List<FinancialAccountCache> GetFinancialAccountsForQuery( List<string> originalAccountIds, string campusId )
        {
            // The filtering for accounts will be handled as such:
            // A. If no accounts are specified, but a campus is specified, find all accounts for that campus. 
            // B. If accounts are specified, and no campus is specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include all child accounts.
            // C. If both accounts and campus are specified, find all parent accounts. If a parent account has `Uses Campus Child Accounts` enabled, include only child accounts for the specified campus.
            if ( campusId.IsNullOrWhiteSpace() && !originalAccountIds.Any() )
            {
                return [];
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
                    var acct = FinancialAccountCache.Get( acctId, AgentRequestContext.RockContext );
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
                            var childAcct = FinancialAccountCache.Get( child.Id, AgentRequestContext.RockContext );
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
                    var acct = FinancialAccountCache.Get( acctId, AgentRequestContext.RockContext );
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
                            var childAcct = FinancialAccountCache.Get( child.Id, AgentRequestContext.RockContext );

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
        /// Gets the financial accounts to be used for filtering based on the
        /// supplied account keys and campus key.
        /// </summary>
        /// <param name="accountIdKeys">The account ids requested to be filtered on.</param>
        /// <param name="campusIdKey">The campus id requested to be filtered on.</param>
        /// <param name="accountIds">The resulting account ids to filter on.</param>
        /// <returns><c>false</c> if filtering was performed and no accounts were available; otherwise <c>true</c>.</returns>
        private bool TryGetMatchingAccountIds( List<string> accountIdKeys, string campusIdKey, out IList<int> accountIds )
        {
            // If they specified any accoun tkeys or a campus key, then we need
            // to resolve the account ids to filter on.
            if ( accountIdKeys?.Any() == true || campusIdKey.IsNotNullOrWhiteSpace() )
            {
                accountIds = GetFinancialAccountsForQuery( accountIdKeys ?? [], campusIdKey )
                    .Select( a => a.Id )
                    .ToList();

                return accountIds.Any();
            }

            // If they didn't specify either, then we can skip this step and not
            // filter on account at all.
            accountIds = [];
            return true;
        }

        #endregion
    }
}
