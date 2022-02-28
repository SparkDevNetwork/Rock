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
            return communicationRecipient.Where( x => x.CommunicationId == communicationId );
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
                        .Where( x => x.MediumEntityTypeId != null )
                        .Where( x => x.MediumEntityTypeId == mediumEntityTypeId );
        }
    }
}
