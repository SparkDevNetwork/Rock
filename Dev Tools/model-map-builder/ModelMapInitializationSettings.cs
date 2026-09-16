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

using System;

using Rock.Configuration;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Minimal initialization settings for the headless model map tool.
    /// </summary>
    /// <remarks>
    /// Rock's <c>WebFormsInitializationSettings</c> probes the RockWeb web.config
    /// via <c>System.Web.Configuration</c>, which throws (and is caught) outside a
    /// web host. This tool only reads schema and defined value data, so none of
    /// those settings (encryption keys, job flags, Spark URLs, etc.) are needed.
    /// Leaving them at their defaults keeps the bootstrap dependency-free and
    /// avoids an exception-driven config probe on every run.
    /// </remarks>
    internal class ModelMapInitializationSettings : InitializationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMapInitializationSettings"/> class.
        /// </summary>
        /// <param name="connectionStringProvider">The provider that supplies the Rock database connection string.</param>
        public ModelMapInitializationSettings( IConnectionStringProvider connectionStringProvider )
            : base( connectionStringProvider )
        {
        }

        /// <summary>
        /// Saving is not supported for this tool; the settings are read-only and
        /// exist only to satisfy the RockApp bootstrap.
        /// </summary>
        public override void Save()
        {
            throw new NotSupportedException( "The model map builder does not persist initialization settings." );
        }
    }
}
