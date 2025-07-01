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

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the detail of a communication.
    /// </summary>
    public class CommunicationDetailBag
    {
        /// <summary>
        /// Gets or sets the name [or subject or push title] of the communication.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CommunicationType"/> for this communication.
        /// </summary>
        public CommunicationType Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="InferredCommunicationStatus"/> for this communication.
        /// </summary>
        public InferredCommunicationStatus InferredStatus { get; set; }

        /// <summary>
        /// Gets or sets the future send datetime for this communication.
        /// </summary>
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the send datetime for this communication.
        /// </summary>
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the total count of all recipients tied to this communication.
        /// </summary>
        public int TotalRecipientCount { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication flow this communication belongs to.
        /// </summary>
        public string CommunicationFlowName { get; set; }

        /// <summary>
        /// Gets or sets the topic of this communication.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets whether this is a bulk communication.
        /// </summary>
        public bool IsBulk { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CommunicationDeliveryBreakdownBag"/> for this communication.
        /// </summary>
        public CommunicationDeliveryBreakdownBag DeliveryBreakdown { get; set; }
    }
}
