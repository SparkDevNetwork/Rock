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

using System.Text.Json;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents a single connected service and its configuration and
    /// bundle information.
    /// </summary>
    internal class ServiceEntry
    {
        /// <summary>
        /// The unique identifier for this service.
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// The name of this service.
        /// </summary>
        public ServiceStatus Status { get; set; }

        /// <summary>
        /// If the <see cref="Status"/> is <see cref="ServiceStatus.Error"/>,
        /// this contains a description of the issue.
        /// </summary>
        public string Issue { get; set; }

        /// <summary>
        /// The configuration for this service.
        /// </summary>
        public JsonElement Configuration { get; set; }

        /// <summary>
        /// The selected bundle for this service.
        /// </summary>
        public JsonElement Bundle { get; set; }

        /// <summary>
        /// Gets the configuration for this service as a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the configuration to.</typeparam>
        /// <returns>The configuration as a strongly typed object.</returns>
        public T GetConfiguration<T>()
        {
            try
            {
                return Configuration.Deserialize<T>( ConnectedServicesProvider.JsonOptions );
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the selected bundle for this service as a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the bundle to.</typeparam>
        /// <returns>The selected bundle as a strongly typed object.</returns>
        public T GetBundle<T>()
        {
            try
            {
                return Bundle.Deserialize<T>( ConnectedServicesProvider.JsonOptions );
            }
            catch
            {
                return default;
            }
        }
    }
}
