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

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A response to a chat message request. This contains the content of the
    /// response as well as other diagnostic data.
    /// </summary>
    [RockInternal( "18.0" )]
    internal class StreamingChatMessageResponse
    {
        #region Properties

        /// <summary>
        /// The text returned by the chat agent.
        /// </summary>
        public string Content
        {
            get
            {
                if ( Items != null && Items.Count > 0 && Items[0] is StreamingTextContent textContent )
                {
                    return textContent.Text;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="StreamingAgentContent"/> items that
        /// describe the content of the chat message response.
        /// </summary>
        internal IList<StreamingAgentContent> Items { get; }

        /// <summary>
        /// Usage details about the chat message response, such as token counts.
        /// This will never be <c>null</c>.
        /// </summary>
        public UsageMetric Usage { get; }

        /// <summary>
        /// The debug information for the chat message response. This will be
        /// <c>null</c> unless debugging was requested.
        /// </summary>
        public ChatMessageDebug Debug { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChatMessageResponse"/> class.
        /// </summary>
        /// <param name="items">The individual items that make up this response.</param>
        /// <param name="usage">The usage metrics associated with the chat message, such as token counts or processing details.</param>
        /// <param name="debug">The debug information for the chat message, providing additional context for troubleshooting or analysis.</param>
        internal StreamingChatMessageResponse( IList<StreamingAgentContent> items, UsageMetric usage, ChatMessageDebug debug )
        {
            Usage = usage;
            Debug = debug;
            Items = items;
        }

        #endregion
    }
}
