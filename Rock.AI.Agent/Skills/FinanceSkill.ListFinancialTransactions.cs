using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;

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
        public IAgentToolResult ListFinancialTransactions(
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
                return Error( "At least one filter must be provided to list financial transactions." )
                    .WithInstructions( $"Call the {nameof( GetFinancialTransactionInsights )} tool to get an aggregated form of the request." );
            }

            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var qry = new FinancialTransactionService( AgentRequestContext.RockContext ).Queryable();

            helper.WhereOptionalIdKey( qry, ft => ft.AuthorizedPersonAlias.PersonId, personIdKey );
            helper.WhereOptionalIdKey( qry, ft => ft.Batch.CampusId, campusIdKey );
            helper.WhereOptionalIdKey( qry, ft => ft.FinancialPaymentDetail.CurrencyTypeValueId, paymentMethodTypeValueIdKey );
            helper.WhereOptionalPropertyBetween( qry, ft => ft.TransactionDateTime, startDate, endDate );

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

            qry = qry.OrderByDescending( t => t.TransactionDateTime )
                .ThenByDescending( t => t.Id );

            // Project AFTER ordering, BEFORE paging
            var projectedQry = qry.Select( ft => new FinancialTransactionResult
            {
                Id = ft.Id,
                AuthorizedPerson = PersonResult.NameOnly( ft.AuthorizedPersonAlias ),
                TransactionDateTime = ft.TransactionDateTime,

                // Only sum details that match the resolved account set (if any)
                TotalAmount = ft.TransactionDetails
                    .Where( d => !hasAccountFilter || accountIds.Contains( d.AccountId ) )
                    .Sum( d => ( decimal? ) d.Amount ) ?? 0m,

                // And only list those matching account details
                Accounts = ft.TransactionDetails
                    .Where( td => !hasAccountFilter || accountIds.Contains( td.AccountId ) )
                    .Select( td => new FinancialAccountTransactionSummaryResult
                    {
                        Amount = td.Amount,
                        Name = td.Account.Name
                    } )
                    .ToList()
            } );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var page = helper.GetPaginatedItems( projectedQry, pageNumber );

            // Trimmed history content (unchanged)
            var historyItems = page.Items.Select( r => new
            {
                r.Id,
                r.TransactionDateTime,
                r.TotalAmount,
                r.AuthorizedPerson,
            } ).ToList();

            // History key should include all accountIdKeys to keep variants distinct
            var historyKey = string.Concat(
                personIdKey,
                campusIdKey,
                accountIdKeys == null ? null : string.Join( "|", accountIdKeys ),
                paymentMethodTypeValueIdKey,
                startDate?.ToString( "o" ),
                endDate?.ToString( "o" ) ).XxHash();

            return helper.GetPaginatedResult( page, page.WithItems( historyItems ) )
                .WithHistoryKey( historyKey );
        }

        #endregion
    }
}
