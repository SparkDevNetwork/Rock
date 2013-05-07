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
    /// Communication Recipient POCO Service class
    /// </summary>
    public partial class CommunicationRecipientService 
    {
        /// <summary>
        /// Gets the specified communication id.
        /// </summary>
        /// <param name="communicationId">The communication id.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public IQueryable<Rock.Model.CommunicationRecipient> Get( int communicationId, CommunicationRecipientStatus status )
        {
            return Repository.AsQueryable()
                .Where( r => 
                    r.CommunicationId == communicationId  &&
                    r.Status == status);
        }

        /// <summary>
        /// Gets the by communication id.
        /// </summary>
        /// <param name="communicationId">The communication id.</param>
        /// <returns></returns>
        public IQueryable<Rock.Model.CommunicationRecipient> GetByCommunicationId( int communicationId )
        {
            return Repository.AsQueryable()
                .Where( r => r.CommunicationId == communicationId );
        }

    }
}
