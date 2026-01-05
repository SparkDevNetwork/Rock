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
    partial class CommunicationFlowInstanceRecipientService
    {
        /// <summary>
        /// Retrieves a queryable collection of Communication Flow Instance Recipients for a Communication Flow identifier.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the Communication Flow.</param>
        internal IQueryable<CommunicationFlowInstanceRecipient> GetByCommunicationFlow( int communicationFlowId )
        {
            return Queryable().Where( cfir => cfir.CommunicationFlowInstance.CommunicationFlowId == communicationFlowId );
        }
        
        /// <summary>
        /// Retrieves a queryable collection of Communication Flow Instance Recipients for a Communication Flow Instance identifier.
        /// </summary>
        /// <param name="communicationFlowInstanceId">The identifier of the Communication Flow Instance.</param>
        internal IQueryable<CommunicationFlowInstanceRecipient> GetByCommunicationFlowInstance( int communicationFlowInstanceId )
        {
            return Queryable().Where( cfir => cfir.CommunicationFlowInstanceId == communicationFlowInstanceId );
        }
    }

    internal static class CommunicationFlowInstanceRecipientServiceExtensions
    {
        /// <summary>
        /// An extension method to filter a queryable to only those that who unsubscribed from a Communication or from a Communication Flow.
        /// </summary>
        internal static IQueryable<CommunicationFlowInstanceRecipient> WhereUnsubscribed( this IQueryable<CommunicationFlowInstanceRecipient> queryable )
        {
            return queryable.Where( CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedExpr );
        }
    
        /// <summary>
        /// An extension method to filter a queryable to only those that who unsubscribed from a communication.
        /// </summary>
        /// <remarks>Recipients who unsubscribe via a specific Communication (via unsubscribe link) only unsubscribe from Communications in the current Communication Flow Instance.</remarks>
        internal static IQueryable<CommunicationFlowInstanceRecipient> WhereUnsubscribedFromCommunication( this IQueryable<CommunicationFlowInstanceRecipient> queryable )
        {
            return queryable.Where( CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedFromCommunicationExpr );
        }
    
        /// <summary>
        /// An extension method to filter a queryable to only those who unsubscribed from a communication flow.
        /// </summary>
        /// <remarks>Recipients who unsubscribe from a Communication Flow (via Email Preference Entry) unsubscribe from Communications in the current and future Communication Flow Instances.</remarks>
        internal static IQueryable<CommunicationFlowInstanceRecipient> WhereUnsubscribedFromCommunicationFlow( this IQueryable<CommunicationFlowInstanceRecipient> queryable )
        {
            return queryable.Where( CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedFromCommunicationFlowExpr );
        }
    }
}
