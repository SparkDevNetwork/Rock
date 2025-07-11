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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// The inactive reason of a Communication Flow Instance Recipient.
    /// </summary>
    public enum CommunicationFlowInstanceRecipientInactiveReason
    {
        /// <summary>
        /// The recipient is inactive because they have unsubscribed from the communication.
        /// </summary>
        Unsubscribed = 0,

        /// <summary>
        /// The recipient is inactive because they have met the conversion goal of the communication flow instance.
        /// </summary>
        ConversionGoalMet = 1,

        /// <summary>
        /// The recipient is inactive because they have opened a communication.
        /// </summary>
        OpenedCommunication = 2,

        /// <summary>
        /// The recipient is inactive because they have clicked a communication link.
        /// </summary>
        ClickedCommunication = 3
    }
}
