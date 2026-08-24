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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

namespace Rock.Net;

/*
    8/24/2026 - CLAUDE

    Crawler detection used to be a hand-maintained regex of eleven keywords
    living in UserAgentInfo. It missed most modern crawlers (headless browsers,
    AI crawlers, HTTP client libraries) and its unanchored "bot" token matched
    real device names such as the CUBOT line of Android phones. This class
    replaces it with the crawler-user-agents dataset, the same MIT-licensed list
    behind the isbot library.

    Reason: The old keyword list let the majority of JavaScript-capable bots
    through, which inflated page view interaction counts.
*/

/// <summary>
/// Crawler detection backed by the crawler-user-agents dataset that ships with
/// Rock as an embedded resource.
/// </summary>
/// <remarks>
/// <para>
/// The dataset is vendored rather than downloaded at runtime because Rock
/// installations cannot be assumed to have outbound internet access. It is
/// refreshed as a step in the release packaging process; see the Related
/// section of the page view bot filtering spec.
/// </para>
/// <para>
/// Source: https://github.com/monperrus/crawler-user-agents (MIT).
/// Snapshot taken 2026-08-24.
/// </para>
/// </remarks>
internal static class CrawlerUserAgents
{
    #region Fields

    /// <summary>
    /// The manifest name of the embedded dataset. Must stay in sync with the
    /// EmbeddedResource entry in Rock.csproj.
    /// </summary>
    private const string ResourceName = "Rock.Net.crawler-user-agents.json";

    /// <summary>
    /// The legacy keyword list, retained only as a fallback for the case where
    /// the embedded dataset cannot be read or contains no usable patterns. A
    /// packaging mistake should degrade Rock to the old behavior rather than
    /// disable crawler detection outright.
    /// </summary>
    /// <remarks>
    /// The bare "bot" token from the original expression has been given a
    /// trailing delimiter requirement here so the fallback does not match
    /// device names that merely contain those three letters.
    /// </remarks>
    private static readonly Regex _fallbackRegex = new( @"bot[\/ ;)]|bot$|googlebot|crawler|spider|robot|crawling|whatsup|chartbeat|facebookexternalhit|pingdom|newrelic", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled );

    /// <summary>
    /// The combined expression built from the embedded dataset, or
    /// <see cref="_fallbackRegex"/> when the dataset could not be loaded.
    /// </summary>
    private static readonly Regex _crawlerRegex;

    /// <summary>
    /// The number of patterns that were loaded from the embedded dataset.
    /// </summary>
    private static readonly int _patternCount;

    #endregion

    #region Constructors

    /// <summary>
    /// Builds the crawler expression once for the lifetime of the process.
    /// </summary>
    static CrawlerUserAgents()
    {
        _crawlerRegex = BuildCrawlerRegex( out _patternCount );
    }

    #endregion

    #region Properties

    /// <summary>
    /// Indicates that the embedded dataset could not be loaded and the legacy
    /// keyword list is being used instead. Exposed so a diagnostic or health
    /// check can surface a packaging failure.
    /// </summary>
    internal static bool IsUsingFallbackList => _patternCount == 0;

    /// <summary>
    /// The number of crawler patterns loaded from the embedded dataset. Zero
    /// when <see cref="IsUsingFallbackList"/> is <c>true</c>.
    /// </summary>
    internal static int PatternCount => _patternCount;

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the supplied user-agent string belongs to a known
    /// crawler, spider, or automated client.
    /// </summary>
    /// <param name="userAgent">The raw user-agent string.</param>
    /// <returns><c>true</c> if the user agent matches a known crawler.</returns>
    internal static bool IsCrawler( string userAgent )
    {
        if ( userAgent.IsNullOrWhiteSpace() )
        {
            return false;
        }

        return _crawlerRegex.IsMatch( userAgent );
    }

    /// <summary>
    /// Reads the embedded dataset and combines every valid pattern into a
    /// single expression.
    /// </summary>
    /// <param name="patternCount">The number of patterns that were loaded.</param>
    /// <returns>The combined expression, or the fallback when loading fails.</returns>
    private static Regex BuildCrawlerRegex( out int patternCount )
    {
        patternCount = 0;

        List<string> patterns;

        try
        {
            patterns = LoadPatterns();
        }
        catch
        {
            /*
                Intentionally ignored: this runs in a static constructor during
                application startup, well before the logging infrastructure is
                guaranteed to be available. A malformed or missing dataset must
                degrade to the fallback list rather than fault the type, which
                would take down every request that touches user-agent parsing.
                IsUsingFallbackList exposes the condition for diagnostics.
            */
            return _fallbackRegex;
        }

        if ( !patterns.Any() )
        {
            return _fallbackRegex;
        }

        /*
            The upstream patterns are authored for several regex flavors, so a
            small number of them may not be valid .NET expressions. Validate
            each one individually and skip the bad ones. Combining first and
            catching afterward would discard the entire list over a single bad
            entry.
        */
        var validPatterns = new List<string>( patterns.Count );

        foreach ( var pattern in patterns )
        {
            try
            {
                _ = new Regex( pattern );
                validPatterns.Add( $"(?:{pattern})" );
            }
            catch
            {
                // Intentionally ignored: an upstream pattern that .NET cannot
                // parse is skipped so the remaining patterns still apply.
            }
        }

        if ( !validPatterns.Any() )
        {
            return _fallbackRegex;
        }

        /*
            Deliberately not RegexOptions.Compiled. Compiling an alternation of
            this size costs significant startup time and memory, and buys
            nothing here because UserAgentParser caches its results by
            user-agent string, so any given agent is matched at most once.
        */
        var combined = string.Join( "|", validPatterns );

        patternCount = validPatterns.Count;

        return new Regex( combined, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant );
    }

    /// <summary>
    /// Extracts the pattern strings from the embedded dataset.
    /// </summary>
    /// <returns>The list of raw pattern strings.</returns>
    private static List<string> LoadPatterns()
    {
        using ( var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream( ResourceName ) )
        {
            if ( stream == null )
            {
                return new List<string>();
            }

            using ( var reader = new StreamReader( stream ) )
            {
                var entries = JArray.Parse( reader.ReadToEnd() );

                return entries
                    .Select( entry => ( string ) entry["pattern"] )
                    .Where( pattern => !string.IsNullOrWhiteSpace( pattern ) )
                    .ToList();
            }
        }
    }

    #endregion
}
