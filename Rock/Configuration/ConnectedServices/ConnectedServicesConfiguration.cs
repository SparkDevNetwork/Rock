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

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// The configuration for all Connected Service related options.
    /// </summary>
    internal class ConnectedServicesConfiguration
    {
        /// <summary>
        /// The token used to authenticate requests to the Rock Connected
        /// Services API.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// The configured settings for the Rock Intelligence service. May
        /// be <c>null</c> if the service is not configured or available.
        /// </summary>
        public RockIntelligence.ServiceConfiguration RockIntelligence { get; set; }

        /// <summary>
        /// The configured settings for the Knowledge Base service. May
        /// be <c>null</c> if the service is not configured or available.
        /// </summary>
        public KnowledgeBase.ServiceConfiguration KnowledgeBase { get; set; }
    }
}
