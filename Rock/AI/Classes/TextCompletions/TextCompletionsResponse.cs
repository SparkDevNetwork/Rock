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

namespace Rock.AI.Classes.TextCompletions
{
    /// <summary>
    /// The class for holding the response from a completion.
    /// </summary>
    public class TextCompletionsResponse
    {
        /// <summary>
        /// A unique identifier for the completion.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The date time that the completion was processed.
        /// </summary>
        public DateTime CompletionDateTime { get; set; }

        /// <summary>
        /// A list of possible choices returned from the provider.
        /// </summary>
        public List<TextCompletionsResponseChoice> Choices { get; set; } = new List<TextCompletionsResponseChoice>();

        /// <summary>
        /// The number of tokens used in the request.
        /// </summary>
        public int TokensUsed { get; set; }

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
