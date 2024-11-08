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

using Newtonsoft.Json;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes
{
    /// <summary>
    /// The Error data returned in the response to a failed request.
    /// </summary>
    internal class OpenAIChatError
    {
        #region Properties

        /// <summary>
        /// Unique identifier for the completion request.
        /// </summary>
        [JsonProperty( "message" )]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// This will always be the text of "text_completion"
        /// </summary>
        [JsonProperty( "type" )]
        public string TypeCode { get; set; }

        /// <summary>
        /// Unix timestamp that indicates when the completion request was created.
        /// </summary>
        [JsonProperty( "code" )]
        public string ErrorCode { get; set; }

        [JsonProperty( "param" )]
        public string AdditionalData { get; set; }

        #endregion
    }
}
