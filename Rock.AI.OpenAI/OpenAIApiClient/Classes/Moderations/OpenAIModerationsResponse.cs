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
using Newtonsoft.Json;
using Rock.AI.Classes.Moderations;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations
{
    /// <summary>
    /// The Reponse object for a moderation.
    /// </summary>
    internal class OpenAIModerationsResponse
    {
        #region Properties

        /// <summary>
        /// Unique identifier for the moderation request.
        /// </summary>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Specifies the ID of the language model used to generate the moderation
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        ///  Results from the moderation request
        /// </summary>
        [JsonProperty( "results" )]
        public OpenAIModerationsResponseResults Results { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Converst the OpenAI completion response to a generic response.
        /// </summary>
        /// <returns></returns>
        internal ModerationsResponse AsModerationsResponse()
        {
            var response = new ModerationsResponse();

            response.Id = this.Id;
            response.IsFlagged = this.Results.IsFlagged;
            response.ModerationsResponseCategories = new ModerationsResponseCategories();
            response.ModerationsResponseCategories.HateScore = this.Results.CategoryScores.Hate;
            response.ModerationsResponseCategories.IsHate = this.Results.Categories.Hate;
            response.ModerationsResponseCategories.IsSelfHarm = this.Results.Categories.SelfHarm;
            response.ModerationsResponseCategories.SelfHarmScore = this.Results.CategoryScores.SelfHarm;
            response.ModerationsResponseCategories.SexualMinorScore = this.Results.CategoryScores.SexualMinors;
            response.ModerationsResponseCategories.IsSexualMinor = this.Results.Categories.SexualMinors;
            response.ModerationsResponseCategories.ViolentScore = this.Results.CategoryScores.Violence;
            response.ModerationsResponseCategories.IsViolent = this.Results.Categories.Violence;
            response.ModerationsResponseCategories.IsSexual = this.Results.Categories.Sexual;
            response.ModerationsResponseCategories.SexualScore = this.Results.CategoryScores.Sexual;
            response.ModerationsResponseCategories.ThreatScore = this.Results.CategoryScores.Threatening;
            response.ModerationsResponseCategories.IsThreat = this.Results.Categories.Threatening;

            return response;
        }

        #endregion
    }
}
