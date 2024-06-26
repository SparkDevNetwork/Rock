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
    /// <summary>
    /// Provides the connection string for the running Rock instance.
    /// </summary>
    internal interface IConnectionStringProvider
    {
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
        string ReadOnlyConnectionString { get; }

        /// <summary>
        /// Gets the connection string used for accessing the analytics database.
        /// This is usually a read-only replica of the primary database and
        /// therefore might not yet have any recent updates made to the primary
        /// database.
        /// </summary>
        string AnalyticsConnectionString { get; }
    }
}
