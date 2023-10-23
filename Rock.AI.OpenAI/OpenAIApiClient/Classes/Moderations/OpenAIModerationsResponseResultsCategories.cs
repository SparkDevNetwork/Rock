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
using Rock.AI.Classes.TextCompletions;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations
{
    /// <summary>
    /// The Reponse object for moderation result categories.
    /// </summary>
    internal class OpenAIModerationsResponseResultsCategories
    {
        #region Properties

        /// <summary>
        /// Hate category result
        /// </summary>
        [JsonProperty( "hate" )]
        public bool Hate { get; set; }

        /// <summary>
        /// Hate/threatening category result
        /// </summary>
        [JsonProperty( "hate/threatening" )]
        public bool Threatening { get; set; }

        /// <summary>
        /// Self-harm category result
        /// </summary>
        [JsonProperty( "hate/threatening" )]
        public bool SelfHarm { get; set; }

        /// <summary>
        /// Sexual category result
        /// </summary>
        [JsonProperty( "sexual" )]
        public bool Sexual { get; set; }

        /// <summary>
        /// Sexual/minors category result
        /// </summary>
        [JsonProperty( "sexual/minors" )]
        public bool SexualMinors { get; set; }

        /// <summary>
        /// Violence category result
        /// </summary>
        [JsonProperty( "violence" )]
        public bool Violence { get; set; }

        /// <summary>
        /// Violence/graphic category result
        /// </summary>
        [JsonProperty( "violence/graphic" )]
        public bool ViolenceGraphic { get; set; }

        #endregion
    }
}
