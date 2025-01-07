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

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the web configuration details.
    /// </summary>
    public class WebConfigConfigurationBag
    {
        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>
        /// The time zone.
        /// </value>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable run jobs in IIS context].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable run jobs in IIS context]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableRunJobsInIISContext { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the upload file.
        /// </summary>
        /// <value>
        /// The maximum size of the upload file.
        /// </value>
        public int? MaxUploadFileSize { get; set; }

        /// <summary>
        /// Gets or sets the length of the login cookie persistence.
        /// </summary>
        /// <value>
        /// The length of the login cookie persistence.
        /// </value>
        public int? LoginCookiePersistenceLength { get; set; }

        /// <summary>
        /// Gets or sets the enable database performance counters.
        /// </summary>
        /// <value>
        /// The enable database performance counters.
        /// </value>
        public bool EnableDatabasePerformanceCounters { get; set; }

        /// <summary>
        /// Gets or sets the azure signal r endpoint.
        /// </summary>
        /// <value>
        /// The azure signal r endpoint.
        /// </value>
        public string AzureSignalREndpoint { get; set; }

        /// <summary>
        /// Gets or sets the azure signal r access key.
        /// </summary>
        /// <value>
        /// The azure signal r access key.
        /// </value>
        public string AzureSignalRAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the observability service.
        /// </summary>
        /// <value>
        /// The name of the observability service.
        /// </value>
        public string ObservabilityServiceName { get; set; }
    }
}
