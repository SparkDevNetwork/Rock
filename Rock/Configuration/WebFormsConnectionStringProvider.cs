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

namespace Rock.Configuration
{
    /// <inheritdoc cref="IConnectionStringProvider" path="/summary"/>
    internal class WebFormsConnectionStringProvider : IConnectionStringProvider
    {
        /// <inheritdoc/>
        public string ConnectionString { get; }

        /// <inheritdoc/>
        public string ReadOnlyConnectionString { get; }

        /// <inheritdoc/>
        public string AnalyticsConnectionString { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebFormsConnectionStringProvider"/> class.
        /// </summary>
        public WebFormsConnectionStringProvider()
        {
            var connectionStrings = System.Configuration.ConfigurationManager.ConnectionStrings;

            ConnectionString = connectionStrings["RockContext"]?.ConnectionString ?? string.Empty;
            ReadOnlyConnectionString = connectionStrings["RockContextReadOnly"]?.ConnectionString ?? ConnectionString;
            AnalyticsConnectionString = connectionStrings["RockContextAnalytics"]?.ConnectionString ?? ConnectionString;
        }
    }
}
