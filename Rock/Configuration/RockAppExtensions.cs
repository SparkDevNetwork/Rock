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
using System.IO;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Data;
using Rock.Lava;

namespace Rock.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="RockApp"/>.
    /// </summary>
    public static class RockAppExtensions
    {
        /// <summary>
        /// The characters that are not allowed in a segment of a path passed to
        /// <see cref="MapPath(RockApp, string, string)"/>. This is the Windows list
        /// of invalid file name characters (without the separators), used on every
        /// platform so a path that works on one host works on all of them.
        /// </summary>
        private static readonly char[] InvalidPathSegmentChars = new[] { '"', '<', '>', '|', ':', '*', '?' }
            .Concat( Enumerable.Range( 0, 32 ).Select( c => ( char ) c ) )
            .ToArray();

        /// <summary>
        /// Reads the initialization settings that are configured to be used
        /// the next time Rock starts. Changes can be persisted by calling
        /// <see cref="InitializationSettings.Save()"/> on the object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal method</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public classes. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <returns>An instance of <see cref="InitializationSettings"/> that can be modified.</returns>
        [RockInternal( "1.16.6" )]
        public static InitializationSettings ReadConfiguredInitializationSettings( this RockApp app )
        {
            // Use DI to get the actual object instance.
            return ( InitializationSettings ) app.GetService( typeof( InitializationSettings ) );
        }

        /// <summary>
        /// Gets the database configuration being used by this Rock instance.
        /// </summary>
        /// <returns>An instance of <see cref="IDatabaseConfiguration"/> that describes the database.</returns>
        [RockInternal( "1.16.6" )]
        public static IDatabaseConfiguration GetDatabaseConfiguration( this RockApp app )
        {
            return ( IDatabaseConfiguration ) app.GetService( typeof ( IDatabaseConfiguration ) );
        }

        /// <summary>
        /// Get the name of the current Lava engine that is being used to
        /// process Lava. If this is called before Rock has finished starting
        /// up it will always return <c>Fluid</c>.
        /// </summary>
        /// <param name="app">The RockApp for which to retrieve the Lava engine name.</param>
        /// <returns>The current Lava engine name.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "This method is meant to be used as the central point to get the engine name. Even though it doesn't use the RockApp itself now, it might in the future." )]
        public static string GetCurrentLavaEngineName( this RockApp app )
        {
            var engine = LavaService.GetCurrentEngine();

            if ( engine == null )
            {
                return "Fluid";
            }
            else
            {
                return engine.EngineName;
            }
        }

        /// <summary>
        /// Checks if the database is available for the Rock application instance.
        /// </summary>
        /// <param name="app">The RockApp instance to inspect.</param>
        /// <returns><c>true</c> if the database is available for the specified RockApp; otherwise, <c>false</c>.</returns>
        public static bool IsDatabaseAvailable( this RockApp app )
        {
            return app?.GetDatabaseConfiguration().IsDatabaseAvailable ?? false;
        }

        /// <summary>
        /// Resolves the Rock URL to the absolute path it refers to on this site.
        /// The name of the theme will be determined automatically, if it can't
        /// be determined then "Rock" will be used.
        /// </summary>
        /// <remarks>
        ///     <para>An input starting with "~~/" will return a theme URL like, "{SiteRoot}/Themes/{CurrentSiteTheme}/{input}" without the leading "~~".</para>
        ///     <para>An input starting with "~/" will return the site root like, {SiteRoot}/{input}" without the leading "~".</para>
        ///     <para>An input of "~~" will return a theme URL like "{SiteRoot}/Themes/{CurrentSiteTheme}" without a trailing slash.</para>
        ///     <para>An input of "~" will return the site root "{SiteRoot}/" with a trailing slash. </para>
        ///     <para>The path of a "~/" or "~~/" input is normalized: backslashes become slashes, repeated slashes collapse, and "." and ".." segments are resolved. The query string and fragment are not changed.</para>
        ///     <para>Leading and trailing whitespace is removed before the input is resolved. A <c>null</c> input returns <c>null</c>, and an empty or whitespace input returns an empty string.</para>
        ///     <para>A full URL with a scheme (like "https://"), or an input starting with "/" is returned without other changes.</para>
        ///     <para>Any other input (a relative path, a bare query string or fragment like "?a=1" or "#top", or a "~" or "~~" not followed by a separator) is not allowed.</para>
        /// </remarks>
        /// <param name="app">The RockApp instance.</param>
        /// <param name="url">The input with prefix <c>"~~"</c> or <c>"~"</c>.</param>
        /// <returns>The resolved URL.</returns>
        /// <exception cref="ArgumentException"><paramref name="url"/> is a relative path, or goes above the application root.</exception>
        public static string ResolveRockUrl( this RockApp app, string url )
        {
            return ResolveRockUrl( app, url, "Rock" );
        }

        /// <summary>
        /// Resolves the Rock URL to the absolute path it refers to on this site.
        /// </summary>
        /// <remarks>
        ///     <para>An input starting with "~~/" will return a theme URL like, "{SiteRoot}/Themes/{CurrentSiteTheme}/{input}" without the leading "~~".</para>
        ///     <para>An input starting with "~/" will return the site root like, {SiteRoot}/{input}" without the leading "~".</para>
        ///     <para>An input of "~~" will return a theme URL like "{SiteRoot}/Themes/{CurrentSiteTheme}" without a trailing slash.</para>
        ///     <para>An input of "~" will return the site root "{SiteRoot}/" with a trailing slash. </para>
        ///     <para>The path of a "~/" or "~~/" input is normalized: backslashes become slashes, repeated slashes collapse, and "." and ".." segments are resolved. The query string and fragment are not changed.</para>
        ///     <para>Leading and trailing whitespace is removed before the input is resolved. A <c>null</c> input returns <c>null</c>, and an empty or whitespace input returns an empty string.</para>
        ///     <para>A full URL with a scheme (like "https://"), or an input starting with "/" is returned without other changes.</para>
        ///     <para>Any other input (a relative path, a bare query string or fragment like "?a=1" or "#top", or a "~" or "~~" not followed by a separator) is not allowed.</para>
        /// </remarks>
        /// <param name="app">The RockApp instance.</param>
        /// <param name="url">The input with prefix <c>"~~"</c> or <c>"~"</c>.</param>
        /// <param name="theme">The name of the theme when a "~~" is encountered.</param>
        /// <returns>The resolved URL.</returns>
        /// <exception cref="ArgumentException"><paramref name="url"/> is a relative path, or goes above the application root.</exception>
        public static string ResolveRockUrl( this RockApp app, string url, string theme )
        {
            var appPath = app.HostingSettings.VirtualRootPath.RemoveTrailingForwardslash();

            /*
                10/26/2026 - CLAUDE

                Callers often resolve a URL that may be blank and then only use
                the result when it is not, such as an optional setting or Lava
                that rendered nothing. A blank URL is harmless, so null returns
                null and whitespace returns an empty string instead of throwing.
                Surrounding whitespace is never part of a URL, so it is removed
                first; Lava output often has leading or trailing line breaks.

                Reason: Blank URLs are harmless and should not throw.
            */
            if ( url == null )
            {
                return null;
            }

            url = url.Trim();

            /*
                10/26/2026 - CLAUDE

                Full URLs and root-relative paths already mean the same thing
                everywhere, so they are returned unchanged, the same as
                Control.ResolveUrl does.

                Reason: These do not need resolving.
            */
            if ( url.Length == 0 || url.StartsWith( "/" ) || HasUriScheme( url ) )
            {
                return url;
            }

            /*
                10/26/2026 - CLAUDE

                A relative path like "page/1" has no single meaning to resolve to.
                RockPage.ResolveRockUrl resolves it against the layout's folder
                while the Obsidian path returned it unchanged, so the browser
                resolved it against the request URL. RockApp can match neither,
                so reject it and require an explicit "~/" path instead. A bare
                query string or fragment ("?a=1", "#top") and a "~" or "~~" not
                followed by a separator are treated as relative too.

                Reason: Make relative URLs fail loudly instead of resolving differently.
            */
            if ( !url.StartsWith( "~" ) )
            {
                throw new ArgumentException( $"The URL '{url}' must not be a relative path.", nameof( url ) );
            }

            // Only the path is normalized, the query string and fragment are
            // kept exactly as supplied.
            var suffixIndex = url.IndexOfAny( new[] { '?', '#' } );
            var path = suffixIndex >= 0 ? url.Substring( 0, suffixIndex ) : url;
            var suffix = suffixIndex >= 0 ? url.Substring( suffixIndex ) : string.Empty;

            // Treat backslashes as separators, the same as VirtualPathUtility.ToAbsolute.
            path = path.Replace( '\\', '/' );

            if ( path == "~" )
            {
                // Special case, make this end with slash.
                return $"{appPath}/{suffix}";
            }

            if ( path == "~~" )
            {
                // Special case, this does not end with a slash.
                return $"{appPath}/Themes/{theme}{suffix}";
            }

            var segments = new List<string>();
            string relativePath;

            if ( path.StartsWith( "~~/" ) )
            {
                segments.Add( "Themes" );
                segments.Add( theme );
                relativePath = path.Substring( 3 );
            }
            else if ( path.StartsWith( "~/" ) )
            {
                relativePath = path.Substring( 2 );
            }
            else
            {
                // Something like "~foo" or "~~foo".
                throw new ArgumentException( $"The URL '{url}' must start with \"~/\" or \"~~/\", or be exactly \"~\" or \"~~\".", nameof( url ) );
            }

            /*
                10/26/2026 - CLAUDE

                Normalize the path the way VirtualPathUtility.ToAbsolute does:
                repeated separators collapse, "." segments are dropped and ".."
                segments remove the previous one. A ".." that would go above
                the app root throws, matching RockApp.MapPath.

                Reason: Match VirtualPathUtility.ToAbsolute normalization.
            */
            foreach ( var segment in relativePath.Split( '/' ) )
            {
                if ( segment.Length == 0 || segment == "." )
                {
                    continue;
                }

                if ( segment == ".." )
                {
                    if ( segments.Count == 0 )
                    {
                        throw new ArgumentException( $"The URL '{url}' must not go above the application root.", nameof( url ) );
                    }

                    segments.RemoveAt( segments.Count - 1 );
                    continue;
                }

                segments.Add( segment );
            }

            var resolvedUrl = $"{appPath}/{string.Join( "/", segments )}";

            // Keep the trailing slash for folder URLs like "~~/" and "~/Content/".
            if ( segments.Count > 0 && ( relativePath.Length == 0 || relativePath.EndsWith( "/" ) ) )
            {
                resolvedUrl += "/";
            }

            return resolvedUrl + suffix;
        }

        /// <summary>
        /// Determines if the URL starts with a URI scheme, such as "https:"
        /// or "mailto:".
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns><c>true</c> if the URL starts with a scheme; otherwise, <c>false</c>.</returns>
        private static bool HasUriScheme( string url )
        {
            if ( url.Length == 0 || !char.IsLetter( url[0] ) )
            {
                return false;
            }

            for ( var i = 1; i < url.Length; i++ )
            {
                var c = url[i];

                if ( c == ':' )
                {
                    return true;
                }

                if ( !char.IsLetterOrDigit( c ) && c != '+' && c != '-' && c != '.' )
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Maps the Rock relative path to the physical path it refers to on
        /// the filesystem. The name of the theme will be determined
        /// automatically, if it can't be determined then "Rock" will be used.
        /// </summary>
        /// <remarks>
        ///     <para>An input like "~~/{input}" will return a path like, "D:\RockWeb\Themes\Rock\{input}".</para>
        ///     <para>An input like "~/{input}" will return the site root like, "D:\RockWeb\{input}".</para>
        ///     <para>An input of "~~" will return a path like "D:\RockWeb\Themes\Rock\" with a trailing slash.</para>
        ///     <para>An input of "~" will return the site root "D:\RockWeb\" with a trailing slash. </para>
        ///     <para>An input starting with "/" is relative to the site root, the same as "~".</para>
        ///     <para>Any other input (relative, physical, UNC, URL, or a "~" or "~~" not followed by a separator) is not allowed.</para>
        /// </remarks>
        /// <param name="app">The RockApp instance.</param>
        /// <param name="path">The input with prefix <c>"~~"</c> or <c>"~"</c>.</param>
        /// <returns>The resolved path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> does not start with "~/", "~~/" or "/" (and is not "~" or "~~"), goes above the application root, or contains characters that are not allowed in a path.</exception>
        public static string MapPath( this RockApp app, string path )
        {
            return MapPath( app, path, "Rock" );
        }

        /// <summary>
        /// Maps the Rock relative path to the physical path it refers to on
        /// the filesystem.
        /// </summary>
        /// <remarks>
        ///     <para>An input like "~~/{input}" will return a path like, "D:\RockWeb\Themes\{theme}\{input}".</para>
        ///     <para>An input like "~/{input}" will return the site root like, "D:\RockWeb\{input}".</para>
        ///     <para>An input of "~~" will return a path like "D:\RockWeb\Themes\{theme}\" with a trailing slash.</para>
        ///     <para>An input of "~" will return the site root "D:\RockWeb\" with a trailing slash. </para>
        ///     <para>An input starting with "/" is relative to the site root, the same as "~".</para>
        ///     <para>Any other input (relative, physical, UNC, URL, or a "~" or "~~" not followed by a separator) is not allowed.</para>
        /// </remarks>
        /// <param name="app">The RockApp instance.</param>
        /// <param name="path">The input with prefix <c>"~~"</c> or <c>"~"</c>.</param>
        /// <param name="theme">The name of the theme when a "~~" is encountered.</param>
        /// <returns>The resolved path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> does not start with "~/", "~~/" or "/" (and is not "~" or "~~"), goes above the application root, or contains characters that are not allowed in a path.</exception>
        public static string MapPath( this RockApp app, string path, string theme )
        {
            /*
                10/23/2026 - CLAUDE

                A blank path has no meaningful mapping. HostingEnvironment.MapPath
                throws for it, while Server.MapPath and Control.MapPath resolve it
                against the current request URL or control folder, which RockApp
                has no equivalent for. No caller relies on a blank path, so throw
                like HostingEnvironment.MapPath instead of returning it unchanged.

                Reason: Match HostingEnvironment.MapPath for blank paths.
            */
            if ( path.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( nameof( path ), "The path to map must not be null, empty or whitespace." );
            }

            /*
                10/26/2026 - CLAUDE

                A relative path has no single meaning to map to. HostingEnvironment.MapPath
                rejects it, Server.MapPath resolves it against the request URL's folder and
                Control.MapPath against the control's folder. Treating it as relative to the
                app root would silently map some callers to a different folder than today,
                so reject it and require an explicit "~/" (or "/") path instead.

                Reason: Avoid mapping relative paths to the wrong folder.
            */
            if ( !path.StartsWith( "~" ) && !path.StartsWith( "/" ) )
            {
                throw new ArgumentException( $"The path '{path}' must start with \"~\" or \"/\".", nameof( path ) );
            }

            // Treat backslashes as separators, the same as HostingEnvironment.MapPath.
            path = path.Replace( '\\', '/' );

            // Rock does not support running under a virtual directory, so an
            // app-absolute path like "/Content/x" is the same as "~/Content/x".
            if ( path.StartsWith( "/" ) )
            {
                path = "~" + path;
            }

            var segments = new List<string>();
            string relativePath;

            if ( path == "~~" || path.StartsWith( "~~/" ) )
            {
                segments.Add( "Themes" );
                segments.Add( theme );
                relativePath = path.Length > 2 ? path.Substring( 3 ) : string.Empty;
            }
            else if ( path == "~" || path.StartsWith( "~/" ) )
            {
                relativePath = path.Length > 1 ? path.Substring( 2 ) : string.Empty;
            }
            else
            {
                // Something like "~foo" or "~~foo": a tilde prefix that is not
                // followed by a separator, which HostingEnvironment.MapPath also rejects.
                throw new ArgumentException( $"The path '{path}' must start with \"~/\", \"~~/\" or \"/\", or be exactly \"~\" or \"~~\".", nameof( path ) );
            }

            /*
                10/26/2026 - CLAUDE

                Normalize the path the way HostingEnvironment.MapPath does:
                repeated separators collapse, "." segments are dropped and ".."
                segments remove the previous one. A ".." that would go above
                the app root throws, so a caller can never be handed a path
                outside the site.

                Reason: Match HostingEnvironment.MapPath normalization.
            */
            foreach ( var segment in relativePath.Split( '/' ) )
            {
                if ( segment.Length == 0 || segment == "." )
                {
                    continue;
                }

                // HostingEnvironment.MapPath rejects these on .NET Framework, and
                // a ":" would otherwise let Path.Combine switch to another drive.
                if ( segment.IndexOfAny( InvalidPathSegmentChars ) >= 0 )
                {
                    throw new ArgumentException( $"The path '{path}' contains characters that are not allowed in a path.", nameof( path ) );
                }

                if ( segment == ".." )
                {
                    if ( segments.Count == 0 )
                    {
                        throw new ArgumentException( $"The path '{path}' must not go above the application root.", nameof( path ) );
                    }

                    segments.RemoveAt( segments.Count - 1 );
                    continue;
                }

                segments.Add( segment );
            }

            var appPath = app.HostingSettings.WebRootPath.TrimEnd( new char[] { '/', '\\' } );
            var mappedPath = segments.Count == 0
                ? appPath
                : Path.Combine( appPath, string.Join( Path.DirectorySeparatorChar.ToString(), segments ) );

            // Keep the trailing separator for folder paths like "~", "~~" and "~/Content/".
            if ( relativePath.Length == 0 || relativePath.EndsWith( "/" ) )
            {
                mappedPath += Path.DirectorySeparatorChar;
            }

            return mappedPath;
        }

        /// <summary>
        /// Creates a new <see cref="RockContext"/> for use in the application.
        /// This provides a central location to creating contexts for accessing
        /// the database so that unit tests can more easily override the behavior.
        /// </summary>
        /// <param name="app">The current Rock application instannce.</param>
        /// <returns>A new instance of <see cref="RockContext"/>.</returns>
        public static RockContext CreateRockContext( this RockApp app )
        {
            return app.GetRequiredService<IRockContextFactory>().CreateRockContext();
        }

        /// <summary>
        /// Gets the <see cref="IChatProvider"/> service from the <see cref="RockApp"/>'s <see cref="IServiceProvider"/>.
        /// </summary>
        /// <remarks>
        /// This will never return <see langword="null"/>, as an exception will be thrown if a <see cref="IChatProvider"/>
        /// service was not registered with the <see cref="RockApp"/>'s <see cref="IServiceProvider"/>.
        /// </remarks>
        /// <param name="rockApp">The <see cref="RockApp"/> from which to get the <see cref="IChatProvider"/>.</param>
        /// <returns>The <see cref="IChatProvider"/> service that was registered with the <see cref="RockApp"/>'s
        /// <see cref="IServiceProvider"/>.</returns>
        /// <exception cref="System.InvalidOperationException">There is no service of type <see cref="IChatProvider"/>.</exception>
        internal static IChatProvider GetChatProvider( this RockApp rockApp )
        {
            return rockApp.GetRequiredService<IChatProvider>();
        }
    }
}
