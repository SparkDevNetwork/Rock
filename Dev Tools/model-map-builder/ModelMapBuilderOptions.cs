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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// Parsed command-line options for the model map builder.
    /// </summary>
    internal class ModelMapBuilderOptions
    {
        /// <summary>
        /// Gets or sets the full path to the RockWeb folder (used to locate the
        /// connection strings config and the XML doc fallback).
        /// </summary>
        public string RockWebPath { get; set; }

        /// <summary>
        /// Gets or sets the full path of the JSON file to write.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the JSON should be minified.
        /// When <see langword="false"/> (the default) the output is indented for
        /// readable source-control diffs.
        /// </summary>
        public bool IsCompact { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each model's methods are
        /// included in the output. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IncludeMethods { get; set; }

        /// <summary>
        /// Parses the command-line arguments into an options instance, filling in
        /// defaults relative to the repository root when not supplied.
        /// </summary>
        /// <param name="args">The raw command-line arguments.</param>
        /// <returns>The populated options.</returns>
        /// <exception cref="ArgumentException">Thrown when an argument is malformed or unknown.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the repository root cannot be located for a default path.</exception>
        public static ModelMapBuilderOptions Parse( string[] args )
        {
            var options = new ModelMapBuilderOptions();

            for ( var i = 0; i < args.Length; i++ )
            {
                var arg = args[i];

                switch ( arg )
                {
                    case "--output":
                        options.OutputPath = RequireValue( args, ref i, arg );
                        break;

                    case "--rockweb":
                        options.RockWebPath = RequireValue( args, ref i, arg );
                        break;

                    case "--compact":
                        options.IsCompact = true;
                        break;

                    case "--include-methods":
                        options.IncludeMethods = true;
                        break;

                    default:
                        throw new ArgumentException( $"Unknown argument '{arg}'. Valid options are --output, --rockweb, --compact, --include-methods." );
                }
            }

            // Resolve defaults relative to the repository root when needed.
            if ( options.RockWebPath.IsNullOrWhiteSpace() || options.OutputPath.IsNullOrWhiteSpace() )
            {
                var repositoryRoot = FindRepositoryRoot();

                if ( repositoryRoot == null )
                {
                    throw new InvalidOperationException( "Could not locate the repository root (a folder containing Rock.sln). Pass --rockweb and --output explicitly." );
                }

                if ( options.RockWebPath.IsNullOrWhiteSpace() )
                {
                    options.RockWebPath = Path.Combine( repositoryRoot, "RockWeb" );
                }

                if ( options.OutputPath.IsNullOrWhiteSpace() )
                {
                    options.OutputPath = Path.Combine( repositoryRoot, "Dev Tools", "docs", "model-map", "model-map.json" );
                }
            }

            return options;
        }

        /// <summary>
        /// Reads the value that follows a flag, advancing the index, and throws if
        /// no value is present.
        /// </summary>
        /// <param name="args">The raw command-line arguments.</param>
        /// <param name="index">The current index, advanced to the consumed value.</param>
        /// <param name="flag">The flag whose value is being read (for error messages).</param>
        /// <returns>The value that followed the flag.</returns>
        private static string RequireValue( string[] args, ref int index, string flag )
        {
            if ( index + 1 >= args.Length )
            {
                throw new ArgumentException( $"The '{flag}' option requires a value." );
            }

            index++;
            return args[index];
        }

        /// <summary>
        /// Walks up from the executing assembly's directory to find the folder
        /// that contains Rock.sln.
        /// </summary>
        /// <returns>The repository root path, or <see langword="null"/> if not found.</returns>
        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

            while ( directory != null )
            {
                if ( File.Exists( Path.Combine( directory.FullName, "Rock.sln" ) ) )
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
