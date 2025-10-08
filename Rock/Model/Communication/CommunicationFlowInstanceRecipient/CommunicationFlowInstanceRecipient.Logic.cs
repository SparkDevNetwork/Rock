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
using System;
using System.Linq.Expressions;

using Rock.Enums.Communication;

namespace Rock.Model
{
    public partial class CommunicationFlowInstanceRecipient
    {
        // In-memory versions, compiled once per AppDomain
        private static readonly Func<CommunicationFlowInstanceRecipient, bool> _isUnsubscribedFromCommunicationFunc =
            CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedFromCommunicationExpr.Compile();
        private static readonly Func<CommunicationFlowInstanceRecipient, bool> _isUnsubscribedFromCommunicationFlowFunc =
            CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedFromCommunicationFlowExpr.Compile();
        private static readonly Func<CommunicationFlowInstanceRecipient, Person> _personFunc =
            CommunicationFlowInstanceRecipientPredicates.PersonExpr.Compile();
        private static readonly Func<CommunicationFlowInstanceRecipient, int> _personIdFunc =
            CommunicationFlowInstanceRecipientPredicates.PersonIdExpr.Compile();

        /// <summary>
        /// Determines whether the recipient unsubscribed via a specific Communication.
        /// </summary>
        /// <remarks>Recipients who unsubscribe via a specific Communication (via unsubscribe link) only unsubscribe from Communications in the current Communication Flow Instance.</remarks>
        internal bool IsUnsubscribedFromCommunication()
        {
            return _isUnsubscribedFromCommunicationFunc( this );
        }

        /// <summary>
        /// Determines whether the recipient is unsubscribed from a Communication flow.
        /// </summary>
        /// <remarks>Recipients who unsubscribe from a Communication Flow (via Email Preference Entry) unsubscribe from Communications in the current and future Communication Flow Instances.</remarks>
        internal bool IsUnsubscribedFromCommunicationFlow()
        {
            return _isUnsubscribedFromCommunicationFlowFunc( this );
        }

        /// <summary>
        /// Gets the Person associated with this CommunicationFlowInstanceRecipient.
        /// </summary>
        public Person GetPerson()
        {
            return _personFunc( this );
        }

        /// <summary>
        /// Gets the Person identifier associated with this CommunicationFlowInstanceRecipient.
        /// </summary>
        public int GetPersonId()
        {
            return _personIdFunc( this );
        }
    }

    internal static partial class CommunicationFlowInstanceRecipientPredicates
    {
        /// <summary>
        /// Expression to determine if a Communication Flow Instance Recipient is unsubscribed from a Communication.
        /// </summary>
        /// <remarks>Recipients who unsubscribe via a specific Communication (via unsubscribe link) only unsubscribe from Communications in the current Communication Flow Instance.</remarks>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, bool>> IsUnsubscribedFromCommunicationExpr =
            r => r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                && r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed;

        /// <summary>
        /// Expression to determine if a Communication Flow Instance Recipient is unsubscribed from a Communication or a Communication Flow.
        /// </summary>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, bool>> IsUnsubscribedExpr =
            r => r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                && ( r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed
                    || r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow );

        /// <summary>
        /// Expression to determine if a Communication Flow Instance Recipient is unsubscribed from a Communication Flow.
        /// </summary>
        /// <remarks>Recipients who unsubscribe from a Communication Flow (via Email Preference Entry) unsubscribe from Communications in the current and future Communication Flow Instances.</remarks>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, bool>> IsUnsubscribedFromCommunicationFlowExpr =
            r => r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                && r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.UnsubscribedFromFlow;

        /// <summary>
        /// Expression to get the Person associated with a CommunicationFlowInstanceRecipient.
        /// </summary>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, Person>> PersonExpr =
            r => r.RecipientPersonAlias.Person;

        /// <summary>
        /// Expression to get the Person identifier associated with a CommunicationFlowInstanceRecipient.
        /// </summary>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, int>> PersonIdExpr =
            r => r.RecipientPersonAlias.PersonId;
    }
}
