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
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// This topic is used by the Conversation system, currently just SMS. It
    /// handles the real-time notification about updates in conversations between
    /// Rock and outside individuals.
    /// </summary>
    [RealTimeTopic]
    internal class ConversationParticipantTopic : Topic<IConversationParticipant>
    {
        /// <summary>
        /// Joins an SMS number to begin receiving notifications about
        /// events related to the number.
        /// </summary>
        /// <param name="rockPhoneNumber">The rock phone number unique identifier.</param>
        public async Task JoinSmsNumber( Guid rockPhoneNumber )
        {
            var state = this.GetConnectionState<ConversationState>( Context.ConnectionId );
            var definedValue = DefinedValueCache.Get( rockPhoneNumber );

            if ( definedValue == null )
            {
                throw new RealTimeException( "Phone number was not found." );
            }

            using ( var rockContext = new RockContext() )
            {
                var person = Context.CurrentPersonId.HasValue
                    ? new PersonService( rockContext ).Get( Context.CurrentPersonId.Value )
                    : null;

                if ( !definedValue.IsAuthorized( Security.Authorization.VIEW, person ) )
                {
                    throw new RealTimeException( "You are not authorized for this phone number." );
                }

                // Multiple blocks could be monitoring the number, so increment
                // a join count and if it is our first one then join the channel.
                var newValue = state.JoinCount.Increment( rockPhoneNumber );

                if ( newValue == 1 )
                {
                    await Channels.AddToChannelAsync( Context.ConnectionId, GetChannelForPhoneNumber( definedValue ) );
                }
            }
        }

        /// <summary>
        /// Leaves an SMS number to indicate that the client no longer wishes
        /// to receive notifications.
        /// </summary>
        /// <param name="rockPhoneNumber">The rock phone number unique identifier.</param>
        public async Task LeaveSmsNumber( Guid rockPhoneNumber )
        {
            var state = this.GetConnectionState<ConversationState>( Context.ConnectionId );
            var definedValue = DefinedValueCache.Get( rockPhoneNumber );

            if ( definedValue == null )
            {
                throw new RealTimeException( "Phone number was not found." );
            }

            // Security check from the Join method is sufficient to have already
            // covered this case too. Even if they don't have access to the
            // phone number, they won't be in the channel anyway so removing them
            // would do nothing.

            // Multiple blocks could be monitoring the number, so decrement
            // our join count for this number and if it hits zero then leave
            // the channel.
            var newValue = state.JoinCount.Decrement( rockPhoneNumber );

            if ( newValue == 0 )
            {
                await Channels.RemoveFromChannelAsync( Context.ConnectionId, GetChannelForPhoneNumber( definedValue ) );
            }
        }

        /// <summary>
        /// Gets the channel name that should be used to send the message.
        /// </summary>
        /// <param name="conversationKey">The conversation key representing the conversation to be communicated with..</param>
        /// <returns>A string that represents the RealTime channel name.</returns>
        public static string GetChannelForConversationKey( string conversationKey )
        {
            var guid = CommunicationService.GetRockPhoneNumberGuidForConversationKey( conversationKey );

            if ( guid.HasValue )
            {
                return $"sms:{guid}";
            }
            else
            {
                throw new Exception( "Conversation key is not valid." );
            }
        }

        /// <summary>
        /// Gets the channel to use when sending messages for the Rock phone number.
        /// </summary>
        /// <param name="rockPhoneNumber">The rock phone number.</param>
        /// <returns>A string that represents the RealTime channel name.</returns>
        private static string GetChannelForPhoneNumber( DefinedValueCache rockPhoneNumber )
        {
            return $"sms:{rockPhoneNumber.Guid}";
        }

        /// <summary>
        /// Custom state used to track information about the participant
        /// in the topic.
        /// </summary>
        private class ConversationState
        {
            /// <summary>
            /// Gets the join count lookup table for the phone numbers.
            /// </summary>
            /// <value>The join count lookup table for the phone numbers.</value>
            public ConcurrentUsageCounter<Guid> JoinCount { get; } = new ConcurrentUsageCounter<Guid>();
        }
    }
}
