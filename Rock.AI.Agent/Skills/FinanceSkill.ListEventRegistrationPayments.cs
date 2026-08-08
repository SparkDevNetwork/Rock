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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Lists transactions of type 'Event Registration' that match the provided filters." )]
    [AgentToolGuid( "90732068-5e8d-48cf-8cd8-8eb05c5a27fb" )]
    public AgentToolResult ListEventRegistrationPayments(
        string personIdKey = null,
        string campusIdKey = null,
        List<string> accountIdKeys = null,
        string paymentMethodTypeValueIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1 )
    {
        // When accountIdKeys and/or campusIdKey are provided, only transactions
        // that contribute (>0) to the resolved set of accounts are returned,
        // and the per-row TotalAmount/Accounts reflect only those contributing
        // details (same behavior as summarize).

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

        var result = GetFinancialTransactionResult( helper, AgentRequestContext, qry, campusIdKey, accountIdKeys, paymentMethodTypeValueIdKey, startDate, endDate, pageNumber, items =>
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
