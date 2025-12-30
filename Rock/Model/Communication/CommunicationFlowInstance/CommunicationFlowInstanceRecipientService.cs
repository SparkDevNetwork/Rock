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

using Rock.Core.EntitySearch;

namespace Rock.Model
{
    partial class CommunicationFlowInstanceService
    {
        /// <summary>
        /// Retrieves a queryable collection of Communication Flow Instances for a Communication Flow identifier.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the Communication Flow.</param>
        internal IQueryable<CommunicationFlowInstance> GetByCommunicationFlow( int communicationFlowId )
        {
            return Queryable().Where( cfi => cfi.CommunicationFlowId == communicationFlowId );
        }
    }

    internal static class CommunicationFlowInstanceServiceExtensions
    {
        /// <summary>
        /// An extension method to filter a queryable to only those that are active.
        /// </summary>
        internal static IQueryable<CommunicationFlowInstance> WhereHasRecipientWhoUnsubscribedFromCommunication( this IQueryable<CommunicationFlowInstance> queryable )
        {
            return queryable.Where( cfi => cfi.CommunicationFlowInstanceRecipients
                .Any( cfir =>
                    cfir.Status == Enums.Communication.CommunicationFlowInstanceRecipientStatus.Inactive
                    && cfir.InactiveReason == Enums.Communication.CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed
                )
            );
        }
    }
}
