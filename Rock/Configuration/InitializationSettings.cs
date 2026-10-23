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

using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Rock.Enums.Configuration;

namespace Rock.Configuration
{
    /// <inheritdoc cref="IInitializationSettings" path="/summary"/>
    public abstract class InitializationSettings : IInitializationSettings
    {
        /// <inheritdoc/>
        public bool IsRunScheduledJobsEnabled { get; set; }

        /// <inheritdoc/>
        public string OrganizationTimeZone { get; set; }

        /// <inheritdoc/>
        public string PasswordKey { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> OldPasswordKeys { get; protected set; }

        /// <inheritdoc/>
        public string DataEncryptionKey { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> OldDataEncryptionKeys { get; protected set; }

        /// <inheritdoc/>
        public string RockStoreUrl { get; set; }

        /// <inheritdoc/>
        public bool IsDuplicateGroupMemberRoleAllowed { get; set; }

        /// <inheritdoc/>
        public bool IsCacheStatisticsEnabled { get; set; }

        /// <inheritdoc/>
        public string ObservabilityServiceName { get; set; }

        /// <inheritdoc/>
        public string AzureSignalREndpoint { get; set; }

        /// <inheritdoc/>
        public string AzureSignalRAccessKey { get; set; }

        /// <inheritdoc/>
        public string SparkApiUrl { get; set; }

        /// <inheritdoc/>
        public string NodeName { get; set; }

        /// <inheritdoc/>
        public string ConnectionString { get; }

        /// <inheritdoc/>
        public string ReadOnlyConnectionString { get; }

        /// <inheritdoc/>
        public string AnalyticsConnectionString { get; }

        /// <inheritdoc/>
        public DeploymentEnvironment DeploymentEnvironment { get; protected set; }

        /// <summary>
        /// <para>
        /// Saves the initialization settings so that they will be used the next
        /// time Rock starts.
        /// </para>
        /// <para>
        /// If any errors occur while trying to save the settings then an
        /// exception will be thrown.
        /// </para>
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationSettings"/> class.
        /// </summary>
        /// <param name="connectionStringProvider">The connection string provider to use for initializing settings.</param>
        [ActivatorUtilitiesConstructor]
        internal InitializationSettings( IConnectionStringProvider connectionStringProvider )
        {
            ConnectionString = connectionStringProvider.ConnectionString;
            ReadOnlyConnectionString = connectionStringProvider.ReadOnlyConnectionString;
            AnalyticsConnectionString = connectionStringProvider.AnalyticsConnectionString;
        }

        /// <summary>
        /// Loads a numbered series of setting values, such as the old password
        /// keys. The first value uses the key as-is and the rest append a
        /// number starting at 1 (<c>OldPasswordKey</c>, <c>OldPasswordKey1</c>,
        /// <c>OldPasswordKey2</c>). Loading stops at the first missing value.
        /// </summary>
        /// <param name="getValue">The function that returns the setting value for a key.</param>
        /// <param name="key">The key of the first value in the series.</param>
        /// <returns>The values that were found, in order.</returns>
        private protected static IReadOnlyList<string> GetNumberedValues( Func<string, string> getValue, string key )
        {
            /*
                10/23/2026 - CLAUDE

                This naming matches the way Encryption and the Database
                authentication provider have always read these keys from
                web.config. The settings were previously loaded starting at
                "OldPasswordKey0", which would not find the existing keys.

                Reason: Keep existing web.config old keys working.
            */
            var values = new List<string>();
            var value = getValue( key );

            for ( int i = 1; !string.IsNullOrWhiteSpace( value ); i++ )
            {
                values.Add( value );
                value = getValue( $"{key}{i}" );
            }

            return values;
        }
    }
}
