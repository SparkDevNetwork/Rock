using System;
using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
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
        /// Lists the event registration payment transactions for the specified filters.
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
        [AgentToolGuid( "90732068-5e8d-48cf-8cd8-8eb05c5a27fb" )]
        public IAgentToolResult ListEventRegistrationPayments(
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
            var contributionTransactionValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid(), AgentRequestContext.RockContext ).Id;
            var qry = new FinancialTransactionService( AgentRequestContext.RockContext )
                .Queryable()
                .Where( ft => ft.TransactionTypeValueId == contributionTransactionValueId );

            qry = helper.WhereOptionalIdKey( qry, ft => ft.AuthorizedPersonAlias.PersonId, personIdKey );

            var result = GetFinancialTransactionResult( helper, qry, campusIdKey, accountIdKeys, paymentMethodTypeValueIdKey, startDate, endDate, pageNumber, items =>
            {
                var registrationEntityTypeId = EntityTypeCache.Get<Registration>( true, AgentRequestContext.RockContext ).Id;
                var registrationIds = items
                    .SelectMany( t => t.Accounts.Where( a => a.EntityTypeId == registrationEntityTypeId ).Select( a => a.EntityId ) ).ToList();

                if ( registrationIds.Any() )
                {
                    var registrationNames = new RegistrationService( AgentRequestContext.RockContext ).Queryable()
                        .Where( r => registrationIds.Contains( r.Id ) )
                        .Select( r => new
                        {
                            r.Id,
                            Name = r.RegistrationInstance.RegistrationTemplate.Name + " - " + r.RegistrationInstance.Name,
                        } )
                        .ToDictionary( r => r.Id, r => r.Name );

                    foreach ( var item in items )
                    {
                        foreach ( var account in item.Accounts )
                        {
                            if ( account.EntityTypeId != registrationEntityTypeId || !account.EntityId.HasValue )
                            {
                                continue;
                            }

                            if ( registrationNames.TryGetValue( account.EntityId.Value, out var registrationName ) )
                            {
                                account.RelatedEntity = new KeyNameResult( account.EntityId.Value, registrationName );
                            }
                        }
                    }
                }

            } );

            return result;
        }

        #endregion
    }
}
