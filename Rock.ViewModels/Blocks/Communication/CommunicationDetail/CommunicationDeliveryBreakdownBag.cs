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

using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the delivery breakdown for a communication.
    /// </summary>
    public class CommunicationDeliveryBreakdownBag
    {
        /// <summary>
        /// Gets or sets the <see cref="Rock.Enums.Communication.CommunicationType"/> represented within these values.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the total count of recipients tied to this communication and <see cref="Rock.Enums.Communication.CommunicationType"/>.
        /// </summary>
        public int RecipientCount { get; set; }

        /// <summary>
        /// Gets or sets the count of recipients for whom communications of this type are still pending (not yet sent).
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients for whom communications of this type are still pending (not yet sent).
        /// </summary>
        public decimal PendingPercentage { get; set; }

        /// <summary>
        /// Gets or sets the count of recipients for whom communications of this type were delivered.
        /// </summary>
        public int DeliveredCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients for whom communications of this type were delivered.
        /// </summary>
        public decimal DeliveredPercentage { get; set; }

        /// <summary>
        /// Gets or sets the count of recipients for whom communications of this type failed to be delivered.
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients for whom communications of this type failed to be delivered.
        /// </summary>
        public decimal FailedPercentage { get; set; }

        /// <summary>
        /// Gets or sets the count of recipients for whom communications of this type were cancelled.
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients for whom communications of this type were cancelled.
        /// </summary>
        public decimal CancelledPercentage { get; set; }
    }
}
