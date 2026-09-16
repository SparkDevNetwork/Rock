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
using System.IO;
using System.Xml;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Data;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Stands up a headless <see cref="RockApp"/> so that Rock's cache layer
    /// (EntityTypeCache, DefinedTypeCache) and RockContext can be used from a
    /// console process. This mirrors the non-web service registration in
    /// <see cref="RockApp"/>'s static constructor, swapping in a connection string
    /// read from the RockWeb configuration file.
    /// </summary>
    internal static class RockAppInitializer
    {
        /// <summary>
        /// The connection string name Rock uses for its primary database.
        /// </summary>
        private const string RockContextConnectionName = "RockContext";

        /// <summary>
        /// Reads the RockWeb connection string and initializes
        /// <see cref="RockApp.Current"/> so the cache layer can query the database.
        /// </summary>
        /// <param name="rockWebPath">The full path to the RockWeb folder.</param>
        /// <exception cref="FileNotFoundException">Thrown when the connection strings config file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the RockContext connection string cannot be found.</exception>
        public static void Initialize( string rockWebPath )
        {
            var connectionString = ReadRockContextConnectionString( rockWebPath );

            var connectionStringProvider = new SuppliedConnectionStringProvider( connectionString );
            var initializationSettings = new ModelMapInitializationSettings( connectionStringProvider );

            var services = new ServiceCollection();

            services.AddSingleton<IConnectionStringProvider>( connectionStringProvider );
            services.AddSingleton<IInitializationSettings>( initializationSettings );

            /*
                7/17/2026 - CLAUDE

                The DatabaseConfiguration, HostingSettings and RockContextFactory
                types are internal to Rock. Registering them via factory delegates
                (rather than open type registration) keeps their construction in
                this friend assembly, so the DI container never has to instantiate
                an internal type from its own dynamic assembly (which would throw a
                MethodAccessException).

                Reason: Avoid DI accessibility failures when constructing internal Rock services.
            */
            services.AddSingleton<IDatabaseConfiguration>( sp => new DatabaseConfiguration( sp.GetRequiredService<IInitializationSettings>() ) );
            services.AddSingleton<IHostingSettings>( sp => new HostingSettings( sp.GetRequiredService<IInitializationSettings>() ) );
            services.AddSingleton<IRockContextFactory>( sp => new RockContextFactory() );

            RockApp.Current = new RockApp( services.BuildServiceProvider() );
        }

        /// <summary>
        /// Reads the <c>RockContext</c> connection string from the RockWeb
        /// <c>web.ConnectionStrings.config</c> file, mirroring the approach used by
        /// Rock.CodeGeneration's <c>CodeGenHelpers</c>.
        /// </summary>
        /// <param name="rockWebPath">The full path to the RockWeb folder.</param>
        /// <returns>The Rock database connection string.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the config file is missing.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the RockContext entry is missing.</exception>
        private static string ReadRockContextConnectionString( string rockWebPath )
        {
            var configPath = Path.Combine( rockWebPath, "web.ConnectionStrings.config" );

            if ( !File.Exists( configPath ) )
            {
                throw new FileNotFoundException( $"Could not find the RockWeb connection strings file at '{configPath}'. Configure a local Rock database first." );
            }

            var document = new XmlDocument();
            document.Load( configPath );

            var node = document.DocumentElement?.SelectSingleNode( $"add[@name = \"{RockContextConnectionName}\"]" );
            var connectionString = node?.Attributes?["connectionString"]?.Value;

            if ( connectionString.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( $"The '{RockContextConnectionName}' connection string was not found in '{configPath}'." );
            }

            return connectionString;
        }
    }
}
