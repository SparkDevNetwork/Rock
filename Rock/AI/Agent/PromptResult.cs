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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents the result of a prompt to the AI model.
    /// Returned from <see cref="IChatAgent.InvokePromptAsync(string, System.Collections.Generic.IDictionary{string, object}, System.Threading.CancellationToken)"/>.
    /// </summary>
    internal class PromptResult
    {
        /// <summary>
        /// The text response from the AI model.
        /// </summary>
        public string ResponseText { get; set; } = string.Empty;

        internal PromptResult( Microsoft.SemanticKernel.FunctionResult functionResult )
        {
            try
            {
                ResponseText = functionResult.GetValue<string>() ?? string.Empty;
            }
            catch( InvalidCastException )
            {
                // The prompt returned something that wasn't a string.
            }
        }
    }
}
