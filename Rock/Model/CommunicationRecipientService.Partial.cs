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
        /// Returns a queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> by <see cref="Rock.Model.Communication"/> and <see cref="Rock.Model.CommunicationRecipientStatus"/>
        /// </summary>
        /// <param name="communicationId">A <see cref="System.Int32"/> representing the CommunicationId of the <see cref="Rock.Model.Communication"/> to search by.</param>
        /// <param name="status">A <see cref="Rock.Model.CommunicationRecipientStatus"/> Enum value representing the status of the communication submission.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the specified <see cref="Rock.Model.Communication"/> and <see cref="Rock.Model.CommunicationRecipientStatus"/></returns>
        public IQueryable<Rock.Model.CommunicationRecipient> Get( int communicationId, CommunicationRecipientStatus status )
        {
            return Repository.AsQueryable()
                .Where( r => 
                    r.CommunicationId == communicationId  &&
                    r.Status == status);
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> by <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <param name="communicationId">A <see cref="System.Int32"/> representing the CommunicationId of a  <see cref="Rock.Model.Communication"/> to search by.</param>
        /// <returns><A queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the specified <see cref="Rock.Model.Communication"/>.</returns>
        public IQueryable<Rock.Model.CommunicationRecipient> GetByCommunicationId( int communicationId )
        {
            return Repository.AsQueryable()
                .Where( r => r.CommunicationId == communicationId );
        }

    }
}
