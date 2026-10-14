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
using System.Text.RegularExpressions;
using System.Web.Hosting;

using dotless.Core;
using dotless.Core.Exceptions;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Loggers;
using dotless.Core.Parser.Functions;
using dotless.Core.Parser.Infrastructure;
using dotless.Core.Parser.Infrastructure.Nodes;
using dotless.Core.Parser.Tree;
using dotless.Core.Stylizers;

using Microsoft.Extensions.Logging;

using Rock.Logging;
using Rock.Model;
using Rock.Utility;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Compiles the Less content of the stylesheet Lava command.
    /// </summary>
    /// <remarks>
    /// The Less content comes from Lava authors, so the compiler is limited
    /// to reading .less and .css files from the style folders.
    /// </remarks>
    internal static class StylesheetLessCompiler
    {
        #region Fields

        /// <summary>
        /// The virtual folders that Less imports may read from.
        /// </summary>
        private static readonly string[] AllowedFolders = new[] { "~/Styles", "~/Themes", "~/Plugins" };

        /// <summary>
        /// The file extensions that Less imports may read.
        /// </summary>
        private static readonly string[] AllowedExtensions = new[] { ".less", ".css" };

        /// <summary>
        /// Finds calls to the Less data-uri function (also named datauri).
        /// </summary>
        private static readonly Regex DataUriCallRegex = new Regex( @"data-?uri\s*\(", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled );

        #endregion Fields

        #region Methods

        /// <summary>
        /// Compiles the Less content into minified CSS.
        /// </summary>
        /// <param name="less">The Less content.</param>
        /// <returns>The compiled CSS, or an empty string if the content could not be compiled.</returns>
        internal static string Compile( string less )
        {
            return Compile( less, HostingEnvironment.MapPath );
        }

        /// <summary>
        /// Compiles the Less content into minified CSS, using the specified function
        /// to map virtual paths. This allows the file rules to be tested without a website.
        /// </summary>
        /// <param name="less">The Less content.</param>
        /// <param name="mapVirtualPath">The function that maps a virtual path (such as "~/Styles") to a physical path.</param>
        /// <returns>The compiled CSS, or an empty string if the content could not be compiled.</returns>
        internal static string Compile( string less, Func<string, string> mapVirtualPath )
        {
            /*
                10/9/2026 - MSE

                The engine is built here instead of with LessWeb.Parse so that file
                access can be limited. The file reader only reads .less and .css
                files from the style folders, embedded resource imports are refused,
                and the data-uri function (which reads files on its own) is refused.
                It is refused twice: the content and every imported file are checked
                for a call to it, and it is replaced in this Env's function list in
                case anything gets past that check. The function list belongs to
                this Env only, so theme compiling is not affected.

                Reason: Limit what files the stylesheet command can read.
            */
            if ( less.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            if ( DataUriCallRegex.IsMatch( less ) )
            {
                CreateLogger().LogWarning( "The stylesheet command does not support the data-uri function." );
                return string.Empty;
            }

            try
            {
                var importer = new StylesheetLessImporter( new StylesheetLessFileReader( mapVirtualPath ) );
                var parser = new dotless.Core.Parser.Parser( 1, new PlainStylizer(), importer );
                var engine = new LessEngine( parser, NullLogger.Instance, true, false );
                var env = new Env( parser )
                {
                    Compress = true
                };

                env.AddFunction( "data-uri", typeof( UnsupportedFunction ) );
                env.AddFunction( "datauri", typeof( UnsupportedFunction ) );
                engine.Env = env;

                var css = engine.TransformToCss( less, null );

                // Less errors are not shown on the page, so they are logged to help the author.
                if ( !engine.LastTransformationSuccessful )
                {
                    CreateLogger().LogWarning( "The Less for the stylesheet command could not be compiled. {ErrorMessage}", engine.LastTransformationError?.Message );
                }

                return css;
            }
            catch ( FileNotFoundException ex )
            {
                // A missing or refused import is a content mistake, so it goes to the
                // Rock log instead of the Exception Log, which it could fill on a busy page.
                CreateLogger().LogWarning( ex, "A Less import for the stylesheet command could not be read." );

                return string.Empty;
            }
            catch ( Exception ex )
            {
                // Less syntax errors are handled by the engine. Anything else is
                // logged here so the details are not shown on the page.
                ExceptionLogService.LogException( ex );

                return string.Empty;
            }
        }

        /// <summary>
        /// Determines whether the file at the specified physical path may be read
        /// by the Less compiler.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file.</param>
        /// <returns><c>true</c> if the file may be read; otherwise <c>false</c>.</returns>
        internal static bool IsAllowedPath( string physicalPath )
        {
            return IsAllowedPath( physicalPath, HostingEnvironment.MapPath );
        }

        /// <summary>
        /// Determines whether the file at the specified physical path may be read
        /// by the Less compiler, using the specified function to map virtual paths.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file.</param>
        /// <param name="mapVirtualPath">The function that maps a virtual path (such as "~/Styles") to a physical path.</param>
        /// <returns><c>true</c> if the file may be read; otherwise <c>false</c>.</returns>
        internal static bool IsAllowedPath( string physicalPath, Func<string, string> mapVirtualPath )
        {
            if ( physicalPath.IsNullOrWhiteSpace() )
            {
                return false;
            }

            string fullPath;

            // The full path is found before the extension is checked because it
            // also removes trailing dots and spaces that Windows ignores.
            try
            {
                fullPath = Path.GetFullPath( physicalPath );
            }
            catch
            {
                // Invalid paths are never allowed.
                return false;
            }

            var extension = Path.GetExtension( fullPath );

            if ( !AllowedExtensions.Any( e => e.Equals( extension, StringComparison.OrdinalIgnoreCase ) ) )
            {
                return false;
            }

            foreach ( var folder in AllowedFolders )
            {
                var physicalFolder = mapVirtualPath( folder );

                if ( physicalFolder != null && FileUtilities.IsPathWithinFolder( fullPath, physicalFolder ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates the logger for the stylesheet command's Less compiler.
        /// </summary>
        /// <returns>The logger.</returns>
        private static Microsoft.Extensions.Logging.ILogger CreateLogger()
        {
            return RockLogger.LoggerFactory.CreateLogger( typeof( StylesheetLessCompiler ).FullName );
        }

        /// <summary>
        /// Converts a file name received from the Less importer into a physical path.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="mapVirtualPath">The function that maps a virtual path to a physical path.</param>
        /// <returns>The physical path, or <c>null</c> if the file name can not be used.</returns>
        private static string MapLessPath( string fileName, Func<string, string> mapVirtualPath )
        {
            if ( fileName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                if ( fileName.StartsWith( "~", StringComparison.Ordinal ) )
                {
                    return mapVirtualPath( fileName );
                }

                // Network paths (a site hosted on a file share) are used as is, and
                // IsAllowedPath still limits them to the site's style folders.
                var isNetworkPath = fileName.StartsWith( "//", StringComparison.Ordinal ) || fileName.StartsWith( @"\\", StringComparison.Ordinal );

                if ( !isNetworkPath && ( fileName.StartsWith( "/", StringComparison.Ordinal ) || fileName.StartsWith( @"\", StringComparison.Ordinal ) ) )
                {
                    return mapVirtualPath( "~" + fileName.Replace( '\\', '/' ) );
                }

                if ( Path.IsPathRooted( fileName ) )
                {
                    return fileName;
                }
            }
            catch
            {
                // Paths that can not be mapped (such as paths above the site root) are not used.
                return null;
            }

            // Relative paths would resolve against the process working folder, so they are not used.
            return null;
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// Reads Less import files, limited to the allowed folders and file types.
        /// </summary>
        private sealed class StylesheetLessFileReader : IFileReader
        {
            /// <summary>
            /// The function that maps a virtual path to a physical path.
            /// </summary>
            private readonly Func<string, string> _mapVirtualPath;

            /// <summary>
            /// Initializes a new instance of the <see cref="StylesheetLessFileReader"/> class.
            /// </summary>
            /// <param name="mapVirtualPath">The function that maps a virtual path to a physical path.</param>
            public StylesheetLessFileReader( Func<string, string> mapVirtualPath )
            {
                _mapVirtualPath = mapVirtualPath;
            }

            /// <inheritdoc/>
            public bool UseCacheDependencies => false;

            /// <inheritdoc/>
            public byte[] GetBinaryFileContents( string fileName )
            {
                // Binary reads are only used to load assemblies for embedded resources.
                throw new FileNotFoundException( "This file is not available to the stylesheet command." );
            }

            /// <inheritdoc/>
            public string GetFileContents( string fileName )
            {
                var physicalPath = MapLessPath( fileName, _mapVirtualPath );

                if ( !IsAllowedPath( physicalPath, _mapVirtualPath ) )
                {
                    throw new FileNotFoundException( "This file is not available to the stylesheet command." );
                }

                var contents = File.ReadAllText( physicalPath );

                // Imported files get the same data-uri check as the content itself.
                if ( DataUriCallRegex.IsMatch( contents ) )
                {
                    throw new FileNotFoundException( "This file is not available to the stylesheet command." );
                }

                return contents;
            }

            /// <inheritdoc/>
            public bool DoesFileExist( string fileName )
            {
                var physicalPath = MapLessPath( fileName, _mapVirtualPath );

                return IsAllowedPath( physicalPath, _mapVirtualPath ) && File.Exists( physicalPath );
            }
        }

        /// <summary>
        /// Imports Less files, refusing imports of embedded resources.
        /// </summary>
        private sealed class StylesheetLessImporter : Importer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StylesheetLessImporter"/> class.
            /// </summary>
            /// <param name="fileReader">The file reader.</param>
            public StylesheetLessImporter( IFileReader fileReader )
                : base( fileReader )
            {
            }

            /// <inheritdoc/>
            public override ImportAction Import( Import import )
            {
                // Embedded resources ("dll://Assembly#Resource") are read without
                // the file reader, so they are refused here, wherever they appear in the path.
                if ( import.Path != null && import.Path.IndexOf( "dll:", StringComparison.OrdinalIgnoreCase ) >= 0 )
                {
                    throw new FileNotFoundException( "This file is not available to the stylesheet command." );
                }

                return base.Import( import );
            }
        }

        /// <summary>
        /// Replaces Less functions that are not supported by the stylesheet command.
        /// </summary>
        private sealed class UnsupportedFunction : Function
        {
            /// <inheritdoc/>
            protected override Node Evaluate( Env env )
            {
                throw new ParsingException( $"The {Name} function is not supported by the stylesheet command.", Location );
            }
        }

        #endregion Support Classes
    }
}
