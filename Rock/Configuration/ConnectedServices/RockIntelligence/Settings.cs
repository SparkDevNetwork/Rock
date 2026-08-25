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

using System.Collections.Generic;

namespace Rock.Configuration.ConnectedServices.RockIntelligence
{
    /// <summary>
    /// The service-specific payload carried by a Rock Intelligence bundle:
    /// how to reach the model service and which models it provides. Stored
    /// on <see cref="ServiceEntryBundle{T}.Settings"/>.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// The URL of an Open AI compatible API endpoint that can be used to
        /// access the model service.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The API key that can be used to authenticate with the model service.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The list of models that are available from the model service.
        /// </summary>
        public List<AIModel> Models { get; set; }
    }
}
