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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The outcome of a request to send confirmation communications to scheduled individuals.
    /// </summary>
    public class GroupSchedulerSendConfirmationsResponseBag
    {
        /// <summary>
        /// Gets or sets the errors encountered while attempting to send communications.
        /// </summary>
        /// <value>
        /// The errors encountered while attempting to send communications.
        /// </value>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Gets or sets the warnings encountered while attempting to send communications.
        /// </summary>
        /// <value>
        /// The warnings encountered while attempting to send communications.
        /// </value>
        public List<string> Warnings { get; set; }

        /// <summary>
        /// Gets or sets whether there are any communications to send.
        /// </summary>
        /// <value>
        /// Whether there are any communications to send.
        /// </value>
        public bool AnyCommunicationsToSend { get; set; }

        /// <summary>
        /// Gets or sets the count of communications sent.
        /// </summary>
        /// <value>
        /// The count of communications sent.
        /// </value>
        public int CommunicationsSentCount { get; set; }
    }
}
