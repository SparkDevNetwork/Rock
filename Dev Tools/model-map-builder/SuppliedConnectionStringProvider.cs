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

using Rock.Configuration;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Supplies a connection string that was read from an external source (the
    /// RockWeb <c>web.ConnectionStrings.config</c> file) to the RockApp being
    /// bootstrapped for this console tool.
    /// </summary>
    /// <remarks>
    /// This mirrors the shape of Rock's own <c>WebFormsConnectionStringProvider</c>,
    /// but takes the value directly rather than reading it from the current
    /// process's <c>ConfigurationManager</c>, since a console tool has no
    /// RockWeb web.config of its own.
    /// </remarks>
    internal class SuppliedConnectionStringProvider : IConnectionStringProvider
    {
        /// <inheritdoc/>
        public string ConnectionString { get; }

        /// <inheritdoc/>
        public string ReadOnlyConnectionString { get; }

        /// <inheritdoc/>
        public string AnalyticsConnectionString { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuppliedConnectionStringProvider"/> class.
        /// </summary>
        /// <param name="connectionString">The Rock database connection string to use for all connection types.</param>
        public SuppliedConnectionStringProvider( string connectionString )
        {
            // The model map only reads schema and defined value data, so the
            // read-only and analytics connections can safely reuse the primary.
            ConnectionString = connectionString;
            ReadOnlyConnectionString = connectionString;
            AnalyticsConnectionString = connectionString;
        }
    }
}
