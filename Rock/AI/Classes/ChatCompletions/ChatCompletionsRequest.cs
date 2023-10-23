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
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace Rock.AI.Classes.ChatCompletions
{
    /// <summary>
    /// The class for creating a new request for a chat completion.
    /// </summary>
    public class ChatCompletionsRequest
    {
        /// <summary>
        /// The model to use for the completion. See the documentation for your provider for valid values.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// The messages to send the the service.
        /// </summary>
        public List<ChatCompletionsRequestMessage> Messages { get; set; } = new List<ChatCompletionsRequestMessage>();

        /// <summary>
        /// The level of randomness or creativity in the generated text. See documentation for your provider for valid values.
        /// </summary>
        public double Temperature { get; set; } = 0.8;

        /// <summary>
        /// The maximum number of tokens the completion should be.
        /// </summary>
        public int MaxTokens { get; set; }
    }
}
