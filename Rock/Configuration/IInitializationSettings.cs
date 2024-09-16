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

using Rock.Attribute;

namespace Rock.Configuration
{
    /// <summary>
    /// The settings that will be used to initialize Rock. These settings are
    /// used during Rock startup so any changes made will not take effect until
    /// the next time Rock starts.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>Plugins should not implement this interface as new properties
    ///         will be added over time.</strong>
    ///     </para>
    /// </remarks>
    public interface IInitializationSettings
    {
        /// <summary>
        /// Gets a value indicating whether scheduled Rock Jobs will be executed
        /// in this process.
        /// </summary>
        /// <value><c>true</c> if scheduled jobs will be executed; otherwise, <c>false</c>.</value>
        bool IsRunScheduledJobsEnabled { get; }

        /// <summary>
        /// <para>
        /// Gets the organization time zone configured for Rock to use. This
        /// is currently a format such as "Mountain Standard Time". IANA time
        /// zone designations are not currently supported.
        /// </para>
        /// <para>
        /// The string "Local" can be used to indicate the local system time zone.
        /// </para>
        /// </summary>
        /// <value>The organization time zone.</value>
        string OrganizationTimeZone { get; }

        /// <summary>
        /// Gets the key used for password encryption operations. This is a
        /// hexadecimal encode string of binary data.
        /// </summary>
        /// <value>The key used for password encryption operations.</value>
        string PasswordKey { get; }

        /// <summary>
        /// Gets a list of strings representing previous <see cref="PasswordKey"/>
        /// values that can be used when decryption password values.
        /// </summary>
        /// <value>A list of previous key values.</value>
        IReadOnlyList<string> OldPasswordKeys { get; }

        /// <summary>
        /// Gets the encryption key used when encrypting and decryption various
        /// bits of data in Rock.
        /// </summary>
        /// <value>The primary data encryption/decryption key.</value>
        string DataEncryptionKey { get; }

        /// <summary>
        /// Gets a list of strings representing previous <see cref="DataEncryptionKey"/>
        /// values that can be used when decryption previously encryped data.
        /// </summary>
        /// <value>A list of previous key values.</value>
        IReadOnlyList<string> OldDataEncryptionKeys { get; }

        /// <summary>
        /// Gets the base URL of the rock store used when searching for plugins.
        /// </summary>
        /// <value>The base URL of the rock store.</value>
        string RockStoreUrl { get; }

        /// <summary>
        /// Gets a value indicating whether duplicate group members are allowed.
        /// When enabled a person can exist in the same group more than once
        /// in a group with the same role.
        /// </summary>
        /// <value><c>true</c> if duplicate group members with the same role are allowed; otherwise <c>false</c>.</value>
        bool IsDuplicateGroupMemberRoleAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether cache statistics are enabled. When
        /// enabled various statistics on cache hit/miss ratios and other
        /// performance counters will be tracked and available. This comes with
        /// a performance penalty.
        /// </summary>
        /// <value><c>true</c> if cache statistics should be tracked; otherwise <c>false</c>.</value>
        bool IsCacheStatisticsEnabled { get; }

        /// <summary>
        /// Gets the name of the service to use when writing Ovbservability
        /// records.
        /// </summary>
        /// <value>The observability service name.</value>
        string ObservabilityServiceName { get; }

        /// <summary>
        /// Gets the Azure SignalR endpoint to use with the RealTime engine.
        /// This is required in a WebFarm environment. If not specified then
        /// the local SignalR instance will be used.
        /// </summary>
        /// <value>The Azure SignalR endpoint URI.</value>
        string AzureSignalREndpoint { get; }

        /// <summary>
        /// Gets the Azure SignalR access key to use when accessing the
        /// <see cref="AzureSignalREndpoint"/>.
        /// </summary>
        /// <value>The Azure SignalR access key.</value>
        string AzureSignalRAccessKey { get; }

        /// <summary>
        /// Gets the base Spark API URL. This is used by various services in
        /// Rock to make API calls to the Spark server.
        /// </summary>
        /// <value>A URL to the Spark API server.</value>
        string SparkApiUrl { get; }

        /// <summary>
        /// Gets the name of the node when running in a WebFarm environment.
        /// This must be unique for each node in the farm.
        /// </summary>
        /// <value>The name of this node in the farm.</value>
        string NodeName { get; }

        /// <summary>
        /// Gets the connection string used for database connections.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the connection string used for accessing the read-only database.
        /// This is usually a read-only replica of the primary database and
        /// therefore might not yet have any recent updates made to the primary
        /// database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal property</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public classes. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.6" )]
        string ReadOnlyConnectionString { get; }

        /// <summary>
        /// Gets the connection string used for accessing the analytics database.
        /// This is usually a read-only replica of the primary database and
        /// therefore might not yet have any recent updates made to the primary
        /// database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal property</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public classes. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.6" )]
        string AnalyticsConnectionString { get; }
    }
}
