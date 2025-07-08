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

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Handles execution for the <see cref="SendFallbackChatNotification"/> event component.
    /// </summary>
    class SendFallbackChatNotificationExecutor : AutomationEventExecutor
    {
        #region Fields

        /// <summary>
        /// The unique identifier of the system communication that will be sent.
        /// </summary>
        private readonly Guid _systemCommunicationGuid;

        /// <summary>
        /// The number of minutes the system will suppress notifications if the recipient has already received a
        /// recent notification and has not yet read the chat message that triggered it.
        /// </summary>
        private readonly int _notificationSuppressionMinutes;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SendFallbackChatNotificationExecutor"/> class.
        /// </summary>
        /// <param name="systemCommunicationGuid">
        /// The unique identifier of the system communication that will be sent.
        /// </param>
        /// <param name="notificationSuppressionMinutes">
        /// he number of minutes the system will suppress notifications if the recipient has already received a recent
        /// notification and has not yet read the chat message that triggered it.
        /// </param>
        public SendFallbackChatNotificationExecutor( Guid systemCommunicationGuid, int notificationSuppressionMinutes )
        {
            _systemCommunicationGuid = systemCommunicationGuid;
            _notificationSuppressionMinutes = notificationSuppressionMinutes;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            /*
                TODO (Jason): This is where the bulk of the work will take place.

                1. TBD...
                2. ...
             */
        }

        #endregion
    }
}
