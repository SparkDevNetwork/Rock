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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the response information after sending a communication via the Communication Entry block.
    /// </summary>
    public class CommunicationEntrySendResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether to redirect view mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if needed to redirect to view mode; otherwise, <c>false</c>.
        /// </value>
        public bool RedirectToViewMode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the communication status.
        /// </summary>
        /// <value>
        /// The communication status.
        /// </value>
        public CommunicationStatus? CommunicationStatus { get; set; }

        /// <summary>
        /// Gets the communication identifier.
        /// </summary>
        /// <value>
        /// The communication identifier.
        /// </value>
        public int CommunicationId { get; set; }

        /// <summary>
        /// Gets the communication unique identifier.
        /// </summary>
        /// <value>
        /// The communication unique identifier.
        /// </value>
        public Guid CommunicationGuid { get; set; }

        /// <summary>
        /// Gets a value indicating whether the page has a detail block for viewing the communication details.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the page has a detail block for viewing the communication details; otherwise, <c>false</c>.
        /// </value>
        public bool HasDetailBlockOnCurrentPage { get; set; }
    }
}
