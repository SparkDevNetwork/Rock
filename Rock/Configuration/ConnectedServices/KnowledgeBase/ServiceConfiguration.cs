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

namespace Rock.Configuration.ConnectedServices.KnowledgeBase
{
    /// <summary>
    /// The configured settings for the Knowledge Base service.
    /// </summary>
    internal class ServiceConfiguration
    {
        /// <summary>
        /// The API key used to authenticate with the Knowledge Base service.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Creates a new <see cref="ServiceConfiguration"/> from the given <see cref="ServiceEntry"/>.
        /// </summary>
        /// <param name="entry">The service entry to create the configuration from.</param>
        /// <returns>A new <see cref="ServiceConfiguration"/> instance.</returns>
        public static ServiceConfiguration FromEntry( ServiceEntry entry )
        {
            var cfg = entry?.GetConfiguration<ServiceEntryConfiguration>();

            if ( cfg == null )
            {
                return null;
            }

            return new ServiceConfiguration
            {
                ApiKey = cfg.ApiKey,
            };
        }

        /// <summary>
        /// The configuration object stored in the service entry. This is used
        /// to deserialize the configuration from the service entry.
        /// </summary>
        private class ServiceEntryConfiguration
        {
            /// <inheritdoc cref="ServiceConfiguration.ApiKey" />
            public string ApiKey { get; set; }
        }
    }
}
