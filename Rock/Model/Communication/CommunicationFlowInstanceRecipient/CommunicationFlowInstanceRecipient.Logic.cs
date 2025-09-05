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
        private static readonly Func<CommunicationFlowInstanceRecipient, bool> _isUnsubscribedFunc =
            CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedExpr.Compile();
        private static readonly Func<CommunicationFlowInstanceRecipient, Person> _personFunc =
            CommunicationFlowInstanceRecipientPredicates.PersonExpr.Compile();
        private static readonly Func<CommunicationFlowInstanceRecipient, int> _personIdFunc =
            CommunicationFlowInstanceRecipientPredicates.PersonIdExpr.Compile();

        /// <summary>
        /// Determines whether the recipient is unsubscribed from the Communication Flow Instance.
        /// </summary>
        /// <returns></returns>
        public bool IsUnsubscribed()
        {
            return _isUnsubscribedFunc( this );
        }

        /// <summary>
        /// Marks the recipient as unsubscribed from the Communication Flow Instance.
        /// </summary>
        public void MarkUnsubscribed()
        {
            // FYI, update CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedExpr if you change this logic.
            Status = CommunicationFlowInstanceRecipientStatus.Inactive;
            InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed;
            
            UnsubscribeCommunicationRecipient = null;
            UnsubscribeCommunicationRecipientId = null;
        }

        /// <summary>
        /// Marks the recipient as unsubscribed from the Communication Flow Instance.
        /// </summary>
        public void MarkUnsubscribedFromCommunication( CommunicationRecipient unsubscribeCommunicationRecipient )
        {
            // FYI, update CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedExpr if you change this logic.
            Status = CommunicationFlowInstanceRecipientStatus.Inactive;
            InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed;

            UnsubscribeCommunicationRecipient = unsubscribeCommunicationRecipient;
            UnsubscribeCommunicationRecipientId = unsubscribeCommunicationRecipient?.Id;
        }

        /// <summary>
        /// Marks the recipient as unsubscribed from the Communication Flow Instance.
        /// </summary>
        public void MarkUnsubscribedFromCommunication( int? unsubscribeCommunicationRecipientId )
        {
            // FYI, update CommunicationFlowInstanceRecipientPredicates.IsUnsubscribedExpr if you change this logic.
            Status = CommunicationFlowInstanceRecipientStatus.Inactive;
            InactiveReason = CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed;

            UnsubscribeCommunicationRecipientId = unsubscribeCommunicationRecipientId;
            // Don't touch the UnsubscribeCommunicationRecipient navigation property, as it may not be loaded.
        }

        /// <summary>
        /// Gets the Person associated with this CommunicationFlowInstanceRecipient.
        /// </summary>
        /// <returns></returns>
        public Person GetPerson()
        {
            return _personFunc( this );
        }

        /// <summary>
        /// Gets the Person identifier associated with this CommunicationFlowInstanceRecipient.
        /// </summary>
        /// <returns></returns>
        public int GetPersonId()
        {
            return _personIdFunc( this );
        }
    }

    internal static partial class CommunicationFlowInstanceRecipientPredicates
    {
        /// <summary>
        /// Expression to determine if a CommunicationFlowInstanceRecipient is unsubscribed.
        /// </summary>
        /// <remarks>Update <see cref="CommunicationFlowInstanceRecipient.MarkUnsubscribed"/> if this is changed.</remarks>
        internal static readonly Expression<Func<CommunicationFlowInstanceRecipient, bool>> IsUnsubscribedExpr =
            r => r.Status == CommunicationFlowInstanceRecipientStatus.Inactive
                && r.InactiveReason == CommunicationFlowInstanceRecipientInactiveReason.Unsubscribed;

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
