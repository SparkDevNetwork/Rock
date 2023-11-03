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

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations
{
    /// <summary>
    /// The Reponse object for a moderation result.
    /// </summary>
    internal class OpenAIModerationsResponseResults
    {
        #region Properties

        /// <summary>
        /// Whether the text is flagged.
        /// </summary>
        [JsonProperty( "flagged" )]
        public bool IsFlagged { get; set; }

        /// <summary>
        ///  The returned categories results.
        /// </summary>
        [JsonProperty( "categories" )]
        public OpenAIModerationsResponseResultsCategories Categories { get; set; }


        /// <summary>
        ///  The returned categories scores.
        /// </summary>
        [JsonProperty( "category_scores" )]
        public OpenAIModerationsResponseResultsCategoryScores CategoryScores { get; set; }

        #endregion
    }
}
