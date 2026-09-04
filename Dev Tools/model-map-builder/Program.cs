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
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Console entry point for the Rock model map builder. Reflects over Rock's
    /// models (using a live database connection read from RockWeb) and writes a
    /// JSON model map to source control.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The console entry point.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>Zero on success; a non-zero exit code on failure.</returns>
        public static int Main( string[] args )
        {
            try
            {
                var options = ModelMapBuilderOptions.Parse( args );

                Console.WriteLine( $"Using RockWeb: {options.RockWebPath}" );

                // Rock has runtime dependencies that are not copied into this tool's
                // output folder. Probe RockWeb\bin for anything that fails to resolve
                // so that reflecting over every Rock type does not fail to load.
                HookAssemblyResolution( options.RockWebPath );

                // Stand up a headless RockApp so the cache layer can query the database.
                RockAppInitializer.Initialize( options.RockWebPath );

                var builder = new ModelMapBuilder( options.RockWebPath );
                var document = builder.Build( options.IncludeMethods );

                WriteDocument( document, options );

                var modelCount = document.Domains.Sum( d => d.Models.Count );
                Console.WriteLine( $"Wrote {document.Domains.Count} domains / {modelCount} models to {options.OutputPath}" );

                foreach ( var warning in builder.SkippedEntityTypeNames )
                {
                    Console.WriteLine( $"Warning: {warning}" );
                }

                return 0;
            }
            catch ( Exception ex )
            {
                Console.Error.WriteLine( $"Model map generation failed: {ex}" );
                return 1;
            }
        }

        /// <summary>
        /// Registers an assembly resolver that probes the RockWeb bin folder for
        /// any assembly that cannot be resolved from the tool's own output folder.
        /// </summary>
        /// <param name="rockWebPath">The full path to the RockWeb folder.</param>
        private static void HookAssemblyResolution( string rockWebPath )
        {
            var binPath = Path.Combine( rockWebPath, "bin" );

            AppDomain.CurrentDomain.AssemblyResolve += ( sender, resolveArgs ) =>
            {
                var assemblyName = new AssemblyName( resolveArgs.Name ).Name;
                var candidate = Path.Combine( binPath, assemblyName + ".dll" );

                return File.Exists( candidate ) ? Assembly.LoadFrom( candidate ) : null;
            };
        }

        /// <summary>
        /// Serializes the document to JSON and writes it to the output path,
        /// creating the target directory if necessary.
        /// </summary>
        /// <param name="document">The model map document to write.</param>
        /// <param name="options">The parsed options controlling output.</param>
        private static void WriteDocument( ModelMapDocument document, ModelMapBuilderOptions options )
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = options.IsCompact ? Formatting.None : Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            var json = JsonConvert.SerializeObject( document, settings );

            var outputDirectory = Path.GetDirectoryName( options.OutputPath );
            if ( outputDirectory.IsNotNullOrWhiteSpace() && !Directory.Exists( outputDirectory ) )
            {
                Directory.CreateDirectory( outputDirectory );
            }

            File.WriteAllText( options.OutputPath, json );
        }
    }
}
