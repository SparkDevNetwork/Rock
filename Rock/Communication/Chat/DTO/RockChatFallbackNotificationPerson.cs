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
using Rock.Model;

namespace Rock.Communication.Chat.DTO
{
    /// <summary>
    /// Represents a <see cref="Person"/> and their <see cref="GroupMember.CommunicationPreference"/> - if applicable -
    /// for a <see cref="ChatUser"/> who needs to receive a fallback notification to alert them of recent chat activity.
    /// </summary>
    internal class RockChatFallbackNotificationPerson
    {
        /// <summary>
        /// Gets or sets the <see cref="Person"/> who should receive a fallback notification.
        /// </summary>
        public Person RecipientPerson { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CommunicationType"/> preference of the <see cref="GroupMember"/> record that
        /// represents this <see cref="ChatChannelMember"/>, if applicable.
        /// </summary>
        public CommunicationType? GroupMemberCommunicationPreference { get; set; }
    }
}
