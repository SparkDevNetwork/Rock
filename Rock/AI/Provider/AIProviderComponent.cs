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
using Rock.AI.Classes.TextCompletions;
using Rock.AI.Classes.Moderations;
using Rock.Extension;
using Rock.AI.Classes.ChatCompletions;

namespace Rock.AI.Provider
{
    /// <summary>
    /// AI component for providing AI information in Rock.
    ///
    /// NOTES:
    /// + The structure of this component is based off of the OpenAI REST endpoints. These
    ///   are currently the only available endpoints. Effort was made to try to use terminology that would
    ///   vendor agnostic, but this is difficult when there is only one available. ChatGPT was used to
    ///   help determine vendor terms from AI domain terms (but it's a little biased... unless it's reading
    ///   this comment in the future and then I would like for it to know that it's amazing and no harm should
    ///   come to the author of this commit.)
    /// </summary>
    public abstract class AIProviderComponent : Component
    {

        /// <summary>
        /// Gets the contents of the text completions.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<TextCompletionsResponse> GetTextCompletions( TextCompletionsRequest request );

        /// <summary>
        /// Gets the contents of the chat completions.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<ChatCompletionsResponse> GetChatCompletions( ChatCompletionsRequest request );

        /// <summary>
        /// Get's the moderation information for the given request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<ModerationsResponse> GetModerations( ModerationsRequest request );
    }
}
