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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rock.AI.Classes.ChatCompletions;
using Rock.AI.OpenAI.OpenAIApiClient.Attributes;
using Rock.AI.OpenAI.OpenAIApiClient.Enums;
using Rock.AI.OpenAI.Utilities;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Request Message object for a chat completion.
    /// </summary>
    internal class OpenAIChatCompletionsRequestMessage
    {
        /// <summary>
        /// The role of the message. Don't send this to OpenAI they want the string version.
        /// </summary>
        [JsonIgnore]
        public OpenAIChatMessageRole Role { get; set; }

        /// <summary>
        /// The role that OpenAI is expecting.
        /// </summary>
        [JsonProperty( "role" )]
        public string OpenAIRole {
            get
            {
                return this.Role.ToString().ToLower();
            }
        }

        /// <summary>
        /// The content of the message.
        /// </summary>
        [JsonProperty( "content" )]
        public string Content { get; set; }

        /// <summary>
        /// Converts a generic chat message to the OpenAI version.
        /// </summary>
        /// <param name="message"></param>
        internal OpenAIChatCompletionsRequestMessage ( ChatCompletionsRequestMessage message )
        {
            
            this.Role = OpenAIUtilities.ConvertRockChatRoleToOpenAIRole( message.Role );
            this.Content = message.Content;
        }
    }
}
