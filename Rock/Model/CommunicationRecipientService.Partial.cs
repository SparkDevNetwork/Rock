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
            return Queryable( "Communication,PersonAlias.Person" )
                .Where( r =>
                    r.CommunicationId == communicationId &&
                    r.Status == status &&
                    ( !r.PersonAlias.Person.IsDeceased.HasValue || !r.PersonAlias.Person.IsDeceased.Value ) );
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> by <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <param name="communicationId">A <see cref="System.Int32"/> representing the CommunicationId of a  <see cref="Rock.Model.Communication"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the specified <see cref="Rock.Model.Communication"/>.</returns>
        public IQueryable<Rock.Model.CommunicationRecipient> GetByCommunicationId( int communicationId )
        {
            return Queryable( "Communication,PersonAlias.Person" )
                .Where( r => r.CommunicationId == communicationId );
        }

    }
}
