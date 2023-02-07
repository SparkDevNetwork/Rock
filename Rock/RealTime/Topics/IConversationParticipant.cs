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

using System.Threading.Tasks;

using Rock.ViewModels.Communication;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// The methods that can be invoked on conversation participant clients.
    /// </summary>
    internal interface IConversationParticipant
    {
        /// <summary>
        /// Called when a new SMS message has been added to a conversation.
        /// </summary>
        /// <param name="message">The new message details.</param>
        Task NewSmsMessage( ConversationMessageBag message );

        /// <summary>
        /// Called when a conversation has been marked as read by an individual.
        /// </summary>
        /// <param name="conversationKey">The conversation key that has been read.</param>
        Task ConversationMarkedAsRead( string conversationKey );
    }
}
