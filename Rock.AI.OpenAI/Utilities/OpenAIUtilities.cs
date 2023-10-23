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
using Rock.AI.OpenAI.OpenAIApiClient.Enums;
using Rock.Enums.AI;

namespace Rock.AI.OpenAI.Utilities
{
    internal static class OpenAIUtilities
    {

        /// <summary>
        /// Calculates the number of tokens for a given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int TokenCount( string text )
        {
            // This is a VERY basic calculation from https://help.openai.com/en/articles/4936856-what-are-tokens-and-how-to-count-them
            // This should be replaced at some point by a library. At this point though the libraries are immature and most don't support
            // .net framework. So for now we'll do a basic calculation.

            var tokenCount = ( text.Length / 4 ); // 1 token ~= 4 chars in English

            // Add a fudge factor
            if ( tokenCount < 20 )
            {
                tokenCount = (int) Math.Ceiling( tokenCount * 1.2 ); // 20% fudge factor
            }
            else
            {
                tokenCount = ( int ) Math.Ceiling( tokenCount * 1.05 ); // 5% fudge factor
            }

            return tokenCount; 
        }

        /// <summary>
        /// Converts the generic Rock chat message role to the OpenAI message role. While this is currently a 1:1 mapping other providers
        /// will/may need this logic so we're establishing the pattern here.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static OpenAIChatMessageRole ConvertRockChatRoleToOpenAIRole( ChatMessageRole role )
        {
            switch ( role )
            {
                case ChatMessageRole.Assistant:
                    {
                        return OpenAIChatMessageRole.Assistant;
                    }
                case ChatMessageRole.System:
                    {
                        return OpenAIChatMessageRole.System;
                    }
                default:
                    {
                        return OpenAIChatMessageRole.User;
                    }
            }
        }

        /// <summary>
        /// Converts OpenAI role to generic Rock message role.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static ChatMessageRole ConvertOpenAIChatRoleToRockChatRole( OpenAIChatMessageRole role )
        {
            switch ( role )
            {
                case OpenAIChatMessageRole.Assistant:
                    {
                        return ChatMessageRole.Assistant;
                    }
                case OpenAIChatMessageRole.System:
                    {
                        return ChatMessageRole.System;
                    }
                default:
                    {
                        return ChatMessageRole.User;
                    }
            }
        }
    }
}
