using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
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

        /// <summary>
        /// Updates the FinancialTransaction query to filter by the specified
        /// person or their giving group (if they have one).
        /// </summary>
        /// <param name="qry">The query to be extended.</param>
        /// <param name="helper">The tool helper.</param>
        /// <param name="personIdKey">The encoded person identifier.</param>
        /// <returns>The same query or a new query with additional filtering applied.</returns>
        private IQueryable<FinancialTransaction> WherePersonOrGivingGroup( IQueryable<FinancialTransaction> qry, AgentToolHelper helper, string personIdKey )
        {
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return qry;
            }

            var person = helper.GetRequiredEntity<Model.Person>( personIdKey );

            if ( person != null && person.GivingGroupId.HasValue )
            {
                var personIdQry = new PersonService( AgentRequestContext.RockContext ).Queryable()
                    .Where( p => p.GivingGroupId == person.GivingGroupId.Value )
                    .Select( p => p.Id );

                return qry.Where( ft => personIdQry.Contains( ft.AuthorizedPersonAlias.PersonId ) );
            }
            else if ( person != null )
            {
                return qry.Where( ft => ft.AuthorizedPersonAlias.PersonId == person.Id );
            }
            else
            {
                return qry.Where( ft => false );
            }
        }

        /// <summary>
        /// Gets the common financial transaction result data for the query.
        /// This is used by tools to list the transaction details.
        /// </summary>
        /// <param name="helper">The tool helper.</param>
        /// <param name="qry">The query that contains the base filtering.</param>
        /// <param name="campusIdKey">The encoded campus identifier to filter on.</param>
        /// <param name="accountIdKeys">The encoded account identifiers to filter on.</param>
        /// <param name="paymentMethodTypeValueIdKey">The encoded payment method identifier to filter on.</param>
        /// <param name="startDate">The start date for transactions.</param>
        /// <param name="endDate">The end date for transactions.</param>
        /// <param name="pageNumber">The page number to retrieve in the set.</param>
        /// <param name="updateItems">A callback that will be called for any items when constructing the result.</param>
        /// <returns>A tool result.</returns>
        private IAgentToolResult GetFinancialTransactionResult(
            AgentToolHelper helper,
            IQueryable<FinancialTransaction> qry,
            string campusIdKey,
            List<string> accountIdKeys,
            string paymentMethodTypeValueIdKey,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            Action<IList<FinancialTransactionResult>> updateItems )
        {
            qry = helper.WhereOptionalIdKey( qry, ft => ft.Batch.CampusId, campusIdKey );
            qry = helper.WhereOptionalIdKey( qry, ft => ft.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, ft => ft.TransactionDateTime, startDate, endDate );

            if ( !TryGetMatchingAccountIds( accountIdKeys, campusIdKey, out var accountIds ) )
            {
                return NoData()
                    .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
            }

            var hasAccountFilter = accountIds.Any();

            // If we have an account filter, only return transactions that actually contribute (>0)
            // to one of the filtered accounts (mirror Summarize's "effective" set).
            if ( hasAccountFilter )
            {
                qry = qry.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
            }

            qry = qry.OrderByDescending( a => a.TransactionDateTime )
                .ThenByDescending( a => a.Id );

            // Project AFTER ordering, BEFORE paging
            var projectedQry = qry.AsExpandable().Select( a => new FinancialTransactionResult
            {
                Id = a.Id,
                AuthorizedPerson = PersonResult.NameOnly( a.AuthorizedPersonAlias ),
                TransactionDateTime = a.TransactionDateTime,

                // Only sum details that match the resolved account set (if any)
                TotalAmount = a.TransactionDetails
                    .Where( d => !hasAccountFilter || accountIds.Contains( d.AccountId ) )
                    .Sum( d => ( decimal? ) d.Amount ) ?? 0m,

                // And only list those matching account details
                Accounts = a.TransactionDetails
                    .Where( td => !hasAccountFilter || accountIds.Contains( td.AccountId ) )
                    .Select( td => new FinancialAccountTransactionResult
                    {
                        Amount = td.Amount,
                        Name = td.Account.Name,
                        EntityTypeId = td.EntityTypeId,
                        EntityId = td.EntityId,
                    } )
                    .ToList(),

                CurrencyType = a.FinancialPaymentDetail.CurrencyTypeValue != null
                    ? new KeyNameResult
                    {
                        Id = a.FinancialPaymentDetail.CurrencyTypeValue.Id,
                        Name = a.FinancialPaymentDetail.CurrencyTypeValue.Value
                    }
                    : null,

                CreditCardType = a.FinancialPaymentDetail.CreditCardTypeValue != null
                    ? new KeyNameResult
                    {
                        Id = a.FinancialPaymentDetail.CreditCardTypeValue.Id,
                        Name = a.FinancialPaymentDetail.CreditCardTypeValue.Value
                    }
                    : null,
            } );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var page = helper.GetPaginatedItems( projectedQry, pageNumber );

            updateItems?.Invoke( page.Items );

            // Trimmed history content (unchanged)
            var historyItems = page.Items.Select( r => new
            {
                r.Id,
                r.TransactionDateTime,
                r.TotalAmount,
                r.AuthorizedPerson,
            } ).ToList();

            var result = helper.GetPaginatedResult( page, page.WithItems( historyItems ) );

            if ( page.Items.Any( a => a.TotalAmount.Value < 0 ) )
            {
                result = result.WithInstructions( "Note: Some transactions in this list have negative amounts, these are refunds to previous transactions." );
            }

            return result;
        }

        #endregion
    }
}
