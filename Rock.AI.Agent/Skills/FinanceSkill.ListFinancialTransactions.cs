using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.FinanceSkill;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        /// <summary>
        /// Lists individual financial transactions matching the supplied filters (at least one is required).
        /// Use this only when raw transaction data is explicitly needed; prefer the summarize tool for
        /// general analytical questions.
        ///
        /// When <paramref name="accountIdKeys"/> and/or <paramref name="campusIdKey"/> are provided, only
        /// transactions that contribute (>0) to the resolved set of accounts are returned, and the per-row
        /// TotalAmount/Accounts reflect only those contributing details (same behavior as summarize).
        /// </summary>
        /// <param name="personIdKey">Optional person IdKey.</param>
        /// <param name="campusIdKey">Optional campus (batch campus) IdKey.</param>
        /// <param name="accountIdKeys">
        /// Optional list of Account/Fund IdKeys. If supplied (or if only campusIdKey is supplied),
        /// the account set is resolved via GetFinancialAccountsForQuery and only contributions to that set are included.
        /// </param>
        /// <param name="paymentMethodTypeValueIdKey">Optional payment method type IdKey.</param>
        /// <param name="startDate">Optional inclusive start date.</param>
        /// <param name="endDate">Optional inclusive end date.</param>
        /// <param name="pageNumber">1-based page number.</param>
        /// <returns>Collection of <see cref="FinancialTransactionResult"/> records.</returns>
        [AgentToolGuid( "20FF0B2E-E403-48CE-B0C9-0CB6D80A7291" )]
        public RockToolResult ListFinancialTransactions(
            string personIdKey = null,
            string campusIdKey = null,
            List<string> accountIdKeys = null,
            string paymentMethodTypeValueIdKey = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1 )
        {
            // Require at least one filter, or punt to summarize tool.
            if ( personIdKey.IsNullOrWhiteSpace()
                && campusIdKey.IsNullOrWhiteSpace()
                && ( accountIdKeys == null || !accountIdKeys.Any() )
                && paymentMethodTypeValueIdKey.IsNullOrWhiteSpace()
                && !startDate.HasValue
                && !endDate.HasValue )
            {
                return RockToolResult.Error( "At least one filter must be provided to list financial transactions." )
                    .WithInstructions( "Call the SummarizeFinancialTransactions tool to get an aggregated form of the request." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();

            var personId = personIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( personIdKey ) : null;
            var campusId = campusIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( campusIdKey ) : null;
            var paymentMethodTypeId = paymentMethodTypeValueIdKey.IsNotNullOrWhiteSpace() ? IdHasher.Instance.GetId( paymentMethodTypeValueIdKey ) : null;

            var options = new FinancialTransactionQueryOptions
            {
                PersonId = personId,
                // BatchCampusId = campusId,
                PaymentMethodTypeId = paymentMethodTypeId,
                StartDate = startDate,
                EndDate = endDate
            };

            List<int> accountIds = new List<int>();
            if ( ( accountIdKeys?.Any() ?? false ) || campusIdKey.IsNotNullOrWhiteSpace() )
            {
                accountIds = GetFinancialAccountsForQuery( accountIdKeys ?? new List<string>(), campusIdKey, rockContext )
                    .Select( a => a.Id )
                    .ToList();

                if ( !accountIds.Any() )
                {
                    return RockToolResult.NoData()
                        .WithInstructions( "No active financial accounts matched the supplied accountIdKeys and/or campusIdKey." );
                }
            }

            var hasAccountFilter = accountIds?.Any() == true;

            // Paging (offset with N+1 lookahead) 
            var pgNumber = Math.Max( 1, pageNumber );
            const int basePageSize = 50;
            var offset = ( pgNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // ask for one extra row to detect hasMore

            // Base query + deterministic ordering (date desc, then id desc)
            var baseQry = GetFinancialTransactionsQueryable( rockContext, options )
                .Include( t => t.AuthorizedPersonAlias.Person )
                .Include( t => t.TransactionDetails.Select( d => d.Account ) );

            // If we have an account filter, only return transactions that actually contribute (>0)
            // to one of the filtered accounts (mirror Summarize's "effective" set).
            if ( hasAccountFilter )
            {
                baseQry = baseQry.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
            }

            baseQry = baseQry.OrderByDescending( t => t.TransactionDateTime )
                .ThenByDescending( t => t.Id );

            // Project AFTER ordering, BEFORE paging
            var projectedQry = baseQry.Select( ft => new FinancialTransactionResult
            {
                Id = ft.Id,
                AuthorizedPerson = new PersonResult
                {
                    Id = ft.AuthorizedPersonAlias.PersonId,
                    NickName = ft.AuthorizedPersonAlias.Person.NickName,
                    LastName = ft.AuthorizedPersonAlias.Person.LastName,
                    IncludePublicProfile = false,
                    IncludeAvatarUrl = false
                },
                TransactionDateTime = ft.TransactionDateTime,

                // Only sum details that match the resolved account set (if any)
                TotalAmount =
                    ft.TransactionDetails
                        .Where( d => !hasAccountFilter || accountIds.Contains( d.AccountId ) )
                        .Sum( d => ( decimal? ) d.Amount ) ?? 0m,

                // And only list those matching account details
                Accounts =
                    ft.TransactionDetails
                        .Where( td => !hasAccountFilter || accountIds.Contains( td.AccountId ) )
                        .Select( td => new FinancialAccountTransactionSummaryResult
                        {
                            Amount = td.Amount,
                            Name = td.Account.Name
                        } )
                        .ToList()
            } );

            var rows = projectedQry
                .Skip( offset )
                .Take( take )
                .ToList();

            var hasMore = rows.Count > basePageSize;
            if ( hasMore )
            {
                rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
            }

            var meta = new Dictionary<string, object>
            {
                { "filters", new Dictionary<string, object?>
                    {
                        { "personIdKey", personIdKey },
                        { "campusIdKey", campusIdKey },
                        { "accountIdKeys", accountIdKeys },
                        { "paymentMethodTypeValueIdKey", paymentMethodTypeValueIdKey },
                        { "startDate", startDate },
                        { "endDate", endDate }
                    }
                },
                { "pageNumber", pgNumber },
                { "pageSize", basePageSize },
                { "returnedRows", rows.Count },
                { "hasMore", hasMore }
            };

            if ( rows.Count == 0 )
            {
                return RockToolResult.NoData()
                    .WithMetadata( meta );
            }

            // Trimmed history content (unchanged)
            var trimmedForHistory = rows.Select( r => new
            {
                r.Id,
                r.TransactionDateTime,
                r.TotalAmount,
                AuthorizedPerson = new
                {
                    r.AuthorizedPerson.Id,
                    r.AuthorizedPerson.NickName,
                    r.AuthorizedPerson.LastName
                },
                PageNumber = pgNumber
            } ).ToList();

            // History key should include all accountIdKeys to keep variants distinct
            var historyKey = string.Concat(
                personIdKey,
                campusIdKey,
                accountIdKeys == null ? null : string.Join( "|", accountIdKeys ),
                paymentMethodTypeValueIdKey,
                startDate?.ToString( "o" ),
                endDate?.ToString( "o" ) ).XxHash();

            return RockToolResult.Success( rows )
                .WithMetadata( meta )
                .WithHistoryContent( trimmedForHistory, historyKey );
        }

        #endregion
    }
}
