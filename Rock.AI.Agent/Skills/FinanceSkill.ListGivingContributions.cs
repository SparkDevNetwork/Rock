using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class FinanceSkill
    {
        #region Tool(s)

        /// <summary>
        /// Lists the giving contribution transactions for the specified filters.
        /// </summary>
        /// <remarks>
        /// When <paramref name="accountIdKeys"/> and/or <paramref name="campusIdKey"/> are provided, only
        /// transactions that contribute (>0) to the resolved set of accounts are returned, and the per-row
        /// TotalAmount/Accounts reflect only those contributing details (same behavior as summarize).
        /// </remarks>
        /// <param name="personIdKey">Encoded person identifier.</param>
        /// <param name="campusIdKey">Encoded campus identifier.</param>
        /// <param name="accountIdKeys">Encoded account identifiers.</param>
        /// <param name="paymentMethodTypeValueIdKey">Encoded payment method identifier,</param>
        /// <param name="startDate">The start date to limit results to.</param>
        /// <param name="endDate">The end date to limit results to.</param>
        /// <param name="pageNumber">1-based page number.</param>
        /// <returns>Collection of <see cref="FinancialTransactionResult"/> records.</returns>
        [AgentToolGuid( "20FF0B2E-E403-48CE-B0C9-0CB6D80A7291" )]
        public IAgentToolResult ListGivingContributions(
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
                    .WithInstructions( $"Call the {nameof( GetGivingContributionInsights )} tool to get an aggregated form of the request." );
            }

            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid(), AgentRequestContext.RockContext ).Id;
            var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId );

            qry = WherePersonOrGivingGroup( qry, helper, personIdKey );

            var result = GetFinancialTransactionResult( helper, AgentRequestContext, qry, campusIdKey, accountIdKeys, paymentMethodTypeValueIdKey, startDate, endDate, pageNumber, null );

            if ( !helper.HasErrors && personIdKey.IsNotNullOrWhiteSpace() )
            {
                result = result.WithInstructions( "Note: This list may include transactions made by other people in the same giving group as the specified person. This is typically the same family, but not always." );
            }

            return result;
        }

        #endregion
    }
}
