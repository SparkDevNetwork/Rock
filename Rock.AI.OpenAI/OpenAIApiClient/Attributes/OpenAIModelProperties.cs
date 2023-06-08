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

namespace Rock.AI.OpenAI.OpenAIApiClient.Attributes
{
    /// <summary>
    /// An attribute to store configuration about the OpenAI models.
    /// </summary>
    internal class OpenAIModelProperties : System.Attribute
    {
        /// <summary>
        /// The maximum number of tokens the model supports.
        /// </summary>
        internal int MaxTokens { get; set; } = 4096;

        internal string Label { get; set; }

       internal OpenAIModelProperties( string label, int maxTokens )
        {
            MaxTokens = maxTokens;
            Label = label;
        }
    }
}
