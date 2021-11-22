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
//
using System.Linq;

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
                    r.Status == status && r.PersonAlias.Person.IsDeceased == false );
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

    #region Extension Methods
    /// <summary>
    /// Extension methods used to query GroupSync.
    /// </summary>
    public static class CommunicationRecipientServiceExtensions
    {
        /// <summary>
        /// Gets the Communication Recipients for the specified communication id.
        /// </summary>
        /// <param name="communicationRecipient">The communication recipient.</param>
        /// <param name="communicationId">The communication identifier.</param>
        /// <returns></returns>
        public static IQueryable<CommunicationRecipient> ByCommunicationId( this IQueryable<CommunicationRecipient> communicationRecipient, int communicationId )
        {
            return communicationRecipient.Where( x => x.CommunicationId == communicationId);
        }

        /// <summary>
        /// Gets the Communication Recipients for the specified status.
        /// </summary>
        /// <param name="communicationRecipient">The communication recipient.</param>
        /// <param name="communicationRecipientStatus">The communication recipient status.</param>
        /// <returns></returns>
        public static IQueryable<CommunicationRecipient> ByStatus( this IQueryable<CommunicationRecipient> communicationRecipient, CommunicationRecipientStatus communicationRecipientStatus )
        {
            return communicationRecipient.Where( x => x.Status == communicationRecipientStatus );
        }

        /// <summary>
        /// Gets the Communication Recipients for the specified medium entity type identifier.
        /// </summary>
        /// <param name="communicationRecipient">The communication recipient.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <returns></returns>
        public static IQueryable<CommunicationRecipient> ByMediumEntityTypeId( this IQueryable<CommunicationRecipient> communicationRecipient, int mediumEntityTypeId )
        {
            return communicationRecipient
                        .Where( x => x.MediumEntityTypeId != null)
                        .Where(x => x.MediumEntityTypeId == mediumEntityTypeId );
        }
    }
    #endregion
}
