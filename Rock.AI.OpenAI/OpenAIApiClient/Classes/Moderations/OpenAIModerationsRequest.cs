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
    /// The Request object for a moderation.
    /// </summary>
    internal class OpenAIModerationsRequest
    {
        #region Properties

        /// <summary>
        /// Text for the moderation request.
        /// </summary>
        [JsonProperty( "input" )]
        public string Input { get; set; }

        /// <summary>
        /// Specifies the ID of the language model used to generate the moderation.
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Converts the generic moderations request to an OpenAI moderation request.
        /// </summary>
        /// <param name="request"></param>
        public OpenAIModerationsRequest( ModerationsRequest request )
        {
            this.Input = request.Input;
            this.Model = request.Model;
        }

        #endregion
    }
}
