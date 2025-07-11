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

using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A configuration class to provide filters when querying for <see cref="Person"/> entities who need to receive a
    /// fallback notification to alert them of recent chat activity.
    /// </summary>
    internal class FallbackChatNotificationsConfig
    {
        /// <summary>
        /// Gets or sets the identifier of the <see cref="Group"/> (chat channel) whose members should be queried.
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the optional list of <see cref="ChatUser.Key"/>s that should be used to query for Rock
        /// <see cref="Person"/> records if <see cref="GroupId"/> is not provided.
        /// </summary>
        public HashSet<string> MemberChatUserKeys { get; set; }

        /// <summary>
        /// Gets or sets the identifier of a <see cref="Person"/> to exclude from the returned query.
        /// </summary>
        /// <remarks>
        /// This is most likely the identifier of the <see cref="Person"/> responsible for triggering the need to send
        /// notifications to other members of the chat channel, and therefore does not need to be notified.
        /// </remarks>
        public int? PersonIdToExclude { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="SystemCommunication"/> that will be sent.
        /// </summary>
        /// <remarks>
        /// This will be used along with <see cref="NotificationSuppressionMinutes"/> to ensure we don't sent a fallback
        /// notification to a given <see cref="Person"/> too often.
        /// </remarks>
        public int SystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes to suppress fallback notifications if the <see cref="Person"/> has
        /// already received a recent notification.
        /// </summary>
        public int NotificationSuppressionMinutes { get; set; } = 60;
    }
}
