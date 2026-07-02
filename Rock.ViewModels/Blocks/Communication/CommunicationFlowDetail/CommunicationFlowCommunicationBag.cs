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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// The communication flow communication details for the Communication Flow Detail block.
    /// </summary>
    public class CommunicationFlowCommunicationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this message within the flow.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of this message.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of days to wait, after the previous message in the flow, before this message is sent.
        /// </summary>
        public int DaysToWait { get; set; }

        /// <summary>
        /// Gets or sets the time of day at which this message is sent.
        /// </summary>
        public TimeSpan TimeToSend { get; set; }

        /// <summary>
        /// Gets or sets the medium (such as Email or SMS) used to send this message.
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the communication template that provides this message's content.
        /// </summary>
        public CommunicationFlowDetailCommunicationTemplateBag CommunicationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the position of this message within the flow's sequence of messages.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the email address a test of this message is sent to.
        /// </summary>
        public string TestEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the phone number a test of this message is sent to.
        /// </summary>
        public string TestSmsPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message has associated performance data,
        /// meaning it has already been sent to recipients as part of at least one flow instance.
        /// </summary>
        public bool HasPerformanceData { get; set; }
    }
}
