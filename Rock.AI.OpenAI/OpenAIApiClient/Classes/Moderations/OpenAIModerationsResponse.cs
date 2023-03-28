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
using Rock.AI.Classes.Moderations;
using Rock.IpAddress;

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
        public List<OpenAIModerationsResponseResults> Results { get; set; }

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Converst the OpenAI completion response to a generic response.
        /// </summary>
        /// <returns></returns>
        internal ModerationsResponse AsModerationsResponse()
        {
            var response = new ModerationsResponse();
            
            response.IsSuccessful = this.IsSuccessful;
            response.ErrorMessage = this.ErrorMessage;

            if ( this.IsSuccessful )
            {
                response.Id = this.Id;

                if ( this.Results.Count > 0 )
                {
                    var result = this.Results.FirstOrDefault();

                    response.IsFlagged = result.IsFlagged;
                    response.ModerationsResponseCategories = new ModerationsResponseCategories();
                    response.ModerationsResponseCategories.HateScore = result.CategoryScores.Hate;
                    response.ModerationsResponseCategories.IsHate = result.Categories.Hate;
                    response.ModerationsResponseCategories.IsSelfHarm = result.Categories.SelfHarm;
                    response.ModerationsResponseCategories.SelfHarmScore = result.CategoryScores.SelfHarm;
                    response.ModerationsResponseCategories.SexualMinorScore = result.CategoryScores.SexualMinors;
                    response.ModerationsResponseCategories.IsSexualMinor = result.Categories.SexualMinors;
                    response.ModerationsResponseCategories.ViolentScore = result.CategoryScores.Violence;
                    response.ModerationsResponseCategories.IsViolent = result.Categories.Violence;
                    response.ModerationsResponseCategories.IsSexual = result.Categories.Sexual;
                    response.ModerationsResponseCategories.SexualScore = result.CategoryScores.Sexual;
                    response.ModerationsResponseCategories.ThreatScore = result.CategoryScores.Threatening;
                    response.ModerationsResponseCategories.IsThreat = result.Categories.Threatening;
                }
            }


            

            return response;
        }

        #endregion
    }
}
