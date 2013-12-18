//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
                return Repository.AsQueryable()
                    .Where( t => 
                        t.AuthorizedPerson.GivingGroupId == givingGroupId.Value &&
                        (t.IsActive || includeInactive) )
                    .OrderByDescending(t => t.IsActive)
                    .ThenByDescending(t => t.StartDate);
            }
            else
            {
                return Repository.AsQueryable()
                    .Where( t =>
                        t.AuthorizedPersonId == personId &&
                        ( t.IsActive || includeInactive ) )
                    .OrderByDescending( t => t.IsActive )
                    .ThenByDescending( t => t.StartDate );
            }
        }
    }
}