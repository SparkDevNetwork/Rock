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

using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Enums.Communication;

namespace Rock.Model
{
    public partial class CommunicationFlowService
    {
        private static readonly IEnumerable<CommunicationFlowInstanceRecipientInactiveReason> UnsubscribedInactiveReasons = new List<CommunicationFlowInstanceRecipientInactiveReason>
        {
            CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed,
            CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow
        };

        /// <summary>
        /// Unsubscribes a person from a flow, preventing them from being a recipient of future communications for this communication flow.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the communication flow to unsubscribe from.</param>
        /// <param name="personId">The identifier of the person to unsubscribe.</param>
        public void UnsubscribePersonFromFlow( int communicationFlowId, int personId )
        {
            foreach ( var communicationFlowInstanceRecipient in new CommunicationFlowInstanceRecipientService( ( RockContext ) Context )
                .Queryable()
                .Where( r =>
                    r.CommunicationFlowInstance.CommunicationFlowId == communicationFlowId
                    && r.RecipientPersonAlias.PersonId == personId
                    && r.Status == CommunicationFlowInstanceRecipientStatus.Active ) )
            {
                communicationFlowInstanceRecipient.Status = CommunicationFlowInstanceRecipientStatus.Inactive;
                communicationFlowInstanceRecipient.InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow;
            }
        }

        /// <summary>
        /// Resubscribes a person to a flow from which they unsubscribed, allowing them to be a recipient of future communications for this communication flow.
        /// </summary>
        /// <param name="communicationFlowId">The identifier of the communication flow to resubscribe to.</param>
        /// <param name="personId">The identifier of the person to resubscribe.</param>
        public void ResubscribePersonToFlow( int communicationFlowId, int personId )
        {
            foreach ( var communicationFlowInstanceRecipient in new CommunicationFlowInstanceRecipientService( ( RockContext ) Context )
                .Queryable()
                .Where( r =>
                    r.CommunicationFlowInstance.CommunicationFlowId == communicationFlowId
                    && r.RecipientPersonAlias.PersonId == personId
                    && r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                    && r.InactiveReason.HasValue
                    && UnsubscribedInactiveReasons.Contains( r.InactiveReason.Value ) ) )
            {
                communicationFlowInstanceRecipient.Status = CommunicationFlowInstanceRecipientStatus.Active;
                communicationFlowInstanceRecipient.InactiveReason = null;
            }
        }
    }
}
