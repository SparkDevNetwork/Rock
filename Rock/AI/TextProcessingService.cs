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

using System.Threading.Tasks;

namespace Rock.AI
{
    /// <summary>
    /// An abstract base class for AI assisted text processing services that
    /// provide chat completion and moderation capabilities.
    /// </summary>
    internal abstract class TextProcessingService
    {
        /// <summary>
        /// Determines whether the text processing service is available for use.
        /// </summary>
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// Gets the chat completion response for the given request asynchronously.
        /// </summary>
        /// <param name="request">The details about the request to process.</param>
        /// <returns>The response from the chat completion request.</returns>
        public abstract Task<ChatCompletionResponse> GetChatCompletionAsync( ChatCompletionRequest request );

        /// <summary>
        /// Gets the moderation response for the given request asynchronously.
        /// </summary>
        /// <param name="request">The detailsa about the request to process.</param>
        /// <returns>The response from the moderation request.</returns>
        public abstract Task<ModerationResponse> GetModerationAsync( ModerationRequest request );
    }
}
