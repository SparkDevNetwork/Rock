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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Configuration;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// Handles CSS file processing, caching, and file change tracking.
    /// </summary>
    internal class CssProcessor : IDisposable
    {
        #region Constants

        /// <summary>
        /// The cache key used to find our CSS content cache object.
        /// </summary>
        private const string CssCacheKey = "Rock.Web.CssProcessor.CssCache";

        #endregion

        #region Fields

        /// <summary>
        /// The regular expression used to identify theme CSS files. This
        /// allows us to apply theme-specific processing to those files.
        /// </summary>
        private readonly Regex _themePathRegex = new Regex( @"^[/\\]Themes[/\\]([^/\\]+)[/\\]Styles[/\\][^/\\]+\.css$", RegexOptions.IgnoreCase | RegexOptions.Compiled );

        /// <summary>
        /// The regular expression used to find @import statements in CSS
        /// files.
        /// </summary>
        private readonly Regex _importRegex = new Regex( @"@import\s+(url\()?['\""](?<url>[^'\""]+)['\""]\)?",RegexOptions.IgnoreCase | RegexOptions.Compiled );

        /// <summary>
        /// The watcher that will be used to monitor CSS file changes and clear
        /// the cache when a change is detected.
        /// </summary>
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// The root directory of the web application, used to resolve relative
        /// paths and monitor for changes.
        /// </summary>
        private readonly string _rootDirectory;

        /// <summary>
        /// Tracks if we have already been disposed.
        /// </summary>
        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        /// The cache object that holds our processed CSS content.
        /// </summary>
        private ConcurrentDictionary<string, ProcessedCssFile> CssCache =>
            RockCache.GetOrAddExisting( CssCacheKey, () => new ConcurrentDictionary<string, ProcessedCssFile>( StringComparer.OrdinalIgnoreCase ) ) as ConcurrentDictionary<string, ProcessedCssFile>;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CssProcessor"/> class.
        /// </summary>
        /// <param name="hostingSettings">The hosting settings that will be used to determine the root web path.</param>
        public CssProcessor( IHostingSettings hostingSettings )
        {
            _rootDirectory = hostingSettings.WebRootPath.TrimEnd( new char[] { '/', '\\' } );

            _watcher = new FileSystemWatcher( _rootDirectory, "*.css" )
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            _watcher.Changed += OnCssFileChanged;
            _watcher.Created += OnCssFileChanged;
            _watcher.Deleted += OnCssFileChanged;
            _watcher.Renamed += OnCssFileChanged;
            _watcher.EnableRaisingEvents = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the processed CSS content, including the last modified time,
        /// and ETag for the given relative path.
        /// </summary>
        /// <param name="relativePath">The relative path to the CSS file (e.g., "/Themes/Rock/Styles/theme.css").</param>
        /// <returns>An instance of <see cref="ProcessedCssFile"/> that represents the content or <c>null</c> if the file wasn't found.</returns>
        public ProcessedCssFile GetCssContent( string relativePath )
        {
            return GetCssContent( relativePath, new HashSet<string>( StringComparer.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Gets the processed CSS content, including the last modified time,
        /// and ETag for the given relative path.
        /// </summary>
        /// <param name="relativePath">The relative path to the CSS file (e.g., "/Themes/Rock/Styles/theme.css").</param>
        /// <param name="recursiveSet">A set of file paths currently being processed to prevent infinite recursion from circular imports.</param>
        /// <returns>An instance of <see cref="ProcessedCssFile"/> that represents the content or <c>null</c> if the file wasn't found.</returns>
        private ProcessedCssFile GetCssContent( string relativePath, HashSet<string> recursiveSet )
        {
            if ( relativePath.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var absolutePath = GetFullPath( relativePath );

            // Verify the file exists and get the last modified time.
            var fileInfo = new FileInfo( absolutePath );
            if ( !fileInfo.Exists )
            {
                return null;
            }

            var lastModified = fileInfo.LastWriteTimeUtc;

            // Try to get from cache.
            if ( CssCache.TryGetValue( relativePath, out var cachedContent ) )
            {
                return cachedContent;
            }

            // Read and process the file
            var fileContent = File.ReadAllText( absolutePath );
            var processedContent = ProcessFile( absolutePath, relativePath, fileContent, recursiveSet );

            cachedContent = new ProcessedCssFile( processedContent, lastModified );
            CssCache[relativePath] = cachedContent;

            return cachedContent;
        }

        /// <summary>
        /// Process the CSS file content and return the processed content.
        /// </summary>
        /// <param name="absolutePath">The absolute filesystem path to the file being processed.</param>
        /// <param name="relativePath">The path relative to the web root to the file being processed.</param>
        /// <param name="fileContent">The original content of the file.</param>
        /// <param name="recursiveSet">The set of files that have already been processed as part of this import chain.</param>
        /// <returns>The processed content of the CSS file.</returns>
        private string ProcessFile( string absolutePath, string relativePath, string fileContent, HashSet<string> recursiveSet )
        {
            fileContent = ProcessImports( absolutePath, fileContent, recursiveSet );
            fileContent = ProcessThemeChanges( relativePath, fileContent );

            return fileContent;
        }

        /// <summary>
        /// Process any <c>@import</c> statements in the CSS file. This will
        /// recursively process any imported files and append a version query
        /// string based on the ETag of the imported file to ensure proper
        /// cache invalidation.
        /// </summary>
        /// <param name="absolutePath">The absolute filesystem path to the file being processed.</param>
        /// <param name="fileContent">The original content of the file.</param>
        /// <param name="recursiveSet"></param>
        /// <returns></returns>
        private string ProcessImports( string absolutePath, string fileContent, HashSet<string> recursiveSet )
        {
            // Prevent infinite recursion by checking if the file has
            // already been processed in the current chain. This would
            // actually indicate in error in one of the CSS files.
            if ( recursiveSet.Contains( absolutePath ) )
            {
                return fileContent;
            }

            // This is faster than a Regex scan. Most CSS files have their
            // @imports at the top of the file so this would find a match early
            // if the file has imports, and if not, we can skip the more
            // expensive regex processing.
            if ( fileContent.IndexOf( "@import", StringComparison.OrdinalIgnoreCase ) == -1 )
            {
                return fileContent;
            }

            recursiveSet.Add( absolutePath );

            var baseDir = Path.GetDirectoryName( absolutePath );

            // Find all @import statements and process each one.
            var result = _importRegex.Replace( fileContent, match =>
            {
                var url = match.Groups["url"].Value;

                if ( url.IsNullOrWhiteSpace() || url.Contains( "://" ) )
                {
                    // External or invalid, leave as-is.
                    return match.Value;
                }

                // Resolve the imported file path to the absolute path on disk.
                var importAbsPath = GetFullPath( url, baseDir );

                if ( !File.Exists( importAbsPath ) )
                {
                    return match.Value;
                }

                // Get the path of the import relative to the web root.
                var importRelativePath = GetRelativePathFromRoot( importAbsPath );

                // Use GetCssContent to get the ETag (hash) of the imported file,
                // with recursion protection. This will use cache if available, or
                // process the imported file and put it in cache if not. Which
                // means we get the correct ETag and then when the imported file
                // is requested, it won't need to be processed again.
                var importResult = GetCssContent( importRelativePath, recursiveSet );
                var hash = importResult?.ETag;

                if ( hash.IsNullOrWhiteSpace() )
                {
                    // If we couldn't get it for some reason, just leave the
                    // import as-is without the version query string.
                    return match.Value;
                }

                // Append ?v=hash while preserving any existing query string.
                // We are aware that this might cause to 'v' parameters, but
                // that isn't a major issue since this is just for the browser
                // to decide if it needs to re-fetch the file.
                var sep = url.Contains( "?" ) ? "&" : "?";
                var newUrl = url + sep + "v=" + hash;

                // Rebuild the @import statement with the new URL.
                if ( match.Value.IndexOf( "url(", StringComparison.OrdinalIgnoreCase ) >= 0 )
                {
                    return $"@import url('{newUrl}')";
                }
                else
                {
                    return $"@import '{newUrl}'";
                }
            } );

            recursiveSet.Remove( absolutePath );

            return result;
        }

        /// <summary>
        /// Process the file for theme.css additions that might need to be made.
        /// </summary>
        /// <param name="relativePath">The path relative to the web root to the file being processed.</param>
        /// <param name="fileContent">The original content of the file.</param>
        /// <returns>The processed file content.</returns>
        private string ProcessThemeChanges( string relativePath, string fileContent )
        {
            // Check if the URL is for /Themes/{themeName}/Styles/theme.css.
            // If so we attempt to process the file for any theme additions.
            // If plugin theme files ever move to a different path then this
            // logic would need to be updated.
            var match = _themePathRegex.Match( relativePath );

            // If it doesn't appear to be a theme file, then just return it as-is.
            if ( !match.Success )
            {
                return fileContent;
            }

            // Try to load the theme and if it is found then amend the file content
            // with the additional theme changes.
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var themeName = match.Groups[1].Value;
                var theme = new ThemeService( rockContext )
                    .Queryable()
                    .FirstOrDefault( t => t.Name == themeName );

                if ( theme != null )
                {
                    return ThemeService.GetThemeCssContent( theme, fileContent );
                }
            }

            return fileContent;
        }

        /// <summary>
        /// Get the relative path from the web root for a given absolute
        /// filesystem path.
        /// </summary>
        /// <param name="absolutePath">The absolute path on the filesystem to the file.</param>
        /// <returns>The path relative to the web root. This will always start with '/'.</returns>
        private string GetRelativePathFromRoot( string absolutePath )
        {
            if ( !absolutePath.StartsWith( _rootDirectory, StringComparison.OrdinalIgnoreCase ) )
            {
                return absolutePath;
            }

            var rel = absolutePath.Substring( _rootDirectory.Length ).TrimStart( Path.DirectorySeparatorChar );

            return "/" + rel.Replace( Path.DirectorySeparatorChar, '/' );
        }

        /// <summary>
        /// Gets the full path of the relative path. If the relative path starts
        /// with a slash, it is considered relative to the web root. If it does
        /// not start with a slash, it is considered relative to the base
        /// directory provided (or the web root if no base directory is provided).
        /// This will also normalize the directory separators and resolve any
        /// relative path components such as '../'.
        /// </summary>
        /// <param name="relativePath">The relative path to either the web root or the base directory.</param>
        /// <param name="baseDir">The base directory to use when resolving paths that don't begin with a '/'.</param>
        /// <returns>The full path of the file.</returns>
        private string GetFullPath( string relativePath, string baseDir = null )
        {
            var normalizedPath = relativePath.Replace( '/', Path.DirectorySeparatorChar );

            if ( normalizedPath.Length >= 1 && normalizedPath[0] == Path.DirectorySeparatorChar )
            {
                return Path.GetFullPath( Path.Combine( _rootDirectory, normalizedPath.TrimStart( Path.DirectorySeparatorChar ) ) );
            }

            return Path.GetFullPath( Path.Combine( baseDir ?? _rootDirectory, normalizedPath ) );
        }

        /// <summary>
        /// Handles file system watcher events to clear the cache on any CSS file change.
        /// </summary>
        private void OnCssFileChanged( object sender, FileSystemEventArgs e )
        {
            RockCache.Remove( CssCacheKey );
        }

        /// <summary>
        /// Disposes the file system watcher and resources.
        /// </summary>
        public void Dispose()
        {
            if ( !_disposed )
            {
                _watcher?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}
