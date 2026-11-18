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
using Rock.AI.OpenAI.Utilities;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions
{
    /// <summary>
    /// The Request object for a chat completion.
    /// </summary>
    [Obsolete( "This feature has been deprecated and is no longer used by Rock. AI configuration happens automatically." )]
    [RockObsolete( "21.0" )]
    internal class OpenAIChatCompletionsRequest
    {
        /// <summary>
        /// The string representation of the model to use. 
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        /// The prompt for the completion.
        /// </summary>
        [JsonProperty( "messages" )]
        public List<OpenAIChatCompletionsRequestMessage> Messages { get; set; } = [];

        /// <summary>
        /// Max Tokens to allow. For now we'll default this to be the max size.
        /// </summary>
        [JsonProperty( "max_tokens", NullValueHandling = NullValueHandling.Ignore )]
        public int MaxTokens
        {
            get
            {
                if ( _maxTokens != 0 )
                {
                    return _maxTokens;
                }

                // Estimate number of tokens in the messages content
                var messagesContent = OpenAIUtilities.TokenCount( string.Join( " ", this.Messages.Select( m => m.Content ) ) );
                var messagesRoles = OpenAIUtilities.TokenCount( string.Join( " ", this.Messages.Select( m => m.Role ) ) );

                // Add a couple of extra tokens:
                // When using the API, you should be aware that the total tokens count includes not only the tokens in your messages
                // but also a few extra tokens for internal formatting purposes.
                var messagesTokenCount = messagesContent + messagesRoles + ( 12 * this.Messages.Count );

                // As of July 2024 the most common output token max.
                const int defaultMaxOutputTokens = 4096;

                // Return the difference
                return defaultMaxOutputTokens - messagesTokenCount;
            }
            set
            {
                _maxTokens = value;
            }
        }
        private int _maxTokens = 0;

        /// <summary>
        /// The output token limit the caller asked for, sent to OpenAI as
        /// "max_completion_tokens". This is only sent when
        /// <see cref="IsMaxCompletionTokensParameterUsed"/> is <c>true</c>
        /// and the caller set an explicit limit. Otherwise the model's own
        /// default output limit is used.
        /// </summary>
        [JsonProperty( "max_completion_tokens" )]
        public int MaxCompletionTokens => _maxTokens;

        /// <summary>
        /// Gets or sets a value indicating whether the request uses the
        /// "max_completion_tokens" parameter instead of "max_tokens". Only the
        /// OpenAI provider sets this. Services behind the OpenAI Compatible
        /// provider may not know the newer parameter, so they keep the
        /// original "max_tokens" behavior.
        /// </summary>
        [JsonIgnore]
        public bool IsMaxCompletionTokensParameterUsed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sampling parameters
        /// (temperature, top_p and stop) are left out of the request.
        /// Reasoning models reject these parameters.
        /// </summary>
        [JsonIgnore]
        public bool AreSamplingParametersOmitted { get; set; }

        /// <summary>
        /// Temperature
        /// </summary>
        [JsonProperty( "temperature" )]
        public double Temperature { get; set; }

        /// <summary>
        /// Diversity of the generated output.
        /// </summary>
        [JsonProperty( "top_p" )]
        public int TopP { get; set; }

        /// <summary>
        /// Whether to stream the request.
        /// </summary>
        [JsonProperty( "stream" )]
        public bool Stream { get; set; }

        /// <summary>
        /// Parameter that can be used to specify a sequence of tokens that the model should stop generating at.
        /// </summary>
        [JsonProperty( "stop" )]
        public string Stop { get; set; }

        /*
            11/17/26 - CLAUDE

            OpenAI's reasoning models (the o-series and the GPT-5 family)
            reject "max_tokens" and require "max_completion_tokens". They also
            reject temperature, top_p and stop. The gpt-4o and gpt-4.1 models
            leave OpenAI's API on January 14, 2027, so churches must move to a
            model that has these rules.

            The computed limit (4,096 minus the input) is not sent as
            "max_completion_tokens". Reasoning models count their hidden
            reasoning against that limit, so a small limit can use up the
            budget and return an empty answer. The computed value can also be
            zero or negative for a long prompt, which OpenAI rejects.

            These ShouldSerialize methods are used by Newtonsoft.Json. The
            default for both flags is false, which sends the same body as
            before this change.

            Reason: Let the OpenAI provider use current OpenAI models.
        */

        /// <summary>
        /// Determines whether the "max_tokens" parameter is sent.
        /// </summary>
        /// <returns><c>true</c> if "max_tokens" is sent; otherwise <c>false</c>.</returns>
        public bool ShouldSerializeMaxTokens()
        {
            return !IsMaxCompletionTokensParameterUsed;
        }

        /// <summary>
        /// Determines whether the "max_completion_tokens" parameter is sent.
        /// </summary>
        /// <returns><c>true</c> if "max_completion_tokens" is sent; otherwise <c>false</c>.</returns>
        public bool ShouldSerializeMaxCompletionTokens()
        {
            return IsMaxCompletionTokensParameterUsed && _maxTokens > 0;
        }

        /// <summary>
        /// Determines whether the "temperature" parameter is sent.
        /// </summary>
        /// <returns><c>true</c> if "temperature" is sent; otherwise <c>false</c>.</returns>
        public bool ShouldSerializeTemperature()
        {
            return !AreSamplingParametersOmitted;
        }

        /// <summary>
        /// Determines whether the "top_p" parameter is sent.
        /// </summary>
        /// <returns><c>true</c> if "top_p" is sent; otherwise <c>false</c>.</returns>
        public bool ShouldSerializeTopP()
        {
            return !AreSamplingParametersOmitted;
        }

        /// <summary>
        /// Determines whether the "stop" parameter is sent.
        /// </summary>
        /// <returns><c>true</c> if "stop" is sent; otherwise <c>false</c>.</returns>
        public bool ShouldSerializeStop()
        {
            return !AreSamplingParametersOmitted;
        }

        /// <summary>
        /// Convert a generic AI completion request to a OpenAI completion request.
        /// </summary>
        /// <param name="request"></param>
        internal OpenAIChatCompletionsRequest( ChatCompletionsRequest request )
        {
            this.Model = request.Model;
            this.Temperature = request.Temperature;
            this.MaxTokens = request.MaxTokens;

            // Convert messages
            foreach ( var message in request.Messages )
            {
                this.Messages.Add( new OpenAIChatCompletionsRequestMessage( message ) );
            }
        }
    }
}
