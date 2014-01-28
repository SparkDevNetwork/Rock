// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialScheduledTransaction"/> entity objects.
    /// </summary>
    public partial class FinancialScheduledTransactionService 
    {
        /// <summary>
        /// Gets schedule transactions associated to a person.  Includes any transactions associated to person
        /// or any other perosn with same giving group id
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="givingGroupId">The giving group identifier.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns>
        /// The <see cref="Rock.Model.FinancialTransaction" /> that matches the transaction code, this value will be null if a match is not found.
        /// </returns>
        public IQueryable<FinancialScheduledTransaction> Get( int personId, int? givingGroupId, bool includeInactive )
        {
            if ( givingGroupId.HasValue )
            {
                return Repository.AsQueryable( "ScheduledTransactionDetails" )
                    .Where( t => 
                        t.AuthorizedPerson.GivingGroupId == givingGroupId.Value &&
                        (t.IsActive || includeInactive) )
                    .OrderByDescending(t => t.IsActive)
                    .ThenByDescending(t => t.StartDate);
            }
            else
            {
                return Repository.AsQueryable( "ScheduledTransactionDetails" )
                    .Where( t =>
                        t.AuthorizedPersonId == personId &&
                        ( t.IsActive || includeInactive ) )
                    .OrderByDescending( t => t.IsActive )
                    .ThenByDescending( t => t.StartDate );
            }
        }

        /// <summary>
        /// Gets the by schedule identifier.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns></returns>
        public FinancialScheduledTransaction GetByScheduleId( string scheduleId )
        {
            return Repository.AsQueryable()
                .Where( t => t.GatewayScheduleId == scheduleId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Updates the status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="currentPersonAlias">The current person alias.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool UpdateStatus(FinancialScheduledTransaction scheduledTransaction, PersonAlias currentPersonAlias, out string errorMessages)
        {
            if ( scheduledTransaction.GatewayEntityType != null )
            {
                var gateway = Rock.Financial.GatewayContainer.GetComponent( scheduledTransaction.GatewayEntityType.Guid.ToString() );
                if ( gateway != null && gateway.IsActive )
                {
                    if ( gateway.GetScheduledPaymentStatus( scheduledTransaction, out errorMessages ) )
                    {
                        Save( scheduledTransaction, currentPersonAlias );
                        return true;
                    }
                }
            }

            errorMessages = "Gateway is invalid or not active";
            return false;

        }
    }
}