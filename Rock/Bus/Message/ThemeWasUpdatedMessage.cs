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

using Rock.Bus.Queue;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Entity Update Message Class
    /// </summary>
    internal class ThemeWasUpdatedMessage : IEventMessage<EntityUpdateQueue>
    {
        /// <summary>
        /// Gets the theme identifier.
        /// </summary>
        public int ThemeId { get; set; }

        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Publishes the message that informs all nodes that a theme was modified.
        /// </summary>
        /// <param name="themeId">The identifier of the theme.</param>
        public static void Publish( int themeId )
        {
            var message = new ThemeWasUpdatedMessage
            {
                ThemeId = themeId
            };

            _ = RockMessageBus.PublishAsync( message, typeof( ThemeWasUpdatedMessage ) );
        }
    }
}
